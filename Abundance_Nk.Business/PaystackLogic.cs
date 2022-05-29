using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Model.Translator;
using Newtonsoft.Json;
using RestSharp;
namespace Abundance_Nk.Business
{
    public class PaystackLogic : BusinessBaseLogic<Paystack, PAYMENT_PAYSTACK>
    {
        private RestClient client;
        protected RestRequest request;
        public static string RestUrl = "https://api.paystack.co/";
        static string ApiEndPoint = "";
        public PaystackLogic()
        {
            translator = new PaystackTranslator();
            client = new RestClient(RestUrl);
        }

        public PaystackRepsonse MakePayment(Payment payment, string Bearer, string SubAccount, int transactionCharge)
        {
            PaystackRepsonse paystackRepsonse = null;
            try
            {
                decimal amount = 0;
                amount = payment.FeeDetails.Sum(a => a.Fee.Amount);
                //amount = amount + 100;
                amount = amount * 100;
                long milliseconds = DateTime.Now.Ticks;
                string testid = milliseconds.ToString();
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ApiEndPoint = "/transaction/initialize";
                request = new RestRequest(ApiEndPoint, Method.POST);
                request.AddHeader("accept", "application/json");
                request.AddHeader("Authorization", "Bearer " + Bearer);
                request.AddParameter("reference", payment.InvoiceNumber);
                request.AddParameter("transaction_charge", transactionCharge);
                var person = request.JsonSerializer.Serialize(payment);
                request.AddParameter("amount", amount);
                request.AddParameter("metadata", person);
                if (!String.IsNullOrEmpty(payment.Person.Email))
                {
                    request.AddParameter("email", payment.Person.Email);
                }
                else
                {
                    request.AddParameter("email", "support@lloydant.com");
                }
                if (!String.IsNullOrEmpty(SubAccount))
                {
                    request.AddParameter("subaccount", SubAccount);
                }

                var result = client.Execute(request);
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    paystackRepsonse = JsonConvert.DeserializeObject<PaystackRepsonse>(result.Content);
                }
                return paystackRepsonse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public PaystackRepsonse VerifyPayment(Payment payment, string Bearer)
        {
            PaystackRepsonse paystackRepsonse = null;
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ApiEndPoint = "/transaction/verify/" + payment.InvoiceNumber;
                request = new RestRequest(ApiEndPoint, Method.GET);
                request.AddHeader("accept", "application/json");
                request.AddHeader("Authorization", "Bearer " + Bearer);
                var result = client.Execute(request);
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    paystackRepsonse = JsonConvert.DeserializeObject<PaystackRepsonse>(result.Content);
                    Update(paystackRepsonse);
                }
                return paystackRepsonse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Paystack GetBy(Payment payment)
        {

            try
            {
                return GetModelBy(a => a.Payment_Id == payment.Id);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Paystack GetBy(string reference)
        {

            try
            {
                return GetModelBy(a => a.PAYMENT.Invoice_Number == reference);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool Update(PaystackRepsonse PaystackRepsonse)
        {
            try
            {
                Expression<Func<PAYMENT_PAYSTACK, bool>> selector = p => p.PAYMENT.Invoice_Number == PaystackRepsonse.data.reference;
                PAYMENT_PAYSTACK _paystackEntity = GetEntityBy(selector);
                if (_paystackEntity != null)
                {
                    _paystackEntity.amount = PaystackRepsonse.data.amount;
                    _paystackEntity.bank = PaystackRepsonse.data.authorization.bank;
                    _paystackEntity.brand = PaystackRepsonse.data.authorization.brand;
                    _paystackEntity.card_type = PaystackRepsonse.data.authorization.card_type;
                    _paystackEntity.channel = PaystackRepsonse.data.channel;
                    _paystackEntity.country_code = PaystackRepsonse.data.authorization.country_code;
                    _paystackEntity.currency = PaystackRepsonse.data.currency;
                    _paystackEntity.domain = PaystackRepsonse.data.domain;
                    _paystackEntity.exp_month = PaystackRepsonse.data.authorization.exp_month;
                    _paystackEntity.exp_year = PaystackRepsonse.data.authorization.exp_year;
                    _paystackEntity.fees = PaystackRepsonse.data.fees.ToString();
                    _paystackEntity.gateway_response = PaystackRepsonse.data.gateway_response;
                    _paystackEntity.ip_address = PaystackRepsonse.data.ip_address;
                    _paystackEntity.last4 = PaystackRepsonse.data.authorization.last4;
                    _paystackEntity.message = PaystackRepsonse.message;
                    _paystackEntity.reference = PaystackRepsonse.data.reference;
                    _paystackEntity.reusable = PaystackRepsonse.data.authorization.reusable;
                    _paystackEntity.signature = PaystackRepsonse.data.authorization.signature;
                    _paystackEntity.status = PaystackRepsonse.data.status;
                    _paystackEntity.transaction_date = PaystackRepsonse.data.transaction_date;
                    int modifiedRecordCount = Save();
                    if (modifiedRecordCount <= 0)
                    {
                        return false;
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Payment ValidatePayment(string reference)
        {
            try
            {
                var details = GetModelBy(a => a.PAYMENT.Invoice_Number == reference);
                if (details != null && details.status == "success" && (details.gateway_response.Contains("Approved") || details.gateway_response.Contains("Successful") || details.gateway_response.Contains("transaction Successful")) && details.domain == "live")
                {
                    return details.Payment;
                }
                //return details.Payment;
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool ValidateAmountPaid(string reference, decimal AmountToBePaid)
        {
            try
            {
                var payment = GetModelBy(a => a.PAYMENT.Invoice_Number == reference && a.status.Contains("success") && a.amount >= AmountToBePaid);
                if (payment != null && payment.Payment.Id > 0)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
