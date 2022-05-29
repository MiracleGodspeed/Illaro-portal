using Abundance_Nk.Model.Entity.Model;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Abundance_Nk.Web.Areas.Applicant.ViewModels
{
    public class ClearanceViewModel
    {
        public ClearanceViewModel()
        {
            
            Abundance_Nk.Business.AdmissionCriteriaLogic admissionCriteriaLogic = new Business.AdmissionCriteriaLogic();
            OLevelGradeSelectList = Utility.PopulateOLevelGradeSelectListItem();
            admissionCriteriaList = admissionCriteriaLogic.GetAll();
        }
        public ClearanceStatus ClearanceStatus { get; set; }
        public ClearanceDisputes ClearanceDisputes { get; set; }
        public Model.Model.Student Student { get; set; }
        public Model.Model.StudentLevel StudentLevel { get; set; }
        public ClearanceLog ClearanceLog { get; set; }
        public bool ViewPanel { get; set; }
        public List<ClearanceLog> ClearanceLogs { get; set; }
        public List<ClearanceUnit> ClearanceUnits { get; set; }
        public List<ClearanceDisputes> ClearanceDisputesList { get; set; }
        public HttpPostedFileBase MyFile { get; set; }
        public bool ViewPrintButton { get; set; }
        public Model.Model.Programme Programme { get; set; }
        public Model.Model.Department Department { get; set; }
        public List<SelectListItem> OLevelSubjectSelectList { get; set; }
        public List<AdmissionCriteria> admissionCriteriaList { get; set; }
        public List<AdmissionCriteriaForOLevelSubject> admissionCriteriaForOLevelSubject { get; set; }
        public List<AdmissionCriteriaForOLevelType> admissionCriteriaForOLevelType { get; set; }
        public List<SelectListItem> OLevelGradeSelectList { get; set; }
        public OLevelSubject OLevelSubject { get; set; }
        public AdmissionCriteriaForOLevelSubject AdmissionCriteriaForOLevelSubjectModel { get; set; }


    }
    public class CriteriaJsonResult
    {
        public bool IsError { get; set; }
        public string Message { get; set; }
        public string Subject { get; set; }
        public string Alternatives { get; set; }
    }
}