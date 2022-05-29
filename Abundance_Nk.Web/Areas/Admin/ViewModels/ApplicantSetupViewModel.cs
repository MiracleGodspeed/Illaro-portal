using Abundance_Nk.Business;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Abundance_Nk.Web.Areas.Admin.ViewModels
{
    public class ApplicantSetupViewModel
    {
        public AppliedCourseLogic appliedCourseLogic;
        public PersonLogic personLogic;
        private DepartmentLogic departmentLogic;
        public ApplicantSetupViewModel()
        {
            appliedCourseLogic = new AppliedCourseLogic();
            personLogic = new PersonLogic();
            departmentLogic = new DepartmentLogic();
            SessionSelectListItem = Utility.PopulateSessionSelectListItem();
            ProgrammeSelectListItem = Utility.PopulateAllProgrammeSelectListItem();
            AdmissionListTypeSelectListItem = Utility.PopulateAdmissionListTypeSelectListItem();
            if (Programme != null && Programme.Id > 0)
            {
                DepartmentSelectListItem = Utility.PopulateDepartmentSelectListItem(Programme);

            }
            LevelSelectListItem = Utility.PopulateLevelSelectListItem();


        }
        public Department Department { get; set; }
        public Programme Programme { get; set; }
        public Session CurrentSession { get; set; }
        public DepartmentOption DepartmentOption { get; set; }
        public bool IsUploadFailed { get; set; } = false;
        public Level Level { get; set; }
        [Display(Name = "Admission List Type")]
        public AdmissionListType AdmissionListType { get; set; }
        public AdmissionListBatch AdmissionListBatch { get; set; }
        public List<SelectListItem> AdmissionListTypeSelectListItem { get; set; }
        public List<SelectListItem> DepartmentSelectListItem { get; set; }
        public List<SelectListItem> DepartmentOpionSelectListItem { get; set; }
        public List<SelectListItem> ProgrammeSelectListItem { get; set; }
        public List<SelectListItem> SessionSelectListItem { get; set; }
        public List<SelectListItem> LevelSelectListItem { get; set; }
        public AppliedCourse AppliedCourse { get; set; }
        public Person Person { get; set; }
        public HttpPostedFileBase File { get; set; }
        public List<AdmissionList> AdmissionList { get; set; }
        public List<FailedUploads> FailedUploadList { get; set; }
        public List<FailedJambOlevelUploads> FailedOlevelUploadList { get; set; }
        public AdmissionList AdmissionListDetail { get; set; }
        public List<AppliedCourse> AppliedCourseList { get; set; }
        public List<JambRecord> JambRecordList { get; set; }
        public List<AdmissionList> AdmiissionLists { get; set; }
        public List<SelectListItem> DepartmentSelectList { get; set; }

        [Display(Name = "Exam No / Application No")]
        public string SearchString { get; set; }
        public bool IsBulk { get; set; }

    }
    public class SampleJambRecordUpload
    {
        //public string SN { get; set; }
        public string JAMBRegNumber { get; set; }
        public string Fullname { get; set; }
        public string Sex { get; set; }
        public string State_Id { get; set; }
        public string JambScore { get; set; }
        public string Course_Name { get; set; }
        public string LGA_Name { get; set; }
        public string Subject1_Id { get; set; }
        public string Subject1_Score { get; set; }
        public string Subject2_Id { get; set; }
        public string Subject2_Score { get; set; }
        public string Subject3_Id { get; set; }
        public string Subject3_Score { get; set; }
        //public string Subject4_Id { get; set; }
        public string EngScore { get; set; }

    }

    public class SampleJambOlevelUpload
    {
        public string RegNum { get; set; }
        public string SubjectName { get; set; }
        public string Grade { get; set; }
        public string ExamSeries { get; set; }
        public string ExamYear { get; set; }
        public string ExamType { get; set; }
        public string ExamNumber { get; set; }
        public string DateCreated { get; set; }
        public string InstName { get; set; }


    }

    public class FailedUploads
    {
        public string SN { get; set; }
        public string JAMBRegNumber { get; set; }
        public string Fullname { get; set; }
        public string Reason { get; set; }

    }
    public class FailedJambOlevelUploads
    {
        public string SN { get; set; }
        public string JAMBRegNumber { get; set; }
        public string Reason { get; set; }

    }
}