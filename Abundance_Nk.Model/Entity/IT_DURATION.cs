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
    
    public partial class IT_DURATION
    {
        public IT_DURATION()
        {
            this.APPLICANT_PREVIOUS_EDUCATION = new HashSet<APPLICANT_PREVIOUS_EDUCATION>();
        }
    
        public int IT_Duration_Id { get; set; }
        public string IT_Duration_Name { get; set; }
        public string IT_Duration_Description { get; set; }
    
        public virtual ICollection<APPLICANT_PREVIOUS_EDUCATION> APPLICANT_PREVIOUS_EDUCATION { get; set; }
    }
}
