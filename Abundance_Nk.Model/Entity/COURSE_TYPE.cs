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
    
    public partial class COURSE_TYPE
    {
        public COURSE_TYPE()
        {
            this.COURSE = new HashSet<COURSE>();
        }
    
        public int Course_Type_Id { get; set; }
        public string Course_Type_Name { get; set; }
        public string Course_Type_Description { get; set; }
    
        public virtual ICollection<COURSE> COURSE { get; set; }
    }
}
