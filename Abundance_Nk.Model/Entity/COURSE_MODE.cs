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
    
    public partial class COURSE_MODE
    {
        public COURSE_MODE()
        {
            this.STUDENT_COURSE_REGISTRATION_DETAIL_AUDIT = new HashSet<STUDENT_COURSE_REGISTRATION_DETAIL_AUDIT>();
            this.STUDENT_COURSE_REGISTRATION_DETAIL = new HashSet<STUDENT_COURSE_REGISTRATION_DETAIL>();
        }
    
        public int Course_Mode_Id { get; set; }
        public string Course_Mode_Name { get; set; }
        public string Course_Mode_Description { get; set; }
    
        public virtual ICollection<STUDENT_COURSE_REGISTRATION_DETAIL_AUDIT> STUDENT_COURSE_REGISTRATION_DETAIL_AUDIT { get; set; }
        public virtual ICollection<STUDENT_COURSE_REGISTRATION_DETAIL> STUDENT_COURSE_REGISTRATION_DETAIL { get; set; }
    }
}