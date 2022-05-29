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
    
    public partial class STUDENT_COURSE_REGISTRATION
    {
        public STUDENT_COURSE_REGISTRATION()
        {
            this.STUDENT_COURSE_REGISTRATION_DETAIL_AUDIT = new HashSet<STUDENT_COURSE_REGISTRATION_DETAIL_AUDIT>();
            this.STUDENT_COURSE_REGISTRATION_DETAIL = new HashSet<STUDENT_COURSE_REGISTRATION_DETAIL>();
        }
    
        public long Student_Course_Registration_Id { get; set; }
        public long Person_Id { get; set; }
        public int Level_Id { get; set; }
        public int Programme_Id { get; set; }
        public int Department_Id { get; set; }
        public int Session_Id { get; set; }
        public Nullable<bool> Approved { get; set; }
        public Nullable<long> Approver_Id { get; set; }
        public Nullable<System.DateTime> Date_Approved { get; set; }
    
        public virtual DEPARTMENT DEPARTMENT { get; set; }
        public virtual LEVEL LEVEL { get; set; }
        public virtual PROGRAMME PROGRAMME { get; set; }
        public virtual SESSION SESSION { get; set; }
        public virtual STAFF STAFF { get; set; }
        public virtual ICollection<STUDENT_COURSE_REGISTRATION_DETAIL_AUDIT> STUDENT_COURSE_REGISTRATION_DETAIL_AUDIT { get; set; }
        public virtual ICollection<STUDENT_COURSE_REGISTRATION_DETAIL> STUDENT_COURSE_REGISTRATION_DETAIL { get; set; }
        public virtual STUDENT STUDENT { get; set; }
    }
}