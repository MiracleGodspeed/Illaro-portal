//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Abundance_Nk.Model.Entity
{
    using System;
    using System.Collections.Generic;
    
    public partial class O_LEVEL_SUBJECT
    {
        public O_LEVEL_SUBJECT()
        {
            this.ADMISSION_CRITERIA_FOR_O_LEVEL_SUBJECT = new HashSet<ADMISSION_CRITERIA_FOR_O_LEVEL_SUBJECT>();
            this.ADMISSION_CRITERIA_FOR_O_LEVEL_SUBJECT_ALTERNATIVE = new HashSet<ADMISSION_CRITERIA_FOR_O_LEVEL_SUBJECT_ALTERNATIVE>();
            this.APPLICANT_JAMB_DETAIL = new HashSet<APPLICANT_JAMB_DETAIL>();
            this.APPLICANT_JAMB_DETAIL1 = new HashSet<APPLICANT_JAMB_DETAIL>();
            this.APPLICANT_JAMB_DETAIL2 = new HashSet<APPLICANT_JAMB_DETAIL>();
            this.APPLICANT_JAMB_DETAIL3 = new HashSet<APPLICANT_JAMB_DETAIL>();
            this.APPLICANT_O_LEVEL_RESULT_DETAIL = new HashSet<APPLICANT_O_LEVEL_RESULT_DETAIL>();
            this.APPLICANT_O_LEVEL_RESULT_DETAIL_AUDIT = new HashSet<APPLICANT_O_LEVEL_RESULT_DETAIL_AUDIT>();
            this.COURSE_TEACHER = new HashSet<COURSE_TEACHER>();
            this.STUDENT_SCORE = new HashSet<STUDENT_SCORE>();
            this.JAMB_O_LEVEL_DETAIL = new HashSet<JAMB_O_LEVEL_DETAIL>();
        }
    
        public int O_Level_Subject_Id { get; set; }
        public string O_Level_Subject_Name { get; set; }
        public string O_Level_Subject_Description { get; set; }
    
        public virtual ICollection<ADMISSION_CRITERIA_FOR_O_LEVEL_SUBJECT> ADMISSION_CRITERIA_FOR_O_LEVEL_SUBJECT { get; set; }
        public virtual ICollection<ADMISSION_CRITERIA_FOR_O_LEVEL_SUBJECT_ALTERNATIVE> ADMISSION_CRITERIA_FOR_O_LEVEL_SUBJECT_ALTERNATIVE { get; set; }
        public virtual ICollection<APPLICANT_JAMB_DETAIL> APPLICANT_JAMB_DETAIL { get; set; }
        public virtual ICollection<APPLICANT_JAMB_DETAIL> APPLICANT_JAMB_DETAIL1 { get; set; }
        public virtual ICollection<APPLICANT_JAMB_DETAIL> APPLICANT_JAMB_DETAIL2 { get; set; }
        public virtual ICollection<APPLICANT_JAMB_DETAIL> APPLICANT_JAMB_DETAIL3 { get; set; }
        public virtual ICollection<APPLICANT_O_LEVEL_RESULT_DETAIL> APPLICANT_O_LEVEL_RESULT_DETAIL { get; set; }
        public virtual ICollection<APPLICANT_O_LEVEL_RESULT_DETAIL_AUDIT> APPLICANT_O_LEVEL_RESULT_DETAIL_AUDIT { get; set; }
        public virtual ICollection<COURSE_TEACHER> COURSE_TEACHER { get; set; }
        public virtual ICollection<STUDENT_SCORE> STUDENT_SCORE { get; set; }
        public virtual ICollection<JAMB_O_LEVEL_DETAIL> JAMB_O_LEVEL_DETAIL { get; set; }
    }
}