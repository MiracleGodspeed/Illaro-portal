using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Abundance_Nk.Model.Entity.Model;
using Abundance_Nk.Web.Controllers;
using Abundance_Nk.Web.Areas.Student.ViewModels;
using Abundance_Nk.Web.Models;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Business;
using System.Transactions;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing;

namespace Abundance_Nk.Web.Areas.Student.Controllers
{
    [AllowAnonymous]
    public class RegistrationController : BaseController
    {
        private RegistrationViewModel viewModel;
        private RegistrationIndexViewModel indexViewModel;
        private RegistrationLogonViewModel logonViewModel;

        private PaymentLogic paymentLogic;
        private string appRoot = ConfigurationManager.AppSettings["AppRoot"];

        public RegistrationController()
        {
            paymentLogic = new PaymentLogic();
            indexViewModel = new RegistrationIndexViewModel();
            logonViewModel = new RegistrationLogonViewModel();
        }

        public ActionResult Logon()
        {
            logonViewModel = new RegistrationLogonViewModel();
            ViewBag.Session = logonViewModel.SessionSelectListItem;
            return View(logonViewModel);
        }

        [HttpPost]
        public ActionResult Logon(RegistrationLogonViewModel viewModel)
        {
            Payment payment = new Payment();

            try
            {
                SessionLogic sessionLogic = new SessionLogic();
                Model.Model.Session applicationSession = sessionLogic.GetApplicationSession() ;
                applicationSession = applicationSession != null ? applicationSession : new Model.Model.Session { Id = (int)Sessions._20182019 };

                if (viewModel.Session != null && viewModel.Session.Id != applicationSession.Id)
                {
                    SetMessage("Registration has closed!", Message.Category.Error);
                    ViewBag.Session = viewModel.SessionSelectListItem;
                    return View(viewModel);
                }
                
                if (viewModel.ConfirmationOrderNumber.Length >  12)
                {
                    //Model.Model.Session session = new Model.Model.Session() { Id = 1 };
                    Model.Model.Session session = viewModel.Session;
                    FeeType feetype = new FeeType() { Id = (int)FeeTypes.SchoolFees };
                    PaymentLogic paymentLogic = new PaymentLogic();
                    payment = paymentLogic.InvalidConfirmationOrderNumber(viewModel.ConfirmationOrderNumber, session, feetype);
                    if (payment != null && payment.Id > 0)
                    {
                        if (payment.FeeType.Id != (int)FeeTypes.SchoolFees && payment.FeeType.Id != (int)FeeTypes.CarryOverSchoolFees)
                        {
                            SetMessage("Confirmation Order Number (" + viewModel.ConfirmationOrderNumber + ") entered is not for School Fees payment! Please enter your School Fees Confirmation Order Number.", Message.Category.Error);
                            ViewBag.Session = logonViewModel.SessionSelectListItem;
                            return View(logonViewModel);
                        }
                    }
                }
                else
                {
                    RemitaPayment remitaPayment = new RemitaPayment();
                    RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                    remitaPayment = remitaPaymentLogic.GetModelsBy(a => a.RRR == viewModel.ConfirmationOrderNumber).FirstOrDefault();
                    if (remitaPayment != null)
                    {
                        payment = remitaPayment.payment;

                        //Get status of transaction
                        RemitaSettings settings = new RemitaSettings();
                        RemitaSettingsLogic settingsLogic = new RemitaSettingsLogic();
                        settings = settingsLogic.GetModelBy(s => s.Payment_SettingId == 2);
                        string remitaVerifyUrl = ConfigurationManager.AppSettings["RemitaVerifyUrl"].ToString();
                        RemitaPayementProcessor remitaPayementProcessor = new RemitaPayementProcessor(settings.Api_key);
                        remitaPayementProcessor.GetTransactionStatus(remitaPayment.RRR, remitaVerifyUrl, 2);
                        remitaPayment = remitaPaymentLogic.GetModelsBy(a => a.RRR == viewModel.ConfirmationOrderNumber).FirstOrDefault();
                        if (remitaPayment != null && remitaPayment.Status.Contains("01:") && (remitaPayment.payment.FeeType.Id == (int)FeeTypes.SchoolFees || remitaPayment.payment.FeeType.Id == (int)FeeTypes.CarryOverSchoolFees))
                        {
                            StudentLogic studentLogic = new StudentLogic();
                            Model.Model.Student student = studentLogic.GetModelBy(s => s.Person_Id == remitaPayment.payment.Person.Id);

                            if (!Utility.HasCompletedSchoolFees(student, remitaPayment.payment.Session))
                            {
                                //this student repeated 100level and was asked to pay as returning student (29900) which is below the suppose amount (50400) for new students
                                if (student?.Id > 0 && remitaPayment.payment.Session?.Id > 0 && student.Id == 149264 && remitaPayment.payment.Session.Id == 11)
                                {
                                }
                                else
                                {
                                    throw new Exception("You have not completed your fees, kindly complete your fees before proceeding.");
                                }
                                
                            }
                        }
                        else
                        {
                            SetMessage("Payment couldn't verified for this payment purpose!", Message.Category.Error);
                            ViewBag.Session = logonViewModel.SessionSelectListItem;
                            return View(logonViewModel);
                        }
                    }
                    else
                    {
                        SetMessage("Payment couldn't verified!", Message.Category.Error);
                        ViewBag.Session = logonViewModel.SessionSelectListItem;
                        return View(logonViewModel);
                    }
                }
              
               
            }
            catch (Exception ex)
            {
                if (ex.Message == "The pin amount tied to the pin is not correct. Please contact support@lloydant.com." || ex.Message == "You have not completed your fees, kindly complete your fees before proceeding.")
                {
                    string checkMessage = checkSchoolFeeShortFall(viewModel.ConfirmationOrderNumber, viewModel.Session, payment != null ? payment.FeeType : new FeeType() { Id = (int)FeeTypes.SchoolFees });
                    if (checkMessage == "True")
                    {
                        //return RedirectToAction("GenerateShortFallInvoice", "Payment", new { area = "Student" });
                        SetMessage("The pin amount is incorrect! Kindly complete your payment before proceeding.", Message.Category.Error);
                        ViewBag.Session = logonViewModel.SessionSelectListItem;
                        return View(logonViewModel);
                    }
                    else if (checkMessage == "False")
                    {
                        SetMessage("Kindly try again! ", Message.Category.Information);
                        ViewBag.Session = logonViewModel.SessionSelectListItem;
                        return View(logonViewModel);
                    }
                    else
                    {
                        SetMessage(checkMessage, Message.Category.Error);
                        ViewBag.Session = logonViewModel.SessionSelectListItem;
                        return View(logonViewModel);
                    }
                }

                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
                ViewBag.Session = logonViewModel.SessionSelectListItem;
                return View(logonViewModel);
            }

            TempData["Payment"] = payment;
            return RedirectToAction("Index", "Registration", new { Area = "Student", sid = Abundance_Nk.Web.Models.Utility.Encrypt(payment.Person.Id.ToString()), sesId = viewModel.Session.Id});
        }
        private string checkSchoolFeeShortFall(string ConfirmationNumber, Model.Model.Session session, FeeType feeType)
        {
            try
            {
                PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
                PaymentTerminalLogic paymentTerminalLogic = new PaymentTerminalLogic();
                PaymentLogic paymentLogic = new PaymentLogic();
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                FeeDetailLogic feeDetailLogic = new FeeDetailLogic();
                ShortFallLogic shortFallLogic = new ShortFallLogic();

                PaymentTerminal paymentTerminal = paymentTerminalLogic.GetModelBy(p => p.Session_Id == session.Id && p.Fee_Type_Id == feeType.Id);
                PaymentEtranzact etranzact = null;
                if (paymentTerminal != null)
                {
                    etranzact = paymentEtranzactLogic.RetrieveEtranzactWebServicePinDetails(ConfirmationNumber, paymentTerminal);
                }
                

                if (etranzact != null)
                {
                    Payment payment = paymentLogic.GetModelBy(p => p.Invoice_Number == etranzact.CustomerID.ToUpper().Trim());
                    StudentLevel studentLevel = studentLevelLogic.GetModelsBy(s => s.Person_Id == payment.Person.Id && s.Session_Id == session.Id).LastOrDefault();

                    if (studentLevel != null)
                    {
                        decimal AmountToPay = feeDetailLogic.GetFeeByDepartmentLevel(studentLevel.Department, studentLevel.Level, studentLevel.Programme, payment.FeeType, payment.Session, payment.PaymentMode);
                        if (etranzact.TransactionAmount < AmountToPay)
                        {
                            ShortFall shortFall = shortFallLogic.GetModelsBy(s => s.PAYMENT.Person_Id == payment.Person.Id && s.PAYMENT.Fee_Type_Id == (int)FeeTypes.ShortFall && s.PAYMENT.Session_Id == session.Id).LastOrDefault();

                            if (shortFall != null && (Convert.ToDecimal(shortFall.Amount) + etranzact.TransactionAmount == AmountToPay))
                            {
                                PaymentEtranzact etranzactShortFall = paymentEtranzactLogic.GetModelBy(p => p.Payment_Id == shortFall.Payment.Id);
                                if (etranzactShortFall != null)
                                {
                                    PaymentEtranzact etranzactToModify = paymentEtranzactLogic.GetModelBy(p => p.Payment_Id == payment.Id);
                                    if (etranzactToModify != null)
                                    {
                                        etranzactToModify.TransactionAmount = AmountToPay;
                                        paymentEtranzactLogic.Modify(etranzactToModify);

                                        return "False";
                                    }
                                }
                                else
                                {
                                    return "True";
                                }

                            }
                            else
                            {
                                return "True";
                            }
                        }
                        else if (etranzact.TransactionAmount == AmountToPay)
                        {
                            PaymentEtranzact etranzactToModify = paymentEtranzactLogic.GetModelBy(p => p.Payment_Id == payment.Id);
                            if (etranzactToModify != null)
                            {
                                etranzactToModify.TransactionAmount = AmountToPay;
                                paymentEtranzactLogic.Modify(etranzactToModify);

                                return "False";
                            }
                        }
                    }
                }
                else
                {
                    RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                    RemitaPayment remitaPayment = remitaPaymentLogic.GetModelsBy(r => r.RRR == ConfirmationNumber && (r.Status.Contains("01") || r.Description.ToLower().Contains("manual"))).LastOrDefault();
                    if (remitaPayment != null)
                    {
                        Payment payment = paymentLogic.GetModelBy(p => p.Payment_Id == remitaPayment.payment.Id);
                        StudentLevel studentLevel = studentLevelLogic.GetModelsBy(s => s.Person_Id == payment.Person.Id && s.Session_Id == session.Id).LastOrDefault();

                        if (studentLevel != null)
                        {
                            decimal AmountToPay = feeDetailLogic.GetFeeByDepartmentLevel(studentLevel.Department, studentLevel.Level, studentLevel.Programme, payment.FeeType, payment.Session, payment.PaymentMode);
                            if (remitaPayment.TransactionAmount < AmountToPay)
                            {
                                ShortFall shortFall = shortFallLogic.GetModelsBy(s => s.PAYMENT.Person_Id == payment.Person.Id && s.PAYMENT.Fee_Type_Id == (int)FeeTypes.ShortFall && s.PAYMENT.Session_Id == session.Id).LastOrDefault();

                                if (shortFall != null && (Convert.ToDecimal(shortFall.Amount) + remitaPayment.TransactionAmount == AmountToPay))
                                {
                                    RemitaPayment remitaShortFall = remitaPaymentLogic.GetModelBy(p => p.Payment_Id == shortFall.Payment.Id && (p.Status.Contains("01") || p.Description.ToLower().Contains("manual")));
                                    if (remitaShortFall != null)
                                    {
                                        return "False";
                                    }
                                    else
                                    {
                                        return "True";
                                    }
                                }
                                else
                                {
                                    return "True";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "True";
        }
        public ActionResult LogonCarryOver()
        {
            return View(logonViewModel);
        }

        public ActionResult Index(string sid, int sesId)
        {
            
            try
            {
                long stId = Convert.ToInt64(Abundance_Nk.Web.Models.Utility.Decrypt(sid));
                StudentLogic studentLogic = new StudentLogic();
                indexViewModel.Student = studentLogic.GetBy(stId);
                indexViewModel.Session = new Session() { Id = sesId };
                indexViewModel.Payment = (Payment)TempData.Peek("Payment");

                if (indexViewModel.Student != null && indexViewModel.Student.Id > 0)
                {
                    indexViewModel.isExtraYearStudent = false;
                    StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                    //indexViewModel.StudentLevel = studentLevelLogic.GetBy(indexViewModel.Student, indexViewModel.Session);
                    //indexViewModel.StudentLevel = studentLevelLogic.GetExtraYearBy(indexViewModel.Student.Id);
                    indexViewModel.StudentLevel = studentLevelLogic.GetModelsBy(s => s.Person_Id == indexViewModel.Student.Id && s.Session_Id == sesId).LastOrDefault();

                    if (indexViewModel.StudentLevel != null && indexViewModel.StudentLevel.Session.Id != indexViewModel.Session.Id)
                    {
                        List<StudentLevel> studentLevels = studentLevelLogic.GetModelsBy(s => s.Person_Id == indexViewModel.Student.Id);
                        StudentLevel currentSessionLevel = studentLevels.LastOrDefault(s => s.Session.Id == indexViewModel.Session.Id);
                        if (currentSessionLevel != null)
                        {
                            indexViewModel.StudentLevel = currentSessionLevel;
                        }
                        else
                        {
                            PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
                            PaymentEtranzact paymentEtranzact = paymentEtranzactLogic.GetModelBy(p => p.ONLINE_PAYMENT.PAYMENT.Session_Id == indexViewModel.Session.Id && p.ONLINE_PAYMENT.PAYMENT.Fee_Type_Id == (int)FeeTypes.SchoolFees && p.ONLINE_PAYMENT.PAYMENT.Person_Id == indexViewModel.Student.Id);
                            if (paymentEtranzact != null)
                            {
                                StudentLevel newStudentLevel = indexViewModel.StudentLevel;
                                newStudentLevel.Session = indexViewModel.Session;
                                if (newStudentLevel.Level.Id == 1)
                                {
                                    newStudentLevel.Level = new Level() { Id = 2 };
                                }
                                else if (newStudentLevel.Level.Id == 3)
                                {
                                    newStudentLevel.Level = new Level() { Id = 4 };
                                }
                                else
                                {
                                    newStudentLevel.Level = indexViewModel.StudentLevel.Level;
                                }

                                StudentLevel createdStudentLevel = studentLevelLogic.Create(newStudentLevel);
                                indexViewModel.StudentLevel = studentLevelLogic.GetModelBy(s => s.Student_Level_Id == createdStudentLevel.Id);
                            }
                        } 
                    }  

                    RemitaPayment remitaPayment = new RemitaPayment();
                    RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                    remitaPayment = remitaPaymentLogic.GetModelsBy(a => a.PAYMENT.Person_Id == indexViewModel.Student.Id && a.PAYMENT.Fee_Type_Id == 3 && a.Status.Contains("01:")).FirstOrDefault();

                    PaymentHistory paymentHistory = new PaymentHistory();
                    List<PaymentView> pastPaymentsRemita = new List<PaymentView>();
                    List<PaymentView> pastPaymentsEtranzact = new List<PaymentView>();

                    if (remitaPayment != null && remitaPayment.payment != null)
                    {
                        pastPaymentsRemita = paymentLogic.GetBy(remitaPayment); 
                    }  
                    pastPaymentsEtranzact = paymentLogic.GetBy(indexViewModel.Payment.Person);
                    //paymentHistory.Payments = paymentLogic.GetBy(remitaPayment);
                    paymentHistory.Payments = new List<PaymentView>();
                    if (pastPaymentsRemita.Count > 0)
                    {
                        paymentHistory.Payments.AddRange(pastPaymentsRemita);
                    }
                    if (pastPaymentsEtranzact.Count > 0)
                    {
                        paymentHistory.Payments.AddRange(pastPaymentsEtranzact); 
                    }

                    paymentHistory.Student = indexViewModel.Student;

                    indexViewModel.PaymentHistory = paymentHistory;
                    if (paymentHistory.Payments == null || paymentHistory.Payments.Count <= 0)
                    {
                        SetMessage("No payment made yet! Kindly generate invoice, go to bank and make your payments", Message.Category.Error);
                    }

                    StudentExtraYearSession extraYear = new StudentExtraYearSession();
                    StudentExtraYearLogic extraYearLogic = new StudentExtraYearLogic();
                    extraYear = extraYearLogic.GetBy(indexViewModel.Payment.Person.Id, indexViewModel.Payment.Session.Id);
                    if (extraYear != null)
                    {
                        indexViewModel.isExtraYearStudent = true;
                    }
                    else
                    {
                        indexViewModel.isExtraYearStudent = false;
                    }
                }
                else
                {
                    SetMessage("Student details not found in the system! Please contact your system administrator.", Message.Category.Error);
                }
            }
            catch(Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
            
            TempData["viewModel"] = indexViewModel;

            return View(indexViewModel);
        }

        public ActionResult SelectCourseRegistrationSession()

        {
            RegistrationIndexViewModel  viewModel = (RegistrationIndexViewModel) TempData["viewModel"];
            try
            {
                if (viewModel == null && System.Web.HttpContext.Current.Session["student"] != null)
                {
                    var studentLogic = new StudentLogic();
                    Model.Model.Student student = System.Web.HttpContext.Current.Session["student"] as Model.Model.Student;
                    student = studentLogic.GetBy(student.Id);

                    StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                    StudentLevel studentLevel = studentLevelLogic.GetBy(student.Id);

                    viewModel = new RegistrationIndexViewModel();
                    viewModel.Student = student;
                    viewModel.StudentLevel = studentLevel;
                }

                indexViewModel.CourseRegistrationSession = new Session();
                ViewBag.Session = indexViewModel.SessionSelectListItem;
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            TempData.Keep("viewModel");
            return View(viewModel);
        }
         [HttpPost]
        public ActionResult SelectCourseRegistrationSession(RegistrationIndexViewModel viewModel)
        {
            try
            {
                SessionSemesterLogic sessionSemesterLogic = new SessionSemesterLogic();
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                List<StudentLevel> studentLevels = studentLevelLogic.GetModelsBy(s => s.Person_Id == viewModel.Student.Id);
                SessionSemester sessionSemester = sessionSemesterLogic.GetModelsBy(s => s.Session_Id == viewModel.CourseRegistrationSession.Id).LastOrDefault();
                bool hasPaidFees = false;
                bool inRegisteredSession = false;
                for (int i = 0; i < studentLevels.Count; i++)
                {
                    if (viewModel.CourseRegistrationSession.Id == studentLevels[i].Session.Id)
                    {
                        inRegisteredSession = true;
                    } 
                }
                //ensure student has paid school fees for the selected session
                if(studentLevels.LastOrDefault().Programme.Id == (int)Programmes.NDDistance || studentLevels.LastOrDefault().Programme.Id == (int)Programmes.HNDDistance)
                {
                    hasPaidFees = Utility.HasCompletedSchoolFeesDistantLearning(viewModel.Student, viewModel.CourseRegistrationSession);
                    bool paidFirstInstallment = Utility.HasCompletedSchoolFirstInstallment(viewModel.Student, viewModel.CourseRegistrationSession);

                    if(!hasPaidFees && !paidFirstInstallment)
                    {
                        SetMessage("You have not paid fees for this session! ", Message.Category.Error);
                        return RedirectToAction("SelectCourseRegistrationSession");
                    }
                }
                else
                {
                    hasPaidFees = Utility.HasCompletedSchoolFees(viewModel.Student, viewModel.CourseRegistrationSession);
                    if (!hasPaidFees)
                    {
                        //this student repeated 100level and was asked to pay as returning student (29900) which is below the suppose amount (50400) for new students
                        if (viewModel.Student?.Id > 0 && viewModel.CourseRegistrationSession?.Id > 0 && (viewModel.Student.Id == 149264 || viewModel.Student.Id == 148467) && viewModel.CourseRegistrationSession.Id == 11)
                        {

                        }
                        else
                        {
                            SetMessage("You have not paid fees for this session! ", Message.Category.Error);
                            return RedirectToAction("SelectCourseRegistrationSession");
                        }
                    }
                }
              
                
                if (inRegisteredSession)
                {
                    return RedirectToAction("Form", "CourseRegistration", new { sid = Utility.Encrypt(viewModel.Student.Id.ToString()), ssid = sessionSemester.Id});

                }
                else
                {
                    SetMessage("You have not registered for this session!", Message.Category.Error);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            //RegistrationIndexViewModel regIndexViewModel = (RegistrationIndexViewModel)TempData["viewModel"];
            //ViewBag.Session = regIndexViewModel.SessionSelectListItem;
            //return View(regIndexViewModel);

             return RedirectToAction("SelectCourseRegistrationSession");
        }
        public ActionResult Form(long sid, int pid)
        {
            RegistrationViewModel existingViewModel = (RegistrationViewModel)TempData["RegistrationViewModel"];

            try
            {
                PopulateAllDropDowns(pid);
                if (existingViewModel != null)
                {
                    viewModel = existingViewModel;
                    SetStudentUploadedPassport(viewModel);
                }
                
                viewModel.LoadStudentInformationFormBy(sid);
                if (viewModel.Student != null && viewModel.Student.Id > 0)
                {
                    if (viewModel.Payment == null)
                    {
                        viewModel.Payment = (Payment)TempData.Peek("Payment");
                    }

                    SetSelectedSittingSubjectAndGrade(viewModel);
                    SetLgaIfExist(viewModel);
                    SetDepartmentIfExist(viewModel);
                    SetDepartmentOptionIfExist(viewModel);
                    SetEntryAndStudyMode(viewModel);
                    SetDateOfBirth();

                    viewModel.Student.Type = new StudentType() { Id = (int)StudentType.EnumName.Returning };
                    if (viewModel.StudentLevel.Programme.Id == 3 || viewModel.StudentLevel.Programme.Id ==  4)
                    {
                        SetPreviousEducationStartDate();
                        SetPreviousEducationEndDate();

                        viewModel.Student.Category = viewModel.StudentLevel.Student.Category;
                        viewModel.Student.Type = viewModel.StudentLevel.Student.Type;
                    }
                    else
                    {
                        viewModel.Student.Category = viewModel.StudentLevel.Student.Category;
                        viewModel.Student.Type = viewModel.StudentLevel.Student.Type;
                    }

                    SetLastEmploymentStartDate();
                    SetLastEmploymentEndDate();
                    SetNdResultDateAwarded();
                    SetStudentAcademicInformationLevel();
                }

            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            TempData["RegistrationViewModel"] = viewModel;
            TempData["imageUrl"] = viewModel.Person.ImageFileUrl;

            return View(viewModel);
        }

        private void SetStudentAcademicInformationLevel()
        {
            try
            {
                if (viewModel.StudentAcademicInformation.Level == null || viewModel.StudentAcademicInformation.Level.Id <= 0)
                {
                    if (viewModel.StudentLevel.Level != null && viewModel.StudentLevel.Level.Id > 0)
                    {
                        viewModel.StudentAcademicInformation.Level = viewModel.StudentLevel.Level;
                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
        }

        private void SetNdResultDateAwarded()
        {
            try
            {
                //if (viewModel.StudentNdResult != null && viewModel.StudentNdResult.DateAwarded != null)
                if (viewModel.StudentNdResult != null && viewModel.StudentNdResult.DateAwarded != DateTime.MinValue)
                {
                    if (viewModel.StudentNdResult.YearAwarded.Id > 0 && viewModel.StudentNdResult.MonthAwarded.Id > 0)
                    {
                        int noOfDays = DateTime.DaysInMonth(viewModel.StudentNdResult.YearAwarded.Id, viewModel.StudentNdResult.MonthAwarded.Id);
                        List<Value> days = Utility.CreateNumberListFrom(1, noOfDays);
                        if (days != null && days.Count > 0)
                        {
                            days.Insert(0, new Value() { Name = "--DD--" });
                        }

                        if (viewModel.StudentNdResult.DayAwarded != null && viewModel.StudentNdResult.DayAwarded.Id > 0)
                        {
                            ViewBag.StudentNdResultDayAwardeds = new SelectList(days, Utility.ID, Utility.NAME, viewModel.StudentNdResult.DayAwarded.Id);
                        }
                        else
                        {
                            ViewBag.StudentNdResultDayAwardeds = new SelectList(days, Utility.ID, Utility.NAME);
                        }
                    }
                }
                else
                {
                    ViewBag.StudentNdResultDayAwardeds = new SelectList(new List<Value>(), Utility.ID, Utility.NAME);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
        }
       
        private void SetLastEmploymentStartDate()
        {
            try
            {
                //if (viewModel.StudentEmploymentInformation != null && viewModel.StudentEmploymentInformation.StartDate != null)
                if (viewModel.StudentEmploymentInformation != null && viewModel.StudentEmploymentInformation.StartDate != DateTime.MinValue)
                {
                    if (viewModel.StudentEmploymentInformation.StartYear.Id > 0 && viewModel.StudentEmploymentInformation.StartMonth.Id > 0)
                    {
                        int noOfDays = DateTime.DaysInMonth(viewModel.StudentEmploymentInformation.StartYear.Id, viewModel.StudentEmploymentInformation.StartMonth.Id);
                        List<Value> days = Utility.CreateNumberListFrom(1, noOfDays);
                        if (days != null && days.Count > 0)
                        {
                            days.Insert(0, new Value() { Name = "--DD--" });
                        }

                        if (viewModel.StudentEmploymentInformation.StartDay != null && viewModel.StudentEmploymentInformation.StartDay.Id > 0)
                        {
                            ViewBag.StudentLastEmploymentStartDays = new SelectList(days, Utility.ID, Utility.NAME, viewModel.StudentEmploymentInformation.StartDay.Id);
                        }
                        else
                        {
                            ViewBag.StudentLastEmploymentStartDays = new SelectList(days, Utility.ID, Utility.NAME);
                        }
                    }
                }
                else
                {
                    ViewBag.StudentLastEmploymentStartDays = new SelectList(new List<Value>(), Utility.ID, Utility.NAME);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
        }

        private void SetLastEmploymentEndDate()
        {
            try
            {
                //if (viewModel.StudentEmploymentInformation != null && viewModel.StudentEmploymentInformation.EndDate != null)
                if (viewModel.StudentEmploymentInformation != null && viewModel.StudentEmploymentInformation.EndDate != DateTime.MinValue)
                {
                    if (viewModel.StudentEmploymentInformation.EndYear.Id > 0 && viewModel.StudentEmploymentInformation.EndMonth.Id > 0)
                    {
                        int noOfDays = DateTime.DaysInMonth(viewModel.StudentEmploymentInformation.EndYear.Id, viewModel.StudentEmploymentInformation.EndMonth.Id);
                        List<Value> days = Utility.CreateNumberListFrom(1, noOfDays);
                        if (days != null && days.Count > 0)
                        {
                            days.Insert(0, new Value() { Name = "--DD--" });
                        }

                        if (viewModel.StudentEmploymentInformation.EndDay != null && viewModel.StudentEmploymentInformation.EndDay.Id > 0)
                        {
                            ViewBag.StudentLastEmploymentEndDays = new SelectList(days, Utility.ID, Utility.NAME, viewModel.StudentEmploymentInformation.EndDay.Id);
                        }
                        else
                        {
                            ViewBag.StudentLastEmploymentEndDays = new SelectList(days, Utility.ID, Utility.NAME);
                        }
                    }
                }
                else
                {
                    ViewBag.StudentLastEmploymentEndDays = new SelectList(new List<Value>(), Utility.ID, Utility.NAME);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
        }

        private void SetEntryAndStudyMode(RegistrationViewModel vModel)
        {
            try
            {
                //set mode of entry

                switch (vModel.StudentLevel.Programme.Id)
                {
                    case 1:
                        {
                            vModel.StudentAcademicInformation.ModeOfEntry = new ModeOfEntry() { Id = 3 };
                            break;
                        }
                    case 2:
                        {
                            vModel.StudentAcademicInformation.ModeOfEntry = new ModeOfEntry() { Id = 2};
                            break;
                        }
                    case 3:
                        {
                            vModel.StudentAcademicInformation.ModeOfEntry = new ModeOfEntry() { Id = 4 };
                            break;
                        }
                    case 4:
                        {
                            vModel.StudentAcademicInformation.ModeOfEntry = new ModeOfEntry() { Id = 1 };

                            break;
                        }
                    case 8:
                        {
                            vModel.StudentAcademicInformation.ModeOfEntry = new ModeOfEntry() { Id = 6 };
                            break;
                        }
                    case 9:
                        {
                            vModel.StudentAcademicInformation.ModeOfEntry = new ModeOfEntry() { Id = 7 };
                            break;
                        }
                }

                //set mode of study
                switch (vModel.StudentLevel.Programme.Id)
                {
                    case 1:
                    case 3:
                        {
                            vModel.StudentAcademicInformation.ModeOfStudy = new ModeOfStudy() { Id = 1 };
                            break;
                        }
                    case 2:
                    case 4:
                        {
                            vModel.StudentAcademicInformation.ModeOfStudy = new ModeOfStudy() { Id = 2 };
                            break;
                        }
                   
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public ActionResult Form(RegistrationViewModel viewModel)
        {
            try
            {

                RegistrationViewModel existingVModel = (RegistrationViewModel)TempData["RegistrationViewModel"];
                SetStudentUploadedPassport(viewModel);
                SetStudentUploadedSignature(viewModel);
                ModelState.Remove("Student.FirstName");
                ModelState.Remove("Student.LastName");
                ModelState.Remove("Student.MobilePhone");
                ModelState.Remove("Payment.Id");

                var asdf = ModelState.Values.Where(v => v.Errors.Count > 0);
                //if (ModelState.IsValid)
                //{
                    if (string.IsNullOrEmpty(viewModel.Person.ImageFileUrl) || viewModel.Person.ImageFileUrl == Utility.DEFAULT_AVATAR)
                    {
                        SetMessage("No Passport uploaded! Please upload your passport to continue.", Message.Category.Error);
                        SetStateVariables(viewModel);
                        return View(viewModel);
                    }
                    if (string.IsNullOrEmpty(viewModel.Person.SignatureFileUrl) || viewModel.Person.SignatureFileUrl == Utility.DEFAULT_AVATAR)
                    {
                        SetMessage("No Signature uploaded! Please upload your signature to continue.", Message.Category.Error);
                        SetStateVariables(viewModel);
                        return View(viewModel);
                    }

                if (existingVModel != null && existingVModel.FirstSittingOLevelResultDetails.Any())
                {
                    viewModel.FirstSittingOLevelResultDetails = existingVModel.FirstSittingOLevelResultDetails;
                    viewModel.FirstSittingOLevelResult = existingVModel.FirstSittingOLevelResult;
                }
                if (existingVModel != null && existingVModel.SecondSittingOLevelResultDetails.Any())
                {
                    viewModel.SecondSittingOLevelResultDetails = existingVModel.SecondSittingOLevelResultDetails;
                    viewModel.SecondSittingOLevelResult = existingVModel.SecondSittingOLevelResult;
                }

                TempData["RegistrationViewModel"] = viewModel;
                    return RedirectToAction("FormPreview", "Registration");
                //}
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            SetStateVariables(viewModel);
            return View(viewModel);
        }

        private void SetStateVariables(RegistrationViewModel viewModel)
        {
            try
            {
                
                TempData["RegistrationViewModel"] = viewModel;
                TempData["imageUrl"] = viewModel.Person.ImageFileUrl;
                TempData["SignatureFile"] = viewModel.Person.SignatureFileUrl;

                PopulateAllDropDowns(viewModel.StudentLevel.Programme.Id);
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
        }
        
        public ActionResult FormPreview()
        {
            RegistrationViewModel viewModel = (RegistrationViewModel)TempData["RegistrationViewModel"];

            try
            {
                if (viewModel != null)
                {
                    viewModel.Person.DateOfBirth = new DateTime(viewModel.Person.YearOfBirth.Id, viewModel.Person.MonthOfBirth.Id, viewModel.Person.DayOfBirth.Id);
                    viewModel.Person.State = viewModel.States.Where(m => m.Id == viewModel.Person.State.Id).SingleOrDefault();
                    viewModel.Person.LocalGovernment = viewModel.Lgas.Where(m => m.Id == viewModel.Person.LocalGovernment.Id).SingleOrDefault();
                    viewModel.Person.Sex = viewModel.Genders.Where(m => m.Id == viewModel.Person.Sex.Id).SingleOrDefault();
                    viewModel.NextOfKin.Relationship = viewModel.Relationships.Where(m => m.Id == viewModel.NextOfKin.Relationship.Id).SingleOrDefault();
                    viewModel.Person.Religion = viewModel.Religions.Where(m => m.Id == viewModel.Person.Religion.Id).SingleOrDefault();
                    viewModel.Student.Title = viewModel.Titles.Where(m => m.Id == viewModel.Student.Title.Id).SingleOrDefault();
                    viewModel.Student.MaritalStatus = viewModel.MaritalStatuses.Where(m => m.Id == viewModel.Student.MaritalStatus.Id).SingleOrDefault();

                    if (viewModel.Student.BloodGroup != null && viewModel.Student.BloodGroup.Id > 0)
                    {
                        viewModel.Student.BloodGroup = viewModel.BloodGroups.Where(m => m.Id == viewModel.Student.BloodGroup.Id).SingleOrDefault();
                    }
                    if (viewModel.Student.Genotype != null && viewModel.Student.Genotype.Id > 0)
                    {
                        viewModel.Student.Genotype = viewModel.Genotypes.Where(m => m.Id == viewModel.Student.Genotype.Id).SingleOrDefault();
                    }

                    viewModel.StudentAcademicInformation.ModeOfEntry = viewModel.ModeOfEntries.Where(m => m.Id == viewModel.StudentAcademicInformation.ModeOfEntry.Id).SingleOrDefault();
                    viewModel.StudentAcademicInformation.ModeOfStudy = viewModel.ModeOfStudies.Where(m => m.Id == viewModel.StudentAcademicInformation.ModeOfStudy.Id).SingleOrDefault();
                    viewModel.Student.Category = viewModel.StudentCategories.Where(m => m.Id == viewModel.Student.Category.Id).SingleOrDefault();
                    viewModel.Student.Type = viewModel.StudentTypes.Where(m => m.Id == viewModel.Student.Type.Id).SingleOrDefault();
                    viewModel.StudentAcademicInformation.Level = viewModel.Levels.Where(m => m.Id == viewModel.StudentAcademicInformation.Level.Id).SingleOrDefault();
                    viewModel.StudentFinanceInformation.Mode = viewModel.ModeOfFinances.Where(m => m.Id == viewModel.StudentFinanceInformation.Mode.Id).SingleOrDefault();
                    viewModel.StudentSponsor.Relationship = viewModel.Relationships.Where(m => m.Id == viewModel.StudentSponsor.Relationship.Id).SingleOrDefault();
                    
                    viewModel.FirstSittingOLevelResult.Type = viewModel.OLevelTypes.Where(m => m.Id == viewModel.FirstSittingOLevelResult.Type.Id).SingleOrDefault();
                    if (viewModel.SecondSittingOLevelResult.Type != null)
                    {
                        viewModel.SecondSittingOLevelResult.Type = viewModel.OLevelTypes.Where(m => m.Id == viewModel.SecondSittingOLevelResult.Type.Id).SingleOrDefault();
                    }

                    if (viewModel.StudentLevel.DepartmentOption == null || viewModel.StudentLevel.DepartmentOption.Id <= 0)
                    {
                        viewModel.StudentLevel.DepartmentOption = new DepartmentOption() { Id = 1 };
                        viewModel.StudentLevel.DepartmentOption.Name = viewModel.StudentLevel.Department.Name;
                    }

                    if (viewModel.StudentLevel.Programme.Id == 3 || viewModel.StudentLevel.Programme.Id == 4)
                    {
                        viewModel.PreviousEducation.StartDate = new DateTime(viewModel.PreviousEducation.StartYear.Id, viewModel.PreviousEducation.StartMonth.Id, viewModel.PreviousEducation.StartDay.Id);
                        viewModel.PreviousEducation.EndDate = new DateTime(viewModel.PreviousEducation.EndYear.Id, viewModel.PreviousEducation.EndMonth.Id, viewModel.PreviousEducation.EndDay.Id);
                        viewModel.StudentEmploymentInformation.StartDate = new DateTime(viewModel.StudentEmploymentInformation.StartYear.Id, viewModel.StudentEmploymentInformation.StartMonth.Id, viewModel.StudentEmploymentInformation.StartDay.Id);
                        viewModel.StudentEmploymentInformation.EndDate = new DateTime(viewModel.StudentEmploymentInformation.EndYear.Id, viewModel.StudentEmploymentInformation.EndMonth.Id, viewModel.StudentEmploymentInformation.EndDay.Id);
                        viewModel.StudentNdResult.DateAwarded = new DateTime(viewModel.StudentNdResult.YearAwarded.Id, viewModel.StudentNdResult.MonthAwarded.Id, viewModel.StudentNdResult.DayAwarded.Id);
                        viewModel.PreviousEducation.ResultGrade = viewModel.ResultGrades.Where(m => m.Id == viewModel.PreviousEducation.ResultGrade.Id).SingleOrDefault();
                    }
                  
                    UpdateOLevelResultDetail(viewModel);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            TempData["RegistrationViewModel"] = viewModel;
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult FormPreview(RegistrationViewModel vm)
        {
            Abundance_Nk.Model.Model.Student newStudent = null;
            RegistrationViewModel viewModel = (RegistrationViewModel)TempData["RegistrationViewModel"];
            PersonType personType = new PersonType() { Id = (int)PersonType.EnumName.Student };

            try
            {
                if (viewModel.StudentAlreadyExist == false)
                {
                    using (TransactionScope transaction = new TransactionScope())
                    {
                        viewModel.Student.Id = viewModel.Person.Id;
                        viewModel.Student.Status = new StudentStatus() { Id = 1 };
                        StudentLogic studentLogic = new StudentLogic();

                        newStudent = viewModel.Student;
                        studentLogic.Modify(viewModel.Student);


                        viewModel.StudentSponsor.Student = newStudent;
                        StudentSponsorLogic sponsorLogic = new StudentSponsorLogic();
                        if (sponsorLogic.GetModelsBy(a => a.Person_Id == newStudent.Id).FirstOrDefault() != null)
                        {
                            sponsorLogic.Modify(viewModel.StudentSponsor);
                        }
                        else
                        {
                            sponsorLogic.Create(viewModel.StudentSponsor);
                        }
                        

                        
                        viewModel.NextOfKin.Person = newStudent;
                        viewModel.NextOfKin.PersonType = new PersonType() { Id = (int)PersonType.EnumName.Student };
                        NextOfKinLogic nextOfKinLogic = new NextOfKinLogic();
                        if (nextOfKinLogic.GetModelsBy(a => a.Person_Id == newStudent.Id).FirstOrDefault() != null)
                        {
                            nextOfKinLogic.Modify(viewModel.NextOfKin);
                        }
                        else
                        {
                            nextOfKinLogic.Create(viewModel.NextOfKin);
                        }


                        if (viewModel.FirstSittingOLevelResult == null || viewModel.FirstSittingOLevelResult.Id <= 0)
                        {
                            viewModel.FirstSittingOLevelResult.Person = viewModel.Person;
                            viewModel.FirstSittingOLevelResult.PersonType = personType;
                            viewModel.FirstSittingOLevelResult.Sitting = new OLevelExamSitting() { Id = 1 };
                        }

                        if (viewModel.SecondSittingOLevelResult == null || viewModel.SecondSittingOLevelResult.Id <= 0)
                        {
                            viewModel.SecondSittingOLevelResult.Person = viewModel.Person;
                            viewModel.SecondSittingOLevelResult.PersonType = personType;
                            viewModel.SecondSittingOLevelResult.Sitting = new OLevelExamSitting() { Id = 2 };
                        }
                      
                        ModifyOlevelResult(viewModel.FirstSittingOLevelResult, viewModel.FirstSittingOLevelResultDetails);
                        ModifyOlevelResult(viewModel.SecondSittingOLevelResult, viewModel.SecondSittingOLevelResultDetails);
                        
                        viewModel.StudentAcademicInformation.Student = newStudent;
                        StudentAcademicInformationLogic academicInformationLogic = new StudentAcademicInformationLogic();
                        if (academicInformationLogic.GetModelBy(a => a.Person_Id == newStudent.Id) != null)
                        {
                            academicInformationLogic.Modify(viewModel.StudentAcademicInformation);
                        }
                        else
                        {
                            academicInformationLogic.Create(viewModel.StudentAcademicInformation);
                        }


                        viewModel.StudentFinanceInformation.Student = newStudent;
                        StudentFinanceInformationLogic financeInformationLogic = new StudentFinanceInformationLogic();
                        if (financeInformationLogic.GetModelsBy(a => a.Person_Id == newStudent.Id).FirstOrDefault() != null)
                        {
                            financeInformationLogic.Modify(viewModel.StudentFinanceInformation);
                        }
                        else
                        {
                            financeInformationLogic.Create(viewModel.StudentFinanceInformation);
                        }
                        


                        if (viewModel.StudentLevel.Programme.Id == 3 || viewModel.StudentLevel.Programme.Id == 4)
                        {
                            ITDuration duration = new ITDuration() { Id = 1 };
                            EducationalQualification qualification = new EducationalQualification() { Id = 45 };
                            viewModel.PreviousEducation.Person = newStudent;
                            viewModel.PreviousEducation.PersonType = personType;
                            viewModel.PreviousEducation.ITDuration = duration;
                            viewModel.PreviousEducation.Qualification = qualification;
                            PreviousEducationLogic previousEducationLogic = new PreviousEducationLogic();
                            if (previousEducationLogic.GetModelsBy(a => a.Person_Id == newStudent.Id).FirstOrDefault() != null)
                            {
                                previousEducationLogic.Modify(viewModel.PreviousEducation);
                            }
                            else
                            {
                                previousEducationLogic.Create(viewModel.PreviousEducation);
                            }
                            

                            viewModel.StudentEmploymentInformation.Student = newStudent;
                            StudentEmploymentInformationLogic employmentInformationLogic = new StudentEmploymentInformationLogic();
                            if (employmentInformationLogic.GetModelsBy(a => a.Person_Id == newStudent.Id).FirstOrDefault() != null)
                            {
                                employmentInformationLogic.Modify(viewModel.StudentEmploymentInformation);                            
                            }
                            else
                            {
                                employmentInformationLogic.Create(viewModel.StudentEmploymentInformation);
                            
                            }
                            
                            viewModel.StudentNdResult.Student = newStudent;
                            StudentNdResultLogic ndResultLogic = new StudentNdResultLogic();
                            if (ndResultLogic.GetModelsBy(a => a.Person_Id == newStudent.Id).FirstOrDefault() != null)
                            {
                                ndResultLogic.Modify(viewModel.StudentNdResult);
                            }
                            else
                            {
                                ndResultLogic.Create(viewModel.StudentNdResult);
                            }
                            
                        }

                        
                        string junkFilePath;
                        string destinationFilePath;
                        SetPersonPassportDestination(viewModel, out junkFilePath, out destinationFilePath);
                        SetPersonSignatureDestination(viewModel, out junkFilePath, out destinationFilePath);
                        PersonLogic personLogic = new PersonLogic();
                        bool personModified = personLogic.Modify(viewModel.Person);
                        if (personModified)
                        {
                            SavePersonPassport(junkFilePath, destinationFilePath, viewModel.Person);
                            SavePersonSignature(junkFilePath, destinationFilePath, viewModel.Person);
                            transaction.Complete();
                        }
                        else
                        {
                            throw new Exception("Passport/Signature save operation failed! Please try again.");
                        }
                        
                        //transaction.Complete();
                    }
                }
                else
                {
                    using (TransactionScope transaction = new TransactionScope())
                    {
                        viewModel.Student.Id = viewModel.Person.Id;
                        viewModel.Student.Status = new StudentStatus() { Id = 1 };
                        StudentLogic studentLogic = new StudentLogic();

                        newStudent = viewModel.Student;
                        studentLogic.Modify(viewModel.Student);


                        viewModel.StudentSponsor.Student = newStudent;
                        StudentSponsorLogic sponsorLogic = new StudentSponsorLogic();
                        sponsorLogic.Modify(viewModel.StudentSponsor);


                        viewModel.NextOfKin.Person = newStudent;
                        viewModel.NextOfKin.PersonType = new PersonType() { Id = (int)PersonType.EnumName.Student };
                        NextOfKinLogic nextOfKinLogic = new NextOfKinLogic();
                        var existingNextOfKin=nextOfKinLogic.GetModelsBy(f => f.Person_Id == newStudent.Id).FirstOrDefault();
                        if (existingNextOfKin != null)
                        {
                            nextOfKinLogic.Modify(viewModel.NextOfKin);
                            
                        }
                        else
                        {

                            nextOfKinLogic.Create(viewModel.NextOfKin);
                        }
                        


                        if (viewModel.FirstSittingOLevelResult == null || viewModel.FirstSittingOLevelResult.Id <= 0)
                        {
                            viewModel.FirstSittingOLevelResult.Person = viewModel.NextOfKin.Person;
                            viewModel.FirstSittingOLevelResult.Sitting = new OLevelExamSitting() { Id = 1 };
                        }

                        if (viewModel.SecondSittingOLevelResult == null || viewModel.SecondSittingOLevelResult.Id <= 0)
                        {
                            viewModel.SecondSittingOLevelResult.Person = viewModel.NextOfKin.Person;
                            viewModel.SecondSittingOLevelResult.Sitting = new OLevelExamSitting() { Id = 2 };
                        }
                        ModifyOlevelResult(viewModel.FirstSittingOLevelResult, viewModel.FirstSittingOLevelResultDetails);
                        ModifyOlevelResult(viewModel.SecondSittingOLevelResult, viewModel.SecondSittingOLevelResultDetails);


                        viewModel.StudentAcademicInformation.Student = newStudent;
                        StudentAcademicInformationLogic academicInformationLogic = new StudentAcademicInformationLogic();
                        academicInformationLogic.Modify(viewModel.StudentAcademicInformation);

                        viewModel.StudentFinanceInformation.Student = newStudent;
                        StudentFinanceInformationLogic financeInformationLogic = new StudentFinanceInformationLogic();
                        financeInformationLogic.Modify(viewModel.StudentFinanceInformation);
                     
                        if (viewModel.StudentLevel.Programme.Id == 3 || viewModel.StudentLevel.Programme.Id == 4)
                        {
                            viewModel.PreviousEducation.Person = newStudent;
                            viewModel.PreviousEducation.PersonType = personType;
                            PreviousEducationLogic previousEducationLogic = new PreviousEducationLogic();
                            previousEducationLogic.Modify(viewModel.PreviousEducation);

                            viewModel.StudentEmploymentInformation.Student = newStudent;
                            StudentEmploymentInformationLogic employmentInformationLogic = new StudentEmploymentInformationLogic();
                            employmentInformationLogic.Modify(viewModel.StudentEmploymentInformation);

                        }

                      
                        PersonLogic personLogic = new PersonLogic();
                        bool personModified = personLogic.Modify(viewModel.Person);
                        transaction.Complete();
                    }
                }
                TempData["RegistrationViewModel"] = viewModel;
                return RedirectToAction("AcknowledgementSlip", "Registration");
            }
            catch (Exception ex)
            {
                newStudent = null;
                SetMessage("Error Occurred! " + ex.Message  , Message.Category.Error);
            }

            TempData["RegistrationViewModel"] = viewModel;
            return View(viewModel);
        }

        private void SaveOLevelResult(Person person, OLevelResult oLevelResult, List<OLevelResultDetail> oLevelResultDetails, PersonType personType)
        {
            try
            {
                OLevelResultLogic oLevelResultLogic = new OLevelResultLogic();
                OLevelResultDetailLogic oLevelResultDetailLogic = new OLevelResultDetailLogic();
                if (oLevelResult != null && oLevelResult.ExamNumber != null && oLevelResult.Type != null && oLevelResult.ExamYear > 0)
                {
                    //oLevelResult.ApplicationForm = applicationForm;
                    oLevelResult.Person = person;
                    oLevelResult.PersonType = personType;
                    //oLevelResult.Sitting = new OLevelExamSitting() { Id = 1 };

                    OLevelResult firstSittingOLevelResult = oLevelResultLogic.Create(oLevelResult);

                    if (oLevelResultDetails != null && oLevelResultDetails.Count > 0 && firstSittingOLevelResult != null)
                    {
                        List<OLevelResultDetail> olevelResultDetails = oLevelResultDetails.Where(m => m.Grade != null && m.Subject != null).ToList();
                        foreach (OLevelResultDetail oLevelResultDetail in olevelResultDetails)
                        {
                            oLevelResultDetail.Header = firstSittingOLevelResult;
                        }

                        oLevelResultDetailLogic.Create(olevelResultDetails);
                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
        }

        private void SavePersonPassport(string junkFilePath, string pathForSaving, Person person)
        {
            try
            {
                string folderPath = Path.GetDirectoryName(pathForSaving);
                string mainFileName = person.Id.ToString() + "__";

                DeleteFileIfExist(folderPath, mainFileName);

                System.IO.File.Move(junkFilePath, pathForSaving);
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void SavePersonSignature(string junkFilePath, string pathForSaving, Person person)
        {
            try
            {
                string folderPath = Path.GetDirectoryName(pathForSaving);
                string mainFileName = person.Id.ToString() + "__";

                DeleteFileIfExist(folderPath, mainFileName);

                System.IO.File.Move(junkFilePath, pathForSaving);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void SetPersonPassportDestination(RegistrationViewModel viewModel, out string junkFilePath, out string destinationFilePath)
        {
            const string TILDA = "~";

            try
            {
                string passportUrl = viewModel.Person.ImageFileUrl;
                junkFilePath = Server.MapPath(TILDA + viewModel.Person.ImageFileUrl);
                destinationFilePath = junkFilePath.Replace("Junk", "Student");
                viewModel.Person.ImageFileUrl = passportUrl.Replace("Junk", "Student");
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void SetPersonSignatureDestination(RegistrationViewModel viewModel, out string junkFilePath, out string destinationFilePath)
        {
            const string TILDA = "~";

            try
            {
                string signatureUrl = viewModel.Person.SignatureFileUrl;
                junkFilePath = Server.MapPath(TILDA + viewModel.Person.SignatureFileUrl);
                destinationFilePath = junkFilePath.Replace("Junk", "Student");
                viewModel.Person.SignatureFileUrl = signatureUrl.Replace("Junk", "Student");
            }
            catch (Exception)
            {
                throw;
            }
        }
        public ActionResult AcknowledgementSlip()
        {
            RegistrationViewModel existingViewModel = (RegistrationViewModel)TempData["RegistrationViewModel"];

            TempData["RegistrationViewModel"] = existingViewModel;
            return View(existingViewModel);
        }

        private void UpdateOLevelResultDetail(RegistrationViewModel viewModel)
        {
            try
            {
                if (viewModel != null && viewModel.FirstSittingOLevelResultDetails != null && viewModel.FirstSittingOLevelResultDetails.Count > 0)
                {
                    foreach (OLevelResultDetail firstSittingOLevelResultDetail in viewModel.FirstSittingOLevelResultDetails)
                    {
                        if (firstSittingOLevelResultDetail.Subject != null)
                        {
                            firstSittingOLevelResultDetail.Subject = viewModel.OLevelSubjects.Where(m => m.Id == firstSittingOLevelResultDetail.Subject.Id).SingleOrDefault();
                        }
                        if (firstSittingOLevelResultDetail.Grade != null)
                        {
                            firstSittingOLevelResultDetail.Grade = viewModel.OLevelGrades.Where(m => m.Id == firstSittingOLevelResultDetail.Grade.Id).SingleOrDefault();
                        }
                    }
                }

                if (viewModel != null && viewModel.SecondSittingOLevelResultDetails != null && viewModel.SecondSittingOLevelResultDetails.Count > 0)
                {
                    foreach (OLevelResultDetail secondSittingOLevelResultDetail in viewModel.SecondSittingOLevelResultDetails)
                    {
                        if (secondSittingOLevelResultDetail.Subject != null)
                        {
                            secondSittingOLevelResultDetail.Subject = viewModel.OLevelSubjects.Where(m => m.Id == secondSittingOLevelResultDetail.Subject.Id).SingleOrDefault();
                        }
                        if (secondSittingOLevelResultDetail.Grade != null)
                        {
                            secondSittingOLevelResultDetail.Grade = viewModel.OLevelGrades.Where(m => m.Id == secondSittingOLevelResultDetail.Grade.Id).SingleOrDefault();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
        }

        private void SetStudentUploadedPassport(RegistrationViewModel viewModel)
        {
            if (viewModel != null && viewModel.Person != null && !string.IsNullOrEmpty((string)TempData["imageUrl"]))
            {
                viewModel.Person.ImageFileUrl = (string)TempData["imageUrl"];
            }
            else
            {
                viewModel.Person.ImageFileUrl = Utility.DEFAULT_AVATAR;
            }
        }
        private void SetStudentUploadedSignature(RegistrationViewModel viewModel)
        {
            if (viewModel != null && viewModel.Person != null && !string.IsNullOrEmpty((string)TempData["SignatureFile"]))
            {
                viewModel.Person.SignatureFileUrl = (string)TempData["SignatureFile"];
            }
            else
            {
                viewModel.Person.SignatureFileUrl = Utility.DEFAULT_SIGNATURE;
            }
        }
        private void PopulateAllDropDowns(int programmeId)
        {
            RegistrationViewModel existingViewModel = (RegistrationViewModel)TempData["RegistrationViewModel"];

            try
            {
                if (existingViewModel == null)
                {
                    viewModel = new RegistrationViewModel();

                    ViewBag.States = viewModel.StateSelectList;
                    ViewBag.Sexes = viewModel.SexSelectList;
                    ViewBag.FirstChoiceFaculties = viewModel.FacultySelectList;
                    ViewBag.SecondChoiceFaculties = viewModel.FacultySelectList;
                    ViewBag.Lgas = new SelectList(new List<LocalGovernment>(), Utility.ID, Utility.NAME);
                    ViewBag.Relationships = viewModel.RelationshipSelectList;
                    ViewBag.FirstSittingOLevelTypes = viewModel.OLevelTypeSelectList;
                    ViewBag.SecondSittingOLevelTypes = viewModel.OLevelTypeSelectList;
                    ViewBag.FirstSittingExamYears = viewModel.ExamYearSelectList;
                    ViewBag.SecondSittingExamYears = viewModel.ExamYearSelectList;
                    ViewBag.Religions = viewModel.ReligionSelectList;
                    ViewBag.Abilities = viewModel.AbilitySelectList;
                    ViewBag.DayOfBirths = new SelectList(new List<Value>(), Utility.ID, Utility.NAME);
                    ViewBag.MonthOfBirths = viewModel.MonthOfBirthSelectList;
                    ViewBag.YearOfBirths = viewModel.YearOfBirthSelectList;
                    ViewBag.Titles = viewModel.TitleSelectList;
                    ViewBag.MaritalStatuses = viewModel.MaritalStatusSelectList;
                    ViewBag.BloodGroups = viewModel.BloodGroupSelectList;
                    ViewBag.Genotypes = viewModel.GenotypeSelectList;
                    ViewBag.ModeOfEntries = viewModel.ModeOfEntrySelectList;
                    ViewBag.ModeOfStudies = viewModel.ModeOfStudySelectList;
                    ViewBag.StudentCategories = viewModel.StudentCategorySelectList;
                    ViewBag.StudentTypes = viewModel.StudentTypeSelectList;
                    ViewBag.Levels = viewModel.LevelSelectList;
                    ViewBag.ModeOfFinances = viewModel.ModeOfFinanceSelectList;
                    ViewBag.Relationships = viewModel.RelationshipSelectList;
                    ViewBag.Faculties = viewModel.FacultySelectList;
                    ViewBag.AdmissionYears = viewModel.AdmissionYearSelectList;
                    ViewBag.GraduationYears = viewModel.GraduationYearSelectList;
                    ViewBag.Programmes = viewModel.ProgrammeSelectList;



                    if (viewModel.DepartmentSelectList != null)
                    {
                        ViewBag.Departments = viewModel.DepartmentSelectList;
                    }
                    else
                    {
                        ViewBag.Departments = new SelectList(new List<Department>(), Utility.ID, Utility.NAME);
                    }

                    if (programmeId == 3 || programmeId == 4)
                    {
                        ViewBag.StudentNdResultDayAwardeds = new SelectList(new List<Value>(), Utility.ID, Utility.NAME);
                        ViewBag.StudentNdResultMonthAwardeds = viewModel.StudentNdResultMonthAwardedSelectList;
                        ViewBag.StudentNdResultYearAwardeds = viewModel.StudentNdResultYearAwardedSelectList;

                        ViewBag.StudentLastEmploymentStartDays = new SelectList(new List<Value>(), Utility.ID, Utility.NAME);
                        ViewBag.StudentLastEmploymentStartMonths = viewModel.StudentLastEmploymentStartMonthSelectList;
                        ViewBag.StudentLastEmploymentStartYears = viewModel.StudentLastEmploymentStartYearSelectList;

                        ViewBag.StudentLastEmploymentEndDays = new SelectList(new List<Value>(), Utility.ID, Utility.NAME);
                        ViewBag.StudentLastEmploymentEndMonths = viewModel.StudentLastEmploymentEndMonthSelectList;
                        ViewBag.StudentLastEmploymentEndYears = viewModel.StudentLastEmploymentEndYearSelectList;

                        ViewBag.PreviousEducationStartDays = new SelectList(new List<Value>(), Utility.ID, Utility.NAME);
                        ViewBag.PreviousEducationStartMonths = viewModel.PreviousEducationStartMonthSelectList;
                        ViewBag.PreviousEducationStartYears = viewModel.PreviousEducationStartYearSelectList;

                        ViewBag.PreviousEducationEndDays = new SelectList(new List<Value>(), Utility.ID, Utility.NAME);
                        ViewBag.PreviousEducationEndMonths = viewModel.PreviousEducationEndMonthSelectList;
                        ViewBag.PreviousEducationEndYears = viewModel.PreviousEducationEndYearSelectList;

                        ViewBag.ResultGrades = viewModel.ResultGradeSelectList;
                    }

                    SetDefaultSelectedSittingSubjectAndGrade(viewModel);
                }
                else
                {
                    if (existingViewModel.Student.Title == null) { existingViewModel.Student.Title = new Title(); }
                    if (existingViewModel.Person.Sex == null) { existingViewModel.Person.Sex = new Sex(); }
                    if (existingViewModel.Student.MaritalStatus == null) { existingViewModel.Student.MaritalStatus = new MaritalStatus(); }
                    if (existingViewModel.Person.Religion == null) { existingViewModel.Person.Religion = new Religion(); }
                    if (existingViewModel.Person.State == null) { existingViewModel.Person.State = new State(); }
                    if (existingViewModel.StudentLevel.Programme == null) { existingViewModel.StudentLevel.Programme = new Programme(); }
                    if (existingViewModel.NextOfKin.Relationship == null) { existingViewModel.NextOfKin.Relationship = new Relationship(); }
                    if (existingViewModel.StudentSponsor.Relationship == null) { existingViewModel.StudentSponsor.Relationship = new Relationship(); }
                    if (existingViewModel.FirstSittingOLevelResult.Type == null) { existingViewModel.FirstSittingOLevelResult.Type = new OLevelType(); }
                    if (existingViewModel.Person.YearOfBirth == null) { existingViewModel.Person.YearOfBirth = new Value(); }
                    if (existingViewModel.Person.MonthOfBirth == null) { existingViewModel.Person.MonthOfBirth = new Value(); }
                    if (existingViewModel.Person.DayOfBirth == null) { existingViewModel.Person.DayOfBirth = new Value(); }
                    if (existingViewModel.StudentLevel.Department == null) { existingViewModel.StudentLevel.Department = new Department(); }
                    if (existingViewModel.Student.BloodGroup == null) { existingViewModel.Student.BloodGroup = new BloodGroup(); }
                    if (existingViewModel.Student.Genotype == null) { existingViewModel.Student.Genotype = new Genotype(); }
                    if (existingViewModel.StudentAcademicInformation.Level == null) { existingViewModel.StudentAcademicInformation.Level = new Level(); }
                    if (existingViewModel.StudentFinanceInformation.Mode == null) { existingViewModel.StudentFinanceInformation.Mode = new ModeOfFinance(); }
                    

                    // PERSONAL INFORMATION
                    ViewBag.Titles = new SelectList(existingViewModel.TitleSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.Student.Title.Id);
                    ViewBag.Sexes = new SelectList(existingViewModel.SexSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.Person.Sex.Id);
                    ViewBag.MaritalStatuses = new SelectList(existingViewModel.MaritalStatusSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.Student.MaritalStatus.Id);
                    SetDateOfBirthDropDown(existingViewModel);
                    ViewBag.Religions = new SelectList(existingViewModel.ReligionSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.Person.Religion.Id);
                    ViewBag.States = new SelectList(existingViewModel.StateSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.Person.State.Id);

                    if (existingViewModel.Person.LocalGovernment != null && existingViewModel.Person.LocalGovernment.Id > 0)
                    {
                        ViewBag.Lgas = new SelectList(existingViewModel.LocalGovernmentSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.Person.LocalGovernment.Id);
                    }
                    else
                    {
                        ViewBag.Lgas = new SelectList(new List<LocalGovernment>(), Utility.VALUE, Utility.TEXT);
                    }
                    ViewBag.BloodGroups = new SelectList(existingViewModel.BloodGroupSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.Student.BloodGroup.Id);
                    ViewBag.Genotypes = new SelectList(existingViewModel.GenotypeSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.Student.Genotype.Id);



                    // ACADEMIC DETAILS
                    ViewBag.ModeOfEntries = new SelectList(existingViewModel.ModeOfEntrySelectList, Utility.VALUE, Utility.TEXT, existingViewModel.StudentAcademicInformation.ModeOfEntry.Id);
                    ViewBag.ModeOfStudies = new SelectList(existingViewModel.ModeOfStudySelectList, Utility.VALUE, Utility.TEXT, existingViewModel.StudentAcademicInformation.ModeOfStudy.Id);
                    ViewBag.Programmes = new SelectList(existingViewModel.ProgrammeSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.StudentLevel.Programme.Id);
                    ViewBag.Faculties = new SelectList(existingViewModel.FacultySelectList, Utility.VALUE, Utility.TEXT, existingViewModel.StudentLevel.Department.Faculty.Id);

                    SetDepartmentIfExist(existingViewModel);
                    SetDepartmentOptionIfExist(existingViewModel);

                    ViewBag.StudentTypes = new SelectList(existingViewModel.StudentTypeSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.Student.Type.Id);
                    ViewBag.StudentCategories = new SelectList(existingViewModel.StudentCategorySelectList, Utility.VALUE, Utility.TEXT, existingViewModel.Student.Category.Id);
                    ViewBag.AdmissionYears = new SelectList(existingViewModel.AdmissionYearSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.StudentAcademicInformation.YearOfAdmission);
                    ViewBag.GraduationYears = new SelectList(existingViewModel.GraduationYearSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.StudentAcademicInformation.YearOfGraduation);

                    if (existingViewModel.StudentAcademicInformation.Level != null && existingViewModel.StudentAcademicInformation.Level.Id > 0)
                    {
                        ViewBag.Levels = new SelectList(existingViewModel.LevelSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.StudentAcademicInformation.Level.Id);
                    }
                    else if (existingViewModel.StudentLevel.Level != null && existingViewModel.StudentLevel.Level.Id > 0)
                    {
                        ViewBag.Levels = new SelectList(existingViewModel.LevelSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.StudentLevel.Level.Id);
                    }

                    // FINANCE DETAILS
                    ViewBag.ModeOfFinances = new SelectList(existingViewModel.ModeOfFinanceSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.StudentFinanceInformation.Mode.Id);

                    // NEXT OF KIN
                    ViewBag.Relationships = new SelectList(existingViewModel.RelationshipSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.StudentSponsor.Relationship.Id);
                   
                    //SPONSOR
                    ViewBag.Relationships = new SelectList(existingViewModel.RelationshipSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.NextOfKin.Relationship.Id);


                    ViewBag.FirstSittingOLevelTypes = new SelectList(existingViewModel.OLevelTypeSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.FirstSittingOLevelResult.Type.Id);
                    ViewBag.FirstSittingExamYears = new SelectList(existingViewModel.ExamYearSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.FirstSittingOLevelResult.ExamYear);
                    ViewBag.SecondSittingExamYears = new SelectList(existingViewModel.ExamYearSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.SecondSittingOLevelResult.ExamYear);

                    if (programmeId == 3 || programmeId == 4)
                    {
                        SetStudentNdResultDateAwardedDropDown(existingViewModel);
                        SetStudentLastEmploymentEndDateDropDown(existingViewModel);
                        SetStudentLastEmploymentStartDateDropDown(existingViewModel);
                        SetPreviousEducationEndDateDropDowns(existingViewModel);
                        SetPreviousEducationStartDateDropDowns(existingViewModel);

                        ViewBag.ResultGrades = new SelectList(existingViewModel.ResultGradeSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.PreviousEducation.ResultGrade.Id);
                    }
                    
                    if (existingViewModel.SecondSittingOLevelResult.Type != null)
                    {
                        ViewBag.SecondSittingOLevelTypes = new SelectList(existingViewModel.OLevelTypeSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.SecondSittingOLevelResult.Type.Id);
                    }
                    else
                    {
                        existingViewModel.SecondSittingOLevelResult.Type = new OLevelType() { Id = 0 };
                        ViewBag.SecondSittingOLevelTypes = new SelectList(existingViewModel.OLevelTypeSelectList, Utility.VALUE, Utility.TEXT, 0);
                    }

                    SetSelectedSittingSubjectAndGrade(existingViewModel);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
        }
        
        private void SetStudentLastEmploymentStartDateDropDown(RegistrationViewModel existingViewModel)
        {
            try
            {
                ViewBag.StudentLastEmploymentStartMonths = new SelectList(existingViewModel.StudentLastEmploymentStartMonthSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.StudentEmploymentInformation.StartMonth.Id);
                ViewBag.StudentLastEmploymentStartYears = new SelectList(existingViewModel.StudentLastEmploymentStartYearSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.StudentEmploymentInformation.StartYear.Id);
                
                if ((existingViewModel.StudentLastEmploymentStartDaySelectList == null || existingViewModel.StudentLastEmploymentStartDaySelectList.Count == 0) && (existingViewModel.StudentEmploymentInformation.StartMonth.Id > 0 && existingViewModel.StudentEmploymentInformation.StartYear.Id > 0))
                {
                    existingViewModel.StudentLastEmploymentStartDaySelectList = Utility.PopulateDaySelectListItem(existingViewModel.StudentEmploymentInformation.StartMonth, existingViewModel.StudentEmploymentInformation.StartYear);
                }
                else
                {
                    if (existingViewModel.StudentLastEmploymentStartDaySelectList != null && existingViewModel.StudentEmploymentInformation.StartDay.Id > 0)
                    {
                        ViewBag.StudentLastEmploymentStartDays = new SelectList(existingViewModel.StudentLastEmploymentStartDaySelectList, Utility.VALUE, Utility.TEXT, existingViewModel.StudentEmploymentInformation.StartDay.Id);
                    }
                    else if (existingViewModel.StudentLastEmploymentStartDaySelectList != null && existingViewModel.StudentEmploymentInformation.StartDay.Id <= 0)
                    {
                        ViewBag.StudentLastEmploymentStartDays = existingViewModel.StudentLastEmploymentStartDaySelectList;
                    }
                    else if (existingViewModel.StudentLastEmploymentStartDaySelectList == null)
                    {
                        existingViewModel.StudentLastEmploymentStartDaySelectList = new List<SelectListItem>();
                        ViewBag.StudentLastEmploymentStartDays = new List<SelectListItem>();
                    }
                }

                if (existingViewModel.StudentEmploymentInformation.StartDay != null && existingViewModel.StudentEmploymentInformation.StartDay.Id > 0)
                {
                    ViewBag.StudentLastEmploymentStartDays = new SelectList(existingViewModel.StudentLastEmploymentStartDaySelectList, Utility.VALUE, Utility.TEXT, existingViewModel.StudentEmploymentInformation.StartDay.Id);
                }
                else
                {
                    ViewBag.StudentLastEmploymentStartDays = existingViewModel.StudentLastEmploymentStartDaySelectList;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void SetStudentLastEmploymentEndDateDropDown(RegistrationViewModel existingViewModel)
        {
            try
            {
                ViewBag.StudentLastEmploymentEndMonths = new SelectList(existingViewModel.StudentLastEmploymentEndMonthSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.StudentEmploymentInformation.EndMonth.Id);
                ViewBag.StudentLastEmploymentEndYears = new SelectList(existingViewModel.StudentLastEmploymentEndYearSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.StudentEmploymentInformation.EndYear.Id);

                if ((existingViewModel.StudentLastEmploymentEndDaySelectList == null || existingViewModel.StudentLastEmploymentEndDaySelectList.Count == 0) && (existingViewModel.StudentEmploymentInformation.EndMonth.Id > 0 && existingViewModel.StudentEmploymentInformation.EndYear.Id > 0))
                {
                    existingViewModel.StudentLastEmploymentEndDaySelectList = Utility.PopulateDaySelectListItem(existingViewModel.StudentEmploymentInformation.EndMonth, existingViewModel.StudentEmploymentInformation.EndYear);
                }
                else
                {
                    if (existingViewModel.StudentLastEmploymentEndDaySelectList != null && existingViewModel.StudentEmploymentInformation.EndDay.Id > 0)
                    {
                        ViewBag.StudentLastEmploymentEndDays = new SelectList(existingViewModel.StudentLastEmploymentEndDaySelectList, Utility.VALUE, Utility.TEXT, existingViewModel.StudentEmploymentInformation.EndDay.Id);
                    }
                    else if (existingViewModel.StudentLastEmploymentEndDaySelectList != null && existingViewModel.StudentEmploymentInformation.EndDay.Id <= 0)
                    {
                        ViewBag.StudentLastEmploymentEndDays = existingViewModel.StudentLastEmploymentEndDaySelectList;
                    }
                    else if (existingViewModel.StudentLastEmploymentEndDaySelectList == null)
                    {
                        existingViewModel.StudentLastEmploymentEndDaySelectList = new List<SelectListItem>();
                        ViewBag.StudentLastEmploymentEndDays = new List<SelectListItem>();
                    }
                }

                if (existingViewModel.StudentEmploymentInformation.EndDay != null && existingViewModel.StudentEmploymentInformation.EndDay.Id > 0)
                {
                    ViewBag.StudentLastEmploymentEndDays = new SelectList(existingViewModel.StudentLastEmploymentEndDaySelectList, Utility.VALUE, Utility.TEXT, existingViewModel.StudentEmploymentInformation.EndDay.Id);
                }
                else
                {
                    ViewBag.StudentLastEmploymentEndDays = existingViewModel.StudentLastEmploymentEndDaySelectList;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        private void SetStudentNdResultDateAwardedDropDown(RegistrationViewModel existingViewModel)
        {
            try
            {
                ViewBag.StudentNdResultMonthAwardeds = new SelectList(existingViewModel.StudentNdResultMonthAwardedSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.StudentNdResult.MonthAwarded.Id);
                ViewBag.StudentNdResultYearAwardeds = new SelectList(existingViewModel.StudentNdResultYearAwardedSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.StudentNdResult.YearAwarded.Id);
                if ((existingViewModel.StudentNdResultDayAwardedSelectList == null || existingViewModel.StudentNdResultDayAwardedSelectList.Count == 0) && (existingViewModel.StudentNdResult.MonthAwarded.Id > 0 && existingViewModel.StudentNdResult.YearAwarded.Id > 0))
                {
                    existingViewModel.StudentNdResultDayAwardedSelectList = Utility.PopulateDaySelectListItem(existingViewModel.StudentNdResult.MonthAwarded, existingViewModel.StudentNdResult.YearAwarded);
                }
                else
                {
                    if (existingViewModel.StudentNdResultDayAwardedSelectList != null && existingViewModel.StudentNdResult.DayAwarded.Id > 0)
                    {
                        ViewBag.StudentNdResultDayAwardeds = new SelectList(existingViewModel.StudentNdResultDayAwardedSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.StudentNdResult.DayAwarded.Id);
                    }
                    else if (existingViewModel.StudentNdResultDayAwardedSelectList != null && existingViewModel.StudentNdResult.DayAwarded.Id <= 0)
                    {
                        ViewBag.StudentNdResultDayAwardeds = existingViewModel.StudentNdResultDayAwardedSelectList;
                    }
                    else if (existingViewModel.StudentNdResultDayAwardedSelectList == null)
                    {
                        existingViewModel.StudentNdResultDayAwardedSelectList = new List<SelectListItem>();
                        ViewBag.StudentNdResultDayAwardeds = new List<SelectListItem>();
                    }
                }

                if (existingViewModel.StudentNdResult.DayAwarded != null && existingViewModel.StudentNdResult.DayAwarded.Id > 0)
                {
                    ViewBag.StudentNdResultDayAwardeds = new SelectList(existingViewModel.StudentNdResultDayAwardedSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.StudentNdResult.DayAwarded.Id);
                }
                else
                {
                    ViewBag.StudentNdResultDayAwardeds = existingViewModel.StudentNdResultDayAwardedSelectList;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void SetDateOfBirthDropDown(RegistrationViewModel existingViewModel)
        {
            try
            {
                ViewBag.MonthOfBirths = new SelectList(existingViewModel.MonthOfBirthSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.Person.MonthOfBirth.Id);
                ViewBag.YearOfBirths = new SelectList(existingViewModel.YearOfBirthSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.Person.YearOfBirth.Id);
                if ((existingViewModel.DayOfBirthSelectList == null || existingViewModel.DayOfBirthSelectList.Count == 0) && (existingViewModel.Person.MonthOfBirth.Id > 0 && existingViewModel.Person.YearOfBirth.Id > 0))
                {
                    existingViewModel.DayOfBirthSelectList = Utility.PopulateDaySelectListItem(existingViewModel.Person.MonthOfBirth, existingViewModel.Person.YearOfBirth);
                }
                else
                {
                    if (existingViewModel.DayOfBirthSelectList != null && existingViewModel.Person.DayOfBirth.Id > 0)
                    {
                        ViewBag.DayOfBirths = new SelectList(existingViewModel.DayOfBirthSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.Person.DayOfBirth.Id);
                    }
                    else if (existingViewModel.DayOfBirthSelectList != null && existingViewModel.Person.DayOfBirth.Id <= 0)
                    {
                        ViewBag.DayOfBirths = existingViewModel.DayOfBirthSelectList;
                    }
                    else if (existingViewModel.DayOfBirthSelectList == null)
                    {
                        existingViewModel.DayOfBirthSelectList = new List<SelectListItem>();
                        ViewBag.DayOfBirths = new List<SelectListItem>();
                    }
                }

                if (existingViewModel.Person.DayOfBirth != null && existingViewModel.Person.DayOfBirth.Id > 0)
                {
                    ViewBag.DayOfBirths = new SelectList(existingViewModel.DayOfBirthSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.Person.DayOfBirth.Id);
                }
                else
                {
                    ViewBag.DayOfBirths = existingViewModel.DayOfBirthSelectList;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void SetPreviousEducationStartDateDropDowns(RegistrationViewModel existingViewModel)
        {
            try
            {
                ViewBag.PreviousEducationStartMonths = new SelectList(existingViewModel.PreviousEducationStartMonthSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.PreviousEducation.StartMonth.Id);
                ViewBag.PreviousEducationStartYears = new SelectList(existingViewModel.PreviousEducationStartYearSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.PreviousEducation.StartYear.Id);
                if ((existingViewModel.PreviousEducationStartDaySelectList == null || existingViewModel.PreviousEducationStartDaySelectList.Count == 0) && (existingViewModel.PreviousEducation.StartMonth.Id > 0 && existingViewModel.PreviousEducation.StartYear.Id > 0))
                {
                    existingViewModel.PreviousEducationStartDaySelectList = Utility.PopulateDaySelectListItem(existingViewModel.PreviousEducation.StartMonth, existingViewModel.PreviousEducation.StartYear);
                }
                else
                {
                    if (existingViewModel.PreviousEducationStartDaySelectList != null && existingViewModel.PreviousEducation.StartDay.Id > 0)
                    {
                        ViewBag.PreviousEducationStartDays = new SelectList(existingViewModel.PreviousEducationStartDaySelectList, Utility.VALUE, Utility.TEXT, existingViewModel.PreviousEducation.StartDay.Id);
                    }
                    else if (existingViewModel.PreviousEducationStartDaySelectList != null && existingViewModel.PreviousEducation.StartDay.Id <= 0)
                    {
                        ViewBag.PreviousEducationStartDays = existingViewModel.PreviousEducationStartDaySelectList;
                    }
                    else if (existingViewModel.PreviousEducationStartDaySelectList == null)
                    {
                        existingViewModel.PreviousEducationStartDaySelectList = new List<SelectListItem>();
                        ViewBag.PreviousEducationStartDays = new List<SelectListItem>();
                    }
                }

                if (existingViewModel.PreviousEducation.StartDay != null && existingViewModel.PreviousEducation.StartDay.Id > 0)
                {
                    ViewBag.PreviousEducationStartDays = new SelectList(existingViewModel.PreviousEducationStartDaySelectList, Utility.VALUE, Utility.TEXT, existingViewModel.PreviousEducation.StartDay.Id);
                }
                else
                {
                    ViewBag.PreviousEducationStartDays = existingViewModel.PreviousEducationStartDaySelectList;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void SetPreviousEducationEndDateDropDowns(RegistrationViewModel existingViewModel)
        {
            try
            {
                ViewBag.PreviousEducationEndMonths = new SelectList(existingViewModel.PreviousEducationEndMonthSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.PreviousEducation.EndMonth.Id);
                ViewBag.PreviousEducationEndYears = new SelectList(existingViewModel.PreviousEducationEndYearSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.PreviousEducation.EndYear.Id);
                if ((existingViewModel.PreviousEducationEndDaySelectList == null || existingViewModel.PreviousEducationEndDaySelectList.Count == 0) && (existingViewModel.PreviousEducation.EndMonth.Id > 0 && existingViewModel.PreviousEducation.EndYear.Id > 0))
                {
                    existingViewModel.PreviousEducationEndDaySelectList = Utility.PopulateDaySelectListItem(existingViewModel.PreviousEducation.EndMonth, existingViewModel.PreviousEducation.EndYear);
                }
                else
                {
                    if (existingViewModel.PreviousEducationEndDaySelectList != null && existingViewModel.PreviousEducation.EndDay.Id > 0)
                    {
                        ViewBag.PreviousEducationEndDays = new SelectList(existingViewModel.PreviousEducationEndDaySelectList, Utility.VALUE, Utility.TEXT, existingViewModel.PreviousEducation.EndDay.Id);
                    }
                    else if (existingViewModel.PreviousEducationEndDaySelectList != null && existingViewModel.PreviousEducation.EndDay.Id <= 0)
                    {
                        ViewBag.PreviousEducationEndDays = existingViewModel.PreviousEducationEndDaySelectList;
                    }
                    else if (existingViewModel.PreviousEducationEndDaySelectList == null)
                    {
                        existingViewModel.PreviousEducationEndDaySelectList = new List<SelectListItem>();
                        ViewBag.PreviousEducationEndDays = new List<SelectListItem>();
                    }
                }

                if (existingViewModel.PreviousEducation.EndDay != null && existingViewModel.PreviousEducation.EndDay.Id > 0)
                {
                    ViewBag.PreviousEducationEndDays = new SelectList(existingViewModel.PreviousEducationEndDaySelectList, Utility.VALUE, Utility.TEXT, existingViewModel.PreviousEducation.EndDay.Id);
                }
                else
                {
                    ViewBag.PreviousEducationEndDays = existingViewModel.PreviousEducationEndDaySelectList;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void SetDefaultSelectedSittingSubjectAndGrade(RegistrationViewModel viewModel)
        {
            try
            {
                if (viewModel != null && viewModel.FirstSittingOLevelResultDetails != null)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        ViewData["FirstSittingOLevelSubjectId" + i] = viewModel.OLevelSubjectSelectList;
                        ViewData["FirstSittingOLevelGradeId" + i] = viewModel.OLevelGradeSelectList;
                    }
                }

                if (viewModel != null && viewModel.SecondSittingOLevelResultDetails != null)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        ViewData["SecondSittingOLevelSubjectId" + i] = viewModel.OLevelSubjectSelectList;
                        ViewData["SecondSittingOLevelGradeId" + i] = viewModel.OLevelGradeSelectList;
                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
        }

        private void SetLgaIfExist(RegistrationViewModel viewModel)
        {
            try
            {
                if (viewModel.Person.State != null && !string.IsNullOrEmpty(viewModel.Person.State.Id))
                {
                    LocalGovernmentLogic localGovernmentLogic = new LocalGovernmentLogic();
                    List<LocalGovernment> lgas = localGovernmentLogic.GetModelsBy(l => l.State_Id == viewModel.Person.State.Id);
                    if (viewModel.Person.LocalGovernment != null && viewModel.Person.LocalGovernment.Id > 0)
                    {
                        ViewBag.Lgas = new SelectList(lgas, Utility.ID, Utility.NAME, viewModel.Person.LocalGovernment.Id);
                    }
                    else
                    {
                        ViewBag.Lgas = new SelectList(lgas, Utility.ID, Utility.NAME);
                    }
                }
                else
                {
                    ViewBag.Lgas = new SelectList(new List<LocalGovernment>(), Utility.ID, Utility.NAME);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
        }

        private void SetDepartmentIfExist(RegistrationViewModel viewModel)
        {
            try
            {
                if (viewModel.StudentLevel.Programme != null && viewModel.StudentLevel.Programme.Id > 0)
                {
                    ProgrammeDepartmentLogic departmentLogic = new ProgrammeDepartmentLogic();
                    List<Department> departments = departmentLogic.GetBy(viewModel.StudentLevel.Programme);
                    if (viewModel.StudentLevel.Department != null && viewModel.StudentLevel.Department.Id > 0)
                    {
                        ViewBag.Departments = new SelectList(departments, Utility.ID, Utility.NAME, viewModel.StudentLevel.Department.Id);
                    }
                    else
                    {
                        ViewBag.Departments = new SelectList(departments, Utility.ID, Utility.NAME);
                    }
                }
                else
                {
                    ViewBag.Departments = new SelectList(new List<Department>(), Utility.ID, Utility.NAME);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
        }

        private void SetDepartmentOptionIfExist(RegistrationViewModel viewModel)
        {
            try
            {
                if (viewModel.StudentLevel.Department != null && viewModel.StudentLevel.Department.Id > 0)
                {
                    DepartmentOptionLogic departmentOptionLogic = new DepartmentOptionLogic();
                    List<DepartmentOption> departmentOptions = departmentOptionLogic.GetModelsBy(l => l.Department_Id == viewModel.StudentLevel.Department.Id);
                    if (viewModel.StudentLevel.DepartmentOption != null && viewModel.StudentLevel.DepartmentOption.Id > 0)
                    {
                        ViewBag.DepartmentOptions = new SelectList(departmentOptions, Utility.ID, Utility.NAME, viewModel.StudentLevel.DepartmentOption.Id);
                    }
                    else
                    {
                        List<DepartmentOption> options = new List<DepartmentOption>();
                        DepartmentOption option = new DepartmentOption() { Id = 0, Name = viewModel.StudentLevel.Department.Name };
                        options.Add(option);

                        ViewBag.DepartmentOptions = new SelectList(options, Utility.ID, Utility.NAME);
                    }
                }
                else
                {
                    ViewBag.DepartmentOptions = new SelectList(new List<DepartmentOption>(), Utility.ID, Utility.NAME);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
        }
        
        private void SetPreviousEducationStartDate()
        {
            try
            {
                if (viewModel.PreviousEducation != null && viewModel.PreviousEducation.StartDate != DateTime.MinValue)
                {
                    int noOfDays = DateTime.DaysInMonth(viewModel.PreviousEducation.StartYear.Id, viewModel.PreviousEducation.StartMonth.Id);
                    List<Value> days = Utility.CreateNumberListFrom(1, noOfDays);
                    if (days != null && days.Count > 0)
                    {
                        days.Insert(0, new Value() { Name = "--DD--" });
                    }

                    if (viewModel.PreviousEducation.StartDay != null && viewModel.PreviousEducation.StartDay.Id > 0)
                    {
                        ViewBag.PreviousEducationStartDays = new SelectList(days, Utility.ID, Utility.NAME, viewModel.PreviousEducation.StartDay.Id);
                    }
                    else
                    {
                        ViewBag.PreviousEducationStartDays = new SelectList(days, Utility.ID, Utility.NAME);
                    }
                }
                else
                {
                    ViewBag.PreviousEducationStartDays = new SelectList(new List<Value>(), Utility.ID, Utility.NAME);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
        }

        private void SetPreviousEducationEndDate()
        {
            try
            {
                if (viewModel.PreviousEducation != null && viewModel.PreviousEducation.EndDate != DateTime.MinValue)
                {
                    int noOfDays = DateTime.DaysInMonth(viewModel.PreviousEducation.EndYear.Id, viewModel.PreviousEducation.EndMonth.Id);
                    List<Value> days = Utility.CreateNumberListFrom(1, noOfDays);
                    if (days != null && days.Count > 0)
                    {
                        days.Insert(0, new Value() { Name = "--DD--" });
                    }

                    if (viewModel.PreviousEducation.EndDay != null && viewModel.PreviousEducation.EndDay.Id > 0)
                    {
                        ViewBag.PreviousEducationEndDays = new SelectList(days, Utility.ID, Utility.NAME, viewModel.PreviousEducation.EndDay.Id);
                    }
                    else
                    {
                        ViewBag.PreviousEducationEndDays = new SelectList(days, Utility.ID, Utility.NAME);
                    }
                }
                else
                {
                    ViewBag.PreviousEducationEndDays = new SelectList(new List<Value>(), Utility.ID, Utility.NAME);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
        }

        private void SetDateOfBirth()
        {
            try
            {
                if (viewModel.Person.DateOfBirth.HasValue)
                {
                    if (viewModel.Person.YearOfBirth.Id > 0 && viewModel.Person.MonthOfBirth.Id > 0)
                    {
                        int noOfDays = DateTime.DaysInMonth(viewModel.Person.YearOfBirth.Id, viewModel.Person.MonthOfBirth.Id);
                        List<Value> days = Utility.CreateNumberListFrom(1, noOfDays);
                        if (days != null && days.Count > 0)
                        {
                            days.Insert(0, new Value() { Name = "--DD--" });
                        }

                        if (viewModel.Person.DayOfBirth != null && viewModel.Person.DayOfBirth.Id > 0)
                        {
                            ViewBag.DayOfBirths = new SelectList(days, Utility.ID, Utility.NAME, viewModel.Person.DayOfBirth.Id);
                        }
                        else
                        {
                            ViewBag.DayOfBirths = new SelectList(days, Utility.ID, Utility.NAME);
                        }
                    }
                }
                else
                {
                    ViewBag.DayOfBirths = new SelectList(new List<Value>(), Utility.ID, Utility.NAME);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
        }

        private void SetSelectedSittingSubjectAndGrade(RegistrationViewModel existingViewModel)
        {
            try
            {
                if (existingViewModel != null && existingViewModel.FirstSittingOLevelResultDetails != null && existingViewModel.FirstSittingOLevelResultDetails.Count > 0)
                {
                    int i = 0;
                    foreach (OLevelResultDetail firstSittingOLevelResultDetail in existingViewModel.FirstSittingOLevelResultDetails)
                    {
                        if (firstSittingOLevelResultDetail.Subject != null && firstSittingOLevelResultDetail.Grade != null)
                        {
                            ViewData["FirstSittingOLevelSubjectId" + i] = new SelectList(existingViewModel.OLevelSubjectSelectList, Utility.VALUE, Utility.TEXT, firstSittingOLevelResultDetail.Subject.Id);
                            ViewData["FirstSittingOLevelGradeId" + i] = new SelectList(existingViewModel.OLevelGradeSelectList, Utility.VALUE, Utility.TEXT, firstSittingOLevelResultDetail.Grade.Id);
                        }
                        else
                        {
                            firstSittingOLevelResultDetail.Subject = new OLevelSubject() { Id = 0 };
                            firstSittingOLevelResultDetail.Grade = new OLevelGrade() { Id = 0 };

                            ViewData["FirstSittingOLevelSubjectId" + i] = new SelectList(existingViewModel.OLevelSubjectSelectList, Utility.VALUE, Utility.TEXT, 0);
                            ViewData["FirstSittingOLevelGradeId" + i] = new SelectList(existingViewModel.OLevelGradeSelectList, Utility.VALUE, Utility.TEXT, 0);
                        }

                        i++;
                    }
                }

                if (existingViewModel != null && existingViewModel.SecondSittingOLevelResultDetails != null && existingViewModel.SecondSittingOLevelResultDetails.Count > 0)
                {
                    int i = 0;
                    foreach (OLevelResultDetail secondSittingOLevelResultDetail in existingViewModel.SecondSittingOLevelResultDetails)
                    {
                        if (secondSittingOLevelResultDetail.Subject != null && secondSittingOLevelResultDetail.Grade != null)
                        {
                            ViewData["SecondSittingOLevelSubjectId" + i] = new SelectList(existingViewModel.OLevelSubjectSelectList, Utility.VALUE, Utility.TEXT, secondSittingOLevelResultDetail.Subject.Id);
                            ViewData["SecondSittingOLevelGradeId" + i] = new SelectList(existingViewModel.OLevelGradeSelectList, Utility.VALUE, Utility.TEXT, secondSittingOLevelResultDetail.Grade.Id);
                        }
                        else
                        {
                            secondSittingOLevelResultDetail.Subject = new OLevelSubject() { Id = 0 };
                            secondSittingOLevelResultDetail.Grade = new OLevelGrade() { Id = 0 };

                            ViewData["SecondSittingOLevelSubjectId" + i] = new SelectList(existingViewModel.OLevelSubjectSelectList, Utility.VALUE, Utility.TEXT, 0);
                            ViewData["SecondSittingOLevelGradeId" + i] = new SelectList(existingViewModel.OLevelGradeSelectList, Utility.VALUE, Utility.TEXT, 0);
                        }

                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
        }

        public JsonResult GetLocalGovernmentsByState(string id)
        {
            try
            {
                LocalGovernmentLogic lgaLogic = new LocalGovernmentLogic();
                List<LocalGovernment> lgas = lgaLogic.GetModelsBy(l => l.State_Id == id);

                return Json(new SelectList(lgas, Utility.ID, Utility.NAME), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public JsonResult GetDayOfBirthBy(string monthId, string yearId)
        {
            try
            {
                if (string.IsNullOrEmpty(monthId) || string.IsNullOrEmpty(yearId))
                {
                    return null;
                }

                Value month = new Value() { Id = Convert.ToInt32(monthId) };
                Value year = new Value() { Id = Convert.ToInt32(yearId) };
                List<Value> days = Utility.GetNumberOfDaysInMonth(month, year);

                return Json(new SelectList(days, Utility.ID, Utility.NAME), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public JsonResult GetDepartmentByProgrammeId(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return null;
                }

                Programme programme = new Programme() { Id = Convert.ToInt32(id) };

                DepartmentLogic departmentLogic = new DepartmentLogic();
                List<Department> departments = departmentLogic.GetBy(programme);

                return Json(new SelectList(departments, Utility.ID, Utility.NAME), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public virtual ActionResult UploadFile(FormCollection form)
        {
            HttpPostedFileBase file = Request.Files["PassportFile"];

            bool isUploaded = false;
            string personId = form["Person.Id"].ToString();
            string passportUrl = form["Person.ImageFileUrl"].ToString();
            string message = "File upload failed";

            string path = null;
            string imageUrl = null;
            string imageUrlDisplay = null;

            try
            {
                if (file != null && file.ContentLength != 0)
                {
                    FileInfo fileInfo = new FileInfo(file.FileName);
                    string fileExtension = fileInfo.Extension;
                    string newFile = personId + "__";
                    string newFileName = newFile + DateTime.Now.ToString().Replace("/", "").Replace(":", "").Replace(" ", "") + fileExtension;

                    string invalidFileMessage = InvalidFile(file.ContentLength, fileExtension);
                    if (!string.IsNullOrEmpty(invalidFileMessage))
                    {
                        isUploaded = false;
                        TempData["imageUrl"] = null;
                        return Json(new { isUploaded = isUploaded, message = invalidFileMessage, imageUrl = passportUrl }, "text/html", JsonRequestBehavior.AllowGet);
                    }

                    string pathForSaving = Server.MapPath("~/Content/Junk");
                    if (this.CreateFolderIfNeeded(pathForSaving))
                    {
                        DeleteFileIfExist(pathForSaving, personId);

                        file.SaveAs(Path.Combine(pathForSaving, newFileName));

                        isUploaded = true;
                        message = "File uploaded successfully!";

                        path = Path.Combine(pathForSaving, newFileName);
                        if (path != null)
                        {
                            imageUrl = "/Content/Junk/" + newFileName;
                            imageUrlDisplay = appRoot + imageUrl + "?t=" + DateTime.Now;

                            //imageUrlDisplay = "/ilaropoly" + imageUrl + "?t=" + DateTime.Now;
                            TempData["imageUrl"] = imageUrl;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                message = string.Format("File upload failed: {0}", ex.Message);
            }

            return Json(new { isUploaded = isUploaded, message = message, imageUrl = imageUrlDisplay }, "text/html", JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public virtual ActionResult UploadFileNew(FormCollection form)
        {
            HttpPostedFileBase file = Request.Files["MyFile"];

            bool isUploaded = false;
            string personId = form["Person.Id"].ToString();
            string passportUrl = form["Person.ImageFileUrl"].ToString();
            string message = "File upload failed";

            string path = null;
            string imageUrl = null;
            string imageUrlDisplay = null;

            try
            {
                if (file != null && file.ContentLength != 0 && !string.IsNullOrEmpty(personId))
                {
                    FileInfo fileInfo = new FileInfo(file.FileName);
                    string fileExtension = fileInfo.Extension;
                    string newFile = personId + "__";
                    string newFileName = newFile + DateTime.Now.ToString().Replace("/", "").Replace(":", "").Replace(" ", "") + fileExtension;

                    decimal sizeAllowed = 50; //50kb
                    string invalidFileMessage = InvalidFile(file.ContentLength, fileExtension, sizeAllowed);
                    if (!string.IsNullOrEmpty(invalidFileMessage))
                    {
                        isUploaded = false;
                        TempData["imageUrl"] = null;
                        return Json(new { isUploaded = isUploaded, message = invalidFileMessage, imageUrl = passportUrl }, "text/html", JsonRequestBehavior.AllowGet);
                    }

                    string pathForSaving = Server.MapPath("~/Content/Student");
                    if (this.CreateFolderIfNeeded(pathForSaving))
                    {
                        DeleteFileIfExist(pathForSaving, personId);

                        file.SaveAs(Path.Combine(pathForSaving, newFileName));

                        isUploaded = true;
                        message = "File uploaded successfully!";

                        path = Path.Combine(pathForSaving, newFileName);
                        if (path != null)
                        {
                            imageUrl = "/Content/Student/" + newFileName;
                            imageUrlDisplay = appRoot + imageUrl + "?t=" + DateTime.Now;

                            //imageUrlDisplay = "/ilaropoly" + imageUrl + "?t=" + DateTime.Now;
                            TempData["imageUrl"] = imageUrl;
                            PersonLogic personLogic = new PersonLogic();
                            var Id = Convert.ToInt64(personId);
                            var person = personLogic.GetModelsBy(f => f.Person_Id == Id).FirstOrDefault();
                            if (person?.Id > 0)
                            {
                                person.ImageFileUrl =imageUrl;
                                personLogic.ModifyImageUrl(person);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                message = string.Format("File upload failed: {0}", ex.Message);
            }
            TempData.Keep();
            return Json(new { isUploaded = isUploaded, message = message, imageUrl = imageUrl }, "text/html", JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public virtual ActionResult UploadSignatureFile(FormCollection form)
        {
            HttpPostedFileBase file = Request.Files["SignatureFile"];

            bool isUploaded = false;
            string personId = form["Person.Id"].ToString();
            string signatureportUrl = form["Person.SignatureFileUrl"].ToString();
            string message = "File upload failed";

            string path = null;
            string signatureImageUrl = null;
            string signatureImageUrlDisplay = null;

            try
            {
                if (file != null && file.ContentLength != 0)
                {
                    FileInfo fileInfo = new FileInfo(file.FileName);
                    string fileExtension = fileInfo.Extension;
                    string newFile = personId + "Signature__";
                    string newFileName = newFile + DateTime.Now.ToString().Replace("/", "").Replace(":", "").Replace(" ", "") + fileExtension;

                    string invalidFileMessage = InvalidFile(file.ContentLength, fileExtension);
                    if (!string.IsNullOrEmpty(invalidFileMessage))
                    {
                        isUploaded = false;
                        TempData["SignatureFile"] = null;
                        return Json(new { isUploaded = isUploaded, message = invalidFileMessage, imageUrl = signatureportUrl }, "text/html", JsonRequestBehavior.AllowGet);
                    }

                    string pathForSaving = Server.MapPath("~/Content/Junk");
                    if (this.CreateFolderIfNeeded(pathForSaving))
                    {
                        DeleteFileIfExist(pathForSaving, personId);

                        file.SaveAs(Path.Combine(pathForSaving, newFileName));

                        isUploaded = true;
                        message = "File uploaded successfully!";

                        path = Path.Combine(pathForSaving, newFileName);
                        if (path != null)
                        {
                            signatureImageUrl = "/Content/Junk/" + newFileName;
                            signatureImageUrlDisplay = appRoot + signatureImageUrl + "?t=" + DateTime.Now;

                            //imageUrlDisplay = "/ilaropoly" + imageUrl + "?t=" + DateTime.Now;
                            TempData["SignatureFile"] = signatureImageUrl;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                message = string.Format("File upload failed: {0}", ex.Message);
            }

            return Json(new { isUploaded = isUploaded, message = message, signatureImageUrl = signatureImageUrlDisplay }, "text/html", JsonRequestBehavior.AllowGet);
        }
        private string InvalidFile(decimal uploadedFileSize, string fileExtension)
        {
            try
            {
                string message = null;
                decimal oneKiloByte = 1024;
                decimal maximumFileSize = 100 * oneKiloByte;

                decimal actualFileSizeToUpload = Math.Round(uploadedFileSize / oneKiloByte, 1);
                if (InvalidFileType(fileExtension))
                {
                    message = "File type '" + fileExtension + "' is invalid! File type must be any of the following: .jpg, .jpeg, .png or .jif ";
                }
                else if (actualFileSizeToUpload > (maximumFileSize / oneKiloByte))
                {
                    message = "Your file size of " + actualFileSizeToUpload.ToString("0.#") + " Kb is too large, maximum allowed size is " + (maximumFileSize / oneKiloByte) + " Kb";
                }

                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private string InvalidFile(decimal uploadedFileSize, string fileExtension, decimal sizeAllowed)
        {
            try
            {
                string message = null;
                decimal oneKiloByte = 1024;
                decimal maximumFileSize = sizeAllowed * oneKiloByte;

                decimal actualFileSizeToUpload = Math.Round(uploadedFileSize / oneKiloByte, 1);
                if (InvalidFileType(fileExtension))
                {
                    message = "File type '" + fileExtension + "' is invalid! File type must be any of the following: .jpg, .jpeg, .png or .gif ";
                }
                else if (actualFileSizeToUpload > (maximumFileSize / oneKiloByte))
                {
                    message = "Your file size of " + actualFileSizeToUpload.ToString("0.#") + " Kb is too large, maximum allowed size is " + (maximumFileSize / oneKiloByte) + " Kb";
                }

                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool InvalidFileType(string extension)
        {
            extension = extension.ToLower();
            switch (extension)
            {
                case ".jpg":
                    return false;
                case ".png":
                    return false;
                case ".gif":
                    return false;
                case ".jpeg":
                    return false;
                default:
                    return true;
            }
        }
       
        private void DeleteFileIfExist(string folderPath, string fileName)
        {
            try
            {
                string wildCard = fileName + "*.*";
                IEnumerable<string> files = Directory.EnumerateFiles(folderPath, wildCard, SearchOption.TopDirectoryOnly);

                if (files != null && files.Count() > 0)
                {
                    foreach (string file in files)
                    {
                        System.IO.File.Delete(file);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool CreateFolderIfNeeded(string path)
        {
            try
            {
                bool result = true;
                if (!Directory.Exists(path))
                {
                    try
                    {
                        Directory.CreateDirectory(path);
                    }
                    catch (Exception)
                    {
                        result = false;
                    }
                }

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void ModifyOlevelResult(OLevelResult oLevelResult, List<OLevelResultDetail> oLevelResultDetails)
        {
            try
            {
                OlevelResultdDetailsAudit olevelResultdDetailsAudit = new OlevelResultdDetailsAudit();
                olevelResultdDetailsAudit.Action = "Modify";
                olevelResultdDetailsAudit.Operation = "Modify O level (Registration Controller)";
                olevelResultdDetailsAudit.Client =  Request.LogonUserIdentity.Name + " (" + HttpContext.Request.UserHostAddress + ")";
                UserLogic loggeduser = new UserLogic();
                olevelResultdDetailsAudit.User = loggeduser.GetModelBy(u => u.User_Name == User.Identity.Name);

                if (olevelResultdDetailsAudit.User == null)
                {
                    olevelResultdDetailsAudit.User = new User() { Id = 1 };
                }

                OLevelResultLogic oLevelResultLogic = new OLevelResultLogic();
                OLevelResultDetailLogic oLevelResultDetailLogic = new OLevelResultDetailLogic();
                if (oLevelResult != null && oLevelResult.ExamNumber != null && oLevelResult.Type != null && oLevelResult.ExamYear > 0)
                {
                    OLevelResult oLevel = oLevelResultLogic.GetModelsBy(p => p.Person_Id == oLevelResult.Person.Id && p.Exam_Number == oLevelResult.ExamNumber && p.Exam_Year == oLevelResult.ExamYear && p.O_Level_Exam_Sitting_Id == oLevelResult.Sitting.Id).FirstOrDefault();
                    if (oLevel != null)
                    {
                        oLevelResult.Id = oLevel.Id;
                    }
                    
                    if (oLevelResult != null && oLevelResult.Id > 0)
                    {
                        oLevelResultDetailLogic.DeleteBy(oLevelResult,olevelResultdDetailsAudit);
                        oLevelResultLogic.Modify(oLevelResult);
                    }
                    else
                    {
                       
                        OLevelResult newOLevelResult = oLevelResultLogic.Create(oLevelResult);
                        oLevelResult.Id = newOLevelResult.Id;
                    }

                    if (oLevelResultDetails != null && oLevelResultDetails.Count > 0)
                    {
                        List<OLevelResultDetail> olevelResultDetails = oLevelResultDetails.Where(m => m.Grade != null && m.Grade.Id > 0 && m.Subject != null && m.Subject.Id > 0).ToList();
                        foreach (OLevelResultDetail oLevelResultDetail in olevelResultDetails)
                        {
                            oLevelResultDetail.Header = oLevelResult;
                            oLevelResultDetailLogic.Create(oLevelResultDetail);
                        }

                        //oLevelResultDetailLogic.Create(olevelResultDetails);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public JsonResult ConvertSignatureDataUrlToImage(string SignatureDataUrl,string personId)
        {
            JsonResultModel result = new JsonResultModel();
            var base64Data = Regex.Match(SignatureDataUrl, @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;
            var binData = Convert.FromBase64String(base64Data);

            MemoryStream ms = new MemoryStream(binData, 0, binData.Length);
            ms.Write(binData, 0, binData.Length);
            Image returnImage = Image.FromStream(ms);

            var Width = (int)(returnImage.Width);
            var Height = (int)(returnImage.Height);
            var newFile = personId + "Signature__";
            string newFileName = newFile + DateTime.Now.ToString().Replace("/", "").Replace(":", "").Replace(" ", "");
            var myUniqueFileName = string.Format("{0}.png", newFileName);
            //generate an image thumbnail for created image
            returnImage = returnImage.GetThumbnailImage(800, 800, null, IntPtr.Zero);

            string pathForSaving = Server.MapPath("~/Content/Signature");
            if (this.CreateFolderIfNeeded(pathForSaving))
            {
                DeleteFileIfExist(pathForSaving, personId);

                returnImage.Save(Path.Combine(pathForSaving, myUniqueFileName));
                var imageUrl= "/Content/Signature/" + myUniqueFileName;
                TempData["SignatureFile"] = imageUrl;
                result.SignatureUrl = imageUrl;
                result.Message = "File uploaded successfully!";
                result.IsError = false;

                
            }

           
            return Json(result);
        }
        public JsonResult ConvertDataUrlToImage(string imageFile, string personId)
        {
            JsonResultModel result = new JsonResultModel();
            var base64Data = Regex.Match(imageFile, @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;
            var binData = Convert.FromBase64String(base64Data);

            MemoryStream ms = new MemoryStream(binData, 0, binData.Length);
            ms.Write(binData, 0, binData.Length);
            Image returnImage = Image.FromStream(ms);

            var Width = (int)(returnImage.Width);
            var Height = (int)(returnImage.Height);
            var newFile = personId + "__";
            string newFileName = newFile + DateTime.Now.ToString().Replace("/", "").Replace(":", "").Replace(" ", "");
            var myUniqueFileName = string.Format("{0}.png", newFileName);
            //generate an image thumbnail for created image
            returnImage = returnImage.GetThumbnailImage(800, 800, null, IntPtr.Zero);

            string pathForSaving = Server.MapPath("~/Content/Junk");
            if (this.CreateFolderIfNeeded(pathForSaving))
            {
                DeleteFileIfExist(pathForSaving, personId);

                returnImage.Save(Path.Combine(pathForSaving, myUniqueFileName));
                var imageUrl = "/Content/Junk/" + myUniqueFileName;
                TempData["imageUrl"] = imageUrl;
                result.ImageFileUrl = imageUrl;
                result.Message = "File uploaded successfully!";
                result.IsError = false;


            }


            return Json(result);

         
        }








    }




}