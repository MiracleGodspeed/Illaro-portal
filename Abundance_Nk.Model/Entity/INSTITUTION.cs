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
    
    public partial class INSTITUTION
    {
        public int Institution_Id { get; set; }
        public string Institution_Name { get; set; }
        public int Institution_Type_Id { get; set; }
    
        public virtual INSTITUTION_TYPE INSTITUTION_TYPE { get; set; }
    }
}