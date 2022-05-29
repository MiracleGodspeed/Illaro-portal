using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Model
{
    public class PaymentView
    {
        public long PersonId { get; set; }
        public long PaymentId { get; set; }
        public string InvoiceNumber { get; set; }
        public string ReceiptNumber { get; set; }
        public string ConfirmationOrderNumber { get; set; }

        public string BankCode { get; set; }
        public string BankName { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }

        public DateTime InvoiceGenerationDate { get; set; }
        public DateTime? PaymentDate { get; set; }

        public int FeeTypeId { get; set; }
        public string FeeTypeName { get; set; }

        public int PaymentTypeId { get; set; }
        public string PaymentTypeName { get; set; }

        public decimal? Amount { get; set; }
    }
}
