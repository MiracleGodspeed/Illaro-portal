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
    
    public partial class GENERAL_AUDIT
    {
        public long General_Audit_Id { get; set; }
        public string Table_Names { get; set; }
        public string Initial_Values { get; set; }
        public string Current_Values { get; set; }
        public long User_Id { get; set; }
        public string Operation { get; set; }
        public string Action { get; set; }
        public System.DateTime Time { get; set; }
        public string Client { get; set; }
    
        public virtual USER USER { get; set; }
    }
}
