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
    
    public partial class DELIVERY_SERVICE_ZONE
    {
        public DELIVERY_SERVICE_ZONE()
        {
            this.TRANSCRIPT_REQUEST = new HashSet<TRANSCRIPT_REQUEST>();
        }
    
        public int Delivery_Service_Zone_Id { get; set; }
        public int Delivery_Service_Id { get; set; }
        public int Geo_Zone_Id { get; set; }
        public int Fee_Id { get; set; }
        public string Country_Id { get; set; }
        public bool Activated { get; set; }
    
        public virtual COUNTRY COUNTRY { get; set; }
        public virtual DELIVERY_SERVICE DELIVERY_SERVICE { get; set; }
        public virtual FEE FEE { get; set; }
        public virtual GEO_ZONE GEO_ZONE { get; set; }
        public virtual ICollection<TRANSCRIPT_REQUEST> TRANSCRIPT_REQUEST { get; set; }
    }
}