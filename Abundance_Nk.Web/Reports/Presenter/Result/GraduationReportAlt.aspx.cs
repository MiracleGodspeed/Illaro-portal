﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Abundance_Nk.Model.Model;
using Abundance_Nk.Business;
using System.Configuration;
using Abundance_Nk.Web.Models;
using Microsoft.Reporting.WebForms;
using Abundance_Nk.Web.Models.Intefaces;


namespace Abundance_Nk.Web.Reports.Presenter.Result
{
    public partial class GraduationReportAlt : System.Web.UI.Page, IReport
    {
        public string Message
        {
            get { return lblMessage.Text; }
            set { lblMessage.Text = value; }
        }

        public ReportViewer Viewer
        {
            get { return rv; }
            set { rv = value; }
        }

        public int ReportType
        {
            get { throw new NotImplementedException(); }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                Message = "";

                if (!IsPostBack)
                {
                    rblSortOption.SelectedIndex = 0;
                    ddlDepartment.Visible = false;
                    ddlDepartmentOption.Visible = false;
                    PopulateAllDropDown();
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message;
            }
        }

        public SessionSemester SelectedSession
        {
            get { return new SessionSemester() { Id = Convert.ToInt32(ddlSession.SelectedValue), Name = ddlSession.SelectedItem.Text }; }
            set { ddlSession.SelectedValue = value.Id.ToString(); }
        }

        public Level Level
        {
            get { return new Level() { Id = Convert.ToInt32(ddlLevel.SelectedValue), Name = ddlLevel.SelectedItem.Text }; }
            set { ddlLevel.SelectedValue = value.Id.ToString(); }
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

        public DepartmentOption DepartmentOption
        {
            get { return new DepartmentOption() { Id = Convert.ToInt32(ddlDepartmentOption.SelectedValue), Name = ddlDepartmentOption.SelectedItem.Text }; }
            set { ddlDepartmentOption.SelectedValue = value.Id.ToString(); }
        }

        public int Type
        {
            get { return Convert.ToInt32(rblSortOption.SelectedValue); }
            set { rblSortOption.SelectedValue = value.ToString(); }
        }

        private void DisplayReportBy(SessionSemester session, Level level, Programme programme, Department department, int type)
        {
            try
            {
                List<Model.Model.Result> resultList = new List<Model.Model.Result>();
                StudentResultLogic resultLogic = new StudentResultLogic();
                SessionSemesterLogic sessionSemesterLogic = new SessionSemesterLogic();
                session = sessionSemesterLogic.GetBy(session.Id);

                if (DepartmentOption != null && DepartmentOption.Id > 0)
                {
                    resultList = resultLogic.GetGraduationListByOptionCapacity(session, level, programme, department, DepartmentOption, type);
                }
                else
                {
                    resultList = resultLogic.GetGraduationListByCapacity(session, level, programme, department, type);
                }
                   
               
                string reportPath = "";
                string bind_ds = "dsMasterSheet";
                 
                if (type == 1)
                {
                    reportPath = @"Reports\Result\GraduationSheetByClassification.rdlc";
                }
                else
                {
                    reportPath = @"Reports\Result\GraduationSheet.rdlc";
                }

                rv.Reset();
                rv.LocalReport.DisplayName = "Graduation List";
                rv.LocalReport.ReportPath = reportPath;
                rv.LocalReport.EnableExternalImages = true;

                string programmeName = programme.Id > 2 ? "Higher National Diploma" : "National Diploma";
                ReportParameter programmeParam = new ReportParameter("Programme", programmeName);
                ReportParameter departmentParam = new ReportParameter("Department", department.Name);
                ReportParameter sessionSemesterParam = new ReportParameter("SessionSemester", session.Name);
                ReportParameter[] reportParams = new ReportParameter[] { departmentParam, programmeParam, sessionSemesterParam };
                rv.LocalReport.SetParameters(reportParams);

                if (resultList != null)
                {
                    rv.ProcessingMode = ProcessingMode.Local;
                    rv.LocalReport.DataSources.Add(new ReportDataSource(bind_ds.Trim(), resultList));
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
                List<SessionSemester> sessionSemesters = Utility.GetAllSessionSemesters();
                if(sessionSemesters != null && sessionSemesters.Count > 0){
                    sessionSemesters = sessionSemesters.Where(s => s.Id == 0 ||  (s.Session == null || s.Session.Id == 7)).ToList();
                }
                Utility.BindDropdownItem(ddlSession, sessionSemesters, Utility.ID, Utility.NAME);
                Utility.BindDropdownItem(ddlLevel, Utility.GetFinalLevels(), Utility.ID, Utility.NAME);
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

                DisplayReportBy(SelectedSession, Level, Programme, Department, Type);
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
        private void PopulateDepartmentOptionDropdownByDepartment(Department department,Programme programme)
        {
            try
            {
                List<DepartmentOption> departments = Utility.GetDepartmentByOptionByDepartmentProgramme(department,programme);
                if (departments != null && departments.Count > 0)
                {
                    Utility.BindDropdownItem(ddlDepartmentOption, Utility.GetDepartmentByOptionByDepartmentProgramme(department,programme), Utility.ID, Utility.NAME);
                    ddlDepartmentOption.Visible = true;
                }
                else
                {
                    DepartmentOption departmentOption = new DepartmentOption(){Id =-1,Name = "No item to be displayed"};
                    List<DepartmentOption> emptyList = new List<DepartmentOption>();
                    emptyList.Add(departmentOption);
                    Utility.BindDropdownItem(ddlDepartmentOption,emptyList, Utility.ID, Utility.NAME);
                    
                    ddlDepartmentOption.Visible = false;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message;
            }
        }
        protected void ddlDepartment_SelectedIndexChanged(object sender,EventArgs e)
        {
            try
            {
                if (Programme != null && Programme.Id > 0 && Department != null && Department.Id > 0)
                {
                    PopulateDepartmentOptionDropdownByDepartment(Department,Programme);
                }
                else
                {
                    ddlDepartmentOption.Visible = false;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message;
            }
        }
    }
}