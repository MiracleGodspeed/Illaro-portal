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
    
    public partial class HOSTEL_ROOM
    {
        public HOSTEL_ROOM()
        {
            this.HOSTEL_ALLOCATION = new HashSet<HOSTEL_ALLOCATION>();
            this.HOSTEL_ALLOCATION_CRITERIA = new HashSet<HOSTEL_ALLOCATION_CRITERIA>();
            this.HOSTEL_ROOM_CORNER = new HashSet<HOSTEL_ROOM_CORNER>();
        }
    
        public long Room_Id { get; set; }
        public string Room_Number { get; set; }
        public int Series_Id { get; set; }
        public bool Reserved { get; set; }
        public bool Activated { get; set; }
        public int Hostel_Id { get; set; }
    
        public virtual HOSTEL HOSTEL { get; set; }
        public virtual ICollection<HOSTEL_ALLOCATION> HOSTEL_ALLOCATION { get; set; }
        public virtual ICollection<HOSTEL_ALLOCATION_CRITERIA> HOSTEL_ALLOCATION_CRITERIA { get; set; }
        public virtual ICollection<HOSTEL_ROOM_CORNER> HOSTEL_ROOM_CORNER { get; set; }
        public virtual HOSTEL_SERIES HOSTEL_SERIES { get; set; }
    }
}
