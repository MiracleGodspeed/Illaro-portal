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
    
    public partial class PUNISHMENT
    {
        public PUNISHMENT()
        {
            this.OFFENCE_PUNISHMENT = new HashSet<OFFENCE_PUNISHMENT>();
        }
    
        public int Punishment_Id { get; set; }
        public string Punishment_Name { get; set; }
    
        public virtual ICollection<OFFENCE_PUNISHMENT> OFFENCE_PUNISHMENT { get; set; }
    }
}
