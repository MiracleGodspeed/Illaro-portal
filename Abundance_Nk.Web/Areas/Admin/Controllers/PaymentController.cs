using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Web.Areas.Admin.ViewModels;
using Abundance_Nk.Web.Controllers;
using Abundance_Nk.Business;
using Abundance_Nk.Model.Entity.Model;
using IsolationLevel = System.Data.IsolationLevel;

namespace Abundance_Nk.Web.Areas.Admin.Controllers
{
    public class PaymentController : BaseController
    {
        private PaymentViewModel _viewModel;
        private PaymentLogic _paymentLogic;
        private ApplicationFormLogic _formLogic;
        private ApplicantLogic _applicantLogic;
        public ActionResult UploadPayment()
        {
            try
            {
                _viewModel = new PaymentViewModel();
                PopulateDropdown(_viewModel);
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(_viewModel);
        }
        [HttpPost]
        public ActionResult UploadPayment(PaymentViewModel viewModel)
        {
            try
            {
                List<PaymentModel> paymentModels = new List<PaymentModel>();

                foreach (string file in Request.Files)
                {
                    HttpPostedFileBase hpf = Request.Files[file];
                    string pathForSaving = Server.MapPath("~/Content/ExcelUploads");
                    string savedFileName = Path.Combine(pathForSaving, hpf.FileName);
                    hpf.SaveAs(savedFileName);

                    IExcelManager excelManager = new ExcelManager();
                    DataSet paymentList = excelManager.ReadExcel(savedFileName);

                    if (paymentList != null && paymentList.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < paymentList.Tables[0].Rows.Count; i++)
                        {
                            var paymentModel = new PaymentModel();

                            paymentModel.ApplicationNumber = paymentList.Tables[0].Rows[i][1].ToString();
                            paymentModel.RRR = paymentList.Tables[0].Rows[i][2].ToString();
                            paymentModel.Name = paymentList.Tables[0].Rows[i][3].ToString();

                            paymentModels.Add(paymentModel);
                        }
                    }
                }

                TempData["PaymentModels"] = paymentModels;
                TempData["FeeType"] = viewModel.FeeType;

                viewModel.PaymentModels = paymentModels;
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            PopulateDropdown(viewModel);
            return View(viewModel);
        }
        public ActionResult SamplePaymentUpload()
        {
            try
            {
                GridView gv = new GridView();
                List<SamplePaymentModel> sample = new List<SamplePaymentModel>();
                sample.Add(new SamplePaymentModel()
                {
                    SN = "1",
                    ApplicationNumber = "FPI/XX/20XX/XXXXXXXXX",
                    RRR = "123456789012",
                    Name = "Adekunle Chukwuma Ciroma"
                });

                string filename = "Sample Payment Upload";
                IExcelServiceManager excelServiceManager = new ExcelServiceManager();
                MemoryStream ms = excelServiceManager.DownloadExcel(sample);
                ms.WriteTo(System.Web.HttpContext.Current.Response.OutputStream);
                System.Web.HttpContext.Current.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                System.Web.HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment;filename=" + filename + ".xlsx");
                System.Web.HttpContext.Current.Response.StatusCode = 200;
                System.Web.HttpContext.Current.Response.End();
            }
            catch (Exception ex)
            {
                SetMessage("Error occurred! " + ex.Message, Message.Category.Error);
                return RedirectToAction("UploadPayment");
            }

            return RedirectToAction("UploadPayment");
        }
        /// <summary>
        /// Generates school fees invoice if not exist for the students, generates or modifies Remita school fees paye
        /// </summary>
        /// <returns>ActionResult</returns>
        public ActionResult SavePayments()
        {
            try
            {
                if (TempData["PaymentModels"] != null)
                {
                    List<PaymentModel> paymentModels = (List<PaymentModel>) TempData["PaymentModels"];
                    Model.Model.FeeType feeType = (FeeType) TempData["FeeType"];

                    foreach (var paymentModel in paymentModels)
                    {
                        ApplicationForm form = null;
                        Payment payment = null;
                        _formLogic = new ApplicationFormLogic();

                        decimal Amt = 0M;

                        if(!string.IsNullOrEmpty(paymentModel.ApplicationNumber))
                            form = _formLogic.GetModelsBy(a => a.Application_Form_Number.ToLower() == paymentModel.ApplicationNumber.ToLower().Trim()).LastOrDefault();
                        
                        if (form != null && form.Id > 0)
                        {
                            ApplicantStatus.Status status = ApplicantStatus.Status.GeneratedSchoolFeesInvoice;
                            using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required))
                            {
                                payment = GenerateInvoiceHelper(form, feeType, status);

                                if (payment != null)
                                {
                                    RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                                    RemitaPayment remitaPayment = remitaPaymentLogic.GetBy(payment.Id);

                                    if (remitaPayment == null)
                                    {
                                        AdmissionListLogic admissionListLogic = new AdmissionListLogic();
                                        FeeDetailLogic feeDetailLogic = new FeeDetailLogic();
                                        AdmissionList admissionList = admissionListLogic.GetModelsBy(a => a.Application_Form_Id == form.Id).LastOrDefault();

                                        Level level = form.ProgrammeFee.Programme.Id == (int)Programmes.NDFullTime || form.ProgrammeFee.Programme.Id == (int)Programmes.NDPartTime ? new Level() { Id = (int)Levels.NDI } : new Level() { Id = (int)Levels.HNDI };

                                        Amt = feeDetailLogic.GetFeeByDepartmentLevel(admissionList.Deprtment, level, form.ProgrammeFee.Programme,
                                                                                        feeType, admissionList.Session, new PaymentMode() { Id = (int)PaymentModes.Full });

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
                                        remitaPayment = remitaProcessor.GenerateRRR(payment.InvoiceNumber, remitaBaseUrl, "SCHOOL FEES", splitItems, settings, Amt);
                                        if (remitaPayment != null)
                                        {
                                            UpdateRemitaPayment(payment.InvoiceNumber, paymentModel.RRR);
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

                    SetMessage("Operation successful!", Message.Category.Information);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return RedirectToAction("UploadPayment");
        }

        private void UpdateRemitaPayment(string ivn, string rrr)
        {
            try
            {
                RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                RemitaPayment remitaPayment = remitaPaymentLogic.GetModelsBy(r => r.PAYMENT.Invoice_Number == ivn).LastOrDefault();
                if (remitaPayment != null)
                {
                    remitaPayment.RRR = rrr;
                    remitaPaymentLogic.Modify(remitaPayment);
                    GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();

                    string Action = "MODIFY";
                    string Operation = "Modified Remita Payment for the invoice " + ivn + " And RRR" + rrr;
                    string Table = "Remita Payment Table";
                    generalAuditLogic.CreateGeneralAudit(Action, Operation, Table);

                }
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
                Payment payment = GenerateInvoice(form, status, feeType);
                if (payment == null)
                {
                    SetMessage("Operation Failed! Invoice could not be generated. Refresh browser & try again", Message.Category.Error);
                }

                payment.Person = form.Person;
                return payment;
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
                _paymentLogic = new PaymentLogic();
                _applicantLogic = new ApplicantLogic();

                Payment payment = new Payment();
                payment.PaymentMode = new PaymentMode() { Id = applicationForm.Setting.PaymentMode.Id };
                payment.PaymentType = new PaymentType() { Id = applicationForm.Setting.PaymentType.Id };
                payment.PersonType = new PersonType() { Id = applicationForm.Setting.PersonType.Id };
                payment.Person = applicationForm.Person;
                payment.DatePaid = DateTime.UtcNow;
                payment.FeeType = feeType;
                payment.Session = applicationForm.Setting.Session;

                if (_paymentLogic.PaymentAlreadyMade(payment))
                {
                    return _paymentLogic.GetBy(applicationForm.Person, feeType);
                }
                else
                {
                    Payment newPayment = null;
                    OnlinePayment newOnlinePayment = null;
                    using (TransactionScope transaction = new TransactionScope())
                    {
                        newPayment = _paymentLogic.Create(payment);
                        if (newPayment != null)
                        {
                            if (feeType.Id == (int)FeeTypes.SchoolFees)
                            {
                                AdmissionList list = new AdmissionList();
                                AdmissionListLogic listLogic = new AdmissionListLogic();
                                list = listLogic.GetBy(applicationForm.Id);
                                int LevelId = GetLevel(applicationForm.ProgrammeFee.Programme.Id);
                                newPayment.FeeDetails = _paymentLogic.SetFeeDetails(newPayment, applicationForm.ProgrammeFee.Programme.Id, LevelId, 1, list.Deprtment.Id, applicationForm.Setting.Session.Id);

                            }
                            else if (feeType.Id == 2 && applicationForm.ProgrammeFee.Programme.Id > 1)
                            {
                                feeType.Id = 9;
                                newPayment.FeeDetails = _paymentLogic.SetFeeDetails(feeType);
                            }

                            PaymentChannel channel = new PaymentChannel() { Id = (int)PaymentChannel.Channels.Etranzact };
                            OnlinePaymentLogic onlinePaymentLogic = new OnlinePaymentLogic();
                            OnlinePayment onlinePayment = new OnlinePayment();
                            onlinePayment.Channel = channel;
                            onlinePayment.Payment = newPayment;
                            newOnlinePayment = onlinePaymentLogic.Create(onlinePayment);
                        }

                        _applicantLogic.UpdateStatus(applicationForm, status);
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
                }
            }
            catch (Exception)
            {
                throw;
            }
            return 0;
        }
        private void PopulateDropdown(PaymentViewModel viewModel)
        {
            try
            {
                if (viewModel.Session != null && viewModel.Session.Id > 0)
                    ViewBag.Session = new SelectList(viewModel.SessionSelectList, "Value", "Text", viewModel.Session.Id);
                else
                    ViewBag.Session = viewModel.SessionSelectList;

                if (viewModel.FeeType != null && viewModel.FeeType.Id > 0)
                    ViewBag.FeeType = new SelectList(viewModel.FeeTypeSelectList, "Value", "Text", viewModel.FeeType.Id);
                else
                    ViewBag.FeeType = viewModel.FeeTypeSelectList;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}