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
    
    public partial class APPLICANT_APPLIED_COURSE_AUDIT
    {
        public long Applicant_Applied_Course_Audit_Id { get; set; }
        public long Person_Id { get; set; }
        public int Programme_Id { get; set; }
        public int Department_Id { get; set; }
        public Nullable<int> Department_Option_Id { get; set; }
        public Nullable<long> Application_Form_Id { get; set; }
        public int OLD_Programme_Id { get; set; }
        public int OLD_Department_Id { get; set; }
        public Nullable<int> OLD_Department_Option_Id { get; set; }
        public Nullable<long> OLD_Application_Form_Id { get; set; }
        public long User_Id { get; set; }
        public string Operation { get; set; }
        public string Action { get; set; }
        public System.DateTime Time { get; set; }
        public string Client { get; set; }
    
        public virtual APPLICANT_APPLIED_COURSE APPLICANT_APPLIED_COURSE { get; set; }
        public virtual APPLICATION_FORM APPLICATION_FORM { get; set; }
        public virtual APPLICATION_FORM APPLICATION_FORM1 { get; set; }
        public virtual DEPARTMENT DEPARTMENT { get; set; }
        public virtual DEPARTMENT_OPTION DEPARTMENT_OPTION { get; set; }
        public virtual DEPARTMENT_OPTION DEPARTMENT_OPTION1 { get; set; }
        public virtual DEPARTMENT DEPARTMENT1 { get; set; }
        public virtual PROGRAMME PROGRAMME { get; set; }
        public virtual PROGRAMME PROGRAMME1 { get; set; }
        public virtual USER USER { get; set; }
    }
}
