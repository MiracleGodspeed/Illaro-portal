using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using Abundance_Nk.Business;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Web.Models;
using Microsoft.Reporting.WebForms;
using ZXing;
using Abundance_Nk.Model.Entity.Model;

namespace Abundance_Nk.Web.Reports.Presenter
{
    public partial class e_Recieptaspx : System.Web.UI.Page
    {
        private int reportType;
        private long paymentId;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                lblMessage.Text = "";
                if (!IsPostBack)
                {
                    if (Request.QueryString["paymentId"] != null)
                    {
                        paymentId = Convert.ToInt64(Utility.Decrypt(Request.QueryString["paymentId"]));
                        BuildReport(paymentId);
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message + ex.InnerException.Message;
            }
        }

        private void BuildReport(long paymentId)
        {
            var paymentLogic = new PaymentLogic();

            try
            {
                List<Receipt> receipts = null;

                receipts = GetReceiptBy(paymentId);

                receipts.FirstOrDefault().barcodeImageUrl = "http://applications.federalpolyilaro.edu.ng/Common/Credential/Receipt?pmid=" + paymentId;

                Payment payment = paymentLogic.GetModelBy(p => p.Payment_Id == paymentId);
                PaymentVerificationLogic paymentVerificationLogic = new PaymentVerificationLogic();
                var verifiedPayment = paymentVerificationLogic.GetModelsBy(p => p.Payment_Id == paymentId).LastOrDefault();

                string bind_dsStudentReport = "dsReceipt";
                string reportPath = "";
                if (receipts.Any())
                {
                    reportPath = @"Reports\VerifiedReceipt.rdlc";

                }
                else
                {
                    reportPath = @"Reports\Receipt.rdlc";

                }

                if (Directory.Exists(Server.MapPath("~/Content/studentReceiptReportFolder")))
                {
                    Directory.Delete(Server.MapPath("~/Content/studentReceiptReportFolder"), true);
                    Directory.CreateDirectory(Server.MapPath("~/Content/studentReceiptReportFolder"));
                }
                else
                {
                    Directory.CreateDirectory(Server.MapPath("~/Content/studentReceiptReportFolder"));
                }

                Warning[] warnings;
                string[] streamIds;
                string mimeType = string.Empty;
                string encoding = string.Empty;
                string extension = string.Empty;

                var rptViewer = new ReportViewer();
                rptViewer.Visible = false;
                rptViewer.Reset();
                rptViewer.LocalReport.DisplayName = "Receipt";
                rptViewer.ProcessingMode = ProcessingMode.Local;
                rptViewer.LocalReport.ReportPath = reportPath;
                rptViewer.LocalReport.EnableExternalImages = true;

                string imageFilePath = GenerateQrCode(receipts.FirstOrDefault().barcodeImageUrl, payment.Id);
                string imagePath = new Uri(Server.MapPath(imageFilePath)).AbsoluteUri;
                ReportParameter QRParameter = new ReportParameter("QRCode", imagePath);

                string reportMark = Server.MapPath("/Content/Images/ilaroredesign.jpg");
                string addressPath = new Uri(reportMark).AbsoluteUri;
                ReportParameter waterMark = new ReportParameter("WaterMark", addressPath);

                ReportParameter[] reportParams = { QRParameter, waterMark };

                rptViewer.LocalReport.SetParameters(reportParams);

                rptViewer.LocalReport.DataSources.Add(new ReportDataSource(bind_dsStudentReport.Trim(), receipts));

                byte[] bytes = rptViewer.LocalReport.Render("PDF", null, out mimeType, out encoding, out extension,
                    out streamIds, out warnings);

                string path = Server.MapPath("~/Content/studentReceiptReportFolder");
                string savelocation = "";
                string fileName = "Receipt" + payment.Id + ".pdf";
                savelocation = Path.Combine(path, "Receipt" + payment.Id + ".pdf");

                File.WriteAllBytes(savelocation, bytes);

                var urlHelp = new UrlHelper(HttpContext.Current.Request.RequestContext);
                Response.Redirect(
                    urlHelp.Action("DownloadReceipt",
                        new
                        {
                            controller = "Credential",
                            area = "Common",
                            path = "~/Content/studentReceiptReportFolder/" + fileName
                        }), false);
                //return File(Server.MapPath(savelocation), "application/zip", reportData.FirstOrDefault().Fullname.Replace(" ", "") + ".zip");
                //Response.Redirect(savelocation, false);
            }
            // }

            catch (Exception ex)
            {
                lblMessage.Text = ex.Message + ex.InnerException.Message;
            }
        }
        public string GenerateQrCode(string url, long paymentId, string alt = "QR code", int height = 100, int width = 100, int margin = 0)
        {
            try
            {
                if (!Directory.Exists(Server.MapPath("~/Content/QRCodes")))
                    Directory.CreateDirectory(Server.MapPath("~/Content/QRCodes"));

                string folderPath = "~/Content/QRCodes/";
                string imagePath = "~/Content/QRCodes/" + paymentId + ".Jpeg";
                string verifyUrl = url;

                string wildCard = paymentId + "*.*";
                IEnumerable<string> files = Directory.EnumerateFiles(Server.MapPath("~/Content/QRCodes/"), wildCard, SearchOption.TopDirectoryOnly);

                if (files != null && files.Count() > 0)
                {
                    return imagePath;
                }
                // If the directory doesn't exist then create it.
                if (!Directory.Exists(Server.MapPath(folderPath)))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var barcodeWriter = new BarcodeWriter();
                barcodeWriter.Format = BarcodeFormat.QR_CODE;
                var result = barcodeWriter.Write(verifyUrl);

                string barcodePath = Server.MapPath(imagePath);
                var barcodeBitmap = new Bitmap(result);
                using (MemoryStream memory = new MemoryStream())
                {
                    using (FileStream fs = new FileStream(barcodePath, FileMode.Create))
                    {
                        barcodeBitmap.Save(memory, ImageFormat.Jpeg);
                        byte[] bytes = memory.ToArray();
                        fs.Write(bytes, 0, bytes.Length);
                    }
                }

                return imagePath;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<Receipt> GetReceiptBy(long pmid)
        {
            List<Receipt> receipts = new List<Receipt>();

            Receipt receipt = null;
            PaymentLogic paymentLogic = new PaymentLogic();
            RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
            PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
            PaymentVerificationLogic paymentVerificationLogic = new PaymentVerificationLogic();
            var verifiedPayment = paymentVerificationLogic.GetModelsBy(p => p.Payment_Id == pmid).LastOrDefault();
            try
            {
                Payment payment = paymentLogic.GetBy(pmid);
                if (payment == null || payment.Id <= 0)
                {
                    return null;
                }
                StudentLogic studentLogic = new StudentLogic();
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                var student = studentLogic.GetModelBy(s => s.Person_Id == payment.Person.Id);
                var studentLevel = studentLevelLogic.GetModelBy(sl => sl.Person_Id == payment.Person.Id && sl.Session_Id == payment.Session.Id);
                if ((studentLevel?.Id <= 0 || studentLevel == null) && student != null)
                {
                    studentLevel = studentLevelLogic.GetBy(student.Id);
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

                    }
                    else
                    {
                        PaymentEtranzact paymentEtranzact = paymentEtranzactLogic.GetModelBy(o => o.Payment_Id == payment.Id);
                        if (paymentEtranzact != null)
                        {
                            payment.DatePaid = paymentEtranzact.TransactionDate != null ? (DateTime)paymentEtranzact.TransactionDate : DateTime.Now;
                            payment.FeeDetails = paymentLogic.SetFeeDetails(payment.FeeType);
                            decimal amountRRR = (decimal)paymentEtranzact.TransactionAmount;

                            receipt = BuildReceipt(payment, remitaPayment.OrderId, remitaPayment, amountRRR, student, studentLevel);


                            // receipt = BuildReceipt(payment,"",paymentEtranzact.ConfirmationNo,amountRRR,student,studentLevel);
                        }
                        else
                            throw new Exception("Payment Not Yet Processed please check back later.");

                    }
                    receipts.Add(receipt);
                    return receipts;
                }

                if (student != null && studentLevel != null && studentLevel.Programme != null && studentLevel.Department != null && studentLevel.Level != null)
                {
                    payment.FeeDetails = paymentLogic.SetFeeDetails(payment, studentLevel.Programme.Id, studentLevel.Level.Id, payment.PaymentMode.Id, studentLevel.Department.Id, payment.Session.Id);

                    
                   
                    if (payment.FeeDetails == null || payment.FeeDetails.Count <= 0 && payment.FeeType.Id != (int)FeeTypes.ShortFall && studentLevel.Department.Id != 44)
                    {
                        throw new Exception("Fee Details for " + payment.FeeType.Name + " not set! please contact your system administrator.");
                    }
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

                    //if (payment.FeeType.Id == (int)FeeTypes.Transcript || payment.FeeType.Id == (int)FeeTypes.CerificateCollection)
                    //{
                    //    //Fee fee = GetPaymentFee(payment);

                    //    payment.FeeDetails = paymentLogic.SetFeeDetails(payment.FeeType);
                    //    if (payment.FeeDetails.Count <= 0)
                    //        payment.FeeDetails = paymentLogic.SetFeeDetails(payment, studentLevel.Programme.Id, studentLevel.Level.Id, payment.PaymentMode.Id, studentLevel.Department.Id, payment.Session.Id);
                    //}
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

                    PaymentEtranzact paymentEtranzact = paymentEtranzactLogic.GetModelBy(o => o.Payment_Id == payment.Id);
                    if (paymentEtranzact != null)
                    {
                        decimal amount = (decimal)paymentEtranzact.TransactionAmount;

                        PaymentScholarship scholarship = new PaymentScholarship();
                        PaymentScholarshipLogic scholarshipLogic = new PaymentScholarshipLogic();
                        if (scholarshipLogic.IsStudentOnScholarship(payment.Person, payment.Session))
                        {
                            scholarship = scholarshipLogic.GetBy(payment.Person);
                            amount = payment.FeeDetails.Sum(p => p.Fee.Amount) - scholarship.Amount;
                        }

                        if (payment.FeeDetails.Sum(f => f.Fee.Amount) != paymentEtranzact.TransactionAmount)
                        {
                            //payment.FeeDetails.ForEach(f =>
                            //{
                            //    f.Fee.Amount = 0M;
                            //});
                            payment.FeeDetails = paymentLogic.FormatReceiptBreakDown(payment.FeeDetails, (decimal)paymentEtranzact.TransactionAmount);
                            if (payment.FeeDetails.Sum(f => f.Fee.Amount) != (decimal)paymentEtranzact.TransactionAmount)
                            {
                                payment.FeeDetails.ForEach(f =>
                                {
                                    f.Fee.Amount = 0M;
                                });
                            }
                        }

                        payment.DatePaid = Convert.ToDateTime(paymentEtranzact.TransactionDate);

                        //receipt = BuildReceipt(payment.Person.FullName, payment.InvoiceNumber, paymentEtranzact, amount, payment.FeeType.Name, payment.Session.Name, student.MatricNumber, studentL.Department.Name, feeDetails);
                        receipt = BuildReceipt(payment, paymentEtranzact.ReceiptNo, paymentEtranzact.ConfirmationNo, amount, student, studentLevel);



                    }
                    else
                    {
                        RemitaPayment remitaPayment = remitaPaymentLogic.GetModelsBy(o => o.Payment_Id == payment.Id).FirstOrDefault();
                        if (remitaPayment != null && (remitaPayment.Status.Contains("01") || remitaPayment.Description.Contains("manual-miracle")))
                        {
                            //if (payment.FeeType.Id == (int)FeeTypes.CerificateCollection || payment.FeeType.Id == (int)FeeTypes.Transcript)
                            //{

                            //    payment.FeeDetails = paymentLogic.SetFeeDetails(payment.FeeType);
                            //    decimal amountRRR = remitaPayment.TransactionAmount;
                            //    receipt = BuildReceipt(payment, remitaPayment.OrderId, remitaPayment, amountRRR, student, studentLevel);

                            //}
                            //else
                            //{

                            payment.DatePaid = remitaPayment.TransactionDate;

                            List<RemitaPayment> remitaShortfallPayment = remitaPaymentLogic.GetModelsBy(o => o.PAYMENT.Person_Id == payment.Person.Id && o.PAYMENT.Session_Id == payment.Session.Id &&
                                                                           o.PAYMENT.Fee_Type_Id == (int)FeeTypes.ShortFall && (o.Status.Contains("01") || o.Status.Contains("00") || o.Status.Contains("manual")));

                            //if (remitaShortfallPayment != null && (remitaShortfallPayment.Status.Contains("01") || remitaPayment.Description.Contains("manual-miracle")) && payment.FeeType.Id == (int)FeeTypes.SchoolFees)

                            if (remitaShortfallPayment.Count() > 1)
                            {
                                for (int i = 0; i < remitaShortfallPayment.Count; i++)
                                {
                                    remitaPayment.TransactionAmount += remitaShortfallPayment[i].TransactionAmount;

                                    //shortFallRRR += remitaShortfallPayment[i].RRR + ", ";
                                }
                            }
                                //remitaPayment.TransactionAmount += remitaShortfallPayment.TransactionAmount;

                            decimal amount = remitaPayment.TransactionAmount;
                            decimal newSum = payment.FeeDetails.Sum(x => x.Fee.Amount);

                            if (studentLevel.Department != null && studentLevel.Department.Id == 44 && !payment.FeeDetails.Any())
                            {
                                decimal setFeeDetailAmount = 0;
                                if (studentLevel.DepartmentOption.Id == (int)DepartmentOptions.GraduateDRVCertificate)
                                {
                                    setFeeDetailAmount = (int)Fees.GraduateDrivingTrainingSchoolFees;
                                }
                                else if (studentLevel.DepartmentOption.Id == (int)DepartmentOptions.ProfessionalDiplomaDrv)
                                {
                                    setFeeDetailAmount = (int)Fees.ProfessionalDiplomaDrivingSchooFees;
                                }
                                else if (studentLevel.DepartmentOption.Id == (int)DepartmentOptions.TechnicalCertDrv)
                                {
                                    setFeeDetailAmount = (int)Fees.TechnicalCertificateDrivingSchooFees;
                                }
                                FeeDetail feeDetail = new FeeDetail()
                                {
                                    Fee = new Fee() { Name = payment.FeeType.Name, Amount = setFeeDetailAmount }
                                };

                                payment.FeeDetails.Add(feeDetail);
                            }

                            if (payment.FeeDetails.Sum(f => f.Fee.Amount) != remitaPayment.TransactionAmount)
                            {
                                //payment.FeeDetails.ForEach(f =>
                                //{
                                //    f.Fee.Amount = 0M;
                                //});
                                payment.FeeDetails = paymentLogic.FormatReceiptBreakDown(payment.FeeDetails, remitaPayment.TransactionAmount);
                                //if (remitaPayment.TransactionAmount > payment.FeeDetails.Sum(f => f.Fee.Amount))
                                //{
                                //    amount = payment.FeeDetails.Sum(f => f.Fee.Amount);
                                //}
                                if (payment.FeeType.Id == (int)FeeTypes.CarryOverSchoolFees)
                                {
                                    payment.FeeDetails.ForEach(f =>
                                    {
                                        f.Fee.Amount = 0M;
                                    });
                                }
                            }

                            PaymentScholarship scholarship = new PaymentScholarship();
                            PaymentScholarshipLogic scholarshipLogic = new PaymentScholarshipLogic();
                            if (scholarshipLogic.IsStudentOnScholarship(payment.Person, payment.Session))
                            {
                                scholarship = scholarshipLogic.GetBy(payment.Person);
                                amount = payment.FeeDetails.Sum(fd => fd.Fee.Amount) - scholarship.Amount;
                            }

                            //receipt = BuildReceipt(payment.Person.FullName, payment.InvoiceNumber, remitaPayment, amount, payment.FeeType.Name, student.MatricNumber, "", payment.Session.Name, studentL.Department.Name, feeDetails);
                            //receipt = BuildReceipt(payment, remitaPayment.OrderId, remitaPayment.RRR, amount, student, studentLevel);
                            if (verifiedPayment != null)
                            {
                                receipt = BuildVerifiedReceipt(payment, verifiedPayment, remitaPayment.OrderId, remitaPayment.RRR, amount, student, studentLevel);

                            }
                            else
                            {
                                receipt = BuildReceipt(payment, remitaPayment.OrderId, remitaPayment.RRR, amount, student, studentLevel);
                            }

                            //receipt = BuildReceipt(payment, remitaPayment.OrderId, remitaPayment, remitaPayment.TransactionAmount, student, studentLevel);



                        }
                    }
                }
                else
                {
                    ApplicationFormLogic applicationFormLogic = new ApplicationFormLogic();
                    AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
                    AdmissionListLogic admissionListLogic = new AdmissionListLogic();

                    var applicant = applicationFormLogic.GetModelBy(ap => ap.Person_Id == payment.Person.Id);
                    var appliedCourse = appliedCourseLogic.GetModelBy(c => c.Person_Id == payment.Person.Id);
                    if (appliedCourse != null && applicant != null)
                    {
                        AdmissionList admissionList = admissionListLogic.GetModelsBy(a => a.APPLICATION_FORM.Person_Id == payment.Person.Id && a.Activated == true).LastOrDefault();
                        if (admissionList != null)
                        {
                            appliedCourse.Department = admissionList.Deprtment;
                            appliedCourse.Option = admissionList.DepartmentOption;
                        }

                        Level level = SetLevel(appliedCourse.Programme);

                        payment.FeeDetails = paymentLogic.SetFeeDetails(payment, appliedCourse.Programme.Id, level.Id, payment.PaymentMode.Id, appliedCourse.Department.Id, payment.Session.Id);

                      

                        if (payment.FeeDetails == null || payment.FeeDetails.Count <= 0 && payment.FeeType.Id != (int)FeeTypes.ShortFall && appliedCourse.Department.Id != 44)
                        {
                            throw new Exception("Fee Details for " + payment.FeeType.Name + " not set! please contact your system administrator.");
                        }
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
                        PaymentEtranzact paymentEtranzact = paymentEtranzactLogic.GetModelBy(o => o.Payment_Id == payment.Id);
                        if (paymentEtranzact != null)
                        {
                            payment.DatePaid = Convert.ToDateTime(paymentEtranzact.TransactionDate);

                            decimal amount = (decimal)paymentEtranzact.TransactionAmount;
                            receipt = BuildReceipt(payment, paymentEtranzact.ReceiptNo, paymentEtranzact.ConfirmationNo, amount, new Student { MatricNumber = applicant.Number },
                                                    new StudentLevel { Programme = appliedCourse.Programme, Department = appliedCourse.Department, Level = level });
                        }
                        else
                        {
                            RemitaPayment remitaPayment = remitaPaymentLogic.GetModelsBy(o => o.Payment_Id == payment.Id).FirstOrDefault();
                            if (remitaPayment != null && remitaPayment.Status.Contains("01"))
                            {
                                payment.DatePaid = remitaPayment.TransactionDate;

                                RemitaPayment remitaShortfallPayment = remitaPaymentLogic.GetModelsBy(o => o.PAYMENT.Person_Id == payment.Person.Id && o.PAYMENT.Session_Id == payment.Session.Id &&
                                                                               o.PAYMENT.Fee_Type_Id == (int)FeeTypes.ShortFall).LastOrDefault();

                                if (remitaShortfallPayment != null && remitaShortfallPayment.Status.Contains("01") && payment.FeeType.Id == (int)FeeTypes.SchoolFees)
                                    remitaPayment.TransactionAmount += remitaShortfallPayment.TransactionAmount;

                                decimal amount = remitaPayment.TransactionAmount;
                                if (appliedCourse.Department != null && appliedCourse.Department.Id == 44 && !payment.FeeDetails.Any())
                                {
                                    decimal setFeeDetailAmount = 0;
                                    if (appliedCourse.Option.Id == (int)DepartmentOptions.GraduateDRVCertificate)
                                    {
                                        setFeeDetailAmount = (int)Fees.GraduateDrivingTrainingSchoolFees;
                                    }
                                    else if (appliedCourse.Option.Id == (int)DepartmentOptions.ProfessionalDiplomaDrv)
                                    {
                                        setFeeDetailAmount = (int)Fees.ProfessionalDiplomaDrivingSchooFees;
                                    }
                                    else if (appliedCourse.Option.Id == (int)DepartmentOptions.TechnicalCertDrv)
                                    {
                                        setFeeDetailAmount = (int)Fees.TechnicalCertificateDrivingSchooFees;
                                    }
                                    FeeDetail feeDetail = new FeeDetail()
                                    {
                                        Fee = new Fee() { Name = payment.FeeType.Name, Amount = setFeeDetailAmount }
                                    };

                                    payment.FeeDetails.Add(feeDetail);
                                }

                                if (payment.FeeDetails.Sum(f => f.Fee.Amount) != remitaPayment.TransactionAmount)
                                {
                                    //payment.FeeDetails.ForEach(f =>
                                    //{
                                    //    f.Fee.Amount = 0M;
                                    //});
                                    payment.FeeDetails = paymentLogic.FormatReceiptBreakDown(payment.FeeDetails, remitaPayment.TransactionAmount);
                                    if (payment.FeeDetails.Sum(f => f.Fee.Amount) != remitaPayment.TransactionAmount)
                                    {
                                        payment.FeeDetails.ForEach(f =>
                                        {
                                            f.Fee.Amount = 0M;
                                        });
                                    }
                                }


                                if (verifiedPayment != null)
                                {
                                    receipt = BuildVerifiedReceipt(payment, verifiedPayment, remitaPayment.OrderId, remitaPayment.RRR, amount, new Student { MatricNumber = applicant.Number }, new StudentLevel { Programme = appliedCourse.Programme, Department = appliedCourse.Department, Level = level });

                                }
                                else
                                {
                                    receipt = BuildReceipt(payment, remitaPayment.OrderId, remitaPayment.RRR, amount, new Student { MatricNumber = applicant.Number },
                                                     new StudentLevel { Programme = appliedCourse.Programme, Department = appliedCourse.Department, Level = level });
                                }

                            }
                        }
                    }

                }
            
                //if(receipt != null && payment.FeeType.Id == (int)FeeTypes.CerificateCollection || payment.FeeType.Id == (int)FeeTypes.Transcript)
                //{
                //    receipts.Add(receipt);
                //    return receipts;
                //}
                if (receipt != null && receipt.FeeDetails != null && receipt.FeeDetails.Count > 0)
                    receipt.FeeDetails.ForEach(p =>
                    {
                        var currentReceipt = new Receipt();
                        currentReceipt.Amount = p.Fee.Amount;
                        currentReceipt.Purpose = p.Fee.Name;
                        currentReceipt.AmountInWords = receipt.AmountInWords;
                        currentReceipt.ApplicationFormNumber = receipt.ApplicationFormNumber;
                        currentReceipt.ConfirmationOrderNumber = receipt.ConfirmationOrderNumber;
                        currentReceipt.Date = receipt.Date;
                        currentReceipt.Department = receipt.Department;
                        currentReceipt.InvoiceNumber = receipt.InvoiceNumber;
                        currentReceipt.MatricNumber = receipt.MatricNumber;
                        currentReceipt.Name = receipt.Name;
                        currentReceipt.Number = receipt.Number;
                        currentReceipt.PaymentId = receipt.PaymentId;
                        currentReceipt.ReceiptNumber = receipt.ReceiptNumber;
                        currentReceipt.Session = receipt.Session;
                        currentReceipt.VerifiedBy = receipt.VerifiedBy;
                        currentReceipt.VerificationDateString = receipt.VerificationDateString;

                        receipts.Add(currentReceipt);
                    });

                return receipts;
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

                    if (tRequest.DestinationCountry != null && tRequest.DestinationCountry.Id == "NIG")
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
                receipt.AmountInWords = amount.ToString();
                receipt.Purpose = payment.FeeType.Name;
                receipt.Date = payment.DatePaid;
                receipt.ApplicationFormNumber = "";
                receipt.MatricNumber = student.MatricNumber;
                receipt.Session = payment.Session.Name;
                receipt.Department = studentLevel.Department.Name;
                receipt.FeeDetails = payment.FeeDetails;

                return receipt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Receipt BuildVerifiedReceipt(Payment payment, PaymentVerification paymentVerification, string Number, string CON, decimal amount, Model.Model.Student student, StudentLevel studentLevel)
        {
            try
            {
                Receipt receipt = new Receipt();
                //receipt.Number = Number;
                receipt.Number = payment.Id.ToString();
                receipt.Name = payment.Person.FullName;
                receipt.ConfirmationOrderNumber = CON;
                receipt.Amount = amount;
                receipt.AmountInWords = amount.ToString();
                receipt.Purpose = payment.FeeType.Name;
                receipt.Date = payment.DatePaid;
                receipt.ApplicationFormNumber = "";
                receipt.MatricNumber = student.MatricNumber;
                receipt.Session = payment.Session.Name;
                receipt.Department = studentLevel.Department.Name;
                receipt.FeeDetails = payment.FeeDetails;
                receipt.VerifiedBy = paymentVerification?.User?.Username;
                receipt.VerificationDateString = paymentVerification.DateVerified.ToLongDateString();
                //receipt.VerificationDate = paymentVerification.DateVerified;
                //receipt.VerificationDateString = paymentVerification.DateVerified.ToLongDateString();

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
                receipt.Department = remitaPayment.Description; //studentLevel.Department.Name;
                //receipt.FeeDetails = payment.FeeDetails;
                receipt.PaymentId = Utility.Encrypt(payment.Id.ToString());

                return receipt;
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
                    case 5:
                        {
                            return level = new Level() { Id = 3 };

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
        public static string NumberToWords(int number)
        {
            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus " + NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + " million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
                var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words;
        }
    }
}