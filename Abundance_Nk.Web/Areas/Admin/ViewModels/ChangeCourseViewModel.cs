﻿using Abundance_Nk.Model.Model;
using Abundance_Nk.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Windows.Forms;

namespace Abundance_Nk.Web.Areas.Admin.ViewModels
{

    public class ChangeCourseViewModel
    {
        public ChangeCourseViewModel()
        {
            ProgrammeSelectList = Utility.PopulateAllProgrammeSelectListItem();
            SessionSelectList = Utility.PopulateAllSessionSelectListItem();
        }
        public string ApplicationFormNumber { get; set; }
        public ApplicationForm ApplicationForm { get; set; }
        public AppliedCourse AppliedCourse { get; set; }
        public Person Person { get; set; }
        public Payment Payment { get; set; }
        public List<SelectListItem> ProgrammeSelectList { get; set; }
        public List<SelectListItem> DepartmentSelectListItem { get; set; }
        public List<SelectListItem> DepartmentOptionSelectListItem { get; set; }
        public decimal OldSchoolFees { get; set; }
        public decimal ShortFallAmount { get; set; }
        public List<SelectListItem> SessionSelectList { get; set; }
        public Session Session { get; set; }
    }
}