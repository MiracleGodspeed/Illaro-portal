using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Model;

namespace Abundance_Nk.Model.Translator
{
    public class PaystackTranslator : TranslatorBase<Paystack, PAYMENT_PAYSTACK>
    {
        private PaymentTranslator paymentTranslator;

        public PaystackTranslator()
        {
            paymentTranslator = new PaymentTranslator();
        }
        public override Paystack TranslateToModel(PAYMENT_PAYSTACK entity)
        {
            try
            {
                Paystack model = null;
                if (entity != null)
                {
                    model = new Paystack();
                    model.Payment = paymentTranslator.Translate(entity.PAYMENT);
                    model.amount = entity.amount;
                    model.bank = entity.bank;
                    model.brand = entity.brand;
                    model.card_type = entity.card_type;
                    model.channel = entity.channel;
                    model.country_code = entity.country_code;
                    model.currency = entity.currency;
                    model.domain = entity.domain;
                    model.exp_month = entity.exp_month;
                    model.exp_year = entity.exp_year;
                    model.fees = entity.fees;
                    model.gateway_response = entity.gateway_response;
                    model.ip_address = entity.ip_address;
                    model.last4 = entity.last4;
                    model.message = entity.message;
                    model.reference = entity.reference;
                    model.reusable = entity.reusable;
                    model.signature = entity.signature;
                    model.status = entity.status;
                    model.transaction_date = entity.transaction_date;
                    model.authorization_url = entity.authorization_url;
                    model.access_code = entity.access_code;
                }
                return model;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override PAYMENT_PAYSTACK TranslateToEntity(Paystack model)
        {
            try
            {
                PAYMENT_PAYSTACK entity = null;
                if (model != null)
                {
                    entity = new PAYMENT_PAYSTACK();
                    entity.Payment_Id = model.Payment.Id;
                    entity.amount = model.amount;
                    entity.bank = model.bank;
                    entity.brand = model.brand;
                    entity.card_type = model.card_type;
                    entity.channel = model.channel;
                    entity.country_code = model.country_code;
                    entity.currency = model.currency;
                    entity.domain = model.domain;
                    entity.exp_month = model.exp_month;
                    entity.exp_year = model.exp_year;
                    entity.fees = model.fees;
                    entity.gateway_response = model.gateway_response;
                    entity.ip_address = model.ip_address;
                    entity.last4 = model.last4;
                    entity.message = model.message;
                    entity.reference = model.reference;
                    entity.reusable = model.reusable;
                    entity.signature = model.signature;
                    entity.status = model.status;
                    entity.transaction_date = model.transaction_date;
                    entity.authorization_url = model.authorization_url;
                    entity.access_code = model.access_code;
                }

                return entity;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
