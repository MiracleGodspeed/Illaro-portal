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
    
    public partial class MENU_GROUP
    {
        public MENU_GROUP()
        {
            this.MENU = new HashSet<MENU>();
        }
    
        public int Menu_Group_Id { get; set; }
        public string Name { get; set; }
        public bool Activated { get; set; }
    
        public virtual ICollection<MENU> MENU { get; set; }
    }
}