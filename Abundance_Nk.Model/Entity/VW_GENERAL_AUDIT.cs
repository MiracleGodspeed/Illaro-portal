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
    
    public partial class VW_GENERAL_AUDIT
    {
        public long User_Id { get; set; }
        public string User_Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public Nullable<bool> Activated { get; set; }
        public Nullable<bool> Super_Admin { get; set; }
        public System.DateTime LastLoginDate { get; set; }
        public int Role_Id { get; set; }
        public string Role_Name { get; set; }
        public string Action { get; set; }
        public string Operation { get; set; }
        public string Initial_Values { get; set; }
        public string Current_Values { get; set; }
        public System.DateTime Time { get; set; }
        public string Client { get; set; }
    }
}
