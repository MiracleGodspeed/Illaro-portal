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
    
    public partial class GEO_ZONE
    {
        public GEO_ZONE()
        {
            this.DELIVERY_SERVICE_ZONE = new HashSet<DELIVERY_SERVICE_ZONE>();
            this.STATE_GEO_ZONE = new HashSet<STATE_GEO_ZONE>();
        }
    
        public int Geo_Zone_Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Activated { get; set; }
    
        public virtual ICollection<DELIVERY_SERVICE_ZONE> DELIVERY_SERVICE_ZONE { get; set; }
        public virtual ICollection<STATE_GEO_ZONE> STATE_GEO_ZONE { get; set; }
    }
}
