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
    
    public partial class DAY
    {
        public DAY()
        {
            this.DAY_ACTIVITY = new HashSet<DAY_ACTIVITY>();
        }
    
        public byte Day_Id { get; set; }
        public string Day_Name { get; set; }
    
        public virtual ICollection<DAY_ACTIVITY> DAY_ACTIVITY { get; set; }
    }
}
