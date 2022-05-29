using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

using Abundance_Nk.Model.Entity;
using Abundance_Nk.Business;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Web.Areas.Admin.ViewModels;
using Abundance_Nk.Web.Controllers;
using Abundance_Nk.Web.Models;

namespace Abundance_Nk.Web.Areas.Admin.Controllers
{
    public class AdmissionProcessingController : BaseController
    {
        private AdmissionProcessingViewModel viewModel;
        private const string ID = "Id";
        private const string NAME = "Name";
        private const string VALUE = "Value";
        private const string TEXT = "Text";
        //private Abundance_NkEntities db = new Abundance_NkEntities();

        public AdmissionProcessingController()
        {
            viewModel = new AdmissionProcessingViewModel();
        }

        public ActionResult Index()
        {
            ViewBag.SessionId = viewModel.SessionSelectList;

            return View(viewModel);
        }

        public ActionResult ViewDetails(int? mid)
        {

            return View();
        }

        public ActionResult ClearApplicant()
        {
            viewModel = new AdmissionProcessingViewModel();
            //viewModel.ProgrammeSelectListItem = Utility.PopulateAllProgrammeSelectListItem();
            //viewModel.SessionSelectList = Utility.PopulateSingleSessionSelectListItem(11);
            //ViewBag.ProgrammeId = viewModel.ProgrammeSelectListItem;
            //ViewBag.SessionId = viewModel.SessionSelectList;
            //ViewBag.DepartmentId = new SelectList(new List<Department>(), ID, NAME);
            viewModel.GetApplicantByStatus(ApplicantStatus.Status.CompletedStudentInformationForm);
            return View(viewModel);
        }
        //[HttpPost]
        //public ActionResult ClearApplicant(AdmissionProcessingViewModel vModel)
        //{
        //    viewModel = new AdmissionProcessingViewModel();
        //    viewModel.ProgrammeSelectListItem = Utility.PopulateAllProgrammeSelectListItem();
        //    viewModel.SessionSelectList = Utility.PopulateSingleSessionSelectListItem(11);
        //    ViewBag.ProgrammeId = viewModel.ProgrammeSelectListItem;
        //    ViewBag.SessionId = viewModel.SessionSelectList;
        //    ViewBag.DepartmentId = new SelectList(new List<Department>(), ID, NAME);
        //    viewModel.GetApplicantDetailsByStatus(ApplicantStatus.Status.CompletedStudentInformationForm, vModel.Session, vModel.Department, vModel.Programme);
        //    return View(viewModel);
        //}

        public ActionResult Index2()
        {
            AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
            AppliedCourse appliedCourse = appliedCourseLogic.GetModelBy(a => a.Person_Id == 32);

            //AdmissionCriteriaLogic admissionCriteriaLogic = new AdmissionCriteriaLogic();
            //ViewBag.RejectReason = admissionCriteriaLogic.EvaluateApplication(appliedCourse);

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult AcceptOrReject(List<int> ids, int sessionId, bool isRejected)
        {
            try
            {
                if (ids != null && ids.Count > 0)
                {
                    List<ApplicationForm> applications = new List<ApplicationForm>();
                    foreach (int id in ids)
                    {
                        ApplicationForm application = new ApplicationForm() { Id = id };
                        applications.Add(application);
                    }

                    ApplicationFormLogic applicationFormLogic = new ApplicationFormLogic();
                    bool accepted = applicationFormLogic.AcceptOrReject(applications, isRejected);
                    if (accepted)
                    {
                        Session session = new Session() { Id = sessionId };
                        viewModel.GetApplicationsBy(!isRejected, session);
                        SetMessage("Select Applications has be successfully Accepted.", Message.Category.Information);
                    }
                    else
                    {
                        SetMessage("Opeartion failed during selected Application Acceptance! Please try again.", Message.Category.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage("Operation failed! " + ex.Message, Message.Category.Error);
            }

            return PartialView("_ApplicationFormsGrid", viewModel.ApplicationForms);
        }

        [HttpPost]
        public ActionResult FindBy(int sessionId, bool isRejected)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Session session = new Session() { Id = sessionId };
                    viewModel.GetApplicationsBy(isRejected, session);
                }

            }
            catch (Exception ex)
            {
                TempData["msg"] = "Operation failed! " + ex.Message;
                SetMessage("Operation failed! " + ex.Message, Message.Category.Error);
            }

            return PartialView("_ApplicationFormsGrid", viewModel.ApplicationForms);
        }

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

        public ActionResult StudentForm(long fid)
        {
            try
            {
                StudentFormViewModel studentFormViewModel = new StudentFormViewModel();
                ApplicationForm form = new ApplicationForm() { Id = fid };

                studentFormViewModel.LoadApplicantionFormBy(fid);

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










        //private AdmissionProcessingViewModel viewModel;
        //private Abundance_NkEntities db = new Abundance_NkEntities();

        //public AdmissionProcessingController()
        //{
        //    viewModel = new AdmissionProcessingViewModel();
        //}

        //public ActionResult Index()
        //{
        //    ViewBag.SessionId = viewModel.SessionSelectList;
            
        //    return View(viewModel);
        //}

        //[AllowAnonymous]
        //public ActionResult Index2()
        //{
        //    try
        //    {
        //        AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
        //        PreviousEducationLogic previousEducationLogic = new PreviousEducationLogic();
        //        AppliedCourse appliedCourse = appliedCourseLogic.GetModelBy(a => a.Person_Id == 32);
        //        PreviousEducation previouseducation = previousEducationLogic.GetModelBy(p => p.Person_Id == 32);

        //        AdmissionCriteriaLogic admissionCriteriaLogic = new AdmissionCriteriaLogic();
        //        string rejectReason = admissionCriteriaLogic.EvaluateApplication(appliedCourse, previouseducation);
        //        ViewBag.RejectReason = rejectReason;
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.RejectReason = ex.Message;
        //        SetMessage(ex.Message, Message.Category.Error);
        //    }

        //    return View(viewModel);
        //}

        //[HttpPost]
        //public ActionResult AcceptOrReject(List<int> ids, int sessionId, bool isRejected)
        //{
        //    try
        //    {
        //        if (ids != null && ids.Count > 0)
        //        {
        //            List<ApplicationForm> applications = new List<ApplicationForm>();

        //            foreach (int id in ids)
        //            {
        //                ApplicationForm application = new ApplicationForm() { Id = id };
        //                applications.Add(application);
        //            }

        //            ApplicationFormLogic applicationFormLogic = new ApplicationFormLogic();
        //            bool accepted = applicationFormLogic.AcceptOrReject(applications, isRejected);
        //            if (accepted)
        //            {
        //                Session session = new Session() { Id = sessionId };
        //                viewModel.GetApplicationsBy(!isRejected, session);
        //                SetMessage("Select Applications has be successfully Accepted.", Message.Category.Information);
        //            }
        //            else
        //            {
        //                SetMessage("Opeartion failed during selected Application Acceptance! Please try again.", Message.Category.Information);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        SetMessage("Operation failed! " + ex.Message, Message.Category.Error);
        //    }

        //    return PartialView("_ApplicationFormsGrid", viewModel.ApplicationForms);
        //}

        //[HttpPost]
        //public ActionResult FindBy(int sessionId, bool isRejected)
        //{
        //    try
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            Session session = new Session() { Id = sessionId };
        //            viewModel.GetApplicationsBy(isRejected, session);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["msg"] = "Operation failed! " + ex.Message;
        //    }

        //    return PartialView("_ApplicationFormsGrid", viewModel.ApplicationForms);
        //}

        ////public ActionResult FindAllAcceptedBy(int sessionId)
        ////{
        ////    try
        ////    {
        ////        if (ModelState.IsValid)
        ////        {
        ////            Session session = new Session() { Id = sessionId };
        ////            viewModel.GetApplicationsBy(false, session);
        ////        }
        ////    }
        ////    catch (Exception ex)
        ////    {

        ////    }

        ////    return PartialView("_ApplicationFormsGrid", viewModel.ApplicationForms);
        ////}

        ////[HttpPost]
        ////public void ApproveOrReject(List<int> ids, string status)
        ////{
        ////    try
        ////    {
        ////        TempData["msg"] = "Operation was successful.";
        ////    }
        ////    catch(Exception ex)
        ////    {
        ////        TempData["msg"] = "Operation failed! " + ex.Message;
        ////    }
        ////}
       
        

        ////[HttpPost]
        ////public ActionResult Index(AdmissionProcessingViewModel admissionProcessingViewModel)
        ////{
        ////    //bool rejected = admissionProcessingViewModel.Rejected;
        ////    //admissionProcessingViewModel.GetApplicationsBy(admissionProcessingViewModel.Rejected);

        ////    return View(admissionProcessingViewModel.ApplicationForms);
        ////}

        //// GET: /Admin/AdmissionProcessing/Details/5
        //public ActionResult Details(long? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    APPLICATION_FORM application_form = db.APPLICATION_FORM.Find(id);
        //    if (application_form == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(application_form);
        //}

        //// GET: /Admin/AdmissionProcessing/Create
        //public ActionResult Create()
        //{
        //    ViewBag.Application_Form_Setting_Id = new SelectList(db.APPLICATION_FORM_SETTING, "Application_Form_Setting_Id", "Exam_Venue");
        //    ViewBag.Application_Programme_Fee_Id = new SelectList(db.APPLICATION_PROGRAMME_FEE, "Application_Programme_Fee_Id", "Application_Programme_Fee_Id");
        //    ViewBag.Payment_Id = new SelectList(db.PAYMENT, "Payment_Id", "Invoice_Number");
        //    ViewBag.Person_Id = new SelectList(db.PERSON, "Person_Id", "First_Name");
        //    return View();
        //}

        //// POST: /Admin/AdmissionProcessing/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include="Application_Form_Id,Serial_Number,Application_Form_Number,Application_Form_Setting_Id,Application_Programme_Fee_Id,Payment_Id,Person_Id,Date_Submitted,Release,Rejected,Reject_Reason,Remarks")] APPLICATION_FORM application_form)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.APPLICATION_FORM.Add(application_form);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    ViewBag.Application_Form_Setting_Id = new SelectList(db.APPLICATION_FORM_SETTING, "Application_Form_Setting_Id", "Exam_Venue", application_form.Application_Form_Setting_Id);
        //    ViewBag.Application_Programme_Fee_Id = new SelectList(db.APPLICATION_PROGRAMME_FEE, "Application_Programme_Fee_Id", "Application_Programme_Fee_Id", application_form.Application_Programme_Fee_Id);
        //    ViewBag.Payment_Id = new SelectList(db.PAYMENT, "Payment_Id", "Invoice_Number", application_form.Payment_Id);
        //    ViewBag.Person_Id = new SelectList(db.PERSON, "Person_Id", "First_Name", application_form.Person_Id);
        //    return View(application_form);
        //}

        //// GET: /Admin/AdmissionProcessing/Edit/5
        //public ActionResult Edit(long? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    APPLICATION_FORM application_form = db.APPLICATION_FORM.Find(id);
        //    if (application_form == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.Application_Form_Setting_Id = new SelectList(db.APPLICATION_FORM_SETTING, "Application_Form_Setting_Id", "Exam_Venue", application_form.Application_Form_Setting_Id);
        //    ViewBag.Application_Programme_Fee_Id = new SelectList(db.APPLICATION_PROGRAMME_FEE, "Application_Programme_Fee_Id", "Application_Programme_Fee_Id", application_form.Application_Programme_Fee_Id);
        //    ViewBag.Payment_Id = new SelectList(db.PAYMENT, "Payment_Id", "Invoice_Number", application_form.Payment_Id);
        //    ViewBag.Person_Id = new SelectList(db.PERSON, "Person_Id", "First_Name", application_form.Person_Id);
        //    return View(application_form);
        //}

        //// POST: /Admin/AdmissionProcessing/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include="Application_Form_Id,Serial_Number,Application_Form_Number,Application_Form_Setting_Id,Application_Programme_Fee_Id,Payment_Id,Person_Id,Date_Submitted,Release,Rejected,Reject_Reason,Remarks")] APPLICATION_FORM application_form)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(application_form).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.Application_Form_Setting_Id = new SelectList(db.APPLICATION_FORM_SETTING, "Application_Form_Setting_Id", "Exam_Venue", application_form.Application_Form_Setting_Id);
        //    ViewBag.Application_Programme_Fee_Id = new SelectList(db.APPLICATION_PROGRAMME_FEE, "Application_Programme_Fee_Id", "Application_Programme_Fee_Id", application_form.Application_Programme_Fee_Id);
        //    ViewBag.Payment_Id = new SelectList(db.PAYMENT, "Payment_Id", "Invoice_Number", application_form.Payment_Id);
        //    ViewBag.Person_Id = new SelectList(db.PERSON, "Person_Id", "First_Name", application_form.Person_Id);
        //    return View(application_form);
        //}

        //// GET: /Admin/AdmissionProcessing/Delete/5
        //public ActionResult Delete(long? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    APPLICATION_FORM application_form = db.APPLICATION_FORM.Find(id);
        //    if (application_form == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(application_form);
        //}

        //// POST: /Admin/AdmissionProcessing/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(long id)
        //{
        //    APPLICATION_FORM application_form = db.APPLICATION_FORM.Find(id);
        //    db.APPLICATION_FORM.Remove(application_form);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        db.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}
    }
}
