using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Abundance_Nk.Business;
using Abundance_Nk.Model.Model;
using System.Web.Mvc;
using Abundance_Nk.Web.Models;

namespace Abundance_Nk.Web.Areas.Admin.ViewModels
{
    public class AdmissionProcessingViewModel
    {
        private ApplicantLogic applicantLogic;
        private ApplicationFormLogic applicationFormLogic;

        public AdmissionProcessingViewModel()
        {
            Session = new Session();
            applicantLogic = new ApplicantLogic();
            applicationFormLogic = new ApplicationFormLogic();
            ApplicationForms = new List<ApplicationForm>();
            ProgrammeSelectListItem = Utility.PopulateAllProgrammeSelectListItem();
            SessionSelectList = Utility.PopulateSessionSelectListItem();
            if (Programme != null && Programme.Id > 0)
            {
                DepartmentSelectListItem = Utility.PopulateDepartmentSelectListItem(Programme);

            }
        }

        public Session Session { get; set; }
        public Programme Programme { get; set; }
        public Department Department { get; set; }
        public List<Model.Model.Applicant> Applicants { set; get; }
        public List<ApplicationForm> ApplicationForms { get; set; }
        public List<SelectListItem> SessionSelectList { get; set; }
        public List<SelectListItem> ProgrammeSelectListItem { get; set; }
        public List<SelectListItem> DepartmentSelectListItem { get; set; }

        public List<ApplicationForm> GetApplicationsBy(bool rejected, Session session)
        {
            try
            {
                ApplicationForms = applicationFormLogic.GetModelsBy(af => af.Rejected == rejected && af.APPLICATION_FORM_SETTING.Session_Id == session.Id);
                return ApplicationForms;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void GetApplicantByStatus(ApplicantStatus.Status status)
        {
            try
            {
                Applicants = applicantLogic.GetModelsBy(a => a.Applicant_Status_Id == (int)status);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void GetApplicantDetailsByStatus(ApplicantStatus.Status status, Session session, Department department, Programme programme)
        {
            try
            {
                Applicants = applicantLogic.GetModelsBy(a => a.Applicant_Status_Id == (int)status && a.APPLICATION_FORM.APPLICATION_PROGRAMME_FEE.Session_Id == session.Id && a.APPLICATION_FORM.APPLICATION_PROGRAMME_FEE.Programme_Id == programme.Id);
            }
            catch (Exception)
            {
                throw;
            }
        }









    }




}