using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Abundance_Nk.Model.Entity.Model;

namespace Abundance_Nk.Model.Model
{
    public class TranscriptRequest
    {
        public Int64 Id { get; set; }
        public Student student { get; set; }
        public Payment payment {get; set;}
        [Display(Name = "Date of Request")]
        public DateTime DateRequested {get; set;}
        [Display(Name="Destination Address")]
        public string DestinationAddress {get; set;}
        [Display(Name = "Destination Address State")]
        public State DestinationState {get; set;}
        [Display(Name = "Destination Address Country")]
        public Country DestinationCountry { get; set; }
       
        public TranscriptClearanceStatus transcriptClearanceStatus {get; set;}
       
        public TranscriptStatus transcriptStatus {get; set;}
        public string ConfirmationOrderNumber { get; set; }

        public string Amount { get; set; }
        public RemitaPayment remitaPayment { get; set; }
        public string RequestType { get; set; }
        public DeliveryServiceZone DeliveryServiceZone { get; set; }
        public string Name { get; set; }
        public string MatricNumber { get; set; }
        public string Status { get; set; }
        public string DeliveryService { get; set; }
        public string GeoZone { get; set; }
        public string CountryName { get; set; }
        public string StateName { get; set; }
        public string InvoiceNumber { get; set; }
        public string RRR { get; set; }
        public string TransactionDate { get; set; }
        public string TransactionAmount { get; set; }
        public string ProgrammeName { get; set; }
        public string DepartmentName { get; set; }
        public Int64 PaymentId { get; set; }
        public int transcriptStatusId { get; set; }
        public string TranscriptName { get; set; }
        public int? YearOfGraduation { get; set; }
        public string WorkPlace { get; set; }
        public bool? RequestStudentCopy { get; set; }
        public bool? StudentOnlyCopy { get; set; }
    }
}
