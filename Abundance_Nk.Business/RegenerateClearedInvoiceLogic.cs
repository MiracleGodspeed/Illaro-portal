using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Model.Translator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Business
{
    public class RegenerateClearedInvoiceLogic : BusinessBaseLogic<RegenerateClearedInvoice, REGENERATED_CLEARED_INVOICE>
    {

        public RegenerateClearedInvoiceLogic()
        {
            translator = new RegenerateClearedInvoiceTranslator();
        }
        public bool SaveClearedInvoice(Payment payment, RemitaPayment remitaPayment)
        {
            if(payment?.Id>0 && remitaPayment?.payment?.Id > 0)
            {
                RegenerateClearedInvoice regenerateClearedInvoice = new RegenerateClearedInvoice()
                {
                    Date_Created = DateTime.UtcNow,
                    Description = remitaPayment.Description,
                    Transaction_Date = remitaPayment.TransactionDate,
                    FeeTypeId = payment.FeeType.Id,
                    InvoiceNumber = payment.InvoiceNumber,
                    OrderId = remitaPayment.OrderId,
                    SessionId = payment.Session.Id,
                    PaymentId = payment.Id,
                    PaymentModeId = payment.PaymentMode.Id,
                    PersonId = payment.Person.Id,
                    Reference_Number = remitaPayment.RRR,
                    TransactionAmount = remitaPayment.TransactionAmount,

                };
                var isCreated=Create(regenerateClearedInvoice);
                if (isCreated?.Id > 0)
                    return true;
            }
            return false;
        }
    }
}
