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
    
    public partial class ATTENDANCE_STATUS
    {
        public ATTENDANCE_STATUS()
        {
            this.ATTENDANCE = new HashSet<ATTENDANCE>();
        }
    
        public int Attendance_Status_Id { get; set; }
        public string Attendance_Name { get; set; }
        public string Attendance_Abbreviation { get; set; }
    
        public virtual ICollection<ATTENDANCE> ATTENDANCE { get; set; }
    }
}
