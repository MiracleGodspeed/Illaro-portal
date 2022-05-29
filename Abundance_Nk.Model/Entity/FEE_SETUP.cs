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
    
    public partial class FEE_SETUP
    {
        public FEE_SETUP()
        {
            this.FEE_SETUP_AUDIT = new HashSet<FEE_SETUP_AUDIT>();
            this.FEESETUP_FEE = new HashSet<FEESETUP_FEE>();
        }
    
        public int FeeSetup_Id { get; set; }
        public string FeeSetUp_Name { get; set; }
        public int PaymentMode_Id { get; set; }
        public int FeeType_Id { get; set; }
        public int Session_Id { get; set; }
        public decimal Amount { get; set; }
        public bool Activated { get; set; }
        public long User_Id { get; set; }
        public System.DateTime Date_Created { get; set; }
    
        public virtual ICollection<FEE_SETUP_AUDIT> FEE_SETUP_AUDIT { get; set; }
        public virtual FEE_TYPE FEE_TYPE { get; set; }
        public virtual PAYMENT_MODE PAYMENT_MODE { get; set; }
        public virtual SESSION SESSION { get; set; }
        public virtual USER USER { get; set; }
        public virtual ICollection<FEESETUP_FEE> FEESETUP_FEE { get; set; }
    }
}
