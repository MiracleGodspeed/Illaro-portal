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
    
    public partial class USER
    {
        public USER()
        {
            this.ADMISSION_LIST_AUDIT = new HashSet<ADMISSION_LIST_AUDIT>();
            this.APPLICANT_APPLIED_COURSE_AUDIT = new HashSet<APPLICANT_APPLIED_COURSE_AUDIT>();
            this.APPLICANT_O_LEVEL_RESULT_AUDIT = new HashSet<APPLICANT_O_LEVEL_RESULT_AUDIT>();
            this.APPLICANT_O_LEVEL_RESULT_DETAIL_AUDIT = new HashSet<APPLICANT_O_LEVEL_RESULT_DETAIL_AUDIT>();
            this.APPLICATION_FORM = new HashSet<APPLICATION_FORM>();
            this.APPLICATION_FORM_SETTING = new HashSet<APPLICATION_FORM_SETTING>();
            this.CLEARANCE_LOG = new HashSet<CLEARANCE_LOG>();
            this.COURSE_ALLOCATION = new HashSet<COURSE_ALLOCATION>();
            this.E_CHAT_RESPONPSE = new HashSet<E_CHAT_RESPONPSE>();
            this.FEE_DETAIL_AUDIT = new HashSet<FEE_DETAIL_AUDIT>();
            this.GENERAL_AUDIT = new HashSet<GENERAL_AUDIT>();
            this.HOSTEL_REQUEST = new HashSet<HOSTEL_REQUEST>();
            this.PAYMENT_VERIFICATION = new HashSet<PAYMENT_VERIFICATION>();
            this.PERSON_AUDIT = new HashSet<PERSON_AUDIT>();
            this.PUTME_RESULT_AUDIT = new HashSet<PUTME_RESULT_AUDIT>();
            this.SCRATCH_CARD_BATCH = new HashSet<SCRATCH_CARD_BATCH>();
            this.STAFF = new HashSet<STAFF>();
            this.STUDENT_COURSE_REGISTRATION_DETAIL_AUDIT = new HashSet<STUDENT_COURSE_REGISTRATION_DETAIL_AUDIT>();
            this.STUDENT_EXAM_RAW_SCORE_SHEET_RESULT = new HashSet<STUDENT_EXAM_RAW_SCORE_SHEET_RESULT>();
            this.STUDENT_EXAM_RAW_SCORE_SHEET_RESULT_NOT_REGISTERED = new HashSet<STUDENT_EXAM_RAW_SCORE_SHEET_RESULT_NOT_REGISTERED>();
            this.STUDENT_RESULT = new HashSet<STUDENT_RESULT>();
            this.STUDENT_UPDATE_AUDIT = new HashSet<STUDENT_UPDATE_AUDIT>();
            this.FEE_SETUP = new HashSet<FEE_SETUP>();
            this.FEE_SETUP_AUDIT = new HashSet<FEE_SETUP_AUDIT>();
            this.APPLICANT_APPLICATION_APPROVAL = new HashSet<APPLICANT_APPLICATION_APPROVAL>();
        }
    
        public long User_Id { get; set; }
        public string User_Name { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public int Security_Question_Id { get; set; }
        public string Security_Answer { get; set; }
        public int Role_Id { get; set; }
        public System.DateTime LastLoginDate { get; set; }
        public Nullable<bool> Activated { get; set; }
        public Nullable<bool> PasswordChanged { get; set; }
        public Nullable<bool> Super_Admin { get; set; }
        public string Signature_Url { get; set; }
        public string Profile_Image_Url { get; set; }
        public bool Archive { get; set; }
    
        public virtual ICollection<ADMISSION_LIST_AUDIT> ADMISSION_LIST_AUDIT { get; set; }
        public virtual ICollection<APPLICANT_APPLIED_COURSE_AUDIT> APPLICANT_APPLIED_COURSE_AUDIT { get; set; }
        public virtual ICollection<APPLICANT_O_LEVEL_RESULT_AUDIT> APPLICANT_O_LEVEL_RESULT_AUDIT { get; set; }
        public virtual ICollection<APPLICANT_O_LEVEL_RESULT_DETAIL_AUDIT> APPLICANT_O_LEVEL_RESULT_DETAIL_AUDIT { get; set; }
        public virtual ICollection<APPLICATION_FORM> APPLICATION_FORM { get; set; }
        public virtual ICollection<APPLICATION_FORM_SETTING> APPLICATION_FORM_SETTING { get; set; }
        public virtual ICollection<CLEARANCE_LOG> CLEARANCE_LOG { get; set; }
        public virtual ICollection<COURSE_ALLOCATION> COURSE_ALLOCATION { get; set; }
        public virtual ICollection<E_CHAT_RESPONPSE> E_CHAT_RESPONPSE { get; set; }
        public virtual FACULTY_OFFICER FACULTY_OFFICER { get; set; }
        public virtual ICollection<FEE_DETAIL_AUDIT> FEE_DETAIL_AUDIT { get; set; }
        public virtual ICollection<GENERAL_AUDIT> GENERAL_AUDIT { get; set; }
        public virtual ICollection<HOSTEL_REQUEST> HOSTEL_REQUEST { get; set; }
        public virtual ICollection<PAYMENT_VERIFICATION> PAYMENT_VERIFICATION { get; set; }
        public virtual ICollection<PERSON_AUDIT> PERSON_AUDIT { get; set; }
        public virtual ICollection<PUTME_RESULT_AUDIT> PUTME_RESULT_AUDIT { get; set; }
        public virtual ROLE ROLE { get; set; }
        public virtual ICollection<SCRATCH_CARD_BATCH> SCRATCH_CARD_BATCH { get; set; }
        public virtual SECURITY_QUESTION SECURITY_QUESTION { get; set; }
        public virtual ICollection<STAFF> STAFF { get; set; }
        public virtual ICollection<STUDENT_COURSE_REGISTRATION_DETAIL_AUDIT> STUDENT_COURSE_REGISTRATION_DETAIL_AUDIT { get; set; }
        public virtual ICollection<STUDENT_EXAM_RAW_SCORE_SHEET_RESULT> STUDENT_EXAM_RAW_SCORE_SHEET_RESULT { get; set; }
        public virtual ICollection<STUDENT_EXAM_RAW_SCORE_SHEET_RESULT_NOT_REGISTERED> STUDENT_EXAM_RAW_SCORE_SHEET_RESULT_NOT_REGISTERED { get; set; }
        public virtual ICollection<STUDENT_RESULT> STUDENT_RESULT { get; set; }
        public virtual ICollection<STUDENT_UPDATE_AUDIT> STUDENT_UPDATE_AUDIT { get; set; }
        public virtual ICollection<FEE_SETUP> FEE_SETUP { get; set; }
        public virtual ICollection<FEE_SETUP_AUDIT> FEE_SETUP_AUDIT { get; set; }
        public virtual ICollection<APPLICANT_APPLICATION_APPROVAL> APPLICANT_APPLICATION_APPROVAL { get; set; }
    }
}
