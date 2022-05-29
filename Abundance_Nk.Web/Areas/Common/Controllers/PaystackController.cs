using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Abundance_Nk.Business;
using Abundance_Nk.Model.Model;

namespace Abundance_Nk.Web.Areas.Common.Controllers
{
    [AllowAnonymous]
    public class PaystackController : Controller
    {
        // GET: Common/Paystack
        public ActionResult Index(Paystack paystack)
        {
            string PaystackSecrect = ConfigurationManager.AppSettings["PaystackSecrect"].ToString();
            PaystackLogic paystackLogic = new PaystackLogic();
            paystackLogic.VerifyPayment(new Payment() { InvoiceNumber = paystack.reference }, PaystackSecrect);
            return RedirectToAction("ConfrenceInvoice", "Confrence", new { Area = "Applicant",  Inv = paystack.reference });
        }
    }
}