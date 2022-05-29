﻿using Abundance_Nk.Business;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Web.Models;
using Ionic.Zip;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Abundance_Nk.Web.Reports.Presenter
{
    public partial class FeesPaymentReport : System.Web.UI.Page
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
                if (SelectedSession == null || SelectedSession.Id <= 0 || SelectedDepartment == null || SelectedDepartment.Id <= 0 || SelectedProgramme == null || SelectedProgramme.Id <= 0 || SelectedLevel == null || SelectedLevel.Id <= 0)
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
        private bool InvalidBulkDownloadInput()
        {
            try
            {
                if (SelectedSession == null || SelectedSession.Id <= 0 ||  SelectedProgramme == null || SelectedProgramme.Id <= 0 || SelectedLevel == null || SelectedLevel.Id <= 0)
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
            Programme programme = new Programme() { Id = Convert.ToInt32(ddlProgramme.SelectedValue) };
            DepartmentLogic departmentLogic = new DepartmentLogic();
            departments = departmentLogic.GetBy(programme);
            Utility.BindDropdownItem(ddlDepartment, departments, Utility.ID, Utility.NAME);
            ddlDepartment.Visible = true;
        }
        private void DisplayReportBy(Session session, Programme programme, Department department, Level level)
        {
            try
            {

                PaymentLogic paymentLogic = new PaymentLogic();
                var report = paymentLogic.GetAllFeesPaymentBy(session, programme, department, level);

                string bind_dsFeesPayment = "dsFeesPayment";
                string reportPath = @"Reports\PaymentReports\FeesPaymentReport.rdlc";


                ReportViewer1.Reset();
                ReportViewer1.LocalReport.DisplayName = "Fees Payment Report";
                ReportViewer1.LocalReport.ReportPath = reportPath;

                if (report != null)
                {
                    ReportViewer1.ProcessingMode = ProcessingMode.Local;
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource(bind_dsFeesPayment.Trim(), report));
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
        private void BulkReportDownloadBy(Session session, Programme programme, Level level)
        {
            try
            {

                PaymentLogic paymentLogic = new PaymentLogic();
                DepartmentLogic departmentLogic = new DepartmentLogic();
                departments = departmentLogic.GetBy(programme);
                string downloadPath = "~/Content/paymenttemp" + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + DateTime.Now.Millisecond;
                if (Directory.Exists(Server.MapPath(downloadPath)))
                {
                    Directory.Delete(Server.MapPath(downloadPath), true);
                }
                else
                {
                    DirectoryInfo folder = Directory.CreateDirectory(Server.MapPath(downloadPath));
                    int filesInFolder = folder.GetFiles().Count();
                    if (filesInFolder > 0)
                    {
                        //complete the code
                    }
                }
                foreach (var department in departments)
                {
                    var report = paymentLogic.GetAllFeesPaymentBy(session, programme, department, level);

                    string bind_dsFeesPayment = "dsFeesPayment";
                    string reportPath = @"Reports\PaymentReports\FeesPaymentReport.rdlc";
                    Warning[] warnings;
                    string[] streamIds;
                    string mimeType = string.Empty;
                    string encoding = string.Empty;
                    string extension = string.Empty;

                    ReportViewer1.Reset();
                    ReportViewer1.LocalReport.DisplayName = "Fees Payment Report";
                    ReportViewer1.LocalReport.ReportPath = reportPath;

                    if (report != null)
                    {
                        ReportViewer1.ProcessingMode = ProcessingMode.Local;
                        ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource(bind_dsFeesPayment.Trim(), report));
                        ReportViewer1.LocalReport.Refresh();
                        //
                        byte[] bytes = ReportViewer1.LocalReport.Render("PDF", null, out mimeType, out encoding, out extension, out streamIds, out warnings);

                        string path = Server.MapPath(downloadPath);
                        var departmentNameModified= department.Name.Replace('/', '_');
                        string savelocation = Path.Combine(path, departmentNameModified + ".pdf");
                        File.WriteAllBytes(savelocation, bytes);
                    }
                }
                using (ZipFile zip = new ZipFile())
                {
                    string file = Server.MapPath(downloadPath);
                    zip.AddDirectory(file, "");
                    string zipFileName = programme.Name.Replace('/', '_');
                    zip.Save(file + zipFileName + ".zip");
                    string export = downloadPath + zipFileName + ".zip";

                    Response.Redirect(export, false);
                    //UrlHelper urlHelp = new UrlHelper(HttpContext.Current.Request.RequestContext);
                    //Response.Redirect(urlHelp.Action("DownloadZip", new { controller = "Result", area = "Admin", downloadName = department.Name }), false);

                    return;
                }

            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message + ex.InnerException.Message; ;
            }
        }

        protected void Bulk_Download(object sender, EventArgs e)
        {
            try
            {
                if (InvalidBulkDownloadInput())
                {
                    lblMessage.Text = "All fields must be selected";
                    return;
                }

                BulkReportDownloadBy(SelectedSession, SelectedProgramme, SelectedLevel);

            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message + ex.InnerException.Message; ;
            }
        }
    }
}