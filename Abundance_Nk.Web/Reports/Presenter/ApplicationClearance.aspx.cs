using Abundance_Nk.Business;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Web.Models;
using Ionic.Zip;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Abundance_Nk.Web.Reports.Presenter
{
    public partial class ApplicationClearance : System.Web.UI.Page
    {
        
        public Session SelectedSession
        {
            get
            {
                return new Session
                {
                    Id = Convert.ToInt32(ddlSession.SelectedValue),
                    Name = ddlSession.SelectedItem.Text
                };
            }
            set { ddlSession.SelectedValue = value.Id.ToString(); }
        }

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
        public Session session
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
        public int Type
        {
            get { return Convert.ToInt32(rblSortOption.SelectedValue); }
            set { rblSortOption.SelectedValue = value.ToString(); }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                Message = "";

                if (!IsPostBack)
                {
                    PopulateAllDropDown();
                    ddlSession.Visible = true;
                    btnDisplayReport.Visible = true;
                    ddlDepartment.Visible = false;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message;
            }
        }
        private async void DisplayReportBy(Session session, Programme programme, Department department, int type)
        {
            try
            {
                ApplicationFormLogic applicationFormLogic = new ApplicationFormLogic();
                List<ApplicationApprovalModel> applicationApprovalModelList = await applicationFormLogic.GetApplicationApproval(session, programme, department, type);

                string bind_ds = "dsApplicationClearance";
                string reportPath = @"Reports\PendingApplicationQualificationReport.rdlc";
                
                rv.Reset();
                if (type == 1)
                {
                    reportPath = @"Reports\ApplicationQualificationReport.rdlc";
                }
                else if (type == 2)
                {
                    reportPath = @"Reports\UnqualifiedApplicationQualificationReport.rdlc";
                }
                else
                {
                    reportPath = @"Reports\PendingApplicationQualificationReport.rdlc";
                }
                rv.LocalReport.DisplayName = "Applicant Qualification Report";
                rv.LocalReport.ReportPath = reportPath;
                rv.LocalReport.EnableExternalImages = true;

                //string programmeName = programme.Id > 2 ? "Higher National Diploma" : "National Diploma";
                //if (applicationApprovalModelList?.Count > 0)
                //{
                //    applicationApprovalModelList.FirstOrDefault().ProgrammeName = programmeName;
                //}

                if (applicationApprovalModelList != null && applicationApprovalModelList.Count > 0)
                {
                    rv.ProcessingMode = ProcessingMode.Local;
                    rv.LocalReport.DataSources.Add(new ReportDataSource(bind_ds.Trim(), applicationApprovalModelList));

                    rv.LocalReport.Refresh();
                    rv.Visible = true;
                }
                else
                {
                    lblMessage.Text = "No result to display";
                    rv.Visible = false;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message + ex.InnerException.Message;
            }
        }
        private async void BulkReportBy(Session session, Programme programme, int type)
        {
            try
            {
                ApplicationFormLogic applicationFormLogic = new ApplicationFormLogic();
                ProgrammeLogic programmeLogic = new ProgrammeLogic();
                var programmeName= programmeLogic.GetModelBy(f => f.Programme_Id == programme.Id);
                List<Department> departments = Utility.GetDepartmentByProgramme(programme);
                string downloadPath = "~/Content/reportApplicant" + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + DateTime.Now.Millisecond;

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
                foreach (var item in departments)
                {
                    Warning[] warnings;
                    string[] streamIds;
                    string mimeType = string.Empty;
                    string encoding = string.Empty;
                    string extension = string.Empty;
                    List<ApplicationApprovalModel> applicationApprovalModelList = await applicationFormLogic.GetApplicationApproval(session, programme, item, type);

                    string bind_ds = "dsApplicationClearance";
                    string reportPath = @"Reports\PendingApplicationQualificationReport.rdlc";

                    rv.Reset();
                    if (type == 1)
                    {
                        reportPath = @"Reports\ApplicationQualificationReport.rdlc";
                    }
                    else if (type == 2)
                    {
                        reportPath = @"Reports\UnqualifiedApplicationQualificationReport.rdlc";
                    }
                    else
                    {
                        reportPath = @"Reports\PendingApplicationQualificationReport.rdlc";
                    }
                    rv.LocalReport.DisplayName = "Applicant Qualification Report";
                    rv.LocalReport.ReportPath = reportPath;
                    rv.LocalReport.EnableExternalImages = true;

                    if (applicationApprovalModelList != null && applicationApprovalModelList.Count > 0)
                    {
                        rv.ProcessingMode = ProcessingMode.Local;
                        rv.LocalReport.DataSources.Add(new ReportDataSource(bind_ds.Trim(), applicationApprovalModelList));

                        rv.LocalReport.Refresh();
                        rv.Visible = true;

                        byte[] bytes = rv.LocalReport.Render("PDF", null, out mimeType, out encoding, out extension, out streamIds, out warnings);

                        string path = Server.MapPath(downloadPath);
                        string savelocation = Path.Combine(path, item.Name + ".pdf");
                        File.WriteAllBytes(savelocation, bytes);
                    }
                    
                }
                using (ZipFile zip = new ZipFile())
                {
                    string file = Server.MapPath(downloadPath);
                    zip.AddDirectory(file, "");
                    string zipFileName = programmeName.Name.Replace("/", "_");
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
                    lblMessage.Text = " Session not set! Please contact your system administrator.";
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
                

                DisplayReportBy(session, Programme, Department, Type);

            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message;
            }
        }
        protected void btnBulkReport_Click(object sender, EventArgs e)
        {
            try
            {
                if (InvalidUserInput())
                {
                    return;
                }


                BulkReportBy(session, Programme, Type);

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