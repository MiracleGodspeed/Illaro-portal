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
    
    public partial class VW_CERTIFICATE_VERFICATION_REMITA
    {
        public long Transcript_Request_Id { get; set; }
        public System.DateTime Date_Requested { get; set; }
        public Nullable<int> Delivery_Service_Zone_Id { get; set; }
        public Nullable<int> Delivery_Service_Id { get; set; }
        public string Delivery_Service { get; set; }
        public Nullable<int> Geo_Zone_Id { get; set; }
        public string Geo_Zone { get; set; }
        public string Destination_Address { get; set; }
        public string Country_Id { get; set; }
        public string Country_Name { get; set; }
        public string State_Id { get; set; }
        public string State_Name { get; set; }
        public string Invoice_Number { get; set; }
        public string RRR { get; set; }
        public string Status { get; set; }
        public System.DateTime Transaction_Date { get; set; }
        public decimal Transaction_Amount { get; set; }
        public string Last_Name { get; set; }
        public string First_Name { get; set; }
        public string Other_Name { get; set; }
        public string Request_Type { get; set; }
        public int Transcript_Status_Id { get; set; }
        public string Transcript_Status_Name { get; set; }
        public string Matric_Number { get; set; }
        public string Programme_Name { get; set; }
        public string Department_Name { get; set; }
        public int Fee_Type_Id { get; set; }
        public Nullable<long> Payment_Id { get; set; }
    }
}
