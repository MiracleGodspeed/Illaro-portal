using System.Collections.Generic;
using System.Web.Mvc;
using Abundance_Nk.Web.Models;
using Abundance_Nk.Model.Model;

namespace Abundance_Nk.Web.Areas.Admin.ViewModels
{
    public class PaymentViewModel
    {
        public PaymentViewModel()
        {
            SessionSelectList = Utility.PopulateAllSessionSelectListItem();
            FeeTypeSelectList = Utility.PopulateFeeTypeSelectListItem();
        }
        public string PaymentGateway { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public Session Session { get; set; }
        public FeeType FeeType { get; set; }
        public List<SelectListItem> SessionSelectList { get; set; }
        public List<SelectListItem> FeeTypeSelectList { get; set; }
        
        public List<PaymentModel> PaymentModels { get; set; }
    }
    public class PaymentJsonResult
    {
        public string Message { get; set; }
        public bool IsError { get; set; }
        public int FeeTypeId { get; set; }
        public string FeeTypeName { get; set; }
        public int TotalCount { get; set; }
        public string TotalAmount { get; set; }
        public string OverallAmount { get; set; }
        public string TransactionDate { get; set; }
        public string MatricNumber { get; set; }
        public string Name { get; set; }
        public string Level { get; set; }
        public string Department { get; set; }
        public string Faculty { get; set; }
        public string Programme { get; set; }
        public string Session { get; set; }
        public string InvoiceNumber { get; set; }
        public string ConfirmationNumber { get; set; }
        public string Amount { get; set; }
    }

    public class PaymentModel
    {
        public string ApplicationNumber { get; set; }
        public string RRR { get; set; }
        public string Name { get; set; }
        public long PersonId { get; set; }
    }
    public class SamplePaymentModel
    {
        public string SN { get; set; }
        public string ApplicationNumber { get; set; }
        public string RRR { get; set; }
        public string Name { get; set; }
    }
}