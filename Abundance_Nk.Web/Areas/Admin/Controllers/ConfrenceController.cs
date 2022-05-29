using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Abundance_Nk.Model.Entity.Model;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Business;
using Abundance_Nk.Model.Entity;
using System.Linq.Expressions;
using System.Web.Routing;
using System.IO;
using Abundance_Nk.Web.Controllers;
using Abundance_Nk.Web.Models;
using System.Transactions;
using Abundance_Nk.Web.Areas.Applicant.ViewModels;
using System.Configuration;
using System.Data.Entity.Validation;
using MailerApp.Business;
using System.Net;
using Newtonsoft.Json;

namespace Abundance_Nk.Web.Areas.Admin.Controllers
{
    public class ConfrenceController : BaseController
    {
        private const string ID = "Id";
        private const string NAME = "Name";
        private const string VALUE = "Value";
        private const string TEXT = "Text";
        private const string DEFAULT_PASSPORT = "/Content/Images/default_avatar.png";
        private const string FIRST_SITTING = "FIRST SITTING";
        private const string SECOND_SITTING = "SECOND SITTING";
        private string appRoot = ConfigurationManager.AppSettings["AppRoot"];

        public ActionResult ConfrencePayments()
        {
            ConfrencePaymentViewModels viewModel = new ConfrencePaymentViewModels();
            return View(viewModel);

        }
        [HttpPost]
        public ActionResult ConfrencePayments(ConfrencePaymentViewModels viewModel)
        {
            if (viewModel != null)
            {

                PaystackLogic paystackLogic = new PaystackLogic();

                List<Paystack> paystacks = new List<Paystack>();
                paystacks = paystackLogic.GetModelsBy(p => p.transaction_date >= viewModel.DateStart && p.transaction_date <= viewModel.DateEnd).ToList();

                if (paystacks.Count > 0)
                {
                    viewModel.Paystacks = paystacks;
                }
                else
                {
                    SetMessage("Error Occurred! No Payments Seen In Paystack Report ", Message.Category.Error);
                }
                return View(viewModel);
            }

            else
            {

                SetMessage("Error Occurred! ", Message.Category.Error);
                return View();
                 }
           
        }
         
    }
}
