﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Microsoft.Reporting.WebForms;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Business;
using Abundance_Nk.Web.Models;
using System.Configuration;


namespace Abundance_Nk.Web.Reports.Presenter
{
    public partial class PostJambApplicant : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                lblMessage.Text = "";

                if (!IsPostBack)
                {
                    Option = 1;
                    PopulateAllDropDown();
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message;
            }
        }

        public Session AcademicSession
        {
            get { return new Session() { Id = Convert.ToInt32(ddlSession.SelectedValue), Name = ddlSession.SelectedItem.Text }; }
            set { ddlSession.SelectedValue = value.Id.ToString(); }
        }

        public Programme Programme
        {
            get { return new Programme() { Id = Convert.ToInt32(ddlProgramme.SelectedValue), Name = ddlProgramme.SelectedItem.Text }; }
            set { ddlProgramme.SelectedValue = value.Id.ToString(); }
        }

        public int Option
        {
            get { return Convert.ToInt32(rblSortOption.SelectedValue); }
            set { rblSortOption.SelectedValue = value.ToString(); }
        }

        private void DisplayReportBy(Session session, Programme programme, SortOption sortOption)
        {
            try
            {
                List<PhotoCard> photoCards = null;
                ApplicationFormLogic applicationformLogic = new ApplicationFormLogic();
                photoCards = applicationformLogic.GetPostJAMBApplicationsBy(session, programme, sortOption);

                //if (programme.Id == -100)
                //{
                //    photoCards = applicationformLogic.GetPostJAMBApplications(session);
                //}
                //else
                //{
                //    photoCards = applicationformLogic.GetPostJAMBApplicationsBy(session, programme);
                //}

                string bind_dsPostJambApplicant = "dsApplicant";
                string reportPath = @"Reports\PostJambApplicant.rdlc";

                if (photoCards != null && photoCards.Count > 0)
                {
                    string appRoot = ConfigurationManager.AppSettings["AppRoot"];

                    foreach (PhotoCard photocard in photoCards)
                    {
                        if (!string.IsNullOrWhiteSpace(photocard.PassportUrl))
                        {
                            photocard.PassportUrl = appRoot + photocard.PassportUrl;
                        }
                        else
                        {
                            photocard.PassportUrl = appRoot + Utility.DEFAULT_AVATAR;
                        }
                    }
                }

                rv.Reset();
                rv.LocalReport.DisplayName = "List of Applicants for " + session.Name;
                rv.LocalReport.ReportPath = reportPath;

                if (photoCards != null)
                {
                    rv.ProcessingMode = ProcessingMode.Local;
                    rv.LocalReport.DataSources.Add(new ReportDataSource(bind_dsPostJambApplicant.Trim(), photoCards));
                    rv.LocalReport.Refresh();
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message;
            }
        }

        private void PopulateAllDropDown()
        {
            try
            {
                UserLogic userLogic = new UserLogic();
                User user = userLogic.GetModelsBy(u => u.User_Name == User.Identity.Name).LastOrDefault();

                Utility.BindDropdownItem(ddlSession, Utility.GetAllSessions(), Utility.ID, Utility.NAME);
                if(user != null && user.Role.Id == 31)
                {
                    Utility.BindDropdownItem(ddlProgramme, Utility.GetODfelProgrammes(), Utility.ID, Utility.NAME);
                }
                else
                {
                    Utility.BindDropdownItem(ddlProgramme, Utility.GetAllProgrammes(), Utility.ID, Utility.NAME);
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message;
            }
        }

        protected void btnDisplayReport_Click(object sender, EventArgs e)
        {
            try
            {
                if (InvalidUserInput())
                {
                    return;
                }

                SortOption sortOption = Option == 2 ? SortOption.ExamNo : Option == 3 ? SortOption.ApplicationNo : SortOption.Name;
                DisplayReportBy(AcademicSession, Programme, sortOption);
            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message;
            }
        }

        private bool InvalidUserInput()
        {
            try
            {
                if (AcademicSession == null || AcademicSession.Id <= 0)
                {
                    lblMessage.Text = "Please select Session";
                    return true;
                }
                else if (Programme == null || Programme.Id <= 0)
                {
                    lblMessage.Text = "Please select Programme";
                    return true;
                }
                //else if (Department == null || Department.Id <= 0)
                //{
                //    lblMessage.Text = "Please select Department";
                //    return true;
                //}

                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //protected void ddlProgramme_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        if (Programme != null && Programme.Id > 0)
        //        {
        //            PopulateDepartmentDropdownByProgramme(Programme);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        lblMessage.Text = ex.Message;
        //    }
        //}

        //private void PopulateDepartmentDropdownByProgramme(Programme programme)
        //{
        //    try
        //    {
        //        Utility.BindDropdownItem(ddlDepartment, Utility.GetDepartmentByProgramme(programme), Utility.ID, Utility.NAME);
        //    }
        //    catch (Exception ex)
        //    {
        //        lblMessage.Text = ex.Message;
        //    }
        //}


         





    }
}