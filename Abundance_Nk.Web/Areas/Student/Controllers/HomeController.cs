using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Abundance_Nk.Business;
using Abundance_Nk.Model.Entity.Model;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Web.Areas.Student.ViewModels;
using Abundance_Nk.Web.Controllers;
using Abundance_Nk.Web.Models;
using System.IO;

namespace Abundance_Nk.Web.Areas.Student.Controllers
{
    [AllowAnonymous]
    public class HomeController : BaseController
    {
        private Model.Model.Student _Student;
        private Model.Model.StudentLevel _StudentLevel;
        private StudentLevelLogic studentLevelLogic;
        private StudentLogic studentLogic;
        public HomeController()
        {
            try
            {
                if (System.Web.HttpContext.Current.Session["student"] != null)
                {
                    studentLogic = new StudentLogic();
                    _Student = System.Web.HttpContext.Current.Session["student"] as Model.Model.Student;
                    _Student = studentLogic.GetBy(_Student.Id);
                    studentLevelLogic = new StudentLevelLogic();
                    _StudentLevel = studentLevelLogic.GetBy(_Student.Id);
                }
                else
                {
                    FormsAuthentication.SignOut();
                    System.Web.HttpContext.Current.Response.Redirect("/Security/Account/Login");

                }

            }
            catch (Exception)
            {

                throw;
            }
        }
        // GET: Student/Home
        public ActionResult Index()
        {
            try
            {
                Model.Model.Student currentStudent = System.Web.HttpContext.Current.Session["student"] as Model.Model.Student;
                currentStudent = studentLogic.GetBy(currentStudent.Id);
                ViewBag.Email= StudentEmail(currentStudent.Id);
                UpdateStudentRRRPayments(currentStudent);
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                //var studentLevel=studentLevelLogic.GetModelsBy(f => f.STUDENT.Person_Id == currentStudent.Id).LastOrDefault();
                if (_StudentLevel?.Id > 0)
                {
                    SessionLogic sessionLogic = new SessionLogic();
                    var session=sessionLogic.GetModelsBy(f=>f.Activated==true).LastOrDefault();
                    if (session?.Id > 0)
                    {
                        var isOverStayed=Utility.hasDoneMoreThanFourSessions(currentStudent, session);
                        if (isOverStayed)
                        {
                            FormsAuthentication.SignOut();
                            SetMessage("Student has over stayed the institution's allowed sessions", Message.Category.Information);
                            return RedirectToAction("Login", "Account", new { Area = "Security" });
                        }
                    }
                    
                    //var isPaid=CheckSchoolFeesDefualters(_StudentLevel);
                    //if (!isPaid)
                    //{
                    //    FormsAuthentication.SignOut();
                    //    SetMessage("Please pay school fees to continue", Message.Category.Information);
                    //    return RedirectToAction("Login", "Account", new { Area = "Security" });
                    //}
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }
            return View();
        }
        private void UpdateStudentRRRPayments(Model.Model.Student student)
        {
            try
            {
                if (student != null)
                {
                    RemitaSettings settings = new RemitaSettings();
                    RemitaSettingsLogic settingsLogic = new RemitaSettingsLogic();
                    RemitaResponse remitaResponse = new RemitaResponse();
                    RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();

                    settings = settingsLogic.GetModelBy(s => s.Payment_SettingId == 1);
                    string remitaVerifyUrl = ConfigurationManager.AppSettings["RemitaVerifyUrl"].ToString();
                    RemitaPayementProcessor remitaProcessor = new RemitaPayementProcessor(settings.Api_key);

                    List<RemitaPayment> remitaPayments = remitaPaymentLogic.GetModelsBy(m => m.PAYMENT.Person_Id == student.Id);
                    //List<RemitaPayment> remitaPayments = remitaPaymentLogic.GetModelsBy(m => !m.Status.Contains("01") && m.PAYMENT.Person_Id == student.Id);

                    foreach (RemitaPayment remitaPayment in remitaPayments)
                    {
                        remitaResponse = remitaProcessor.TransactionStatus(remitaVerifyUrl, remitaPayment);
                        if (remitaResponse != null && remitaResponse.Status != null && remitaPayment.RRR != "110264805317" && remitaPayment.RRR != "350272406734" && remitaPayment.RRR != "150352613944")
                        {
                            remitaPayment.Status = remitaResponse.Status + ":" + remitaResponse.StatusCode;
                            remitaPayment.TransactionAmount = remitaResponse.amount > 0M ? remitaResponse.amount : remitaPayment.TransactionAmount;
                            remitaPaymentLogic.Modify(remitaPayment);
                        }
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
        }
        public ActionResult Profile()
        {
            try
            {
                if (_Student != null && _StudentLevel != null)
                {
                    return RedirectToAction("Form", "Registration", new { Area = "Student", sid = _Student.Id, pid = _StudentLevel.Programme.Id });
                }
                else
                {
                    FormsAuthentication.SignOut();
                    RedirectToAction("Login", "Account", new { Area = "Security" });
                }
            }
            catch (Exception)
            {

                throw;
            }
            return RedirectToAction("Index");
        }
        public ActionResult Fees()
        {
            try
            {
                if (_Student != null && _StudentLevel != null)
                {
                    return RedirectToAction("GenerateInvoice", "Payment", new { Area = "Student", sid = _Student.Id });

                }
                else
                {
                    FormsAuthentication.SignOut();
                    RedirectToAction("Login", "Account", new { Area = "Security" });
                }
            }
            catch (Exception)
            {

                throw;
            }
            return RedirectToAction("Index");
        }
        public ActionResult ExtraYearFees()
        {
            try
            {
                if (_Student != null && _StudentLevel != null)
                {
                    return RedirectToAction("Index", "ExtraYear", new { Area = "Student" });

                }
                else
                {
                    FormsAuthentication.SignOut();
                    RedirectToAction("Login", "Account", new { Area = "Security" });
                }
            }
            catch (Exception)
            {

                throw;
            }
            return RedirectToAction("Index");
        }
        public ActionResult PaymentHistory()
        {
            var paymentHistory = new PaymentHistory();
            try
            {
                if (_Student != null && _StudentLevel != null)
                {
                    var paymentLogic = new PaymentLogic();
                    paymentHistory.Payments = paymentLogic.GetBy(_Student);

                    paymentHistory.Student = _Student;
                }
                else
                {
                    FormsAuthentication.SignOut();
                    RedirectToAction("Login", "Account", new { Area = "Security" });
                }
            }
            catch (Exception)
            {
                throw;
            }
            return View(paymentHistory);
        }
        public ActionResult CourseRegistration()
        {
            try
            {
                if (_Student != null && _StudentLevel != null)
                {
                    //To be removed
                    //SetMessage("Registration has closed.", Message.Category.Error);
                    //return View("Index");
                    return RedirectToAction("SelectCourseRegistrationSession", "Registration", new { Area = "Student" });

                }
                else
                {
                    FormsAuthentication.SignOut();
                    RedirectToAction("Login", "Account", new { Area = "Security" });
                }
            }
            catch (Exception)
            {

                throw;
            }
            return RedirectToAction("Index");
        }
        public ActionResult ExtraYearRegistration()
        {
            try
            {
                if (_Student != null && _StudentLevel != null)
                {
                    return RedirectToAction("Step_3", "ExtraYear", new { Area = "Student" });

                }
                else
                {
                    FormsAuthentication.SignOut();
                    RedirectToAction("Login", "Account", new { Area = "Security" });
                }
            }
            catch (Exception)
            {

                throw;
            }
            return RedirectToAction("Index");
        }
        public ActionResult Result()
        {
            try
            {
                if (_Student != null && _StudentLevel != null)
                {
                    return RedirectToAction("Check", "Result", new { Area = "Student" });

                }
                else
                {
                    FormsAuthentication.SignOut();
                    RedirectToAction("Login", "Account", new { Area = "Security" });
                }
            }
            catch (Exception)
            {

                throw;
            }
            return RedirectToAction("Index");
        }
        public ActionResult ChangePassword()
        {
            try
            {
                if (_Student != null && _StudentLevel != null)
                {
                    StudentViewModel studentViewModel = new StudentViewModel();
                    studentViewModel.Student = _Student;
                    return View(studentViewModel);
                }
                else
                {
                    FormsAuthentication.SignOut();
                    RedirectToAction("Login", "Account", new { Area = "Security" });
                }
            }
            catch (Exception)
            {

                throw;
            }
            return View();
        }
        [HttpPost]
        public ActionResult ChangePassword(StudentViewModel studentViewModel)
        {
            try
            {
                ModelState.Remove("Student.FirstName");
                ModelState.Remove("Student.LastName");
                ModelState.Remove("Student.MobilePhone");
                if (ModelState.IsValid)
                {
                    var studentLogic = new StudentLogic();
                    var LoggedInUser = new Model.Model.Student();
                    LoggedInUser = studentLogic.GetModelBy(
                            u =>
                                u.Matric_Number == studentViewModel.Student.MatricNumber &&
                                u.Password_hash == studentViewModel.OldPassword);
                    if (LoggedInUser != null)
                    {
                        LoggedInUser.PasswordHash = studentViewModel.NewPassword;
                        studentLogic.ChangeUserPassword(LoggedInUser);
                        TempData["Message"] = "Password Changed successfully! Please keep password in a safe place";
                        return RedirectToAction("Index", "Home", new { Area = "Student" });
                    }
                    SetMessage("Please log off and log in then try again.", Message.Category.Error);

                    return View(studentViewModel);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return View();
        }
        public ActionResult GenerateShortFallInvoice()
        {
            try
            {
                if (_Student != null && _StudentLevel != null)
                {
                    return RedirectToAction("GenerateShortFallInvoice", "Payment", new { Area = "Student" });
                }
                else
                {
                    FormsAuthentication.SignOut();
                    RedirectToAction("Login", "Account", new { Area = "Security" });
                }
            }
            catch (Exception)
            {

                throw;
            }
            return RedirectToAction("Index");
        }
        public ActionResult PayShortFallFee()
        {
            try
            {
                if (_Student != null && _StudentLevel != null)
                {
                    return RedirectToAction("PayShortFallFee", "Payment", new { Area = "Student" });
                }
                else
                {
                    FormsAuthentication.SignOut();
                    RedirectToAction("Login", "Account", new { Area = "Security" });
                }
            }
            catch (Exception)
            {

                throw;
            }
            return RedirectToAction("Index");
        }
        public ActionResult Logon()
        {
            RegistrationLogonViewModel logonViewModel = new RegistrationLogonViewModel();
            ViewBag.Session = logonViewModel.SessionSelectListItem;
            return View(logonViewModel);
        }

        [HttpPost]
        public ActionResult Logon(RegistrationLogonViewModel viewModel)
        {
            Payment payment = new Payment();
            RegistrationLogonViewModel logonViewModel = new RegistrationLogonViewModel();

            try
            {
                SessionLogic sessionLogic = new SessionLogic();
                Model.Model.Session applicationSession = sessionLogic.GetApplicationSession();
                applicationSession = applicationSession != null ? applicationSession : new Model.Model.Session { Id = (int)Sessions._20182019 };

                if (viewModel.Session != null && viewModel.Session.Id != applicationSession.Id)
                {
                    SetMessage("Registration has closed!", Message.Category.Error);
                    ViewBag.Session = viewModel.SessionSelectListItem;
                    return View(viewModel);
                }

                if (viewModel.ConfirmationOrderNumber.Length > 12)
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
                        //Get status of transaction
                        RemitaSettings settings = new RemitaSettings();
                        RemitaSettingsLogic settingsLogic = new RemitaSettingsLogic();
                        settings = settingsLogic.GetModelBy(s => s.Payment_SettingId == 2);
                        string remitaVerifyUrl = ConfigurationManager.AppSettings["RemitaVerifyUrl"].ToString();
                        RemitaPayementProcessor remitaPayementProcessor = new RemitaPayementProcessor(settings.Api_key);
                        remitaPayementProcessor.GetTransactionStatus(remitaPayment.RRR, remitaVerifyUrl, 2);
                        remitaPayment = remitaPaymentLogic.GetModelsBy(a => a.RRR == viewModel.ConfirmationOrderNumber).FirstOrDefault();
                        if (remitaPayment != null && (remitaPayment.Status.Contains("01:") || remitaPayment.Status.Contains("00:")) && remitaPayment.payment.FeeType.Id == (int)FeeTypes.SchoolFees)
                        {
                            payment = remitaPayment.payment;
                        }
                        else
                        {
                            SetMessage("Payment couldn't verified for this payment purpose!", Message.Category.Error);
                            ViewBag.Session = logonViewModel.SessionSelectListItem;
                            return View(logonViewModel);
                        }
                    }
                    else if (remitaPayment == null && viewModel.Session.Id == 12)
                    {
                        RemitaPayementProcessor r = new RemitaPayementProcessor("918567");
                        RemitaPayment remitaPaymentToFetch = new RemitaPayment { RRR = viewModel.ConfirmationOrderNumber, MerchantCode = "538661740" };
                        StudentLogic studentLogic = new StudentLogic();
                        var getStudent = studentLogic.GetModelBy(x => x.Matric_Number == User.Identity.Name);
                        if (getStudent != null)
                        {
                            Payment validatedPayment =  ValidatePayWithCardDispute(getStudent.MatricNumber, true);
                            if(validatedPayment != null)
                            {
                                remitaPaymentToFetch.payment = validatedPayment;
                                remitaPayment = r.GetStatusForPayWithCardDispute(remitaPaymentToFetch);
                            }

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
                if (ex.Message == "The pin amount tied to the pin is not correct. Please contact support@lloydant.com.")
                {
                    string checkMessage = checkSchoolFeeShortFall(viewModel.ConfirmationOrderNumber, viewModel.Session, new FeeType() { Id = (int)FeeTypes.SchoolFees });
                    if (checkMessage == "True")
                    {
                        return RedirectToAction("GenerateShortFallInvoice", "Payment", new { area = "Student" });
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

            SetMessage("School Fees payment has been confirmed, You can click on the 'Receipt' link to generate your receipt. ", Message.Category.Information);
            ViewBag.Session = logonViewModel.SessionSelectListItem;
            return View(logonViewModel);
        }

        public string GetStudentDetails()
        {
            try
            {
                StudentLogic studentLogic = new StudentLogic();
                var getStudent = studentLogic.GetModelBy(x => x.Matric_Number == User.Identity.Name);
                if (getStudent != null)
                    return getStudent.MatricNumber;
                return null;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        public Payment ValidatePayWithCardDispute(string RegNumber, bool isStudent)
        {
            try
            {
                RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                if (isStudent)
                {
                    StudentLogic studentLogic = new StudentLogic();
                    PaymentLogic paymentLogic = new PaymentLogic();
                    var getStudent = studentLogic.GetModelBy(x => x.Matric_Number == RegNumber);
                    if (getStudent != null)
                    {
                        var getPayment = remitaPaymentLogic.GetModelsBy(x => x.PAYMENT.Fee_Type_Id == 3 && x.PAYMENT.Session_Id == 12 && x.PAYMENT.Person_Id == getStudent.Id 
                        && (!x.Status.Contains("01") || !x.Status.Contains("00"))).LastOrDefault();
                        if (getPayment != null)
                            return getPayment.payment;
                    }
                }
                else
                {
                    ApplicationFormLogic applicationFormLogic = new ApplicationFormLogic();
                    var getPerson = applicationFormLogic.GetModelBy(x => x.Application_Form_Number == RegNumber);
                    if (getPerson != null)
                    {
                        var getPayment = remitaPaymentLogic.GetModelsBy(x => x.PAYMENT.Fee_Type_Id == 3 && x.PAYMENT.Session_Id == 12 && x.PAYMENT.Person_Id == getPerson.Id
                        && (!x.Status.Contains("01") || !x.Status.Contains("00"))).LastOrDefault();
                        if (getPayment != null)
                            return getPayment.payment;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
                PaymentEtranzact etranzact = paymentEtranzactLogic.RetrieveEtranzactWebServicePinDetails(ConfirmationNumber, paymentTerminal);

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
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "True";
        }
        public ActionResult Step_3()
        {
            RegistrationLogonViewModel logonViewModel = new RegistrationLogonViewModel();
            ViewBag.Session = logonViewModel.SessionSelectListItem;
            return View(logonViewModel);
        }
        public string StudentEmail(long studentId)
        {
            PersonLogic personLogic = new PersonLogic();
            if (studentId > 0)
            {
                var person = personLogic.GetModelsBy(f => f.Person_Id == studentId).FirstOrDefault();
                if (person?.Id > 0)
                {
                    return person.Email;
                }
            }
           
            return null;
        }
        public JsonResult UpdateEmail(string emailAddress)
        {
            JsonResultModel result = new JsonResultModel();
            Model.Model.Student currentStudent = System.Web.HttpContext.Current.Session["student"] as Model.Model.Student;
            try
            {
                if (currentStudent?.Id > 0)
                {
                    PersonLogic personLogic = new PersonLogic();
                    var person = personLogic.GetModelsBy(f => f.Person_Id == currentStudent.Id).FirstOrDefault();
                    if (person?.Id > 0)
                    {
                        person.Email=emailAddress;
                        personLogic.Modify(person);
                        result.IsError = false;
                        result.Message = "You have successfully Updated your email address!";
                    }
                }

            }
            catch (Exception ex)
            {

                result.IsError = true;
                result.Message = ex.Message;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public bool CheckSchoolFeesDefualters(StudentLevel studentLevel)
        {
            SessionLogic sessionLogic = new SessionLogic();
            if (studentLevel?.Id > 0)
            {
                RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
                PaymentLogic paymentLogic = new PaymentLogic();
                // check for students in second year
                if (studentLevel.Level?.Id==2 || studentLevel.Level?.Id == 4)
                {
                    var secondTrSessionName=studentLevel.Session.Name;
                    if (!string.IsNullOrEmpty(secondTrSessionName))
                    {
                        var splitArray= secondTrSessionName.Split('/');
                        var firstYrSessionName = Convert.ToString(Convert.ToInt32((splitArray[0])) - 1 + "/" + Convert.ToInt32((splitArray[0])));
                        var firstYearSession=sessionLogic.GetModelsBy(f => f.Session_Name == firstYrSessionName.Trim()).FirstOrDefault();
                        if (firstYearSession?.Id > 0)
                        {
                            
                            var paidSchoolFees=remitaPaymentLogic.GetModelsBy(f => f.PAYMENT.Person_Id == studentLevel.Student.Id && f.PAYMENT.Session_Id == firstYearSession.Id && (f.PAYMENT.Fee_Type_Id == 3 || f.PAYMENT.Fee_Type_Id == 10) && (f.Status.Contains("01")|| f.Description.Contains("manual"))).FirstOrDefault();
                            if (paidSchoolFees == null)
                            {
                                var payment=paymentLogic.GetModelsBy(f => f.Person_Id == studentLevel.Student.Id && f.Session_Id == firstYearSession.Id && (f.Fee_Type_Id == 3 || f.Fee_Type_Id == 10)).FirstOrDefault();
                                if (payment?.Id > 0)
                                {
                                    var etranzactRecord=paymentEtranzactLogic.GetModelsBy(f => f.Payment_Id==payment.Id).FirstOrDefault();
                                    if (etranzactRecord?.ConfirmationNo!= null)
                                    {

                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                                else
                                {
                                    return false;
                                }

                                
                                
                            }
                                
                            CourseRegistrationLogic courseRegistrationLogic = new CourseRegistrationLogic();
                           var sessionCourseRegistration= courseRegistrationLogic.GetModelsBy(f => f.Session_Id == studentLevel.Session.Id && f.Person_Id == studentLevel.Student.Id && f.Level_Id == studentLevel.Level.Id).FirstOrDefault();
                            if (sessionCourseRegistration?.Id > 0)
                            {
                                var paidCurrentSchoolFees = remitaPaymentLogic.GetModelsBy(f => f.PAYMENT.Person_Id == sessionCourseRegistration.Student.Id &&
                                f.PAYMENT.Session_Id == sessionCourseRegistration.Session.Id && (f.PAYMENT.Fee_Type_Id == 3 || f.PAYMENT.Fee_Type_Id == 10)
                                && (f.Status.Contains("01") || f.Description.Contains("manual"))).FirstOrDefault();
                                //check if has paid school fees for the current session and level with course registration
                                if(paidCurrentSchoolFees==null)
                                    if (paidCurrentSchoolFees == null)
                                    {
                                        var payment = paymentLogic.GetModelsBy(f => f.Person_Id == studentLevel.Student.Id && f.Session_Id == sessionCourseRegistration.Session.Id && (f.Fee_Type_Id == 3 || f.Fee_Type_Id == 10)).FirstOrDefault();
                                        if (payment?.Id > 0)
                                        {
                                            var etranzactRecord = paymentEtranzactLogic.GetModelsBy(f => f.Payment_Id == payment.Id).FirstOrDefault();
                                            if (etranzactRecord?.ConfirmationNo != null)
                                            {

                                            }
                                            else
                                            {
                                                return false;
                                            }
                                        }
                                        else
                                        {
                                            return false;
                                        }



                                    }
                            }

                        }
                        return true;
                    }
                }
                else
                {
                    CourseRegistrationLogic courseRegistrationLogic = new CourseRegistrationLogic();
                    var sessionCourseRegistration = courseRegistrationLogic.GetModelsBy(f => f.Session_Id == studentLevel.Session.Id && f.Person_Id == studentLevel.Student.Id && f.Level_Id == studentLevel.Level.Id).FirstOrDefault();
                    if (sessionCourseRegistration?.Id > 0)
                    {
                        var paidCurrentSchoolFees = remitaPaymentLogic.GetModelsBy(f => f.PAYMENT.Person_Id == sessionCourseRegistration.Student.Id &&
                        f.PAYMENT.Session_Id == sessionCourseRegistration.Session.Id && (f.PAYMENT.Fee_Type_Id == 3 || f.PAYMENT.Fee_Type_Id == 10)
                        && (f.Status.Contains("01") || f.Description.Contains("manual"))).FirstOrDefault();
                        //check if has paid school fees for the current session and level with course registration
                        if (paidCurrentSchoolFees == null)
                            if (paidCurrentSchoolFees == null)
                            {
                                var payment = paymentLogic.GetModelsBy(f => f.Person_Id == studentLevel.Student.Id && f.Session_Id == sessionCourseRegistration.Session.Id && (f.Fee_Type_Id == 3 || f.Fee_Type_Id == 10)).FirstOrDefault();
                                if (payment?.Id > 0)
                                {
                                    var etranzactRecord = paymentEtranzactLogic.GetModelsBy(f => f.Payment_Id == payment.Id).FirstOrDefault();
                                    if (etranzactRecord?.ConfirmationNo != null)
                                    {

                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                                else
                                {
                                    return false;
                                }



                            }
                    }
                }
                return true;
            }
            return true;
        }
        //public ActionResult PayFees()
        //{
        //    try
        //    {
        //        if (_Student != null && _StudentLevel != null)
        //        {
        //            return RedirectToAction("Logon", "Registration", new { Area = "Student" });
        //        }
        //        else
        //        {
        //            FormsAuthentication.SignOut();
        //            RedirectToAction("Login", "Account", new { Area = "Security" });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    return RedirectToAction("Index");
        //}

        //public ActionResult OtherFees()
        //{
        //    try
        //    {
        //        if (_Student != null && _StudentLevel != null)
        //        {
        //            return RedirectToAction("OldFees", "Payment", new { Area = "Student", Detail = Utility.Encrypt(_Student.Id.ToString()) });

        //        }
        //        else
        //        {
        //            FormsAuthentication.SignOut();
        //            RedirectToAction("Login", "Account", new { Area = "Security" });
        //        }
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //    return RedirectToAction("Index");

        //}

        //public ActionResult PaymentReceipt()
        //{
        //    try
        //    {
        //        if (_Student != null && _StudentLevel != null)
        //        {
        //            return RedirectToAction("PrintReceipt", "Registration", new { Area = "Student" });
        //        }
        //        else
        //        {
        //            FormsAuthentication.SignOut();
        //            RedirectToAction("Login", "Account", new { Area = "Security" });
        //        }
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //    return RedirectToAction("Index");


        //}
       


    }
}