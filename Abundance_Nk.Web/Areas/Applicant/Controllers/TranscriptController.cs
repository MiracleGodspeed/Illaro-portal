using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Business;
using Abundance_Nk.Web.Areas.Applicant.ViewModels;
using Abundance_Nk.Web.Models;
using Abundance_Nk.Web.Controllers;
using System.Configuration;
using System.Net;
using System.Transactions;
using System.Web.Script.Serialization;
using Abundance_Nk.Model.Entity.Model;
using Newtonsoft.Json;
using System.Collections;

namespace Abundance_Nk.Web.Areas.Applicant.Controllers
{
    [AllowAnonymous]
    public class TranscriptController : BaseController
    {
        private const string ID = "Id";
        private const string NAME = "Name";
        TranscriptViewModel viewModel;
        public ActionResult Index(string type)
        {
            try
            {
                viewModel = new TranscriptViewModel();
                ViewBag.StateId = viewModel.StateSelectList;
                ViewBag.CountryId = viewModel.CountrySelectList;

                if (type == "")
                {
                    type = null;
                }

                viewModel.RequestType = type;
                TempData["RequestType"] = type;
                TempData.Keep("RequestType");
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Index(TranscriptViewModel transcriptViewModel)
        {
            try
            {
                if (transcriptViewModel.transcriptRequest.student.MatricNumber != null)
                {
                    TranscriptRequestLogic transcriptRequestLogic = new TranscriptRequestLogic();
                    TranscriptRequest tRequest = transcriptRequestLogic.GetModelsBy(t => t.STUDENT.Matric_Number == transcriptViewModel.transcriptRequest.student.MatricNumber).LastOrDefault();

                    if (tRequest != null)
                    {
                        PersonLogic personLogic = new PersonLogic();
                        Abundance_Nk.Model.Model.Person person = personLogic.GetModelBy(p => p.Person_Id == tRequest.student.Id);
                        tRequest.student.FirstName = person.FirstName;
                        tRequest.student.LastName = person.LastName;
                        tRequest.student.OtherName = person.OtherName;
                        transcriptViewModel.transcriptRequest = tRequest;

                        if (tRequest.payment != null && tRequest.payment.Id > 0)
                        {
                            RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                            tRequest.remitaPayment = remitaPaymentLogic.GetBy(tRequest.payment.Id);
                            if (tRequest.remitaPayment != null && tRequest.remitaPayment.Status.Contains("01"))
                            {

                                GetStudentDetails(transcriptViewModel);
                                transcriptViewModel.transcriptRequest.payment = null;
                            }
                        }
                    }
                    else
                    {
                        GetStudentDetails(transcriptViewModel);
                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
            return View(transcriptViewModel);
        }
        public ActionResult IndexAlt(TranscriptViewModel transcriptViewModel)
        {
            string type = Convert.ToString(TempData["RequestType"]);
            if (type == "")
            {
                type = null;
            }
            TempData.Keep("RequestType");

            try
            {

                if (transcriptViewModel.transcriptRequest.student == null)
                {
                    SetMessage("Enter Your Matriculation Number", Message.Category.Error);
                    return RedirectToAction("Index", new { type = type });
                }

                if (transcriptViewModel.transcriptRequest.student.MatricNumber != null)
                {
                    TranscriptRequestLogic transcriptRequestLogic = new TranscriptRequestLogic();
                    List<TranscriptRequest> tRequests = transcriptRequestLogic.GetModelsBy(t => t.STUDENT.Matric_Number == transcriptViewModel.transcriptRequest.student.MatricNumber && t.Request_Type == type);

                    if (tRequests.Count > 0)
                    {
                        PersonLogic personLogic = new PersonLogic();
                        long sid = tRequests.FirstOrDefault().student.Id;
                        Abundance_Nk.Model.Model.Person person = personLogic.GetModelBy(p => p.Person_Id == sid);

                        for (int i = 0; i < tRequests.Count; i++)
                        {
                            tRequests[i].student.FirstName = person.FirstName;
                            tRequests[i].student.LastName = person.LastName;
                            tRequests[i].student.OtherName = person.OtherName;
                            //transcriptViewModel.transcriptRequest = tRequests[i];

                            if (tRequests[i].payment != null && tRequests[i].payment.Id > 0)
                            {
                                RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                                tRequests[i].remitaPayment = remitaPaymentLogic.GetBy(tRequests[i].payment.Id);
                                if (tRequests[i].remitaPayment != null && tRequests[i].remitaPayment.Status.Contains("01"))
                                {
                                    GetStudentDetails(transcriptViewModel);
                                    //tRequests[i].payment = null;
                                }
                                else
                                {
                                    tRequests[i].payment = null;
                                }
                            }
                        }

                        transcriptViewModel.TranscriptRequests = tRequests;
                        transcriptViewModel.RequestType = type;

                        return View(transcriptViewModel);
                    }
                    else
                    {
                        StudentLogic studentLogic = new StudentLogic();
                        Model.Model.Student student = new Model.Model.Student();
                        student = studentLogic.GetBy(transcriptViewModel.transcriptRequest.student.MatricNumber);

                        if (student != null)
                        {
                            return RedirectToAction("Request", new { sid = student.Id,isNew=true });
                        }
                        else
                        {
                            return RedirectToAction("Request", new { sid = 0, isNew = true });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            transcriptViewModel.RequestType = type;
            return View(transcriptViewModel);
        }
        private static void GetStudentDetails(TranscriptViewModel transcriptViewModel)
        {
            StudentLogic studentLogic = new StudentLogic();
            Model.Model.Student student = new Model.Model.Student();
            string MatricNumber = transcriptViewModel.transcriptRequest.student.MatricNumber;
            student = studentLogic.GetBy(MatricNumber);
            if (student != null)
            {
                PersonLogic personLogic = new PersonLogic();
                Abundance_Nk.Model.Model.Person person = personLogic.GetModelBy(p => p.Person_Id == student.Id);
                student.FirstName = person.FirstName;
                student.LastName = person.LastName;
                student.OtherName = person.OtherName;
                transcriptViewModel.transcriptRequest.student = student;

            }
        }

        public ActionResult Request(long sid,bool isNew)
        {
            string type = Convert.ToString(TempData["RequestType"]);
            if (type == "")
            {
                type = null;
            }

            TempData.Keep("RequestType");

            try
            {
                viewModel = new TranscriptViewModel();
                ViewBag.Response = GetResponseForMail();
                //viewModel.StudentLevel = new StudentLevel();
                TranscriptRequest transcriptRequest = new TranscriptRequest();
                StudentLogic studentLogic = new StudentLogic();
                TranscriptRequestLogic transcriptRequestLogic = new TranscriptRequestLogic();
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                
                if (sid > 0)
                {
                    // transcriptRequest = transcriptRequestLogic.GetBy(sid);

                    transcriptRequest = transcriptRequestLogic.GetModelsBy(tr => tr.Student_id == sid).FirstOrDefault();
                    viewModel.StudentLevel =studentLevelLogic.GetModelsBy(f => f.Person_Id == sid).LastOrDefault();
                    if (transcriptRequest != null && !isNew)
                    {
                        viewModel.transcriptRequest = transcriptRequest;
                    }
                    else
                    {
                        Abundance_Nk.Model.Model.Student student = studentLogic.GetBy(sid);
                        viewModel.transcriptRequest = new TranscriptRequest();
                        viewModel.transcriptRequest.student = student;
                    }
                }

                viewModel.RequestType = type;
                if (type == "Convocation Fee")
                {
                    ViewBag.StateId = new SelectList(viewModel.StateSelectList, "Value", "Text", "OG");
                    ViewBag.CountryId = new SelectList(viewModel.CountrySelectList, "Value", "Text", "NIG");

                    if (sid > 0)
                    {
                        viewModel.transcriptRequest.DestinationAddress = "FEDPOLY ILARO";
                        viewModel.transcriptRequest.DestinationState = new State() { Id = "OG" };
                        viewModel.transcriptRequest.DestinationCountry = new Country() { Id = "NIG" };
                    }
                    else
                    {
                        viewModel.transcriptRequest = new TranscriptRequest();
                        viewModel.transcriptRequest.DestinationAddress = "FEDPOLY ILARO";
                        viewModel.transcriptRequest.DestinationState = new State() { Id = "OG" };
                        viewModel.transcriptRequest.DestinationCountry = new Country() { Id = "NIG" };
                    }
                }
                else
                {
                    ViewBag.StateId = viewModel.StateSelectList;
                    ViewBag.CountryId = viewModel.CountrySelectList;
                    if (sid > 0)
                    {
                        //Do nothing
                    }
                    else
                    {
                        viewModel.transcriptRequest = new TranscriptRequest();
                    }
                }

                ViewBag.DeliveryServices = viewModel.DeliveryServiceSelectList;
                ViewBag.YearOfGraduation = viewModel.GraduationYearSelectList;

                KeepInvoiceGenerationDropDownState(viewModel);
                ViewBag.DeliveryServiceZones = new SelectList(new List<DeliveryServiceZone>(), "Id", "Name");
                if (viewModel.StudentLevel == null)
                {
                    viewModel.StudentLevel = new StudentLevel();
                    viewModel.StudentLevel.Programme = new Programme();
                    viewModel.StudentLevel.Department = new Department();
                    viewModel.StudentLevel.DepartmentOption = new DepartmentOption();
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            viewModel.RequestType = type;
            TempData["TranscriptViewModel"] = viewModel;
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Request(TranscriptViewModel transcriptViewModel)
        {
            string type = Convert.ToString(TempData["RequestType"]);
            if (type == "")
            {
                type = null;
            }
            TempData.Keep("RequestType");
            //if (transcriptViewModel.StudentCopyOnlyRequest == 1)
            //{
            //    TempData["StudentCopyOnly"] = "Yes";
            //}


            try
            {
                viewModel = new TranscriptViewModel();
                ReloadDropdown(transcriptViewModel);
                if (transcriptViewModel.transcriptRequest != null && transcriptViewModel.transcriptRequest.Id <= 0 && transcriptViewModel.transcriptRequest.student != null && transcriptViewModel.transcriptRequest.DestinationCountry != null)
                {
                    if (transcriptViewModel.transcriptRequest.student.Id < 1)
                    {
                        Model.Model.Student student = new Model.Model.Student();
                        Person person = new Person();
                        StudentLevel studentLevel = new StudentLevel();
                        StudentLogic stduentLogic = new StudentLogic();
                        StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                        PersonLogic personLogic = new PersonLogic();

                        student = transcriptViewModel.transcriptRequest.student;

                        Role role = new Role() { Id = 5 };
                        Nationality nationality = new Nationality() { Id = 1 };

                        person.LastName = student.LastName;
                        person.FirstName = student.FirstName;
                        person.OtherName = student.OtherName;
                        person.State = new State() { Id = "OG" };
                        person.Role = role;
                        person.Nationality = nationality;
                        person.DateEntered = DateTime.Now;
                        person.Type = new PersonType() { Id = 3 };
                        person.Email = student.Email;
                        person.MobilePhone = transcriptViewModel.Person.MobilePhone;
                        person.ContactAddress = transcriptViewModel.Person.ContactAddress;
                        person = personLogic.Create(person);
                        if (person != null && person.Id > 0)
                        {
                            string Type = student.MatricNumber.Substring(0, 1);
                            if (Type == "H")
                            {
                                student.Type = new StudentType() { Id = 2 };
                            }
                            else
                            {
                                student.Type = new StudentType() { Id = 1 };
                            }
                            student.Id = person.Id;
                            student.Category = new StudentCategory() { Id = 2 };
                            student.Status = new StudentStatus() { Id = 1 };
                            student = stduentLogic.Create(student);
                             var existStudentLevel = studentLevelLogic.GetModelsBy(g => g.Person_Id == student.Id).LastOrDefault();

                            if (existStudentLevel == null && student!=null)
                            {
                                StudentLevel createStudentLevel = new StudentLevel()
                                {
                                    Programme = new Programme { Id = transcriptViewModel.StudentLevel.Programme.Id },
                                    Department = new Department { Id = transcriptViewModel.StudentLevel.Department.Id },
                                DepartmentOption=new DepartmentOption { Id= transcriptViewModel.StudentLevel.DepartmentOption?.Id>0? transcriptViewModel.StudentLevel.DepartmentOption.Id:0},
                                    Student = student,
                                    Level = new Level { Id = 2 },
                                    Session = new Session { Id = 9 }

                                };
                                studentLevelLogic.Create(createStudentLevel);
                            }
                        }

                    }

                    TranscriptRequestLogic transcriptRequestLogic = new TranscriptRequestLogic();
                    TranscriptRequest transcriptRequest = new TranscriptRequest();
                    transcriptRequest = transcriptViewModel.transcriptRequest;
                    transcriptRequest.DateRequested = DateTime.Now;
                    transcriptRequest.YearOfGraduation = transcriptViewModel.YearOfGraduation;
                    transcriptRequest.WorkPlace = transcriptViewModel.WorkPlace;
                    if(transcriptViewModel.StudentCopyRequestType == 1)
                    {
                        transcriptRequest.RequestStudentCopy = true;
                    }
                    if (transcriptViewModel.StudentCopyOnlyRequest == 1)
                    {
                        transcriptRequest.StudentOnlyCopy = true;
                        transcriptRequest.RequestStudentCopy = true;
                    }
                    else
                    {
                        transcriptRequest.StudentOnlyCopy = false;
                    }
                    //transcriptRequest.DestinationCountry = new Country { Id = "NIG" };
                    if (transcriptRequest.DestinationState.Id == null)
                    {
                        transcriptRequest.DestinationState = new State() { Id = "OT" };
                    }
                    transcriptRequest.transcriptClearanceStatus = new TranscriptClearanceStatus { TranscriptClearanceStatusId = 4 };
                    transcriptRequest.transcriptStatus = new TranscriptStatus { TranscriptStatusId = 1 };
                    transcriptRequest.RequestType = type;
                    if (transcriptRequest.DestinationCountry.Id == "OTH")
                    {
                        transcriptRequest.DestinationState = new State() { Id = "OT" };
                    }

                    if (string.IsNullOrEmpty(transcriptRequest.DestinationCountry.Id))
                    {
                        transcriptRequest.DestinationState.Id = null;
                    }

                    if (transcriptViewModel.DeliveryServiceZone != null)
                    {
                        transcriptRequest.DeliveryServiceZone = transcriptViewModel.DeliveryServiceZone;
                    }

                    transcriptRequest = transcriptRequestLogic.Create(transcriptRequest);

                    return RedirectToAction("TranscriptPayment", new { tid = transcriptRequest.Id });
                }
                else
                {
                    TranscriptRequestLogic transcriptRequestLogic = new TranscriptRequestLogic();

                    if (transcriptViewModel.transcriptRequest.payment == null)
                    {
                        TranscriptRequest transcriptRequest = new TranscriptRequest();
                        transcriptRequest = transcriptViewModel.transcriptRequest;
                        transcriptRequest.DateRequested = DateTime.Now;
                        transcriptRequest.YearOfGraduation = transcriptViewModel.YearOfGraduation;
                        transcriptRequest.WorkPlace = transcriptViewModel.WorkPlace;
                        //transcriptRequest.DestinationCountry = new Country { Id = "NIG" };
                        if (transcriptRequest.DestinationState.Id == null)
                        {
                            transcriptRequest.DestinationState = new State() { Id = "OT" };
                        }
                        transcriptRequest.transcriptClearanceStatus = new TranscriptClearanceStatus { TranscriptClearanceStatusId = 4 };
                        transcriptRequest.transcriptStatus = new TranscriptStatus { TranscriptStatusId = 1 };
                        transcriptRequest.RequestType = type;
                        if (transcriptRequest.DestinationCountry.Id == "OTH")
                        {
                            transcriptRequest.DestinationState = new State() { Id = "OT" };
                        }

                        if (string.IsNullOrEmpty(transcriptRequest.DestinationCountry.Id))
                        {
                            transcriptRequest.DestinationState.Id = null;
                        }

                        if (transcriptViewModel.DeliveryServiceZone != null)
                        {
                            transcriptRequest.DeliveryServiceZone = transcriptViewModel.DeliveryServiceZone;
                        }
                        transcriptRequest = transcriptRequestLogic.Create(transcriptRequest);
                        return RedirectToAction("TranscriptPayment", new { tid = transcriptRequest.Id });

                    }
                    if (transcriptViewModel.transcriptRequest.DestinationCountry.Id == "OTH")
                    {
                        transcriptViewModel.transcriptRequest.DestinationState = new State() { Id = "OT" };
                    }
                    transcriptRequestLogic.Modify(transcriptViewModel.transcriptRequest);
                    return RedirectToAction("TranscriptPayment", new { tid = transcriptViewModel.transcriptRequest.Id });
                }

            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            viewModel.RequestType = type;
            TempData["TranscriptViewModel"] = viewModel;
            return View(viewModel);
        }
       
        public ActionResult TranscriptPayment(long tid)
        {
            viewModel = new TranscriptViewModel();
            string type = Convert.ToString(TempData["RequestType"]);
            if (type == "")
            {
                type = null;
            }
            TempData.Keep("RequestType");

            try
            {
                TranscriptRequestLogic transcriptRequestLogic = new TranscriptRequestLogic();
                TranscriptRequest tRequest = transcriptRequestLogic.GetModelBy(t => t.Transcript_Request_Id == tid);
                PersonLogic personLogic = new PersonLogic();
                Person person = new Person();
                RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                StateGeoZoneLogic stateGeoZoneLogic = new StateGeoZoneLogic();
                StateGeoZone stateGeoZone = new StateGeoZone();
                person = personLogic.GetModelBy(p => p.Person_Id == tRequest.student.Id);
                decimal studentCopyCharge = 0;
                if (person != null)
                {
                    tRequest.student.ImageFileUrl = person.ImageFileUrl;
                    tRequest.student.FirstName = person.FirstName;
                    tRequest.student.LastName = person.LastName;
                    tRequest.student.OtherName = person.OtherName;
                    if(tRequest.RequestStudentCopy == true)
                    {
                        //Check if student copy has earlier been requested
                        var HasStudentCopyBeenRequested = transcriptRequestLogic.GetModelsBy(t => t.Student_id == tRequest.student.Id && t.Request_Student_Copy == true && t.Transcript_Status_Id >= 4);

                        if(HasStudentCopyBeenRequested != null && HasStudentCopyBeenRequested.Count > 0 || tRequest.StudentOnlyCopy == true)
                        {
                            FeeLogic feeLogic = new FeeLogic();
                            var feeStudentCopy = feeLogic.GetModelBy(x => x.Fee_Id == (int)TranscriptFees.StudentCopyTranscript);
                            studentCopyCharge = feeStudentCopy.Amount;
                            viewModel.StudentCopyAmount = studentCopyCharge;
                        }

                        

                    }
                    if (tRequest.payment != null)
                    {
                        tRequest.remitaPayment = remitaPaymentLogic.GetBy(tRequest.payment.Id);
                    }
                    viewModel.transcriptRequest = tRequest;
                    viewModel.RequestType = type;

                    //Get the Amount to be paid
                    Fee fee = null;
                    PaymentLogic paymentLogic = new PaymentLogic();
                    if (tRequest.DestinationCountry != null && tRequest.DestinationCountry.Id == "NIG")
                    {
                        stateGeoZone = stateGeoZoneLogic.GetModelsBy(x => x.State_Id == tRequest.DestinationState.Id && x.Activated).LastOrDefault();
                       
                        if (type == "Certificate Collection")
                        {
                            fee = new Fee() { Id = 131 };

                        }
                        else if (type == "Certificate Verification")
                        {
                            fee = new Fee() { Id = 132 };

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
                            if (stateGeoZone != null && stateGeoZone.GeoZone.Id == (int)GeoZones.SouthWest)
                            {
                                fee = new Fee() { Id = (int)TranscriptFees.SouthWestNIG };
                            }
                            else
                            {
                                fee = new Fee() { Id = (int)TranscriptFees.OthersNIG };
                            }
                            
                        }
                        else
                        {
                            if (stateGeoZone != null && stateGeoZone.GeoZone.Id == (int)GeoZones.SouthWest)
                            {
                                fee = new Fee() { Id = (int)TranscriptFees.SouthWestNIG };
                            }
                            else
                            {
                                fee = new Fee() { Id = (int)TranscriptFees.OthersNIG };
                            }
                        }

                    }
                    else
                    {
                        if (type == "Certificate Collection")
                        {
                            fee = new Fee() { Id = 131 };
                        }
                        else if (type == "Certificate Verification")
                        {
                            fee = new Fee() { Id = 132 };
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
                           
                                fee = ResolveForegnTranscriptFee(tRequest);
                            
                            //else
                            //{
                            //    fee = new Fee() { Id = 47 };

                            //}
                        }
                    }
                    //Check if student copy is requested alone
                    if (tRequest.StudentOnlyCopy == true)
                    {
                        viewModel.TotalAmount = studentCopyCharge;
                        viewModel.Amount = studentCopyCharge;
                    }
                    else
                    {
                        var feeDetail = paymentLogic.SetFeeDetails(fee.Id);
                        if (feeDetail?.Count > 0)
                        {
                            viewModel.Amount = feeDetail.LastOrDefault().Fee.Amount;
                            viewModel.TotalAmount = viewModel.Amount + (tRequest.DeliveryServiceZone != null ? tRequest.DeliveryServiceZone.Fee.Amount : 0) + studentCopyCharge;
                        }
                    }

                    

                }

            }
            catch (Exception ex)
            {

                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
            viewModel.RequestType = type;
            TempData["TranscriptViewModel"] = viewModel;
            return View(viewModel);
        }

        public ActionResult ProcessPayment(long tid)
        {
            string type = Convert.ToString(TempData["RequestType"]);
            if (type == "")
            {
                type = null;
            }
            TempData.Keep("RequestType");

            try
            {
                TranscriptViewModel viewModel = new TranscriptViewModel();
                TranscriptRequestLogic transcriptRequestLogic = new TranscriptRequestLogic();
                TranscriptRequest tRequest = transcriptRequestLogic.GetModelBy(t => t.Transcript_Request_Id == tid);
                StateGeoZoneLogic stateGeoZoneLogic = new StateGeoZoneLogic();
                StateGeoZone stateGeoZone = new StateGeoZone();
                if (tRequest != null)
                {
                    Decimal Amt = 0;
                    Abundance_Nk.Model.Model.Student student = tRequest.student;
                    PersonLogic personLogic = new PersonLogic();
                    Abundance_Nk.Model.Model.Person person = personLogic.GetModelBy(t => t.Person_Id == tRequest.student.Id);
                    Payment payment = new Payment();
                    
                    using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
                    {
                        FeeType feeType = new FeeType();
                        if (type == "Convocation Fee")
                        {
                            feeType = new FeeType() { Id = (int)FeeTypes.ConvocationFee };
                        }
                        else if (type == "Certificate Collection")
                        {
                            feeType = new FeeType() { Id = (int)FeeTypes.CerificateCollection };
                        }
                        else if(type== "Certificate Verification")
                        {
                            feeType = new FeeType() { Id = (int)FeeTypes.CertificateVerification };
                        }
                        else if (type == "Transcript Verification")
                        {
                            feeType = new FeeType() { Id = (int)FeeTypes.Transcript };
                        }
                        else
                        {

                            feeType = new FeeType() { Id = (int)FeeTypes.Transcript };
                        }

                        payment = CreatePayment(student, feeType);

                        if (payment != null)
                        {
                            Fee fee = null;
                            PaymentLogic paymentLogic = new PaymentLogic();
                            //Get Payment Specific Setting
                            RemitaSettings settings = new RemitaSettings();
                            RemitaSettingsLogic settingsLogic = new RemitaSettingsLogic();

                            if (tRequest.DestinationCountry != null && tRequest.DestinationCountry.Id == "NIG")
                            {
                                stateGeoZone = stateGeoZoneLogic.GetModelsBy(x => x.State_Id == tRequest.DestinationState.Id && x.Activated).LastOrDefault();
                                if (type == "Certificate Collection")
                                {
                                    fee = new Fee() { Id = 131 };
                                    settings = settingsLogic.GetBy(8);
                                }
                                else if(type== "Certificate Verification")
                                {
                                    fee = new Fee() { Id = 132 };
                                    settings = settingsLogic.GetBy(8);
                                }
                                else if (type == "Transcript Verification")
                                {
                                    fee = new Fee() { Id = 46, Name = "TRANSCRIPT VERIFICATION(LOCAL)" };
                                    settings = settingsLogic.GetBy(4);
                                }
                                else if (type == "Convocation Fee")
                                {
                                    fee = new Fee() { Id = 60 };
                                    settings = settingsLogic.GetBy(4);
                                }
                                else if (type == null)
                                {
                                    //fee = new Fee() { Id = 46 };

                                    if (stateGeoZone != null && stateGeoZone.GeoZone.Id == (int)GeoZones.SouthWest)
                                    {
                                        fee = new Fee() { Id = (int)TranscriptFees.SouthWestNIG };
                                    }
                                    else
                                    {
                                        fee = new Fee() { Id = (int)TranscriptFees.OthersNIG };
                                    }
                                    settings = settingsLogic.GetBy(4);
                                }
                                else
                                {
                                    //fee = new Fee() { Id = 46 };
                                    if (stateGeoZone != null && stateGeoZone.GeoZone.Id == (int)GeoZones.SouthWest)
                                    {
                                        fee = new Fee() { Id = (int)TranscriptFees.SouthWestNIG };
                                    }
                                    else
                                    {
                                        fee = new Fee() { Id = (int)TranscriptFees.OthersNIG };
                                    }
                                    settings = settingsLogic.GetBy(4);
                                }

                            }
                            else
                            {
                                if (type == "Certificate Collection")
                                {
                                    fee = new Fee() { Id = 131 };
                                    settings = settingsLogic.GetBy(8);
                                }
                                else if (type == "Certificate Verification")
                                {
                                    fee = new Fee() { Id = 132 };
                                    settings = settingsLogic.GetBy(8);
                                }
                                else if (type == "Transcript Verification")
                                {
                                    fee = new Fee() { Id = 47, Name = "TRANSCRIPT VERIFICATION(INTERNATIONAL)" };
                                    settings = settingsLogic.GetBy(5);
                                }
                                else if (type == "Convocation Fee")
                                {
                                    fee = new Fee() { Id = 60 };
                                    settings = settingsLogic.GetBy(4);
                                }
                                else
                                {
                                    //fee = new Fee() { Id = 47 };
                                    fee = ResolveForegnTranscriptFee(tRequest);
                                    settings = settingsLogic.GetBy(5);
                                }
                            }

                            SetSettingsDescription(settings, type);

                            payment.FeeDetails = paymentLogic.SetFeeDetails(fee.Id);
                            //Amt = payment.FeeDetails.Sum(a => a.Fee.Amount);

                            FeeDetail feeDetail = payment.FeeDetails != null && payment.FeeDetails.Count > 0 ? payment.FeeDetails.LastOrDefault() : new FeeDetail();
                            Amt = feeDetail.Fee != null ? feeDetail.Fee.Amount : 0;

                            if (type == null || type.Contains("Transcript") && tRequest.DeliveryServiceZone != null)
                            {
                                //Amt = payment.FeeDetails.Sum(a => a.Fee.Amount) + tRequest.DeliveryServiceZone.Fee.Amount;
                                Amt = Amt + tRequest.DeliveryServiceZone.Fee.Amount;
                            }

                            //Get Split Specific details;
                            List<RemitaSplitItems> splitItems = new List<RemitaSplitItems>();
                            RemitaSplitItems singleItem = new RemitaSplitItems();
                            RemitaSplitItemLogic splitItemLogic = new RemitaSplitItemLogic();
                            if (type == null)
                            {
                                singleItem = splitItemLogic.GetBy(1);
                                singleItem.deductFeeFrom = "1";
                                singleItem.beneficiaryAmount = "2500";
                                splitItems.Add(singleItem);
                                singleItem = splitItemLogic.GetBy(6);
                                singleItem.deductFeeFrom = "0";
                                singleItem.beneficiaryAmount = Convert.ToString(Amt - Convert.ToDecimal(splitItems[0].beneficiaryAmount));
                                splitItems.Add(singleItem);
                            }

                            
                            viewModel.Payment = payment;
                            viewModel.Person = person;
                            viewModel.Amount = Amt;
                            viewModel.RemitaSettings = settings;
                            viewModel.Fee = fee;

                            TempData["TranscriptViewModel"] = viewModel;

                            transaction.Complete();
                        }
                    }

                    tRequest.payment = payment;
                    tRequest.transcriptStatus = new TranscriptStatus { TranscriptStatusId = 3 };
                    transcriptRequestLogic.Modify(tRequest);

                    //return RedirectToAction("TranscriptInvoice", new { controller = "Credential", area = "Common", pmid = payment.Id });
                    return RedirectToAction("ProcessTranscriptRemitaPayment");
                    //move payment to invoiceGeneration
                }

            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
            return RedirectToAction("Index", new { type = type });
        }
        public ActionResult ProcessTranscriptRemitaPayment()
        {
            TranscriptViewModel viewModel = (TranscriptViewModel)TempData["TranscriptViewModel"];
            TempData.Keep("RequestType");
            try
            {
                if (viewModel != null && viewModel.Payment != null && viewModel.Fee != null && viewModel.RemitaSettings != null)
                {
                    Payment payment = viewModel.Payment;
                    if (payment != null)
                    {
                        decimal Amt = 0;
                        if (payment.FeeType.Id == (int)FeeTypes.CerificateCollection)
                        {
                            //Get Student supposed year of graduation and add #300 for each year student delays in collecting certificate
                            StudentLogic studentLogic = new StudentLogic();

                            var getStudent = studentLogic.GetModelBy(p => p.Person_Id == viewModel.Person.Id);
                            if (getStudent != null)
                            {
                                string[] matNumberItems = getStudent.MatricNumber.Split('/');
                                string admittedYear = matNumberItems[2];
                                if (getStudent.MatricNumber == "CA/95/070")
                                    admittedYear = "95";
                                //Trying to cover for people in 1999
                                string getCurrentYear = DateTime.Now.Year.ToString();
                                getCurrentYear = getCurrentYear.Substring(2);

                                int intAdmittedYear = Convert.ToInt32(admittedYear);
                                int expectedYearOfGraduation = 0;
                                if (intAdmittedYear >= 0 && intAdmittedYear <= Convert.ToInt32(getCurrentYear))
                                {
                                    expectedYearOfGraduation = (2000 + intAdmittedYear) + 2;
                                }
                                if (intAdmittedYear >= 79 && intAdmittedYear <= 99)
                                {
                                    expectedYearOfGraduation = (1900 + intAdmittedYear) + 2;
                                }
                                
                                var checkYearDiff = DateTime.Now.Year - expectedYearOfGraduation;
                                decimal lateCollectionCharge = 0;
                                if (checkYearDiff > 0)
                                {
                                    lateCollectionCharge = 300 * checkYearDiff;
                                }
                                
                                Amt = viewModel.Amount + lateCollectionCharge;
                            }
                        }
                        else
                        {
                            //Get Payment Specific Setting
                            Amt = viewModel.Amount;
                        }
                        RemitaSettings settings = new RemitaSettings();
                        settings = viewModel.RemitaSettings;

                        Remita remita = new Remita()
                        {
                            merchantId = settings.MarchantId,
                            serviceTypeId = settings.serviceTypeId,
                            orderId = payment.InvoiceNumber,
                            totalAmount = (decimal)Amt,
                            payerName = viewModel.Person.FullName,
                            payerEmail = !string.IsNullOrEmpty(viewModel.Person.Email) ? viewModel.Person.Email : "test@lloydant.com",
                            payerPhone = viewModel.Person.MobilePhone,
                            //responseurl = "https://applications.federalpolyilaro.edu.ng/Applicant/Transcript/RemitaResponse",
                            //responseurl = "http://localhost:2700/Applicant/Transcript/RemitaResponse",
                            responseurl = ConfigurationManager.AppSettings["RemitaResponseUrl"],
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

            TempData["TranscriptViewModel"] = viewModel;

            return View(viewModel);
        }
        public List<Value> GetResponseForMail()
        {
            List<Value> Values = new List<Value>();

            Values.Add(new Value() { Id = 1, Name = "Yes" });
            Values.Add(new Value() { Id = 2, Name = "No" });

            return Values;
        }
        public ActionResult RemitaResponse(string orderID)
        {
            RemitaResponse remitaResponse = new RemitaResponse();
            TranscriptViewModel viewModel = (TranscriptViewModel)TempData["TranscriptViewModel"];
            if (viewModel == null)
            {
                viewModel = new TranscriptViewModel();
            }

            Payment payment = new Payment();

            //HttpRequest request = new HttpRequest(null, "http://localhost:2700/Applicant/Transcript/RemitaResponse", "orderID");

            try
            {
                string merchant_id = viewModel.RemitaSettings.MarchantId;
                string apiKey = viewModel.RemitaSettings.Api_key;
                string hashed;
                string checkstatusurl = ConfigurationManager.AppSettings["RemitaVerifyUrl"];
                string url;

                if (orderID != null)
                {
                    PaymentLogic paymentLogic = new PaymentLogic();
                    RemitaPaymentLogic remitaLogic = new RemitaPaymentLogic();
                    RemitaPayment remitaPyament = new RemitaPayment();

                    //orderID = request.QueryString["orderID"].ToString();

                    payment = viewModel.Payment ?? paymentLogic.GetBy(orderID);
                    //paymentLogic.SetFeeDetails(payment);

                    remitaPyament = payment != null ? remitaLogic.GetModelBy(p => p.Payment_Id == payment.Id) : null;

                    string hash_string = orderID + apiKey + merchant_id;
                    System.Security.Cryptography.SHA512Managed sha512 = new System.Security.Cryptography.SHA512Managed();
                    Byte[] EncryptedSHA512 = sha512.ComputeHash(System.Text.Encoding.UTF8.GetBytes(hash_string));
                    sha512.Clear();
                    hashed = BitConverter.ToString(EncryptedSHA512).Replace("-", "").ToLower();
                    url = checkstatusurl + "/" + merchant_id + "/" + orderID + "/" + hashed + "/" + "orderstatus.reg";

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
                                remitaPyament.Description = viewModel.RemitaSettings.Description;
                                if (remitaLogic.GetBy(payment.Id) == null)
                                {
                                    remitaLogic.Create(remitaPyament);
                                }
                            }

                            //viewModel.remitaPayment = remitaPyament;
                            viewModel.Payment = payment;
                            viewModel.Person = payment.Person;
                        }
                        else
                        {
                            RemitaPayment remitaPayment = remitaLogic.GetBy(orderID);
                            string remitaVerifyUrl = ConfigurationManager.AppSettings["RemitaVerifyUrl"];
                            RemitaPayementProcessor remitaProcessor = new RemitaPayementProcessor(apiKey);
                            remitaResponse = remitaProcessor.TransactionStatus(remitaVerifyUrl, remitaPayment);
                            if (remitaResponse != null && remitaResponse.Status != null)
                            {
                                remitaPayment.Status = remitaResponse.Status + ":" + remitaResponse.StatusCode;
                                remitaLogic.Modify(remitaPayment);

                                payment = paymentLogic.GetBy(remitaPayment.payment.Id);
                                //viewModel.remitaPayment = remitaPayment;
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

                            //viewModel.remitaPayment = remitaPayment;
                        }

                        //string hash = viewModel.RemitaSettings.MarchantId + remitaResponse.RRR + viewModel.RemitaSettings.Api_key;
                        //RemitaPayementProcessor myRemitaProcessor = new RemitaPayementProcessor(hash);
                        //viewModel.Hash = myRemitaProcessor.HashPaymentDetailToSHA512(hash);
                    }
                    else
                    {
                        SetMessage("Order ID was not generated from this system", Message.Category.Error);
                    }
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

            TempData["TranscriptViewModel"] = viewModel;
            TempData.Keep("RequestType");

            return RedirectToAction("TranscriptInvoice", new { controller = "Credential", area = "Common", pmid = payment.Id });
        }
        private void SetSettingsDescription(RemitaSettings settings, string type)
        {
            try
            {
                if (settings.Payment_SettingId == 4)
                {
                    if (type == null)
                    {
                        settings.Description = "TRANSCRIPT LOCAL";
                    }
                    if (type == "Transcript Verification")
                    {
                        settings.Description = "TRANSCRIPT VERIFICATION LOCAL";
                    }
                    if (type == "Certificate Collection")
                    {
                        settings.Description = "CERTIFICATE LOCAL";
                    }
                    if (type == "Certificate Verification")
                    {
                        settings.Description = "CERTIFICATE VERIFICATION LOCAL";
                    }
                }
                if (settings.Payment_SettingId == 5)
                {
                    if (type == null)
                    {
                        settings.Description = "TRANSCRIPT INTERNATIONAL";
                    }
                    if (type == "Transcript Verification")
                    {
                        settings.Description = "TRANSCRIPT VERIFICATION INTERNATIONAL";
                    }
                    if (type == "Certificate Collection")
                    {
                        settings.Description = "CERTIFICATE INTERNATIONAL";
                    }
                    if (type == "Certificate Verification")
                    {
                        settings.Description = "CERTIFICATE VERIFICATION INTERNATIONAL";
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GenerateHash(string apiKey, RemitaPayment remitaPayment)
        {
            try
            {
                string hash = remitaPayment.MerchantCode + remitaPayment.RRR + apiKey;
                RemitaPayementProcessor remitaProcessor = new RemitaPayementProcessor(hash);
                string hashConcatenate = remitaProcessor.HashPaymentDetailToSHA512(hash);
                return hashConcatenate;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public ActionResult MakePayment()
        {

            return View();
        }
        [HttpPost]
        public ActionResult MakePayment(TranscriptViewModel viewModel)
        {
            try
            {
                Payment payment = new Payment();
                PaymentLogic paymentLogic = new PaymentLogic();
                PaymentEtranzact paymentEtranzact = new PaymentEtranzact();
                PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();

                Model.Model.Session session = new Model.Model.Session() { Id = 1 };
                FeeType feetype = new FeeType() { Id = (int)FeeTypes.Transcript };

                payment = paymentLogic.InvalidConfirmationOrderNumber(viewModel.PaymentEtranzact.ConfirmationNo, session, feetype);
                if (payment != null && payment.Id > 0)
                {
                    if (payment.FeeType.Id != (int)FeeTypes.Transcript)
                    {
                        paymentEtranzact = paymentEtranzactLogic.GetModelBy(p => p.Confirmation_No == viewModel.PaymentEtranzact.ConfirmationNo);
                        if (paymentEtranzact != null)
                        {
                            viewModel.PaymentEtranzact = paymentEtranzact;
                            viewModel.Paymentstatus = true;
                            TranscriptRequestLogic transcriptRequestLogic = new TranscriptRequestLogic();
                            TranscriptRequest transcriptRequest = new TranscriptRequest();
                            payment = paymentLogic.GetModelBy(p => p.Invoice_Number == paymentEtranzact.CustomerID);
                            transcriptRequest = transcriptRequestLogic.GetModelBy(p => p.Payment_Id == payment.Id);

                            //Determine the breakdown of payment




                            return RedirectToAction("TranscriptPayment", new { tid = transcriptRequest.Id });

                        }

                        else
                        {
                            viewModel.Paymentstatus = false;
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

        private Payment CreatePayment(Abundance_Nk.Model.Model.Student student, FeeType feeType)
        {
            try
            {
                Payment payment = new Payment();
                PaymentLogic paymentLogic = new PaymentLogic();
                payment.PaymentMode = new PaymentMode() { Id = 1 };
                payment.PaymentType = new PaymentType() { Id = 2 };
                payment.PersonType = new PersonType() { Id = 4 };
                payment.FeeType = feeType;
                payment.DatePaid = DateTime.Now;
                payment.Person = student;
                payment.Session = new Session { Id = 7 };

                OnlinePayment newOnlinePayment = null;
                Payment newPayment = paymentLogic.Create(payment);
                if (newPayment != null)
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
            catch (Exception)
            {
                throw;
            }
        }

        public JsonResult GetState(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return null;
                }

                var country = id;
                return Json(country, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void ReloadDropdown(TranscriptViewModel transcriptViewModel)
        {
            if (transcriptViewModel.transcriptRequest.DestinationState != null && !string.IsNullOrEmpty(transcriptViewModel.transcriptRequest.DestinationState.Id))
            {
                ViewBag.StateId = new SelectList(viewModel.StateSelectList, Utility.VALUE, Utility.TEXT, transcriptViewModel.transcriptRequest.DestinationState.Id);
                ViewBag.CountryId = new SelectList(viewModel.CountrySelectList, Utility.VALUE, Utility.TEXT, transcriptViewModel.transcriptRequest.DestinationCountry.Id);

            }
            else
            {
                ViewBag.StateId = new SelectList(viewModel.StateSelectList, Utility.VALUE, Utility.TEXT);
                ViewBag.CountryId = new SelectList(viewModel.CountrySelectList, Utility.VALUE, Utility.TEXT, transcriptViewModel.transcriptRequest.DestinationCountry.Id);
            }

            if (viewModel.DeliveryService != null && viewModel.DeliveryService.Id > 0)
            {
                ViewBag.DeliveryServices = new SelectList(viewModel.DeliveryServiceSelectList, Utility.VALUE, Utility.TEXT, transcriptViewModel.DeliveryService.Id);
            }
            else
            {
                ViewBag.DeliveryServices = new SelectList(viewModel.DeliveryServiceSelectList, Utility.VALUE, Utility.TEXT);
            }

            ViewBag.DeliveryServiceZones = new SelectList(new List<DeliveryServiceZone>(), "Id", "Name");
        }

        public ActionResult VerificationFees(string feeTypeId)
        {
            try
            {
                int feeType = Convert.ToInt32(Utility.Decrypt(feeTypeId));
                viewModel = new TranscriptViewModel();
                ViewBag.FeeTypeId = viewModel.FeesTypeSelectList;
                FeeTypeLogic feeTypeLogic = new FeeTypeLogic();
                viewModel.FeeType = feeTypeLogic.GetModelBy(a => a.Fee_Type_Id == feeType);

            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
            return View(viewModel);

        }

        [HttpPost]
        public ActionResult VerificationFees(TranscriptViewModel transcriptViewModel)
        {
            try
            {
                if (transcriptViewModel.StudentVerification.Student.MatricNumber != null)
                {
                    RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                    transcriptViewModel.StudentVerification.RemitaPayment =
                        remitaPaymentLogic.GetModelBy(
                            r => r.PAYMENT.Person_Id == transcriptViewModel.StudentVerification.Student.Id);
                    if (transcriptViewModel.StudentVerification.RemitaPayment != null)
                    {
                        PersonLogic personLogic = new PersonLogic();
                        Abundance_Nk.Model.Model.Person person = personLogic.GetModelBy(p => p.Person_Id == transcriptViewModel.StudentVerification.Student.Id);
                        transcriptViewModel.StudentVerification.Student.FirstName = person.FirstName;
                        transcriptViewModel.StudentVerification.Student.LastName = person.LastName;
                        transcriptViewModel.StudentVerification.Student.OtherName = person.OtherName;
                    }
                    else
                    {
                        StudentLogic studentLogic = new StudentLogic();
                        Model.Model.Student student = new Model.Model.Student();
                        string MatricNumber = transcriptViewModel.StudentVerification.Student.MatricNumber;
                        student = studentLogic.GetBy(MatricNumber);
                        if (student != null)
                        {
                            PersonLogic personLogic = new PersonLogic();
                            Abundance_Nk.Model.Model.Person person = personLogic.GetModelBy(p => p.Person_Id == student.Id);
                            student.FirstName = person.FirstName;
                            student.LastName = person.LastName;
                            student.OtherName = person.OtherName;
                            transcriptViewModel.StudentVerification.Student = student;
                        }

                    }
                    FeeTypeLogic feeTypeLogic = new FeeTypeLogic();
                    transcriptViewModel.StudentVerification.FeeType = feeTypeLogic.GetModelBy(a => a.Fee_Type_Id == transcriptViewModel.FeeType.Id);

                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
            return View(transcriptViewModel);
        }

        public ActionResult VerificationRequest(long sid, string feeTypeId)
        {
            try
            {
                viewModel = new TranscriptViewModel();
                StudentLogic studentLogic = new StudentLogic();
                if (sid > 0)
                {
                    viewModel.StudentVerification.Student = studentLogic.GetBy(sid);
                    if (viewModel.StudentVerification.Student != null)
                    {
                        viewModel.StudentVerification.FeeType = new FeeType();
                        int feeType = Convert.ToInt32(Utility.Decrypt(feeTypeId));
                        viewModel.StudentVerification.FeeType.Id = feeType;
                    }


                }
                int feeType_Id = Convert.ToInt32(Utility.Decrypt(feeTypeId));
                ViewBag.FeeTypeId = new SelectList(viewModel.FeesTypeSelectList, "Value", "Text", feeType_Id);
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
            TempData["TranscriptViewModel"] = viewModel;
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult VerificationRequest(TranscriptViewModel transcriptViewModel)
        {
            try
            {
                viewModel = new TranscriptViewModel();

                if (transcriptViewModel.StudentVerification != null && transcriptViewModel.StudentVerification.RemitaPayment == null)
                {
                    if (transcriptViewModel.StudentVerification.Student.Id < 1)
                    {
                        Model.Model.Student student = new Model.Model.Student();
                        Person person = new Person();
                        StudentLevel studentLevel = new StudentLevel();
                        StudentLogic stduentLogic = new StudentLogic();
                        StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                        PersonLogic personLogic = new PersonLogic();

                        student = transcriptViewModel.StudentVerification.Student;

                        Role role = new Role() { Id = 5 };
                        Nationality nationality = new Nationality() { Id = 1 };

                        person.LastName = student.LastName;
                        person.FirstName = student.FirstName;
                        person.OtherName = student.OtherName;
                        person.Email = student.Email;
                        person.MobilePhone = student.MobilePhone;
                        person.State = new State() { Id = "OG" };
                        person.Role = role;
                        person.Nationality = nationality;
                        person.DateEntered = DateTime.Now;
                        person.Type = new PersonType() { Id = 3 };
                        person = personLogic.Create(person);
                        if (person != null && person.Id > 0)
                        {
                            string StudentType = student.MatricNumber.Substring(0, 1);
                            if (StudentType == "H")
                            {
                                student.Type = new StudentType() { Id = 2 };
                            }
                            else
                            {
                                student.Type = new StudentType() { Id = 1 };
                            }
                            student.Id = person.Id;
                            student.Category = new StudentCategory() { Id = 2 };
                            student.Status = new StudentStatus() { Id = 1 };
                            student = stduentLogic.Create(student);
                            transcriptViewModel.StudentVerification.Student.Id = student.Id;
                            return RedirectToAction("ProcessVerificationPayment", new { sid = student.Id, feetypeId = transcriptViewModel.StudentVerification.FeeType.Id });
                        }

                    }
                    return RedirectToAction("ProcessVerificationPayment", new { sid = transcriptViewModel.StudentVerification.Student.Id, feetypeId = transcriptViewModel.StudentVerification.FeeType.Id });


                }

            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
            TempData["TranscriptViewModel"] = viewModel;
            return RedirectToAction("ProcessVerificationPayment", new { sid = transcriptViewModel.StudentVerification.Student.Id, feetypeId = transcriptViewModel.StudentVerification.FeeType.Id });

        }
        public ActionResult ProcessVerificationPayment(long sid, int feetypeId)
        {
            try
            {
                viewModel = new TranscriptViewModel();
                viewModel.StudentVerification = new StudentVerification();
                StudentLogic studentLogic = new StudentLogic();
                viewModel.StudentVerification.Student = studentLogic.GetBy(sid);
                if (viewModel.StudentVerification.Student != null)
                {
                    Decimal Amt = 0;

                    PersonLogic personLogic = new PersonLogic();
                    Abundance_Nk.Model.Model.Person person = personLogic.GetModelBy(t => t.Person_Id == viewModel.StudentVerification.Student.Id);
                    Payment payment = new Payment();
                    using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
                    {
                        payment = CreatePayment(viewModel.StudentVerification.Student, new FeeType() { Id = feetypeId });
                        if (payment != null)
                        {
                            Fee fee = null;
                            PaymentLogic paymentLogic = new PaymentLogic();

                            //Get Payment Specific Setting
                            RemitaSettings settings = new RemitaSettings();
                            RemitaSettingsLogic settingsLogic = new RemitaSettingsLogic();

                            if (feetypeId == 14)
                            {
                                fee = new Fee() { Id = 47, Name = "CERTIFICATE COLLECTION" };
                                settings = settingsLogic.GetBy(8);
                            }
                            else if (feetypeId == 15)
                            {
                                fee = new Fee() { Id = 49, Name = "STUDENTSHIP VERIFICATION" };
                                settings = settingsLogic.GetBy(7);
                            }
                            else if (feetypeId == 16)
                            {
                                fee = new Fee() { Id = 48, Name = "WES VERIFICATION" };
                                settings = settingsLogic.GetBy(6);
                            }
                            else if (feetypeId == 24)
                            {
                                fee = new Fee() { Id = 132, Name = "LOCAL CERTIFICATE VERIFICATION" };
                                settings = settingsLogic.GetBy(6);
                            }
                            

                            payment.FeeDetails = paymentLogic.SetFeeDetails(fee.Id);

                            //Amt = payment.FeeDetails.Sum(a => a.Fee.Amount);
                            Amt = payment.FeeDetails.Select(a => a.Fee.Amount).FirstOrDefault();

                            if (feetypeId == 16 || feetypeId == 24)
                            {
                                viewModel.Payment = payment;
                                viewModel.Person = person;
                                viewModel.Amount = Amt;
                                viewModel.RemitaSettings = settings;
                                viewModel.Fee = fee;

                                transaction.Complete();

                                TempData["TranscriptViewModel"] = viewModel;
                                if (feetypeId == 16)
                                {
                                    TempData["RequestType"] = "Wes";
                                }
                                else
                                {
                                    TempData["RequestType"] = "Local Verification";
                                }

                               
                                return RedirectToAction("ProcessTranscriptRemitaPayment");
                            }
                            else
                            {
                                if (viewModel.StudentVerification.Student.Email == null)
                                {
                                    viewModel.StudentVerification.Student.Email = viewModel.StudentVerification.Student.FullName + "@gmail.com";
                                }
                                Remita remitaObj = new Remita()
                                {
                                    merchantId = settings.MarchantId,
                                    serviceTypeId = settings.serviceTypeId,
                                    orderId = payment.InvoiceNumber,
                                    payerEmail = viewModel.StudentVerification.Student.Email,
                                    payerName = viewModel.StudentVerification.Student.FullName,
                                    payerPhone = viewModel.StudentVerification.Student.MobilePhone,
                                    paymenttype = fee.Name,
                                    responseurl = settings.Response_Url,
                                    totalAmount = viewModel.StudentVerification.Amount

                                };
                                string toHash = remitaObj.merchantId + remitaObj.serviceTypeId + remitaObj.orderId + remitaObj.totalAmount + remitaObj.responseurl + settings.Api_key;

                                RemitaPayementProcessor remitaPayementProcessor = new RemitaPayementProcessor(settings.Api_key);
                                remitaObj.hash = remitaPayementProcessor.HashPaymentDetailToSHA512(toHash);
                                viewModel.StudentVerification.Remita = remitaObj;
                                RemitaPayment remitaPayment = new RemitaPayment() { payment = payment, OrderId = payment.InvoiceNumber, RRR = payment.InvoiceNumber, Status = "025", Description = fee.Name, TransactionDate = DateTime.Now, TransactionAmount = viewModel.StudentVerification.Amount };
                                RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                                remitaPaymentLogic.Create(remitaPayment);
                            }

                        }

                        transaction.Complete();
                    }

                    viewModel.StudentVerification.Payment = payment;

                    return View(viewModel);
                }

            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
            return RedirectToAction("VerificationFees");
        }

        //public ActionResult ProcessVerificationPayment(long sid, int feetypeId)
        //{
        //    try
        //    {
        //        viewModel = new TranscriptViewModel();
        //        viewModel.StudentVerification = new StudentVerification();
        //        StudentLogic studentLogic = new StudentLogic();
        //        viewModel.StudentVerification.Student = studentLogic.GetBy(sid);
        //        if (viewModel.StudentVerification.Student != null)
        //        {
        //            PersonLogic personLogic = new PersonLogic();
        //            Abundance_Nk.Model.Model.Person person = personLogic.GetModelBy(t => t.Person_Id == viewModel.StudentVerification.Student.Id);
        //            Payment payment = new Payment();
        //            using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
        //            {
        //                payment = CreatePayment(viewModel.StudentVerification.Student, new FeeType() { Id = (int)FeeTypes.Transcript });
        //                if (payment != null)
        //                {
        //                    Fee fee = null;
        //                    PaymentLogic paymentLogic = new PaymentLogic();

        //                    //Get Payment Specific Setting
        //                    RemitaSettings settings = new RemitaSettings();
        //                    RemitaSettingsLogic settingsLogic = new RemitaSettingsLogic();

        //                    if (feetypeId == 14)
        //                    {
        //                        fee = new Fee() { Id = 47, Name = "CERTIFICATE COLLECTION" };
        //                        settings = settingsLogic.GetBy(8);
        //                    }
        //                    else if (feetypeId == 15)
        //                    {
        //                        fee = new Fee() { Id = 49, Name = "STUDENTSHIP VERIFICATION" };
        //                        settings = settingsLogic.GetBy(7);
        //                    }
        //                    else if (feetypeId == 16)
        //                    {
        //                        fee = new Fee() { Id = 48, Name = "WES VERIFICATION" };
        //                        settings = settingsLogic.GetBy(6);
        //                    }
        //                    payment.FeeDetails = paymentLogic.SetFeeDetails(fee.Id);
        //                    viewModel.StudentVerification.Amount = payment.FeeDetails.Sum(a => a.Fee.Amount);
        //                    if (viewModel.StudentVerification.Student.Email == null)
        //                    {
        //                        viewModel.StudentVerification.Student.Email = viewModel.StudentVerification.Student.FullName + "@gmail.com";
        //                    }
        //                    Remita remitaObj = new Remita()
        //                    {
        //                        merchantId = settings.MarchantId,
        //                        serviceTypeId = settings.serviceTypeId,
        //                        orderId = payment.InvoiceNumber,
        //                        payerEmail = viewModel.StudentVerification.Student.Email,
        //                        payerName = viewModel.StudentVerification.Student.FullName,
        //                        payerPhone = viewModel.StudentVerification.Student.MobilePhone,
        //                        paymenttype = fee.Name,
        //                        responseurl = settings.Response_Url,
        //                        totalAmount = viewModel.StudentVerification.Amount

        //                    };
        //                    string toHash = remitaObj.merchantId + remitaObj.serviceTypeId + remitaObj.orderId + remitaObj.totalAmount + remitaObj.responseurl + settings.Api_key;

        //                    RemitaPayementProcessor remitaPayementProcessor = new RemitaPayementProcessor(settings.Api_key);
        //                    remitaObj.hash = remitaPayementProcessor.HashPaymentDetailToSHA512(toHash);
        //                    viewModel.StudentVerification.Remita = remitaObj;
        //                    RemitaPayment remitaPayment = new RemitaPayment() { payment = payment, OrderId = payment.InvoiceNumber, RRR = payment.InvoiceNumber, Status = "025", Description = fee.Name, TransactionDate = DateTime.Now, TransactionAmount = viewModel.StudentVerification.Amount };
        //                    RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
        //                    remitaPaymentLogic.Create(remitaPayment);
        //                }

        //                transaction.Complete();
        //            }
        //            viewModel.StudentVerification.Payment = payment;

        //            return View(viewModel);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
        //    }
        //    return RedirectToAction("VerificationFees");
        //}
        public ActionResult ProcessCertificatePayment(int feeId, long personId, long transcriptId)
        {
            try
            {
                viewModel = new TranscriptViewModel();
                viewModel.StudentVerification = new StudentVerification();
                StudentLogic studentLogic = new StudentLogic();
                PaymentLogic paymentLogic = new PaymentLogic();
                TranscriptRequestLogic transcriptRequestLogic = new TranscriptRequestLogic();
                //FeeLogic feeLogic = new FeeLogic();
                viewModel.StudentVerification.Student = studentLogic.GetBy(personId);
                if (viewModel.StudentVerification.Student != null)
                {
                    //PersonLogic personLogic = new PersonLogic();
                    //Abundance_Nk.Model.Model.Person person = personLogic.GetModelBy(t => t.Person_Id == viewModel.StudentVerification.Student.Id);
                    Payment payment = new Payment();

                    using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
                    {
                        payment = CreatePayment(viewModel.StudentVerification.Student, new FeeType() { Id = (int)FeeTypes.CerificateCollection });
                        if (payment != null)
                        {
                            //Get Payment Specific Setting
                            RemitaSettings settings = new RemitaSettings();
                            RemitaSettingsLogic settingsLogic = new RemitaSettingsLogic();

                            Fee fee = new Fee() { Id = 46, Name = "CERTIFICATE COLLECTION" };
                            settings = settingsLogic.GetBy(8);

                            payment.FeeDetails = paymentLogic.SetFeeDetails(fee.Id);
                            viewModel.StudentVerification.Amount = payment.FeeDetails.Sum(a => a.Fee.Amount);
                            if (viewModel.StudentVerification.Student.Email == null)
                            {
                                viewModel.StudentVerification.Student.Email = viewModel.StudentVerification.Student.FullName + "@gmail.com";
                            }
                            Remita remitaObj = new Remita()
                            {
                                merchantId = settings.MarchantId,
                                serviceTypeId = settings.serviceTypeId,
                                orderId = payment.InvoiceNumber,
                                payerEmail = viewModel.StudentVerification.Student.Email,
                                payerName = viewModel.StudentVerification.Student.FullName,
                                payerPhone = viewModel.StudentVerification.Student.MobilePhone,
                                paymenttype = fee.Name,
                                responseurl = settings.Response_Url,
                                totalAmount = viewModel.StudentVerification.Amount

                            };
                            string toHash = remitaObj.merchantId + remitaObj.serviceTypeId + remitaObj.orderId + remitaObj.totalAmount + remitaObj.responseurl + settings.Api_key;

                            RemitaPayementProcessor remitaPayementProcessor = new RemitaPayementProcessor(settings.Api_key);
                            remitaObj.hash = remitaPayementProcessor.HashPaymentDetailToSHA512(toHash);
                            viewModel.StudentVerification.Remita = remitaObj;
                            RemitaPayment remitaPayment = new RemitaPayment() { payment = payment, OrderId = payment.InvoiceNumber, RRR = payment.InvoiceNumber, Status = "025", Description = fee.Name, TransactionDate = DateTime.Now, TransactionAmount = viewModel.StudentVerification.Amount };
                            RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                            remitaPaymentLogic.Create(remitaPayment);

                            TranscriptRequest transcriptRequest = transcriptRequestLogic.GetModelBy(t => t.Transcript_Request_Id == transcriptId);

                            if (transcriptRequest != null)
                            {
                                transcriptRequest.payment = payment;
                                transcriptRequest.transcriptStatus = new TranscriptStatus() { TranscriptStatusId = (int)Model.Entity.Model.TranscriptStatusList.AwaitingPaymentConfirmation };
                                transcriptRequest.transcriptClearanceStatus = new TranscriptClearanceStatus() { TranscriptClearanceStatusId = (int)Model.Entity.Model.TranscriptClearanceStatusList.Completed };

                                transcriptRequestLogic.Modify(transcriptRequest);
                            }
                        }

                        transaction.Complete();
                    }
                    viewModel.StudentVerification.Payment = payment;

                    return View(viewModel);
                }

            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
            return RedirectToAction("VerificationFees");
        }
        public ActionResult ProcessTranscriptVerificationPayment(int feeId, long personId, long transcriptId)
        {
            try
            {
                viewModel = new TranscriptViewModel();
                viewModel.StudentVerification = new StudentVerification();
                StudentLogic studentLogic = new StudentLogic();
                PaymentLogic paymentLogic = new PaymentLogic();
                TranscriptRequestLogic transcriptRequestLogic = new TranscriptRequestLogic();
                //FeeLogic feeLogic = new FeeLogic();
                viewModel.StudentVerification.Student = studentLogic.GetBy(personId);
                if (viewModel.StudentVerification.Student != null)
                {
                    //PersonLogic personLogic = new PersonLogic();
                    //Abundance_Nk.Model.Model.Person person = personLogic.GetModelBy(t => t.Person_Id == viewModel.StudentVerification.Student.Id);
                    Payment payment = new Payment();

                    TranscriptRequest transcriptRequest = transcriptRequestLogic.GetModelBy(t => t.Transcript_Request_Id == transcriptId);

                    using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
                    {
                        payment = CreatePayment(viewModel.StudentVerification.Student, new FeeType() { Id = (int)FeeTypes.Transcript });
                        if (payment != null)
                        {
                            //Get Payment Specific Setting
                            RemitaSettings settings = new RemitaSettings();
                            RemitaSettingsLogic settingsLogic = new RemitaSettingsLogic();

                            Fee fee = new Fee();
                            if (transcriptRequest.DestinationCountry.Id == "NIG")
                            {
                                fee = new Fee() { Id = 46, Name = "TRANSCRIPT VERIFICATION(LOCAL)" };
                                settings = settingsLogic.GetBy(4);
                            }
                            else
                            {
                                fee = new Fee() { Id = 47, Name = "TRANSCRIPT VERIFICATION(INTERNATIONAL)" };
                                settings = settingsLogic.GetBy(5);
                            }

                            payment.FeeDetails = paymentLogic.SetFeeDetails(fee.Id);
                            viewModel.StudentVerification.Amount = payment.FeeDetails.Sum(a => a.Fee.Amount);
                            if (viewModel.StudentVerification.Student.Email == null)
                            {
                                viewModel.StudentVerification.Student.Email = viewModel.StudentVerification.Student.FullName + "@gmail.com";
                            }
                            Remita remitaObj = new Remita()
                            {
                                merchantId = settings.MarchantId,
                                serviceTypeId = settings.serviceTypeId,
                                orderId = payment.InvoiceNumber,
                                payerEmail = viewModel.StudentVerification.Student.Email,
                                payerName = viewModel.StudentVerification.Student.FullName,
                                payerPhone = viewModel.StudentVerification.Student.MobilePhone,
                                paymenttype = fee.Name,
                                responseurl = settings.Response_Url,
                                totalAmount = viewModel.StudentVerification.Amount

                            };
                            string toHash = remitaObj.merchantId + remitaObj.serviceTypeId + remitaObj.orderId + remitaObj.totalAmount + remitaObj.responseurl + settings.Api_key;

                            RemitaPayementProcessor remitaPayementProcessor = new RemitaPayementProcessor(settings.Api_key);
                            remitaObj.hash = remitaPayementProcessor.HashPaymentDetailToSHA512(toHash);
                            viewModel.StudentVerification.Remita = remitaObj;
                            RemitaPayment remitaPayment = new RemitaPayment() { payment = payment, OrderId = payment.InvoiceNumber, RRR = payment.InvoiceNumber, Status = "025", Description = fee.Name, TransactionDate = DateTime.Now, TransactionAmount = viewModel.StudentVerification.Amount };
                            RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                            remitaPaymentLogic.Create(remitaPayment);



                            if (transcriptRequest != null)
                            {
                                transcriptRequest.payment = payment;
                                transcriptRequest.transcriptStatus = new TranscriptStatus() { TranscriptStatusId = (int)Model.Entity.Model.TranscriptStatusList.AwaitingPaymentConfirmation };
                                transcriptRequest.transcriptClearanceStatus = new TranscriptClearanceStatus() { TranscriptClearanceStatusId = (int)Model.Entity.Model.TranscriptClearanceStatusList.Completed };

                                transcriptRequestLogic.Modify(transcriptRequest);
                            }
                        }

                        transaction.Complete();
                    }
                    viewModel.StudentVerification.Payment = payment;

                    return View(viewModel);
                }

            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            return RedirectToAction("VerificationFees");
        }
        public ActionResult ProcessConvocationPayment(int feeId, long personId, long transcriptId)
        {
            try
            {
                viewModel = new TranscriptViewModel();
                viewModel.StudentVerification = new StudentVerification();
                StudentLogic studentLogic = new StudentLogic();
                PaymentLogic paymentLogic = new PaymentLogic();
                TranscriptRequestLogic transcriptRequestLogic = new TranscriptRequestLogic();
                //FeeLogic feeLogic = new FeeLogic();
                viewModel.StudentVerification.Student = studentLogic.GetBy(personId);
                if (viewModel.StudentVerification.Student != null)
                {
                    //PersonLogic personLogic = new PersonLogic();
                    //Abundance_Nk.Model.Model.Person person = personLogic.GetModelBy(t => t.Person_Id == viewModel.StudentVerification.Student.Id);
                    Payment payment = new Payment();

                    TranscriptRequest transcriptRequest = transcriptRequestLogic.GetModelBy(t => t.Transcript_Request_Id == transcriptId);

                    using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
                    {
                        payment = CreatePayment(viewModel.StudentVerification.Student, new FeeType() { Id = (int)FeeTypes.ConvocationFee });
                        if (payment != null)
                        {
                            //Get Payment Specific Setting
                            RemitaSettings settings = new RemitaSettings();
                            RemitaSettingsLogic settingsLogic = new RemitaSettingsLogic();

                            Fee fee = new Fee() { Id = 60, Name = "CONVOCATION FEE" };

                            settings = settingsLogic.GetBy(4);

                            payment.FeeDetails = paymentLogic.SetFeeDetails(fee.Id);
                            viewModel.StudentVerification.Amount = payment.FeeDetails.Sum(a => a.Fee.Amount);
                            if (viewModel.StudentVerification.Student.Email == null)
                            {
                                viewModel.StudentVerification.Student.Email = viewModel.StudentVerification.Student.FullName + "@gmail.com";
                            }
                            Remita remitaObj = new Remita()
                            {
                                merchantId = settings.MarchantId,
                                serviceTypeId = settings.serviceTypeId,
                                orderId = payment.InvoiceNumber,
                                payerEmail = viewModel.StudentVerification.Student.Email,
                                payerName = viewModel.StudentVerification.Student.FullName,
                                payerPhone = viewModel.StudentVerification.Student.MobilePhone,
                                paymenttype = fee.Name,
                                responseurl = settings.Response_Url,
                                totalAmount = viewModel.StudentVerification.Amount

                            };
                            string toHash = remitaObj.merchantId + remitaObj.serviceTypeId + remitaObj.orderId + remitaObj.totalAmount + remitaObj.responseurl + settings.Api_key;

                            RemitaPayementProcessor remitaPayementProcessor = new RemitaPayementProcessor(settings.Api_key);
                            remitaObj.hash = remitaPayementProcessor.HashPaymentDetailToSHA512(toHash);
                            viewModel.StudentVerification.Remita = remitaObj;
                            RemitaPayment remitaPayment = new RemitaPayment() { payment = payment, OrderId = payment.InvoiceNumber, RRR = payment.InvoiceNumber, Status = "025", Description = fee.Name, TransactionDate = DateTime.Now, TransactionAmount = viewModel.StudentVerification.Amount };
                            RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                            remitaPaymentLogic.Create(remitaPayment);

                            if (transcriptRequest != null)
                            {
                                transcriptRequest.payment = payment;
                                transcriptRequest.transcriptStatus = new TranscriptStatus() { TranscriptStatusId = (int)Model.Entity.Model.TranscriptStatusList.AwaitingPaymentConfirmation };
                                transcriptRequest.transcriptClearanceStatus = new TranscriptClearanceStatus() { TranscriptClearanceStatusId = (int)Model.Entity.Model.TranscriptClearanceStatusList.Completed };

                                transcriptRequestLogic.Modify(transcriptRequest);
                            }
                        }

                        transaction.Complete();
                    }
                    viewModel.StudentVerification.Payment = payment;

                    return View(viewModel);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
            return RedirectToAction("VerificationFees");
        }

        public ActionResult ConvocationFee()
        {
            viewModel = new TranscriptViewModel();
            try
            {
                SetFeeTypeDropDown(viewModel);

                ViewBag.States = viewModel.StateSelectListItem;
                ViewBag.Sessions = viewModel.AllSessionSelectListItem;
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

        private void SetFeeTypeDropDown(TranscriptViewModel viewModel)
        {
            try
            {
                if (viewModel.FeeTypeSelectListItem != null && viewModel.FeeTypeSelectListItem.Count > 0)
                {
                    viewModel.FeeType = new FeeType();
                    viewModel.FeeType.Id = (int)FeeTypes.ConvocationFee;
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
        public ActionResult ConvocationFee(TranscriptViewModel viewModel)
        {
            try
            {
                PersonLogic personLogic = new PersonLogic();
                StudentLogic studentLogic = new StudentLogic();
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();

                viewModel.FeeType = new FeeType() { Id = (int)FeeTypes.ConvocationFee };
                viewModel.PaymentMode = new PaymentMode() { Id = 1 };
                viewModel.PaymentType = new PaymentType() { Id = 2 };

                ViewBag.States = viewModel.StateSelectListItem;
                ViewBag.Sessions = viewModel.AllSessionSelectListItem;
                ViewBag.Programmes = viewModel.ProgrammeSelectListItem;
                ViewBag.Departments = new SelectList(new List<Department>(), Utility.ID, Utility.NAME);
                ViewBag.DepartmentOptions = new SelectList(new List<DepartmentOption>(), Utility.ID, Utility.NAME);

                Model.Model.Student student = studentLogic.GetModelsBy(s => s.Matric_Number == viewModel.Student.MatricNumber.Trim()).LastOrDefault();

                if (student != null)
                {
                    viewModel.StudentAlreadyExist = true;
                    viewModel.Person = personLogic.GetModelBy(p => p.Person_Id == student.Id);
                    viewModel.Student = student;
                    viewModel.StudentLevel = studentLevelLogic.GetModelsBy(l => l.Person_Id == student.Id).LastOrDefault();


                    if (viewModel.StudentLevel != null && viewModel.StudentLevel.Programme.Id > 0)
                    {
                        ViewBag.Programmes = new SelectList(viewModel.ProgrammeSelectListItem, Utility.VALUE, Utility.TEXT, viewModel.StudentLevel.Programme.Id);
                    }
                    viewModel.LevelSelectListItem = Utility.PopulateLevelSelectListItem();
                    ViewBag.Levels = viewModel.LevelSelectListItem;

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
                            ViewBag.Levels = viewModel.LevelSelectListItem;
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

            viewModel.ShowInvoicePage = true;
            SetFeeTypeDropDown(viewModel);

            if (viewModel.StudentLevel == null)
            {
                viewModel.StudentAlreadyExist = false;
            }

            return View(viewModel);
        }
        public void CheckAndUpdateStudentLevel(TranscriptViewModel viewModel)
        {
            try
            {
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                List<StudentLevel> studentLevelList = studentLevelLogic.GetModelsBy(s => s.Person_Id == viewModel.Student.Id);
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
                else if (viewModel.StudentLevel == null)
                {

                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void SetDepartmentIfExist(TranscriptViewModel viewModel)
        {
            try
            {
                if (viewModel.StudentLevel != null && viewModel.StudentLevel.Programme != null && viewModel.StudentLevel.Programme.Id > 0)
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

        private void SetDepartmentOptionIfExist(TranscriptViewModel viewModel)
        {
            try
            {
                if (viewModel.StudentLevel != null && viewModel.StudentLevel.Department != null && viewModel.StudentLevel.Department.Id > 0)
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
        [HttpPost]
        public ActionResult ConvocationFeeInvocie(TranscriptViewModel viewModel)
        {
            try
            {
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                PaymentLogic paymentLogic = new PaymentLogic();
                PersonLogic personLogic = new PersonLogic();

                if (InvalidDepartmentSelection(viewModel))
                {
                    KeepInvoiceGenerationDropDownState(viewModel);
                    return RedirectToAction("ConvocationFee");
                }

                if (InvalidMatricNumber(viewModel.Student.MatricNumber))
                {
                    KeepInvoiceGenerationDropDownState(viewModel);
                    return RedirectToAction("ConvocationFee");
                }

                Payment payment = null;


                RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();

                if (viewModel.StudentAlreadyExist == false)
                {
                    using (TransactionScope transaction = new TransactionScope())
                    {
                        CreatePerson(viewModel);
                        if (viewModel.Student != null && viewModel.Student.Id > 0)
                        {
                            //do nothing   
                        }
                        else
                        {
                            CreateStudent(viewModel);
                        }

                        payment = CreatePayment(viewModel);
                        CreateStudentLevel(viewModel);


                        RemitaPayment existingRemitaPayment = remitaPaymentLogic.GetBy(payment.Id);
                        if (existingRemitaPayment == null)
                        {
                            //Get Payment Specific Setting
                            RemitaSettings settings = new RemitaSettings();
                            RemitaSettingsLogic settingsLogic = new RemitaSettingsLogic();
                            settings = settingsLogic.GetBy((int)RemitaServiceSettings.Convocation);

                            decimal amt = Convert.ToDecimal(payment.Amount);

                            //Get BaseURL
                            string remitaBaseUrl = ConfigurationManager.AppSettings["RemitaBaseUrl"].ToString();
                            RemitaPayementProcessor remitaProcessor = new RemitaPayementProcessor(settings.Api_key);
                            RemitaPayment remitaPayment = remitaProcessor.GenerateRRRCard(payment.InvoiceNumber, remitaBaseUrl, "CONVOCATION FEE", settings, amt);

                            if (remitaPayment != null)
                            {
                                transaction.Complete();
                            }
                        }

                    }
                }
                else
                {
                    using (TransactionScope transaction = new TransactionScope())
                    {
                        FeeType feeType = new FeeType() { Id = (int)FeeTypes.ConvocationFee };
                        payment = paymentLogic.GetBy(feeType, viewModel.Person, viewModel.Session);
                        if (payment == null || payment.Id <= 0)
                        {
                            payment = CreatePayment(viewModel);
                        }

                        RemitaPayment existingRemitaPayment = remitaPaymentLogic.GetBy(payment.Id);
                        if (existingRemitaPayment == null)
                        {
                            //Get Payment Specific Setting
                            RemitaSettings settings = new RemitaSettings();
                            RemitaSettingsLogic settingsLogic = new RemitaSettingsLogic();
                            settings = settingsLogic.GetBy((int)RemitaServiceSettings.Convocation);

                            decimal amt = Convert.ToDecimal(payment.Amount);

                            //Get BaseURL
                            string remitaBaseUrl = ConfigurationManager.AppSettings["RemitaBaseUrl"].ToString();
                            RemitaPayementProcessor remitaProcessor = new RemitaPayementProcessor(settings.Api_key);
                            
                            RemitaPayment remitaPayment = remitaProcessor.GenerateRRRCard(payment.InvoiceNumber, remitaBaseUrl, "CONVOCATION FEE", settings, amt);
                            viewModel.Hash= GenerateHash(settings.Api_key, remitaPayment);
                            viewModel.RemitaPayment = remitaPayment;
                            if (remitaPayment != null)
                            {
                                transaction.Complete();
                            }
                        }
                        else
                        {
                            viewModel.RemitaPayment = existingRemitaPayment;
                            viewModel.Payment = payment;
                        }
                    }
                }
                
                TempData["PaymentViewModel"] = viewModel;
                return RedirectToAction("Invoice", "Credential", new { Area = "Common", pmid = Abundance_Nk.Web.Models.Utility.Encrypt(payment.Id.ToString()), });

            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            KeepInvoiceGenerationDropDownState(viewModel);
            return RedirectToAction("ConvocationFee");
        }

        public ActionResult CardPayment()
        {
            TranscriptViewModel viewModel = new TranscriptViewModel();
            viewModel = (TranscriptViewModel)TempData["PaymentViewModel"];
            viewModel.ResponseUrl = ConfigurationManager.AppSettings["RemitaCardResponseUrl"].ToString();
            TempData.Keep("PaymentViewModel");

            return View(viewModel);
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

        private bool InvalidDepartmentSelection(TranscriptViewModel viewModel)
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

        private void KeepInvoiceGenerationDropDownState(TranscriptViewModel viewModel)
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

                if (viewModel.Person!=null && viewModel.Person.State != null && !string.IsNullOrEmpty(viewModel.Person.State.Id))
                {
                    ViewBag.States = new SelectList(viewModel.StateSelectListItem, Utility.VALUE, Utility.TEXT, viewModel.Person.State.Id);
                }
                else
                {
                    ViewBag.States = new SelectList(viewModel.StateSelectListItem, Utility.VALUE, Utility.TEXT);
                }

                if (viewModel.StudentLevel!=null && viewModel.StudentLevel.Level != null && viewModel.StudentLevel.Level.Id > 0)
                {
                    ViewBag.Levels = new SelectList(viewModel.LevelSelectListItem, Utility.VALUE, Utility.TEXT, viewModel.StudentLevel.Level.Id);
                }
                else
                {
                    ViewBag.Levels = new SelectList(viewModel.LevelSelectListItem, Utility.VALUE, Utility.TEXT);
                }

                if (viewModel.StudentLevel!=null && viewModel.StudentLevel.Programme != null && viewModel.StudentLevel.Programme.Id > 0)
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

        private Person CreatePerson(TranscriptViewModel viewModel)
        {
            try
            {
                PersonLogic personLogic = new PersonLogic();

                Role role = new Role() { Id = 5 };
                //PersonType personType = new PersonType() { Id = viewModel.PersonType.Id };
                Nationality nationality = new Nationality() { Id = 1 };

                viewModel.Person.Role = role;
                viewModel.Person.Nationality = nationality;
                viewModel.Person.DateEntered = DateTime.Now;
                viewModel.Person.Type = new PersonType() { Id = (int)PersonTypes.Student };

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

        public Model.Model.Student CreateStudent(TranscriptViewModel viewModel)
        {
            try
            {
                StudentLogic studentLogic = new StudentLogic();

                viewModel.Student.Number = 4;
                viewModel.Student.Category = new StudentCategory() { Id = viewModel.StudentLevel.Level.Id <= 2 ? 1 : 2 };
                viewModel.Student.Type = new StudentType() { Id = viewModel.StudentLevel.Programme.Id <= 2 ? 1 : 2 };
                viewModel.Student.Id = viewModel.Person.Id;
                viewModel.Student.Status = new StudentStatus() { Id = 1 };

                return studentLogic.Create(viewModel.Student);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private Payment CreatePayment(TranscriptViewModel viewModel)
        {
            Payment newPayment = new Payment();
            try
            {
                PaymentLogic paymentLogic = new PaymentLogic();
                OnlinePaymentLogic onlinePaymentLogic = new OnlinePaymentLogic();

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
                if (viewModel.FeeType.Id == (int)FeeTypes.ConvocationFee)
                {
                    //Deferentiate between HND and ND
                    int levelId = viewModel.StudentLevel.Programme.Id > 2 ? 4 : 2;
                    newPayment.FeeDetails = paymentLogic.SetFeeDetails(newPayment, viewModel.StudentLevel.Programme.Id, levelId, pyamentMode.Id, viewModel.StudentLevel.Department.Id, viewModel.Session.Id);
                }
                    
                else{
                    newPayment.FeeDetails = paymentLogic.SetFeeDetails(newPayment, viewModel.StudentLevel.Programme.Id, viewModel.StudentLevel.Level.Id, pyamentMode.Id, viewModel.StudentLevel.Department.Id, viewModel.Session.Id);
                }
                    
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
                payment.Amount = Convert.ToString(Amt);
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

        private StudentLevel CreateStudentLevel(TranscriptViewModel viewModel)
        {
            try
            {
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();

                viewModel.StudentLevel.Session = viewModel.Session;
                viewModel.StudentLevel.Student = viewModel.Student;
                return studentLevelLogic.Create(viewModel.StudentLevel);

            }
            catch (Exception)
            {
                throw;
            }
        }
        public ActionResult PayConvocationFee()
        {
            try
            {
                viewModel = new TranscriptViewModel();
                ViewBag.Sessions = viewModel.SessionSelectListItem;
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult PayConvocationFee(TranscriptViewModel viewModel)
        {
            try
            {
                if (viewModel.ConfirmationNumber != null)
                {
                    PaymentLogic paymentLogic = new PaymentLogic();

                    if (viewModel.ConfirmationNumber.Length > 12)
                    {
                        Model.Model.Session session = viewModel.Session;
                        FeeType feetype = new FeeType() { Id = (int)FeeTypes.ConvocationFee };

                        Payment payment = paymentLogic.InvalidConfirmationOrderNumber(viewModel.ConfirmationNumber, session, feetype);

                        if (payment != null && payment.Id > 0)
                        {
                            if (payment.FeeType.Id != (int)FeeTypes.ConvocationFee)
                            {
                                SetMessage("Confirmation Order Number (" + viewModel.ConfirmationNumber + ") entered is not for Convocation Fee payment! Please enter your Convocation Fee Confirmation Order Number.", Message.Category.Error);
                                ViewBag.Sessions = viewModel.SessionSelectListItem;
                                return View(viewModel);
                            }

                            return RedirectToAction("Receipt", "Credential", new { area = "Common", pmid = payment.Id });
                        }
                    }
                    else
                    {
                        SetMessage("Invalid Confirmation Order Number!", Message.Category.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            ViewBag.Sessions = viewModel.SessionSelectListItem;
            return View(viewModel);
        }

        public JsonResult GetDeliveryServices(string countryId, string stateId)
        {
            try
            {
                List<DeliveryService> deliveryServicesInStateCountry = new List<DeliveryService>();

                if (!string.IsNullOrEmpty(countryId) && !string.IsNullOrEmpty(stateId))
                {
                    DeliveryServiceZoneLogic deliveryServiceZoneLogic = new DeliveryServiceZoneLogic();
                    DeliveryServiceLogic deliveryServiceLogic = new DeliveryServiceLogic();
                    StateGeoZoneLogic stateGeoZoneLogic = new StateGeoZoneLogic();

                    List<StateGeoZone> stateGeoZones = stateGeoZoneLogic.GetModelsBy(s => s.State_Id == stateId && s.Activated);

                    for (int i = 0; i < stateGeoZones.Count; i++)
                    {
                        StateGeoZone stateGeoZone = stateGeoZones[i];

                        List<DeliveryServiceZone> deliveryServiceZones = deliveryServiceZoneLogic.GetModelsBy(s => s.Country_Id == countryId && s.Geo_Zone_Id == stateGeoZone.GeoZone.Id && s.Activated);

                        List<DeliveryService> deliveryServices = deliveryServiceLogic.GetModelsBy(s => s.Activated);

                        for (int j = 0; j < deliveryServices.Count; j++)
                        {
                            DeliveryService deliveryService = deliveryServices[j];
                            if (deliveryServiceZones.Count(s => s.DeliveryService.Id == deliveryService.Id) > 0)
                            {
                                if (deliveryServicesInStateCountry.Count(s => s.Id == deliveryService.Id) <= 0)
                                {

                                    deliveryServicesInStateCountry.Add(deliveryService);
                                }

                            }
                        }
                    }
                }

                return Json(new SelectList(deliveryServicesInStateCountry, "Id", "Name"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public JsonResult GetDeliveryServiceZones(string countryId, string stateId, int deliveryServiceId)
        {
            try
            {
                List<DeliveryServiceZone> deliveryServiceZones = new List<DeliveryServiceZone>();

                if (!string.IsNullOrEmpty(countryId) && !string.IsNullOrEmpty(stateId) && deliveryServiceId > 0)
                {
                    DeliveryServiceZoneLogic deliveryServiceZoneLogic = new DeliveryServiceZoneLogic();
                    StateGeoZoneLogic stateGeoZoneLogic = new StateGeoZoneLogic();

                    List<StateGeoZone> stateGeoZones = stateGeoZoneLogic.GetModelsBy(s => s.State_Id == stateId && s.Activated);

                    for (int i = 0; i < stateGeoZones.Count; i++)
                    {
                        StateGeoZone stateGeoZone = stateGeoZones[i];

                        List<DeliveryServiceZone> currentDeliveryServiceZones = deliveryServiceZoneLogic.GetModelsBy(s => s.Country_Id == countryId && s.Geo_Zone_Id == stateGeoZone.GeoZone.Id && s.Delivery_Service_Id == deliveryServiceId && s.Activated);

                        for (int j = 0; j < currentDeliveryServiceZones.Count; j++)
                        {
                            currentDeliveryServiceZones[j].Name = currentDeliveryServiceZones[j].GeoZone.Name + " - " + currentDeliveryServiceZones[j].Fee.Amount;

                            DeliveryServiceZone serviceZone = currentDeliveryServiceZones[j];

                            if (deliveryServiceZones.Count(s => s.Id == serviceZone.Id) <= 0)
                            {
                                deliveryServiceZones.Add(currentDeliveryServiceZones[j]);
                            }

                        }
                    }
                }

                return Json(new SelectList(deliveryServiceZones, "Id", "Name"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
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

                return Json(new SelectList(departmentOptions, ID, NAME), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public JsonResult GetDepartmentByProgrammeId(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return null;
                }

                Programme programme = new Programme() { Id = Convert.ToInt32(id) };

                DepartmentLogic departmentLogic = new DepartmentLogic();
                List<Department> departments = departmentLogic.GetBy(programme);

                return Json(new SelectList(departments, ID, NAME), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Fee ResolveForegnTranscriptFee(TranscriptRequest model)
        {
            try
            {
                TranscriptRequestLogic transcriptRequestLogic = new TranscriptRequestLogic();
                Fee fee = null;
                string[] categoryOne = new string[] { "ESP", "ITA", "CAN", "UK" };
                if(model.DestinationCountry != null && model.DestinationCountry.Id != null)
                {
                    var check = Array.Exists(categoryOne, x => x == model.DestinationCountry.Id);
                    if (check)
                    {
                        fee = new Fee() { Id = (int)TranscriptFees.International_SP_IT_CAN_US };
                    }
                    else
                    {
                        fee = new Fee() { Id = (int)TranscriptFees.InternationalOthers };
                    }

                    return fee;
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