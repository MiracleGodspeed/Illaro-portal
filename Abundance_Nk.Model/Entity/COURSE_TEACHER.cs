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
    
    public partial class COURSE_TEACHER
    {
        public long Course_Teacher_Id { get; set; }
        public long Staff_Id { get; set; }
        public int CourseId { get; set; }
        public System.DateTime Date_Assigned { get; set; }
    
        public virtual STAFF STAFF { get; set; }
        public virtual O_LEVEL_SUBJECT O_LEVEL_SUBJECT { get; set; }
    }
}
