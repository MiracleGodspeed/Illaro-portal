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
    
    public partial class VW_STUDENT_REGISTERED_COURSE_CARRYOVER
    {
        public long Student_Course_Registration_Id { get; set; }
        public long Person_Id { get; set; }
        public int Level_Id { get; set; }
        public int Programme_Id { get; set; }
        public int Department_Id { get; set; }
        public int Session_Id { get; set; }
        public Nullable<bool> Approved { get; set; }
        public Nullable<long> Approver_Id { get; set; }
        public Nullable<System.DateTime> Date_Approved { get; set; }
        public long Course_Id { get; set; }
        public int Course_Mode_Id { get; set; }
        public int Semester_Id { get; set; }
        public Nullable<decimal> Test_Score { get; set; }
        public Nullable<decimal> Exam_Score { get; set; }
        public long Student_Course_Registration_Detail_Id { get; set; }
    }
}
