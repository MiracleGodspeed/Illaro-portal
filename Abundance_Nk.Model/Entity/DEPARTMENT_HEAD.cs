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
    
    public partial class DEPARTMENT_HEAD
    {
        public int Department_Head_Id { get; set; }
        public long Person_Id { get; set; }
        public System.DateTime Date { get; set; }
        public int Department_Id { get; set; }
    
        public virtual DEPARTMENT DEPARTMENT { get; set; }
        public virtual STAFF STAFF { get; set; }
    }
}