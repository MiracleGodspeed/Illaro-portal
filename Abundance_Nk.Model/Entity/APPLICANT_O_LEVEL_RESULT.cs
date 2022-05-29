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
    
    public partial class APPLICANT_O_LEVEL_RESULT
    {
        public APPLICANT_O_LEVEL_RESULT()
        {
            this.APPLICANT_O_LEVEL_RESULT_AUDIT = new HashSet<APPLICANT_O_LEVEL_RESULT_AUDIT>();
            this.APPLICANT_O_LEVEL_RESULT_DETAIL_AUDIT = new HashSet<APPLICANT_O_LEVEL_RESULT_DETAIL_AUDIT>();
            this.APPLICANT_O_LEVEL_RESULT_DETAIL = new HashSet<APPLICANT_O_LEVEL_RESULT_DETAIL>();
        }
    
        public long Applicant_O_Level_Result_Id { get; set; }
        public long Person_Id { get; set; }
        public string Exam_Number { get; set; }
        public int Exam_Year { get; set; }
        public int O_Level_Exam_Sitting_Id { get; set; }
        public int O_Level_Type_Id { get; set; }
        public Nullable<long> Application_Form_Id { get; set; }
        public string Scanned_Copy_Url { get; set; }
        public string Scratch_Card_Pin { get; set; }
    
        public virtual APPLICATION_FORM APPLICATION_FORM { get; set; }
        public virtual ICollection<APPLICANT_O_LEVEL_RESULT_AUDIT> APPLICANT_O_LEVEL_RESULT_AUDIT { get; set; }
        public virtual ICollection<APPLICANT_O_LEVEL_RESULT_DETAIL_AUDIT> APPLICANT_O_LEVEL_RESULT_DETAIL_AUDIT { get; set; }
        public virtual PERSON PERSON { get; set; }
        public virtual ICollection<APPLICANT_O_LEVEL_RESULT_DETAIL> APPLICANT_O_LEVEL_RESULT_DETAIL { get; set; }
        public virtual O_LEVEL_EXAM_SITTING O_LEVEL_EXAM_SITTING { get; set; }
        public virtual O_LEVEL_TYPE O_LEVEL_TYPE { get; set; }
    }
}
