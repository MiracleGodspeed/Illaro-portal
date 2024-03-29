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
    
    public partial class DEPARTMENT
    {
        public DEPARTMENT()
        {
            this.ADMISSION_CRITERIA = new HashSet<ADMISSION_CRITERIA>();
            this.ADMISSION_LIST = new HashSet<ADMISSION_LIST>();
            this.ADMISSION_LIST_AUDIT = new HashSet<ADMISSION_LIST_AUDIT>();
            this.APPLICANT_APPLIED_COURSE = new HashSet<APPLICANT_APPLIED_COURSE>();
            this.APPLICANT_APPLIED_COURSE_AUDIT = new HashSet<APPLICANT_APPLIED_COURSE_AUDIT>();
            this.APPLICANT_APPLIED_COURSE_AUDIT1 = new HashSet<APPLICANT_APPLIED_COURSE_AUDIT>();
            this.COMPLAIN_LOG = new HashSet<COMPLAIN_LOG>();
            this.COURSE = new HashSet<COURSE>();
            this.COURSE_ALLOCATION = new HashSet<COURSE_ALLOCATION>();
            this.COURSE_ALLOCATION1 = new HashSet<COURSE_ALLOCATION>();
            this.COURSE_REGISTRATION_STATUS = new HashSet<COURSE_REGISTRATION_STATUS>();
            this.COURSE_UNIT = new HashSet<COURSE_UNIT>();
            this.DEPARTMENT_CAPACITY = new HashSet<DEPARTMENT_CAPACITY>();
            this.DEPARTMENT_HEAD = new HashSet<DEPARTMENT_HEAD>();
            this.DEPARTMENT_OPTION = new HashSet<DEPARTMENT_OPTION>();
            this.FEE_DETAIL_AUDIT = new HashSet<FEE_DETAIL_AUDIT>();
            this.FEE_DETAIL = new HashSet<FEE_DETAIL>();
            this.HOSTEL_BLACKLIST = new HashSet<HOSTEL_BLACKLIST>();
            this.HOSTEL_REQUEST = new HashSet<HOSTEL_REQUEST>();
            this.PROGRAMME_DEPARTMENT = new HashSet<PROGRAMME_DEPARTMENT>();
            this.STAFF_DEPARTMENT = new HashSet<STAFF_DEPARTMENT>();
            this.STUDENT_COURSE_REGISTRATION = new HashSet<STUDENT_COURSE_REGISTRATION>();
            this.STUDENT_LEVEL = new HashSet<STUDENT_LEVEL>();
            this.STUDENT_MATRIC_NUMBER_ASSIGNMENT = new HashSet<STUDENT_MATRIC_NUMBER_ASSIGNMENT>();
            this.STUDENT_RESULT = new HashSet<STUDENT_RESULT>();
            this.VENUE_DESIGNATION = new HashSet<VENUE_DESIGNATION>();
            this.LIVE_LECTURES = new HashSet<LIVE_LECTURES>();
            this.JAMB_RECORD = new HashSet<JAMB_RECORD>();
        }
    
        public int Department_Id { get; set; }
        public string Department_Name { get; set; }
        public string Department_Code { get; set; }
        public int Faculty_Id { get; set; }
        public bool Active { get; set; }
    
        public virtual ICollection<ADMISSION_CRITERIA> ADMISSION_CRITERIA { get; set; }
        public virtual ICollection<ADMISSION_LIST> ADMISSION_LIST { get; set; }
        public virtual ICollection<ADMISSION_LIST_AUDIT> ADMISSION_LIST_AUDIT { get; set; }
        public virtual ICollection<APPLICANT_APPLIED_COURSE> APPLICANT_APPLIED_COURSE { get; set; }
        public virtual ICollection<APPLICANT_APPLIED_COURSE_AUDIT> APPLICANT_APPLIED_COURSE_AUDIT { get; set; }
        public virtual ICollection<APPLICANT_APPLIED_COURSE_AUDIT> APPLICANT_APPLIED_COURSE_AUDIT1 { get; set; }
        public virtual ICollection<COMPLAIN_LOG> COMPLAIN_LOG { get; set; }
        public virtual ICollection<COURSE> COURSE { get; set; }
        public virtual ICollection<COURSE_ALLOCATION> COURSE_ALLOCATION { get; set; }
        public virtual ICollection<COURSE_ALLOCATION> COURSE_ALLOCATION1 { get; set; }
        public virtual ICollection<COURSE_REGISTRATION_STATUS> COURSE_REGISTRATION_STATUS { get; set; }
        public virtual ICollection<COURSE_UNIT> COURSE_UNIT { get; set; }
        public virtual ICollection<DEPARTMENT_CAPACITY> DEPARTMENT_CAPACITY { get; set; }
        public virtual FACULTY FACULTY { get; set; }
        public virtual ICollection<DEPARTMENT_HEAD> DEPARTMENT_HEAD { get; set; }
        public virtual ICollection<DEPARTMENT_OPTION> DEPARTMENT_OPTION { get; set; }
        public virtual ICollection<FEE_DETAIL_AUDIT> FEE_DETAIL_AUDIT { get; set; }
        public virtual ICollection<FEE_DETAIL> FEE_DETAIL { get; set; }
        public virtual ICollection<HOSTEL_BLACKLIST> HOSTEL_BLACKLIST { get; set; }
        public virtual ICollection<HOSTEL_REQUEST> HOSTEL_REQUEST { get; set; }
        public virtual ICollection<PROGRAMME_DEPARTMENT> PROGRAMME_DEPARTMENT { get; set; }
        public virtual ICollection<STAFF_DEPARTMENT> STAFF_DEPARTMENT { get; set; }
        public virtual ICollection<STUDENT_COURSE_REGISTRATION> STUDENT_COURSE_REGISTRATION { get; set; }
        public virtual ICollection<STUDENT_LEVEL> STUDENT_LEVEL { get; set; }
        public virtual ICollection<STUDENT_MATRIC_NUMBER_ASSIGNMENT> STUDENT_MATRIC_NUMBER_ASSIGNMENT { get; set; }
        public virtual ICollection<STUDENT_RESULT> STUDENT_RESULT { get; set; }
        public virtual ICollection<VENUE_DESIGNATION> VENUE_DESIGNATION { get; set; }
        public virtual ICollection<LIVE_LECTURES> LIVE_LECTURES { get; set; }
        public virtual ICollection<JAMB_RECORD> JAMB_RECORD { get; set; }
    }
}
