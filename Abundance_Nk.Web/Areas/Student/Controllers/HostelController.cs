using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using Abundance_Nk.Business;
using Abundance_Nk.Model.Entity.Model;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Web.Areas.Student.ViewModels;
using Abundance_Nk.Web.Controllers;
using Abundance_Nk.Web.Models;
using System.Configuration;

namespace Abundance_Nk.Web.Areas.Student.Controllers
{
    [AllowAnonymous]
    public class HostelController : BaseController
    {
        private HostelViewModel viewModel;
        private SessionLogic _sessionLogic;
        private RemitaPaymentLogic _remitaPaymentLogic;
        public ActionResult CreateHostelRequest()
        {
            viewModel = new HostelViewModel();
            return View(viewModel);
        }
        [HttpPost]
        public ActionResult CreateHostelRequest(HostelViewModel viewModel)
        {
            try
            {
                if (viewModel.Student.MatricNumber != null)
                {
                    HostelRequestLogic hostelRequestLogic = new HostelRequestLogic();
                    _sessionLogic = new SessionLogic();
                    PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
                    StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                    StudentLogic studentLogic = new StudentLogic();
                    PersonLogic personLogic = new PersonLogic();
                    AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
                    HostelBlacklistLogic hostelBlacklistLogic = new HostelBlacklistLogic();

                    Model.Model.Student student = new Model.Model.Student();
                    Person person = new Person();
                    StudentLevel studentLevel = new StudentLevel();
                    Programme programme = new Programme();
                    Department department = new Department();
                    Level level = new Level();

                    List<StudentLevel> studentLevels = new List<StudentLevel>();

                    Session session = _sessionLogic.GetModelsBy(s => s.Active_For_Hostel != null && s.Active_For_Hostel.Value).LastOrDefault() ?? new Session { Id = (int)Sessions._20182019 };

                    List<Model.Model.Student> students = studentLogic.GetModelsBy(s => s.Matric_Number == viewModel.Student.MatricNumber);
                    if (students.Count != 1 && viewModel.Student.MatricNumber.Length < 20)
                    {
                        SetMessage("Student with this Matriculation Number does not exist Or Matric Number is Duplicate!", Message.Category.Error);
                        return View(viewModel);
                    }

                    if (students.Count == 0 && !viewModel.Student.MatricNumber.Contains("/"))
                    {
                        //PaymentEtranzact paymentEtranzact = paymentEtranzactLogic.GetModelsBy(p => p.Confirmation_No == viewModel.Student.MatricNumber && 
                        //(p.ONLINE_PAYMENT.PAYMENT.Fee_Type_Id == (int)FeeTypes.AcceptanceFee || p.ONLINE_PAYMENT.PAYMENT.Fee_Type_Id == (int)FeeTypes.HNDAcceptance) &&
                        //p.ONLINE_PAYMENT.PAYMENT.Session_Id == session.Id).LastOrDefault();
                        //if (paymentEtranzact == null)
                        //{
                        //    SetMessage("Confirmation Order Number is not for Current session's Acceptance Fee!", Message.Category.Error);
                        //    return View(viewModel); 
                        //}

                        _remitaPaymentLogic = new RemitaPaymentLogic();
                        RemitaPayment remitaPayment = _remitaPaymentLogic.GetModelsBy(r => r.RRR == viewModel.Student.MatricNumber &&
                                                    (r.PAYMENT.Fee_Type_Id == (int)FeeTypes.AcceptanceFee || r.PAYMENT.Fee_Type_Id == (int)FeeTypes.HNDAcceptance) &&
                                                    r.PAYMENT.Session_Id == viewModel.Session.Id && r.Status.Contains("01")).LastOrDefault();
                        if (remitaPayment == null)
                        {
                            SetMessage("RRR Number is not for Current session's Acceptance Fee!", Message.Category.Error);
                            return View(viewModel);
                        }

                        person = remitaPayment.payment.Person;
                        AppliedCourse appliedCourse = appliedCourseLogic.GetModelBy(a => a.Person_Id == person.Id);
                        if (appliedCourse == null)
                        {
                            SetMessage("No Applied course record!", Message.Category.Error);
                            return View(viewModel);
                        }

                        programme = appliedCourse.Programme;
                        department = appliedCourse.Department;
                        level = new Level() { Id = 1 };
                        if (programme.Id == 3)
                        {
                            level = new Level() { Id = 3 };
                        }
                    }
                    else
                    {
                        student = students.FirstOrDefault();
                        person = personLogic.GetModelBy(p => p.Person_Id == student.Id);
                        studentLevels = studentLevelLogic.GetModelsBy(sl => sl.STUDENT.Person_Id == student.Id);
                        if (studentLevels.Count == 0)
                        {
                            SetMessage("You have not registered for this session!", Message.Category.Error);
                            return View(viewModel);
                        }

                        int maxLevelId = studentLevels.Max(sl => sl.Level.Id);
                        studentLevel = studentLevels.Where(sl => sl.Level.Id == maxLevelId).LastOrDefault();
                        programme = studentLevel.Programme;
                        department = studentLevel.Department;
                        level = studentLevel.Level;
                    }

                    //check blacklist

                    HostelBlacklist hostelBlacklist = hostelBlacklistLogic.GetModelsBy(h => h.Person_Id == person.Id && h.Session_Id == session.Id).LastOrDefault();

                    if (hostelBlacklist != null)
                    {
                        SetMessage("You cannot request for hostel allocation because of " + hostelBlacklist.Reason, Message.Category.Error);
                        return View(viewModel);
                    }

                    //List<PaymentEtranzact> paymentEtranzacts = paymentEtranzactLogic.GetModelsBy(p => p.ONLINE_PAYMENT.PAYMENT.Person_Id == student.Id && p.ONLINE_PAYMENT.PAYMENT.Session_Id == session.Id && p.ONLINE_PAYMENT.PAYMENT.Fee_Type_Id == (int)FeeTypes.SchoolFees);

                    //if (paymentEtranzacts.Count == 0)
                    //{
                    //    SetMessage("Pay School Fees before making hostel request!", Message.Category.Error);
                    //    return View(viewModel);
                    //}

                    HostelRequest hostelRequest = hostelRequestLogic.GetModelBy(h => h.Person_Id == person.Id && h.Session_Id == session.Id && h.Expired == false);
                    //Check for Sex
                    if (person.Sex == null)
                    {
                        SetMessage("Error! Ensure that your student profile(Sex) is completely filled", Message.Category.Error);
                        return View(viewModel);
                    }
                    if (hostelRequest == null)
                    {
                        if (student != null && student.Id > 0)
                        {
                            hostelRequest = new HostelRequest();
                            hostelRequest.Approved = false;
                            hostelRequest.Department = studentLevel.Department;
                            hostelRequest.Programme = studentLevel.Programme;
                            hostelRequest.RequestDate = DateTime.Now;
                            hostelRequest.Session = session;
                            hostelRequest.Student = student;
                            hostelRequest.Person = person;
                            hostelRequest.Expired = false;
                            SetLevel(student, studentLevel, hostelRequest);

                            hostelRequestLogic.Create(hostelRequest);

                        }
                        else
                        {
                            hostelRequest = new HostelRequest();
                            hostelRequest.Approved = false;
                            hostelRequest.Department = department;
                            hostelRequest.Programme = programme;
                            hostelRequest.RequestDate = DateTime.Now;
                            hostelRequest.Session = session;
                            hostelRequest.Student = student;
                            hostelRequest.Person = person;
                            hostelRequest.Level = level;
                            hostelRequest.Expired = false;
                            hostelRequestLogic.Create(hostelRequest);
                        }

                        SetMessage("Your request has been submitted. Check Back Later For Invoice Generation!", Message.Category.Information);
                        return View(viewModel);
                    }
                    if (hostelRequest != null && hostelRequest.Approved)
                    {
                        SetMessage("Your request has been approved proceed to generate invoice!", Message.Category.Information);
                        return View(viewModel);
                    }
                    if (hostelRequest != null && !hostelRequest.Approved)
                    {
                        SetMessage("Your request has not been approved!", Message.Category.Error);
                        return View(viewModel);
                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
                Bugsnag.AspNet.Client.Current.Notify(ex);
            }

            return View(viewModel);
        }

        private void SetLevel(Model.Model.Student student, StudentLevel studentLevel, HostelRequest hostelRequest)
        {
            try
            {
                Session session = _sessionLogic.GetModelsBy(s => s.Active_For_Hostel != null && s.Active_For_Hostel.Value).LastOrDefault() ?? new Session { Id = (int)Sessions._20182019 };
                string currentSessionSuffix = session.Name.Substring(2, 2);

                currentSessionSuffix = "/" + currentSessionSuffix + "/";

                if (!student.MatricNumber.Contains(currentSessionSuffix))
                {
                    if (studentLevel.Programme.Id == 1 || studentLevel.Programme.Id == 2)
                    {
                        hostelRequest.Level = new Level() { Id = 2 };
                    }
                    else if (studentLevel.Programme.Id == 3)
                    {
                        hostelRequest.Level = new Level() { Id = 4 };
                    }
                }
                else
                {
                    if (studentLevel.Programme.Id == 1 || studentLevel.Programme.Id == 2)
                    {
                        hostelRequest.Level = new Level() { Id = 1 };
                    }
                    else if (studentLevel.Programme.Id == 3)
                    {
                        hostelRequest.Level = new Level() { Id = 3 };
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        //public ActionResult ModifyRequest()
        //{
        //    try
        //    {
        //        HostelRequestLogic requestLogic = new HostelRequestLogic();
        //        StudentLevelLogic studentLevelLogic = new StudentLevelLogic();

        //        List<HostelRequest> requests = requestLogic.GetModelsBy(h => h.Session_Id == (int)Sessions._20172018 && h.STUDENT.Matric_Number.Contains("/16/"));
        //        for (int i = 0; i < requests.Count; i++)
        //        {
        //            HostelRequest request = requests[i];
        //            if (request.Student != null && request.Student.MatricNumber.Contains("/16/"))
        //            {
        //                StudentLevel studentLevel = studentLevelLogic.GetModelsBy(s => s.Person_Id == request.Student.Id).LastOrDefault();
        //                if (studentLevel.Programme.Id == 1 || studentLevel.Programme.Id == 2)
        //                {
        //                    request.Level = new Level() { Id = 2 };
        //                }
        //                else if (studentLevel.Programme.Id == 3)
        //                {
        //                    request.Level = new Level() { Id = 4 };
        //                }

        //                requestLogic.Modify(request);
        //            }
        //        }

        //        SetMessage("Successful!", Message.Category.Information);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }

        //    return RedirectToAction("CreateHostelRequest");
        //}

        public ActionResult GenerateHostelInvoice()
        {
            viewModel = new HostelViewModel();
            try
            {
                SetFeeTypeDropDown(viewModel);
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }

        private void SetFeeTypeDropDown(HostelViewModel viewModel)
        {
            try
            {
                FeeTypeLogic feeTypeLogic = new FeeTypeLogic();
                if (viewModel.FeeTypeSelectListItem != null && viewModel.FeeTypeSelectListItem.Count > 0)
                {
                    viewModel.FeeType = feeTypeLogic.GetModelBy(ft => ft.Fee_Type_Id == (int)FeeTypes.HostelFee);
                    ViewBag.FeeTypes = new SelectList(viewModel.FeeTypeSelectListItem, Utility.VALUE, Utility.TEXT, viewModel.FeeType.Id);
                }
                else
                {
                    ViewBag.FeeTypes = new SelectList(new List<FeeType>(), Utility.ID, Utility.NAME);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
        }

        [HttpPost]
        public ActionResult GenerateHostelInvoice(HostelViewModel viewModel)
        {
            try
            {
                StudentLogic studentLogic = new StudentLogic();
                HostelAllocationLogic hostelAllocationLogic = new HostelAllocationLogic();
                HostelAllocationCriteriaLogic hostelAllocationCriteriaLogic = new HostelAllocationCriteriaLogic();
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
                PersonLogic personLogic = new PersonLogic();
                PaymentLogic paymentLogic = new PaymentLogic();
                _sessionLogic = new SessionLogic();
                HostelAllocationCountLogic hostelAllocationCountLogic = new HostelAllocationCountLogic();
                HostelRequestLogic hostelRequestLogic = new HostelRequestLogic();

                AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
                Programme programme = new Programme();
                Department department = new Department();
                Level level = new Level();

                Model.Model.Student student = new Model.Model.Student();
                Person person = new Person();
                Payment payment = new Payment();
                StudentLevel studentLevel = new StudentLevel();
                HostelAllocation hostelAllocation = new HostelAllocation();
                HostelAllocation existingHostelAllocation = new HostelAllocation();

                List<StudentLevel> studentLevels = new List<StudentLevel>();

                viewModel.Session = _sessionLogic.GetModelsBy(s => s.Active_For_Hostel != null && s.Active_For_Hostel.Value).LastOrDefault() ?? new Session { Id = (int)Sessions._20182019 }; ;
                List<HostelAllocationCriteria> hostelAllocationCriteriaList = new List<HostelAllocationCriteria>();
                
                List<Model.Model.Student> students = studentLogic.GetModelsBy(s => s.Matric_Number == viewModel.Student.MatricNumber);
                
                if (students.Count != 1 && viewModel.Student.MatricNumber.Contains("/"))
                {
                    SetMessage("Student with this Matriculation Number does not exist Or Matric Number is Duplicate!", Message.Category.Error);
                    SetFeeTypeDropDown(viewModel);
                    return View(viewModel);
                }

                if (students.Count == 0 && !viewModel.Student.MatricNumber.Contains("/"))
                {
                    _remitaPaymentLogic = new RemitaPaymentLogic();
                    RemitaPayment remitaPayment = _remitaPaymentLogic.GetModelsBy(r => r.RRR == viewModel.Student.MatricNumber &&
                                                (r.PAYMENT.Fee_Type_Id == (int)FeeTypes.AcceptanceFee || r.PAYMENT.Fee_Type_Id == (int)FeeTypes.HNDAcceptance) &&
                                                r.PAYMENT.Session_Id == viewModel.Session.Id && r.Status.Contains("01")).LastOrDefault();

                    if (remitaPayment != null)
                    {
                        person = remitaPayment.payment.Person;
                        AppliedCourse appliedCourse = appliedCourseLogic.GetModelBy(a => a.Person_Id == person.Id);
                        if (appliedCourse != null)
                        {
                            programme = appliedCourse.Programme;
                            department = appliedCourse.Department;
                            level = new Level() { Id = (int)Levels.NDI };
                            if (programme.Id == (int)Programmes.HNDFullTime || programme.Id == (int)Programmes.HNDWeekend)
                            {
                                level = new Level() { Id = (int)Levels.HNDI };
                            }
                        }
                        else
                        {
                            SetMessage("No Applied course record!", Message.Category.Error);
                            SetFeeTypeDropDown(viewModel);
                            return View(viewModel);
                        }

                    }
                    else
                    {

                        SetMessage("RRR Number is not for Current session's Acceptance Fee!", Message.Category.Error);
                        SetFeeTypeDropDown(viewModel);
                        return View(viewModel);

                    }

                }
                else
                {
                    student = students.FirstOrDefault();
                    person = personLogic.GetModelBy(p => p.Person_Id == student.Id);
                    studentLevels = studentLevelLogic.GetModelsBy(sl => sl.STUDENT.Person_Id == student.Id);
                    if (studentLevels.Count == 0)
                    {
                        SetMessage("You have not registered for this session!", Message.Category.Error);
                        SetFeeTypeDropDown(viewModel);
                        return View(viewModel);
                    }

                    int maxLevelId = studentLevels.Max(sl => sl.Level.Id);
                    studentLevel = studentLevels.LastOrDefault(sl => sl.Level.Id == maxLevelId);
                    viewModel.StudentLevel = studentLevel;
                    programme = studentLevel.Programme;
                    department = studentLevel.Department;
                    level = studentLevel.Level;
                }

                if (person != null)
                {
                    viewModel.Person = person;
                    HostelRequest hostelRequest = hostelRequestLogic.GetModelsBy(h => h.Person_Id == person.Id && h.Session_Id == viewModel.Session.Id && h.Expired == false).LastOrDefault();

                    if (hostelRequest != null)
                    {
                        if(!hostelRequest.Approved)
                        {
                            SetMessage("Your request for hostel allocation has not been approved!", Message.Category.Error);
                            SetFeeTypeDropDown(viewModel);
                            return View(viewModel);
                        }

                        //Cover for records without Approval Date
                        if (hostelRequest.Approved && hostelRequest.ApprovalDate != null && hostelRequest.Hostel != null)
                        {
                            // check for expiration
                            var isValid = CheckHostelRequestValidity(person, viewModel.Session);
                            if (!isValid)
                            {
                                SetMessage("Your request for hostel allocation has Expired, More Than 3 Days from the day of Approval, Request again!", Message.Category.Error);
                                SetFeeTypeDropDown(viewModel);
                                return View(viewModel);
                            }

                            RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                            FeeType feeType = new FeeType() { Id = (int)FeeTypes.HostelFee };
                            double amount = GetHostelFee(hostelRequest.Hostel);
                            Payment checkPayment = paymentLogic.GetModelBy(p => p.Person_Id == viewModel.Person.Id && p.Fee_Type_Id == feeType.Id && p.Session_Id == viewModel.Session.Id);
                            if (checkPayment != null)
                            {
                                var existingRemitaPayment = remitaPaymentLogic.GetModelsBy(f => f.Payment_Id == checkPayment.Id).LastOrDefault();
                                if (existingRemitaPayment != null && existingRemitaPayment.TransactionAmount == (decimal)amount)
                                {
                                    payment = checkPayment;
                                    payment.Amount = amount.ToString();
                                }
                                else
                                {
                                   
                                    using (TransactionScope scope = new TransactionScope())
                                    {
                                        payment = CreatePayment(viewModel, hostelRequest.Hostel);
                                        hostelRequest.Payment = payment;
                                        hostelRequestLogic.Modify(hostelRequest);

                                        //Get Payment Specific Setting
                                        RemitaSettings settings = new RemitaSettings();
                                        RemitaSettingsLogic settingsLogic = new RemitaSettingsLogic();
                                        settings = settingsLogic.GetBy((int)RemitaServiceSettings.Accomodation);

                                        decimal amt = Convert.ToDecimal(payment.Amount);

                                        //Get BaseURL
                                        string remitaBaseUrl = ConfigurationManager.AppSettings["RemitaBaseUrl"].ToString();
                                        RemitaPayementProcessor remitaProcessor = new RemitaPayementProcessor(settings.Api_key);
                                        RemitaPayment remitaPayment = remitaProcessor.GenerateRRRCard(payment.InvoiceNumber, remitaBaseUrl, "ACCOMODATION FEES", settings, amt);

                                        if (remitaPayment != null)
                                        {
                                            scope.Complete();
                                        }
                                    }

                                }
                            }
                            else
                            {
                                using (TransactionScope scope = new TransactionScope())
                                {
                                    payment = CreatePayment(viewModel, hostelRequest.Hostel);
                                    hostelRequest.Payment = payment;
                                    hostelRequestLogic.Modify(hostelRequest);
                                    //Get Payment Specific Setting
                                    RemitaSettings settings = new RemitaSettings();
                                    RemitaSettingsLogic settingsLogic = new RemitaSettingsLogic();
                                    settings = settingsLogic.GetBy((int)RemitaServiceSettings.Accomodation);

                                    decimal amt = Convert.ToDecimal(payment.Amount);

                                    //Get BaseURL
                                    string remitaBaseUrl = ConfigurationManager.AppSettings["RemitaBaseUrl"].ToString();
                                    RemitaPayementProcessor remitaProcessor = new RemitaPayementProcessor(settings.Api_key);
                                    RemitaPayment remitaPayment = remitaProcessor.GenerateRRRCard(payment.InvoiceNumber, remitaBaseUrl, "ACCOMODATION FEES", settings, amt);

                                    if (remitaPayment != null)
                                    {
                                        scope.Complete();
                                    }
                                }
                            }



                        }

                        viewModel.Student = student;
                        viewModel.StudentLevel = studentLevel;
                        viewModel.Payment = payment;
                        TempData["ViewModel"] = viewModel;

                        return RedirectToAction("Invoice");
                    }
                    else
                    {
                        SetMessage("Make a request for hostel allocation before generating invoice!", Message.Category.Error);
                        SetFeeTypeDropDown(viewModel);
                        return View(viewModel);
                    }
                    
                }
               
        }
    
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
                Bugsnag.AspNet.Client.Current.Notify(ex);
            }

            SetFeeTypeDropDown(viewModel);
            return View(viewModel);
        }

       
        private Payment CreatePayment(HostelViewModel viewModel, Hostel hostel)
        {

            try
            {
                PaymentLogic paymentLogic = new PaymentLogic();
                OnlinePaymentLogic onlinePaymentLogic = new OnlinePaymentLogic();
                Payment newPayment = new Payment();
                PaymentMode paymentMode = new PaymentMode() { Id = 1 };
                PaymentType paymentType = new PaymentType() { Id = 2 };
                PersonType personType = viewModel.Person.Type;
                FeeType feeType = new FeeType() { Id = (int)FeeTypes.HostelFee };

                Payment payment = new Payment();
                payment.PaymentMode = paymentMode;
                payment.PaymentType = paymentType;
                payment.PersonType = personType;
                payment.FeeType = feeType;
                payment.DatePaid = DateTime.Now;
                payment.Person = viewModel.Person;
                payment.Session = viewModel.Session;

                Payment checkPayment = paymentLogic.GetModelBy(p => p.Person_Id == viewModel.Person.Id && p.Fee_Type_Id == feeType.Id && p.Session_Id == viewModel.Session.Id);
                if (checkPayment != null)
                {
                    newPayment = checkPayment;
                }
                else
                {
                    newPayment = paymentLogic.Create(payment);
                }

                OnlinePayment newOnlinePayment = null;
                if (newPayment != null)
                {
                    OnlinePayment onlinePaymentCheck = onlinePaymentLogic.GetModelBy(op => op.Payment_Id == newPayment.Id);
                    if (onlinePaymentCheck == null)
                    {
                        PaymentChannel channel = new PaymentChannel() { Id = (int)PaymentChannel.Channels.Etranzact };
                        OnlinePayment onlinePayment = new OnlinePayment();
                        onlinePayment.Channel = channel;
                        onlinePayment.Payment = newPayment;
                        newOnlinePayment = onlinePaymentLogic.Create(onlinePayment);
                    }

                }

                double amount = GetHostelFee(hostel);

                HostelFeeLogic hostelFeeLogic = new HostelFeeLogic();
                HostelFee hostelFee = new HostelFee();
                HostelFee existingHostelFee = hostelFeeLogic.GetModelsBy(h => h.Payment_Id == newPayment.Id).LastOrDefault();
                if (existingHostelFee == null)
                {
                    hostelFee.Hostel = hostel;
                    hostelFee.Payment = newPayment;
                    hostelFee.Amount = amount;
                    hostelFeeLogic.Create(hostelFee);
                }

                newPayment.Amount = amount.ToString();
                return newPayment;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private double GetHostelFee(Hostel hostel)
        {
            double amount = 0;

            HostelAmountLogic hostelAmountLogic = new HostelAmountLogic();
            HostelAmount hostelAmount = new HostelAmount();
           
            try
            {
                if (hostel != null && hostel.Id > 0)
                {
                    hostelAmount = hostelAmountLogic.GetModelBy(p => p.Hostel_Id == hostel.Id);
                    if (hostelAmount != null)
                    {
                        amount = Convert.ToDouble(hostelAmount.Amount);
                    }
                    
                }
            }
            catch (Exception)
            {
                throw;
            }

            return amount;
        }

        public ActionResult Invoice()
        {
            viewModel = (HostelViewModel)TempData["ViewModel"];
            try
            {
                PaymentLogic paymentLogic = new PaymentLogic();
                HostelRequestLogic hostelRequestLogic = new HostelRequestLogic();
                if (viewModel.Payment != null)
                {
                    Payment payment = paymentLogic.GetModelBy(p => p.Payment_Id == viewModel.Payment.Id);
                    if (payment != null && payment.FeeType.Id == (int)FeeTypes.HostelFee)
                    {
                        Invoice invoice = new Invoice();
                        invoice.Person = payment.Person;
                        invoice.Payment = payment;
                        invoice.FeeDetails = new List<FeeDetail>();

                        var hostelRequest = hostelRequestLogic.GetModelsBy(d => d.Payment_Id == payment.Id && d.Approved).LastOrDefault();
                        if (hostelRequest != null &&  hostelRequest.ApprovalDate != null)
                        {
                            DateTime approveDate = (DateTime)hostelRequest.ApprovalDate;
                            var validityPeriod = approveDate.AddDays(3);
                            invoice.PaymentValidityPeriod = validityPeriod.ToString("dddd, dd MMMM yyyy h:mm tt");
                        }

                        
                        Model.Model.Student student = new Model.Model.Student();
                        StudentLogic studentLogic = new StudentLogic();
                        student = studentLogic.GetBy(payment.Person.Id);
                        if (student != null)
                        {
                            invoice.MatricNumber = student.MatricNumber;
                        }

                        invoice.paymentEtranzactType = new PaymentEtranzactType { Name = "FEDPOLY ILARO ACCOMODATION FEE" };
                        RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                        RemitaPayment remitaPayment = remitaPaymentLogic.GetBy(payment.Id);
                        if (remitaPayment != null)
                        {
                            invoice.remitaPayment = remitaPayment;
                            invoice.Amount = Convert.ToDecimal(remitaPayment.TransactionAmount);
                            invoice.Payment.FeeDetails = null;

                            if (remitaPayment.Status.Contains("01"))
                            {
                                invoice.Paid = true;
                            }

                        }
                        else
                        {
                            SetMessage("Cannot Display Invoice! ", Message.Category.Error);
                            return RedirectToAction("GenerateHostelInvoice");
                        }
                        
                        return View(invoice);
                    }


                }

            }
            catch (Exception)
            {
                throw;
            }

            return View();
        }

        public ActionResult PayHostelFee()
        {
            try
            {
                viewModel = new HostelViewModel();
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult PayHostelFee(HostelViewModel viewModel)
        {
            try
            {
                if (viewModel.ConfirmationOrder != null)
                {
                    //PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
                    PaymentLogic paymentLogic = new PaymentLogic();
                    HostelAllocationLogic hostelAllocationLogic = new HostelAllocationLogic();
                    RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                    SessionLogic sessionLogic = new SessionLogic();
                    HostelAllocationCountLogic hostelAllocationCountLogic = new HostelAllocationCountLogic();
                    HostelAllocationCriteriaLogic hostelAllocationCriteriaLogic = new HostelAllocationCriteriaLogic();
                    List<HostelAllocationCriteria> hostelAllocationCriteriaList = new List<HostelAllocationCriteria>();
                    StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                    HostelAllocation hostelAllocation = new HostelAllocation();
                    HostelRequestLogic hostelRequestLogic = new HostelRequestLogic();


                    Payment payment = null;
                    RemitaPayment remitaPayment = new RemitaPayment();
                    RemitaPayementProcessor remitaPayementProcessor = new RemitaPayementProcessor("918567");

                    if (viewModel.ConfirmationOrder.Length >= 12)
                    {
                        Model.Model.Session session = sessionLogic.GetHostelSession();
                        FeeType feetype = new FeeType() { Id = (int)FeeTypes.HostelFee };
                        //Payment payment = paymentLogic.InvalidConfirmationOrderNumber(viewModel.ConfirmationOrder, feetype.Id);

                        remitaPayment = remitaPaymentLogic.GetModelsBy(a => a.RRR == viewModel.ConfirmationOrder).FirstOrDefault();

                        if (remitaPayment != null)
                        {
                            remitaPayment = remitaPayementProcessor.GetStatus(remitaPayment.OrderId);
                            if (remitaPayment.Status.Contains("01:")|| remitaPayment.Description.Contains("Manual"))
                            {
                                payment = remitaPayment.payment;
                            }
                        }

                        if (payment != null && payment.Id > 0)
                        {
                            if (payment.FeeType.Id != (int)FeeTypes.HostelFee)
                            {
                                SetMessage("Confirmation Order Number / RRR (" + viewModel.ConfirmationOrder + ") entered is not for Hostel Fee payment! Please enter your " +
                                                "Hostel Fee Confirmation Order Number.", Message.Category.Error);
                                return View(viewModel);
                            }
                            //Ensure that the request is is still valid with in the time of payment
                            var isValid = CheckHostelRequestValidity(payment.Person, payment.Session);
                            if (!isValid)
                            {

                                SetMessage("Your request for hostel allocation has Expired, Contact Administration For Solution!", Message.Category.Error);
                                SetFeeTypeDropDown(viewModel);
                                return View(viewModel);
                            }

                            // Allocate Hostel
                            var existingHostelAllocation = hostelAllocationLogic.GetModelBy(ha => ha.Session_Id == payment.Session.Id && ha.Student_Id == payment.Person.Id && ha.Occupied);
                            if (existingHostelAllocation != null)
                            {

                                payment = paymentLogic.GetModelBy(p => p.Person_Id == payment.Person.Id && p.Fee_Type_Id == existingHostelAllocation.Payment.FeeType.Id && p.Session_Id == existingHostelAllocation.Session.Id);
                                return RedirectToAction("HostelReceipt", new { pmid = payment.Id });

                            }
                            //var studentCurentLevel = studentLevelLogic.GetModelsBy(c => c.Person_Id == payment.Person.Id && c.Session_Id == payment.Session.Id).LastOrDefault();
                            var hostelRequest=hostelRequestLogic.GetModelsBy(d => d.Person_Id == payment.Person.Id && d.Session_Id == payment.Session.Id && d.Approved == true).LastOrDefault();
                            if (payment.Person.Sex == null)
                            {
                                SetMessage("Error! Ensure that your student profile(Sex) is completely filled", Message.Category.Error);
                                SetFeeTypeDropDown(viewModel);
                                return View(viewModel);
                            }

                            HostelAllocationCount hostelAllocationCount = hostelAllocationCountLogic.GetModelBy(h => h.Sex_Id == payment.Person.Sex.Id && h.Level_Id == hostelRequest.Level.Id);
                            if (hostelAllocationCount.Free == 0)
                            {
                                SetMessage("Error! The Set Number for free Bed Spaces for your level has been exausted!", Message.Category.Error);
                                SetFeeTypeDropDown(viewModel);
                                return View(viewModel);
                            }
                            
                            if (hostelRequest.Level.Id == 0)
                            {
                                SetMessage("You have not registered for this session!", Message.Category.Error);
                                SetFeeTypeDropDown(viewModel);
                                return View(viewModel);
                            }
                            hostelAllocationCriteriaList = hostelAllocationCriteriaLogic.GetModelsBy(hac => hac.Level_Id == hostelRequest.Level.Id && hac.HOSTEL.HOSTEL_TYPE.Hostel_Type_Name == payment.Person.Sex.Name &&
                                                            hac.HOSTEL_ROOM.Reserved == false && hac.HOSTEL_ROOM.Activated && hac.HOSTEL.Activated && hac.HOSTEL_SERIES.Activated &&
                                                            hac.HOSTEL_ROOM_CORNER.Activated);

                            if (hostelAllocationCriteriaList.Count == 0)
                            {
                                SetMessage("Hostel Allocation Criteria for your Level has not been set!", Message.Category.Error);
                                SetFeeTypeDropDown(viewModel);
                                return View(viewModel);
                            }

                            for (int i = 0; i < hostelAllocationCriteriaList.Count; i++)
                            {
                                hostelAllocation.Corner = hostelAllocationCriteriaList[i].Corner;
                                hostelAllocation.Hostel = hostelAllocationCriteriaList[i].Hostel;
                                hostelAllocation.Occupied = true;
                                hostelAllocation.Room = hostelAllocationCriteriaList[i].Room;
                                hostelAllocation.Series = hostelAllocationCriteriaList[i].Series;
                                hostelAllocation.Session = hostelRequest.Session;
                                //hostelAllocation.Student = student;
                                hostelAllocation.Person = payment.Person;

                                HostelAllocation allocationCheck = hostelAllocationLogic.GetModelBy(h => h.Corner_Id == hostelAllocation.Corner.Id && h.Hostel_Id == hostelAllocation.Hostel.Id &&
                                                                    h.Room_Id == hostelAllocation.Room.Id && h.Series_Id == hostelAllocation.Series.Id && h.Session_Id == hostelRequest.Session.Id);
                                if (allocationCheck != null)
                                {
                                    continue;
                                }

                                using (TransactionScope scope = new TransactionScope())
                                {
                                    //payment = CreatePayment(viewModel, hostelAllocationCriteriaList[i].Hostel);
                                    hostelAllocation.Payment = payment;

                                    HostelAllocation newHostelAllocation = hostelAllocationLogic.Create(hostelAllocation);

                                    hostelAllocationCount.Free -= 1;
                                    hostelAllocationCount.TotalCount -= 1;
                                    hostelAllocationCount.LastModified = DateTime.Now;
                                    hostelAllocationCountLogic.Modify(hostelAllocationCount);

                                    scope.Complete();
                                }






                                //HostelAllocation hostelAllocation = hostelAllocationLogic.GetModelBy(ha => ha.Student_Id == payment.Person.Id && ha.Session_Id == payment.Session.Id);

                                //if (hostelAllocation != null)
                                //{
                                //    hostelAllocation.Occupied = true;
                                //    hostelAllocationLogic.Modify(hostelAllocation);
                                //}
                                //else
                                //{
                                //    SetMessage("Allocation does not exist, this could be because you didn't pay within the specified time. Contact your administrator.", Message.Category.Error);
                                //    return RedirectToAction("HostelReceipt");
                                //}

                                return RedirectToAction("HostelReceipt", new { pmid = payment.Id });
                            }
                            SetMessage("Bed Spaces have been exhausted for" + " " + hostelRequest.Level.Name, Message.Category.Error);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
                Bugsnag.AspNet.Client.Current.Notify(ex);
            }

            return View(viewModel);
        }
        public ActionResult HostelReceipt(long pmid)
        {
            try
            {
                viewModel = new HostelViewModel();
                HostelAllocationLogic hostelAllocationLogic = new HostelAllocationLogic();
                PaymentLogic paymentLogic = new PaymentLogic();
                HostelFeeLogic hostelFeeLogic = new HostelFeeLogic();
                HostelRequestLogic hostelRequestLogic = new HostelRequestLogic();
                StudentLogic studentLogic = new StudentLogic();
                HostelFee hostelFee = new HostelFee();
                Payment payment = new Payment();
                viewModel.barcodeImageUrl = "http://applications.federalpolyilaro.edu.ng/Student/Hostel/HostelReceipt?sesid=" + pmid;

                payment = paymentLogic.GetModelBy(p => p.Payment_Id == pmid);
                hostelFee = hostelFeeLogic.GetModelBy(h => h.Payment_Id == pmid);
                HostelAllocation hostelAllocation = hostelAllocationLogic.GetModelBy(ha => ha.Payment_Id == pmid && ha.Session_Id == payment.Session.Id && ha.Student_Id == payment.Person.Id);
                var request=hostelRequestLogic.GetModelsBy(d => d.Payment_Id == pmid).LastOrDefault();
                if (request != null)
                {
                    request.Expired = true;
                    request.Reason = "PAID";
                    hostelRequestLogic.Modify(request);
                    viewModel.HostelRequest = request;
                }
                if (hostelAllocation != null)
                {
                    viewModel.HostelAllocation = hostelAllocation;
                    viewModel.HostelFee = hostelFee;
                    viewModel.Student = studentLogic.GetModelsBy(f => f.Person_Id == hostelAllocation.Person.Id).FirstOrDefault();
                    return View(viewModel);
                }

            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        public ActionResult HostelRequest()
        {
            try
            {
                viewModel = new HostelViewModel();
                
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return View(viewModel);
        }
        [HttpPost]
        public ActionResult HostelRequest(HostelViewModel hostelViewModel)
        {
            try
            {
                //if()

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View(viewModel);
        }
        public bool CheckHostelRequestValidity(Person person, Session session)
        {
            try
            {
                HostelRequestLogic hostelRequestLogic = new HostelRequestLogic();
                HostelRequestCountLogic requestCountLogic = new HostelRequestCountLogic();
                HostelRequest hostelRequest = hostelRequestLogic.GetModelsBy(h => h.Person_Id == person.Id && h.Session_Id == session.Id).LastOrDefault();
                if(hostelRequest!=null && hostelRequest.Reason!=null && hostelRequest.Reason.Contains("PAID"))
                {
                    return true;
                }
                if ( hostelRequest!=null && hostelRequest.Approved && hostelRequest.ApprovalDate != null)
                {
                    // check for expiration
                    HostelRequestCount requestCount = requestCountLogic.GetModelBy(h => h.Level_Id == hostelRequest.Level.Id && h.Sex_Id == hostelRequest.Person.Sex.Id && h.Approved);
                    if (requestCount != null)
                    {
                        TimeSpan DateTimeDiff;
                        DateTimeDiff = TimeSpan.Zero;
                        DateTime currentDate = DateTime.Now;
                        DateTimeDiff = currentDate - (DateTime)hostelRequest.ApprovalDate;
                        var sumDayAndHour = (DateTimeDiff.Days * 24) + DateTimeDiff.Hours;
                        if (hostelRequest != null && sumDayAndHour > 72)
                        {
                            using (TransactionScope scope = new TransactionScope())
                            {
                                SetFeeTypeDropDown(viewModel);
                                hostelRequest.Reason = "Delay In Payment";
                                hostelRequest.Expired = true;
                                hostelRequest.Approved = false;
                                hostelRequestLogic.Modify(hostelRequest);
                                //Release the room from the hostelrequestcount table
                                requestCount.TotalCount += 1;
                                requestCountLogic.Modify(requestCount);

                                scope.Complete();

                            }
                            return false;
                        }
                    }
                    
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return true;
        }
    }
}