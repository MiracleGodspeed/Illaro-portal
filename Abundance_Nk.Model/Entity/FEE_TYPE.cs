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
    
    public partial class FEE_TYPE
    {
        public FEE_TYPE()
        {
            this.APPLICATION_PROGRAMME_FEE = new HashSet<APPLICATION_PROGRAMME_FEE>();
            this.FEE_DETAIL = new HashSet<FEE_DETAIL>();
            this.FEE_DETAIL_AUDIT = new HashSet<FEE_DETAIL_AUDIT>();
            this.PAYMENT_ETRANZACT_TYPE = new HashSet<PAYMENT_ETRANZACT_TYPE>();
            this.PAYMENT = new HashSet<PAYMENT>();
            this.PAYMENT_TERMINAL = new HashSet<PAYMENT_TERMINAL>();
            this.SCRATCH_CARD_TYPE = new HashSet<SCRATCH_CARD_TYPE>();
            this.FEE_SETUP = new HashSet<FEE_SETUP>();
            this.FEE_SETUP_AUDIT = new HashSet<FEE_SETUP_AUDIT>();
        }
    
        public int Fee_Type_Id { get; set; }
        public string Fee_Type_Name { get; set; }
        public string Fee_Type_Description { get; set; }
    
        public virtual ICollection<APPLICATION_PROGRAMME_FEE> APPLICATION_PROGRAMME_FEE { get; set; }
        public virtual ICollection<FEE_DETAIL> FEE_DETAIL { get; set; }
        public virtual ICollection<FEE_DETAIL_AUDIT> FEE_DETAIL_AUDIT { get; set; }
        public virtual ICollection<PAYMENT_ETRANZACT_TYPE> PAYMENT_ETRANZACT_TYPE { get; set; }
        public virtual ICollection<PAYMENT> PAYMENT { get; set; }
        public virtual ICollection<PAYMENT_TERMINAL> PAYMENT_TERMINAL { get; set; }
        public virtual ICollection<SCRATCH_CARD_TYPE> SCRATCH_CARD_TYPE { get; set; }
        public virtual ICollection<FEE_SETUP> FEE_SETUP { get; set; }
        public virtual ICollection<FEE_SETUP_AUDIT> FEE_SETUP_AUDIT { get; set; }
    }
}