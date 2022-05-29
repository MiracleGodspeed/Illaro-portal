﻿using Abundance_Nk.Business;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Web.Models;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Abundance_Nk.Web.Reports.Presenter
{
    public partial class CourseRegistrationReport : System.Web.UI.Page
    {
        List<Department> departments;
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                lblMessage.Text = "";

                if (!IsPostBack)
                {
                    Utility.BindDropdownItem(ddlSession, Utility.GetAllSessions(), Utility.ID, Utility.NAME);

                    Utility.BindDropdownItem(ddlProgramme, Utility.GetAllProgrammes(), Utility.ID, Utility.NAME);

                    Utility.BindDropdownItem(ddlLevel, Utility.GetAllLevels(), Utility.ID, Utility.NAME);

                    ddlDepartment.Visible = false;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message + ex.InnerException.Message;
            }
        }
        public Session SelectedSession
        {
            get { return new Session() { Id = Convert.ToInt32(ddlSession.SelectedValue), Name = ddlSession.SelectedItem.Text }; }
            set { ddlSession.SelectedValue = value.Id.ToString(); }
        }
        public Programme SelectedProgramme
        {
            get { return new Programme() { Id = Convert.ToInt32(ddlProgramme.SelectedValue), Name = ddlProgramme.SelectedItem.Text }; }
            set { ddlProgramme.SelectedValue = value.Id.ToString(); }
        }
        public Department SelectedDepartment
        {
            get { return new Department() { Id = Convert.ToInt32(ddlDepartment.SelectedValue), Name = ddlDepartment.SelectedItem.Text }; }
            set { ddlDepartment.SelectedValue = value.Id.ToString(); }
        }
        public Level SelectedLevel
        {
            get { return new Level() { Id = Convert.ToInt32(ddlLevel.SelectedValue), Name = ddlLevel.SelectedItem.Text }; }
            set { ddlLevel.SelectedValue = value.Id.ToString(); }
        }


        private bool InvalidUserInput()
        {
            try
            {
                if (SelectedSession == null || SelectedSession.Id <= 0 || SelectedDepartment == null || SelectedDepartment.Id <= 0 || SelectedProgramme == null || SelectedProgramme.Id <= 0 )
                {
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }
        protected void ddlProgramme_SelectedIndexChanged1(object sender, EventArgs e)
        {
            try
            {
                Programme programme = new Programme() { Id = Convert.ToInt32(ddlProgramme.SelectedValue) };
                DepartmentLogic departmentLogic = new DepartmentLogic();
                departments = departmentLogic.GetBy(programme);
                Utility.BindDropdownItem(ddlDepartment, departments, Utility.ID, Utility.NAME);
                ddlDepartment.Visible = true;
            }
            catch (Exception)
            {                
                throw;
            }
            
        }
        private void DisplayReportBy(Session session, Programme programme, Department department, Level level)
        {
            try
            {
                CourseRegistrationDetailLogic courseRegistrationDetailLogic = new CourseRegistrationDetailLogic();
                var report = courseRegistrationDetailLogic.GetCourseRegistrationBy(session, programme, department, level);

                string bind_dsCourseRegistration = "dsCourseRegistration";
                string reportPath = @"Reports\CourseRegistrationReport.rdlc";


                ReportViewer1.Reset();
                ReportViewer1.LocalReport.DisplayName = "Course Registration Report";
                ReportViewer1.LocalReport.ReportPath = reportPath;

                if (report != null)
                {
                    ReportViewer1.ProcessingMode = ProcessingMode.Local;
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource(bind_dsCourseRegistration.Trim(), report));
                    ReportViewer1.LocalReport.Refresh();
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message + ex.InnerException.Message; ;
            }
        }



        protected void Display_Button_Click1(object sender, EventArgs e)
        {
            try
            {
                if (InvalidUserInput())
                {
                    lblMessage.Text = "All fields must be selected";
                    return;
                }

                DisplayReportBy(SelectedSession, SelectedProgramme, SelectedDepartment, SelectedLevel);

            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message + ex.InnerException.Message; ;
            }
        }

        //protected void ddlSession_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        //Session session = new Session() { Id = Convert.ToInt32(ddlSession.SelectedValue) };
        //        //SemesterLogic semesterLogic = new SemesterLogic();
        //        //SessionSemesterLogic sessionSemesterLogic = new SessionSemesterLogic();
        //        //sessionSemesterList = sessionSemesterLogic.GetModelsBy(p => p.Session_Id == session.Id);

        //        //semesters = new List<Semester>();
        //        //foreach (SessionSemester item in sessionSemesterList)
        //        //{
        //        //    semesters.Add(item.Semester);
        //        //}
        //        ////SemesterLogic SemesterLogic = new SemesterLogic();
        //        ////semesters = SemesterLogic.GetAll();
        //        //Utility.BindDropdownItem(ddlSemester, semesters, Utility.ID, Utility.NAME);
        //        //ddlSemester.Visible = true;
        //    }
        //    catch (Exception)
        //    {
                
        //        throw;
        //    }
            
        //}

        protected void rbnDepartment_CheckedChanged(object sender, EventArgs e)
        {
            ddlLevel.Visible = false;
        }

        protected void rbnLevel_CheckedChanged(object sender, EventArgs e)
        {
            ddlLevel.Visible = true;
        }
    }
}