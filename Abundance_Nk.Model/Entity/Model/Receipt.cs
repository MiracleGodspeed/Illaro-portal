using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using Abundance_Nk.Model.Entity.Model;

namespace Abundance_Nk.Model.Model
{
    public class Receipt
    {
        [Display(Name="Receipt Number")]
        public string Number { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Confirmation Order Number")]
        public string ConfirmationOrderNumber { get; set; }

        [Display(Name = "Invoice Number")]
        public string InvoiceNumber { get; set; }

        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Display(Name = "Amount In Words")]
        public string AmountInWords { get; set; }

        [Display(Name = "Purpose")]
        public string Purpose { get; set; }

        [Display(Name = "Date")]
        public DateTime Date { get; set; }
        public DateTime VerificationDate { get; set; }
        public string VerificationDateString { get; set; }
        public string ApplicationFormNumber { get; set; }
        public string PaymentMode { get; set; }
        public string ReceiptNumber { get; set; }
        public string MatricNumber { get; set; }
        public string barcodeImageUrl { get; set; }
        public string VerifiedBy { get; set; }
        public bool IsShortFall { get; set; }
        public string PaymentId { get; set; }
        public string Session { get; set; }
        public string Level { get; set; }
        public string Description { get; set; }
        public string Department { get; set; }
        public string Programme { get; set; }
        public string Faculty { get; set; }
        public List<FeeDetail> FeeDetails { get; set; }
        public PaymentVerification PaymentVerification { get; set; }
        public string ShortFallRRR { get; set; }
        public List<ClearanceLog> ClearanceLogs { get; set; }
        public Librarian Librarian { get; set; }
        public Bursar Bursar { get; set; }
        public StudentAffair StudentAffair { get; set; }
        public Hostel Hostel { get; set; }
        public DepartmentClearance DepartmentClearance { get; set; }
        public Department DepartmentObj { get; set; }
        public Health Health { get; set; }
        public bool IsVerified { get; set; }
    }

    public class Librarian
    {
        public string Name { get; set; }
        public string Date { get; set; }
        public string Signature { get; set; }
    }
    public class Bursar
    {
        public string Name { get; set; }
        public string Date { get; set; }
        public string Signature { get; set; }
    }
    public class StudentAffair
    {
        public string Name { get; set; }
        public string Date { get; set; }
        public string Signature { get; set; }
    }
    public class Health
    {
        public string Name { get; set; }
        public string Date { get; set; }
        public string Signature { get; set; }
    }
    public class DepartmentClearance
    {
        public string Name { get; set; }
        public string Date { get; set; }
        public string Signature { get; set; }
    }
    public class ELearningEmail
    {
        public string Name { get; set; }
        public string message { get; set; }
        public string header { get; set; }
        public string footer { get; set; }
    }

}
