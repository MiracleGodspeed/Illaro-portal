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
    
    public partial class VW_APPLICATION_EXAM_NUMBER
    {
        public long Application_Form_Id { get; set; }
        public Nullable<long> Serial_Number { get; set; }
        public Nullable<int> Application_Exam_Serial_Number { get; set; }
        public string Application_Exam_Number { get; set; }
        public int Session_Id { get; set; }
        public int Programme_Id { get; set; }
        public int Department_Id { get; set; }
        public int Application_Form_Setting_Id { get; set; }
    }
}