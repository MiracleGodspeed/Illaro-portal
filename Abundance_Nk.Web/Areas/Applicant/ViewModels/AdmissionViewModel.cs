using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Abundance_Nk.Model.Model;
using Abundance_Nk.Business;
using System.Transactions;
using System.ComponentModel.DataAnnotations;
using Abundance_Nk.Model.Entity.Model;
using Abundance_Nk.Web.Models;
using System.Web.Mvc;

namespace Abundance_Nk.Web.Areas.Applicant.ViewModels
{
    public class AdmissionViewModel : OLevelResultViewModel
    {
        private PaymentLogic paymentLogic;
        private PaymentEtranzactLogic paymentEtranzactLogic;
        private PaymentEtranzactTypeLogic paymentEtranzactTypeLogic;
        private ApplicantLogic applicantLogic;
        private AppliedCourseLogic appliedCourseLogic;
        private ApplicationFormLogic applicationFormLogic;
        private OnlinePaymentLogic onlinePaymentLogic;
        private AdmissionListLogic admissionListLogic;
        private RemitaPaymentLogic remitaPaymentLogic;
        public AdmissionViewModel()
        {
            ApplicationForm = new ApplicationForm();
            ApplicationForm.Person = new Person();

            Applicant = new Abundance_Nk.Model.Model.Applicant();
            Applicant.Status = new ApplicantStatus();
            
            AppliedCourse = new AppliedCourse();
            AppliedCourse.Programme = new Programme();
            AppliedCourse.Department = new Department();
            admissionList = new AdmissionList();
            admissionList.Deprtment = new Department();
            Payment = new Payment();
            paymentEtranzactLogic = new PaymentEtranzactLogic();
            paymentEtranzactTypeLogic = new PaymentEtranzactTypeLogic();

            Invoice = new Invoice();
            Invoice.Payment = new Payment();
            Invoice.Payment.FeeType = new FeeType();
            //Invoice.Payment.Fee.Type = new FeeType();
            Invoice.Person = new Person();

            paymentLogic = new PaymentLogic();
            appliedCourseLogic = new AppliedCourseLogic();
            applicationFormLogic = new ApplicationFormLogic();
            applicantLogic = new ApplicantLogic();
            onlinePaymentLogic = new OnlinePaymentLogic();
            admissionListLogic = new AdmissionListLogic();

            ScratchCard = new ScratchCard();
            ChangeOfCourseApplies = false;
            OptionSelectList = Utility.PopulateOptionSelectListItem();
        }

        public bool Loaded { get; set; }
        public bool IsDriversLicenseInvoiceGenerated { get; set; }
        public bool IsDriversLicenseReceiptGenerated { get; set; }
        public ScratchCard ScratchCard { get; set; }
        public Remita remita { get; set; }
        public RemitaPayment remitaPayment { get; set; }
        public RemitaResponse remitaResponse { get; set; }
        public PaymentMode PaymentMode { get; set; }
        public Receipt Receipt { get; set; }
        public Invoice Invoice { get; set; }
        public Abundance_Nk.Model.Model.Applicant Applicant { get; set; }
        public ApplicationForm ApplicationForm { get; set; }
        public AppliedCourse AppliedCourse { get; set; }
        public AdmissionList admissionList { get; set; }
        public long DepartmentOptionId { get; set; } = 0;
        public Model.Model.Student Student { get; set; }
        
        public Payment Payment { get; set; }
        public bool IsAdmitted { get; set; }
        public int ApplicantStatusId { get; set; }

        [Display(Name = "Acceptance Confirmation Number")]
        public string AcceptanceConfirmationOrderNumber { get; set; }

        [Display(Name = "School Fees Confirmation Number")]
        public string SchoolFeesConfirmationOrderNumber { get; set; }


        [Display(Name = "Drivers License Confirmation Number")]
        public string DriversLicenseConfirmationOrderNumber { get; set; }

        [Display(Name = "Acceptance Invoice Number")]
        public string AcceptanceInvoiceNumber { get; set; }

        public bool ChangeOfCourseApplies { get; set; }

        [Display(Name = "Acceptance Receipt Number")]
        public string AcceptanceReceiptNumber { get; set; }

        [Display(Name = "School Fees Invoice Number")]
        public string SchoolFeesInvoiceNumber { get; set; }

        [Display(Name = "Drivers License Invoice Number")]
        public string DriversLicenseInvoiceNumber { get; set; }
        
        [Display(Name = "School Fees Receipt Number")]
        public string SchoolFeesReceiptNumber { get; set; }

        [Display(Name = "Drivers License Receipt Number")]
        public string DriversLicenseReceiptNumber { get; set; }

        public string ConfirmationOrderNumber { get; set; }
        [Display(Name = "Hostel Confirmation Number")]
        public string HostelConfirmationOrderNumber { get; set; }
        [Display(Name = "Hostel Invoice Number")]
        public string HostelInvoiceNumber { get; set; }
        [Display(Name = "Include Robotic")]
        public string IncludeRoboticFee { get; set; }
        [Display(Name = "Include CISCO")]
        public string IncludeCISCOFee { get; set; }
        [Display(Name = "RegenerateInvoice")]
        public string RegenerateInvoice { get; set; }
        public List<SelectListItem> OptionSelectList { get; set; }
        public void GetApplicationBy(long formId)
        {
            try
            {
                ApplicantLogic applicantLogic = new ApplicantLogic();               
                remitaPayment = new RemitaPayment();
                remitaPaymentLogic = new RemitaPaymentLogic();
                ApplicationForm = applicationFormLogic.GetModelBy(a => a.Application_Form_Id == formId);
                if (ApplicationForm != null && ApplicationForm.Id > 0)
                {
                    AppliedCourse = appliedCourseLogic.GetModelBy(f => f.Application_Form_Id == ApplicationForm.Id);
                    admissionList = admissionListLogic.GetBy(ApplicationForm.Id);
                    Payment = paymentLogic.GetModelBy(p => p.Payment_Id == ApplicationForm.Payment.Id);
                    Applicant = applicantLogic.GetModelBy(a => a.Application_Form_Id == ApplicationForm.Id);
                    IsAdmitted = admissionListLogic.IsAdmitted(ApplicationForm);

                    if (Applicant != null && Applicant.Status != null)
                    {
                        ApplicantStatusId = Applicant.Status.Id;
                    }

                    if (admissionList != null && AppliedCourse.Department != admissionList.Deprtment)
                    {
                        ChangeOfCourseApplies = true;
                    }
                    if(AppliedCourse != null && AppliedCourse.Programme != null && (AppliedCourse.Programme.Id == (int)Programmes.HNDDistance || AppliedCourse.Programme.Id == (int)Programmes.NDDistance))
                    {
                        PaymentModeSelectList = Utility.PopulatePaymentModeSelectListItemDistantLearning(Payment);

                    }

                    //get acceptance payment
                    FeeType acceptanceFee = new FeeType() { Id = (int)FeeTypes.AcceptanceFee };
                    FeeType driversLicenseFee = new FeeType() { Id = (int)FeeTypes.DriversLicense };
                    Payment acceptancePayment = paymentLogic.GetBy(ApplicationForm.Person, acceptanceFee);
                    Payment driversLicensePayment = paymentLogic.GetBy(ApplicationForm.Person, driversLicenseFee);

                    if (acceptancePayment == null && ApplicationForm.ProgrammeFee.Programme.Id != (int)Programmes.DrivingCertificate)
                    {
                        acceptanceFee = new FeeType() { Id = 9 };
                        acceptancePayment = paymentLogic.GetBy(ApplicationForm.Person, acceptanceFee);
                    }
                    else if (acceptancePayment == null && ApplicationForm.ProgrammeFee.Programme.Id == (int)Programmes.DrivingCertificate)
                    {
                        acceptanceFee = new FeeType() { Id = (int)FeeTypes.DrivingAcceptance };
                        acceptancePayment = paymentLogic.GetBy(ApplicationForm.Person, acceptanceFee);
                    }
                    //Check for Driving Course Option to ascertain acceptance payment or not
                    if(admissionList != null &&  admissionList.DepartmentOption?.Id == 16)
                    {
                        Applicant.Status = new ApplicantStatus() { Id = 4, Name="Cleared and Accepted" };
                        applicantLogic.Modify(Applicant);
                        AcceptanceConfirmationOrderNumber = "Not Applicable";
                    }
                    else if(acceptancePayment != null )
                    {
                        AcceptanceInvoiceNumber = acceptancePayment.InvoiceNumber;
                        remitaPayment = remitaPaymentLogic.GetBy(acceptancePayment.Id);
                        PaymentModeSelectList = Utility.PopulatePaymentModeSelectListItemDistantLearning(acceptancePayment);


                        if (remitaPayment != null && remitaPayment.Status.Contains("01"))
                        {
                            AcceptanceReceiptNumber = remitaPayment.OrderId;
                            if (ApplicantStatusId >= (int)ApplicantStatus.Status.ClearedAndAccepted)
                            {
                                AcceptanceConfirmationOrderNumber = remitaPayment.RRR;
                            }
                        }

                    }

                    if(driversLicensePayment != null)
                    {
                        DriversLicenseInvoiceNumber = driversLicensePayment.InvoiceNumber;
                    }


                    //get school fees payment
                    FeeType schoolFees = new FeeType() { Id = (int)FeeTypes.SchoolFees };
                    Payment schoolFeesPayment = paymentLogic.GetBy(ApplicationForm.Person, schoolFees);
                    if (schoolFeesPayment != null)
                    {
                        SchoolFeesInvoiceNumber = schoolFeesPayment.InvoiceNumber;
                        remitaPayment = remitaPaymentLogic.GetBy(schoolFeesPayment.Id);
                        if (remitaPayment != null && remitaPayment.Status.Contains("01"))
                        {
                            SchoolFeesReceiptNumber = remitaPayment.OrderId;
                            if (ApplicantStatusId >= (int)ApplicantStatus.Status.GeneratedSchoolFeesReceipt)
                            {
                                SchoolFeesConfirmationOrderNumber = remitaPayment.RRR;
                            }
                        }

                     
                    }
                    // get hostel fee payment
                    FeeType hostelFees = new FeeType() { Id = (int)FeeTypes.HostelFee };
                    Payment hostelFeePayment = paymentLogic.GetModelsBy(g => g.Fee_Type_Id == hostelFees.Id && g.Person_Id==ApplicationForm.Person.Id).LastOrDefault();
                    if (hostelFeePayment != null)
                    {
                        HostelInvoiceNumber = hostelFeePayment.InvoiceNumber;
                        var hostelpayment=remitaPaymentLogic.GetBy(hostelFeePayment.Id);
                        if(hostelpayment!=null && ( hostelpayment.Status.Contains("01:")|| hostelpayment.Description.Contains("Manual")))
                        {
                            HostelConfirmationOrderNumber = hostelpayment.RRR;
                        }
                    }

                    Loaded = true;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ApplicationForm GetApplicationFormBy(string formNumber)
        {
            try
            {
                return applicationFormLogic.GetModelBy(f => f.Application_Form_Number == formNumber);
            }
            catch(Exception)
            {
                throw;
            }
        }

        public void GetInvoiceBy(string invoiceNumber)
        {
            try
            {
                List<int> displayFee = new List<int>() { 169, 170, 172, 173, 174, 175, 171 };
                Payment payment = paymentLogic.GetBy(invoiceNumber);
                AdmissionList list = new AdmissionList();
                StudentLevel studentLevel = new StudentLevel();
                StudentLevelLogic levelLogic = new StudentLevelLogic();
                Department department = new Department();
                AppliedCourse appliedCourse = new AppliedCourse();
                AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
                appliedCourse = appliedCourseLogic.GetBy(payment.Person);
                if (payment.FeeType.Id == (int)FeeTypes.SchoolFees || payment.FeeType.Id == (int)FeeTypes.AcceptanceFee || payment.FeeType.Id == (int)FeeTypes.HNDAcceptance || payment.FeeType.Id == (int)FeeTypes.DrivingAcceptance || payment.FeeType.Id == (int)FeeTypes.DriversLicense)
                {
                    studentLevel = levelLogic.GetBy(payment.Person.Id);
                    if (studentLevel != null)
                    {
                        department = studentLevel.Department;
                        payment.FeeDetails = paymentLogic.SetFeeDetails(payment, studentLevel.Programme.Id, studentLevel.Level.Id, 1, studentLevel.Department.Id, studentLevel.Session.Id);
                    }
                    else
                    {
                        
                        AdmissionListLogic listLogic = new AdmissionListLogic();
                        list = listLogic.GetBy(payment.Person);
                        if (list != null)
                        {
                            Level level = new Level();
                            level = SetLevel(list.Form.ProgrammeFee.Programme);
                            department = list.Deprtment;
                            if(appliedCourse.Programme.Id == (int)Programmes.HNDDistance || appliedCourse.Programme.Id == (int)Programmes.NDDistance)
                            {
                                payment.FeeDetails = paymentLogic.SetFeeDetails(payment, list.Form.ProgrammeFee.Programme.Id, level.Id, payment.PaymentMode.Id, list.Deprtment.Id, list.Form.Setting.Session.Id);
                            }
                            else
                            {
                                payment.FeeDetails = paymentLogic.SetFeeDetails(payment, list.Form.ProgrammeFee.Programme.Id, level.Id, 1, list.Deprtment.Id, list.Form.Setting.Session.Id);
                            }
                            
                           
                        }
                    }

                }
                else if (payment.FeeType.Id == (int)FeeTypes.ChangeOfCourseFees)
                {   
                    

                    if (appliedCourse != null)
                    {
                        Level level = new Level();
                        level = SetLevel(appliedCourse.Programme);
                        department = appliedCourse.Department;
                        payment.FeeDetails = paymentLogic.SetFeeDetails(payment, appliedCourse.Programme.Id, level.Id, 1, appliedCourse.Department.Id, appliedCourse.ApplicationForm.Setting.Session.Id);
                      
                    }
                }
               

                Invoice = new Invoice();
                Invoice.Payment = payment;
                Invoice.PaymentMode = payment.PaymentMode;
                Invoice.Person = payment.Person;
                Invoice.JambRegistrationNumber ="";
                Invoice.Department = department;
                Invoice.Programme = appliedCourse != null ? appliedCourse.Programme : null;
             
                //Show some fee of the Feetype on the invoice
                Invoice.FeeDetails = payment.FeeDetails.Where(f => displayFee.Contains(f.Fee.Id)).ToList();
                Invoice.Amount = payment.FeeDetails.Sum(f => f.Fee.Amount);
                //Preloaded amounts
                if(list.Deprtment != null && list.Deprtment.Id == 44 && payment.FeeType.Id == (int)FeeTypes.SchoolFees)
                {
                    if(list.DepartmentOption.Id == 16)
                    {
                        Invoice.Amount = Convert.ToDecimal(Fees.GraduateDrivingTrainingSchoolFees);
                    }
                    else if (list.DepartmentOption.Id == 17)
                    {
                        Invoice.Amount = Convert.ToDecimal(Fees.TechnicalCertificateDrivingSchooFees);
                    }
                    else if (list.DepartmentOption.Id == 18)
                    {
                        Invoice.Amount = Convert.ToDecimal(Fees.ProfessionalDiplomaDrivingSchooFees);
                    }
                }
                else if (Invoice.FeeDetails.Count > 0)
                {
                    var feeAmmount = Invoice.FeeDetails.Sum(f => f.Fee.Amount);
                    Invoice.Amount = Invoice.Amount - feeAmmount;
                }

                remitaPayment = new RemitaPayment();
                remitaPaymentLogic = new RemitaPaymentLogic();
                remitaPayment = remitaPaymentLogic.GetBy(payment.Id);
                if (remitaPayment != null)
                {
                    Invoice.remitaPayment = remitaPayment;
                }

                studentLevel = levelLogic.GetBy(payment.Person.Id);

                if (payment.FeeType.Id == (int)FeeTypes.SchoolFees || payment.FeeType.Id == (int)FeeTypes.AcceptanceFee || payment.FeeType.Id == (int)FeeTypes.HNDAcceptance)
                {
                    AdmissionList admissionList = new AdmissionList();
                    AdmissionListLogic admissionListLogic = new AdmissionListLogic();
                    admissionList = admissionListLogic.GetBy(payment.Person);
                    if (admissionList != null && studentLevel == null)
                    {
                        Level thisLevel = new Level();
                        thisLevel = SetLevel(admissionList.Form.ProgrammeFee.Programme);
                        Invoice.paymentEtranzactType = paymentEtranzactTypeLogic.GetModelBy(p => p.Fee_Type_Id == payment.FeeType.Id && p.Level_Id == thisLevel.Id && p.Programme_Id == admissionList.Form.ProgrammeFee.Programme.Id && p.Session_Id == payment.Session.Id);
                    }
                    if (studentLevel != null)
                    {
                        Invoice.paymentEtranzactType = paymentEtranzactTypeLogic.GetModelBy(p => p.Fee_Type_Id == payment.FeeType.Id && p.Level_Id == studentLevel.Level.Id && p.Programme_Id == studentLevel.Programme.Id && p.Session_Id == payment.Session.Id);
                    }
                    #region//pick robotic or cosmotic fee if added
                    if (payment.FeeType.Id == (int)FeeTypes.SchoolFees && admissionList.Form.ProgrammeFee.Programme.Id != (int)Programmes.HNDDistance && admissionList.Form.ProgrammeFee.Programme.Id != (int)Programmes.NDDistance)
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
                        var feeAmmount = Invoice.FeeDetails.Sum(f => f.Fee.Amount);
                        var totalSum=payment.FeeDetails.Sum(f => f.Fee.Amount);
                        Invoice.Amount = totalSum - feeAmmount;
                    }
                    #endregion
                }
                else if (payment.FeeType.Id == (int)FeeTypes.ChangeOfCourseFees)
                {
                    Invoice.paymentEtranzactType = paymentEtranzactTypeLogic.GetModelBy(p => p.Fee_Type_Id == payment.FeeType.Id && p.Session_Id == payment.Session.Id);
                }
                else
                {
                    Invoice.paymentEtranzactType = paymentEtranzactTypeLogic.GetBy(payment.FeeType); 
                }   

                AcceptanceInvoiceNumber = payment.InvoiceNumber;
            }
            catch (Exception ex)
            {
                throw ex;
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
                    case 5:
                        {
                            return level = new Level() { Id = 3 };

                        }
                    case 7:
                        {
                            return level = new Level() { Id = 1 };

                        }
                    case 8:
                        {
                            return level = new Level() { Id = 3 };

                        }
                    case 9:
                        {
                            return level = new Level() { Id = 1 };

                        }
                }
                return level = new Level();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Receipt GetReceiptBy(string invoiceNumber)
        {
            try
            {
                Payment payment = paymentLogic.GetBy(invoiceNumber);
                if (payment == null ||payment.Id <= 0)
                {
                    return null;
                }

                ApplicationForm applicationForm = applicationFormLogic.GetModelBy(ap => ap.Person_Id == payment.Person.Id);
                AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
                AdmissionListLogic admissionListLogic = new AdmissionListLogic();
                StudentLogic studentLogic = new StudentLogic();

                AdmissionList admissionList = admissionListLogic.GetModelsBy(a => a.APPLICATION_FORM.Person_Id == payment.Person.Id && a.Activated == true).LastOrDefault();

                var appliedCourse = appliedCourseLogic.GetModelBy(c => c.Person_Id == payment.Person.Id);
                var _student = studentLogic.GetModelBy(x => x.Person_Id == appliedCourse.Person.Id);

                if (admissionList != null)
                {
                    appliedCourse.Department = admissionList.Deprtment;
                    appliedCourse.Option = admissionList.DepartmentOption;
                }

                if (appliedCourse != null && applicationForm != null)
                {
                    Level level = SetLevel(appliedCourse.Programme);

                    payment.FeeDetails = paymentLogic.SetFeeDetails(payment, appliedCourse.Programme.Id, level.Id, payment.PaymentMode.Id, appliedCourse.Department.Id, payment.Session.Id);

                    if (payment.FeeDetails == null || payment.FeeDetails.Count <= 0 && payment.FeeType.Id != (int)FeeTypes.ShortFall && appliedCourse.Department.Id != 44)
                    {
                        throw new Exception("Fee Details for " + payment.FeeType.Name + " not set! please contact your system administrator.");
                    }

                    paymentEtranzactLogic = new PaymentEtranzactLogic();
                    PaymentEtranzact paymentEtranzact = paymentEtranzactLogic.GetModelBy(o => o.Payment_Id == payment.Id);
                    if (paymentEtranzact != null)
                    {
                        payment.DatePaid = Convert.ToDateTime(paymentEtranzact.TransactionDate);

                        decimal amount = (decimal)paymentEtranzact.TransactionAmount;
                        Receipt = BuildReceipt(payment, paymentEtranzact.ReceiptNo, paymentEtranzact.ConfirmationNo, amount, new Model.Model.Student { MatricNumber = applicationForm.Number },
                                                new StudentLevel { Programme = appliedCourse.Programme, Department = appliedCourse.Department, Level = level });
                    }
                    else
                    {
                        remitaPaymentLogic = new RemitaPaymentLogic();
                        RemitaPayment remitaPayment = remitaPaymentLogic.GetBy(payment.Id);
                        if (remitaPayment != null)
                        {
                            if (remitaPayment.Status.Contains("01") || remitaPayment.Description.Contains("manual"))
                            {
                                List<RemitaPayment> remitaShortfallPayment = remitaPaymentLogic.GetModelsBy(o => o.PAYMENT.Person_Id == payment.Person.Id && o.PAYMENT.Session_Id == payment.Session.Id &&
                                                                   o.PAYMENT.Fee_Type_Id == (int)FeeTypes.ShortFall && o.Status.Contains("01"));

                                string shortFallRRR = "";
                                if (remitaShortfallPayment != null && remitaShortfallPayment.Count > 0 && payment.FeeType.Id == (int)FeeTypes.SchoolFees)
                                {
                                    for (int i = 0; i < remitaShortfallPayment.Count; i++)
                                    {
                                        remitaPayment.TransactionAmount += remitaShortfallPayment[i].TransactionAmount;

                                        shortFallRRR += remitaShortfallPayment[i].RRR + ", ";
                                    }
                                }

                                payment.DatePaid = remitaPayment.TransactionDate;
                                if (payment.FeeType.Id == (int)FeeTypes.SchoolFees && appliedCourse.Programme.Id != (int)Programmes.HNDDistance && appliedCourse.Programme.Id != (int)Programmes.NDDistance)
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
                                decimal amount = remitaPayment.TransactionAmount;
                                Receipt = BuildReceipt(payment, remitaPayment.OrderId, remitaPayment.RRR, amount, _student != null && _student.MatricNumber != null ? new Model.Model.Student { MatricNumber = _student.MatricNumber } : new Model.Model.Student { MatricNumber = applicationForm.Number },
                                                    new StudentLevel { Programme = appliedCourse.Programme, Department = appliedCourse.Department, Level = level });

                                if (remitaPayment != null && remitaShortfallPayment != null && remitaShortfallPayment.Count > 0 && payment.FeeType.Id == (int)FeeTypes.SchoolFees)
                                    Receipt.ShortFallRRR = shortFallRRR;
                            }
                            else
                            {
                                RemitaPayementProcessor remitaPayementProcessor = new RemitaPayementProcessor("918567");
                                var getStatus = remitaPayementProcessor.GetStatus(remitaPayment.OrderId);
                                if (getStatus.Status.Contains("01:"))
                                {
                                    remitaPayment = getStatus;
                                    List<RemitaPayment> remitaShortfallPayment = remitaPaymentLogic.GetModelsBy(o => o.PAYMENT.Person_Id == payment.Person.Id && o.PAYMENT.Session_Id == payment.Session.Id &&
                                                                   o.PAYMENT.Fee_Type_Id == (int)FeeTypes.ShortFall && o.Status.Contains("01"));

                                    string shortFallRRR = "";
                                    if (remitaShortfallPayment != null && remitaShortfallPayment.Count > 0 && payment.FeeType.Id == (int)FeeTypes.SchoolFees)
                                    {
                                        for (int i = 0; i < remitaShortfallPayment.Count; i++)
                                        {
                                            remitaPayment.TransactionAmount += remitaShortfallPayment[i].TransactionAmount;

                                            shortFallRRR += remitaShortfallPayment[i].RRR + ", ";
                                        }
                                    }

                                    payment.DatePaid = remitaPayment.TransactionDate;

                                    decimal amount = remitaPayment.TransactionAmount;
                                    Receipt = BuildReceipt(payment, remitaPayment.OrderId, remitaPayment.RRR, amount, new Model.Model.Student { MatricNumber = applicationForm.Number },
                                                        new StudentLevel { Programme = appliedCourse.Programme, Department = appliedCourse.Department, Level = level });

                                    if (remitaPayment != null && remitaShortfallPayment != null && remitaShortfallPayment.Count > 0 && payment.FeeType.Id == (int)FeeTypes.SchoolFees)
                                        Receipt.ShortFallRRR = shortFallRRR;
                           
                                }

                            }
                        }
                    }
                }
                return Receipt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Payment GenerateInvoice(ApplicationForm applicationForm, ApplicantStatus.Status status, FeeType feeType)
        {
            try
            {
                Payment payment = new Payment();
                payment.PaymentMode = new PaymentMode() { Id = applicationForm.Setting.PaymentMode.Id };
                payment.PaymentType = new PaymentType() { Id = applicationForm.Setting.PaymentType.Id };
                payment.PersonType = new PersonType() { Id = applicationForm.Setting.PersonType.Id };
                payment.Person = applicationForm.Person;
                payment.DatePaid = DateTime.Now;
                payment.FeeType = feeType;
                payment.Session = applicationForm.Setting.Session;
                
                if (paymentLogic.PaymentAlreadyMade(payment))
                {
                    var applicant=applicantLogic.GetModelsBy(f => f.Application_Form_Id == applicationForm.Id).FirstOrDefault();
                    if(applicant?.Status.Id < (int)status)
                    {
                        applicantLogic.UpdateStatus(applicationForm, status);
                    }
                    return paymentLogic.GetBy(applicationForm.Person, feeType);
                }
                else
                {
                    Payment newPayment = null;
                    OnlinePayment newOnlinePayment = null;
                    using (TransactionScope transaction = new TransactionScope())
                    {
                        newPayment = paymentLogic.Create(payment);
                        
                        AdmissionList list = new AdmissionList();
                        AdmissionListLogic listLogic = new AdmissionListLogic();
                        FeeDetailLogic feeDetailLogic = new FeeDetailLogic();
                        list = listLogic.GetBy(applicationForm.Id);

                        int LevelId = GetLevel(applicationForm.ProgrammeFee.Programme.Id);

                        if (newPayment != null)
                        {
                            if(feeType.Id == (int)FeeTypes.SchoolFees && (applicationForm.ProgrammeFee.Programme.Id == (int)Programmes.NDDistance || applicationForm.ProgrammeFee.Programme.Id == (int)Programmes.HNDDistance))
                            {
                                //newPayment.FeeDetails = paymentLogic.SetFeeDetails(feeType);
                                newPayment.FeeDetails = paymentLogic.SetFeeDetails(newPayment, applicationForm.ProgrammeFee.Programme.Id, LevelId, payment.PaymentMode.Id, list.Deprtment.Id, applicationForm.Setting.Session.Id);
                            }
                            else if (feeType.Id == 3)
                            {
                                newPayment.FeeDetails = paymentLogic.SetFeeDetails(newPayment, applicationForm.ProgrammeFee.Programme.Id, LevelId, 1, list.Deprtment.Id, applicationForm.Setting.Session.Id);

                              

                            }
                            else if (applicationForm.ProgrammeFee.Programme.Id == (int)Programmes.NDDistance || applicationForm.ProgrammeFee.Programme.Id == (int)Programmes.HNDDistance)
                            {
                                newPayment.FeeDetails = paymentLogic.SetFeeDetails(feeType);
                            }
                            else if (feeType.Id == 2 && applicationForm.ProgrammeFee.Programme.Id > 1)
                            {
                                feeType.Id = 9;
                                newPayment.FeeDetails = paymentLogic.SetFeeDetails(feeType);
                            }

                            if (applicationForm.ProgrammeFee.Programme.Id == (int)Programmes.HNDWeekend)
                            {
                                newPayment.FeeDetails = paymentLogic.SetFeeDetails(newPayment, applicationForm.ProgrammeFee.Programme.Id, LevelId, 1, list.Deprtment.Id, applicationForm.Setting.Session.Id);
                            }

                            PaymentChannel channel = new PaymentChannel() { Id = (int)PaymentChannel.Channels.Etranzact };
                            OnlinePaymentLogic onlinePaymentLogic = new OnlinePaymentLogic();
                            OnlinePayment onlinePayment = new OnlinePayment();
                            onlinePayment.Channel = channel;
                            onlinePayment.Payment = newPayment;
                            newOnlinePayment = onlinePaymentLogic.Create(onlinePayment);
                        }

                        applicantLogic.UpdateStatus(applicationForm, status);
                        transaction.Complete();
                    }

                    return newPayment;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Receipt GenerateReceipt(string invoiceNumber, long formId, ApplicantStatus.Status status)
        {
            try
            {
                Payment payment = paymentLogic.GetBy(invoiceNumber);
                if (payment == null || payment.Id <= 0)
                {
                    return null;
                }

                Receipt receipt = null;
                ApplicationForm applicationForm = applicationFormLogic.GetBy(formId);
                AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
                AdmissionListLogic admissionListLogic = new AdmissionListLogic();

                AdmissionList admissionList = admissionListLogic.GetModelsBy(a => a.APPLICATION_FORM.Person_Id == payment.Person.Id && a.Activated == true).LastOrDefault();
                
                var appliedCourse = appliedCourseLogic.GetModelBy(c => c.Person_Id == payment.Person.Id);

                if (admissionList != null)
                {
                    appliedCourse.Department = admissionList.Deprtment;
                    appliedCourse.Option = admissionList.DepartmentOption;
                }

                if (appliedCourse != null && applicationForm != null)
                {
                    Level level = SetLevel(appliedCourse.Programme);

                    payment.FeeDetails = paymentLogic.SetFeeDetails(payment, appliedCourse.Programme.Id, level.Id, payment.PaymentMode.Id, appliedCourse.Department.Id, payment.Session.Id);

                    if (payment.FeeDetails == null || payment.FeeDetails.Count <= 0 && payment.FeeType.Id != (int)FeeTypes.ShortFall && admissionList.Deprtment.Id != 44)
                    {
                        throw new Exception("Fee Details for " + payment.FeeType.Name + " not set! please contact your system administrator.");
                    }

                    paymentEtranzactLogic = new PaymentEtranzactLogic();
                    PaymentEtranzact paymentEtranzact = paymentEtranzactLogic.GetModelBy(o => o.Payment_Id == payment.Id);
                    if (paymentEtranzact != null)
                    {
                        using (TransactionScope transaction = new TransactionScope())
                        {
                            bool updated = onlinePaymentLogic.UpdateTransactionNumber(payment, paymentEtranzact.ConfirmationNo);
                            applicantLogic.UpdateStatus(applicationForm, status);

                            transaction.Complete();
                        }

                        payment.DatePaid = Convert.ToDateTime(paymentEtranzact.TransactionDate);

                        decimal amount = (decimal)paymentEtranzact.TransactionAmount;
                        receipt = BuildReceipt(payment, paymentEtranzact.ReceiptNo, paymentEtranzact.ConfirmationNo, amount, new Model.Model.Student { MatricNumber = applicationForm.Number },
                                                new StudentLevel { Programme = appliedCourse.Programme, Department = appliedCourse.Department, Level = level });
                    }
                    else
                    {
                        remitaPaymentLogic = new RemitaPaymentLogic();
                        RemitaPayment remitaPayment = remitaPaymentLogic.GetBy(payment.Id);
                        if (remitaPayment != null && (remitaPayment.Status.Contains("01") || remitaPayment.Status.Contains("00") || remitaPayment.Description.Contains("manual")))                          
                        {
                            using (TransactionScope transaction = new TransactionScope())
                            {
                                bool updated = onlinePaymentLogic.UpdateTransactionNumber(payment, remitaPayment.RRR);
                                applicantLogic.UpdateStatus(applicationForm, status);

                                transaction.Complete();
                            }

                            List<RemitaPayment> remitaShortfallPayment = remitaPaymentLogic.GetModelsBy(o => o.PAYMENT.Person_Id == payment.Person.Id && o.PAYMENT.Session_Id == payment.Session.Id &&
                                                                   o.PAYMENT.Fee_Type_Id == (int)FeeTypes.ShortFall && o.Status.Contains("01"));

                            string shortFallRRR = "";
                            if (remitaShortfallPayment != null && remitaShortfallPayment.Count > 0 && payment.FeeType.Id == (int) FeeTypes.SchoolFees)
                            {
                                for (int i = 0; i < remitaShortfallPayment.Count; i++)
                                {
                                    remitaPayment.TransactionAmount += remitaShortfallPayment[i].TransactionAmount;

                                    shortFallRRR += remitaShortfallPayment[i].RRR + ", ";
                                }
                            }
                            //check for robotic and cisco optional fees
                            if (payment.FeeType.Id == (int)FeeTypes.SchoolFees && appliedCourse.Programme.Id != (int)Programmes.HNDDistance && appliedCourse.Programme.Id != (int)Programmes.NDDistance)
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
                            payment.DatePaid = remitaPayment.TransactionDate;

                            decimal amount = remitaPayment.TransactionAmount;
                            receipt = BuildReceipt(payment, remitaPayment.OrderId, remitaPayment.RRR, amount, new Model.Model.Student { MatricNumber = applicationForm.Number },
                                                new StudentLevel { Programme = appliedCourse.Programme, Department = appliedCourse.Department, Level = level });

                            if (remitaPayment != null && remitaShortfallPayment != null && remitaShortfallPayment.Count > 0 && payment.FeeType.Id == (int)FeeTypes.SchoolFees)
                                receipt.ShortFallRRR = shortFallRRR;
                        }
                    }

                  
                }
                
                return receipt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public Receipt BuildReceipt(Payment payment, string Number, string CON, decimal amount, Model.Model.Student student, StudentLevel studentLevel)
        {
            try
            {
                Receipt receipt = new Receipt();
                //receipt.Number = Number;
                receipt.Number = payment.Id.ToString();
                receipt.Name = payment.Person.FullName;
                receipt.ConfirmationOrderNumber = CON;
                receipt.Amount = amount;
                receipt.AmountInWords = "";
                receipt.Purpose = payment.FeeType.Name;
                receipt.Date = payment.DatePaid;
                receipt.ApplicationFormNumber = "";
                receipt.MatricNumber = student.MatricNumber;
                receipt.Session = payment.Session.Name;
                receipt.Department = studentLevel.Department.Name;
                receipt.DepartmentObj = studentLevel.Department;
                receipt.FeeDetails = payment.FeeDetails;
                receipt.PaymentId = Utility.Encrypt(payment.Id.ToString());
                receipt.barcodeImageUrl = "http://applications.federalpolyilaro.edu.ng/Common/Credential/Receipt?pmid=" + payment.Id;

                return receipt;
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        private Int32 GetLevel(int ProgrammeId)
        {
            try
            {
               //set mode of study
                switch (ProgrammeId)
                {
                    case 1:
                        {
                            return 1;
                        }
                    case 2:
                        {
                            return 1;
                        }
                    case 3:
                        {
                            return 3;
                        }
                    case 4:
                        {
                            return 3;
                        }
                    case 5:
                        {
                            return 3;
                        }
                    case 8:
                        {
                            return 3;
                        }
                    case 9:
                        {
                            return 1;
                        }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return 0;
        }
        
    }
}