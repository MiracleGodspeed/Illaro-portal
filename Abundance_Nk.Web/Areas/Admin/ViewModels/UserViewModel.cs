using Abundance_Nk.Model.Model;
using Abundance_Nk.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Windows.Forms.VisualStyles;
using Abundance_Nk.Model.Entity.Model;

namespace Abundance_Nk.Web.Areas.Admin.ViewModels
{
    public class UserViewModel
    {
        public UserViewModel()
        {
            SexSelectList = Utility.PopulateSexSelectListItem();
            RoleSelectList = Utility.PopulateRoleSelectListItem();
            SecurityQuestionSelectList = Utility.PopulateSecurityQuestionSelectListItem();
            SessionSelectList = Utility.PopulateAllSessionSelectListItem();
            LevelSelectList = Utility.PopulateLevelSelectListItem();
            ProgrammeSelectList = Utility.PopulateProgrammeSelectListItem();
            DepartmentSelectList = Utility.PopulateAllDepartmentSelectListItem();
            CurrentSessionSelectList = Utility.PopulatePastSessionSelectListItem();
        }
        public User User { get; set; }
        public CourseAllocation CourseAllocation { get; set; }
        public List<User> Users { get; set; }
        public List<SelectListItem> SexSelectList { get; set; }
        public List<SelectListItem> RoleSelectList { get; set; }
        public List<SelectListItem> SecurityQuestionSelectList { get; set; }
        public List<SelectListItem> SessionSelectList { get; set; }
        public List<SelectListItem> LevelSelectList { get; set; }
        public List<SelectListItem> ProgrammeSelectList { get; set; }
        public Staff Staff { get; set; }
        public StaffDepartment StaffDepartment { get; set; }
        public List<SelectListItem> DepartmentSelectList { get; set; }
        public Department Department { get; set; }
        public List<SelectListItem> CurrentSessionSelectList { get; set; }
        public Session Session { get; set; }
        public bool RemoveHOD { get; set; }
    }
    public class MyJsonResult
    {
        public bool IsError { get; set; }
        public string Message { get; set; }
        public List<GeneralAudit> Audits { get; set; }
    }
}