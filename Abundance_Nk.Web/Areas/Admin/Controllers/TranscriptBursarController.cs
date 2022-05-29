using Abundance_Nk.Business;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Web.Areas.Admin.ViewModels;
using Abundance_Nk.Web.Controllers;
using Abundance_Nk.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace Abundance_Nk.Web.Areas.Admin.Controllers
{
    public class TranscriptBursarController : BaseController
    {
        TranscriptBursarViewModel viewModel;
        private MailMessage mail;
        public TranscriptBursarController()
        {
            mail = new MailMessage();
        }

        // GET: Admin/TranscriptBursar
        public ActionResult Index()
        {
            //rewrite this
            Person person;
            viewModel = new TranscriptBursarViewModel();
            TranscriptRequestLogic requestLogic = new TranscriptRequestLogic();
            PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
            RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();

            PersonLogic personLogic = new PersonLogic();
            
            try
            {
                viewModel.transcriptRequests = requestLogic.GetModelsBy(t => t.Date_Requested >= new DateTime(2016, 12, 1) && t.Date_Requested <= new DateTime(2017, 2, 8));

                List<TranscriptRequest> transcriptRequestList = new List<TranscriptRequest>();

                for (int i = 0; i < viewModel.transcriptRequests.Count; i++)
                {
                    TranscriptRequest transcriptRequest = viewModel.transcriptRequests[i];

                    person = personLogic.GetModelBy(p => p.Person_Id == transcriptRequest.student.Id);
                    transcriptRequest.student.FullName = person.FullName;
                    if (transcriptRequest.payment != null)
                    {
                        RemitaPayment remitaPayment = remitaPaymentLogic.GetModelBy(p => p.Payment_Id == transcriptRequest.payment.Id);
                        if (remitaPayment != null)
                        {
                            transcriptRequest.ConfirmationOrderNumber = remitaPayment.RRR;
                            transcriptRequest.remitaPayment = remitaPayment;
                            if (remitaPayment.Status.Contains("01:"))
                            {
                                transcriptRequest.Amount = remitaPayment.TransactionAmount.ToString();
                            }
                        }
                        else
                        {

                            transcriptRequest.Amount = "Payment not yet made";
                        }


                    }
                    transcriptRequestList.Add(transcriptRequest);
                }

                //foreach (TranscriptRequest transcriptRequest in viewModel.transcriptRequests)
                //{
                //    person = personLogic.GetModelBy(p => p.Person_Id == transcriptRequest.student.Id);
                //    transcriptRequest.student.FullName = person.FullName;
                //    if (transcriptRequest.payment != null)
                //    {
                //        RemitaPayment remitaPayment = remitaPaymentLogic.GetModelBy(p => p.Payment_Id == transcriptRequest.payment.Id);
                //        if (remitaPayment != null)
                //        {
                //            transcriptRequest.ConfirmationOrderNumber = remitaPayment.RRR;
                //            transcriptRequest.remitaPayment = remitaPayment;
                //            if (remitaPayment.Status.Contains("01:"))
                //            {
                //                transcriptRequest.Amount = remitaPayment.TransactionAmount.ToString();
                //            }
                //        }
                //        else
                //        {

                //            transcriptRequest.Amount = "Payment not yet made";
                //        }


                //    }
                //    transcriptRequestList.Add(transcriptRequest);
                //}


                viewModel.transcriptRequests = transcriptRequestList.OrderBy(t => t.RequestType).ThenBy(t => t.DestinationCountry).ThenBy(t => t.DateRequested).ToList();
                return View(viewModel);
            }
            catch (Exception)
            {

                return View(viewModel);
            }

        }

        [HttpGet]
        public ActionResult UpdateStatus(long tid, string confirmationOrder)
            {
                

                viewModel = new TranscriptBursarViewModel();
                try
                {
                    PaymentEtranzact paymentEtranzact = new PaymentEtranzact();
                    PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
                    paymentEtranzact = paymentEtranzactLogic.GetModelBy(p => p.Confirmation_No == confirmationOrder);
                    Person person;
                    PersonLogic personLogic = new PersonLogic();
                    TranscriptRequestLogic transcriptRequestLogic = new TranscriptRequestLogic();
                    TranscriptRequest tRequest = transcriptRequestLogic.GetModelBy(t => t.Transcript_Request_Id == tid);              
                    if (paymentEtranzact == null)
                    {
                        SetMessage("Payment cannot be confirmed at the moment" , Message.Category.Error);
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        tRequest.transcriptStatus = new TranscriptStatus { TranscriptStatusId = 4 };
                        transcriptRequestLogic.Modify(tRequest);
                        person = personLogic.GetModelBy(p => p.Person_Id == tRequest.student.Id);
                        string studentMail = person.Email;
                        //MailSender("ugochukwuaronu@gmail.com", "lawsgacc@gmail.com");
                    }
                
                }
                catch (Exception ex)
                {
                    SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
                }
                SetMessage("Payment confirmed", Message.Category.Information);
                return RedirectToAction("Index");
            }
        public void MailSender(string StudentMail, string BursarMail)
        {
            try
            {
                 const string Subject = "Transcript Payment Confirmation";
                 const string Body = "This is to inform you that your transcript payment has been confirmed and your request processed";
                 if (ModelState.IsValid)
                {
                    mail.To.Add(StudentMail);
                    mail.From = new MailAddress(BursarMail);
                    mail.Subject = Subject;
                    mail.Body = Body;
                    mail.IsBodyHtml = true;
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 25;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential("lawsgacc@gmail.com", "gaccount");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                    TempData["msg"] = "Mail Sent";
                }
                else
                {
                    TempData["msg"] = "Mail Not Sent";
                }

            }
            catch (Exception e)
            {
                TempData["msg"] = "Mail Not Sent" + "\n" + e.Message;
            }
        }

        public ActionResult GetStatus(string order_Id)
        {
            try
            {
                RemitaSettings settings = new RemitaSettings();
                RemitaSettingsLogic settingsLogic = new RemitaSettingsLogic();
                settings = settingsLogic.GetModelBy(s => s.Payment_SettingId == 1);
                RemitaResponse remitaResponse = new RemitaResponse();
                RemitaPayment remitaPayment = new RemitaPayment();
                RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                remitaPayment = remitaPaymentLogic.GetModelBy(m => m.OrderId == order_Id);
                string remitaVerifyUrl = ConfigurationManager.AppSettings["RemitaVerifyUrl"].ToString();
                RemitaPayementProcessor remitaProcessor = new RemitaPayementProcessor(settings.Api_key);
                remitaResponse = remitaProcessor.TransactionStatus(remitaVerifyUrl, remitaPayment);
                if (remitaResponse != null && remitaResponse.Status != null)
                {
                    remitaPayment.Status = remitaResponse.Status + ":" + remitaResponse.StatusCode;
                    remitaPaymentLogic.Modify(remitaPayment);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("remote"))
                {
                    TempData["msg"] = "Could not get the status from the server, kindly try again. " + ex.Message;
                }
                else
                {
                    //throw ex;
                    TempData["msg"] = "Could not get the status from the server, kindly try again. " + ex.Message;
                }
            }

            return RedirectToAction("Index");
            
        }

        public ActionResult UpdateRRRBulk()
            {
                try
                {
                    BackgroundWorker m = new BackgroundWorker();
                    m.DoWork += m_DoWork;
                    var task1 = Task.Run(() => m.RunWorkerAsync());
                    RedirectToAction("Index");
                }
                catch (Exception)
                {
                
                    throw;
                }
                return View();
            }

        void m_DoWork(object sender,DoWorkEventArgs e)
    {
                RemitaSettings settings = new RemitaSettings();
                RemitaSettingsLogic settingsLogic = new RemitaSettingsLogic();
                settings = settingsLogic.GetModelBy(s => s.Payment_SettingId == 1);
                RemitaResponse remitaResponse = new RemitaResponse();
                RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                string remitaVerifyUrl = ConfigurationManager.AppSettings["RemitaVerifyUrl"].ToString();
                RemitaPayementProcessor remitaProcessor = new RemitaPayementProcessor(settings.Api_key);
                List<RemitaPayment> remitaPayments = remitaPaymentLogic.GetModelsBy(m => m.Status.Contains("025"));
                foreach (RemitaPayment remitaPayment in remitaPayments)
                {
                    remitaResponse = remitaProcessor.TransactionStatus(remitaVerifyUrl, remitaPayment);
                    if (remitaResponse != null && remitaResponse.Status != null)
                    {
                        remitaPayment.Status = remitaResponse.Status + ":" + remitaResponse.StatusCode;
                        remitaPaymentLogic.Modify(remitaPayment);
                    }
                }
               
    }

        public ActionResult TranscriptReport()
        {
            try
            {
                viewModel = new TranscriptBursarViewModel();
                RemitaPaymentLogic remitaPyamentLogic = new RemitaPaymentLogic();
                viewModel.remitaPayments = remitaPyamentLogic.GetModelsBy(a => a.Status.Contains("01") && a.Description == "TRANSCRIPT");

            }
            catch (Exception)
            {

                throw;
            }
            return View(viewModel);
        }
        public ActionResult CertificateReport()
        {
            try
            {
                viewModel = new TranscriptBursarViewModel();
                RemitaPaymentLogic remitaPyamentLogic = new RemitaPaymentLogic();
                viewModel.remitaPayments = remitaPyamentLogic.GetModelsBy(a => a.Status.Contains("01") && a.Description == "CERTIFICATE COLLECTION");

            }
            catch (Exception)
            {

                throw;
            }
            return View(viewModel);
        }


        public ActionResult VerifyTranscriptRequestPayment()
        {
            TranscriptRequestViewModel viewModel = new TranscriptRequestViewModel();
            try
            {

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View(viewModel);
        }
        [HttpPost]
        public ActionResult VerifyTranscriptRequestPayment(TranscriptRequestViewModel viewModel)
        {
            viewModel.GroupTranscriptByYears = new List<GroupTranscriptByYear>();
            viewModel.GroupTranscriptByMonths = new List<GroupTranscriptByMonth>();
            ViewBag.Transcript = viewModel.TranscriptStatusSelectItem;
            List<TranscriptRequest> transcriptRequestList = new List<TranscriptRequest>();
            TranscriptRequestLogic transcriptRequestLogic = new TranscriptRequestLogic();
            try
            {

                if (viewModel.RequestType != null)
                {
                    if (viewModel.RequestType == "certificateverification")
                    {
                        ViewBag.Title2 = "CERTIFICATE VERIFICATION";
                        TempData["Title"] = "CERTIFICATE VERIFICATION";
                        TempData.Keep("Title");
                        transcriptRequestList = transcriptRequestLogic.GetCertificateVerification();
                    }
                    else if (viewModel.RequestType == "transcriptrequest")
                    {
                        ViewBag.Title2 = "TRANSCRIPT REQUEST";
                        TempData["Title"] = "TRANSCRIPT REQUEST";
                        TempData.Keep("Title");
                        
                        transcriptRequestList = transcriptRequestLogic.GetTranscriptRequests();
                        //transcriptRequestList = transcriptRequestLogic.GetVerifiedTranscriptRequestPayment();
                    }
                    else if (viewModel.RequestType == "transcriptverification")
                    {
                        ViewBag.Title2 = "TRANSCRIPT VERIFICATION";
                        TempData["Title"] = "TRANSCRIPT VERIFICATION";
                        TempData.Keep("Title");
                        transcriptRequestList = transcriptRequestLogic.GetTranscriptVerification();
                    }
                    else if (viewModel.RequestType == "wesverification")
                    {

                    }
                }

                if (transcriptRequestList.Count > 0)
                {
                    var groupedByYearRequests = transcriptRequestList.GroupBy(x => x.DateRequested.Year).OrderBy(g => g.Key).ToList();
                    if (groupedByYearRequests.Count > 0)
                    {

                        foreach (var groupedByYearRequest in groupedByYearRequests)
                        {
                            int count = 0;
                            int intYear = 0;
                            var year = groupedByYearRequest.Key;
                            intYear = Convert.ToInt32(year);
                            foreach (var transcriptRequest in transcriptRequestList)
                            {
                                int requestYear = Convert.ToInt32(transcriptRequest.DateRequested.Year);
                                if (intYear == requestYear)
                                {
                                    count += 1;
                                }
                            }
                            GroupTranscriptByYear groupTranscript = new GroupTranscriptByYear();
                            groupTranscript.Year = intYear;
                            groupTranscript.TranscriptCount = count;
                            viewModel.GroupTranscriptByYears.Add(groupTranscript);
                        }

                    }
                }
                //viewModel.GroupTranscriptByYears.OrderBy(f =>f.Year);
                viewModel.TranscriptRequests = new List<TranscriptRequest>();
                ViewBag.Department = new SelectList(viewModel.DepartmentSelectListItem, Utility.VALUE, Utility.TEXT);
                TempData["PaidTranscript"] = transcriptRequestList;
                TempData.Keep("PaidTranscript");
            }
            catch (Exception ex)
            {

                throw ex;
            }
            viewModel.Active = true;
            return View(viewModel);
        }

        public ActionResult TranscriptCountByMonth(int year)
        {
            ViewBag.Title2 = TempData["Title"] as string;
            TempData.Keep("Title");
            TranscriptRequestViewModel viewModel = new TranscriptRequestViewModel();
            viewModel.GroupTranscriptByMonths = new List<GroupTranscriptByMonth>();
            viewModel.GroupTranscriptByYears = new List<GroupTranscriptByYear>();
            viewModel.TranscriptRequests = new List<TranscriptRequest>();
            List<TranscriptRequest> ByselectedYear = new List<TranscriptRequest>();
            ViewBag.Transcript = viewModel.TranscriptStatusSelectItem;
            try
            {
                TranscriptRequestLogic transcriptRequestLogic = new TranscriptRequestLogic();
                var allPaidTranscriptRequests = (List<TranscriptRequest>)TempData["PaidTranscript"];
                TempData.Keep("PaidTranscript");
                //var allUnprocessedTranscripts = transcriptRequestLogic.GetModelsBy(tr => tr.Transcript_Status_Id == 3 && tr.Date_Requested.Year == year);
                if (allPaidTranscriptRequests.Count > 0)
                {
                    for (int i = 0; i < allPaidTranscriptRequests.Count; i++)
                    {
                        var seletedYear = Convert.ToInt32(allPaidTranscriptRequests[i].DateRequested.Year);
                        if (seletedYear == year)
                        {
                            ByselectedYear.Add(allPaidTranscriptRequests[i]);
                        }
                    }
                    var groupedByMonthRequests = ByselectedYear.GroupBy(x => x.DateRequested.Month).OrderBy(g => g.Key).ToList();
                    if (groupedByMonthRequests.Count > 0)
                    {
                        foreach (var groupedByMonthRequest in groupedByMonthRequests)
                        {
                            int count = 0;
                            var stringMonth = MonthName(groupedByMonthRequest.Key);

                            foreach (var allPaidTranscriptRequest in ByselectedYear)
                            {
                                int requestMonth = Convert.ToInt32(allPaidTranscriptRequest.DateRequested.Month);
                                if (groupedByMonthRequest.Key == requestMonth)
                                {
                                    count += 1;
                                }
                            }
                            GroupTranscriptByMonth groupTranscriptByMonth = new GroupTranscriptByMonth();
                            groupTranscriptByMonth.Month = stringMonth;
                            groupTranscriptByMonth.TranscriptCount = count;
                            groupTranscriptByMonth.Year = year;
                            groupTranscriptByMonth.intMonth = groupedByMonthRequest.Key;
                            viewModel.GroupTranscriptByMonths.Add(groupTranscriptByMonth);
                        }

                    }

                }
                viewModel.Active = true;
                return View("VerifyTranscriptRequestPayment", viewModel);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public ActionResult TranscriptRequestByMonth(int month, int year)
        {
            ViewBag.Title2 = TempData["Title"] as string;
            TempData.Keep("Title");
            TranscriptRequestViewModel viewModel = new TranscriptRequestViewModel();
            viewModel.GroupTranscriptByMonths = new List<GroupTranscriptByMonth>();
            viewModel.GroupTranscriptByYears = new List<GroupTranscriptByYear>();
            viewModel.TranscriptRequests = new List<TranscriptRequest>();
            ViewBag.Transcript = viewModel.TranscriptStatusSelectItem;
            try
            {
                //TranscriptRequestLogic transcriptRequestLogic = new TranscriptRequestLogic();
                var allUnprocessedTranscripts = (List<TranscriptRequest>)TempData["PaidTranscript"];
                TempData.Keep("PaidTranscript");
                var allYearandMonth = allUnprocessedTranscripts.Where(x => x.DateRequested.Year == year && x.DateRequested.Month == month).ToList();
                //var allUnprocessedTranscripts = transcriptRequestLogic.GetModelsBy(tr => tr.Transcript_Status_Id == 3 && tr.Date_Requested.Year == year && tr.Date_Requested.Month == month);
                if (allUnprocessedTranscripts.Count > 0)
                {
                    //viewModel.TranscriptRequests = allUnprocessedTranscripts;
                    viewModel.TranscriptRequests = allYearandMonth;
                }
                viewModel.Active = true;
                return View("VerifyTranscriptRequestPayment", viewModel);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public ActionResult DispatchTranscript(long trId)
        {
            try
            {
                if (trId > 0)
                {
                    TranscriptRequestLogic transcriptRequestLogic = new TranscriptRequestLogic();
                    var request = transcriptRequestLogic.GetModelBy(x => x.Transcript_Request_Id == trId);
                    if (request != null)
                    {
                        request.transcriptStatus = new TranscriptStatus { TranscriptStatusId = 5 };
                        transcriptRequestLogic.Modify(request);
                    }

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return RedirectToAction("VerifyTranscriptRequestPayment");
        }

        public List<TranscriptRequest> GetPaidTrancriptRequest(List<TranscriptRequest> transcriptRequests)
        {
            List<TranscriptRequest> transcriptRequestList = new List<TranscriptRequest>();
            try
            {
                RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                //Get all paid transcript request
                if (transcriptRequests != null && transcriptRequests.Count > 0)
                {
                    for (int i = 0; i < transcriptRequests.Count; i++)
                    {
                        var paymentId = transcriptRequests[i].payment.Id;
                        var remitapayment = remitaPaymentLogic.GetModelsBy(x => x.Payment_Id == paymentId && ((x.Status.Contains("01") || (x.Description.Contains("manual"))))).FirstOrDefault();
                        if (remitapayment != null)
                        {
                            transcriptRequestList.Add(transcriptRequests[i]);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return transcriptRequestList;
        }
        public string MonthName(int month)
        {
            string myMonth = null;
            try
            {

                switch (month)
                {
                    case 1:
                        myMonth = "JANUARY";
                        break;
                    case 2:
                        myMonth = "FEBRUARY";
                        break;
                    case 3:
                        myMonth = "MARCH";
                        break;
                    case 4:
                        myMonth = "APRIL";
                        break;
                    case 5:
                        myMonth = "MAY";
                        break;
                    case 6:
                        myMonth = "JUNE";
                        break;
                    case 7:
                        myMonth = "JULY";
                        break;
                    case 8:
                        myMonth = "AUGUST";
                        break;
                    case 9:
                        myMonth = "SEPTEMBER";
                        break;
                    case 10:
                        myMonth = "OCTOBER";
                        break;
                    case 11:
                        myMonth = "NOVEMBER";
                        break;
                    case 12:
                        myMonth = "DECEMBER";
                        break;

                }
                return myMonth;
            }

            catch (Exception ex)
            {
                throw ex;
            }

        }

        public ActionResult VerifyPayment(long pmid)
        {
            try
            {
                if (pmid > 0)
                {
                    TranscriptRequestLogic transcriptRequestLogic = new TranscriptRequestLogic();
                    PaymentLogic paymentLogic = new PaymentLogic();
                    PaymentVerificationLogic paymentVerificationLogic = new PaymentVerificationLogic();
                    RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                    UserLogic userLogic = new UserLogic();
                    User user = userLogic.GetModelsBy(x => x.User_Name == User.Identity.Name).FirstOrDefault();
                    var request=transcriptRequestLogic.GetModelsBy(f => f.Payment_Id == pmid).LastOrDefault();
                    string client = Request.LogonUserIdentity.Name + " (" + HttpContext.Request.UserHostAddress + ")";
                    if (request != null)
                    {
                        var remitaPayment=remitaPaymentLogic.GetModelsBy(f => f.Payment_Id == pmid).LastOrDefault();
                        if(remitaPayment!=null && remitaPayment.Status.Contains("01"))
                        {
                            using (TransactionScope transactionScope = new TransactionScope())
                            {
                                request.transcriptStatus = new TranscriptStatus { TranscriptStatusId = 4 };
                                transcriptRequestLogic.Modify(request);
                                PaymentVerification paymentVerification = new PaymentVerification();
                                paymentVerification.User = user;
                                paymentVerification.Payment = request.payment;
                                paymentVerification.DateVerified = DateTime.Now;
                                paymentVerification.Comment = client;
                                paymentVerificationLogic.Create(paymentVerification);

                                transactionScope.Complete();

                            }
                            SetMessage("Verification was successful", Message.Category.Information);

                        }
                        else
                        {
                            SetMessage("Verification was not successful", Message.Category.Warning);
                        }
                    }
                }
                


            }
            catch(Exception ex)
            {
                throw ex;
            }
            return RedirectToAction("VerifyTranscriptRequestPayment");
        }



    }
}