using Abundance_Nk.Model.Model;
using Abundance_Nk.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Abundance_Nk.Model.Entity.Model;

namespace Abundance_Nk.Web.Areas.Admin.ViewModels
{
    public class StaffCourseAllocationViewModel
    {
        public StaffCourseAllocationViewModel()
        {
            SessionSelectListResult = Utility.PopulateResultSessionSelectListItem();
            SessionSelectListAllocation = Utility.PopulateAllocationSessionSelectListItem();
            LevelSelectList = Utility.PopulateLevelSelectListItem();
            ProgrammeSelectList = Utility.PopulateProgrammeSelectListItem();
            LevelList = Utility.GetAllLevels();
            UserSelectList = Utility.PopulateStaffAndHODSelectListItem();
            PastSessionSelectList = Utility.PopulatePastSessionSelectListItem();
        }
        public List<SelectListItem> LevelSelectList { get; set; }
        public List<SelectListItem> ProgrammeSelectList { get; set; }
        public List<SelectListItem> SessionSelectListResult { get; set; }
        public List<SelectListItem> SessionSelectListAllocation { get; set; }
        public List<SelectListItem> UserSelectList { get; set; }
        public List<ResultFormat> resultFormatList { get; set; }
        public List<Level> LevelList { get; set; }
        public Level Level { get; set; }
        public Programme Programme { get; set; }
        public Department Department { get; set; }
        public Session Session { get; set; }
        public Semester Semester { get; set; }
        public Course Course { get; set; }
        public User User { get; set; }
        public CourseAllocation CourseAllocation { get; set; }
        public List<CourseAllocation> CourseAllocationList { get; set; }
        public long cid { get; set; }
        public List<CourseAllocation> CourseAllocations { get; set; }
        public List<UploadedCourseFormat> UploadedCourses { get; set; }
        public List<ExamRawScoreSheetReport> ExamRawScoreSheetReports { get; set; }
        public List<CourseRegistrationDetail> CourseRegistrationDetails { get; set; }
        public List<StudentExamRawScoreSheet> RawScoreSheets { get; set; }
        public List<ResultUpdateModel> ResultUpdates { get; set; }
        public DepartmentOption DepartmentOption { get; set; }
        public List<string> MatricNumbers { get; set; }
        public List<SelectListItem> PastSessionSelectList { get; set; }
    }
}