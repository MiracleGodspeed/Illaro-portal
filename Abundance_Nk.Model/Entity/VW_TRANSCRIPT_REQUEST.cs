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
    
    public partial class VW_TRANSCRIPT_REQUEST
    {
        public long Person_Id { get; set; }
        public string Name { get; set; }
        public string Matric_Number { get; set; }
        public Nullable<System.DateTime> Date_Requested { get; set; }
        public string Destination_Address { get; set; }
        public string Country_Name { get; set; }
        public string State_Name { get; set; }
        public long Transcript_Request_Id { get; set; }
        public string Request_Type { get; set; }
        public string Transcript_clearance_Status_Name { get; set; }
        public string Transcript_Status_Name { get; set; }
        public string Invoice_Number { get; set; }
        public string Confirmation_No { get; set; }
        public string Session_Name { get; set; }
        public int Programme_Id { get; set; }
        public string Programme_Name { get; set; }
        public int Department_Id { get; set; }
        public string Department_Name { get; set; }
        public long Payment_Id { get; set; }
    }
}
