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
using System.Web.Script.Serialization;
using Abundance_Nk.Web.Areas.Student.ViewModels;

namespace Abundance_Nk.Web.Areas.Applicant.Controllers
{
    [AllowAnonymous]
    public class FormController :BaseController
    {
        private const string ID = "Id";
        private const string NAME = "Name";
        private const string VALUE = "Value";
        private const string TEXT = "Text";
        private const string DEFAULT_PASSPORT = "/Content/Images/default_avatar.png";
        private const string FIRST_SITTING = "FIRST SITTING";
        private const string SECOND_SITTING = "SECOND SITTING";
        private string appRoot = ConfigurationManager.AppSettings["AppRoot"];

        private PostJambViewModel viewModel;

        public ActionResult PostJambProgramme()
        {
            PostJAMBProgrammeViewModel viewModel = new PostJAMBProgrammeViewModel();

            try
            {
                TempData["viewModel"] = null;
                TempData["imageUrl"] = null;
                TempData["ProgrammeViewModel"] = null;
                TempData["PostJAMBFormPaymentViewModel"] = null;
            }
            catch(Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message,Message.Category.Error);
            }

            return View(viewModel);
        }

      


        [HttpPost]
        public ActionResult PostJambProgramme(PostJAMBProgrammeViewModel viewModel) 
        {
            RemitaPayment remitaPayment = new RemitaPayment();
            RemitaPaymentLogic remitaPyamentLogic = new RemitaPaymentLogic();
            bool IsFirstChoice = false;
            bool IsAdmitted = false;

            try
            {
                if(ModelState.IsValid)
                {
                    //live code -- to uncomment later
                    Payment payment = InvalidConfirmationOrderNumber(viewModel.ConfirmationOrderNumber);

                    int[] applicationFeeTypes = { 1, 4, 5, 19 };
                    remitaPayment = remitaPyamentLogic.GetModelsBy(a => (a.RRR == viewModel.ConfirmationOrderNumber && (a.Description.ToLower().Contains("manual") || a.Status == "01:"))).FirstOrDefault(); //|| /*(a.RRR == viewModel.ConfirmationOrderNumber && a.Status.Contains("021") && applicationFeeTypes.Contains(a.PAYMENT.Fee_Type_Id))).FirstOrDefault();*/
                    //(a.RRR == viewModel.ConfirmationOrderNumber && applicationFeeTypes.Contains(a.PAYMENT.Fee_Type_Id))).FirstOrDefault();

                    if (payment == null || payment.Id <= 0)
                    {
                        if(remitaPayment == null || remitaPayment.payment.Id <= 0)
                        {
                            SetMessage("Confirmation Order Number/RRR entered was not found or valid for this payment!",Message.Category.Error);
                            return View(viewModel);
                        }
                        payment = remitaPayment.payment;
                    }
                    
                    if (remitaPayment == null || remitaPayment.RRR == null)
                    {
                        PaymentEtranzactLogic etranzactLogic = new PaymentEtranzactLogic();
                        bool pinUseStatus = etranzactLogic.IsPinUsed(viewModel.ConfirmationOrderNumber,(int)payment.Person.Id);
                        if(pinUseStatus)
                        {
                            SetMessage("Pin has been used by another applicant! Please cross check and Try again.",Message.Category.Error);
                            return View(viewModel);
                        }
                    }
                    
                    AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
                    AppliedCourse appliedCourse = appliedCourseLogic.GetModelBy(m => m.Person_Id == payment.Person.Id);
                   //string[] pinsToAllow = { "76845078821502194949013", "76753541791497611123377", "76753553591469444342403", "03359304291498738129605", "03289650321498745135951" };
                   //if (appliedCourse != null && appliedCourse.Programme != null && (appliedCourse.Programme.Id == (int)Programmes.HNDFullTime || appliedCourse.Programme.Id == (int)Programmes.NDPartTime))
                   // {
                   //     if (!pinsToAllow.Contains(viewModel.ConfirmationOrderNumber))
                   //     {
                   //         SetMessage("Application has been closed for your selected programme.", Message.Category.Error);
                   //         return View(viewModel);
                   //     }
                   // }

                    ApplicantJambDetailLogic studentJambDetailLogic = new ApplicantJambDetailLogic();
                    ApplicantJambDetail studentJambDetail = studentJambDetailLogic.GetModelBy(m => m.Person_Id == payment.Person.Id);
                    PostJAMBFormPaymentViewModel postJAMBFormPaymentViewModel = new PostJAMBFormPaymentViewModel();
                    AdmissionListLogic admissionListLogic = new AdmissionListLogic();
                    JambRecordLogic jambRecordLogic = new JambRecordLogic();
                    OLevelSubject EnglishLanguage = new OLevelSubject() { Id = 1 };
                    JambRecord jambRecord = null;
                    if (studentJambDetail != null)
                    {
                        studentJambDetail.Subject1 = EnglishLanguage;
                        jambRecord = jambRecordLogic.GetModelBy(jr => jr.Jamb_Registration_Number == studentJambDetail.JambRegistrationNumber); 
                    }
                    var checkAdmissionList = admissionListLogic.GetModelBy(x => x.APPLICATION_FORM.Person_Id == payment.Person.Id);
                    if (checkAdmissionList != null && checkAdmissionList.Id > 0)
                    {
                        IsAdmitted = true;
                        postJAMBFormPaymentViewModel.IsAdmitted = true;
                    }
                    if (jambRecord != null)
                    {


                        InstitutionChoice firstChoiceInstitution = new InstitutionChoice() { Id = 1 };

                        studentJambDetail.JambScore = jambRecord.TotalJambScore;
                        studentJambDetail.Subject1 = new OLevelSubject() { Id = Convert.ToInt32(jambRecord.Subject4) };
                        studentJambDetail.Subject2 = new OLevelSubject() { Id = Convert.ToInt32(jambRecord.Subject1) };
                        studentJambDetail.Subject3 = new OLevelSubject() { Id = Convert.ToInt32(jambRecord.Subject2) };
                        studentJambDetail.Subject4 = new OLevelSubject() { Id = Convert.ToInt32(jambRecord.Subject3) };
                        studentJambDetail.Subject1Score = jambRecord.Score4;
                        studentJambDetail.Subject2Score = jambRecord.Score1;
                        studentJambDetail.Subject3Score = jambRecord.Score2;
                        studentJambDetail.Subject4Score = jambRecord.Score3;
                        studentJambDetail.InstitutionChoice = firstChoiceInstitution;

                        studentJambDetailLogic.Modify(studentJambDetail);
                        IsFirstChoice = true;
                        postJAMBFormPaymentViewModel.IsJambLoaded = true;

                        //InstitutionChoice firstChoiceInstitution = new InstitutionChoice() { Id = 1 };                        

                        //studentJambDetail.JambScore = jambRecord.TotalJambScore;                        
                        //studentJambDetail.Subject2 = new OLevelSubject() { Id = Convert.ToInt32(jambRecord.Subject2) };
                        //studentJambDetail.Subject3 = new OLevelSubject() { Id = Convert.ToInt32(jambRecord.Subject3) };
                        //studentJambDetail.Subject4 = new OLevelSubject() { Id = Convert.ToInt32(jambRecord.Subject4) };
                        //if (jambRecord.FirstChoiceInstitution == "FEDPO-ILA")
                        //{
                        //    studentJambDetail.InstitutionChoice = firstChoiceInstitution;
                        //}

                        //studentJambDetailLogic.Modify(studentJambDetail);
                    }
                    //studentJambDetailLogic.Modify(studentJambDetail);

                    if (studentJambDetail != null)
                    {
                        postJAMBFormPaymentViewModel.JambRegistrationNumber = studentJambDetail.JambRegistrationNumber;
                        postJAMBFormPaymentViewModel.ApplicantJambDetail = studentJambDetail;
                    }

                    postJAMBFormPaymentViewModel.Payment = payment;
                    if(appliedCourse != null)
                    {
                        postJAMBFormPaymentViewModel.Programme = appliedCourse.Programme;
                        postJAMBFormPaymentViewModel.AppliedCourse = appliedCourse;
                        postJAMBFormPaymentViewModel.Person = payment.Person;
                        postJAMBFormPaymentViewModel.Programme = appliedCourse.Programme;
                        postJAMBFormPaymentViewModel.remitaPayment = remitaPayment;
                        postJAMBFormPaymentViewModel.Initialise();
                    }
                    else
                    {
                        SetMessage("The Confirmation Order Number entered does not have a corresponding applied course! Please contact your system administrator",Message.Category.Error);
                        return View(viewModel);
                    }

                    TempData["PostJAMBFormPaymentViewModel"] = postJAMBFormPaymentViewModel;

                    if (appliedCourse.Programme.Id == (int)Programmes.CiscoCertificate)
                    {
                        return RedirectToAction("CiscoForm", "Form");

                    }
                    else if (appliedCourse.Programme.Id == (int)Programmes.DrivingCertificate)
                    {
                        return RedirectToAction("DrivingExperienceForm", "Form");
                    }
                    //else if (appliedCourse.Programme.Id == (int)Programmes.NDFullTime)
                    //{
                    //    if (IsAdmitted || IsFirstChoice)
                    //    {
                    //        return RedirectToAction("PostJambForm", "Form");

                    //    }
                    //    else
                    //    {
                    //        return RedirectToAction("FirstChoiceInstitutionCheck");
                    //    }

                    //}
                    else
                    {
                        return RedirectToAction("PostJambForm", "Form");
                    }
                }
            }
            catch(Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message,Message.Category.Error);
            }

            return View(viewModel);
        }
        private Payment InvalidConfirmationOrderNumber(string confirmationOrderNumber)
        {
            Payment payment = new Payment();
            PaymentEtranzactLogic etranzactLogic = new PaymentEtranzactLogic();
            PaymentEtranzact etranzactDetails = etranzactLogic.GetModelBy(m => m.Confirmation_No == confirmationOrderNumber);
            if(etranzactDetails == null || etranzactDetails.ReceiptNo == null)
            {
                RemitaPayment remitaPayment = new RemitaPayment();
                RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                RemitaPayementProcessor r = new RemitaPayementProcessor("918567");
                remitaPayment = remitaPaymentLogic.GetModelsBy(a => a.RRR == confirmationOrderNumber).FirstOrDefault();

                if (remitaPayment == null)
                {
                    //Get Remita Record for omitted RRR
                    RemitaPayment remitaPaymentToFetch = new RemitaPayment { RRR = confirmationOrderNumber, MerchantCode = "538661740" };
                    remitaPayment = r.GetStatus(remitaPaymentToFetch);
                }
                
                if (remitaPayment != null && !remitaPayment.Description.ToLower().Contains("manual"))
                {
                    remitaPayment = r.GetStatus(remitaPayment.OrderId);
                    if (remitaPayment.TransactionAmount == 10000||remitaPayment.TransactionAmount == 5000 || remitaPayment.TransactionAmount == 12700 || remitaPayment.TransactionAmount == 3950 || remitaPayment.TransactionAmount == 12800 || remitaPayment.TransactionAmount == 2000 || remitaPayment.TransactionAmount==1800)
                    {
                        if (remitaPayment.Status.Contains("01"))
                        {
                            payment = remitaPayment.payment;
                            return payment;
                        }
                        else
                        {
                            SetMessage("Kindly make payment to proceed!", Message.Category.Error);
                            payment = null;
                            return payment;
                        }
                    }
                    else
                    {
                        SetMessage("The pin amount tied to the pin is not correct. Please contact support@lloydant.com.",Message.Category.Error);
                        payment = null;
                        return payment;
                    }
                }
                else if(remitaPayment == null )
                {
                    Session session = new Session(){ Id = 8 };
                    PaymentTerminal paymentTerminal = new PaymentTerminal();
                    PaymentTerminalLogic paymentTerminalLogic = new PaymentTerminalLogic();
                    paymentTerminal = paymentTerminalLogic.GetModelBy(p => p.Fee_Type_Id == 1 && p.Session_Id == session.Id);

                    etranzactDetails = etranzactLogic.RetrievePinAlternative(confirmationOrderNumber,paymentTerminal);
                    if(etranzactDetails != null && etranzactDetails.ReceiptNo != null)
                    {
                        PaymentLogic paymentLogic = new PaymentLogic();
                        payment = paymentLogic.GetModelBy(m => m.Invoice_Number == etranzactDetails.CustomerID);
                        if(payment != null && payment.Id > 0)
                        {
                            FeeDetailLogic feeDetailLogic = new FeeDetailLogic();
                            decimal feeDetail = feeDetailLogic.GetModelsBy(a => a.Fee_Type_Id == payment.FeeType.Id && a.Session_Id == payment.Session.Id).Sum(p => p.Fee.Amount);
                            if (payment.Session.Id == session.Id)
                            {
                                if(!etranzactLogic.ValidatePin(etranzactDetails,payment,feeDetail))
                                {
                                    if (payment.FeeType != null && payment.FeeType.Id == (int)FeeTypes.NDFullTimeApplicationForm && payment.Session.Id == (int)Sessions._20172018 && etranzactDetails.TransactionAmount == 2500M)
                                    {
                                        //do nothing
                                    }
                                    else
                                    {
                                        SetMessage("The pin amount tied to the pin is not correct. Please contact support@lloydant.com.", Message.Category.Error);
                                        payment = null;
                                        return payment;
                                    }
                                }
                            }
                            
                        }
                        else
                        {
                            SetMessage("The invoice number attached to the pin doesn't belong to you! Please cross check and try again.",Message.Category.Error);

                        }
                    }
                    else
                    {
                        SetMessage("Confirmation Order Number entered seems not to be valid! Please cross check and try again.",Message.Category.Error);

                    }
                }
            }
            else
            {
                SessionLogic sessionLogic = new SessionLogic();
                Session session = sessionLogic.GetModelsBy(s => s.Active_For_Application.Value == true).LastOrDefault();
                session = session ?? new Session() {Id = 7};
                //Session session = new Session(){Id=7};
                PaymentLogic paymentLogic = new PaymentLogic();
                payment = paymentLogic.GetModelBy(m => m.Invoice_Number == etranzactDetails.CustomerID);
                if(payment != null && payment.Id > 0)
                {
                    FeeDetail feeDetail = new FeeDetail();
                    FeeDetailLogic feeDetailLogic = new FeeDetailLogic();
                    feeDetail = feeDetailLogic.GetModelsBy(a => a.Fee_Type_Id == payment.FeeType.Id).FirstOrDefault();
                    if (payment.Session.Id == session.Id)
                    {
                         if(!etranzactLogic.ValidatePin(etranzactDetails,payment,feeDetail.Fee.Amount))
                        {
                            if (payment.FeeType != null && payment.FeeType.Id == (int)FeeTypes.NDFullTimeApplicationForm && payment.Session.Id == (int)Sessions._20172018 && etranzactDetails.TransactionAmount == 2500M)
                            {
                                //do nothing
                            }
                            else
                            {
                                SetMessage("The pin amount tied to the pin is not correct. Please contact support@lloydant.com.", Message.Category.Error);
                                payment = null;
                                return payment;
                            }
                        }
                    }
                   
                }
                else
                {
                    SetMessage("The invoice number attached to the pin doesn't belong to you! Please cross check and try again.",Message.Category.Error);

                }
            }

            return payment;
        }
        private void CreateStudentJambDetail(string jambRegNo,Person person)
        {
            try
            {
                if(string.IsNullOrEmpty(jambRegNo))
                {
                    return;
                }

                ApplicantJambDetail jambDetail = new ApplicantJambDetail();
                jambDetail.JambRegistrationNumber = jambRegNo;
                jambDetail.Person = person;

                ApplicantJambDetailLogic studentJambDetailLogic = new ApplicantJambDetailLogic();
                studentJambDetailLogic.Create(jambDetail);
            }
            catch(Exception)
            {
                throw;
            }
        }
        private bool InvalidJambRegistrationNumber(PostJAMBFormPaymentViewModel viewModel)
        {
            const int TEN = 10;
            const string EIGHT = "1";

            try
            {
                if(string.IsNullOrEmpty(viewModel.JambRegistrationNumber))
                {
                    SetMessage("Please enter your JAMB Registration Number!",Message.Category.Error);
                    return true;
                }
                else if(viewModel.JambRegistrationNumber.Length != TEN)
                {
                    SetMessage("JAMB Registration Number must be equal to " + TEN + " digits!",Message.Category.Error);
                    return true;
                }

                int lastAlphabet;
                int secondTolastAlphabet;
                int firstEightNumbers;

                string firstEightCharacters = viewModel.JambRegistrationNumber.Substring(0,8);
                string lastCharacter = viewModel.JambRegistrationNumber.Substring(9,1);
                string secondToLastCharacter = viewModel.JambRegistrationNumber.Substring(8,1);
                string firstCharacter = viewModel.JambRegistrationNumber.Substring(0,1);

                bool lastCharacterIsAlphabet = int.TryParse(lastCharacter,out lastAlphabet);
                bool secondToLastCharacterIsAlphabet = int.TryParse(secondToLastCharacter,out secondTolastAlphabet);
                bool firstEightCharacterIsNumber = int.TryParse(firstEightCharacters,out firstEightNumbers);

                if (firstCharacter != EIGHT)
                {
                    SetMessage("The JAMB Registration Number must start with a 1",Message.Category.Error);
                    return true;
                }

                if(firstEightCharacterIsNumber == false)
                {
                    SetMessage("The first eight characters of JAMB Registration Number must all be numbers! Please modify.",Message.Category.Error);
                    return true;
                }

                if(secondToLastCharacterIsAlphabet && lastCharacterIsAlphabet)
                {
                    SetMessage("The last two characters of JAMB Registration Number must be alphabets! Please modify.",Message.Category.Error);
                    return true;
                }
                else if(lastCharacterIsAlphabet)
                {
                    SetMessage("The last character of JAMB Registration Number must be an alphabet! Please modify.",Message.Category.Error);
                    return true;
                }
                else if(secondToLastCharacterIsAlphabet)
                {
                    SetMessage("The second to the last character of JAMB Registration Number must be an alphabet! Please modify.",Message.Category.Error);
                    return true;
                }

                return false;
            }
            catch(Exception)
            {
                throw;
            }
        }

        public ActionResult GenerateShortFallInvoice()
        {
            PaymentViewModel model = new PaymentViewModel();

            try
            {
                PopulateDropDown(model);

            }

            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);

        }
        [HttpPost]
        public ActionResult GenerateShortFallInvoice(PaymentViewModel model)
        {

            try
            {
                if (model.Student.MatricNumber != null && model.PaymentEtranzact.ConfirmationNo != null && model.FeeType.Id > 0 && model.Session.Id > 0)
                {
                    StudentLogic studentLogic = new StudentLogic();
                    PaymentLogic paymentLogic = new PaymentLogic();
                    PaymentTerminalLogic paymentTerminalLogic = new PaymentTerminalLogic();
                    PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
                    FeeDetailLogic feeDetailLogic = new FeeDetailLogic();
                    StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                    ShortFallLogic shortFallLogic = new ShortFallLogic();
                    OnlinePaymentLogic onlinePaymentLogic = new OnlinePaymentLogic();
                    ApplicationForm applicationForm = new ApplicationForm();
                    ApplicationFormLogic formLogic = new ApplicationFormLogic();
                    AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
                    AdmissionListLogic admissionListLogic = new AdmissionListLogic();

                    decimal invoiceAmount = 0M;
                    double shortFallAmount = 0.0;

                    applicationForm = formLogic.GetModelBy(x => x.Application_Form_Number == model.Student.MatricNumber);
                    if(applicationForm == null)
                    {
                        PopulateDropDown(model);
                        SetMessage("Application Number not found! ", Message.Category.Error);
                        return View(model);
                    }

                    Model.Model.Student student = new Model.Model.Student() { Id = applicationForm.Person.Id };

                    if (applicationForm != null)
                    {
                        //StudentLevel studentLevel = studentLevelLogic.GetModelsBy(s => s.Person_Id == student.Id && s.Session_Id == viewModel.Session.Id).LastOrDefault();
                        AppliedCourse appliedCourse = appliedCourseLogic.GetModelBy(s => s.Application_Form_Id == applicationForm.Id);
                        AdmissionList admissionList = admissionListLogic.GetModelBy(x => x.Application_Form_Id == applicationForm.Id);

                        //FeeDetail feeDetail = new FeeDetail();
                        List<FeeDetail> feeDetail = new List<FeeDetail>();
                        if (model.FeeType.Id == (int)FeeTypes.AcceptanceFee || model.FeeType.Id == (int)FeeTypes.CarryOverSchoolFees || model.FeeType.Id == (int)FeeTypes.SchoolFees)
                        {
                            //feeDetail = feeDetailLogic.GetModelsBy(f => f.Programme_Id == studentLevel.Programme.Id && f.Department_Id == studentLevel.Department.Id && f.Fee_Type_Id == viewModel.FeeType.Id && f.Level_Id == studentLevel.Level.Id && f.Session_Id == viewModel.Session.Id).LastOrDefault();
                            if(admissionList != null)
                            {
                                if (appliedCourse.Programme.Id == (int)Programmes.HNDFullTime)
                                {
                                    feeDetail = feeDetailLogic.GetModelsBy(f => f.Programme_Id == appliedCourse.Programme.Id && f.Department_Id == admissionList.Deprtment.Id && f.Fee_Type_Id == model.FeeType.Id && f.Level_Id == (int)Levels.HNDI && f.Session_Id == model.Session.Id);
                                }
                                else
                                {
                                    feeDetail = feeDetailLogic.GetModelsBy(f => f.Programme_Id == appliedCourse.Programme.Id && f.Department_Id == admissionList.Deprtment.Id && f.Fee_Type_Id == model.FeeType.Id && f.Level_Id == (int)Levels.NDI && f.Session_Id == model.Session.Id && f.Payment_Mode_Id == 1);

                                }
                            }
                            else
                            {
                                if (appliedCourse.Programme.Id == (int)Programmes.HNDFullTime)
                                {
                                    feeDetail = feeDetailLogic.GetModelsBy(f => f.Programme_Id == appliedCourse.Programme.Id && f.Department_Id == appliedCourse.Department.Id && f.Fee_Type_Id == model.FeeType.Id && f.Level_Id == (int)Levels.HNDI && f.Session_Id == model.Session.Id);
                                }
                                else
                                {
                                    feeDetail = feeDetailLogic.GetModelsBy(f => f.Programme_Id == appliedCourse.Programme.Id && f.Department_Id == appliedCourse.Department.Id && f.Fee_Type_Id == model.FeeType.Id && f.Level_Id == (int)Levels.NDI && f.Session_Id == model.Session.Id && f.Payment_Mode_Id == 1);


                                }
                            }
                            
                        }
                        else
                        {
                            feeDetail = feeDetailLogic.GetModelsBy(f => f.Fee_Type_Id == model.FeeType.Id && f.Session_Id == model.Session.Id);
                        }

                        if (model.FeeType.Id == (int)FeeTypes.CarryOverSchoolFees)
                        {
                            StudentExtraYearLogic extraYearLogic = new StudentExtraYearLogic();
                            StudentExtraYearSession extraYear = extraYearLogic.GetModelsBy(e => e.Person_Id == student.Id).LastOrDefault();

                            if (extraYear != null)
                            {
                                int lastSession = Convert.ToInt32(extraYear.LastSessionRegistered.Name.Substring(0, 4));
                                int currentSession = Convert.ToInt32(extraYear.Session.Name.Substring(0, 4));
                                int NoOfOutstandingSession = currentSession - lastSession;
                                if (NoOfOutstandingSession == 0)
                                {
                                    NoOfOutstandingSession = 1;
                                }

                                invoiceAmount = feeDetail.Sum(f => f.Fee.Amount) * NoOfOutstandingSession;
                            }
                            else
                            {
                                invoiceAmount = feeDetail.Sum(f => f.Fee.Amount);
                            }
                        }
                        else
                        {
                            invoiceAmount = feeDetail.Sum(f => f.Fee.Amount);
                        }

                        Payment existingShortFallPayment = paymentLogic.GetModelsBy(p => p.Session_Id == model.Session.Id && p.Fee_Type_Id == (int)FeeTypes.ShortFall && p.Person_Id == student.Id).LastOrDefault();
                        if (existingShortFallPayment != null)
                        {
                            ShortFall existingShortFall = shortFallLogic.GetModelsBy(s => s.Payment_Id == existingShortFallPayment.Id).LastOrDefault();
                            if (existingShortFall != null)
                            {
                                return RedirectToAction("ShortFallInvoice", "Credential", new { area = "Common", pmid = existingShortFallPayment.Id, amount = existingShortFall.Amount });
                            }
                        }

                        //PaymentTerminal paymentTerminal = paymentTerminalLogic.GetModelBy(p => p.Session_Id == viewModel.Session.Id && p.Fee_Type_Id == viewModel.FeeType.Id);
                        //PaymentEtranzact etranzact = paymentEtranzactLogic.RetrieveEtranzactWebServicePinDetails(viewModel.PaymentEtranzact.ConfirmationNo, paymentTerminal);

                        RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                        RemitaPayment remitaPayment = remitaPaymentLogic.GetModelBy(p => p.RRR == model.PaymentEtranzact.ConfirmationNo);

                        if (remitaPayment != null)
                        {
                            Payment existingPayment = paymentLogic.GetModelsBy(p => p.Session_Id == model.Session.Id && p.Fee_Type_Id == model.FeeType.Id && p.Person_Id == student.Id).LastOrDefault();
                            if (existingPayment != null)
                            {
                                if (existingPayment.InvoiceNumber != remitaPayment.payment.InvoiceNumber)
                                {
                                    SetMessage("Confirmation Order Number or RRR not valid for the selected payment type! ", Message.Category.Error);

                                    PopulateDropDown(model);
                                    return View(viewModel);
                                }

                            }

                            if (invoiceAmount > remitaPayment.TransactionAmount)
                            {
                                shortFallAmount = Convert.ToDouble(invoiceAmount - remitaPayment.TransactionAmount);

                                Payment createdPayment = new Payment();

                                using (TransactionScope scope = new TransactionScope())
                                {
                                    Payment payment = new Payment();
                                    payment.DatePaid = DateTime.Now;
                                    payment.FeeType = new FeeType() { Id = (int)FeeTypes.ShortFall };
                                    payment.PaymentMode = new PaymentMode() { Id = (int)PaymentModes.Full };
                                    payment.PaymentType = new PaymentType() { Id = (int)Paymenttypes.OnlinePayment };
                                    payment.Person = new Person() { Id = student.Id };
                                    payment.PersonType = new PersonType() { Id = (int)PersonTypes.Student };
                                    payment.Session = model.Session;

                                    createdPayment = paymentLogic.Create(payment);

                                    if (createdPayment != null)
                                    {
                                        OnlinePayment onlinePaymentCheck = onlinePaymentLogic.GetModelBy(op => op.Payment_Id == createdPayment.Id);
                                        if (onlinePaymentCheck == null)
                                        {
                                            PaymentChannel channel = new PaymentChannel() { Id = (int)PaymentChannel.Channels.Etranzact };
                                            OnlinePayment onlinePayment = new OnlinePayment();
                                            onlinePayment.Channel = channel;
                                            onlinePayment.Payment = createdPayment;

                                            onlinePaymentLogic.Create(onlinePayment);
                                        }
                                    }

                                    ShortFall shortFall = new ShortFall();
                                    shortFall.Amount = shortFallAmount;
                                    shortFall.Payment = createdPayment;

                                    shortFallLogic.Create(shortFall);

                                    //Get Payment Specific Setting
                                    RemitaSettings settings = new RemitaSettings();
                                    RemitaSettingsLogic settingsLogic = new RemitaSettingsLogic();
                                    settings = settingsLogic.GetBy(2);

                                    decimal amt = Convert.ToDecimal(shortFallAmount);

                                    //Get BaseURL
                                    string remitaBaseUrl = ConfigurationManager.AppSettings["RemitaBaseUrl"].ToString();
                                    RemitaPayementProcessor remitaProcessor = new RemitaPayementProcessor(settings.Api_key);
                                    model.RemitaPayment = remitaProcessor.GenerateRRRCard(createdPayment.InvoiceNumber, remitaBaseUrl, "SCHOOL FEES SHORTFALL", settings, amt);
                                    model.Hash = GenerateHash(settings.Api_key, model.RemitaPayment);

                                    if (model.RemitaPayment != null)
                                    {
                                        scope.Complete();
                                    }
                                }

                                return RedirectToAction("ShortFallInvoice", "Credential", new { area = "Common", pmid = createdPayment.Id, amount = shortFallAmount });
                            }
                            else
                            {
                                SetMessage("No shortFall to generate! ", Message.Category.Error);
                            }
                        }
                    }
                    else
                    {
                        SetMessage("Application Number not found! ", Message.Category.Error);
                    }
                }
                else
                {
                    SetMessage("Check the selected fields and try again! ", Message.Category.Error);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            PopulateDropDown(model);

            return View(viewModel);

        }


        public ActionResult PayShortFallFee()
        {
            try
            {
                PaymentViewModel model = new PaymentViewModel();
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult PayShortFallFee(PaymentViewModel model)
        {
            try
            {
                if (model.PaymentEtranzact.ConfirmationNo != null)
                {
                    PaymentLogic paymentLogic = new PaymentLogic();
                    RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();

                    //if (model.PaymentEtranzact.ConfirmationNo.Length > 12)
                    //{
                        //Model.Model.Session session = new Model.Model.Session() { Id = 7 };
                        FeeType feetype = new FeeType() { Id = (int)FeeTypes.ShortFall };
                        RemitaPayment remitaPayment = remitaPaymentLogic.GetModelBy(x => x.RRR == model.PaymentEtranzact.ConfirmationNo);
                    if(remitaPayment == null)
                    {
                        SetMessage("Invalid RRR number", Message.Category.Error);
                        return View(model);
                    }
                    Payment payment = paymentLogic.GetModelBy(x => x.Payment_Id == remitaPayment.payment.Id);

                    //Payment payment = paymentLogic.GetModelBy(model.);
                    if (payment != null && payment.Id > 0)
                        {
                            if (payment.FeeType.Id != (int)FeeTypes.ShortFall)
                            {
                                SetMessage("Confirmation Order Number (" + model.PaymentEtranzact.ConfirmationNo + ") entered is not for shortfall fee payment! Please enter your shortfall fee confirmation order number.", Message.Category.Error);
                                return View(model);
                            }

                            return RedirectToAction("Receipt", "Credential", new { area = "Common", pmid = payment.Id });
                        }
                    //}
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            return View(model);
        }


        private string GenerateHash(string apiKey, RemitaPayment remitaPayment)
        {
            string hashConcatenate = null;
            try
            {
                if (remitaPayment != null)
                {
                    string hash = remitaPayment.MerchantCode + remitaPayment.RRR + apiKey;
                    RemitaPayementProcessor remitaProcessor = new RemitaPayementProcessor(hash);
                    hashConcatenate = remitaProcessor.HashPaymentDetailToSHA512(hash);
                }

                return hashConcatenate;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void PopulateDropDown(PaymentViewModel viewModel)
        {
            try
            {
                //int[] feeTypesToDisplay = { (int)FeeTypes.AcceptanceFee, (int)FeeTypes.CarryOverSchoolFees, (int)FeeTypes.SchoolFees, (int)FeeTypes.HostelFee };
                int[] feeTypesToDisplay = { (int)FeeTypes.AcceptanceFee, (int)FeeTypes.CarryOverSchoolFees, (int)FeeTypes.SchoolFees, (int)FeeTypes.HostelFee };
                List<string> feeTypes = new List<string>();

                for (int i = 0; i < feeTypesToDisplay.Length; i++)
                {
                    feeTypes.Add(feeTypesToDisplay[i].ToString());
                }

                viewModel.FeeTypeSelectListItem = viewModel.FeeTypeSelectListItem.Where(f => f.Value == "" || feeTypes.Contains(f.Value)).ToList();

                if (viewModel.Session != null && viewModel.Session.Id > 0 && viewModel.FeeType != null && viewModel.FeeType.Id > 0)
                {
                    ViewBag.FeeTypes = new SelectList(viewModel.FeeTypeSelectListItem, Utility.VALUE, Utility.TEXT, viewModel.FeeType.Id);
                    ViewBag.Sessions = new SelectList(viewModel.SessionSelectListItem, Utility.VALUE, Utility.TEXT, viewModel.Session.Id);
                }
                else
                {
                    ViewBag.FeeTypes = new SelectList(viewModel.FeeTypeSelectListItem, Utility.VALUE, Utility.TEXT);
                    ViewBag.Sessions = new SelectList(viewModel.SessionSelectListItem, Utility.VALUE, Utility.TEXT);
                }
            }
            catch (Exception)
            {
                throw;
            }

        }
        public ActionResult PostJambFormInvoiceGeneration()
        {
            PostJAMBFormPaymentViewModel postJAMBFormPaymentViewModel = new PostJAMBFormPaymentViewModel();

            try
            {
                ViewBag.StateId = postJAMBFormPaymentViewModel.StateSelectList;
                ViewBag.ProgrammeId = postJAMBFormPaymentViewModel.ApplicationProgrammeSelectListItem;
                ViewBag.DepartmentId = new SelectList(new List<Department>().OrderBy(D => D.Name),ID,NAME);
                ViewBag.DepartmentOptionId = new SelectList(new List<DepartmentOption>(),ID,NAME);
            }
            catch(Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message,Message.Category.Error);
            }

            TempData["PostJAMBFormPaymentViewModel"] = postJAMBFormPaymentViewModel;
            return View(postJAMBFormPaymentViewModel);
        }
        //[HttpPost]
        //public ActionResult PostJambFormInvoiceGeneration(PostJAMBFormPaymentViewModel viewModel)
        //{
        //    try
        //    {
        //        //CheckSelectedProgramme
        //        //int[] currentProgrammes = {(int)Programmes.NDFullTime};
        //        //if (!currentProgrammes.Contains(viewModel.Programme.Id))
        //        //{
        //        //    SetMessage("Form is not on sale for the selected programme!", Message.Category.Error);
        //        //    KeepApplicationFormInvoiceGenerationDropDownState(viewModel);
        //        //    return View(viewModel);
        //        //}

        //        ModelState.Remove("Person.DateOfBirth");
        //        if(ModelState.IsValid)
        //        {
        //            viewModel.Initialise();

        //            if(viewModel.Programme.Id == (int)Programmes.NDFullTime)
        //            {
        //                if(InvalidJambRegistrationNumber(viewModel))
        //                {
        //                    KeepApplicationFormInvoiceGenerationDropDownState(viewModel);
        //                    return View(viewModel);
        //                }
        //            }

        //            if(InvalidDepartmentSelection(viewModel))
        //            {
        //                KeepApplicationFormInvoiceGenerationDropDownState(viewModel);
        //                return View(viewModel);
        //            }

        //            //using (TransactionScope transaction = new TransactionScope())
        //            using(TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required,new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
        //            {
        //                Person person = CreatePerson(viewModel);
        //                Payment payment = CreatePayment(viewModel);

        //                viewModel.Payment = new Payment() { Id = payment.Id,InvoiceNumber = payment.InvoiceNumber,PaymentType = payment.PaymentType,Person = person,FeeDetails = payment.FeeDetails,Session = viewModel.ApplicationFormSetting.Session };

        //                AppliedCourse appliedCourse = CreateAppliedCourse(viewModel);
        //                CreateStudentJambDetail(viewModel.JambRegistrationNumber,person);
        //                //transaction.Complete();
        //                if (payment != null)
        //                {
        //                    //Get Payment Specific Setting
        //                    decimal Amt = 0;
        //                    Amt = payment.FeeDetails.Sum(p => p.Fee.Amount);
        //                    RemitaSettings settings = new RemitaSettings();
        //                    RemitaSettingsLogic settingsLogic = new RemitaSettingsLogic();
        //                    settings = settingsLogic.GetBy(3);

        //                    //Get Split Specific details;
        //                    List<RemitaSplitItems> splitItems = new List<RemitaSplitItems>();
        //                    RemitaSplitItems singleItem = new RemitaSplitItems();
        //                    RemitaSplitItemLogic splitItemLogic = new RemitaSplitItemLogic();
        //                    singleItem = splitItemLogic.GetBy(4);
        //                    singleItem.deductFeeFrom = "1";
        //                    splitItems.Add(singleItem);
        //                    singleItem = splitItemLogic.GetBy(6);
        //                    singleItem.deductFeeFrom = "0";
        //                    singleItem.beneficiaryAmount = Convert.ToString(Amt - Convert.ToDecimal(splitItems[0].beneficiaryAmount));
        //                    splitItems.Add(singleItem);

        //                    //Get BaseURL
        //                    string remitaBaseUrl = ConfigurationManager.AppSettings["RemitaBaseUrl"].ToString();
        //                    Amt = payment.FeeDetails.Sum(a => a.Fee.Amount);

        //                    RemitaPayementProcessor remitaProcessor = new RemitaPayementProcessor(settings.Api_key);
        //                    viewModel.remitaPayment = remitaProcessor.GenerateRRR(payment.InvoiceNumber, remitaBaseUrl, "SALE OF FORMS", splitItems, settings, Amt);
        //                    if (viewModel.remitaPayment != null)
        //                    {
        //                        transaction.Complete();
        //                    }

        //                }

        //            }

        //            TempData["PostJAMBFormPaymentViewModel"] = viewModel;
        //            return RedirectToAction("Invoice","Form");
        //        }
        //    }
        //    catch(Exception ex)
        //    {
        //        SetMessage("Error Occurred! " + ex.Message,Message.Category.Error);
        //    }

        //    KeepApplicationFormInvoiceGenerationDropDownState(viewModel);
        //    return View(viewModel);
        //}
        [HttpPost]
        public ActionResult PostJambFormInvoiceGeneration(PostJAMBFormPaymentViewModel viewModel)
        {
            try
            {
                //switch (viewModel.Programme.Id)
                //{
                    //case (int)Programmes.HNDFullTime:
                    //    int[] includedHndFullTimeDepartments  = {17, 8, 11, 12, 14, 20, 1, 3, 5, 13, 6, 7, 9, 15, 2, 4, 10, 19, 21, 22, 16, 18};
                    //    if (!includedHndFullTimeDepartments.Contains(viewModel.AppliedCourse.Department.Id))
                    //    {
                    //        SetMessage("Form is not on sale for the selected department!", Message.Category.Error);
                    //        KeepApplicationFormInvoiceGenerationDropDownState(viewModel);
                    //        return View(viewModel);
                    //    }
                    //    break;
                    //case (int)Programmes.NDPartTime:
                    //    int[] includedNdPartTimeDepartments = {17, 27, 8, 20, 1, 3, 5, 13, 18, 23, 7, 9, 15};
                    //    if (!includedNdPartTimeDepartments.Contains(viewModel.AppliedCourse.Department.Id))
                    //    {
                    //        SetMessage("Form is not on sale for the selected department!", Message.Category.Error);
                    //        KeepApplicationFormInvoiceGenerationDropDownState(viewModel);
                    //        return View(viewModel);
                    //    }
                    //    break;
                //    case (int)Programmes.NDFullTime:
                //        int[] includedNdFullTimeDepartments = {31,32,34 };
                //        if (!includedNdFullTimeDepartments.Contains(viewModel.AppliedCourse.Department.Id))
                //        {
                //            SetMessage("Form is not on sale for the selected department!", Message.Category.Error);
                //            KeepApplicationFormInvoiceGenerationDropDownState(viewModel);
                //            return View(viewModel);
                //        }
                //        break;
                //    default:
                //        //do nothing
                //        break;
                //}
                
                ModelState.Remove("Person.DateOfBirth");
                ModelState.Remove("FeeType.Name");
                if (ModelState.IsValid)
                {
                    viewModel.Initialise();

                    if (viewModel.Programme.Id == (int)Programmes.NDFullTime)
                    {
                        if (InvalidJambRegistrationNumber(viewModel))
                        {
                            KeepApplicationFormInvoiceGenerationDropDownState(viewModel);
                            return View(viewModel);
                        }
                    }

                    if (InvalidDepartmentSelection(viewModel))
                    {
                        KeepApplicationFormInvoiceGenerationDropDownState(viewModel);
                        return View(viewModel);
                    }

                    using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
                    {
                        Person person = CreatePerson(viewModel);
                        Payment payment = CreatePayment(viewModel);

                        viewModel.Payment = new Payment() { Id = payment.Id, InvoiceNumber = payment.InvoiceNumber, PaymentType = payment.PaymentType, Person = person, FeeDetails = payment.FeeDetails, Session = viewModel.ApplicationFormSetting.Session };

                        AppliedCourse appliedCourse = CreateAppliedCourse(viewModel);
                        CreateStudentJambDetail(viewModel.JambRegistrationNumber, person);

                        transaction.Complete();
                    }

                    TempData["PostJAMBFormPaymentViewModel"] = viewModel;

                    return RedirectToAction("ProcessRemitaPayment");
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            KeepApplicationFormInvoiceGenerationDropDownState(viewModel);
            return View(viewModel);
        }
        public ActionResult ProcessRemitaPayment()
        {
            PostJAMBFormPaymentViewModel viewModel = (PostJAMBFormPaymentViewModel)TempData["PostJAMBFormPaymentViewModel"];
            try
            {
                if (viewModel != null)
                {
                    PaymentLogic paymentLogic = new PaymentLogic();
                    Payment payment = paymentLogic.GetBy(viewModel.Payment.Id);
                    if (payment != null)
                    {
                        //Get Payment Specific Setting
                        decimal Amt = 0;
                        Amt = payment.FeeDetails.Sum(p => p.Fee.Amount);
                        RemitaSettings settings = new RemitaSettings();
                        RemitaSettingsLogic settingsLogic = new RemitaSettingsLogic();
                        settings = settingsLogic.GetBy(3);

                        Remita remita = new Remita()
                        {
                            merchantId = settings.MarchantId,
                            serviceTypeId = settings.serviceTypeId,
                            orderId = payment.InvoiceNumber.ToString(),
                            totalAmount = (decimal)Amt,
                            payerName = payment.Person.FullName,
                            payerEmail = !string.IsNullOrEmpty(payment.Person.Email) ? payment.Person.Email : "test@lloydant.com",
                            payerPhone = payment.Person.MobilePhone,
                            responseurl = "https://applications.federalpolyilaro.edu.ng/Applicant/Form/RemitaResponse",
                            //responseurl = "http://localhost:2600/Applicant/Form/RemitaResponse",
                            paymenttype = "RRRGEN",
                            amt = Amt.ToString()
                        };

                        remita.amt = remita.amt.Split('.')[0];

                        string hash_string = remita.merchantId + remita.serviceTypeId + remita.orderId + remita.amt + remita.responseurl + settings.Api_key;
                        System.Security.Cryptography.SHA512Managed sha512 = new System.Security.Cryptography.SHA512Managed();
                        Byte[] EncryptedSHA512 = sha512.ComputeHash(System.Text.Encoding.UTF8.GetBytes(hash_string));
                        sha512.Clear();
                        string hashed = BitConverter.ToString(EncryptedSHA512).Replace("-", "").ToLower();

                        remita.hash = hashed;

                        viewModel.Remita = remita;
                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            TempData["PostJAMBFormPaymentViewModel"] = viewModel;

            return View(viewModel);
        }
        public ActionResult RemitaResponse(string orderID)
        {
            RemitaResponse remitaResponse = new RemitaResponse();
            PostJAMBFormPaymentViewModel viewModel = (PostJAMBFormPaymentViewModel)TempData["PostJAMBFormPaymentViewModel"];
            if (viewModel == null)
            {
                viewModel = new PostJAMBFormPaymentViewModel();
            }

            try
            {
                string merchant_id = "538661740";
                string apiKey = "918567";
                string hashed;
                string checkstatusurl = ConfigurationManager.AppSettings["RemitaVerifyUrl"].ToString();
                string url;

                if (Request.QueryString["orderID"] != null)
                {
                    PaymentLogic paymentLogic = new PaymentLogic();
                    RemitaPaymentLogic remitaLogic = new RemitaPaymentLogic();
                    AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
                    ApplicationProgrammeFeeLogic programmeFeeLogic = new ApplicationProgrammeFeeLogic();
                    AppliedCourse appliedCourse = new AppliedCourse();
                    ApplicationProgrammeFee programmeFee = new ApplicationProgrammeFee();
                    RemitaPayment remitaPyament = new RemitaPayment();

                    orderID = Request.QueryString["orderID"].ToString();

                    Payment payment = paymentLogic.GetBy(orderID);
                    paymentLogic.SetFeeDetails(payment);

                    if (payment != null)
                    {
                        remitaPyament = remitaLogic.GetModelBy(p => p.Payment_Id == payment.Id);
                        appliedCourse = appliedCourseLogic.GetModelsBy(a => a.Person_Id == payment.Person.Id).LastOrDefault();
                        if (appliedCourse != null)
                        {
                            programmeFee = programmeFeeLogic.GetModelBy(p => p.Fee_Type_Id == payment.FeeType.Id && p.Session_Id == payment.Session.Id && p.Programme_Id == appliedCourse.Programme.Id);
                        }
                    }
                    else
                    {
                        remitaPyament = null;
                    }

                    string hash_string = orderID + apiKey + merchant_id;
                    System.Security.Cryptography.SHA512Managed sha512 = new System.Security.Cryptography.SHA512Managed();
                    Byte[] EncryptedSHA512 = sha512.ComputeHash(System.Text.Encoding.UTF8.GetBytes(hash_string));
                    sha512.Clear();
                    hashed = BitConverter.ToString(EncryptedSHA512).Replace("-", "").ToLower();
                    url = checkstatusurl + "/" + merchant_id + "/" + orderID + "/" + hashed + "/" + "orderstatus.reg";


                    //viewModel.Hash = hashed;

                    if (remitaPyament == null)
                    {
                        string jsondata = new WebClient().DownloadString(url);
                        remitaResponse = JsonConvert.DeserializeObject<RemitaResponse>(jsondata);
                    }

                    if (remitaResponse != null && remitaResponse.Status != null)
                    {
                        if (payment != null)
                        {
                            if (remitaPyament != null)
                            {
                                remitaPyament.Status = remitaResponse.Status + ":" + remitaResponse.StatusCode;
                                remitaLogic.Modify(remitaPyament);
                            }
                            else
                            {
                                remitaPyament = new RemitaPayment();
                                remitaPyament.payment = payment;
                                remitaPyament.RRR = remitaResponse.RRR;
                                remitaPyament.OrderId = remitaResponse.orderId;
                                remitaPyament.Status = remitaResponse.Status;
                                remitaPyament.TransactionAmount = remitaResponse.amount;
                                remitaPyament.TransactionDate = DateTime.Now;
                                remitaPyament.MerchantCode = merchant_id;
                                remitaPyament.Description = "APPLICATION FORM";
                                if (remitaLogic.GetBy(payment.Id) == null)
                                {
                                    remitaLogic.Create(remitaPyament);
                                }
                            }

                            viewModel.remitaPayment = remitaPyament;
                            viewModel.Payment = payment;
                            viewModel.Person = payment.Person;

                        }
                        else
                        {
                            RemitaPayment remitaPayment = remitaLogic.GetBy(orderID);
                            string remitaVerifyUrl = ConfigurationManager.AppSettings["RemitaVerifyUrl"].ToString();
                            RemitaPayementProcessor remitaProcessor = new RemitaPayementProcessor(apiKey);
                            remitaResponse = remitaProcessor.TransactionStatus(remitaVerifyUrl, remitaPayment);
                            if (remitaResponse != null && remitaResponse.Status != null)
                            {
                                remitaPayment.Status = remitaResponse.Status + ":" + remitaResponse.StatusCode;
                                remitaLogic.Modify(remitaPayment);

                                payment = paymentLogic.GetBy(remitaPayment.payment.Id);
                                viewModel.remitaPayment = remitaPayment;
                                if (payment != null)
                                {
                                    viewModel.Payment = payment;
                                    viewModel.Person = payment.Person;
                                }
                            }
                            else
                            {
                                SetMessage("Payment does not exist!", Message.Category.Error);
                            }

                            viewModel.remitaPayment = remitaPayment;
                        }
                        
                        string hash = "538661740" + remitaResponse.RRR + "918567";
                        RemitaPayementProcessor myRemitaProcessor = new RemitaPayementProcessor(hash);
                        viewModel.Hash = myRemitaProcessor.HashPaymentDetailToSHA512(hash);
                    }
                    else
                    {
                        SetMessage("Order ID was not generated from this system", Message.Category.Error);
                    }

                    viewModel.Department = appliedCourse != null ? appliedCourse.Department : new Department();
                    viewModel.ApplicationProgrammeFee = programmeFee != null ? programmeFee : new ApplicationProgrammeFee();
                    viewModel.AppliedCourse = appliedCourse;
                }
                else
                {
                    SetMessage("No data was received!", Message.Category.Error);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            TempData["PostJAMBFormPaymentViewModel"] = viewModel;
            return View(viewModel);
        }
        public ActionResult RRRApplicationForms()
        {
            PostJAMBFormPaymentViewModel postJAMBFormPaymentViewModel = new PostJAMBFormPaymentViewModel();

            try
            {
                ViewBag.StateId = postJAMBFormPaymentViewModel.StateSelectList;
                ViewBag.ProgrammeId = postJAMBFormPaymentViewModel.ProgrammeSelectListItem;
                ViewBag.DepartmentId = new SelectList(new List<Department>().OrderBy(D => D.Name),ID,NAME);
                ViewBag.DepartmentOptionId = new SelectList(new List<DepartmentOption>(),ID,NAME);
            }
            catch(Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message,Message.Category.Error);
            }

            TempData["PostJAMBFormPaymentViewModel"] = postJAMBFormPaymentViewModel;
            return View(postJAMBFormPaymentViewModel);
        }
        [HttpPost]
        public ActionResult RRRApplicationForms(PostJAMBFormPaymentViewModel viewModel)
        {
            try
            {
                ModelState.Remove("Person.DateOfBirth");
                if(ModelState.IsValid)
                {
                    viewModel.Initialise();

                    if(viewModel.Programme.Id == 1)
                    {
                        if(InvalidJambRegistrationNumber(viewModel))
                        {
                            KeepApplicationFormInvoiceGenerationDropDownState(viewModel);
                            return View(viewModel);
                        }
                    }

                    if(InvalidDepartmentSelection(viewModel))
                    {
                        KeepApplicationFormInvoiceGenerationDropDownState(viewModel);
                        return View(viewModel);
                    }

                    //using (TransactionScope transaction = new TransactionScope())
                    using(TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required,new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
                    {
                        Person person = CreatePerson(viewModel);
                        Payment payment = CreatePayment(viewModel);

                        viewModel.Payment = new Payment() { Id = payment.Id,InvoiceNumber = payment.InvoiceNumber,PaymentType = payment.PaymentType,Person = person,FeeDetails = payment.FeeDetails };

                        AppliedCourse appliedCourse = CreateAppliedCourse(viewModel);
                        CreateStudentJambDetail(viewModel.JambRegistrationNumber,person);

                        RemitaPayment remitaPayment = new RemitaPayment();
                        RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                        remitaPayment.RRR = viewModel.remitaPayment.RRR;
                        remitaPayment.payment = payment;
                        remitaPayment.Description = "MANUAL PAYMENT APPLICATION";
                        remitaPayment.OrderId = payment.InvoiceNumber;
                        remitaPayment.Receipt_No = payment.InvoiceNumber;
                        remitaPayment.Status = "01:";
                        remitaPayment.TransactionAmount = payment.FeeDetails.Sum(p => p.Fee.Amount);
                        remitaPayment.TransactionDate = DateTime.Now;
                        remitaPaymentLogic.Create(remitaPayment);

                        transaction.Complete();
                    }

                    TempData["PostJAMBFormPaymentViewModel"] = viewModel;
                    return RedirectToAction("Invoice","Form");

                }
            }
            catch(DbEntityValidationException ex)
            {
                SetMessage("Error Occurred! " + ex.Message,Message.Category.Error);


                foreach(var eve in ex.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name,eve.Entry.State);
                    foreach(var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName,ve.ErrorMessage);
                    }
                }
                throw;


            }

            KeepApplicationFormInvoiceGenerationDropDownState(viewModel);
            return View(viewModel);
        }
        private bool InvalidDepartmentSelection(PostJAMBFormPaymentViewModel viewModel)
        {
            try
            {
                if(viewModel.AppliedCourse.Department == null || viewModel.AppliedCourse.Department.Id <= 0)
                {
                    SetMessage("Please select Department!",Message.Category.Error);
                    return true;
                }
                else if(viewModel.AppliedCourse.Option == null || viewModel.AppliedCourse.Option.Id <= 0)
                {
                    viewModel.DepartmentOptionSelectListItem = Utility.PopulateDepartmentOptionSelectListItem(viewModel.AppliedCourse.Department,viewModel.Programme);
                    if(viewModel.Programme.Id > 2 && viewModel.DepartmentOptionSelectListItem != null && viewModel.DepartmentOptionSelectListItem.Count > 0)
                    {
                        SetMessage("Please select Department Option!",Message.Category.Error);
                        return true;
                    }
                }

                return false;
            }
            catch(Exception)
            {
                throw;
            }
        }
        private void KeepApplicationFormInvoiceGenerationDropDownState(PostJAMBFormPaymentViewModel viewModel)
        {
            try
            {
                if(viewModel.Person.State != null && !string.IsNullOrEmpty(viewModel.Person.State.Id))
                {
                    ViewBag.StateId = new SelectList(viewModel.StateSelectList,VALUE,TEXT,viewModel.Person.State.Id);
                }
                else
                {
                    ViewBag.StateId = new SelectList(viewModel.StateSelectList,VALUE,TEXT);
                }

                if(viewModel.Programme != null && viewModel.Programme.Id > 0)
                {
                    viewModel.DepartmentSelectListItem = Utility.PopulateDepartmentSelectListItem(viewModel.Programme);
                    ViewBag.ProgrammeId = new SelectList(viewModel.ProgrammeSelectListItem,VALUE,TEXT,viewModel.Programme.Id);
                    if(viewModel.AppliedCourse.Department != null && viewModel.AppliedCourse.Department.Id > 0)
                    {

                        viewModel.DepartmentOptionSelectListItem = Utility.PopulateDepartmentOptionSelectListItem(viewModel.AppliedCourse.Department,viewModel.Programme);

                        ViewBag.DepartmentId = new SelectList(viewModel.DepartmentSelectListItem,VALUE,TEXT,viewModel.AppliedCourse.Department.Id);
                        if(viewModel.AppliedCourse.Option != null && viewModel.AppliedCourse.Option.Id > 0)
                        {
                            ViewBag.DepartmentOptionId = new SelectList(viewModel.DepartmentOptionSelectListItem,VALUE,TEXT,viewModel.AppliedCourse.Option.Id);
                        }
                        else
                        {
                            if(viewModel.DepartmentOptionSelectListItem != null && viewModel.DepartmentOptionSelectListItem.Count > 0)
                            {
                                ViewBag.DepartmentOptionId = new SelectList(viewModel.DepartmentOptionSelectListItem,VALUE,TEXT);
                            }
                            else
                            {
                                ViewBag.DepartmentOptionId = new SelectList(new List<DepartmentOption>(),ID,NAME);
                            }
                        }
                    }
                    else
                    {
                        ViewBag.DepartmentId = new SelectList(viewModel.DepartmentSelectListItem,VALUE,TEXT);
                        ViewBag.DepartmentOptionId = new SelectList(new List<DepartmentOption>(),ID,NAME);
                    }
                }
                else
                {
                    ViewBag.ProgrammeId = new SelectList(viewModel.ProgrammeSelectListItem,VALUE,TEXT);
                    ViewBag.DepartmentId = new SelectList(new List<Department>(),ID,NAME);
                    ViewBag.DepartmentOptionId = new SelectList(new List<DepartmentOption>(),ID,NAME);
                }
            }
            catch(Exception)
            {
                throw;
            }
        }

        //private void MaintainAllDropDownSelection(PostJAMBProgrammeViewModel viewModel)
        //{
        //    try
        //    {
        //        ViewBag.ProgrammeId = new SelectList(viewModel.ProgrammeSelectListItem, VALUE, TEXT, viewModel.Programme.Id);

        //        viewModel.DepartmentSelectList = Utility.PopulateDepartmentSelectListItem(viewModel.Programme);
        //        if (viewModel.AppliedCourse.Department != null && viewModel.AppliedCourse.Department.Id > 0)
        //        {
        //            ViewBag.DepartmentId = new SelectList(viewModel.DepartmentSelectList, VALUE, TEXT, viewModel.AppliedCourse.Department.Id);
        //        }
        //        else
        //        {
        //            ViewBag.DepartmentId = viewModel.DepartmentSelectList;
        //        }

        //        //if (viewModel.AppliedCourse.SecondChoiceDepartment != null && viewModel.AppliedCourse.SecondChoiceDepartment.Id > 0)
        //        //{
        //        //    ViewBag.SecondChoiceDepartmentId = new SelectList(viewModel.DepartmentSelectList, VALUE, TEXT, viewModel.AppliedCourse.SecondChoiceDepartment.Id);
        //        //}
        //        //else
        //        //{
        //        //    ViewBag.SecondChoiceDepartmentId = viewModel.DepartmentSelectList;
        //        //    //ViewBag.SecondChoiceDepartmentId = new SelectList(new List<Department>(), VALUE, TEXT, 0);
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
        //    }
        //}

        //[HttpPost]
        //public ActionResult PostJambFormInvoiceGeneration(PostJAMBFormPaymentViewModel viewModel)
        //{
        //    try
        //    {
        //        ModelState.Remove("Person.DateOfBirth");
        //        if (ModelState.IsValid)
        //        {
        //            viewModel.Initialise();

        //            if (viewModel.Programme.Id == 1 || viewModel.Programme.Id == 2)
        //            {
        //                if (InvalidJambRegistrationNumber(viewModel))
        //                {
        //                    ViewBag.ProgrammeId = new SelectList(viewModel.ProgrammeSelectListItem, VALUE, TEXT, viewModel.Programme.Id);
        //                    ViewBag.DepartmentId = new SelectList(new List<Department>(), ID, NAME);
        //                    ViewBag.StateId = new SelectList(viewModel.StateSelectList, VALUE, TEXT, viewModel.Person.State.Id);
        //                    return View(viewModel);
        //                }
        //            }

        //            using (TransactionScope transaction = new TransactionScope())
        //            {
        //                Person person = CreatePerson(viewModel);
        //                Payment payment = CreatePayment(viewModel);

        //                viewModel.Payment = new Payment() { Id = payment.Id, InvoiceNumber = payment.InvoiceNumber, PaymentType = payment.PaymentType, Person = person, FeeDetails = payment.FeeDetails };

        //                AppliedCourse appliedCourse = CreateAppliedCourse(viewModel);
        //                CreateStudentJambDetail(viewModel.JambRegistrationNumber, person);

        //                transaction.Complete();
        //            }

        //            TempData["PostJAMBFormPaymentViewModel"] = viewModel;
        //            return RedirectToAction("Invoice", "Form");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
        //    }

        //    ViewBag.ProgrammeId = new SelectList(viewModel.ProgrammeSelectListItem, VALUE, TEXT, viewModel.Programme.Id);
        //    if (viewModel.Person.State != null && !string.IsNullOrEmpty(viewModel.Person.State.Id))
        //    {
        //        ViewBag.StateId = new SelectList(viewModel.StateSelectList, VALUE, TEXT, viewModel.Person.State.Id);
        //    }
        //    else
        //    {
        //        ViewBag.StateId = new SelectList(viewModel.StateSelectList, VALUE, TEXT);
        //    }

        //    if (viewModel.Programme != null && viewModel.Programme.Id > 0)
        //    {
        //        viewModel.DepartmentSelectListItem = Utility.PopulateDepartmentSelectListItem(viewModel.Programme);
        //        if (viewModel.AppliedCourse.Department != null && viewModel.AppliedCourse.Department.Id > 0)
        //        {
        //            ViewBag.DepartmentId = new SelectList(viewModel.DepartmentSelectListItem, VALUE, TEXT, viewModel.AppliedCourse.Department.Id);
        //        }
        //        else
        //        {
        //            ViewBag.DepartmentId = new SelectList(viewModel.DepartmentSelectListItem, VALUE, TEXT);
        //        }
        //    }
        //    else
        //    {
        //        ViewBag.DepartmentId = new SelectList(new List<Department>(), ID, NAME);
        //    }

        //    return View(viewModel);
        //}

        public ActionResult Invoice()
        {
            PostJAMBFormPaymentViewModel viewModel = (PostJAMBFormPaymentViewModel)TempData["PostJAMBFormPaymentViewModel"];
            DepartmentLogic departmentLogic = new DepartmentLogic();
            viewModel.Department = departmentLogic.GetModelBy(m => m.Department_Id == viewModel.AppliedCourse.Department.Id);

            TempData["PostJAMBFormPaymentViewModel"] = viewModel;
            return View(viewModel);
        }

        public ActionResult PostJambForm()
        {
            PostJambViewModel existingViewModel = (PostJambViewModel)TempData["viewModel"];
            PostJAMBFormPaymentViewModel postJAMBFormPaymentViewModel = (PostJAMBFormPaymentViewModel)TempData["PostJAMBFormPaymentViewModel"];

            try
            {
                PopulateAllDropDowns();
                if(existingViewModel != null)
                {
                    viewModel = existingViewModel;
                    SetStudentUploadedPassport(viewModel);
                }

                if(postJAMBFormPaymentViewModel != null)
                {
                    if(postJAMBFormPaymentViewModel != null && postJAMBFormPaymentViewModel.Programme != null)
                    {
                        viewModel.Session = postJAMBFormPaymentViewModel.CurrentSession;
                        viewModel.Programme = postJAMBFormPaymentViewModel.Programme;
                        viewModel.IsAdmitted = postJAMBFormPaymentViewModel.IsAdmitted;
                        viewModel.IsJambLoaded = postJAMBFormPaymentViewModel.IsJambLoaded;

                        if (viewModel.Programme.Id == (int)Programmes.NDFullTime || viewModel.Programme.Id == (int)Programmes.NDDistance)
                        {
                            viewModel.PreviousEducation = null;
                            viewModel.ApplicantJambDetail.JambRegistrationNumber = postJAMBFormPaymentViewModel.JambRegistrationNumber;
                            viewModel.ApplicantJambDetail = postJAMBFormPaymentViewModel.ApplicantJambDetail;
                        }
                        else if(viewModel.Programme.Id == 2)
                        {
                            viewModel.PreviousEducation = null;
                            //viewModel.ApplicantJambDetail = null;
                            viewModel.ApplicantJambDetail = postJAMBFormPaymentViewModel.ApplicantJambDetail ?? new ApplicantJambDetail();
                        }
                        else if (viewModel.Programme.Id == (int)Programmes.HNDFullTime || viewModel.Programme.Id == (int)Programmes.HNDPartTime || viewModel.Programme.Id == (int)Programmes.HNDWeekend || viewModel.Programme.Id==(int)Programmes.HNDDistance)
                        {
                            viewModel.ApplicantJambDetail = null;
                        }

                        viewModel.Payment = postJAMBFormPaymentViewModel.Payment;
                        viewModel.AppliedCourse = postJAMBFormPaymentViewModel.AppliedCourse;
                        viewModel.ApplicationFormSetting = postJAMBFormPaymentViewModel.ApplicationFormSetting;
                        viewModel.ApplicationProgrammeFee = postJAMBFormPaymentViewModel.ApplicationProgrammeFee;
                        viewModel.remitaPyament = postJAMBFormPaymentViewModel.remitaPayment;
                        viewModel.Person.Id = postJAMBFormPaymentViewModel.Person.Id;
                        viewModel.Person.FirstName = postJAMBFormPaymentViewModel.Person.FirstName;
                        viewModel.Person.LastName = postJAMBFormPaymentViewModel.Person.LastName;
                        viewModel.Person.OtherName = postJAMBFormPaymentViewModel.Person.OtherName;
                        viewModel.Person.FullName = postJAMBFormPaymentViewModel.Person.FullName;
                        viewModel.Person.Email = postJAMBFormPaymentViewModel.Person.Email;
                        viewModel.Person.MobilePhone = postJAMBFormPaymentViewModel.Person.MobilePhone;
                        viewModel.Person.ContactAddress = postJAMBFormPaymentViewModel.Person.ContactAddress;
                        viewModel.Person.DateEntered = postJAMBFormPaymentViewModel.Person.DateEntered;
                        viewModel.Person.State = postJAMBFormPaymentViewModel.Person.State;

                        SetLgaIfExist(viewModel);
                    }
                }

                ApplicationForm applicationform = viewModel.GetApplicationFormBy(viewModel.Person,viewModel.Payment);
                JambOlevel jambOlevel = new JambOlevel();
                JambOlevelLogic jambOlevelLogic = new JambOlevelLogic();
                if (viewModel.ApplicantJambDetail != null && viewModel.ApplicantJambDetail.JambRegistrationNumber != null)
                {
                    jambOlevel = jambOlevelLogic.GetModelsBy(x => x.JAMB_RECORD.Jamb_Registration_Number == viewModel.ApplicantJambDetail.JambRegistrationNumber).LastOrDefault();
                }
                if ((applicationform != null && applicationform.Id > 0) && viewModel.ApplicationAlreadyExist == false)
                {
                    viewModel.ApplicationAlreadyExist = true;

                    viewModel.LoadApplicantionFormBy(viewModel.Person,viewModel.Payment);
                    SetSelectedSittingSubjectAndGrade(viewModel);

                    SetLgaIfExist(viewModel);

                    SetDateOfBirth();
                    if (viewModel.AppliedCourse.Programme.Id == (int)Programmes.HNDFullTime || viewModel.AppliedCourse.Programme.Id == (int)Programmes.HNDPartTime || 
                        viewModel.AppliedCourse.Programme.Id == (int)Programmes.HNDWeekend ||
                        viewModel.AppliedCourse.Programme.Id == (int)Programmes.HNDDistance)
                    {
                        SetPreviousEducationEndDate();
                        SetPreviousEducationStartDate();
                    }
                }

                if (applicationform != null)
                {
                    viewModel.ApplicationForm = applicationform;
                }
                //checking if applicant olevel details exists in JAMB record
                else if (jambOlevel != null && jambOlevel.Id > 0)
                {
                    JambOlevelDetailLogic jambOlevelDetailLogic = new JambOlevelDetailLogic();

                    List<JambOlevelDetail> jambOlevelDetails = jambOlevelDetailLogic.GetModelsBy(x => x.Jamb_O_Level_Id == jambOlevel.Id);
                    if (jambOlevelDetails != null && jambOlevelDetails.Count > 0)
                    {
                        viewModel.SetApplicantOLevelResultFromJamb(jambOlevel, jambOlevelDetails);
                        SetSelectedSittingSubjectAndGrade(viewModel);
                    }
                }

                if ((viewModel.Programme.Id == (int)Programmes.HNDFullTime || viewModel.Programme.Id == (int)Programmes.HNDPartTime || viewModel.Programme.Id == (int)Programmes.HNDWeekend ||
                        viewModel.Programme.Id == (int)Programmes.HNDDistance) && viewModel.ApplicationForm != null && viewModel.ApplicationForm.Id > 0)
                {
                    PreviousEducationLogic previousEducationLogic = new PreviousEducationLogic();
                    PreviousEducation previousEducation = previousEducationLogic.GetModelsBy(p => p.Application_Form_Id == viewModel.ApplicationForm.Id).LastOrDefault();

                    if (previousEducation != null)
                    {
                        
                        viewModel.PreviousEducation.CertificateCopyUrl = CheckIfFileExistsInServer(previousEducation.CertificateCopyUrl)? previousEducation.CertificateCopyUrl:null ;
                        viewModel.PreviousEducation.ResultCopyUrl = CheckIfFileExistsInServer(previousEducation.ResultCopyUrl) ? previousEducation.ResultCopyUrl : null ;
                        viewModel.PreviousEducation.ITLetterOfCompletion = CheckIfFileExistsInServer(previousEducation.ITLetterOfCompletion) ? previousEducation.ITLetterOfCompletion : null;
                        SetPreviousEducationImageUrl(previousEducation);
                    }
                }
                
            }
            catch(Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message,Message.Category.Error);
            }


            TempData["viewModel"] = viewModel;
            TempData["imageUrl"] = viewModel.Person.ImageFileUrl;
            TempData["PostJAMBFormPaymentViewModel"] = postJAMBFormPaymentViewModel;
            return View(viewModel);
        }

      
        public ActionResult CiscoForm()
        {
            PostJambViewModel existingViewModel = (PostJambViewModel)TempData["viewModel"];
            PostJAMBFormPaymentViewModel postJAMBFormPaymentViewModel = (PostJAMBFormPaymentViewModel)TempData["PostJAMBFormPaymentViewModel"];

            try
            {
                existingViewModel = null;
                PopulateAllDropDowns();
               
                if (postJAMBFormPaymentViewModel != null)
                {
                    if (postJAMBFormPaymentViewModel != null && postJAMBFormPaymentViewModel.Programme != null)
                    {
                        viewModel.Session = postJAMBFormPaymentViewModel.CurrentSession;
                        viewModel.Programme = postJAMBFormPaymentViewModel.Programme;

                        if (viewModel.Programme.Id == 1)
                        {
                            viewModel.PreviousEducation = null;
                            viewModel.ApplicantJambDetail.JambRegistrationNumber = postJAMBFormPaymentViewModel.JambRegistrationNumber;
                            viewModel.ApplicantJambDetail = postJAMBFormPaymentViewModel.ApplicantJambDetail;
                        }
                        else if (viewModel.Programme.Id == 2)
                        {
                            viewModel.PreviousEducation = null;
                            viewModel.ApplicantJambDetail = null;
                        }
                        else if (viewModel.Programme.Id == 3 || viewModel.Programme.Id == 4 || viewModel.Programme.Id == 5)
                        {
                            viewModel.ApplicantJambDetail = null;
                        }

                        viewModel.Payment = postJAMBFormPaymentViewModel.Payment;
                        viewModel.AppliedCourse = postJAMBFormPaymentViewModel.AppliedCourse;
                        viewModel.ApplicationFormSetting = postJAMBFormPaymentViewModel.ApplicationFormSetting;
                        viewModel.ApplicationProgrammeFee = postJAMBFormPaymentViewModel.ApplicationProgrammeFee;
                        viewModel.remitaPyament = postJAMBFormPaymentViewModel.remitaPayment;
                        viewModel.Person = postJAMBFormPaymentViewModel.Person;
                        viewModel.Person.ImageFileUrl = !string.IsNullOrEmpty(viewModel.Person.ImageFileUrl) ? viewModel.Person.ImageFileUrl : Utility.DEFAULT_AVATAR;

                        SetLgaIfExist(viewModel);
                    }
                }

                ApplicationForm applicationform = viewModel.GetApplicationFormBy(viewModel.Person, viewModel.Payment);
                if ((applicationform != null && applicationform.Id > 0) && viewModel.ApplicationAlreadyExist == false)
                {
                    viewModel.ApplicationAlreadyExist = true;

                    viewModel.LoadApplicantionFormBy(viewModel.Person, viewModel.Payment);
                    SetSelectedSittingSubjectAndGrade(viewModel);

                    SetLgaIfExist(viewModel);

                    if (viewModel.AppliedCourse.Programme.Id == 3 || viewModel.AppliedCourse.Programme.Id == 4 || viewModel.AppliedCourse.Programme.Id == 5)
                    {
                        SetPreviousEducationEndDate();
                        SetPreviousEducationStartDate();
                    }
                }

                NextOfKinLogic nextOfKinLogic = new NextOfKinLogic();
                NextOfKin nextOfKin = nextOfKinLogic.GetModelsBy(s => s.Person_Id == viewModel.Person.Id).LastOrDefault();
                if (nextOfKin != null)
                {
                    viewModel.Sponsor = new Sponsor();
                    viewModel.Sponsor.Person = viewModel.Person;
                    viewModel.Sponsor.ContactAddress = nextOfKin.ContactAddress;
                    viewModel.Sponsor.Name = nextOfKin.Name;
                    viewModel.Sponsor.MobilePhone = nextOfKin.MobilePhone;
                    viewModel.Sponsor.Relationship = nextOfKin.Relationship;
                }
                ApplicantLogic applicantLogic = new ApplicantLogic();
                viewModel.Applicant = applicantLogic.GetModelsBy(a => a.Person_Id == viewModel.Person.Id).LastOrDefault();

                PreviousEducationLogic previousEducationLogic = new PreviousEducationLogic();
                viewModel.PreviousEducation = previousEducationLogic.GetModelsBy(s => s.Person_Id == viewModel.Person.Id).LastOrDefault();
                if (viewModel.PreviousEducation != null)
                {
                    SetPreviousEducationEndDate();
                    SetPreviousEducationStartDate();
                }

                if (viewModel.AppliedCourse != null && viewModel.AppliedCourse.Option != null)
                {
                    DepartmentOptionLogic optionLogic = new DepartmentOptionLogic();
                    ViewBag.OptionList = new SelectList(optionLogic.GetBy(viewModel.AppliedCourse.Department, viewModel.AppliedCourse.Programme), "Id", "Name");
                }

                if (viewModel.PreviousEducation != null)
                {
                    ViewBag.PreviousEducationEndMonthSelectList = new SelectList(Utility.GetMonthsInYear(), "Id", "Name", viewModel.PreviousEducation.EndDate.Month);
                    ViewBag.PreviousEducationEndYearSelectList = new SelectList(Utility.PopulateYearSelectListItem(1970, true), "Value", "Text", viewModel.PreviousEducation.EndDate.Year);
                    
                }
                else
                {
                    ViewBag.PreviousEducationEndMonthSelectList = new SelectList(Utility.GetMonthsInYear(), "Id", "Name");
                    ViewBag.PreviousEducationEndYearSelectList = new SelectList(Utility.PopulateYearSelectListItem(1970, true), "Value", "Text");
                    ViewBag.PreviousEducationITEndMonthSelectList = new SelectList(Utility.GetMonthsInYear(), "Id", "Name");
                    ViewBag.PreviousEducationITEndYearSelectList = new SelectList(Utility.PopulateYearSelectListItem(1970, true), "Value", "Text");
                    ViewBag.PreviousEducationITStartMonthSelectList = new SelectList(Utility.GetMonthsInYear(), "Id", "Name");
                    ViewBag.PreviousEducationITStartYearSelectList = new SelectList(Utility.PopulateYearSelectListItem(1970, true), "Value", "Text");
                }
                
                if (applicationform != null)
                {
                    viewModel.ApplicationForm = applicationform;
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            TempData["viewModel"] = viewModel;
            //TempData["imageUrl"] = viewModel.Person.ImageFileUrl;
            TempData["PostJAMBFormPaymentViewModel"] = postJAMBFormPaymentViewModel;

            viewModel.Person.ImageFileUrl = !string.IsNullOrEmpty(viewModel.Person.ImageFileUrl) ? viewModel.Person.ImageFileUrl : Utility.DEFAULT_AVATAR;

            return View(viewModel);
        }
        private void SetLgaIfExist(PostJambViewModel viewModel)
        {
            try
            {
                if(viewModel.Person.State != null && !string.IsNullOrEmpty(viewModel.Person.State.Id))
                {
                    LocalGovernmentLogic LocalGovernmentLogic = new LocalGovernmentLogic();
                    List<LocalGovernment> lgas = LocalGovernmentLogic.GetModelsBy(l => l.State_Id == viewModel.Person.State.Id);
                    if(viewModel.Person.LocalGovernment != null && viewModel.Person.LocalGovernment.Id > 0)
                    {
                        ViewBag.LgaId = new SelectList(lgas,ID,NAME,viewModel.Person.LocalGovernment.Id);
                    }
                    else
                    {
                        ViewBag.LgaId = new SelectList(lgas,ID,NAME);
                    }
                }
                else
                {
                    ViewBag.LgaId = new SelectList(new List<LocalGovernment>(),ID,NAME);
                }
            }
            catch(Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message,Message.Category.Error);
            }
        }

        private void SetDateOfBirth()
        {
            try
            {
                if(viewModel.Person.DateOfBirth.HasValue)
                {
                    int noOfDays = DateTime.DaysInMonth(viewModel.Person.YearOfBirth.Id,viewModel.Person.MonthOfBirth.Id);
                    List<Value> days = Utility.CreateNumberListFrom(1,noOfDays);
                    if(days != null && days.Count > 0)
                    {
                        days.Insert(0,new Value() { Name = "--DD--" });
                    }

                    if(viewModel.Person.DayOfBirth != null && viewModel.Person.DayOfBirth.Id > 0)
                    {
                        ViewBag.DayOfBirthId = new SelectList(days,ID,NAME,viewModel.Person.DayOfBirth.Id);
                    }
                    else
                    {
                        ViewBag.DayOfBirthId = new SelectList(days,ID,NAME);
                    }
                }
                else
                {
                    ViewBag.DayOfBirthId = new SelectList(new List<Value>(),ID,NAME);
                }
            }
            catch(Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message,Message.Category.Error);
            }
        }

        private void SetPreviousEducationStartDate()
        {
            try
            {
                if(viewModel.PreviousEducation.StartDate != null)
                {
                    int noOfDays = DateTime.DaysInMonth(viewModel.PreviousEducation.StartYear.Id,viewModel.PreviousEducation.StartMonth.Id);
                    List<Value> days = Utility.CreateNumberListFrom(1,noOfDays);
                    if(days != null && days.Count > 0)
                    {
                        days.Insert(0,new Value() { Name = "--DD--" });
                    }

                    if(viewModel.PreviousEducation.StartDay != null && viewModel.PreviousEducation.StartDay.Id > 0)
                    {
                        ViewBag.PreviousEducationStartDayId = new SelectList(days,ID,NAME,viewModel.PreviousEducation.StartDay.Id);
                    }
                    else
                    {
                        ViewBag.PreviousEducationStartDayId = new SelectList(days,ID,NAME);
                    }
                }
                else
                {
                    ViewBag.PreviousEducationStartDayId = new SelectList(new List<Value>(),ID,NAME);
                }
            }
            catch(Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message,Message.Category.Error);
            }
        }

        private void SetPreviousEducationEndDate()
        {
            try
            {
                if(viewModel.PreviousEducation.EndDate != null)
                {
                    int noOfDays = DateTime.DaysInMonth(viewModel.PreviousEducation.EndYear.Id,viewModel.PreviousEducation.EndMonth.Id);
                    List<Value> days = Utility.CreateNumberListFrom(1,noOfDays);
                    if(days != null && days.Count > 0)
                    {
                        days.Insert(0,new Value() { Name = "--DD--" });
                    }

                    if(viewModel.PreviousEducation.EndDay != null && viewModel.PreviousEducation.EndDay.Id > 0)
                    {
                        ViewBag.PreviousEducationEndDayId = new SelectList(days,ID,NAME,viewModel.PreviousEducation.EndDay.Id);
                    }
                    else
                    {
                        ViewBag.PreviousEducationEndDayId = new SelectList(days,ID,NAME);
                    }
                }
                else
                {
                    ViewBag.PreviousEducationEndDayId = new SelectList(new List<Value>(),ID,NAME);
                }
            }
            catch(Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message,Message.Category.Error);
            }
        }

        [HttpPost]
        public ActionResult PostJambForm(PostJambViewModel viewModel)
        {
            PostJAMBFormPaymentViewModel postJAMBFormPaymentViewModel = (PostJAMBFormPaymentViewModel)TempData["PostJAMBFormPaymentViewModel"];
            TempData.Keep("PostJAMBFormPaymentViewModel");
            TempData.Keep("viewModel");

            try
            {
                SetStudentUploadedPassport(viewModel);

                ModelState["ApplicationProgrammeFee.FeeType.Name"].Errors.Clear();
                ModelState["remitaPyament.payment.Id"].Errors.Clear();


                //foreach (ModelState modelState in ViewData.ModelState.Values)
                //{
                //    foreach (ModelError error in modelState.Errors)
                //    {
                //        SetMessage(error.ErrorMessage, Message.Category.Information);
                //    }
                //}

                var errors = from modelstate in ModelState.AsQueryable().Where(f => f.Value.Errors.Count > 0) select new { Title = modelstate.Key };

                if(ModelState.IsValid)
                {
                    if(InvalidDateOfBirth(viewModel))
                    {
                        SetPostJAMBStateVariables(viewModel,postJAMBFormPaymentViewModel);
                        return View(viewModel);
                    }

                    if(InvalidNumberOfOlevelSubject(viewModel.FirstSittingOLevelResultDetails,viewModel.SecondSittingOLevelResultDetails))
                    {
                        SetPostJAMBStateVariables(viewModel,postJAMBFormPaymentViewModel);
                        return View(viewModel);
                    }

                    if(InvalidOlevelSubjectOrGrade(viewModel.FirstSittingOLevelResultDetails,viewModel.OLevelSubjects,viewModel.OLevelGrades,FIRST_SITTING))
                    {
                        SetPostJAMBStateVariables(viewModel,postJAMBFormPaymentViewModel);
                        return View(viewModel);
                    }

                    if(viewModel.SecondSittingOLevelResult != null)
                    {
                        if(viewModel.SecondSittingOLevelResult.ExamNumber != null && viewModel.SecondSittingOLevelResult.Type != null && viewModel.SecondSittingOLevelResult.Type.Id > 0 && viewModel.SecondSittingOLevelResult.ExamYear > 0)
                        {
                            if(InvalidOlevelSubjectOrGrade(viewModel.SecondSittingOLevelResultDetails,viewModel.OLevelSubjects,viewModel.OLevelGrades,SECOND_SITTING))
                            {
                                SetPostJAMBStateVariables(viewModel,postJAMBFormPaymentViewModel);
                                return View(viewModel);
                            }
                        }
                        //else if (viewModel.SecondSittingOLevelResult.ExamNumber != null || viewModel.SecondSittingOLevelResult.Type.Id > 0 || viewModel.SecondSittingOLevelResult.ExamYear > 0)
                        //{
                        //    SetMessage("One or more fields not set in " + SECOND_SITTING + " header! Please modify and try again.", Message.Category.Error);

                        //    SetPostJAMBStateVariables(viewModel, postJAMBFormPaymentViewModel);
                        //    return View(viewModel);
                        //}
                    }

                    if(InvalidOlevelResultHeaderInformation(viewModel.FirstSittingOLevelResultDetails,viewModel.FirstSittingOLevelResult,FIRST_SITTING))
                    {
                        SetPostJAMBStateVariables(viewModel,postJAMBFormPaymentViewModel);
                        return View(viewModel);
                    }

                    if(InvalidOlevelResultHeaderInformation(viewModel.SecondSittingOLevelResultDetails,viewModel.SecondSittingOLevelResult,SECOND_SITTING))
                    {
                        SetPostJAMBStateVariables(viewModel,postJAMBFormPaymentViewModel);
                        return View(viewModel);
                    }

                    if(NoOlevelSubjectSpecified(viewModel.FirstSittingOLevelResultDetails,viewModel.FirstSittingOLevelResult,FIRST_SITTING))
                    {
                        SetPostJAMBStateVariables(viewModel,postJAMBFormPaymentViewModel);
                        return View(viewModel);
                    }
                    if(NoOlevelSubjectSpecified(viewModel.SecondSittingOLevelResultDetails,viewModel.SecondSittingOLevelResult,SECOND_SITTING))
                    {
                        SetPostJAMBStateVariables(viewModel,postJAMBFormPaymentViewModel);
                        return View(viewModel);
                    }

                    if(InvalidOlevelType(viewModel.FirstSittingOLevelResult.Type, viewModel.SecondSittingOLevelResult.Type))
                    {
                        SetPostJAMBStateVariables(viewModel,postJAMBFormPaymentViewModel);
                        return View(viewModel);
                    }


                    //if (InvalidOlevelResultSubject(viewModel.FirstSittingOLevelResultDetails, viewModel.OLevelSubjects, viewModel.OLevelGrades, FIRST_SITTING))
                    //{
                    //    SetPostJAMBStateVariables(viewModel, postJAMBFormPaymentViewModel);
                    //    return View(viewModel);
                    //}

                    if (viewModel.Programme.Id == (int)Programmes.HNDFullTime || viewModel.Programme.Id == (int)Programmes.HNDPartTime || viewModel.Programme.Id == (int)Programmes.HNDWeekend || viewModel.Programme.Id == (int)Programmes.HNDDistance)
                    {

                        if(InvalidPreviousEducationStartAndEndDate(viewModel.PreviousEducation))
                        {
                            SetPostJAMBStateVariables(viewModel,postJAMBFormPaymentViewModel);
                            return View(viewModel);
                        }

                        if (InvalidPreviousEducationResultUpload(viewModel.PreviousEducation))
                        {
                            SetPostJAMBStateVariables(viewModel, postJAMBFormPaymentViewModel);
                            return View(viewModel);
                        }

                        if (InvalidITDuration(viewModel))
                        {
                            SetPostJAMBStateVariables(viewModel, postJAMBFormPaymentViewModel);
                            return View(viewModel);
                        }
                        if (string.IsNullOrEmpty(viewModel.PreviousEducation.ITLetterOfCompletion))
                        {
                           
                            SetMessage("No IT Letter of Completion uploaded! Please upload your IT Letter of Completion to continue.", Message.Category.Error);
                            SetPostJAMBStateVariables(viewModel, postJAMBFormPaymentViewModel);
                            return View(viewModel);
                        }
                        if (string.IsNullOrEmpty(viewModel.PreviousEducation.ResultCopyUrl) || (string.IsNullOrEmpty(viewModel.PreviousEducation.ResultCopyUrl)))
                        {
                            
                            SetMessage("No ND Credential  uploaded! Please upload your ND Credential to continue.", Message.Category.Error);
                            SetPostJAMBStateVariables(viewModel, postJAMBFormPaymentViewModel);
                            return View(viewModel);
                        }
                        
                    }

                    bool ARCheck = false;
                    for (int i = 0; i < viewModel.FirstSittingOLevelResultDetails.Count; i++)
                    {
                        if (viewModel.FirstSittingOLevelResultDetails[i].Grade.Id == 10)
                        {
                            ARCheck = true;
                        }
                    }
                    for (int i = 0; i < viewModel.SecondSittingOLevelResultDetails.Count; i++)
                    {
                        if (viewModel.SecondSittingOLevelResultDetails[i].Grade.Id == 10)
                        {
                            ARCheck = true;
                        }
                    }
                    if (ARCheck == false)
                    {
                        if (string.IsNullOrEmpty(viewModel.FirstSittingOLevelResult.ScannedCopyUrl) || viewModel.FirstSittingOLevelResult.ScannedCopyUrl == DEFAULT_PASSPORT)
                        {
                            SetMessage("No scanned copy of o-level uploaded! Please upload your first sitting o-level scanned copy to continue.", Message.Category.Error);
                            SetPostJAMBStateVariables(viewModel, postJAMBFormPaymentViewModel);
                            return View(viewModel);
                        }
                        if (viewModel.SecondSittingOLevelResultDetails.Count > 0 && viewModel.SecondSittingOLevelResultDetails.FirstOrDefault().Grade.Id >0)
                        {
                            if (string.IsNullOrEmpty(viewModel.SecondSittingOLevelResult.ScannedCopyUrl) || viewModel.SecondSittingOLevelResult.ScannedCopyUrl == DEFAULT_PASSPORT)
                            {
                                SetMessage("No scanned copy of o-level uploaded! Please upload your second sitting o-level scanned copy to continue.", Message.Category.Error);
                                SetPostJAMBStateVariables(viewModel, postJAMBFormPaymentViewModel);
                                return View(viewModel);
                            }
                        }
                         
                    } 

                    if(string.IsNullOrEmpty(viewModel.Person.ImageFileUrl) || viewModel.Person.ImageFileUrl == DEFAULT_PASSPORT)
                    {
                        SetMessage("No Passport uploaded! Please upload your passport to continue.",Message.Category.Error);
                        SetPostJAMBStateVariables(viewModel,postJAMBFormPaymentViewModel);
                        return View(viewModel);
                    }

                    //viewModel.ApplicantJambDetail = UpdateApplicantJambDetail(postJAMBFormPaymentViewModel);
                    if (postJAMBFormPaymentViewModel != null && postJAMBFormPaymentViewModel.ApplicantJambDetail != null)
                    {
                        if (postJAMBFormPaymentViewModel.ApplicantJambDetail.Subject1 != null)
                        {
                            viewModel.ApplicantJambDetail.Subject1 = GetSubject(postJAMBFormPaymentViewModel.ApplicantJambDetail.Subject1);
                        }
                        else
                        {
                            viewModel.ApplicantJambDetail.Subject1 = GetSubject(viewModel.ApplicantJambDetail.Subject1);
                        }
                        if (postJAMBFormPaymentViewModel.ApplicantJambDetail.Subject2 != null)
                        {
                            viewModel.ApplicantJambDetail.Subject2 = GetSubject(postJAMBFormPaymentViewModel.ApplicantJambDetail.Subject2);
                        }
                        else
                        {
                            viewModel.ApplicantJambDetail.Subject2 = GetSubject(viewModel.ApplicantJambDetail.Subject2);
                        }
                        if (postJAMBFormPaymentViewModel.ApplicantJambDetail.Subject3 != null)
                        {
                            viewModel.ApplicantJambDetail.Subject3 = GetSubject(postJAMBFormPaymentViewModel.ApplicantJambDetail.Subject3);
                        }
                        else
                        {
                            viewModel.ApplicantJambDetail.Subject3 = GetSubject(viewModel.ApplicantJambDetail.Subject3);
                        }
                        if (postJAMBFormPaymentViewModel.ApplicantJambDetail.Subject4 != null)
                        {
                            viewModel.ApplicantJambDetail.Subject4 = GetSubject(postJAMBFormPaymentViewModel.ApplicantJambDetail.Subject4);
                        }
                        else
                        {
                            viewModel.ApplicantJambDetail.Subject4 = GetSubject(viewModel.ApplicantJambDetail.Subject4);
                        } 
                    }  

                    TempData["viewModel"] = viewModel;
                    return RedirectToAction("PostJambPreview","Form");
                }
            }
            catch(Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message,Message.Category.Error);
            }

            SetPostJAMBStateVariables(viewModel,postJAMBFormPaymentViewModel);
            return View(viewModel);
        }

        private bool InvalidITDuration(PostJambViewModel viewModel)
        {
            try
            {
                PreviousEducation previousEducation = viewModel.PreviousEducation;
                if (previousEducation != null && previousEducation.ITDuration != null && previousEducation.ITDuration.Id > 0)
                {
                    TimeSpan timeSpan = new TimeSpan();
                    switch (previousEducation.ITDuration.Id)
                    {
                        case 1:
                            timeSpan = new TimeSpan(365, 0, 0, 0);
                            break;
                        case 2:
                            timeSpan = new TimeSpan(730, 0, 0, 0);
                            break;
                        case 3:
                            timeSpan = new TimeSpan(182, 0, 0, 0);
                            break;
                    }

                    DateTime previousEducationEndDate = new DateTime(previousEducation.EndYear.Id, previousEducation.EndMonth.Id, previousEducation.EndDay.Id);

                    DateTime otherSchoolsPreviousEducationEndDate = new DateTime(2017, 9, 30);
                    DateTime ilaroPreviousEducationEndDate = new DateTime(2018, 3, 31);
                    DateTime ITEndDate = previousEducationEndDate + timeSpan;

                    if (viewModel.Programme != null && viewModel.Programme.Id == 3)
                    {
                        if (previousEducationEndDate > otherSchoolsPreviousEducationEndDate)
                        {
                            if (!string.IsNullOrEmpty(previousEducation.SchoolName) && previousEducation.SchoolName.ToLower().Contains("fed") && previousEducation.SchoolName.ToLower().Contains("poly") &&
                                previousEducation.SchoolName.ToLower().Contains("ilaro"))
                            {
                                TimeSpan nineMonthTimeSpan = new TimeSpan(270, 0, 0, 0);
                                DateTime ITDuration = previousEducationEndDate + nineMonthTimeSpan;
                                if (ITDuration > DateTime.Now)
                                {
                                    SetMessage("The selected previous education end date and the IT duration selected exceeds the date of application.", Message.Category.Error);
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                DateTime ITDuration = previousEducationEndDate + timeSpan;
                                if (ITDuration > DateTime.Now)
                                {
                                    SetMessage("The selected previous education end date and the IT duration selected exceeds the date of application.", Message.Category.Error);
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                                
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else if (viewModel.Programme != null && viewModel.Programme.Id == 5)
                    {
                        if (ITEndDate > DateTime.Now)
                        {
                                SetMessage("The selected previous education end date and the IT duration selected exceeds the date of application.", Message.Category.Error);
                                return true;
                            
                        }
                        else
                        {
                            if (previousEducation.ResultGrade.Id > 3 && previousEducation.ITDuration.Id == 1)
                            {
                                SetMessage("You must possess atleast lower credit to apply for this programe or you should have an IT experience of two years,", Message.Category.Error);
                                return true;
                            }

                            else
                            {
                                return false;
                            }
                        }
                        
                    }
                    //Distant Learning
                    else if (viewModel.Programme != null && viewModel.Programme.Id == 8)
                    {
                        if (ITEndDate > DateTime.Now)
                        {
                            SetMessage("The selected previous education end date and the IT duration selected exceeds the date of application.", Message.Category.Error);
                            return true;

                        }
                        else
                        {
                            if (previousEducation.ResultGrade.Id > 4 && previousEducation.ITDuration.Id == 1)
                            {
                                SetMessage("You must possess atleast lower credit to apply for this programe or you should have an IT experience of two years,", Message.Category.Error);
                                return true;
                            }
                            else if (previousEducation.ResultGrade.Id > 3 && previousEducation.ITDuration.Id == 1)
                            {
                                SetMessage("You must possess atleast lower credit to apply for this programe or you should have an IT experience of two years,", Message.Category.Error);
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }

                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    SetMessage("IT Duration selected is not valid, kindly check and try again.", Message.Category.Error);
                    return true;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        //private ApplicantJambDetail UpdateApplicantJambDetail(PostJAMBFormPaymentViewModel viewModel)
        //{
        //    try
        //    {
        //        ApplicantJambDetailLogic applicantJambDetailLogic = new ApplicantJambDetailLogic();
        //        ApplicantJambDetail applicantJambDetail = new ApplicantJambDetail();
        //        applicantJambDetail = viewModel.ApplicantJambDetail;
        //        applicantJambDetail.Subject1 = GetSubject(viewModel.ApplicantJambDetail.Subject1);
        //        applicantJambDetail.Subject2 = GetSubject(viewModel.ApplicantJambDetail.Subject2);
        //        applicantJambDetail.Subject3 = GetSubject(viewModel.ApplicantJambDetail.Subject3);
        //        applicantJambDetail.Subject4 = GetSubject(viewModel.ApplicantJambDetail.Subject4);

        //        //applicantJambDetailLogic.Modify(applicantJambDetail);

        //        return applicantJambDetail;
        //    }
        //    catch (Exception)
        //    {   
        //        throw;
        //    }
        //}

        private OLevelSubject GetSubject(OLevelSubject oLevelSubject)
        {
            try
            {
                OLevelSubjectLogic oLevelSubjectLogic = new OLevelSubjectLogic();

                if (oLevelSubject == null)
                {
                  return null; 
                }
                OLevelSubject thisOLevelSubject = oLevelSubjectLogic.GetModelBy(ol => ol.O_Level_Subject_Id == oLevelSubject.Id);

                return thisOLevelSubject;
            }
            catch (Exception)
            {  
                throw;
            }
        }

        private bool NoOlevelSubjectSpecified(List<OLevelResultDetail> oLevelResultDetails,OLevelResult oLevelResult,string sitting)
        {
            try
            {
                if(!string.IsNullOrEmpty(oLevelResult.ExamNumber) || (oLevelResult.Type != null && oLevelResult.Type.Id > 0) || (oLevelResult.ExamYear > 0))
                {
                    if(oLevelResultDetails != null && oLevelResultDetails.Count > 0)
                    {
                        List<OLevelResultDetail> oLevelResultDetailsEntered = oLevelResultDetails.Where(r => r.Subject.Id > 0).ToList();
                        if(oLevelResultDetailsEntered == null || oLevelResultDetailsEntered.Count <= 0)
                        {
                            SetMessage("No O-Level Subject specified for " + sitting + "! At least one subject must be specified when Exam Number, O-Level Type and Year are all specified for the sitting.",Message.Category.Error);
                            return true;
                        }
                    }
                }

                return false;
            }
            catch(Exception)
            {
                throw;
            }
        }

        private bool InvalidOlevelType(OLevelType firstSittingOlevelType,OLevelType secondSittingOlevelType)
        {
            try
            {
                if(firstSittingOlevelType != null && secondSittingOlevelType != null)
                {
                    if((firstSittingOlevelType.Id != secondSittingOlevelType.Id) && firstSittingOlevelType.Id > 0 && secondSittingOlevelType.Id > 0)
                    {
                        //if(firstSittingOlevelType.Id == (int)OLevelTypes.Nabteb)
                        //{
                        //    SetMessage("NABTEB O-Level Type in " + FIRST_SITTING + " cannot be combined with any other O-Level Type! Please modify.",Message.Category.Error);
                        //    return true;
                        //}
                        //else if(secondSittingOlevelType.Id == (int)OLevelTypes.Nabteb)
                        //{
                        //    SetMessage("NABTEB O-Level Type in " + SECOND_SITTING + " cannot be combined with any other O-Level Type! Please modify.",Message.Category.Error);
                        //    return true;
                        //}
                    }
                }

                return false;
            }
            catch(Exception)
            {
                throw;
            }
        }

        private bool InvalidPreviousEducationStartAndEndDate(PreviousEducation previousEducation)
        {
            const int ONE_YEAR = 365;

            try
            {
                if(InvalidPreviousEducationStartDate(previousEducation))
                {
                    return true;
                }
                else if(InvalidPreviousEducationEndDate(previousEducation))
                {
                    return true;
                }

                DateTime previousEducationStartDate = new DateTime(previousEducation.StartYear.Id,previousEducation.StartMonth.Id,previousEducation.StartDay.Id);
                DateTime previousEducationEndDate = new DateTime(previousEducation.EndYear.Id,previousEducation.EndMonth.Id,previousEducation.EndDay.Id);

                bool isStartDateInTheFuture = Utility.IsDateInTheFuture(previousEducationStartDate);
                bool isEndDateInTheFuture = Utility.IsDateInTheFuture(previousEducationEndDate);

                if(isStartDateInTheFuture)
                {
                    SetMessage("Previous Education Start Date cannot be a future date!",Message.Category.Error);
                    return true;
                }
                else if(isEndDateInTheFuture)
                {
                    SetMessage("Previous Education End Date cannot be a future date!",Message.Category.Error);
                    return true;
                }
                else if(Utility.IsStartDateGreaterThanEndDate(previousEducationStartDate,previousEducationEndDate))
                {
                    SetMessage("Previous Education Start Date '" + previousEducationStartDate.ToShortDateString() + "' cannot be greater than End Date '" + previousEducationEndDate.ToShortDateString() + "'! Please modify and try again.",Message.Category.Error);
                    return true;
                }
                else if(Utility.IsDateOutOfRange(previousEducationStartDate,previousEducationEndDate,ONE_YEAR))
                {
                    SetMessage("Previous Education duration must not be less than one year, twelve months or 365 days to be qualified!",Message.Category.Error);
                    return true;
                }

                return false;
            }
            catch(Exception)
            {
                throw;
            }
        }

        private bool InvalidPreviousEducationResultUpload(PreviousEducation previousEducation)
        {
            try
            {
                if (string.IsNullOrEmpty(previousEducation.ResultCopyUrl))
                {
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }


        private void SetStudentUploadedPassport(PostJambViewModel viewModel)
        {
            if(viewModel != null && viewModel.Person != null && !string.IsNullOrEmpty((string)TempData["imageUrl"]))
            {
                viewModel.Person.ImageFileUrl = (string)TempData["imageUrl"];
            }
            else
            {
                if (string.IsNullOrEmpty(viewModel.Person.ImageFileUrl))
                {
                    viewModel.Person.ImageFileUrl = DEFAULT_PASSPORT;
                }
                
                //viewModel.PassportUrl = appRoot + DEFAULT_PASSPORT;
            }
            var firstSittingLink = (string)TempData["CredentialimageUrl"];
            var ItLetterimageUrl = (string)TempData["ItLetterimageUrl"];
            if (viewModel.FirstSittingOLevelResult != null && !string.IsNullOrEmpty((string)TempData["CredentialimageUrl"]))
            {
                viewModel.FirstSittingOLevelResult.ScannedCopyUrl = (string)TempData["CredentialimageUrl"];
            }
             if( viewModel.SecondSittingOLevelResult != null && !string.IsNullOrEmpty((string)TempData["CredentialimageUrl2"]))
            {
                viewModel.SecondSittingOLevelResult.ScannedCopyUrl = (string)TempData["CredentialimageUrl2"];
            }

             if (viewModel.PreviousEducation != null && !string.IsNullOrEmpty((string)TempData["CertificateimageUrl"]))
            {
                viewModel.PreviousEducation.ResultCopyUrl = (string)TempData["CertificateimageUrl"];
            }
            if (viewModel.PreviousEducation != null && !string.IsNullOrEmpty((string)TempData["ItLetterimageUrl"]))
            {
                viewModel.PreviousEducation.ITLetterOfCompletion = (string)TempData["ItLetterimageUrl"];
            }

        }
       

        private bool InvalidPreviousEducationStartDate(PreviousEducation previousEducation)
        {
            try
            {
                if(previousEducation.StartYear == null || previousEducation.StartYear.Id <= 0)
                {
                    SetMessage("Please select Previous Education Start Year!",Message.Category.Error);
                    return true;
                }
                else if(previousEducation.StartMonth == null || previousEducation.StartMonth.Id <= 0)
                {
                    SetMessage("Please select Previous Education Start Month!",Message.Category.Error);
                    return true;
                }
                else if(previousEducation.StartDay == null || previousEducation.StartDay.Id <= 0)
                {
                    SetMessage("Please select Previous Education Start Day!",Message.Category.Error);
                    return true;
                }

                return false;
            }
            catch(Exception)
            {
                throw;
            }
        }

        private bool InvalidPreviousEducationEndDate(PreviousEducation previousEducation)
        {
            try
            {
                if(previousEducation.EndYear == null || previousEducation.EndYear.Id <= 0)
                {
                    SetMessage("Please select Previous Education End Year!",Message.Category.Error);
                    return true;
                }
                else if(previousEducation.EndMonth == null || previousEducation.EndMonth.Id <= 0)
                {
                    SetMessage("Please select Previous Education End Month!",Message.Category.Error);
                    return true;
                }
                else if(previousEducation.EndDay == null || previousEducation.EndDay.Id <= 0)
                {
                    SetMessage("Please select Previous Education End Day!",Message.Category.Error);
                    return true;
                }

                return false;
            }
            catch(Exception)
            {
                throw;
            }
        }

        private bool InvalidDateOfBirth(PostJambViewModel viewModel)
        {
            try
            {
                if(viewModel.Person.YearOfBirth == null || viewModel.Person.YearOfBirth.Id <= 0)
                {
                    SetMessage("Please select Year of Birth!",Message.Category.Error);
                    return true;
                }
                else if(viewModel.Person.MonthOfBirth == null || viewModel.Person.MonthOfBirth.Id <= 0)
                {
                    SetMessage("Please select Month of Birth!",Message.Category.Error);
                    return true;
                }
                else if(viewModel.Person.DayOfBirth == null || viewModel.Person.DayOfBirth.Id <= 0)
                {
                    SetMessage("Please select Day of Birth!",Message.Category.Error);
                    return true;
                }

                viewModel.Person.DateOfBirth = new DateTime(viewModel.Person.YearOfBirth.Id,viewModel.Person.MonthOfBirth.Id,viewModel.Person.DayOfBirth.Id);
                if(viewModel.Person.DateOfBirth == null)
                {
                    SetMessage("Please enter Date of Birth!",Message.Category.Error);
                    return true;
                }

                TimeSpan difference = DateTime.Now - (DateTime)viewModel.Person.DateOfBirth;
                if(difference.Days == 0)
                {
                    SetMessage("Date of Birth cannot be todays date!",Message.Category.Error);
                    return true;
                }
                else if(difference.Days == -1)
                {
                    SetMessage("Date of Birth cannot be yesterdays date date!",Message.Category.Error);
                    return true;
                }

                if(difference.Days < 4380)
                {
                    SetMessage("Applicant cannot be less than twelve years!",Message.Category.Error);
                    return true;
                }

                return false;
            }
            catch(Exception)
            {
                throw;
            }
        }

        private void SetPostJAMBStateVariables(PostJambViewModel viewModel,PostJAMBFormPaymentViewModel postJAMBFormPaymentViewModel)
        {
            try
            {
                TempData["viewModel"] = viewModel;
                TempData["PostJAMBFormPaymentViewModel"] = postJAMBFormPaymentViewModel;
                TempData["imageUrl"] = viewModel.Person.ImageFileUrl;
                if(postJAMBFormPaymentViewModel.ApplicantJambDetail != null && !string.IsNullOrEmpty(postJAMBFormPaymentViewModel.ApplicantJambDetail.JambRegistrationNumber))
                {
                    viewModel.ApplicantJambDetail = postJAMBFormPaymentViewModel.ApplicantJambDetail;
                }
                PopulateAllDropDowns();
                TempData["PostJAMBFormPaymentViewModel"] = postJAMBFormPaymentViewModel;
            }
            catch(Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message,Message.Category.Error);
            }
        }

        private bool InvalidOlevelResultHeaderInformation(List<OLevelResultDetail> resultDetails,OLevelResult oLevelResult,string sitting)
        {
            try
            {
                if(resultDetails != null && resultDetails.Count > 0)
                {
                    List<OLevelResultDetail> subjectList = resultDetails.Where(r => r.Subject.Id > 0).ToList();

                    if(subjectList != null && subjectList.Count > 0)
                    {
                        if(string.IsNullOrEmpty(oLevelResult.ExamNumber))
                        {
                            SetMessage("O-Level Exam Number not set for " + sitting + " ! Please modify",Message.Category.Error);
                            return true;
                        }
                        else if(oLevelResult.Type == null || oLevelResult.Type.Id <= 0)
                        {
                            SetMessage("O-Level Exam Type not set for " + sitting + " ! Please modify",Message.Category.Error);
                            return true;
                        }
                        else if(oLevelResult.ExamYear <= 0)
                        {
                            SetMessage("O-Level Exam Year not set for " + sitting + " ! Please modify",Message.Category.Error);
                            return true;
                        }
                        else if (string.IsNullOrEmpty(oLevelResult.ScratchCardPin))
                        {
                            SetMessage("O-Level Scratch Card Pin not set for " + sitting + " ! Please modify", Message.Category.Error);
                            return true;
                        }

                        //if (string.IsNullOrEmpty(oLevelResult.ExamNumber) || oLevelResult.Type == null || oLevelResult.Type.Id <= 0 || oLevelResult.ExamYear <= 0)
                        //{
                        //    SetMessage("Header Information not set for " + sitting + " O-Level Result Details! ", Message.Category.Error);
                        //    return true;
                        //}
                    }
                }

                return false;
            }
            catch(Exception)
            {
                throw;
            }
        }

        private bool InvalidOlevelSubjectOrGrade(List<OLevelResultDetail> oLevelResultDetails,List<OLevelSubject> subjects,List<OLevelGrade> grades,string sitting)
        {
            try
            {
                List<OLevelResultDetail> subjectList = null;
                if(oLevelResultDetails != null && oLevelResultDetails.Count > 0)
                {
                    subjectList = oLevelResultDetails.Where(r => r.Subject.Id > 0 || r.Grade.Id > 0).ToList();
                }

                foreach(OLevelResultDetail oLevelResultDetail in subjectList)
                {
                    OLevelSubject subject = subjects.Where(s => s.Id == oLevelResultDetail.Subject.Id).SingleOrDefault();
                    OLevelGrade grade = grades.Where(g => g.Id == oLevelResultDetail.Grade.Id).SingleOrDefault();

                    List<OLevelResultDetail> results = subjectList.Where(o => o.Subject.Id == oLevelResultDetail.Subject.Id).ToList();
                    if(results != null && results.Count > 1)
                    {
                        SetMessage("Duplicate " + subject.Name.ToUpper() + " Subject detected in " + sitting + "! Please modify.",Message.Category.Error);
                        return true;
                    }
                    else if(oLevelResultDetail.Subject.Id > 0 && oLevelResultDetail.Grade.Id <= 0)
                    {
                        SetMessage("No Grade specified for Subject " + subject.Name.ToUpper() + " in " + sitting + "! Please modify.",Message.Category.Error);
                        return true;
                    }
                    else if(oLevelResultDetail.Subject.Id <= 0 && oLevelResultDetail.Grade.Id > 0)
                    {
                        SetMessage("No Subject specified for Grade" + grade.Name.ToUpper() + " in " + sitting + "! Please modify.",Message.Category.Error);
                        return true;
                    }

                    //else if (oLevelResultDetail.Grade.Id > 0 && oLevelResultDetail.Subject.Id <= 0)
                    //{
                    //    SetMessage("No Grade specified for " + subject.Name.ToUpper() + " for " + sitting + "! Please modify.", Message.Category.Error);
                    //    return true;
                    //}
                    //else if (oLevelResultDetail.Subject.Id <= 0 && oLevelResultDetail.Grade.Id > 0)
                    //{
                    //    SetMessage("No Subject specified for " + grade.Name.ToUpper() + " for " + sitting + "! Please modify.", Message.Category.Error);
                    //    return true;
                    //}
                }

                return false;
            }
            catch(Exception)
            {
                throw;
            }
        }

        private bool InvalidNumberOfOlevelSubject(List<OLevelResultDetail> firstSittingResultDetails,List<OLevelResultDetail> secondSittingResultDetails)
        {
            const int FIVE = 5;

            try
            {
                int totalNoOfSubjects = 0;

                List<OLevelResultDetail> firstSittingSubjectList = null;
                List<OLevelResultDetail> secondSittingSubjectList = null;

                if(firstSittingResultDetails != null && firstSittingResultDetails.Count > 0)
                {
                    firstSittingSubjectList = firstSittingResultDetails.Where(r => r.Subject.Id > 0).ToList();
                    if(firstSittingSubjectList != null)
                    {
                        totalNoOfSubjects += firstSittingSubjectList.Count;
                    }
                }

                if(secondSittingResultDetails != null && secondSittingResultDetails.Count > 0)
                {
                    secondSittingSubjectList = secondSittingResultDetails.Where(r => r.Subject.Id > 0).ToList();
                    if(secondSittingSubjectList != null)
                    {
                        totalNoOfSubjects += secondSittingSubjectList.Count;
                    }
                }

                if(totalNoOfSubjects == 0)
                {
                    SetMessage("No O-Level Result Details found for both sittings!",Message.Category.Error);
                    return true;
                }
                else if(totalNoOfSubjects < FIVE)
                {
                    SetMessage("O-Level Result cannot be less than " + FIVE + " subjects in both sittings!",Message.Category.Error);
                    return true;
                }

                return false;
            }
            catch(Exception)
            {
                throw;
            }
        }

        //private bool InvalidOlevelResultSubject(List<OLevelResultDetail> resultDetails, List<OLevelSubject> subjects, List<OLevelGrade> grades, string sitting)
        //{
        //    try
        //    {
        //        List<OLevelResultDetail>  subjectList = resultDetails.Where(r => r.Subject.Id > 0).ToList();

        //        if (subjectList == null || subjectList.Count == 0)
        //        {
        //            SetMessage("No O-Level Result Details found!", Message.Category.Error);
        //            return true;
        //        }
        //        else if (subjectList.Count < 8)
        //        {
        //            SetMessage("O-Level Result cannot be less than less 8 subjects in " + sitting + "!", Message.Category.Error);
        //            return true;
        //        }

        //        foreach (OLevelResultDetail oLevelResultDetail in subjectList)
        //        {
        //            OLevelSubject subject = subjects.Where(s => s.Id == oLevelResultDetail.Subject.Id).SingleOrDefault();
        //            OLevelGrade grade = grades.Where(g => g.Id == oLevelResultDetail.Grade.Id).SingleOrDefault();

        //            List<OLevelResultDetail> results = subjectList.Where(o => o.Subject.Id == oLevelResultDetail.Subject.Id).ToList();
        //            if (results != null && results.Count > 1)
        //            {
        //                SetMessage("Duplicate " + subject.Name.ToUpper() + " Subject detected in " + sitting + "! Please modify.", Message.Category.Error);
        //                return true;
        //            }
        //            else if (oLevelResultDetail.Subject.Id > 0 && oLevelResultDetail.Grade.Id <= 0)
        //            {
        //                SetMessage("No Grade specified for " + subject.Name.ToUpper() + " for " + sitting + "! Please modify.", Message.Category.Error);
        //                return true;
        //            }
        //            else if (oLevelResultDetail.Subject.Id <= 0 && oLevelResultDetail.Grade.Id > 0)
        //            {
        //                SetMessage("No Subject specified for " + grade.Name.ToUpper() + " for " + sitting + "! Please modify.", Message.Category.Error);
        //                return true;
        //            }
        //        }

        //        return false;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        public ActionResult PostJambPreview()
        {
            PostJambViewModel viewModel = (PostJambViewModel)TempData["viewModel"];

            try
            {
                if(viewModel != null)
                {
                    viewModel.Person.DateOfBirth = new DateTime(viewModel.Person.YearOfBirth.Id,viewModel.Person.MonthOfBirth.Id,viewModel.Person.DayOfBirth.Id);
                    viewModel.Person.State = viewModel.States.Where(m => m.Id == viewModel.Person.State.Id).SingleOrDefault();
                    viewModel.Person.LocalGovernment = viewModel.Lgas.Where(m => m.Id == viewModel.Person.LocalGovernment.Id).SingleOrDefault();
                    viewModel.Person.Sex = viewModel.Genders.Where(m => m.Id == viewModel.Person.Sex.Id).SingleOrDefault();
                    viewModel.Applicant.Ability = viewModel.Abilities.Where(m => m.Id == viewModel.Applicant.Ability.Id).SingleOrDefault();
                    viewModel.Sponsor.Relationship = viewModel.Relationships.Where(m => m.Id == viewModel.Sponsor.Relationship.Id).SingleOrDefault();
                    viewModel.Person.Religion = viewModel.Religions.Where(m => m.Id == viewModel.Person.Religion.Id).SingleOrDefault();

                    if(viewModel.FirstSittingOLevelResult.Type != null)
                    {
                        viewModel.FirstSittingOLevelResult.Type = viewModel.OLevelTypes.Where(m => m.Id == viewModel.FirstSittingOLevelResult.Type.Id).SingleOrDefault();
                    }

                    if (viewModel.SecondSittingOLevelResult.Type != null && viewModel.SecondSittingOLevelResult.Type.Id > 0)
                    {
                        viewModel.SecondSittingOLevelResult.Type = viewModel.OLevelTypes.Where(m => m.Id == viewModel.SecondSittingOLevelResult.Type.Id).SingleOrDefault();
                    }

                    if (viewModel.AppliedCourse.Programme.Id == (int)Programmes.HNDFullTime || viewModel.AppliedCourse.Programme.Id == (int)Programmes.HNDPartTime || 
                        viewModel.AppliedCourse.Programme.Id == (int)Programmes.HNDWeekend || viewModel.AppliedCourse.Programme.Id == (int)Programmes.HNDDistance)
                    {
                        viewModel.PreviousEducation.StartDate = new DateTime(viewModel.PreviousEducation.StartYear.Id,viewModel.PreviousEducation.StartMonth.Id,viewModel.PreviousEducation.StartDay.Id);
                        viewModel.PreviousEducation.EndDate = new DateTime(viewModel.PreviousEducation.EndYear.Id,viewModel.PreviousEducation.EndMonth.Id,viewModel.PreviousEducation.EndDay.Id);
                        viewModel.PreviousEducation.Qualification = viewModel.EducationalQualifications.Where(m => m.Id == viewModel.PreviousEducation.Qualification.Id).SingleOrDefault();
                        viewModel.PreviousEducation.ResultGrade = viewModel.ResultGrades.Where(m => m.Id == viewModel.PreviousEducation.ResultGrade.Id).SingleOrDefault();
                        viewModel.PreviousEducation.ITDuration = viewModel.ITDurations.Where(m => m.Id == viewModel.PreviousEducation.ITDuration.Id).SingleOrDefault();
                    }
                    else
                    {
                        if (viewModel.AppliedCourse.Programme.Id == (int)Programmes.NDFullTime || viewModel.AppliedCourse.Programme.Id == (int)Programmes.NDPartTime || viewModel.AppliedCourse.Programme.Id == (int)Programmes.NDDistance)
                        {
                            viewModel.ApplicantJambDetail.InstitutionChoice = viewModel.InstitutionChoices.Where(m => m.Id == viewModel.ApplicantJambDetail.InstitutionChoice.Id).SingleOrDefault();
                            viewModel.ApplicantJambDetail.Subject1 = viewModel.OLevelSubjects.Where(o => o.Id == viewModel.ApplicantJambDetail.Subject1.Id).SingleOrDefault();
                            viewModel.ApplicantJambDetail.Subject2 = viewModel.OLevelSubjects.Where(o => o.Id == viewModel.ApplicantJambDetail.Subject2.Id).SingleOrDefault();
                            viewModel.ApplicantJambDetail.Subject3 = viewModel.OLevelSubjects.Where(o => o.Id == viewModel.ApplicantJambDetail.Subject3.Id).SingleOrDefault();
                            viewModel.ApplicantJambDetail.Subject4 = viewModel.OLevelSubjects.Where(o => o.Id == viewModel.ApplicantJambDetail.Subject4.Id).SingleOrDefault(); 
                        }  
                    }

                    UpdateOLevelResultDetail(viewModel);
                }
            }
            catch(Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message,Message.Category.Error);
            }

            TempData["viewModel"] = viewModel;
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult PostJambPreview(PostJambViewModel viewModel)
        {
            ApplicationForm newApplicationForm = null;
            PostJambViewModel existingViewModel = (PostJambViewModel)TempData["viewModel"];

            try
            {
                if(viewModel.ApplicationAlreadyExist == false)
                {

                    using(TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required,new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
                    {
                        ApplicationForm applicationForm = new ApplicationForm();
                        ApplicationFormLogic applicationFormLogic = new ApplicationFormLogic();
                        applicationForm.ProgrammeFee = new ApplicationProgrammeFee() { Id = existingViewModel.ApplicationProgrammeFee.Id };
                        applicationForm.Setting = new ApplicationFormSetting() { Id = existingViewModel.ApplicationFormSetting.Id };
                        applicationForm.DateSubmitted = DateTime.Now;
                        applicationForm.Person = viewModel.Person;
                        applicationForm.Payment = viewModel.Payment;
                        applicationForm.ProgrammeFee.Programme = existingViewModel.Programme;

                        applicationForm = applicationFormLogic.Create(applicationForm);
                        existingViewModel.ApplicationFormNumber = applicationForm.Number;
                        applicationForm.Person = existingViewModel.Person;
                        existingViewModel.ApplicationForm = applicationForm;
                        newApplicationForm = applicationForm;
                        existingViewModel.Applicant.Person = viewModel.Person;
                        existingViewModel.Applicant.ApplicationForm = newApplicationForm;
                        existingViewModel.Applicant.Status = new ApplicantStatus() { Id = (int)ApplicantStatus.Status.SubmittedApplicationForm };
                        ApplicantLogic applicantLogic = new ApplicantLogic();
                        applicantLogic.Create(existingViewModel.Applicant);

                        //update application no in applied course object
                        existingViewModel.AppliedCourse.Person = viewModel.Person;
                        existingViewModel.AppliedCourse.ApplicationForm = newApplicationForm;
                        AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
                        appliedCourseLogic.Modify(existingViewModel.AppliedCourse);

                        SponsorLogic sponsorLogic = new SponsorLogic();
                        existingViewModel.Sponsor.ApplicationForm = newApplicationForm;
                        existingViewModel.Sponsor.Person = viewModel.Person;
                        if (sponsorLogic.GetModelsBy(s => s.Person_Id == viewModel.Person.Id).LastOrDefault() == null)
                        {
                            sponsorLogic.Create(existingViewModel.Sponsor);
                        }
                        string CredentialjunkFilePath ="";
                        string CredentialdestinationFilePath="";

                        OLevelResultLogic oLevelResultLogic = new OLevelResultLogic();
                        OLevelResultDetailLogic oLevelResultDetailLogic = new OLevelResultDetailLogic();
                        if(existingViewModel.FirstSittingOLevelResult != null && existingViewModel.FirstSittingOLevelResult.ExamNumber != null && existingViewModel.FirstSittingOLevelResult.Type != null 
                            && existingViewModel.FirstSittingOLevelResult.ExamYear > 0)
                        {
                            SetPersonCredentialDestination(existingViewModel,out CredentialjunkFilePath,out CredentialdestinationFilePath,1);
                            SavePersonPassport(CredentialjunkFilePath,CredentialdestinationFilePath,existingViewModel.Person);
                       
                            existingViewModel.FirstSittingOLevelResult.ApplicationForm = newApplicationForm;
                            existingViewModel.FirstSittingOLevelResult.Person = viewModel.Person;
                            existingViewModel.FirstSittingOLevelResult.Sitting = new OLevelExamSitting() { Id = 1 };
                            OLevelResult firstSittingOLevelResult = oLevelResultLogic.Create(existingViewModel.FirstSittingOLevelResult);

                            if(existingViewModel.FirstSittingOLevelResultDetails != null && existingViewModel.FirstSittingOLevelResultDetails.Count > 0 && firstSittingOLevelResult != null)
                            {
                                List<OLevelResultDetail> olevelResultDetails = existingViewModel.FirstSittingOLevelResultDetails.Where(m => m.Grade != null && m.Subject != null).ToList();
                                foreach(OLevelResultDetail oLevelResultDetail in olevelResultDetails)
                                {
                                    oLevelResultDetail.Header = firstSittingOLevelResult;
                                }

                                oLevelResultDetailLogic.Create(olevelResultDetails);
                            }
                        }

                        if(existingViewModel.SecondSittingOLevelResult != null && existingViewModel.SecondSittingOLevelResult.ExamNumber != null && 
                            existingViewModel.SecondSittingOLevelResult.Type != null && existingViewModel.SecondSittingOLevelResult.ExamYear > 0)
                        {
                             SetPersonCredentialDestination(existingViewModel,out CredentialjunkFilePath,out CredentialdestinationFilePath,2);
                             SavePersonPassport(CredentialjunkFilePath,CredentialdestinationFilePath,existingViewModel.Person);
                       
                            List<OLevelResultDetail> olevelResultDetails = existingViewModel.SecondSittingOLevelResultDetails.Where(m => m.Grade != null && m.Subject != null).ToList();
                            if(olevelResultDetails.Count > 0)
                            {
                                existingViewModel.SecondSittingOLevelResult.ApplicationForm = newApplicationForm;
                                existingViewModel.SecondSittingOLevelResult.Person = viewModel.Person;
                                existingViewModel.SecondSittingOLevelResult.Sitting = new OLevelExamSitting() { Id = 2 };
                                OLevelResult secondSittingOLevelResult = oLevelResultLogic.Create(existingViewModel.SecondSittingOLevelResult);

                                if(existingViewModel.SecondSittingOLevelResultDetails != null && existingViewModel.SecondSittingOLevelResultDetails.Count > 0 && secondSittingOLevelResult != null)
                                {
                                    foreach(OLevelResultDetail oLevelResultDetail in olevelResultDetails)
                                    {
                                        oLevelResultDetail.Header = secondSittingOLevelResult;
                                    }

                                    oLevelResultDetailLogic.Create(olevelResultDetails);
                                }
                            }
                        }

                      

                        if(existingViewModel.Programme.Id == (int)Programmes.NDFullTime)
                        {
                            if(existingViewModel.ApplicantJambDetail != null)
                            {
                                ApplicantJambDetailLogic applicantJambDetailLogic = new ApplicantJambDetailLogic();
                                existingViewModel.ApplicantJambDetail.ApplicationForm = newApplicationForm;
                                existingViewModel.ApplicantJambDetail.Person = viewModel.Person;
                                existingViewModel.ApplicantJambDetail.Subject1 = existingViewModel.ApplicantJambDetail.Subject1;
                                existingViewModel.ApplicantJambDetail.Subject2 = existingViewModel.ApplicantJambDetail.Subject2;
                                existingViewModel.ApplicantJambDetail.Subject3 = existingViewModel.ApplicantJambDetail.Subject3;
                                existingViewModel.ApplicantJambDetail.Subject4 = existingViewModel.ApplicantJambDetail.Subject4;
                                applicantJambDetailLogic.Modify(existingViewModel.ApplicantJambDetail);
                            }
                        }
                        else if (existingViewModel.Programme.Id == (int)Programmes.NDPartTime || existingViewModel.Programme.Id == (int)Programmes.NDDistance)
                        {
                            if (existingViewModel.ApplicantJambDetail != null && existingViewModel.ApplicantJambDetail.JambRegistrationNumber != null)
                            {
                                ApplicantJambDetailLogic applicantJambDetailLogic = new ApplicantJambDetailLogic();
                                if (applicantJambDetailLogic.GetModelsBy(a => a.Person_Id == viewModel.Person.Id).LastOrDefault() == null)
                                {
                                    existingViewModel.ApplicantJambDetail.ApplicationForm = newApplicationForm;
                                    existingViewModel.ApplicantJambDetail.Person = viewModel.Person;
                                    existingViewModel.ApplicantJambDetail.Subject1 = existingViewModel.ApplicantJambDetail.Subject1;
                                    existingViewModel.ApplicantJambDetail.Subject2 = existingViewModel.ApplicantJambDetail.Subject2;
                                    existingViewModel.ApplicantJambDetail.Subject3 = existingViewModel.ApplicantJambDetail.Subject3;
                                    existingViewModel.ApplicantJambDetail.Subject4 = existingViewModel.ApplicantJambDetail.Subject4;
                                    applicantJambDetailLogic.Create(existingViewModel.ApplicantJambDetail);
                                }
                                else
                                {
                                    existingViewModel.ApplicantJambDetail.ApplicationForm = newApplicationForm;
                                    existingViewModel.ApplicantJambDetail.Person = viewModel.Person;
                                    existingViewModel.ApplicantJambDetail.Subject1 = existingViewModel.ApplicantJambDetail.Subject1;
                                    existingViewModel.ApplicantJambDetail.Subject2 = existingViewModel.ApplicantJambDetail.Subject2;
                                    existingViewModel.ApplicantJambDetail.Subject3 = existingViewModel.ApplicantJambDetail.Subject3;
                                    existingViewModel.ApplicantJambDetail.Subject4 = existingViewModel.ApplicantJambDetail.Subject4;
                                    applicantJambDetailLogic.Modify(existingViewModel.ApplicantJambDetail);
                                }
                            }
                            
                        }
                        else if (existingViewModel.Programme.Id == (int)Programmes.HNDFullTime || existingViewModel.Programme.Id == (int)Programmes.HNDPartTime || 
                            existingViewModel.Programme.Id == (int)Programmes.HNDWeekend || existingViewModel.Programme.Id == (int)Programmes.HNDDistance)
                        {
                            if(existingViewModel.PreviousEducation != null)
                            {
                                PreviousEducationLogic previousEducationLogic = new PreviousEducationLogic();
                                existingViewModel.PreviousEducation.ApplicationForm = newApplicationForm;
                                existingViewModel.PreviousEducation.Person = viewModel.Person;
                                
                                string pathForSavingCredentials = Server.MapPath("~/Content/Credential/Credential");

                                if (System.IO.File.Exists(Path.Combine(pathForSavingCredentials, viewModel.Person.Id + "_Certificate.jpg")))
                                {
                                    existingViewModel.PreviousEducation.CertificateCopyUrl = "/Content/Credential/Credential/" + viewModel.Person.Id + "_Certificate.jpg";
                                }

                                if (System.IO.File.Exists(Path.Combine(pathForSavingCredentials, viewModel.Person.Id + "_Result.jpg")))
                                {
                                    existingViewModel.PreviousEducation.ResultCopyUrl = "/Content/Credential/Credential/" + viewModel.Person.Id + "_Result.jpg";
                                }

                                previousEducationLogic.Create(existingViewModel.PreviousEducation);
                            }
                        }

                        //set reject reason
                        if (existingViewModel.Programme.Id == (int)Programmes.HNDFullTime || existingViewModel.Programme.Id == (int)Programmes.HNDPartTime ||
                            existingViewModel.Programme.Id == (int)Programmes.HNDWeekend || existingViewModel.Programme.Id == (int)Programmes.HNDDistance)
                        {
                            newApplicationForm.Release = false;
                            existingViewModel.AppliedCourse.Person = existingViewModel.Person;

                            AdmissionCriteriaLogic admissionCriteriaLogic = new AdmissionCriteriaLogic();
                            string rejectReason = admissionCriteriaLogic.EvaluateApplication(existingViewModel.AppliedCourse,existingViewModel.PreviousEducation);
                            if(string.IsNullOrEmpty(rejectReason))
                            {
                                newApplicationForm.Rejected = false;
                            }
                            else
                            {
                                newApplicationForm.Rejected = true;
                                newApplicationForm.RejectReason = rejectReason;

                                if(!applicationFormLogic.SetRejectReason(newApplicationForm))
                                {
                                    throw new Exception("Rejected! " + rejectReason);
                                }
                            }

                            existingViewModel.ApplicationForm = newApplicationForm;
                        }
                        else
                        {
                            newApplicationForm.Release = true;
                            newApplicationForm.Rejected = false;
                            existingViewModel.AppliedCourse.Person = existingViewModel.Person;

                            //bool checkAR = false;
                            //if (existingViewModel.Programme.Id == 2)
                            //{
                            //    OLevelResultDetailLogic oLevelResultLogic1 = new OLevelResultDetailLogic();
                            //    List<OLevelResultDetail> oLevelResults = oLevelResultLogic1.GetModelsBy(o => o.APPLICANT_O_LEVEL_RESULT.Application_Form_Id == existingViewModel.AppliedCourse.ApplicationForm.Id);
                            //    for (int i = 0; i < oLevelResults.Count; i++)
                            //    {
                            //        if (oLevelResults[i].Grade.Id == 10)
                            //        {
                            //            checkAR = true;
                            //        }
                            //    }
                            //}

                            //if (!checkAR)
                            //{
                            //    AdmissionCriteriaLogic admissionCriteriaLogic = new AdmissionCriteriaLogic();
                            //    string rejectReason = admissionCriteriaLogic.EvaluateApplication(existingViewModel.AppliedCourse);
                            //    if (string.IsNullOrEmpty(rejectReason))
                            //    {
                            //        newApplicationForm.Rejected = false;
                            //    }
                            //    else
                            //    {
                            //        newApplicationForm.Rejected = true;
                            //        newApplicationForm.RejectReason = rejectReason;

                            //        if (!applicationFormLogic.SetRejectReason(newApplicationForm))
                            //        {
                            //            if (existingViewModel.Programme.Id == 2)
                            //            {
                            //                //
                            //            }
                            //            else
                            //            {
                            //                throw new Exception("Rejected! " + rejectReason);
                            //            }
                                        
                            //        }
                            //    }
                            //}
                            applicationFormLogic.SetRejectReason(newApplicationForm);
                            existingViewModel.ApplicationForm = newApplicationForm;
                        }

                        PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
                        paymentEtranzactLogic.UpdatePin(existingViewModel.Payment,existingViewModel.Person);

                        string junkFilePath;
                        string destinationFilePath;
                        SetPersonPassportDestination(existingViewModel,out junkFilePath,out destinationFilePath);
                        
                        PersonLogic personLogic = new PersonLogic();
                        bool personModified = personLogic.Modify(existingViewModel.Person);
                        if(personModified)
                        {
                            SavePersonPassport(junkFilePath,destinationFilePath,existingViewModel.Person);
                            transaction.Complete();

                            //SendSms(existingViewModel.ApplicationForm,existingViewModel.Programme);
                            SendSmartSms(existingViewModel.ApplicationForm, existingViewModel.Programme);
                            TempData["viewModel"] = existingViewModel;
                            return RedirectToAction("PostJambSlip","Form");
                        }
                        else
                        {
                            throw new Exception("Passport save operation failed! Please try again.");
                        }
                    }
                }
                //else if (existingViewModel.Programme.Id == 3)
                //{
                //    existingViewModel.ApplicationFormNumber = viewModel.ApplicationForm.Number;
                //    TempData["viewModel"] = existingViewModel;
                //    return RedirectToAction("PostJambSlip", "Form"); 
                //}
                else
                {
                    ApplicationFormLogic applicationFormLogic = new ApplicationFormLogic();
                    newApplicationForm = applicationFormLogic.GetModelsBy(a => a.Application_Form_Number == viewModel.ApplicationForm.Number).LastOrDefault();
                    existingViewModel.ApplicationFormNumber = viewModel.ApplicationForm.Number;
                    AdmissionListLogic admissionListLogic = new AdmissionListLogic();
                    AdmissionList isAdmitted = null;
                    if (existingViewModel?.ApplicationForm?.Id>0)
                    isAdmitted = admissionListLogic.GetModelsBy(f => f.Application_Form_Id == existingViewModel.ApplicationForm.Id).FirstOrDefault();
                    using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
                    {
                        
                        //applicant can only modify olevel result if havent been offered admission
                        if (existingViewModel.Programme.Id == (int)Programmes.NDFullTime || existingViewModel.Programme.Id == (int)Programmes.NDDistance)
                        {
                            if (existingViewModel.ApplicantJambDetail != null  && isAdmitted==null)
                            {
                                ApplicantJambDetailLogic applicantJambDetailLogic = new ApplicantJambDetailLogic();
                                existingViewModel.ApplicantJambDetail.ApplicationForm = newApplicationForm;
                                existingViewModel.ApplicantJambDetail.Person = viewModel.Person;
                                existingViewModel.ApplicantJambDetail.Subject1 = existingViewModel.ApplicantJambDetail.Subject1;
                                existingViewModel.ApplicantJambDetail.Subject2 = existingViewModel.ApplicantJambDetail.Subject2;
                                existingViewModel.ApplicantJambDetail.Subject3 = existingViewModel.ApplicantJambDetail.Subject3;
                                existingViewModel.ApplicantJambDetail.Subject4 = existingViewModel.ApplicantJambDetail.Subject4;
                                applicantJambDetailLogic.Modify(existingViewModel.ApplicantJambDetail);
                            }
                        }
                        else if (existingViewModel.Programme.Id == (int)Programmes.NDPartTime)
                        {
                            if (existingViewModel.ApplicantJambDetail != null && existingViewModel.ApplicantJambDetail.JambRegistrationNumber != null)
                            {
                                ApplicantJambDetailLogic applicantJambDetailLogic = new ApplicantJambDetailLogic();
                                if (applicantJambDetailLogic.GetModelsBy(a => a.Person_Id == viewModel.Person.Id).LastOrDefault() == null)
                                {
                                    existingViewModel.ApplicantJambDetail.ApplicationForm = newApplicationForm;
                                    existingViewModel.ApplicantJambDetail.Person = viewModel.Person;
                                    existingViewModel.ApplicantJambDetail.Subject1 = existingViewModel.ApplicantJambDetail.Subject1;
                                    existingViewModel.ApplicantJambDetail.Subject2 = existingViewModel.ApplicantJambDetail.Subject2;
                                    existingViewModel.ApplicantJambDetail.Subject3 = existingViewModel.ApplicantJambDetail.Subject3;
                                    existingViewModel.ApplicantJambDetail.Subject4 = existingViewModel.ApplicantJambDetail.Subject4;
                                    applicantJambDetailLogic.Create(existingViewModel.ApplicantJambDetail);
                                }
                                else
                                {
                                    if(isAdmitted == null)
                                    {
                                        existingViewModel.ApplicantJambDetail.ApplicationForm = newApplicationForm;
                                        existingViewModel.ApplicantJambDetail.Person = viewModel.Person;
                                        existingViewModel.ApplicantJambDetail.Subject1 = existingViewModel.ApplicantJambDetail.Subject1;
                                        existingViewModel.ApplicantJambDetail.Subject2 = existingViewModel.ApplicantJambDetail.Subject2;
                                        existingViewModel.ApplicantJambDetail.Subject3 = existingViewModel.ApplicantJambDetail.Subject3;
                                        existingViewModel.ApplicantJambDetail.Subject4 = existingViewModel.ApplicantJambDetail.Subject4;
                                        applicantJambDetailLogic.Modify(existingViewModel.ApplicantJambDetail);
                                    }
                                    
                                }
                            }
                            
                        }

                        SponsorLogic sponsorLogic = new SponsorLogic();
                        existingViewModel.Sponsor.Person = viewModel.Person;

                        if (sponsorLogic.GetModelsBy(s => s.Person_Id == viewModel.Person.Id).LastOrDefault() == null)
                        {
                            existingViewModel.Sponsor.ApplicationForm = newApplicationForm;
                            sponsorLogic.Create(existingViewModel.Sponsor);
                        }
                        else
                        {
                            sponsorLogic.Modify(existingViewModel.Sponsor);
                        }

                        PersonLogic personLogic = new PersonLogic();
                        bool personModified = personLogic.Modify(existingViewModel.Person);


                        string CredentialjunkFilePath="";
                        string CredentialdestinationFilePath="";
                        if (existingViewModel.FirstSittingOLevelResult.Id > 0)
                        {
                           SetPersonCredentialDestination(existingViewModel,out CredentialjunkFilePath,out CredentialdestinationFilePath,1);
                           SavePersonPassport(CredentialjunkFilePath,CredentialdestinationFilePath,existingViewModel.Person);
                       
                        }
                        if (existingViewModel.SecondSittingOLevelResult.Id > 0)
                        {
                            SetPersonCredentialDestination(existingViewModel, out CredentialjunkFilePath,out CredentialdestinationFilePath, 2);
                            SavePersonPassport(CredentialjunkFilePath,CredentialdestinationFilePath,existingViewModel.Person);
                       
                        }
                        

                        //MODIFY O-LEVEL
                        existingViewModel.SecondSittingOLevelResult.ApplicationForm = existingViewModel.ApplicationForm;
                        existingViewModel.SecondSittingOLevelResult.Person = existingViewModel.Person;
                        existingViewModel.SecondSittingOLevelResult.PersonType =new PersonType(){Id = 4};
                        existingViewModel.SecondSittingOLevelResult.Sitting =new OLevelExamSitting(){Id = 2};

                        if (isAdmitted == null)//cannot modify record after admission  
                        {
                            ModifyOlevelResult(existingViewModel.FirstSittingOLevelResult, existingViewModel.FirstSittingOLevelResultDetails);
                            ModifyOlevelResult(existingViewModel.SecondSittingOLevelResult, existingViewModel.SecondSittingOLevelResultDetails);
                        }

                        
                           
                        //set resject reason
                        if (existingViewModel.Programme.Id == (int)Programmes.HNDFullTime || existingViewModel.Programme.Id == (int)Programmes.HNDPartTime || 
                            existingViewModel.Programme.Id == (int)Programmes.HNDWeekend || existingViewModel.Programme.Id == (int)Programmes.HNDDistance)
                        {
                            if(existingViewModel.PreviousEducation != null && existingViewModel.PreviousEducation.SchoolName != null)
                            {
                                PreviousEducationLogic previousEducationLogic = new PreviousEducationLogic();
                                PreviousEducation previousEducation = previousEducationLogic.GetModelBy(p => p.Application_Form_Id == existingViewModel.ApplicationForm.Id);
                                if(previousEducation != null && isAdmitted==null)//cannot modify record after admission 
                                {
                                    existingViewModel.PreviousEducation.Id = previousEducation.Id;
                                    existingViewModel.PreviousEducation.ApplicationForm = existingViewModel.ApplicationForm;
                                    existingViewModel.PreviousEducation.Person = existingViewModel.Person;
                                    previousEducationLogic.Modify(existingViewModel.PreviousEducation);
                                }

                            }

                            newApplicationForm.Release = false;
                            existingViewModel.AppliedCourse.Person = existingViewModel.Person;

                            AdmissionCriteriaLogic admissionCriteriaLogic = new AdmissionCriteriaLogic();
                            string rejectReason = admissionCriteriaLogic.EvaluateApplication(existingViewModel.AppliedCourse,existingViewModel.PreviousEducation);
                            if(string.IsNullOrEmpty(rejectReason))
                            {
                                newApplicationForm.Rejected = false;
                                newApplicationForm.RejectReason = null;
                            }
                            else
                            {
                                newApplicationForm.Rejected = true;
                                newApplicationForm.RejectReason = rejectReason;

                                if(!applicationFormLogic.SetRejectReason(newApplicationForm))
                                {
                                    throw new Exception("Rejected! " + rejectReason);
                                }
                            }

                            applicationFormLogic.Modify(newApplicationForm);

                            existingViewModel.ApplicationForm = newApplicationForm;
                        }
                        else
                        {
                           newApplicationForm.Release = true;
                           newApplicationForm.Rejected = false;
                           AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
                           existingViewModel.AppliedCourse = appliedCourseLogic.GetBy(existingViewModel.Person);

                            //bool checkAR = false;
                            //if (existingViewModel.Programme.Id == 2)
                            //{
                            //    OLevelResultDetailLogic oLevelResultLogic = new OLevelResultDetailLogic();
                            //    List<OLevelResultDetail> oLevelResults = oLevelResultLogic.GetModelsBy(o => o.APPLICANT_O_LEVEL_RESULT.Application_Form_Id == existingViewModel.AppliedCourse.ApplicationForm.Id);
                            //    for (int i = 0; i < oLevelResults.Count; i++)
                            //    {
                            //        if (oLevelResults[i].Grade.Id == 10)
                            //        {
                            //            checkAR = true;
                            //        }
                            //    } 
                            //}
                            
                            //if (!checkAR)
                            //{
                            //    AdmissionCriteriaLogic admissionCriteriaLogic = new AdmissionCriteriaLogic();
                            //    string rejectReason = admissionCriteriaLogic.EvaluateApplication(existingViewModel.AppliedCourse);
                            //    if (string.IsNullOrEmpty(rejectReason))
                            //    {
                            //        newApplicationForm.Rejected = false;
                            //        newApplicationForm.RejectReason = null;
                            //    }
                            //    else
                            //    {
                            //        newApplicationForm.Rejected = true;
                            //        newApplicationForm.RejectReason = rejectReason;

                            //        if (!applicationFormLogic.SetRejectReason(newApplicationForm))
                            //        {
                            //            throw new Exception("Rejected! " + rejectReason);
                            //        }
                            //    } 
                            //}

                            applicationFormLogic.Modify(newApplicationForm);

                            existingViewModel.ApplicationForm = newApplicationForm;
                        }
                        transaction.Complete();
                    }

                    existingViewModel.ApplicationFormNumber = viewModel.ApplicationForm.Number;
                    TempData["viewModel"] = existingViewModel;
                    return RedirectToAction("PostJambSlip","Form");
                }

                //get newly created application form


            }
            catch(Exception ex)
            {
                newApplicationForm = null;
                viewModel.ApplicationForm = null;
                existingViewModel.ApplicationForm = null;

                SetMessage("Error Occurred! " + ex.Message,Message.Category.Error);
            }

            TempData["viewModel"] = existingViewModel;
            return View(existingViewModel);
        }

        public ActionResult PostJAMBSlip(PostJambViewModel viewModel)
        {
            PostJambViewModel existingViewModel = (PostJambViewModel)TempData["viewModel"];
            if (existingViewModel == null || existingViewModel.ApplicationForm == null || existingViewModel.ApplicationForm.Setting == null)
            {
                SetMessage("Application has been submitted and your Application Number has been generated, kindly enter your RRR and proceed to print your acknowledgment slip.", 
                    Message.Category.Information);
                return RedirectToAction("PostJambProgramme");
            }
            if (existingViewModel != null && existingViewModel.ApplicationForm != null)
            {
                existingViewModel.Session = existingViewModel.ApplicationForm.Setting.Session;
            }
            TempData["viewModel"] = existingViewModel;
            return View(existingViewModel);
        }
        public ActionResult AcknowledgmentSlip(string formId)
        {
            viewModel = new PostJambViewModel();
            try
            {
                //long Fid = Convert.ToInt64(Utility.Decrypt(formId));
                long Fid = Convert.ToInt64(formId);
                //long _Fid = Utility.Decrypt(formId);

                var applicationFormSettingLogic = new ApplicationFormSettingLogic();
                var applicationFormLogic = new ApplicationFormLogic();
                var appliedCourseLogic = new AppliedCourseLogic();
                //var oLevelResultLogic = new OLevelResultLogic();
                //var oLevelResultDetailLogic = new OLevelResultDetailLogic();
                var applicantJambDetailLogic = new ApplicantJambDetailLogic();
                var personLogic = new PersonLogic();
                PreviousEducationLogic previousEducationLogic = new PreviousEducationLogic();
                EducationalQualificationLogic educationQualificationLogic = new EducationalQualificationLogic();
                RankingDataLogic rankingDataLogic = new RankingDataLogic();
                viewModel.ApplicationForm = applicationFormLogic.GetBy(Fid);
                if (viewModel.ApplicationForm != null && viewModel.ApplicationForm.Id > 0)
                {
                    viewModel.ApplicationFormNumber = viewModel.ApplicationForm.Number;
                    viewModel.ApplicationFormSetting = applicationFormSettingLogic.GetModelBy(a => a.Application_Form_Setting_Id == viewModel.ApplicationForm.Setting.Id);
                    viewModel.Session = viewModel.ApplicationFormSetting.Session;
                    viewModel.Person = personLogic.GetModelBy(a => a.Person_Id == viewModel.ApplicationForm.Person.Id);
                    viewModel.AppliedCourse = appliedCourseLogic.GetBy(viewModel.Person);
                    viewModel.Programme = viewModel.AppliedCourse.Programme;
                    viewModel.ApplicantJambDetail = applicantJambDetailLogic.GetModelsBy(s => s.Person_Id == viewModel.Person.Id && s.Application_Form_Id == viewModel.ApplicationForm.Id).LastOrDefault();
                    viewModel.ApplicantApplicationApproval = new ApplicantApplicationApproval();
                    if (viewModel.ApplicantJambDetail != null)
                    {
                        OLevelSubject emptySubject = new OLevelSubject() { Id = 0, Name = "" };
                        if (viewModel.ApplicantJambDetail.Subject1 == null)
                        {
                            viewModel.ApplicantJambDetail.Subject1 = emptySubject;
                        }
                        if (viewModel.ApplicantJambDetail.Subject2 == null)
                        {
                            viewModel.ApplicantJambDetail.Subject2 = emptySubject;
                        }
                        if (viewModel.ApplicantJambDetail.Subject3 == null)
                        {
                            viewModel.ApplicantJambDetail.Subject3 = emptySubject;
                        }
                        if (viewModel.ApplicantJambDetail.Subject4 == null)
                        {
                            viewModel.ApplicantJambDetail.Subject4 = emptySubject;
                        }
                    }

                    if (viewModel.PreviousEducation != null && viewModel.PreviousEducation.Id > 0)
                    {
                        viewModel.PreviousEducation = previousEducationLogic.GetModelBy(pe => pe.Application_Form_Id == viewModel.ApplicationForm.Id);
                        EducationalQualification qualification = educationQualificationLogic.GetModelBy(q => q.Educational_Qualification_Id == viewModel.PreviousEducation.Qualification.Id);
                        viewModel.PreviousEducation.Qualification = qualification;

                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            TempData["viewModel"] = viewModel;

            return View("PostJAMBSlip", viewModel);
        }
        public ActionResult ExamSlip()
        {
            PostJambViewModel existingViewModel = (PostJambViewModel)TempData["viewModel"];

            TempData["viewModel"] = existingViewModel;
            return View(existingViewModel);
        }

        private void SendSms(ApplicationForm applicationForm,Programme programme)
        {
            try
            {
                //send sms to applicant
                //Sms textMessage = new Sms();
                //textMessage.Sender = "";

                string message = "";
                string number = "234" + applicationForm.Person.MobilePhone;
                if(applicationForm.Rejected == false)
                {
                    message = "Hello " + applicationForm.Person.LastName + ", Your application for Admission into the " + programme.ShortName + " programme has been received. Your application no is " + applicationForm.Number + " Thanks";
                }
                else
                {
                    message = "Hello " + applicationForm.Person.LastName + ", Your application for Admission into the " + programme.ShortName + " programme has been rejected. You failed to meet the entry criteria. Thanks";
                }

                // textMessage.SendSMS(number, message);

                InfoBipSMS smsClient = new InfoBipSMS();
                InfoSMS smsMessage = new InfoSMS();
                smsMessage.from = "POLYILARO";
                smsMessage.to = number;
                smsMessage.text = message;
                smsClient.SendSMS(smsMessage);
            }
            catch(Exception)
            {
                //do nothing
            }
        }



        private void SavePersonPassport(string junkFilePath,string pathForSaving,Person person)
        {
            try
            {
                if (System.IO.File.Exists(junkFilePath))
                {
                    if(junkFilePath == pathForSaving){
                        return;
                    }

                    string folderPath = Path.GetDirectoryName(pathForSaving);
                    string mainFileName = person.Id.ToString() + "__";

                    DeleteFileIfExist(folderPath,mainFileName);

                    System.IO.File.Move(junkFilePath,pathForSaving);
                    
                }
                
            }
            catch(Exception)
            {
                throw;
            }
        }

        private void SetPersonPassportDestination(PostJambViewModel existingViewModel,out string junkFilePath,out string destinationFilePath)
        {
            const string TILDA = "~";

            try
            {
                string passportUrl = existingViewModel.Person.ImageFileUrl;
                junkFilePath = Server.MapPath(TILDA + existingViewModel.Person.ImageFileUrl);
                destinationFilePath = junkFilePath.Replace("Junk","Student");
                existingViewModel.Person.ImageFileUrl = passportUrl.Replace("Junk","Student");
            }
            catch(Exception)
            {
                throw;
            }
        }

        private void SetPersonCredentialDestination(PostJambViewModel existingViewModel,out string junkFilePath,out string destinationFilePath,int sitting)
        {
            const string TILDA = "~";

            try
            {
                if (sitting == 1)
                {
                    string passportUrl = existingViewModel.FirstSittingOLevelResult.ScannedCopyUrl;
                    junkFilePath = Server.MapPath(TILDA + existingViewModel.FirstSittingOLevelResult.ScannedCopyUrl);
                    destinationFilePath = junkFilePath.Replace("Junk","Credential");
                    if (passportUrl != null)
                    {
                        existingViewModel.FirstSittingOLevelResult.ScannedCopyUrl = passportUrl.Replace("Junk", "Credential");
                    }
                }
                else
                {
                    string passportUrl = existingViewModel.SecondSittingOLevelResult.ScannedCopyUrl;
                    junkFilePath = Server.MapPath(TILDA + existingViewModel.SecondSittingOLevelResult.ScannedCopyUrl);
                    destinationFilePath = junkFilePath.Replace("Junk","Credential");
                    if (passportUrl != null)
                    {
                        existingViewModel.SecondSittingOLevelResult.ScannedCopyUrl = passportUrl.Replace("Junk", "Credential");
                    }
                    
                }
               
              
                
            }
            catch(Exception)
            {
                throw;
            }
        }

   
        //[HttpPost]
        //public ActionResult PostJAMBPreview(PostJAMBViewModel viewModel)
        //{
        //    //const string CHANNEL = "INTERSWITCH";
        //    PostJAMBViewModel existingViewModel = (PostJAMBViewModel)TempData["viewModel"];

        //    try
        //    {
        //        //Role role = new Role() { Id = 6 };
        //        //PersonType personType = new PersonType() { Id = existingViewModel.ApplicationFormSetting.PersonType.Id };

        //        //Nationality nationality = new Nationality() { Id = 1 };
        //        //StudentType studentType = new StudentType() { Id = 3 };
        //        //StudentCategory studentCategory = new StudentCategory() { Id = 1 };

        //        //existingViewModel.Student.Role = role;
        //        //existingViewModel.Student.Type = studentType;
        //        //existingViewModel.Student.PersonType = personType;
        //        //existingViewModel.Student.Nationality = nationality;
        //        //existingViewModel.Student.Category = studentCategory;
        //        //existingViewModel.Student.DateEntered = DateTime.Now;

        //        //StudentLogic studentLogic = new StudentLogic();
        //        //Abundance_Nk.Model.Model.Student student = studentLogic.Add(existingViewModel.Student);
        //        //if (student != null && student.Id > 0)
        //        //{
        //        //    existingViewModel.Student.FullName = student.FullName;
        //        //}

        //        SponsorLogic sponsorLogic = new SponsorLogic();
        //        existingViewModel.Sponsor.Student = viewModel.Student;
        //        sponsorLogic.Create(existingViewModel.Sponsor);

        //        //    AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
        //        //    existingViewModel.AppliedCourse.Student = student;
        //        //    appliedCourseLogic.Create(existingViewModel.AppliedCourse);


        //        if (existingViewModel.IsAwaitingResult == false)
        //        {
        //            OLevelResultLogic oLevelResultLogic = new OLevelResultLogic();
        //            OLevelResultDetailLogic oLevelResultDetailLogic = new OLevelResultDetailLogic();
        //            if (existingViewModel.FirstSittingOLevelResult != null && existingViewModel.FirstSittingOLevelResult.ExamNumber != null && existingViewModel.FirstSittingOLevelResult.Type != null && existingViewModel.FirstSittingOLevelResult.ExamYear > 0)
        //            {
        //                existingViewModel.FirstSittingOLevelResult.Student = viewModel.Student;
        //                existingViewModel.FirstSittingOLevelResult.Sitting = new OLevelExamSitting() { Id = 1 };
        //                OLevelResult firstSittingOLevelResult = oLevelResultLogic.Create(existingViewModel.FirstSittingOLevelResult);

        //                if (existingViewModel.FirstSittingOLevelResultDetails != null && existingViewModel.FirstSittingOLevelResultDetails.Count > 0 && firstSittingOLevelResult != null)
        //                {
        //                    List<OLevelResultDetail> olevelResultDetails = existingViewModel.FirstSittingOLevelResultDetails.Where(m => m.Grade != null && m.Subject != null).ToList();
        //                    foreach (OLevelResultDetail oLevelResultDetail in olevelResultDetails)
        //                    {
        //                        oLevelResultDetail.Header = firstSittingOLevelResult;
        //                    }

        //                    oLevelResultDetailLogic.Create(olevelResultDetails);
        //                }
        //            }

        //            if (existingViewModel.SecondSittingOLevelResult != null && existingViewModel.SecondSittingOLevelResult.ExamNumber != null && existingViewModel.SecondSittingOLevelResult.Type != null && existingViewModel.SecondSittingOLevelResult.ExamYear > 0)
        //            {
        //                List<OLevelResultDetail> olevelResultDetails = existingViewModel.SecondSittingOLevelResultDetails.Where(m => m.Grade != null && m.Subject != null).ToList();
        //                if (olevelResultDetails != null && olevelResultDetails.Count > 0)
        //                {
        //                    existingViewModel.SecondSittingOLevelResult.Student = viewModel.Student;
        //                    existingViewModel.SecondSittingOLevelResult.Sitting = new OLevelExamSitting() { Id = 2 };
        //                    OLevelResult secondSittingOLevelResult = oLevelResultLogic.Create(existingViewModel.SecondSittingOLevelResult);

        //                    if (existingViewModel.SecondSittingOLevelResultDetails != null && existingViewModel.SecondSittingOLevelResultDetails.Count > 0 && secondSittingOLevelResult != null)
        //                    {
        //                        foreach (OLevelResultDetail oLevelResultDetail in olevelResultDetails)
        //                        {
        //                            oLevelResultDetail.Header = secondSittingOLevelResult;
        //                        }

        //                        oLevelResultDetailLogic.Create(olevelResultDetails);
        //                    }
        //                }
        //            }
        //        }

        //        if (existingViewModel.Programme.Id == 3)
        //        {
        //            if (existingViewModel.PreviousEducation != null)
        //            {
        //                PreviousEducationLogic previousEducationLogic = new PreviousEducationLogic();
        //                existingViewModel.PreviousEducation.Student = viewModel.Student;
        //                previousEducationLogic.Create(existingViewModel.PreviousEducation);
        //            }
        //        }
        //        else
        //        {
        //            if (existingViewModel.StudentJambDetail != null)
        //            {
        //                StudentJambDetailLogic studentJambDetailLogic = new StudentJambDetailLogic();
        //                existingViewModel.StudentJambDetail.Student = viewModel.Student;
        //                studentJambDetailLogic.Create(existingViewModel.StudentJambDetail);
        //            }
        //        }

        //        //Payment payment = new Payment();
        //        //PaymentLogic paymentLogic = new PaymentLogic();
        //        //payment.PaymentMode = new PaymentMode() { Id = existingViewModel.ApplicationFormSetting.PaymentMode.Id };
        //        //payment.PaymentType = new PaymentType() { Id = existingViewModel.ApplicationFormSetting.PaymentType.Id };
        //        //payment.PersonType = new PersonType() { Id = existingViewModel.ApplicationFormSetting.PersonType.Id };
        //        //payment.Fee = new Fee() { Id = existingViewModel.ApplicationProgrammeFee.Fee.Id };
        //        //payment.DatePaid = DateTime.Now;

        //        //OnlinePayment newOnlinePayment = null;
        //        //Payment newPayment = paymentLogic.Create(payment);
        //        //if (newPayment != null)
        //        //{
        //        //    OnlinePaymentLogic onlinePaymentLogic = new OnlinePaymentLogic();
        //        //    OnlinePayment onlinePayment = new OnlinePayment();
        //        //    onlinePayment.Channel = CHANNEL;
        //        //    onlinePayment.Payment = newPayment;
        //        //    onlinePayment.TransactionNumber = newPayment.Id.ToString();
        //        //    newOnlinePayment = onlinePaymentLogic.Create(onlinePayment);
        //        //}

        //        ApplicationForm applicationForm = new ApplicationForm();
        //        ApplicationFormLogic applicationFormLogic = new ApplicationFormLogic();
        //        applicationForm.ProgrammeFee = new ApplicationProgrammeFee() { Id = existingViewModel.ApplicationProgrammeFee.Id };
        //        applicationForm.Setting = new ApplicationFormSetting() { Id = existingViewModel.ApplicationFormSetting.Id };
        //        applicationForm.DateSubmitted = DateTime.Now;
        //        applicationForm.Person = (Person)viewModel.Student;
        //        applicationForm.Payment = viewModel.Payment;
        //        applicationForm.ProgrammeFee.Programme = existingViewModel.AppliedCourse.FirstChoiceProgramme;
        //        applicationForm.IsAwaitingResult = existingViewModel.IsAwaitingResult;

        //        ApplicationForm newApplicationForm = applicationFormLogic.Create(applicationForm);
        //        existingViewModel.ApplicationFormNumber = newApplicationForm.Number;


        //        TempData["viewModel"] = existingViewModel;
        //        return RedirectToAction("PostJAMBSlip", "Form");

        //    }
        //    catch (Exception ex)
        //    {
        //        SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
        //    }

        //    TempData["viewModel"] = existingViewModel;
        //    return View(existingViewModel);
        //}

        //private void GenerateInvoice(PostJAMBFormPaymentViewModel postJAMBFormPaymentViewModel)
        //{
        //    try
        //    {
        //        Abundance_Nk.Model.Model.Student student = CreatePerson(postJAMBFormPaymentViewModel);
        //        Payment payment = CreatePayment(postJAMBFormPaymentViewModel);


        //    }
        //    catch (Exception ex)
        //    {
        //        SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
        //    }
        //}

        private Person CreatePerson(PostJAMBFormPaymentViewModel existingViewModel)
        {
            try
            {
                Role role = new Role() { Id = 6 };
                PersonType personType = new PersonType() { Id = existingViewModel.ApplicationFormSetting.PersonType.Id };
                Nationality nationality = new Nationality() { Id = 1 };

                existingViewModel.Person.Role = role;
                existingViewModel.Person.Type = personType;
                existingViewModel.Person.Nationality = nationality;
                existingViewModel.Person.DateEntered = DateTime.Now;

                PersonLogic personLogic = new PersonLogic();
                Person person = personLogic.Create(existingViewModel.Person);
                if(person != null && person.Id > 0)
                {
                    existingViewModel.Person = person;
                }

                return person;
            }
            catch(Exception)
            {
                throw;
            }
        }

        private Payment CreatePayment(PostJAMBFormPaymentViewModel existingViewModel)
        {

            try
            {
                Payment payment = new Payment();
                PaymentLogic paymentLogic = new PaymentLogic();
                payment.PaymentMode = new PaymentMode() { Id = existingViewModel.ApplicationFormSetting.PaymentMode.Id };
                payment.PaymentType = new PaymentType() { Id = existingViewModel.ApplicationFormSetting.PaymentType.Id };
                payment.PersonType = new PersonType() { Id = existingViewModel.ApplicationFormSetting.PersonType.Id };
                payment.FeeType = new FeeType() { Id = existingViewModel.ApplicationProgrammeFee.FeeType.Id };
                payment.DatePaid = DateTime.Now;
                payment.Person = existingViewModel.Person;
                payment.Session = existingViewModel.ApplicationFormSetting.Session;

                OnlinePayment newOnlinePayment = null;
                Payment newPayment = paymentLogic.Create(payment);
                if(newPayment != null)
                {
                    PaymentChannel channel = new PaymentChannel() { Id = (int)PaymentChannel.Channels.Etranzact };
                    OnlinePaymentLogic onlinePaymentLogic = new OnlinePaymentLogic();
                    OnlinePayment onlinePayment = new OnlinePayment();
                    onlinePayment.Channel = channel;
                    onlinePayment.Payment = newPayment;
                    newOnlinePayment = onlinePaymentLogic.Create(onlinePayment);
                }

                return newPayment;
            }
            catch(Exception)
            {
                throw;
            }
        }

        private AppliedCourse CreateAppliedCourse(PostJAMBFormPaymentViewModel viewModel)
        {
            try
            {
                AppliedCourse appliedCourse = new AppliedCourse();
                appliedCourse.Programme = viewModel.Programme;
                appliedCourse.Department = viewModel.AppliedCourse.Department;
                if (viewModel.AppliedCourse.Option != null && viewModel.AppliedCourse.Option.Id > 0)
                {
                    appliedCourse.Option = viewModel.AppliedCourse.Option; 
                }
                appliedCourse.Person = viewModel.Person;

                AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
                return appliedCourseLogic.Create(appliedCourse);
            }
            catch(Exception)
            {
                throw;
            }
        }

        private void UpdateOLevelResultDetail(PostJambViewModel viewModel)
        {
            try
            {
                if(viewModel != null && viewModel.FirstSittingOLevelResultDetails != null && viewModel.FirstSittingOLevelResultDetails.Count > 0)
                {
                    foreach(OLevelResultDetail firstSittingOLevelResultDetail in viewModel.FirstSittingOLevelResultDetails)
                    {
                        if(firstSittingOLevelResultDetail.Subject != null)
                        {
                            firstSittingOLevelResultDetail.Subject = viewModel.OLevelSubjects.Where(m => m.Id == firstSittingOLevelResultDetail.Subject.Id).SingleOrDefault();
                        }
                        if(firstSittingOLevelResultDetail.Grade != null)
                        {
                            firstSittingOLevelResultDetail.Grade = viewModel.OLevelGrades.Where(m => m.Id == firstSittingOLevelResultDetail.Grade.Id).SingleOrDefault();
                        }
                    }
                }

                if(viewModel != null && viewModel.SecondSittingOLevelResultDetails != null && viewModel.SecondSittingOLevelResultDetails.Count > 0)
                {
                    foreach(OLevelResultDetail secondSittingOLevelResultDetail in viewModel.SecondSittingOLevelResultDetails)
                    {
                        if(secondSittingOLevelResultDetail.Subject != null)
                        {
                            secondSittingOLevelResultDetail.Subject = viewModel.OLevelSubjects.Where(m => m.Id == secondSittingOLevelResultDetail.Subject.Id).SingleOrDefault();
                        }
                        if(secondSittingOLevelResultDetail.Grade != null)
                        {
                            secondSittingOLevelResultDetail.Grade = viewModel.OLevelGrades.Where(m => m.Id == secondSittingOLevelResultDetail.Grade.Id).SingleOrDefault();
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message,Message.Category.Error);
            }
        }

        private void PopulateAllDropDowns()
        {
            PostJambViewModel existingViewModel = (PostJambViewModel)TempData["viewModel"];
            PostJAMBFormPaymentViewModel postJAMBFormPaymentViewModel = (PostJAMBFormPaymentViewModel)TempData["PostJAMBFormPaymentViewModel"];

            try
            {
                if(existingViewModel == null)
                {
                    viewModel = new PostJambViewModel();

                    ViewBag.StateId = viewModel.StateSelectList;
                    ViewBag.LicenseTypeId = viewModel.LicenceTypeSelectList;
                    ViewBag.SexId = viewModel.SexSelectList;
                    ViewBag.FirstChoiceFacultyId = viewModel.FacultySelectList;
                    ViewBag.SecondChoiceFacultyId = viewModel.FacultySelectList;
                    ViewBag.LgaId = new SelectList(new List<LocalGovernment>(),ID,NAME);
                    ViewBag.RelationshipId = viewModel.RelationshipSelectList;
                    ViewBag.FirstSittingOLevelTypeId = viewModel.OLevelTypeSelectList;
                    ViewBag.SecondSittingOLevelTypeId = viewModel.OLevelTypeSelectList;
                    ViewBag.FirstSittingExamYearId = viewModel.ExamYearSelectList;
                    ViewBag.SecondSittingExamYearId = viewModel.ExamYearSelectList;
                    ViewBag.ReligionId = viewModel.ReligionSelectList;
                    ViewBag.AbilityId = viewModel.AbilitySelectList;
                    ViewBag.ResultGradeId = viewModel.ResultGradeSelectList;
                    ViewBag.QualificationId = viewModel.EducationalQualificationSelectList;
                    ViewBag.ITDurationId = viewModel.ITDurationSelectList;

                    ViewBag.DayOfBirthId = new SelectList(new List<Value>(),ID,NAME);
                    ViewBag.MonthOfBirthId = viewModel.MonthOfBirthSelectList;
                    ViewBag.YearOfBirthId = viewModel.YearOfBirthSelectList;

                    if (postJAMBFormPaymentViewModel.Programme.Id == (int)Programmes.HNDFullTime || postJAMBFormPaymentViewModel.Programme.Id == (int)Programmes.HNDPartTime || 
                        postJAMBFormPaymentViewModel.Programme.Id == (int)Programmes.HNDWeekend || postJAMBFormPaymentViewModel.Programme.Id == (int)Programmes.HNDDistance)
                    {
                        ViewBag.PreviousEducationStartDayId = new SelectList(new List<Value>(),ID,NAME);
                        ViewBag.PreviousEducationStartMonthId = viewModel.PreviousEducationStartMonthSelectList;
                        ViewBag.PreviousEducationStartYearId = viewModel.PreviousEducationStartYearSelectList;

                        ViewBag.PreviousEducationEndDayId = new SelectList(new List<Value>(),ID,NAME);
                        ViewBag.PreviousEducationEndMonthId = viewModel.PreviousEducationEndMonthSelectList;
                        ViewBag.PreviousEducationEndYearId = viewModel.PreviousEducationEndYearSelectList;

                        ViewBag.ResultGradeId = viewModel.ResultGradeSelectList;
                        ViewBag.QualificationId = viewModel.EducationalQualificationSelectList;
                        ViewBag.ITDurationId = viewModel.ITDurationSelectList;
                    }
                    else
                    {
                        if (postJAMBFormPaymentViewModel.ApplicantJambDetail != null)
                        {
                            ViewBag.JambScoreId = new SelectList(viewModel.JambScoreSelectList, "Value", "Text", postJAMBFormPaymentViewModel.ApplicantJambDetail.JambScore);
                            if (postJAMBFormPaymentViewModel.ApplicantJambDetail.InstitutionChoice != null)
                            {
                                ViewBag.InstitutionChoiceId = new SelectList(viewModel.InstitutionChoiceSelectList, "Value", "Text", postJAMBFormPaymentViewModel.ApplicantJambDetail.InstitutionChoice.Id); 
                            }
                            else
                            {
                                ViewBag.InstitutionChoiceId = viewModel.InstitutionChoiceSelectList; 
                            }

                            if (postJAMBFormPaymentViewModel.ApplicantJambDetail.Subject1 != null)
                            {
                                ViewBag.Subject1Id = new SelectList(viewModel.OLevelSubjectSelectList, VALUE, TEXT, postJAMBFormPaymentViewModel.ApplicantJambDetail.Subject1.Id); 
                            }
                            else
                            {
                                ViewBag.Subject1Id = viewModel.OLevelSubjectSelectList;
                            }

                            if (postJAMBFormPaymentViewModel.ApplicantJambDetail.Subject2 != null)
                            {
                                ViewBag.Subject2Id = new SelectList(viewModel.OLevelSubjectSelectList, VALUE, TEXT, postJAMBFormPaymentViewModel.ApplicantJambDetail.Subject2.Id);
                            }
                            else
                            {
                                ViewBag.Subject2Id = viewModel.OLevelSubjectSelectList;
                            }

                            if (postJAMBFormPaymentViewModel.ApplicantJambDetail.Subject3 != null)
                            {
                                ViewBag.Subject3Id = new SelectList(viewModel.OLevelSubjectSelectList, VALUE, TEXT, postJAMBFormPaymentViewModel.ApplicantJambDetail.Subject3.Id);
                            }
                            else
                            {
                                ViewBag.Subject3Id = viewModel.OLevelSubjectSelectList;
                            }

                            if (postJAMBFormPaymentViewModel.ApplicantJambDetail.Subject4 != null)
                            {
                                ViewBag.Subject4Id = new SelectList(viewModel.OLevelSubjectSelectList, VALUE, TEXT, postJAMBFormPaymentViewModel.ApplicantJambDetail.Subject4.Id);
                            }
                            else
                            {
                                ViewBag.Subject4Id = viewModel.OLevelSubjectSelectList;
                            }
                        }
                        else
                        {
                            ViewBag.JambScoreId = viewModel.JambScoreSelectList;
                            ViewBag.InstitutionChoiceId = viewModel.InstitutionChoiceSelectList;
                            ViewBag.Subject1Id = viewModel.OLevelSubjectSelectList;
                            ViewBag.Subject2Id = viewModel.OLevelSubjectSelectList;
                            ViewBag.Subject3Id = viewModel.OLevelSubjectSelectList;
                            ViewBag.Subject4Id = viewModel.OLevelSubjectSelectList;
                        }
                    }

                    SetDefaultSelectedSittingSubjectAndGrade(viewModel);
                }
                else
                {
                    if(existingViewModel.Person.Religion == null) { existingViewModel.Person.Religion = new Religion(); }
                    if(existingViewModel.Person.State == null) { existingViewModel.Person.State = new State(); }
                    if(existingViewModel.Person.Sex == null) { existingViewModel.Person.Sex = new Sex(); }
                    if(existingViewModel.AppliedCourse.Programme == null) { existingViewModel.AppliedCourse.Programme = new Programme(); }
                    if(existingViewModel.Sponsor.Relationship == null) { existingViewModel.Sponsor.Relationship = new Relationship(); }
                    if(existingViewModel.FirstSittingOLevelResult.Type == null) { existingViewModel.FirstSittingOLevelResult.Type = new OLevelType(); }
                    if(existingViewModel.Applicant.Ability == null) { existingViewModel.Applicant.Ability = new Ability(); }
                    if(existingViewModel.Person.YearOfBirth == null) { existingViewModel.Person.YearOfBirth = new Value(); }
                    if(existingViewModel.Person.MonthOfBirth == null) { existingViewModel.Person.MonthOfBirth = new Value(); }
                    if(existingViewModel.Person.DayOfBirth == null) { existingViewModel.Person.DayOfBirth = new Value(); }

                    ViewBag.ReligionId = new SelectList(existingViewModel.ReligionSelectList,VALUE,TEXT,existingViewModel.Person.Religion.Id);
                    ViewBag.StateId = new SelectList(existingViewModel.StateSelectList,VALUE,TEXT,existingViewModel.Person.State.Id);
                    ViewBag.SexId = new SelectList(existingViewModel.SexSelectList,VALUE,TEXT,existingViewModel.Person.Sex.Id);
                    ViewBag.ProgrammeId = new SelectList(existingViewModel.FacultySelectList,VALUE,TEXT,existingViewModel.AppliedCourse.Programme.Id);
                    ViewBag.RelationshipId = new SelectList(existingViewModel.RelationshipSelectList,VALUE,TEXT,existingViewModel.Sponsor.Relationship.Id);
                    ViewBag.FirstSittingOLevelTypeId = new SelectList(existingViewModel.OLevelTypeSelectList,VALUE,TEXT,existingViewModel.FirstSittingOLevelResult.Type.Id);
                    ViewBag.FirstSittingExamYearId = new SelectList(existingViewModel.ExamYearSelectList,VALUE,TEXT,existingViewModel.FirstSittingOLevelResult.ExamYear);
                    ViewBag.SecondSittingExamYearId = new SelectList(existingViewModel.ExamYearSelectList,VALUE,TEXT,existingViewModel.SecondSittingOLevelResult.ExamYear);
                    ViewBag.AbilityId = new SelectList(existingViewModel.AbilitySelectList,VALUE,TEXT,existingViewModel.Applicant.Ability.Id);

                    SetDateOfBirthDropDown(existingViewModel);

                    if (postJAMBFormPaymentViewModel != null)
                    {
                        if (postJAMBFormPaymentViewModel.Programme != null && postJAMBFormPaymentViewModel.Programme.Id == (int)Programmes.HNDFullTime ||
                            postJAMBFormPaymentViewModel.Programme.Id == (int)Programmes.HNDPartTime || postJAMBFormPaymentViewModel.Programme.Id == (int)Programmes.HNDWeekend
                            || postJAMBFormPaymentViewModel.Programme.Id == (int)Programmes.HNDDistance)
                        {
                            SetPreviousEducationEndDateDropDowns(existingViewModel);
                            SetPreviousEducationStartDateDropDowns(existingViewModel);
                            ViewBag.ResultGradeId = new SelectList(existingViewModel.ResultGradeSelectList, VALUE, TEXT, existingViewModel.PreviousEducation.ResultGrade.Id);
                            ViewBag.QualificationId = new SelectList(existingViewModel.EducationalQualificationSelectList, VALUE, TEXT, existingViewModel.PreviousEducation.Qualification.Id);
                            ViewBag.ITDurationId = new SelectList(existingViewModel.ITDurationSelectList, VALUE, TEXT, existingViewModel.PreviousEducation.ITDuration.Id);
                        }
                        else
                        {
                            //take care if browser is refreshed
                            ViewBag.InstitutionChoiceId = new SelectList(existingViewModel.InstitutionChoiceSelectList, VALUE, TEXT, existingViewModel.ApplicantJambDetail.InstitutionChoice.Id);
                            ViewBag.JambScoreId = new SelectList(existingViewModel.JambScoreSelectList, VALUE, TEXT, existingViewModel.ApplicantJambDetail.JambScore);
                            if (existingViewModel.ApplicantJambDetail.Subject1 != null)
                            {
                                ViewBag.Subject1Id = new SelectList(existingViewModel.OLevelSubjectSelectList, VALUE, TEXT, existingViewModel.ApplicantJambDetail.Subject1.Id);
                            }
                            else
                            {
                                ViewBag.Subject1Id = existingViewModel.OLevelSubjectSelectList;
                            }

                            if (existingViewModel.ApplicantJambDetail.Subject2 != null)
                            {
                                ViewBag.Subject2Id = new SelectList(existingViewModel.OLevelSubjectSelectList, VALUE, TEXT, existingViewModel.ApplicantJambDetail.Subject2.Id);
                            }
                            else
                            {
                                ViewBag.Subject2Id = existingViewModel.OLevelSubjectSelectList;
                            }

                            if (existingViewModel.ApplicantJambDetail.Subject3 != null)
                            {
                                ViewBag.Subject3Id = new SelectList(existingViewModel.OLevelSubjectSelectList, VALUE, TEXT, existingViewModel.ApplicantJambDetail.Subject3.Id);
                            }
                            else
                            {
                                ViewBag.Subject3Id = existingViewModel.OLevelSubjectSelectList;
                            }

                            if (existingViewModel.ApplicantJambDetail.Subject4 != null)
                            {
                                ViewBag.Subject4Id = new SelectList(existingViewModel.OLevelSubjectSelectList, VALUE, TEXT, existingViewModel.ApplicantJambDetail.Subject4.Id);
                            }
                            else
                            {
                                ViewBag.Subject4Id = existingViewModel.OLevelSubjectSelectList;
                            }
                        }
                    }
                    else
                    {
                        ViewBag.InstitutionChoiceId = new SelectList(existingViewModel.InstitutionChoiceSelectList,VALUE,TEXT,existingViewModel.ApplicantJambDetail.InstitutionChoice.Id);
                        ViewBag.JambScoreId = new SelectList(existingViewModel.JambScoreSelectList,VALUE,TEXT,existingViewModel.ApplicantJambDetail.JambScore);
                        if (existingViewModel.ApplicantJambDetail.Subject1 != null)
                        {
                            ViewBag.Subject1Id = new SelectList(existingViewModel.OLevelSubjectSelectList, VALUE, TEXT, existingViewModel.ApplicantJambDetail.Subject1.Id);
                        }
                        else
                        {
                            ViewBag.Subject1Id = existingViewModel.OLevelSubjectSelectList;
                        }

                        if (existingViewModel.ApplicantJambDetail.Subject2 != null)
                        {
                            ViewBag.Subject2Id = new SelectList(existingViewModel.OLevelSubjectSelectList, VALUE, TEXT, existingViewModel.ApplicantJambDetail.Subject2.Id);
                        }
                        else
                        {
                            ViewBag.Subject2Id = existingViewModel.OLevelSubjectSelectList;
                        }

                        if (existingViewModel.ApplicantJambDetail.Subject3 != null)
                        {
                            ViewBag.Subject3Id = new SelectList(existingViewModel.OLevelSubjectSelectList, VALUE, TEXT, existingViewModel.ApplicantJambDetail.Subject3.Id);
                        }
                        else
                        {
                            ViewBag.Subject3Id = existingViewModel.OLevelSubjectSelectList;
                        }

                        if (existingViewModel.ApplicantJambDetail.Subject4 != null)
                        {
                            ViewBag.Subject4Id = new SelectList(existingViewModel.OLevelSubjectSelectList, VALUE, TEXT, existingViewModel.ApplicantJambDetail.Subject4.Id);
                        }
                        else
                        {
                            ViewBag.Subject4Id = existingViewModel.OLevelSubjectSelectList;
                        }
                    }

                    if(existingViewModel.Person.LocalGovernment != null && existingViewModel.Person.LocalGovernment.Id > 0)
                    {
                        ViewBag.LgaId = new SelectList(existingViewModel.LocalGovernmentSelectList,VALUE,TEXT,existingViewModel.Person.LocalGovernment.Id);
                    }
                    else
                    {
                        ViewBag.LgaId = new SelectList(new List<LocalGovernment>(),VALUE,TEXT);
                    }

                    if(existingViewModel.SecondSittingOLevelResult.Type != null)
                    {
                        ViewBag.SecondSittingOLevelTypeId = new SelectList(existingViewModel.OLevelTypeSelectList,VALUE,TEXT,existingViewModel.SecondSittingOLevelResult.Type.Id);
                    }
                    else
                    {
                        ViewBag.SecondSittingOLevelTypeId = new SelectList(existingViewModel.OLevelTypeSelectList,VALUE,TEXT,0);
                    }

                    SetSelectedSittingSubjectAndGrade(existingViewModel);
                }
            }
            catch(Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message,Message.Category.Error);
            }
        }

        private void SetDateOfBirthDropDown(PostJambViewModel existingViewModel)
        {
            try
            {
                ViewBag.MonthOfBirthId = new SelectList(existingViewModel.MonthOfBirthSelectList,VALUE,TEXT,existingViewModel.Person.MonthOfBirth.Id);
                ViewBag.YearOfBirthId = new SelectList(existingViewModel.YearOfBirthSelectList,VALUE,TEXT,existingViewModel.Person.YearOfBirth.Id);
                if((existingViewModel.DayOfBirthSelectList == null || existingViewModel.DayOfBirthSelectList.Count == 0) && (existingViewModel.Person.MonthOfBirth.Id > 0 && existingViewModel.Person.YearOfBirth.Id > 0))
                {
                    existingViewModel.DayOfBirthSelectList = Utility.PopulateDaySelectListItem(existingViewModel.Person.MonthOfBirth,existingViewModel.Person.YearOfBirth);
                }
                else
                {
                    if(existingViewModel.DayOfBirthSelectList != null && existingViewModel.Person.DayOfBirth.Id > 0)
                    {
                        ViewBag.DayOfBirthId = new SelectList(existingViewModel.DayOfBirthSelectList,VALUE,TEXT,existingViewModel.Person.DayOfBirth.Id);
                    }
                    else if(existingViewModel.DayOfBirthSelectList != null && existingViewModel.Person.DayOfBirth.Id <= 0)
                    {
                        ViewBag.DayOfBirthId = existingViewModel.DayOfBirthSelectList;
                    }
                    else if(existingViewModel.DayOfBirthSelectList == null)
                    {
                        existingViewModel.DayOfBirthSelectList = new List<SelectListItem>();
                        ViewBag.DayOfBirthId = new List<SelectListItem>();
                    }
                }

                if(existingViewModel.Person.DayOfBirth != null && existingViewModel.Person.DayOfBirth.Id > 0)
                {
                    ViewBag.DayOfBirthId = new SelectList(existingViewModel.DayOfBirthSelectList,VALUE,TEXT,existingViewModel.Person.DayOfBirth.Id);
                }
                else
                {
                    ViewBag.DayOfBirthId = existingViewModel.DayOfBirthSelectList;
                }
            }
            catch(Exception)
            {
                throw;
            }
        }

        private void SetPreviousEducationStartDateDropDowns(PostJambViewModel existingViewModel)
        {
            try
            {
                ViewBag.PreviousEducationStartMonthId = new SelectList(existingViewModel.PreviousEducationStartMonthSelectList,VALUE,TEXT,existingViewModel.PreviousEducation.StartMonth.Id);
                ViewBag.PreviousEducationStartYearId = new SelectList(existingViewModel.PreviousEducationStartYearSelectList,VALUE,TEXT,existingViewModel.PreviousEducation.StartYear.Id);
                if((existingViewModel.PreviousEducationStartDaySelectList == null || existingViewModel.PreviousEducationStartDaySelectList.Count == 0) && (existingViewModel.PreviousEducation.StartMonth.Id > 0 && existingViewModel.PreviousEducation.StartYear.Id > 0))
                {
                    existingViewModel.PreviousEducationStartDaySelectList = Utility.PopulateDaySelectListItem(existingViewModel.PreviousEducation.StartMonth,existingViewModel.PreviousEducation.StartYear);
                }
                else
                {
                    if(existingViewModel.PreviousEducationStartDaySelectList != null && existingViewModel.PreviousEducation.StartDay.Id > 0)
                    {
                        ViewBag.PreviousEducationStartDayId = new SelectList(existingViewModel.PreviousEducationStartDaySelectList,VALUE,TEXT,existingViewModel.PreviousEducation.StartDay.Id);
                    }
                    else if(existingViewModel.PreviousEducationStartDaySelectList != null && existingViewModel.PreviousEducation.StartDay.Id <= 0)
                    {
                        ViewBag.PreviousEducationStartDayId = existingViewModel.PreviousEducationStartDaySelectList;
                    }
                    else if(existingViewModel.PreviousEducationStartDaySelectList == null)
                    {
                        existingViewModel.PreviousEducationStartDaySelectList = new List<SelectListItem>();
                        ViewBag.PreviousEducationStartDayId = new List<SelectListItem>();
                    }
                }

                if(existingViewModel.PreviousEducation.StartDay != null && existingViewModel.PreviousEducation.StartDay.Id > 0)
                {
                    ViewBag.PreviousEducationStartDayId = new SelectList(existingViewModel.PreviousEducationStartDaySelectList,VALUE,TEXT,existingViewModel.PreviousEducation.StartDay.Id);
                }
                else
                {
                    ViewBag.PreviousEducationStartDayId = existingViewModel.PreviousEducationStartDaySelectList;
                }
            }
            catch(Exception)
            {
                throw;
            }
        }

        private void SetPreviousEducationEndDateDropDowns(PostJambViewModel existingViewModel)
        {
            try
            {
                ViewBag.PreviousEducationEndMonthId = new SelectList(existingViewModel.PreviousEducationEndMonthSelectList,VALUE,TEXT,existingViewModel.PreviousEducation.EndMonth.Id);
                ViewBag.PreviousEducationEndYearId = new SelectList(existingViewModel.PreviousEducationEndYearSelectList,VALUE,TEXT,existingViewModel.PreviousEducation.EndYear.Id);
                if((existingViewModel.PreviousEducationEndDaySelectList == null || existingViewModel.PreviousEducationEndDaySelectList.Count == 0) && (existingViewModel.PreviousEducation.EndMonth.Id > 0 && existingViewModel.PreviousEducation.EndYear.Id > 0))
                {
                    existingViewModel.PreviousEducationEndDaySelectList = Utility.PopulateDaySelectListItem(existingViewModel.PreviousEducation.EndMonth,existingViewModel.PreviousEducation.EndYear);
                }
                else
                {
                    if(existingViewModel.PreviousEducationEndDaySelectList != null && existingViewModel.PreviousEducation.EndDay.Id > 0)
                    {
                        ViewBag.PreviousEducationEndDayId = new SelectList(existingViewModel.PreviousEducationEndDaySelectList,VALUE,TEXT,existingViewModel.PreviousEducation.EndDay.Id);
                    }
                    else if(existingViewModel.PreviousEducationEndDaySelectList != null && existingViewModel.PreviousEducation.EndDay.Id <= 0)
                    {
                        ViewBag.PreviousEducationEndDayId = existingViewModel.PreviousEducationEndDaySelectList;
                    }
                    else if(existingViewModel.PreviousEducationEndDaySelectList == null)
                    {
                        existingViewModel.PreviousEducationEndDaySelectList = new List<SelectListItem>();
                        ViewBag.PreviousEducationEndDayId = new List<SelectListItem>();
                    }
                }

                if(existingViewModel.PreviousEducation.EndDay != null && existingViewModel.PreviousEducation.EndDay.Id > 0)
                {
                    ViewBag.PreviousEducationEndDayId = new SelectList(existingViewModel.PreviousEducationEndDaySelectList,VALUE,TEXT,existingViewModel.PreviousEducation.EndDay.Id);
                }
                else
                {
                    ViewBag.PreviousEducationEndDayId = existingViewModel.PreviousEducationEndDaySelectList;
                }
            }
            catch(Exception)
            {
                throw;
            }
        }

        private void SetDefaultSelectedSittingSubjectAndGrade(PostJambViewModel viewModel)
        {
            try
            {
                if(viewModel != null && viewModel.FirstSittingOLevelResultDetails != null)
                {
                    for(int i = 0;i < 9;i++)
                    {
                        ViewData["FirstSittingOLevelSubjectId" + i] = viewModel.OLevelSubjectSelectList;
                        ViewData["FirstSittingOLevelGradeId" + i] = viewModel.OLevelGradeSelectList;
                    }
                }

                if(viewModel != null && viewModel.SecondSittingOLevelResultDetails != null)
                {
                    for(int i = 0;i < 9;i++)
                    {
                        ViewData["SecondSittingOLevelSubjectId" + i] = viewModel.OLevelSubjectSelectList;
                        ViewData["SecondSittingOLevelGradeId" + i] = viewModel.OLevelGradeSelectList;
                    }
                }
            }
            catch(Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message,Message.Category.Error);
            }
        }

        private void SetSelectedSittingSubjectAndGrade(PostJambViewModel existingViewModel)
        {
            try
            {
                if(existingViewModel != null && existingViewModel.FirstSittingOLevelResultDetails != null && existingViewModel.FirstSittingOLevelResultDetails.Count > 0)
                {
                    int i = 0;
                    foreach(OLevelResultDetail firstSittingOLevelResultDetail in existingViewModel.FirstSittingOLevelResultDetails)
                    {
                        if(firstSittingOLevelResultDetail.Subject != null && firstSittingOLevelResultDetail.Grade != null)
                        {
                            ViewData["FirstSittingOLevelSubjectId" + i] = new SelectList(existingViewModel.OLevelSubjectSelectList,VALUE,TEXT,firstSittingOLevelResultDetail.Subject.Id);
                            ViewData["FirstSittingOLevelGradeId" + i] = new SelectList(existingViewModel.OLevelGradeSelectList,VALUE,TEXT,firstSittingOLevelResultDetail.Grade.Id);
                        }
                        else
                        {
                            ViewData["FirstSittingOLevelSubjectId" + i] = new SelectList(existingViewModel.OLevelSubjectSelectList,VALUE,TEXT,0);
                            ViewData["FirstSittingOLevelGradeId" + i] = new SelectList(existingViewModel.OLevelGradeSelectList,VALUE,TEXT,0);
                        }

                        i++;
                    }
                }

                if(existingViewModel != null && existingViewModel.SecondSittingOLevelResultDetails != null && existingViewModel.SecondSittingOLevelResultDetails.Count > 0)
                {
                    int i = 0;
                    foreach(OLevelResultDetail secondSittingOLevelResultDetail in existingViewModel.SecondSittingOLevelResultDetails)
                    {
                        if(secondSittingOLevelResultDetail.Subject != null && secondSittingOLevelResultDetail.Grade != null)
                        {
                            ViewData["SecondSittingOLevelSubjectId" + i] = new SelectList(existingViewModel.OLevelSubjectSelectList,VALUE,TEXT,secondSittingOLevelResultDetail.Subject.Id);
                            ViewData["SecondSittingOLevelGradeId" + i] = new SelectList(existingViewModel.OLevelGradeSelectList,VALUE,TEXT,secondSittingOLevelResultDetail.Grade.Id);
                        }
                        else
                        {
                            ViewData["SecondSittingOLevelSubjectId" + i] = new SelectList(existingViewModel.OLevelSubjectSelectList,VALUE,TEXT,0);
                            ViewData["SecondSittingOLevelGradeId" + i] = new SelectList(existingViewModel.OLevelGradeSelectList,VALUE,TEXT,0);
                        }

                        i++;
                    }
                }
            }
            catch(Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message,Message.Category.Error);
            }
        }

        public JsonResult GetLocalGovernmentsByState(string id)
        {
            try
            {
                LocalGovernmentLogic lgaLogic = new LocalGovernmentLogic();

                Expression<Func<LOCAL_GOVERNMENT,bool>> selector = l => l.State_Id == id;
                List<LocalGovernment> lgas = lgaLogic.GetModelsBy(selector);

                return Json(new SelectList(lgas,"Id","Name"),JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public JsonResult GetDayOfBirthBy(string monthId,string yearId)
        {
            try
            {
                if(string.IsNullOrEmpty(monthId) || string.IsNullOrEmpty(yearId))
                {
                    return null;
                }

                Value month = new Value() { Id = Convert.ToInt32(monthId) };
                Value year = new Value() { Id = Convert.ToInt32(yearId) };
                List<Value> days = Utility.GetNumberOfDaysInMonth(month,year);

                return Json(new SelectList(days,ID,NAME),JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public JsonResult GetDepartmentByProgrammeId(string id)
        {
            try
            {
                //List<int> openDepartmentIds = new List<int>() {30,33};
                //List<int> hndOpenDepartmentIds = new List<int>() {28};
                if (string.IsNullOrEmpty(id))
                {
                    return null;
                }

                Programme programme = new Programme() { Id = Convert.ToInt32(id) };

                DepartmentLogic departmentLogic = new DepartmentLogic();
                //if (programme.Id == 1)
                //{
                //    List<Department> opendepartments=departmentLogic.GetModelsBy(f => openDepartmentIds.Contains(f.Department_Id));
                //    return Json(new SelectList(opendepartments, ID, NAME), JsonRequestBehavior.AllowGet);
                //}
                //if (programme.Id == 3)
                //{
                //    List<Department> opendepartments = departmentLogic.GetModelsBy(f => hndOpenDepartmentIds.Contains(f.Department_Id));
                //    return Json(new SelectList(opendepartments, ID, NAME), JsonRequestBehavior.AllowGet);
                //}
                List<Department> departments = departmentLogic.GetBy(programme);

                return Json(new SelectList(departments,ID,NAME),JsonRequestBehavior.AllowGet);

            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public JsonResult GetDepartmentOptionByDepartment(string id,string programmeid)
        {
            try
            {
                if(string.IsNullOrEmpty(id))
                {
                    return null;
                }

                Department department = new Department() { Id = Convert.ToInt32(id) };
                Programme programme = new Programme() { Id = Convert.ToInt32(programmeid) };
                DepartmentOptionLogic departmentLogic = new DepartmentOptionLogic();
                List<DepartmentOption> departmentOptions = departmentLogic.GetBy(department,programme);

                return Json(new SelectList(departmentOptions,ID,NAME),JsonRequestBehavior.AllowGet);

            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
         [HttpPost]
        public virtual ActionResult UploadFile(FormCollection form)
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
                if(file != null && file.ContentLength != 0)
                {
                    FileInfo fileInfo = new FileInfo(file.FileName);
                    string fileExtension = fileInfo.Extension;
                    string newFile = personId + "__";
                    string newFileName = newFile + DateTime.Now.ToString().Replace("/","").Replace(":","").Replace(" ","") + fileExtension;

                    decimal sizeAllowed = 50; //50kb
                    string invalidFileMessage = InvalidFile(file.ContentLength,fileExtension,sizeAllowed);
                    if(!string.IsNullOrEmpty(invalidFileMessage))
                    {
                        isUploaded = false;
                        TempData["imageUrl"] = null;
                        return Json(new { isUploaded = isUploaded,message = invalidFileMessage,imageUrl = passportUrl },"text/html",JsonRequestBehavior.AllowGet);
                    }

                    string pathForSaving = Server.MapPath("~/Content/Junk");
                    if(this.CreateFolderIfNeeded(pathForSaving))
                    {
                        DeleteFileIfExist(pathForSaving,personId);

                        file.SaveAs(Path.Combine(pathForSaving,newFileName));

                        isUploaded = true;
                        message = "File uploaded successfully!";

                        path = Path.Combine(pathForSaving,newFileName);
                        if(path != null)
                        {
                            imageUrl = "/Content/Junk/" + newFileName;
                            imageUrlDisplay = appRoot + imageUrl + "?t=" + DateTime.Now;

                            //imageUrlDisplay = "/ilaropoly" + imageUrl + "?t=" + DateTime.Now;
                            TempData["imageUrl"] = imageUrl;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                message = string.Format("File upload failed: {0}",ex.Message);
            }
            TempData.Keep();
            return Json(new { isUploaded = isUploaded,message = message,imageUrl = imageUrl },"text/html",JsonRequestBehavior.AllowGet);
        }

        private string InvalidFile(decimal uploadedFileSize,string fileExtension,decimal sizeAllowed )
        {
            try
            {
                string message = null;
                decimal oneKiloByte = 1024;
                decimal maximumFileSize = sizeAllowed * oneKiloByte;

                decimal actualFileSizeToUpload = Math.Round(uploadedFileSize / oneKiloByte,1);
                if(InvalidFileType(fileExtension))
                {
                    message = "File type '" + fileExtension + "' is invalid! File type must be any of the following: .jpg, .jpeg, .png or .gif ";
                }
                else if(actualFileSizeToUpload > (maximumFileSize / oneKiloByte))
                {
                    message = "Your file size of " + actualFileSizeToUpload.ToString("0.#") + " Kb is too large, maximum allowed size is " + (maximumFileSize / oneKiloByte) + " Kb";
                }

                return message;
            }
            catch(Exception)
            {
                throw;
            }
        }
        private string InvalidFilePdf(decimal uploadedFileSize, string fileExtension, decimal sizeAllowed)
        {
            try
            {
                string message = null;
                decimal oneKiloByte = 1024;
                decimal maximumFileSize = sizeAllowed * oneKiloByte;

                decimal actualFileSizeToUpload = Math.Round(uploadedFileSize / oneKiloByte, 1);
                if (InvalidFileTypePdf(fileExtension))
                {
                    message = "File type '" + fileExtension + "' is invalid! File type must be .pdf ";
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
            switch(extension)
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
        private bool InvalidFileTypePdf(string extension)
        {
            extension = extension.ToLower();
            switch (extension)
            {
                case ".pdf":
                    return false;
                default:
                    return true;
            }
        }

        private void DeleteFileIfExist(string folderPath,string fileName)
        {
            try
            {
                string wildCard = fileName + "*.*";
                IEnumerable<string> files = Directory.EnumerateFiles(folderPath,wildCard,SearchOption.TopDirectoryOnly);

                if(files != null && files.Count() > 0)
                {
                    foreach(string file in files)
                    {
                        System.IO.File.Delete(file);
                    }
                }
            }
            catch(Exception)
            {
                throw;
            }
        }

        private bool CreateFolderIfNeeded(string path)
        {
            try
            {
                bool result = true;
                if(!Directory.Exists(path))
                {
                    try
                    {
                        Directory.CreateDirectory(path);
                    }
                    catch(Exception)
                    {
                        /*TODO: You must process this exception.*/
                        result = false;
                    }
                }

                return result;
            }
            catch(Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public virtual ActionResult UploadCredentialFile(FormCollection form)
        {
            HttpPostedFileBase file = Request.Files["MyCredentialFileFirstSitting"];

            bool isUploaded = false;
            string personId = form["Person.Id"].ToString();
            string passportUrl = form["Person.ImageFileUrl"].ToString();
            string message = "File upload failed";

            string path = null;
            string imageUrl = null;
            string imageUrlDisplay = null;

            try
            {
                if(file != null && file.ContentLength != 0)
                {
                    FileInfo fileInfo = new FileInfo(file.FileName);
                    string fileExtension = fileInfo.Extension;
                    string newFile = personId + "_credential_";
                    string newFileName = newFile + DateTime.Now.ToString().Replace("/","").Replace(":","").Replace(" ","") + fileExtension;

                    if (fileExtension.Contains("pdf"))
                    {
                        decimal sizeAllowed = 500;
                        string invalidFileMessage = InvalidFilePdf(file.ContentLength, fileExtension, sizeAllowed);
                        if (!string.IsNullOrEmpty(invalidFileMessage))
                        {
                            isUploaded = false;
                            TempData["CredentialimageUrl"] = null;
                            return Json(new { isUploaded = isUploaded, message = invalidFileMessage, imageUrl = passportUrl }, "text/html", JsonRequestBehavior.AllowGet);
                        }

                        string pathForSaving = Server.MapPath("~/Content/Junk/Credential");
                        if (this.CreateFolderIfNeeded(pathForSaving))
                        {
                            DeleteFileIfExist(pathForSaving, newFile);

                            file.SaveAs(Path.Combine(pathForSaving, newFileName));

                            isUploaded = true;
                            message = "File uploaded successfully!";

                            path = Path.Combine(pathForSaving, newFileName);
                            if (path != null)
                            {

                                imageUrl = "/Content/Junk/Credential/" + newFileName;
                                imageUrlDisplay = appRoot + imageUrl + "?t=" + DateTime.Now;

                                //imageUrlDisplay = "/ilaropoly" + imageUrl + "?t=" + DateTime.Now;
                                TempData["CredentialimageUrl"] = imageUrl;
                            }
                        }
                    }
                    else
                    {
                        isUploaded = false;
                        TempData["CredentialimageUrl"] = null;
                        return Json(new { isUploaded = isUploaded, message = "Only pdf is supported.", imageUrl = passportUrl }, "text/html", JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch(Exception ex)
            {
                message = string.Format("File upload failed: {0}",ex.Message);
            }
            TempData.Keep("CredentialimageUrl");
            return Json(new { isUploaded = isUploaded,message = message,imageUrl = imageUrl },"text/html",JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
       
        public virtual ActionResult UploadCredentialFile2(FormCollection form)
        {
            HttpPostedFileBase file = Request.Files["MyCredentialFileSecondSitting"];

            bool isUploaded = false;
            string personId = form["Person.Id"].ToString();
            string passportUrl = form["Person.ImageFileUrl"].ToString();
            string message = "File upload failed";

            string path = null;
            string imageUrl = null;
            string imageUrlDisplay = null;

            try
            {
                if(file != null && file.ContentLength != 0)
                {
                    FileInfo fileInfo = new FileInfo(file.FileName);
                    string fileExtension = fileInfo.Extension;
                    string newFile = personId + "_credential2_";
                    string newFileName = newFile + DateTime.Now.ToString().Replace("/","").Replace(":","").Replace(" ","") + fileExtension;

                    if (fileExtension.Contains("pdf"))
                    {
                        decimal sizeAllowed = 500;
                        string invalidFileMessage = InvalidFilePdf(file.ContentLength, fileExtension, sizeAllowed);
                       
                        if (!string.IsNullOrEmpty(invalidFileMessage))
                        {
                            isUploaded = false;
                            TempData["CredentialimageUrl2"] = null;
                            return Json(new { isUploaded = isUploaded, message = invalidFileMessage, imageUrl = passportUrl }, "text/html", JsonRequestBehavior.AllowGet);
                        }

                        string pathForSaving = Server.MapPath("~/Content/Junk/Credential");
                        if (this.CreateFolderIfNeeded(pathForSaving))
                        {

                            file.SaveAs(Path.Combine(pathForSaving, newFileName));

                            isUploaded = true;
                            message = "File uploaded successfully!";

                            path = Path.Combine(pathForSaving, newFileName);
                            if (path != null)
                            {


                                imageUrl = "/Content/Junk/Credential/" + newFileName;
                                imageUrlDisplay = appRoot + imageUrl + "?t=" + DateTime.Now;

                                //imageUrlDisplay = "/ilaropoly" + imageUrl + "?t=" + DateTime.Now;
                                TempData["CredentialimageUrl2"] = imageUrl;
                            }
                        }
                    }
                    else
                    {
                        isUploaded = false;
                        TempData["CredentialimageUrl2"] = null;
                        return Json(new { isUploaded = isUploaded, message = "Only pdf is supported.", imageUrl = passportUrl }, "text/html", JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch(Exception ex)
            {
                message = string.Format("File upload failed: {0}",ex.Message);
            }
            TempData.Keep("CredentialimageUrl2");
            return Json(new { isUploaded = isUploaded,message = message,imageUrl = imageUrl },"text/html",JsonRequestBehavior.AllowGet);
        }
     
        [HttpPost]
        public virtual ActionResult UploadCretificateFile(FormCollection form)
        {
            HttpPostedFileBase file = Request.Files["MyCredentialFileCertificate"];

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
                    string newFile = personId + "_certificate_credential_";
                    string newFileName = newFile + DateTime.Now.ToString().Replace("/", "").Replace(":", "").Replace(" ", "") + fileExtension;

                    if (fileExtension.Contains("pdf"))
                    {
                        decimal sizeAllowed = 500;
                        string invalidFileMessage = InvalidFilePdf(file.ContentLength, fileExtension, sizeAllowed);
                        
                        if (!string.IsNullOrEmpty(invalidFileMessage))
                        {
                            isUploaded = false;
                            TempData["CertificateimageUrl"] = null;
                            return Json(new { isUploaded = isUploaded, message = invalidFileMessage, imageUrl = passportUrl }, "text/html", JsonRequestBehavior.AllowGet);
                        }

                        string pathForSaving = Server.MapPath("~/Content/Junk/Credential");
                        if (this.CreateFolderIfNeeded(pathForSaving))
                        {
                            DeleteFileIfExist(pathForSaving, newFile);

                            file.SaveAs(Path.Combine(pathForSaving, newFileName));

                            isUploaded = true;
                            message = "File uploaded successfully!";

                            path = Path.Combine(pathForSaving, newFileName);
                            if (path != null)
                            {

                                imageUrl = "/Content/Junk/Credential/" + newFileName;
                                imageUrlDisplay = appRoot + imageUrl + "?t=" + DateTime.Now;

                                TempData["CertificateimageUrl"] = imageUrl;
                            }
                        }
                    }
                    else
                    {
                        isUploaded = false;
                        TempData["CertificateimageUrl"] = null;
                        return Json(new { isUploaded = isUploaded, message = "Only pdf is supported.", imageUrl = passportUrl }, "text/html", JsonRequestBehavior.AllowGet);
                    }
                }


            }
            catch (Exception ex)
            {
                message = string.Format("File upload failed: {0}", ex.Message);
            }
            TempData.Keep("CertificateimageUrl");
            return Json(new { isUploaded = isUploaded, message = message, imageUrl = imageUrl }, "text/html", JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public virtual ActionResult UploadITLetterOfCompletionFile(FormCollection form)
        {
            HttpPostedFileBase file = Request.Files["ItLetterOfCompletion"];

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
                    string newFile = personId + "_itLetter_credential_";
                    string newFileName = newFile + DateTime.Now.ToString().Replace("/", "").Replace(":", "").Replace(" ", "") + fileExtension;

                    if (fileExtension.Contains("pdf"))
                    {
                        decimal sizeAllowed = 500;
                        string invalidFileMessage = InvalidFilePdf(file.ContentLength, fileExtension, sizeAllowed);

                        if (!string.IsNullOrEmpty(invalidFileMessage))
                        {
                            isUploaded = false;
                            TempData["ItLetterimageUrl"] = null;
                            return Json(new { isUploaded = isUploaded, message = invalidFileMessage, imageUrl = passportUrl }, "text/html", JsonRequestBehavior.AllowGet);
                        }

                        string pathForSaving = Server.MapPath("~/Content/Junk/Credential");
                        if (this.CreateFolderIfNeeded(pathForSaving))
                        {
                            DeleteFileIfExist(pathForSaving, newFile);

                            file.SaveAs(Path.Combine(pathForSaving, newFileName));

                            isUploaded = true;
                            message = "File uploaded successfully!";

                            path = Path.Combine(pathForSaving, newFileName);
                            if (path != null)
                            {

                                imageUrl = "/Content/Junk/Credential/" + newFileName;
                                imageUrlDisplay = appRoot + imageUrl + "?t=" + DateTime.Now;

                                TempData["ItLetterimageUrl"] = imageUrl;
                            }
                        }
                    }
                    else
                    {
                        isUploaded = false;
                        TempData["ItLetterimageUrl"] = null;
                        return Json(new { isUploaded = isUploaded, message = "Only pdf is supported.", imageUrl = passportUrl }, "text/html", JsonRequestBehavior.AllowGet);
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

        private void ModifyOlevelResult(OLevelResult oLevelResult, List<OLevelResultDetail> oLevelResultDetails)
        {
            try
            {
                OlevelResultdDetailsAudit olevelResultdDetailsAudit = new OlevelResultdDetailsAudit();
                olevelResultdDetailsAudit.Operation = "Modify";
                olevelResultdDetailsAudit.Action = "Modify O level (Form Controller)";
                olevelResultdDetailsAudit.Client =  Request.LogonUserIdentity.Name + " (" + HttpContext.Request.UserHostAddress + ")";
                UserLogic loggeduser = new UserLogic();
                olevelResultdDetailsAudit.User = loggeduser.GetModelBy(u => u.User_Id == 1);

                OLevelResultDetailLogic oLevelResultDetailLogic = new OLevelResultDetailLogic();
                if (oLevelResult != null && oLevelResult.ExamNumber != null && oLevelResult.Type != null && oLevelResult.ExamYear > 0 && oLevelResult.ScratchCardPin != null)
                {
                    if (oLevelResult != null && oLevelResult.Id > 0)
                    {
                        oLevelResultDetailLogic.DeleteBy(oLevelResult,olevelResultdDetailsAudit);
                        OLevelResultLogic oLevelResultLogic = new OLevelResultLogic();
                        oLevelResultLogic.Modify(oLevelResult);
                    }
                    else
                    {
                        OLevelResultLogic oLevelResultLogic = new OLevelResultLogic();
                        OLevelResult newOLevelResult = oLevelResultLogic.Create(oLevelResult);
                        oLevelResult.Id = newOLevelResult.Id;
                    }

                    if (oLevelResultDetails != null && oLevelResultDetails.Count > 0)
                    {
                        List<OLevelResultDetail> olevelResultDetails = oLevelResultDetails.Where(m => m.Grade != null && m.Grade.Id > 0 && m.Subject != null && m.Subject.Id > 0).ToList();
                        foreach (OLevelResultDetail oLevelResultDetail in olevelResultDetails)
                        {
                            oLevelResultDetail.Header = oLevelResult;
                            oLevelResultDetailLogic.Create(oLevelResultDetail,olevelResultdDetailsAudit);
                        }

                        //oLevelResultDetailLogic.Create(olevelResultDetails);
                    }
                }
            }
            //catch (Exception)
            //{
            //    throw;
            //}
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",ve.PropertyName, ve.ErrorMessage);
                    }
                }
                
            }
            
        }

        public JsonResult UploadScannedFile(FormCollection formData)
        {
            JsonResultModel jsonModel = new JsonResultModel();
            try
            {
                HttpPostedFileBase file = Request.Files["image"];
                string type = formData["type"];
                long personId = Convert.ToInt64(formData["personId"]);

                if (!string.IsNullOrEmpty(type) && personId > 0)
                {
                    if (file != null && file.ContentLength != 0)
                    {
                        FileInfo fileInfo = new FileInfo(file.FileName);
                        string fileExtension = fileInfo.Extension;
                        string newFile = personId + "_" + type;
                        string newFileName = newFile + fileExtension;

                        if (fileExtension.Contains("pdf"))
                        {
                            decimal sizeAllowed = 50; //50kb
                            string invalidFileMessage = InvalidFile(file.ContentLength, fileExtension, sizeAllowed);
                            if (invalidFileMessage == "File type '" + fileExtension + "' is invalid! File type must be any of the following: .jpg, .jpeg, .png or .gif ")
                            {
                                invalidFileMessage = null;
                            }
                            if (!string.IsNullOrEmpty(invalidFileMessage))
                            {
                                jsonModel.IsError = true;
                                jsonModel.Message = invalidFileMessage;
                                return Json(jsonModel, JsonRequestBehavior.AllowGet);
                            }

                            string pathForSaving = Server.MapPath("~/Content/Credential/Credential");
                            if (CreateFolderIfNeeded(pathForSaving))
                            {
                                if (System.IO.File.Exists(Path.Combine(pathForSaving, newFileName)))
                                {
                                    System.IO.File.Delete(Path.Combine(pathForSaving, newFileName));
                                }
                                //DeleteFileIfExist(pathForSaving, personId);

                                file.SaveAs(Path.Combine(pathForSaving, newFileName));

                                PreviousEducationLogic previousEducationLogic = new PreviousEducationLogic();
                                PreviousEducation previousEducation = previousEducationLogic.GetModelsBy(p => p.Person_Id == personId).LastOrDefault();

                                if (previousEducation != null)
                                {
                                    if (type == "Certificate")
                                    {
                                        previousEducation.CertificateCopyUrl = "/Content/Credential/Credential/" + newFileName;
                                    }
                                    else if (type == "Result")
                                    {
                                        previousEducation.ResultCopyUrl = "/Content/Credential/Credential/" + newFileName;
                                    }

                                    previousEducationLogic.Modify(previousEducation);
                                }

                                jsonModel.IsError = false;
                                jsonModel.Message = "File uploaded successfully!";

                                string path = Path.Combine(pathForSaving, newFileName);
                                if (path != null)
                                {
                                    jsonModel.ImageFileUrl = "/Content/Credential/Credential/" + newFileName;
                                }
                            }
                        }
                        else
                        {
                            jsonModel.IsError = true;
                            jsonModel.Message = "Only pdf file is supported.";
                        }
                    }
                }
                else
                {
                    jsonModel.IsError = true;
                    jsonModel.Message = "Validation failure";
                }
            }
            catch (Exception ex)
            {
                jsonModel.IsError = true;
                jsonModel.Message = "Error! " + ex.Message;
            }

            return Json(jsonModel, JsonRequestBehavior.AllowGet);
        }
        public JsonResult RemoveScannedFile(FormCollection formData)
        {
            JsonResultModel jsonModel = new JsonResultModel();
            try
            {
                HttpPostedFileBase file = Request.Files["image"];

                if (file != null && file.ContentLength != 0)
                {
                    jsonModel.IsError = false;
                    jsonModel.Message = "File removed successfully!";
                }
            }
            catch (Exception ex)
            {
                jsonModel.IsError = true;
                jsonModel.Message = "Error! " + ex.Message;
            }

            return Json(jsonModel, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveBioData(string bioData)
        {
            ApplicationFormJsonModel jsonData = new ApplicationFormJsonModel();
            PostJambViewModel existingViewModel = (PostJambViewModel)TempData["viewModel"];
            TempData.Keep("viewModel");
            //TempData["viewModel"]
            try
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                jsonData = serializer.Deserialize<ApplicationFormJsonModel>(bioData);

                int sexId = 0, lgaId = 0, religionId = 0, abilityId = 0;
                long personId = 0;
                long applicantId = 0;
                if (!long.TryParse(jsonData.ApplicantId, out applicantId))
                {
                    applicantId = 0;
                }
                string homeTown = jsonData.HomeTown, mobilePhone = jsonData.MobilePhone, email = jsonData.Email, homeAddress = jsonData.HomeAddress, otherAbility = jsonData.OtherAbility;
                string extraCurricullarActivities = jsonData.ExtraCurricullarActivities;
                string stateId = jsonData.StateId;
                DateTime dob = new DateTime();

                string lastName = jsonData.LastName, firstName = jsonData.FirstName, otherName = jsonData.OtherName;

                if (int.TryParse(jsonData.SexId, out sexId) && int.TryParse(jsonData.LocalGovernmentId, out lgaId) && int.TryParse(jsonData.ReligionId, out religionId)
                    && int.TryParse(jsonData.AbilityId, out abilityId) && long.TryParse(jsonData.PersonId, out personId) && !string.IsNullOrEmpty(homeTown) &&
                    !string.IsNullOrEmpty(mobilePhone) && !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(homeAddress)
                    && !string.IsNullOrEmpty(extraCurricullarActivities) && !string.IsNullOrEmpty(stateId) && DateTime.TryParse(jsonData.DateOfBirth, out dob))
                {
                    TimeSpan difference = DateTime.Now - dob;

                    //Requirement Checks for Driving Course Programme Starts here ****

                    if(existingViewModel.AppliedCourse.Programme.Id == 7)
                    {
                        if (existingViewModel.AppliedCourse.Option != null)
                        {
                            if (difference.Days < 6570 && existingViewModel.AppliedCourse.Option.Id == 16)
                            {
                                jsonData.IsError = true;
                                jsonData.Message = "Graduate Driving Training Applicants are expected to not be less than eighteen(18) years of age!";
                                return Json(jsonData, JsonRequestBehavior.AllowGet);
                            }
                        }

                        if (existingViewModel.AppliedCourse.Option != null)
                        {
                            if (difference.Days < 7665 && existingViewModel.AppliedCourse.Option.Id == 17)
                            {
                                jsonData.IsError = true;
                                jsonData.Message = "For Technical Certificate in Driving instruction, applicants are expected to not be less than twenty-one(21) years of age!";
                                return Json(jsonData, JsonRequestBehavior.AllowGet);
                            }
                        }
                        if (existingViewModel.AppliedCourse.Option != null)
                        {
                            if (difference.Days < 8760 && existingViewModel.AppliedCourse.Option.Id == 18)
                            {
                                jsonData.IsError = true;
                                jsonData.Message = "For Professional Diploma in Driving Instruction, applicant are expected to not be less than twenty-four(24) years of age!";
                                return Json(jsonData, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    //Requirement Checks for Driving Course Programme Stops here ****
                    if (difference.Days == 0)
                    {
                        jsonData.IsError = true;
                        jsonData.Message = "Date of Birth cannot be todays date!";
                        return Json(jsonData, JsonRequestBehavior.AllowGet);
                    }
                    else if (difference.Days == 1)
                    {
                        jsonData.IsError = true;
                        jsonData.Message = "Date of Birth cannot be yesterdays date!";
                        return Json(jsonData, JsonRequestBehavior.AllowGet);
                    }

                    if (difference.Days < 4380)
                    {
                        jsonData.IsError = true;
                        jsonData.Message = "Applicant cannot be less than twelve years!";
                        return Json(jsonData, JsonRequestBehavior.AllowGet);
                    }

                    


                    if (string.IsNullOrEmpty(email))
                    {
                        jsonData.IsError = true;
                        jsonData.Message = "Email address is required!";
                        return Json(jsonData, JsonRequestBehavior.AllowGet);
                    }

                    if (string.IsNullOrEmpty(mobilePhone))
                    {
                        jsonData.IsError = true;
                        jsonData.Message = "Phone number is required!";
                        return Json(jsonData, JsonRequestBehavior.AllowGet);
                    }

                    PersonLogic personLogic = new PersonLogic();
                    Person person = personLogic.GetModelBy(p => p.Person_Id == personId);
                    if (person != null)
                    {
                        person.Sex = new Sex() { Id = Convert.ToByte(sexId) };
                        person.State = new State() { Id = stateId };
                        person.LocalGovernment = new LocalGovernment() { Id = lgaId };
                        person.Religion = new Religion() { Id = religionId };
                        person.HomeTown = homeTown;
                        person.MobilePhone = mobilePhone;
                        person.Email = email;
                        person.HomeAddress = homeAddress;
                        person.DateOfBirth = dob;

                        person.LastName = !string.IsNullOrEmpty(lastName) ? lastName : person.LastName;
                        person.FirstName = !string.IsNullOrEmpty(firstName) ? firstName : person.FirstName;
                        person.OtherName = !string.IsNullOrEmpty(otherName) ? otherName : person.OtherName;

                        personLogic.Modify(person);
                    }

                    //ApplicantLogic applicantLogic = new ApplicantLogic();
                    //Model.Model.Applicant applicant = applicantLogic.GetModelBy(a => a.Person_Id == personId);
                    //if (applicant == null)
                    //{
                    //    applicant = new Model.Model.Applicant();
                    //    applicant.Ability = new Ability(){ Id = abilityId };
                    //    applicant.Person = new Person(){ Id = personId };
                    //    applicant.ExtraCurricullarActivities = extraCurricullarActivities;
                    //    applicant.OtherAbility = string.IsNullOrEmpty(otherAbility) ? null : otherAbility;

                    //    applicantLogic.Create(applicant);
                    //}
                    //else
                    //{
                    //    applicant.Ability = new Ability() { Id = abilityId };
                    //    applicant.ExtraCurricullarActivities = extraCurricullarActivities;
                    //    applicant.OtherAbility = otherAbility;

                    //    applicantLogic.Modify(applicant);
                    //}

                    jsonData.IsError = false;
                    jsonData.Message = "Operation Successful! ";
                }
                else
                {
                    jsonData.IsError = true;
                    jsonData.Message = "Validation for one of the fields failed, kindly check and try again! ";
                }
            }
            catch (Exception ex)
            {
                jsonData.IsError = true;
                jsonData.Message = "An error occured! " + ex.Message;
            }

            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveNextOfKinDetails(string PersonId, string Name, string Address, string MobilePhone, string RelationShipId)
        {
            ApplicationFormJsonModel jsonData = new ApplicationFormJsonModel();
            try
            {
                if (!string.IsNullOrEmpty(PersonId) && !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Address) && !string.IsNullOrEmpty(MobilePhone) && !string.IsNullOrEmpty(RelationShipId))
                {
                    long personId = Convert.ToInt64(PersonId);
                    NextOfKinLogic nextOfKinLogic = new NextOfKinLogic();
                    PersonLogic personLogic = new PersonLogic();
                    Person person = personLogic.GetModelBy(p => p.Person_Id == personId);
                    if (person != null)
                    {
                        if (MobilePhone.Length > 15)
                        {
                            jsonData.IsError = true;
                            jsonData.Message = "Mobile number validation failed.";
                            return Json(jsonData, JsonRequestBehavior.AllowGet);
                        }
                        NextOfKin nextOfKin = nextOfKinLogic.GetModelsBy(n => n.Person_Id == personId).LastOrDefault();
                        if (nextOfKin == null)
                        {
                            nextOfKin = new NextOfKin();
                            nextOfKin.Person = person;
                            nextOfKin.MobilePhone = MobilePhone;
                            nextOfKin.ContactAddress = Address;
                            nextOfKin.Name = Name;
                            nextOfKin.Relationship = new Relationship()
                            {
                                Id = Convert.ToInt32(RelationShipId)
                            };
                            nextOfKin.PersonType = new PersonType()
                            {
                                Id = person.Type.Id
                            };

                            nextOfKinLogic.Create(nextOfKin);
                        }
                        else
                        {
                            nextOfKin.MobilePhone = MobilePhone;
                            nextOfKin.ContactAddress = Address;
                            nextOfKin.Name = Name;
                            nextOfKin.Relationship = new Relationship()
                            {
                                Id = Convert.ToInt32(RelationShipId)
                            };

                            nextOfKinLogic.Modify(nextOfKin);
                        }

                        jsonData.IsError = false;
                        jsonData.Message = "Operation Successful! ";
                    }
                }
                else
                {
                    jsonData.IsError = true;
                    jsonData.Message = "Validation for one of the fields failed, kindly check and try again! ";
                }
            }
            catch (Exception ex)
            {
                jsonData.IsError = true;
                jsonData.Message = "An error occured! " + ex.Message;
            }

            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveUtmeDetails(string jambnumber, string jambscore, string firstSubject, string secondSubject, string thirdsubject, string fourthSubject, string institutionChoiceId,
            string personid, int firstSubjectScore, int secondSubjectScore, int thirdSubjectScore, int fourthSubjectScore)
        {

            ApplicationFormJsonModel jsonData = new ApplicationFormJsonModel();
            try
            {
                if (!string.IsNullOrEmpty(jambnumber) && !string.IsNullOrEmpty(jambscore) && !string.IsNullOrEmpty(firstSubject) && !string.IsNullOrEmpty(secondSubject) && !string.IsNullOrEmpty(thirdsubject) &&
                   !string.IsNullOrEmpty(fourthSubject) && !string.IsNullOrEmpty(institutionChoiceId) && !string.IsNullOrEmpty(personid) && firstSubjectScore >= 0 && secondSubjectScore >= 0 &&
                    thirdSubjectScore >= 0 && fourthSubjectScore >= 0)
                {
                    long personId = Convert.ToInt64(personid);
                    ApplicantJambDetailLogic applicantJambDetailLogic = new ApplicantJambDetailLogic();
                    PersonLogic personLogic = new PersonLogic();
                    Person person = personLogic.GetModelBy(p => p.Person_Id == personId);
                    if (person != null)
                    {
                        ApplicantJambDetail applicantJambDetail = applicantJambDetailLogic.GetModelsBy(a => a.Person_Id == personId).LastOrDefault();
                        if (applicantJambDetail == null)
                        {
                            applicantJambDetail = new ApplicantJambDetail();

                            applicantJambDetail.Person = person;
                            applicantJambDetail.InstitutionChoice = new InstitutionChoice()
                            {
                                Id = Convert.ToInt32(institutionChoiceId)
                            };
                            applicantJambDetail.JambRegistrationNumber = jambnumber;
                            applicantJambDetail.JambScore = Convert.ToSByte(jambscore);
                            applicantJambDetail.Subject1 = new OLevelSubject()
                            {
                                Id = Convert.ToInt32(firstSubject)
                            };
                            applicantJambDetail.Subject2 = new OLevelSubject()
                            {
                                Id = Convert.ToInt32(secondSubject)
                            };
                            applicantJambDetail.Subject3 = new OLevelSubject()
                            {
                                Id = Convert.ToInt32(thirdsubject)
                            };
                            applicantJambDetail.Subject4 = new OLevelSubject()
                            {
                                Id = Convert.ToInt32(fourthSubject)
                            };

                            applicantJambDetailLogic.Create(applicantJambDetail);
                        }
                        else
                        {
                            applicantJambDetail.InstitutionChoice = new InstitutionChoice()
                            {
                                Id = Convert.ToInt32(institutionChoiceId)
                            };
                            //applicantJambDetail.JambRegistrationNumber = jambnumber;
                            applicantJambDetail.JambScore = Convert.ToInt16(jambscore);

                            applicantJambDetail.Subject1 = new OLevelSubject()
                            {
                                Id = Convert.ToInt32(firstSubject)
                            };
                            applicantJambDetail.Subject2 = new OLevelSubject()
                            {
                                Id = Convert.ToInt32(secondSubject)
                            };
                            applicantJambDetail.Subject3 = new OLevelSubject()
                            {
                                Id = Convert.ToInt32(thirdsubject)
                            };
                            applicantJambDetail.Subject4 = new OLevelSubject()
                            {
                                Id = Convert.ToInt32(fourthSubject)
                            };

                            applicantJambDetail.JambRegistrationNumber = jambnumber;

                            applicantJambDetailLogic.Modify(applicantJambDetail);
                        }

                        jsonData.IsError = false;
                        jsonData.Message = "Operation Successful! ";

                    }
                    else
                    {
                        jsonData.IsError = true;
                        jsonData.Message = "Validation for one of the fields failed, kindly check and try again! ";
                    }
                }
                else
                {
                    jsonData.IsError = true;
                    jsonData.Message = "Validation for one of the fields failed, kindly check and try again! ";
                }
            }
            catch (Exception ex)
            {
                jsonData.IsError = true;
                jsonData.Message = "An error occured! " + ex.Message;
            }

            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SavePreviousEducation(string course, string endDate, int qualificationId, int resultGradeId, int ITDurationId, long personId, string previousSchoolId, string itStartDate, string itEndDate)
        {
            const int ONE_YEAR = 365;

            ApplicationFormJsonModel jsonData = new ApplicationFormJsonModel();
            try
            {
                //DateTime startDateVal = new DateTime();
                DateTime endDateVal = new DateTime();
                DateTime itStartDateVal = new DateTime();
                DateTime itEndDateVal = new DateTime();
                DateTime startD_ = new DateTime();
                DateTime endD_ = new DateTime();
                AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
                AppliedCourse appliedCourse = appliedCourseLogic.GetModelBy(x => x.APPLICATION_FORM.Person_Id == personId);

                //Driving Course
                if(appliedCourse != null && appliedCourse.Programme.Id == 7)
                {
                    
                    if (DateTime.TryParse(itStartDate, out itStartDateVal) && DateTime.TryParse(endDate, out endDateVal))
                    {
                         startD_ = itStartDateVal;
                        endD_ = endDateVal;
                    }
                    PreviousEducationLogic previousEducationLogic = new PreviousEducationLogic();

                    PreviousEducation previousEducation = previousEducationLogic.GetModelsBy(p => p.Person_Id == personId).LastOrDefault();
                    if (previousEducation == null)
                    {
                        previousEducation = new PreviousEducation();
                        previousEducation.EndDate = endD_;
                        previousEducation.StartDate = startD_;
                        previousEducation.Course = course;
                        previousEducation.Person = new Person() { Id = personId };
                        previousEducation.ITDuration = new ITDuration() { Id = ITDurationId };
                        previousEducation.ResultGrade = new ResultGrade() { Id = resultGradeId };
                        previousEducation.Qualification = new EducationalQualification() { Id = qualificationId };
                        previousEducation.SchoolName = previousSchoolId.Trim();

                        previousEducationLogic.Create(previousEducation);
                        jsonData.IsError = false;
                        jsonData.Message = "Operation Successful! ";
                        //return Json(jsonData, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        previousEducation.EndDate = endDateVal;
                        previousEducation.StartDate = startD_;
                        previousEducation.Course = course;
                        previousEducation.Person = new Person() { Id = personId };
                        previousEducation.ITDuration = new ITDuration() { Id = ITDurationId };
                        previousEducation.ResultGrade = new ResultGrade() { Id = resultGradeId };
                        previousEducation.Qualification = new EducationalQualification() { Id = qualificationId };
                        previousEducation.SchoolName = previousSchoolId.Trim();

                        previousEducationLogic.Modify(previousEducation);
                    }

                    jsonData.IsError = false;
                    jsonData.Message = "Operation Successful! ";
                    return Json(jsonData, JsonRequestBehavior.AllowGet);
                }

                if (!string.IsNullOrEmpty(course) && DateTime.TryParse(endDate, out endDateVal) &&
                    qualificationId > 0 && resultGradeId > 0 && ITDurationId > 0 && personId > 0 && DateTime.TryParse(itStartDate, out itStartDateVal) && DateTime.TryParse(itEndDate, out itEndDateVal)
                    && !string.IsNullOrEmpty(previousSchoolId))
                {
                    //DateTime previousEducationStartDate = startDateVal;
                    DateTime previousEducationEndDate = endDateVal;


                    //bool isStartDateInTheFuture = Utility.IsDateInTheFuture(previousEducationStartDate);
                    bool isEndDateInTheFuture = Utility.IsDateInTheFuture(previousEducationEndDate);

                    bool isITStartDateInTheFuture = Utility.IsDateInTheFuture(itStartDateVal);
                    bool isITEndDateInTheFuture = Utility.IsDateInTheFuture(itEndDateVal);

                    if (isITStartDateInTheFuture)
                    {
                        jsonData.IsError = true;
                        jsonData.Message = "IT Start Date cannot be a future date!";
                        return Json(jsonData, JsonRequestBehavior.AllowGet);
                    }
                    if (isITEndDateInTheFuture)
                    {
                        jsonData.IsError = true;
                        jsonData.Message = "IT End Date cannot be a future date!";
                        return Json(jsonData, JsonRequestBehavior.AllowGet);
                    }

                    if (Utility.IsStartDateGreaterThanEndDate(itStartDateVal, itEndDateVal))
                    {
                        jsonData.IsError = true;
                        jsonData.Message = "IT Start Date '" + itStartDateVal.ToShortDateString() + "' cannot be greater than End Date '" +
                                            itEndDateVal.ToShortDateString() + "'! Please modify and try again.";
                        return Json(jsonData, JsonRequestBehavior.AllowGet);
                    }

                    //if (isStartDateInTheFuture)
                    //{
                    //    jsonData.IsError = true;
                    //    jsonData.Message = "Previous Education Start Date cannot be a future date!";
                    //    return Json(jsonData, JsonRequestBehavior.AllowGet);
                    //}
                    if (isEndDateInTheFuture)
                    {
                        jsonData.IsError = true;
                        jsonData.Message = "Previous Education End Date cannot be a future date!";
                        return Json(jsonData, JsonRequestBehavior.AllowGet);
                    }
                    //if (Utility.IsStartDateGreaterThanEndDate(previousEducationStartDate, previousEducationEndDate))
                    //{
                    //    jsonData.IsError = true;
                    //    jsonData.Message = "Previous Education Start Date '" + previousEducationStartDate.ToShortDateString() + "' cannot be greater than End Date '" + 
                    //                        previousEducationEndDate.ToShortDateString() + "'! Please modify and try again.";
                    //    return Json(jsonData, JsonRequestBehavior.AllowGet);
                    //}
                    //if (Utility.IsDateOutOfRange(previousEducationStartDate, previousEducationEndDate, ONE_YEAR))
                    //{
                    //    jsonData.IsError = true;
                    //    jsonData.Message = "Previous Education duration must not be less than one year, twelve months or 365 days to be qualified!";
                    //    return Json(jsonData, JsonRequestBehavior.AllowGet);
                    //}

                    PreviousEducationLogic previousEducationLogic = new PreviousEducationLogic();
                    
                    PreviousEducation previousEducation = previousEducationLogic.GetModelsBy(p => p.Person_Id == personId).LastOrDefault();
                    if (previousEducation == null)
                    {
                        previousEducation = new PreviousEducation();
                        previousEducation.EndDate = endDateVal;
                        //previousEducation.StartDate = startDateVal;
                        previousEducation.Course = course;
                        previousEducation.Person = new Person() { Id = personId };
                        previousEducation.ITDuration = new ITDuration() { Id = ITDurationId };
                        previousEducation.ResultGrade = new ResultGrade() { Id = resultGradeId };
                        previousEducation.Qualification = new EducationalQualification() { Id = qualificationId };
                        //previousEducation.SchoolName = tertiaryInstitution != null ? tertiaryInstitution.Name : previousSchoolId.Trim();

                        previousEducationLogic.Create(previousEducation);
                    }
                    else
                    {
                        previousEducation.EndDate = endDateVal;
                        //previousEducation.StartDate = startDateVal;
                        previousEducation.Course = course;
                        previousEducation.Person = new Person() { Id = personId };
                        previousEducation.ITDuration = new ITDuration() { Id = ITDurationId };
                        previousEducation.ResultGrade = new ResultGrade() { Id = resultGradeId };
                        previousEducation.Qualification = new EducationalQualification() { Id = qualificationId };
                        //previousEducation.SchoolName = tertiaryInstitution != null ? tertiaryInstitution.Name : previousSchoolId.Trim();

                        previousEducationLogic.Modify(previousEducation);
                    }

                    jsonData.IsError = false;
                    jsonData.Message = "Operation Successful! ";
                }
                else
                {
                    jsonData.IsError = true;
                    jsonData.Message = "Validation for one of the fields failed, kindly check and try again! ";
                }
            }
            catch (Exception ex)
            {
                jsonData.IsError = true;
                jsonData.Message = "An error occured! " + ex.Message;
            }

            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        

        public JsonResult SaveOLevelResultAndApplication(string dataArray, string firstSitting, string secondSitting)
        {
            ApplicationForm newApplicationForm = null;
            var existingViewModel = new PostJambViewModel();
            PostJambViewModel populatedModel = (PostJambViewModel)TempData["viewModel"];
            TempData.Keep("viewModel");
            var globalFormId = "";

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            ApplicationFormJsonModel applicationJsonData = serializer.Deserialize<ApplicationFormJsonModel>(dataArray);
            List<OLevelResultDetailJsonModel> firstSittingOLevelJsonData = serializer.Deserialize<List<OLevelResultDetailJsonModel>>(firstSitting);
            List<OLevelResultDetailJsonModel> secondSittingOLevelJsonData = serializer.Deserialize<List<OLevelResultDetailJsonModel>>(secondSitting);
            try
            {
             
                bool applicationExist = Convert.ToBoolean(applicationJsonData.ApplicationAlreadyExist);
                PopulateModelsFromJsonData(existingViewModel, applicationJsonData, applicationExist);
                PopulateOLevelResultDetailFromJsonData(existingViewModel, firstSittingOLevelJsonData, 1);
                PopulateOLevelResultDetailFromJsonData(existingViewModel, secondSittingOLevelJsonData, 2);

                PreviousEducationLogic previousEducationLogic = new PreviousEducationLogic();
                ApplicationFormLogic applicationFormLogic = new ApplicationFormLogic();
                DrivingExperienceLogic drivingExperienceLogic = new DrivingExperienceLogic();
                DrivingExperience drivingExperience = new DrivingExperience();

                PreviousEducation previousEducation = previousEducationLogic.GetModelsBy(p => p.Person_Id == existingViewModel.Person.Id).LastOrDefault();
                existingViewModel.PreviousEducation = previousEducation;

                ApplicationForm existingForm = applicationFormLogic.GetModelsBy(a => a.Person_Id == existingViewModel.Person.Id).LastOrDefault();
                if (existingForm != null)
                {
                    existingViewModel.ApplicationForm = existingForm;
                    globalFormId = Convert.ToString(existingForm.Id);
                    applicationExist = true;
                }

                if (existingViewModel.ApplicationForm == null || existingViewModel.ApplicationForm.Id <= 0 || existingViewModel.ApplicationForm.Number == null)
                {
                    using (var transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
                    {
                        ApplicationForm applicationForm = new ApplicationForm();
                        applicationForm.ProgrammeFee = new ApplicationProgrammeFee
                        {
                            Id = existingViewModel.ApplicationProgrammeFee.Id
                        };
                        applicationForm.Setting = new ApplicationFormSetting
                        {
                            Id = existingViewModel.ApplicationFormSetting.Id
                        };

                        applicationForm.DateSubmitted = DateTime.Now;
                        applicationForm.Person = existingViewModel.Person;
                        applicationForm.Payment = existingViewModel.Payment;
                        applicationForm.ExamNumber = null;
                        applicationForm.ExamSerialNumber = null;
                        applicationForm.SerialNumber = null;
                        applicationForm.Number = null;
                        applicationForm.Release = false;
                        applicationForm.Rejected = false;
                        applicationForm.RejectReason = null;
                        applicationForm.Remarks = null;
                        //applicationForm.ProgrammeFee.Programme = existingViewModel.ApplicationProgrammeFee.Programme;

                        //Abundance_NkEntities abundanceNkEntities = new Abundance_NkEntities();
                        //var result = abundanceNkEntities.GenerateApplicationNumber(null, null, null, null, applicationForm.Setting.Id, applicationForm.ProgrammeFee.Id, applicationForm.Payment.Id,
                        //                                    applicationForm.Person.Id, applicationForm.DateSubmitted, false, false, null, null);

                        applicationForm = applicationFormLogic.Create(applicationForm);
                        globalFormId = Convert.ToString(applicationForm.Id);
                        if (applicationForm != null && populatedModel.AppliedCourse.Option != null)
                        {
                            existingViewModel.ApplicationForm = applicationForm;
                            populatedModel.ApplicationForm = applicationForm;
                            if (populatedModel.AppliedCourse.Option.Id == 16 || populatedModel.AppliedCourse.Option.Id == 17)
                            {
                                existingViewModel.Applicant.Person = existingViewModel.Person;
                                existingViewModel.Applicant.ApplicationForm = applicationForm;
                                existingViewModel.Applicant.Status = new ApplicantStatus
                                {
                                    Id = (int)ApplicantStatus.Status.SubmittedApplicationForm
                                };
                                var _applicantLogic = new ApplicantLogic();
                                _applicantLogic.Create(existingViewModel.Applicant);
                                transaction.Complete();
                                applicationJsonData.ApplicationFormId = Convert.ToString(applicationForm.Id);
                                globalFormId = Convert.ToString(applicationForm.Id);
                                applicationJsonData.IsError = false;
                                applicationJsonData.Message = "Saved Successfuly";
                                return Json(applicationJsonData, JsonRequestBehavior.AllowGet);
                            }
                        }

                        drivingExperience = drivingExperienceLogic.GetModelBy(d => d.Person_Id == existingViewModel.Person.Id);
                        if(drivingExperience != null)
                        {
                            drivingExperience.Application_Form_Id = applicationForm.Id;
                            drivingExperienceLogic.Modify(drivingExperience);
                        }

                        //foreach (var item in result.ToList())
                        //{
                        //    applicationForm.Number = item.Application_Form_Number;
                        //    applicationForm.Id = item.Application_Form_Id;
                        //    applicationForm.ExamNumber = item.Application_Exam_Number;
                        //}

                        existingViewModel.ApplicationFormNumber = applicationForm.Number;
                        applicationForm.Person = existingViewModel.Person;
                        existingViewModel.ApplicationForm = applicationForm;
                        newApplicationForm = applicationForm;

                        existingViewModel.Applicant.Person = existingViewModel.Person;
                        existingViewModel.Applicant.ApplicationForm = newApplicationForm;
                        existingViewModel.Applicant.Status = new ApplicantStatus
                        {
                            Id = (int)ApplicantStatus.Status.SubmittedApplicationForm
                        };
                        var applicantLogic = new ApplicantLogic();
                        applicantLogic.Create(existingViewModel.Applicant);

                        //update application no in applied course object
                        existingViewModel.AppliedCourse.Person = existingViewModel.Person;
                        existingViewModel.AppliedCourse.ApplicationForm = newApplicationForm;
                        var appliedCourseLogic = new AppliedCourseLogic();
                        appliedCourseLogic.Modify(existingViewModel.AppliedCourse);

                        //var sponsorLogic = new SponsorLogic();
                        //existingViewModel.Sponsor.ApplicationForm = newApplicationForm;
                        //existingViewModel.Sponsor.Person = existingViewModel.Person;
                        //sponsorLogic.Create(existingViewModel.Sponsor);
                        Message msg = new Message();

                        if (InvalidNumberOfOlevelSubject(existingViewModel.FirstSittingOLevelResultDetails, existingViewModel.SecondSittingOLevelResultDetails))
                        {
                            
                            applicationJsonData.IsError = true;
                            msg = (Message)TempData["Message"];
                            applicationJsonData.Message = "O-Level Validation failed. " + msg.Description;
                            return Json(applicationJsonData, JsonRequestBehavior.AllowGet);
                        }

                        if (InvalidOlevelSubjectOrGrade(existingViewModel.FirstSittingOLevelResultDetails, existingViewModel.OLevelSubjects, existingViewModel.OLevelGrades, FIRST_SITTING))
                        {
                            applicationJsonData.IsError = true;
                            msg = (Message)TempData["Message"];
                            applicationJsonData.Message = "O-Level Validation failed. " + msg.Description;
                            return Json(applicationJsonData, JsonRequestBehavior.AllowGet);
                        }

                        if (existingViewModel.SecondSittingOLevelResult != null)
                        {
                            if (existingViewModel.SecondSittingOLevelResult.ExamNumber != null && existingViewModel.SecondSittingOLevelResult.Type != null &&
                                existingViewModel.SecondSittingOLevelResult.Type.Id > 0 && existingViewModel.SecondSittingOLevelResult.ExamYear > 0)
                            {
                                if (InvalidOlevelSubjectOrGrade(existingViewModel.SecondSittingOLevelResultDetails, existingViewModel.OLevelSubjects, existingViewModel.OLevelGrades, SECOND_SITTING))
                                {
                                    applicationJsonData.IsError = true;
                                    msg = (Message)TempData["Message"];
                                    applicationJsonData.Message = "O-Level Validation failed. " + msg.Description;
                                    return Json(applicationJsonData, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }

                        if (InvalidOlevelResultHeaderInformation(existingViewModel.FirstSittingOLevelResultDetails, existingViewModel.FirstSittingOLevelResult, FIRST_SITTING))
                        {
                            applicationJsonData.IsError = true;
                            msg = (Message)TempData["Message"];
                            applicationJsonData.Message = "O-Level Validation failed. " + msg.Description;
                            return Json(applicationJsonData, JsonRequestBehavior.AllowGet);
                        }

                        if (InvalidOlevelResultHeaderInformation(existingViewModel.SecondSittingOLevelResultDetails, existingViewModel.SecondSittingOLevelResult, SECOND_SITTING))
                        {
                            applicationJsonData.IsError = true;
                            msg = (Message)TempData["Message"];
                            applicationJsonData.Message = "O-Level Validation failed. " + msg.Description;
                            return Json(applicationJsonData, JsonRequestBehavior.AllowGet);
                        }

                        if (NoOlevelSubjectSpecified(existingViewModel.FirstSittingOLevelResultDetails, existingViewModel.FirstSittingOLevelResult, FIRST_SITTING))
                        {
                            applicationJsonData.IsError = true;
                            msg = (Message)TempData["Message"];
                            applicationJsonData.Message = "O-Level Validation failed. " + msg.Description;
                            return Json(applicationJsonData, JsonRequestBehavior.AllowGet);
                        }
                        if (NoOlevelSubjectSpecified(existingViewModel.SecondSittingOLevelResultDetails, existingViewModel.SecondSittingOLevelResult, SECOND_SITTING))
                        {
                            applicationJsonData.IsError = true;
                            msg = (Message)TempData["Message"];
                            applicationJsonData.Message = "O-Level Validation failed. " + msg.Description;
                            return Json(applicationJsonData, JsonRequestBehavior.AllowGet);
                        }

                        if (InvalidOlevelType(existingViewModel.FirstSittingOLevelResult.Type, existingViewModel.SecondSittingOLevelResult.Type))
                        {
                            applicationJsonData.IsError = true;
                            msg = (Message)TempData["Message"];
                            applicationJsonData.Message = "O-Level Validation failed. " + msg.Description;
                            return Json(applicationJsonData, JsonRequestBehavior.AllowGet);
                        }


                        var oLevelResultLogic = new OLevelResultLogic();
                        var oLevelResultDetailLogic = new OLevelResultDetailLogic();
                        if (existingViewModel.FirstSittingOLevelResult != null && existingViewModel.FirstSittingOLevelResult.ExamNumber != null &&
                            existingViewModel.FirstSittingOLevelResult.Type != null &&
                            existingViewModel.FirstSittingOLevelResult.ExamYear > 0 && existingViewModel.FirstSittingOLevelResult.ScratchCardPin != null)
                        {
                            OLevelResult existingOLevelResult = oLevelResultLogic.GetModelsBy(o => o.Application_Form_Id == newApplicationForm.Id && o.O_Level_Exam_Sitting_Id == 1).LastOrDefault();
                            if (existingOLevelResult == null)
                            {
                                existingViewModel.FirstSittingOLevelResult.ApplicationForm = newApplicationForm;
                                existingViewModel.FirstSittingOLevelResult.Person = existingViewModel.Person;
                                existingViewModel.FirstSittingOLevelResult.Sitting = new OLevelExamSitting { Id = 1 };
                                OLevelResult firstSittingOLevelResult = oLevelResultLogic.Create(existingViewModel.FirstSittingOLevelResult);

                                if (existingViewModel.FirstSittingOLevelResultDetails != null && existingViewModel.FirstSittingOLevelResultDetails.Count > 0 && firstSittingOLevelResult != null)
                                {
                                    List<OLevelResultDetail> olevelResultDetails = existingViewModel.FirstSittingOLevelResultDetails.Where(m => m.Grade != null && m.Subject != null).ToList();
                                    foreach (OLevelResultDetail oLevelResultDetail in olevelResultDetails)
                                    {
                                        oLevelResultDetail.Header = firstSittingOLevelResult;
                                    }

                                    oLevelResultDetailLogic.Create(olevelResultDetails);
                                }
                            }
                        }

                        if (existingViewModel.SecondSittingOLevelResult != null &&
                            existingViewModel.SecondSittingOLevelResult.ExamNumber != null &&
                            existingViewModel.SecondSittingOLevelResult.Type != null &&
                            existingViewModel.SecondSittingOLevelResult.ExamYear > 0 &&
                            existingViewModel.SecondSittingOLevelResult.ScratchCardPin != null)
                        {
                            OLevelResult existingOLevelResult = oLevelResultLogic.GetModelsBy(o => o.Application_Form_Id == newApplicationForm.Id && o.O_Level_Exam_Sitting_Id == 2).LastOrDefault();
                            if (existingOLevelResult == null)
                            {
                                List<OLevelResultDetail> olevelResultDetails = existingViewModel.SecondSittingOLevelResultDetails.Where(m => m.Grade != null && m.Subject != null).ToList();
                                if (olevelResultDetails != null && olevelResultDetails.Count > 0)
                                {
                                    existingViewModel.SecondSittingOLevelResult.ApplicationForm = newApplicationForm;
                                    existingViewModel.SecondSittingOLevelResult.Person = existingViewModel.Person;
                                    existingViewModel.SecondSittingOLevelResult.Sitting = new OLevelExamSitting { Id = 2 };
                                    OLevelResult secondSittingOLevelResult = oLevelResultLogic.Create(existingViewModel.SecondSittingOLevelResult);

                                    if (existingViewModel.SecondSittingOLevelResultDetails != null && existingViewModel.SecondSittingOLevelResultDetails.Count > 0 && secondSittingOLevelResult != null)
                                    {
                                        foreach (OLevelResultDetail oLevelResultDetail in olevelResultDetails)
                                        {
                                            oLevelResultDetail.Header = secondSittingOLevelResult;
                                        }

                                        oLevelResultDetailLogic.Create(olevelResultDetails);
                                    }
                                }
                            }
                        }

                        //set reject reason
                        if (existingViewModel.AppliedCourse.Programme.Id == 3 || existingViewModel.AppliedCourse.Programme.Id == 4 || existingViewModel.AppliedCourse.Programme.Id == 5)
                        {
                            newApplicationForm.Release = false;
                            existingViewModel.AppliedCourse.Person = existingViewModel.Person;

                            AdmissionCriteriaLogic admissionCriteriaLogic = new AdmissionCriteriaLogic();
                            string rejectReason = admissionCriteriaLogic.EvaluateApplication(existingViewModel.AppliedCourse, existingViewModel.PreviousEducation);
                            if (string.IsNullOrEmpty(rejectReason))
                            {
                                newApplicationForm.Rejected = false;
                            }
                            else
                            {
                                newApplicationForm.Rejected = true;
                                newApplicationForm.RejectReason = rejectReason;

                                if (!applicationFormLogic.SetRejectReason(newApplicationForm))
                                {
                                    //Do nothing
                                    //throw new Exception("Rejected! " + rejectReason);
                                }
                            }

                            existingViewModel.ApplicationForm = newApplicationForm;
                        }
                        else if(existingViewModel.AppliedCourse.Programme.Id == 7)
                        {
                            transaction.Complete();

                            applicationJsonData.IsError = false;
                            applicationJsonData.ApplicationFormId = globalFormId;
                            return Json(applicationJsonData, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            newApplicationForm.Release = true;
                            //newApplicationForm.Rejected = false;
                            existingViewModel.AppliedCourse.Person = existingViewModel.Person;

                            AdmissionCriteriaLogic admissionCriteriaLogic = new AdmissionCriteriaLogic();
                            string rejectReason = admissionCriteriaLogic.EvaluateApplication(existingViewModel.AppliedCourse);
                            if (string.IsNullOrEmpty(rejectReason))
                            {
                                newApplicationForm.Rejected = false;
                            }
                            else
                            {
                                newApplicationForm.Rejected = true;
                                newApplicationForm.RejectReason = rejectReason;

                                if (!applicationFormLogic.SetRejectReason(newApplicationForm))
                                {
                                    //if (existingViewModel.Programme.Id == 2)
                                    //{
                                    //    //
                                    //}
                                    //else
                                    //{
                                    //    throw new Exception("Rejected! " + rejectReason);
                                    //}
                                    //Do nothing
                                }
                            }

                            //bool checkAR = false;
                            //if (existingViewModel.Programme.Id == 2)
                            //{
                            //    OLevelResultDetailLogic oLevelResultLogic1 = new OLevelResultDetailLogic();
                            //    List<OLevelResultDetail> oLevelResults = oLevelResultLogic1.GetModelsBy(o => o.APPLICANT_O_LEVEL_RESULT.Application_Form_Id == existingViewModel.AppliedCourse.ApplicationForm.Id);
                            //    for (int i = 0; i < oLevelResults.Count; i++)
                            //    {
                            //        if (oLevelResults[i].Grade.Id == 10)
                            //        {
                            //            checkAR = true;
                            //        }
                            //    }
                            //}

                            //if (!checkAR)
                            //{
                            //    AdmissionCriteriaLogic admissionCriteriaLogic = new AdmissionCriteriaLogic();
                            //    string rejectReason = admissionCriteriaLogic.EvaluateApplication(existingViewModel.AppliedCourse);
                            //    if (string.IsNullOrEmpty(rejectReason))
                            //    {
                            //        newApplicationForm.Rejected = false;
                            //    }
                            //    else
                            //    {
                            //        newApplicationForm.Rejected = true;
                            //        newApplicationForm.RejectReason = rejectReason;

                            //        if (!applicationFormLogic.SetRejectReason(newApplicationForm))
                            //        {
                            //            if (existingViewModel.Programme.Id == 2)
                            //            {
                            //                //
                            //            }
                            //            else
                            //            {
                            //                throw new Exception("Rejected! " + rejectReason);
                            //            }

                            //        }
                            //    }
                            //}
                        }

                        //Modify previous education
                        //PreviousEducation previousEducation = previousEducationLogic.GetModelsBy(p => p.Person_Id == existingViewModel.Person.Id).LastOrDefault();
                        if (previousEducation != null && previousEducation.ApplicationForm == null)
                        {
                            previousEducation.ApplicationForm = newApplicationForm;
                            previousEducationLogic.Modify(previousEducation);
                        }

                        //Modify jamb detail
                        ApplicantJambDetailLogic jambDetailLogic = new ApplicantJambDetailLogic();
                        ApplicantJambDetail applicantJambDetail = jambDetailLogic.GetModelsBy(p => p.Person_Id == existingViewModel.Person.Id).LastOrDefault();
                        if (applicantJambDetail != null && applicantJambDetail.ApplicationForm == null)
                        {
                            applicantJambDetail.ApplicationForm = newApplicationForm;
                            jambDetailLogic.Modify(applicantJambDetail);
                        }

                        //Modify next of kin
                        NextOfKinLogic nextOfKinLogic = new NextOfKinLogic();
                        NextOfKin nextOfKin = nextOfKinLogic.GetModelsBy(p => p.Person_Id == existingViewModel.Person.Id).LastOrDefault();
                        if (nextOfKin != null && nextOfKin.ApplicationForm == null)
                        {
                            nextOfKin.ApplicationForm = newApplicationForm;
                            nextOfKinLogic.Modify(nextOfKin);
                        }

                        applicationFormLogic.SetRejectReason(newApplicationForm);

                        existingViewModel.ApplicationForm = newApplicationForm;

                        transaction.Complete();

                        applicationJsonData.ApplicationFormId = Utility.Encrypt(Convert.ToString(newApplicationForm.Id));
                        applicationJsonData.ApplicationFormNumber = newApplicationForm.Number;

                        //SendSms(existingViewModel.ApplicationForm, existingViewModel.Programme);

                    }
                }
                else
                {
                    ApplicationFormLogic formLogic = new ApplicationFormLogic();
                    newApplicationForm = formLogic.GetModelsBy(a => a.Application_Form_Number == existingViewModel.ApplicationForm.Number).LastOrDefault();
                    existingViewModel.ApplicationFormNumber = existingViewModel.ApplicationForm.Number;
                    applicationJsonData.ApplicationFormId = Utility.Encrypt(Convert.ToString(existingViewModel.ApplicationForm.Id));
                    applicationJsonData.ApplicationFormNumber = existingViewModel.ApplicationForm.Number;
                    using (var transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
                    {
                        //var sponsorLogic = new SponsorLogic();
                        //existingViewModel.Sponsor.Person = existingViewModel.Person;
                        //sponsorLogic.Modify(existingViewModel.Sponsor);

                        //MODIFY DRVEXP
                        //drivingExperience = drivingExperienceLogic.GetModelBy(d => d.Person_Id == existingViewModel.Person.Id);
                        //if (drivingExperience != null)
                        //{
                        //    drivingExperience.Application_Form_Id = existingViewModel.ApplicationForm.Id;
                        //    drivingExperienceLogic.Modify(drivingExperience);
                        //}
                        existingViewModel.SecondSittingOLevelResult.ApplicationForm = existingViewModel.ApplicationForm;
                        existingViewModel.SecondSittingOLevelResult.Person = existingViewModel.Person;
                        existingViewModel.SecondSittingOLevelResult.PersonType = new PersonType { Id = 4 };
                        existingViewModel.SecondSittingOLevelResult.Sitting = new OLevelExamSitting { Id = 2 };

                        existingViewModel.FirstSittingOLevelResult.ApplicationForm = existingViewModel.ApplicationForm;
                        existingViewModel.FirstSittingOLevelResult.Person = existingViewModel.Person;
                        existingViewModel.FirstSittingOLevelResult.PersonType = new PersonType { Id = 4 };
                        existingViewModel.FirstSittingOLevelResult.Sitting = new OLevelExamSitting { Id = 1 };

                        ModifyOlevelResult(existingViewModel.FirstSittingOLevelResult, existingViewModel.FirstSittingOLevelResultDetails);
                        ModifyOlevelResult(existingViewModel.SecondSittingOLevelResult, existingViewModel.SecondSittingOLevelResultDetails);

                        //Modify previous education

                        //PreviousEducation previousEducation = previousEducationLogic.GetModelsBy(p => p.Person_Id == existingViewModel.Person.Id).LastOrDefault();
                        if (previousEducation != null && previousEducation.ApplicationForm == null)
                        {
                            previousEducation.ApplicationForm = existingViewModel.ApplicationForm;
                            previousEducationLogic.Modify(previousEducation);
                        }

                        //Modify jamb detail
                        ApplicantJambDetailLogic jambDetailLogic = new ApplicantJambDetailLogic();
                        ApplicantJambDetail applicantJambDetail = jambDetailLogic.GetModelsBy(p => p.Person_Id == existingViewModel.Person.Id).LastOrDefault();
                        if (applicantJambDetail != null && applicantJambDetail.ApplicationForm == null)
                        {
                            applicantJambDetail.ApplicationForm = existingViewModel.ApplicationForm;
                            jambDetailLogic.Modify(applicantJambDetail);
                        }

                        //Modify next of kin
                        NextOfKinLogic nextOfKinLogic = new NextOfKinLogic();
                        NextOfKin nextOfKin = nextOfKinLogic.GetModelsBy(p => p.Person_Id == existingViewModel.Person.Id).LastOrDefault();
                        if (nextOfKin != null && nextOfKin.ApplicationForm == null)
                        {
                            nextOfKin.ApplicationForm = existingViewModel.ApplicationForm;
                            nextOfKinLogic.Modify(nextOfKin);
                        }

                        //set reject reason
                        if (existingViewModel.AppliedCourse.Programme.Id == 3 || existingViewModel.AppliedCourse.Programme.Id == 4 || existingViewModel.AppliedCourse.Programme.Id == 5)
                        {
                            newApplicationForm.Release = false;
                            existingViewModel.AppliedCourse.Person = existingViewModel.Person;

                            AdmissionCriteriaLogic admissionCriteriaLogic = new AdmissionCriteriaLogic();
                            string rejectReason = admissionCriteriaLogic.EvaluateApplication(existingViewModel.AppliedCourse, existingViewModel.PreviousEducation);
                            if (string.IsNullOrEmpty(rejectReason))
                            {
                                newApplicationForm.Rejected = false;
                                newApplicationForm.RejectReason = null;
                            }
                            else
                            {
                                newApplicationForm.Rejected = true;
                                newApplicationForm.RejectReason = rejectReason;

                                if (!applicationFormLogic.SetRejectReason(newApplicationForm))
                                {
                                    //throw new Exception("Rejected! " + rejectReason);
                                }
                            }

                            applicationFormLogic.Modify(newApplicationForm);

                            existingViewModel.ApplicationForm = newApplicationForm;
                        }
                        //else if (existingViewModel.AppliedCourse.Programme.Id == (int)Programmes.DrivingCertificate)
                        //{
                        //    DrivingExperienceLogic experienceLogic = new DrivingExperienceLogic();
                        //    DrivingExperience d_experience = experienceLogic.GetModelBy(d => d.)

                           
                        //}
                        else
                        {
                            newApplicationForm.Release = true;
                            newApplicationForm.Rejected = false;

                            AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
                            existingViewModel.AppliedCourse = appliedCourseLogic.GetBy(existingViewModel.Person);

                            AdmissionCriteriaLogic admissionCriteriaLogic = new AdmissionCriteriaLogic();
                            string rejectReason = admissionCriteriaLogic.EvaluateApplication(existingViewModel.AppliedCourse);
                            if (string.IsNullOrEmpty(rejectReason))
                            {
                                newApplicationForm.Rejected = false;
                                newApplicationForm.RejectReason = null;
                            }
                            else
                            {
                                newApplicationForm.Rejected = true;
                                newApplicationForm.RejectReason = rejectReason;

                                if (!applicationFormLogic.SetRejectReason(newApplicationForm))
                                {
                                    //throw new Exception("Rejected! " + rejectReason);
                                }
                            }

                            //bool checkAR = false;
                            //if (existingViewModel.Programme.Id == 2)
                            //{
                            //    OLevelResultDetailLogic oLevelResultLogic = new OLevelResultDetailLogic();
                            //    List<OLevelResultDetail> oLevelResults = oLevelResultLogic.GetModelsBy(o => o.APPLICANT_O_LEVEL_RESULT.Application_Form_Id == existingViewModel.AppliedCourse.ApplicationForm.Id);
                            //    for (int i = 0; i < oLevelResults.Count; i++)
                            //    {
                            //        if (oLevelResults[i].Grade.Id == 10)
                            //        {
                            //            checkAR = true;
                            //        }
                            //    } 
                            //}

                            //if (!checkAR)
                            //{
                            //    AdmissionCriteriaLogic admissionCriteriaLogic = new AdmissionCriteriaLogic();
                            //    string rejectReason = admissionCriteriaLogic.EvaluateApplication(existingViewModel.AppliedCourse);
                            //    if (string.IsNullOrEmpty(rejectReason))
                            //    {
                            //        newApplicationForm.Rejected = false;
                            //        newApplicationForm.RejectReason = null;
                            //    }
                            //    else
                            //    {
                            //        newApplicationForm.Rejected = true;
                            //        newApplicationForm.RejectReason = rejectReason;

                            //        if (!applicationFormLogic.SetRejectReason(newApplicationForm))
                            //        {
                            //            throw new Exception("Rejected! " + rejectReason);
                            //        }
                            //    } 
                            //}

                            applicationFormLogic.Modify(newApplicationForm);

                            existingViewModel.ApplicationForm = newApplicationForm;
                        }

                        transaction.Complete();
                    }
                }
                
                applicationJsonData.IsError = false;
                applicationJsonData.ApplicationFormId = globalFormId;
            }
            catch (Exception ex)
            {
                applicationJsonData.IsError = true;
                applicationJsonData.Message = "Error! " + ex.Message;
            }

            return Json(applicationJsonData, JsonRequestBehavior.AllowGet);
        }
        private void PopulateModelsFromJsonData(PostJambViewModel viewModel, ApplicationFormJsonModel applicationJsonData, bool applicationExist)
        {
            viewModel.Person = new Person();
            viewModel.FirstSittingOLevelResult = new OLevelResult();
            viewModel.SecondSittingOLevelResult = new OLevelResult();
            viewModel.Sponsor = new Sponsor();
            viewModel.AppliedCourse = new AppliedCourse();
            viewModel.ApplicationForm = new ApplicationForm();
            viewModel.Session = new Session();
            viewModel.ApplicationFormSetting = new ApplicationFormSetting();
            viewModel.ApplicationProgrammeFee = new ApplicationProgrammeFee();
            viewModel.PreviousEducation = new PreviousEducation();
            
            try
            {
                ApplicationFormLogic applicationFormLogic = new ApplicationFormLogic();
                ApplicationFormSettingLogic applicationFormSettingLogic = new ApplicationFormSettingLogic();

                if (applicationExist)
                {
                    viewModel.FirstSittingOLevelResult.Id = Convert.ToInt32(applicationJsonData.FirstSittingOLevelResultId);
                    viewModel.SecondSittingOLevelResult.Id = !string.IsNullOrEmpty(applicationJsonData.SecondSittingOLevelResultId) ? Convert.ToInt32(applicationJsonData.SecondSittingOLevelResultId) : 0;
                    viewModel.ApplicationForm.Id = Convert.ToInt64(applicationJsonData.ApplicationFormId);
                    viewModel.ApplicationForm.Number = applicationJsonData.ApplicationFormNumber;

                    applicationJsonData.ApplicationFormNumber = applicationFormLogic.GetEntityBy(a => a.Application_Form_Id == viewModel.ApplicationForm.Id).Application_Form_Number;
                }

                viewModel.Session.Id = Convert.ToInt32(applicationJsonData.SessionId);
                viewModel.ApplicationFormSetting.Id = Convert.ToInt32(applicationJsonData.ApplicationFormSettingId);
                viewModel.ApplicationFormSetting = applicationFormSettingLogic.GetModelBy(afs => afs.Application_Form_Setting_Id == viewModel.ApplicationFormSetting.Id);

                viewModel.ApplicationProgrammeFee.Id = Convert.ToInt32(applicationJsonData.ProgrammeFeeId);
                viewModel.Person.Id = Convert.ToInt64(applicationJsonData.PersonId);
                if (!string.IsNullOrEmpty(applicationJsonData.PaymentId))
                {
                    viewModel.Payment = new Payment() { Id = Convert.ToInt64(applicationJsonData.PaymentId) };
                }

                viewModel.FirstSittingOLevelResult.Type = new OLevelType() { Id = Convert.ToInt32(applicationJsonData.FirstSittingOLevelResultTypeId) };
                viewModel.FirstSittingOLevelResult.ExamNumber = applicationJsonData.FirstSittingOLevelResultExamNumber;
                viewModel.FirstSittingOLevelResult.ExamYear = Convert.ToInt32(applicationJsonData.FirstSittingOLevelResultExamYear);
                viewModel.FirstSittingOLevelResult.ScratchCardPin = applicationJsonData.FirstSittingOLevelResultPin;

                viewModel.SecondSittingOLevelResult.Type = new OLevelType() { Id = Convert.ToInt32(applicationJsonData.SecondSittingOLevelResultTypeId) };
                viewModel.SecondSittingOLevelResult.ExamNumber = applicationJsonData.SecondSittingOLevelResultExamNumber;
                viewModel.SecondSittingOLevelResult.ExamYear = Convert.ToInt32(applicationJsonData.SecondSittingOLevelResultExamYear);
                viewModel.SecondSittingOLevelResult.ScratchCardPin = applicationJsonData.SecondSittingOLevelResultPin;

                viewModel.AppliedCourse.Programme = new Programme() { Id = Convert.ToInt32(applicationJsonData.ProgrammeId) };
                viewModel.AppliedCourse.Department = new Department() { Id = Convert.ToInt32(applicationJsonData.DepartmentId) };

                applicationJsonData.OtherAbility = !string.IsNullOrEmpty(applicationJsonData.OtherAbility) ? applicationJsonData.OtherAbility : null;
                viewModel.Applicant = new Model.Model.Applicant()
                {
                    Ability = new Ability() { Id = Convert.ToInt32(applicationJsonData.AbilityId) },
                    OtherAbility = applicationJsonData.OtherAbility,
                    ExtraCurricullarActivities = applicationJsonData.ExtraCurricullarActivities
                };

            }
            catch (Exception)
            {
                throw;
            }
        }
        public void PopulateOLevelResultDetailFromJsonData(PostJambViewModel viewModel, List<OLevelResultDetailJsonModel> oLevelResultDetailJsonModels, int type)
        {
            List<OLevelResultDetail> oLevelResultDetails = new List<OLevelResultDetail>();
            try
            {
                OLevelGradeLogic oLevelGradeLogic = new OLevelGradeLogic();
                OLevelSubjectLogic oLevelSubjectLogic = new OLevelSubjectLogic();

                List<OLevelResultDetailJsonModel> myOLevelModels = oLevelResultDetailJsonModels.Where(o => o.GradeId != "0" && o.SubjectId != "0").ToList();

                if (myOLevelModels.Count > 0)
                {
                    for (int i = 0; i < myOLevelModels.Count; i++)
                    {
                        OLevelResultDetail oLevelResultDetail = new OLevelResultDetail();
                        int oLevelSubjectId = Convert.ToInt32(myOLevelModels[i].SubjectId);
                        O_LEVEL_SUBJECT oLevelSubject = oLevelSubjectLogic.GetEntityBy(o => o.O_Level_Subject_Id == oLevelSubjectId);
                        int oLevelGradeId = Convert.ToInt32(myOLevelModels[i].GradeId);
                        O_LEVEL_GRADE oLevelGrade = oLevelGradeLogic.GetEntityBy(o => o.O_Level_Grade_Id == oLevelGradeId);

                        oLevelResultDetail.Grade = new OLevelGrade() { Id = oLevelGradeId, Name = oLevelGrade.O_Level_Grade_Name };
                        oLevelResultDetail.Subject = new OLevelSubject() { Id = oLevelSubjectId, Name = oLevelSubject.O_Level_Subject_Name };

                        oLevelResultDetails.Add(oLevelResultDetail);

                        myOLevelModels[i].SubjectName = oLevelSubject.O_Level_Subject_Name;
                        myOLevelModels[i].GradeName = oLevelGrade.O_Level_Grade_Name;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            if (type == 1)
            {
                viewModel.FirstSittingOLevelResultDetails = oLevelResultDetails;
            }
            else
            {
                viewModel.SecondSittingOLevelResultDetails = oLevelResultDetails;
            }
        }

        public JsonResult UploadImage(FormCollection formData)
        {
            ApplicationFormJsonModel jsonModel = new ApplicationFormJsonModel();
            try
            {
                HttpPostedFileBase file = Request.Files["image"];

                string personId = formData["personId"].ToString();

                if (file != null && file.ContentLength != 0)
                {
                    FileInfo fileInfo = new FileInfo(file.FileName);
                    string fileExtension = fileInfo.Extension;
                    string newFile = personId + "__";
                    string newFileName = newFile + DateTime.Now.ToString().Replace("/", "").Replace(":", "").Replace(" ", "") + fileExtension;

                    decimal sizeAllowed = 50; //50kb
                    string invalidFileMessage = InvalidFile(file.ContentLength, fileExtension, sizeAllowed);
                    if (!string.IsNullOrEmpty(invalidFileMessage))
                    {
                        jsonModel.IsError = true;
                        jsonModel.Message = invalidFileMessage;
                        return Json(jsonModel, JsonRequestBehavior.AllowGet);
                    }

                    string pathForSaving = Server.MapPath("~/Content/Student");
                    if (CreateFolderIfNeeded(pathForSaving))
                    {
                        DeleteFileIfExist(pathForSaving, personId);

                        file.SaveAs(Path.Combine(pathForSaving, newFileName));

                        PersonLogic personLogic = new PersonLogic();
                        long myPersonId = Convert.ToInt64(personId);
                        Person person = personLogic.GetModelBy(p => p.Person_Id == myPersonId);

                        person.ImageFileUrl = "/Content/Student/" + newFileName;

                        personLogic.Modify(person);

                        jsonModel.IsError = false;
                        jsonModel.Message = "File uploaded successfully!";

                        string path = Path.Combine(pathForSaving, newFileName);
                        if (path != null)
                        {
                            jsonModel.ImageFileUrl = "/Content/Student/" + newFileName;
                            //imageUrlDisplay = appRoot + imageUrl + "?t=" + DateTime.Now;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                jsonModel.IsError = true;
                jsonModel.Message = "Error! " + ex.Message;
            }

            return Json(jsonModel, JsonRequestBehavior.AllowGet);
        }
        public JsonResult RemoveImage(FormCollection formData)
        {
            ApplicationFormJsonModel jsonModel = new ApplicationFormJsonModel();
            try
            {
                HttpPostedFileBase file = Request.Files["image"];

                string personId = formData["personId"].ToString();

                if (file != null && file.ContentLength != 0)
                {
                    jsonModel.IsError = false;
                    jsonModel.Message = "File removed successfully!";
                }
            }
            catch (Exception ex)
            {
                jsonModel.IsError = true;
                jsonModel.Message = "Error! " + ex.Message;
            }

            return Json(jsonModel, JsonRequestBehavior.AllowGet);
        }

        public JsonResult PreLoadOLevel(string firstSitting, string secondSitting)
        {
            var existingViewModel = new PostJambViewModel();

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            ApplicationFormJsonModel applicationJsonData = new ApplicationFormJsonModel();
            List<OLevelResultDetailJsonModel> firstSittingOLevelJsonData = serializer.Deserialize<List<OLevelResultDetailJsonModel>>(firstSitting);
            List<OLevelResultDetailJsonModel> secondSittingOLevelJsonData = serializer.Deserialize<List<OLevelResultDetailJsonModel>>(secondSitting);
            try
            {
                PopulateOLevelResultDetailFromJsonData(existingViewModel, firstSittingOLevelJsonData, 1);
                PopulateOLevelResultDetailFromJsonData(existingViewModel, secondSittingOLevelJsonData, 2);

                applicationJsonData.FirstSittingOLevelJsonData = firstSittingOLevelJsonData;
                applicationJsonData.SecondSittingOLevelJsonData = secondSittingOLevelJsonData;

                applicationJsonData.IsError = false;
            }
            catch (Exception ex)
            {
                applicationJsonData.IsError = true;
                applicationJsonData.Message = "Error! " + ex.Message;
            }

            return Json(applicationJsonData, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveAcademicDetails(int optionSecondChoice, long personId)
        {
            ApplicationFormJsonModel jsonData = new ApplicationFormJsonModel();
            try
            {
                if (optionSecondChoice > 0 && personId > 0)
                {
                    jsonData.IsError = false;
                    jsonData.Message = "Operation Successful! ";
                }
                else
                {
                    jsonData.IsError = true;
                    jsonData.Message = "Validation for one of the fields failed, kindly check and try again! ";
                }
            }
            catch (Exception ex)
            {
                jsonData.IsError = true;
                jsonData.Message = "An error occured! " + ex.Message;
            }

            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ClearSecondSitting(long personId)
        {
            try
            {
                OLevelResultDetailAuditLogic resultDetailAuditLogic = new OLevelResultDetailAuditLogic();
                OLevelResultAuditLogic resultAuditLogic = new OLevelResultAuditLogic();
                OLevelResultDetailLogic resultDetailLogic = new OLevelResultDetailLogic();
                OLevelResultLogic resultLogic = new OLevelResultLogic();

                if (personId > 0)
                {
                    if (resultLogic.GetModelsBy(s => s.Person_Id == personId && s.O_Level_Exam_Sitting_Id == 2).LastOrDefault() == null)
                        return Json("No second sitting result found.", JsonRequestBehavior.AllowGet);

                    using (TransactionScope scope = new TransactionScope())
                    {
                        resultDetailAuditLogic.Delete(r => r.APPLICANT_O_LEVEL_RESULT.Person_Id == personId && r.APPLICANT_O_LEVEL_RESULT.O_Level_Exam_Sitting_Id == 2);
                        resultAuditLogic.Delete(r => r.APPLICANT_O_LEVEL_RESULT.Person_Id == personId && r.APPLICANT_O_LEVEL_RESULT.O_Level_Exam_Sitting_Id == 2);

                        resultDetailLogic.Delete(r => r.APPLICANT_O_LEVEL_RESULT.Person_Id == personId && r.APPLICANT_O_LEVEL_RESULT.O_Level_Exam_Sitting_Id == 2);
                        resultLogic.Delete(r => r.Person_Id == personId && r.O_Level_Exam_Sitting_Id == 2);

                        scope.Complete();
                    }

                    return Json("Second sitting.", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Parameter not set.", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json("Error! " + ex.Message, JsonRequestBehavior.AllowGet); ;
            }
        }
        public  void  SendSmartSms(ApplicationForm applicationForm, Programme programme)
        {
            try
            {
                if(applicationForm!=null && programme != null)
                {
                    string message = "";
                    string number = "234" + applicationForm.Person.MobilePhone;
                    if (applicationForm.Rejected == false)
                    {
                        message = "Hello " + applicationForm.Person.LastName + ", Your application for Admission into the " + programme.ShortName + " programme has been received. Your application no is " + applicationForm.Number + " Thanks";
                    }
                    else
                    {
                        message = "Hello " + applicationForm.Person.LastName + ", Your application for Admission into the " + programme.ShortName + " programme has been rejected. You failed to meet the entry criteria. Thanks";
                    }

                    SmartSMS smartSMS = new SmartSMS();
                    smartSMS.sender = "POLYILARO";
                    smartSMS.to = number;
                    smartSMS.message = message;
                    smartSMS.type = 0;
                    smartSMS.routing =3;
                    SmartSmsLogic smartSmsLogic = new SmartSmsLogic();
                    smartSmsLogic.SendSingleSmsMessage(smartSMS);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void SetPreviousEducationImageUrl(PreviousEducation previousEducation)
        {
            TempData["ItLetterimageUrl"] = previousEducation.ITLetterOfCompletion;
            if (!String.IsNullOrEmpty(previousEducation.ResultCopyUrl))
            {
                TempData["CertificateimageUrl"] = previousEducation.ResultCopyUrl;
            }
            else
            {
               
                if (!String.IsNullOrEmpty(previousEducation.CertificateCopyUrl))
                {
                    TempData["CertificateimageUrl"] = previousEducation.CertificateCopyUrl;
                }
            }
        }
        public bool CheckIfFileExistsInServer(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                string filePathNDResult = System.Web.HttpContext.Current.Server.MapPath(path);
                if (System.IO.File.Exists(filePathNDResult))
                {
                    return true;
                }
            }
            return false;
        }
        public ActionResult DrivingExperienceForm()
        {
            PostJambViewModel existingViewModel = (PostJambViewModel)TempData["viewModel"];
            TempData.Keep("viewModel");
            PostJAMBFormPaymentViewModel postJAMBFormPaymentViewModel = (PostJAMBFormPaymentViewModel)TempData["PostJAMBFormPaymentViewModel"];
            TempData.Keep("PostJAMBFormPaymentViewModel");

            try
            {
                existingViewModel = null;
                PopulateAllDropDowns();

                if (postJAMBFormPaymentViewModel != null)
                {
                    if (postJAMBFormPaymentViewModel != null && postJAMBFormPaymentViewModel.Programme != null)
                    {
                        viewModel.Session = postJAMBFormPaymentViewModel.CurrentSession;
                        viewModel.Programme = postJAMBFormPaymentViewModel.Programme;

                        if (viewModel.Programme.Id == 1)
                        {
                            viewModel.PreviousEducation = null;
                            viewModel.ApplicantJambDetail.JambRegistrationNumber = postJAMBFormPaymentViewModel.JambRegistrationNumber;
                            viewModel.ApplicantJambDetail = postJAMBFormPaymentViewModel.ApplicantJambDetail;
                        }
                        else if (viewModel.Programme.Id == 2)
                        {
                            viewModel.PreviousEducation = null;
                            viewModel.ApplicantJambDetail = null;
                        }
                        else if (viewModel.Programme.Id == 3 || viewModel.Programme.Id == 4 || viewModel.Programme.Id == 5)
                        {
                            viewModel.ApplicantJambDetail = null;
                        }

                        viewModel.Payment = postJAMBFormPaymentViewModel.Payment;
                        viewModel.AppliedCourse = postJAMBFormPaymentViewModel.AppliedCourse;
                        viewModel.ApplicationFormSetting = postJAMBFormPaymentViewModel.ApplicationFormSetting;
                        viewModel.ApplicationProgrammeFee = postJAMBFormPaymentViewModel.ApplicationProgrammeFee;
                        viewModel.remitaPyament = postJAMBFormPaymentViewModel.remitaPayment;
                        viewModel.Person = postJAMBFormPaymentViewModel.Person;
                        viewModel.Person.ImageFileUrl = !string.IsNullOrEmpty(viewModel.Person.ImageFileUrl) ? viewModel.Person.ImageFileUrl : Utility.DEFAULT_AVATAR;

                        SetLgaIfExist(viewModel);
                    }
                }

                ApplicationForm applicationform = viewModel.GetApplicationFormBy(viewModel.Person, viewModel.Payment);
                if ((applicationform != null && applicationform.Id > 0) && viewModel.ApplicationAlreadyExist == false)
                {
                    viewModel.ApplicationAlreadyExist = true;
                    viewModel.ApplicationForm = applicationform;

                    viewModel.LoadApplicantionFormBy(viewModel.Person, viewModel.Payment);
                    SetSelectedSittingSubjectAndGrade(viewModel);

                    SetLgaIfExist(viewModel);

                    if (viewModel.AppliedCourse.Programme.Id == 3 || viewModel.AppliedCourse.Programme.Id == 4 || viewModel.AppliedCourse.Programme.Id == 5)
                    {
                        SetPreviousEducationEndDate();
                        SetPreviousEducationStartDate();
                    }
                }


               if(applicationform != null)
                {
                    DrivingExperienceLogic exp_logic = new DrivingExperienceLogic();
                    DrivingExperience d_experience = exp_logic.GetModelBy(d => d.Application_Form_Id == applicationform.Id);
                    if (d_experience != null)
                    {
                        viewModel.DrivingExperience = d_experience;
                    }
                }


                NextOfKinLogic nextOfKinLogic = new NextOfKinLogic();
                NextOfKin nextOfKin = nextOfKinLogic.GetModelsBy(s => s.Person_Id == viewModel.Person.Id).LastOrDefault();
                if (nextOfKin != null)
                {
                    viewModel.Sponsor = new Sponsor();
                    viewModel.Sponsor.Person = viewModel.Person;
                    viewModel.Sponsor.ContactAddress = nextOfKin.ContactAddress;
                    viewModel.Sponsor.Name = nextOfKin.Name;
                    viewModel.Sponsor.MobilePhone = nextOfKin.MobilePhone;
                    viewModel.Sponsor.Relationship = nextOfKin.Relationship;
                }
                ApplicantLogic applicantLogic = new ApplicantLogic();
                viewModel.Applicant = applicantLogic.GetModelsBy(a => a.Person_Id == viewModel.Person.Id).LastOrDefault();

                PreviousEducationLogic previousEducationLogic = new PreviousEducationLogic();
                viewModel.PreviousEducation = previousEducationLogic.GetModelsBy(s => s.Person_Id == viewModel.Person.Id).LastOrDefault();
                if (viewModel.PreviousEducation != null)
                {
                    SetPreviousEducationEndDate();
                    SetPreviousEducationStartDate();
                    
                }

                if (viewModel.AppliedCourse != null && viewModel.AppliedCourse.Option != null)
                {
                    DepartmentOptionLogic optionLogic = new DepartmentOptionLogic();
                    ViewBag.OptionList = new SelectList(optionLogic.GetBy(viewModel.AppliedCourse.Department, viewModel.AppliedCourse.Programme), "Id", "Name");
                }

                if (viewModel.PreviousEducation != null)
                {
                    ViewBag.PreviousEducationEndMonthSelectList = new SelectList(Utility.GetMonthsInYear(), "Id", "Name", viewModel.PreviousEducation.EndDate.Month);
                    ViewBag.QualificationId = new SelectList(Utility.PopulateEducationalQualificationSelectListItem(), "Value", "Text");
                    ViewBag.ResultGradeId = new SelectList(Utility.PopulateResultGradeSelectListItem(), "Value", "Text");
                    //ViewBag.QualificationId = new SelectList(Utility.PopulateEducationalQualificationSelectListItem(), "Id", "Name", viewModel.PreviousEducation.Qualification.Id);
                    //ViewBag.ResultGradeId = new SelectList(Utility.PopulateResultGradeSelectListItem(), "Id", "Name", viewModel.PreviousEducation.ResultGrade.Id);
                    ViewBag.PreviousEducationEndYearSelectList = new SelectList(Utility.PopulateYearSelectListItem(1970, true), "Value", "Text", viewModel.PreviousEducation.EndDate.Year);

                }
                else
                {
                    ViewBag.PreviousEducationEndMonthSelectList = new SelectList(Utility.GetMonthsInYear(), "Id", "Name");
                    ViewBag.PreviousEducationEndYearSelectList = new SelectList(Utility.PopulateYearSelectListItem(1970, true), "Value", "Text");
                    ViewBag.PreviousEducationITEndMonthSelectList = new SelectList(Utility.GetMonthsInYear(), "Id", "Name");
                    ViewBag.PreviousEducationITEndYearSelectList = new SelectList(Utility.PopulateYearSelectListItem(1970, true), "Value", "Text");
                    ViewBag.PreviousEducationITStartMonthSelectList = new SelectList(Utility.GetMonthsInYear(), "Id", "Name");
                    ViewBag.PreviousEducationITStartYearSelectList = new SelectList(Utility.PopulateYearSelectListItem(1970, true), "Value", "Text");
                    ViewBag.QualificationId = new SelectList(Utility.PopulateEducationalQualificationSelectListItem(), "Value", "Text");
                    ViewBag.ResultGradeId = new SelectList(Utility.PopulateResultGradeSelectListItem(), "Value", "Text");
                }

                if (applicationform != null)
                {
                    viewModel.ApplicationForm = applicationform;
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            TempData["viewModel"] = viewModel;
            TempData.Keep("viewModel");
            //TempData["imageUrl"] = viewModel.Person.ImageFileUrl;
            //TempData.Keep("imageUrl");
            TempData["PostJAMBFormPaymentViewModel"] = postJAMBFormPaymentViewModel;            
            TempData.Keep("PostJAMBFormPaymentViewModel");

            viewModel.Person.ImageFileUrl = !string.IsNullOrEmpty(viewModel.Person.ImageFileUrl) ? viewModel.Person.ImageFileUrl : Utility.DEFAULT_AVATAR;

            return View(viewModel);
        }

        public JsonResult SaveDrivingExperience(long personId, string li_num, string iss_date, string exp_date, int f_mark, decimal height, int yoe, int li_type, long appFormId)
        {
            PostJAMBFormPaymentViewModel postJAMBFormPaymentViewModel = (PostJAMBFormPaymentViewModel)TempData["PostJAMBFormPaymentViewModel"];
            TempData.Keep("PostJAMBFormPaymentViewModel");
            DrivingExperience drivingExperience = new DrivingExperience();
            DrivingExperienceLogic drivingExperienceLogic = new DrivingExperienceLogic();
            LicenseTypeLogic licenseTypeLogic = new LicenseTypeLogic();
            ApplicationFormLogic formLogic = new ApplicationFormLogic();
            ApplicationFormJsonModel jsonData = new ApplicationFormJsonModel();
            PersonLogic personLogic = new PersonLogic();
            decimal reqExp = 3;
           
            try
            {
                
                    if (postJAMBFormPaymentViewModel.AppliedCourse.Option != null && (postJAMBFormPaymentViewModel.AppliedCourse.Option.Id == 17 || postJAMBFormPaymentViewModel.AppliedCourse.Option.Id == 18))
                    {
                        if (yoe < reqExp)
                        {
                            jsonData.IsError = true;
                            jsonData.Message = "This application is intended for applicants having not less than three(3) years of driving experience!";
                            return Json(jsonData, JsonRequestBehavior.AllowGet);
                        }
                    }


                if (personId > 0 && li_type > 0 && li_num != null)
                {
                    var doesExist = drivingExperienceLogic.GetModelBy(d => d.Person_Id == personId);
                    if(doesExist != null)
                    {
                        drivingExperience.FacialMarks = f_mark == 1 ? true : false;
                        drivingExperience.Height = height;
                        drivingExperience.YearsOfExperience = yoe;
                        drivingExperience.Licence_Type_Id = li_type;
                        drivingExperience.IssuedDate = Convert.ToDateTime(iss_date);
                        drivingExperience.ExpiryDate = Convert.ToDateTime(exp_date);
                        drivingExperience.LicenseNumber = li_num;
                        drivingExperience.PersonId = doesExist.PersonId;
                        if(appFormId>0)
                        drivingExperience.Application_Form_Id = appFormId;
                        drivingExperienceLogic.Modify(drivingExperience);

                        jsonData.IsError = false;
                        jsonData.Message = "Driving Experience Saved!";
                        return Json(jsonData, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        drivingExperience.PersonId = personId;
                        drivingExperience.FacialMarks = f_mark == 1 ? true : false;
                        drivingExperience.Height = height;
                        drivingExperience.YearsOfExperience = yoe;
                        drivingExperience.Licence_Type_Id = li_type;
                        drivingExperience.IssuedDate = Convert.ToDateTime(iss_date);
                        drivingExperience.ExpiryDate = Convert.ToDateTime(exp_date);
                        drivingExperience.LicenseNumber = li_num;
                        if (appFormId > 0)
                            drivingExperience.Application_Form_Id = appFormId;
                        drivingExperienceLogic.Create(drivingExperience);

                        jsonData.IsError = false;
                        jsonData.Message = "Driving Experience Saved!";
                        return Json(jsonData, JsonRequestBehavior.AllowGet);
                    }
                    
                }
                

            }
            catch (Exception ex)
            {
                return Json(ex, JsonRequestBehavior.AllowGet);
            }
                return Json(jsonData, JsonRequestBehavior.AllowGet);

        }
        public ActionResult ReprintAcknowledgeSlip()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ReprintAcknowledgeSlip(string ApplicationNo)
        {
            viewModel = new PostJambViewModel();
            try
            {
                ApplicationFormLogic applicationFormLogic = new ApplicationFormLogic();
                if (string.IsNullOrEmpty(ApplicationNo))
                {
                    SetMessage("Please enter Application No. to continue", Message.Category.Information);
                    return View();
                }
                if (!string.IsNullOrEmpty(ApplicationNo))
                {
                    var form=applicationFormLogic.GetModelsBy(f => f.Application_Form_Number == ApplicationNo.Trim()).FirstOrDefault();
                    if (form==null ||form?.Id==0)
                    {
                        SetMessage("Application does not exist", Message.Category.Information);
                        return View();
                    }
                    if (form?.Id > 0)
                    {
                        var applicationFormSettingLogic = new ApplicationFormSettingLogic();
                        var appliedCourseLogic = new AppliedCourseLogic();
                        //var oLevelResultLogic = new OLevelResultLogic();
                        //var oLevelResultDetailLogic = new OLevelResultDetailLogic();
                        var applicantJambDetailLogic = new ApplicantJambDetailLogic();
                        var personLogic = new PersonLogic();
                        PreviousEducationLogic previousEducationLogic = new PreviousEducationLogic();
                        EducationalQualificationLogic educationQualificationLogic = new EducationalQualificationLogic();
                        RankingDataLogic rankingDataLogic = new RankingDataLogic();
                        ApplicantApplicationApprovalLogic applicantApplicationApprovalLogic = new ApplicantApplicationApprovalLogic();
                        viewModel.ApplicantApplicationApproval = new ApplicantApplicationApproval();
                        viewModel.ApplicantApplicationApproval = applicantApplicationApprovalLogic.GetModelsBy(f => f.Application_Form_Id == form.Id).FirstOrDefault();
                        if (viewModel.ApplicantApplicationApproval == null)
                        {
                            SetMessage("Your application has not been cleared check back later", Message.Category.Information);
                            return View();
                        }
                        viewModel.ApplicationForm = applicationFormLogic.GetBy(form.Id);
                        if (viewModel.ApplicationForm != null && viewModel.ApplicationForm.Id > 0)
                        {
                            viewModel.ApplicationFormNumber = viewModel.ApplicationForm.Number;
                            viewModel.ApplicationFormSetting = applicationFormSettingLogic.GetModelBy(a => a.Application_Form_Setting_Id == viewModel.ApplicationForm.Setting.Id);
                            viewModel.Session = viewModel.ApplicationFormSetting.Session;
                            viewModel.Person = personLogic.GetModelBy(a => a.Person_Id == viewModel.ApplicationForm.Person.Id);
                            viewModel.AppliedCourse = appliedCourseLogic.GetBy(viewModel.Person);
                            viewModel.Programme = viewModel.AppliedCourse.Programme;
                            viewModel.ApplicantJambDetail = applicantJambDetailLogic.GetModelsBy(s => s.Person_Id == viewModel.Person.Id && s.Application_Form_Id == viewModel.ApplicationForm.Id).LastOrDefault();
                            
                            
                            if (viewModel.ApplicantJambDetail != null)
                            {
                                OLevelSubject emptySubject = new OLevelSubject() { Id = 0, Name = "" };
                                if (viewModel.ApplicantJambDetail.Subject1 == null)
                                {
                                    viewModel.ApplicantJambDetail.Subject1 = emptySubject;
                                }
                                if (viewModel.ApplicantJambDetail.Subject2 == null)
                                {
                                    viewModel.ApplicantJambDetail.Subject2 = emptySubject;
                                }
                                if (viewModel.ApplicantJambDetail.Subject3 == null)
                                {
                                    viewModel.ApplicantJambDetail.Subject3 = emptySubject;
                                }
                                if (viewModel.ApplicantJambDetail.Subject4 == null)
                                {
                                    viewModel.ApplicantJambDetail.Subject4 = emptySubject;
                                }
                            }

                            if (viewModel.PreviousEducation != null && viewModel.PreviousEducation.Id > 0)
                            {
                                viewModel.PreviousEducation = previousEducationLogic.GetModelBy(pe => pe.Application_Form_Id == viewModel.ApplicationForm.Id);
                                EducationalQualification qualification = educationQualificationLogic.GetModelBy(q => q.Educational_Qualification_Id == viewModel.PreviousEducation.Qualification.Id);
                                viewModel.PreviousEducation.Qualification = qualification;

                            }
                        }

                    }
                }
                

                
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            TempData["viewModel"] = viewModel;

            return View("ApplicationClearanceSlip", viewModel);
        }
        public ActionResult ApplicationClearanceSlip(PostJambViewModel viewModel)
        {
            PostJambViewModel existingViewModel = (PostJambViewModel)TempData["viewModel"];
            if (existingViewModel == null || existingViewModel.ApplicationForm == null || existingViewModel.ApplicationForm.Setting == null)
            {
                SetMessage("Please Enter your Application No. to continue",
                    Message.Category.Information);
                return RedirectToAction("ReprintAcknowledgeSlip");
            }
            if (existingViewModel != null && existingViewModel.ApplicationForm != null)
            {
                existingViewModel.Session = existingViewModel.ApplicationForm.Setting.Session;
            }
            TempData["viewModel"] = existingViewModel;
            return View(existingViewModel);
        }

        public JsonResult FetchJambRecord(string JambRegNumber)
        {
            try
            {
                ApplicationFormJsonModel jsonData = new ApplicationFormJsonModel();
                JambRecord jambRecord = new JambRecord();
                JambRecordLogic jambRecordLogic = new JambRecordLogic();
                jambRecord = jambRecordLogic.GetModelBy(x => x.Jamb_Registration_Number == JambRegNumber.ToString());
                if (jambRecord != null)
                {
                    string[] nameSplit = jambRecord.CandidateName.Split(' ');
                    var resolveFullName = nameSplit.ToList();

                    if (resolveFullName.Count > 2)
                    {
                        jsonData.OtherName = nameSplit[2];
                    }
                    else
                    {
                        jsonData.OtherName = " ";
                    }
                    jsonData.LastName = nameSplit[0];
                    jsonData.FirstName = nameSplit[1];
                    jsonData.DepartmentId = jambRecord.Course != null ? jambRecord.Course.Id.ToString() : null;
                    jsonData.SessionId = jambRecord.Session != null ? jambRecord.Session.Id.ToString() : null;
                    jsonData.StateId = jambRecord.State != null ? jambRecord.State.Id : null;
                    jsonData.SexId = jambRecord.Sex != null ? jambRecord.Sex.Id.ToString() : null;
                    jsonData.DataExist = true;
                    return Json(jsonData, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    jsonData.DataExist = false;
                    return Json(jsonData, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                return Json("Error! " + ex.Message, JsonRequestBehavior.AllowGet); ;
            }
        }

    }
}