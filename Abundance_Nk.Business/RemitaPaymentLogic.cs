using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Translator;
using System.Linq.Expressions;

namespace Abundance_Nk.Business
{
  public  class RemitaPaymentLogic : BusinessBaseLogic<RemitaPayment, REMITA_PAYMENT>
    {
      public RemitaPaymentLogic()
        {
            translator = new RemitaPaymentTranslator();
        }


      public RemitaPayment GetBy(long PaymentID)
      {
          try
          {
              Expression<Func<REMITA_PAYMENT, bool>> selector = a => a.PAYMENT.Payment_Id == PaymentID;
              return GetModelBy(selector);

          }
          catch (Exception)
          {
              throw;
          }
      }

      private REMITA_PAYMENT GetEntityBy(RemitaPayment remitaPayment)
      {
          try
          {
              Expression<Func<REMITA_PAYMENT, bool>> selector = s => s.PAYMENT.Payment_Id == remitaPayment.payment.Id;
              REMITA_PAYMENT entity = GetEntityBy(selector);

              return entity;
          }
          catch (Exception)
          {
              throw;
          }
      }


      public RemitaPayment GetBy(string OrderId)
      {
          try
          {
              Expression<Func<REMITA_PAYMENT, bool>> selector = s => s.OrderId == OrderId || s.RRR == OrderId;
              return GetModelBy(selector);
          }
          catch (Exception)
          {
              throw;
          }
      }


      public bool Modify (RemitaPayment remitaPayment)
      {
          try
          {
              REMITA_PAYMENT entity = GetEntityBy(remitaPayment);

              if (entity == null)
              {
                  throw new Exception(NoItemFound);
              }

              entity.RRR = remitaPayment.RRR;
              entity.Status = remitaPayment.Status;

              if (remitaPayment.BankCode != null)
              {
                  entity.Bank_Code = remitaPayment.BankCode;
              }
              if (remitaPayment.MerchantCode != null)
              {
                  entity.Merchant_Code = remitaPayment.MerchantCode;
              }
             
              if (remitaPayment.TransactionAmount > 0)
              {
                  entity.Transaction_Amount = remitaPayment.TransactionAmount;
              }

                if (remitaPayment.isVerified)
                {
                    entity.Used = remitaPayment.isVerified;
                }
              int modifiedRecordCount = Save();
              if (modifiedRecordCount <= 0)
              {
                  return false;
              }

              return true;
          }
          catch (Exception)
          {
              throw;
          }
      }

      public void DeleteBy(long PaymentID)
      {
          try
          {
              Expression<Func<REMITA_PAYMENT, bool>> selector = a => a.PAYMENT.Payment_Id == PaymentID;
              Delete(selector);
          }
          catch (Exception)
          {
              throw;
          }
      }

  
  }


}
