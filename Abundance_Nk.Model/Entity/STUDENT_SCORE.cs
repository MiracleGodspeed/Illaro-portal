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
    
    public partial class STUDENT_SCORE
    {
        public long Student_Score_Id { get; set; }
        public long Person_Id { get; set; }
        public long Class_Assessment_Id { get; set; }
        public int Subject_Id { get; set; }
        public byte Score_Obtained { get; set; }
        public long Score_Entered_By_Staff_Id { get; set; }
        public System.DateTime Score_Entered_Date { get; set; }
    
        public virtual CLASS_ASSESSMENT CLASS_ASSESSMENT { get; set; }
        public virtual O_LEVEL_SUBJECT O_LEVEL_SUBJECT { get; set; }
        public virtual STAFF STAFF { get; set; }
        public virtual STUDENT STUDENT { get; set; }
    }
}
