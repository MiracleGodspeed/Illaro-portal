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
    
    public partial class JAMB_O_LEVEL_DETAIL
    {
        public long Id { get; set; }
        public long Jamb_O_Level_Id { get; set; }
        public int Subject_Id { get; set; }
        public int Grade_Id { get; set; }
    
        public virtual JAMB_O_LEVEL JAMB_O_LEVEL { get; set; }
        public virtual O_LEVEL_GRADE O_LEVEL_GRADE { get; set; }
        public virtual O_LEVEL_SUBJECT O_LEVEL_SUBJECT { get; set; }
    }
}
