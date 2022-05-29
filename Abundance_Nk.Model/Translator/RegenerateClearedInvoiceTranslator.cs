using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Translator
{
    public class RegenerateClearedInvoiceTranslator : TranslatorBase<RegenerateClearedInvoice, REGENERATED_CLEARED_INVOICE>
    {
        public override RegenerateClearedInvoice TranslateToModel(REGENERATED_CLEARED_INVOICE entity)
        {
            try
            {
                RegenerateClearedInvoice model = null;
                if (entity != null)
                {
                    model = new RegenerateClearedInvoice();
                    model.Id = entity.Id;
                    model.PaymentId = entity.PaymentId;
                    model.Description = entity.Description;
                    model.Date_Created = entity.Date_Created;
                    model.FeeTypeId = entity.FeeType_Id;
                    model.InvoiceNumber = entity.Invoice_Number;
                    model.OrderId = entity.Order_Id;
                    model.PaymentModeId = entity.Payment_Mode_Id;
                    model.PersonId = entity.PersonId;
                    model.Reference_Number = entity.Reference_Number;
                    model.SessionId = entity.Session_Id;
                    model.TransactionAmount = entity.Transaction_Amount;
                    model.Transaction_Date = entity.Transaction_Date;
                }

                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override REGENERATED_CLEARED_INVOICE TranslateToEntity(RegenerateClearedInvoice model)
        {
            try
            {
                REGENERATED_CLEARED_INVOICE entity = null;
                if (model != null)
                {
                    entity = new REGENERATED_CLEARED_INVOICE();
                    entity.PaymentId = model.PaymentId;
                    entity.Description = model.Description;
                    entity.Date_Created = model.Date_Created;
                    entity.FeeType_Id = model.FeeTypeId;
                    entity.Invoice_Number = model.InvoiceNumber;
                    entity.Order_Id = model.OrderId;
                    entity.Payment_Mode_Id = model.PaymentModeId;
                    entity.PersonId = model.PersonId;
                    entity.Reference_Number = model.Reference_Number;
                    entity.Session_Id = model.SessionId;
                    entity.Transaction_Amount = model.TransactionAmount;
                    entity.Transaction_Date = model.Transaction_Date;
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
