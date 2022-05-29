using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Abundance_Nk.Web.Areas.Admin.ViewModels;
using Abundance_Nk.Web.Controllers;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Business;
using Abundance_Nk.Web.Areas.Applicant.ViewModels;
using System.IO;
using System.Drawing;
using Abundance_Nk.Web.Models;
using BarcodeLib;
using Abundance_Nk.Model.Entity.Model;
using Abundance_Nk.Web.Areas.Common.Models;

namespace Abundance_Nk.Web.Areas.Common.Controllers
{
    [AllowAnonymous]
    public class CredentialController : BaseController
    {
        public ActionResult ApplicationForm(long fid)
        {
            try
            {
                ApplicationFormViewModel applicationFormViewModel = new ApplicationFormViewModel();
                ApplicationForm form = new ApplicationForm() { Id = fid };

                applicationFormViewModel.GetApplicationFormBy(form);
                if (applicationFormViewModel.Person != null && applicationFormViewModel.Person.Id > 0)
                {
                    applicationFormViewModel.SetApplicantAppliedCourse(applicationFormViewModel.Person);
                }

                return View(applicationFormViewModel);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ActionResult StudentForm(string fid)
        {
            try
            {
                Int64 formId = Convert.ToInt64(Abundance_Nk.Web.Models.Utility.Decrypt(fid));

                ApplicationForm form = new ApplicationForm() { Id = formId };
                StudentFormViewModel studentFormViewModel = new StudentFormViewModel();

                studentFormViewModel.LoadApplicantionFormBy(formId);
                if (studentFormViewModel.ApplicationForm.Person != null && studentFormViewModel.ApplicationForm.Person.Id > 0)
                {
                    studentFormViewModel.LoadStudentInformationFormBy(studentFormViewModel.ApplicationForm.Person.Id);
                }

                return View(studentFormViewModel);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ActionResult PostUtmeResult(string jn)
        {
            PostUtmeResult result = null;

            try
            {
                if (!string.IsNullOrWhiteSpace(jn))
                {
                    PostUtmeResultLogic postUtmeResultLogic = new PostUtmeResultLogic();
                    result = postUtmeResultLogic.GetModelBy(m => m.REGNO == jn);
                    if (result == null || result.Id <= 0)
                    {
                        //SetMessage("Registration Number / Jamb No was not found! Please check that you have typed in the correct detail", Message.Category.Error);
                        return View(result);
                    }
                    else
                    {
                        result.Fullname = result.Fullname;
                        result.Regno = result.Regno;
                        result.Eng = result.Eng;
                        result.Sub2 = result.Sub2;
                        result.Sub3 = result.Sub3;
                        result.Sub4 = result.Sub4;
                        result.Scr2 = result.Scr2;
                        result.Scr3 = result.Scr3;
                        result.Scr4 = result.Scr4;
                        result.Total = result.Total;
                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage("Operation failed! " + ex.Message, Message.Category.Error);
            }

            return View(result);
        }

        public ActionResult Receipt(long pmid)
        {
            Receipt receipt = null;

            try
            {
                receipt = GetReceiptBy(pmid);
                if (receipt == null)
                {
                    SetMessage("No receipt found!", Message.Category.Error);
                }
                else
                {
                    //PaymentVerificationLogic verificationLogic = new PaymentVerificationLogic();
                    //receipt.PaymentVerification = verificationLogic.GetBy(pmid);
                    receipt.barcodeImageUrl = "http://applications.federalpolyilaro.edu.ng/Common/Credential/Receipt?pmid=" + pmid;
                    receipt.IsVerified = true;
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            return View(receipt);
        }

        public Receipt GetReceiptBy(long pmid)
        {
            Receipt receipt = null;
            PaymentLogic paymentLogic = new PaymentLogic();
            RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
            PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
            StudentLogic studentLogic = new StudentLogic();
            StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
            AdmissionListLogic admissionListLogic = new AdmissionListLogic();

            try
            {
                Payment payment = paymentLogic.GetBy(pmid);
                if (payment == null || payment.Id <= 0)
                {
                    return null;
                }
                var student = studentLogic.GetBy(payment.Person.Id);
                var studentLevel = studentLevelLogic.GetModelBy(sl => sl.Person_Id == payment.Person.Id && sl.Session_Id == payment.Session.Id);
                if (payment.FeeType?.Id > 0 && (payment.FeeType.Id == (int)FeeTypes.AcceptanceFee || payment.FeeType.Id == (int)FeeTypes.HNDAcceptance) && student == null && studentLevel == null)
                {
                    RemitaPayment remitaPayment = remitaPaymentLogic.GetModelsBy(o => o.Payment_Id == payment.Id).FirstOrDefault();
                    if (remitaPayment != null && remitaPayment.Status.Contains("01") || remitaPayment.Status.Contains("manual-miracle"))
                    {
                        AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
                        var appliedCourse = appliedCourseLogic.GetModelsBy(f => f.APPLICATION_FORM.Person_Id == payment.Person.Id).FirstOrDefault();
                        payment.DatePaid = remitaPayment.TransactionDate;
                        payment.FeeDetails = paymentLogic.SetFeeDetails(payment.FeeType);
                        decimal amountRRR = remitaPayment.TransactionAmount;
                        receipt = BuildReceipt(payment, remitaPayment.OrderId, remitaPayment, amountRRR, appliedCourse);
                        return receipt;
                    }
                }
                if (studentLevel == null)
                {
                    studentLevel = studentLevelLogic.GetModelsBy(f => f.Person_Id == payment.Person.Id).LastOrDefault();
                    if (studentLevel == null)
                    {
                        StudentLevel createLevel = new StudentLevel()
                        {
                            Programme = new Programme() { Id = 1 },
                            Department = new Department() { Id = 1 },
                            Level = new Level() { Id = 1 },
                            Student = student,
                            Active = true,
                            Session = payment.Session,
                        };
                        studentLevel = studentLevelLogic.Create(createLevel);
                    }

                }


                if (payment.FeeType.Id == (int)FeeTypes.CerificateCollection || payment.FeeType.Id == (int)FeeTypes.Transcript || payment.FeeType.Id == (int)FeeTypes.ConvocationFee)
                {
                    RemitaPayment remitaPayment = remitaPaymentLogic.GetModelsBy(o => o.Payment_Id == payment.Id).FirstOrDefault();
                    if (remitaPayment != null && remitaPayment.Status.Contains("01"))
                    {
                        payment.DatePaid = remitaPayment.TransactionDate;
                        payment.FeeDetails = paymentLogic.SetFeeDetails(payment.FeeType);
                        decimal amountRRR = remitaPayment.TransactionAmount;
                        receipt = BuildReceipt(payment, remitaPayment.OrderId, remitaPayment, amountRRR, student, studentLevel);
                        return receipt;
                    }
                }

                if (student == null || studentLevel == null)
                {
                    if (payment.FeeType.Id == (int)FeeTypes.CerificateCollection || payment.FeeType.Id == (int)FeeTypes.Transcript)
                    {
                        studentLevel = studentLevelLogic.GetStudentLevel(payment.Person.Id);
                        payment.FeeDetails = paymentLogic.SetFeeDetails(payment.FeeType);


                        if (studentLevel == null || studentLevel.Programme == null || studentLevel.Department == null || studentLevel.Level == null)
                            throw new Exception("StudentLevel record not found for this session.");
                    }
                    else if (payment.FeeType.Id == (int)FeeTypes.AcceptanceFee || payment.FeeType.Id == (int)FeeTypes.HNDAcceptance)
                    {
                        var candidate = admissionListLogic.GetBy(payment.Person);
                        int levelId = payment.FeeType.Id == (int)FeeTypes.AcceptanceFee ? 1 : 3;
                        studentLevel = new StudentLevel()
                        {
                            Level = new Level() { Id = levelId },
                            Programme = candidate.Form.ProgrammeFee.Programme,
                            Department = candidate.Deprtment
                        };
                        payment.FeeDetails = paymentLogic.SetFeeDetails(payment.FeeType);
                    }
                    else
                        throw new Exception("StudentLevel record not found for this session.");
                }

                payment.FeeDetails = paymentLogic.SetFeeDetails(payment, studentLevel.Programme.Id, studentLevel.Level.Id, payment.PaymentMode.Id, studentLevel.Department.Id, payment.Session.Id);

                if (payment.FeeDetails == null || payment.FeeDetails.Count <= 0 && payment.FeeType.Id != (int)FeeTypes.ShortFall && payment.FeeType.Id != (int)FeeTypes.CerificateCollection && payment.FeeType.Id != (int)FeeTypes.ConvocationFee && payment.FeeType.Id != (int)FeeTypes.Transcript)
                {
                    throw new Exception("Fee Details for " + payment.FeeType.Name + ", " + studentLevel.Programme.Name + ", " + studentLevel.Department.Name + ", " +
                        studentLevel.Level.Name + ", " + payment.Session.Name + " not set! please contact your system administrator.");
                }

                if (payment.FeeType.Id == (int)FeeTypes.Transcript)
                {
                    Fee fee = GetPaymentFee(payment);

                    payment.FeeDetails = paymentLogic.SetFeeDetails(payment, studentLevel.Programme.Id, studentLevel.Level.Id, payment.PaymentMode.Id, studentLevel.Department.Id,
                                        payment.Session.Id, fee.Id);
                    if (payment.FeeDetails.Count <= 0)
                        payment.FeeDetails = paymentLogic.SetFeeDetails(payment, studentLevel.Programme.Id, studentLevel.Level.Id, payment.PaymentMode.Id, studentLevel.Department.Id, payment.Session.Id);
                }
                if (payment.FeeType.Id == (int)FeeTypes.CerificateCollection)
                {
                    payment.FeeDetails = paymentLogic.SetFeeDetails(payment, studentLevel.Programme.Id, studentLevel.Level.Id, payment.PaymentMode.Id, studentLevel.Department.Id,
                                        payment.Session.Id, (int)Fees.CertificateFee_5K);
                    if (payment.FeeDetails.Count <= 0)
                        payment.FeeDetails = paymentLogic.SetFeeDetails(payment, studentLevel.Programme.Id, studentLevel.Level.Id, payment.PaymentMode.Id, studentLevel.Department.Id, payment.Session.Id);

                }


                //Resolve for shortfall
                if (payment.FeeType.Id == (int)FeeTypes.ShortFall)
                {
                    RemitaPayment remitaPayment = remitaPaymentLogic.GetModelsBy(o => o.Payment_Id == payment.Id).FirstOrDefault();
                    //Manually set feedetails params for shortfall since shortfall generation are automatic and feedetails not set. //Miracle
                    Fee fee = new Fee()
                    {
                        Amount = remitaPayment.TransactionAmount,
                        Name = "ShortFall"
                    };
                    FeeType feeTypeShortFall = new FeeType() { Name = "Short-Fall" };
                    FeeDetail shortFallFeeDetail = new FeeDetail()
                    {
                        Fee = fee,
                        FeeType = feeTypeShortFall
                    };
                    payment.FeeDetails.Add(shortFallFeeDetail);


                }
                //
                if (payment.FeeType.Id == (int)FeeTypes.SchoolFees && studentLevel.Programme.Id != (int)Programmes.HNDDistance && studentLevel.Programme.Id != (int)Programmes.NDDistance)
                {
                    FeeinPaymentLogic feeinPaymentLogic = new FeeinPaymentLogic();
                    FeeDetailLogic feeDetailLogic = new FeeDetailLogic();
                    var feeinPayments = feeinPaymentLogic.GetModelsBy(f => f.Payment_Id == payment.Id);
                    foreach (var item in feeinPayments)
                    {
                        if (item.IsIncluded)
                            payment.FeeDetails = feeDetailLogic.AlterFeeDetailRecords(item.Fee.Id, false, payment.FeeDetails);
                        else
                            payment.FeeDetails = feeDetailLogic.AlterFeeDetailRecords(item.Fee.Id, true, payment.FeeDetails);
                    }
                }
                PaymentEtranzact paymentEtranzact = paymentEtranzactLogic.GetModelBy(o => o.Payment_Id == payment.Id);
                if (paymentEtranzact != null)
                {
                    decimal amount = (decimal)paymentEtranzact.TransactionAmount;
                    //amount = amount != payment.FeeDetails.Sum(f => f.Fee.Amount) ? payment.FeeDetails.Sum(f => f.Fee.Amount) : amount;
                    if (payment.FeeDetails.Sum(f => f.Fee.Amount) != paymentEtranzact.TransactionAmount)
                    {
                        payment.FeeDetails = paymentLogic.FormatReceiptBreakDown(payment.FeeDetails, (decimal)paymentEtranzact.TransactionAmount);
                        if (payment.FeeDetails.Sum(f => f.Fee.Amount) != (decimal)paymentEtranzact.TransactionAmount)
                        {
                            payment.FeeDetails.ForEach(f =>
                            {
                                f.Fee.Amount = 0M;
                            });
                        }

                        //payment.FeeDetails.ForEach(f =>
                        //{
                        //    f.Fee.Amount = 0M;
                        //});
                    }

                    PaymentScholarship scholarship = new PaymentScholarship();
                    PaymentScholarshipLogic scholarshipLogic = new PaymentScholarshipLogic();
                    if (scholarshipLogic.IsStudentOnScholarship(payment.Person, payment.Session))
                    {
                        scholarship = scholarshipLogic.GetBy(payment.Person);
                        amount = payment.FeeDetails.Sum(p => p.Fee.Amount) - scholarship.Amount;
                    }

                    payment.DatePaid = Convert.ToDateTime(paymentEtranzact.TransactionDate);

                    //receipt = BuildReceipt(payment.Person.FullName, payment.InvoiceNumber, paymentEtranzact, amount, payment.FeeType.Name);
                    receipt = BuildReceipt(payment, paymentEtranzact.ReceiptNo, paymentEtranzact.ConfirmationNo, amount, student, studentLevel);
                }
                else
                {
                    RemitaPayment remitaPayment = remitaPaymentLogic.GetModelsBy(o => o.Payment_Id == payment.Id).FirstOrDefault();
                    if (remitaPayment != null && remitaPayment.Status.Contains("01") || remitaPayment.Description.Contains("manual-miracle"))
                    {
                        if (payment.FeeType.Id == (int)FeeTypes.CerificateCollection || payment.FeeType.Id == (int)FeeTypes.Transcript)
                        {
                            payment.DatePaid = remitaPayment.TransactionDate;

                            payment.FeeDetails = paymentLogic.SetFeeDetails(payment.FeeType);
                            decimal amountRRR = remitaPayment.TransactionAmount;
                            receipt = BuildReceipt(payment, remitaPayment.OrderId, remitaPayment, amountRRR, student, studentLevel);
                            return receipt;
                        }

                        decimal amount = remitaPayment.TransactionAmount;
                        //amount = amount != payment.FeeDetails.Sum(f => f.Fee.Amount) ? payment.FeeDetails.Sum(f => f.Fee.Amount) : amount;
                        if (payment.FeeDetails.Sum(f => f.Fee.Amount) != remitaPayment.TransactionAmount)
                        {

                            payment.FeeDetails = paymentLogic.FormatReceiptBreakDown(payment.FeeDetails, remitaPayment.TransactionAmount);
                            if (payment.FeeDetails.Sum(f => f.Fee.Amount) != remitaPayment.TransactionAmount)
                            {
                                if (remitaPayment.TransactionAmount > payment.FeeDetails.Sum(f => f.Fee.Amount) && payment.FeeType.Id != (int)FeeTypes.CarryOverSchoolFees)
                                {
                                    amount = payment.FeeDetails.Sum(f => f.Fee.Amount);
                                }
                                if(payment.FeeType.Id == (int)FeeTypes.CarryOverSchoolFees)
                                {
                                    payment.FeeDetails.ForEach(f =>
                                    {
                                        f.Fee.Amount = 0M;
                                    });
                                }

                            }
                        }

                        PaymentScholarship scholarship = new PaymentScholarship();
                        PaymentScholarshipLogic scholarshipLogic = new PaymentScholarshipLogic();
                        if (scholarshipLogic.IsStudentOnScholarship(payment.Person, payment.Session))
                        {
                            scholarship = scholarshipLogic.GetBy(payment.Person);
                            amount = payment.FeeDetails.Sum(p => p.Fee.Amount) - scholarship.Amount;
                        }

                        payment.DatePaid = remitaPayment.TransactionDate;

                        //receipt = BuildReceipt(payment.Person.FullName, payment.InvoiceNumber, remitaPayment, amount, payment.FeeType.Name, student.MatricNumber, "");
                        receipt = BuildReceipt(payment, remitaPayment.OrderId, remitaPayment.RRR, amount, student, studentLevel);
                    }
                }
                return receipt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private Fee GetPaymentFee(Payment payment)
        {
            Fee fee = null;
            try
            {
                if (payment != null)
                {
                    TranscriptRequestLogic requestLogic = new TranscriptRequestLogic();

                    TranscriptRequest tRequest = requestLogic.GetModelsBy(t => t.Payment_Id == payment.Id).LastOrDefault();

                    string type = tRequest != null ? tRequest.RequestType : null;

                    if (tRequest != null && tRequest.DestinationCountry != null && tRequest.DestinationCountry.Id == "NIG")
                    {
                        if (type == "Certificate Verification" || type == "Certificate Collection")
                        {
                            fee = new Fee() { Id = 46 };
                        }
                        else if (type == "Transcript Verification")
                        {
                            fee = new Fee() { Id = 46, Name = "TRANSCRIPT VERIFICATION(LOCAL)" };
                        }
                        else if (type == "Convocation Fee")
                        {
                            fee = new Fee() { Id = 60 };
                        }
                        else if (type == null)
                        {
                            fee = new Fee() { Id = 46 };
                        }
                        else
                        {
                            fee = new Fee() { Id = 46 };
                        }

                    }
                    else
                    {

                        if (type == "Certificate Verification" || type == "Certificate Collection")
                        {
                            fee = new Fee() { Id = 46 };
                        }
                        else if (type == "Transcript Verification")
                        {
                            fee = new Fee() { Id = 47, Name = "TRANSCRIPT VERIFICATION(INTERNATIONAL)" };
                        }
                        else if (type == "Convocation Fee")
                        {
                            fee = new Fee() { Id = 60 };
                        }
                        else
                        {
                            fee = new Fee() { Id = 47 };
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return fee;
        }

        public Receipt BuildReceipt(string name, string invoiceNumber, RemitaPayment remitaPayment, decimal amount, string purpose, string MatricNumber, string ApplicationFormNumber)
        {
            try
            {
                Receipt receipt = new Receipt();
                receipt.Number = remitaPayment.OrderId;
                receipt.Name = name;
                receipt.ConfirmationOrderNumber = remitaPayment.RRR;
                receipt.Amount = remitaPayment.TransactionAmount;
                receipt.AmountInWords = "";
                receipt.Purpose = purpose;
                receipt.Date = DateTime.Now;
                receipt.ApplicationFormNumber = ApplicationFormNumber;
                receipt.MatricNumber = MatricNumber;
                return receipt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Receipt BuildReceipt(string name, string invoiceNumber, PaymentEtranzact paymentEtranzact, decimal amount, string purpose)
        {
            try
            {
                Receipt receipt = new Receipt();
                receipt.Number = paymentEtranzact.ReceiptNo;
                receipt.Name = name;
                receipt.ConfirmationOrderNumber = paymentEtranzact.ConfirmationNo;
                receipt.Amount = amount;
                receipt.AmountInWords = "";
                receipt.Purpose = purpose;
                receipt.Date = DateTime.Now;

                return receipt;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public Receipt BuildReceipt(Payment payment, string Number, string CON, decimal amount, Model.Model.Student student, StudentLevel studentLevel)
        {
            try
            {
                Receipt receipt = new Receipt();
                receipt.Number = payment.Id.ToString();
                receipt.Name = payment.Person.FullName;
                receipt.ConfirmationOrderNumber = CON;
                receipt.Amount = amount;
                receipt.AmountInWords = "";
                receipt.Purpose = payment.FeeType.Name;
                receipt.Date = payment.DatePaid;
                receipt.ApplicationFormNumber = "";
                receipt.MatricNumber = student != null ? student.MatricNumber : "N/A";
                receipt.Session = payment.Session.Name;
                receipt.Department = studentLevel.Department.Name;
                receipt.FeeDetails = payment.FeeDetails;
                receipt.PaymentId = Utility.Encrypt(payment.Id.ToString());

                return receipt;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public Receipt BuildReceipt(Payment payment, string Number, RemitaPayment remitaPayment, decimal amount, Model.Model.Student student, StudentLevel studentLevel)
        {
            try
            {
                Receipt receipt = new Receipt();
                receipt.Number = payment.Id.ToString();
                receipt.Name = payment.Person.FullName;
                receipt.ConfirmationOrderNumber = remitaPayment.RRR;
                receipt.Amount = amount;
                receipt.AmountInWords = "";
                receipt.Purpose = payment.FeeType.Name;
                receipt.Date = payment.DatePaid;
                receipt.ApplicationFormNumber = "";
                receipt.MatricNumber = student.MatricNumber;
                receipt.Session = payment.Session.Name;
                //                receipt.Department = studentLevel.Department.Name;
                //receipt.FeeDetails = payment.FeeDetails;
                receipt.PaymentId = Utility.Encrypt(payment.Id.ToString());

                return receipt;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public Receipt BuildReceipt(Payment payment, string Number, RemitaPayment remitaPayment, decimal amount, AppliedCourse appliedCourse)
        {
            try
            {
                Receipt receipt = new Receipt();
                receipt.Number = payment.Id.ToString();
                receipt.Name = payment.Person.FullName;
                receipt.ConfirmationOrderNumber = remitaPayment.RRR;
                receipt.Amount = amount;
                receipt.AmountInWords = "";
                receipt.Purpose = payment.FeeType.Name;
                receipt.Date = payment.DatePaid;
                receipt.ApplicationFormNumber = appliedCourse?.Person.Id > 0 && appliedCourse?.ApplicationForm?.Id > 0 ? appliedCourse.ApplicationForm.Number : "N/A";
                receipt.MatricNumber = appliedCourse?.Person.Id > 0 && appliedCourse?.ApplicationForm?.Id > 0 ? appliedCourse.ApplicationForm.Number : "N/A";
                receipt.Session = payment.Session.Name;
                //                receipt.Department = studentLevel.Department.Name;
                //receipt.FeeDetails = payment.FeeDetails;
                receipt.PaymentId = Utility.Encrypt(payment.Id.ToString());

                return receipt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ActionResult AdmissionLetter(string fid)
        {
            AdmissionLetter admissionLetter = null;
            Int64 formId = Convert.ToInt64(Abundance_Nk.Web.Models.Utility.Decrypt(fid));
            try
            {
                admissionLetter = GetAdmissionLetterBy(formId);
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            return View(admissionLetter);
        }

        public ActionResult ODFELAdmissionLetter(string fid)
        {
            AdmissionLetter admissionLetter = null;
            Int64 formId = Convert.ToInt64(Abundance_Nk.Web.Models.Utility.Decrypt(fid));
            try
            {
                admissionLetter = GetAdmissionLetterBy(formId);
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            return View(admissionLetter);
        }


        public AdmissionLetter GetAdmissionLetterBy(long formId)
        {
            try
            {
                AdmissionLetter admissionLetter = null;
                ApplicationFormLogic applicationFormLogic = new ApplicationFormLogic();
                ApplicationForm applicationForm = applicationFormLogic.GetBy(formId);


                if (applicationForm != null && applicationForm.Id > 0)
                {
                    AdmissionList list = new AdmissionList();
                    AdmissionListLogic listLogic = new AdmissionListLogic();
                    list = listLogic.GetBy(applicationForm.Id);

                    FeeDetailLogic feeDetailLogic = new FeeDetailLogic();
                    List<FeeDetail> feeDetails = feeDetailLogic.GetModelsBy(f => f.Fee_Type_Id == (int)FeeTypes.SchoolFees);

                    AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
                    AppliedCourse appliedCourse = appliedCourseLogic.GetModelBy(a => a.Person_Id == applicationForm.Person.Id);
                    if (appliedCourse == null)
                    {
                        throw new Exception("Applicant Applied Course cannot be found! Please contact your system administrator.");
                    }

                    admissionLetter = new AdmissionLetter();
                    admissionLetter.Person = applicationForm.Person;
                    admissionLetter.Session = applicationForm.Payment.Session;
                    admissionLetter.FeeDetails = feeDetails;
                    admissionLetter.Programme = appliedCourse.Programme;
                    admissionLetter.Department = list.Deprtment;
                    admissionLetter.RegistrationEndDate = applicationForm.Setting.RegistrationEndDate;
                    admissionLetter.RegistrationEndTime = applicationForm.Setting.RegistrationEndTime;
                    admissionLetter.RegistrationEndTimeString = applicationForm.Setting.RegistrationEndTimeString;

                    if (admissionLetter.Session == null || admissionLetter.Session.Id <= 0)
                    {
                        throw new Exception("Session not set for this admission period! Please contact your system administrator.");
                    }
                    else if (!admissionLetter.RegistrationEndDate.HasValue)
                    {
                        throw new Exception("Registration End Date not set for this admission period! Please contact your system administrator.");
                    }
                    else if (!admissionLetter.RegistrationEndTime.HasValue)
                    {
                        throw new Exception("Registration End Time not set for this admission period! Please contact your system administrator.");
                    }

                    string programmeType = "National Diploma";
                    if (appliedCourse.Programme.Id == 1 || appliedCourse.Programme.Id == 2)
                    {
                        admissionLetter.ProgrammeType = programmeType;
                    }
                    else if (appliedCourse.Programme.Id == 7)
                    {
                        admissionLetter.ProgrammeType = "Certificate";
                    }
                    else
                    {
                        admissionLetter.ProgrammeType = "Higher " + programmeType;
                    }
                }

                return admissionLetter;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ActionResult AdmissionSlip(string fid)
        {
            AdmissionLetter admissionLetter = null;
            Int64 formId = Convert.ToInt64(Abundance_Nk.Web.Models.Utility.Decrypt(fid));
            try
            {
                admissionLetter = GetAdmissionLetterBy(formId);
            }
            catch (Exception)
            {
                throw;
            }

            return View(admissionLetter);
        }

        public ActionResult FinancialClearanceSlip(string pid)
        {
            try
            {
                Int64 paymentid = Convert.ToInt64(Abundance_Nk.Web.Models.Utility.Decrypt(pid));
                StudentLogic studentLogic = new StudentLogic();
                Model.Model.Student student = studentLogic.GetBy(paymentid);


                PaymentLogic paymentLogic = new PaymentLogic();
                PaymentHistory paymentHistory = new PaymentHistory();
                paymentHistory.Payments = paymentLogic.GetBy(student);
                paymentHistory.Student = student;

                return View(paymentHistory);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ActionResult Invoice(string pmid)
        {
            try
            {
                List<int> displayFee = new List<int>() { 169, 170, 172, 173, 174, 175, 171 };
                Int64 paymentid = Convert.ToInt64(Abundance_Nk.Web.Models.Utility.Decrypt(pmid));
                PaymentLogic paymentLogic = new PaymentLogic();
                RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                Payment payment = paymentLogic.GetBy(paymentid);
                //if (payment.FeeType.Id == (int)FeeTypes.SchoolFees || payment.FeeType.Id == (int)FeeTypes.CarryOverSchoolFees || payment.FeeType.Id == (int)FeeTypes.ConvocationFee)
                //{
                if (payment != null && payment.FeeType.Id == (int)FeeTypes.ShortFall)
                {
                    var isPaid = remitaPaymentLogic.GetModelBy(x => x.Payment_Id == payment.Id);
                    return RedirectToAction("ShortFallInvoice", "Credential", new { area = "Common", pmid = paymentid, amount = isPaid.TransactionAmount });
                }
                Invoice invoice = new Invoice();
                invoice.Person = payment.Person;
                invoice.Payment = payment;
                invoice.PaymentMode = payment.PaymentMode;

                invoice.barcodeImageUrl = GenerateBarcode(paymentid);

                Model.Model.Student student = new Model.Model.Student();
                StudentLogic studentLogic = new StudentLogic();
                student = studentLogic.GetBy(payment.Person.Id);


                //PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
                //PaymentEtranzactType paymentEtranzactType = new PaymentEtranzactType();
                //PaymentEtranzactTypeLogic PaymentEtranzactTypeLogic = new Business.PaymentEtranzactTypeLogic();

                PaymentMode paymentMode = new PaymentMode() { Id = 1 };
                StudentLevel studentLevel = new StudentLevel();
                StudentLevelLogic levelLogic = new StudentLevelLogic();
                studentLevel = levelLogic.GetModelsBy(sl => sl.Person_Id == student.Id && sl.Session_Id == payment.Session.Id).LastOrDefault();
                if (studentLevel == null)
                {
                    studentLevel = levelLogic.GetBy(student.Id);
                }
                if (studentLevel != null)
                {
                    invoice.MatricNumber = student.MatricNumber;
                    invoice.Programme = studentLevel.Programme;
                    if (studentLevel.Programme.Id == (int)Programmes.HNDDistance || studentLevel.Programme.Id == (int)Programmes.NDDistance)
                    {
                        paymentMode.Id = payment.PaymentMode.Id;
                    }
                    if (payment?.Id > 0 && payment.FeeType?.Id > 0 && payment.FeeType.Id == (int)FeeTypes.ConvocationFee)
                    {
                        //Deferentiate between HND and ND
                        int levelId = studentLevel.Programme.Id > 2 ? 4 : 2;
                        payment.FeeDetails = paymentLogic.SetFeeDetails(payment, studentLevel.Programme.Id, levelId, paymentMode.Id, studentLevel.Department.Id, payment.Session.Id);
                    }

                    else
                    {
                        payment.FeeDetails = paymentLogic.SetFeeDetails(payment, studentLevel.Programme.Id, studentLevel.Level.Id, paymentMode.Id, studentLevel.Department.Id, payment.Session.Id);
                    }

                    #region
                    if (payment.FeeType.Id == (int)FeeTypes.SchoolFees && studentLevel.Programme.Id != (int)Programmes.HNDDistance && studentLevel.Programme.Id != (int)Programmes.NDDistance)
                    {
                        FeeinPaymentLogic feeinPaymentLogic = new FeeinPaymentLogic();
                        FeeDetailLogic feeDetailLogic = new FeeDetailLogic();
                        var feeinPayments = feeinPaymentLogic.GetModelsBy(f => f.Payment_Id == payment.Id);
                        foreach (var item in feeinPayments)
                        {
                            if (item.IsIncluded)
                                payment.FeeDetails = feeDetailLogic.AlterFeeDetailRecords(item.Fee.Id, false, payment.FeeDetails);
                            else
                                payment.FeeDetails = feeDetailLogic.AlterFeeDetailRecords(item.Fee.Id, true, payment.FeeDetails);
                        }
                    }
                    #endregion
                    invoice.FeeDetails = payment.FeeDetails.Where(f => displayFee.Contains(f.Fee.Id)).ToList();
                    invoice.Amount = payment.FeeDetails.Sum(f => f.Fee.Amount);
                    if (invoice.FeeDetails.Count > 0)
                    {
                        var feeAmmount = invoice.FeeDetails.Sum(f => f.Fee.Amount);
                        invoice.Amount = invoice.Amount - feeAmmount;
                    }

                    //paymentEtranzactType = PaymentEtranzactTypeLogic.GetModelBy(p => p.Level_Id == studentLevel.Level.Id && p.Payment_Mode_Id == payment.PaymentMode.Id && p.Fee_Type_Id == payment.FeeType.Id && p.Programme_Id == studentLevel.Programme.Id && p.Session_Id == payment.Session.Id);
                    //invoice.paymentEtranzactType = paymentEtranzactType;
                    invoice.Department = studentLevel.Department;
                }
                else
                {
                    AdmissionList list = new AdmissionList();
                    AdmissionListLogic listLogic = new AdmissionListLogic();
                    list = listLogic.GetBy(payment.Person);
                    if (list != null)
                    {
                        Level level = new Level();
                        level = SetLevel(list.Form.ProgrammeFee.Programme);
                        payment.FeeDetails = paymentLogic.SetFeeDetails(payment, list.Form.ProgrammeFee.Programme.Id, level.Id, 1, list.Deprtment.Id, payment.Session.Id);
                        //paymentEtranzactType = PaymentEtranzactTypeLogic.GetModelBy(p => p.Level_Id == level.Id && p.Payment_Mode_Id == payment.PaymentMode.Id && p.Fee_Type_Id == payment.FeeType.Id && p.Programme_Id == list.Form.ProgrammeFee.Programme.Id && p.Session_Id == payment.Session.Id);
                        //invoice.paymentEtranzactType = paymentEtranzactType;
                        invoice.FeeDetails = payment.FeeDetails.Where(f => displayFee.Contains(f.Fee.Id)).ToList();
                        invoice.Department = list.Deprtment;
                    }
                }


                //PaymentEtranzact paymentEtranzact = paymentEtranzactLogic.GetBy(payment);
                //if (paymentEtranzact != null)
                //{
                //    invoice.Paid = true;
                //}

                RemitaPayment remitaPayment = new RemitaPayment();
                //RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                remitaPayment = remitaPaymentLogic.GetBy(payment.Id);
                if (remitaPayment != null)
                {
                    invoice.remitaPayment = remitaPayment;
                    if (remitaPayment.Status.Contains("01"))
                    {
                        invoice.Paid = true;
                    }
                }

                PaymentScholarship scholarship = new PaymentScholarship();
                PaymentScholarshipLogic scholarshipLogic = new PaymentScholarshipLogic();
                if (scholarshipLogic.IsStudentOnScholarship(payment.Person, payment.Session))
                {
                    scholarship = scholarshipLogic.GetBy(payment.Person);
                    invoice.paymentScholarship = scholarship;
                    invoice.Amount = payment.FeeDetails.Sum(p => p.Fee.Amount) - scholarship.Amount;
                    invoice.FeeDetails = payment.FeeDetails.Where(f => displayFee.Contains(f.Fee.Id)).ToList();
                    if (invoice.FeeDetails.Count > 0)
                    {
                        invoice.Amount = invoice.Amount - invoice.FeeDetails.Sum(f => f.Fee.Amount);
                    }

                }

                TempData.Keep("PaymentViewModel");

                return View(invoice);
                //}

            }
            catch (Exception)
            {
                throw;
            }
            return View();
        }

        private string GenerateBarcode(long paymentid)
        {
            string barcodeImageUrl = "";
            try
            {
                BarcodeLib.Barcode barcode = new BarcodeLib.Barcode(paymentid.ToString(), TYPE.CODE39);
                Image image = barcode.Encode(TYPE.CODE39, paymentid.ToString());
                byte[] imageByteData = imageToByteArray(image);
                string imageBase64Data = Convert.ToBase64String(imageByteData);
                string imageDataURL = string.Format("data:image/jpg;base64,{0}", imageBase64Data);

                barcodeImageUrl = imageDataURL;
            }
            catch (Exception)
            {
                throw;
            }

            return barcodeImageUrl;
        }
        public ActionResult ShortFallInvoice(string pmid, string amount)
        {
            try
            {

                int paymentid = Convert.ToInt32(pmid);
                PaymentLogic paymentLogic = new PaymentLogic();
                Payment payment = paymentLogic.GetBy(paymentid);

                RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();

                if ((payment.FeeType.Id == (int)FeeTypes.ShortFall))
                {
                    if (TempData["FeeDetail"] != null)
                    {
                        payment.FeeDetails = (List<FeeDetail>)TempData["FeeDetail"];
                    }

                    Invoice invoice = new Invoice();
                    invoice.Person = payment.Person;
                    invoice.Payment = payment;

                    //PaymentEtranzactType paymentEtranzactType = new PaymentEtranzactType();
                    //PaymentEtranzactTypeLogic paymentEtranzactTypeLogic = new PaymentEtranzactTypeLogic();

                    //paymentEtranzactType = paymentEtranzactTypeLogic.GetModelBy(p => p.Fee_Type_Id == payment.FeeType.Id && p.Session_Id == payment.Session.Id);
                    //invoice.paymentEtranzactType = paymentEtranzactType;

                    RemitaPayment remitaPayment = new RemitaPayment();
                    remitaPayment = remitaPaymentLogic.GetBy(payment.Id);

                    if (remitaPayment != null)
                    {
                        invoice.remitaPayment = remitaPayment;
                        if (remitaPayment.Status.Contains("01"))
                        {
                            invoice.Paid = true;
                        }
                    }

                    invoice.Amount = Convert.ToDecimal(amount);

                    return View(invoice);
                }
                else
                {
                    Person oldPerson = new Person();
                    oldPerson = (Person)TempData["OldPerson"];
                    PostJambViewModel viewModel = new PostJambViewModel();
                    AppliedCourse appliedCourse = new AppliedCourse();
                    ApplicationForm applicationForm = new Model.Model.ApplicationForm();
                    ApplicationFormLogic applicationFormLogic = new ApplicationFormLogic();
                    AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
                    Session session = new Model.Model.Session() { Id = payment.Session.Id };
                    SessionLogic sessionLogic = new SessionLogic();
                    ApplicationFormSetting applicationFormSetting = new ApplicationFormSetting();
                    ApplicantJambDetail applicantJambDetail = new ApplicantJambDetail();
                    ApplicantJambDetailLogic applicantJambDetailLogic = new ApplicantJambDetailLogic();
                    applicantJambDetail = applicantJambDetailLogic.GetModelBy(p => p.Person_Id == oldPerson.Id);
                    ApplicationFormSettingLogic applicationFormSettingLogic = new ApplicationFormSettingLogic();
                    applicationFormSetting = applicationFormSettingLogic.GetModelBy(p => p.Application_Form_Setting_Id == 2);
                    //session = sessionLogic.GetModelBy(p=>p.Activated == true);
                    appliedCourse = appliedCourseLogic.GetModelBy(p => p.Person_Id == payment.Person.Id);
                    applicationForm = applicationFormLogic.GetModelBy(p => p.Person_Id == payment.Person.Id);
                    viewModel.AppliedCourse = appliedCourse;
                    viewModel.Person = payment.Person;
                    viewModel.Session = session;
                    viewModel.Programme = appliedCourse.Programme;
                    viewModel.ApplicationFormNumber = applicationForm.Number;
                    viewModel.ApplicationFormSetting = applicationFormSetting;
                    viewModel.ApplicantJambDetail = applicantJambDetail;
                    viewModel.ApplicationForm = applicationForm;
                    TempData["viewModel"] = viewModel;
                    return RedirectToAction("PostJAMBSlip", new { controller = "Form", area = "Applicant" });
                }

            }
            catch (Exception)
            {
                throw;
            }

        }

        private Level SetLevel(Programme programme)
        {
            try
            {
                Level level;
                switch (programme.Id)
                {
                    case 1:
                        {
                            return level = new Level() { Id = 1 };

                        }
                    case 2:
                        {
                            return level = new Level() { Id = 1 };

                        }
                    case 3:
                        {
                            return level = new Level() { Id = 3 };

                        }
                    case 4:
                        {
                            return level = new Level() { Id = 3 };

                        }
                }
                return level = new Level();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public ActionResult TranscriptInvoice(string pmid)
        {
            try
            {
                string type = Convert.ToString(TempData["RequestType"]);
                if (type == "")
                {
                    type = null;
                }
                TempData.Keep("RequestType");

                int paymentid = Convert.ToInt32(pmid);
                PaymentLogic paymentLogic = new PaymentLogic();
                Payment payment = paymentLogic.GetBy(paymentid);

                Invoice invoice = new Invoice();
                invoice.Person = payment.Person;
                invoice.Payment = payment;

                if (type == null || type == "Transcript Verification")
                {
                    invoice.Payment.FeeType.Name = "Transcript";
                }
                //if (type == "Certificate Collection" || type == "Certificate Verification")
                //{
                //    invoice.Payment.FeeType.Name = "Certificate";
                //}
                if (type == "Certificate Collection")
                {
                    invoice.Payment.FeeType.Name = "Certificate Collection";
                }
                if (type == "Certificate Verification")
                {
                    invoice.Payment.FeeType.Name = "Certificate Verification";
                }
                if (type == "Wes")
                {
                    invoice.Payment.FeeType.Name = "WES Verification";
                }

                RemitaPayment remitaPayment = new RemitaPayment();
                RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                remitaPayment = remitaPaymentLogic.GetBy(payment.Id);
                if (remitaPayment != null)
                {
                    invoice.remitaPayment = remitaPayment;
                    invoice.Amount = remitaPayment.TransactionAmount;
                }

                TranscriptViewModel viewModel = new TranscriptViewModel();
                string hash = "538661740" + remitaPayment.RRR + "918567";
                RemitaPayementProcessor remitaProcessor = new RemitaPayementProcessor(hash);
                viewModel.Hash = remitaProcessor.HashPaymentDetailToSHA512(hash);
                viewModel.RemitaPayment = remitaPayment;
                viewModel.RemitaPayment.MerchantCode = "538661740";
                viewModel.RemitaPayment.RRR = remitaPayment.RRR;
                TempData["TranscriptViewModel"] = viewModel;
                return View(invoice);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public ActionResult CardPayment()
        {
            TranscriptViewModel viewModel = (TranscriptViewModel)TempData["TranscriptViewModel"];
            TempData.Keep("TranscriptViewModel");

            return View(viewModel);
        }

        public byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            return ms.ToArray();
        }
        public ActionResult VerifySchoolFees()
        {
            return View();
        }
        [HttpPost]
        public ActionResult VerifySchoolFees(Receipt model)
        {
            if (model.barcodeImageUrl != null)
            {
                int startIndex = model.barcodeImageUrl.IndexOf("pmid=");
                int pmid = Convert.ToInt32(model.barcodeImageUrl.Substring(startIndex).Split('=')[1]);
                if (pmid > 0)
                {

                    Payment payment = new Payment() { Id = pmid };
                    var loggeduser = new UserLogic();
                    var paymentEtranzactLogic = new PaymentEtranzactLogic();
                    var paymentVerificationLogic = new PaymentVerificationLogic();
                    var paymentEtranzact = paymentEtranzactLogic.GetBy(payment);
                    RemitaPayment remitaPayment = new RemitaPayment();

                    if (paymentEtranzact != null)
                    {
                        var paymentVerification = paymentVerificationLogic.GetBy(pmid);
                        if (paymentVerification == null)
                        {
                            string client = Request.LogonUserIdentity.Name + " ( " + HttpContext.Request.UserHostAddress + ")";
                            paymentVerification = new PaymentVerification();
                            paymentVerification.Payment = payment;
                            paymentVerification.User = loggeduser.GetModelBy(u => u.User_Name == User.Identity.Name);
                            paymentVerification.DateVerified = DateTime.Now;
                            paymentVerification.Comment = client;
                            paymentVerification = paymentVerificationLogic.Create(paymentVerification);
                            paymentVerification.Payment = payment;
                            paymentVerification.User = loggeduser.GetModelBy(u => u.User_Name == User.Identity.Name);
                            paymentVerification.DateVerified = DateTime.Now;
                            paymentVerification.Comment = client;
                        }
                        else
                        {

                            SetMessage("Payment has been verified previously on." + paymentVerification.DateVerified, Message.Category.Warning);
                            return View();
                        }
                    }
                    else
                    {
                        RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                        remitaPayment = remitaPaymentLogic.GetModelBy(p => p.Payment_Id == pmid && p.Status.Contains("01:"));
                        if (remitaPayment != null)
                        {
                            var paymentVerification = paymentVerificationLogic.GetBy(pmid);
                            if (paymentVerification == null)
                            {
                                string client = Request.LogonUserIdentity.Name + " ( " + HttpContext.Request.UserHostAddress + ")";
                                paymentVerification = new PaymentVerification();
                                paymentVerification.Payment = payment;
                                paymentVerification.User = loggeduser.GetModelBy(u => u.User_Name == User.Identity.Name);
                                paymentVerification.DateVerified = DateTime.Now;
                                paymentVerification.Comment = client;
                                paymentVerification = paymentVerificationLogic.Create(paymentVerification);
                                paymentVerification.Payment = payment;
                                paymentVerification.User = loggeduser.GetModelBy(u => u.User_Name == User.Identity.Name);
                                paymentVerification.DateVerified = DateTime.Now;
                                paymentVerification.Comment = client;
                            }
                            else
                            {
                                SetMessage("Payment has been verified previously on." + paymentVerification.DateVerified, Message.Category.Warning);
                                return View();
                            }
                        }
                    }

                    if (paymentEtranzact == null && remitaPayment == null)
                    {
                        SetMessage("Payment has not been made.", Message.Category.Warning);
                    }
                    else
                    {
                        return RedirectToAction("Receipt", "Credential", new { pmid = pmid });
                    }
                }
                else
                {
                    SetMessage("Payment Could not be verified! Please ensure that student has made payment", Message.Category.Warning);
                }
            }

            return View();
        }
        public ActionResult FeeVerificationReport()
        {
            return View();
        }
        public ActionResult DownloadReceipt(string path)
        {
            try
            {
                return File(Server.MapPath(path), "application/pdf", "StudentReceipt.pdf");
            }
            catch (Exception)
            {
                throw;
            }
        }
        public ActionResult VerifyBySerialNumber()
        {
            return View();
        }

        [HttpPost]
        public ActionResult VerifyBySerialNumber(Receipt model)
        {
            try
            {
                string serialStr = !string.IsNullOrEmpty(model.PaymentId) ? model.PaymentId : "0";

                long pmid = Convert.ToInt32(serialStr);

                if (pmid > 0)
                {
                    Payment payment = new Payment() { Id = pmid };

                    var paymentEtranzactLogic = new PaymentEtranzactLogic();
                    var paymentVerificationLogic = new PaymentVerificationLogic();

                    var paymentEtranzact = paymentEtranzactLogic.GetBy(payment);
                    RemitaPayment remitaPayment = new RemitaPayment();

                    if (paymentEtranzact == null)
                    {
                        RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                        remitaPayment = remitaPaymentLogic.GetModelBy(p => p.Payment_Id == pmid && p.Status.Contains("01:"));
                    }

                    if (paymentEtranzact == null && remitaPayment == null)
                    {
                        SetMessage("Payment has not been made.", Message.Category.Warning);
                    }
                    else
                    {
                        return RedirectToAction("Receipt", "Credential", new { pmid = pmid });
                    }
                }
                else
                {
                    SetMessage("Invalid serial number.", Message.Category.Warning);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View();
        }
        public ActionResult ClearanceReceipt(string sid)
        {
            try
            {
                Receipt receipt = new Model.Model.Receipt();
                Int64 studentId = Convert.ToInt64(Abundance_Nk.Web.Models.Utility.Decrypt(sid));
                ClearanceLogLogic clearanceLogLogic = new ClearanceLogLogic();
                receipt.ClearanceLogs = clearanceLogLogic.GetModelsBy(f => f.Closed && f.Student_Id == studentId);
                receipt.barcodeImageUrl = "http://applications.federalpolyilaro.edu.ng/Common/Credential/ClearanceReceipt?pmid=" + sid; ;
                if (receipt.ClearanceLogs.Count > 0)
                {
                    receipt.Name = receipt.ClearanceLogs.FirstOrDefault().Student.FullName;
                    receipt.MatricNumber = receipt.ClearanceLogs.FirstOrDefault().Student.MatricNumber;
                    StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                    var level = studentLevelLogic.GetModelsBy(g => g.Person_Id == studentId).LastOrDefault();
                    if (level != null)
                    {
                        receipt.Department = level.Department.Name;
                        receipt.Programme = level.Programme.Name;
                    }
                    for (int i = 0; i < receipt.ClearanceLogs.Count; i++)
                    {
                        if (receipt.ClearanceLogs[i].ClearanceUnit.Id == 1)
                        {
                            receipt.Bursar = new Bursar();
                            receipt.Bursar.Name = receipt.ClearanceLogs[i].User.Username.ToUpper();
                            receipt.Bursar.Date = receipt.ClearanceLogs[i].DateCleared.Value.ToString("d");
                            receipt.Bursar.Signature = receipt.ClearanceLogs[i].User.SignatureUrl;
                        }
                        else if (receipt.ClearanceLogs[i].ClearanceUnit.Id == 2)
                        {
                            receipt.Librarian = new Librarian();
                            receipt.Librarian.Name = receipt.ClearanceLogs[i].User.Username.ToUpper();
                            receipt.Librarian.Date = receipt.ClearanceLogs[i].DateCleared.Value.ToString("d");
                            receipt.Librarian.Signature = receipt.ClearanceLogs[i].User.SignatureUrl;
                        }
                        else if (receipt.ClearanceLogs[i].ClearanceUnit.Id == 3)
                        {
                            receipt.StudentAffair = new StudentAffair();
                            receipt.StudentAffair.Name = receipt.ClearanceLogs[i].User.Username.ToUpper();
                            receipt.StudentAffair.Date = receipt.ClearanceLogs[i].DateCleared.Value.ToString("d");
                            receipt.StudentAffair.Signature = receipt.ClearanceLogs[i].User.SignatureUrl;
                        }
                        else if (receipt.ClearanceLogs[i].ClearanceUnit.Id == 4)
                        {
                            receipt.Health = new Health();
                            receipt.Health.Name = receipt.ClearanceLogs[i].User.Username.ToUpper();
                            receipt.Health.Date = receipt.ClearanceLogs[i].DateCleared.Value.ToString("d");
                            receipt.Health.Signature = receipt.ClearanceLogs[i].User.SignatureUrl;
                        }
                        else if (receipt.ClearanceLogs[i].ClearanceUnit.Id == 5)
                        {
                            receipt.DepartmentClearance = new DepartmentClearance();
                            receipt.DepartmentClearance.Name = receipt.ClearanceLogs[i].User.Username.ToUpper();
                            receipt.DepartmentClearance.Date = receipt.ClearanceLogs[i].DateCleared.Value.ToString("d");
                            receipt.DepartmentClearance.Signature = receipt.ClearanceLogs[i].User.SignatureUrl;
                        }
                    }
                }
                return View(receipt);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public ActionResult GetStudent(string id)
        {
            StudentIDModel studentIDModel = new StudentIDModel();
            studentIDModel.Payments = new List<PaymentView>();
            if (!string.IsNullOrEmpty(id))
            {
                var personId = Convert.ToInt64(id);
                StudentLogic studentLogic = new StudentLogic();
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                var studentLevel = studentLevelLogic.GetModelsBy(f => f.Person_Id == personId).FirstOrDefault();
                if (studentLevel?.Id > 0)
                {
                    StudentAcademicInformationLogic studentAcademicInformationLogic = new StudentAcademicInformationLogic();
                    studentIDModel.BloodGroup = studentLevel.Student.BloodGroup != null ? studentLevel.Student.BloodGroup.Name.ToUpper() : "";
                    studentIDModel.Name = studentLevel.Student.FullName != null ? studentLevel.Student.FullName.ToUpper() : "";
                    studentIDModel.MatricNo = studentLevel.Student.MatricNumber != null ? studentLevel.Student.MatricNumber.ToUpper() : "";
                    studentIDModel.PersonId = studentLevel.Student.Id;
                    studentIDModel.PassportUrl = "https://applications.federalpolyilaro.edu.ng/" + studentLevel.Student.ImageFileUrl;
                    studentIDModel.Department = studentLevel.Department.Name != null ? studentLevel.Department.Name.ToUpper() : "";
                    studentIDModel.Programme = studentLevel.Programme.Name != null ? studentLevel.Programme.Name.ToUpper() : "";
                    studentIDModel.GraduationYear = studentAcademicInformationLogic.GetModelsBy(f => f.Person_Id == personId).Select(f => f.YearOfGraduation).FirstOrDefault();

                    //get payment history
                    var paymentLogic = new PaymentLogic();
                    studentIDModel.Payments = paymentLogic.GetBy(studentLevel.Student);
                    studentIDModel.Payments = studentIDModel.Payments.Where(p => p.ConfirmationOrderNumber != null).ToList();


                }

            }


            return View(studentIDModel);
        }

    }
}