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
    
    public partial class GENOTYPE
    {
        public GENOTYPE()
        {
            this.STUDENT = new HashSet<STUDENT>();
        }
    
        public int Genotype_Id { get; set; }
        public string Genotype_Name { get; set; }
    
        public virtual ICollection<STUDENT> STUDENT { get; set; }
    }
}