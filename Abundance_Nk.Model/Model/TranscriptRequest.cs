using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

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
    }
}
