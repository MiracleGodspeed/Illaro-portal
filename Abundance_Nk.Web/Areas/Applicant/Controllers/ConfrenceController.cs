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
using System.Web.Routing;
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

namespace Abundance_Nk.Web.Areas.Applicant.Controllers
{
    [AllowAnonymous]
    public class ConfrenceController : BaseController
    {
        private const string ID = "Id";
        private const string NAME = "Name";
        private const string VALUE = "Value";
        private const string TEXT = "Text";
        private const string DEFAULT_PASSPORT = "/Content/Images/default_avatar.png";
        private const string FIRST_SITTING = "FIRST SITTING";
        private const string SECOND_SITTING = "SECOND SITTING";
        private string appRoot = ConfigurationManager.AppSettings["AppRoot"];

        public ActionResult ConfrenceInvoiceGeneration()
        {
            ConfrencePaymentViewModels confrencePaymentViewModels = new ConfrencePaymentViewModels();

            try
            {
                ViewBag.TitleId = confrencePaymentViewModels.TitleSelectListItem;
                ViewBag.SexId = confrencePaymentViewModels.SexSelectListItem;
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! ", Message.Category.Error);
            }

            TempData["ConfrencePaymentViewModels"] = confrencePaymentViewModels;
            return View(confrencePaymentViewModels);
        }

        [HttpPost]
        public ActionResult ConfrenceInvoiceGeneration(ConfrencePaymentViewModels viewModel)
        {
            try
            {
                PersonLogic personLogic = new PersonLogic();
                PaymentLogic paymentLogic = new PaymentLogic();
                Person checkPersonExist = personLogic.GetModelsBy(p => p.Email == viewModel.Person.Email).FirstOrDefault();
                if (checkPersonExist != null)
                {
                    SetMessage(viewModel.Person.Email + "is Already in our database kindly proceed to Confirm Payment to continue with your application", Message.Category.Error);
                    ViewBag.TitleId = viewModel.TitleSelectListItem;
                    ViewBag.SexId = viewModel.SexSelectListItem;
                    return View(viewModel);
                }
                using (TransactionScope transaction = new TransactionScope())
                {
                    Role role = new Role() { Id = 26 };
                    PersonType personType = new PersonType() { Id = (int)PersonTypes.ConferenceAttendee };
                    Nationality nationality = new Nationality() { Id = 1 };
                    State state = new State() { Id = "OT" };
                    Person person = new Person();
                    person.FirstName = viewModel.Person.FirstName;
                    person.LastName = viewModel.Person.LastName;
                    TitleLogic titleLogic = new TitleLogic();
                    Title title = titleLogic.GetModelBy(t => t.Title_Id == viewModel.Title.Id);
                    person.Title = title.Name;
                    person.Sex = viewModel.Sex;
                    string phone = viewModel.PhoneCode + viewModel.PhoneNO;
                    person.MobilePhone = phone;
                    person.Email = viewModel.Person.Email;


                    person.Nationality = nationality;
                    person.DateEntered = DateTime.Now;
                    person.State = state;
                    person.Role = role;
                    person.Type = personType;
                    PersonLogic personLogicput = new PersonLogic();

                    person = personLogicput.Create(person);

                    Confrence confrence = new Confrence();
                    confrence.Person_Id = person.Id;
                    confrence.Postal_Code = viewModel.InstPostal;
                    confrence.State = viewModel.InstState;
                    confrence.Institution = viewModel.Institution;
                    confrence.Date_Applied = DateTime.Now;
                    confrence.Country = viewModel.InstsCountry;
                    confrence.City = viewModel.InCity;
                    
                    confrence.Department = viewModel.InstDepartment;
                    string conStatus = "";
                    if (viewModel.TouristAttractionFee == true)
                    {
                        conStatus = "ConfrenceAndTourism";
                    }
                    else
                    {
                        conStatus = "ConfrenceOnly";

                    }
                    confrence.Amount_Package = conStatus;
                    string pathForSaving = Server.MapPath("~/Content/ConfrenceUploads");
                    string savedFileName = "";
                    foreach (string file in Request.Files)
                    {
                        HttpPostedFileBase hpf = Request.Files[file] as HttpPostedFileBase;
                        if (hpf.ContentLength == 0)
                            continue;
                        if (this.CreateFolderIfNeeded(pathForSaving))
                        {
                            FileInfo fileInfo = new FileInfo(hpf.FileName);
                            string fileExtension = fileInfo.Extension;
                            if (fileExtension == ".pdf")
                            {
                                string newFile = person.Id + "__Con";
                                var newFileName = newFile + fileExtension;

                                savedFileName = Path.Combine(pathForSaving, newFileName);
                                hpf.SaveAs(savedFileName);

                            }
                            else
                            {
                                ViewBag.TitleId = viewModel.TitleSelectListItem;
                                ViewBag.SexId = viewModel.SexSelectListItem;
                                SetMessage("Error: Sorry only PDF Documents are allowed!", Message.Category.Error);
                                return View(viewModel);
                            }

                        }



                    }

                    confrence.Research_DocumentUrl = person.Id + "__Con.pdf";
                    ConfrenceLogic confrenceLogic = new ConfrenceLogic();
                    confrence = confrenceLogic.Create(confrence);

                    Payment payment = new Payment();

                    FeeTypeLogic feeTypeLogic = new FeeTypeLogic();

                    var feetype = feeTypeLogic.GetModelBy(f => f.Fee_Type_Id == (int)FeeTypes.Conference); // The payment type is coming from payment table
                    var paymenttype = new PaymentType() { Id = (int)Paymenttypes.CardPayment};
                    var paymentMode =new PaymentMode() { Id = (int)PaymentModes.Full };
                    payment.FeeType = feetype;
                    payment.Person = person;

                    SessionLogic sessionLogic = new SessionLogic();
                    var thesession = sessionLogic.GetModelBy(s => s.Activated == true);

                    payment.PaymentMode = paymentMode;
                    payment.DatePaid = DateTime.Now;
                    PersonTypeLogic personTypeLogic = new PersonTypeLogic();
                    var persontype = new PersonType() { Id =(int) PersonTypes.ConferenceAttendee };
                    payment.PersonType = persontype;
                    payment.PaymentType = paymenttype;
                    if (thesession != null)
                    {
                        payment.Session = thesession;
                    }
                    else
                    {
                        ViewBag.TitleId = viewModel.TitleSelectListItem;
                        ViewBag.SexId = viewModel.SexSelectListItem;
                        SetMessage("Error No section seen", Message.Category.Error);
                        return View(viewModel);

                    }
                    string FormatDate = DateTime.Now.ToString();
                    FormatDate = FormatDate.Replace("/", "").Replace("AM", "").Replace("PM", "").Replace("201", "").Replace(" ", "").Replace(":", "");


                    payment.InvoiceNumber = "FPCN" + thesession.Id + "100" + FormatDate + confrence.Confrence_Id;
                    PaymentLogic paymentLogicput = new PaymentLogic();

                    payment = paymentLogic.Create(payment);
                    viewModel.Payment = payment;
                    transaction.Complete();
                    //string PaystackSecrect = ConfigurationManager.AppSettings["PaystackSecrect"].ToString();
                    //string PaystackSubAccount = ConfigurationManager.AppSettings["PaystackSubAccount"].ToString();


                    //if (!String.IsNullOrEmpty(PaystackSecrect))
                    //{
                    //    payment.Person = person;
                    //    PaystackLogic paystackLogic = new PaystackLogic();
                    //    viewModel.Paystack = paystackLogic.GetBy(payment);

                    //    FeeDetailLogic myfeeDetailLogic = new FeeDetailLogic();
                    //    int transactionCharge = 0;
                    //    if (viewModel.TouristAttractionFee == true)
                    //    {
                    //        transactionCharge = 1000;
                    //        var getFeeDetail = myfeeDetailLogic.GetModelsBy(f => f.Fee_Type_Id == (int)FeeTypes.ConferenceWithTourism && f.Session_Id == thesession.Id).ToList(); // getting the ID from the feedetail table
                    //        payment.FeeDetails = getFeeDetail;

                    //    }
                    //    else
                    //    {
                    //        transactionCharge = 1000;
                    //        var getFeeDetail = myfeeDetailLogic.GetModelsBy(f => f.Fee_Type_Id == (int)FeeTypes.Conference && f.Session_Id == thesession.Id).ToList(); // getting the ID from the feedetail table
                    //        payment.FeeDetails = getFeeDetail;

                    //    }

                    //    PaystackRepsonse paystackRepsonse = paystackLogic.MakePayment(payment, PaystackSecrect, PaystackSubAccount, transactionCharge);
                    //    if (paystackRepsonse != null && paystackRepsonse.status && !String.IsNullOrEmpty(paystackRepsonse.data.authorization_url))
                    //    {
                    //        paystackRepsonse.Paystack = new Paystack();
                    //        paystackRepsonse.Paystack.Payment = payment;
                    //        paystackRepsonse.Paystack.authorization_url = paystackRepsonse.data.authorization_url;
                    //        paystackRepsonse.Paystack.access_code = paystackRepsonse.data.access_code;
                    //        viewModel.Paystack = paystackLogic.Create(paystackRepsonse.Paystack);
                    //        transaction.Complete();
                    //        TempData["ConfrencePaymentViewModels"] = viewModel;
                    //        string paystackurl = paystackRepsonse.data.authorization_url.ToString();
                    //        return Redirect(paystackurl);


                    //    }
                    //}


                    //else
                    //{
                    //    ViewBag.TitleId = viewModel.TitleSelectListItem;
                    //    ViewBag.SexId = viewModel.SexSelectListItem;
                    //    SetMessage("Error: Paystack Payment not activated", Message.Category.Error);
                    //    return View(viewModel);
                        
                    //}


                }
                return RedirectToAction("ConfrenceInvoice", new { Inv = viewModel.Payment.InvoiceNumber });

            }
            catch (Exception ex)
            {
                ViewBag.TitleId = viewModel.TitleSelectListItem;
                ViewBag.SexId = viewModel.SexSelectListItem;
                SetMessage("Error:" + ex, Message.Category.Error);


                return View(viewModel);
            }
        }

        public ActionResult ConfrenceInvoice(string Inv)
        {
            ConfrencePaymentViewModels viewModels = new ConfrencePaymentViewModels();
            if (Inv != null)
            {
                PaymentLogic paymentLogic = new PaymentLogic();
                var getPayment = paymentLogic.GetModelBy(p => p.Invoice_Number == Inv);
                if (getPayment != null)
                {
                    PaystackLogic paystackLogic = new PaystackLogic();
                    var checkPaystackSuccess = paystackLogic.GetModelBy(p => p.Payment_Id == getPayment.Id && p.status == "success" && p.amount > 0);
                    if (checkPaystackSuccess == null)
                    {
                        ConfrenceLogic confrenceLogic = new ConfrenceLogic();
                        PersonLogic personLogic = new PersonLogic();
                        var getPerson = personLogic.GetModelBy(p => p.Person_Id == getPayment.Person.Id);
                        if (getPerson != null)
                        {
                            ViewBag.fullName = getPerson.Title + " " + getPerson.FullName;
                            ViewBag.Email = getPerson.Email;
                            ViewBag.Phone = getPerson.MobilePhone;
                            if(getPerson.Sex.Id == 1)
                            {
                                ViewBag.Sex = "Male";
                            }
                            else
                            {
                                ViewBag.Sex = "Female";
                            }
                            var getConfrenceDetail = confrenceLogic.GetModelBy(c => c.Person_Id == getPerson.Id);
                            if (getConfrenceDetail != null)
                            {
                                ViewBag.Department = getConfrenceDetail.Department;
                                ViewBag.Institution = getConfrenceDetail.Institution;
                                ViewBag.City = getConfrenceDetail.City;
                                ViewBag.State = getConfrenceDetail.State;
                                ViewBag.PostCode = getConfrenceDetail.Postal_Code;
                                ViewBag.Country = getConfrenceDetail.Country;
                                ViewBag.ResearchDoc = "/Content/ConfrenceUploads/" + getConfrenceDetail.Research_DocumentUrl;
                                ViewBag.Ivn = Inv;
                                if (getConfrenceDetail.Amount_Package == "ConfrenceOnly")
                                {
                                    ViewBag.Amount = "₦100 For Confrence Registration Fee";

                                }
                                else if (getConfrenceDetail.Amount_Package == "ConfrenceAndTourism")
                                {
                                    ViewBag.Amount = "₦10100 For Confrence Registration Fee and Tourist Attractions";

                                }
                                else
                                {
                                    SetMessage("No Document Uploaded", Message.Category.Error);

                                }
                            }

                        }
                        else
                        {
                            SetMessage("No User Information Detected", Message.Category.Error);

                        }
                    }
                    else
                    {
                        SetMessage("No Payment Recored Associated With This Invoice Number in Confrence Payments", Message.Category.Error);


                    }
                }
                else
                {
                    SetMessage("Error: Invalid Invoice Number", Message.Category.Error);

                }
            }
            else
            {
                SetMessage("No Invoice Number Detected", Message.Category.Error);
            }
            
                return View(viewModels);

            

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
            TempData.Keep();
            return Json(new { isUploaded = isUploaded, message = message, imageUrl = imageUrl }, "text/html", JsonRequestBehavior.AllowGet);
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
                        /*TODO: You must process this exception.*/
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

    }
}