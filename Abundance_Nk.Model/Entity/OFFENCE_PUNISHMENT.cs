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
    
    public partial class OFFENCE_PUNISHMENT
    {
        public int Offence_Punishment_Id { get; set; }
        public byte Offence_Id { get; set; }
        public int Punishment_Id { get; set; }
    
        public virtual OFFENCE OFFENCE { get; set; }
        public virtual PUNISHMENT PUNISHMENT { get; set; }
    }
}
