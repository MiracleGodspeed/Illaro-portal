using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Model
{
    public class PaymentVerification
    {
        public Payment Payment { get; set; }
        public User User { get; set; }
        public StudentPayment StudentPayment { get; set; }
        public Department Department { get; set; }
        public DateTime DateVerified { get; set; }
        public string Comment { get; set; }
    }

    public class FeePaymentVerification
    {
        public string fullname { get; set; }
        public string matricNumber { get; set; }
        public string invoiceNumber { get; set; }
        public DateTime? queryDate { get; set; }
        public string transactionDate { get; set; }
        public string verifiedDate { get; set; }
        public string rrr { get; set; }
        public string status { get; set; }
        public string departmentName { get; set; }
        public string description { get; set; }
        public string amount { get; set; }
        public long paymentId { get; set; }
    }
}
