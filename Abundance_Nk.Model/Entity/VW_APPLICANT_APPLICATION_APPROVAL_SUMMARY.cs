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
    
    public partial class VW_APPLICANT_APPLICATION_APPROVAL_SUMMARY
    {
        public long Person_Id { get; set; }
        public string Name { get; set; }
        public string Application_Form_Number { get; set; }
        public long Application_Form_Id { get; set; }
        public int Programme_Id { get; set; }
        public string Programme_Name { get; set; }
        public int Department_Id { get; set; }
        public string Department_Name { get; set; }
        public Nullable<int> Department_Option_Id { get; set; }
        public string Department_Option_Name { get; set; }
        public int Session_Id { get; set; }
        public string Session_Name { get; set; }
        public Nullable<long> Acted_On_Form_Id { get; set; }
        public string Clearance_Code { get; set; }
        public Nullable<System.DateTime> Date_Treated { get; set; }
        public Nullable<bool> Is_Approved { get; set; }
        public string Remarks { get; set; }
        public string User_Name { get; set; }
        public string Email { get; set; }
        public Nullable<long> User_Id { get; set; }
    }
}