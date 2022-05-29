using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

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
        public string ApplicationFormNumber { get; set; }

        public string MatricNumber { get; set; }
    }



}
