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
    
    public partial class STUDENT_SPONSOR
    {
        public long Person_Id { get; set; }
        public int Relationship_Id { get; set; }
        public string Sponsor_Name { get; set; }
        public string Sponsor_Contact_Address { get; set; }
        public string Sponsor_Mobile_Phone { get; set; }
        public string Email { get; set; }
    
        public virtual RELATIONSHIP RELATIONSHIP { get; set; }
        public virtual STUDENT STUDENT { get; set; }
    }
}
