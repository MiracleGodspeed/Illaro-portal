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
    
    public partial class LOCAL_GOVERNMENT
    {
        public LOCAL_GOVERNMENT()
        {
            this.PERSON_AUDIT = new HashSet<PERSON_AUDIT>();
            this.PERSON_AUDIT1 = new HashSet<PERSON_AUDIT>();
            this.PERSON = new HashSet<PERSON>();
            this.PERSON1 = new HashSet<PERSON>();
        }
    
        public int Local_Government_Id { get; set; }
        public string Local_Government_Name { get; set; }
        public string State_Id { get; set; }
    
        public virtual STATE STATE { get; set; }
        public virtual ICollection<PERSON_AUDIT> PERSON_AUDIT { get; set; }
        public virtual ICollection<PERSON_AUDIT> PERSON_AUDIT1 { get; set; }
        public virtual ICollection<PERSON> PERSON { get; set; }
        public virtual ICollection<PERSON> PERSON1 { get; set; }
    }
}
