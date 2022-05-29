using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Transactions;
using Abundance_Nk.Business;
using Abundance_Nk.Web.Models;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Web.Controllers;
using Abundance_Nk.Web.Areas.Student.ViewModels;
using System.Configuration;
using Abundance_Nk.Model.Entity.Model;

namespace Abundance_Nk.Web.Areas.Student.Controllers
{
	[AllowAnonymous]
	public class PaymentController : BaseController
	{
		private PersonLogic personLogic;
		private PaymentLogic paymentLogic;
		private OnlinePaymentLogic onlinePaymentLogic;
		private StudentLevelLogic studentLevelLogic;
		private StudentLogic studentLogic;

		private PaymentViewModel viewModel;

		public PaymentController()
		{
			personLogic = new PersonLogic();
			paymentLogic = new PaymentLogic();
			onlinePaymentLogic = new OnlinePaymentLogic();
			studentLevelLogic = new StudentLevelLogic();
			studentLogic = new StudentLogic();

			viewModel = new PaymentViewModel();
		}

		public ActionResult Index()
		{
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

		private void SetFeeTypeDropDown(PaymentViewModel viewModel)
		{
			try
			{
				if (viewModel.FeeTypeSelectListItem != null && viewModel.FeeTypeSelectListItem.Count > 0)
				{
					viewModel.FeeType.Id = (int)FeeTypes.SchoolFees;
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
		public ActionResult Index(PaymentViewModel viewModel)
		{
			try
			{
				if (InvalidMatricNumber(viewModel.Student.MatricNumber))
				{
					SetFeeTypeDropDown(viewModel);
					return View(viewModel);
				}
				
				Model.Model.Student student = studentLogic.GetBy(viewModel.Student.MatricNumber);
				if (student != null && student.Id > 0)
				{
					return RedirectToAction("GenerateInvoice", "Payment", new { sid = student.Id });
				}
				else
				{
					return RedirectToAction("GenerateInvoice", "Payment", new { sid = 0 });
				}
			}
			catch (Exception ex)
			{
				SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
			}

			SetFeeTypeDropDown(viewModel);
			return View(viewModel);
		}

		public ActionResult GenerateInvoice(long sid)
		{
			try
			{
				viewModel.FeeType = new FeeType() { Id = (int)FeeTypes.SchoolFees };
				viewModel.PaymentMode = new PaymentMode() { Id = 1 };
				viewModel.PaymentType = new PaymentType() { Id = 2 };

				ViewBag.States = viewModel.StateSelectListItem;
				ViewBag.Sessions = viewModel.SessionSelectListItem;
				ViewBag.Programmes = viewModel.ProgrammeSelectListItem;
				ViewBag.Departments = new SelectList(new List<Department>(), Utility.ID, Utility.NAME);
				ViewBag.DepartmentOptions = new SelectList(new List<DepartmentOption>(), Utility.ID, Utility.NAME);
                ViewBag.PaymentModeId = viewModel.PaymentModeSelectList;
                ViewBag.OptionItems = viewModel.OptionSelectList;

                if (sid > 0)
				{
					viewModel.StudentAlreadyExist = true;
					viewModel.Person = personLogic.GetModelBy(p => p.Person_Id == sid);
					viewModel.Student = studentLogic.GetModelBy(s => s.Person_Id == sid);

					viewModel.StudentLevel = studentLevelLogic.GetBy(sid);
					if (viewModel.StudentLevel != null && viewModel.StudentLevel.Programme.Id > 0)
					{
						ViewBag.Programmes = new SelectList(viewModel.ProgrammeSelectListItem, Utility.VALUE, Utility.TEXT, viewModel.StudentLevel.Programme.Id);
					}
					viewModel.LevelSelectListItem = Utility.PopulateLevelSelectListItem();
					//ViewBag.Levels = viewModel.LevelSelectListItem;

					if (viewModel.Person != null && viewModel.Person.Id > 0)
					{
						if (viewModel.Person.State != null && !string.IsNullOrWhiteSpace(viewModel.Person.State.Id))
						{
							ViewBag.States = new SelectList(viewModel.StateSelectListItem, Utility.VALUE, Utility.TEXT, viewModel.Person.State.Id);
						}
					}

					if (viewModel.StudentLevel != null && viewModel.StudentLevel.Id > 0)
					{
						if (viewModel.StudentLevel.Level != null && viewModel.StudentLevel.Level.Id > 0)
						{
							//ViewBag.Levels = new SelectList(viewModel.LevelSelectListItem, Utility.VALUE, Utility.TEXT, viewModel.StudentLevel.Level.Id);
							//Commented because students weren't confirming level before generating invoice
							ViewBag.Levels = GetCorrectLevel(viewModel.LevelSelectListItem, viewModel.StudentLevel.Programme, viewModel.StudentLevel.Student);
						}
					}

					SetDepartmentIfExist(viewModel);
					SetDepartmentOptionIfExist(viewModel);
				}
				else
				{
					ViewBag.Levels = new SelectList(new List<Level>(), Utility.ID, Utility.NAME);
					//ViewBag.Sessions = Utility.PopulateSessionSelectListItem();
				}
			}
			catch (Exception ex)
			{
				SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
			}

			return View(viewModel);
		}


        [HttpPost]
        public ActionResult GenerateInvoice(PaymentViewModel viewModel)
        {
            try
            {

                ModelState.Remove("Student.LastName");
                ModelState.Remove("Student.FirstName");
                ModelState.Remove("Person.DateOfBirth");
                ModelState.Remove("Student.MobilePhone");
                ModelState.Remove("Student.SchoolContactAddress");
                ModelState.Remove("FeeType.Name");
                ModelState.Remove("StudentAlreadyExists");

                //if (ModelState.IsValid)
                //{
                    ViewBag.OptionItems = viewModel.OptionSelectList;
                    var hasGeneratedSchoolFeesInvoice = HasAlreadyGeneratedSchoolFeesInvoice(viewModel.Session, viewModel.Person);
                    if (hasGeneratedSchoolFeesInvoice && viewModel.RegenerateInvoice=="true")
                    {
                        FeeType schoolFees = new FeeType { Id = (int)FeeTypes.SchoolFees };
                        paymentLogic.ClearInvoiceForRegeneration(viewModel.Session, schoolFees, viewModel.Person);
                        hasGeneratedSchoolFeesInvoice = false;
                    }
                    bool isExtraYearStudent = false;
                    if (InvalidDepartmentSelection(viewModel))
                    {
                        KeepInvoiceGenerationDropDownState(viewModel);
                        return View(viewModel);
                    }

                    if (InvalidMatricNumber(viewModel.Student.MatricNumber))
                    {
                        KeepInvoiceGenerationDropDownState(viewModel);
                        return View(viewModel);
                    }

                    FeeDetailLogic feeDetailLogic = new FeeDetailLogic();
                    decimal Amt = 0M;
                    if (viewModel.StudentLevel != null && (viewModel.StudentLevel.Programme.Id == (int)Programmes.NDDistance || viewModel.StudentLevel.Programme.Id == (int)Programmes.HNDDistance))
                    {
                        StudentLevel studentLevel = studentLevelLogic.GetBy(viewModel.Student.MatricNumber);

                        Amt = feeDetailLogic.GetFeeByDepartmentLevel(viewModel.StudentLevel.Department,
                                            studentLevel.Level, viewModel.StudentLevel.Programme, viewModel.FeeType,
                                            viewModel.Session, viewModel.PaymentMode);
                    }
                    else
                    {
                        //Amt = feeDetailLogic.GetFeeByDepartmentLevel(viewModel.StudentLevel.Department,
                        //                    viewModel.StudentLevel.Level, viewModel.StudentLevel.Programme, viewModel.FeeType,
                        //                    viewModel.Session, new PaymentMode() { Id = (int)PaymentModes.Full });
                        var feeDetails = feeDetailLogic.GetFeeDetailByDepartmentLevel(viewModel.StudentLevel.Department,
                                            viewModel.StudentLevel.Level, viewModel.StudentLevel.Programme, viewModel.FeeType,
                                            viewModel.Session, new PaymentMode() { Id = (int)PaymentModes.Full });
                        isExtraYearStudent = Utility.CheckExtraYearStudent(viewModel.Student, viewModel.Session);
                        #region
                        if (!hasGeneratedSchoolFeesInvoice)
                        {
                            if (viewModel.IncludeCISCOFee == "true")
                            {

                                feeDetails = feeDetailLogic.AlterFeeDetailRecords((int)Fees.CISCOCERTFIEDNETWORKASSOCIATED, false, feeDetails);
                            }


                            else
                                feeDetails = feeDetailLogic.AlterFeeDetailRecords((int)Fees.CISCOCERTFIEDNETWORKASSOCIATED, true, feeDetails);

                            if (viewModel.IncludeRoboticFee == "true")

                                feeDetails = feeDetailLogic.AlterFeeDetailRecords((int)Fees.RoboticS_UAE, false, feeDetails);

                            else
                                feeDetails = feeDetailLogic.AlterFeeDetailRecords((int)Fees.RoboticS_UAE, true, feeDetails);
                        }
                       

                        #endregion
                        Amt=feeDetails.Sum(f => f.Fee.Amount);

                    }


                    if (isExtraYearStudent)
                    {
                        return RedirectToAction("Step_1", "ExtraYear", new { Area = "Student", sid = viewModel.Student.Id });
                    }

                    Payment payment = null;
                    FeeinPaymentLogic feeinPaymentLogic = new FeeinPaymentLogic();
                    if (viewModel.StudentAlreadyExist == false)
                    {
                        using (TransactionScope transaction = new TransactionScope())
                        {
                            CreatePerson(viewModel);
                            CreateStudent(viewModel);
                            payment = CreatePayment(viewModel);
                            CreateStudentLevel(viewModel);

                            if (payment != null)
                            {
                                if (viewModel.StudentLevel != null && (viewModel.StudentLevel.Programme.Id != (int)Programmes.NDDistance && viewModel.StudentLevel.Programme.Id != (int)Programmes.HNDDistance) && !hasGeneratedSchoolFeesInvoice)
                                {
                                    if (viewModel.IncludeCISCOFee=="true")
                                    {
                                        feeinPaymentLogic.CreateRecord(payment, (int)Fees.CISCOCERTFIEDNETWORKASSOCIATED, true);
                                    }
                                    else
                                    {
                                        feeinPaymentLogic.CreateRecord(payment, (int)Fees.CISCOCERTFIEDNETWORKASSOCIATED, false);
                                    }
                                    if (viewModel.IncludeRoboticFee == "true")
                                    {
                                        feeinPaymentLogic.CreateRecord(payment, (int)Fees.RoboticS_UAE, true);
                                    }
                                    else
                                    {
                                        feeinPaymentLogic.CreateRecord(payment, (int)Fees.RoboticS_UAE, false);
                                    }

                                }


                                RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                                RemitaPayment remitaPayment = remitaPaymentLogic.GetBy(payment.Id);

                                if (remitaPayment == null)
                                {
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
                                    viewModel.RemitaPayment = remitaProcessor.GenerateRRR(payment.InvoiceNumber, remitaBaseUrl, "SCHOOL FEES", splitItems, settings, Amt);

                                    viewModel.Hash = GenerateHash(settings.Api_key, viewModel.RemitaPayment);

                                    if (viewModel.RemitaPayment != null)
                                    {
                                        transaction.Complete();
                                    }
                                }
                                else
                                {
                                    transaction.Complete();
                                }

                                //Get specific amount;
                                //Amt = payment.FeeDetails.Sum(p => p.Fee.Amount);

                                //Get Payment Specific Setting
                                //RemitaSettings settings = new RemitaSettings();
                                //RemitaSettingsLogic settingsLogic = new RemitaSettingsLogic();
                                //settings = settingsLogic.GetBy(2);

                                ////Get Split Specific details;
                                //List<RemitaSplitItems> splitItems = new List<RemitaSplitItems>();
                                //RemitaSplitItems singleItem = new RemitaSplitItems();
                                //RemitaSplitItemLogic splitItemLogic = new RemitaSplitItemLogic();
                                //singleItem = splitItemLogic.GetBy(3);
                                //singleItem.deductFeeFrom = "1";
                                //singleItem.beneficiaryAmount = "5000";
                                //splitItems.Add(singleItem);
                                //singleItem = splitItemLogic.GetBy(1);
                                //singleItem.deductFeeFrom = "0";
                                //singleItem.beneficiaryAmount = Convert.ToString(Amt - Convert.ToDecimal(splitItems[0].beneficiaryAmount));
                                //splitItems.Add(singleItem);


                                ////Get BaseURL
                                //string remitaBaseUrl = ConfigurationManager.AppSettings["RemitaBaseUrl"].ToString();
                                //RemitaPayementProcessor remitaProcessor = new RemitaPayementProcessor(settings.Api_key);
                                //viewModel.RemitaPayment = remitaProcessor.GenerateRRR(payment.InvoiceNumber, remitaBaseUrl, "SCHOOL FEES", splitItems, settings, Amt);
                                //if (viewModel.RemitaPayment != null)
                                //{
                                //    transaction.Complete();
                                //}
                                ////transaction.Complete();
                            }

                            //transaction.Complete();
                        }
                    }
                    else
                    {
                        using (TransactionScope transaction = new TransactionScope())
                        {
                            StudentLevel studentLevel = studentLevelLogic.GetBy(viewModel.Student.MatricNumber);
                            if (studentLevel != null)
                            {

                                //viewModel.StudentLevel.Id = studentLevel.Id;
                                //viewModel.StudentLevel.Student = studentLevel.Student;
                                //studentLevelLogic.Modify(viewModel.StudentLevel);
                            }
                            personLogic.Modify(viewModel.Person);


                            FeeType feeType = new FeeType() { Id = (int)FeeTypes.SchoolFees };
                            PaymentMode paymentMode = new PaymentMode() { Id = (int)PaymentModes.Full };
                            if (studentLevel != null && (studentLevel.Programme.Id == (int)Programmes.HNDDistance || studentLevel.Programme.Id == (int)Programmes.NDDistance))
                            {
                                paymentMode = viewModel.PaymentMode;
                                bool installmentOrder = paymentLogic.ResolveInstallmentPaymentOrder(feeType, viewModel.Person, viewModel.Session, viewModel.PaymentMode);
                                if (!installmentOrder)
                                {
                                    SetMessage("Error Occurred! " + "If first installment payment has been made please make sure payment for second installment has been made before attemoting to generate invoice for third installment", Message.Category.Error);
                                    KeepInvoiceGenerationDropDownState(viewModel);
                                    return View(viewModel);
                                }

                                payment = paymentLogic.DistantLearningGetBy(feeType, viewModel.Person, viewModel.Session, viewModel.PaymentMode);
                                if(payment != null)
                                {
                                    return RedirectToAction("Invoice", "Credential", new { Area = "Common", pmid = Abundance_Nk.Web.Models.Utility.Encrypt(payment.Id.ToString()), });
                                }
                               


                            }
                            else
                            {
                                payment = paymentLogic.GetBy(feeType, viewModel.Person, viewModel.Session);
                            }

                            if (payment == null || payment.Id <= 0)
                            {
                                payment = CreatePayment(viewModel);
                            }
                            //if (viewModel.StudentLevel != null && (viewModel.StudentLevel.Programme.Id != (int)Programmes.NDDistance && viewModel.StudentLevel.Programme.Id != (int)Programmes.HNDDistance) && feeType?.Id== 3 && !hasGeneratedSchoolFeesInvoice)
                            //{
                            //    if (viewModel.IncludeCISCOFee == "true")
                            //    {
                            //        feeinPaymentLogic.CreateRecord(payment, (int)Fees.CISCOCERTFIEDNETWORKASSOCIATED, true);
                            //    }
                            //    else
                            //    {
                            //        feeinPaymentLogic.CreateRecord(payment, (int)Fees.CISCOCERTFIEDNETWORKASSOCIATED, false);
                            //    }
                            //    if (viewModel.IncludeRoboticFee == "true")
                            //    {
                            //        feeinPaymentLogic.CreateRecord(payment, (int)Fees.RoboticS_UAE, true);
                            //    }
                            //    else
                            //    {
                            //        feeinPaymentLogic.CreateRecord(payment, (int)Fees.RoboticS_UAE, false);
                            //    }

                            //}
                            RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                            RemitaPayment remitaPayment = remitaPaymentLogic.GetBy(payment.Id);
                           
#region
                            if (viewModel.StudentLevel != null && (viewModel.StudentLevel.Programme.Id != (int)Programmes.NDDistance && viewModel.StudentLevel.Programme.Id != (int)Programmes.HNDDistance) && feeType?.Id == 3 && !hasGeneratedSchoolFeesInvoice)
                            {
                                if (viewModel.IncludeCISCOFee == "true")
                                {
                                    feeinPaymentLogic.CreateRecord(payment, (int)Fees.CISCOCERTFIEDNETWORKASSOCIATED, true);
                                }
                                else
                                {
                                    feeinPaymentLogic.CreateRecord(payment, (int)Fees.CISCOCERTFIEDNETWORKASSOCIATED, false);
                                }
                                if (viewModel.IncludeRoboticFee == "true")
                                {
                                    feeinPaymentLogic.CreateRecord(payment, (int)Fees.RoboticS_UAE, true);
                                }
                                else
                                {
                                    feeinPaymentLogic.CreateRecord(payment, (int)Fees.RoboticS_UAE, false);
                                }

                            }
#endregion
                            if (remitaPayment == null)
                            {
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
                                viewModel.RemitaPayment = remitaProcessor.GenerateRRR(payment.InvoiceNumber, remitaBaseUrl, "SCHOOL FEES", splitItems, settings, Amt);
                                viewModel.Hash = GenerateHash(settings.Api_key, viewModel.RemitaPayment);

                                if (viewModel.RemitaPayment != null)
                                {
                                    transaction.Complete();
                                }
                            }
                            else
                            {
                                if (remitaPayment.TransactionAmount != Amt && !remitaPayment.Status.Contains("01"))
                                {
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
                                    viewModel.RemitaPayment = remitaProcessor.GenerateRRR(payment.InvoiceNumber, remitaBaseUrl, "SCHOOL FEES", splitItems, settings, Amt);
                                    viewModel.Hash = GenerateHash(settings.Api_key, viewModel.RemitaPayment);

                                    if (viewModel.RemitaPayment != null)
                                    {
                                        transaction.Complete();
                                    }
                                }
                                else
                                {
                                    //Get Payment Specific Setting
                                    RemitaSettings settings = new RemitaSettings();
                                    RemitaSettingsLogic settingsLogic = new RemitaSettingsLogic();
                                    settings = settingsLogic.GetBy(2);

                                    viewModel.RemitaPayment = remitaPayment;
                                    viewModel.Hash = GenerateHash(settings.Api_key, viewModel.RemitaPayment);

                                    transaction.Complete();
                                }
                            }

                            //RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                            //RemitaPayment remitaPayment = remitaPaymentLogic.GetBy(payment.Id);

                            //if (remitaPayment == null)
                            //{
                            //    //Get Payment Specific Setting
                            //    RemitaSettings settings = new RemitaSettings();
                            //    RemitaSettingsLogic settingsLogic = new RemitaSettingsLogic();
                            //    settings = settingsLogic.GetBy(1);

                            //    //Get Split Specific details;
                            //    List<RemitaSplitItems> splitItems = new List<RemitaSplitItems>();
                            //    RemitaSplitItems singleItem = new RemitaSplitItems();
                            //    RemitaSplitItemLogic splitItemLogic = new RemitaSplitItemLogic();
                            //    singleItem = splitItemLogic.GetBy(3);
                            //    singleItem.deductFeeFrom = "1";
                            //    singleItem.beneficiaryAmount = "5000";
                            //    splitItems.Add(singleItem);
                            //    singleItem = splitItemLogic.GetBy(1);
                            //    singleItem.deductFeeFrom = "0";
                            //    singleItem.beneficiaryAmount = Convert.ToString(Amt - Convert.ToDecimal(splitItems[0].beneficiaryAmount));
                            //    splitItems.Add(singleItem);

                            //    //Get BaseURL
                            //    string remitaBaseUrl = ConfigurationManager.AppSettings["RemitaBaseUrl"].ToString();
                            //    RemitaPayementProcessor remitaProcessor = new RemitaPayementProcessor(settings.Api_key);
                            //    viewModel.RemitaPayment = remitaProcessor.GenerateRRR(payment.InvoiceNumber, remitaBaseUrl, "SCHOOL FEES", splitItems, settings, Amt);
                            //    if (viewModel.RemitaPayment != null)
                            //    {
                            //        transaction.Complete();
                            //    }
                            //}
                            //else
                            //{
                            //    transaction.Complete();
                            //}
                            //transaction.Complete();
                        }
                    }

                    CheckAndUpdateStudentLevel(viewModel);

                    TempData["PaymentViewModel"] = viewModel;
                    return RedirectToAction("Invoice", "Credential", new { Area = "Common", pmid = Abundance_Nk.Web.Models.Utility.Encrypt(payment.Id.ToString()), });
                //}
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            KeepInvoiceGenerationDropDownState(viewModel);
            return View(viewModel);
        }
        //public bool ResolveInstallmentPaymentOrder(FeeType feeType, Person person, Session session, PaymentMode paymentMode)
        //{
        //    try
        //    {
        //        //Expression<Func<PAYMENT, bool>> selector = p => p.Fee_Type_Id == feeType.Id && p.Person_Id == person.Id && p.Session_Id == session.Id && p.Payment_Mode_Id == paymentMode.Id;
        //        RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
        //        PaymentLogic paymentLogic = new PaymentLogic();
        //        Payment payment = paymentLogic.GetModelsBy(p => p.Fee_Type_Id == feeType.Id && p.Person_Id == person.Id && p.Session_Id == session.Id && p.Payment_Mode_Id == paymentMode.Id).FirstOrDefault();
        //        if (paymentMode.Id == (int)PaymentModes.SecondInstallment)
        //        {
        //            paymentMode.Id = (int)PaymentModes.FirstInstallment;

        //            var isFirstInstallmentPaid = remitaPaymentLogic.GetModelsBy(x => x.PAYMENT.Fee_Type_Id == feeType.Id && x.PAYMENT.Person_Id == person.Id && x.PAYMENT.Session_Id == session.Id && x.PAYMENT.Payment_Mode_Id == paymentMode.Id && (x.Description.Contains("01") || x.Description.Contains("manual"))).LastOrDefault();
        //            if (isFirstInstallmentPaid != null)
        //            {
        //                return true;
        //            }
        //            else
        //            {
        //                SetMessage("Error Occurred! " + "Invoice already generated for the specified", Message.Category.Error);

        //            }
        //        }

        //        return false;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
        private List<SelectListItem> GetCorrectLevel(List<SelectListItem> levelList, Programme programme, Model.Model.Student student)
        {
            try
            {
                SessionLogic sessionLogic = new SessionLogic();
                Session session = sessionLogic.GetModelBy(p => p.Active_For_Fees == true);

                string[] sessionItems = session.Name.Split('/');
                string sessionNameStr = sessionItems[0];

                string currentSessionSuffix = sessionNameStr.Substring(2, 2);
                currentSessionSuffix = "/" + currentSessionSuffix + "/";

                int[] hndProgrammes = {(int)Programmes.HNDFullTime, (int)Programmes.HNDPartTime, (int)Programmes.HNDWeekend};
                int[] ndProgrammes = {(int)Programmes.NDFullTime, (int)Programmes.NDPartTime};
                if (ndProgrammes.Contains(programme.Id))
                {
                    if (student.MatricNumber.Contains(currentSessionSuffix))
                    {
                        return levelList.Where(l => string.IsNullOrEmpty(l.Value) || l.Value == Convert.ToString((int)Levels.NDI)).ToList();
                    }
                    else
                    {
                        return levelList.Where(l => string.IsNullOrEmpty(l.Value) || l.Value == Convert.ToString((int)Levels.NDII)).ToList();
                    }
                }
                if (hndProgrammes.Contains(programme.Id))
                {
                    if (student.MatricNumber.Contains(currentSessionSuffix))
                    {
                        return levelList.Where(l => string.IsNullOrEmpty(l.Value) || l.Value == Convert.ToString((int)Levels.HNDI)).ToList();
                    }
                    else
                    {
                        return levelList.Where(l => string.IsNullOrEmpty(l.Value) || l.Value == Convert.ToString((int)Levels.HNDII)).ToList();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return levelList;
        }

		private void SetDepartmentIfExist(PaymentViewModel viewModel)
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

		private void SetDepartmentOptionIfExist(PaymentViewModel viewModel)
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
						ViewBag.DepartmentOptions = new SelectList(new List<DepartmentOption>(), Utility.ID, Utility.NAME);
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

	
        public ActionResult CardPayment()
        {
            PaymentViewModel viewModel = (PaymentViewModel)TempData["PaymentViewModel"];
            viewModel.ResponseUrl = ConfigurationManager.AppSettings["RemitaCardResponseUrl"].ToString();
            TempData.Keep("PaymentViewModel");

            return View(viewModel);
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
		public void CheckAndUpdateStudentLevel(PaymentViewModel viewModel)
		{
			try
			{
				StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
				List<StudentLevel> studentLevelList = studentLevelLogic.GetModelsBy(s => s.Person_Id == viewModel.Person.Id);
				//viewModel.StudentLevel = studentLevelLogic.GetBy(viewModel.Student, viewModel.Session);

				if (studentLevelList.Count != 0 && viewModel.StudentLevel != null)
				{
					StudentLevel currentSessionLevel = studentLevelList.LastOrDefault(s => s.Session.Id == viewModel.Session.Id);
					if (currentSessionLevel != null)
					{
						viewModel.StudentLevel = currentSessionLevel;
					}
					else
					{
						StudentLevel newStudentLevel = studentLevelList.LastOrDefault();
						newStudentLevel.Session = viewModel.Session;
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
							newStudentLevel.Level = viewModel.StudentLevel.Level;
						}

						StudentLevel createdStudentLevel = studentLevelLogic.Create(newStudentLevel);
						viewModel.StudentLevel = studentLevelLogic.GetModelBy(s => s.Student_Level_Id == createdStudentLevel.Id);
					}
				}
			}
			catch (Exception)
			{   
				throw;
			} 
		}
		public ActionResult CarryIndex()
		{
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

		[HttpPost]
		public ActionResult CarryIndex(PaymentViewModel viewModel)
		{
			try
			{

				if (InvalidMatricNumber(viewModel.Student.MatricNumber))
				{
					SetFeeTypeDropDown(viewModel);
					return View(viewModel);
				}

				Model.Model.Student student = studentLogic.GetModelsBy(m => m.Matric_Number == viewModel.Student.MatricNumber).LastOrDefault();             
				if (student != null && student.Id > 0)
				{
					return RedirectToAction("GenerateCarryOverInvoice", "Payment", new { sid = student.Id });
				}
				else
				{
					return RedirectToAction("GenerateCarryOverInvoice", "Payment", new { sid = 0 });
				}
			}
			catch (Exception ex)
			{
				SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
			}

			SetFeeTypeDropDown(viewModel);
			return View(viewModel);
		}

		public ActionResult GenerateCarryOverInvoice()
		{
			try
			{
				viewModel.FeeType = new FeeType() { Id = (int)FeeTypes.CarryOverSchoolFees };
				viewModel.PaymentMode = new PaymentMode() { Id = 1 };
				viewModel.PaymentType = new PaymentType() { Id = 2 };

				ViewBag.States = viewModel.StateSelectListItem;
				ViewBag.Sessions = viewModel.SessionSelectListItem;
				ViewBag.Programmes = viewModel.ProgrammeSelectListItem;
				ViewBag.Departments = new SelectList(new List<Department>(), Utility.ID, Utility.NAME);
				ViewBag.DepartmentOptions = new SelectList(new List<DepartmentOption>(), Utility.ID, Utility.NAME);
				ViewBag.Levels = new SelectList(new List<Level>(), Utility.ID, Utility.NAME);
				
			}
			catch (Exception ex)
			{
				SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
			}

			return View(viewModel);
		}

		[HttpPost]
		public ActionResult GenerateCarryOverInvoice(PaymentViewModel viewModel)
		{
			try
			{
				ModelState.Remove("Student.LastName");
				ModelState.Remove("Student.FirstName");
				ModelState.Remove("Person.DateOfBirth");
				ModelState.Remove("Student.MobilePhone");
				ModelState.Remove("Student.SchoolContactAddress");
				ModelState.Remove("FeeType.Name");

				if (ModelState.IsValid)
				{
					if (InvalidDepartmentSelection(viewModel))
					{
						KeepInvoiceGenerationDropDownState(viewModel);
						return View(viewModel);
					}

					if (InvalidMatricNumber(viewModel.Student.MatricNumber))
					{
						KeepInvoiceGenerationDropDownState(viewModel);
						return View(viewModel);
					}

				  

					Payment payment = null;
					if (viewModel.StudentAlreadyExist == false)
					{
						using (TransactionScope transaction = new TransactionScope())
						{
							CreatePerson(viewModel);
							CreateStudent(viewModel);
							payment = CreatePayment(viewModel);
							CreateStudentLevel(viewModel);

							transaction.Complete();
						}
					}
					else
					{
						personLogic.Modify(viewModel.Person);
						FeeType feeType = new FeeType() { Id = (int)FeeTypes.CarryOverSchoolFees };
						payment = paymentLogic.GetBy(feeType, viewModel.Person, viewModel.Session);
					}


					if (payment == null || payment.Id <= 0)
					{
						payment = CreatePayment(viewModel);
					}

					TempData["PaymentViewModel"] = viewModel;
					return RedirectToAction("Invoice", "Credential", new { Area = "Common", pmid = Abundance_Nk.Web.Models.Utility.Encrypt(payment.Id.ToString()), });
				}
			}
			catch (Exception ex)
			{
				SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
			}

			KeepInvoiceGenerationDropDownState(viewModel);
			return View(viewModel);
		}

		private bool DoesMatricNumberExist(string matricNo)
		{
			try
			{
				Abundance_Nk.Model.Model.Student student = studentLogic.GetModelsBy(m => m.Matric_Number == matricNo).LastOrDefault();
				if (student == null)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			catch (Exception ex)
			{
				
				throw;
			}

		}

		private bool InvalidMatricNumber(string matricNo)
		{
			try
			{
				string baseMatricNo = null;
				string[] matricNoArray = matricNo.Split('/');
				
				if (matricNoArray.Length > 0)
				{
					string[] matricNoArrayCopy = new string[matricNoArray.Length - 1];
					for (int i = 0; i < matricNoArray.Length; i++)
					{
						if (i != matricNoArray.Length - 1)
						{
							matricNoArrayCopy[i] = matricNoArray[i];
						}
					}
					if (matricNoArrayCopy.Length > 0)
					{
						baseMatricNo = string.Join("/", matricNoArrayCopy); 
					}
				}
				else
				{
					SetMessage("Invalid Matric Number entered!", Message.Category.Error);
					return true;
				}

				if (!string.IsNullOrWhiteSpace(baseMatricNo))
				{
					//StudentMatricNumberAssignmentLogic studentMatricNumberAssignmentLogic = new StudentMatricNumberAssignmentLogic();
					//bool isInvalid = studentMatricNumberAssignmentLogic.IsInvalid(baseMatricNo);
					//if (isInvalid)
					//{
					//    SetMessage("Invalid Matric Number entered!", Message.Category.Error);
					//    return true;
					//}
				}
				else
				{
					SetMessage("Invalid Matric Number entered!", Message.Category.Error);
					return true;
				}
								
				return false;
			}
			catch (Exception)
			{
				throw;
			}
		}

		private bool InvalidDepartmentSelection(PaymentViewModel viewModel)
		{
			try
			{
				if (viewModel.StudentLevel.Department == null || viewModel.StudentLevel.Department.Id <= 0)
				{
					SetMessage("Please select Department!", Message.Category.Error);
					return true;
				}
				else if ((viewModel.StudentLevel.DepartmentOption == null && viewModel.StudentLevel.Programme.Id > 2) || (viewModel.StudentLevel.DepartmentOption != null && viewModel.StudentLevel.DepartmentOption.Id <= 0 && viewModel.StudentLevel.Programme.Id > 2))
				{
					viewModel.DepartmentOptionSelectListItem = Utility.PopulateDepartmentOptionSelectListItem(viewModel.StudentLevel.Department, viewModel.StudentLevel.Programme);
					if (viewModel.DepartmentOptionSelectListItem != null && viewModel.DepartmentOptionSelectListItem.Count > 0)
					{
						SetMessage("Please select Department Option!", Message.Category.Error);
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

		private void KeepInvoiceGenerationDropDownState(PaymentViewModel viewModel)
		{
			try
			{
				if (viewModel.Session != null && viewModel.Session.Id > 0)
				{
					ViewBag.Sessions = new SelectList(viewModel.SessionSelectListItem, Utility.VALUE, Utility.TEXT, viewModel.Session.Id);
				}
				else
				{
					ViewBag.Levels = new SelectList(viewModel.LevelSelectListItem, Utility.VALUE, Utility.TEXT);
				}
                if(viewModel.PaymentMode != null && viewModel.PaymentMode.Id > 0)
                {
                    ViewBag.PaymentModeId = new SelectList(viewModel.PaymentModeSelectList, Utility.VALUE, Utility.TEXT, viewModel.PaymentMode.Id);

                }
                else
                {
                    ViewBag.PaymentModeId = new SelectList(viewModel.PaymentModeSelectList, Utility.VALUE, Utility.TEXT);
                }

                if (viewModel.Person.State != null && !string.IsNullOrEmpty(viewModel.Person.State.Id))
				{
					ViewBag.States = new SelectList(viewModel.StateSelectListItem, Utility.VALUE, Utility.TEXT, viewModel.Person.State.Id);
				}
				else
				{
					ViewBag.States = new SelectList(viewModel.StateSelectListItem, Utility.VALUE, Utility.TEXT);
				}

				if (viewModel.StudentLevel.Level != null && viewModel.StudentLevel.Level.Id > 0)
				{
					ViewBag.Levels = new SelectList(viewModel.LevelSelectListItem, Utility.VALUE, Utility.TEXT, viewModel.StudentLevel.Level.Id);
				}
				else
				{
					ViewBag.Levels = new SelectList(viewModel.LevelSelectListItem, Utility.VALUE, Utility.TEXT);
				}

				if (viewModel.StudentLevel.Programme != null && viewModel.StudentLevel.Programme.Id > 0)
				{
					viewModel.DepartmentSelectListItem = Utility.PopulateDepartmentSelectListItem(viewModel.StudentLevel.Programme);
					ViewBag.Programmes = new SelectList(viewModel.ProgrammeSelectListItem, Utility.VALUE, Utility.TEXT, viewModel.StudentLevel.Programme.Id);

					if (viewModel.StudentLevel.Department != null && viewModel.StudentLevel.Department.Id > 0)
					{
						viewModel.DepartmentOptionSelectListItem = Utility.PopulateDepartmentOptionSelectListItem(viewModel.StudentLevel.Department, viewModel.StudentLevel.Programme);
						ViewBag.Departments = new SelectList(viewModel.DepartmentSelectListItem, Utility.VALUE, Utility.TEXT, viewModel.StudentLevel.Department.Id);
						
						if (viewModel.StudentLevel.DepartmentOption != null && viewModel.StudentLevel.DepartmentOption.Id > 0)
						{
							ViewBag.DepartmentOptions = new SelectList(viewModel.DepartmentOptionSelectListItem, Utility.VALUE, Utility.TEXT, viewModel.StudentLevel.DepartmentOption.Id);
						}
						else
						{
							if (viewModel.DepartmentOptionSelectListItem != null && viewModel.DepartmentOptionSelectListItem.Count > 0)
							{
								ViewBag.DepartmentOptions = new SelectList(viewModel.DepartmentOptionSelectListItem, Utility.VALUE, Utility.TEXT);
							}
							else
							{
								ViewBag.DepartmentOptions = new SelectList(new List<DepartmentOption>(), Utility.ID, Utility.NAME);
							}
						}
					}
					else
					{
						ViewBag.Departments = new SelectList(viewModel.DepartmentSelectListItem, Utility.VALUE, Utility.TEXT);
						ViewBag.DepartmentOptions = new SelectList(new List<DepartmentOption>(), Utility.ID, Utility.NAME);
					}
				}
				else
				{
					ViewBag.Programmes = new SelectList(viewModel.ProgrammeSelectListItem, Utility.VALUE, Utility.TEXT);
					ViewBag.Departments = new SelectList(new List<Department>(), Utility.ID, Utility.NAME);
					ViewBag.DepartmentOptions = new SelectList(new List<DepartmentOption>(), Utility.ID, Utility.NAME);
				}
			}
			catch (Exception ex)
			{
				SetMessage("Error Occured " + ex.Message, Message.Category.Error);
			}
		}

		private Person CreatePerson(PaymentViewModel viewModel)
		{
			try
			{
				Role role = new Role() { Id = 5 };
				//PersonType personType = new PersonType() { Id = viewModel.PersonType.Id };
				Nationality nationality = new Nationality() { Id = 1 };

				viewModel.Person.Role = role;
				viewModel.Person.Nationality = nationality;
				viewModel.Person.DateEntered = DateTime.Now;
				//viewModel.Person.PersonType = personType;

				Person person = personLogic.Create(viewModel.Person);
				if (person != null && person.Id > 0)
				{
					viewModel.Person.Id = person.Id;
				}

				return person;
			}
			catch (Exception)
			{
				throw;
			}
		}

		public Model.Model.Student CreateStudent(PaymentViewModel viewModel)
		{
			try
			{
			   
				viewModel.Student.Number = 4;
				viewModel.Student.Category = new StudentCategory() { Id = viewModel.StudentLevel.Level.Id <= 2 ? 1 : 2 };
				viewModel.Student.Id = viewModel.Person.Id;
				
				return studentLogic.Create(viewModel.Student);
			}
			catch (Exception)
			{
				throw;
			}
		}

		private Payment CreatePayment(PaymentViewModel viewModel)
		{
			Payment newPayment = new Payment();
			try
			{
                if (viewModel.PaymentMode?.Id == 0 || viewModel.PaymentMode==null)
                    viewModel.PaymentMode = new PaymentMode { Id = 1 };
				Payment payment = new Payment();
				payment.PaymentMode = viewModel.PaymentMode;
				payment.PaymentType = viewModel.PaymentType;
				payment.PersonType = viewModel.Person.Type;
				payment.FeeType = viewModel.FeeType;
				payment.DatePaid = DateTime.Now;
				payment.Person = viewModel.Person;
				payment.Session = viewModel.Session;

				PaymentMode pyamentMode = new PaymentMode() { Id = 1 };
				OnlinePayment newOnlinePayment = null;
				newPayment = paymentLogic.Create(payment);
				newPayment.FeeDetails = paymentLogic.SetFeeDetails(newPayment, viewModel.StudentLevel.Programme.Id, viewModel.StudentLevel.Level.Id,pyamentMode.Id, viewModel.StudentLevel.Department.Id, viewModel.Session.Id);
				Decimal Amt = newPayment.FeeDetails.Sum(p => p.Fee.Amount);

				if (newPayment != null)
				{
					PaymentChannel channel = new PaymentChannel() { Id = (int)PaymentChannel.Channels.Etranzact };
					OnlinePayment onlinePayment = new OnlinePayment();
					onlinePayment.Channel = channel;
					onlinePayment.Payment = newPayment;
					newOnlinePayment = onlinePaymentLogic.Create(onlinePayment);

				}

					payment = newPayment;
					if (payment != null)
					{
					   // transaction.Complete();
					}
			   
				return newPayment;
			}
			catch (Exception)
			{
				throw;
			}
		}

		private StudentLevel CreateStudentLevel(PaymentViewModel viewModel)
		{
			try
			{
				//StudentLevel studentLevel = new StudentLevel();
				//studentLevel.Department = viewModel.StudentLevel.Department;
				//studentLevel.DepartmentOption = viewModel.StudentLevel.DepartmentOption;
				//studentLevel.Level = viewModel.StudentLevel.Level;
				//studentLevel.Programme = viewModel.Programme;

				viewModel.StudentLevel.Session = viewModel.Session;
				viewModel.StudentLevel.Student = viewModel.Student;
				return studentLevelLogic.Create(viewModel.StudentLevel);


				//StudentLevel studentLevel = new StudentLevel();
				//studentLevel.Department = viewModel.StudentLevel.Department;
				//studentLevel.DepartmentOption = viewModel.StudentLevel.DepartmentOption;
				//studentLevel.Session = viewModel.Session;
				//studentLevel.Level = viewModel.StudentLevel.Level;
				//studentLevel.Programme = viewModel.Programme;
				//studentLevel.Student = viewModel.Student;

				//return studentLevelLogic.Create(studentLevel);
			}
			catch (Exception)
			{
				throw;
			}
		}

		public ActionResult GetDepartmentAndLevelByProgrammeId(string id)
		{
			try
			{
				if (string.IsNullOrEmpty(id))
				{
					return null;
				}

				List<Level> levels = null;
				List<Department> departments = null;
				Programme programme = new Programme() { Id = Convert.ToInt32(id) };
				if (programme.Id > 0)
				{
					DepartmentLogic departmentLogic = new DepartmentLogic();
					departments = departmentLogic.GetBy(programme);

					LevelLogic levelLogic = new LevelLogic();
					if (programme.Id <= 2)
					{
						levels = levelLogic.GetONDs();
					}
					else if (programme.Id > 2)
					{
						levels = levelLogic.GetHNDs();
					}
				}

				//return Json(new SelectList(departments, Utility.ID, Utility.NAME), JsonRequestBehavior.AllowGet);
				//return Json(new { departments = departments, levels = levels }, "text/html", JsonRequestBehavior.AllowGet);
				return Json(new { Departments = departments, Levels = levels }, "json", JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
	   
		public JsonResult GetDepartmentOptionByDepartment(string id, string programmeid)
		{
			try
			{
				if (string.IsNullOrEmpty(id))
				{
					return null;
				}

				Department department = new Department() { Id = Convert.ToInt32(id) };
				Programme programme = new Programme() { Id = Convert.ToInt32(programmeid) };
				DepartmentOptionLogic departmentLogic = new DepartmentOptionLogic();
				List<DepartmentOption> departmentOptions = departmentLogic.GetBy(department, programme);

				return Json(new SelectList(departmentOptions, Utility.ID, Utility.NAME), JsonRequestBehavior.AllowGet);

			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		public ActionResult GenerateShortFallInvoice()
		{
			try
			{
				viewModel = new PaymentViewModel();

				if (System.Web.HttpContext.Current.Session["student"] != null)
				{
					studentLogic = new StudentLogic();
					Model.Model.Student student = System.Web.HttpContext.Current.Session["student"] as Model.Model.Student;
					student = studentLogic.GetBy(student.Id);

					viewModel.Student = student;
				}

				PopulateDropDown(viewModel);
			}
			catch (Exception ex)
			{
				SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
			}

			return View(viewModel);
		}

		[HttpPost]
		public ActionResult GenerateShortFallInvoice(PaymentViewModel viewModel)
		{
			try
			{
				if (viewModel.Student.MatricNumber != null && viewModel.PaymentEtranzact.ConfirmationNo != null && viewModel.FeeType.Id > 0 && viewModel.Session.Id > 0)
				{
					StudentLogic studentLogic = new StudentLogic();
					PaymentLogic paymentLogic = new PaymentLogic();
					PaymentTerminalLogic paymentTerminalLogic = new PaymentTerminalLogic();
					PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
					FeeDetailLogic feeDetailLogic = new FeeDetailLogic();
					StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
					ShortFallLogic shortFallLogic = new ShortFallLogic();
					OnlinePaymentLogic onlinePaymentLogic = new OnlinePaymentLogic();

					decimal invoiceAmount = 0M;
					double shortFallAmount = 0.0;

					Model.Model.Student student = studentLogic.GetModelsBy(s => s.Matric_Number == viewModel.Student.MatricNumber).LastOrDefault();

					if (student != null)
					{
						StudentLevel studentLevel = studentLevelLogic.GetModelsBy(s => s.Person_Id == student.Id && s.Session_Id == viewModel.Session.Id).LastOrDefault();
						//FeeDetail feeDetail = new FeeDetail();
						List<FeeDetail> feeDetail = new List<FeeDetail>();
                        if (viewModel.FeeType.Id == (int)FeeTypes.AcceptanceFee || viewModel.FeeType.Id == (int)FeeTypes.CarryOverSchoolFees || viewModel.FeeType.Id == (int)FeeTypes.SchoolFees)
						{
							//feeDetail = feeDetailLogic.GetModelsBy(f => f.Programme_Id == studentLevel.Programme.Id && f.Department_Id == studentLevel.Department.Id && f.Fee_Type_Id == viewModel.FeeType.Id && f.Level_Id == studentLevel.Level.Id && f.Session_Id == viewModel.Session.Id).LastOrDefault();

                            feeDetail = feeDetailLogic.GetModelsBy(f => f.Programme_Id == studentLevel.Programme.Id && f.Department_Id == studentLevel.Department.Id && f.Fee_Type_Id == viewModel.FeeType.Id && f.Level_Id == studentLevel.Level.Id && f.Session_Id == viewModel.Session.Id);
                        }
						else
						{
							feeDetail = feeDetailLogic.GetModelsBy(f => f.Fee_Type_Id == viewModel.FeeType.Id && f.Session_Id == viewModel.Session.Id);
						}

                        if (viewModel.FeeType.Id == (int)FeeTypes.CarryOverSchoolFees)
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
						
						Payment existingShortFallPayment = paymentLogic.GetModelsBy(p => p.Session_Id == viewModel.Session.Id && p.Fee_Type_Id == (int)FeeTypes.ShortFall && p.Person_Id == student.Id).LastOrDefault();
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
                        RemitaPayment remitaPayment = remitaPaymentLogic.GetModelBy(p => p.RRR == viewModel.PaymentEtranzact.ConfirmationNo);

                        if (remitaPayment != null)
						{
							Payment existingPayment = paymentLogic.GetModelsBy(p => p.Session_Id == viewModel.Session.Id && p.Fee_Type_Id == viewModel.FeeType.Id && p.Person_Id == student.Id).LastOrDefault();
							if (existingPayment != null)
							{
                                if (existingPayment.InvoiceNumber != remitaPayment.payment.InvoiceNumber)
								{
									SetMessage("Confirmation Order Number or RRR not valid for the selected payment type! ", Message.Category.Error);

									PopulateDropDown(viewModel);
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
									payment.Session = viewModel.Session;

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
                                    viewModel.RemitaPayment = remitaProcessor.GenerateRRRCard(createdPayment.InvoiceNumber, remitaBaseUrl, "SCHOOL FEES SHORTFALL", settings, amt);
                                    viewModel.Hash = GenerateHash(settings.Api_key, viewModel.RemitaPayment);

                                    if (viewModel.RemitaPayment != null)
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
						SetMessage("Matric Number not found! ", Message.Category.Error);
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

			PopulateDropDown(viewModel);

			return View(viewModel);
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

		public ActionResult PayShortFallFee()
		{
			try
			{
				viewModel = new PaymentViewModel();
			}
			catch (Exception ex)
			{
				SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
			}

			return View(viewModel);
		}
		[HttpPost]
		public ActionResult PayShortFallFee(PaymentViewModel viewModel)
		{
			try
			{
				if (viewModel.PaymentEtranzact.ConfirmationNo != null)
				{
					PaymentLogic paymentLogic = new PaymentLogic();

					if (viewModel.PaymentEtranzact.ConfirmationNo.Length > 12)
					{
						Model.Model.Session session = new Model.Model.Session() { Id = 7 };
						FeeType feetype = new FeeType() { Id = (int)FeeTypes.ShortFall };
						Payment payment = paymentLogic.InvalidConfirmationOrderNumber(viewModel.PaymentEtranzact.ConfirmationNo, feetype.Id);
						if (payment != null && payment.Id > 0)
						{
							if (payment.FeeType.Id != (int)FeeTypes.ShortFall)
							{
								SetMessage("Confirmation Order Number (" + viewModel.PaymentEtranzact.ConfirmationNo + ") entered is not for shortfall fee payment! Please enter your shortfall fee confirmation order number.", Message.Category.Error);
								return View(viewModel);
							}

							return RedirectToAction("Receipt", "Credential", new { area = "Common", pmid = payment.Id });
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
        public bool HasAlreadyGeneratedSchoolFeesInvoice(Session session, Person person)
        {
            if (session?.Id > 0 && person?.Id>0)
            {
                PaymentLogic paymentLogic = new PaymentLogic();
                var hasSchoolFeesInvoice = paymentLogic.GetModelsBy(f => f.Person_Id == person.Id && f.Fee_Type_Id == (int)FeeTypes.SchoolFees && f.Session_Id==session.Id).FirstOrDefault();
                if (hasSchoolFeesInvoice?.Id > 0)
                    return true;
            }
            return false;
        }
        public bool ClearInvoiceForRegeneration(Session session, FeeType feeType, Person person)
        {
            PaymentLogic paymentLogic = new PaymentLogic();
            var payment=paymentLogic.GetModelsBy(f => f.Session_Id == session.Id && f.Person_Id == person.Id && f.Fee_Type_Id == feeType.Id).FirstOrDefault();
            if (payment?.Id > 0)
            {
                RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                var remitaPayment=remitaPaymentLogic.GetModelBy(f => f.Payment_Id == payment.Id);
                if(remitaPayment!=null && (remitaPayment.Status.Contains("01") || remitaPayment.Status.Contains("00:") || remitaPayment.Status.Contains("998")))
                {
                    return false;
                }
                else if(remitaPayment!=null && (!remitaPayment.Status.Contains("01") && !remitaPayment.Status.Contains("00:") && !remitaPayment.Status.Contains("998")))
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
                    var isSaved=regenerateClearedInvoiceLogic.SaveClearedInvoice(payment, remitaPayment);
                    if (isSaved)
                    {
                        paymentLogic.ClearRRR(payment.Id);
                        return true;
                    }
                        
                    }
                }
            }
            return false;
        }
    }

}