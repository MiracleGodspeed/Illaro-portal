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
    
    public partial class E_COURSE_CONTENT_DOWNLOAD
    {
        public long E_Course_Content_Download_Id { get; set; }
        public long E_Course_Content_Id { get; set; }
        public long Person_Id { get; set; }
        public Nullable<System.DateTime> Date_Viewed { get; set; }
    
        public virtual PERSON PERSON { get; set; }
        public virtual E_COURSE_CONTENT E_COURSE_CONTENT { get; set; }
    }
}
