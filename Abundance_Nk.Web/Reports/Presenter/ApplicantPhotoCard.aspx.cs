﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Abundance_Nk.Business;
using Abundance_Nk.Model.Model;
using System.Configuration;
using Microsoft.Reporting.WebForms;
using Abundance_Nk.Web.Models;

namespace Abundance_Nk.Web.Reports.Presenter
{
    public partial class ApplicantPhotoCard : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                lblMessage.Text = "";

                if (!IsPostBack)
                {
                    rblSortOption.SelectedIndex = 0;
                    ddlDepartment.Visible = false;
                    PopulateAllDropDown();
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message;
            }
        }

        public Session SelectedSession
        {
            get { return new Session() { Id = Convert.ToInt32(ddlSession.SelectedValue), Name = ddlSession.SelectedItem.Text }; }
            set { ddlSession.SelectedValue = value.Id.ToString(); }
        }

        public Programme Programme
        {
            get { return new Programme() { Id = Convert.ToInt32(ddlProgramme.SelectedValue), Name = ddlProgramme.SelectedItem.Text }; }
            set { ddlProgramme.SelectedValue = value.Id.ToString(); }
        }

        public Department Department
        {
            get { return new Department() { Id = Convert.ToInt32(ddlDepartment.SelectedValue), Name = ddlDepartment.SelectedItem.Text }; }
            set { ddlDepartment.SelectedValue = value.Id.ToString(); }
        }
        
        public int Option
        {
            get { return Convert.ToInt32(rblSortOption.SelectedValue); }
            set { rblSortOption.SelectedValue = value.ToString(); }
        }
        
        private void DisplayReportBy(Session session, Programme programme, Department department, SortOption sortOption)
        {
            try
            {
                List<PhotoCard> photoCards = null;
                ApplicationFormLogic applicationformLogic = new ApplicationFormLogic();
                photoCards = applicationformLogic.GetPostJAMBApplicationsBy(session, programme, department, sortOption);

                //if (programme.Id == -100)
                //{
                //    photoCards = applicationformLogic.GetPostJAMBApplications(session);
                //}
                //else
                //{
                //    photoCards = applicationformLogic.GetPostJAMBApplicationsBy(session, programme);
                //}

                string bind_dsPhotoCard = "dsPhotoCard";
                string reportPath = @"Reports\PhotoCard.rdlc";

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
                rv.LocalReport.DisplayName = "Applicant Photo Card";
                rv.LocalReport.ReportPath = reportPath;
                rv.LocalReport.EnableExternalImages = true;

                if (photoCards != null)
                {
                    rv.ProcessingMode = ProcessingMode.Local;
                    rv.LocalReport.DataSources.Add(new ReportDataSource(bind_dsPhotoCard.Trim(), photoCards));
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
                Utility.BindDropdownItem(ddlSession, Utility.GetAllSessions(), Utility.ID, Utility.NAME);
                Utility.BindDropdownItem(ddlProgramme, Utility.GetAllProgrammes(), Utility.ID, Utility.NAME);
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
                if (SelectedSession == null || SelectedSession.Id <= 0)
                {
                    lblMessage.Text = "Please select Session";
                    return true;
                }
                else if (Programme == null || Programme.Id <= 0)
                {
                    lblMessage.Text = "Please select Programme";
                    return true;
                }
                else if (Department == null || Department.Id <= 0)
                {
                    lblMessage.Text = "Please select Department";
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                throw;
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
                DisplayReportBy(SelectedSession, Programme, Department, sortOption);
            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message;
            }
        }

        protected void ddlProgramme_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (Programme != null && Programme.Id > 0)
                {
                    PopulateDepartmentDropdownByProgramme(Programme);
                }
                else
                {
                    ddlDepartment.Visible = false;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message;
            }
        }

        private void PopulateDepartmentDropdownByProgramme(Programme programme)
        {
            try
            {
                List<Department> departments = Utility.GetDepartmentByProgramme(programme);
                if (departments != null && departments.Count > 0)
                {
                    Utility.BindDropdownItem(ddlDepartment, Utility.GetDepartmentByProgramme(programme), Utility.ID, Utility.NAME);
                    ddlDepartment.Visible = true;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message;
            }
        }








    }
}