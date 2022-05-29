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
    
    public partial class APPLICATION_FORM
    {
        public APPLICATION_FORM()
        {
            this.ADMISSION_LIST = new HashSet<ADMISSION_LIST>();
            this.ADMISSION_LIST_AUDIT = new HashSet<ADMISSION_LIST_AUDIT>();
            this.APPLICANT_APPLIED_COURSE = new HashSet<APPLICANT_APPLIED_COURSE>();
            this.APPLICANT_APPLIED_COURSE_AUDIT = new HashSet<APPLICANT_APPLIED_COURSE_AUDIT>();
            this.APPLICANT_APPLIED_COURSE_AUDIT1 = new HashSet<APPLICANT_APPLIED_COURSE_AUDIT>();
            this.APPLICANT_JAMB_DETAIL = new HashSet<APPLICANT_JAMB_DETAIL>();
            this.APPLICANT_LEVEL = new HashSet<APPLICANT_LEVEL>();
            this.APPLICANT_O_LEVEL_RESULT = new HashSet<APPLICANT_O_LEVEL_RESULT>();
            this.APPLICANT_O_LEVEL_RESULT_AUDIT = new HashSet<APPLICANT_O_LEVEL_RESULT_AUDIT>();
            this.APPLICANT_SPONSOR = new HashSet<APPLICANT_SPONSOR>();
            this.CHANGE_OF_COURSE = new HashSet<CHANGE_OF_COURSE>();
            this.NEXT_OF_KIN = new HashSet<NEXT_OF_KIN>();
            this.APPLICANT_PREVIOUS_EDUCATION = new HashSet<APPLICANT_PREVIOUS_EDUCATION>();
            this.DRIVING_EXPERIENCE = new HashSet<DRIVING_EXPERIENCE>();
            this.STUDENT = new HashSet<STUDENT>();
        }
    
        public long Application_Form_Id { get; set; }
        public Nullable<long> Serial_Number { get; set; }
        public string Application_Form_Number { get; set; }
        public Nullable<int> Application_Exam_Serial_Number { get; set; }
        public string Application_Exam_Number { get; set; }
        public int Application_Form_Setting_Id { get; set; }
        public int Application_Programme_Fee_Id { get; set; }
        public long Payment_Id { get; set; }
        public long Person_Id { get; set; }
        public System.DateTime Date_Submitted { get; set; }
        public bool Release { get; set; }
        public bool Rejected { get; set; }
        public string Reject_Reason { get; set; }
        public string Remarks { get; set; }
        public Nullable<long> Verification_Officer { get; set; }
        public string Verification_Comment { get; set; }
        public Nullable<bool> Verification_Status { get; set; }
    
        public virtual ICollection<ADMISSION_LIST> ADMISSION_LIST { get; set; }
        public virtual ICollection<ADMISSION_LIST_AUDIT> ADMISSION_LIST_AUDIT { get; set; }
        public virtual APPLICANT APPLICANT { get; set; }
        public virtual ICollection<APPLICANT_APPLIED_COURSE> APPLICANT_APPLIED_COURSE { get; set; }
        public virtual ICollection<APPLICANT_APPLIED_COURSE_AUDIT> APPLICANT_APPLIED_COURSE_AUDIT { get; set; }
        public virtual ICollection<APPLICANT_APPLIED_COURSE_AUDIT> APPLICANT_APPLIED_COURSE_AUDIT1 { get; set; }
        public virtual APPLICANT_CLEARANCE APPLICANT_CLEARANCE { get; set; }
        public virtual ICollection<APPLICANT_JAMB_DETAIL> APPLICANT_JAMB_DETAIL { get; set; }
        public virtual ICollection<APPLICANT_LEVEL> APPLICANT_LEVEL { get; set; }
        public virtual ICollection<APPLICANT_O_LEVEL_RESULT> APPLICANT_O_LEVEL_RESULT { get; set; }
        public virtual ICollection<APPLICANT_O_LEVEL_RESULT_AUDIT> APPLICANT_O_LEVEL_RESULT_AUDIT { get; set; }
        public virtual ICollection<APPLICANT_SPONSOR> APPLICANT_SPONSOR { get; set; }
        public virtual APPLICATION_FORM_SETTING APPLICATION_FORM_SETTING { get; set; }
        public virtual APPLICATION_PROGRAMME_FEE APPLICATION_PROGRAMME_FEE { get; set; }
        public virtual PAYMENT PAYMENT { get; set; }
        public virtual PERSON PERSON { get; set; }
        public virtual ICollection<CHANGE_OF_COURSE> CHANGE_OF_COURSE { get; set; }
        public virtual ICollection<NEXT_OF_KIN> NEXT_OF_KIN { get; set; }
        public virtual USER USER { get; set; }
        public virtual ICollection<APPLICANT_PREVIOUS_EDUCATION> APPLICANT_PREVIOUS_EDUCATION { get; set; }
        public virtual ICollection<DRIVING_EXPERIENCE> DRIVING_EXPERIENCE { get; set; }
        public virtual APPLICANT_APPLICATION_APPROVAL APPLICANT_APPLICATION_APPROVAL { get; set; }
        public virtual ICollection<STUDENT> STUDENT { get; set; }
    }
}
