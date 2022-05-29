using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Model
{
    public class FeeinPayment
    {
        public long FeeInPaymentId { get; set; }
        public bool IsIncluded { get; set; }

        public Payment  Payment { get; set; }
        public Fee Fee { get; set; }
    }
}
