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
    
    public partial class STUDENT_COURSE_REGISTRATION_DETAIL_AUDIT
    {
        public long Student_Course_Registration_Detail_Audit_Id { get; set; }
        public Nullable<long> Student_Course_Registration_Detail_Id { get; set; }
        public Nullable<long> Student_Course_Registration_Id { get; set; }
        public long Course_Id { get; set; }
        public int Course_Mode_Id { get; set; }
        public Nullable<int> Course_Unit { get; set; }
        public int Semester_Id { get; set; }
        public Nullable<decimal> Test_Score { get; set; }
        public Nullable<decimal> Exam_Score { get; set; }
        public string Special_Case { get; set; }
        public Nullable<long> User_Id { get; set; }
        public string Operation { get; set; }
        public string Action { get; set; }
        public System.DateTime Time { get; set; }
        public string Client { get; set; }
        public Nullable<long> Person_Id { get; set; }
    
        public virtual COURSE COURSE { get; set; }
        public virtual COURSE_MODE COURSE_MODE { get; set; }
        public virtual SEMESTER SEMESTER { get; set; }
        public virtual STUDENT_COURSE_REGISTRATION STUDENT_COURSE_REGISTRATION { get; set; }
        public virtual USER USER { get; set; }
        public virtual STUDENT STUDENT { get; set; }
    }
}