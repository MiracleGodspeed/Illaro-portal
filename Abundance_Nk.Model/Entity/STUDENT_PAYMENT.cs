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
    
    public partial class STUDENT_PAYMENT
    {
        public long Payment_Id { get; set; }
        public long Person_Id { get; set; }
        public int Session_Semester_Id { get; set; }
        public int Level_Id { get; set; }
    
        public virtual LEVEL LEVEL { get; set; }
        public virtual PAYMENT PAYMENT { get; set; }
        public virtual SESSION_SEMESTER SESSION_SEMESTER { get; set; }
        public virtual STUDENT STUDENT { get; set; }
    }
}
