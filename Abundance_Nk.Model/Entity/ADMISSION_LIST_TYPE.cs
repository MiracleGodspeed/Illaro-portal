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
    
    public partial class ADMISSION_LIST_TYPE
    {
        public ADMISSION_LIST_TYPE()
        {
            this.ADMISSION_LIST_BATCH = new HashSet<ADMISSION_LIST_BATCH>();
        }
    
        public int Admission_List_Type_Id { get; set; }
        public string Admission_List_Type_Name { get; set; }
    
        public virtual ICollection<ADMISSION_LIST_BATCH> ADMISSION_LIST_BATCH { get; set; }
    }
}
