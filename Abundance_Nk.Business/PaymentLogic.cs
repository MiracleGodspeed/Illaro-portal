using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Abundance_Nk.Model.Model;
using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Translator;
using System.Linq.Expressions;
using Abundance_Nk.Model.Entity.Model;

namespace Abundance_Nk.Business
{
    public class PaymentLogic : BusinessBaseLogic<Payment, PAYMENT>
    {
        private FeeDetailLogic feeDetailLogic;

        public PaymentLogic()
        {
            feeDetailLogic = new FeeDetailLogic();
            translator = new PaymentTranslator();
        }

        public List<PaymentView> GetBy(Person person)
        {
            try
            {
                List<PaymentView> payments = (from p in repository.GetBy<VW_PAYMENT>(p => p.Person_Id == person.Id && p.Confirmation_No != null)
                                              select new PaymentView
                                              {
                                                  PersonId = p.Person_Id,
                                                  PaymentId = p.Payment_Id,
                                                  InvoiceNumber = p.Invoice_Number,
                                                  ReceiptNumber = p.Receipt_No,
                                                  ConfirmationOrderNumber = p.Confirmation_No,
                                                  BankCode = p.Bank_Code,
                                                  BankName = p.Bank_Name,
                                                  BranchCode = p.Branch_Code,
                                                  BranchName = p.Branch_Name,
                                                  PaymentDate = p.Transaction_Date,
                                                  FeeTypeId = p.Fee_Type_Id,
                                                  FeeTypeName = p.Fee_Type_Name,
                                                  PaymentTypeId = p.Payment_Type_Id,
                                                  PaymentTypeName = p.Payment_Type_Name,
                                                  Amount = p.Transaction_Amount
                                              }).ToList();
                return payments;

            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<PaymentView> GetEtranzactPaymentBy(Person person)
        {
            try
            {
                List<PaymentView> payments = (from p in repository.GetBy<VW_PAYMENT_ETRANZACT>(p => p.Person_Id == person.Id && p.Confirmation_No != null)
                                              select new PaymentView
                                              {
                                                  PersonId = p.Person_Id,
                                                  PaymentId = p.Payment_Id,
                                                  InvoiceNumber = p.Invoice_Number,
                                                  //ReceiptNumber = p.Receipt_No,
                                                  ConfirmationOrderNumber = p.Confirmation_No,
                                                  BankCode = p.Bank_Code,
                                                  BankName = p.Bank_Name,
                                                  BranchCode = p.Branch_Code,
                                                  BranchName = p.Branch_Name,
                                                  PaymentDate = p.Date_Paid,
                                                  FeeTypeId = p.Fee_Type_Id,
                                                  FeeTypeName = p.Fee_Type_Name,
                                                  PaymentTypeId = p.Payment_Type_Id,
                                                  PaymentTypeName = p.Payment_Type_Name,
                                                  Amount = p.Transaction_Amount,
                                                  SessionName = p.Session_Name
                                              }).ToList();
                return payments;

            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<PaymentView> GetBy(RemitaPayment remitaPayment)
        {
            try
            {
                List<PaymentView> payments = (from p in repository.GetBy<VW_REMITA_PAYMENT>(p => p.Person_Id == remitaPayment.payment.Person.Id)
                                              select new PaymentView
                                              {
                                                  PersonId = p.Person_Id,
                                                  PaymentId = p.Payment_Id,
                                                  InvoiceNumber = p.Invoice_Number,
                                                  ReceiptNumber = p.Invoice_Number,
                                                  ConfirmationOrderNumber = p.RRR,
                                                  BankCode = p.Bank_Code,
                                                  BankName = "",
                                                  BranchCode = p.Branch_Code,
                                                  BranchName = "",
                                                  PaymentDate = p.Transaction_Date,
                                                  FeeTypeId = p.Fee_Type_Id,
                                                  FeeTypeName = p.Fee_Type_Name,
                                                  PaymentTypeId = p.Payment_Type_Id,
                                                  PaymentTypeName = p.Payment_Type_Name,
                                                  Amount = p.Transaction_Amount,
                                              }).ToList();
                return payments;

            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<PaymentView> GetPaymentRemitaBy(Person person)
        {
            try
            {
                List<PaymentView> payments = (from p in repository.GetBy<VW_REMITA_PAYMENT>(p => p.Person_Id == person.Id)
                                              select new PaymentView
                                              {
                                                  PersonId = p.Person_Id,
                                                  PaymentId = p.Payment_Id,
                                                  InvoiceNumber = p.Invoice_Number,
                                                  ReceiptNumber = p.Invoice_Number,
                                                  ConfirmationOrderNumber = p.RRR,
                                                  BankCode = p.Bank_Code,
                                                  BankName = "",
                                                  BranchCode = p.Branch_Code,
                                                  BranchName = "",
                                                  PaymentDate = p.Date_Paid,
                                                  FeeTypeId = p.Fee_Type_Id,
                                                  FeeTypeName = p.Fee_Type_Name,
                                                  PaymentTypeId = p.Payment_Type_Id,
                                                  PaymentTypeName = p.Payment_Type_Name,
                                                  Amount = p.Transaction_Amount,
                                                  SessionName = p.Session_Name
                                              }).ToList();
                return payments;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<AcceptanceView> GetRemitaReportBy(Session session, Department department, Programme programme)
        {
            try
            {
                List<AcceptanceView> payments = (from p in repository.GetBy<ACCPTANCE__REPORT>(p => p.Department_Id == department.Id && p.Programme_Id == programme.Id && p.Session_Id == session.Id)
                                                 select new AcceptanceView
                                                 {
                                                     Person_Id = (long)p.Person_Id,
                                                     Application_Exam_Number = p.Application_Exam_Number,
                                                     Invoice_Number = p.Invoice_Number,
                                                     Application_Form_Number = p.Application_Form_Number,
                                                     First_Choice_Department_Name = p.Department_Name,
                                                     Name = p.SURNAME + " " + p.FIRSTNAME + " " + p.OTHER_NAMES,
                                                     RRR = p.Department_Option_Name,
                                                     Programme_Name = p.Programme_Name,
                                                     DepartmentOption = p.Department_Option_Name
                                                 }).Distinct().OrderBy(b => b.Name).ToList();
                return payments;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public Payment GetBy(Person person, FeeType feeType)
        {
            try
            {
                Expression<Func<PAYMENT, bool>> selector = p => p.Person_Id == person.Id && p.Fee_Type_Id == feeType.Id;
                Payment payment = GetModelsBy(selector).FirstOrDefault();

                SetFeeDetails(payment);

                return payment;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Payment GetBy(long id)
        {
            try
            {
                Expression<Func<PAYMENT, bool>> selector = p => p.Payment_Id == id;
                Payment payment = GetModelBy(selector);

                SetFeeDetails(payment);

                return payment;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Payment GetBy(FeeType feeType, Person person, Session session)
        {
            try
            {
                Expression<Func<PAYMENT, bool>> selector = p => p.Fee_Type_Id == feeType.Id && p.Person_Id == person.Id && p.Session_Id == session.Id;
                Payment payment = GetModelsBy(selector).FirstOrDefault();

                SetFeeDetails(payment);

                return payment;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Payment DistantLearningGetBy(FeeType feeType, Person person, Session session, PaymentMode paymentMode)
        {
            try
            {
                Expression<Func<PAYMENT, bool>> selector = p => p.Fee_Type_Id == feeType.Id && p.Person_Id == person.Id && p.Session_Id == session.Id && p.Payment_Mode_Id == paymentMode.Id;
                Payment payment = GetModelsBy(selector).FirstOrDefault();

                SetFeeDetails(payment);

                return payment;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool ResolveInstallmentPaymentOrder(FeeType feeType, Person person, Session session, PaymentMode paymentMode)
        {
            try
            {
                Expression<Func<PAYMENT, bool>> selector = p => p.Fee_Type_Id == feeType.Id && p.Person_Id == person.Id && p.Session_Id == session.Id && p.Payment_Mode_Id == paymentMode.Id;
                Payment payment = GetModelsBy(selector).FirstOrDefault();
                if (paymentMode.Id == (int)PaymentModes.Full)
                {
                    RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                    //paymentMode.Id = (int)PaymentModes.Full;

                    //If specified payment mode is Full payment, Check if a person has prior generated and paid in installments
                    var doesInstallmentPaymentExists = remitaPaymentLogic.GetModelsBy(x => x.PAYMENT.Fee_Type_Id == feeType.Id && x.PAYMENT.Person_Id == person.Id && x.PAYMENT.Session_Id == session.Id && (x.Description.Contains("01") || x.Description.Contains("manual")) && 
                    (x.PAYMENT.Payment_Mode_Id == (int)PaymentModes.FirstInstallment || x.PAYMENT.Payment_Mode_Id == (int)PaymentModes.SecondInstallment || x.PAYMENT.Payment_Mode_Id == (int)PaymentModes.ThirdInstallment)).ToList();
                    
                    if (doesInstallmentPaymentExists != null && doesInstallmentPaymentExists.Count > 0)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                if (paymentMode.Id == (int)PaymentModes.SecondInstallment)
                {
                    RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                    //paymentMode.Id = (int)PaymentModes.FirstInstallment;

                    var isFirstInstallmentPaid = remitaPaymentLogic.GetModelsBy(x => x.PAYMENT.Fee_Type_Id == feeType.Id && x.PAYMENT.Person_Id == person.Id && x.PAYMENT.Session_Id == session.Id && x.PAYMENT.Payment_Mode_Id == (int)PaymentModes.FirstInstallment && (x.Description.Contains("01") || x.Description.Contains("manual"))).LastOrDefault();
                    if (isFirstInstallmentPaid != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (paymentMode.Id == (int)PaymentModes.ThirdInstallment)
                {
                    RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                    //paymentMode.Id = (int)PaymentModes.SecondInstallment;

                    var isSecondInstallmentPaid = remitaPaymentLogic.GetModelsBy(x => x.PAYMENT.Fee_Type_Id == feeType.Id && x.PAYMENT.Person_Id == person.Id && x.PAYMENT.Session_Id == session.Id && x.PAYMENT.Payment_Mode_Id == (int)PaymentModes.SecondInstallment && (x.Description.Contains("01") || x.Description.Contains("manual"))).LastOrDefault();
                    if (isSecondInstallmentPaid != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void SetFeeDetails(Payment payment)
        {
            try
            {
                if (payment != null && payment.Id > 0)
                {
                    payment.FeeDetails = new List<FeeDetail>();
                    payment.FeeDetails.Add(feeDetailLogic.GetModelsBy(f => f.Fee_Type_Id == payment.FeeType.Id && f.Session_Id == payment.Session.Id).FirstOrDefault());

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void SetFeeDetails(Payment payment, Int32? ProgrammeId, Int32? DepartmentId, Int32? SessionId)
        {
            try
            {
                if (payment != null && payment.Id > 0)
                {
                    payment.FeeDetails = feeDetailLogic.GetModelsBy(f => f.Fee_Type_Id == 9 && f.Programme_Id == ProgrammeId && f.Department_Id == DepartmentId && f.Session_Id == SessionId);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<FeeDetail> SetFeeDetails(Payment payment, Int32? ProgrammeId, Int32? LevelId, Int32? PaymentModeId, Int32? DepartmentId, Int32? SessionId)
        {
            List<FeeDetail> feedetail = new List<FeeDetail>();
            try
            {
                if (payment != null && payment.Id > 0)
                {
                    feedetail = feeDetailLogic.GetModelsBy(f => f.Fee_Type_Id == payment.FeeType.Id && f.Programme_Id == ProgrammeId && f.Level_Id == LevelId && f.Payment_Mode_Id == PaymentModeId && f.Department_Id == DepartmentId && f.Session_Id == SessionId);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return feedetail;
        }
        public List<FeeDetail> SetFeeDetails(Payment payment, Int32? ProgrammeId, Int32? LevelId, Int32? PaymentModeId, Int32? DepartmentId, Int32? SessionId, Int32? FeeId)
        {
            List<FeeDetail> feedetail = new List<FeeDetail>();
            try
            {
                if (payment != null && payment.Id > 0)
                {
                    feedetail = feeDetailLogic.GetModelsBy(f => f.Fee_Type_Id == payment.FeeType.Id && f.Programme_Id == ProgrammeId && f.Level_Id == LevelId && f.Payment_Mode_Id == PaymentModeId &&
                                                            f.Department_Id == DepartmentId && f.Session_Id == SessionId && f.Fee_Id == FeeId);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return feedetail;
        }

        public List<FeeDetail> SetFeeDetails(FeeType feeType)
        {
            List<FeeDetail> feedetail = new List<FeeDetail>();
            try
            {
                if (feeType != null && feeType.Id > 0)
                {

                    feedetail.Add(feeDetailLogic.GetModelsBy(f => f.Fee_Type_Id == feeType.Id).FirstOrDefault());
                }
            }
            catch (Exception)
            {
                throw;
            }
            return feedetail;
        }

        public List<FeeDetail> SetFeeDetails(long FeeId)
        {
            List<FeeDetail> feedetail = new List<FeeDetail>();
            try
            {
                if (FeeId > 0)
                {
                    feedetail = feeDetailLogic.GetModelsBy(f => f.Fee_Id == FeeId);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return feedetail;
        }

        public Payment GetBy(string invoiceNumber)
        {
            try
            {
                Expression<Func<PAYMENT, bool>> selector = p => p.Invoice_Number == invoiceNumber;
                Payment payment = GetModelBy(selector);

                AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                StudentLevel studentLevel = new StudentLevel();
                PaymentMode paymentMode = new PaymentMode() { Id = 1 };

                studentLevel = studentLevelLogic.GetModelsBy(s => s.Person_Id == payment.Person.Id && s.Session_Id == payment.Session.Id).LastOrDefault();
                if (studentLevel == null)
                {
                    studentLevel = studentLevelLogic.GetModelsBy(s => s.Person_Id == payment.Person.Id).LastOrDefault();
                }

                AppliedCourse appliedCourse = appliedCourseLogic.GetModelBy(a => a.Person_Id == payment.Person.Id);
                Session session = payment.Session;

                if (studentLevel != null)
                {

                    if (studentLevel != null)
                    {
                        payment.FeeDetails = SetFeeDetails(payment, studentLevel.Programme.Id, studentLevel.Level.Id, paymentMode.Id, studentLevel.Department.Id, payment.Session.Id);
                        //if (studentLevel.Programme.Id == 2 || studentLevel.Programme.Id == 3)
                        //{
                        //    SetFeeDetails(payment, studentLevel.Programme.Id, studentLevel.Department.Id, session.Id);

                        //}
                        //else
                        //{
                        //    SetFeeDetails(payment);
                        //}
                    }

                }
                else if (appliedCourse != null)
                {
                    Int32 levelId = 1;
                    if (appliedCourse.Programme.Id == 3)
                    {
                        levelId = 3;
                    }
                    if (payment.FeeType.Id == (int)FeeTypes.SchoolFees && (appliedCourse.Programme.Id == (int)Programmes.HNDDistance || appliedCourse.Programme.Id == (int)Programmes.NDDistance))
                    {
                        payment.FeeDetails = SetFeeDetails(payment, appliedCourse.Programme.Id, levelId, payment.PaymentMode.Id, appliedCourse.Department.Id, payment.Session.Id);
                    }
                    else
                    {
                        payment.FeeDetails = SetFeeDetails(payment, appliedCourse.Programme.Id, levelId, paymentMode.Id, appliedCourse.Department.Id, payment.Session.Id);
                    }
                   

                    
                    //if (appliedCourse.Programme.Id == 2 || appliedCourse.Programme.Id == 3)
                    //{
                    //    SetFeeDetails(payment, appliedCourse.Programme.Id, appliedCourse.Department.Id, session.Id);
                    //}
                    //else
                    //{
                    //    SetFeeDetails(payment);
                    //} 
                }
                else
                {
                    SetFeeDetails(payment);
                }

                return payment;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool PaymentAlreadyMade(Payment payment)
        {
            try
            {
                Expression<Func<PAYMENT, bool>> selector = p => p.Fee_Type_Id == payment.FeeType.Id && p.Payment_Mode_Id == payment.PaymentMode.Id && p.Person_Id == payment.Person.Id && p.Session_Id == payment.Session.Id;
                List<Payment> payments = GetModelsBy(selector);
                if (payments != null && payments.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool SetInvoiceNumber(Payment payment)
        {
            try
            {
                Expression<Func<PAYMENT, bool>> selector = p => p.Payment_Id == payment.Id;
                PAYMENT entity = base.GetEntityBy(selector);

                if (entity == null)
                {
                    throw new Exception(NoItemFound);
                }

                entity.Payment_Serial_Number = payment.SerialNumber;
                entity.Invoice_Number = payment.InvoiceNumber;

                int modifiedRecordCount = Save();
                if (modifiedRecordCount <= 0)
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override Payment Create(Payment payment)
        {
            try
            {
                Payment newPayment = base.Create(payment);
                if (newPayment == null || newPayment.Id <= 0)
                {
                    throw new Exception("Payment ID not set!");
                }

                newPayment = SetNextPaymentNumber(newPayment);
                SetInvoiceNumber(newPayment);
                newPayment.FeeType = payment.FeeType;
                if (newPayment.Session == null)
                    newPayment.Session = payment.Session;
                SetFeeDetails(newPayment);

                return newPayment;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Payment SetNextPaymentNumber(Payment payment)
        {
            try
            {
                payment.SerialNumber = payment.Id;
                payment.InvoiceNumber = "FPI" + DateTime.Now.ToString("yy") + PaddNumber(payment.Id, 10);

                return payment;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string PaddNumber(long id, int maxCount)
        {
            try
            {
                string idInString = id.ToString();
                string paddNumbers = "";
                if (idInString.Count() < maxCount)
                {
                    int zeroCount = maxCount - id.ToString().Count();
                    StringBuilder builder = new StringBuilder();
                    for (int counter = 0; counter < zeroCount; counter++)
                    {
                        builder.Append("0");
                    }

                    builder.Append(id);
                    paddNumbers = builder.ToString();
                    return paddNumbers;
                }

                return paddNumbers;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool InvalidConfirmationOrderNumber(string invoiceNo, string confirmationOrderNo)
        {
            try
            {
                List<PaymentEtranzact> payments = (from p in repository.GetBy<VW_PAYMENT>(p => p.Invoice_Number == invoiceNo)
                                                   select new PaymentEtranzact
                                                   {
                                                       ConfirmationNo = p.Confirmation_No,
                                                   }).ToList();

                if (payments != null)
                {
                    if (payments.Count > 1)
                    {
                        throw new Exception("Duplicate Invoice Number '" + invoiceNo + "' detected! Please contact your system administrator.");
                    }
                    else if (payments.Count == 1)
                    {
                        if (payments[0].ConfirmationNo == confirmationOrderNo)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Payment InvalidConfirmationOrderNumber(string confirmationOrderNumber, int feeType)
        {
            try
            {
                Payment payment = new Payment();
                PaymentEtranzactLogic etranzactLogic = new PaymentEtranzactLogic();
                PaymentEtranzact etranzactDetails = etranzactLogic.GetModelBy(m => m.Confirmation_No == confirmationOrderNumber);
                if (etranzactDetails == null || etranzactDetails.ReceiptNo == null)
                {
                    PaymentTerminal paymentTerminal = new PaymentTerminal();
                    PaymentTerminalLogic paymentTerminalLogic = new PaymentTerminalLogic();
                    paymentTerminal = paymentTerminalLogic.GetModelBy(p => p.Fee_Type_Id == feeType && p.Session_Id == 7);

                    etranzactDetails = etranzactLogic.RetrievePinAlternative(confirmationOrderNumber, paymentTerminal);
                    if (etranzactDetails != null && etranzactDetails.ReceiptNo != null)
                    {
                        PaymentLogic paymentLogic = new PaymentLogic();
                        payment = paymentLogic.GetModelBy(m => m.Invoice_Number == etranzactDetails.CustomerID);
                        if (payment != null && payment.Id > 0)
                        {
                            decimal amountToPay = 0M;
                            if (payment.FeeType.Id == (int)FeeTypes.HostelFee)
                            {
                                HostelFeeLogic hostelFeeLogic = new HostelFeeLogic();
                                HostelFee hostelFee = hostelFeeLogic.GetModelBy(h => h.Payment_Id == payment.Id);
                                amountToPay = Convert.ToDecimal(hostelFee.Amount);
                            }
                            else if (payment.FeeType.Id == (int)FeeTypes.ChangeOfCourseFees)
                            {
                                AppliedCourse appliedCourse = new AppliedCourse();
                                AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
                                appliedCourse = appliedCourseLogic.GetBy(payment.Person);

                                if (appliedCourse != null)
                                {
                                    Level level = new Level();
                                    level = SetLevel(appliedCourse.Programme);
                                    payment.FeeDetails = paymentLogic.SetFeeDetails(payment, appliedCourse.Programme.Id, level.Id, 1, appliedCourse.Department.Id, appliedCourse.ApplicationForm.Setting.Session.Id);

                                    amountToPay = payment.FeeDetails.Sum(p => p.Fee.Amount);
                                }
                            }
                            else if (payment.FeeType.Id == (int)FeeTypes.ShortFall)
                            {
                                ShortFallLogic shortFallLogic = new ShortFallLogic();
                                ShortFall shortFall = shortFallLogic.GetModelBy(h => h.Payment_Id == payment.Id);
                                if (shortFall != null)
                                {
                                    amountToPay = Convert.ToDecimal(shortFall.Amount);
                                }
                            }
                            else if (payment.FeeType.Id == (int)FeeTypes.ConvocationFee)
                            {
                                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                                StudentLevel studentLevel = studentLevelLogic.GetModelsBy(s => s.Person_Id == payment.Person.Id && s.Session_Id == payment.Session.Id).LastOrDefault();
                                if (studentLevel == null)
                                {
                                    throw new Exception("No student level record for the paid session");
                                }

                                payment.FeeDetails = paymentLogic.SetFeeDetails(payment, studentLevel.Programme.Id, studentLevel.Level.Id, 1, studentLevel.Department.Id, payment.Session.Id);

                                amountToPay = payment.FeeDetails.Sum(p => p.Fee.Amount);
                            }
                            else
                            {
                                FeeDetail feeDetail = new FeeDetail();
                                FeeDetailLogic feeDetailLogic = new FeeDetailLogic();
                                feeDetail = feeDetailLogic.GetModelBy(a => a.Fee_Type_Id == payment.FeeType.Id);

                                amountToPay = feeDetail.Fee.Amount;
                            }

                            if (!etranzactLogic.ValidatePin(etranzactDetails, payment, amountToPay))
                            {
                                throw new Exception("The pin amount tied to the pin is not correct. Please contact support@lloydant.com.");
                            }
                        }
                        else
                        {
                            throw new Exception("The invoice number attached to the pin doesn't belong to you! Please cross check and try again.");
                        }
                    }
                    else
                    {
                        throw new Exception("Confirmation Order Number entered seems not to be valid! Please cross check and try again.");
                    }
                }
                else
                {
                    PaymentLogic paymentLogic = new PaymentLogic();
                    payment = paymentLogic.GetModelBy(m => m.Invoice_Number == etranzactDetails.CustomerID);
                    if (payment != null && payment.Id > 0)
                    {
                        decimal amountToPay = 0M;
                        if (payment.FeeType.Id == (int)FeeTypes.HostelFee)
                        {
                            HostelFeeLogic hostelFeeLogic = new HostelFeeLogic();
                            HostelFee hostelFee = hostelFeeLogic.GetModelBy(h => h.Payment_Id == payment.Id);
                            amountToPay = Convert.ToDecimal(hostelFee.Amount);
                        }
                        else if (payment.FeeType.Id == (int)FeeTypes.ChangeOfCourseFees)
                        {
                            AppliedCourse appliedCourse = new AppliedCourse();
                            AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
                            appliedCourse = appliedCourseLogic.GetBy(payment.Person);

                            if (appliedCourse != null)
                            {
                                Level level = new Level();
                                level = SetLevel(appliedCourse.Programme);
                                payment.FeeDetails = paymentLogic.SetFeeDetails(payment, appliedCourse.Programme.Id, level.Id, 1, appliedCourse.Department.Id, appliedCourse.ApplicationForm.Setting.Session.Id);

                                amountToPay = payment.FeeDetails.Sum(p => p.Fee.Amount);
                            }
                        }
                        else if (payment.FeeType.Id == (int)FeeTypes.ShortFall)
                        {
                            ShortFallLogic shortFallLogic = new ShortFallLogic();
                            ShortFall shortFall = shortFallLogic.GetModelBy(h => h.Payment_Id == payment.Id);
                            if (shortFall != null)
                            {
                                amountToPay = Convert.ToDecimal(shortFall.Amount);
                            }
                        }
                        else if (payment.FeeType.Id == (int)FeeTypes.ConvocationFee)
                        {
                            StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                            StudentLevel studentLevel = studentLevelLogic.GetModelsBy(s => s.Person_Id == payment.Person.Id && s.Session_Id == payment.Session.Id).LastOrDefault();
                            if (studentLevel == null)
                            {
                                throw new Exception("No student level record for the paid session");
                            }

                            payment.FeeDetails = paymentLogic.SetFeeDetails(payment, studentLevel.Programme.Id, studentLevel.Level.Id, 1, studentLevel.Department.Id, payment.Session.Id);

                            amountToPay = payment.FeeDetails.Sum(p => p.Fee.Amount);
                        }
                        else
                        {
                            List<FeeDetail> feeDetails = new List<FeeDetail>();
                            FeeDetailLogic feeDetailLogic = new FeeDetailLogic();
                            feeDetails = feeDetailLogic.GetModelsBy(a => a.Fee_Type_Id == payment.FeeType.Id);

                            amountToPay = feeDetails.Sum(a => a.Fee.Amount); ;
                        }
                        if (!etranzactLogic.ValidatePin(etranzactDetails, payment, amountToPay))
                        {
                            throw new Exception("The pin amount tied to the pin is not correct. Please contact support@lloydant.com.");
                        }
                    }
                    else
                    {
                        throw new Exception("The invoice number attached to the pin doesn't belong to you! Please cross check and try again.");
                    }
                }

                return payment;
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
        public Payment InvalidConfirmationOrderNumber(string confirmationOrderNumber, string ivn, int feeType)
        {
            try
            {
                Payment payment = new Payment();
                PaymentEtranzactLogic etranzactLogic = new PaymentEtranzactLogic();
                PaymentEtranzact etranzactDetails = etranzactLogic.GetModelBy(m => m.Confirmation_No == confirmationOrderNumber);
                if (etranzactDetails == null || etranzactDetails.ReceiptNo == null)
                {
                    PaymentTerminal paymentTerminal = new PaymentTerminal();
                    PaymentTerminalLogic paymentTerminalLogic = new PaymentTerminalLogic();
                    paymentTerminal = paymentTerminalLogic.GetModelBy(p => p.Fee_Type_Id == feeType && p.Session_Id == 1);

                    etranzactDetails = etranzactLogic.RetrievePinsWithoutInvoice(confirmationOrderNumber, ivn, feeType, paymentTerminal);
                    if (etranzactDetails != null && etranzactDetails.ReceiptNo != null)
                    {
                        PaymentLogic paymentLogic = new PaymentLogic();
                        payment = paymentLogic.GetModelBy(m => m.Invoice_Number == etranzactDetails.CustomerID);
                        if (payment != null && payment.Id > 0)
                        {
                            FeeDetail feeDetail = new FeeDetail();
                            FeeDetailLogic feeDetailLogic = new FeeDetailLogic();
                            feeDetail = feeDetailLogic.GetModelBy(a => a.Fee_Type_Id == payment.FeeType.Id);
                            if (!etranzactLogic.ValidatePin(etranzactDetails, payment, feeDetail.Fee.Amount))
                            {
                                throw new Exception("The pin amount tied to the pin is not correct. Please contact support@lloydant.com.");

                            }
                        }
                        else
                        {
                            throw new Exception("The invoice number attached to the pin doesn't belong to you! Please cross check and try again.");
                        }
                    }
                    else
                    {
                        throw new Exception("Confirmation Order Number entered seems not to be valid! Please cross check and try again.");
                    }
                }
                else
                {
                    PaymentLogic paymentLogic = new PaymentLogic();
                    payment = paymentLogic.GetModelBy(m => m.Invoice_Number == etranzactDetails.CustomerID);
                    if (payment != null && payment.Id > 0)
                    {
                        //FeeDetail feeDetail = new FeeDetail();
                        FeeDetailLogic feeDetailLogic = new FeeDetailLogic();
                        //feeDetail = feeDetailLogic.GetModelBy(a => a.Fee_Type_Id == payment.FeeType.Id);

                        List<FeeDetail> feeDetails = feeDetailLogic.GetModelsBy(a => a.Fee_Type_Id == payment.FeeType.Id);
                        decimal amount = feeDetails.Sum(a => a.Fee.Amount);
                        if (!etranzactLogic.ValidatePin(etranzactDetails, payment, amount))
                        {
                            throw new Exception("The pin amount tied to the pin is not correct. Please contact support@lloydant.com.");
                            //payment = null;
                            //return payment;
                        }
                    }
                    else
                    {
                        throw new Exception("The invoice number attached to the pin doesn't belong to you! Please cross check and try again.");
                    }
                }

                return payment;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Payment InvalidConfirmationOrderNumber(string confirmationOrderNumber, Session session, FeeType feetype)
        {
            try
            {
                Payment payment = new Payment();
                PaymentEtranzactLogic etranzactLogic = new PaymentEtranzactLogic();
                PaymentEtranzact etranzactDetails = etranzactLogic.GetModelBy(m => m.Confirmation_No == confirmationOrderNumber);
                if (etranzactDetails == null || etranzactDetails.ReceiptNo == null)
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

                    if (remitaPayment != null)
                    {
                        //remitaPayment = !remitaPayment.Status.Contains("01") ? r.GetStatus(remitaPayment.OrderId) : remitaPayment;
                        //remitaPayment.TransactionAmount == 2500 &
                        if (remitaPayment.Status.Contains("01") || remitaPayment.Status.Contains("00") || remitaPayment.Description.Contains("manual"))
                        {
                            FeeDetailLogic feeDetailLogic = new FeeDetailLogic();

                            StudentLevel studentLevel = new StudentLevel();
                            StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                            studentLevel = studentLevelLogic.GetBy(remitaPayment.payment.Person.Id);

                            AdmissionList admissionList = new AdmissionList();
                            AdmissionListLogic admissionLogic = new AdmissionListLogic();
                            admissionList = admissionLogic.GetBy(remitaPayment.payment.Person);

                            if (studentLevel != null)
                            {
                                decimal AmountToPay = feeDetailLogic.GetFeeByDepartmentLevel(studentLevel.Department, studentLevel.Level, studentLevel.Programme, remitaPayment.payment.FeeType,
                                                                                                remitaPayment.payment.Session, remitaPayment.payment.PaymentMode);

                                //Check to resolve for cisco/robotics
                                if (AmountToPay > remitaPayment.TransactionAmount)
                                {
                                    AmountToPay = ResolveAmountPaidAndExpexctedAmountDisparity(remitaPayment.TransactionAmount, AmountToPay);
                                }
                                if (remitaPayment.TransactionAmount < AmountToPay)
                                {
                                    if (CheckShortFallRemita(remitaPayment, AmountToPay))
                                    {
                                        //paid shortfall
                                    }
                                    else
                                    {
                                        throw new Exception("The pin amount tied to the pin is not correct. Please contact support@lloydant.com.");
                                    }
                                }

                            }
                            else
                            {
                                if (admissionList != null)
                                {
                                    Level level;
                                    if (admissionList.Form.ProgrammeFee.Programme.Id == (int)Programmes.NDFullTime || admissionList.Form.ProgrammeFee.Programme.Id == (int)Programmes.NDPartTime || admissionList.Form.ProgrammeFee.Programme.Id == (int)Programmes.DrivingCertificate || admissionList.Form.ProgrammeFee.Programme.Id == (int)Programmes.NDDistance)
                                    {
                                        level = new Level() { Id = 1 };
                                    }
                                    else
                                    {
                                        level = new Level() { Id = 3 };
                                    }

                                  
                                    decimal AmountToPay = feeDetailLogic.GetFeeByDepartmentLevel(admissionList.Deprtment, level, admissionList.Form.ProgrammeFee.Programme, remitaPayment.payment.FeeType,
                                                                                                    remitaPayment.payment.Session, remitaPayment.payment.PaymentMode);


                                    //if (remitaPayment.payment.FeeType.Id == (int)FeeTypes.SchoolFees && admissionList.Form.ProgrammeFee.Programme.Id != (int)Programmes.HNDDistance && admissionList.Form.ProgrammeFee.Programme.Id != (int)Programmes.NDDistance)
                                    //{
                                    //    FeeinPaymentLogic feeinPaymentLogic = new FeeinPaymentLogic();
                                    //    //FeeDetailLogic feeDetailLogic = new FeeDetailLogic();
                                    //    var feeinPayments = feeinPaymentLogic.GetModelsBy(f => f.Payment_Id == remitaPayment.payment.Id);
                                    //    foreach (var item in feeinPayments)
                                    //    {
                                    //        if (item.IsIncluded)
                                    //            payment.FeeDetails = feeDetailLogic.AlterFeeDetailRecords(item.Fee.Id, false, payment.FeeDetails);
                                    //        else
                                    //            payment.FeeDetails = feeDetailLogic.AlterFeeDetailRecords(item.Fee.Id, true, payment.FeeDetails);
                                    //    }

                                    //    AmountToPay = payment.FeeDetails.Sum(f => f.Fee.Amount);
                                    //}
                                    if(AmountToPay == 0 || AmountToPay > remitaPayment.TransactionAmount)
                                    {
                                        AmountToPay = feeDetailLogic.GetFeeByDepartmentLevel(admissionList.Deprtment, level, admissionList.Form.ProgrammeFee.Programme, remitaPayment.payment.FeeType,remitaPayment.payment.Session, remitaPayment.payment.PaymentMode);

                                        AmountToPay = ResolveAmountPaidAndExpexctedAmountDisparity(remitaPayment.TransactionAmount, AmountToPay);
                                    }

                                    if (admissionList.Deprtment.Id == 44)
                                    {
                                        if (feetype?.Id > 0 && feetype.Id == 26)
                                        {
                                            AmountToPay = Convert.ToDecimal(Fees.AllDrivingAcceptance);
                                        }
                                        else
                                        {
                                            if (admissionList.DepartmentOption.Id == 16)
                                            {
                                                AmountToPay = Convert.ToDecimal(Fees.GraduateDrivingTrainingSchoolFees);
                                            }
                                            else if (admissionList.DepartmentOption.Id == 17)
                                            {
                                                AmountToPay = Convert.ToDecimal(Fees.TechnicalCertificateDrivingSchooFees);
                                            }
                                            else if (admissionList.DepartmentOption.Id == 18)
                                            {
                                                AmountToPay = Convert.ToDecimal(Fees.ProfessionalDiplomaDrivingSchooFees);
                                            }
                                        }
                                    }
                                    if (remitaPayment.TransactionAmount < AmountToPay)
                                    {
                                        if (CheckShortFallRemita(remitaPayment, AmountToPay))
                                        {
                                            //paid shortfall
                                        }
                                        else
                                        {
                                            throw new Exception("The pin amount tied to the pin is not correct. Please contact support@lloydant.com.");
                                        }
                                    }
                                }
                            }

                            payment = remitaPayment.payment;
                            return payment;
                        }
                        else
                        {
                            throw new Exception("Payment could not be verified, Try again in a few minutes");
                        }
                    }

                    PaymentTerminal paymentTerminal = new PaymentTerminal();
                    PaymentTerminalLogic paymentTerminalLogic = new PaymentTerminalLogic();
                    paymentTerminal = paymentTerminalLogic.GetModelsBy(p => p.Fee_Type_Id == feetype.Id && p.Session_Id == session.Id).FirstOrDefault();

                    etranzactDetails = etranzactLogic.RetrievePinAlternative(confirmationOrderNumber, paymentTerminal);
                    if (etranzactDetails != null && etranzactDetails.ReceiptNo != null)
                    {
                        PaymentLogic paymentLogic = new PaymentLogic();
                        payment = paymentLogic.GetModelBy(m => m.Invoice_Number == etranzactDetails.CustomerID);
                        if (payment != null && payment.Id > 0)
                        {
                            List<FeeDetail> feeDetail = new List<FeeDetail>();
                            FeeDetailLogic feeDetailLogic = new FeeDetailLogic();


                            StudentLevel studentLevel = new StudentLevel();
                            StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                            studentLevel = studentLevelLogic.GetBy(payment.Person.Id);

                            AdmissionList admissionList = new AdmissionList();
                            AdmissionListLogic admissionLogic = new AdmissionListLogic();
                            admissionList = admissionLogic.GetBy(payment.Person);

                            if (studentLevel != null)
                            {

                                if (studentLevel != null)
                                {
                                    decimal AmountToPay = feeDetailLogic.GetFeeByDepartmentLevel(studentLevel.Department, studentLevel.Level, studentLevel.Programme, payment.FeeType, payment.Session, payment.PaymentMode);
                                    if (!etranzactLogic.ValidatePin(etranzactDetails, payment, AmountToPay))
                                    {
                                        if (CheckShortFall(etranzactDetails, payment, AmountToPay))
                                        {
                                            //paid shortfall
                                        }
                                        else
                                        {
                                            throw new Exception("The pin amount tied to the pin is not correct. Please contact support@lloydant.com.");
                                        }
                                    }
                                }
                                else
                                {
                                    throw new Exception("Cannot retrieve amount for your department. Please contact support@lloydant.com.");

                                }

                            }
                            else
                            {
                                if (admissionList != null)
                                {
                                    Level level;
                                    if (admissionList.Form.ProgrammeFee.Programme.Id == 1 || admissionList.Form.ProgrammeFee.Programme.Id == 2 || admissionList.Form.ProgrammeFee.Programme.Id == 5)
                                    {
                                        level = new Level() { Id = 1 };
                                    }
                                    else
                                    {
                                        level = new Level() { Id = 3 };
                                    }

                                    decimal AmountToPay = feeDetailLogic.GetFeeByDepartmentLevel(admissionList.Deprtment, level, admissionList.Form.ProgrammeFee.Programme, payment.FeeType, payment.Session, payment.PaymentMode);
                                    if (!etranzactLogic.ValidatePin(etranzactDetails, payment, AmountToPay))
                                    {
                                        if (CheckShortFall(etranzactDetails, payment, AmountToPay))
                                        {
                                            //paid shortfall
                                        }
                                        else
                                        {
                                            throw new Exception("The pin amount tied to the pin is not correct. Please contact support@lloydant.com.");
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            throw new Exception("The invoice number attached to the pin doesn't belong to you! Please cross check and try again.");
                        }
                    }
                    else
                    {
                        throw new Exception("Confirmation Order Number entered seems not to be valid! Please cross check and try again.");
                    }
                }
                else
                {
                    PaymentLogic paymentLogic = new PaymentLogic();
                    payment = paymentLogic.GetModelBy(m => m.Invoice_Number == etranzactDetails.CustomerID);
                    if (payment != null && payment.Id > 0)
                    {
                        List<FeeDetail> feeDetail = new List<FeeDetail>();
                        FeeDetailLogic feeDetailLogic = new FeeDetailLogic();

                        if (payment.FeeType.Id == (int)FeeTypes.SchoolFees || payment.FeeType.Id == (int)FeeTypes.CarryOverSchoolFees)
                        {

                            AdmissionList admissionList = new AdmissionList();
                            AdmissionListLogic admissionLogic = new AdmissionListLogic();
                            admissionList = admissionLogic.GetBy(payment.Person);

                            if (admissionList != null)
                            {
                                Level level;
                                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                                StudentLevel currentStudentLevel = studentLevelLogic.GetModelsBy(sl => sl.Person_Id == payment.Person.Id && sl.Session_Id == payment.Session.Id).LastOrDefault();
                                if (currentStudentLevel != null)
                                {
                                    level = currentStudentLevel.Level;
                                }
                                else if (admissionList.Session.Id != payment.Session.Id)
                                {
                                    if (admissionList.Form.ProgrammeFee.Programme.Id == 1 || admissionList.Form.ProgrammeFee.Programme.Id == 2 || admissionList.Form.ProgrammeFee.Programme.Id == 5)
                                    {
                                        level = new Level() { Id = 2 };
                                    }
                                    else
                                    {
                                        level = new Level() { Id = 4 };
                                    }
                                }
                                else
                                {
                                    if (admissionList.Form.ProgrammeFee.Programme.Id == 1 || admissionList.Form.ProgrammeFee.Programme.Id == 2 || admissionList.Form.ProgrammeFee.Programme.Id == 5)
                                    {
                                        level = new Level() { Id = 1 };
                                    }
                                    else
                                    {
                                        level = new Level() { Id = 3 };
                                    }
                                }

                                decimal AmountToPay = 0M;

                                if (payment.FeeType.Id == (int)FeeTypes.CarryOverSchoolFees)
                                {
                                    StudentExtraYearSession extraYear = new StudentExtraYearSession();
                                    StudentExtraYearLogic extraYearLogic = new StudentExtraYearLogic();
                                    extraYear = extraYearLogic.GetBy(payment.Person.Id, payment.Session.Id);

                                    if (extraYear != null)
                                    {
                                        int lastSession =
                                            Convert.ToInt32(extraYear.LastSessionRegistered.Name.Substring(0, 4));
                                        int currentSession = Convert.ToInt32(payment.Session.Name.Substring(0, 4));
                                        int NoOfOutstandingSession = currentSession - lastSession;
                                        if (NoOfOutstandingSession == 0)
                                        {
                                            NoOfOutstandingSession = 1;
                                        }

                                        AmountToPay = feeDetailLogic.GetFeeByDepartmentLevel(admissionList.Deprtment, level, admissionList.Form.ProgrammeFee.Programme, payment.FeeType, payment.Session, payment.PaymentMode) * NoOfOutstandingSession;
                                    }
                                }
                                else
                                {
                                    AmountToPay = feeDetailLogic.GetFeeByDepartmentLevel(admissionList.Deprtment, level, admissionList.Form.ProgrammeFee.Programme, payment.FeeType, payment.Session, payment.PaymentMode);
                                }

                                if (!etranzactLogic.ValidatePin(etranzactDetails, payment, AmountToPay))
                                {
                                    if (CheckShortFall(etranzactDetails, payment, AmountToPay))
                                    {
                                        //paid shortfall
                                    }
                                    else
                                    {
                                        throw new Exception("The pin amount tied to the pin is not correct. Please contact support@lloydant.com.");
                                    }
                                }
                            }
                            else
                            {
                                StudentLevel studentLevel = new StudentLevel();
                                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();

                                //studentLevel = studentLevelLogic.GetBy(payment.Person.Id);
                                //studentLevel = studentLevelLogic.GetExtraYearBy(payment.Person.Id);
                                studentLevel = studentLevelLogic.GetModelsBy(s => s.Session_Id == payment.Session.Id && s.Person_Id == payment.Person.Id).LastOrDefault();
                                if (studentLevel != null)
                                {
                                    //decimal AmountToPay = feeDetailLogic.GetFeeByDepartmentLevel(studentLevel.Department, studentLevel.Level, studentLevel.Programme, payment.FeeType, payment.Session, payment.PaymentMode);
                                    //if (!etranzactLogic.ValidatePin(etranzactDetails, payment, AmountToPay))
                                    //{
                                    //    throw new Exception("The pin amount tied to the pin is not correct. Please contact support@lloydant.com.");

                                    //}
                                }
                                else
                                {
                                    throw new Exception("Cannot retrieve amount for your department. Please contact support@lloydant.com.");

                                }

                            }
                        }
                        else if (payment.FeeType.Id == (int)FeeTypes.AcceptanceFee || payment.FeeType.Id == (int)FeeTypes.HNDAcceptance)
                        {
                            AdmissionList admissionList = new AdmissionList();
                            AdmissionListLogic admissionLogic = new AdmissionListLogic();
                            admissionList = admissionLogic.GetBy(payment.Person);



                            if (admissionList != null)
                            {
                                Level level;
                                if (admissionList.Form.ProgrammeFee.Programme.Id == 1 || admissionList.Form.ProgrammeFee.Programme.Id == 2 || admissionList.Form.ProgrammeFee.Programme.Id == 5)
                                {
                                    level = new Level() { Id = 1 };
                                }
                                else
                                {
                                    level = new Level() { Id = 3 };
                                }

                                decimal AmountToPay = feeDetailLogic.GetFeeByDepartmentLevel(admissionList.Deprtment, level, admissionList.Form.ProgrammeFee.Programme, payment.FeeType, payment.Session, payment.PaymentMode);
                                if (!etranzactLogic.ValidatePin(etranzactDetails, payment, AmountToPay))
                                {
                                    if (CheckShortFall(etranzactDetails, payment, AmountToPay))
                                    {
                                        //paid shortfall
                                    }
                                    else
                                    {
                                        throw new Exception("The pin amount tied to the pin is not correct. Please contact support@lloydant.com.");
                                    }
                                }
                            }
                        }
                        else if (payment.FeeType.Id == (int)FeeTypes.ConvocationFee)
                        {
                            StudentLevel studentLevel = new StudentLevel();
                            StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                            studentLevel = studentLevelLogic.GetModelsBy(s => s.Person_Id == payment.Person.Id && s.Session_Id == payment.Session.Id).LastOrDefault();

                            if (studentLevel != null)
                            {
                                decimal AmountToPay = feeDetailLogic.GetFeeByDepartmentLevel(studentLevel.Department, studentLevel.Level, studentLevel.Programme, payment.FeeType, payment.Session, payment.PaymentMode);
                                if (!etranzactLogic.ValidatePin(etranzactDetails, payment, AmountToPay))
                                {
                                    if (CheckShortFall(etranzactDetails, payment, AmountToPay))
                                    {
                                        //paid shortfall
                                    }
                                    else
                                    {
                                        throw new Exception("The pin amount tied to the pin is not correct. Please contact support@lloydant.com.");
                                    }
                                }
                            }
                        }
                        else
                        {
                            payment.FeeDetails = feeDetailLogic.GetModelsBy(a => a.Fee_Type_Id == payment.FeeType.Id);

                            if (!etranzactLogic.ValidatePin(etranzactDetails, payment, payment.FeeDetails.Sum(p => p.Fee.Amount)))
                            {
                                if (CheckShortFall(etranzactDetails, payment, payment.FeeDetails.Sum(p => p.Fee.Amount)))
                                {
                                    //paid shortfall
                                }
                                else
                                {
                                    throw new Exception("The pin amount tied to the pin is not correct. Please contact support@lloydant.com.");
                                }
                            }
                        }

                    }
                    else
                    {
                        throw new Exception("The invoice number attached to the pin doesn't belong to you! Please cross check and try again.");
                    }
                }

                return payment;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Payment InvalidConfirmationOrderNumber(string confirmationOrderNumber, Session session, FeeType feetype, string ApplicationNumber)
        {
            try
            {
                Payment payment = new Payment();
                PaymentEtranzactLogic etranzactLogic = new PaymentEtranzactLogic();
                PaymentEtranzact etranzactDetails = etranzactLogic.GetModelBy(m => m.Confirmation_No == confirmationOrderNumber);
                if (etranzactDetails == null || etranzactDetails.ReceiptNo == null)
                {

                    RemitaPayment remitaPayment = new RemitaPayment();
                    RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                    RemitaPayementProcessor r = new RemitaPayementProcessor("918567");

                    remitaPayment = remitaPaymentLogic.GetModelsBy(a => a.RRR == confirmationOrderNumber).FirstOrDefault();

                    if (remitaPayment == null && feetype.Id == 3)
                    {
                        //Get Remita Record for omitted RRR
                        Payment getPayment = ValidatePayWithCardDispute(ApplicationNumber, false);
                        RemitaPayment remitaPaymentToFetch = new RemitaPayment { RRR = confirmationOrderNumber, MerchantCode = "538661740" };
                        if(getPayment != null)
                        {
                            remitaPaymentToFetch.payment = getPayment;
                        }
                        remitaPayment = r.GetStatusForPayWithCardDispute(remitaPaymentToFetch);
                    }
                   if(!remitaPayment.Status.Contains("01") && !remitaPayment.Status.Contains("00"))
                        remitaPayment = r.GetStatus(remitaPayment);
                    if (remitaPayment != null)
                    {
                        //remitaPayment = !remitaPayment.Status.Contains("01") ? r.GetStatus(remitaPayment.OrderId) : remitaPayment;
                        //remitaPayment.TransactionAmount == 2500 &
                        //remitaPayment = r.GetStatus(remitaPayment);

                        if (remitaPayment.Status.Contains("01") || remitaPayment.Status.Contains("00") || remitaPayment.Description.Contains("manual"))
                        {
                            FeeDetailLogic feeDetailLogic = new FeeDetailLogic();

                            StudentLevel studentLevel = new StudentLevel();
                            StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                            studentLevel = studentLevelLogic.GetBy(remitaPayment.payment.Person.Id);

                            AdmissionList admissionList = new AdmissionList();
                            AdmissionListLogic admissionLogic = new AdmissionListLogic();
                            admissionList = admissionLogic.GetBy(remitaPayment.payment.Person);

                            if (studentLevel != null)
                            {
                                decimal AmountToPay = feeDetailLogic.GetFeeByDepartmentLevel(studentLevel.Department, studentLevel.Level, studentLevel.Programme, remitaPayment.payment.FeeType,
                                                                                                remitaPayment.payment.Session, remitaPayment.payment.PaymentMode);

                                //Check to resolve for cisco/robotics
                                if (AmountToPay > remitaPayment.TransactionAmount)
                                {
                                    AmountToPay = ResolveAmountPaidAndExpexctedAmountDisparity(remitaPayment.TransactionAmount, AmountToPay);
                                }
                                if (remitaPayment.TransactionAmount < AmountToPay)
                                {
                                    if (CheckShortFallRemita(remitaPayment, AmountToPay))
                                    {
                                        //paid shortfall
                                    }
                                    else
                                    {
                                        throw new Exception("The pin amount tied to the pin is not correct. Please contact support@lloydant.com.");
                                    }
                                }

                            }
                            else
                            {
                                if (admissionList != null)
                                {
                                    Level level;
                                    if (admissionList.Form.ProgrammeFee.Programme.Id == (int)Programmes.NDFullTime || admissionList.Form.ProgrammeFee.Programme.Id == (int)Programmes.NDPartTime || admissionList.Form.ProgrammeFee.Programme.Id == (int)Programmes.DrivingCertificate || admissionList.Form.ProgrammeFee.Programme.Id == (int)Programmes.NDDistance)
                                    {
                                        level = new Level() { Id = 1 };
                                    }
                                    else
                                    {
                                        level = new Level() { Id = 3 };
                                    }


                                    decimal AmountToPay = feeDetailLogic.GetFeeByDepartmentLevel(admissionList.Deprtment, level, admissionList.Form.ProgrammeFee.Programme, remitaPayment.payment.FeeType,
                                                                                                    remitaPayment.payment.Session, remitaPayment.payment.PaymentMode);


                                    //if (remitaPayment.payment.FeeType.Id == (int)FeeTypes.SchoolFees && admissionList.Form.ProgrammeFee.Programme.Id != (int)Programmes.HNDDistance && admissionList.Form.ProgrammeFee.Programme.Id != (int)Programmes.NDDistance)
                                    //{
                                    //    FeeinPaymentLogic feeinPaymentLogic = new FeeinPaymentLogic();
                                    //    //FeeDetailLogic feeDetailLogic = new FeeDetailLogic();
                                    //    var feeinPayments = feeinPaymentLogic.GetModelsBy(f => f.Payment_Id == remitaPayment.payment.Id);
                                    //    foreach (var item in feeinPayments)
                                    //    {
                                    //        if (item.IsIncluded)
                                    //            payment.FeeDetails = feeDetailLogic.AlterFeeDetailRecords(item.Fee.Id, false, payment.FeeDetails);
                                    //        else
                                    //            payment.FeeDetails = feeDetailLogic.AlterFeeDetailRecords(item.Fee.Id, true, payment.FeeDetails);
                                    //    }

                                    //    AmountToPay = payment.FeeDetails.Sum(f => f.Fee.Amount);
                                    //}
                                    if (AmountToPay == 0 || AmountToPay > remitaPayment.TransactionAmount)
                                    {
                                        AmountToPay = feeDetailLogic.GetFeeByDepartmentLevel(admissionList.Deprtment, level, admissionList.Form.ProgrammeFee.Programme, remitaPayment.payment.FeeType, remitaPayment.payment.Session, remitaPayment.payment.PaymentMode);

                                        AmountToPay = ResolveAmountPaidAndExpexctedAmountDisparity(remitaPayment.TransactionAmount, AmountToPay);
                                    }

                                    if (admissionList.Deprtment.Id == 44)
                                    {
                                        if (feetype?.Id > 0 && feetype.Id == 26)
                                        {
                                            AmountToPay = Convert.ToDecimal(Fees.AllDrivingAcceptance);
                                        }
                                        else
                                        {
                                            if (admissionList.DepartmentOption.Id == 16)
                                            {
                                                AmountToPay = Convert.ToDecimal(Fees.GraduateDrivingTrainingSchoolFees);
                                            }
                                            else if (admissionList.DepartmentOption.Id == 17)
                                            {
                                                AmountToPay = Convert.ToDecimal(Fees.TechnicalCertificateDrivingSchooFees);
                                            }
                                            else if (admissionList.DepartmentOption.Id == 18)
                                            {
                                                AmountToPay = Convert.ToDecimal(Fees.ProfessionalDiplomaDrivingSchooFees);
                                            }
                                        }
                                    }
                                    if (remitaPayment.TransactionAmount < AmountToPay)
                                    {
                                        if (CheckShortFallRemita(remitaPayment, AmountToPay))
                                        {
                                            //paid shortfall
                                        }
                                        else
                                        {
                                            throw new Exception("The pin amount tied to the pin is not correct. Please contact support@lloydant.com.");
                                        }
                                    }
                                }
                            }

                            payment = remitaPayment.payment;
                            return payment;
                        }
                        else
                        {
                            throw new Exception("Payment could not be verified, Try again in a few minutes");
                        }
                    }

                    PaymentTerminal paymentTerminal = new PaymentTerminal();
                    PaymentTerminalLogic paymentTerminalLogic = new PaymentTerminalLogic();
                    paymentTerminal = paymentTerminalLogic.GetModelsBy(p => p.Fee_Type_Id == feetype.Id && p.Session_Id == session.Id).FirstOrDefault();

                    etranzactDetails = etranzactLogic.RetrievePinAlternative(confirmationOrderNumber, paymentTerminal);
                    if (etranzactDetails != null && etranzactDetails.ReceiptNo != null)
                    {
                        PaymentLogic paymentLogic = new PaymentLogic();
                        payment = paymentLogic.GetModelBy(m => m.Invoice_Number == etranzactDetails.CustomerID);
                        if (payment != null && payment.Id > 0)
                        {
                            List<FeeDetail> feeDetail = new List<FeeDetail>();
                            FeeDetailLogic feeDetailLogic = new FeeDetailLogic();


                            StudentLevel studentLevel = new StudentLevel();
                            StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                            studentLevel = studentLevelLogic.GetBy(payment.Person.Id);

                            AdmissionList admissionList = new AdmissionList();
                            AdmissionListLogic admissionLogic = new AdmissionListLogic();
                            admissionList = admissionLogic.GetBy(payment.Person);

                            if (studentLevel != null)
                            {

                                if (studentLevel != null)
                                {
                                    decimal AmountToPay = feeDetailLogic.GetFeeByDepartmentLevel(studentLevel.Department, studentLevel.Level, studentLevel.Programme, payment.FeeType, payment.Session, payment.PaymentMode);
                                    if (!etranzactLogic.ValidatePin(etranzactDetails, payment, AmountToPay))
                                    {
                                        if (CheckShortFall(etranzactDetails, payment, AmountToPay))
                                        {
                                            //paid shortfall
                                        }
                                        else
                                        {
                                            throw new Exception("The pin amount tied to the pin is not correct. Please contact support@lloydant.com.");
                                        }
                                    }
                                }
                                else
                                {
                                    throw new Exception("Cannot retrieve amount for your department. Please contact support@lloydant.com.");

                                }

                            }
                            else
                            {
                                if (admissionList != null)
                                {
                                    Level level;
                                    if (admissionList.Form.ProgrammeFee.Programme.Id == 1 || admissionList.Form.ProgrammeFee.Programme.Id == 2 || admissionList.Form.ProgrammeFee.Programme.Id == 5)
                                    {
                                        level = new Level() { Id = 1 };
                                    }
                                    else
                                    {
                                        level = new Level() { Id = 3 };
                                    }

                                    decimal AmountToPay = feeDetailLogic.GetFeeByDepartmentLevel(admissionList.Deprtment, level, admissionList.Form.ProgrammeFee.Programme, payment.FeeType, payment.Session, payment.PaymentMode);
                                    if (!etranzactLogic.ValidatePin(etranzactDetails, payment, AmountToPay))
                                    {
                                        if (CheckShortFall(etranzactDetails, payment, AmountToPay))
                                        {
                                            //paid shortfall
                                        }
                                        else
                                        {
                                            throw new Exception("The pin amount tied to the pin is not correct. Please contact support@lloydant.com.");
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            throw new Exception("The invoice number attached to the pin doesn't belong to you! Please cross check and try again.");
                        }
                    }
                    else
                    {
                        throw new Exception("Confirmation Order Number entered seems not to be valid! Please cross check and try again.");
                    }
                }
                else
                {
                    PaymentLogic paymentLogic = new PaymentLogic();
                    payment = paymentLogic.GetModelBy(m => m.Invoice_Number == etranzactDetails.CustomerID);
                    if (payment != null && payment.Id > 0)
                    {
                        List<FeeDetail> feeDetail = new List<FeeDetail>();
                        FeeDetailLogic feeDetailLogic = new FeeDetailLogic();

                        if (payment.FeeType.Id == (int)FeeTypes.SchoolFees || payment.FeeType.Id == (int)FeeTypes.CarryOverSchoolFees)
                        {

                            AdmissionList admissionList = new AdmissionList();
                            AdmissionListLogic admissionLogic = new AdmissionListLogic();
                            admissionList = admissionLogic.GetBy(payment.Person);

                            if (admissionList != null)
                            {
                                Level level;
                                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                                StudentLevel currentStudentLevel = studentLevelLogic.GetModelsBy(sl => sl.Person_Id == payment.Person.Id && sl.Session_Id == payment.Session.Id).LastOrDefault();
                                if (currentStudentLevel != null)
                                {
                                    level = currentStudentLevel.Level;
                                }
                                else if (admissionList.Session.Id != payment.Session.Id)
                                {
                                    if (admissionList.Form.ProgrammeFee.Programme.Id == 1 || admissionList.Form.ProgrammeFee.Programme.Id == 2 || admissionList.Form.ProgrammeFee.Programme.Id == 5)
                                    {
                                        level = new Level() { Id = 2 };
                                    }
                                    else
                                    {
                                        level = new Level() { Id = 4 };
                                    }
                                }
                                else
                                {
                                    if (admissionList.Form.ProgrammeFee.Programme.Id == 1 || admissionList.Form.ProgrammeFee.Programme.Id == 2 || admissionList.Form.ProgrammeFee.Programme.Id == 5)
                                    {
                                        level = new Level() { Id = 1 };
                                    }
                                    else
                                    {
                                        level = new Level() { Id = 3 };
                                    }
                                }

                                decimal AmountToPay = 0M;

                                if (payment.FeeType.Id == (int)FeeTypes.CarryOverSchoolFees)
                                {
                                    StudentExtraYearSession extraYear = new StudentExtraYearSession();
                                    StudentExtraYearLogic extraYearLogic = new StudentExtraYearLogic();
                                    extraYear = extraYearLogic.GetBy(payment.Person.Id, payment.Session.Id);

                                    if (extraYear != null)
                                    {
                                        int lastSession =
                                            Convert.ToInt32(extraYear.LastSessionRegistered.Name.Substring(0, 4));
                                        int currentSession = Convert.ToInt32(payment.Session.Name.Substring(0, 4));
                                        int NoOfOutstandingSession = currentSession - lastSession;
                                        if (NoOfOutstandingSession == 0)
                                        {
                                            NoOfOutstandingSession = 1;
                                        }

                                        AmountToPay = feeDetailLogic.GetFeeByDepartmentLevel(admissionList.Deprtment, level, admissionList.Form.ProgrammeFee.Programme, payment.FeeType, payment.Session, payment.PaymentMode) * NoOfOutstandingSession;
                                    }
                                }
                                else
                                {
                                    AmountToPay = feeDetailLogic.GetFeeByDepartmentLevel(admissionList.Deprtment, level, admissionList.Form.ProgrammeFee.Programme, payment.FeeType, payment.Session, payment.PaymentMode);
                                }

                                if (!etranzactLogic.ValidatePin(etranzactDetails, payment, AmountToPay))
                                {
                                    if (CheckShortFall(etranzactDetails, payment, AmountToPay))
                                    {
                                        //paid shortfall
                                    }
                                    else
                                    {
                                        throw new Exception("The pin amount tied to the pin is not correct. Please contact support@lloydant.com.");
                                    }
                                }
                            }
                            else
                            {
                                StudentLevel studentLevel = new StudentLevel();
                                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();

                                //studentLevel = studentLevelLogic.GetBy(payment.Person.Id);
                                //studentLevel = studentLevelLogic.GetExtraYearBy(payment.Person.Id);
                                studentLevel = studentLevelLogic.GetModelsBy(s => s.Session_Id == payment.Session.Id && s.Person_Id == payment.Person.Id).LastOrDefault();
                                if (studentLevel != null)
                                {
                                    //decimal AmountToPay = feeDetailLogic.GetFeeByDepartmentLevel(studentLevel.Department, studentLevel.Level, studentLevel.Programme, payment.FeeType, payment.Session, payment.PaymentMode);
                                    //if (!etranzactLogic.ValidatePin(etranzactDetails, payment, AmountToPay))
                                    //{
                                    //    throw new Exception("The pin amount tied to the pin is not correct. Please contact support@lloydant.com.");

                                    //}
                                }
                                else
                                {
                                    throw new Exception("Cannot retrieve amount for your department. Please contact support@lloydant.com.");

                                }

                            }
                        }
                        else if (payment.FeeType.Id == (int)FeeTypes.AcceptanceFee || payment.FeeType.Id == (int)FeeTypes.HNDAcceptance)
                        {
                            AdmissionList admissionList = new AdmissionList();
                            AdmissionListLogic admissionLogic = new AdmissionListLogic();
                            admissionList = admissionLogic.GetBy(payment.Person);



                            if (admissionList != null)
                            {
                                Level level;
                                if (admissionList.Form.ProgrammeFee.Programme.Id == 1 || admissionList.Form.ProgrammeFee.Programme.Id == 2 || admissionList.Form.ProgrammeFee.Programme.Id == 5)
                                {
                                    level = new Level() { Id = 1 };
                                }
                                else
                                {
                                    level = new Level() { Id = 3 };
                                }

                                decimal AmountToPay = feeDetailLogic.GetFeeByDepartmentLevel(admissionList.Deprtment, level, admissionList.Form.ProgrammeFee.Programme, payment.FeeType, payment.Session, payment.PaymentMode);
                                if (!etranzactLogic.ValidatePin(etranzactDetails, payment, AmountToPay))
                                {
                                    if (CheckShortFall(etranzactDetails, payment, AmountToPay))
                                    {
                                        //paid shortfall
                                    }
                                    else
                                    {
                                        throw new Exception("The pin amount tied to the pin is not correct. Please contact support@lloydant.com.");
                                    }
                                }
                            }
                        }
                        else if (payment.FeeType.Id == (int)FeeTypes.ConvocationFee)
                        {
                            StudentLevel studentLevel = new StudentLevel();
                            StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                            studentLevel = studentLevelLogic.GetModelsBy(s => s.Person_Id == payment.Person.Id && s.Session_Id == payment.Session.Id).LastOrDefault();

                            if (studentLevel != null)
                            {
                                decimal AmountToPay = feeDetailLogic.GetFeeByDepartmentLevel(studentLevel.Department, studentLevel.Level, studentLevel.Programme, payment.FeeType, payment.Session, payment.PaymentMode);
                                if (!etranzactLogic.ValidatePin(etranzactDetails, payment, AmountToPay))
                                {
                                    if (CheckShortFall(etranzactDetails, payment, AmountToPay))
                                    {
                                        //paid shortfall
                                    }
                                    else
                                    {
                                        throw new Exception("The pin amount tied to the pin is not correct. Please contact support@lloydant.com.");
                                    }
                                }
                            }
                        }
                        else
                        {
                            payment.FeeDetails = feeDetailLogic.GetModelsBy(a => a.Fee_Type_Id == payment.FeeType.Id);

                            if (!etranzactLogic.ValidatePin(etranzactDetails, payment, payment.FeeDetails.Sum(p => p.Fee.Amount)))
                            {
                                if (CheckShortFall(etranzactDetails, payment, payment.FeeDetails.Sum(p => p.Fee.Amount)))
                                {
                                    //paid shortfall
                                }
                                else
                                {
                                    throw new Exception("The pin amount tied to the pin is not correct. Please contact support@lloydant.com.");
                                }
                            }
                        }

                    }
                    else
                    {
                        throw new Exception("The invoice number attached to the pin doesn't belong to you! Please cross check and try again.");
                    }
                }

                return payment;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public decimal ResolveAmountPaidAndExpexctedAmountDisparity(decimal AmountPaid, decimal ExpectedAmount)
        {
            try
            {
                decimal robotics = 10000;
                decimal cisco = 6000;

                if (ExpectedAmount - robotics == AmountPaid || ExpectedAmount - cisco == AmountPaid || (ExpectedAmount - robotics + cisco) == AmountPaid)
                    return AmountPaid;
                return ExpectedAmount;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        private bool CheckShortFall(PaymentEtranzact etranzactDetails, Payment payment, decimal amountToPay)
        {
            bool paid = false;
            try
            {
                ShortFallLogic shortFallLogic = new ShortFallLogic();
                ShortFall shortFall = shortFallLogic.GetModelsBy(s => s.Payment_Id == payment.Id).LastOrDefault();

                if (shortFall != null)
                {
                    paid = Convert.ToDecimal(shortFall.Amount) + etranzactDetails.TransactionAmount == amountToPay;
                }
            }
            catch (Exception)
            {
                throw;
            }

            return paid;
        }
        private bool CheckShortFallRemita(RemitaPayment remitaPayment, decimal amountToPay)
        {
            try
            {
                RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                List<RemitaPayment> remitaPaymentShortfalls = remitaPaymentLogic.GetModelsBy(r => r.PAYMENT.Person_Id == remitaPayment.payment.Person.Id && r.PAYMENT.Fee_Type_Id == 12 &&
                                                                                        (r.Status.Contains("01") || r.Description.Contains("manual-miracle")) && r.PAYMENT.Session_Id == remitaPayment.payment.Session.Id);

                decimal shortFallAmount = 0M;

                if (remitaPaymentShortfalls.Count > 0)
                {
                    for (int i = 0; i < remitaPaymentShortfalls.Count; i++)
                    {
                        shortFallAmount += remitaPaymentShortfalls[i].TransactionAmount;
                    }

                   var sumPaid = shortFallAmount + remitaPayment.TransactionAmount;
                    
                    if (amountToPay > remitaPayment.TransactionAmount)
                    {
                        amountToPay = ResolveAmountPaidAndExpexctedAmountDisparity(sumPaid, amountToPay);
                    }
                    return shortFallAmount + remitaPayment.TransactionAmount >= amountToPay;
                }
                else
                    return false;
            }
            catch (Exception)
            {
                throw;
            }

            return false;
        }
        public bool Modify(Payment payment)
        {
            try
            {
                Expression<Func<PAYMENT, bool>> selector = p => p.Payment_Id == payment.Id;
                PAYMENT entity = GetEntityBy(selector);

                if (entity == null || entity.Person_Id <= 0)
                {
                    throw new Exception(NoItemFound);
                }

                if (payment.Person != null)
                {
                    entity.Person_Id = payment.Person.Id;
                }

                entity.Payment_Serial_Number = payment.SerialNumber;
                entity.Invoice_Number = payment.InvoiceNumber;
                entity.Date_Paid = payment.DatePaid;

                if (payment.PaymentMode != null)
                {
                    entity.Payment_Mode_Id = payment.PaymentMode.Id;
                }
                if (payment.PaymentType != null)
                {
                    entity.Payment_Type_Id = payment.PaymentType.Id;
                }
                if (payment.PersonType != null)
                {
                    entity.Person_Type_Id = payment.PersonType.Id;
                }
                if (payment.FeeType != null)
                {
                    entity.Fee_Type_Id = payment.FeeType.Id;
                }
                if (payment.Session != null)
                {
                    entity.Session_Id = payment.Session.Id;
                }

                entity.Fee_Type_Id = payment.FeeType.Id;
                int modifiedRecordCount = Save();
                if (modifiedRecordCount <= 0)
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public decimal GetPaymentAmount(Payment payment)
        {
            decimal Amount = 0;
            try
            {
                FeeDetail feeDetail = new FeeDetail();
                FeeDetailLogic feeDetailLogic = new FeeDetailLogic();
                feeDetail = feeDetailLogic.GetModelBy(f => f.Fee_Type_Id == payment.FeeType.Id);
                Amount = feeDetail.Fee.Amount;
            }
            catch (Exception ex)
            {

                throw;
            }
            return Amount;
        }

        public void DeleteBy(long PaymentID)
        {
            try
            {
                Expression<Func<PAYMENT, bool>> selector = a => a.Payment_Id == PaymentID;
                Delete(selector);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<RegistrationBalanceReport> GetRegistrationBalanceList(Session session, Semester semester)
        {
            PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
            List<RegistrationBalanceReport> RegistrationBalanceList = new List<RegistrationBalanceReport>();
            StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
            STUDENT_LEVEL studentLevel = new STUDENT_LEVEL();

            CourseRegistrationLogic courseRegistrationLogic = new CourseRegistrationLogic();
            CourseRegistrationDetailLogic courseRegistrationDetailLogic = new CourseRegistrationDetailLogic();
            STUDENT_COURSE_REGISTRATION courseRegistration = new STUDENT_COURSE_REGISTRATION();
            STUDENT_COURSE_REGISTRATION_DETAIL courseRegistrationDetail = new STUDENT_COURSE_REGISTRATION_DETAIL();

            try
            {
                if (session != null)
                {
                    List<PAYMENT> payments = GetEntitiesBy(p => p.Fee_Type_Id == 3 && p.Session_Id == session.Id).Take(3000).ToList();
                    foreach (PAYMENT payment in payments)
                    {
                        int studentNumberPay = 0;
                        int studentNumberReg = 0;
                        PAYMENT_ETRANZACT paymentEtranzact = new PAYMENT_ETRANZACT();
                        paymentEtranzact = paymentEtranzactLogic.GetEntityBy(p => p.Payment_Id == payment.Payment_Id);
                        if (paymentEtranzact != null)
                        {
                            RegistrationBalanceReport registrationBalanceReport = new RegistrationBalanceReport();
                            studentLevel = studentLevelLogic.GetEntityBy(p => p.Person_Id == payment.Person_Id && p.Session_Id == session.Id);

                            if (session != null && semester != null)
                            {
                                courseRegistration = courseRegistrationLogic.GetEntityBy(p => p.Session_Id == session.Id && p.Person_Id == payment.Person_Id && p.Level_Id == studentLevel.Level_Id && p.Department_Id == studentLevel.Department_Id && p.Programme_Id == studentLevel.Programme_Id);
                                if (courseRegistration != null)
                                {
                                    courseRegistrationDetail = courseRegistrationDetailLogic.GetEntitiesBy(p => p.Semester_Id == semester.Id && p.Student_Course_Registration_Id == courseRegistration.Student_Course_Registration_Id).FirstOrDefault();
                                }
                            }

                            registrationBalanceReport.Department = studentLevel.DEPARTMENT.Department_Name;

                            if (studentLevel.Level_Id == 1 && studentLevel.Programme_Id == 2)
                            {
                                registrationBalanceReport.ProgrammePayment = "PART TIME 1 (PAY)";

                                //registrationBalanceReport.Payment = "(PAY)";
                                registrationBalanceReport.PaymentNumber = studentNumberPay += 1;
                                if (courseRegistrationDetail != null)
                                {
                                    registrationBalanceReport.ProgrammeRegistration = " PART TIME 1 (REG)";
                                    registrationBalanceReport.RegistrationNumber = studentNumberReg += 1;
                                }

                                RegistrationBalanceList.Add(registrationBalanceReport);
                            }
                            else if (studentLevel.Level_Id == 2 && studentLevel.Programme_Id == 2)
                            {
                                registrationBalanceReport.ProgrammePayment = "PART TIME 2 (PAY)";
                                //registrationBalanceReport.Payment = "(PAY)";
                                registrationBalanceReport.PaymentNumber = studentNumberPay += 1;
                                if (courseRegistrationDetail != null)
                                {
                                    registrationBalanceReport.ProgrammeRegistration = "PART TIME 2 (REG)";
                                    registrationBalanceReport.RegistrationNumber = studentNumberReg += 1;
                                }

                                RegistrationBalanceList.Add(registrationBalanceReport);
                            }
                            else
                            {
                                registrationBalanceReport.ProgrammePayment = studentLevel.LEVEL.Level_Name + " (PAY)";
                                //registrationBalanceReport.Payment = "(PAY)";
                                registrationBalanceReport.PaymentNumber = studentNumberPay += 1;
                                if (courseRegistrationDetail != null)
                                {
                                    registrationBalanceReport.ProgrammeRegistration = studentLevel.LEVEL.Level_Name + " (REG)";
                                    registrationBalanceReport.RegistrationNumber = studentNumberReg += 1;
                                }

                                RegistrationBalanceList.Add(registrationBalanceReport);
                            }

                        }

                    }
                }

            }
            catch (Exception)
            {
                throw;
            }
            return RegistrationBalanceList;
        }
        public List<PaymentReport> GetPaymentsBy(Session session)
        {
            PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
            List<PaymentReport> PaymentReportList = new List<PaymentReport>();
            StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
            STUDENT_LEVEL studentLevel = new STUDENT_LEVEL();

            try
            {
                if (session != null)
                {
                    List<PAYMENT_ETRANZACT> etranzacts = (from a in repository.GetBy<VW_SCHOOL_FEES_PAYMENT>(s => s.Fee_Type_Id == (int)FeeTypes.SchoolFees && s.Session_Id == session.Id)
                                                          select new PAYMENT_ETRANZACT()
                                                          {
                                                              ONLINE_PAYMENT = new ONLINE_PAYMENT() { PAYMENT = new PAYMENT() { Person_Id = a.Person_Id } }
                                                          }).ToList();

                    for (int i = 0; i < etranzacts.Count; i++)
                    {
                        PAYMENT_ETRANZACT etranzact = etranzacts[i];

                        int studentNumber = 0;
                        PaymentReport paymentReport = new PaymentReport();
                        studentLevel = studentLevelLogic.GetEntitiesBy(p => p.Person_Id == etranzact.ONLINE_PAYMENT.PAYMENT.Person_Id && p.Session_Id == session.Id).LastOrDefault();

                        if (studentLevel != null)
                        {
                            paymentReport.Department = studentLevel.DEPARTMENT.Department_Name;
                            if (studentLevel.Level_Id == (int)LevelList.ND1 && studentLevel.Programme_Id == (int)Programmes.NDPartTime)
                            {
                                paymentReport.Programme = "PART TIME 1";

                                paymentReport.StudentNumber = studentNumber += 1;
                                PaymentReportList.Add(paymentReport);
                            }
                            else if (studentLevel.Level_Id == (int)LevelList.ND2 && studentLevel.Programme_Id == (int)Programmes.NDPartTime)
                            {
                                paymentReport.Programme = "PART TIME 2";
                                paymentReport.StudentNumber = studentNumber += 1;
                                PaymentReportList.Add(paymentReport);
                            }
                            else
                            {
                                paymentReport.Programme = studentLevel.LEVEL.Level_Name;
                                paymentReport.StudentNumber = studentNumber += 1;
                                PaymentReportList.Add(paymentReport);
                            }
                        }
                    }


                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
            return PaymentReportList;
        }
        public List<DuplicateMatricNumberFix> GetPartTimeGuys()
        {
            List<DuplicateMatricNumberFix> dupList = new List<DuplicateMatricNumberFix>();
            List<PAYMENT> paymentList = GetEntitiesBy(p => p.Fee_Type_Id == 6 && p.Session_Id == 1);
            PaymentEtranzactLogic etranzactLogic = new PaymentEtranzactLogic();
            foreach (PAYMENT paymentItem in paymentList)
            {
                PAYMENT_ETRANZACT paymentEtranzact = etranzactLogic.GetEntityBy(p => p.Payment_Id == paymentItem.Payment_Id);
                if (paymentEtranzact != null)
                {
                    DuplicateMatricNumberFix dup = new DuplicateMatricNumberFix();
                    dup.Fullname = paymentItem.PERSON.Last_Name + " " + paymentItem.PERSON.First_Name + " " + paymentItem.PERSON.Other_Name;
                    dup.ConfirmationOrder = paymentEtranzact.Confirmation_No;
                    dup.ReceiptNumber = paymentEtranzact.Receipt_No;
                    dupList.Add(dup);
                }
            }
            return dupList.OrderBy(p => p.Fullname).ToList();
        }
        public List<FeesPaymentReport> GetFeesPaymentBy(Session session, Programme programme, Department department, Level level)
        {
            PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
            List<FeesPaymentReport> feesPaymentReportList = new List<FeesPaymentReport>();
            StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
            RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
            List<STUDENT_LEVEL> studentLevelList = new List<STUDENT_LEVEL>();
            List<ADMISSION_LIST> admissionList = new List<ADMISSION_LIST>();
            List<PAYMENT> payments = new List<PAYMENT>();
            PAYMENT_ETRANZACT paymentEtranzact = new PAYMENT_ETRANZACT();
            REMITA_PAYMENT remitaPayment = new REMITA_PAYMENT();
            PAYMENT_ETRANZACT thisPaymentEtranzact = new PAYMENT_ETRANZACT();
            REMITA_PAYMENT thisRemitaPayment = new REMITA_PAYMENT();

            try
            {
                if (session != null && programme != null && department != null && level != null)
                {
                    studentLevelList = studentLevelLogic.GetEntitiesBy(p => p.Department_Id == department.Id && p.Level_Id == level.Id && p.Programme_Id == programme.Id && p.Session_Id == session.Id);

                    foreach (STUDENT_LEVEL studentLevel in studentLevelList)
                    {
                        List<PAYMENT> confirmedPayments = new List<PAYMENT>();
                        payments = GetEntitiesBy(p => p.Person_Id == studentLevel.Person_Id && (p.Fee_Type_Id == 3 || p.Fee_Type_Id == 2));
                        foreach (PAYMENT payment in payments)
                        {
                            paymentEtranzact = paymentEtranzactLogic.GetEntityBy(p => p.Payment_Id == payment.Payment_Id);
                            if (paymentEtranzact != null)
                            {
                                confirmedPayments.Add(payment);
                            }
                            else
                            {
                                remitaPayment = remitaPaymentLogic.GetEntityBy(p => p.Payment_Id == payment.Payment_Id);
                                if (remitaPayment != null)
                                {
                                    confirmedPayments.Add(payment);
                                }

                            }
                        }

                        FeesPaymentReport feesPaymentReport = new FeesPaymentReport();
                        foreach (PAYMENT confirmedPayment in confirmedPayments)
                        {
                            thisPaymentEtranzact = paymentEtranzactLogic.GetEntityBy(p => p.Payment_Id == confirmedPayment.Payment_Id);
                            thisRemitaPayment = remitaPaymentLogic.GetEntityBy(p => p.Payment_Id == confirmedPayment.Payment_Id);
                            if (confirmedPayment.Fee_Type_Id == 2)
                            {

                                if (thisPaymentEtranzact != null)
                                {
                                    feesPaymentReport.AcceptanceTransactionAmount = "N" + paymentEtranzactLogic.GetModelBy(p => p.Payment_Id == confirmedPayment.Payment_Id).TransactionAmount.ToString();
                                    feesPaymentReport.AcceptanceFeeInvoiceNumber = thisPaymentEtranzact.Confirmation_No;
                                }
                                else if (thisRemitaPayment != null)
                                {
                                    feesPaymentReport.AcceptanceTransactionAmount = "N" + remitaPaymentLogic.GetModelBy(p => p.Payment_Id == confirmedPayment.Payment_Id).TransactionAmount.ToString();
                                    feesPaymentReport.AcceptanceFeeInvoiceNumber = thisRemitaPayment.RRR;
                                }
                            }
                            else if (confirmedPayment.Fee_Type_Id == 3 && confirmedPayment.Session_Id == session.Id)
                            {
                                if (studentLevel.Level_Id == 1 || studentLevel.Level_Id == 3)
                                {
                                    if (thisPaymentEtranzact != null)
                                    {
                                        feesPaymentReport.FirstYearFeesTransactionAmount = "N" + paymentEtranzactLogic.GetModelBy(p => p.Payment_Id == confirmedPayment.Payment_Id).TransactionAmount.ToString();
                                        feesPaymentReport.FirstYearSchoolFeesInvoiceNumber = thisPaymentEtranzact.Confirmation_No;
                                    }
                                    else if (thisRemitaPayment != null)
                                    {
                                        feesPaymentReport.FirstYearFeesTransactionAmount = "N" + remitaPaymentLogic.GetModelBy(p => p.Payment_Id == confirmedPayment.Payment_Id).TransactionAmount.ToString();
                                        feesPaymentReport.FirstYearSchoolFeesInvoiceNumber = thisRemitaPayment.RRR;
                                    }
                                }
                                if (studentLevel.Level_Id == 2 || studentLevel.Level_Id == 4)
                                {
                                    if (thisPaymentEtranzact != null)
                                    {
                                        feesPaymentReport.SecondYearFeesTransactionAmount = "N" + paymentEtranzactLogic.GetModelBy(p => p.Payment_Id == confirmedPayment.Payment_Id).TransactionAmount.ToString();
                                        feesPaymentReport.SecondYearSchoolFeesInvoiceNumber = thisPaymentEtranzact.Confirmation_No;
                                    }
                                    else if (thisRemitaPayment != null)
                                    {
                                        feesPaymentReport.SecondYearFeesTransactionAmount = "N" + remitaPaymentLogic.GetModelBy(p => p.Payment_Id == confirmedPayment.Payment_Id).TransactionAmount.ToString();
                                        feesPaymentReport.SecondYearSchoolFeesInvoiceNumber = thisRemitaPayment.RRR;
                                    }
                                }
                            }
                            else if (confirmedPayment.Fee_Type_Id == 3 && confirmedPayment.Session_Id != session.Id)
                            {
                                if (thisPaymentEtranzact != null)
                                {
                                    feesPaymentReport.FirstYearFeesTransactionAmount = "N" + paymentEtranzactLogic.GetModelBy(p => p.Payment_Id == confirmedPayment.Payment_Id).TransactionAmount.ToString();
                                    feesPaymentReport.FirstYearSchoolFeesInvoiceNumber = thisPaymentEtranzact.Confirmation_No;
                                }
                                else if (thisRemitaPayment != null)
                                {
                                    feesPaymentReport.FirstYearFeesTransactionAmount = "N" + remitaPaymentLogic.GetModelBy(p => p.Payment_Id == confirmedPayment.Payment_Id).TransactionAmount.ToString();
                                    feesPaymentReport.FirstYearSchoolFeesInvoiceNumber = thisRemitaPayment.RRR;
                                }
                            }
                        }


                        feesPaymentReport.Department = department.Name;
                        feesPaymentReport.Level = level.Name;
                        feesPaymentReport.MatricNumber = studentLevel.STUDENT.Matric_Number;
                        if (studentLevel.STUDENT.APPLICATION_FORM != null)
                        {
                            feesPaymentReport.ApplicationNumber = studentLevel.STUDENT.APPLICATION_FORM.Application_Form_Number;
                        }
                        feesPaymentReport.Programme = programme.Name;
                        feesPaymentReport.Session = session.Name;
                        feesPaymentReport.Name = studentLevel.STUDENT.PERSON.Last_Name + " " + studentLevel.STUDENT.PERSON.First_Name;

                        feesPaymentReportList.Add(feesPaymentReport);
                    }

                }
            }

            catch (Exception)
            {
                throw;
            }

            return feesPaymentReportList.OrderBy(p => p.Name).ToList();
        }
        public List<FeesPaymentReport> GetAllFeesPaymentBy(Session session, Programme programme, Department department, Level level)
        {
            List<FeesPaymentReport> feesPaymentReportList = new List<FeesPaymentReport>();

            try
            {
                if (session != null && programme != null && department != null && level != null)
                {

                    if (level.Id == 1 || level.Id == 3)
                    {
                        var paymentReport = (from a in repository.GetBy<VW_ALL_PAYMENT_REPORT_NEW_STUDENTS>(a => a.Department_Id == department.Id && a.Level_Id == level.Id && a.Programme_Id == programme.Id && a.Session_Id == session.Id)
                                             select new FeesPaymentReport
                                             {
                                                 Name = a.Last_Name + " " + a.First_Name + " " + a.Other_Name,
                                                 ApplicationNumber = a.Application_Form_Number,
                                                 MatricNumber = a.Matric_Number,
                                                 AcceptanceFeeInvoiceNumber = a.ACCEPTANCE_FEE_INVOICE,
                                                 AcceptanceTransactionAmount = a.ACCEPTANCE_FEE_AMOUNT.ToString(),
                                                 FirstYearSchoolFeesInvoiceNumber = a.FIRST_YEAR_FEE_INVOICE,
                                                 FirstYearFeesTransactionAmount = a.FIRST_YEAR_SCHOOL_FEE_AMOUNT.ToString(),
                                                 SecondYearSchoolFeesInvoiceNumber = a.SECOND_YEAR_FEE_INVOICE,
                                                 SecondYearFeesTransactionAmount = a.SECOND_YEAR_SCHOOL_FEE_AMOUNT.ToString(),
                                                 Session = a.Session_Name,
                                                 Programme = a.Programme_Name,
                                                 Department = a.Department_Name,
                                                 Level = a.Level_Name,
                                             }).ToList();
                        feesPaymentReportList = paymentReport;
                    }
                    else
                    {
                        feesPaymentReportList = GetFeesPaymentBy(session, programme, department, level);
                    }



                }
            }

            catch (Exception)
            {
                throw;
            }
            return feesPaymentReportList.OrderBy(p => p.Name).ToList();

        }


        public List<AcceptanceView> GetAcceptanceCount(Session session, string dateFrom, string dateTo)
        {
            try
            {
                List<AcceptanceView> payments = new List<AcceptanceView>();
                string[] ndProgrammeNames = { "ND Full Time", "ND Part Time" };
                string[] hndProgrammeNames = { "HND Full Time", "HND Part Time" };
                int ndCount = 0;
                int hndCount = 0;
                int otherCount = 0;

                if (!string.IsNullOrEmpty(dateFrom) && !string.IsNullOrEmpty(dateTo))
                {
                    DateTime applicationFrom = ConvertToDate(dateFrom);
                    DateTime applicationTo = ConvertToDate(dateTo);

                    payments = (from p in repository.GetBy<VW_ACCEPTANCE_REPORT>(p => p.Session_Id == session.Id && (p.Transaction_Date >= applicationFrom && p.Transaction_Date <= applicationTo))
                                select new AcceptanceView
                                {
                                    Person_Id = p.Person_Id,
                                    Application_Exam_Number = p.Application_Exam_Number,
                                    Invoice_Number = p.Invoice_Number,
                                    Application_Form_Number = p.Application_Form_Number,
                                    First_Choice_Department_Name = p.Department_Name,
                                    Name = p.SURNAME + " " + p.FIRSTNAME + " " + p.OTHER_NAMES,
                                    RRR = p.Invoice_Number,
                                    Programme_Name = p.Programme_Name,
                                    InvoiceDate = p.Transaction_Date == null ? p.Date_Paid.ToLongDateString() : p.Transaction_Date.ToLongDateString(),
                                    Count = 1
                                }).OrderBy(b => b.Name).ToList();

                    ndCount = payments.Count(p => ndProgrammeNames.Contains(p.Programme_Name));
                    hndCount = payments.Count(p => hndProgrammeNames.Contains(p.Programme_Name));
                }
                else
                {
                    payments = (from p in repository.GetBy<VW_ACCEPTANCE_REPORT>(p => p.Session_Id == session.Id)
                                select new AcceptanceView
                                {
                                    Person_Id = p.Person_Id,
                                    Application_Exam_Number = p.Application_Exam_Number,
                                    Invoice_Number = p.Invoice_Number,
                                    Application_Form_Number = p.Application_Form_Number,
                                    First_Choice_Department_Name = p.Department_Name,
                                    Name = p.SURNAME + " " + p.FIRSTNAME + " " + p.OTHER_NAMES,
                                    RRR = p.Invoice_Number,
                                    Programme_Name = p.Programme_Name,
                                    InvoiceDate = p.Transaction_Date == null ? p.Date_Paid.ToLongDateString() : p.Transaction_Date.ToLongDateString(),
                                    Count = 1
                                }).OrderBy(b => b.Name).ToList();

                    ndCount = payments.Count(p => ndProgrammeNames.Contains(p.Programme_Name));
                    hndCount = payments.Count(p => hndProgrammeNames.Contains(p.Programme_Name));
                }

                for (int i = 0; i < payments.Count; i++)
                {
                    payments[i].HNDCount = hndCount;
                    payments[i].NDCount = ndCount;
                    payments[i].OTHERCount = otherCount;
                    payments[i].TotalCount = payments.Count;
                }

                return payments;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DateTime ConvertToDate(string date)
        {
            DateTime newDate = new DateTime();
            try
            {
                string[] dateSplit = date.Split('/');
                newDate = new DateTime(Convert.ToInt32(dateSplit[2]), Convert.ToInt32(dateSplit[1]), Convert.ToInt32(dateSplit[0]));
            }
            catch (Exception)
            {
                throw;
            }

            return newDate;
        }

        public List<AcceptanceView> GetAcceptanceReport(Session session, Department department, Programme programme, string dateFrom, string dateTo)
        {

            try
            {
                List<AcceptanceView> payments = new List<AcceptanceView>();

                if (!string.IsNullOrEmpty(dateFrom) && !string.IsNullOrEmpty(dateTo))
                {
                    DateTime applicationFrom = ConvertToDate(dateFrom);
                    DateTime applicationTo = ConvertToDate(dateTo);

                    payments = (from p in repository.GetBy<VW_ACCEPTANCE_REPORT>(p => p.Department_Id == department.Id && p.Programme_Id == programme.Id && p.Session_Id == session.Id && (p.Transaction_Date >= applicationFrom && p.Transaction_Date <= applicationTo))
                                select new AcceptanceView
                                {
                                    Person_Id = p.Person_Id,
                                    Application_Exam_Number = p.Application_Exam_Number,
                                    Invoice_Number = p.Confirmation_No,
                                    Application_Form_Number = p.Application_Form_Number,
                                    First_Choice_Department_Name = p.Department_Name,
                                    Name = p.SURNAME + " " + p.FIRSTNAME + " " + p.OTHER_NAMES,
                                    RRR = p.Confirmation_No,
                                    Programme_Name = p.Programme_Name,
                                    InvoiceDate = p.Transaction_Date == null ? p.Date_Paid.ToLongDateString() : p.Transaction_Date.ToLongDateString(),
                                }).Distinct().OrderBy(b => b.Name).ToList();

                }
                else
                {
                    payments = (from p in repository.GetBy<VW_ACCEPTANCE_REPORT>(p => p.Department_Id == department.Id && p.Programme_Id == programme.Id && p.Session_Id == session.Id)
                                select new AcceptanceView
                                {
                                    Person_Id = p.Person_Id,
                                    Application_Exam_Number = p.Application_Exam_Number,
                                    Invoice_Number = p.Confirmation_No,
                                    Application_Form_Number = p.Application_Form_Number,
                                    First_Choice_Department_Name = p.Department_Name,
                                    Name = p.SURNAME + " " + p.FIRSTNAME + " " + p.OTHER_NAMES,
                                    RRR = p.Confirmation_No,
                                    Programme_Name = p.Programme_Name,
                                    InvoiceDate = p.Transaction_Date == null ? p.Date_Paid.ToLongDateString() : p.Transaction_Date.ToLongDateString(),
                                }).Distinct().OrderBy(b => b.Name).ToList();

                }

                List<AcceptanceView> uniquePayments = new List<AcceptanceView>();
                for (int i = 0; i < payments.Count; i++)
                {
                    if (!uniquePayments.Exists(p => p.Person_Id == payments[i].Person_Id))
                    {
                        uniquePayments.Add(payments[i]);
                    }
                }

                return uniquePayments;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<PaymentSummary> GetPaymentSummary(DateTime dateFrom, DateTime dateTo, string gateway)
        {
            try
            {
                List<PaymentSummary> payments = new List<PaymentSummary>();

                if (gateway == "Etranzact")
                {
                    List<PaymentSummary> paymentsETranzact = GetEtranzactSummary(dateFrom, dateTo);
                    payments.AddRange(paymentsETranzact);
                }
                else
                {
                    List<PaymentSummary> paymentsRemita = GetRemitaSummary(dateFrom, dateTo);
                    payments.AddRange(paymentsRemita);
                }

                //var transcriptPayments = repository.GetBy<VW_PAYMENT_SUMMARY_REMITA>(p => p.Transaction_Date != null && p.Transaction_Date >= dateFrom && p.Transaction_Date <= dateTo && p.Fee_Type_Id == 13).ToList();

                return payments;

            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<PaymentSummary> GetPaymentSummaryByFeeType(DateTime dateFrom, DateTime dateTo, int feeTypeId, string gateway)
        {
            try
            {
                List<PaymentSummary> payments = new List<PaymentSummary>();

                if (gateway == "Etranzact")
                {
                    payments = (from p in repository.GetBy<VW_PAYMENT_SUMMARY_ETRANZACT>(p => p.Transaction_Date != null && p.Transaction_Date >= dateFrom && p.Transaction_Date <= dateTo && p.Fee_Type_Id == feeTypeId)
                                select new PaymentSummary
                                {
                                    PersonId = p.Person_Id,
                                    Name = p.Name,
                                    MatricNumber = p.Matric_Number,
                                    SessionId = p.Session_Id,
                                    SessionName = p.Session_Name,
                                    FeeTypeId = p.Fee_Type_Id,
                                    FeeTypeName = p.Fee_Type_Name,
                                    LevelId = p.Level_Id,
                                    LevelName = p.Level_Name,
                                    ProgrammeId = p.Programme_Id,
                                    ProgrammeName = p.Programme_Name,
                                    DepartmentId = p.Department_Id,
                                    DepartmentName = p.Department_Name,
                                    FacultyId = p.Faculty_Id,
                                    FacultyName = p.Faculty_Name,
                                    TransactionDate = p.Transaction_Date,
                                    TransactionAmount = p.Transaction_Amount,
                                    PaymentEtranzactId = p.Payment_Etranzact_Id,
                                    InvoiceNumber = p.Invoice_Number,
                                    ConfirmationNumber = p.Confirmation_No
                                }).ToList();
                }
                else
                {
                    payments = (from p in repository.GetBy<VW_PAYMENT_SUMMARY_REMITA>(p => p.Transaction_Date != null && p.Transaction_Date >= dateFrom && p.Transaction_Date <= dateTo && p.Fee_Type_Id == feeTypeId)
                                select new PaymentSummary
                                {
                                    PersonId = p.Person_Id,
                                    Name = p.Name,
                                    MatricNumber = p.Matric_Number,
                                    SessionId = p.Session_Id,
                                    SessionName = p.Session_Name,
                                    FeeTypeId = p.Fee_Type_Id,
                                    FeeTypeName = p.Fee_Type_Name,
                                    LevelId = p.Level_Id,
                                    LevelName = p.Level_Name,
                                    ProgrammeId = p.Programme_Id,
                                    ProgrammeName = p.Programme_Name,
                                    DepartmentId = p.Department_Id,
                                    DepartmentName = p.Department_Name,
                                    FacultyId = p.Faculty_Id,
                                    FacultyName = p.Faculty_Name,
                                    TransactionDate = p.Transaction_Date,
                                    TransactionAmount = p.Transaction_Amount,
                                    RRR = p.RRR,
                                    Status = p.Status,
                                    PaymentEtranzactId = p.Payment_Id,
                                    InvoiceNumber = p.Invoice_Number,
                                    ConfirmationNumber = p.Confirmation_Number
                                }).ToList();
                }

                List<PaymentSummary> masterPayments = new List<PaymentSummary>();

                List<int> feeTypes = payments.Select(p => p.FeeTypeId).Distinct().ToList();
                feeTypes.ForEach(p =>
                {
                    List<PaymentSummary> feeTypePayments = payments.Where(e => e.FeeTypeId == p).ToList();
                    decimal totalAmount = Convert.ToDecimal(feeTypePayments.Sum(f => f.TransactionAmount));

                    feeTypePayments.ForEach(f =>
                    {
                        f.TotalAmount = totalAmount;

                        masterPayments.Add(f);
                    });
                });


                return masterPayments;

            }
            catch (Exception)
            {
                throw;
            }
        }
        public decimal GetTotalPaymentByFeeType(DateTime dateFrom, DateTime dateTo, int feeTypeId, string gateway)
        {
            try
            {
                decimal? amount = 0M;

                if (gateway == "Etranzact")
                {
                    amount = repository.GetBy<VW_PAYMENT_SUMMARY_ETRANZACT>(p => p.Transaction_Date != null &&
                                p.Transaction_Date >= dateFrom && p.Transaction_Date <= dateTo && p.Fee_Type_Id == feeTypeId).Sum(p => p.Transaction_Amount);

                }
                else
                {
                    amount = repository.GetBy<VW_PAYMENT_SUMMARY_REMITA>(p => p.Transaction_Date != null &&
                                 p.Transaction_Date >= dateFrom && p.Transaction_Date <= dateTo && p.Fee_Type_Id == feeTypeId).Sum(p => p.Transaction_Amount);

                }

                return Convert.ToDecimal(amount);

            }
            catch (Exception)
            {
                throw;
            }
        }
        private List<PaymentSummary> GetEtranzactSummary(DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                List<PaymentSummary> payments = (from p in repository.GetAll<VW_PAYMENT_SUMMARY_ETRANZACT>()
                                                 select new PaymentSummary
                                                 {
                                                     PersonId = p.Person_Id,
                                                     Name = p.Name,
                                                     MatricNumber = p.Matric_Number,
                                                     SessionId = p.Session_Id,
                                                     SessionName = p.Session_Name,
                                                     FeeTypeId = p.Fee_Type_Id,
                                                     FeeTypeName = p.Fee_Type_Name,
                                                     LevelId = p.Level_Id,
                                                     LevelName = p.Level_Name,
                                                     ProgrammeId = p.Programme_Id,
                                                     ProgrammeName = p.Programme_Name,
                                                     DepartmentId = p.Department_Id,
                                                     DepartmentName = p.Department_Name,
                                                     FacultyId = p.Faculty_Id,
                                                     FacultyName = p.Faculty_Name,
                                                     TransactionDate = p.Transaction_Date,
                                                     TransactionAmount = p.Transaction_Amount,
                                                     PaymentEtranzactId = p.Payment_Etranzact_Id,
                                                     InvoiceNumber = p.Invoice_Number,
                                                     ConfirmationNumber = p.Confirmation_No
                                                 }).ToList();

                payments = payments.Where(p => p.TransactionDate != null && p.TransactionDate >= dateFrom && p.TransactionDate <= dateTo).ToList();

                return payments;

            }
            catch (Exception)
            {
                throw;
            }
        }

        private List<PaymentSummary> GetRemitaSummary(DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                List<PaymentSummary> payments = (from p in repository.GetAll<VW_PAYMENT_SUMMARY_REMITA>()
                                                 select new PaymentSummary
                                                 {
                                                     PersonId = p.Person_Id,
                                                     Name = p.Name,
                                                     MatricNumber = p.Matric_Number,
                                                     SessionId = p.Session_Id,
                                                     SessionName = p.Session_Name,
                                                     FeeTypeId = p.Fee_Type_Id,
                                                     FeeTypeName = p.Fee_Type_Name,
                                                     LevelId = p.Level_Id,
                                                     LevelName = p.Level_Name,
                                                     ProgrammeId = p.Programme_Id,
                                                     ProgrammeName = p.Programme_Name,
                                                     DepartmentId = p.Department_Id,
                                                     DepartmentName = p.Department_Name,
                                                     FacultyId = p.Faculty_Id,
                                                     FacultyName = p.Faculty_Name,
                                                     TransactionDate = p.Transaction_Date,
                                                     TransactionAmount = p.Transaction_Amount,
                                                     RRR = p.RRR,
                                                     Status = p.Status,
                                                     PaymentEtranzactId = p.Payment_Id,
                                                     InvoiceNumber = p.Invoice_Number,
                                                     ConfirmationNumber = p.Confirmation_Number
                                                 }).ToList();

                payments = payments.Where(p => p.TransactionDate != null && p.TransactionDate >= dateFrom && p.TransactionDate <= dateTo).ToList();

                return payments;

            }
            catch (Exception)
            {
                throw;
            }
        }
        public bool ClearRRR(long paymentId)
        {
            try
            {
                Expression<Func<PAYMENT, bool>> selector = p => p.Payment_Id == paymentId;
                Payment payment = GetModelBy(selector);
                RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                OnlinePaymentLogic onlinePaymentLogic = new OnlinePaymentLogic();
                TranscriptRequestLogic transcriptRequestLogic = new TranscriptRequestLogic();
                FeeinPaymentLogic feeinPaymentLogic = new FeeinPaymentLogic();
                if (payment?.Id > 0)
                {
                    remitaPaymentLogic.DeleteBy(payment.Id);
                    onlinePaymentLogic.DeleteBy(payment.Id);
                    transcriptRequestLogic.DeleteBy(payment.Id);
                    feeinPaymentLogic.DeleteBy(payment.Id);
                    DeleteBy(paymentId);

                    return true;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return false;

        }

        public List<FeePaymentVerification> GetPaymentConfirmation(Session session, Programme programme, FeeType feeType, Department department, Level level, DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                var date__init = Convert.ToDateTime("01/01/0001");
                //if(dateFrom == date__init && (feeType.Id == 18 || feeType.Id == 14))
                //{
                //    SetMessage("Error occurred! " + ex.Message, Message.Category.Error);
                //    return null;
                //}
                if (dateFrom != date__init && feeType.Id > 0 && (session == null || session.Id <= 0))
                {
                    // var date__init = Convert.ToDateTime("01/01/0001");
                    var paymentsNew = (from sr in repository.GetBy<VW_PAYMENT_CONFIRMATION_NEW_STUDENTS>(x => x.Fee_Type_Id == feeType.Id && x.Programme_Id == programme.Id && x.Department_Id == department.Id && (x.Status.Contains("01") || x.Status.Contains("manual")))
                                       select new FeePaymentVerification
                                       {
                                           fullname = sr.Name,
                                           matricNumber = "-",
                                           rrr = sr.RRR,
                                           transactionDate = sr.Transaction_Date.ToLongDateString(),
                                           queryDate = sr.Transaction_Date,
                                           amount = sr.Transaction_Amount.ToString(),
                                           paymentId = sr.Payment_Id,
                                           invoiceNumber = sr.Invoice_Number,
                                           description = sr.Description
                                       }).OrderBy(x => x.queryDate).ToList();

                    if (paymentsNew.Count > 0)
                    {
                        if (dateFrom != date__init)
                        {
                            var _filter = paymentsNew.Where(x => x.queryDate >= dateFrom && x.queryDate <= dateTo).ToList();
                            paymentsNew = RemoveDuplicatesRecords(_filter);
                        }
                        else
                        {
                            return RemoveDuplicatesRecords(paymentsNew);
                        }


                    }
                    return paymentsNew;
                }
                //Acceptance Fees
                else if (feeType.Id == (int)FeeTypes.HNDAcceptance || feeType.Id == (int)FeeTypes.AcceptanceFee)
                {
                    var paymentsNew = new List<FeePaymentVerification>();
                    //var date__init = Convert.ToDateTime("01/01/0001");
                    if (dateFrom != date__init)
                    {
                        paymentsNew = (from sr in repository.GetBy<VW_PAYMENT_CONFIRMATION_NEW_STUDENTS>(x => x.Session_Id == session.Id && x.Fee_Type_Id == feeType.Id && x.Programme_Id == programme.Id && x.Department_Id == department.Id && (x.Status.Contains("01") || x.Status.Contains("manual")))
                                       select new FeePaymentVerification
                                       {
                                           fullname = sr.Name,
                                           matricNumber = "-",
                                           rrr = sr.RRR,
                                           transactionDate = sr.Transaction_Date.ToLongDateString(),
                                           queryDate = sr.Transaction_Date,
                                           amount = sr.Transaction_Amount.ToString(),
                                           paymentId = sr.Payment_Id,
                                           invoiceNumber = sr.Invoice_Number,
                                           description = sr.Description
                                       })
                                     //.OrderBy(x => x.queryDate)
                                     .ToList();
                    }
                    else
                    {
                        paymentsNew = (from sr in repository.GetBy<VW_PAYMENT_CONFIRMATION_NEW_STUDENTS>(x => x.Session_Id == session.Id && x.Fee_Type_Id == feeType.Id && x.Programme_Id == programme.Id && x.Department_Id == department.Id && (x.Status.Contains("01") || x.Status.Contains("manual")))
                                       select new FeePaymentVerification
                                       {
                                           fullname = sr.Name,
                                           matricNumber = "-",
                                           rrr = sr.RRR,
                                           transactionDate = sr.Transaction_Date.ToLongDateString(),
                                           queryDate = sr.Transaction_Date,
                                           amount = sr.Transaction_Amount.ToString(),
                                           paymentId = sr.Payment_Id,
                                           invoiceNumber = sr.Invoice_Number,
                                           description = sr.Description
                                       })
                                      //.OrderBy(x => x.queryDate)
                                      .ToList();
                    }


                    //if (paymentsNew.Count > 0)
                    //{
                    //    if (dateFrom != date__init)
                    //    {
                    //        var _filter = paymentsNew.Where(x => x.queryDate >= dateFrom && x.queryDate <= dateTo).ToList();
                    //        paymentsNew = RemoveDuplicatesRecords(_filter);
                    //    }
                    //    else
                    //    {
                    //        return RemoveDuplicatesRecords(paymentsNew);
                    //    }


                    //}
                    return paymentsNew;
                }
                //Other Fees
                else
                {
                    // var date__init = Convert.ToDateTime("01/01/0001");
                    var paymentsNew = (from sr in repository.GetBy<VW_PAYMENT_CONFIRMATION>(x => x.Session_Id == session.Id && x.Fee_Type_Id == feeType.Id && x.Programme_Id == programme.Id && x.Department_Id == department.Id && (x.Status.Contains("01") || x.Status.Contains("manual")))
                                       select new FeePaymentVerification
                                       {
                                           fullname = sr.Name,
                                           matricNumber = sr.Matric_Number != null ? sr.Matric_Number : "-",
                                           rrr = sr.RRR,
                                           transactionDate = sr.Transaction_Date.ToLongDateString(),
                                           queryDate = sr.Transaction_Date,
                                           amount = sr.Transaction_Amount.ToString(),
                                           paymentId = sr.Payment_Id,
                                           invoiceNumber = sr.Invoice_Number,
                                           description = sr.Description
                                       })
                                       //.OrderBy(x => x.queryDate)
                                       .ToList();

                    if (paymentsNew.Count > 0)
                    {
                        if (dateFrom != date__init)
                        {
                            var _filter = paymentsNew.Where(x => x.queryDate >= dateFrom && x.queryDate <= dateTo).ToList();
                            paymentsNew = RemoveDuplicatesRecords(_filter);
                        }
                        else
                        {
                            return RemoveDuplicatesRecords(paymentsNew);
                        }


                    }
                    return paymentsNew;
                }
                //if(level != null && level.Id > 0)
                //{


            }
            // }
            //else
            //{
            //    var date__init = Convert.ToDateTime("01/01/0001");
            //    var paymentsNew = (from sr in repository.GetBy<VW_PAYMENT_CONFIRMATION>(x => x.Session_Id == session.Id && x.Fee_Type_Id == feeType.Id && x.Programme_Id == programme.Id && x.Department_Id == department.Id && (x.Status.Contains("01") || x.Status.Contains("manual")))
            //                       select new FeePaymentVerification
            //                       {
            //                           fullname = sr.Name,
            //                           matricNumber = sr.Matric_Number != null ? sr.Matric_Number : "-",
            //                           rrr = sr.RRR,
            //                           transactionDate = sr.Transaction_Date.ToLongDateString(),
            //                           queryDate = sr.Transaction_Date,
            //                           amount = sr.Transaction_Amount.ToString(),
            //                           paymentId = sr.Payment_Id,
            //                           invoiceNumber = sr.Invoice_Number,
            //                           description = sr.Description
            //                       }).OrderBy(x => x.queryDate).ToList();

            //    if (paymentsNew.Count > 0)
            //    {
            //        if (dateFrom != date__init)
            //        {
            //            var _filter = paymentsNew.Where(x => x.queryDate >= dateFrom && x.queryDate <= dateTo).ToList();
            //            paymentsNew = RemoveDuplicatesRecords(_filter);
            //        }
            //        else
            //        {
            //            return RemoveDuplicatesRecords(paymentsNew);
            //        }


            //    }

            //    return paymentsNew;
            //}

            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<FeePaymentVerification> GetVerifiedPayments(Faculty faculty, Level level, Session session, Programme programme, FeeType feeType, Department department, DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                var date__init = Convert.ToDateTime("01/01/0001");
                var paymentsNew = (from sr in repository.GetBy<VW_SCHOOL_FEES_VERIFICATION>(x => x.Session_Id == session.Id && x.Fee_Type_Id == feeType.Id && x.Programme_Id == programme.Id && x.Faculty_Id == faculty.Id && x.Level_Id == level.Id)
                                   select new FeePaymentVerification
                                   {
                                       fullname = sr.Name,
                                       //matricNumber = sr.Matric_Number != null ? sr.Matric_Number : "-",
                                       rrr = sr.RRR,
                                       verifiedDate = sr.DateVerified.ToLongDateString(),
                                       queryDate = sr.DateVerified,
                                       amount = sr.Transaction_Amount.ToString(),
                                       paymentId = sr.Payment_Id,
                                       invoiceNumber = sr.Invoice_Number,
                                       departmentName = sr.Department_Name
                                   }).OrderBy(x => x.queryDate).ToList();

                if (paymentsNew.Count > 0)
                {
                    if (dateFrom != date__init)
                    {
                        var _filter = paymentsNew.Where(x => x.queryDate >= dateFrom && x.queryDate <= dateTo).ToList();
                        paymentsNew = RemoveDuplicatesRecords(_filter);
                    }
                    else
                    {
                        return RemoveDuplicatesRecords(paymentsNew);
                    }


                }

                return paymentsNew;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<FeePaymentVerification> GetVerifiedAcceptance(Faculty faculty, Session session, Programme programme, FeeType feeType, Department department, DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                var date__init = Convert.ToDateTime("01/01/0001");
                var paymentsNew = (from sr in repository.GetBy<VW_ACCEPTANCE_VERIFICATION>(x => x.Session_Id == session.Id && x.Fee_Type_Id == feeType.Id && x.Programme_Id == programme.Id && x.Faculty_Id == faculty.Id)
                                   select new FeePaymentVerification
                                   {
                                       fullname = sr.Name,
                                       //matricNumber = sr.Matric_Number != null ? sr.Matric_Number : "-",
                                       rrr = sr.RRR,
                                       verifiedDate = sr.DateVerified.ToLongDateString(),
                                       queryDate = sr.DateVerified,
                                       amount = sr.Transaction_Amount.ToString(),
                                       paymentId = sr.Payment_Id,
                                       invoiceNumber = sr.Invoice_Number,
                                       departmentName = sr.Department_Name
                                   }).OrderBy(x => x.queryDate).ToList();

                if (paymentsNew.Count > 0)
                {
                    if (dateFrom != date__init)
                    {
                        var _filter = paymentsNew.Where(x => x.queryDate >= dateFrom && x.queryDate <= dateTo).ToList();
                        paymentsNew = RemoveDuplicatesRecords(_filter);
                    }
                    else
                    {
                        return RemoveDuplicatesRecords(paymentsNew);
                    }


                }

                return paymentsNew;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<FeePaymentVerification> RemoveDuplicatesRecords(List<FeePaymentVerification> items)
        {
            List<FeePaymentVerification> returnList = new List<FeePaymentVerification>();
            foreach (var item in items)
            {
                var isDuplicate = returnList.Where(x => x.fullname == item.fullname).ToList();
                if (isDuplicate == null || isDuplicate.Count <= 0)
                {
                    returnList.Add(item);
                }
            }
            return returnList;
        }
        public List<FeeDetail> FormatReceiptBreakDown(List<FeeDetail> feeDetails, decimal amountPaid)
        {
            List<int> resolveFee = new List<int>()
            {
                6000, //CISCO
                10000 //ROBOTICS
            };
            var feeDetailAmount = feeDetails.Sum(f => f.Fee.Amount);
            var differenceInAmount = feeDetailAmount - amountPaid;
            if (differenceInAmount == resolveFee.Sum())
            {
                foreach(var item in resolveFee)
                {
                    feeDetails.Remove((feeDetails.Where(f => f.Fee.Amount == item).LastOrDefault()));
                }
                return feeDetails;
            }
            if (differenceInAmount > 0)
            {
                feeDetails.Remove((feeDetails.Where(f => f.Fee.Amount == differenceInAmount).LastOrDefault()));
                return feeDetails;
            }
            //else if(amountPaid > feeDetailAmount)
            //{
            //    differenceInAmount = amountPaid - feeDetailAmount;
            //    if(differenceInAmount > 0)
            //    {
            //        feeDetails.Remove((feeDetails.Where(f => f.Fee.Amount == differenceInAmount).LastOrDefault()));
            //        return feeDetails;
            //    }
            //}
            return feeDetails;
        }

        public bool ClearInvoiceForRegeneration(Session session, FeeType feeType, Person person)
        {
            
            var payment = GetModelsBy(f => f.Session_Id == session.Id && f.Person_Id == person.Id && f.Fee_Type_Id == feeType.Id).FirstOrDefault();
            if (payment?.Id > 0)
            {
                RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                var remitaPayment = remitaPaymentLogic.GetModelBy(f => f.Payment_Id == payment.Id);
                if (remitaPayment != null && (remitaPayment.Status.Contains("01") || remitaPayment.Status.Contains("00:") || remitaPayment.Status.Contains("998")))
                {
                    return false;
                }
                else if (remitaPayment != null && (!remitaPayment.Status.Contains("01") && !remitaPayment.Status.Contains("00:") && !remitaPayment.Status.Contains("998")))
                {
                    RemitaSettingsLogic settingsLogic = new RemitaSettingsLogic();
                    var settings = settingsLogic.GetBy(2);
                    RemitaPayementProcessor remitaPayementProcessor = new RemitaPayementProcessor(settings.Api_key);
                    var paymentStatus = remitaPayementProcessor.GetStatus(remitaPayment);
                    if (paymentStatus != null && (paymentStatus.Status.Contains("01") || paymentStatus.Status.Contains("00:") || paymentStatus.Status.Contains("998")))
                    {
                        return false;
                        //do nothing
                    }
                    else
                    {
                        RegenerateClearedInvoiceLogic regenerateClearedInvoiceLogic = new RegenerateClearedInvoiceLogic();
                        var isSaved = regenerateClearedInvoiceLogic.SaveClearedInvoice(payment, remitaPayment);
                        if (isSaved)
                        {
                            ClearRRR(payment.Id);
                            return true;
                        }

                    }
                }
            }
            return false;
        }


        public Payment ValidatePayWithCardDispute(string RegNumber, bool isStudent)
        {
            try
            {
                RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                if (isStudent)
                {
                    StudentLogic studentLogic = new StudentLogic();
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
                        var getPayment = remitaPaymentLogic.GetModelsBy(x => x.PAYMENT.Fee_Type_Id == 3 && x.PAYMENT.Session_Id == 12 && x.PAYMENT.Person_Id == getPerson.Person.Id
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

    }


}

