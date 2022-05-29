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
    
    public partial class FEE_DETAIL_AUDIT
    {
        public int Fee_Detail_Audit_Id { get; set; }
        public int Fee_Detail_Id { get; set; }
        public int Fee_Id { get; set; }
        public int Fee_Type_Id { get; set; }
        public Nullable<int> Department_Id { get; set; }
        public Nullable<int> Programme_Id { get; set; }
        public Nullable<int> Level_Id { get; set; }
        public Nullable<int> Session_Id { get; set; }
        public long User_Id { get; set; }
        public string Operation { get; set; }
        public string Action { get; set; }
        public string Client { get; set; }
        public System.DateTime Date_Uploaded { get; set; }
    
        public virtual DEPARTMENT DEPARTMENT { get; set; }
        public virtual FEE FEE { get; set; }
        public virtual FEE_DETAIL FEE_DETAIL { get; set; }
        public virtual FEE_TYPE FEE_TYPE { get; set; }
        public virtual LEVEL LEVEL { get; set; }
        public virtual PROGRAMME PROGRAMME { get; set; }
        public virtual SESSION SESSION { get; set; }
        public virtual USER USER { get; set; }
    }
}
