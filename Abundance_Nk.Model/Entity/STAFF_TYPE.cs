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
    
    public partial class STAFF_TYPE
    {
        public STAFF_TYPE()
        {
            this.STAFF = new HashSet<STAFF>();
        }
    
        public int Staff_Type_Id { get; set; }
        public string Staff_Type_Name { get; set; }
        public string Staff_Type_Description { get; set; }
    
        public virtual ICollection<STAFF> STAFF { get; set; }
    }
}
