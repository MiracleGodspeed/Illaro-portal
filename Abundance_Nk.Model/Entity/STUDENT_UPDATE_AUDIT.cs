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
    
    public partial class STUDENT_UPDATE_AUDIT
    {
        public long Student_Update_Audit_Id { get; set; }
        public long Student_Id { get; set; }
        public long User_Id { get; set; }
        public System.DateTime Date { get; set; }
    
        public virtual USER USER { get; set; }
        public virtual STUDENT STUDENT { get; set; }
    }
}
