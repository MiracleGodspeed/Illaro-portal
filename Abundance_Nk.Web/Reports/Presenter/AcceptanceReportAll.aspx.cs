using Abundance_Nk.Business;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Web.Models;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Ionic.Zip;

namespace Abundance_Nk.Web.Reports.Presenter
{
  
    public partial class AcceptanceReportAll : System.Web.UI.Page
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
        private void DisplayReport(Session session, Department department, Programme programme)
        {
            try
            {
                PaymentLogic paymentLogic = new PaymentLogic();
                var report = paymentLogic.GetAcceptanceReport(session, department, programme, txtBoxDateFrom.Text, txtBoxDateTo.Text);
                string bind_dsStudentPaymentSummary = "DataSet";
                string reportPath = @"Reports\AcceptanceReport.rdlc";


                ReportViewer1.Reset();
                ReportViewer1.LocalReport.DisplayName = "Acceptance Report ";
                ReportViewer1.LocalReport.ReportPath = reportPath;

                if (report != null)
                {
                    ReportViewer1.ProcessingMode = ProcessingMode.Local;
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource(bind_dsStudentPaymentSummary.Trim(), report));
                    ReportViewer1.LocalReport.Refresh();
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message + ex.InnerException.Message; 
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
        protected void ddlProgramme_SelectedIndexChanged1(object sender, EventArgs e)
        {
            Programme programme = new Programme() { Id = Convert.ToInt32(ddlProgramme.SelectedValue) };
            DepartmentLogic departmentLogic = new DepartmentLogic();
            departments = departmentLogic.GetBy(programme);
            Utility.BindDropdownItem(ddlDepartment, departments, Utility.ID, Utility.NAME);
            ddlDepartment.Visible = true;
        }
        protected void Display_Button_Click(object sender, EventArgs e)
        {
            if (InvalidUserInput())
            {
                lblMessage.Text = "All fields must be selected";
                return;
            }


            DisplayReport(SelectedSession, SelectedDepartment, SelectedProgramme);
        
        }

        protected void btBulk_Click(object sender, EventArgs e)
        {
            try
            {
                if (SelectedSession == null || SelectedSession.Id <= 0)
                {
                    lblMessage.Text = "Please select Session";
                    return;
                }

                ProgrammeLogic programmeLogic = new ProgrammeLogic();
                DepartmentLogic departmentLogic = new DepartmentLogic();

                List<Programme> programmes = programmeLogic.GetAll();

                string downloadPath = "~/Content/temp" + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + DateTime.Now.Millisecond;

                if (Directory.Exists(Server.MapPath(downloadPath)))
                {
                    Directory.Delete(Server.MapPath(downloadPath), true);
                }

                Directory.CreateDirectory(Server.MapPath(downloadPath));

                string zipName = "Acceptance Report ";

                for (int i = 0; i < programmes.Count; i++)
                {
                    Programme currentProgramme = programmes[i];
                    List<Department> departments = departmentLogic.GetBy(currentProgramme);

                    for (int j = 0; j < departments.Count; j++)
                    {
                        Department currentDepartment = departments[j];
                        PaymentLogic paymentLogic = new PaymentLogic();
                        var report = paymentLogic.GetAcceptanceReport(SelectedSession, currentDepartment, currentProgramme, txtBoxDateFrom.Text, txtBoxDateTo.Text);

                        if (report.Count > 0)
                        {
                            Warning[] warnings;
                            string[] streamIds;
                            string mimeType = string.Empty;
                            string encoding = string.Empty;
                            string extension = string.Empty;

                            string bind_dsStudentPaymentSummary = "DataSet";
                            string reportPath = @"Reports\AcceptanceReport.rdlc";

                            ReportViewer rptViewer = new ReportViewer();
                            rptViewer.Visible = false;
                            rptViewer.Reset();
                            rptViewer.LocalReport.DisplayName = "Acceptance Report";
                            rptViewer.ProcessingMode = ProcessingMode.Local;
                            rptViewer.LocalReport.ReportPath = reportPath;
                            rptViewer.LocalReport.EnableExternalImages = true;
                            rptViewer.LocalReport.DataSources.Add(new ReportDataSource(bind_dsStudentPaymentSummary.Trim(), report));

                            byte[] bytes = rptViewer.LocalReport.Render("PDF", null, out mimeType, out encoding, out extension, out streamIds, out warnings);

                            string path = Server.MapPath(downloadPath);
                            string savelocation = Path.Combine(path, currentProgramme.Name.Replace("/", "_") + " " + currentDepartment.Name.Replace("/", "_") + ".pdf");
                            File.WriteAllBytes(savelocation, bytes);
                        }
                    }
                }
                using (ZipFile zip = new ZipFile())
                {
                    string file = Server.MapPath(downloadPath);
                    zip.AddDirectory(file, "");
                    string zipFileName = zipName;
                    zip.Save(file + zipFileName + ".zip");
                    string export = downloadPath + zipFileName + ".zip";

                    Response.Redirect(export, false);
                    return;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message + ex.InnerException.Message; 
            }
        }

    }
}