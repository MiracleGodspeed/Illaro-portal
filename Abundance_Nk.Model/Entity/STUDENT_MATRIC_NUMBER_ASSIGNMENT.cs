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
    
    public partial class STUDENT_MATRIC_NUMBER_ASSIGNMENT
    {
        public int Faculty_Id { get; set; }
        public int Department_Id { get; set; }
        public int Level_Id { get; set; }
        public int Programme_Id { get; set; }
        public int Session_Id { get; set; }
        public int Matric_Serial_Number_Start_From { get; set; }
        public string Matric_Number_Start_From { get; set; }
        public bool Used { get; set; }
    
        public virtual DEPARTMENT DEPARTMENT { get; set; }
        public virtual FACULTY FACULTY { get; set; }
        public virtual LEVEL LEVEL { get; set; }
        public virtual PROGRAMME PROGRAMME { get; set; }
        public virtual SESSION SESSION { get; set; }
    }
}