using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Translator
{
    public class FeeinPaymentTranslator : TranslatorBase<FeeinPayment, FEE_IN_PAYMENT>
    {
        private FeeTranslator feeTranslator;
        private PaymentTranslator paymentTranslator;

        public FeeinPaymentTranslator()
        {
            feeTranslator = new FeeTranslator();
            paymentTranslator = new PaymentTranslator();
        }

        public override FeeinPayment TranslateToModel(FEE_IN_PAYMENT entity)
        {
            try
            {
                FeeinPayment model = null;
                if (entity != null)
                {
                    model = new FeeinPayment();
                    model.FeeInPaymentId = entity.Fee_In_Payment_Id;
                    model.Fee = feeTranslator.Translate(entity.FEE);
                    model.Payment = paymentTranslator.Translate(entity.PAYMENT);
                    model.IsIncluded = entity.Is_Included;
                }

                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override FEE_IN_PAYMENT TranslateToEntity(FeeinPayment model)
        {
            try
            {
                FEE_IN_PAYMENT entity = null;
                if (model != null)
                {
                    entity = new FEE_IN_PAYMENT();
                    entity.Fee_In_Payment_Id = model.FeeInPaymentId;
                    entity.Fee_Id = model.Fee.Id;
                    entity.Payment_Id = model.Payment.Id;
                    entity.Is_Included = model.IsIncluded;

                }

                return entity;
            }
            catch (Exception)
            {
                throw;
            }
        }



    }
}
