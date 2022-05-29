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
    
    public partial class SEMESTER
    {
        public SEMESTER()
        {
            this.COURSE = new HashSet<COURSE>();
            this.COURSE_ALLOCATION = new HashSet<COURSE_ALLOCATION>();
            this.COURSE_EVALUATION_ANSWER = new HashSet<COURSE_EVALUATION_ANSWER>();
            this.COURSE_UNIT = new HashSet<COURSE_UNIT>();
            this.SESSION_SEMESTER = new HashSet<SESSION_SEMESTER>();
            this.STUDENT_COURSE_REGISTRATION_DETAIL_AUDIT = new HashSet<STUDENT_COURSE_REGISTRATION_DETAIL_AUDIT>();
            this.STUDENT_COURSE_REGISTRATION_DETAIL = new HashSet<STUDENT_COURSE_REGISTRATION_DETAIL>();
            this.STUDENT_DEFERMENT_LOG = new HashSet<STUDENT_DEFERMENT_LOG>();
            this.STUDENT_EXAM_RAW_SCORE_SHEET_RESULT_NOT_REGISTERED = new HashSet<STUDENT_EXAM_RAW_SCORE_SHEET_RESULT_NOT_REGISTERED>();
            this.STUDENT_EXAM_RAW_SCORE_SHEET_RESULT = new HashSet<STUDENT_EXAM_RAW_SCORE_SHEET_RESULT>();
            this.STUDENT_RESULT_STATUS = new HashSet<STUDENT_RESULT_STATUS>();
        }
    
        public int Semester_Id { get; set; }
        public string Semester_Name { get; set; }
    
        public virtual ICollection<COURSE> COURSE { get; set; }
        public virtual ICollection<COURSE_ALLOCATION> COURSE_ALLOCATION { get; set; }
        public virtual ICollection<COURSE_EVALUATION_ANSWER> COURSE_EVALUATION_ANSWER { get; set; }
        public virtual ICollection<COURSE_UNIT> COURSE_UNIT { get; set; }
        public virtual ICollection<SESSION_SEMESTER> SESSION_SEMESTER { get; set; }
        public virtual ICollection<STUDENT_COURSE_REGISTRATION_DETAIL_AUDIT> STUDENT_COURSE_REGISTRATION_DETAIL_AUDIT { get; set; }
        public virtual ICollection<STUDENT_COURSE_REGISTRATION_DETAIL> STUDENT_COURSE_REGISTRATION_DETAIL { get; set; }
        public virtual ICollection<STUDENT_DEFERMENT_LOG> STUDENT_DEFERMENT_LOG { get; set; }
        public virtual ICollection<STUDENT_EXAM_RAW_SCORE_SHEET_RESULT_NOT_REGISTERED> STUDENT_EXAM_RAW_SCORE_SHEET_RESULT_NOT_REGISTERED { get; set; }
        public virtual ICollection<STUDENT_EXAM_RAW_SCORE_SHEET_RESULT> STUDENT_EXAM_RAW_SCORE_SHEET_RESULT { get; set; }
        public virtual ICollection<STUDENT_RESULT_STATUS> STUDENT_RESULT_STATUS { get; set; }
    }
}