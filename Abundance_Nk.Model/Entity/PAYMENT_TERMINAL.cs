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
    
    public partial class PAYMENT_TERMINAL
    {
        public PAYMENT_TERMINAL()
        {
            this.PAYMENT_ETRANZACT = new HashSet<PAYMENT_ETRANZACT>();
        }
    
        public long Payment_Terminal_Id { get; set; }
        public string Terminal_Id { get; set; }
        public int Fee_Type_Id { get; set; }
        public int Session_Id { get; set; }
    
        public virtual FEE_TYPE FEE_TYPE { get; set; }
        public virtual ICollection<PAYMENT_ETRANZACT> PAYMENT_ETRANZACT { get; set; }
        public virtual SESSION SESSION { get; set; }
    }
}