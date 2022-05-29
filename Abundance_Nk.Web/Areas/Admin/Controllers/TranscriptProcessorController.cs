using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Business;
using Abundance_Nk.Web.Areas.Admin.ViewModels;
using Abundance_Nk.Web.Models;
using Abundance_Nk.Web.Controllers;
using System.Transactions;
using Abundance_Nk.Model.Entity.Model;

namespace Abundance_Nk.Web.Areas.Admin.Controllers
{
    public class TranscriptProcessorController : BaseController
    {
        TranscriptProcessorViewModel viewModel;
        private const string EmailTemplateVerificationPath = "/EmailTemplate/VerificationTemplate.cshtml";
        // GET: Admin/TranscriptProcessor
        public ActionResult Index()
        {
            Person person;
            viewModel = new TranscriptProcessorViewModel();
            TranscriptRequestLogic requestLogic = new TranscriptRequestLogic();
            PersonLogic personLogic = new PersonLogic();
            viewModel.transcriptRequests = requestLogic.GetAll();
            PopulateDropDown(viewModel);

            try
            {
                for (int i = 0; i < viewModel.transcriptRequests.Count(); i++)
                {
                    person = personLogic.GetAll().Where(p => p.Id == viewModel.transcriptRequests[i].student.Id).FirstOrDefault();
                    viewModel.transcriptRequests[i].student.FirstName = person.FirstName;
                    viewModel.transcriptRequests[i].student.LastName = person.LastName;
                    viewModel.transcriptRequests[i].student.OtherName = person.OtherName;
                    viewModel.transcriptRequests[i].student.FullName = person.FullName;
                }
                return View(viewModel);
            }
            catch (Exception)
            {

                return View(viewModel);
            }
        }
        public ActionResult ViewTranscriptRequests()
        {
            viewModel = new TranscriptProcessorViewModel();

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult ViewTranscriptRequests(TranscriptProcessorViewModel viewModel)
        {
            try
            {
                DateTime DateFrom = new DateTime();
                DateTime DateTo = new DateTime();
                if (DateTime.TryParse(viewModel.DateFrom, out DateFrom))
                {
                    //do nothing
                }
                else
                {
                    DateFrom = DateTime.Now;
                }
                if (DateTime.TryParse(viewModel.DateTo, out DateTo))
                {
                    //do nothing
                }
                else
                {
                    DateTo = DateTime.Now;
                }
               
                TranscriptRequestLogic requestLogic = new TranscriptRequestLogic();
                List<TranscriptRequest> transcriptRequests = new List<TranscriptRequest>();

                //transcriptRequests = requestLogic.GetModelsBy(t => t.Date_Requested <= DateTo && t.Date_Requested >= DateFrom && t.Transcript_Status_Id > 1);
                //transcriptRequests = requestLogic.GetTranscriptRequests(DateFrom, DateTo);
                transcriptRequests = requestLogic.GetTranscriptRequests();

                viewModel.transcriptRequests = new List<TranscriptRequest>();

                for (int i = 0; i < transcriptRequests.Count; i++)
                {
                    transcriptRequests[i].Name = transcriptRequests[i].Name != null ? transcriptRequests[i].Name.ToUpper() : transcriptRequests[i].Name;
                    transcriptRequests[i].MatricNumber = transcriptRequests[i].MatricNumber != null ? transcriptRequests[i].MatricNumber.ToUpper() : transcriptRequests[i].MatricNumber;
                    transcriptRequests[i].DestinationAddress = transcriptRequests[i].DestinationAddress != null ? transcriptRequests[i].DestinationAddress.ToUpper() : transcriptRequests[i].DestinationAddress;
                    transcriptRequests[i].Status = transcriptRequests[i].Status != null ? transcriptRequests[i].Status.ToUpper() : "";

                    viewModel.transcriptRequests.Add(transcriptRequests[i]);
                }

                viewModel.transcriptRequests = viewModel.transcriptRequests.OrderBy(t => t.DateRequested).ToList();
                //PopulateDropDown(viewModel);
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        private void PopulateDropDown(TranscriptProcessorViewModel viewModel)
        {
            int i = 0;
            foreach (TranscriptRequest t in viewModel.transcriptRequests)
            {
                ViewData["status" + i] = new SelectList(viewModel.transcriptSelectList, Utility.VALUE, Utility.TEXT, t.transcriptStatus.TranscriptStatusId);
                i++;
            }
        }
    
        public ActionResult UpdateStatus(long tid, long stat)
        {
            viewModel = new TranscriptProcessorViewModel();
            try
            {
                TranscriptRequestLogic transcriptRequestLogic = new TranscriptRequestLogic();
                TranscriptRequest tRequest = transcriptRequestLogic.GetModelBy(t => t.Transcript_Request_Id == tid);
                tRequest.transcriptStatus = new TranscriptStatus { TranscriptStatusId = (int)stat };
                transcriptRequestLogic.Modify(tRequest);
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
            return RedirectToAction("Index");
        }
    
        public ActionResult Clearance ()
        {
            try
            {
                 viewModel = new TranscriptProcessorViewModel();
                 TranscriptRequestLogic requestLogic = new TranscriptRequestLogic();
                 viewModel.transcriptRequests = requestLogic.GetAll();
                 PopulateDropDown(viewModel);
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
            return View(viewModel);

        }
        public ActionResult UpdateClearance(long tid, long stat)
        {
            viewModel = new TranscriptProcessorViewModel();
            try
            {
                TranscriptRequestLogic transcriptRequestLogic = new TranscriptRequestLogic();
                TranscriptRequest tRequest = transcriptRequestLogic.GetModelBy(t => t.Transcript_Request_Id == tid);
                tRequest.transcriptClearanceStatus = new TranscriptClearanceStatus { TranscriptClearanceStatusId = (int)stat };
                transcriptRequestLogic.Modify(tRequest);
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
            return RedirectToAction("Clearance");
        }
    
        public ActionResult ViewTranscriptDetails()
        {
            viewModel = new TranscriptProcessorViewModel();

            return View(viewModel); 
        }
        [HttpPost]
        public ActionResult ViewTranscriptDetails(TranscriptProcessorViewModel viewModel)
        {
            try
            {
                if (viewModel.transcriptRequest.student.MatricNumber != null)
                {
                    PersonLogic personLogic = new PersonLogic();
                    StudentLogic studentLogic = new StudentLogic();
                    TranscriptRequestLogic transcriptRequestLogic = new TranscriptRequestLogic();

                    Model.Model.Student student = studentLogic.GetModelBy(s => s.Matric_Number == viewModel.transcriptRequest.student.MatricNumber);
                    if (student != null)
                    {
                        Person person = personLogic.GetModelBy(p => p.Person_Id == student.Id);
                        List<TranscriptRequest> transcriptRequests = transcriptRequestLogic.GetModelsBy(tr => tr.Student_id == student.Id);
                        if (transcriptRequests == null)
                        {
                            SetMessage("The student has not made a transcript request", Message.Category.Error);
                        }
                        else
                        {
                            viewModel.RequestDateString = transcriptRequests.LastOrDefault().DateRequested.ToShortDateString();
                            viewModel.transcriptRequests = transcriptRequests;
                            viewModel.Person = person;
                        }
                    }
                    else
                    {
                        SetMessage("Matric Number is not valid, or the student has not made a transcript request", Message.Category.Error);
                    }
                }
                else
                {
                    SetMessage("Enter Matric Number!", Message.Category.Error);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }
                        
            return View(viewModel);
        }
        public ActionResult EditTranscriptDetails(long Id)
        {
            try
            {
                viewModel = new TranscriptProcessorViewModel();
                if (Id > 0)
                {
                    PersonLogic personLogic = new PersonLogic();
                    StudentLogic studentLogic = new StudentLogic();
                    TranscriptRequestLogic transcriptRequestLogic = new TranscriptRequestLogic();
                    TranscriptRequest transcriptRequest = transcriptRequestLogic.GetModelBy(a => a.Transcript_Request_Id == Id);
                    Model.Model.Student student = transcriptRequest.student;
                    if (student != null)
                    {
                        Person person = personLogic.GetModelBy(p => p.Person_Id == student.Id);
                        viewModel.RequestDateString = transcriptRequest.DateRequested.ToShortDateString();
                        viewModel.transcriptRequest = transcriptRequest;
                        viewModel.Person = person;
                       
                    }
                    else
                    {
                        SetMessage("Matric Number is not valid, or the student has not made a transcript request", Message.Category.Error);
                    }
                }
                else
                {
                    SetMessage("Enter Matric Number!", Message.Category.Error);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            RetainDropDownList(viewModel);
            return View(viewModel);
        }

        public ActionResult SaveTranscriptDetails(TranscriptProcessorViewModel viewModel)
        {
            try
            {
                if (viewModel.transcriptRequest != null)
                {
                    PersonLogic personLogic = new PersonLogic();
                    StudentLogic studentLogic = new StudentLogic();
                    TranscriptRequestLogic transcriptRequestLogic = new TranscriptRequestLogic();
 
                    Person person = new Person();
                    Model.Model.Student student = new Model.Model.Student();

                    person.Id = viewModel.transcriptRequest.student.Id;
                    person.LastName = viewModel.transcriptRequest.student.LastName;
                    person.FirstName = viewModel.transcriptRequest.student.FirstName;
                    person.OtherName = viewModel.transcriptRequest.student.OtherName;
                    bool isPersonModified = personLogic.Modify(person);

                    student.Id = viewModel.transcriptRequest.student.Id;
                    student.MatricNumber = viewModel.transcriptRequest.student.MatricNumber;
                    bool isStudentModified = studentLogic.Modify(student);

                    if (viewModel.transcriptRequest.DestinationCountry.Id == "OTH")
                    {
                        viewModel.transcriptRequest.DestinationState.Id = "OT"; 
                    }
                    bool isTranscriptRequestModified = transcriptRequestLogic.Modify(viewModel.transcriptRequest);

                    if (isTranscriptRequestModified && isStudentModified)
                    {
                        SetMessage("Operation Successful!", Message.Category.Information);
                        return RedirectToAction("ViewTranscriptDetails");
                    }
                    if (isTranscriptRequestModified && !isStudentModified)
                    {
                        SetMessage("Not all fields were modified!", Message.Category.Information);
                        return RedirectToAction("ViewTranscriptDetails");
                    }
                    if (!isTranscriptRequestModified && isStudentModified)
                    {
                        SetMessage("Not all fields were modified!", Message.Category.Information);
                        return RedirectToAction("ViewTranscriptDetails");
                    }
                    if (!isTranscriptRequestModified && !isStudentModified)
                    {
                        SetMessage("No item modified!", Message.Category.Information);
                        return RedirectToAction("ViewTranscriptDetails");
                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return RedirectToAction("EditTranscriptDetails");
        }
        public ActionResult DeleteTranscriptDetails(long Id)
        {
            try
            {
                if (Id > 0)
                {
                    TranscriptRequestLogic transcriptRequestLogic = new TranscriptRequestLogic();
                    OnlinePaymentLogic onlinePaymentLogic = new OnlinePaymentLogic();
                    PaymentLogic paymentLogic = new PaymentLogic();

                    TranscriptRequest transcriptRequest = transcriptRequestLogic.GetModelBy(tr => tr.Transcript_Request_Id == Id);
                    TranscriptRequest transcriptRequestAlt = transcriptRequest;
                    if (transcriptRequest != null)
                    {
                        using (TransactionScope scope = new TransactionScope())
                        {
                            transcriptRequestLogic.Delete(tr => tr.Transcript_Request_Id == transcriptRequest.Id);

                            //if (transcriptRequest.payment != null)
                            //{
                            //    OnlinePayment onlinePayment = onlinePaymentLogic.GetModelBy(op => op.Payment_Id == transcriptRequestAlt.payment.Id);
                            //    if (onlinePayment != null)
                            //    {
                            //        onlinePaymentLogic.Delete(op => op.Payment_Id == transcriptRequestAlt.payment.Id);
                            //    }

                            //    paymentLogic.Delete(p => p.Payment_Id == transcriptRequestAlt.payment.Id); 
                            //}
                            

                            SetMessage("Operation Successful!", Message.Category.Information);
                            scope.Complete();
                        }
                        
                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return RedirectToAction("ViewTranscriptDetails");
        }
        private void RetainDropDownList(TranscriptProcessorViewModel viewModel)
        {
            try
            {
                if (viewModel.transcriptRequest != null)
                {
                    if (viewModel.transcriptRequest.DestinationCountry != null)
                    {
                        ViewBag.Country = new SelectList(viewModel.CountrySelectList, "Value", "Text", viewModel.transcriptRequest.DestinationCountry.Id);
                    }
                    else
                    {
                        ViewBag.Country = viewModel.CountrySelectList;
                    }

                    if (viewModel.transcriptRequest.DestinationCountry != null)
                    {
                        ViewBag.State = new SelectList(viewModel.StateSelectList, "Value", "Text", viewModel.transcriptRequest.DestinationState.Id);
                    }
                    else
                    {
                        ViewBag.State = viewModel.StateSelectList;
                    }

                    if (viewModel.transcriptRequest.transcriptClearanceStatus != null)
                    {
                        ViewBag.TranscriptClearanceStatus = new SelectList(viewModel.transcriptClearanceSelectList, "Value", "Text", viewModel.transcriptRequest.transcriptClearanceStatus.TranscriptClearanceStatusId);
                    }
                    else
                    {
                        ViewBag.TranscriptClearanceStatus = viewModel.transcriptClearanceSelectList;
                    }

                    if (viewModel.transcriptRequest.transcriptStatus != null)
                    {
                        ViewBag.TranscriptStatus = new SelectList(viewModel.transcriptSelectList, "Value", "Text", viewModel.transcriptRequest.transcriptStatus.TranscriptStatusId);
                    }
                    else
                    {
                        ViewBag.TranscriptStatus = viewModel.transcriptSelectList;
                    } 
                }
                else
                {
                    ViewBag.Country = viewModel.CountrySelectList;
                    ViewBag.State = viewModel.StateSelectList;
                    ViewBag.TranscriptClearanceStatus = viewModel.transcriptClearanceSelectList;
                    ViewBag.TranscriptStatus = viewModel.transcriptSelectList;
                }
                
            }
            catch (Exception)
            {
                throw;
            }
        }
        public ActionResult ViewTranscriptVerificationRequests()
        {
            viewModel = new TranscriptProcessorViewModel();
            return View(viewModel);
        }
        [HttpPost]
        public ActionResult ViewTranscriptVerificationRequests(TranscriptProcessorViewModel viewModel)
        {
            try
            {
                DateTime DateFrom = new DateTime();
                DateTime DateTo = new DateTime();
                if (DateTime.TryParse(viewModel.DateFrom, out DateFrom))
                {
                    //do nothing
                }
                else
                {
                    DateFrom = DateTime.Now;
                }
                if (DateTime.TryParse(viewModel.DateTo, out DateTo))
                {
                    //do nothing
                }
                else
                {
                    DateTo = DateTime.Now;
                }

                TranscriptRequestLogic requestLogic = new TranscriptRequestLogic();
                TranscriptStatusLogic transcriptStatusLogic = new TranscriptStatusLogic();
                List<TranscriptRequest> transcriptRequests = new List<TranscriptRequest>();

                //transcriptRequests = requestLogic.GetModelsBy(t => t.Date_Requested <= DateTo && t.Date_Requested >= DateFrom && t.Transcript_Status_Id > 1);
                transcriptRequests = requestLogic.GetTranscriptRequestsByVerification(DateFrom, DateTo);

                viewModel.transcriptRequests = new List<TranscriptRequest>();

                for (int i = 0; i < transcriptRequests.Count; i++)
                {
                    transcriptRequests[i].Name = transcriptRequests[i].Name != null ? transcriptRequests[i].Name.ToUpper() : transcriptRequests[i].Name;
                    transcriptRequests[i].MatricNumber = transcriptRequests[i].MatricNumber != null ? transcriptRequests[i].MatricNumber.ToUpper() : transcriptRequests[i].MatricNumber;
                    transcriptRequests[i].DestinationAddress = transcriptRequests[i].DestinationAddress != null ? transcriptRequests[i].DestinationAddress.ToUpper() : transcriptRequests[i].DestinationAddress;
                    transcriptRequests[i].Status = transcriptRequests[i].Status != null ? transcriptRequests[i].Status.ToUpper() : "";
                    var transcriptStatus = transcriptRequests[i].Status;
                    transcriptRequests[i].transcriptStatus = transcriptStatusLogic.GetModelBy(t => t.Transcript_Status_Name.Contains(transcriptStatus));
                    viewModel.transcriptRequests.Add(transcriptRequests[i]);
                }

                viewModel.transcriptRequests = viewModel.transcriptRequests.OrderBy(t => t.DateRequested).ToList();
                PopulateDropDown(viewModel);
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        public ActionResult UpdateVerificationStatus(long tid, long stat)
        {
            viewModel = new TranscriptProcessorViewModel();
            try
            {
                TranscriptRequestLogic transcriptRequestLogic = new TranscriptRequestLogic();
                TranscriptRequest tRequest = transcriptRequestLogic.GetModelBy(t => t.Transcript_Request_Id == tid);
                tRequest.transcriptStatus = new TranscriptStatus { TranscriptStatusId = (int)stat };
               var modified = transcriptRequestLogic.Modify(tRequest);
               if (modified && tRequest.student.Email != null && tRequest.transcriptStatus.TranscriptStatusId >= (int)TranscriptStatusList.RequestProcessed)
                {
                    SendEmail(tRequest.student.Email, viewModel, EmailTemplateVerificationPath);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
            return RedirectToAction("ViewTranscriptVerificationRequests","TranscriptProcessor",new {area ="Admin"});
        }
        public bool SendEmail<T>(string emailAddress, T templatemodel, string templateFilePath) where T : class
        {
            try
            {

                IEmailServiceProvider sendEmail = new EmailServiceProvider();
                // Get SetUp the Template 
                string result = sendEmail.TemplateSetUp(templatemodel, templateFilePath);
                // send mail
                var emailResponse = sendEmail.Send(emailAddress, result);
                if (emailResponse.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
            }
            catch (Exception)
            {

                throw;
            }
            return false;
        }

        public ActionResult ViewTranscriptRequest()
        {
            TranscriptRequestViewModel viewModel = new TranscriptRequestViewModel();
            try
            {

            }
            catch(Exception ex)
            {
                throw ex;
            }
            return View(viewModel);
        }
        [HttpPost]
        public ActionResult ViewTranscriptRequest(TranscriptRequestViewModel viewModel)
        {
            viewModel.GroupTranscriptByYears = new List<GroupTranscriptByYear>();
            viewModel.GroupTranscriptByMonths = new List<GroupTranscriptByMonth>();
            ViewBag.Transcript = viewModel.TranscriptStatusSelectItem;
            List<TranscriptRequest> allTranscriptRequestList = new List<TranscriptRequest>();
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
                        //transcriptRequestList = transcriptRequestLogic.GetCertificateVerification().Where(i=>i.transcriptStatusId==4).ToList();
                        transcriptRequestList = transcriptRequestLogic.GetVerifiedCertificateVerificationPayment();
                    }
                    else if (viewModel.RequestType == "transcriptrequest")
                    {
                        ViewBag.Title2 = "TRANSCRIPT REQUEST";
                        TempData["Title"]= "TRANSCRIPT REQUEST";
                        TempData.Keep("Title"); 
                        //transcriptRequestList = transcriptRequestLogic.GetTranscriptRequests().Where(i => i.transcriptStatusId == 4).ToList();
                        transcriptRequestList = transcriptRequestLogic.GetTranscriptRequests();
                    }
                    else if (viewModel.RequestType == "transcriptverification")
                    {
                        ViewBag.Title2 = "TRANSCRIPT VERIFICATION";
                        TempData["Title"] = "TRANSCRIPT VERIFICATION";
                        TempData.Keep("Title");
                        
                        //transcriptRequestList = transcriptRequestLogic.GetTranscriptVerification().Where(i => i.transcriptStatusId == 4).ToList(); 
                        transcriptRequestList = transcriptRequestLogic.GetVerifiedTranscriptVerificationPayment().ToList();
                    }
                    else if (viewModel.RequestType == "wesverification")
                    {

                    }
                }

                if (transcriptRequestList.Count > 0)
                {
                    var groupedByYearRequests = transcriptRequestList.GroupBy(x => x.DateRequested.Year).OrderBy(g=>g.Key).ToList();
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
            ViewBag.Title2=TempData["Title"] as string;
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
                    var groupedByMonthRequests = ByselectedYear.GroupBy(x => x.DateRequested.Month).OrderBy(g=>g.Key).ToList();
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
                return View("ViewTranscriptRequest", viewModel);
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
                return View("ViewTranscriptRequest", viewModel);
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
            return RedirectToAction("ViewTranscriptRequest");
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
                        var remitapayment = remitaPaymentLogic.GetModelsBy(x => x.Payment_Id == paymentId && ((x.Status.Contains("01")||(x.Description.Contains("manual"))))).FirstOrDefault();
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

        public ActionResult PreviewTranscriptApplication(long pmid)
        {
            TranscriptRequestViewModel viewmodel = new TranscriptRequestViewModel();
            try
            {
               
                StudentLogic studentLogic = new StudentLogic();
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                TranscriptRequestLogic transcriptRequestLogic = new TranscriptRequestLogic();
                RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                if (pmid > 0)
                {

                    var request = transcriptRequestLogic.GetModelBy(g => g.Payment_Id == pmid);
                    if (request != null)
                    {
                        var amountPaid=remitaPaymentLogic.GetModelsBy(t => t.Payment_Id == request.payment.Id).LastOrDefault();
                        request.TransactionAmount = Convert.ToString(amountPaid.TransactionAmount);
                        viewmodel.Student = request.student;
                        viewmodel.StudentLevel= studentLevelLogic.GetModelsBy(f => f.Person_Id == request.student.Id).LastOrDefault();
                        viewmodel.TranscriptRequest = request;

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View(viewmodel);
        }

    }
}
