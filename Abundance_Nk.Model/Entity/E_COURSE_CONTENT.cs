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
    
    public partial class E_COURSE_CONTENT
    {
        public E_COURSE_CONTENT()
        {
            this.E_COURSE_CONTENT_DOWNLOAD = new HashSet<E_COURSE_CONTENT_DOWNLOAD>();
        }
    
        public long Id { get; set; }
        public long Course_Id { get; set; }
        public int E_Content_Type_Id { get; set; }
        public string Url { get; set; }
        public Nullable<int> Views { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public bool Active { get; set; }
        public string Video_Url { get; set; }
        public bool IsDelete { get; set; }
        public string LiveStream_Url { get; set; }
    
        public virtual COURSE COURSE { get; set; }
        public virtual E_CONTENT_TYPE E_CONTENT_TYPE { get; set; }
        public virtual ICollection<E_COURSE_CONTENT_DOWNLOAD> E_COURSE_CONTENT_DOWNLOAD { get; set; }
    }
}
