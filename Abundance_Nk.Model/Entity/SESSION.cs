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
    
    public partial class SESSION
    {
        public SESSION()
        {
            this.ADMISSION_LIST = new HashSet<ADMISSION_LIST>();
            this.APPLICANT_LEVEL = new HashSet<APPLICANT_LEVEL>();
            this.APPLICATION_FORM_SETTING = new HashSet<APPLICATION_FORM_SETTING>();
            this.APPLICATION_PROGRAMME_FEE = new HashSet<APPLICATION_PROGRAMME_FEE>();
            this.CHANGE_OF_COURSE = new HashSet<CHANGE_OF_COURSE>();
            this.COURSE_ALLOCATION = new HashSet<COURSE_ALLOCATION>();
            this.COURSE_EVALUATION_ANSWER = new HashSet<COURSE_EVALUATION_ANSWER>();
            this.COURSE_REGISTRATION_STATUS = new HashSet<COURSE_REGISTRATION_STATUS>();
            this.DEPARTMENT_CAPACITY = new HashSet<DEPARTMENT_CAPACITY>();
            this.FEE_DETAIL = new HashSet<FEE_DETAIL>();
            this.FEE_DETAIL_AUDIT = new HashSet<FEE_DETAIL_AUDIT>();
            this.HOSTEL_ALLOCATION = new HashSet<HOSTEL_ALLOCATION>();
            this.HOSTEL_BLACKLIST = new HashSet<HOSTEL_BLACKLIST>();
            this.HOSTEL_REQUEST = new HashSet<HOSTEL_REQUEST>();
            this.PAYMENT = new HashSet<PAYMENT>();
            this.PAYMENT_ETRANZACT_TYPE = new HashSet<PAYMENT_ETRANZACT_TYPE>();
            this.PAYMENT_SCHOLARSHIP = new HashSet<PAYMENT_SCHOLARSHIP>();
            this.PAYMENT_TERMINAL = new HashSet<PAYMENT_TERMINAL>();
            this.PUTME_RESULT = new HashSet<PUTME_RESULT>();
            this.SESSION_SEMESTER = new HashSet<SESSION_SEMESTER>();
            this.STUDENT_COURSE_REGISTRATION = new HashSet<STUDENT_COURSE_REGISTRATION>();
            this.STUDENT_DEFERMENT_LOG = new HashSet<STUDENT_DEFERMENT_LOG>();
            this.STUDENT_EXAM_RAW_SCORE_SHEET_RESULT_NOT_REGISTERED = new HashSet<STUDENT_EXAM_RAW_SCORE_SHEET_RESULT_NOT_REGISTERED>();
            this.STUDENT_EXAM_RAW_SCORE_SHEET_RESULT = new HashSet<STUDENT_EXAM_RAW_SCORE_SHEET_RESULT>();
            this.STUDENT_EXTRA_YEAR_SESSION = new HashSet<STUDENT_EXTRA_YEAR_SESSION>();
            this.STUDENT_EXTRA_YEAR_SESSION1 = new HashSet<STUDENT_EXTRA_YEAR_SESSION>();
            this.STUDENT_EXTRA_YEAR_SESSION2 = new HashSet<STUDENT_EXTRA_YEAR_SESSION>();
            this.STUDENT_LEVEL = new HashSet<STUDENT_LEVEL>();
            this.STUDENT_MATRIC_NUMBER_ASSIGNMENT = new HashSet<STUDENT_MATRIC_NUMBER_ASSIGNMENT>();
            this.STUDENT_RESULT_STATUS = new HashSet<STUDENT_RESULT_STATUS>();
            this.FEE_SETUP = new HashSet<FEE_SETUP>();
            this.FEE_SETUP_AUDIT = new HashSet<FEE_SETUP_AUDIT>();
            this.LIVE_LECTURES = new HashSet<LIVE_LECTURES>();
            this.JAMB_RECORD = new HashSet<JAMB_RECORD>();
        }
    
        public int Session_Id { get; set; }
        public string Session_Name { get; set; }
        public System.DateTime Start_Date { get; set; }
        public System.DateTime End_date { get; set; }
        public Nullable<bool> Activated { get; set; }
        public Nullable<bool> Active_For_Result { get; set; }
        public Nullable<bool> Active_For_Allocation { get; set; }
        public Nullable<bool> Active_For_Application { get; set; }
        public Nullable<bool> Active_For_Hostel { get; set; }
        public Nullable<bool> Active_For_Fees { get; set; }
    
        public virtual ICollection<ADMISSION_LIST> ADMISSION_LIST { get; set; }
        public virtual ICollection<APPLICANT_LEVEL> APPLICANT_LEVEL { get; set; }
        public virtual ICollection<APPLICATION_FORM_SETTING> APPLICATION_FORM_SETTING { get; set; }
        public virtual ICollection<APPLICATION_PROGRAMME_FEE> APPLICATION_PROGRAMME_FEE { get; set; }
        public virtual ICollection<CHANGE_OF_COURSE> CHANGE_OF_COURSE { get; set; }
        public virtual ICollection<COURSE_ALLOCATION> COURSE_ALLOCATION { get; set; }
        public virtual ICollection<COURSE_EVALUATION_ANSWER> COURSE_EVALUATION_ANSWER { get; set; }
        public virtual ICollection<COURSE_REGISTRATION_STATUS> COURSE_REGISTRATION_STATUS { get; set; }
        public virtual ICollection<DEPARTMENT_CAPACITY> DEPARTMENT_CAPACITY { get; set; }
        public virtual ICollection<FEE_DETAIL> FEE_DETAIL { get; set; }
        public virtual ICollection<FEE_DETAIL_AUDIT> FEE_DETAIL_AUDIT { get; set; }
        public virtual ICollection<HOSTEL_ALLOCATION> HOSTEL_ALLOCATION { get; set; }
        public virtual ICollection<HOSTEL_BLACKLIST> HOSTEL_BLACKLIST { get; set; }
        public virtual ICollection<HOSTEL_REQUEST> HOSTEL_REQUEST { get; set; }
        public virtual ICollection<PAYMENT> PAYMENT { get; set; }
        public virtual ICollection<PAYMENT_ETRANZACT_TYPE> PAYMENT_ETRANZACT_TYPE { get; set; }
        public virtual ICollection<PAYMENT_SCHOLARSHIP> PAYMENT_SCHOLARSHIP { get; set; }
        public virtual ICollection<PAYMENT_TERMINAL> PAYMENT_TERMINAL { get; set; }
        public virtual ICollection<PUTME_RESULT> PUTME_RESULT { get; set; }
        public virtual ICollection<SESSION_SEMESTER> SESSION_SEMESTER { get; set; }
        public virtual ICollection<STUDENT_COURSE_REGISTRATION> STUDENT_COURSE_REGISTRATION { get; set; }
        public virtual ICollection<STUDENT_DEFERMENT_LOG> STUDENT_DEFERMENT_LOG { get; set; }
        public virtual ICollection<STUDENT_EXAM_RAW_SCORE_SHEET_RESULT_NOT_REGISTERED> STUDENT_EXAM_RAW_SCORE_SHEET_RESULT_NOT_REGISTERED { get; set; }
        public virtual ICollection<STUDENT_EXAM_RAW_SCORE_SHEET_RESULT> STUDENT_EXAM_RAW_SCORE_SHEET_RESULT { get; set; }
        public virtual ICollection<STUDENT_EXTRA_YEAR_SESSION> STUDENT_EXTRA_YEAR_SESSION { get; set; }
        public virtual ICollection<STUDENT_EXTRA_YEAR_SESSION> STUDENT_EXTRA_YEAR_SESSION1 { get; set; }
        public virtual ICollection<STUDENT_EXTRA_YEAR_SESSION> STUDENT_EXTRA_YEAR_SESSION2 { get; set; }
        public virtual ICollection<STUDENT_LEVEL> STUDENT_LEVEL { get; set; }
        public virtual ICollection<STUDENT_MATRIC_NUMBER_ASSIGNMENT> STUDENT_MATRIC_NUMBER_ASSIGNMENT { get; set; }
        public virtual ICollection<STUDENT_RESULT_STATUS> STUDENT_RESULT_STATUS { get; set; }
        public virtual ICollection<FEE_SETUP> FEE_SETUP { get; set; }
        public virtual ICollection<FEE_SETUP_AUDIT> FEE_SETUP_AUDIT { get; set; }
        public virtual ICollection<LIVE_LECTURES> LIVE_LECTURES { get; set; }
        public virtual ICollection<JAMB_RECORD> JAMB_RECORD { get; set; }
    }
}
