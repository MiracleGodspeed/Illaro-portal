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
    
    public partial class COMPLAIN_LOG
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Mobile_Number { get; set; }
        public string Complain { get; set; }
        public string Application_Number { get; set; }
        public string Exam_Number { get; set; }
        public string RRR { get; set; }
        public string Confirmation_Order_Number { get; set; }
        public bool Status { get; set; }
        public System.DateTime Date_Submitted { get; set; }
        public string Ticket_Id { get; set; }
        public string Comment { get; set; }
        public int Department_Id { get; set; }
        public int Programme_Id { get; set; }
    
        public virtual DEPARTMENT DEPARTMENT { get; set; }
        public virtual PROGRAMME PROGRAMME { get; set; }
    }
}