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
    
    public partial class ATTENDANCE
    {
        public long Attendance_Id { get; set; }
        public int Attendance_Status_Id { get; set; }
        public int Session_Term_Id { get; set; }
        public System.DateTime Date { get; set; }
    
        public virtual ATTENDANCE_STATUS ATTENDANCE_STATUS { get; set; }
        public virtual SESSION_SEMESTER SESSION_SEMESTER { get; set; }
    }
}
