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
    
    public partial class HOSTEL_ROOM_CORNER
    {
        public HOSTEL_ROOM_CORNER()
        {
            this.HOSTEL_ALLOCATION = new HashSet<HOSTEL_ALLOCATION>();
            this.HOSTEL_ALLOCATION_CRITERIA = new HashSet<HOSTEL_ALLOCATION_CRITERIA>();
        }
    
        public int Corner_Id { get; set; }
        public string Corner_Name { get; set; }
        public long Room_Id { get; set; }
        public bool Activated { get; set; }
    
        public virtual ICollection<HOSTEL_ALLOCATION> HOSTEL_ALLOCATION { get; set; }
        public virtual ICollection<HOSTEL_ALLOCATION_CRITERIA> HOSTEL_ALLOCATION_CRITERIA { get; set; }
        public virtual HOSTEL_ROOM HOSTEL_ROOM { get; set; }
    }
}