using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Web;
using System.Web.Mvc;
using Abundance_Nk.Model.Entity.Model;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Web.Models;

namespace Abundance_Nk.Web.Areas.Admin.ViewModels
{
    public class StudentCourseRegistrationViewModel
    {
        public StudentCourseRegistrationViewModel()
        {
            ProgrammeSelectList = Utility.PopulateAllProgrammeSelectListItem();
            SessionSelectList = Utility.PopulateAllSessionSelectListItem();
            SemesterSelectList = Utility.PopulateSemesterSelectListItem();
            LevelList = Utility.GetAllLevels();
        }
        public List<StudentDeferementLog> StudentDeferementLogs { get; set; }
        public StudentDeferementLog StudentDeferementLog { get; set; }
        public Course Course { get; set; }
        public Department Department { get; set; }
        public DepartmentOption DepartmentOption { get; set; }
        public int OptionId { get; set; }
        public Level Level { get; set; }
        public Session Session { get; set; }
        public Programme Programme { get; set; }
        public Semester Semester { get; set; }
        public Model.Model.Student Student { get; set; }
        public StudentLevel StudentLevel { get; set; }
        public CourseRegistration CourseRegistration { get; set; }
        public CourseRegistrationDetail CourseRegistrationDetail { get; set; }
        public List<Level> LevelList { get; set; }
        public List<Course> Courses { get; set; }
        public List<Payment> Payments { get; set; }
        public List<StudentLevel> StudentLevelList { get; set; }
        public List<SelectListItem> SemesterSelectList { get; set; }
        public List<SelectListItem> ProgrammeSelectList { get; set; }
        public List<SelectListItem> SessionSelectList { get; set; }
        public List<CourseRegistration> CourseRegistrations { get; set; }
        public List<PaymentEtranzact> PaymentEtranzacts { get; set; }
        public PaymentEtranzact PaymentEtranzact { get; set; }
        public Decimal Amount { get; set; }
        public bool IsExtraYear { get; set; }
        public List<CourseRegistrationDetailAudit> CourseRegistrationDetailAuditList { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
    }

    public class JsonPostResult
    {
        public bool IsError { get; set; }
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }
    }
}