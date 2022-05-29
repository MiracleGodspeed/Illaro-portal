﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace Abundance_Nk.Model.Model
{
    public class PreviousEducation
    {
        public long Id { get; set; }
        public Person Person { get; set; }
        public PersonType PersonType { get; set; }

        [Required]
        [Display(Name = "Institution Attended")]
        public string SchoolName { get; set; }

        //[Required]
        public string Course { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        public Value StartDay { get; set; }
        public Value StartMonth { get; set; }
        public Value StartYear { get; set; }
        public Value EndDay { get; set; }
        public Value EndMonth { get; set; }
        public Value EndYear { get; set; }

        public ITDuration ITDuration { get; set; }
        public ResultGrade ResultGrade { get; set; }
        public EducationalQualification Qualification { get; set; }
        public ApplicationForm ApplicationForm { get; set; }
        public bool CertificateStatus { get; set; }
        public bool ConvocationStatus { get; set; }
        public string ResultCopyUrl { get; set; }
        public string CertificateCopyUrl { get; set; }
        public string ITLetterOfCompletion { get; set; }

        //[Display(Name = "Pre ND")]
        //public string PreND { get; set; }

        //[Display(Name = "Pre ND Year From")]
        //public int? PreNDYearFrom { get; set; }

        //[Display(Name = "Pre ND Year To")]
        //public int? PreNDYearTo { get; set; }



    }

        

}
