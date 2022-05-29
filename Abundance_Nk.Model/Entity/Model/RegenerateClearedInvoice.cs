using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Model
{
    public class RegenerateClearedInvoice
    {
        public long Id { get; set; }
        public long PaymentId { get; set; }
        public long PersonId { get; set; }
        public decimal? TransactionAmount { get; set; }
        public DateTime Date_Created { get; set; }
        public string InvoiceNumber { get; set; }
        public string Reference_Number { get; set; }
        public int FeeTypeId { get; set; }
        public int PaymentModeId { get; set; }
        public string Description { get; set; }
        public string OrderId { get; set; }
        public DateTime? Transaction_Date { get; set; }
        public int SessionId { get; set; }
    }
}
