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
    
    public partial class FEESETUP_FEE
    {
        public int FeeSetUpFee_Id { get; set; }
        public int FeeSetUp_Id { get; set; }
        public int Fee_Id { get; set; }
        public bool Activated { get; set; }
    
        public virtual FEE FEE { get; set; }
        public virtual FEE_SETUP FEE_SETUP { get; set; }
    }
}
