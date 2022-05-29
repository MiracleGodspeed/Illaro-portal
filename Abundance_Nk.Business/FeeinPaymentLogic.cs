using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Model.Translator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Business
{
    public class FeeinPaymentLogic : BusinessBaseLogic<FeeinPayment, FEE_IN_PAYMENT>
    {
        public FeeinPaymentLogic()
        {
            translator = new FeeinPaymentTranslator();
        }
        public string CreateRecord(Payment payment, int feeId, bool isInclude)
        {
            string Message = string.Empty;
            var hasTheFee=GetModelBy(f => f.Payment_Id == payment.Id && f.Fee_Id == feeId);
            if (hasTheFee?.FeeInPaymentId > 0)
            {
                if (isInclude != hasTheFee.IsIncluded)
                    Message = "You can not make changes to this Payment Invoice";
            }
            else
            {
                FeeinPayment feeinPayment = new FeeinPayment()
                {
                    Fee = new Fee { Id = feeId },
                    IsIncluded = isInclude,
                    Payment = payment,

                };
                Create(feeinPayment);
            }
            return Message;
        }
        public void DeleteBy(long PaymentID)
        {
            try
            {
                
                Expression<Func<FEE_IN_PAYMENT, bool>> selector = a => a.PAYMENT.Payment_Id == PaymentID;
                var feeinPayment = GetEntitiesBy(selector);
                if(feeinPayment.Count>0)
                Delete(feeinPayment);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
