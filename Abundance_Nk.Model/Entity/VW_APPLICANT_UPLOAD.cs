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
    
    public partial class VW_APPLICANT_UPLOAD
    {
        public string Name { get; set; }
        public string Image_File_Url { get; set; }
        public string Email { get; set; }
        public string Mobile_Phone { get; set; }
        public string OLevelResult { get; set; }
        public int O_Level_Exam_Sitting_Id { get; set; }
        public string O_Level_Exam_Sitting_Name { get; set; }
        public string CertificateUrl { get; set; }
        public string ResultUrl { get; set; }
        public int Programme_Id { get; set; }
        public string Programme_Short_Name { get; set; }
        public string Programme_Name { get; set; }
        public int Department_Id { get; set; }
        public string Department_Name { get; set; }
        public string Department_Code { get; set; }
        public Nullable<int> Department_Option_Id { get; set; }
        public string Department_Option_Name { get; set; }
        public int Session_Id { get; set; }
        public string Session_Name { get; set; }
        public string Application_Form_Number { get; set; }
        public long Application_Form_Id { get; set; }
        public int O_Level_Type_Id { get; set; }
        public string O_Level_Type_Name { get; set; }
        public string O_Level_Type_Short_Name { get; set; }
        public System.DateTime Date_Submitted { get; set; }
        public long Person_Id { get; set; }
        public string IT_Letter_Of_Completion { get; set; }
    }
}
