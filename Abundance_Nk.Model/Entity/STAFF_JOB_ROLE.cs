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
    
    public partial class STAFF_JOB_ROLE
    {
        public long Staff_Job_Role_Id { get; set; }
        public long Staff_Id { get; set; }
        public int Job_role_Id { get; set; }
        public System.DateTime Date_Entered { get; set; }
    
        public virtual JOB_ROLE JOB_ROLE { get; set; }
        public virtual STAFF STAFF { get; set; }
    }
}