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
    
    public partial class VW_PUTME_RESULT_DETAILS
    {
        public long Person_Id { get; set; }
        public string Name { get; set; }
        public string Sex_Name { get; set; }
        public string State_Name { get; set; }
        public string Local_Government_Name { get; set; }
        public long Application_Form_Id { get; set; }
        public string Application_Exam_Number { get; set; }
        public string Application_Form_Number { get; set; }
        public string Reject_Reason { get; set; }
        public bool Rejected { get; set; }
        public long Applicant_O_Level_Result_Id { get; set; }
        public int Exam_Year { get; set; }
        public long Applicant_O_Level_Result_Detail_Id { get; set; }
        public int O_Level_Type_Id { get; set; }
        public string O_Level_Type_Name { get; set; }
        public int O_Level_Exam_Sitting_Id { get; set; }
        public string O_Level_Exam_Sitting_Name { get; set; }
        public int O_Level_Grade_Id { get; set; }
        public string O_Level_Grade_Name { get; set; }
        public int O_Level_Subject_Id { get; set; }
        public string O_Level_Subject_Name { get; set; }
        public int Programme_Id { get; set; }
        public string Programme_Name { get; set; }
        public int Department_Id { get; set; }
        public string Department_Name { get; set; }
        public Nullable<int> Department_Option_Id { get; set; }
        public string Department_Option_Name { get; set; }
        public int Session_Id { get; set; }
        public string Session_Name { get; set; }
        public Nullable<double> RAW_SCORE { get; set; }
        public string Scanned_Copy_Url { get; set; }
        public string Applicant_Jamb_Registration_Number { get; set; }
        public System.DateTime Date_Submitted { get; set; }
    }
}
