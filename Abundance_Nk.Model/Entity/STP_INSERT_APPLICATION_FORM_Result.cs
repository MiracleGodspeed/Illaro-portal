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
    
    public partial class STP_INSERT_APPLICATION_FORM_Result
    {
        public long Application_Form_Id { get; set; }
        public string Application_Form_Number { get; set; }
        public string Application_Exam_Number { get; set; }
        public int Application_Form_Setting_Id { get; set; }
        public int Application_Programme_Fee_Id { get; set; }
        public System.DateTime Date_Submitted { get; set; }
        public long Payment_Id { get; set; }
        public long Person_Id { get; set; }
        public string Reject_Reason { get; set; }
        public bool Rejected { get; set; }
        public bool Release { get; set; }
        public string Remarks { get; set; }
        public Nullable<long> Serial_Number { get; set; }
    }
}
