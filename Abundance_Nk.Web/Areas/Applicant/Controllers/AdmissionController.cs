using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Abundance_Nk.Business;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Web.Controllers;
using Abundance_Nk.Web.Areas.Applicant.ViewModels;
using Abundance_Nk.Web.Models;
using System.Transactions;
using System.Configuration;
using Abundance_Nk.Model.Entity.Model;
using Abundance_Nk.Web.Areas.Admin.ViewModels;

namespace Abundance_Nk.Web.Areas.Applicant.Controllers
{
	[AllowAnonymous]
	public class AdmissionController : BaseController
	{
		private AdmissionViewModel viewModel;

		public AdmissionController()
		{
			viewModel = new AdmissionViewModel();
		}

		public ActionResult CheckStatus()
		{
			return View(viewModel);
		}

		[HttpPost]
		public ActionResult CheckStatus(AdmissionViewModel vModel)
		{
			try
			{
                TempData["NewFormNumber"] = vModel.ApplicationForm.Number;

                if (ModelState.IsValid)
				{
					ApplicationFormLogic applicationFormLogic = new ApplicationFormLogic();
                    bool signIn = applicationFormLogic.IsValidApplicationNumberAndRemitaPin(vModel.ApplicationForm.Number, vModel.ScratchCard.Pin);

					if (signIn)
					{   
						ApplicationForm form = viewModel.GetApplicationFormBy(vModel.ApplicationForm.Number);

						if (form != null && form.ProgrammeFee.Programme.Id == 1)
						{
							ApplicantJambDetailLogic applicantJambDetailLogic = new ApplicantJambDetailLogic();
							ChangeOfCourseLogic changeOfCourseLogic = new ChangeOfCourseLogic();
							PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
							ApplicantJambDetail applicantJambDetail = applicantJambDetailLogic.GetModelBy(a => a.Application_Form_Id == form.Id && a.Person_Id == form.Person.Id);
                            ChangeOfCourse changeOfCourse = new ChangeOfCourse();
                            if (applicantJambDetail!=null && applicantJambDetail.JambRegistrationNumber != null)
                            {
                               changeOfCourse = changeOfCourseLogic.GetModelBy(c => c.Jamb_Registration_Number == applicantJambDetail.JambRegistrationNumber && c.Session_Id == form.Payment.Session.Id);
                            }
							
							if (changeOfCourse != null && changeOfCourse.Id>0)
							{
								PaymentEtranzact paymentEtranzact = null;
								if (changeOfCourse.OldPerson != null)
								{
									paymentEtranzact = paymentEtranzactLogic.GetModelBy(p => p.ONLINE_PAYMENT.PAYMENT.Fee_Type_Id == (int)FeeTypes.ChangeOfCourseFees && p.ONLINE_PAYMENT.PAYMENT.Session_Id == form.Payment.Session.Id && p.ONLINE_PAYMENT.PAYMENT.Person_Id == changeOfCourse.OldPerson.Id); 
								} 
								
								if (paymentEtranzact == null)
								{
									return RedirectToAction("GenerateChangeOfCourseInvoice");
								}
							}
						} 

						return RedirectToAction("Index", new { fid = Abundance_Nk.Web.Models.Utility.Encrypt(form.Id.ToString()) }); 
					}
				}
			}
			catch (Exception ex)
			{
				SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
			}

			return View(vModel);
		}
		
		public ActionResult Index(string fid)
		{
			try
			{
				TempData["FormViewModel"] = null;
				Int64 formId = Convert.ToInt64(Abundance_Nk.Web.Models.Utility.Decrypt(fid));
				viewModel.GetApplicationBy(formId);
                ViewBag.OptionItems = viewModel.OptionSelectList;

                PopulateAllDropDowns(viewModel);
				viewModel.GetOLevelResultBy(formId);
				SetSelectedSittingSubjectAndGrade(viewModel);

                //Added this to get matric number on step 6
                StudentLogic studentLogic = new StudentLogic();
                var checkStudentData = studentLogic.GetModelBy(s => s.Person_Id == viewModel.Applicant.Person.Id);
                if(checkStudentData != null)
                {
                    viewModel.Student = checkStudentData;
                }

                //check acceptance debtors
                
                if (!CheckAcceptancePayment(viewModel.ApplicationForm))
                {
                    viewModel.ApplicantStatusId = (int)ApplicantStatus.Status.GeneratedAcceptanceInvoice;
                    SetMessage("Kindly pay acceptance fee before proceeding! ", Message.Category.Error);
                }
			}
			catch (Exception ex)
			{
				SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
			}

			return View(viewModel);
		}

        private bool CheckAcceptanceDebtors(AdmissionViewModel viewModel)
        {
            try
            {
                string[] acceptanceDebtors = {"FPI/ND/2018/000005715", "FPI/ND/2018/000005737", "FPI/PND/2018/0000010594", "FPI/ND/2018/000003501", "FPI/ND/2018/000004071",
                    "FPI/HND/2018/00000713", "FPI/HND/2018/000001057", "FPI/ND/2018/000004409", "FPI/ND/2018/000002163", "FPI/HND/2018/000003052", "FPI/ND/2018/000003112",
                    "FPI/ND/2018/000007196", "FPI/ND/2018/000007502", "FPI/PND/2018/00000608", "FPI/HND/2018/000001249", "FPI/ND/2018/000004233", "FPI/ND/2018/000005267",
                    "FPI/HND/2018/000001953", "FPI/PND/2018/000008066"};
                if (acceptanceDebtors.Contains(viewModel.ApplicationForm.Number))
                {
                    ApplicantLogic applicantLogic = new ApplicantLogic();
                    Model.Model.Applicant applicant = applicantLogic.GetModelsBy(a => a.Application_Form_Id == viewModel.ApplicationForm.Id).LastOrDefault();
                    if (applicant != null)
                    {
                        applicantLogic.UpdateStatus(viewModel.ApplicationForm, ApplicantStatus.Status.GeneratedAcceptanceInvoice);
                    }

                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void PopulateAllDropDowns(AdmissionViewModel existingViewModel)
		{
			//AdmissionViewModel existingViewModel = (AdmissionViewModel)TempData["viewModel"];
			//PostJAMBFormPaymentViewModel postJAMBFormPaymentViewModel = (PostJAMBFormPaymentViewModel)TempData["PostJAMBFormPaymentViewModel"];

			try
			{
				if (existingViewModel.Loaded)
				{
					if (existingViewModel.FirstSittingOLevelResult.Type == null) { existingViewModel.FirstSittingOLevelResult.Type = new OLevelType(); }

					ViewBag.FirstSittingOLevelTypeId = new SelectList(existingViewModel.OLevelTypeSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.FirstSittingOLevelResult.Type.Id);
					ViewBag.FirstSittingExamYearId = new SelectList(existingViewModel.ExamYearSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.FirstSittingOLevelResult.ExamYear);
					ViewBag.SecondSittingExamYearId = new SelectList(existingViewModel.ExamYearSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.SecondSittingOLevelResult.ExamYear);
                  
                    if(viewModel.PaymentModeSelectList != null)
                    {
                        ViewBag.PaymentModeId = viewModel.PaymentModeSelectList.Where(x => x.Value != "1");
                    }




                    if (existingViewModel.SecondSittingOLevelResult.Type != null)
					{
						ViewBag.SecondSittingOLevelTypeId = new SelectList(existingViewModel.OLevelTypeSelectList, Utility.VALUE, Utility.TEXT, existingViewModel.SecondSittingOLevelResult.Type.Id);
					}
					else
					{
						ViewBag.SecondSittingOLevelTypeId = new SelectList(existingViewModel.OLevelTypeSelectList, Utility.VALUE, Utility.TEXT, 0);
					}

					SetSelectedSittingSubjectAndGrade(existingViewModel);
				}
				else
				{
					viewModel = new AdmissionViewModel();

					ViewBag.FirstSittingOLevelTypeId = viewModel.OLevelTypeSelectList;
					ViewBag.SecondSittingOLevelTypeId = viewModel.OLevelTypeSelectList;
					ViewBag.FirstSittingExamYearId = viewModel.ExamYearSelectList;
					ViewBag.SecondSittingExamYearId = viewModel.ExamYearSelectList;
                    if(viewModel.PaymentModeSelectList != null)
                    {
                        ViewBag.PaymentModeId = viewModel.PaymentModeSelectList.Where(x => x.Value != "1");
                    }



                    //SetDefaultSelectedSittingSubjectAndGrade(viewModel);
                }
			}
			catch (Exception ex)
			{
				SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
			}
		}

		[HttpPost]
		public ActionResult GenerateAcceptanceInvoice(string applicationFormNo)
		{
			Payment payment = null;
			Decimal Amt = 0;
			try
			{
				ApplicationForm form = viewModel.GetApplicationFormBy(applicationFormNo);
				if (form != null && form.Id > 0)
				{
					AdmissionList list = new AdmissionList();
					AdmissionListLogic listLogic = new AdmissionListLogic();
                    FeeDetailLogic feeDetailLogic = new FeeDetailLogic();
					list = listLogic.GetBy(form.Id);

					AppliedCourse appliedCourse = new AppliedCourse();
					AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
					appliedCourse = appliedCourseLogic.GetBy(form.Person);

				   
					if (form != null && form.Id > 0)
					{
						
						FeeType feeType = new FeeType() { Id = (int)FeeTypes.AcceptanceFee };
                        if(form.ProgrammeFee.Programme.Id == (int)Programmes.DrivingCertificate)
                        {
                            feeType = new FeeType() { Id = (int)FeeTypes.DrivingAcceptance };

                        }
                        else if (appliedCourse.Programme.Id == (int)Programmes.HNDDistance || appliedCourse.Programme.Id == (int)Programmes.NDDistance)
						{
							feeType = new FeeType() { Id = 2 };
						}
                        else if (form.ProgrammeFee.Programme.Id > 1)
                        {
                            feeType = new FeeType() { Id = 9 };
                        }
                        ApplicantStatus.Status status = ApplicantStatus.Status.GeneratedAcceptanceInvoice;
						using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
						{

							payment = GenerateInvoiceHelper(form, feeType, status);

							if (payment != null)
							{
                                payment.FeeDetails = feeDetailLogic.GetModelsBy(f => f.Department_Id == appliedCourse.Department.Id && f.Programme_Id == appliedCourse.Programme.Id && f.Fee_Type_Id == feeType.Id && f.Session_Id == payment.Session.Id);
                                //var feeDetail=payment.FeeDetails.Where(f => f.Department.Id == appliedCourse.Department.Id && f.Programme.Id == appliedCourse.Programme.Id && f.FeeType.Id == feeType.Id);
                                
                                    Amt = payment.FeeDetails.Sum(a => a.Fee.Amount);
                              
							   
								//GENERATE RRR
							    RemitaPayment remitaPayment = ProcessRemitaPayment(payment);
							    if (remitaPayment != null && !string.IsNullOrEmpty(remitaPayment.RRR))
							    {
							        transaction.Complete();
							    }
							}

						}
					}
				}
			}
			catch (Exception ex)
			{
				SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
				return Json(new { }, "text/html", JsonRequestBehavior.AllowGet);
  
			}

			return Json(new { InvoiceNumber = payment.InvoiceNumber }, "text/html", JsonRequestBehavior.AllowGet);
		}


        public RemitaPayment ProcessRemitaPayment(Payment payment)
	    {
	        try
	        {
                RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                RemitaPayment remitaPayment = null;
                if (payment != null)
	            {
                    //Emeka added a check for existing RRR
                   
                    remitaPayment = remitaPaymentLogic.GetBy(payment.Id);
                    if (remitaPayment == null)
                    {
                       
                       //Get Payment Specific Setting
                        RemitaSettings settings = new RemitaSettings();
                        RemitaSettingsLogic settingsLogic = new RemitaSettingsLogic();
                        settings = settingsLogic.GetBy(1);

                        //Get BaseURL
                        decimal Amt = 0;
                        Amt = payment.FeeDetails.Sum(p => p.Fee.Amount);
                        string remitaBaseUrl = ConfigurationManager.AppSettings["RemitaBaseUrl"].ToString();
                        RemitaPayementProcessor remitaProcessor = new RemitaPayementProcessor(settings.Api_key);
                        remitaPayment = remitaProcessor.GenerateRRRCard(payment.InvoiceNumber, remitaBaseUrl, "ACCEPTANCE FEES", settings, Amt);
                        // string Hash = GenerateHash(settings.Api_key, remitaPayment);
                   
                    }
                    
                   
                }
                return remitaPayment;
            }
	        catch (Exception ex)
	        {
	            SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
	        }
	        return null;
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
	        catch (Exception ex)
	        {
	            throw ex;
	        }
	    }

		public ActionResult GenerateChangeOfCourseInvoice()
		{
			return View();
		}
	   
		[HttpPost]
		public ActionResult GenerateChangeOfCourseInvoice(AdmissionViewModel viewModel)
		{
			string applictionNumber = viewModel.ApplicationForm.Number;
			Payment payment = null;
			try
			{
				ApplicationForm form = viewModel.GetApplicationFormBy(applictionNumber);
				if (form != null && form.Id > 0)
				{
					if (form != null && form.Id > 0)
					{
						FeeType feeType = new FeeType() { Id = (int)FeeTypes.ChangeOfCourseFees };
						ApplicantStatus.Status status = ApplicantStatus.Status.SubmittedApplicationForm;
						payment = GenerateInvoiceHelper(form, feeType, status);
						if (payment == null)
						{
							SetMessage("An Error Occurred while generating invoice. Please try again! ", Message.Category.Error);
							return View(); 
						}      
					}
				}
				else
				{
					SetMessage("Error! Application Number not found", Message.Category.Error);
					return View(viewModel); 
				}
			}
			catch (Exception ex)
			{
				SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
				return View();
			}

			return RedirectToAction("Invoice", new { ivn = payment.InvoiceNumber });
		}

		public ActionResult PayChangeOfCourseFee()
		{
			try
			{
				viewModel = new AdmissionViewModel();
			}
			catch (Exception ex)
			{
				SetMessage("Error! " + ex.Message, Message.Category.Error);
			}

			return View(viewModel);
		}
		[HttpPost]
		public ActionResult PayChangeOfCourseFee(AdmissionViewModel viewModel)
		{
			try
			{
				if (viewModel.ConfirmationOrderNumber != null)
				{
					PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
					PaymentLogic paymentLogic = new PaymentLogic();
					HostelAllocationLogic hostelAllocationLogic = new HostelAllocationLogic();

					if (viewModel.ConfirmationOrderNumber.Length > 12)
					{
						Model.Model.Session session = new Model.Model.Session() { Id = 7 };
						FeeType feetype = new FeeType() { Id = (int)FeeTypes.ChangeOfCourseFees };
						Payment payment = paymentLogic.InvalidConfirmationOrderNumber(viewModel.ConfirmationOrderNumber, feetype.Id);
						if (payment != null && payment.Id > 0)
						{
							if (payment.FeeType.Id != (int)FeeTypes.ChangeOfCourseFees)
							{
								SetMessage("Confirmation Order Number (" + viewModel.ConfirmationOrderNumber + ") entered is not for Change of Course Fee payment! Please enter your Change of Course Fee Confirmation Order Number.", Message.Category.Error);
								return View(viewModel);
							}

							SetMessage("Your Change Of Course Fee payment has been successfully confirmed, you can now proceed with registration using the Check Admission Status link", Message.Category.Information); 
							
						}
					}
				}
			}
			catch (Exception ex)
			{
				SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
			}

			return View(viewModel);
		}
		public ActionResult MakePayment(string PaymentId, string formId)
		{
			try
			{
				Int64 fid = Convert.ToInt64(Abundance_Nk.Web.Models.Utility.Decrypt(formId));
				string invoice = Abundance_Nk.Web.Models.Utility.Decrypt(PaymentId);
				RemitaPayment remitaPyament = new RemitaPayment();
				RemitaPaymentLogic remitaLogic = new RemitaPaymentLogic();
				Payment payment = new Payment();
				PaymentLogic pL = new PaymentLogic();
				payment = pL.GetModelBy(p => p.Invoice_Number == invoice);

				decimal Amount = pL.GetPaymentAmount(payment);

				List<RemitaSplitItems> splitItems = new List<RemitaSplitItems>();
				RemitaSplitItemLogic splitItemLogic = new RemitaSplitItemLogic();
				splitItems = splitItemLogic.GetAll();
				splitItems[0].deductFeeFrom = "0";
				if (splitItems.Count > 1)
				{
					splitItems[1].deductFeeFrom = "1";
				}
				RemitaSplitItems its = new RemitaSplitItems();
				its = splitItemLogic.GetModelBy(a => a.Id == 1);
				its.deductFeeFrom = "0";

				string remitaBaseUrl = ConfigurationManager.AppSettings["RemitaBaseUrl"].ToString();

				RemitaSettings settings = new RemitaSettings();
				RemitaSettingsLogic settingsLogic = new RemitaSettingsLogic();
				settings = settingsLogic.GetModelBy(s => s.Payment_SettingId == 1);

				long milliseconds = DateTime.Now.Ticks;
				RemitaPayementProcessor remitaPayementProcessor = new RemitaPayementProcessor(settings.Api_key);
				Remita remita = new Remita()
				{

					merchantId = settings.MarchantId,
					serviceTypeId = settings.serviceTypeId,
					orderId = payment.InvoiceNumber,
					totalAmount = Amount,
					payerName = payment.Person.FullName,
					payerEmail = "support@lloydant.com",
					payerPhone = payment.Person.MobilePhone,
					responseurl = settings.Response_Url,
					lineItems = splitItems,
					

				};

				RemitaResponse remitaResponse = remitaPayementProcessor.PostJsonDataToUrl(remitaBaseUrl, remita, payment);
				if (remitaResponse.Status != null && remitaResponse.StatusCode.Equals("025"))
				{
					remitaPyament = new RemitaPayment();
					remitaPyament.payment = payment;
					remitaPyament.RRR = remitaResponse.RRR;
					remitaPyament.OrderId = remitaResponse.orderId;
					remitaPyament.Status = remitaResponse.StatusCode + ":" + remitaResponse.Status;
					remitaPyament.TransactionAmount = remita.totalAmount;
					remitaPyament.TransactionDate = DateTime.Now;
					remitaPyament.MerchantCode = remita.merchantId;
					remitaPyament.Description = "ACCEPTANCE FEES";
					if (remitaLogic.GetBy(payment.Id) == null)
					{
						remitaLogic.Create(remitaPyament);
					}
					remita.hash = remitaPayementProcessor.HashPaymentDetailToSHA512(remita.merchantId + remitaResponse.RRR + settings.Api_key);
					viewModel.ApplicationForm.Id = fid;
					viewModel.remita = remita;
					viewModel.remitaResponse = remitaResponse;
					return View(viewModel);

				}
				else if (remitaResponse.StatusCode.Trim().Equals("028"))
				{
					remitaPyament = new RemitaPayment();
					remitaPyament = remitaLogic.GetModelBy(r => r.OrderId == payment.InvoiceNumber);
					if (remitaPyament != null)
					{
						viewModel.ApplicationForm.Id = fid;
						remitaResponse.RRR = remitaPyament.RRR;
						remitaResponse.orderId = remitaPyament.OrderId;
						remita.hash = remitaPayementProcessor.HashPaymentDetailToSHA512(remita.merchantId + remitaResponse.RRR + settings.Api_key);
						viewModel.remita = remita;
						viewModel.remitaResponse = remitaResponse;
						return View(viewModel);
					}
				}

				return View(viewModel);
			}
			catch (Exception ex)
			{
				SetMessage("Error Occurred:" + ex.Message, Message.Category.Error);
				return View(viewModel);
			}

		}
		
		[HttpPost]
		public ActionResult GenerateSchoolFeesInvoice(string formNo,string cisco,string robotic,string regenerateInvoice)
		{
			Payment payment = null;
			Decimal Amt = 0;
            PaymentLogic paymentLogic = new PaymentLogic();
			try
			{
               var hasSchoolFeesInvoice= HasAlreadyGeneratedSchoolFeesInvoice(formNo);
                ApplicationForm form = viewModel.GetApplicationFormBy(formNo);
				if (form != null && form.Id > 0)
				{
                    if (hasSchoolFeesInvoice && regenerateInvoice == "true")
                    {
                        FeeType schoolFees = new FeeType { Id = (int)FeeTypes.SchoolFees };
                        paymentLogic.ClearInvoiceForRegeneration(form.Setting.Session, schoolFees, form.Person);
                        hasSchoolFeesInvoice = false;
                    }
                    //Check for Acceptance
                    bool hasPaidAcceptance = CheckAcceptancePayment(form);
                    if (!hasPaidAcceptance)
                    {
                        return Json(new { InvoiceNumber = "No Acceptance" }, "text/html", JsonRequestBehavior.AllowGet);
                    }

				   // return Json(new { Error = "School Fees payment is not yet enabled!" }, "text/html", JsonRequestBehavior.AllowGet);
					FeeType feeType = new FeeType() { Id = (int)FeeTypes.SchoolFees };
					ApplicantStatus.Status status = ApplicantStatus.Status.GeneratedSchoolFeesInvoice;
					using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
					{
						payment = GenerateInvoiceHelper(form, feeType, status);


						if (payment != null)
						{

                            RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                            viewModel.remitaPayment = remitaPaymentLogic.GetBy(payment.Id);

                            if (viewModel.remitaPayment == null)
                            {
                                //Get specific amount;

                                AdmissionListLogic admissionListLogic = new AdmissionListLogic();
                                FeeDetailLogic feeDetailLogic = new FeeDetailLogic();
                                AdmissionList admissionList = admissionListLogic.GetModelsBy(a => a.Application_Form_Id == form.Id).LastOrDefault();

                                Level level = form.ProgrammeFee.Programme.Id == (int)Programmes.NDFullTime || form.ProgrammeFee.Programme.Id == (int)Programmes.NDPartTime || form.ProgrammeFee.Programme.Id == (int)Programmes.DrivingCertificate ? new Level() { Id = (int)Levels.NDI } : new Level() { Id = (int)Levels.HNDI };

                                //Amt = feeDetailLogic.GetFeeByDepartmentLevel(admissionList.Deprtment, level, form.ProgrammeFee.Programme,
                                                                                //feeType, admissionList.Session, new PaymentMode() { Id = (int)PaymentModes.Full });
#region
                                var feeDetails = feeDetailLogic.GetFeeDetailByDepartmentLevel(admissionList.Deprtment,
                                          level, form.ProgrammeFee.Programme, feeType,
                                          admissionList.Session, new PaymentMode() { Id = (int)PaymentModes.Full });

                                if (!hasSchoolFeesInvoice)
                                {
                                    FeeinPaymentLogic feeinPaymentLogic = new FeeinPaymentLogic();
                                    if (cisco=="true")
                                        feeDetails = feeDetailLogic.AlterFeeDetailRecords((int)Fees.CISCOCERTFIEDNETWORKASSOCIATED, false, feeDetails);
                                    else if (cisco=="false")
                                        feeDetails = feeDetailLogic.AlterFeeDetailRecords((int)Fees.CISCOCERTFIEDNETWORKASSOCIATED, true, feeDetails);

                                    if (robotic=="true")

                                        feeDetails = feeDetailLogic.AlterFeeDetailRecords((int)Fees.RoboticS_UAE, false, feeDetails);

                                    else if (robotic=="false")
                                        feeDetails = feeDetailLogic.AlterFeeDetailRecords((int)Fees.RoboticS_UAE, true, feeDetails);

                                    if (form?.Id>0 && (form.ProgrammeFee.Programme.Id != (int)Programmes.NDDistance && form.ProgrammeFee.Programme.Id != (int)Programmes.HNDDistance))
                                    {
                                        if (cisco == "true")
                                        {
                                            feeinPaymentLogic.CreateRecord(payment, (int)Fees.CISCOCERTFIEDNETWORKASSOCIATED, true);
                                        }
                                        else if(cisco=="false")
                                        {
                                            feeinPaymentLogic.CreateRecord(payment, (int)Fees.CISCOCERTFIEDNETWORKASSOCIATED, false);
                                        }
                                        if (robotic == "true")
                                        {
                                            feeinPaymentLogic.CreateRecord(payment, (int)Fees.RoboticS_UAE, true);
                                        }
                                        else if(robotic=="false")
                                        {
                                            feeinPaymentLogic.CreateRecord(payment, (int)Fees.RoboticS_UAE, false);
                                        }

                                    }
                                }
                                

                                #endregion
                                Amt = feeDetails.Sum(f => f.Fee.Amount);

                                if (admissionList.Deprtment.Id == 44)
                                {
                                    if(admissionList.DepartmentOption.Id == 16)
                                    {
                                        Amt = Convert.ToDecimal(Fees.GraduateDrivingTrainingSchoolFees);
                                    }
                                    if (admissionList.DepartmentOption.Id == 17)
                                    {
                                        Amt = Convert.ToDecimal(Fees.TechnicalCertificateDrivingSchooFees);
                                    }
                                    else if(admissionList.DepartmentOption.Id == 18)
                                    {
                                        Amt = Convert.ToDecimal(Fees.ProfessionalDiplomaDrivingSchooFees);
                                    }
                                }
                                //Get Payment Specific Setting
                                RemitaSettings settings = new RemitaSettings();
                                RemitaSettingsLogic settingsLogic = new RemitaSettingsLogic();
                                settings = settingsLogic.GetBy(2);

                                //Get Split Specific details;
                                List<RemitaSplitItems> splitItems = new List<RemitaSplitItems>();
                                RemitaSplitItems singleItem = new RemitaSplitItems();
                                RemitaSplitItemLogic splitItemLogic = new RemitaSplitItemLogic();
                                singleItem = splitItemLogic.GetBy(7);
                                singleItem.deductFeeFrom = "1";
                                splitItems.Add(singleItem);
                                singleItem = splitItemLogic.GetBy(6);
                                singleItem.deductFeeFrom = "0";
                                singleItem.beneficiaryAmount = Convert.ToString(Amt - Convert.ToDecimal(splitItems[0].beneficiaryAmount));
                                splitItems.Add(singleItem);


                                //Get BaseURL
                                string remitaBaseUrl = ConfigurationManager.AppSettings["RemitaBaseUrl"].ToString();
                                RemitaPayementProcessor remitaProcessor = new RemitaPayementProcessor(settings.Api_key);
                                viewModel.remitaPayment = remitaProcessor.GenerateRRR(payment.InvoiceNumber, remitaBaseUrl, "SCHOOL FEES", splitItems, settings, Amt);
                                if (viewModel.remitaPayment != null)
                                {
                                    transaction.Complete();
                                }
                            }
                            else
                            {
                                transaction.Complete();
                            }
						}
					}
				}
			}
			catch (Exception ex)
			{
				SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
			}

			return Json(new { InvoiceNumber = payment.InvoiceNumber }, "text/html", JsonRequestBehavior.AllowGet);
		}


        //Drivers License Invoice
        [HttpPost]
        public ActionResult GenerateDriversLicenceFeesInvoice(string formNo)
        {
            Payment payment = null;
            Decimal Amt = 0;
            try
            {
                ApplicationForm form = viewModel.GetApplicationFormBy(formNo);
                if (form != null && form.Id > 0)
                {
                    //Check for Acceptance
                    bool hasPaidAcceptance = CheckAcceptancePayment(form);
                    if (!hasPaidAcceptance)
                    {
                        return Json(new { InvoiceNumber = "No Acceptance" }, "text/html", JsonRequestBehavior.AllowGet);
                    }

                    // return Json(new { Error = "School Fees payment is not yet enabled!" }, "text/html", JsonRequestBehavior.AllowGet);
                    FeeType feeType = new FeeType() { Id = (int)FeeTypes.DriversLicense };
                    ApplicantStatus.Status status = ApplicantStatus.Status.GeneratedSchoolFeesInvoice;
                    using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
                    {
                        payment = GenerateInvoiceHelper(form, feeType, status);


                        if (payment != null)
                        {

                            RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                            viewModel.remitaPayment = remitaPaymentLogic.GetBy(payment.Id);

                            if (viewModel.remitaPayment == null)
                            {
                                //Get specific amount;

                                AdmissionListLogic admissionListLogic = new AdmissionListLogic();
                                FeeDetailLogic feeDetailLogic = new FeeDetailLogic();
                                AdmissionList admissionList = admissionListLogic.GetModelsBy(a => a.Application_Form_Id == form.Id).LastOrDefault();

                                Level level = form.ProgrammeFee.Programme.Id == (int)Programmes.NDFullTime || form.ProgrammeFee.Programme.Id == (int)Programmes.NDPartTime || form.ProgrammeFee.Programme.Id == (int)Programmes.DrivingCertificate ? new Level() { Id = (int)Levels.NDI } : new Level() { Id = (int)Levels.HNDI };

                                Amt = feeDetailLogic.GetFeeByDepartmentLevel(admissionList.Deprtment, level, form.ProgrammeFee.Programme,
                                                                                feeType, admissionList.Session, new PaymentMode() { Id = (int)PaymentModes.Full });
                                //if (admissionList.Deprtment.Id == 44)
                                //{
                                //    if (admissionList.DepartmentOption.Id == 16)
                                //    {
                                //        Amt = Convert.ToDecimal(Fees.GraduateDrivingTrainingSchoolFees);
                                //    }
                                //    if (admissionList.DepartmentOption.Id == 17)
                                //    {
                                //        Amt = Convert.ToDecimal(Fees.TechnicalCertificateDrivingSchooFees);
                                //    }
                                //    else if (admissionList.DepartmentOption.Id == 18)
                                //    {
                                //        Amt = Convert.ToDecimal(Fees.ProfessionalDiplomaDrivingSchooFees);
                                //    }
                                //}
                                //Get Payment Specific Setting
                                RemitaSettings settings = new RemitaSettings();
                                RemitaSettingsLogic settingsLogic = new RemitaSettingsLogic();
                                settings = settingsLogic.GetBy(2);

                                //Get Split Specific details;
                                List<RemitaSplitItems> splitItems = new List<RemitaSplitItems>();
                                RemitaSplitItems singleItem = new RemitaSplitItems();
                                RemitaSplitItemLogic splitItemLogic = new RemitaSplitItemLogic();
                                singleItem = splitItemLogic.GetBy(7);
                                singleItem.deductFeeFrom = "1";
                                splitItems.Add(singleItem);
                                singleItem = splitItemLogic.GetBy(6);
                                singleItem.deductFeeFrom = "0";
                                singleItem.beneficiaryAmount = Convert.ToString(Amt - Convert.ToDecimal(splitItems[0].beneficiaryAmount));
                                splitItems.Add(singleItem);


                                //Get BaseURL
                                string remitaBaseUrl = ConfigurationManager.AppSettings["RemitaBaseUrl"].ToString();
                                RemitaPayementProcessor remitaProcessor = new RemitaPayementProcessor(settings.Api_key);
                                viewModel.remitaPayment = remitaProcessor.GenerateRRR(payment.InvoiceNumber, remitaBaseUrl, "DRIVERS LICENSE FEES", splitItems, settings, Amt);
                                if (viewModel.remitaPayment != null)
                                {
                                    transaction.Complete();
                                }
                            }
                            else
                            {
                                transaction.Complete();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            return Json(new { InvoiceNumber = payment.InvoiceNumber }, "text/html", JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult GenerateSchoolFeesInvoiceDistantLearning(string formNo, int paymentMode)
        {
            Payment payment = null;
            Decimal Amt = 0;
            try
            {
                ApplicationForm form = viewModel.GetApplicationFormBy(formNo);
                if (form != null && form.Id > 0)
                {
                    //Check for Acceptance
                    bool hasPaidAcceptance = CheckAcceptancePayment(form);
                    if (!hasPaidAcceptance)
                    {
                        return Json(new { InvoiceNumber = "No Acceptance" }, "text/html", JsonRequestBehavior.AllowGet);
                    }

                    // return Json(new { Error = "School Fees payment is not yet enabled!" }, "text/html", JsonRequestBehavior.AllowGet);
                    FeeType feeType = new FeeType() { Id = (int)FeeTypes.SchoolFees };
                    ApplicantStatus.Status status = ApplicantStatus.Status.GeneratedSchoolFeesInvoice;
                    using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
                    {
                        AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();

                        AppliedCourse appliedCourse = appliedCourseLogic.GetModelsBy(x => x.Person_Id == form.Person.Id).LastOrDefault();

                        if(appliedCourse.Programme.Id == (int)Programmes.HNDDistance || appliedCourse.Programme.Id == (int)Programmes.NDDistance)
                        {
                            PaymentLogic paymentLogic = new PaymentLogic();
                            form.Setting.PaymentMode = new PaymentMode { Id = paymentMode };
                            bool isPaymentResolved = paymentLogic.ResolveInstallmentPaymentOrder(form.Payment.FeeType, form.Person, form.Payment.Session, new PaymentMode { Id = paymentMode });
                            if (!isPaymentResolved)
                            {
                                //First Installment has not yet been generated.paid for
                                return Json(new { InvoiceNumber = "FirstInstallmentError." }, "text/html", JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                payment = GenerateInvoiceHelper(form, feeType, status);
                            }

                        }
                        else
                        {
                            payment = GenerateInvoiceHelper(form, feeType, status);

                        }


                        if (payment != null)
                        {

                            RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                            viewModel.remitaPayment = remitaPaymentLogic.GetBy(payment.Id);

                            if (viewModel.remitaPayment == null)
                            {
                                //Get specific amount;

                                AdmissionListLogic admissionListLogic = new AdmissionListLogic();
                                FeeDetailLogic feeDetailLogic = new FeeDetailLogic();
                                AdmissionList admissionList = admissionListLogic.GetModelsBy(a => a.Application_Form_Id == form.Id).LastOrDefault();

                                Level level = form.ProgrammeFee.Programme.Id == (int)Programmes.NDFullTime || form.ProgrammeFee.Programme.Id == (int)Programmes.NDPartTime || form.ProgrammeFee.Programme.Id == (int)Programmes.NDDistance ? new Level() { Id = (int)Levels.NDI } : new Level() { Id = (int)Levels.HNDI };

                                Amt = feeDetailLogic.GetFeeByDepartmentLevel(admissionList.Deprtment, level, form.ProgrammeFee.Programme,
                                                                                feeType, admissionList.Session, new PaymentMode() { Id = paymentMode });
                                if (admissionList.Deprtment.Id == 44)
                                {
                                    if (admissionList.DepartmentOption.Id == 16)
                                    {
                                        Amt = Convert.ToDecimal(Fees.GraduateDrivingTrainingSchoolFees);
                                    }
                                    if (admissionList.DepartmentOption.Id == 17)
                                    {
                                        Amt = Convert.ToDecimal(Fees.TechnicalCertificateDrivingSchooFees);
                                    }
                                    else if (admissionList.DepartmentOption.Id == 18)
                                    {
                                        Amt = Convert.ToDecimal(Fees.ProfessionalDiplomaDrivingSchooFees);
                                    }
                                }
                                //Get Payment Specific Setting
                                RemitaSettings settings = new RemitaSettings();
                                RemitaSettingsLogic settingsLogic = new RemitaSettingsLogic();
                                settings = settingsLogic.GetBy(2);

                                //Get Split Specific details;
                                List<RemitaSplitItems> splitItems = new List<RemitaSplitItems>();
                                RemitaSplitItems singleItem = new RemitaSplitItems();
                                RemitaSplitItemLogic splitItemLogic = new RemitaSplitItemLogic();
                                singleItem = splitItemLogic.GetBy(7);
                                singleItem.deductFeeFrom = "1";
                                splitItems.Add(singleItem);
                                singleItem = splitItemLogic.GetBy(6);
                                singleItem.deductFeeFrom = "0";
                                singleItem.beneficiaryAmount = Convert.ToString(Amt - Convert.ToDecimal(splitItems[0].beneficiaryAmount));
                                splitItems.Add(singleItem);


                                //Get BaseURL
                                string remitaBaseUrl = ConfigurationManager.AppSettings["RemitaBaseUrl"].ToString();
                                RemitaPayementProcessor remitaProcessor = new RemitaPayementProcessor(settings.Api_key);
                                viewModel.remitaPayment = remitaProcessor.GenerateRRR(payment.InvoiceNumber, remitaBaseUrl, "SCHOOL FEES", splitItems, settings, Amt);
                                if (viewModel.remitaPayment != null)
                                {
                                    transaction.Complete();
                                }
                            }
                            else
                            {
                                transaction.Complete();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            return Json(new { InvoiceNumber = payment.InvoiceNumber }, "text/html", JsonRequestBehavior.AllowGet);
        }

        private bool CheckAcceptancePayment(ApplicationForm form)
        {
            try
            {
                
                int[] acceptanceFeeTypes = { (int)FeeTypes.AcceptanceFee, (int)FeeTypes.HNDAcceptance, (int)FeeTypes.DrivingAcceptance };
                if(form.ProgrammeFee.Programme.Id == 7)
                {
                    AdmissionListLogic listLogic = new AdmissionListLogic();
                    var getOption = listLogic.GetModelBy(x => x.Application_Form_Id == form.Id);
                    if(getOption != null && getOption.DepartmentOption.Id == 16)
                    {
                        return true;
                    }

                }
                RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();

                RemitaPayment remitaPayment = remitaPaymentLogic.GetModelsBy(r => r.PAYMENT.Person_Id == form.Person.Id && acceptanceFeeTypes.Contains(r.PAYMENT.Fee_Type_Id) && (r.Status.Contains("01") || r.Description.Contains("manual"))).LastOrDefault();
                if (remitaPayment != null)
                    return true;

                PaymentEtranzactLogic etranzactLogic = new PaymentEtranzactLogic();
                PaymentEtranzact paymentEtranzact = etranzactLogic.GetModelsBy(e => e.ONLINE_PAYMENT.PAYMENT.Person_Id == form.Person.Id && acceptanceFeeTypes.Contains(e.ONLINE_PAYMENT.PAYMENT.Fee_Type_Id)).LastOrDefault();
                if (paymentEtranzact != null)
                    return true;

                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private Payment GenerateInvoiceHelper(ApplicationForm form, FeeType feeType, ApplicantStatus.Status status)
		{
			try
			{
				Payment payment = viewModel.GenerateInvoice(form, status, feeType);
				if (payment == null)
				{
					SetMessage("Operation Failed! Invoice could not be generated. Refresh browser & try again", Message.Category.Error);
				}

				viewModel.Invoice.Payment = payment;
				viewModel.Invoice.Person = form.Person;
				viewModel.Invoice.JambRegistrationNumber = "";

                if (payment.FeeType.Id == (int)FeeTypes.AcceptanceFee || payment.FeeType.Id == (int)FeeTypes.HNDAcceptance || payment.FeeType.Id == (int)FeeTypes.DrivingAcceptance)
				{
					viewModel.AcceptanceInvoiceNumber = payment.InvoiceNumber;
				}
				else if (payment.FeeType.Id == (int)FeeTypes.SchoolFees)
				{
					viewModel.SchoolFeesInvoiceNumber = payment.InvoiceNumber;
				}
			    payment.Person = form.Person;
				return payment;
			}
			catch (Exception)
			{
				throw;
			}
		}
		
		public ActionResult GenerateAcceptanceReceipt(long fid, string ivn, string con, int st)
		{
			try
			{
				string successMeassge = "Acceptance Receipt has been successfully generated and ready for printing. Print the Acceptance Receipt or Admission Letter by clicking on the Print Receipt or Print Admission Letter button.";
				

				if (!GenerateReceiptHelper(fid, ivn, con, st, successMeassge))
				{
					return PartialView("_Message",new Message("RRR has not been paid for! Please crosscheck and try again"));
				}
			}
			catch (Exception ex)
			{
				SetMessage(ex.Message, Message.Category.Error);
			}

			return PartialView("_Message", TempData["Message"]);
		}

		private bool GenerateReceiptHelper(long fid, string ivn, string con, int st, string successMeassge)
		{
			try
			{
			   
			//    RemitaPayment remitaPayment = new RemitaPayment();
			//    RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
			//    remitaPayment = remitaPaymentLogic.GetModelsBy(a => a.RRR == con).FirstOrDefault();

			//    //Fix for students that paid before setup
			//    if (remitaPayment != null)
			//    {
			//        if (remitaPayment.Description.Contains("MANUAL PAYMENT ACCEPTANCE"))
			//        {
			//            Receipt receipt = GetReceipt(ivn, fid, st);
			//            if (receipt != null)
			//            {
			//                SetMessage(successMeassge, Message.Category.Information);
			//                return true;
			//            }
			//        }


			//        remitaPayment = remitaPaymentLogic.GetModelsBy(a => a.RRR == con).FirstOrDefault();
			//        if (remitaPayment != null && remitaPayment.Status.Contains("01:"))
			//        {
			//            if (remitaPayment.payment.InvoiceNumber == ivn)
			//            {
			//                Receipt receipt = GetReceipt(ivn, fid, st);
			//                if (receipt != null)
			//                {
			//                    SetMessage(successMeassge, Message.Category.Information);
			//                    return true;
			//                }
			//            }
			//            else
			//            {
			//                SetMessage("Your Receipt generation failed because the Confirmation Order Number (" + con + ") entered belongs to another invoice number! Please enter your Confirmation Order Number correctly.", Message.Category.Error);
			//            }
			//        }

			//        //Get status of transaction
			//        RemitaSettings settings = new RemitaSettings();
			//        RemitaSettingsLogic settingsLogic = new RemitaSettingsLogic();
			//        int settingsId = 0;
			//        if ((remitaPayment.payment.FeeType.Id == 2) || (remitaPayment.payment.FeeType.Id == 9))
			//        {
			//            settingsId = 1;
			//            settings = settingsLogic.GetModelBy(s => s.Payment_SettingId == settingsId);
			//        }
			//        else if (remitaPayment.payment.FeeType.Id == 3)
			//        {
			//            settingsId = 2;
			//            settings = settingsLogic.GetModelBy(s => s.Payment_SettingId == settingsId);
			//        }

			//        string remitaVerifyUrl = ConfigurationManager.AppSettings["RemitaVerifyUrl"].ToString();
			//        RemitaPayementProcessor remitaPayementProcessor = new RemitaPayementProcessor(settings.Api_key);
			//        remitaPayment = remitaPayementProcessor.GetStatus(remitaPayment.OrderId);

			//        if (remitaPayment != null && remitaPayment.Status.Contains("01:"))
			//        {
			//            if (remitaPayment.payment.InvoiceNumber == ivn)
			//            {
			//                Receipt receipt = GetReceipt(ivn, fid, st);
			//                if (receipt != null)
			//                {
			//                    SetMessage(successMeassge, Message.Category.Information);
			//                    return true;
			//                }
			//            }
			//            else
			//            {
			//                SetMessage("Your Receipt generation failed because the Confirmation Order Number (" + con + ") entered belongs to another invoice number! Please enter your Confirmation Order Number correctly.", Message.Category.Error);
			//            }
			//        }

			//    }

				

					Model.Model.Session session = new Model.Model.Session() { Id = 7 };
					FeeType feetype = null;
					if (st == 3 || st == 4)
					{
						Payment paymentx = new Payment();
						PaymentLogic paymentLogics = new PaymentLogic();
						paymentx = paymentLogics.GetBy(ivn);
						if (paymentx.FeeType.Id == (int) FeeTypes.AcceptanceFee)
						{
							feetype = new FeeType() { Id = (int)FeeTypes.AcceptanceFee };
						}
						else if (paymentx.FeeType.Id == (int) FeeTypes.HNDAcceptance)
						{
							feetype = new FeeType() { Id = (int)FeeTypes.HNDAcceptance };
						}
                        else if (paymentx.FeeType.Id == (int)FeeTypes.DrivingAcceptance)
                        {
                            feetype = new FeeType() { Id = (int)FeeTypes.DrivingAcceptance };
                        }

                    }
					else
					{
					   feetype = new FeeType() { Id = (int)FeeTypes.SchoolFees };
					}

					PaymentLogic paymentLogic = new PaymentLogic();
                    var appFormNumner = TempData["NewFormNumber"].ToString();
                    TempData.Keep("NewFormNumber");

                Payment payment = paymentLogic.InvalidConfirmationOrderNumber(con, session, feetype, appFormNumner);
					if (payment != null && payment.Id > 0)
					{
					   
							if (payment.InvoiceNumber == ivn)
							{
								Receipt receipt = GetReceipt(ivn, fid, st);
								if (receipt != null)
								{
									SetMessage(successMeassge, Message.Category.Information);
									return true;
								}
							}
							else
							{
								SetMessage("Your Receipt generation failed because the Confirmation Order Number (" + con + ") entered belongs to another invoice number! Please enter your Confirmation Order Number correctly.", Message.Category.Error);
							}
						
					}

				

				SetMessage("Your Receipt generation failed because the Confirmation Number (" + con + ") entered could not be verified. Please confirm and try again", Message.Category.Error);
				return false;
			}
			catch (Exception)
			{
				throw;
			}
		}
        public ActionResult GenerateDriversLicenceFeesReceipt(long fid, string ivn, string con, int st)
        {
            try
            {
                string successMeassge = "Drivers Licencse Fee Receipt has been successfully generated and ready for printing. Click on the Print Receipt button to print receipt.";

                //using (TransactionScope transaction = new TransactionScope())
                //{
                bool isSuccessfull = GenerateReceiptHelper(fid, ivn, con, st, successMeassge);
                if (isSuccessfull)
                {
                    //assign matric number
                    ApplicantLogic applicantLogic = new ApplicantLogic();
                    ApplicationFormView applicant = applicantLogic.GetBy(fid);
                    if (applicant != null)
                    {

                        StudentLogic studentLogic = new StudentLogic();
                        bool matricNoAssigned = studentLogic.AssignMatricNumber(applicant);
                        if (matricNoAssigned)
                        {
                            //transaction.Complete();
                        }
                    }
                }

                //}
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            return PartialView("_Message", TempData["Message"]);
        }

        public ActionResult GenerateSchoolFeesReceipt(long fid, string ivn, string con, int st)
		{
			try
			{
				string successMeassge = "School Fees Receipt has been successfully generated and ready for printing. Click on the Print Receipt button to print receipt.";

                //using (TransactionScope transaction = new TransactionScope())
                //{
                TempData["FormId"] = fid;
					bool isSuccessfull = GenerateReceiptHelper(fid, ivn, con, st, successMeassge);
					if (isSuccessfull)
					{
						//assign matric number
						ApplicantLogic applicantLogic = new ApplicantLogic();
						ApplicationFormView applicant = applicantLogic.GetBy(fid);
						if (applicant != null)
						{
						   
							StudentLogic studentLogic = new StudentLogic();
							bool matricNoAssigned = studentLogic.AssignMatricNumber(applicant);
							if (matricNoAssigned)
							{
								//transaction.Complete();
							}
						}
					}
                    
                //}
			}
			catch (Exception ex)
			{
				SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
			}

			return PartialView("_Message", TempData["Message"]);
		}
		
		public ActionResult Invoice(string ivn)
		{
			try
			{
                List<int> displayFee = new List<int>() { 169, 170, 172, 173, 174, 175, 171 };
                if (string.IsNullOrEmpty(ivn))
				{
					SetMessage("Invoice Not Found! Refresh and Try again ", Message.Category.Error);
				}

				viewModel.GetInvoiceBy(ivn);

                PaymentLogic paymentLogic = new PaymentLogic();
                Payment payment = paymentLogic.GetModelsBy(p => p.Invoice_Number == ivn).LastOrDefault();
                if (payment != null && payment.FeeType.Id == (int)FeeTypes.SchoolFees)
                {
                    RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                    RemitaPayment remitaPayment = remitaPaymentLogic.GetModelBy(r => r.Payment_Id == payment.Id);
                    if (remitaPayment != null)
                    {
                        //Get Payment Specific Setting
                        RemitaSettings settings = new RemitaSettings();
                        RemitaSettingsLogic settingsLogic = new RemitaSettingsLogic();
                        settings = settingsLogic.GetBy(2);

                        string Hash = GenerateHash(settings.Api_key, remitaPayment);

                        Student.ViewModels.PaymentViewModel paymentViewModel = new Student.ViewModels.PaymentViewModel();
                        paymentViewModel.RemitaPayment = remitaPayment;
                        paymentViewModel.Hash = Hash;

                        TempData["PaymentViewModel"] = paymentViewModel;
                    }
                }
			}
			catch (Exception ex)
			{
				SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
			}

			return View(viewModel.Invoice);
		}

		public ActionResult Receipt(string ivn, long fid, int st)
		{
			Receipt receipt = null;

			try
			{
				receipt = GetReceipt(ivn, fid, st);
			}
			catch (Exception ex)
			{
				SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
			}

			return View(receipt);
		}

		private Receipt GetReceipt(string ivn, long fid, int st)
		{
			Receipt receipt = null;

			try
			{
				ApplicantStatus.Status status = (ApplicantStatus.Status)st;
				if (IsNextApplicationStatus(fid, st))
				{
					receipt = viewModel.GenerateReceipt(ivn, fid, status);

                    PaymentVerificationLogic verificationLogic = new PaymentVerificationLogic();
                    PaymentLogic paymentLogic = new PaymentLogic();
                    var paymentId = paymentLogic.GetModelBy(x => x.Invoice_Number == ivn);
                    if (paymentId != null)
                    {
                        //receipt.PaymentVerification = verificationLogic.GetBy(paymentId.Id);

                    }
                }
				else
				{
					receipt = viewModel.GetReceiptBy(ivn);
                    receipt.IsVerified = true;
                    PaymentVerificationLogic verificationLogic = new PaymentVerificationLogic();
                    PaymentLogic paymentLogic = new PaymentLogic();
                    var paymentId = paymentLogic.GetModelBy(x => x.Invoice_Number == ivn);
                    if (paymentId != null)
                    {
                        //receipt.PaymentVerification = verificationLogic.GetBy(paymentId.Id);

                    }
                }

				if (receipt == null)
				{
					SetMessage("No receipt found for Invoice No " + ivn, Message.Category.Error);
				}
				return receipt;
			}
			catch (Exception)
			{
				throw;
			}
		}

		public ActionResult ProcessChangeOfCourse()
		{
			try
			{
			   viewModel = new AdmissionViewModel();
			}
			catch (Exception ex)
			{   
				SetMessage("Error! " + ex.Message, Message.Category.Error);
			}
			
			return View(viewModel);
		}
		[HttpPost]
		public ActionResult ProcessChangeOfCourse(AdmissionViewModel viewModel)
		{
			try
			{
				if (viewModel.ConfirmationOrderNumber != null && viewModel.ApplicationForm.Number != null)
				{
					PaymentLogic paymentLogic = new PaymentLogic();
					ApplicationFormLogic applicationFormLogic = new ApplicationFormLogic();
					AdmissionListLogic admissionListLogic = new AdmissionListLogic();
					AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
					ApplicantJambDetailLogic applicantJambDetailLogic = new ApplicantJambDetailLogic();
					ApplicantLogic applicantLogic = new ApplicantLogic();
					ApplicantClearanceLogic applicantClearanceLogic = new ApplicantClearanceLogic();
					OLevelResultLogic oLevelResultLogic = new OLevelResultLogic();
					PreviousEducationLogic previousEducationLogic = new PreviousEducationLogic();
					SponsorLogic sponsorLogic = new SponsorLogic();
					ChangeOfCourseLogic changeOfCourseLogic = new ChangeOfCourseLogic();
					PersonLogic personLogic = new PersonLogic();

					ApplicationForm createdApplicationForm = new ApplicationForm();
					Person createdPerson = new Person();

					if (viewModel.ConfirmationOrderNumber.Length > 12)
					{
						FeeType feetype = new FeeType() { Id = (int)FeeTypes.ChangeOfCourseFees };
						Payment payment = paymentLogic.InvalidConfirmationOrderNumber(viewModel.ConfirmationOrderNumber, feetype.Id);
						if (payment != null && payment.Id > 0)
						{
							if (payment.FeeType.Id != (int)FeeTypes.ChangeOfCourseFees)
							{
								SetMessage("Confirmation Order Number (" + viewModel.ConfirmationOrderNumber + ") entered is not for Change of Course Fee payment! Please enter your Change of Course Fee Confirmation Order Number.", Message.Category.Error);
								return View(viewModel);
							}

							ApplicationForm applicationFormOld = applicationFormLogic.GetModelsBy(a => a.Application_Form_Number.Trim() == viewModel.ApplicationForm.Number.Trim()).LastOrDefault();
							long oldApplicationId = applicationFormOld.Id;

							ChangeOfCourse checkIfChangeOfCourseExist = changeOfCourseLogic.GetModelsBy(a => a.Old_Person_Id == applicationFormOld.Person.Id && a.Session_Id == applicationFormOld.Payment.Session.Id).LastOrDefault();
							if (checkIfChangeOfCourseExist != null)
							{
								if (checkIfChangeOfCourseExist.ApplicationForm != null)
								{
									AdmissionList admissionList = admissionListLogic.GetModelBy(a => a.Application_Form_Id == checkIfChangeOfCourseExist.ApplicationForm.Id && a.Session_Id == checkIfChangeOfCourseExist.Session.Id);
									if (admissionList != null)
									{
										viewModel.ApplicationForm = checkIfChangeOfCourseExist.ApplicationForm;
										
										SetMessage("Your Change Of Course Fee payment has been successfully confirmed, your new application details can be seen below. You can now proceed with registration using the Check Admission Status link", Message.Category.Information);

										return View(viewModel);
									}  
								}   
							}

							using (TransactionScope scope = new TransactionScope())
							{   
								if (applicationFormOld != null)
								{
									Person oldPerson = personLogic.GetModelBy(p => p.Person_Id == applicationFormOld.Person.Id);
									Person newPerson = new Person();
									
									newPerson.ContactAddress = oldPerson.ContactAddress;
									newPerson.DateEntered = DateTime.Now;
									newPerson.DateOfBirth = oldPerson.DateOfBirth;
									newPerson.Email = oldPerson.Email;
									newPerson.FirstName = oldPerson.FirstName;
									newPerson.HomeAddress = oldPerson.HomeAddress;
									newPerson.HomeTown = oldPerson.HomeTown;
									newPerson.ImageFileUrl = oldPerson.ImageFileUrl;
									newPerson.Initial = oldPerson.Initial;
									newPerson.LastName = oldPerson.LastName;
									newPerson.LocalGovernment = oldPerson.LocalGovernment;
									newPerson.MobilePhone = oldPerson.MobilePhone;
									newPerson.Nationality = oldPerson.Nationality;
									newPerson.OtherName = oldPerson.OtherName;
									newPerson.Religion = oldPerson.Religion;
									newPerson.Role = oldPerson.Role;
									newPerson.Sex = oldPerson.Sex;
									newPerson.State = oldPerson.State;
									newPerson.SignatureFileUrl = oldPerson.SignatureFileUrl;
									newPerson.Title = oldPerson.Title;
									newPerson.Type = oldPerson.Type;

									createdPerson = personLogic.Create(newPerson);

									AdmissionList admissionList = admissionListLogic.GetModelsBy(a => a.Application_Form_Id == oldApplicationId).LastOrDefault();

									if (admissionList == null)
									{
										SetMessage("Error! Applicant has not been given admission", Message.Category.Error);
										return View(viewModel);
									}

									AppliedCourse appliedCourseOld = appliedCourseLogic.GetModelsBy(a => a.Application_Form_Id == oldApplicationId).LastOrDefault();

									AppliedCourse appliedCourseNew = new AppliedCourse();

									appliedCourseNew.Department = admissionList.Deprtment;
									appliedCourseNew.ApplicationForm = null;
									appliedCourseNew.Option = admissionList.DepartmentOption;
									appliedCourseNew.Person = createdPerson;
									appliedCourseNew.Programme = appliedCourseOld.Programme;

									appliedCourseLogic.Create(appliedCourseNew);

									//Create New Application
									ApplicationForm applicationFormNew = new ApplicationForm();

									applicationFormNew.DateSubmitted = DateTime.Now;
									applicationFormNew.Payment = payment;
									applicationFormNew.Person = createdPerson;
									applicationFormNew.ProgrammeFee = applicationFormOld.ProgrammeFee;
									applicationFormNew.RejectReason = applicationFormOld.RejectReason;
									applicationFormNew.Rejected = applicationFormOld.Rejected;
									applicationFormNew.Release = applicationFormOld.Release;
									applicationFormNew.Remarks = applicationFormOld.Remarks;
									applicationFormNew.Setting = applicationFormOld.Setting;
									applicationFormNew.IsAwaitingResult = applicationFormOld.IsAwaitingResult;

									createdApplicationForm = applicationFormLogic.Create(applicationFormNew);

									if (createdApplicationForm.Number == null)
									{
										SetMessage("Error! Try Again", Message.Category.Error);
										return View(viewModel);
									}                                                

									admissionList.Form = createdApplicationForm;
									admissionListLogic.ModifyListOnly(admissionList);

									//Create New Jamb Details
									ApplicantJambDetail applicantJambDetail = applicantJambDetailLogic.GetModelsBy(a => a.Application_Form_Id == oldApplicationId).LastOrDefault();
									if (applicantJambDetail != null)
									{
										ApplicantJambDetail newApplicantJambDetail = new ApplicantJambDetail();
										newApplicantJambDetail.ApplicationForm = createdApplicationForm;
										newApplicantJambDetail.Person = createdPerson;
										newApplicantJambDetail.JambRegistrationNumber = applicantJambDetail.JambRegistrationNumber;
										newApplicantJambDetail.InstitutionChoice = applicantJambDetail.InstitutionChoice;
										newApplicantJambDetail.JambScore = applicantJambDetail.JambScore;
										newApplicantJambDetail.Subject1 = applicantJambDetail.Subject1;
										newApplicantJambDetail.Subject2 = applicantJambDetail.Subject2;
										newApplicantJambDetail.Subject3 = applicantJambDetail.Subject3;
										newApplicantJambDetail.Subject4 = applicantJambDetail.Subject4;
										
										applicantJambDetailLogic.Create(newApplicantJambDetail);
									}

									//create New Applicant
									Model.Model.Applicant applicant = applicantLogic.GetModelsBy(a => a.Application_Form_Id == oldApplicationId).LastOrDefault();
									if (applicant != null)
									{
										Model.Model.Applicant newApplicant = new Model.Model.Applicant();
										newApplicant.ApplicationForm = createdApplicationForm;
										newApplicant.Person = createdPerson;
										newApplicant.Ability = applicant.Ability;
										newApplicant.ExtraCurricullarActivities = applicant.ExtraCurricullarActivities;
										newApplicant.OtherAbility = applicant.OtherAbility;
										newApplicant.Status = applicant.Status;
										
										applicantLogic.Create(newApplicant);
									}

									//Modify Applicant Clearance if exist
									//ApplicantClearance applicantClearance = applicantClearanceLogic.GetModelsBy(a => a.Application_Form_Id == oldApplicationId).LastOrDefault();
									//if (applicantClearance != null)
									//{
									//    applicantClearance.ApplicationForm = createdApplicationForm;
									//    applicantClearanceLogic.Modify(applicantClearance);
									//}

									//Modify OLevelResult
									List<OLevelResult> oLevelResults = oLevelResultLogic.GetModelsBy(o => o.Application_Form_Id == oldApplicationId);
									if (oLevelResults.Count > 0)
									{
										for (int i = 0; i < oLevelResults.Count; i++)
										{
											oLevelResults[i].ApplicationForm = createdApplicationForm;
											oLevelResults[i].Person = createdPerson;
											oLevelResultLogic.Modify(oLevelResults[i]);
										}
									}

									//Check and Modify Previous Education
									PreviousEducation previousEducation = previousEducationLogic.GetModelsBy(p => p.Application_Form_Id == oldApplicationId).LastOrDefault();
									if (previousEducation != null)
									{
										previousEducation.ApplicationForm = createdApplicationForm;
										previousEducation.Person = createdPerson;
										previousEducationLogic.Modify(previousEducation);
									}

									
									//Modify Change Of Course Detail
									if (applicantJambDetail != null)
									{
										ChangeOfCourse changeOfCourse = changeOfCourseLogic.GetModelsBy(c => c.Jamb_Registration_Number == applicantJambDetail.JambRegistrationNumber && c.Session_Id == payment.Session.Id).LastOrDefault();
										changeOfCourse.ApplicationForm = createdApplicationForm;
										changeOfCourse.NewPerson = createdPerson;
										changeOfCourse.OldPerson = applicationFormOld.Person;

										changeOfCourseLogic.Modify(changeOfCourse);
									}
									else
									{
										SetMessage("No Jamb Detail record found for Applicant ", Message.Category.Error);
									}
								}

								scope.Complete();
							}

							AppliedCourse createdAppliedCourse = appliedCourseLogic.GetBy(createdPerson);
							createdAppliedCourse.Person = createdPerson;
							createdAppliedCourse.ApplicationForm = createdApplicationForm;
							appliedCourseLogic.Modify(createdAppliedCourse);

							//Check and Create Applicant Sponsor
							Sponsor sponsor = sponsorLogic.GetModelsBy(s => s.Application_Form_Id == oldApplicationId).LastOrDefault();
							if (sponsor != null)
							{
								Sponsor newSponsor = new Sponsor();
								newSponsor.ApplicationForm = createdApplicationForm;
								newSponsor.Person = createdPerson;
								newSponsor.ContactAddress = sponsor.ContactAddress;
								newSponsor.MobilePhone = sponsor.MobilePhone;
								newSponsor.Name = sponsor.Name;
								newSponsor.Relationship = sponsor.Relationship;

								sponsorLogic.Create(newSponsor);
							}

							viewModel.ApplicationForm = applicationFormLogic.GetModelBy(a => a.Application_Form_Id == createdApplicationForm.Id);
							viewModel.ApplicationForm.Person = personLogic.GetModelBy(p => p.Person_Id == createdPerson.Id);
							SetMessage("Your Change Of Course Fee payment has been successfully confirmed, your new application details can be seen below. You can now proceed with registration using the Check Admission Status link", Message.Category.Information);

						}
					}
					else
					{
						SetMessage("Error! the confirmation order number entered is not valid", Message.Category.Error);
					}
				}
			}
			catch (Exception ex)
			{
				SetMessage("Error! " + ex.Message, Message.Category.Error);
			}

			return View(viewModel);
		}
		private bool IsNextApplicationStatus(long formId, int status)
		{
			try
			{
				ApplicationForm form = new ApplicationForm() { Id = formId };

				ApplicantLogic applicantLogic = new ApplicantLogic();
				Model.Model.Applicant applicant = applicantLogic.GetBy(form);
				if (applicant != null)
				{
					if (applicant.Status.Id < (int)status)
					{
						return true;
					}
				}
				else
				{
					throw new Exception("Applicant Status not found!");
				}

				return false;
			}
			catch (Exception)
			{
				throw;
			}
		}

		private void SetSelectedSittingSubjectAndGrade(AdmissionViewModel olevelViewModel)
		{
			try
			{
				if (olevelViewModel != null && olevelViewModel.FirstSittingOLevelResultDetails != null && olevelViewModel.FirstSittingOLevelResultDetails.Count > 0)
				{
					int i = 0;
					foreach (OLevelResultDetail firstSittingOLevelResultDetail in olevelViewModel.FirstSittingOLevelResultDetails)
					{
						if (firstSittingOLevelResultDetail.Subject != null && firstSittingOLevelResultDetail.Grade != null)
						{
							ViewData["FirstSittingOLevelSubjectId" + i] = new SelectList(olevelViewModel.OLevelSubjectSelectList, Utility.VALUE, Utility.TEXT, firstSittingOLevelResultDetail.Subject.Id);
							ViewData["FirstSittingOLevelGradeId" + i] = new SelectList(olevelViewModel.OLevelGradeSelectList, Utility.VALUE, Utility.TEXT, firstSittingOLevelResultDetail.Grade.Id);
						}
						else
						{
							ViewData["FirstSittingOLevelSubjectId" + i] = new SelectList(olevelViewModel.OLevelSubjectSelectList, Utility.VALUE, Utility.TEXT, 0);
							ViewData["FirstSittingOLevelGradeId" + i] = new SelectList(olevelViewModel.OLevelGradeSelectList, Utility.VALUE, Utility.TEXT, 0);
						}

						i++;
					}
				}

				if (olevelViewModel != null && olevelViewModel.SecondSittingOLevelResultDetails != null && olevelViewModel.SecondSittingOLevelResultDetails.Count > 0)
				{
					int i = 0;
					foreach (OLevelResultDetail secondSittingOLevelResultDetail in olevelViewModel.SecondSittingOLevelResultDetails)
					{
						if (secondSittingOLevelResultDetail.Subject != null && secondSittingOLevelResultDetail.Grade != null)
						{
							ViewData["SecondSittingOLevelSubjectId" + i] = new SelectList(olevelViewModel.OLevelSubjectSelectList, Utility.VALUE, Utility.TEXT, secondSittingOLevelResultDetail.Subject.Id);
							ViewData["SecondSittingOLevelGradeId" + i] = new SelectList(olevelViewModel.OLevelGradeSelectList, Utility.VALUE, Utility.TEXT, secondSittingOLevelResultDetail.Grade.Id);
						}
						else
						{
							ViewData["SecondSittingOLevelSubjectId" + i] = new SelectList(olevelViewModel.OLevelSubjectSelectList, Utility.VALUE, Utility.TEXT, 0);
							ViewData["SecondSittingOLevelGradeId" + i] = new SelectList(olevelViewModel.OLevelGradeSelectList, Utility.VALUE, Utility.TEXT, 0);
						}

						i++;
					}
				}
			}
			catch (Exception ex)
			{
				SetMessage(ex.Message, Message.Category.Error);
			}
		}

		private bool InvalidOlevelResult(AdmissionViewModel viewModel)
		{
			try
			{
				if (InvalidNumberOfOlevelSubject(viewModel.FirstSittingOLevelResultDetails, viewModel.SecondSittingOLevelResultDetails))
				{
					return true;
				}

				if (InvalidOlevelSubjectOrGrade(viewModel.FirstSittingOLevelResultDetails, viewModel.OLevelSubjects, viewModel.OLevelGrades, Utility.FIRST_SITTING))
				{
					return true;
				}

				if (viewModel.SecondSittingOLevelResult != null)
				{
					if (viewModel.SecondSittingOLevelResult.ExamNumber != null && viewModel.SecondSittingOLevelResult.Type != null && viewModel.SecondSittingOLevelResult.Type.Id > 0 && viewModel.SecondSittingOLevelResult.ExamYear > 0)
					{
						if (InvalidOlevelSubjectOrGrade(viewModel.SecondSittingOLevelResultDetails, viewModel.OLevelSubjects, viewModel.OLevelGrades, Utility.SECOND_SITTING))
						{
							return true;
						}
					}
				}

				if (InvalidOlevelResultHeaderInformation(viewModel.FirstSittingOLevelResultDetails, viewModel.FirstSittingOLevelResult, Utility.FIRST_SITTING))
				{
					return true;
				}

				if (InvalidOlevelResultHeaderInformation(viewModel.SecondSittingOLevelResultDetails, viewModel.SecondSittingOLevelResult, Utility.SECOND_SITTING))
				{
					return true;
				}

				if (NoOlevelSubjectSpecified(viewModel.FirstSittingOLevelResultDetails, viewModel.FirstSittingOLevelResult, Utility.FIRST_SITTING))
				{
					return true;
				}
				if (NoOlevelSubjectSpecified(viewModel.SecondSittingOLevelResultDetails, viewModel.SecondSittingOLevelResult, Utility.SECOND_SITTING))
				{
					return true;
				}

				//if (InvalidOlevelType(viewModel.FirstSittingOLevelResult.Type, viewModel.SecondSittingOLevelResult.Type))
				//{
				//    return true;
				//}

				return false;
			}
			catch (Exception)
			{
				throw;
			}
		}

		private bool InvalidOlevelSubjectOrGrade(List<OLevelResultDetail> oLevelResultDetails, List<OLevelSubject> subjects, List<OLevelGrade> grades, string sitting)
		{
			try
			{
				List<OLevelResultDetail> subjectList = null;
				if (oLevelResultDetails != null && oLevelResultDetails.Count > 0)
				{
					subjectList = oLevelResultDetails.Where(r => r.Subject.Id > 0 || r.Grade.Id > 0).ToList();
				}

				foreach (OLevelResultDetail oLevelResultDetail in subjectList)
				{
					OLevelSubject subject = subjects.Where(s => s.Id == oLevelResultDetail.Subject.Id).SingleOrDefault();
					OLevelGrade grade = grades.Where(g => g.Id == oLevelResultDetail.Grade.Id).SingleOrDefault();

					List<OLevelResultDetail> results = subjectList.Where(o => o.Subject.Id == oLevelResultDetail.Subject.Id).ToList();
					if (results != null && results.Count > 1)
					{
						SetMessage("Duplicate " + subject.Name.ToUpper() + " Subject detected in " + sitting + "! Please modify.", Message.Category.Error);
						return true;
					}
					else if (oLevelResultDetail.Subject.Id > 0 && oLevelResultDetail.Grade.Id <= 0)
					{
						SetMessage("No Grade specified for Subject " + subject.Name.ToUpper() + " in " + sitting + "! Please modify.", Message.Category.Error);
						return true;
					}
					else if (oLevelResultDetail.Subject.Id <= 0 && oLevelResultDetail.Grade.Id > 0)
					{
						SetMessage("No Subject specified for Grade" + grade.Name.ToUpper() + " in " + sitting + "! Please modify.", Message.Category.Error);
						return true;
					}
				}

				return false;
			}
			catch (Exception)
			{
				throw;
			}
		}

		private bool InvalidOlevelResultHeaderInformation(List<OLevelResultDetail> resultDetails, OLevelResult oLevelResult, string sitting)
		{
			try
			{
				if (resultDetails != null && resultDetails.Count > 0)
				{
					List<OLevelResultDetail> subjectList = resultDetails.Where(r => r.Subject.Id > 0).ToList();

					if (subjectList != null && subjectList.Count > 0)
					{
						if (string.IsNullOrEmpty(oLevelResult.ExamNumber))
						{
							SetMessage("O-Level Exam Number not set for " + sitting + " ! Please modify", Message.Category.Error);
							return true;
						}
						else if (oLevelResult.Type == null || oLevelResult.Type.Id <= 0)
						{
							SetMessage("O-Level Exam Type not set for " + sitting + " ! Please modify", Message.Category.Error);
							return true;
						}
						else if (oLevelResult.ExamYear <= 0)
						{
							SetMessage("O-Level Exam Year not set for " + sitting + " ! Please modify", Message.Category.Error);
							return true;
						}
					}
				}

				return false;
			}
			catch (Exception)
			{
				throw;
			}
		}

		private bool NoOlevelSubjectSpecified(List<OLevelResultDetail> oLevelResultDetails, OLevelResult oLevelResult, string sitting)
		{
			try
			{
				if (!string.IsNullOrEmpty(oLevelResult.ExamNumber) || (oLevelResult.Type != null && oLevelResult.Type.Id > 0) || (oLevelResult.ExamYear > 0))
				{
					if (oLevelResultDetails != null && oLevelResultDetails.Count > 0)
					{
						List<OLevelResultDetail> oLevelResultDetailsEntered = oLevelResultDetails.Where(r => r.Subject.Id > 0).ToList();
						if (oLevelResultDetailsEntered == null || oLevelResultDetailsEntered.Count <= 0)
						{
							SetMessage("No O-Level Subject specified for " + sitting + "! At least one subject must be specified when Exam Number, O-Level Type and Year are all specified for the sitting.", Message.Category.Error);
							return true;
						}
					}
				}

				return false;
			}
			catch (Exception)
			{
				throw;
			}
		}

		private bool InvalidNumberOfOlevelSubject(List<OLevelResultDetail> firstSittingResultDetails, List<OLevelResultDetail> secondSittingResultDetails)
		{
			const int FIVE = 5;

			try
			{
				int totalNoOfSubjects = 0;

				List<OLevelResultDetail> firstSittingSubjectList = null;
				List<OLevelResultDetail> secondSittingSubjectList = null;

				if (firstSittingResultDetails != null && firstSittingResultDetails.Count > 0)
				{
					firstSittingSubjectList = firstSittingResultDetails.Where(r => r.Subject.Id > 0).ToList();
					if (firstSittingSubjectList != null)
					{
						totalNoOfSubjects += firstSittingSubjectList.Count;
					}
				}

				if (secondSittingResultDetails != null && secondSittingResultDetails.Count > 0)
				{
					secondSittingSubjectList = secondSittingResultDetails.Where(r => r.Subject.Id > 0).ToList();
					if (secondSittingSubjectList != null)
					{
						totalNoOfSubjects += secondSittingSubjectList.Count;
					}
				}

				if (totalNoOfSubjects == 0)
				{
					SetMessage("No O-Level Result Details found for both sittings!", Message.Category.Error);
					return true;
				}
				else if (totalNoOfSubjects < FIVE)
				{
					SetMessage("O-Level Result cannot be less than " + FIVE + " subjects in both sittings!", Message.Category.Error);
					return true;
				}

				return false;
			}
			catch (Exception)
			{
				throw;
			}
		}

		private bool InvalidOlevelType(OLevelType firstSittingOlevelType, OLevelType secondSittingOlevelType)
		{
			try
			{
				if (firstSittingOlevelType != null && secondSittingOlevelType != null)
				{
					if ((firstSittingOlevelType.Id != secondSittingOlevelType.Id) && firstSittingOlevelType.Id > 0 && secondSittingOlevelType.Id > 0)
					{
						if (firstSittingOlevelType.Id == (int)OLevelTypes.Nabteb)
						{
							SetMessage("NABTEB O-Level Type in " + Utility.FIRST_SITTING + " cannot be combined with any other O-Level Type! Please modify.", Message.Category.Error);
							return true;
						}
						else if (secondSittingOlevelType.Id == (int)OLevelTypes.Nabteb)
						{
							SetMessage("NABTEB O-Level Type in " + Utility.SECOND_SITTING + " cannot be combined with any other O-Level Type! Please modify.", Message.Category.Error);
							return true;
						}
					}
				}

				return false;
			}
			catch (Exception)
			{
				throw;
			}
		}
		[HttpPost]
		public ActionResult VerifyOlevelResult(AdmissionViewModel viewModel)
		{
			string enc = Abundance_Nk.Web.Models.Utility.Encrypt(viewModel.ApplicationForm.Id.ToString());

			try
			{
				//check if applicant has been previously cleared
				ApplicantClearanceLogic applicantClearanceLogic = new ApplicantClearanceLogic();
				if (applicantClearanceLogic.IsCleared(viewModel.ApplicationForm))
				{
					if (string.IsNullOrWhiteSpace(viewModel.ApplicationForm.RejectReason))
					{
						if (viewModel.ApplicantStatusId < (int)ApplicantStatus.Status.ClearedAndAccepted)
						{
							ApplicantStatus.Status newStatus = ApplicantStatus.Status.ClearedAndAccepted;
							ApplicantLogic applicantLogic = new ApplicantLogic();
							applicantLogic.UpdateStatus(viewModel.ApplicationForm, newStatus); 
						}
						SetMessage("You have already been successfully cleared. Congratulations! kindly move on to next step.", Message.Category.Information);
						return RedirectToAction("Index", new { fid = enc });
					}
					else
					{
						SetMessage("You have already been successfully cleared. But we regret to re-inform you that you did not qualify with the following reason: " + viewModel.ApplicationForm.RejectReason + ". Kindly try again next academic session if you've corrected your deficiency.", Message.Category.Information);
					}
				}

				//validate o-level result entry
				if (InvalidOlevelResult(viewModel))
				{
					return RedirectToAction("Index", new { fid = enc });
				}

				using (TransactionScope transaction = new TransactionScope())
				{
					//get applicant's applied course
					if (viewModel.FirstSittingOLevelResult == null || viewModel.FirstSittingOLevelResult.Id <= 0)
					{
						viewModel.FirstSittingOLevelResult.ApplicationForm = viewModel.ApplicationForm;
						viewModel.FirstSittingOLevelResult.Person = viewModel.ApplicationForm.Person;
						viewModel.FirstSittingOLevelResult.Sitting = new OLevelExamSitting() { Id = 1 };
					}

					if (viewModel.SecondSittingOLevelResult == null || viewModel.SecondSittingOLevelResult.Id <= 0)
					{
						viewModel.SecondSittingOLevelResult.ApplicationForm = viewModel.ApplicationForm;
						viewModel.SecondSittingOLevelResult.Person = viewModel.ApplicationForm.Person;
						viewModel.SecondSittingOLevelResult.Sitting = new OLevelExamSitting() { Id = 2 };
					}

					ModifyOlevelResult(viewModel.FirstSittingOLevelResult, viewModel.FirstSittingOLevelResultDetails);
					ModifyOlevelResult(viewModel.SecondSittingOLevelResult, viewModel.SecondSittingOLevelResultDetails);


					//get applicant's applied course
					AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
					AdmissionListLogic admissionListLogic = new AdmissionListLogic();
					AppliedCourse appliedCourse = appliedCourseLogic.GetBy(viewModel.ApplicationForm.Person);

					//Set department to admitted department since it might vary
					AdmissionList admissionList = new AdmissionList();
					admissionList = admissionListLogic.GetBy(viewModel.ApplicationForm.Person);
					appliedCourse.Department = admissionList.Deprtment;

					if (appliedCourse == null)
					{
						SetMessage("Your O-Level was successfully verified, but could not be cleared because no Applied Course was not found for your application", Message.Category.Error);
						return RedirectToAction("Index", new { fid = enc });
					}

					//set reject reason if exist
					ApplicantStatus.Status newStatus;
					AdmissionCriteriaLogic admissionCriteriaLogic = new AdmissionCriteriaLogic();
					string rejectReason = admissionCriteriaLogic.EvaluateApplication(appliedCourse);
					if (string.IsNullOrWhiteSpace(rejectReason))
					{
						newStatus = ApplicantStatus.Status.ClearedAndAccepted;
						//newStatus = ApplicantStatus.Status.GeneratedAcceptanceReceipt;
						viewModel.ApplicationForm.Rejected = false;
						viewModel.ApplicationForm.Release = false;
					}
					else
					{
						newStatus = ApplicantStatus.Status.ClearedAndRejected;
						viewModel.ApplicationForm.Rejected = true;
						viewModel.ApplicationForm.Release = true;
					}

					viewModel.ApplicationForm.RejectReason = rejectReason;
					ApplicationFormLogic applicationFormLogic = new ApplicationFormLogic();
					applicationFormLogic.SetRejectReason(viewModel.ApplicationForm);


					//set applicant new status
					ApplicantLogic applicantLogic = new ApplicantLogic();
					applicantLogic.UpdateStatus(viewModel.ApplicationForm, newStatus);

					
					//save clearance metadata
					ApplicantClearance applicantClearance = new ApplicantClearance();
					applicantClearance = applicantClearanceLogic.GetBy(viewModel.ApplicationForm);
					if (applicantClearance == null)
					{
						applicantClearance = new ApplicantClearance();
						applicantClearance.ApplicationForm = viewModel.ApplicationForm;
						applicantClearance.Cleared = string.IsNullOrWhiteSpace(viewModel.ApplicationForm.RejectReason) ? true : false;
						applicantClearance.DateCleared = DateTime.Now;
						applicantClearanceLogic.Create(applicantClearance);
					}
					else
					{
						applicantClearance.Cleared = string.IsNullOrWhiteSpace(viewModel.ApplicationForm.RejectReason) ? true : false;
						applicantClearance.DateCleared = DateTime.Now;
						applicantClearanceLogic.Modify(applicantClearance);
					}

					transaction.Complete();
				}

				if (string.IsNullOrWhiteSpace(viewModel.ApplicationForm.RejectReason))
				{
					SetMessage("O-Level result has been successfully verified and you have been automatically cleared by the system", Message.Category.Information);
				}
				else
				{
					SetMessage("We regret to inform you that you did not qualify with the following reason: " + viewModel.ApplicationForm.RejectReason, Message.Category.Error);
				}
			}
			catch (Exception ex)
			{
				SetMessage(ex.Message, Message.Category.Error);
			}

			return RedirectToAction("Index", new { fid = enc });
		}
		private void ModifyOlevelResult(OLevelResult oLevelResult, List<OLevelResultDetail> oLevelResultDetails)
		{
			try
			{
				OlevelResultdDetailsAudit olevelResultdDetailsAudit = new OlevelResultdDetailsAudit();
				olevelResultdDetailsAudit.Action = "Modify";
				olevelResultdDetailsAudit.Operation = "Modify O level (Admission Controller)";
				olevelResultdDetailsAudit.Client =  Request.LogonUserIdentity.Name + " (" + HttpContext.Request.UserHostAddress + ")";
				UserLogic loggeduser = new UserLogic();
				olevelResultdDetailsAudit.User = loggeduser.GetModelBy(u => u.User_Id == 1);

				OLevelResultDetailLogic oLevelResultDetailLogic = new OLevelResultDetailLogic();
				if (oLevelResult != null && oLevelResult.ExamNumber != null && oLevelResult.Type != null && oLevelResult.ExamYear > 0)
				{
					if (oLevelResult != null && oLevelResult.Id > 0)
					{
						oLevelResultDetailLogic.DeleteBy(oLevelResult,olevelResultdDetailsAudit);
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
			catch (Exception)
			{
				throw;
			}
		}

        public ActionResult PrintCertificateReceipt()
        {
            return View();
        }

        [HttpPost]
        public ActionResult PrintCertificateReceipt(AdmissionViewModel viewModel)
        {
            try
            {
                if (viewModel.ConfirmationOrderNumber != null)
                {
                    RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                    RemitaPayment remitaPayment = new RemitaPayment();
                    RemitaPayementProcessor remitaPayementProcessor = new RemitaPayementProcessor("918567");
                    var checkPayment = remitaPaymentLogic.GetModelsBy(a => a.RRR == viewModel.ConfirmationOrderNumber).FirstOrDefault();
                    if(checkPayment != null)
                    {
                        if (checkPayment.Status.Contains("01:"))
                        {
                            return RedirectToAction("Receipt", "Credential", new { Area = "Common", pmid = checkPayment.payment.Id });
                        }
                        else
                        {
                            var getStatus = remitaPayementProcessor.GetStatus(checkPayment.OrderId);
                            if (getStatus.Status.Contains("01:"))
                            {
                                return RedirectToAction("Receipt", "Credential", new { Area = "Common", pmid = getStatus.payment.Id });
                            }
                            else
                            {
                                SetMessage("Payment Not Successful", Message.Category.Error);
                            }
                        }
                    }
                    else
                    {
                        PaymentLogic paymentLogic = new PaymentLogic();

                        if (viewModel.ConfirmationOrderNumber.Length > 12)
                        {
                            Model.Model.Session session = new Model.Model.Session() { Id = 7 };
                            //old convocation fee check
                            FeeType feetype = new FeeType() { Id = (int)FeeTypes.ConvocationFee };
                            Payment payment = paymentLogic.InvalidConfirmationOrderNumber(viewModel.ConfirmationOrderNumber, feetype.Id);
                            if (payment != null && payment.Id > 0)
                            {
                                if (payment.FeeType.Id != (int)FeeTypes.ConvocationFee)
                                {
                                    SetMessage("Confirmation Order Number / RRR (" + viewModel.ConfirmationOrderNumber + ") entered is not for convocation fee payment! Please enter your convocation fee confirmation order number / RRR.", Message.Category.Error);
                                    return View(viewModel);
                                }

                                return RedirectToAction("Receipt", "Credential", new { area = "Common", pmid = payment.Id });
                            }
                        }

                        //SetMessage("Enter a valid RRR Number", Message.Category.Error);
                        SetMessage("Please enter a valid RRR Number", Message.Category.Error);
                    }
                }
            }
            catch(Exception ex)
            {
             SetMessage(ex.Message, Message.Category.Error);
            }
            return View();
        }

        public ActionResult CreateHostelRequest(string AcceptanceRRR)
        {
            JsonResultModel result = new JsonResultModel();
            try
            {
                //if (viewModel.Student.MatricNumber != null)
                //{
                HostelRequestLogic hostelRequestLogic = new HostelRequestLogic();
                SessionLogic _sessionLogic = new SessionLogic();
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


                Session session = _sessionLogic.GetModelsBy(s => s.Active_For_Hostel != null && s.Active_For_Hostel.Value).LastOrDefault() ?? new Session { Id = (int)Sessions._20182019 };

                //List<Model.Model.Student> students = studentLogic.GetModelsBy(s => s.Matric_Number == viewModel.Student.MatricNumber);


                RemitaPaymentLogic _remitaPaymentLogic = new RemitaPaymentLogic();
                RemitaPayment remitaPayment = _remitaPaymentLogic.GetModelsBy(r => r.RRR == AcceptanceRRR &&
                                            (r.PAYMENT.Fee_Type_Id == (int)FeeTypes.AcceptanceFee || r.PAYMENT.Fee_Type_Id == (int)FeeTypes.HNDAcceptance) &&
                                            r.PAYMENT.Session_Id == session.Id && r.Status.Contains("01")).LastOrDefault();
                if (remitaPayment == null)
                {
                    //SetMessage("RRR Number is not for Current session's Acceptance Fee!", Message.Category.Error);
                    result.IsError = true;
                    result.Message = "RRR Number is not for Current session's Acceptance Fee!";
                    return Json(result, JsonRequestBehavior.AllowGet);

                    //return View(viewModel);
                }

                person = remitaPayment.payment.Person;
                AppliedCourse appliedCourse = appliedCourseLogic.GetModelBy(a => a.Person_Id == person.Id);
                if (appliedCourse == null)
                {
                    //SetMessage("No Applied course record!", Message.Category.Error);
                    //return View(viewModel);
                    result.IsError = true;
                    result.Message = "No Applied course record!";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                programme = appliedCourse.Programme;
                department = appliedCourse.Department;
                level = new Level() { Id = 1 };
                if (programme.Id == 3)
                {
                    level = new Level() { Id = 3 };
                }

                //check blacklist

                HostelBlacklist hostelBlacklist = hostelBlacklistLogic.GetModelsBy(h => h.Person_Id == person.Id && h.Session_Id == session.Id).LastOrDefault();

                if (hostelBlacklist != null)
                {
                    result.IsError = true;
                    result.Message = "You cannot request for hostel allocation because of " + hostelBlacklist.Reason;
                    return Json(result, JsonRequestBehavior.AllowGet);

                }

                HostelRequest hostelRequest = hostelRequestLogic.GetModelBy(h => h.Person_Id == person.Id && h.Session_Id == session.Id && h.Expired == false);
                //Check for Sex
                if (person.Sex == null)
                {
                    result.IsError = true;
                    result.Message = "Error! Ensure that your student profile(Sex) is completely filled";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                if (hostelRequest == null)
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

                    result.IsError = false;
                    result.Message = "Your request has been submitted. Check Back Later For Invoice Generation!";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                if (hostelRequest != null && hostelRequest.Approved)
                {

                    result.IsError = false;
                    result.Message = "Your request has been approved proceed to generate invoice!";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                if (hostelRequest != null && !hostelRequest.Approved)
                {
                    //SetMessage("Your request has not been approved!", Message.Category.Error);
                    //return View(viewModel);
                    result.IsError = false;
                    result.Message = "Your request has not been approved!";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                //}
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Message = "Error Occurred!" + ex.Message;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
            //return Json(new { InvoiceNumber = payment.InvoiceNumber }, "text/html", JsonRequestBehavior.AllowGet);
        }
        public ActionResult GenerateHostelInvoice(string AcceptanceRRR)
        {
            JsonResultModel result = new JsonResultModel();
            try
            {
                HostelAllocationLogic hostelAllocationLogic = new HostelAllocationLogic();
                HostelAllocationCriteriaLogic hostelAllocationCriteriaLogic = new HostelAllocationCriteriaLogic();
                PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
                PersonLogic personLogic = new PersonLogic();
                PaymentLogic paymentLogic = new PaymentLogic();
                SessionLogic _sessionLogic = new SessionLogic();
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


                var hostelApplicationSession = _sessionLogic.GetModelsBy(s => s.Active_For_Hostel != null && s.Active_For_Hostel.Value).LastOrDefault();
                List<HostelAllocationCriteria> hostelAllocationCriteriaList = new List<HostelAllocationCriteria>();

                RemitaPaymentLogic _remitaPaymentLogic = new RemitaPaymentLogic();
                RemitaPayment remitaPayment = _remitaPaymentLogic.GetModelsBy(r => r.RRR == AcceptanceRRR &&
                                            (r.PAYMENT.Fee_Type_Id == (int)FeeTypes.AcceptanceFee || r.PAYMENT.Fee_Type_Id == (int)FeeTypes.HNDAcceptance) &&
                                            r.PAYMENT.Session_Id == hostelApplicationSession.Id && r.Status.Contains("01")).LastOrDefault();
                if (remitaPayment == null)
                {
                    result.IsError = true;
                    result.Message = "Please Pay your acceptance before proceeding to Hostel Processes!";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                HostelRequest hostelRequest = hostelRequestLogic.GetModelsBy(h => h.Person_Id == remitaPayment.payment.Person.Id && h.Session_Id == hostelApplicationSession.Id && h.Expired == false).LastOrDefault();
                if (hostelRequest == null)
                {
                    //SetMessage("Make a request for hostel allocation before generating invoice!", Message.Category.Error);
                    result.IsError = true;
                    result.Message = "Make a request for hostel allocation before generating invoice!";
                    return Json(result, JsonRequestBehavior.AllowGet);

                }
                if (hostelRequest != null && !hostelRequest.Approved)
                {

                    result.IsError = true;
                    result.Message = "Your request for hostel allocation has not been approved!";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                //Cover for records without Approval Date
                if (hostelRequest.ApprovalDate != null)
                {
                    // check for expiration

                    var isValid = CheckHostelRequestValidity(remitaPayment.payment.Person, hostelApplicationSession);
                    if (!isValid)
                    {
                        result.IsError = true;
                        result.Message = "Your request for hostel allocation has Expired, More Than 3 Days from the day of Approval, Request again!";
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }


                //using (TransactionScope scope = new TransactionScope())
                //{
                payment = CreatePayment(remitaPayment.payment.Person, hostelApplicationSession, hostelRequest.Hostel);
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
                RemitaPayment remitaPayment2 = remitaProcessor.GenerateRRRCard(payment.InvoiceNumber, remitaBaseUrl, "ACCOMODATION FEES", settings, amt);

                //if (remitaPayment2 != null)
                //{
                //    //scope.Complete();
                //}
                ////}
                //TempData["Payment"] = payment;

                //return RedirectToAction("HostelInvoice");
                result.IsError = false;
                result.Message = "Invoice Generated successfully.Click Print Hostel Invoice";
                result.HostelInvoiceNo = payment.InvoiceNumber;
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {
                result.IsError = true;
                result.Message = "Error" + ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public bool CheckHostelRequestValidity(Person person, Session session)
        {
            try
            {
                HostelRequestLogic hostelRequestLogic = new HostelRequestLogic();
                HostelRequestCountLogic requestCountLogic = new HostelRequestCountLogic();
                HostelRequest hostelRequest = hostelRequestLogic.GetModelsBy(h => h.Person_Id == person.Id && h.Session_Id == session.Id && h.Expired == false).LastOrDefault();
                if (hostelRequest != null && hostelRequest.Reason != null && hostelRequest.Reason.Contains("PAID"))
                {
                    return true;
                }
                if ( hostelRequest != null && hostelRequest.ApprovalDate != null)
                {
                    // check for expiration
                    HostelRequestCount requestCount = requestCountLogic.GetModelBy(h => h.Level_Id == hostelRequest.Level.Id && h.Sex_Id == hostelRequest.Person.Sex.Id && h.Approved);
                    TimeSpan DateTimeDiff;
                    DateTimeDiff = TimeSpan.Zero;
                    DateTime currentDate = DateTime.Now;
                    DateTimeDiff = currentDate - (DateTime)hostelRequest.ApprovalDate;
                    var sumDayAndHour = (DateTimeDiff.Days * 24) + DateTimeDiff.Hours;
                    if (hostelRequest != null && sumDayAndHour > 72)
                    {
                        using (TransactionScope scope = new TransactionScope())
                        {

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
            catch (Exception ex)
            {
                throw ex;
            }
            return true;
        }
        private Payment CreatePayment(Person person, Session session, Hostel hostel)
        {

            try
            {
                PaymentLogic paymentLogic = new PaymentLogic();
                OnlinePaymentLogic onlinePaymentLogic = new OnlinePaymentLogic();

                Payment newPayment = new Payment();

                PaymentMode paymentMode = new PaymentMode() { Id = 1 };
                PaymentType paymentType = new PaymentType() { Id = 2 };
                PersonType personType = person.Type;
                FeeType feeType = new FeeType() { Id = (int)FeeTypes.HostelFee };

                Payment payment = new Payment();
                payment.PaymentMode = paymentMode;
                payment.PaymentType = paymentType;
                payment.PersonType = personType;
                payment.FeeType = feeType;
                payment.DatePaid = DateTime.Now;
                payment.Person = person;
                payment.Session = session;

                Payment checkPayment = paymentLogic.GetModelBy(p => p.Person_Id == person.Id && p.Fee_Type_Id == feeType.Id && p.Session_Id == session.Id);
                if (checkPayment != null)
                {
                    newPayment = checkPayment;
                }
                else
                {
                    newPayment = paymentLogic.Create(payment);

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
                }

                double amount = 0;
                if (hostel == null)
                {
                    amount = 15000;
                }
                else
                {
                    amount = GetHostelFee(hostel);
                }


                HostelFeeLogic hostelFeeLogic = new HostelFeeLogic();
                HostelFee hostelFee = new HostelFee();

                HostelFee existingHostelFee = hostelFeeLogic.GetModelsBy(h => h.Payment_Id == newPayment.Id).LastOrDefault();

                if (existingHostelFee == null && hostel != null)
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
                hostelAmount = hostelAmountLogic.GetModelBy(p => p.Hostel_Id == hostel.Id);

                amount = Convert.ToDouble(hostelAmount.Amount);
            }
            catch (Exception)
            {
                throw;
            }

            return amount;
        }
        public ActionResult HostelInvoice(string hostelivn)
        {

           // var redirectpayment = (Payment)TempData["Payment"];
            try
            {
               
                if (!String.IsNullOrEmpty(hostelivn))
                {
                    var inv = hostelivn.Trim();
                    //Int64 paymentid = Convert.ToInt64(Abundance_Nk.Web.Models.Utility.Decrypt(pmid));
                    PaymentLogic paymentLogic = new PaymentLogic();
                    HostelRequestLogic hostelRequestLogic = new HostelRequestLogic();
                    Payment payment = paymentLogic.GetModelBy(p => p.Invoice_Number == inv);
                    if (payment.FeeType.Id == (int)FeeTypes.HostelFee)
                    {
                        Invoice invoice = new Invoice();
                        invoice.Person = payment.Person;
                        invoice.Payment = payment;
                        invoice.FeeDetails = new List<FeeDetail>();

                        var hostelRequest = hostelRequestLogic.GetModelBy(d => d.Payment_Id == payment.Id);
                        if (hostelRequest != null)
                        {
                            DateTime approveDate = (DateTime)hostelRequest.ApprovalDate;
                            var validityPeriod = approveDate.AddDays(3);
                            invoice.PaymentValidityPeriod = validityPeriod.ToString("dddd, dd MMMM yyyy h:mm tt");
                        }


                        RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                        RemitaPayment remitaPayment = remitaPaymentLogic.GetBy(payment.Id);

                        invoice.paymentEtranzactType = new PaymentEtranzactType { Name = "FEDPOLY ILARO ACCOMODATION FEE" };
                        invoice.remitaPayment = remitaPayment;
                        //PaymentEtranzact paymentEtranzact = paymentEtranzactLogic.GetBy(payment);
                        if (remitaPayment != null && remitaPayment.Status.Contains("01"))
                        {
                            invoice.Paid = true;
                        }

                        HostelFeeLogic hostelFeeLogic = new HostelFeeLogic();
                        HostelFee hostelFee = hostelFeeLogic.GetModelBy(h => h.Payment_Id == payment.Id);

                        invoice.Amount = Convert.ToDecimal(hostelFee.Amount);

                        invoice.Payment.FeeDetails = null;
                        

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
        public ActionResult PayHostelFee(string HostelRRR, string hostelInvoiceNo)
        {
            JsonResultModel result = new JsonResultModel();
            try
            {
                if (HostelRRR != null && hostelInvoiceNo!=null)
                {
                    //PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
                    PaymentLogic paymentLogic = new PaymentLogic();
                    HostelAllocationLogic hostelAllocationLogic = new HostelAllocationLogic();
                    RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                    SessionLogic sessionLogic = new SessionLogic();
                    HostelAllocationCountLogic hostelAllocationCountLogic = new HostelAllocationCountLogic();
                    HostelAllocationCriteriaLogic hostelAllocationCriteriaLogic = new HostelAllocationCriteriaLogic();
                    List<HostelAllocationCriteria> hostelAllocationCriteriaList = new List<HostelAllocationCriteria>();
                    //StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                    HostelAllocation hostelAllocation = new HostelAllocation();
                    HostelRequestLogic hostelRequestLogic = new HostelRequestLogic();


                    Payment payment = null;
                    payment = paymentLogic.GetModelBy(g => g.Invoice_Number == hostelInvoiceNo.Trim());
                    RemitaPayment remitaPayment = new RemitaPayment();
                    RemitaPayementProcessor remitaPayementProcessor = new RemitaPayementProcessor("918567");

                    if (HostelRRR.Length >= 12)
                    {
                        Model.Model.Session session = sessionLogic.GetHostelSession();
                        FeeType feetype = new FeeType() { Id = (int)FeeTypes.HostelFee };

                        remitaPayment = remitaPaymentLogic.GetModelsBy(a => a.RRR == HostelRRR.Trim()).FirstOrDefault();

                        if (remitaPayment != null)
                        {
                            remitaPayment = remitaPayementProcessor.GetStatus(remitaPayment.OrderId);
                            if (remitaPayment.Status.Contains("01:") || remitaPayment.Description.Contains("Manual") && remitaPayment.payment.Id == payment.Id)
                            {
                                payment = remitaPayment.payment;
                            }
                            else
                            {
                                result.IsError = true;
                                result.Message = "Ensure you are entering the correct RRR";
                                return Json(result, JsonRequestBehavior.AllowGet);
                            }
                        }

                        if (payment != null && payment.Id > 0)
                        {
                            if (payment.FeeType.Id != (int)FeeTypes.HostelFee)
                            {
                                result.IsError = true;
                                result.Message = "Confirmation Order Number / RRR (" + HostelRRR + ") entered is not for Hostel Fee payment!Please enter your " +

                                               "Hostel Fee Confirmation Order Number.";
                                return Json(result, JsonRequestBehavior.AllowGet);
                            }
                            //Ensure that the request is is still valid with in the time of payment
                            var isValid = CheckHostelRequestValidity(payment.Person, payment.Session);
                            if (!isValid)
                            {

                                result.IsError = true;
                                result.Message = "Your request for hostel allocation has Expired, Contact Administration For Solution!";
                                return Json(result, JsonRequestBehavior.AllowGet);

                            }

                            // Allocate Hostel
                            //var existingHostelAllocation = hostelAllocationLogic.GetModelBy(ha => ha.Session_Id == payment.Session.Id && ha.Student_Id == payment.Person.Id && ha.Occupied);
                            //if (existingHostelAllocation != null)
                            //{

                            //    payment = paymentLogic.GetModelBy(p => p.Person_Id == payment.Person.Id && p.Fee_Type_Id == existingHostelAllocation.Payment.FeeType.Id && p.Session_Id == existingHostelAllocation.Session.Id);
                            //    return RedirectToAction("HostelReceipt", new { pmid = payment.Id });

                            //}
                            //var studentCurentLevel = studentLevelLogic.GetModelsBy(c => c.Person_Id == payment.Person.Id && c.Session_Id == payment.Session.Id).LastOrDefault();
                            var hostelRequest = hostelRequestLogic.GetModelsBy(d => d.Person_Id == payment.Person.Id && d.Session_Id == payment.Session.Id && d.Approved == true).LastOrDefault();
                            if (payment.Person.Sex == null)
                            {
                                result.IsError = true;
                                result.Message = "Error! Ensure that your student profile(Sex) is completely filled";
                                return Json(result, JsonRequestBehavior.AllowGet);
                            }

                            HostelAllocationCount hostelAllocationCount = hostelAllocationCountLogic.GetModelBy(h => h.Sex_Id == payment.Person.Sex.Id && h.Level_Id == hostelRequest.Level.Id);
                            if (hostelAllocationCount.Free == 0)
                            {

                                result.IsError = true;
                                result.Message = "Error! The Set Number for free Bed Spaces for your level has been exausted!";
                                return Json(result, JsonRequestBehavior.AllowGet);

                            }
                            var existhostelAllocation = hostelAllocationLogic.GetModelBy(h => h.PERSON.Person_Id== payment.Person.Id && h.Session_Id == hostelRequest.Session.Id);
                            if (existhostelAllocation == null)
                            {

                                hostelAllocationCriteriaList = hostelAllocationCriteriaLogic.GetModelsBy(hac => hac.Level_Id == hostelRequest.Level.Id && hac.HOSTEL.HOSTEL_TYPE.Hostel_Type_Name == payment.Person.Sex.Name &&
                                                            hac.HOSTEL_ROOM.Reserved == false && hac.HOSTEL_ROOM.Activated && hac.HOSTEL.Activated && hac.HOSTEL_SERIES.Activated &&
                                                            hac.HOSTEL_ROOM_CORNER.Activated);

                                if (hostelAllocationCriteriaList.Count == 0)
                                {

                                    result.IsError = true;
                                    result.Message = "Hostel Allocation Criteria for your Level has not been set!";
                                    return Json(result, JsonRequestBehavior.AllowGet);
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
                                    result.IsError = false;
                                    result.HostelInvoiceNo = payment.InvoiceNumber;
                                    result.Message = "Receipt Grenerated Successfully";
                                    return Json(result, JsonRequestBehavior.AllowGet);
                                }
                                result.IsError = true;
                                result.Message = "Bed Spaces have been exhausted for" + " " + hostelRequest.Level.Name;
                                return Json(result, JsonRequestBehavior.AllowGet);
                            }

                            result.IsError = false;
                            result.HostelInvoiceNo = payment.InvoiceNumber;
                            result.Message = "Receipt Grenerated Successfully";
                            return Json(result, JsonRequestBehavior.AllowGet);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Message = "Error" + ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            return View(viewModel);
        }
        public ActionResult HostelReceipt(string inv)
        {
            try
            {
                var viewModel = new HostelViewModel();
                HostelAllocationLogic hostelAllocationLogic = new HostelAllocationLogic();
                PaymentLogic paymentLogic = new PaymentLogic();
                HostelFeeLogic hostelFeeLogic = new HostelFeeLogic();
                HostelRequestLogic hostelRequestLogic = new HostelRequestLogic();

                HostelFee hostelFee = new HostelFee();
                Payment payment = new Payment();
                if (!String.IsNullOrEmpty(inv))
                {
                    payment = paymentLogic.GetModelBy(p => p.Invoice_Number == inv.Trim());
                    if (payment != null)
                    {
                        hostelFee = hostelFeeLogic.GetModelBy(h => h.Payment_Id == payment.Id);
                        HostelAllocation hostelAllocation = hostelAllocationLogic.GetModelBy(ha => ha.Payment_Id == payment.Id && ha.Session_Id == payment.Session.Id && ha.Student_Id == payment.Person.Id);
                        var request = hostelRequestLogic.GetModelsBy(d => d.Payment_Id == payment.Id).LastOrDefault();
                        if (request != null)
                        {
                            request.Expired = true;
                            request.Reason = "PAID";
                            hostelRequestLogic.Modify(request);
                        }
                        if (hostelAllocation != null)
                        {
                            viewModel.HostelAllocation = hostelAllocation;
                            viewModel.HostelFee = hostelFee;
                            return View(viewModel);
                        }
                    }

                }


            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        public bool HasAlreadyGeneratedSchoolFeesInvoice(string formNo)
        {
            ApplicationFormLogic applicationFormLogic = new ApplicationFormLogic();
            var applicationForm=applicationFormLogic.GetModelsBy(f => f.Application_Form_Number == formNo).FirstOrDefault();
            if (applicationForm?.Id > 0)
            {
                PaymentLogic paymentLogic = new PaymentLogic();
                var hasSchoolFeesInvoice=paymentLogic.GetModelsBy(f => f.Person_Id == applicationForm.Person.Id && f.Fee_Type_Id == (int)FeeTypes.SchoolFees).FirstOrDefault();
                if (hasSchoolFeesInvoice?.Id > 0)
                    return true;
            }
            return false;
        }
    }
}