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
    
    public partial class ROLE
    {
        public ROLE()
        {
            this.MENU_IN_ROLE = new HashSet<MENU_IN_ROLE>();
            this.PERSON = new HashSet<PERSON>();
            this.PERSON1 = new HashSet<PERSON>();
            this.PERSON_AUDIT = new HashSet<PERSON_AUDIT>();
            this.PERSON_AUDIT1 = new HashSet<PERSON_AUDIT>();
            this.ROLE_RIGHT = new HashSet<ROLE_RIGHT>();
            this.USER = new HashSet<USER>();
        }
    
        public int Role_Id { get; set; }
        public string Role_Name { get; set; }
        public string Role_Description { get; set; }
    
        public virtual ICollection<MENU_IN_ROLE> MENU_IN_ROLE { get; set; }
        public virtual ICollection<PERSON> PERSON { get; set; }
        public virtual ICollection<PERSON> PERSON1 { get; set; }
        public virtual ICollection<PERSON_AUDIT> PERSON_AUDIT { get; set; }
        public virtual ICollection<PERSON_AUDIT> PERSON_AUDIT1 { get; set; }
        public virtual ICollection<ROLE_RIGHT> ROLE_RIGHT { get; set; }
        public virtual ICollection<USER> USER { get; set; }
    }
}
