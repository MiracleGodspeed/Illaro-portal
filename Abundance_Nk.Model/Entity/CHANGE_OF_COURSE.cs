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
    
    public partial class CHANGE_OF_COURSE
    {
        public int Change_Of_Course_Id { get; set; }
        public string Jamb_Registration_Number { get; set; }
        public Nullable<long> Application_Form_Id { get; set; }
        public int Session_Id { get; set; }
        public Nullable<long> New_Person_Id { get; set; }
        public Nullable<long> Old_Person_Id { get; set; }
    
        public virtual APPLICATION_FORM APPLICATION_FORM { get; set; }
        public virtual PERSON PERSON { get; set; }
        public virtual PERSON PERSON1 { get; set; }
        public virtual SESSION SESSION { get; set; }
    }
}
