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
    
    public partial class REPORT_CARD_COMMENT
    {
        public long Report_Card_Comment_Id { get; set; }
        public long Report_Card_Id { get; set; }
        public long Commentator_Id { get; set; }
        public string Comment { get; set; }
        public System.DateTime Comment_Date { get; set; }
    
        public virtual REPORT_CARD REPORT_CARD { get; set; }
        public virtual STAFF STAFF { get; set; }
    }
}
