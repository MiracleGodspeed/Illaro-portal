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

    public partial class StudentReportAlt : System.Web.UI.Page
    {
        List<Department> departments;
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                lblMessage.Text = "";

                if (!IsPostBack)
                {
                    List<Session> sessions = Utility.GetAllSessions().Where(s => s.Id == 0 || s.Id == 7).ToList();
                    Utility.BindDropdownItem(ddlSession, sessions, Utility.ID, Utility.NAME);

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
        protected void Display_Button_Click1(object sender, EventArgs e)
        {
            try
            {
                if (InvalidUserInput())
                {
                    lblMessage.Text = "All fields must be selected";
                    return;
                }

                DisplayReportBy(SelectedSession, SelectedDepartment, SelectedProgramme,SelectedLevel);
            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message + ex.InnerException.Message; ;
            }
        }
        private bool InvalidUserInput()
        {
            try
            {
                if (SelectedSession == null || SelectedSession.Id <= 0 || SelectedDepartment == null || SelectedDepartment.Id <= 0 || SelectedProgramme == null || SelectedProgramme.Id <= 0)
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
        private void DisplayReportBy(Session session, Department department, Programme programme,Level level)
        {
            try
            {
                StudentLogic studentLogic = new StudentLogic();
                List<Abundance_Nk.Model.Model.StudentReport> studentReportList = studentLogic.GetRegisteredStudentByCapacity(department, programme, session, level);

                string bind_dsStudentPaymentSummary = "dsStudentReport";
                string reportPath = @"Reports\StudentMatriculationReport.rdlc";

                ReportViewer1.Reset();
                ReportViewer1.LocalReport.DisplayName = "Student Matriculation Report ";
                ReportViewer1.LocalReport.ReportPath = reportPath;

                if (studentReportList != null)
                {
                    ReportViewer1.ProcessingMode = ProcessingMode.Local;
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource(bind_dsStudentPaymentSummary.Trim(), studentReportList));
                    ReportViewer1.LocalReport.Refresh();
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message + ex.InnerException.Message; ;
            }
        }

       

        protected void ddlProgramme_SelectedIndexChanged1(object sender, EventArgs e)
        {
            Programme programme = new Programme() { Id = Convert.ToInt32(ddlProgramme.SelectedValue) };
            DepartmentLogic departmentLogic = new DepartmentLogic();
            departments = departmentLogic.GetBy(programme);
            Utility.BindDropdownItem(ddlDepartment, departments, Utility.ID, Utility.NAME);
            ddlDepartment.Visible = true;
        }

       


    }
}