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
    
    public partial class PAYMENT_ETRANZACT_TYPE
    {
        public PAYMENT_ETRANZACT_TYPE()
        {
            this.PAYMENT_ETRANZACT = new HashSet<PAYMENT_ETRANZACT>();
        }
    
        public int Payment_Etranzact_Type_Id { get; set; }
        public string Payment_Etranzact_Type_Name { get; set; }
        public int Fee_Type_Id { get; set; }
        public Nullable<int> Programme_Id { get; set; }
        public Nullable<int> Level_Id { get; set; }
        public Nullable<int> Payment_Mode_Id { get; set; }
        public Nullable<int> Session_Id { get; set; }
    
        public virtual FEE_TYPE FEE_TYPE { get; set; }
        public virtual LEVEL LEVEL { get; set; }
        public virtual ICollection<PAYMENT_ETRANZACT> PAYMENT_ETRANZACT { get; set; }
        public virtual PAYMENT_MODE PAYMENT_MODE { get; set; }
        public virtual PROGRAMME PROGRAMME { get; set; }
        public virtual SESSION SESSION { get; set; }
    }
}
