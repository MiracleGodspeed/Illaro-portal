using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Abundance_Nk.Model.Entity;
using Abundance_Nk.Business;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Web.Controllers;
using Abundance_Nk.Web.Areas.Admin.ViewModels;
using Abundance_Nk.Web.Models;
using System.Transactions;
using System.Linq.Expressions;
using System.IO;
using System.Data.OleDb;
using System.Net.Mime;
using System.Web.UI;
using Abundance_Nk.Model.Entity.Model;
using Ionic.Zip;
using System.Configuration;

namespace Abundance_Nk.Web.Areas.Admin.Views
{
    public class SupportController : BaseController
    {
        private const string ID = "Id";
        private const string NAME = "Name";
        private const string VALUE = "Value";
        private const string TEXT = "Text";
        private const string FIRST_SITTING = "FIRST SITTING";
        private const string SECOND_SITTING = "SECOND SITTING";
        private Abundance_NkEntities db = new Abundance_NkEntities();
        private SupportViewModel viewmodel;

        // GET: /Admin/Support/
        public ActionResult Index()
        {
            viewmodel = new SupportViewModel();
            try
            {
                ViewBag.DepartmentId = new SelectList(new List<Department>(), ID, NAME);
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
            TempData["SupportViewModel"] = viewmodel;
            return View();
        }

        [HttpPost]
        public ActionResult Index(SupportViewModel vModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (vModel.InvoiceNumber != null)
                    {
                        TempData["Invoice/ConfirmationOrder"] = vModel.InvoiceNumber;
                        Payment payment = new Payment();
                        PaymentLogic paymentLogic = new PaymentLogic();
                        PaymentEtranzact p = new PaymentEtranzact();
                        PaymentEtranzactLogic pe = new PaymentEtranzactLogic();
                        p = pe.GetModelBy(n => n.Confirmation_No == vModel.InvoiceNumber);
                        if (p != null && p.ConfirmationNo != null)
                        {
                            payment = paymentLogic.GetModelBy(m => m.Invoice_Number == p.CustomerID);
                        }
                        else
                        {

                            payment = paymentLogic.GetModelBy(m => m.Invoice_Number == vModel.InvoiceNumber);

                        } 

                        if (payment != null && payment.Id > 0)
                        {
                            AppliedCourse appliedcourse = new AppliedCourse();
                            AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
                            ApplicantJambDetail applicantjambdetail = new ApplicantJambDetail();
                            ApplicantJambDetailLogic applicantjambdetailLogic = new ApplicantJambDetailLogic();
                            List<DepartmentOption> departmentOptions = new List<DepartmentOption>();

                            appliedcourse = appliedCourseLogic.GetModelBy(n => n.Person_Id == payment.Person.Id);
                            if (appliedcourse != null && appliedcourse.Department.Id > 0)
                            {
                                applicantjambdetail = applicantjambdetailLogic.GetModelBy(x => x.Person_Id == payment.Person.Id);
                                if (applicantjambdetail != null && applicantjambdetail.Person.Id > 0)
                                {
                                    vModel.ApplicantJambDetail = applicantjambdetail;
                                } 

                                vModel.AppliedCourse = appliedcourse;
                                vModel.DepartmentSelectListItem = Utility.PopulateDepartmentSelectListItem(appliedcourse.Programme);
                                vModel.ProgrammeSelectListItem = Utility.PopulateAllProgrammeSelectListItem();

                                if (vModel.DepartmentSelectListItem != null)
                                {
                                    ViewBag.DepartmentId = new SelectList(vModel.DepartmentSelectListItem, VALUE, TEXT, appliedcourse.Department.Id);

                                }
                                else
                                {
                                    ViewBag.DepartmentId = new SelectList(new List<Department>(), ID, NAME);

                                }
                                if (vModel.ProgrammeSelectListItem != null)
                                {

                                    ViewBag.ProgrammeId = new SelectList(vModel.ProgrammeSelectListItem, VALUE, TEXT, appliedcourse.Programme.Id);

                                }
                                else
                                {
                                    ViewBag.ProgrammeId = new SelectList(new List<Programme>(), ID, NAME);

                                }
                                if (vModel.DepartmentOption != null)
                                {
                                    ViewBag.DepartmentOptionId = new SelectList(departmentOptions, ID, NAME, vModel.DepartmentOption.Id);
                                }
                                else
                                {
                                    ViewBag.DepartmentOptionId = new SelectList(departmentOptions, ID, NAME);
                                }

                                vModel.Payment = payment;
                                vModel.Person = payment.Person;
                                vModel.InvoiceNumber = payment.InvoiceNumber;
                                TempData["SupportViewModel"] = viewmodel;
                                return View(vModel);
                            }


                        }
                        else
                        {
                            SetMessage("Invoice does not exist! ", Message.Category.Error);
                            return View(vModel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            return View(vModel);
        }

        [HttpPost]
        public ActionResult UpdateInvoice(SupportViewModel vModel)
        {
            try
            {
                foreach (ModelState modelState in ViewData.ModelState.Values)
                {
                    foreach (ModelError error in modelState.Errors)
                    {
                        error.ErrorMessage.ToString();
                    }
                }

                ModelState.Remove("Person.DateEntered");
                ModelState.Remove("Person.DateOfBirth");
                ModelState.Remove("Person.FirstName");
                ModelState.Remove("Person.LastName");
                ModelState.Remove("Person.MobilePhone");
                ModelState.Remove("Person.Sex.Id");
                ModelState.Remove("Person.Religion.Id");
                ModelState.Remove("Person.LocalGovernment.Id");
                if (vModel.AppliedCourse.Option == null || vModel.AppliedCourse.Option.Id <= 0)
                {
                    ModelState.Remove("AppliedCourse.Option.Id"); 
                }  
                ModelState.Remove("AppliedCourse.ApplicationForm.Id");
                ModelState.Remove("ApplicantJambDetail.Person.Id");
                ModelState.Remove("ApplicantJambDetail.JambRegistrationNumber");


                if (ModelState.IsValid)
                {
                    using (TransactionScope transaction = new TransactionScope())
                    {
                        string operation = "UPDATE";
                        string action = "MODIFY APPLICANT PERSON AND APPLIED COURSE DETAILS";
                        string client = Request.LogonUserIdentity.Name + " (" + HttpContext.Request.UserHostAddress + ")";

                        if (vModel.Person != null && vModel.AppliedCourse != null)
                        {
                            PersonAudit personAudit = new PersonAudit();
                            UserLogic loggeduser = new UserLogic();
                            personAudit.User = loggeduser.GetModelBy(u => u.User_Name == User.Identity.Name);
                            personAudit.Operation = operation;
                            personAudit.Action = action;
                            personAudit.Time = DateTime.Now;
                            personAudit.Client = client;

                            PersonLogic personLogic = new PersonLogic();
                            bool personChanged = personLogic.Modify(vModel.Person, personAudit);

                            AdmissionListAudit AdmissionListAudit = new Model.Model.AdmissionListAudit();
                            AdmissionListAudit.Action = action;
                            AdmissionListAudit.Client = client;
                            AdmissionListAudit.Operation = operation;
                            AdmissionListAudit.Time = DateTime.Now;
                            AdmissionListAudit.User = loggeduser.GetModelBy(u => u.User_Name == User.Identity.Name);

                            AdmissionListLogic admissionListLogic = new AdmissionListLogic();
                            AdmissionList admissionList = new AdmissionList();
                            admissionList = admissionListLogic.GetModelsBy(a => a.APPLICATION_FORM.Person_Id == vModel.Person.Id).LastOrDefault();
                            if (admissionList != null)
                            {
                                admissionList.Deprtment = vModel.AppliedCourse.Department;
                                admissionList.DepartmentOption = vModel.AppliedCourse.Option;

                                admissionListLogic.Modify(admissionList, AdmissionListAudit);
                            }  

                            AppliedCourseAudit appliedCourseAudit = new AppliedCourseAudit();
                            appliedCourseAudit.User = loggeduser.GetModelBy(u => u.User_Name == User.Identity.Name);
                            appliedCourseAudit.Operation = operation;
                            appliedCourseAudit.Action = action;
                            appliedCourseAudit.Time = DateTime.Now;
                            appliedCourseAudit.Client = client;

                            AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
                            vModel.AppliedCourse.Person = vModel.Person;
                            bool appliedCourseChanged = appliedCourseLogic.Modify(vModel.AppliedCourse, appliedCourseAudit);

                            StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                            CourseRegistrationLogic courseRegistrationLogic = new CourseRegistrationLogic();
                            List<StudentLevel> studentLevels = studentLevelLogic.GetModelsBy(s => s.Person_Id == vModel.Person.Id);
                            List<CourseRegistration> courseRegistrations = courseRegistrationLogic.GetModelsBy(c => c.Person_Id == vModel.Person.Id);
                            if (studentLevels.Count >= 0)
                            {
                                for (int i = 0; i < studentLevels.Count; i++)
                                {
                                    if (courseRegistrations.Count > 0)
                                    {
                                        if (courseRegistrations.FirstOrDefault().Department.Id == studentLevels[i].Department.Id && courseRegistrations.FirstOrDefault().Programme.Id == studentLevels[i].Programme.Id)
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            StudentLevel currentStudentLevel = studentLevels[i];
                                            currentStudentLevel.Programme = vModel.AppliedCourse.Programme;
                                            currentStudentLevel.Department = vModel.AppliedCourse.Department;
                                            if (vModel.AppliedCourse.Option != null)
                                            {
                                                currentStudentLevel.DepartmentOption = vModel.AppliedCourse.Option;
                                            }
                                            studentLevelLogic.Modify(currentStudentLevel, currentStudentLevel.Id); 
                                        }
                                    }
                                    if (courseRegistrations.Count == 0)
                                    {
                                        StudentLevel currentStudentLevel = studentLevels[i];
                                        currentStudentLevel.Programme = vModel.AppliedCourse.Programme;
                                        currentStudentLevel.Department = vModel.AppliedCourse.Department;
                                        if (vModel.AppliedCourse.Option != null)
                                        {
                                            currentStudentLevel.DepartmentOption = vModel.AppliedCourse.Option;
                                        }

                                        studentLevelLogic.Modify(currentStudentLevel, currentStudentLevel.Id); 
                                    } 
                                }
                                
                            }

                            if (vModel.AppliedCourse.Department.Id != appliedCourseAudit.OldDepartment.Id)
                            {
                                ApplicationForm appForm = new ApplicationForm();
                                ApplicationFormLogic applicationFormLogic = new ApplicationFormLogic();
                                appForm = applicationFormLogic.GetModelBy(m => m.Person_Id == vModel.Person.Id);
                                if (appForm != null)
                                {
                                    Department department = new Department();
                                    DepartmentLogic departmentLogic = new DepartmentLogic();
                                    department = departmentLogic.GetModelBy(d => d.Department_Id == vModel.AppliedCourse.Department.Id);
                                    vModel.AppliedCourse.Department = department;
                                    //appForm =  applicationFormLogic.SetNextExamNumber(appForm,vModel.AppliedCourse);
                                    applicationFormLogic.Modify(appForm);
                                }
                            }

                            if (vModel.ApplicantJambDetail.JambRegistrationNumber != null && appliedCourseAudit.OldProgramme.Id == 1 && appliedCourseAudit.Programme.Id == 1)
                            {
                                ApplicantJambDetail appDetail = new ApplicantJambDetail();
                                ApplicantJambDetailLogic appJambDetailLogic = new ApplicantJambDetailLogic();
                                appDetail = appJambDetailLogic.GetModelBy(m => m.Person_Id == appliedCourseAudit.AppliedCourse.Person.Id);

                                if (appDetail != null)
                                {
                                    appDetail.JambRegistrationNumber = vModel.ApplicantJambDetail.JambRegistrationNumber;
                                    appJambDetailLogic.Modify(appDetail);
                                }
                                else
                                {
                                    vModel.ApplicantJambDetail.Person = appliedCourseAudit.AppliedCourse.Person;
                                    appJambDetailLogic.Create(vModel.ApplicantJambDetail);
                                }

                            }
                            else if (vModel.ApplicantJambDetail.JambRegistrationNumber != null && appliedCourseAudit.OldProgramme.Id != 1 && appliedCourseAudit.Programme.Id == 1)
                            {
                                ApplicantJambDetailLogic appJambDetailLogic = new ApplicantJambDetailLogic();
                                vModel.ApplicantJambDetail.Person = appliedCourseAudit.AppliedCourse.Person;
                                appJambDetailLogic.Create(vModel.ApplicantJambDetail);

                            }
                            else if (appliedCourseAudit.Programme.Id != 1)
                            {
                                ApplicantJambDetailLogic appJambDetailLogic = new ApplicantJambDetailLogic();
                                Selector = r => r.PERSON.Person_Id == appliedCourseAudit.AppliedCourse.Person.Id;
                                appJambDetailLogic.Delete(Selector);
                            }
                            //here

                            string number = (string)TempData["Invoice/ConfirmationOrder"];
                            Payment payment = new Payment();
                            PaymentLogic paymentLogic = new Business.PaymentLogic();
                            PaymentEtranzact p = new PaymentEtranzact();
                            PaymentEtranzactLogic pe = new PaymentEtranzactLogic();
                            p = pe.GetModelBy(n => n.Confirmation_No == number);
                            if (p != null && p.ConfirmationNo != null)
                            {
                                payment = paymentLogic.GetModelBy(m => m.Invoice_Number == p.CustomerID);
                            }
                            else
                            {

                                payment = paymentLogic.GetModelBy(m => m.Invoice_Number == number);

                            }
                            ApplicationProgrammeFee ApplicationProgrammeFee = new ApplicationProgrammeFee();
                            ApplicationProgrammeFeeLogic ApplicationProgrammeFeeLogic = new ApplicationProgrammeFeeLogic();

                            ApplicationProgrammeFee = ApplicationProgrammeFeeLogic.GetModelBy(z => z.Programme_Id == appliedCourseAudit.Programme.Id && z.Session_Id == 7 && z.Fee_Type_Id == payment.FeeType.Id);


                            //Payment payment = new Payment();
                            //PaymentLogic PaymentLogic = new Business.PaymentLogic();
                            //payment = PaymentLogic.GetModelBy(p => p.Person_Id == appliedCourseAudit.AppliedCourse.Person.Id);
                            if (ApplicationProgrammeFee != null)
                            {
                                payment.FeeType = ApplicationProgrammeFee.FeeType;
                                paymentLogic.Modify(payment); 
                            }
                            

                            transaction.Complete();
                        }
                    }

                    TempData["Message"] = "Record was successfully updated";
                    return RedirectToAction("Index", "Support");
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "System Message :" + ex.Message;
                return RedirectToAction("Index", "Support");
            }

            TempData["Message"] = "Record was not updated! Crosscheck entries and try again";
            return RedirectToAction("Index", "Support");
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

       protected Expression<Func<APPLICANT_JAMB_DETAIL, bool>> Selector { get; set; }
    
       public ActionResult SendSms()
        {
            viewmodel = new SupportViewModel();
            try
            {

            }
            catch (Exception ex)
            {
                 SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
            return View();
        }
        
       public ActionResult ViewAuditReport()
       {
           try
           {
               SupportViewModel supportModel = new SupportViewModel();

               List<PersonAudit> personAudit = new List<PersonAudit>();
               PersonAuditLogic personAuditLogic = new PersonAuditLogic();

               personAudit = personAuditLogic.GetAll();
               supportModel.personAudit = personAudit;
               return View(supportModel);
           }
           catch (Exception ex)
           {
               
               throw;
           }
       }

       public ActionResult ViewAuditReportDetails(long Id)
       {
           try
           {
               SupportViewModel supportModel = new SupportViewModel();
               if (Id > 0)
               {
                   PersonAudit personAudit = new PersonAudit();
                   PersonAuditLogic personAuditLogic = new PersonAuditLogic();

                   personAudit = personAuditLogic.GetModelBy(p => p.Person_Audit_Id == Id);
                   supportModel.personAuditDetails = personAudit;

                   

                   return View(supportModel);
               }

               return View(supportModel);
           }
           catch (Exception ex)
           {
               
               throw;
           }
       }
    
       public ActionResult CorrectOlevel()
       {
           viewmodel = new SupportViewModel();
           try
           {
              
               PopulateOlevelDropdowns(viewmodel);
           }
           catch (Exception ex)
           {
               
               SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
          
           }
           return View(viewmodel);
       }
      
       [HttpPost]
       public ActionResult CorrectOlevel(SupportViewModel supportModel)
       
       {
          
           try
           {
               if (ModelState.IsValid)
               {
                   if (supportModel.InvoiceNumber != null)
                   {
                       Payment payment = new Payment();
                       PaymentLogic paymentLogic = new PaymentLogic();
                       PaymentEtranzact p = new PaymentEtranzact();
                       PaymentEtranzactLogic pe = new PaymentEtranzactLogic();
                       p = pe.GetModelBy(n => n.Confirmation_No == supportModel.InvoiceNumber);
                       if (p != null && p.ConfirmationNo != null)
                       {
                           payment = paymentLogic.GetModelBy(m => m.Invoice_Number == p.CustomerID);
                       }
                       else
                       {

                           payment = paymentLogic.GetModelBy(m => m.Invoice_Number == supportModel.InvoiceNumber);

                       }




                       if (payment != null && payment.Id > 0)
                       {
                           AppliedCourse appliedcourse = new AppliedCourse();
                           AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
                           OLevelResultDetail resultDetail = new OLevelResultDetail();
                           
                           appliedcourse = appliedCourseLogic.GetModelBy(n => n.Person_Id == payment.Person.Id);
                           if (appliedcourse != null && appliedcourse.Department.Id > 0)
                           {
                               ApplicationForm applicationform = supportModel.GetApplicationFormBy(payment.Person, payment);
                               if (applicationform != null && applicationform.Id > 0)
                               {
                                   

                                   OLevelResult olevelResult = new OLevelResult();
                                   OLevelResultLogic olevelResultLogic = new OLevelResultLogic();
                                   olevelResult = olevelResultLogic.GetModelBy(m => m.Person_Id == payment.Person.Id && m.O_Level_Exam_Sitting_Id == 1 && m.Application_Form_Id != null);
                                    if (olevelResult != null)
                                    {
                                        List<OLevelResultDetail> olevelResultdetails = new List<OLevelResultDetail>();
                                        OLevelResultDetailLogic olevelResultdetailsLogic = new OLevelResultDetailLogic();

                                        olevelResultdetails = olevelResultdetailsLogic.GetModelsBy(m => m.Applicant_O_Level_Result_Id == olevelResult.Id);
                                        supportModel.FirstSittingOLevelResult = olevelResult;
                                        supportModel.FirstSittingOLevelResultDetails = olevelResultdetails;
                                    }

                                    olevelResult = olevelResultLogic.GetModelBy(m => m.Person_Id == payment.Person.Id && m.O_Level_Exam_Sitting_Id == 2 && m.Application_Form_Id != null);
                                    if (olevelResult != null)
                                    {
                                        List<OLevelResultDetail> olevelResultdetails = new List<OLevelResultDetail>();
                                        OLevelResultDetailLogic olevelResultdetailsLogic = new OLevelResultDetailLogic();

                                        olevelResultdetails = olevelResultdetailsLogic.GetModelsBy(m => m.Applicant_O_Level_Result_Id == olevelResult.Id);
                                        supportModel.SecondSittingOLevelResult = olevelResult;
                                        supportModel.SecondSittingOLevelResultDetails = olevelResultdetails;
                                    }
                            }

                             
                               supportModel.AppliedCourse = appliedcourse;                              
                               supportModel.Payment = payment;
                               supportModel.Person = payment.Person;
                               supportModel.InvoiceNumber = payment.InvoiceNumber;
                               //SetSelectedSittingSubjectAndGrade(supportModel);
                               PopulateOlevelDropdowns(supportModel);


                               TempData["SupportViewModel"] = supportModel;
                               return View(supportModel);
                           }


                       }
                       else
                       {
                           SetMessage("Invoice does not exist! ", Message.Category.Error);
                           return View(supportModel);
                       }
                   }
               }
           }
           catch (Exception ex)
           {

               SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);

           }
          
           return View(viewmodel);
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

                       //if (string.IsNullOrEmpty(oLevelResult.ExamNumber) || oLevelResult.Type == null || oLevelResult.Type.Id <= 0 || oLevelResult.ExamYear <= 0)
                       //{
                       //    SetMessage("Header Information not set for " + sitting + " O-Level Result Details! ", Message.Category.Error);
                       //    return true;
                       //}
                   }
               }

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

                   //else if (oLevelResultDetail.Grade.Id > 0 && oLevelResultDetail.Subject.Id <= 0)
                   //{
                   //    SetMessage("No Grade specified for " + subject.Name.ToUpper() + " for " + sitting + "! Please modify.", Message.Category.Error);
                   //    return true;
                   //}
                   //else if (oLevelResultDetail.Subject.Id <= 0 && oLevelResultDetail.Grade.Id > 0)
                   //{
                   //    SetMessage("No Subject specified for " + grade.Name.ToUpper() + " for " + sitting + "! Please modify.", Message.Category.Error);
                   //    return true;
                   //}
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

       private void PopulateOlevelDropdowns(SupportViewModel viewmodel)
       {
           try
           {
               
               ViewBag.FirstSittingOLevelTypeId = viewmodel.OLevelTypeSelectList;
               ViewBag.SecondSittingOLevelTypeId = viewmodel.OLevelTypeSelectList;
               ViewBag.FirstSittingExamYearId = viewmodel.ExamYearSelectList;
               ViewBag.SecondSittingExamYearId = viewmodel.ExamYearSelectList;
               ViewBag.ResultGradeId = viewmodel.ResultGradeSelectList;

               SetSelectedSittingSubjectAndGrade(viewmodel);
           }
           catch (Exception ex)
           {
               
               throw;
           }
       }

       private void SetDefaultSelectedSittingSubjectAndGrade(SupportViewModel viewModel)
       {
           try
           {
               if (viewModel != null && viewModel.FirstSittingOLevelResultDetails != null)
               {
                   for (int i = 0; i < 9; i++)
                   {
                       ViewData["FirstSittingOLevelSubjectId" + i] = viewModel.OLevelSubjectSelectList;
                       ViewData["FirstSittingOLevelGradeId" + i] = viewModel.OLevelGradeSelectList;
                   }
               }

             
           }
           catch (Exception ex)
           {
               SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
           }
       }

       private void SetSelectedSittingSubjectAndGrade(SupportViewModel existingViewModel)
       {
           try
           {
               if (existingViewModel != null && existingViewModel.FirstSittingOLevelResultDetails != null && existingViewModel.FirstSittingOLevelResultDetails.Count > 0)
               {
                   int i = 0;
                   foreach (OLevelResultDetail firstSittingOLevelResultDetail in existingViewModel.FirstSittingOLevelResultDetails)
                   {
                       if (firstSittingOLevelResultDetail.Subject != null && firstSittingOLevelResultDetail.Grade != null)
                       {
                           ViewData["FirstSittingOLevelSubjectId" + i] = new SelectList(existingViewModel.OLevelSubjectSelectList, VALUE, TEXT, firstSittingOLevelResultDetail.Subject.Id);
                           ViewData["FirstSittingOLevelGradeId" + i] = new SelectList(existingViewModel.OLevelGradeSelectList, VALUE, TEXT, firstSittingOLevelResultDetail.Grade.Id);
                       }
                       else
                       {
                           ViewData["FirstSittingOLevelSubjectId" + i] = new SelectList(existingViewModel.OLevelSubjectSelectList, VALUE, TEXT, 0);
                           ViewData["FirstSittingOLevelGradeId" + i] = new SelectList(existingViewModel.OLevelGradeSelectList, VALUE, TEXT, 0);
                       }

                       i++;
                   }
               }

               if (existingViewModel != null && existingViewModel.SecondSittingOLevelResultDetails != null && existingViewModel.SecondSittingOLevelResultDetails.Count > 0)
               {
                   int i = 0;
                   foreach (OLevelResultDetail secondSittingOLevelResultDetail in existingViewModel.SecondSittingOLevelResultDetails)
                   {
                       if (secondSittingOLevelResultDetail.Subject != null && secondSittingOLevelResultDetail.Grade != null)
                       {
                           ViewData["SecondSittingOLevelSubjectId" + i] = new SelectList(existingViewModel.OLevelSubjectSelectList, VALUE, TEXT, secondSittingOLevelResultDetail.Subject.Id);
                           ViewData["SecondSittingOLevelGradeId" + i] = new SelectList(existingViewModel.OLevelGradeSelectList, VALUE, TEXT, secondSittingOLevelResultDetail.Grade.Id);
                       }
                       else
                       {
                           ViewData["SecondSittingOLevelSubjectId" + i] = new SelectList(existingViewModel.OLevelSubjectSelectList, VALUE, TEXT, 0);
                           ViewData["SecondSittingOLevelGradeId" + i] = new SelectList(existingViewModel.OLevelGradeSelectList, VALUE, TEXT, 0);
                       }

                       i++;
                   }
               }
           }
           catch (Exception ex)
           {
               SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
           }
       }
       [HttpPost]
       public ActionResult UpdateOlevel(SupportViewModel viewModel)
       {
          
           try
           {
               ModelState.Remove("Person.DateEntered");
               ModelState.Remove("Person.DateOfBirth");
               ModelState.Remove("Person.FirstName");
               ModelState.Remove("Person.LastName");
               ModelState.Remove("Person.MobilePhone");
               ModelState.Remove("Person.Sex.Id");
               ModelState.Remove("Person.Religion.Id");
               ModelState.Remove("Person.LocalGovernment.Id");
               ModelState.Remove("AppliedCourse.Option.Id");
               ModelState.Remove("AppliedCourse.ApplicationForm.Id");
               ModelState.Remove("ApplicantJambDetail.Person.Id");
               ModelState.Remove("ApplicationForm.Id");
               ModelState.Remove("ApplicationForm.Person.Id");
               
               foreach (ModelState modelState in ViewData.ModelState.Values)
               {
                   foreach (ModelError error in modelState.Errors)
                   {
                       //DoSomethingWith(error);
                       SetMessage(error.ErrorMessage, Message.Category.Information);
                   }
               }
              
                   if (InvalidOlevelSubjectOrGrade(viewModel.FirstSittingOLevelResultDetails, viewModel.OLevelSubjects, viewModel.OLevelGrades, FIRST_SITTING))
                   {
                       TempData["Message"] = "O-level Subjects contains duplicates";
                       return RedirectToAction("CorrectOlevel");
                   }

                   if (InvalidNumberOfOlevelSubject(viewModel.FirstSittingOLevelResultDetails, viewModel.SecondSittingOLevelResultDetails))
                   {
                       TempData["Message"] = "Invalid number of O-level Subjects";
                       return RedirectToAction("CorrectOlevel");
                   }
                   if (InvalidOlevelSubjectOrGrade(viewModel.SecondSittingOLevelResultDetails, viewModel.OLevelSubjects, viewModel.OLevelGrades, SECOND_SITTING))
                   {
                       TempData["Message"] = "Second sitting O-level Subjects contains duplicates";
                       return RedirectToAction("CorrectOlevel");
                   }

                   using (TransactionScope transaction = new TransactionScope())
                   {
                       OLevelResultLogic oLevelResultLogic = new OLevelResultLogic();
                       OLevelResultDetailLogic oLevelResultDetailLogic = new OLevelResultDetailLogic();
                       
                       ApplicationFormLogic appFormLogic = new ApplicationFormLogic();
                       viewModel.ApplicationForm = appFormLogic.GetBy(viewModel.AppliedCourse.ApplicationForm.Id);

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
                       ApplicantLogic applicantLogic = new ApplicantLogic();
                       AppliedCourse appliedCourse = appliedCourseLogic.GetBy(viewModel.ApplicationForm.Person);

                       //Set department to admitted department since it might vary
                       AdmissionList admissionList = new AdmissionList();
                       admissionList = admissionListLogic.GetBy(viewModel.ApplicationForm.Person);
                       if (admissionList != null)
                       {
                           appliedCourse.Department = admissionList.Deprtment;

                           if (appliedCourse == null)
                           {
                               SetMessage("Your O-Level was successfully verified, but could not be cleared because no Applied Course was not found for your application", Message.Category.Error);
                               return RedirectToAction("CorrectOlevel");
                           }

                           //set reject reason if exist
                           ApplicantStatus.Status newStatus;
                           AdmissionCriteriaLogic admissionCriteriaLogic = new AdmissionCriteriaLogic();
                           string rejectReason = admissionCriteriaLogic.EvaluateApplication(appliedCourse);
                           if (string.IsNullOrWhiteSpace(rejectReason))
                           {
                               
                               Abundance_Nk.Model.Model.Applicant applicant = new Model.Model.Applicant();
                               applicant = applicantLogic.GetModelBy(p => p.Person_Id == appliedCourse.Person.Id);
                               newStatus = (ApplicantStatus.Status)applicant.Status.Id;
                               //newStatus = ApplicantStatus.Status.ClearedAndAccepted;
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
                           //ApplicantLogic applicantLogic = new ApplicantLogic();
                           applicantLogic.UpdateStatus(viewModel.ApplicationForm, newStatus);


                           //save clearance metadata
                           ApplicantClearance applicantClearance = new ApplicantClearance();
                           ApplicantClearanceLogic applicantClearanceLogic = new ApplicantClearanceLogic();
                        GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();

                        applicantClearance = applicantClearanceLogic.GetBy(viewModel.ApplicationForm);
                           if (applicantClearance == null)
                           {
                               applicantClearance = new ApplicantClearance(); 
                               applicantClearance.ApplicationForm = viewModel.ApplicationForm;
                               applicantClearance.Cleared = string.IsNullOrWhiteSpace(viewModel.ApplicationForm.RejectReason) ? true : false;
                               applicantClearance.DateCleared = DateTime.Now;
                               applicantClearanceLogic.Create(applicantClearance);

                            string Action = "CREATE";
                            string Operation = "Cleared the Applicant " + applicantClearance.ApplicationForm.Number;
                            string Table = "Applicant Clearance Table";
                            generalAuditLogic.CreateGeneralAudit(Action, Operation, Table);

                           }
                           else
                           {
                               applicantClearance.Cleared = string.IsNullOrWhiteSpace(viewModel.ApplicationForm.RejectReason) ? true : false;
                               applicantClearance.DateCleared = DateTime.Now;
                               applicantClearanceLogic.Modify(applicantClearance);
                            string Action = "Modify";
                            string Operation = "Cleared the Applicant " + applicantClearance.ApplicationForm.Number;
                            string Table = "Applicant Clearance Table";
                            generalAuditLogic.CreateGeneralAudit(Action, Operation, Table);

                        }

                    }
                       transaction.Complete();
                   }
               }
               
          
           catch (Exception ex)
           {

               SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);

           }
           TempData["Message"] = "O-Level result updated";
           return RedirectToAction("CorrectOlevel");
       }

       private void ModifyOlevelResult(OLevelResult oLevelResult, List<OLevelResultDetail> oLevelResultDetails)
       {
           try
           {
               OlevelResultdDetailsAudit olevelResultdDetailsAudit = new OlevelResultdDetailsAudit();
                olevelResultdDetailsAudit.Action = "Modify";
                olevelResultdDetailsAudit.Operation = "Modify O level (Support Controller)";
                olevelResultdDetailsAudit.Client =  Request.LogonUserIdentity.Name + " (" + HttpContext.Request.UserHostAddress + ")";
                UserLogic loggeduser = new UserLogic();
                olevelResultdDetailsAudit.User = loggeduser.GetModelBy(u => u.User_Name == User.Identity.Name);

               OLevelResultLogic oLevelResultLogic = new OLevelResultLogic();
              
               OLevelResultDetailLogic oLevelResultDetailLogic = new OLevelResultDetailLogic();
               if (oLevelResult != null && oLevelResult.ExamNumber != null && oLevelResult.Type != null && oLevelResult.ExamYear > 0)
               {
                   if (oLevelResult != null && oLevelResult.Id > 0)
                   {

                       oLevelResultDetailLogic.DeleteBy(oLevelResult,olevelResultdDetailsAudit);
                       oLevelResultLogic.Modify(oLevelResult);
                   }
                   else
                   {
                      
                       OLevelResult newOLevelResult = oLevelResultLogic.Create(oLevelResult);
                       oLevelResult.Id = newOLevelResult.Id;
                   }

                   if (oLevelResultDetails != null && oLevelResultDetails.Count > 0)
                   {
                       List<OLevelResultDetail> olevelResultDetails = oLevelResultDetails.Where(m => m.Grade != null && m.Grade.Id > 0 && m.Subject != null && m.Subject.Id > 0).ToList();
                       foreach (OLevelResultDetail oLevelResultDetail in olevelResultDetails)
                       {
                           oLevelResultDetail.Header = oLevelResult;
                           oLevelResultDetailLogic.Create(oLevelResultDetail);
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
    
        [HttpGet]
        public ActionResult PinRetrieval()
        {

            return View();
        }
        [HttpPost]
        public ActionResult PinRetrieval(SupportViewModel viewmodel)
        {
            ApplicationFormLogic applicationFormLogic = new ApplicationFormLogic();
            PersonLogic personLogic = new PersonLogic();

            SupportViewModel supportViewM = new SupportViewModel();
            PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
            PaymentLogic paymentLogic = new PaymentLogic();

            supportViewM.ApplicationForm = applicationFormLogic.GetModelBy(p => p.Application_Form_Number == viewmodel.ApplicationForm.Number);
            Person person = new Person();
            long personId = supportViewM.ApplicationForm.Person.Id;
            Payment payment = new Payment();
            payment = paymentLogic.GetModelBy(p => p.Person_Id == personId);

            PaymentEtranzact paymentEtranzact = new PaymentEtranzact();
            paymentEtranzact = paymentEtranzactLogic.GetModelBy(p => p.Payment_Id == payment.Id);

            ScratchCard scratchCard = new ScratchCard();
            ScratchCardLogic scratchCardLogic = new ScratchCardLogic();
            scratchCard = scratchCardLogic.GetModelBy(p => p.Person_Id == personId);

            string confrimationNumber = paymentEtranzact.ConfirmationNo;
            decimal? transactionAmount = paymentEtranzact.TransactionAmount;
            string invoiceNumber = paymentEtranzact.CustomerID;

            ViewBag.scratchCardPin = scratchCard;
            ViewBag.paymentEtransact = paymentEtranzact;
            return View();
        }

        public ActionResult ResetInvoice()
        {
            try
            {

            }
            catch (Exception ex)
            {
                 SetMessage(ex.Message, Message.Category.Warning);
                 
            }
            return View();
        }

        [HttpPost]
        public ActionResult ResetInvoice(SupportViewModel viewmodel)
        {
            try
            {
                return View(viewmodel);
                Payment payment = new Payment();
                PaymentLogic paymentLogic = new PaymentLogic();
                payment = paymentLogic.GetModelBy(n => n.Invoice_Number == viewmodel.InvoiceNumber && n.Fee_Type_Id != 1);
                if (payment != null && payment.Id != null)
                {
                    RemitaPayment remitaPayment = new RemitaPayment();
                    RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();

                    remitaPayment = remitaPaymentLogic.GetBy(payment.Id);
                    if (remitaPayment != null)
                    {
                        if (remitaPayment.Status.Contains("01:"))
                        {
                            SetMessage("This invoice has been paid for and can't be deleted!", Message.Category.Warning);
                            return View(viewmodel);
                        }
                        else
                        {
                            ApplicationFormLogic appFormLogic = new ApplicationFormLogic();
                            viewmodel.ApplicationForm = appFormLogic.GetBy(payment.Person);
                            ApplicantStatus.Status newStatus = new ApplicantStatus.Status();
                            if (payment.FeeType.Id == 2 || payment.FeeType.Id == 9)
                            {
                                newStatus = ApplicantStatus.Status.SubmittedApplicationForm;
                            }
                            else if (payment.FeeType.Id == 3)
                            {
                                newStatus = ApplicantStatus.Status.GeneratedAcceptanceReceipt;
                            }


                            OnlinePayment onlinePayment = new OnlinePayment();
                            OnlinePaymentLogic onlinePaymentLogic = new OnlinePaymentLogic();
                            onlinePayment = onlinePaymentLogic.GetBy(payment.Id);
                            if (onlinePayment != null)
                            {
                                using (TransactionScope scope = new TransactionScope())
                                {
                                    onlinePaymentLogic.DeleteBy(onlinePayment.Payment.Id);
                                    remitaPaymentLogic.DeleteBy(remitaPayment.payment.Id);
                                    paymentLogic.DeleteBy(payment.Id);
                                   
                                    ApplicantLogic applicantLogic = new ApplicantLogic();
                                    applicantLogic.UpdateStatus(viewmodel.ApplicationForm, newStatus);

                                    scope.Complete();
                                    SetMessage("The " + payment.FeeType.Name + "  invoice  " + payment.InvoiceNumber + "  for " + payment.Person.FullName + " has been deleted! Please log into your profile and generate a new one", Message.Category.Information);
                                    GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();

                                    string Action = "DELETE";
                                    string Operation = "Deleted the invoice " + payment.InvoiceNumber;
                                    string Table = "Payment Table";
                                    generalAuditLogic.CreateGeneralAudit(Action, Operation, Table);

                                    return View(viewmodel);
                                }
                              
                            }
                        
                        }
                    }
                    else
                    {
                        SetMessage("Sorry, this invoice can't be deleted!", Message.Category.Warning);
                        return View(viewmodel);
                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage(ex.Message, Message.Category.Warning);
            }

            return View(viewmodel);
        }
        public ActionResult UpdateStudentData()
        {
            viewmodel = new SupportViewModel();
            try
            {
                //ViewBag.DepartmentId = new SelectList(new List<Department>(), ID, NAME);
                //ViewBag.ProgrammeId = Utility.PopulateAllProgrammeSelectListItem();
                              
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
            return View();
        }
   
        [HttpPost]
        public ActionResult UpdateStudentData(SupportViewModel viewmodel)
        {
            try
            {
                if (viewmodel.ApplicationForm.Number != null)
                {
                    StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                    StudentLogic studentLogic = new StudentLogic();
                    ApplicationFormLogic applicatinformLogic = new ApplicationFormLogic();
                    AdmissionListLogic admissionListLogic = new AdmissionListLogic();
                    AdmissionList admissionList = new AdmissionList();
                    viewmodel.ApplicationForm = applicatinformLogic.GetModelBy(a => a.Application_Form_Number == viewmodel.ApplicationForm.Number);
                    admissionList = admissionListLogic.GetModelBy(p => p.Application_Form_Id == viewmodel.ApplicationForm.Id);
                    using (TransactionScope scope = new TransactionScope())
                    {
                      
                
                        
                        viewmodel.studentLevel = studentLevelLogic.GetModelsBy(s => s.Person_Id == viewmodel.ApplicationForm.Person.Id).LastOrDefault();

                        // clear course registration data and assign new matric number 
                        CurrentSessionSemesterLogic currentSessionSemesterLogic = new CurrentSessionSemesterLogic();
                        viewmodel.CurrentSessionSemester = currentSessionSemesterLogic.GetCurrentSessionSemester();

                        CourseRegistrationLogic courseRegistrationLogic = new CourseRegistrationLogic();
                        CourseRegistrationDetailLogic courseRegistrationDetailLogic = new CourseRegistrationDetailLogic();
                        CourseRegistration courseRegistration = courseRegistrationLogic.GetBy(viewmodel.studentLevel.Student, viewmodel.studentLevel.Level, viewmodel.studentLevel.Programme, viewmodel.studentLevel.Department, viewmodel.CurrentSessionSemester.SessionSemester.Session);
                        if (courseRegistration != null && courseRegistration.Id > 0)
                        {
                            Expression<Func<STUDENT_COURSE_REGISTRATION_DETAIL, bool>> selector = cr => cr.Student_Course_Registration_Id == courseRegistration.Id;
                            if (courseRegistrationDetailLogic.Delete(selector))
                            {
                                Expression<Func<STUDENT_COURSE_REGISTRATION, bool>> deleteSelector = cr => cr.Student_Course_Registration_Id == courseRegistration.Id;
                                courseRegistrationLogic.Delete(deleteSelector);
                                GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();

                                string Action = "DELETE";
                                string Operation = "Deleted course registration for " + courseRegistration.Student.MatricNumber + " course " + courseRegistration.Name + " in the session" + courseRegistration.Session.Name;
                                string Table = "Course Registration Table";
                                generalAuditLogic.CreateGeneralAudit(Action, Operation, Table);


                            }
                            else
                            {
                                Expression<Func<STUDENT_COURSE_REGISTRATION, bool>> deleteSelector = cr => cr.Student_Course_Registration_Id == courseRegistration.Id;
                                courseRegistrationLogic.Delete(deleteSelector);

                                scope.Complete();
                            }
                        }


                        Expression<Func<STUDENT_LEVEL, bool>> deleteStudentLevelSelector = sl => sl.Person_Id == viewmodel.ApplicationForm.Person.Id;
                        List<StudentLevel> studentItems = studentLevelLogic.GetModelsBy(deleteStudentLevelSelector);

                        if (studentLevelLogic.Delete(deleteStudentLevelSelector))
                        {
                            foreach (StudentLevel studentItem in studentItems)
                            {
                                CheckStudentSponsor(studentItem);
                                CheckStudentFinanceInformation(studentItem);
                                CheckStudentAcademicInformation(studentItem);
                                CheckStudentResultDetails(studentItem);
                            }
                      
                            Expression<Func<STUDENT, bool>> deleteStudentSelector = sl => sl.Person_Id == viewmodel.ApplicationForm.Person.Id;
                            if (studentLogic.Delete(deleteStudentSelector))
                            {
                                ApplicantLogic applicantLogic = new ApplicantLogic();
                                ApplicationFormView applicant = applicantLogic.GetBy(viewmodel.ApplicationForm.Id);
                                if (applicant != null)
                                {
                                    bool matricNoAssigned = studentLogic.AssignMatricNumber(applicant);
                                    if (matricNoAssigned)
                                    {
                                        scope.Complete();
                                        SetMessage("Student was successfully reset ", Message.Category.Information);
                                    }
                                }

                            }
                        }




                    }

                    }
                
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

           
            return View(viewmodel);
        }
        private static void CheckStudentSponsor(StudentLevel studentItem)
        {
            StudentSponsorLogic studentSponsorLogic = new StudentSponsorLogic();
            StudentSponsor studentSponsor = studentSponsorLogic.GetModelBy(ss => ss.Person_Id == studentItem.Student.Id);
            if (studentSponsor != null)
            {
                Expression<Func<STUDENT_SPONSOR, bool>> deleteStudentSponsorSelector = ss => ss.Person_Id == studentItem.Student.Id;
                studentSponsorLogic.Delete(deleteStudentSponsorSelector);
            }
        }
        private static void CheckStudentFinanceInformation(StudentLevel studentItem)
        {
            StudentFinanceInformationLogic studentFinanceInformationLogic = new StudentFinanceInformationLogic();
            StudentFinanceInformation studentFinanceInformation = studentFinanceInformationLogic.GetModelBy(ss => ss.Person_Id == studentItem.Student.Id);
            if (studentFinanceInformation != null)
            {
                Expression<Func<STUDENT_FINANCE_INFORMATION, bool>> deleteStudentFinanceInfoSelector = sfi => sfi.Person_Id == studentItem.Student.Id;
                studentFinanceInformationLogic.Delete(deleteStudentFinanceInfoSelector);
            }
        }
        private static void CheckStudentAcademicInformation(StudentLevel studentItem)
        {
            StudentAcademicInformationLogic studentAcademicInformationLogic = new StudentAcademicInformationLogic();
            StudentAcademicInformation studentAcademicInformation = studentAcademicInformationLogic.GetModelBy(ss => ss.Person_Id == studentItem.Student.Id);
            if (studentAcademicInformation != null)
            {
                Expression<Func<STUDENT_ACADEMIC_INFORMATION, bool>> deleteStudentAcademicInfoSelector = sai => sai.Person_Id == studentItem.Student.Id;
                studentAcademicInformationLogic.Delete(deleteStudentAcademicInfoSelector);
            }
        }
        private static void CheckStudentResultDetails(StudentLevel studentItem)
        {
            StudentResultDetailLogic studentResultDetailLogic = new StudentResultDetailLogic();
            List<StudentResultDetail> studentResultDetails = studentResultDetailLogic.GetModelsBy(srd => srd.Person_Id == studentItem.Student.Id);
            if (studentResultDetails.Count != 0)
            {
                Expression<Func<STUDENT_RESULT_DETAIL, bool>> deleteStudentResultDetailSelector = srd => srd.Person_Id == studentItem.Student.Id;
                studentResultDetailLogic.Delete(deleteStudentResultDetailSelector);
            }
        }
        private List<CourseRegistrationDetail> GetRegisteredCourse(CourseRegistration courseRegistration, List<Course> courses, Semester semester, List<CourseRegistrationDetail> registeredCourseDetails, CourseMode courseMode)
        {
            try
            {
                List<CourseRegistrationDetail> courseRegistrationDetails = null;
                if (registeredCourseDetails != null && registeredCourseDetails.Count > 0)
                {
                    if (courses != null && courses.Count > 0)
                    {
                        courseRegistrationDetails = new List<CourseRegistrationDetail>();
                        foreach (Course course in courses)
                        {
                            CourseRegistrationDetail registeredCourseDetail = registeredCourseDetails.Where(c => c.Course.Id == course.Id && c.Mode.Id == courseMode.Id).SingleOrDefault();
                            if (registeredCourseDetail != null && registeredCourseDetail.Id > 0)
                            {
                                registeredCourseDetail.Course.IsRegistered = true;
                                courseRegistrationDetails.Add(registeredCourseDetail);
                            }
                            else
                            {
                                CourseRegistrationDetail courseRegistrationDetail = new CourseRegistrationDetail();

                                courseRegistrationDetail.Course = course;
                                courseRegistrationDetail.Semester = semester;
                                courseRegistrationDetail.Course.IsRegistered = false;
                                //courseRegistrationDetail.Mode = new CourseMode() { Id = 1 };

                                courseRegistrationDetail.Mode = courseMode;
                                courseRegistrationDetail.CourseRegistration = courseRegistration;

                                courseRegistrationDetails.Add(courseRegistrationDetail);
                            }
                        }
                    }
                }

                return courseRegistrationDetails;
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        [AllowAnonymous]
        public ActionResult CheckMatricNumberDuplicate(string matric)
        {
           
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult CheckMatricNumberDuplicate(SupportViewModel viewmodel)
        {           
            try
            {
                StudentLogic studentLogic = new StudentLogic();
               // studentLogic.CheckMatricNumberDuplicate(viewmodel.Pin);
               
            }
            catch (Exception ex)
            {
                SetMessage(ex.Message, Message.Category.Information);
            }
            return View();
        }

        public ActionResult ResetStep()
        {
            try
            {
                viewmodel = new SupportViewModel();
                ViewBag.StatusId = viewmodel.StatusSelectListItem;

            }
            catch (Exception ex)
            {
                SetMessage(ex.Message, Message.Category.Warning);

            }
            return View(viewmodel);
        }
        [HttpPost]
        public ActionResult ResetStep(SupportViewModel viewmodel)
        {
            try
            {
                if (viewmodel.ApplicationForm.Number != null)
                {
                    ApplicantLogic applicantLogic = new ApplicantLogic();
                    viewmodel.Applicants = applicantLogic.GetApplicantsBy(viewmodel.ApplicationForm.Number);

                    if (viewmodel.Applicants.Status != null)
                    {
                        ViewBag.StatusId = new SelectList(viewmodel.StatusSelectListItem, VALUE, TEXT, viewmodel.Applicants.Status.Id);
                    }
                    else
                    {
                        ViewBag.StatusId = viewmodel.StatusSelectListItem;

                    }


                }
            }
            catch (Exception ex)
            {
                SetMessage(ex.Message, Message.Category.Warning);

            }
            return View(viewmodel);
        }
        [HttpPost]
        public ActionResult UpdateStep(SupportViewModel viewmodel)
        {
            try
            {
                ApplicantLogic applicantLogic = new ApplicantLogic();
                ApplicantStatus.Status status = (ApplicantStatus.Status)viewmodel.Applicants.Status.Id;
                applicantLogic.UpdateStatus(viewmodel.Applicants.ApplicationForm, status);
                GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();

                string Action = "UPDATED STEP";
                string Operation = "Updated step for " + viewmodel.Applicants.ApplicationForm.Number + " to status " + status;
                string Table = "Applicant Status";
                generalAuditLogic.CreateGeneralAudit(Action, Operation, Table);


            }
            catch (Exception ex)
            {
                SetMessage(ex.Message, Message.Category.Warning);
            }
            SetMessage("Updated Successfully", Message.Category.Information);
            return RedirectToAction("ResetStep");
        }

        public ActionResult Correction()
        {
            try
            {
                viewmodel = new SupportViewModel();
                viewmodel.studentModel = new Model.Model.Student();
                ViewBag.Level = viewmodel.LevelSelectList;
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred: " + ex.Message, Message.Category.Error);
            }
            return View(viewmodel);
        }

        [HttpPost]
        public ActionResult Correction(SupportViewModel viewmodel)
        {

            Person person = new Person();
            StudentLogic studentLogic = new StudentLogic();
            PersonLogic personLogic = new PersonLogic();
            StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
            StudentLevel studentLevel = new StudentLevel();
            LevelLogic levelLogic = new LevelLogic();
            DepartmentLogic departmentLogic = new DepartmentLogic();
            ProgrammeLogic programmeLogic = new ProgrammeLogic();
            Programme programme = new Programme();
            List<Department> departments = new List<Department>();
            DepartmentLogic deparmentLogic = new DepartmentLogic();
            List<DepartmentOption> departmentOptions = new List<DepartmentOption>();

           // Model.Model.Session session = new Session(){Id = 7};

            try
            {
                List<Abundance_Nk.Model.Model.Student> studnetList = studentLogic.GetModelsBy(p => p.Matric_Number == viewmodel.studentModel.MatricNumber);
                int count = studnetList.Count;
                if (count == 1)
                {
                    int studentModelId = (int)studentLogic.GetModelBy(p => p.Matric_Number == viewmodel.studentModel.MatricNumber).Id;
                    person = personLogic.GetModelBy(p => p.Person_Id == studentModelId);
                    viewmodel.studentModel.MatricNumber = viewmodel.studentModel.MatricNumber;
                    viewmodel.studentModel.LastName = person.LastName;
                    viewmodel.studentModel.FirstName = person.FirstName;
                    viewmodel.studentModel.OtherName = person.OtherName;
                    viewmodel.studentModel.Id = person.Id;
                    //studentLevel = studentLevelLogic.GetBy(viewmodel.studentModel.MatricNumber);

                    Model.Model.Student student = studnetList[0];

                    studentLevel = studentLevelLogic.GetModelsBy(s =>  s.Person_Id == student.Id).LastOrDefault();

                    if (studentLevel != null)
                    {
                        programme = programmeLogic.GetModelBy(p => p.Programme_Id == studentLevel.Programme.Id);
                        ViewBag.Programme = new SelectList(programmeLogic.GetAll(), ID, NAME, studentLevel.Programme.Id);
                        departments = deparmentLogic.GetBy(programme);
                        ViewBag.Department = new SelectList(departments, ID, NAME, studentLevel.Department.Id);
                        if (studentLevel.DepartmentOption != null)
                        {
                            ViewBag.DepartmentOption = new SelectList(departmentOptions, ID, NAME, studentLevel.DepartmentOption.Id);
                        }
                        else
                        {
                            ViewBag.DepartmentOption = new SelectList(departmentOptions, ID, NAME);
                        }

                        ViewBag.Level = new SelectList(levelLogic.GetAll(), ID, NAME, studentLevel.Level.Id);
                        viewmodel.DepartmentList = departments;
                        viewmodel.ProgrammeList = programmeLogic.GetAll();
                        //studentLevel = studentLevelLogic.GetBy(viewmodel.studentModel.MatricNumber);
                        viewmodel.Session = studentLevel.Session;
                    }

                    else
                    {
                        SetMessage("No Student Level Found", Message.Category.Error);
                        return View("Correction", new SupportViewModel());

                    }
                }
                else if (count < 1)
                {
                    SetMessage("Matric Number does not exist", Message.Category.Error);
                    return View("Correction", new SupportViewModel());
                }
                else if (count > 1)
                {
                    SetMessage("Duplicate Matriculation Number", Message.Category.Error);
                    return View("Correction", new SupportViewModel());
                }

            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred: " + ex.Message, Message.Category.Error);
            }

            TempData["studentModel"] = viewmodel;
            return View("Correction", viewmodel);
        }

        [HttpPost]
        public ActionResult SaveName(SupportViewModel viewmodel)
        {
            Person person = new Person();
            PersonLogic personLogic = new PersonLogic();
            LevelLogic levelLogic = new LevelLogic();
            try
            {
                SupportViewModel initialstudentModel = (SupportViewModel)TempData["studentModel"];
                person = personLogic.GetModelBy(p => p.Person_Id == initialstudentModel.studentModel.Id);
                person.FirstName = viewmodel.studentModel.FirstName;
                person.LastName = viewmodel.studentModel.LastName;
                person.OtherName = viewmodel.studentModel.OtherName;

                bool modified = personLogic.Modify(person);
                GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();

                string Action = "MODIFIED";
                string Operation = "Modified the person Name for " + person.Id + " From " + person.FullName + " to " + viewmodel.studentModel.FirstName + viewmodel.studentModel.LastName + viewmodel.studentModel.OtherName;
                string Table = "Person Table";
                generalAuditLogic.CreateGeneralAudit(Action, Operation, Table);


                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                StudentLevel studentLevel = new StudentLevel();
                studentLevel.Level = viewmodel.Level;
                studentLevel.Department = viewmodel.Department;
                studentLevel.Programme = viewmodel.Programme;
                if (viewmodel.DepartmentOption !=null)
                {
                    studentLevel.DepartmentOption = viewmodel.DepartmentOption;
                }
                studentLevel.Session = initialstudentModel.Session;

                bool isStudentLevelModified = studentLevelLogic.Modify(studentLevel, person);
                if (modified || isStudentLevelModified)
                {
                    string Action2 = "MODIFIED";
                    string Operation2 = "Modified the student level for " + person.Id +  " to " + viewmodel.Level.Name + viewmodel.Department.Name + viewmodel.Programme.Name;
                    string Table2 = "Person Table";
                    generalAuditLogic.CreateGeneralAudit(Action2, Operation2, Table2);

                    SetMessage("Correction Successful", Message.Category.Information);
                }
                else
                {
                    SetMessage("Correction Unsuccessful", Message.Category.Information);
                }
                ViewBag.Programme = new SelectList(initialstudentModel.ProgrammeList, ID, NAME, viewmodel.Programme.Id);
                ViewBag.Department = new SelectList(initialstudentModel.DepartmentList, ID, NAME, viewmodel.Department.Id);
                ViewBag.Level = new SelectList(levelLogic.GetAll(), ID, NAME, studentLevel.Level.Id);
                ViewBag.DepartmentOption = new SelectList(new List<DepartmentOption>(), ID, NAME);
            }

            catch (Exception ex)
            {
                SetMessage("Error: " + ex.Message, Message.Category.Error);
            }

            return View("Correction", viewmodel);
        }

        public ActionResult MatricNumberCorrection()
        {
            return View();
        }
        [HttpPost]
        public ActionResult MatricNumberCorrection(SupportViewModel viewmodel)
        {
            try
            {
                ApplicationForm applicationForm = new ApplicationForm();
                ApplicationFormLogic applicationFormLogic = new ApplicationFormLogic();
                applicationForm = applicationFormLogic.GetModelBy(p => p.Application_Form_Number == viewmodel.ApplicationForm.Number);
                Abundance_Nk.Model.Model.Student student = new Model.Model.Student();
                StudentLogic studentLogic = new StudentLogic();
                StudentLevel studentLevel = new StudentLevel();
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                student = studentLogic.GetModelBy(p => p.Person_Id == applicationForm.Person.Id);
                List<Abundance_Nk.Model.Model.Student> studentList = new List<Model.Model.Student>();
                studentList = studentLogic.GetModelsBy(p => p.Matric_Number == student.MatricNumber);
                Abundance_Nk.Model.Model.Student studentToUpdate = updateStudentDuplicateMatricNumber(studentList, student);
                studentLevel = studentLevelLogic.GetModelBy(p => p.Person_Id == studentToUpdate.Id);
                bool isMatricNumberUpdated = studentLogic.UpdateMatricNumber(studentLevel, studentToUpdate);
                string Operation = "Modified the matric number " + student.MatricNumber + " To " + viewmodel.MatricNumber;

                viewmodel.MatricNumber = studentToUpdate.MatricNumber;

                GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();

                string Action = "MODIFY";
                string Table = "Student Table";
                generalAuditLogic.CreateGeneralAudit(Action, Operation, Table);

                //if (studentList.Count > 1)
                //{
                //    bool isMatricNumberUpdated = studentLogic.UpdateMatricNumber(studentLevel, student);
                //    viewmodel.MatricNumber = student.MatricNumber;
                //}


            }
            catch (Exception)
            {

                throw;
            }
            return View(viewmodel);
        }
        public ActionResult ResetCourseRegistration()
        {
            SupportViewModel viewModel = new SupportViewModel();
            ViewBag.Session = viewModel.SessionSelectList;
            return View();
        }
        [HttpPost]
        public ActionResult ResetCourseRegistration(SupportViewModel viewModel)
        {
            try
            {
                StudentLogic studentLogic = new StudentLogic();
                CourseRegistrationLogic courseRegistrationLogic = new CourseRegistrationLogic();
                CourseRegistrationDetailLogic courseRegistrationDetailLogic = new CourseRegistrationDetailLogic();
                if (viewModel != null)
                {
                    if (viewModel.studentModel != null && viewModel.Session != null)
                    {
                        string matricNumber = viewModel.studentModel.MatricNumber;
                        List<Abundance_Nk.Model.Model.Student> students = studentLogic.GetModelsBy(p => p.Matric_Number == matricNumber);
                        if (students.Count == 1)
                        {
                            Abundance_Nk.Model.Model.Student student = students[0];
                            CourseRegistration courseRegistration = courseRegistrationLogic.GetModelBy(p => p.Person_Id == student.Id && p.Session_Id == viewModel.Session.Id);
                            if (courseRegistration != null)
                            {
                                List<CourseRegistrationDetail> courseRegistrationDetails = courseRegistrationDetailLogic.GetModelsBy(p => p.Student_Course_Registration_Id == courseRegistration.Id);
                                if (courseRegistrationDetails != null)
                                {
                                    if (courseRegistrationDetails.Count > 0)
                                    {
                                        foreach (CourseRegistrationDetail courseRegistrationDetail in courseRegistrationDetails)
                                        {
                                            if (courseRegistrationDetail.ExamScore == null && courseRegistrationDetail.TestScore == null)
                                            {
                                                bool isDeleted = courseRegistrationDetailLogic.Delete(p => p.Student_Course_Registration_Detail_Id == courseRegistrationDetail.Id);
                                            }
                                        }


                                    }
                                }
                                bool isCourseRegistrationDeleted = courseRegistrationLogic.Delete(p => p.Student_Course_Registration_Id == courseRegistration.Id);
                                if (isCourseRegistrationDeleted)
                                {
                                    SetMessage("Reset successful ", Message.Category.Information);
                                }
                                else
                                {
                                    SetMessage("Reset Failed ", Message.Category.Error);
                                }
                            }
                        }
                        else
                        {
                            SetMessage("Reset Failed ", Message.Category.Error);
                        }
       
                       
                    }
                }
            }
            catch (Exception ex)
            {
                 SetMessage("Error Occurred: " + ex.Message, Message.Category.Error);
                 ViewBag.Session = viewModel.SessionSelectList;
            }
            ViewBag.Session = viewModel.SessionSelectList;
            return View();
        }
        //public ActionResult FixAllMatricNumberDuplicates()
        //{
        //  //  DuplicateMatricNumberFix duplicateMatricNumber = new DuplicateMatricNumberFix ();
        //    List<string> matricNumList = new List<string>();
        //    matricNumList.Add("PN/BF/12/1923");
            
        //    foreach (string item in matricNumList)
        //    {
        //        StudentLogic studentLogic = new StudentLogic();
        //        StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
        //          List<Abundance_Nk.Model.Model.Student> studentList = new List<Model.Model.Student>();
        //          studentList = studentLogic.GetModelsBy(p => p.Matric_Number == item);
                  
        //          foreach (Abundance_Nk.Model.Model.Student studentItem in studentList)
        //          {
        //              StudentLevel studentLevel = new StudentLevel();
        //              Abundance_Nk.Model.Model.Student studentToUpdate = updateStudentDuplicateMatricNumber(studentList, studentItem);
        //              studentLevel = studentLevelLogic.GetModelBy(p => p.Person_Id == studentToUpdate.Id);
        //              bool isMatricNumberUpdated = studentLogic.UpdateMatricNumber(studentLevel, studentToUpdate);
        //          }
               
        //    }
        //    return View();
        //}
        private Abundance_Nk.Model.Model.Student updateStudentDuplicateMatricNumber(List<Model.Model.Student> studentList, Abundance_Nk.Model.Model.Student student)
        {
            try
            {

                Abundance_Nk.Model.Model.Student studentWithStudentNumber = null;
                StudentLogic studentLogic = new StudentLogic();

                foreach (Abundance_Nk.Model.Model.Student studentItem in studentList)
                {
                    string firstmatricNumber = studentItem.MatricNumber;
                    string[] matricNoArray = firstmatricNumber.Split('/');
                    long studentNumber = Convert.ToInt64(matricNoArray[3]);
                    studentItem.Number = studentNumber;
                    studentLogic.Modify(studentItem);
                    studentWithStudentNumber = studentItem;
                }

                return studentWithStudentNumber;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public ActionResult UpdatePayment()
        {
            SupportViewModel viewmodel = new SupportViewModel();
            try
            {
                ViewBag.FeeTypeId = viewmodel.FeeTypeSelectList;
                ViewBag.Session = viewmodel.SessionSelectList;
            }
            catch (Exception ex)
            {
                SetMessage("Error occurred! " + ex.Message, Message.Category.Error);

            }
            
            return View();
        }
       
        [HttpPost]
        public ActionResult UpdatePayment(SupportViewModel viewmodel)
        {
            try
            {
                PaymentEtranzact paymentEtranzact = new PaymentEtranzact();
                PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
                PaymentTerminal paymentTerminal = new PaymentTerminal();
                PaymentTerminalLogic paymentTerminalLogic = new PaymentTerminalLogic();
                paymentTerminal = paymentTerminalLogic.GetModelBy(p => p.Fee_Type_Id == viewmodel.FeeType.Id && p.Session_Id == viewmodel.Session.Id);
                paymentEtranzact = paymentEtranzactLogic.RetrieveEtranzactWebServicePinDetails(viewmodel.PaymentEtranzact.ConfirmationNo, paymentTerminal);
                viewmodel.PaymentEtranzact = paymentEtranzact;
                viewmodel.PaymentEtranzact.Terminal = paymentTerminal;

            }
            catch (Exception ex)
            {
                SetMessage("Error occurred! " + ex.Message, Message.Category.Error);

            }

            ViewBag.FeeTypeId = viewmodel.FeeTypeSelectList;
            ViewBag.Session = viewmodel.SessionSelectList;
            return View(viewmodel);
        }
      
        [HttpPost]
        public ActionResult SavePayment(SupportViewModel viewmodel)
        {
            try
            {
                PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
                PaymentEtranzact paymetEtranzact = new PaymentEtranzact();
                paymetEtranzact = viewmodel.PaymentEtranzact;
                PaymentLogic paymentLogic = new PaymentLogic();
                Payment payment = new Payment();
                payment = paymentLogic.GetModelBy(p => p.Invoice_Number == viewmodel.PaymentEtranzact.CustomerID);
                PaymentTerminalLogic paymentTerminalLogic = new PaymentTerminalLogic();
                PaymentTerminal paymentTerminal = paymentTerminalLogic.GetModelBy(p => p.Fee_Type_Id == payment.FeeType.Id && p.Session_Id == viewmodel.Session.Id);
                PaymentEtranzactType paymentEtranzactType = new PaymentEtranzactType();
                PaymentEtranzactTypeLogic paymentEtranzactTypeLogic = new PaymentEtranzactTypeLogic();
                paymentEtranzactType = paymentEtranzactTypeLogic.GetModelsBy(p => p.Fee_Type_Id == payment.FeeType.Id).LastOrDefault();
                paymetEtranzact.EtranzactType = paymentEtranzactType;
                paymetEtranzact.Terminal = paymentTerminal;
                if (payment != null)
                {
                    OnlinePayment onlinePayment = new OnlinePayment();
                    OnlinePaymentLogic onlinePaymentLogic = new OnlinePaymentLogic();
                    onlinePayment = onlinePaymentLogic.GetModelBy(c => c.PAYMENT_CHANNEL.Payment_Channnel_Id == (int)PaymentChannel.Channels.Etranzact && c.Payment_Id == payment.Id);
                    paymetEtranzact.Payment = onlinePayment;
                }
                PaymentEtranzact paymentEtranzactCheck = paymentEtranzactLogic.GetModelBy(p => p.Confirmation_No == paymetEtranzact.ConfirmationNo);
                if (paymentEtranzactCheck == null)
                {
                    paymetEtranzact = paymentEtranzactLogic.Create(paymetEtranzact);
                    GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();

                    string Action = "MODIFY";
                    string Operation = "Updated the payment Etranzact with ID " + paymetEtranzact.ConfirmationNo;
                    string Table = "Payment Etanzact Table";
                    generalAuditLogic.CreateGeneralAudit(Action, Operation, Table);

                    SetMessage("Payment Updated Successfully! ", Message.Category.Information);
                }
                else
                {
                    SetMessage("Payment already exists! ", Message.Category.Warning);
                }
               

            }
            catch (Exception ex)
            {
                SetMessage("Error occurred! " + ex.Message, Message.Category.Error);

            }

            ViewBag.FeeTypeId = viewmodel.FeeTypeSelectList;
            ViewBag.Session = viewmodel.SessionSelectList;
            return View("UpdatePayment", viewmodel);
        }

        public ActionResult UpdatePassport()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UpdatePassport(SupportViewModel viewmodel)
        {
            try
            {
                ApplicationFormLogic applicationFormLogic = new ApplicationFormLogic();
                StudentLogic studentLogic = new StudentLogic();

                ApplicationForm applicationForm = new ApplicationForm();

                applicationForm = applicationFormLogic.GetModelsBy(p => p.Application_Form_Number == viewmodel.ApplicationForm.Number).LastOrDefault();
                
                if (applicationForm == null)
                {
                    Model.Model.Student student = studentLogic.GetModelsBy(s => s.Matric_Number == viewmodel.ApplicationForm.Number).LastOrDefault();
                    if (student != null)
                    {
                        viewmodel.Person = student;
                    }
                }
                else
                {
                    viewmodel.Person = applicationForm.Person;
                }

                if (viewmodel.Person == null)
                {
                    SetMessage("Student not found! ", Message.Category.Error);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error occured! " + ex.Message, Message.Category.Error);

            }

            return View(viewmodel);
        }

        [HttpPost]
        public ActionResult SaveStudentPassport(SupportViewModel viewmodel)
        {
            try
            {
                PersonLogic personLogic = new PersonLogic();
                Person person = personLogic.GetModelBy(p => p.Person_Id == viewmodel.Person.Id);
                string OldUrl = person.ImageFileUrl;
                string extension = Path.GetExtension(viewmodel.File.FileName).ToLower();

                string invalidImage = InvalidFile(viewmodel.File.ContentLength, extension);
                if (string.IsNullOrEmpty(invalidImage))
                {
                    string imageUrl = getImageURL(viewmodel, person);
                    person.ImageFileUrl = imageUrl;
                    personLogic.Modify(person);
                    GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();

                    string Action = "MODIFY";
                    string Operation = "Changes Passport for the person from " + OldUrl + " To " + person.ImageFileUrl;
                    string Table = "Payment Table";
                    generalAuditLogic.CreateGeneralAudit(Action, Operation, Table);

                    SetMessage("Correction Successful ", Message.Category.Information);
                }
                else
                {
                    SetMessage("Operation Failed, " + invalidImage, Message.Category.Error);
                }


            }
            catch (Exception)
            {

                throw;
            }
            return View("UpdatePassport", viewmodel);
        }
        private string getImageURL(SupportViewModel viewModel, Person person)
        {
            if (viewModel.File != null)
            {
                //Saving image to a folder and saving its url to db
                string[] allowedFIleExtensions = new[] { ".jpg", ".png", ".jpeg", ".Jpeg" };

                string filenameWithExtension = Path.GetFileName(viewModel.File.FileName); // getting filename
                string extension = Path.GetExtension(viewModel.File.FileName).ToLower(); // getting only the extension
                if (allowedFIleExtensions.Contains(extension)) // check extension type
                {

                    string FileNameWithoutExtension = Path.GetFileNameWithoutExtension(filenameWithExtension); //retireves filename without extension
                    FileNameWithoutExtension = person.FullName;
                    string FileNameInServer = FileNameWithoutExtension + DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + "_" + extension; // add reg number after underscore to make the filename unique for each person
                    string pathToFileInServer = Path.Combine(Server.MapPath("/Content/Student/"), FileNameInServer);
                    string passportUrl = "/Content/Student/" + FileNameInServer;
                    viewModel.File.SaveAs(pathToFileInServer);
                    return passportUrl;
                }
                else
                {
                    return person.ImageFileUrl;
                }
            }
            else
            {
                return person.ImageFileUrl;
            }

        }
        private string InvalidFile(decimal uploadedFileSize, string fileExtension)
        {
            try
            {
                string message = null;
                decimal oneKiloByte = 1024;
                decimal maximumFileSize = 50 * oneKiloByte;

                decimal actualFileSizeToUpload = Math.Round(uploadedFileSize / oneKiloByte, 1);
                if (InvalidFileType(fileExtension))
                {
                    message = "File type '" + fileExtension + "' is invalid! File type must be any of the following: .jpg, .jpeg, .png or .jif ";
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
        public ActionResult UnlockStudentData()
        {
            SupportViewModel viewModel = new SupportViewModel();
            return View(viewModel);
        }
        [HttpPost]
        public ActionResult UnlockStudentData(SupportViewModel viewModel)
        {
            SupportViewModel vModel = new SupportViewModel();
            try
            {
                StudentLogic studentLogic = new StudentLogic();
                Model.Model.Student student = new Model.Model.Student();
                student = studentLogic.GetModelBy(p => p.Matric_Number == viewModel.studentModel.MatricNumber);
                StudentCategoryLogic categoryLogic = new StudentCategoryLogic();
                vModel.studentModel = student;
                TempData["student"] = student;
                ViewBag.Category = new SelectList(categoryLogic.GetAll(), "Id", "Name", student.Category.Id);
            }
            catch (Exception ex)
            {
                SetMessage("Error, " + ex.Message, Message.Category.Error);
            }
            return View(vModel);
        }
        [HttpPost]
        public ActionResult SaveUnlockedData(SupportViewModel viewModel)
        {
            try
            {
                StudentLogic studentLogic = new StudentLogic();
                Model.Model.Student student = new Model.Model.Student();
                student = (Model.Model.Student)TempData["student"];
                student.Category = viewModel.studentModel.Category;
                studentLogic.Modify(student);
                GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();

                string Action = "MODIFY";
                string Operation = "Unlocked data for the student  " + student.MatricNumber;
                string Table = "Student Table";
                generalAuditLogic.CreateGeneralAudit(Action, Operation, Table);

                SetMessage("Save Successful", Message.Category.Information);
            }
            catch (Exception ex)
            {
                SetMessage("Error, " + ex.Message, Message.Category.Error);
            }
            return RedirectToAction("UnlockStudentData");
        }
        public ActionResult ViewCarryOverCourses()
        {
            try
            {
                SupportViewModel viewModel = new SupportViewModel();
                ViewBag.Session = viewModel.AllSessionSelectList;
                ViewBag.Semester = new SelectList(new List<Semester>(), ID, NAME);
                ViewBag.Level = new SelectList(viewModel.LevelList, ID, NAME);

            }
            catch (Exception ex)
            {
                SetMessage("Error Occured!" + ex.Message, Message.Category.Error);

            }
            return View();
        }
        [HttpPost]
        public ActionResult ViewCarryOverCourses(SupportViewModel viewModel)
        {
            try
            {
                StudentLevel studentLevel = new StudentLevel();
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                StudentLogic studentLogic = new StudentLogic();
                CourseLogic courseLogic = new CourseLogic();
                if (viewModel != null)
                {
                    Model.Model.Student student = studentLogic.GetModelBy(p => p.Matric_Number == viewModel.MatricNumber);
                    if (student != null)
                    {
                        studentLevel = studentLevelLogic.GetModelsBy(p => p.Person_Id == student.Id).FirstOrDefault();
                        if (studentLevel != null && studentLevel.Department != null && viewModel.Level != null && viewModel.Semester != null)
                        {
                            viewModel.Courses = courseLogic.GetBy(studentLevel.Programme,studentLevel.Department, viewModel.Level, viewModel.Semester);
                            viewModel.Department = studentLevel.Department;
                            viewModel.Programme = studentLevel.Programme;
                            TempData["supportViewModel"] = viewModel;

                            ViewBag.Session = viewModel.AllSessionSelectList;
                            ViewBag.Semester = new SelectList(new List<Semester>(), ID, NAME);
                            ViewBag.Level = new SelectList(viewModel.LevelList, ID, NAME, viewModel.Level);

                            return View(viewModel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occured!" + ex.Message, Message.Category.Error);

            }
            return View();
        }
       // [HttpPost]
        //public ActionResult AddCarryOverCourses(SupportViewModel checkedCourseVM)
        //{
        //    try
        //    {
        //        SupportViewModel viewModel = (SupportViewModel)TempData["supportViewModel"];
        //        CourseRegistration courseRegistration = new CourseRegistration();
        //        CourseRegistration courseRegistrationNew = new CourseRegistration();
        //        CourseRegistrationDetail courseRegistrationDetail = new CourseRegistrationDetail();
        //        CourseRegistrationLogic courseRegistrationLogic = new CourseRegistrationLogic();
        //        CourseRegistrationDetailLogic courseRegistrationDetailLogic = new CourseRegistrationDetailLogic();
        //        StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
        //        StudentLogic studentLogic = new StudentLogic();
        //        if (viewModel != null && viewModel.MatricNumber != null && viewModel.Session != null && viewModel.Semester != null && viewModel.Programme != null && viewModel.Department != null && viewModel.Level != null && viewModel.Courses != null)
        //        {
        //            Model.Model.Student student = studentLogic.GetModelBy(p => p.Matric_Number == viewModel.MatricNumber);
        //            CourseMode courseMode = new CourseMode() { Id = 1 };

        //            if (student != null)
        //            {
        //                foreach (Course course in checkedCourseVM.Courses)
        //                {
        //                    if (course.IsRegistered == true)
        //                    {
        //                        courseRegistration = courseRegistrationLogic.GetModelBy(p => p.Department_Id == viewModel.Department.Id && p.Level_Id == viewModel.Level.Id && p.Person_Id == student.Id && p.Programme_Id == viewModel.Programme.Id && p.Session_Id == viewModel.Session.Id);
        //                        if (courseRegistration == null)
        //                        {
        //                            using (TransactionScope trans = new TransactionScope())
        //                            {
        //                                courseRegistration = new CourseRegistration();
        //                                courseRegistration.Student = student;
        //                                courseRegistration.Level = viewModel.Level;
        //                                courseRegistration.Programme = viewModel.Programme;
        //                                courseRegistration.Department = viewModel.Department;
        //                                courseRegistration.Session = viewModel.Session;
        //                                courseRegistrationNew = courseRegistrationLogic.Create(courseRegistration);

        //                                courseRegistrationDetail.CourseRegistration = courseRegistrationNew;
        //                                courseRegistrationDetail.Course = course;
        //                                courseRegistrationDetail.Mode = courseMode;
        //                                courseRegistrationDetail.Semester = viewModel.Semester;
        //                                courseRegistrationDetail.TestScore = 0;
        //                                courseRegistrationDetail.ExamScore = 0;
        //                                courseRegistrationDetailLogic.Create(courseRegistrationDetail);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            courseRegistrationDetail = courseRegistrationDetailLogic.GetModelBy(p => p.Student_Course_Registration_Id == courseRegistration.Id && p.Course_Id == course.Id && p.Semester_Id == viewModel.Semester.Id);
        //                            if (courseRegistrationDetail != null)
        //                            {
        //                                courseRegistrationDetail.TestScore = 0;
        //                                courseRegistrationDetail.ExamScore = 0;
        //                                courseRegistrationDetailLogic.Modify(courseRegistrationDetail);
        //                            }
        //                            else
        //                            {
        //                                courseRegistrationDetail = new CourseRegistrationDetail();
        //                                courseRegistrationDetail.CourseRegistration = courseRegistration;
        //                                courseRegistrationDetail.Course = course;
        //                                courseRegistrationDetail.Mode = courseMode;
        //                                courseRegistrationDetail.Semester = viewModel.Semester;
        //                                courseRegistrationDetail.TestScore = 0;
        //                                courseRegistrationDetail.ExamScore = 0;
        //                                courseRegistrationDetailLogic.Create(courseRegistrationDetail);
        //                            }

        //                        }

        //                        SetMessage("Course Added Successfully", Message.Category.Information);
        //                    }
        //                }

        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        SetMessage("Error: " + ex.Message, Message.Category.Error);
        //    }

        //    return RedirectToAction("ViewCarryOverCourses");
        //}
        public ActionResult AddCarryOverCourses(SupportViewModel checkedCourseVM)
        {
            try
            {
                string operation = "MODIFY/ADD CARRYOVER";
                string action = "ADMIN :CHANGES FROM ADMIN CONSOLE (SupportController)";
                string client = Request.LogonUserIdentity.Name + " (" + HttpContext.Request.UserHostAddress + ")";
                var courseRegistrationDetailAudit = new CourseRegistrationDetailAudit();
                courseRegistrationDetailAudit.Action = action;
                courseRegistrationDetailAudit.Operation = operation;
                courseRegistrationDetailAudit.Client = client;
                 UserLogic loggeduser = new UserLogic();
                courseRegistrationDetailAudit.User = loggeduser.GetModelBy(u => u.User_Name == User.Identity.Name);


                SupportViewModel viewModel = (SupportViewModel)TempData["supportViewModel"];
                CourseRegistration courseRegistration = new CourseRegistration();
                CourseRegistration courseRegistrationNew = new CourseRegistration();
                CourseRegistrationDetail courseRegistrationDetail = new CourseRegistrationDetail();
                CourseRegistrationLogic courseRegistrationLogic = new CourseRegistrationLogic();
                CourseRegistrationDetailLogic courseRegistrationDetailLogic = new CourseRegistrationDetailLogic();
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                StudentLogic studentLogic = new StudentLogic();
                if (viewModel != null && viewModel.MatricNumber != null && viewModel.Session != null && viewModel.Semester != null && viewModel.Programme != null && viewModel.Department != null && viewModel.Level != null && viewModel.Courses != null)
                {
                    Model.Model.Student student = studentLogic.GetModelBy(p => p.Matric_Number == viewModel.MatricNumber);
                    CourseMode courseMode = new CourseMode() { Id = 1 };

                    if (student != null)
                    {
                        foreach (Course course in checkedCourseVM.Courses)
                        {
                            if (course.IsRegistered == true)
                            {
                                List<CourseRegistration> courseRegistrationList = courseRegistrationLogic.GetModelsBy(p => p.Department_Id == viewModel.Department.Id && p.Level_Id == viewModel.Level.Id && p.Person_Id == student.Id && p.Programme_Id == viewModel.Programme.Id && p.Session_Id == viewModel.Session.Id);
                                if (courseRegistrationList.Count > 1)
                                {
                                    bool isRemoved = courseRegistrationDetailLogic.RemoveDuplicateCourseRegistration(courseRegistrationList);
                                }
                               
                                
                 
                                courseRegistration = courseRegistrationLogic.GetModelBy(p => p.Department_Id == viewModel.Department.Id && p.Level_Id == viewModel.Level.Id && p.Person_Id == student.Id && p.Programme_Id == viewModel.Programme.Id && p.Session_Id == viewModel.Session.Id);
                                if (courseRegistration == null)
                                {
                                    //using (TransactionScope trans = new TransactionScope(TransactionScopeOption.Required))
                                    //{
                                        courseRegistration = new CourseRegistration();
                                        courseRegistration.Student = student;
                                        courseRegistration.Level = viewModel.Level;
                                        courseRegistration.Programme = viewModel.Programme;
                                        courseRegistration.Department = viewModel.Department;
                                        courseRegistration.Session = viewModel.Session;
                                        courseRegistrationNew = courseRegistrationLogic.CreateCourseRegistration(courseRegistration);

                                        courseRegistrationDetail.CourseRegistration = courseRegistrationNew;
                                        courseRegistrationDetail.Course = course;
                                        courseRegistrationDetail.Mode = courseMode;
                                        courseRegistrationDetail.Semester = viewModel.Semester;
                                        courseRegistrationDetail.TestScore = 0;
                                        courseRegistrationDetail.ExamScore = 0;
                                        courseRegistrationDetailLogic.Create(courseRegistrationDetail,courseRegistrationDetailAudit);

                                        //trans.Complete();

                                        SetMessage("Course Added Successfully", Message.Category.Information);
                                    //}
                                }
                                else
                                {
                                    courseRegistrationDetail = courseRegistrationDetailLogic.GetModelBy(p => p.Student_Course_Registration_Id == courseRegistration.Id && p.Course_Id == course.Id && p.Semester_Id == viewModel.Semester.Id);
                                    if (courseRegistrationDetail != null)
                                    {
                                        courseRegistrationDetail.TestScore = 0;
                                        courseRegistrationDetail.ExamScore = 0;
                                        courseRegistrationDetailLogic.Modify(courseRegistrationDetail,courseRegistrationDetailAudit);
                                    }
                                    else
                                    {
                                        courseRegistrationDetail = new CourseRegistrationDetail();
                                        courseRegistrationDetail.CourseRegistration = courseRegistration;
                                        courseRegistrationDetail.Course = course;
                                        courseRegistrationDetail.Mode = courseMode;
                                        courseRegistrationDetail.Semester = viewModel.Semester;
                                        courseRegistrationDetail.TestScore = 0;
                                        courseRegistrationDetail.ExamScore = 0;
                                        courseRegistrationDetailLogic.Create(courseRegistrationDetail,courseRegistrationDetailAudit);
                                    }

                                    SetMessage("Course Added Successfully", Message.Category.Information);

                                }

                            }
                            //else
                            //{
                            //    SetMessage("No Course Was Added", Message.Category.Information);
                            //}
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error: " + ex.Message, Message.Category.Error);
            }

            return RedirectToAction("ViewCarryOverCourses");
        }

        public ActionResult ChangeMatricNumber()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ChangeMatricNumber(SupportViewModel viewModel)
        {
            try
            {
                if (viewModel != null)
                {
                    StudentLogic studentLogic = new StudentLogic();
                    Model.Model.Student currentMatricNumberStudent = studentLogic.GetModelBy(p => p.Matric_Number == viewModel.MatricNumber);
                    if (currentMatricNumberStudent == null)
                    {
                        SetMessage("The Current Matric Number does not exist", Message.Category.Error);
                        return View();
                    }
                    List<Model.Model.Student> studentNewMatricNumberCheck = studentLogic.GetModelsBy(p => p.Matric_Number == viewModel.MatricNumberAlt);
                    if (studentNewMatricNumberCheck.Count == 0)
                    {
                        currentMatricNumberStudent.MatricNumber = viewModel.MatricNumberAlt;
                        studentLogic.Modify(currentMatricNumberStudent);

                        SetMessage("Matric Number Correction Successful", Message.Category.Information);
                        return View();
                    }
                    else
                    {
                        SetMessage("New Matric Number has already been assigned", Message.Category.Error);
                        return View();
                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error: " + ex.Message, Message.Category.Error);
            }
            return View();
        }
        public ActionResult AddStaff()
        {
            SupportViewModel viewmodel = new SupportViewModel();
            return View();
        }
        [HttpPost]
        public ActionResult AddStaff(SupportViewModel viewModel)
        {
            try
            {
                viewModel = new SupportViewModel();
                List<User> users = new List<User>();
                foreach (string file in Request.Files)
                {
                    HttpPostedFileBase hpf = Request.Files[file] as HttpPostedFileBase;
                    string pathForSaving = Server.MapPath("~/Content/ExcelUploads");
                    string savedFileName = Path.Combine(pathForSaving, hpf.FileName);
                    hpf.SaveAs(savedFileName);
                    DataSet FileSet = ReadExcel(savedFileName);
                    if (FileSet != null && FileSet.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < FileSet.Tables[0].Rows.Count; i++)
                        {
                            User User = new User();
                            User.Username = FileSet.Tables[0].Rows[i][0].ToString().Trim();
                            User.Password = FileSet.Tables[0].Rows[i][1].ToString().Trim();
                            Role role = new Role() { Id = 10 };
                            User.Role = role;
                            User.LastLoginDate = DateTime.Now;
                            User.SecurityAnswer = "a";
                            SecurityQuestion securityQuestion = new SecurityQuestion() { Id = 1 };
                            User.SecurityQuestion = securityQuestion;

                           users.Add(User);
                        }
                    }
                }
                viewModel.Users = users;
                TempData["supportViewModel"] = viewModel;

            }
            catch (Exception ex)
            {
                SetMessage("Error" + ex.Message, Message.Category.Error);
            }
            return View(viewModel);
        }
        public ActionResult SaveAddedStaff()
        {
            try
            {
                UserLogic userLogic = new UserLogic();
                User user = new Model.Model.User();
                bool itemSaved = false;
                SupportViewModel viewModel = new SupportViewModel();
                viewModel = (SupportViewModel)TempData["supportViewModel"];

                foreach (User item in viewModel.Users)
                {
                    List<User> users = userLogic.GetModelsBy(u => u.User_Name == item.Username);
                    if (users.Count == 0)
                    {
                        userLogic.Create(item);
                        GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();

                        string Action = "CREATED";
                        string Operation = "Created the user  " + user.Username;
                        string Table = "User Table";
                        generalAuditLogic.CreateGeneralAudit(Action, Operation, Table);

                        itemSaved = true;
                    }
                }
                if (itemSaved)
                {

                    SetMessage("Added Successfully", Message.Category.Information);
                }
                else
                {
                    SetMessage("No Staff Added", Message.Category.Error);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error." + ex.Message, Message.Category.Error);
            }
            return RedirectToAction("AddStaff");
        }
        public ActionResult StaffAllocationUpload()
        {
            try
            {
                SupportViewModel viewModel = new SupportViewModel();
                ViewBag.AllSession = viewModel.SessionSelectList;
                ViewBag.Semester = new SelectList(new List<Semester>(), ID, NAME);
                ViewBag.Programme = viewModel.ProgrammeSelectListItem;
                ViewBag.Department = new SelectList(new List<Department>(), ID, NAME);
                ViewBag.Level = new SelectList(viewModel.LevelList, ID, NAME);
           
            }
            catch (Exception)
            {
                
                throw;
            }
            return View();
        }
        [HttpPost]
        public ActionResult StaffAllocationUpload(SupportViewModel viewModel)
        {
            try
            {
                SessionLogic sessionLogic = new SessionLogic();
                Session session = sessionLogic.GetModelBy(p => p.Activated == true);
                List<CourseAllocation> courseAllocations = new List<CourseAllocation>();
                foreach (string file in Request.Files)
                {
                    HttpPostedFileBase hpf = Request.Files[file] as HttpPostedFileBase;
                    string pathForSaving = Server.MapPath("~/Content/ExcelUploads");
                    string savedFileName = Path.Combine(pathForSaving, hpf.FileName);
                    hpf.SaveAs(savedFileName);
                    DataSet FileSet = ReadExcel(savedFileName);
                    if (FileSet != null && FileSet.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < FileSet.Tables[0].Rows.Count; i++)
                        {
                            
                            string courseCode = FileSet.Tables[0].Rows[i][0].ToString().Trim();
                            string username = FileSet.Tables[0].Rows[i][1].ToString().Trim();
                            Course course = new Course() { Code = courseCode };
                            User user = new Model.Model.User() { Username = username};

                            CourseAllocation courseAllocation = new CourseAllocation();
                            courseAllocation.Department = viewModel.Department;
                            courseAllocation.Level = viewModel.Level;
                            courseAllocation.Programme = viewModel.Programme;
                            courseAllocation.Session = session;
                            courseAllocation.Course = course;
                            courseAllocation.User = user;
                            courseAllocations.Add(courseAllocation);
                        }
                    }
                }
                viewModel.CourseAllocationList = courseAllocations;
                TempData["supportViewModel"] = viewModel;
                RetainDropdownState(viewModel);

            }
            catch (Exception ex)
            {
                SetMessage("Error" + ex.Message, Message.Category.Error);
            }
            return View(viewModel);
        }

        public ActionResult SaveStaffAllocationUpload()
        {
            SupportViewModel viewModel = (SupportViewModel)TempData["supportViewModel"];
            try
            {
                SessionLogic sessionLogic = new SessionLogic();
                Session session = sessionLogic.GetModelBy(p => p.Activated == true);
                CourseAllocationLogic courseAllocationLogic = new CourseAllocationLogic();
                CourseLogic courseLogic = new CourseLogic();
                UserLogic userLogic = new UserLogic();
                bool itemSaved = true;
               
                foreach (CourseAllocation courseAllocationItem in viewModel.CourseAllocationList)
                {
                    User user = userLogic.GetModelBy(u => u.User_Name == courseAllocationItem.User.Username);
                    Course course = courseLogic.GetModelsBy(p => p.Course_Code == courseAllocationItem.Course.Code && p.Department_Id == viewModel.Department.Id && p.Level_Id == viewModel.Level.Id ).FirstOrDefault();
                    if (user != null && course != null)
                    {
                        courseAllocationItem.User = user;
                        courseAllocationItem.Course = course;
                        CourseAllocation courseAllocationCheck = courseAllocationLogic.GetModelBy(p => p.Course_Id == course.Id && p.User_Id == user.Id &&p.Session_Id == session.Id && p.Programme_Id == viewModel.Programme.Id);
                        if (courseAllocationCheck == null)
                        {
                            GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();

                            string Action = "CREATED";
                            string Operation = "Created course allocation for  " + user.Username;
                            string Table = "Course Allocation Table";
                            generalAuditLogic.CreateGeneralAudit(Action, Operation, Table);

                            courseAllocationLogic.Create(courseAllocationItem);

                        }                       
                        itemSaved = true;
                    }                 
                }
                if (itemSaved)
                {
                    SetMessage("Courses allocated successfully", Message.Category.Information);
                }
                else
                {
                    SetMessage("No Staff Added", Message.Category.Error);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error." + ex.Message, Message.Category.Error);
            }
            RetainDropdownState(viewModel);
            return RedirectToAction("StaffAllocationUpload");
        }
        [HttpPost]
        public ActionResult DownloadStaffUser(SupportViewModel viewModel)
        {
            try
            {
                GridView gv = new GridView();
                List<User> staffUsers = new List<User>();
                UserLogic userLogic = new UserLogic();
                List<StaffUser> staffUserList = new List<StaffUser>();
                staffUsers = userLogic.GetModelsBy(x => x.Role_Id == viewModel.Role.Id);
                if (staffUsers.Count > 0)
                {
                    for (int i = 0; i < staffUsers.Count; i++)
                    {
                        StaffUser staffUser = new StaffUser();
                        staffUser.USER_NAME = staffUsers[i].Username;
                        staffUser.PASSWORD = staffUsers[i].Password;
                        staffUserList.Add(staffUser);
                    }
                    if (staffUserList.Count > 0)
                    {
                        List<StaffUser> orderedstaffUserList = staffUserList.OrderBy(x => x.USER_NAME).ToList();
                        List<StaffUser> finalstaffUserList = new List<StaffUser>();
                        for (int i = 0; i < orderedstaffUserList.Count; i++)
                        {
                            orderedstaffUserList[i].SN = (i + 1);
                            finalstaffUserList.Add(orderedstaffUserList[i]);
                        }
                        gv.DataSource = finalstaffUserList;
                        gv.Caption = "Course Staff User List";
                        gv.DataBind();
                        string filename = "User List";
                        return new DownloadFileActionResult(gv, filename + ".xls");

                    }
                    else
                    {
                        Response.Write("No data available for download");
                        Response.End();
                        return new JavaScriptResult();
                    }
                }
            }
            catch (Exception ex)
            {

                SetMessage("Error occurred! " + ex.Message, Message.Category.Error);
            }
            return RedirectToAction("EditStaff");

        }
        public ActionResult EditStaff()
        {
            try
            {
                SupportViewModel viewmodel = new SupportViewModel();
                ViewBag.Role = Utility.PopulateStaffRoleSelectListItem();
            }
            catch (Exception ex)
            {  
                SetMessage("Error occurred! " + ex.Message, Message.Category.Error);
            }
            
            return View();
        }
        [HttpPost]
        public ActionResult EditStaff(SupportViewModel viewModel)
        {
            try
            {
                viewModel = new SupportViewModel();
                List<Model.Model.User> users = new List<Model.Model.User>();
                foreach (string file in Request.Files)
                {
                    HttpPostedFileBase hpf = Request.Files[file] as HttpPostedFileBase;
                    string pathForSaving = Server.MapPath("~/Content/ExcelUploads");
                    string savedFileName = Path.Combine(pathForSaving, hpf.FileName);
                    hpf.SaveAs(savedFileName);
                    DataSet FileSet = ReadExcel(savedFileName);
                    if (FileSet != null && FileSet.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 1; i < FileSet.Tables[0].Rows.Count; i++)
                        {
                            Model.Model.User User = new Model.Model.User();
                            User.Username = FileSet.Tables[0].Rows[i][1].ToString().Trim();
                            User.Password = FileSet.Tables[0].Rows[i][2].ToString().Trim();
                            users.Add(User);
                        }
                    }
                }
                viewModel.Users = users;
                TempData["EditStaff"] = viewModel;

            }
            catch (Exception ex)
            {
                SetMessage("Error" + ex.Message, Message.Category.Error);
            }
            ViewBag.Role = Utility.PopulateStaffRoleSelectListItem();
            return View(viewModel);
        }
        public ActionResult SaveEditedStaff()
        {
            try
            {
                UserLogic userLogic = new UserLogic();
                User user = new Model.Model.User();
                bool itemSaved = false;
                bool isModified = false;
                SupportViewModel viewModel = new SupportViewModel();
                viewModel = (SupportViewModel)TempData["EditStaff"];
                for (int i = 0; i < viewModel.Users.Count; i++)
                {
                    List<User> editedUsers = new List<User>();
                    List<User> UsersList = new List<User>();

                    Model.Model.User editedUser = new Model.Model.User();
                    string username = viewModel.Users[i].Username;
                    editedUsers = userLogic.GetModelsBy(x => x.User_Name == username);
                    UsersList = viewModel.Users.FindAll(x => x.Username == username);
                    if (editedUsers.Count > 0)
                    {
                        for (int j = 0, k = 0; j < editedUsers.Count && k < UsersList.Count; j++, k++)
                        {
                            editedUsers[j].Username = UsersList[k].Username;
                            editedUsers[j].Password = UsersList[k].Password;
                            if (editedUsers[j].Username == "" || editedUsers[j].Password == "")
                            {
                                continue;
                            }
                            isModified = userLogic.Modify(editedUsers[j]);
                        }

                    }
                    else
                    {
                        continue;
                    }
                    if (isModified == false)
                    {
                        continue;
                    }
                    itemSaved = true;
                }

                if (itemSaved)
                {
                    SetMessage("Edited Successfully", Message.Category.Information);
                }
                else
                {
                    SetMessage("No Staff Edited", Message.Category.Error);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error." + ex.Message, Message.Category.Error);
            }
            return RedirectToAction("EditStaff");
        }

        public ActionResult DeleteDuplicateMatricNumber()
        {
            try
            {
                viewmodel = new SupportViewModel();
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View();
        }
        [HttpPost]
        public ActionResult DeleteDuplicateMatricNumber(SupportViewModel viewModel)
        {
            try
            {
                StudentLogic studentLogic = new StudentLogic();
                List<Model.Model.Student> students = studentLogic.GetModelsBy(s => s.Matric_Number == viewModel.studentModel.MatricNumber);
                if (students.Count == 0)
                {
                    SetMessage("Matric Number does not exist", Message.Category.Error);
                    return View();
                }
                viewModel.StudentList = students;
                TempData["ViewModel"] = viewModel;
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        public ActionResult FixDuplicateMatricNumber()
        {
            try
            {
                SupportViewModel viewModel = (SupportViewModel)TempData["ViewModel"];

                if (viewModel != null && viewModel.StudentList != null && viewModel.StudentList.Count > 1)
                {
                    StudentLogic studentLogic = new StudentLogic();
                    PersonLogic personLogic = new PersonLogic();
                    StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                    PaymentLogic paymentLogic = new PaymentLogic();
                    CourseRegistrationLogic courseRegistrationLogic = new CourseRegistrationLogic();

                    long firstPersonId = viewModel.StudentList[0].Id;
                    Model.Model.Student firstStudentRecord = new Model.Model.Student() { Id = firstPersonId };


                    for (int i = 1; i < viewModel.StudentList.Count; i++)
                    {
                        using (TransactionScope scope = new TransactionScope())
                        {
                            Model.Model.Student currentStudent = viewModel.StudentList[i];

                            List<CourseRegistration> courseRegistrations = courseRegistrationLogic.GetModelsBy(p => p.Person_Id == currentStudent.Id);
                            foreach (CourseRegistration courseRegistration in courseRegistrations)
                            {
                                string operation = "MODIFY";
                                string action = "ADMIN :CHANGES FROM ADMIN CONSOLE (SupportController)";
                                string client = Request.LogonUserIdentity.Name + " (" + HttpContext.Request.UserHostAddress + ")";
                                var courseRegistrationDetailAudit = new CourseRegistrationDetailAudit();
                                courseRegistrationDetailAudit.Action = action;
                                courseRegistrationDetailAudit.Operation = operation;
                                courseRegistrationDetailAudit.Client = client;
                                UserLogic loggeduser = new UserLogic();
                                courseRegistrationDetailAudit.User = loggeduser.GetModelsBy(u => u.User_Name == User.Identity.Name).LastOrDefault();

                                courseRegistration.Student = firstStudentRecord;
                                courseRegistrationLogic.ModifyRegOnly(courseRegistration, courseRegistrationDetailAudit);
                            }

                            List<Payment> payments = paymentLogic.GetModelsBy(p => p.Person_Id == currentStudent.Id);
                            foreach (Payment payment in payments)
                            {
                                payment.Person = firstStudentRecord;
                                paymentLogic.Modify(payment);
                            }

                            List<StudentLevel> studentLevels = studentLevelLogic.GetModelsBy(s => s.Person_Id == currentStudent.Id);
                            foreach (StudentLevel studentLevel in studentLevels)
                            {
                                studentLevel.Student = firstStudentRecord;
                                studentLevelLogic.ModifyByStudentLevelId(studentLevel);
                            }

                            StudentLevel studentLevelItem = new StudentLevel() { Student = currentStudent };

                            CheckStudentAcademicInformation(studentLevelItem);
                            CheckStudentExamRawScoreSheet(studentLevelItem, firstPersonId);
                            CheckStudentFinanceInformation(studentLevelItem);
                            CheckStudentResultDetails(studentLevelItem);
                            CheckStudentSponsor(studentLevelItem);
                            CheckStudentUpdateAudit(studentLevelItem, firstPersonId);
                            CheckStudentCourseEvaluation(studentLevelItem, firstPersonId);
                            CheckStudentNDResult(studentLevelItem, firstPersonId);
                            CheckStudentEmploymentInformation(studentLevelItem, firstPersonId);
                            CheckApplicantPreviousEducation(studentLevelItem, firstPersonId);
                            CheckApplicantOLevelResult(studentLevelItem, firstPersonId);

                            studentLogic.Delete(s => s.Person_Id == currentStudent.Id);
                            //personLogic.Delete(p => p.Person_Id == currentStudent.Id);

                            scope.Complete();
                        }
                    }

                    SetMessage("Operation Succesful! ", Message.Category.Information);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View("DeleteDuplicateMatricNumber");
        } 
        //public ActionResult EditMatricNumber(string sid)
        //{
        //    try
        //    {
        //        viewmodel = new SupportViewModel();
        //        long studentId = Convert.ToInt64(sid);
        //        StudentLogic studentLogic = new StudentLogic();
        //        Model.Model.Student student = studentLogic.GetModelBy(s => s.Person_Id == studentId);
        //        viewmodel.studentModel = student;
        //    }
        //    catch (Exception ex)
        //    {
        //        SetMessage("Error! " + ex.Message, Message.Category.Error);
        //    }

        //    return View(viewmodel);
        //}
        //public ActionResult SaveEditedMatricNumber(SupportViewModel viewModel)
        //{
        //    try
        //    {
        //        StudentLogic studentLogic = new StudentLogic();
        //        Model.Model.Student student = new Model.Model.Student();
        //        List<Model.Model.Student> students = studentLogic.GetModelsBy(s => s.Matric_Number == viewModel.studentModel.MatricNumber) ;
        //        if (students.Count != 0)
        //        {
        //            SetMessage("Error! New Matric Number already exist", Message.Category.Error);
        //            return RedirectToAction("DeleteDuplicateMatricNumber");
        //        }

        //        student = studentLogic.GetModelBy(s => s.Person_Id == viewModel.studentModel.Id);
        //        student.MatricNumber = viewModel.studentModel.MatricNumber;
        //        studentLogic.Modify(student);
        //        SetMessage("Operation Successful", Message.Category.Information);
        //    }
        //    catch (Exception ex)
        //    {
        //        SetMessage("Error! " + ex.Message, Message.Category.Error);
        //    }

        //    return RedirectToAction("DeleteDuplicateMatricNumber");
        //}
        public ActionResult EditMatricNumber(string sid)
        {
            try
            {
                viewmodel = new SupportViewModel();
                long studentId = Convert.ToInt64(sid);
                StudentLogic studentLogic = new StudentLogic();
                Model.Model.Student student = studentLogic.GetModelBy(s => s.Person_Id == studentId);
                viewmodel.studentModel = student;
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewmodel);
        }
        public ActionResult SaveEditedMatricNumber(SupportViewModel viewModel)
        {
            try
            {
                StudentLogic studentLogic = new StudentLogic();
                Model.Model.Student student = new Model.Model.Student();

                student = studentLogic.GetModelBy(s => s.Person_Id == viewModel.studentModel.Id);
                string Operation = "Modified the student matric number from  " + student.MatricNumber + " To " + viewModel.studentModel.MatricNumber;

                student.MatricNumber = viewModel.studentModel.MatricNumber;
                studentLogic.Modify(student);
                GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();

                string Action = "MODIFY";
                string Table = "Student Table";
                generalAuditLogic.CreateGeneralAudit(Action, Operation, Table);

                SetMessage("Operation Successful! ", Message.Category.Information);
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return RedirectToAction("DeleteDuplicateMatricNumber");
        }
        public long legitPersonId { get; set; }
        private long GetLegitimateStudent(List<StudentLevel> studentList)
        {
            long personId = 0;
            try
            {
                SessionLogic sessionLogic = new SessionLogic();
                Session currentSession = sessionLogic.GetModelBy(s => s.Activated == true);

                foreach (StudentLevel studentItem in studentList)
                {

                    CourseRegistrationLogic courseRegistrationLogic = new CourseRegistrationLogic();

                    List<CourseRegistration> courseRegistrations = courseRegistrationLogic.GetListBy(studentItem.Student, studentItem.Programme, studentItem.Department);
                    foreach (CourseRegistration courseregistration in courseRegistrations)
                    {
                        if (courseRegistrations.Count >= 1 && courseregistration.Session.Id != currentSession.Id)
                        {
                            personId = courseregistration.Student.Id;
                        }
                        else if (courseRegistrations.Count == 1 && personId == 0)
                        {
                            personId = courseregistration.Student.Id;
                        }
                    }
                }
                if (personId == 0)
                {
                    List<StudentLevel> studentLevels = studentList.Where(s => s.Session.Id == currentSession.Id).ToList();
                    if (studentLevels.Count >= 1)
                    {
                        personId = studentLevels.FirstOrDefault().Student.Id;
                    }
                    else
                    {
                        if (studentList.Count >= 1)
                        {
                            personId = studentList.FirstOrDefault().Student.Id;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return personId;
        }
        private static void CheckStudentUpdateAudit(StudentLevel studentItem, long correctPersonId)
        {
            try
            {
                StudentUpdateAuditLogic studentUpdateAuditLogic = new StudentUpdateAuditLogic();
                List<StudentUpdateAudit> studentUpdateAudits = studentUpdateAuditLogic.GetModelsBy(ss => ss.Student_Id == studentItem.Student.Id);
                if (studentUpdateAudits.Count > 0)
                {
                    foreach (StudentUpdateAudit studentUpdateAudit in studentUpdateAudits)
                    {
                        studentUpdateAudit.Student = new Model.Model.Student() { Id = correctPersonId };
                        studentUpdateAuditLogic.Modify(studentUpdateAudit);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private static void CheckStudentCourseEvaluation(StudentLevel studentItem, long correctPersonId)
        {
            try
            {
                CourseEvaluationAnswerLogic courseEvaluationAnswerLogic = new CourseEvaluationAnswerLogic();
                List<CourseEvaluationAnswer> studeCourseEvaluationAnswers = courseEvaluationAnswerLogic.GetModelsBy(ss => ss.Person_Id == studentItem.Student.Id);
                if (studeCourseEvaluationAnswers.Count > 0)
                {

                    foreach (CourseEvaluationAnswer studeCourseEvaluationAnswer in studeCourseEvaluationAnswers)
                    {
                        studeCourseEvaluationAnswer.Student = new Model.Model.Student() { Id = correctPersonId };
                        courseEvaluationAnswerLogic.Modify(studeCourseEvaluationAnswer);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private static void CheckStudentNDResult(StudentLevel studentItem, long correctPersonId)
        {
            try
            {
                StudentNdResultLogic studentNdResultLogic = new StudentNdResultLogic();
                List<StudentNdResult> studentNdResults = studentNdResultLogic.GetModelsBy(ss => ss.Person_Id == studentItem.Student.Id);
                if (studentNdResults.Count > 0)
                {
                    foreach (StudentNdResult studentNdResult in studentNdResults)
                    {
                        if (studentNdResultLogic.GetModelsBy(s => s.Person_Id == correctPersonId).LastOrDefault() == null)
                        {
                            studentNdResult.Student = new Model.Model.Student() { Id = correctPersonId };
                            studentNdResultLogic.Modify(studentNdResult);
                        }
                        else
                        {
                            Expression<Func<STUDENT_ND_RESULT, bool>> deleteNdResultSelector = ss => ss.Person_Id == studentItem.Student.Id;
                            studentNdResultLogic.Delete(deleteNdResultSelector);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private static void CheckStudentEmploymentInformation(StudentLevel studentItem, long correctPersonId)
        {
            try
            {
                StudentEmploymentInformationLogic studentEmploymentInformationLogic = new StudentEmploymentInformationLogic();
                List<StudentEmploymentInformation> studentEmploymentInformations = studentEmploymentInformationLogic.GetModelsBy(ss => ss.Person_Id == studentItem.Student.Id);
                if (studentEmploymentInformations.Count > 0)
                {
                    foreach (StudentEmploymentInformation studentEmploymentInformation in studentEmploymentInformations)
                    {
                        if (studentEmploymentInformationLogic.GetModelsBy(s => s.Person_Id == correctPersonId).LastOrDefault() == null)
                        {
                            studentEmploymentInformation.Student = new Model.Model.Student() { Id = correctPersonId };
                            studentEmploymentInformationLogic.Modify(studentEmploymentInformation);
                        }
                        else
                        {
                            Expression<Func<STUDENT_EMPLOYMENT_INFORMATION, bool>> deleteEmploymentInformationtSelector = ss => ss.Person_Id == studentItem.Student.Id;
                            studentEmploymentInformationLogic.Delete(deleteEmploymentInformationtSelector);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private static void CheckApplicantPreviousEducation(StudentLevel studentItem, long correctPersonId)
        {
            try
            {
                PreviousEducationLogic previousEducationLogic = new PreviousEducationLogic();
                List<PreviousEducation> appPreviousEducationList = previousEducationLogic.GetModelsBy(ss => ss.Person_Id == studentItem.Student.Id);
                if (appPreviousEducationList.Count > 0)
                {
                    foreach (PreviousEducation appPreviousEducation in appPreviousEducationList)
                    {
                        if (previousEducationLogic.GetModelsBy(s => s.Person_Id == correctPersonId).LastOrDefault() == null)
                        {
                            appPreviousEducation.Person = new Model.Model.Person() { Id = correctPersonId };
                            previousEducationLogic.Modify(appPreviousEducation);
                        }
                        else
                        {
                            Expression<Func<APPLICANT_PREVIOUS_EDUCATION, bool>> deletepreviousEducationSelector = ss => ss.Person_Id == studentItem.Student.Id;
                            previousEducationLogic.Delete(deletepreviousEducationSelector);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private static void CheckApplicantOLevelResult(StudentLevel studentItem, long correctPersonId)
        {
            try
            {
                OLevelResultLogic oLevelResultLogic = new OLevelResultLogic();
                List<OLevelResult> oLevelResults = oLevelResultLogic.GetModelsBy(ss => ss.Person_Id == studentItem.Student.Id);
                if (oLevelResults.Count > 0)
                {
                    foreach (OLevelResult oLevelResult in oLevelResults)
                    {
                        oLevelResult.Person = new Model.Model.Person() { Id = correctPersonId };
                        oLevelResultLogic.Modify(oLevelResult);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private static void CheckStudentExamRawScoreSheet(StudentLevel studentItem, long correctPersonId)
        {
            try
            {
                StudentExamRawScoreSheetResultLogic studentExamRawScoreSheetResultLogic = new StudentExamRawScoreSheetResultLogic();
                List<StudentExamRawScoreSheet> studentExamRawScoreSheets = studentExamRawScoreSheetResultLogic.GetModelsBy(ss => ss.Student_Id == studentItem.Student.Id);
                if (studentExamRawScoreSheets.Count > 0)
                {
                    foreach (StudentExamRawScoreSheet studentExamRawScoreSheet in studentExamRawScoreSheets)
                    {
                        studentExamRawScoreSheet.Student = new Model.Model.Student() { Id = correctPersonId };
                        studentExamRawScoreSheetResultLogic.Modify(studentExamRawScoreSheet);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

        }
        public ActionResult ViewStudentPayments()
        {
            SupportViewModel viewModel = null;
            try
            {
                viewModel = new SupportViewModel();
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult ViewStudentPayments(SupportViewModel viewModel)
        {
            try
            {
                if (viewModel.studentModel.MatricNumber != null)
                {
                    PaymentLogic paymentLogic = new PaymentLogic();
                    StudentLogic studentLogic = new StudentLogic();
                    List<Payment> paymentList = new List<Payment>();

                    List<Model.Model.Student> students = studentLogic.GetModelsBy(s => s.Matric_Number == viewModel.studentModel.MatricNumber);
                    if (students.Count != 1)
                    {
                        SetMessage("No Student with this Matric Number OR Matric Number is Duplicate", Message.Category.Error);
                        return View(viewModel);
                    }

                    long studentId = students[0].Id;
                    paymentList = paymentLogic.GetModelsBy(p => p.Person_Id == studentId);
                    if (paymentList.Count == 0)
                    {
                        SetMessage("No Payment Record for this Student", Message.Category.Error);
                        return View(viewModel);
                    }

                    viewModel.Payments = paymentList;
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }

        public ActionResult EditPayment(long pmid)
        {
            SupportViewModel viewModel = new SupportViewModel();
            try
            {
                if (pmid > 0)
                {
                    PaymentLogic paymentLogic = new PaymentLogic();
                    StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                    Payment payment = paymentLogic.GetModelBy(p => p.Payment_Id == pmid);
                    viewModel.studentLevel = new StudentLevel();
                    if (payment?.Id > 0)
                    {
                        viewModel.studentLevel = studentLevelLogic.GetModelsBy(f => f.Person_Id == payment.Person.Id && f.Session_Id == payment.Session.Id).FirstOrDefault();
                    }
                    RetainPaymentDropDownState(payment);

                    viewModel.Payment = payment;
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult EditPayment(SupportViewModel viewModel)
        {
            try
            {
                if (viewModel.Payment != null)
                {
                    PaymentLogic paymentLogic = new PaymentLogic();
                    StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                    paymentLogic.Modify(viewModel.Payment);
                   var studentLevel= studentLevelLogic.GetModelBy(f => f.Student_Level_Id == viewModel.studentLevel.Id);
                    if (studentLevel?.Id > 0)
                    {
                        studentLevel.Level = new Level { Id = viewModel.studentLevel.Level.Id };
                        studentLevelLogic.Modify(studentLevel);
                    }
                    


                    SetMessage("Opeartion Successful!", Message.Category.Information);
                    return RedirectToAction("ViewStudentPayments");
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            RetainPaymentDropDownState(viewModel.Payment);
            return View(viewModel);
        }

        public ActionResult DownloadHNDApplicantScannedResult()
        {
            SupportViewModel viewModel = new SupportViewModel();
            try
            {
                ViewBag.Session = viewModel.AllSessionSelectList;
                ViewBag.Programme = viewModel.ProgrammeSelectListItem;
                ViewBag.Department = new SelectList(new List<Department>(), "Id", "Name");
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult DownloadHNDApplicantScannedResult(SupportViewModel viewModel)
        {
            try
            {
                if (viewModel != null)
                {
                    DateTime DateFrom = new DateTime();
                    DateTime DateTo = new DateTime();

                    if (!DateTime.TryParse(viewModel.DateFrom, out DateFrom) || !DateTime.TryParse(viewModel.DateTo, out DateTo))
                    {
                        DateFrom = DateTime.Now;
                        DateTo = DateTime.Now;
                    }

                    string tempFolder = "~/Content/TempFolder" + DateTime.Now.ToString().Replace("/", "").Replace(":", "").Replace(" ", "");

                    ApplicantLogic applicantLogic = new ApplicantLogic();
                    List<ApplicantResult> results = applicantLogic.GetScannedOlevelResultsHND(viewModel.Programme, viewModel.Department, viewModel.Session, DateFrom, DateTo);
                    if (results.Count > 0)
                    {
                        List<string> scannedResultUrllList = results.Select(r => r.ScannedCopyUrl).Distinct().ToList();
                        List<string> scannedNdResultUrllList = results.Select(r => r.NdResultUrl).Distinct().ToList();
                        List<string> scannedCertificateUrllList = results.Select(r => r.CertificateUrl).Distinct().ToList();

                        if (Directory.Exists(Server.MapPath(tempFolder)))
                        {
                            Directory.Delete(Server.MapPath(tempFolder), true);
                            Directory.CreateDirectory(Server.MapPath(tempFolder));
                        }
                        else
                        {
                            Directory.CreateDirectory(Server.MapPath(tempFolder));
                        }

                        for (int i = 0; i < scannedResultUrllList.Count; i++)
                        {
                            string thisUrl = scannedResultUrllList[i];
                            if (string.IsNullOrEmpty(thisUrl))
                            {
                                continue;
                            }
                            ApplicantResult personResult = results.FirstOrDefault(r => r.ScannedCopyUrl == thisUrl);

                            string[] imgUrl = thisUrl.Split('/');
                            string mainImg = imgUrl[4];

                            if (string.IsNullOrEmpty(mainImg))
                            {
                                continue;
                            }    

                            string filePath = Server.MapPath(thisUrl);
                            if (!System.IO.File.Exists(filePath))
                            {
                                continue;
                            }

                            FileInfo fileInfo = new FileInfo(mainImg);
                            string fileExtension = fileInfo.Extension;
                            string newFileName = personResult.Name.Replace(" ", "_") + "_" + personResult.NumberOfSittings + fileExtension;

                            System.IO.File.Copy(filePath, Server.MapPath(Path.Combine(tempFolder, newFileName)), true);  
                            
                        }

                        for (int i = 0; i < scannedNdResultUrllList.Count; i++)
                        {
                            string thisUrl = scannedNdResultUrllList[i];
                            if (string.IsNullOrEmpty(thisUrl))
                            {
                                continue;
                            }
                            ApplicantResult personResult = results.FirstOrDefault(r => r.NdResultUrl == thisUrl);

                            string[] imgUrl = thisUrl.Split('/');
                            string mainImg = imgUrl[4];

                            if (string.IsNullOrEmpty(mainImg))
                            {
                                continue;
                            }

                            string filePath = Server.MapPath(thisUrl);
                            if (!System.IO.File.Exists(filePath))
                            {
                                continue;
                            }

                            FileInfo fileInfo = new FileInfo(mainImg);
                            string fileExtension = fileInfo.Extension;
                            string newFileName = personResult.Name.Replace(" ", "_") + "_" + "ND_Result" + fileExtension;

                            System.IO.File.Copy(filePath, Server.MapPath(Path.Combine(tempFolder, newFileName)), true);

                        }

                        for (int i = 0; i < scannedCertificateUrllList.Count; i++)
                        {
                            string thisUrl = scannedCertificateUrllList[i];
                            if (string.IsNullOrEmpty(thisUrl))
                            {
                                continue;
                            }
                            ApplicantResult personResult = results.FirstOrDefault(r => r.CertificateUrl == thisUrl);

                            string[] imgUrl = thisUrl.Split('/');
                            string mainImg = imgUrl[4];

                            if (string.IsNullOrEmpty(mainImg))
                            {
                                continue;
                            }

                            string filePath = Server.MapPath(thisUrl);
                            if (!System.IO.File.Exists(filePath))
                            {
                                continue;
                            }

                            FileInfo fileInfo = new FileInfo(mainImg);
                            string fileExtension = fileInfo.Extension;
                            string newFileName = personResult.Name.Replace("/", "_") + "_" + "Certificate" + fileExtension;

                            System.IO.File.Copy(filePath, Server.MapPath(Path.Combine(tempFolder, newFileName)), true);

                        }

                        using (ZipFile zip = new ZipFile())
                        {
                            string file = Server.MapPath(tempFolder);
                            zip.AddDirectory(file, "");
                            string zipFileName = results.FirstOrDefault().Department.Replace("/", "");
                            zip.Save(file + zipFileName + ".zip");                 
                        }

                        string savedFile = tempFolder + results.FirstOrDefault().Department.Replace("/", "") + ".zip";
                        RetainDropdownState(viewModel);

                        return File(Server.MapPath(savedFile), "application/zip", results.FirstOrDefault().Department.Replace("/", "") + ".zip");
                    }  
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            ViewBag.Session = viewModel.AllSessionSelectList;
            ViewBag.Programme = viewModel.ProgrammeSelectListItem;
            ViewBag.Department = new SelectList(new List<Department>(), "Id", "Name");
            return View(viewModel);
        }
        public ActionResult DownloadHNDApplicantScannedResultSingle()
        {
            SupportViewModel viewModel = new SupportViewModel();
            try
            {
                ViewBag.Session = viewModel.AllSessionSelectList;
                ViewBag.Programme = viewModel.ProgrammeSelectListItem;
                ViewBag.Department = new SelectList(new List<Department>(), "Id", "Name");
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
            
        }
        [HttpPost]
        public ActionResult DownloadHNDApplicantScannedResultSingle(SupportViewModel viewModel)
        {
            try
            {
                DateTime DateFrom = new DateTime();
                DateTime DateTo = new DateTime();

                if (!DateTime.TryParse(viewModel.DateFrom, out DateFrom) || !DateTime.TryParse(viewModel.DateTo, out DateTo))
                {
                    DateFrom = DateTime.Now;
                    DateTo = DateTime.Now;
                }
                if(viewModel.Programme.Id==3 || viewModel.Programme.Id == 4 || viewModel.Programme.Id == 5)
                {
                    ApplicantLogic applicantLogic = new ApplicantLogic();
                    List<ApplicantResult> results = applicantLogic.GetScannedOlevelResultsHND(viewModel.Programme, viewModel.Department, viewModel.Session, DateFrom, DateTo.AddDays(1));
                    List<ApplicantResult> masterList = new List<ApplicantResult>();
                    List<long> distinctResult = results.Select(p => p.PersonId).Distinct().ToList();
                    for (int i = 0; i < distinctResult.Count; i++)
                    {
                        masterList.Add(results.Where(r => r.PersonId == distinctResult[i]).FirstOrDefault());
                    }

                    ViewBag.Session = viewModel.AllSessionSelectList;
                    ViewBag.Programme = viewModel.ProgrammeSelectListItem;
                    ViewBag.Department = new SelectList(Utility.PopulateDepartmentSelectListItem(viewModel.Programme), "Value", "Text", viewModel.Department.Id);

                    viewModel.ApplicantResults = masterList.OrderBy(r => r.Name).ToList();
                    TempData["viewModel"] = viewModel;
                }
                else if(viewModel.Programme.Id == 1 || viewModel.Programme.Id == 2 || viewModel.Programme.Id == 6)
                {
                    ApplicantJambDetailLogic applicantJambDetailLogic = new ApplicantJambDetailLogic();
                    List<ApplicantResult> results = applicantJambDetailLogic.GetScannedNDJambApplicantResults(viewModel.Programme, viewModel.Department, viewModel.Session, DateFrom, DateTo.AddDays(1));
                    List<ApplicantResult> masterList = new List<ApplicantResult>();
                    List<string> distinctResult = results.Select(p => p.JambRegNumber).Distinct().ToList();
                    for (int i = 0; i < distinctResult.Count; i++)
                    {
                        masterList.Add(results.FirstOrDefault(r => r.JambRegNumber == distinctResult[i]));
                    }

                    ViewBag.Session = viewModel.AllSessionSelectList;
                    ViewBag.Programme = viewModel.ProgrammeSelectListItem;
                    ViewBag.Department = new SelectList(Utility.PopulateDepartmentSelectListItem(viewModel.Programme), "Value", "Text", viewModel.Department.Id);

                    viewModel.NDApplicantResults = masterList.OrderBy(r => r.Name).ToList();
                    TempData["viewModel"] = viewModel;
                }
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            ViewBag.Session = viewModel.AllSessionSelectList;
            ViewBag.Programme = viewModel.ProgrammeSelectListItem;
            ViewBag.Department = new SelectList(new List<Department>(), "Id", "Name");
            return View(viewModel);
        }

        public ActionResult DownloadScannedResult(long pId)
        {
            try
            {
                if (pId > 0)
                {   
                    SupportViewModel viewModel = (SupportViewModel) TempData["viewModel"];
                    TempData.Keep("viewModel");
                    ApplicantResult Result = viewModel.ApplicantResults.Where(r => r.PersonId == pId).FirstOrDefault();

                    if (Result.ScannedCopyUrl == null || Result.ScannedCopyUrl == "")
                    {
                       SetMessage("Result was not uploaded", Message.Category.Error);
                       return RedirectToAction("DownloadHNDApplicantScannedResultSingle");
                    }
                    string path = Server.MapPath(Result.ScannedCopyUrl);
                    string[] img = Result.ScannedCopyUrl.Split('/');
                    string image = img[4];
                    FileInfo fileInfo = new FileInfo(image);
                    string fileExtension = fileInfo.Extension;

                    return File(path, MediaTypeNames.Application.Octet, Result.Name.Replace(" ","_") + fileExtension);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return RedirectToAction("DownloadHNDApplicantScannedResultSingle");
        }
        public ActionResult DownloadNDJambApplicantScannedResultSingle()
        {
            SupportViewModel viewModel = new SupportViewModel();
            try
            {
                ViewBag.Session = viewModel.AllSessionSelectList;
                ViewBag.Programme = viewModel.ProgrammeSelectListItem;
                ViewBag.Department = new SelectList(new List<Department>(), "Id", "Name");
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);

        }
        [HttpPost]
        public ActionResult DownloadNDJambApplicantScannedResultSingle(SupportViewModel viewModel)
        {  
            try
            {
                DateTime DateFrom = new DateTime();
                DateTime DateTo = new DateTime();

                if (!DateTime.TryParse(viewModel.DateFrom, out DateFrom) || !DateTime.TryParse(viewModel.DateTo, out DateTo))
                {
                    DateFrom = DateTime.Now;
                    DateTo = DateTime.Now;
                }

                ApplicantJambDetailLogic applicantJambDetailLogic = new ApplicantJambDetailLogic();
                List<ApplicantResult> results = applicantJambDetailLogic.GetScannedNDJambApplicantResults(viewModel.Programme, viewModel.Department, viewModel.Session, DateFrom, DateTo);
                List<ApplicantResult> masterList = new List<ApplicantResult>();
                List<string> distinctResult = results.Select(p => p.JambRegNumber).Distinct().ToList();
                for (int i = 0; i < distinctResult.Count; i++)
                {
                    masterList.Add(results.FirstOrDefault(r => r.JambRegNumber == distinctResult[i]));
                }

                ViewBag.Session = viewModel.AllSessionSelectList;
                ViewBag.Programme = viewModel.ProgrammeSelectListItem;
                ViewBag.Department = new SelectList(Utility.PopulateDepartmentSelectListItem(viewModel.Programme), "Value", "Text", viewModel.Department.Id);

                viewModel.ApplicantResults = masterList.OrderBy(r => r.Name).ToList();
                TempData["viewModel"] = viewModel;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            ViewBag.Session = viewModel.AllSessionSelectList;
            ViewBag.Programme = viewModel.ProgrammeSelectListItem;
            ViewBag.Department = new SelectList(new List<Department>(), "Id", "Name");
            return View(viewModel);
        }

        public ActionResult DownloadScannedResultND(long pId)
        {
            try
            {
                if (pId > 0)
                {
                    SupportViewModel viewModel = (SupportViewModel)TempData["viewModel"];
                    TempData.Keep("viewModel");
                    ApplicantResult Result = viewModel.ApplicantResults.Where(r => r.PersonId == pId).FirstOrDefault();

                    if (Result.ScannedCopyUrl == null || Result.ScannedCopyUrl == "")
                    {
                        SetMessage("Result was not uploaded", Message.Category.Error);
                        return RedirectToAction("DownloadNDJambApplicantScannedResultSingle");
                    }
                    string path = Server.MapPath(Result.ScannedCopyUrl);
                    string[] img = Result.ScannedCopyUrl.Split('/');
                    string image = img[4];
                    FileInfo fileInfo = new FileInfo(image);
                    string fileExtension = fileInfo.Extension;

                    return File(path, MediaTypeNames.Application.Octet, Result.ApplicationFormNumber + fileExtension);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return RedirectToAction("DownloadNDJambApplicantScannedResultSingle");
        }

        public ActionResult UpdateApplicationNumber()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UpdateApplicationNumberPost()
        {
            try
            {
                ApplicationFormLogic applicationFormLogic = new ApplicationFormLogic();
                AdmissionListLogic admissionListLogic = new AdmissionListLogic();

                List<DuplicateApplicationNumber> applicationForms = applicationFormLogic.GetDuplicateApplicationForm();
                List<string> distinctApplicationNumber = applicationForms.Select(a => a.Number).Distinct().ToList();
                
                long? maxSerial = applicationFormLogic.GetEntitiesBy(r => r.PAYMENT.Session_Id == 7).Max(r => r.Serial_Number);
                ApplicationForm maxApplicationForm = applicationFormLogic.GetModelBy(r => r.Serial_Number == maxSerial && r.PAYMENT.Session_Id == 7);
                string[] splitLstAppNumber = maxApplicationForm.Number.Split('/');
                long startCount = Convert.ToInt64(splitLstAppNumber[3]) + 1;

                for (int i = 0; i < distinctApplicationNumber.Count; i++)
                {
                    List<DuplicateApplicationNumber> duplicateNumbers = applicationForms.Where(a => a.Number == distinctApplicationNumber[i]).ToList();

                    for (int j = 0; j < duplicateNumbers.Count; j++)
                    {
                        DuplicateApplicationNumber currentDuplicateApplicationNumber = duplicateNumbers[j];
                        ApplicationForm currentApplicationForm = applicationFormLogic.GetModelBy(a => a.Application_Form_Id == currentDuplicateApplicationNumber.ApplicationId);
                        AdmissionList admissionList = admissionListLogic.GetModelBy(a => a.Application_Form_Id == currentDuplicateApplicationNumber.ApplicationId);
                        if (admissionList == null || (admissionList == null && currentApplicationForm.ProgrammeFee.Programme.Id == 1))
                        {  
                            string[] splitAppform = currentApplicationForm.Number.Split('/');
                            long newNumber = startCount;
                            string newAppNumber = splitAppform[0] + "/" + splitAppform[1] + "/" + splitAppform[2] + "/" + "00000" + newNumber;

                            ApplicationForm checkApplicationForm = applicationFormLogic.GetModelBy(a => a.Application_Form_Number == newAppNumber);
                            if (checkApplicationForm == null)
                            {
                                string Operation = "Modified the student app number from  " + currentApplicationForm.Number + " To " + newAppNumber;

                                currentApplicationForm.Number = newAppNumber;
                                currentApplicationForm.SerialNumber = newNumber;
                                applicationFormLogic.Modify(currentApplicationForm);
                                GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();
                               
                                string Action = "MODIFY";
                                string Table = "Student Table";
                                generalAuditLogic.CreateGeneralAudit(Action, Operation, Table);

                                startCount++;
                            }
                        }
                    }
                }

                SetMessage("Operation Successful! ", Message.Category.Information);
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View("UpdateApplicationNumber");
        }
        public ActionResult UpdateExamNumber()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UpdateExamNumberPost()
        {
            try
            {
                AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
                ApplicationFormLogic applicationFormLogic = new ApplicationFormLogic();
                List<APPLICATION_FORM> applicationForms = applicationFormLogic.GetEntitiesBy(a => a.Application_Exam_Number == null && a.PAYMENT.Session_Id == 7 && (a.Application_Programme_Fee_Id == 11 || a.Application_Programme_Fee_Id == 10));
                for (int i = 0; i < applicationForms.Count; i++)
                {
                    long applicationId = applicationForms[i].Application_Form_Id;
                    if (applicationId == 0)
                    {
                        continue;
                    }
                    ApplicationForm applicationForm = applicationFormLogic.GetModelBy(a => a.Application_Form_Id == applicationId);
                    AppliedCourse appliedCourse = appliedCourseLogic.GetModelBy(a => a.Application_Form_Id == applicationId);

                    ApplicationForm newApplicationForm = applicationFormLogic.SetNextExamNumber(applicationForm, appliedCourse);
                    ApplicationForm checkApplicationForm = applicationFormLogic.GetModelBy(a => a.Application_Exam_Number == newApplicationForm.ExamNumber && a.PAYMENT.Session_Id == newApplicationForm.Payment.Session.Id && a.Application_Programme_Fee_Id == newApplicationForm.ProgrammeFee.Id);
                    if (checkApplicationForm == null)
                    {
                        string Operation = "Modified the student Exam number from  " + applicationForm.ExamNumber + " To " + newApplicationForm.ExamNumber;

                        applicationFormLogic.Modify(newApplicationForm);
                        GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();

                        string Action = "MODIFY";
                        string Table = "Student Table";
                        generalAuditLogic.CreateGeneralAudit(Action, Operation, Table);

                    }
                }

                SetMessage("Operation Successful! ", Message.Category.Information);
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View("UpdateExamNumber");
        }
        public ActionResult DownloadStudentPassport()
        {
            SupportViewModel viewModel = new SupportViewModel();
            try
            {
                ViewBag.Session = viewModel.SessionSelectList;
                ViewBag.Programme = viewModel.ProgrammeSelectListItem;
                ViewBag.Department = new SelectList(new List<Department>(), ID, NAME);
                ViewBag.Level = new SelectList(viewModel.LevelList, ID, NAME);

            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult DownloadStudentPassport(SupportViewModel viewModel)
        {
            try
            {
                if (viewModel != null)
                {
                    StudentLevelLogic studentLevelLogic = new StudentLevelLogic();

                    List<StudentLevel> studentLevels = studentLevelLogic.GetModelsBy(s => s.Session_Id == viewModel.Session.Id && s.Department_Id == viewModel.Department.Id && s.Programme_Id == viewModel.Programme.Id && s.Level_Id == viewModel.Level.Id).OrderBy(a => a.Student.MatricNumber).ToList();

                    if (studentLevels.Count > 0)
                    {
                        if (Directory.Exists(Server.MapPath("~/Content/tempFolder")))
                        {
                            Directory.Delete(Server.MapPath("~/Content/tempFolder"), true);
                            Directory.CreateDirectory(Server.MapPath("~/Content/tempFolder"));
                        }
                        else
                        {
                            Directory.CreateDirectory(Server.MapPath("~/Content/tempFolder"));
                        }

                        List<StudentBioDataFormat> sort = new List<StudentBioDataFormat>();

                        GridView gv = new GridView();
                        RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                        bool noDateRange = true;
                        DateTime from = DateTime.Now;
                        DateTime to = DateTime.Now;
                        if (viewModel.DateFrom != null && viewModel.DateTo != null)
                        {
                            from = Convert.ToDateTime(viewModel.DateFrom);
                            to = Convert.ToDateTime(viewModel.DateTo);
                            noDateRange = false;
                        }
                            
                        for (int i = 0; i < studentLevels.Count; i++)
                        {
                           
                            StudentLevel currentStudentLevel = studentLevels[i];
                            if (!noDateRange)
                            {
                                var needToBePulled=remitaPaymentLogic.GetModelsBy(f => f.PAYMENT.Fee_Type_Id == (int)FeeTypes.SchoolFees
                                && f.PAYMENT.Session_Id == viewModel.Session.Id && f.PAYMENT.Person_Id == currentStudentLevel.Student.Id && f.Transaction_Date >= from && f.Transaction_Date <= to).FirstOrDefault();
                                if (needToBePulled == null)
                                    continue;
                            }
                            string imagePath = currentStudentLevel.Student.ImageFileUrl;
                            string signaturePath = currentStudentLevel.Student.SignatureFileUrl;

                            if (imagePath == null || imagePath == "")
                            {
                                continue;
                            }

                            string[] splitUrl = imagePath.Split('/');
                            string imageUrl = splitUrl[3];
                            
                            string[] splitSignatureUrl = imagePath.Split('/');
                            string signatureUrl = splitSignatureUrl[3];

                            FileInfo fileInfo = new FileInfo(imageUrl);
                            string fileExtension = fileInfo.Extension;
                            string newFileName = currentStudentLevel.Student.MatricNumber.Replace("/", "_") + fileExtension;

                            FileInfo sigFileInfo = new FileInfo(signatureUrl);
                            string sigFileExtension = fileInfo.Extension;
                            string sigNewFileName = "sign_" + currentStudentLevel.Student.MatricNumber.Replace("/", "_") + fileExtension;

                            if (!System.IO.File.Exists(Server.MapPath("~" + imagePath)))
                            {
                                continue;
                            }

                            if (System.IO.File.Exists(Server.MapPath("~" + signaturePath)))
                            {

                                System.IO.File.Copy(Server.MapPath(signaturePath), Server.MapPath(Path.Combine("~/Content/tempFolder/",  sigNewFileName)), true);
                            }

                            System.IO.File.Copy(Server.MapPath(imagePath),Server.MapPath(Path.Combine("~/Content/tempFolder/", newFileName)), true);

                            StudentBioDataFormat format = new StudentBioDataFormat();
                            format.Session = studentLevels.FirstOrDefault().Session.Name;
                            format.Department = studentLevels.FirstOrDefault().Department.Name;
                            format.Programme = studentLevels.FirstOrDefault().Programme.Name;
                            format.Level = studentLevels.FirstOrDefault().Level.Name;
                            format.Name = currentStudentLevel.Student.FullName;
                            format.MatricNumber = currentStudentLevel.Student.MatricNumber;
                            format.PassportUrl = "/" + (studentLevels.FirstOrDefault().Department.Code + "_" + studentLevels.FirstOrDefault().Level.Name + "_" + studentLevels.FirstOrDefault().Programme.Name).Replace(" ", "") + "/" + newFileName;
                            format.SignatureUrl = "/" + (studentLevels.FirstOrDefault().Department.Code + "_" + studentLevels.FirstOrDefault().Level.Name + "_" + studentLevels.FirstOrDefault().Programme.Name).Replace(" ", "") + "/" + sigNewFileName;
                            format.BloodGroup = currentStudentLevel.Student.BloodGroup!=null?currentStudentLevel.Student.BloodGroup.Name:"N/A";
                            format.Genotype = currentStudentLevel.Student.Genotype!=null?currentStudentLevel.Student.Genotype.Name: "N/A";


                            format.SN = i + 1;
                            sort.Add(format);

                        }
                        //sort.OrderBy(s => s.MatricNumber);
                        gv.DataSource = sort;
                        gv.Caption = studentLevels.FirstOrDefault().Department.Name + " " + studentLevels.FirstOrDefault().Level.Name + " " + studentLevels.FirstOrDefault().Programme.Name + " " + studentLevels.FirstOrDefault().Session.Name + " " + "SESSION";
                        gv.DataBind();
                        SaveStudentDetailsToExce(gv, "Student Details.xls");

                        string zipFileName = (studentLevels.FirstOrDefault().Department.Code + "_" + studentLevels.FirstOrDefault().Level.Name + "_" + studentLevels.FirstOrDefault().Programme.Name).Replace(" ", "") + ".zip";

                        using (ZipFile zip = new ZipFile())
                        {
                            string file = Server.MapPath("~/Content/tempFolder/");
                            zip.AddDirectory(file, "");

                            zip.Save(file + zipFileName);
                        }

                        string savedFile = "~/Content/tempFolder/" + zipFileName;
                        RetainDropdownState(viewModel);

                        return File(Server.MapPath(savedFile), "application/zip", zipFileName);
                    }

                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occured!" + ex.Message, Message.Category.Error);
            }

            RetainDropdownState(viewModel);
            return View(viewModel);
        }
        public ActionResult DownloadApplicantPassport()
        {
            SupportViewModel viewModel = new SupportViewModel();
            try
            {
                ViewBag.Session = viewModel.SessionSelectList;
                ViewBag.Programme = viewModel.ProgrammeSelectListItem;
                ViewBag.Department = new SelectList(new List<Department>(), ID, NAME);

            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult DownloadApplicantPassport(SupportViewModel viewModel)
        {
            try
            {
                if (viewModel != null)
                {
                    ApplicationFormLogic applicationFormLogic = new ApplicationFormLogic();

                    List<PhotoCard> applicants = applicationFormLogic.GetPostJAMBApplicationsBy(viewModel.Session, viewModel.Programme, viewModel.Department, SortOption.Name);

                    if (applicants.Count > 0)
                    {
                        if (Directory.Exists(Server.MapPath("~/Content/tempFolder")))
                        {
                            Directory.Delete(Server.MapPath("~/Content/tempFolder"), true);
                            Directory.CreateDirectory(Server.MapPath("~/Content/tempFolder"));
                        }
                        else
                        {
                            Directory.CreateDirectory(Server.MapPath("~/Content/tempFolder"));
                        }

                        List<StudentDetailFormat> sort = new List<StudentDetailFormat>();

                        GridView gv = new GridView();

                        for (int i = 0; i < applicants.Count; i++)
                        {
                            PhotoCard applicantPhotoCard = applicants[i];
                            string imagePath = applicantPhotoCard.PassportUrl;

                            if (imagePath == null || imagePath == "")
                            {
                                continue;
                            }

                            string[] splitUrl = imagePath.Split('/');
                            string imageUrl = splitUrl[3];
                            FileInfo fileInfo = new FileInfo(imageUrl);
                            string fileExtension = fileInfo.Extension;
                            string newFileName = applicantPhotoCard.AplicationFormNumber.Replace("/", "_") + fileExtension;

                            if (!System.IO.File.Exists(Server.MapPath("~" + imagePath)))
                            {
                                continue;
                            }

                            System.IO.File.Copy(Server.MapPath(imagePath), Server.MapPath(Path.Combine("~/Content/tempFolder/", newFileName)), true);

                            StudentDetailFormat format = new StudentDetailFormat();
                            format.Session = applicantPhotoCard.SessionName;
                            format.Department = applicantPhotoCard.FirstChoiceDepartment;
                            format.Programme = applicantPhotoCard.AppliedProgrammeName;
                            format.Name = applicantPhotoCard.Name;
                            format.MatricNumber = applicantPhotoCard.AplicationFormNumber;
                            format.PassportUrl = "/" + (applicantPhotoCard.FirstChoiceDepartment.Replace("/", "_") + "_" + applicantPhotoCard.AppliedProgrammeName).Replace(" ", "") + "/" + newFileName;

                            format.SN = i + 1;
                            sort.Add(format);

                        }

                        
                        gv.DataSource = sort;
                        gv.Caption = applicants.FirstOrDefault().FirstChoiceDepartment + " " + applicants.FirstOrDefault().AppliedProgrammeName + " " + applicants.FirstOrDefault().SessionName + " " + "SESSION";
                        gv.DataBind();
                        SaveStudentDetailsToExce(gv, "Applicant Details.xls");

                        string zipFileName = (applicants.FirstOrDefault().FirstChoiceDepartment.Replace("/", "_") + "_" + applicants.FirstOrDefault().AppliedProgrammeName).Replace(" ", "") + ".zip";

                        using (ZipFile zip = new ZipFile())
                        {
                            string file = Server.MapPath("~/Content/tempFolder/");
                            zip.AddDirectory(file, "");

                            zip.Save(file + zipFileName);
                        }

                        string savedFile = "~/Content/tempFolder/" + zipFileName;
                        RetainDropdownState(viewModel);

                        return File(Server.MapPath(savedFile), "application/zip", zipFileName);
                    }

                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occured!" + ex.Message, Message.Category.Error);
            }

            RetainDropdownState(viewModel);
            return View(viewModel);
        }
        public void SaveStudentDetailsToExce(GridView ExcelGridView, string fileName)
        {
            try
            {
                Response.Clear();

                //Response.AddHeader("content-disposition", "attachment;filename=" + fileName);

                Response.Charset = "";

                Response.Cache.SetCacheability(HttpCacheability.NoCache);

                Response.ContentType = "application/vnd.ms-excel";

                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);
                ExcelGridView.RenderControl(htw);

                Response.Write(sw.ToString());
                string renderedGridView = sw.ToString();
                System.IO.File.WriteAllText(Server.MapPath(Path.Combine("~/Content/tempFolder/", fileName)), renderedGridView);

            }
            catch (Exception)
            {

                throw;
            }
        }
        public ActionResult DownloadApplicantScannedOLevelBulk()
        {
            SupportViewModel viewModel = new SupportViewModel();
            try
            {
                ViewBag.Session = viewModel.SessionSelectList;
                ViewBag.Programme = viewModel.ProgrammeSelectListItem;
                ViewBag.Department = new SelectList(new List<Department>(), ID, NAME); 
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult DownloadApplicantScannedOLevelBulk(SupportViewModel viewModel)
        {
            try
            {
                if (viewModel != null)
                {
                    DateTime DateFrom = new DateTime();
                    DateTime DateTo = new DateTime();

                    if (!DateTime.TryParse(viewModel.DateFrom, out DateFrom) || !DateTime.TryParse(viewModel.DateTo, out DateTo))
                    {
                        DateFrom = DateTime.Now;
                        DateTo = DateTime.Now;
                    }

                    string tempFolder = "~/Content/TempFolder" + DateTime.Now.ToString().Replace("/", "").Replace(":", "").Replace(" ", "");

                    ApplicantLogic applicantLogic = new ApplicantLogic();
                    List<ApplicantResult> applicantResults = new List<ApplicantResult>();

                    applicantResults = applicantLogic.GetApplicantDetails(viewModel.Programme, viewModel.Department, viewModel.Session, DateFrom, DateTo);
                    List<string> distinctScannedCopy = applicantResults.Select(a => a.ScannedCopyUrl).Distinct().ToList();

                    if (Directory.Exists(Server.MapPath(tempFolder)))
                    {
                        Directory.Delete(Server.MapPath(tempFolder), true);
                        Directory.CreateDirectory(Server.MapPath(tempFolder));
                    }
                    else
                    {
                        Directory.CreateDirectory(Server.MapPath(tempFolder));
                    }

                    for (int i = 0; i < distinctScannedCopy.Count; i++)
                    {
                        ApplicantResult currentResult = applicantResults.FirstOrDefault(a => a.ScannedCopyUrl == distinctScannedCopy[i]);
                        string resultPath = currentResult.ScannedCopyUrl;
                        if (string.IsNullOrEmpty(resultPath))
                        {
                            continue;
                        }
                        string[] splitUrl = resultPath.Split('/');
                        string resultUrl = splitUrl[4];
                        FileInfo fileInfo = new FileInfo(resultUrl);
                        string fileExtension = fileInfo.Extension;
                        string newFileName = currentResult.Name.Replace(" ", "_") + "_" + currentResult.NumberOfSittings + fileExtension;

                        if (!System.IO.File.Exists(Server.MapPath("~" + resultPath)))
                        {
                            continue;
                        }

                        System.IO.File.Copy(Server.MapPath(resultPath), Server.MapPath(Path.Combine(tempFolder, newFileName)), true);
                    }
                    using (ZipFile zip = new ZipFile())
                    {
                        string file = Server.MapPath(tempFolder);
                        zip.AddDirectory(file, "");
                        string zipFileName = (applicantResults.FirstOrDefault().Department + "_" + applicantResults.FirstOrDefault().Programme).Replace(" ", "").Replace("/", "_");
                        zip.Save(file + zipFileName + ".zip");
                    }

                    string savedFile = tempFolder + (applicantResults.FirstOrDefault().Department + "_" + applicantResults.FirstOrDefault().Programme).Replace(" ", "").Replace("/", "_") + ".zip";
                    RetainDropdownState(viewModel);

                    return File(Server.MapPath(savedFile), "application/zip", (applicantResults.FirstOrDefault().Department + "_" + applicantResults.FirstOrDefault().Programme).Replace(" ", "").Replace("/", "_") + ".zip");
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occured!" + ex.Message, Message.Category.Error);
            }

            RetainDropdownState(viewModel);
            return View(viewModel);
        }
        public ActionResult UpdateCourseOption()
        {
            SupportViewModel viewModel = new SupportViewModel();
            try
            {
                Programme programme = new Programme() { Id = 3};
                DepartmentLogic departmentLogic = new DepartmentLogic();
                List<Department> departments = departmentLogic.GetBy(programme);
                departments.Insert(0, new Department(){ Id = 0, Name = "-- Select Department --"});

                ViewBag.Session = viewModel.SessionSelectList;
                ViewBag.Department = new SelectList(departments, ID, NAME);
                ViewBag.Level = new SelectList(viewModel.LevelList, ID, NAME);
                ViewBag.Semester = new SelectList(new List<Semester>(), ID, NAME);

            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult UpdateCourseOption(SupportViewModel viewModel)
        {
            Programme programme = new Programme() { Id = 3 };
            try
            {
                if (viewModel != null)
                {
                    string operation = "MODIFY";
                    string action = "ADMIN :CHANGES FROM ADMIN CONSOLE (StudentController)";
                    string client = Request.LogonUserIdentity.Name + " (" + HttpContext.Request.UserHostAddress + ")";
                    var courseRegistrationDetailAudit = new CourseRegistrationDetailAudit();
                    courseRegistrationDetailAudit.Action = action;
                    courseRegistrationDetailAudit.Operation = operation;
                    courseRegistrationDetailAudit.Client = client;
                     UserLogic loggeduser = new UserLogic();
                courseRegistrationDetailAudit.User = loggeduser.GetModelBy(u => u.User_Name == User.Identity.Name);


                    StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                    CourseLogic courseLogic = new CourseLogic();
                    CourseRegistrationDetailLogic courseRegistrationDetailLogic = new CourseRegistrationDetailLogic();
                    StudentExamRawScoreSheetResultLogic studentExamRawScoreSheetResultLogic = new StudentExamRawScoreSheetResultLogic();
                    StudentResultLogic studentResultLogic = new StudentResultLogic();
                    CourseAllocationLogic courseAllocationLogic = new CourseAllocationLogic();
                    CourseEvaluationAnswerLogic courseEvaluationAnswerLogic = new CourseEvaluationAnswerLogic();
                    PersonLogic personLogic = new PersonLogic();
                    StudentResultDetailLogic studentResultDetailLogic = new StudentResultDetailLogic();
                    SessionSemesterLogic sessionSemesterLogic = new SessionSemesterLogic();
                    DepartmentOptionLogic departmentOptionLogic = new DepartmentOptionLogic();

                    Model.Model.Session session = viewModel.Session;
                    Semester semester = viewModel.Semester;
                    Level level = viewModel.Level;
                    Department department = viewModel.Department;
                    
                    SessionSemester sessionSemester = sessionSemesterLogic.GetModelBy(s => s.Session_Id == session.Id && s.Semester_Id == semester.Id);
                    List<DepartmentOption> departmentOptions = departmentOptionLogic.GetBy(department, programme);
                    List<long> wrongAllocations = new List<long>();

                    for (int l = 0; l < departmentOptions.Count; l++)
                    {
                        DepartmentOption departmentOption = departmentOptions[l];
                        List<StudentLevel> studentLevelList = studentLevelLogic.GetModelsBy(s => s.Programme_Id == programme.Id && s.Department_Id == department.Id && s.Department_Option_Id == departmentOption.Id && s.Session_Id == session.Id && s.Level_Id == level.Id);
                        List<long> personIdList = studentLevelList.Select(s => s.Student.Id).Distinct().ToList();
                        List<Course> courseList = courseLogic.GetModelsBy(c => c.Department_Id == department.Id && c.Department_Option_Id == departmentOption.Id && c.Level_Id == level.Id && c.Semester_Id == semester.Id);
                        
                        long wrongAllocationId = 0;

                        for (int i = 0; i < courseList.Count; i++)
                        {
                            Course correctCourse = courseList[i];
                            CourseAllocation allocatedCourse = courseAllocationLogic.GetModelsBy(c => c.Session_Id == session.Id && c.Semester_Id == semester.Id && c.COURSE.Course_Code == correctCourse.Code && c.Department_Id == department.Id && c.Level_Id == level.Id && c.Programme_Id == programme.Id).LastOrDefault();
                            if (allocatedCourse != null && allocatedCourse.Course == null)
                            {
                               allocatedCourse = courseAllocationLogic.GetModelBy(c => c.Course_Allocation_Id == allocatedCourse.Id); 
                            }
                            if (allocatedCourse != null)
                            {
                                Course wrongAllocatedCourse = allocatedCourse.Course;
                                wrongAllocationId = allocatedCourse.Id;
                                if (wrongAllocatedCourse != null)
                                {
                                    if (wrongAllocatedCourse.Id != correctCourse.Id)
                                    {
                                        if (wrongAllocatedCourse.DepartmentOption == null)
                                        {
                                            wrongAllocations.Add(wrongAllocationId);
                                        }
                                    }  
                                }
                            }

                            for (int j = 0; j < personIdList.Count; j++)
                            {
                                using (TransactionScope scope = new TransactionScope())
                                {
                                    long currentPersonId = personIdList[j];
                                    Person person = personLogic.GetModelBy(p => p.Person_Id == currentPersonId);

                                    StudentExamRawScoreSheet studentExamRawScoreSheet = studentExamRawScoreSheetResultLogic.GetModelsBy(s => s.COURSE.Course_Code == correctCourse.Code && s.Student_Id == currentPersonId && s.Session_Id == session.Id).LastOrDefault();
                                    List<CourseRegistrationDetail> courseRegistrationDetails = courseRegistrationDetailLogic.GetModelsBy(c => c.COURSE.Course_Code == correctCourse.Code && c.STUDENT_COURSE_REGISTRATION.Person_Id == currentPersonId && c.STUDENT_COURSE_REGISTRATION.Session_Id == session.Id);
                                    List<StudentResultDetail> studentResultDetails = studentResultDetailLogic.GetModelsBy(s => s.COURSE.Course_Code == correctCourse.Code && s.Person_Id == currentPersonId && s.STUDENT_RESULT.Session_Semester_Id == sessionSemester.Id);
                                    CourseEvaluationAnswer courseEvaluationAnswer = courseEvaluationAnswerLogic.GetModelsBy(c => c.COURSE.Course_Code == correctCourse.Code && c.Person_Id == currentPersonId && c.Session_Id == session.Id && c.Semester_Id == semester.Id).LastOrDefault();

                                    //Update CourseRegistrationDetail
                                    if (courseRegistrationDetails.Count > 0 )
                                    {
                                        for (int o = 0; o < courseRegistrationDetails.Count; o++)
                                        {
                                            CourseRegistrationDetail courseRegistrationDetail = courseRegistrationDetails[o];
                                            //if (courseRegistrationDetail.Course.Id == correctCourse.Id)
                                            //{
                                            //    continue;
                                            //}
                                            courseRegistrationDetail.Course = correctCourse;
                                            if (studentExamRawScoreSheet != null)
                                            {
                                                courseRegistrationDetail.TestScore = Convert.ToDecimal(studentExamRawScoreSheet.T_CA);
                                                courseRegistrationDetail.ExamScore = Convert.ToDecimal(studentExamRawScoreSheet.T_EX);
                                                if (courseRegistrationDetail.CourseUnit == 0 || courseRegistrationDetail.CourseUnit == null)
                                                {
                                                    courseRegistrationDetail.CourseUnit = studentExamRawScoreSheet.Course.Unit;
                                                }
                                            }

                                            courseRegistrationDetailLogic.Modify(courseRegistrationDetail,courseRegistrationDetailAudit);
                                        }  
                                    }

                                    //Update StudentExamRawScore
                                    if (studentExamRawScoreSheet != null)
                                    {
                                        if (studentExamRawScoreSheet.Course.Id != correctCourse.Id)
                                        {
                                            studentExamRawScoreSheet.Course = correctCourse;
                                            studentExamRawScoreSheetResultLogic.Modify(studentExamRawScoreSheet);
                                        }  
                                    }

                                    //Update StudentResultDetail
                                    if (studentResultDetails.Count > 0)
                                    {
                                        for (int k = 0; k < studentResultDetails.Count; k++)
                                        {
                                            StudentResultDetail thisResultDetail = studentResultDetails[k];
                                            if (thisResultDetail != null && thisResultDetail.Course == null)
                                            {
                                               thisResultDetail = studentResultDetailLogic.GetModelBy(rd => rd.Student_Result_Id == thisResultDetail.Header.Id); 
                                            }
                                            if (thisResultDetail.Course.Id == correctCourse.Id)
                                            {
                                                continue;
                                            }
                                            StudentResultDetail newResultDetail = new StudentResultDetail();
                                            newResultDetail = thisResultDetail;
                                            
                                            Expression<Func<STUDENT_RESULT_DETAIL, bool>> selector = s => s.Student_Result_Id == thisResultDetail.Header.Id;

                                            if (studentResultDetailLogic.Delete(selector))
                                            {
                                                newResultDetail.Course = correctCourse;
                                                studentResultDetailLogic.Create(newResultDetail);
                                            }
                                            //studentResultDetailLogic.Modify(studentResultDetails[k], sessionSemester);
                                        }
                                    }

                                    //Update CourseEvaluation
                                    if (courseEvaluationAnswer != null)
                                    {
                                        if (courseEvaluationAnswer.Course != null && courseEvaluationAnswer.Course.Id != correctCourse.Id)
                                        {
                                            courseEvaluationAnswer.Course = correctCourse;
                                            courseEvaluationAnswerLogic.Modify(courseEvaluationAnswer);
                                        }  
                                    }

                                    scope.Complete();
                                }   
                            }

                            //Finally Add CourseAllocation
                            if (allocatedCourse != null)
                            {
                                if (allocatedCourse.Course.Id != correctCourse.Id)
                                {
                                    List<CourseAllocation> checkPreviousAllocation = courseAllocationLogic.GetModelsBy(c => c.Session_Id == session.Id && c.Course_Id == correctCourse.Id && c.Department_Id == department.Id && c.Programme_Id == programme.Id && c.Level_Id == level.Id);
                                    if (checkPreviousAllocation.Count == 0)
                                    {
                                        CourseAllocation courseAllocation = new CourseAllocation();
                                        courseAllocation = allocatedCourse;
                                        courseAllocation.Course = correctCourse;
                                        courseAllocationLogic.Create(courseAllocation);
                                    }
                                }  
                            } 
                        }
                    }

                    if (wrongAllocations.Count > 0)
                    {
                        for (int m = 0; m < wrongAllocations.Count; m++)
                        {
                            long currentAllocationId = wrongAllocations[m];
                            courseAllocationLogic.Delete(c => c.Course_Allocation_Id == currentAllocationId);
                        } 
                    }

                    SetMessage("Operation Successful! ", Message.Category.Information);
                }  
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }


            DepartmentLogic departmentLogic = new DepartmentLogic();
            List<Department> departments = departmentLogic.GetBy(programme);
            departments.Insert(0, new Department() { Id = 0, Name = "-- Select Department --" });

            ViewBag.Session = viewModel.SessionSelectList;
            ViewBag.Department = new SelectList(departments, ID, NAME, viewModel.Department.Id);
            ViewBag.Level = new SelectList(viewModel.LevelList, ID, NAME);
            ViewBag.Semester = new SelectList(new List<Semester>(), ID, NAME);

            return View(viewModel);
        }

        public ActionResult ViewApplicantJambRegDetail()
        {
            SupportViewModel viewModel = new SupportViewModel();
            try
            {
                
                
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult ViewApplicantJambRegDetail(SupportViewModel viewModel)
        {
            try
            {
                if (viewModel.ApplicationForm.Number != null)
                {
                    ApplicantJambDetailLogic applicantJambDetailLogic = new ApplicantJambDetailLogic();
                    ChangeOfCourseLogic changeOfCourseLogic = new ChangeOfCourseLogic();
                    List<ApplicantJambDetail> applicantJambDetailList = new List<ApplicantJambDetail>(); 
                    List<ChangeOfCourse> changeOfCourseList = new List<ChangeOfCourse>();

                    if (viewModel.ApplicationForm.Number.Length < 15)
                    {
                        applicantJambDetailList = applicantJambDetailLogic.GetModelsBy(a => a.APPLICATION_FORM.Application_Form_Id == Convert.ToInt64(viewModel.ApplicationForm.Number)); 
                    }
                    else
                    {
                        applicantJambDetailList = applicantJambDetailLogic.GetModelsBy(a => a.APPLICATION_FORM.Application_Form_Number == viewModel.ApplicationForm.Number); 
                    }
                    for (int i = 0; i < applicantJambDetailList.Count; i++)
                    {
                        ApplicantJambDetail applicantJambDetail = applicantJambDetailList[i];
                        ChangeOfCourse changeOfCourse = changeOfCourseLogic.GetModelsBy(c => c.Jamb_Registration_Number == applicantJambDetail.JambRegistrationNumber).LastOrDefault();
                        if (changeOfCourse != null)
                        {
                           changeOfCourseList.Add(changeOfCourse); 
                        }
                    }

                    viewModel.ChangeOfCourseList = changeOfCourseList;
                    viewModel.ApplicantJambDetailList = applicantJambDetailList;
                }

            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }

        public ActionResult EditApplicantJambRegDetail(int pid)
        {
            SupportViewModel viewModel = new SupportViewModel();
            try
            {
                if (pid > 0)
                {
                    ApplicantJambDetailLogic applicantJambDetailLogic = new ApplicantJambDetailLogic();
                    ApplicantJambDetail applicantJambDetail = applicantJambDetailLogic.GetModelsBy(a => a.Person_Id == pid).LastOrDefault();
                    
                    viewModel.ApplicantJambDetail = applicantJambDetail;
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult EditApplicantJambRegDetail(SupportViewModel viewModel)
        {
            try
            {
                if (viewModel.ApplicantJambDetail != null)
                {
                    ApplicantJambDetailLogic applicantJambDetailLogic = new ApplicantJambDetailLogic();
                    ApplicationFormLogic applicationFormLogic = new ApplicationFormLogic();
                    ApplicantJambDetail applicantJambDetail = applicantJambDetailLogic.GetModelsBy(a => a.Person_Id == viewModel.ApplicantJambDetail.Person.Id).LastOrDefault();
                    ApplicationForm applicationForm = applicationFormLogic.GetModelsBy(a => a.Application_Form_Number == viewModel.ApplicantJambDetail.ApplicationForm.Number).LastOrDefault();
                    
                    applicantJambDetail.ApplicationForm = applicationForm;
                    applicantJambDetail.JambRegistrationNumber = viewModel.ApplicantJambDetail.JambRegistrationNumber;
                    string Operation = "Modified Applicant Jamb Detail of " + applicationForm.ExamNumber;

                  
                    GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();

                    string Action = "MODIFY";
                    string Table = "APPlicant Table";
                    generalAuditLogic.CreateGeneralAudit(Action, Operation, Table);

                    applicantJambDetailLogic.Modify(applicantJambDetail);
                    SetMessage("Operation Successful! ", Message.Category.Information);

                }
            }
            catch (Exception)
            {
                throw;
            }

            return View(viewModel);
        }

        public ActionResult EditChangeOfCourse(int cid)
        {
            SupportViewModel viewModel = new SupportViewModel();
            try
            {
                if (cid > 0)
                {
                    ChangeOfCourseLogic changeOfCourseLogic = new ChangeOfCourseLogic();
                    ChangeOfCourse changeOfCourse = changeOfCourseLogic.GetModelsBy(a => a.Change_Of_Course_Id == cid).LastOrDefault();

                    viewModel.ChangeOfCourse = changeOfCourse;
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult EditChangeOfCourse(SupportViewModel viewModel)
        {
            try
            {
                if (viewModel.ChangeOfCourse != null)
                {
                    ChangeOfCourseLogic changeOfCourseLogic = new ChangeOfCourseLogic();
                    ApplicationFormLogic applicationFormLogic = new ApplicationFormLogic();
                    ChangeOfCourse changeOfCourse = changeOfCourseLogic.GetModelsBy(a => a.Change_Of_Course_Id == viewModel.ChangeOfCourse.Id).LastOrDefault();
                    ApplicationForm applicationForm = applicationFormLogic.GetModelsBy(a => a.Application_Form_Number == viewModel.ApplicationForm.Number).LastOrDefault();

                    changeOfCourse.JambRegistrationNumber = viewModel.ChangeOfCourse.JambRegistrationNumber;
                    changeOfCourse.ApplicationForm = applicationForm;
                    changeOfCourse.NewPerson = new Person(){ Id = Convert.ToInt64(viewModel.ChangeOfCourse.NewPerson.Id)};
                    changeOfCourse.OldPerson = new Person(){ Id = Convert.ToInt64(viewModel.ChangeOfCourse.OldPerson.Id)};
                    string Operation = "Changed Course for   " + applicationForm.Number;


                    GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();

                    string Action = "MODIFY";
                    string Table = "Change of Course Table";
                    generalAuditLogic.CreateGeneralAudit(Action, Operation, Table);

                    changeOfCourseLogic.Modify(changeOfCourse);

                    SetMessage("Operation Successful! ", Message.Category.Information);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        private void RetainPaymentDropDownState(Payment payment)
        {
            try
            {
                if (payment != null)
                {
                    if (payment.Session != null)
                    {
                        ViewBag.Session = new SelectList(Utility.GetAllSessions(), "Id", "Name", payment.Session.Id);
                    }
                    else
                        ViewBag.Session = Utility.GetAllSessions();
                    ViewBag.PaymentMode = new SelectList(Utility.PopulatePaymentModeSelectListItem(), "Value", "Text", payment.PaymentMode.Id);
                    ViewBag.FeeType = new SelectList(Utility.PopulateFeeTypeSelectListItem(), "Value", "Text", payment.FeeType.Id);
                    ViewBag.Level = Utility.PopulateLevelSelectListItem();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private DataSet ReadExcel(string filepath)
        {
            DataSet Result = null;
            try
            {
                string xConnStr = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + filepath + ";" + "Extended Properties=Excel 8.0;";
                OleDbConnection connection = new OleDbConnection(xConnStr);

                connection.Open();
                DataTable sheet = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                foreach (DataRow dataRow in sheet.Rows)
                {
                    string sheetName = dataRow[2].ToString().Replace("'", "");
                    OleDbCommand command = new OleDbCommand("Select * FROM [" + sheetName + "]", connection);
                    // Create DbDataReader to Data Worksheet

                    OleDbDataAdapter MyData = new OleDbDataAdapter();
                    MyData.SelectCommand = command;
                    DataSet ds = new DataSet();
                    ds.Clear();
                    MyData.Fill(ds);
                    connection.Close();

                    Result = ds;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Result;

        }
        public JsonResult GetSemester(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return null;
                }

                Session session = new Session() { Id = Convert.ToInt32(id) };
                SemesterLogic semesterLogic = new SemesterLogic();
                List<SessionSemester> sessionSemesterList = new List<SessionSemester>();
                SessionSemesterLogic sessionSemesterLogic = new SessionSemesterLogic();
                sessionSemesterList = sessionSemesterLogic.GetModelsBy(p => p.Session_Id == session.Id);

                List<Semester> semesters = new List<Semester>();
                SemesterLogic SemesterLogic = new SemesterLogic();
                semesters = SemesterLogic.GetAll();
                return Json(new SelectList(semesters, ID, NAME), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public JsonResult GetDepartments(string id)
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

        public JsonResult GetDepartmentOptions(string ProgId ,  string DeptId)
        {
            try
            {
                if (string.IsNullOrEmpty(ProgId) || string.IsNullOrEmpty(DeptId))
                {
                    return null;
                }

                Programme programme = new Programme() { Id = Convert.ToInt32(ProgId) };
                Department department = new Department() { Id = Convert.ToInt32(DeptId) };
                DepartmentOptionLogic departmentOptionLogic = new DepartmentOptionLogic();
                List<DepartmentOption> departmentOptions = new List<DepartmentOption>();
                if (programme.Id == 3)                  
                {
                    departmentOptions = departmentOptionLogic.GetBy(department, programme);
                }
                
                return Json(new SelectList(departmentOptions, ID, NAME), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public JsonResult GetCourses(int[] ids)
        {
            try
            {
                if (ids.Count() == 0)
                {
                    return null;
                }
                Department department = new Department() { Id = Convert.ToInt32(ids[0]) };
                Level level = new Level() { Id = Convert.ToInt32(ids[1]) };
                Semester semester = new Semester() { Id = Convert.ToInt32(ids[2]) };
                Programme programme = new Programme() { Id = Convert.ToInt32(ids[3]) };
                List<Course> courseList = Utility.GetCoursesByLevelDepartmentAndSemester(programme,level, department, semester);

                return Json(new SelectList(courseList, ID, NAME), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void RetainDropdownState(SupportViewModel viewModel)
        {
            try
            {
                SemesterLogic semesterLogic = new SemesterLogic();
                DepartmentLogic departmentLogic = new DepartmentLogic();
                SessionLogic sessionLogic = new SessionLogic();
                ProgrammeLogic programmeLogic = new ProgrammeLogic();
                LevelLogic levelLogic = new LevelLogic();
                if (viewModel != null)
                {
                    if (viewModel.Session != null)
                    {

                        ViewBag.Session = new SelectList(sessionLogic.GetModelsBy(p => p.Activated == true), ID, NAME, viewModel.Session.Id);
                        ViewBag.AllSession = new SelectList(sessionLogic.GetAll(), ID, NAME, viewModel.Session.Id);
                    }
                    else
                    {
                        ViewBag.Session = viewModel.SessionSelectList;
                        ViewBag.AllSession = viewModel.SessionSelectList;
                    }
                    if (viewModel.Semester != null)
                    {
                        ViewBag.Semester = new SelectList(semesterLogic.GetAll(), ID, NAME, viewModel.Semester.Id);
                    }
                    else
                    {
                        ViewBag.Semester = new SelectList(new List<Semester>(), ID, NAME);
                    }
                    if (viewModel.Programme != null)
                    {
                        ViewBag.Programme = new SelectList(programmeLogic.GetModelsBy(p => p.Activated == true), ID, NAME, viewModel.Programme.Id);
                    }
                    else
                    {
                        ViewBag.Programme = viewModel.ProgrammeSelectListItem;
                    }
                    if (viewModel.Department != null && viewModel.Programme != null)
                    {
                        ViewBag.Department = new SelectList(departmentLogic.GetBy(viewModel.Programme), ID, NAME, viewModel.Department.Id);
                    }
                    else
                    {
                        ViewBag.Department = new SelectList(new List<Department>(), ID, NAME);
                    }
                    if (viewModel.Level != null)
                    {
                        ViewBag.Level = new SelectList(levelLogic.GetAll(), ID, NAME, viewModel.Level.Id);
                    }
                    else
                    {
                        ViewBag.Level = new SelectList(viewModel.LevelList, ID, NAME);
                    }
    

                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public ActionResult ResetStudentPassword()
        {
            try
            {
                SupportViewModel viewModel = new SupportViewModel();
                return View(viewModel);
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpPost]
        public ActionResult ResetStudentPassword(SupportViewModel viewModel)
        {
            try
            {
                StudentLogic studentLogic = new StudentLogic();
                Model.Model.Student student = studentLogic.GetBy(viewModel.studentModel.MatricNumber);
                if (student != null && student.Id > 0)
                {
                    student.PasswordHash = "1234567";
                    studentLogic.ChangeUserPassword(student);
                    SetMessage("Password has been reset!", Message.Category.Information);

                }
                else
                {
                    SetMessage("User was not found!", Message.Category.Information);
                }

                return View();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public JsonResult GetFileForPreview(long personId, int fileId)
        {
            JsonResultModel result = new JsonResultModel();
            try
            {
                if (personId > 0 && fileId > 0)
                {
                    //1 => firstSitting, 2 => secondSitting, 3 => ndResult, 4 => certificate

                    OLevelResultLogic oLevelResultLogic = new OLevelResultLogic();
                    PreviousEducationLogic previousEducationLogic = new PreviousEducationLogic();

                    string fileUrl = null;
                    switch (fileId)
                    {
                        case 1:
                            OLevelResult oLevelResultFirstSitting = oLevelResultLogic.GetModelsBy(o => o.Person_Id == personId && o.O_Level_Exam_Sitting_Id == 1).LastOrDefault();
                            fileUrl = oLevelResultFirstSitting != null && !string.IsNullOrEmpty(oLevelResultFirstSitting.ScannedCopyUrl) ? oLevelResultFirstSitting.ScannedCopyUrl : null;
                            break;
                        case 2:
                            OLevelResult oLevelResultSecondSitting = oLevelResultLogic.GetModelsBy(o => o.Person_Id == personId && o.O_Level_Exam_Sitting_Id == 2).LastOrDefault();
                            fileUrl = oLevelResultSecondSitting != null && !string.IsNullOrEmpty(oLevelResultSecondSitting.ScannedCopyUrl) ? oLevelResultSecondSitting.ScannedCopyUrl : null;
                            break;
                        case 3:
                            PreviousEducation previousEducationNDResult = previousEducationLogic.GetModelsBy( p => p.Person_Id == personId).LastOrDefault();
                            fileUrl = previousEducationNDResult != null && !string.IsNullOrEmpty(previousEducationNDResult.ResultCopyUrl) ? previousEducationNDResult.ResultCopyUrl : null;
                            break;
                        case 4:
                            PreviousEducation previousEducationCertificate = previousEducationLogic.GetModelsBy(p => p.Person_Id == personId).LastOrDefault();
                            fileUrl = previousEducationCertificate != null && !string.IsNullOrEmpty(previousEducationCertificate.CertificateCopyUrl) ? previousEducationCertificate.CertificateCopyUrl : null;
                            break;
                        case 5:
                            PreviousEducation previousEducationITLetterOfCompletion = previousEducationLogic.GetModelsBy(p => p.Person_Id == personId).LastOrDefault();
                            fileUrl = previousEducationITLetterOfCompletion != null && !string.IsNullOrEmpty(previousEducationITLetterOfCompletion.ITLetterOfCompletion) ? previousEducationITLetterOfCompletion.ITLetterOfCompletion : null;
                            break;
                    }

                    if (fileUrl != null)
                    {
                        result.Url = fileUrl;
                        result.IsError = false;
                        result.Message = "Operation Successful!";
                    }
                }
                else
                {
                    result.IsError = true;
                    result.Message = "Parameter not set!";
                }
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Message = "Error! " + ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult StudentRecord()
        {
            SupportViewModel viewModel = new SupportViewModel();

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult StudentRecord(SupportViewModel viewModel)
        {
            try
            {
                if (viewModel.Student != null && !string.IsNullOrEmpty(viewModel.Student.MatricNumber))
                {
                    PersonLogic personLogic = new PersonLogic();
                    viewModel.Person = personLogic.GetPersonByMatricNumber(viewModel.Student.MatricNumber);

                    if (viewModel.Person != null)
                    {
                        StudentLogic studentLogic = new StudentLogic();
                        viewModel.Student = studentLogic.GetModelBy(s => s.Person_Id == viewModel.Person.Id);

                        AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
                        viewModel.AppliedCourse = appliedCourseLogic.GetModelBy(s => s.Person_Id == viewModel.Person.Id);
                        if (viewModel.AppliedCourse != null)
                            viewModel.ApplicationForm = viewModel.AppliedCourse.ApplicationForm;

                        ApplicantJambDetailLogic jambDetailLogic = new ApplicantJambDetailLogic();
                        if (viewModel.ApplicationForm != null)
                            viewModel.ApplicantJambDetail = jambDetailLogic.GetModelsBy(j => j.Application_Form_Id == viewModel.ApplicationForm.Id).LastOrDefault();
                        else
                            viewModel.ApplicantJambDetail = jambDetailLogic.GetModelsBy(j => j.Person_Id == viewModel.Person.Id).LastOrDefault();

                        AdmissionListLogic listLogic = new AdmissionListLogic();
                        viewModel.AdmissionList = viewModel.ApplicationForm != null ? listLogic.GetModelsBy(a => a.Application_Form_Id == viewModel.ApplicationForm.Id).LastOrDefault() : null;

                        StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                        viewModel.StudentLevels = viewModel.Student != null ? studentLevelLogic.GetModelsBy(s => s.Person_Id == viewModel.Student.Id) : null;

                        PaymentLogic paymentLogic = new PaymentLogic();
                        viewModel.Payments = paymentLogic.GetModelsBy(p => p.Person_Id == viewModel.Person.Id);

                        ScratchCardLogic scratchCardLogic = new ScratchCardLogic();
                        viewModel.ScratchCards = scratchCardLogic.GetModelsBy(s => s.Person_Id == viewModel.Person.Id);

                        viewModel.PaymentHistory = new PaymentHistory();
                        viewModel.PaymentHistory.Payments = paymentLogic.GetEtranzactPaymentBy(viewModel.Person).ToList();
                        List<PaymentView> remitaPayments = paymentLogic.GetPaymentRemitaBy(viewModel.Person);
                        viewModel.PaymentHistory.Payments = viewModel.PaymentHistory.Payments != null ? viewModel.PaymentHistory.Payments.Where(p => p.ConfirmationOrderNumber != null).ToList() : null;
                        viewModel.PaymentHistory.Payments.AddRange(remitaPayments);

                    }
                    else
                        SetMessage("Student not found.", Message.Category.Error);
                }
                else
                    SetMessage("Invalid search parameter.", Message.Category.Error);
            }
            catch (Exception ex)
            {
                SetMessage("Error Occured while Processing your request.Please Try Again. " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        public ActionResult ReassignMatricNumber()
        {
            SupportViewModel viewModel = new SupportViewModel();
            ViewBag.Session = viewModel.AllSessionSelectList;
            return View(viewModel);
        }
        [HttpPost]
        public ActionResult ReassignMatricNumber(SupportViewModel viewModel)
        {
            try
            {
                if(viewModel == null || viewModel.Student == null || string.IsNullOrEmpty(viewModel.Student.MatricNumber))
                {
                    SetMessage("Parameter not set.", Message.Category.Error);
                    ViewBag.Session = viewModel.AllSessionSelectList;
                    return View(viewModel);
                }

                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                StudentLogic studentLogic = new StudentLogic();

                StudentLevel studentLevel = studentLevelLogic.GetModelsBy(s => s.STUDENT.Matric_Number == viewModel.Student.MatricNumber.Trim()).LastOrDefault();
                if (studentLevel == null)
                {
                    SetMessage("Student level details not found.", Message.Category.Error);
                    ViewBag.Session = viewModel.AllSessionSelectList;
                    return View(viewModel);
                }

                int levelId = 1;
                levelId = studentLevel.Programme.Id > 2 ? 3 : levelId;

                string prevMatNumber = studentLevel.Student.MatricNumber;

                StudentMatricNumberAssignmentLogic assignmentLogic = new StudentMatricNumberAssignmentLogic();
                StudentMatricNumberAssignment assignment = assignmentLogic.GetModelsBy(a => a.Department_Id == studentLevel.Department.Id && a.Level_Id == levelId && a.Programme_Id ==
                                                        studentLevel.Programme.Id && a.Session_Id == viewModel.Session.Id).LastOrDefault();
                if (assignment == null)
                {
                    SetMessage("Matric Number Assignment not set.", Message.Category.Error);
                    ViewBag.Session = viewModel.AllSessionSelectList;
                    return View(viewModel);
                }

                int studentNumber = assignment.MatricSerialNoStartFrom + 1;
                int studentNumberLenght = Convert.ToString(studentNumber).Length;

                string prefix = GetZeroPrefix(4 - studentNumberLenght);

                string matricNumber = assignment.MatricNoStartFrom + prefix + Convert.ToString(studentNumber);

                Model.Model.Student existingStudent = studentLogic.GetModelsBy(s => s.Matric_Number == matricNumber).LastOrDefault();
                if (existingStudent != null)
                {
                    SetMessage("Matric Number already exist.", Message.Category.Error);
                    ViewBag.Session = viewModel.AllSessionSelectList;
                    return View(viewModel);
                }

                using (TransactionScope scope = new TransactionScope())
                {
                    studentLevel.Student.MatricNumber = matricNumber;
                    studentLevel.Student.Number = studentNumber;

                    studentLogic.Modify(studentLevel.Student);

                    assignment.MatricSerialNoStartFrom = studentNumber;
                    assignmentLogic.Modify(assignment);

                    GeneralAuditLogic auditLogic = new GeneralAuditLogic();
                    UserLogic userLogic = new UserLogic();

                    User user = userLogic.GetModelBy(p => p.User_Name == User.Identity.Name);
                    string client = Request.LogonUserIdentity.Name + " (" + HttpContext.Request.UserHostAddress + ")";

                    GeneralAudit generalAudit = new GeneralAudit();
                    generalAudit.Action = "UPDATE";
                    generalAudit.Client = client;
                    generalAudit.CurrentValues = matricNumber;
                    generalAudit.InitialValues = prevMatNumber;
                    generalAudit.Operation = "Re-assigned matric number for " + studentLevel.Student.Name;
                    generalAudit.TableNames = "STUDENT, STUDENT_MATRIC_NUMBER_ASSIGNMENT";
                    generalAudit.Time = DateTime.Now;
                    generalAudit.User = user;

                    auditLogic.Create(generalAudit);

                    scope.Complete();
                }

                SetMessage("Operation Successful! New Matric Number is: " + matricNumber, Message.Category.Information);
            }
            catch (Exception ex)
            {
                SetMessage("Error Occured while Processing your request.Please Try Again. " + ex.Message, Message.Category.Error);
            }

            ViewBag.Session = viewModel.AllSessionSelectList;
            return View(viewModel);
        }

        private string GetZeroPrefix(int zeroLength)
        {
            string zeros = "";
            for (int i = 0; i < zeroLength; i++)
            {
                zeros += "0";
            }

            return zeros;
        }

        public ActionResult AdmissionManual()
        {
            try
            {
                return File(Server.MapPath("~/Content/Manual/") + "Admissions_Manual.pdf", "application/pdf", "User_Manual.pdf");
            }
            catch (Exception ex)
            {
                SetMessage("Error Occured. " + ex.Message, Message.Category.Error);
                return RedirectToAction("Index", "Profile", new { Area = "Admin" });
            }
        }
        public ActionResult RDCManual()
        {
            try
            {
                return File(Server.MapPath("~/Content/Manual/") + "R&DC Manual.pdf", "application/pdf", "User_Manual.pdf");
            }
            catch (Exception ex)
            {
                SetMessage("Error Occured. " + ex.Message, Message.Category.Error);
                return RedirectToAction("Index", "Profile", new { Area = "Admin" });
            }
        }

        
        public ActionResult ViewAllProgramme()
        {
            SupportViewModel viewModel = new SupportViewModel();
            ProgrammeLogic programmeLogic = new ProgrammeLogic();

            try
            {
                viewModel.ProgrammeList=programmeLogic.GetAll();
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return View(viewModel);
        }
        public ActionResult ActivateProgramme(int prId, string action)
        {
            SupportViewModel viewModel = new SupportViewModel();
            ProgrammeLogic programmeLogic = new ProgrammeLogic();

            try
            {
                var programme = programmeLogic.GetModelBy(m=>m.Programme_Id==prId);
                if (programme != null)
                {
                    if(action == "activate")
                    {
                        programme.Activated = true;
                        programmeLogic.Modify(programme);
                    }
                    else
                    {
                        programme.ActiveForApllication = true;
                        programmeLogic.Modify(programme);
                    }
                    SetMessage("Operation Successful", Message.Category.Information);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return RedirectToAction("ViewAllProgramme");
        }
        public ActionResult DeActivateProgramme(int prId, string action)
        {
            SupportViewModel viewModel = new SupportViewModel();
            ProgrammeLogic programmeLogic = new ProgrammeLogic();

            try
            {
                var programme = programmeLogic.GetModelBy(m => m.Programme_Id == prId);
                if (programme != null)
                {
                    if (action == "activate")
                    {
                        programme.Activated = false;
                        programmeLogic.Modify(programme);
                    }
                    else
                    {
                        programme.ActiveForApllication = false;
                        programmeLogic.Modify(programme);
                    }
                    SetMessage("Operation Successful", Message.Category.Information);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return RedirectToAction("ViewAllProgramme");
        }
        public ActionResult StudentList()
        {
            SupportViewModel viewModel = new SupportViewModel();
            try
            {
                ViewBag.Session = viewModel.SessionSelectList;
                ViewBag.Programme = viewModel.ProgrammeSelectListItem;
                ViewBag.Department = new SelectList(new List<Department>(), ID, NAME);
                ViewBag.Level = viewModel.LevelSelectList;
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return View(viewModel);
        }
        [HttpPost]
        public ActionResult StudentList(SupportViewModel viewModel)
        {
            try
            {
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                DepartmentLogic departmentLogic = new DepartmentLogic();
                viewModel.StudentLevels=studentLevelLogic.GetModelsBy(f => f.Department_Id == viewModel.Department.Id && f.Programme_Id == viewModel.Programme.Id && f.Session_Id == viewModel.Session.Id);
                viewModel.ShowTable = true;
                ViewBag.Session = viewModel.SessionSelectList;
                ViewBag.Programme = viewModel.ProgrammeSelectListItem;
                ViewBag.Department = new SelectList(new List<Department>(), ID, NAME);
                if (viewModel.Department != null && viewModel.Programme != null)
                {
                    ViewBag.Department = new SelectList(departmentLogic.GetBy(viewModel.Programme), ID, NAME, viewModel.Department.Id);
                }
                
                ViewBag.Level = viewModel.LevelSelectList;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View(viewModel);
        }
        public ActionResult StudentCountBySex()
        {
            SupportViewModel viewModel = new SupportViewModel();
            try
            {
                ViewBag.Session = viewModel.SessionSelectList;
                ViewBag.Programme = viewModel.ProgrammeSelectListItem;
                ViewBag.Level = viewModel.LevelSelectList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View(viewModel);
        }
        [HttpPost]
        public ActionResult StudentCountBySex(SupportViewModel viewModel)
        {
            try
            {
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                DepartmentLogic departmentLogic = new DepartmentLogic();
                List<StudentCountByDepartment> studentCountByDepartments = new List<StudentCountByDepartment>();
                viewModel.StudentLevels = studentLevelLogic.GetModelsBy(f =>f.Programme_Id == viewModel.Programme.Id && f.Session_Id == viewModel.Session.Id);
                if (viewModel.StudentLevels.Count > 0)
                {
                    var groupedByDept=viewModel.StudentLevels.GroupBy(f => f.Department.Id).ToList();
                    if (groupedByDept.Count > 0)
                    {
                        for(int i=0; i<groupedByDept.Count; i++)
                        {
                            int maleCount = 0;
                            int femaleCount = 0;
                            int unknownSexCount = 0;
                            int deptId = groupedByDept[i].Key;
                            var uniqueDept=viewModel.StudentLevels.Where(g => g.Department.Id == deptId).ToList();
                            if (uniqueDept.Count > 0)
                            {
                                StudentCountByDepartment studentCountByDepartment = new StudentCountByDepartment();
                                studentCountByDepartment.TotalCount = uniqueDept.Count();
                                studentCountByDepartment.DepartmentName = uniqueDept.FirstOrDefault().Department.Name;
                                for (int f=0; f<uniqueDept.Count; f++)
                                {
                                    if (uniqueDept[f].Student.Sex != null)
                                    {
                                        if (uniqueDept[f].Student.Sex.Id == (int)Sexes.Male)
                                        {
                                            maleCount += 1;
                                        }
                                        else
                                        {
                                            femaleCount += 1;
                                        }
                                    }
                                    else
                                    {
                                        unknownSexCount += 1;
                                    }
                                }
                                studentCountByDepartment.FemaleCount = femaleCount;
                                studentCountByDepartment.MaleCount = maleCount;
                                studentCountByDepartment.UnknownSex = unknownSexCount;
                                studentCountByDepartments.Add(studentCountByDepartment);
                            }
                            
                        }
                    }
                }
                viewModel.ShowTable = true;
                viewModel.StudentCountByDepartments = studentCountByDepartments.Count>0? studentCountByDepartments.OrderBy(f=>f.DepartmentName).ToList():studentCountByDepartments;

                ViewBag.Session = viewModel.SessionSelectList;
                ViewBag.Programme = viewModel.ProgrammeSelectListItem;
                ViewBag.Level = viewModel.LevelSelectList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View(viewModel);
        }
        public ActionResult ApplicationFormSummary(string from, string to, string ses)
        {
            viewmodel = new SupportViewModel();
            if(!String.IsNullOrEmpty(from) && !String.IsNullOrEmpty(to) && !String.IsNullOrEmpty(ses))
            {
                DateTime fromDate = Convert.ToDateTime(from);
                DateTime toDate = Convert.ToDateTime(to);
                int sessionId = Convert.ToInt32(ses);
                ApplicationFormLogic applicationFormLogic = new ApplicationFormLogic();
                SessionLogic sessionLogic = new SessionLogic();
                var session=sessionLogic.GetModelBy(f => f.Session_Id == sessionId);
                viewmodel.ApplicationCountSummaryList= applicationFormLogic.GetApplicationFormCountSummary(fromDate, toDate, session);
            }
            return View(viewmodel);
        }
        public JsonResult GetAcknowledgementSlipForPreview(long personId)
        {
            JsonResultModel result = new JsonResultModel();
            try
            {
                if (personId > 0)
                {
                    ApplicationFormLogic applicationFormLogic = new ApplicationFormLogic();
                    AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
                    ApplicantLogic applicantLogic = new ApplicantLogic();
                    ApplicantJambDetailLogic applicantJambDetailLogic = new ApplicantJambDetailLogic();
                    OLevelResultDetailLogic oLevelResultDetailLogic = new OLevelResultDetailLogic();
                    SponsorLogic sponsorLogic = new SponsorLogic();
                    PreviousEducationLogic previousEducationLogic = new PreviousEducationLogic();
                    PersonLogic personLogic = new PersonLogic();
                    OLevelResultLogic oLevelResultLogic = new OLevelResultLogic();
                    var applicantAppliedCourse=appliedCourseLogic.GetModelsBy(f => f.Person_Id == personId).FirstOrDefault();
                    if (applicantAppliedCourse!=null)
                    {

                        result.ApplicationPreviewModel = new ApplicationPreviewModel();
                        result.ApplicationPreviewModel.Applicant = applicantLogic.GetModelsBy(f => f.Person_Id == personId).FirstOrDefault();
                        result.ApplicationPreviewModel.ApplicantJambDetail = applicantJambDetailLogic.GetModelsBy(f => f.Person_Id == personId).FirstOrDefault();
                        result.ApplicationPreviewModel.AppliedCourse = appliedCourseLogic.GetModelsBy(f => f.Person_Id == personId).FirstOrDefault();
                        result.ApplicationPreviewModel.FirstSittingDetail = oLevelResultDetailLogic.GetModelsBy(f => f.APPLICANT_O_LEVEL_RESULT.Person_Id == personId && f.APPLICANT_O_LEVEL_RESULT.O_Level_Exam_Sitting_Id == 1).ToList();
                        result.ApplicationPreviewModel.SecondSittingDetail = oLevelResultDetailLogic.GetModelsBy(f => f.APPLICANT_O_LEVEL_RESULT.Person_Id == personId && f.APPLICANT_O_LEVEL_RESULT.O_Level_Exam_Sitting_Id == 2).ToList();
                        result.ApplicationPreviewModel.Sponsor = sponsorLogic.GetModelsBy(f => f.Person_Id == personId).FirstOrDefault();
                        result.ApplicationPreviewModel.PreviousEducation = previousEducationLogic.GetModelsBy(f => f.Person_Id == personId).FirstOrDefault();
                        result.ApplicationPreviewModel.Person = personLogic.GetModelBy(f => f.Person_Id == personId);
                        result.ApplicationPreviewModel.FirstOlevel = oLevelResultLogic.GetModelsBy(f => f.Person_Id == personId && f.O_Level_Exam_Sitting_Id == 1).FirstOrDefault();
                        result.ApplicationPreviewModel.SecondOlevel = oLevelResultLogic.GetModelsBy(f => f.Person_Id == personId && f.O_Level_Exam_Sitting_Id == 2).FirstOrDefault();
                        //result.IsRejected = applicantAppliedCourse?.ApplicationForm?.Id > 0 ? applicantAppliedCourse.ApplicationForm.Rejected : false;
                        //result.Sessionname = applicantAppliedCourse?.ApplicationForm?.Id > 0 ? applicantAppliedCourse.ApplicationForm.Setting.Session.Name : "";
                        //result.Fullname = applicantAppliedCourse?.Person?.FullName != null ? applicantAppliedCourse.Person.FullName : "";
                        //result.ProgrammeName = applicantAppliedCourse?.Programme?.Id > 0 ? applicantAppliedCourse.Programme.Name : "";
                        //result.ProgrammeShortName = applicantAppliedCourse?.Programme?.Id > 0 ? applicantAppliedCourse.Programme.ShortName : "";
                        //result.DepartmentName = applicantAppliedCourse?.Department?.Id > 0 ? applicantAppliedCourse.Department.Name : "";
                        //result.ProgrammeId= applicantAppliedCourse?.Programme?.Id > 0 ? applicantAppliedCourse.Programme.Id : 0;
                        //result.IsError = false;
                        //result.ImageFileUrl = applicantAppliedCourse?.Person?.ImageFileUrl != null ? applicantAppliedCourse.Person.ImageFileUrl : "";

                    }
                }
                else
                {
                    result.IsError = true;
                    result.Message = "Something went wrong";
                }
            }
            catch (Exception ex)
            {

                result.IsError = true;
                result.Message = ex.Message;

            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult BulkDownloadUploadedCredentials(int sessionId, int programmeId, int departmentId, string dateFrom, string dateTo)
        {
            try
            {
                if (sessionId > 0 && programmeId > 0 && departmentId > 0 && !string.IsNullOrEmpty(dateFrom) && !string.IsNullOrEmpty(dateTo))
                {
                    DateTime from = Convert.ToDateTime(dateFrom);
                    DateTime to = Convert.ToDateTime(dateTo);
                    ApplicantLogic applicantLogic = new ApplicantLogic();
                    Programme programme = new Programme { Id = programmeId };
                    Department department = new Department { Id = departmentId };
                    Session session = new Session { Id = sessionId };
                    var result=applicantLogic.GetApplicantsCredentialUploadsForBulkDownload(programme,session,department,from,to);
                    if (result?.Count > 0)
                    {
                        string tempFolder = "~/Content/TempStudentCredentialFolder_" +result.FirstOrDefault().ProgrammeCode+ "_"+ result.FirstOrDefault().DepartmentCode+"/";
                        if (Directory.Exists(Server.MapPath(tempFolder)))
                        {
                            Directory.Delete(Server.MapPath(tempFolder), true);
                            Directory.CreateDirectory(Server.MapPath(tempFolder));
                        }
                        else
                        {
                            Directory.CreateDirectory(Server.MapPath(tempFolder));
                        }
                        var distinctPerson = result.Distinct().Select(f => f.PersonId).ToList();
                        for(int i=0; i< distinctPerson.Count; i++)
                        {
                            long personId = distinctPerson[i];

                            //string studentTempFolder = "~/Content/TempCredentialRootFolder" + DateTime.Now.ToString().Replace("/", "").Replace(":", "").Replace(" ", "");
                            var studentCredential=result.Where(f => f.PersonId == personId).ToList();
                            if (studentCredential?.Count > 0)
                            {
                                var studentTempFolderPath = tempFolder+"Credentials_For_" + "_"+ studentCredential.FirstOrDefault().ApplicationFormNumber.Replace("/", "_")+"/";
                                if (Directory.Exists(Server.MapPath(studentTempFolderPath)))
                                {
                                    Directory.Delete(Server.MapPath(studentTempFolderPath), true);
                                    Directory.CreateDirectory(Server.MapPath(studentTempFolderPath));
                                }
                                else
                                {
                                    Directory.CreateDirectory(Server.MapPath(studentTempFolderPath));
                                }
                                for (int j=0; j<studentCredential.Count; j++)
                                {
                                    //check for OLevel Result
                                    if (!String.IsNullOrEmpty(studentCredential[j].OLevelResults))
                                    {
                                        string filePathOlevelResult = System.Web.HttpContext.Current.Server.MapPath(studentCredential[j].OLevelResults);
                                        if (System.IO.File.Exists(filePathOlevelResult))
                                        {
                                            string[] filePathOlevelResultSplit = studentCredential[j].OLevelResults.Split('/');
                                            string oLevelFile = filePathOlevelResultSplit[4];

                                            if (string.IsNullOrEmpty(oLevelFile))
                                            {
                                                continue;
                                            }
                                            
                                            FileInfo fileInfo = new FileInfo(oLevelFile);
                                            string fileExtension = fileInfo.Extension;
                                            string newFileName =studentCredential[i].NumberOfSittings +"_Olevel" + fileExtension;
                                            System.IO.File.Copy(filePathOlevelResult, Server.MapPath(Path.Combine(studentTempFolderPath, newFileName)), true);
                                        }
                                    }
                                    //check for ND Result
                                    if (!String.IsNullOrEmpty(studentCredential[j].NdResultUrl))
                                    {
                                        string filePathNDResult = System.Web.HttpContext.Current.Server.MapPath(studentCredential[j].NdResultUrl);
                                        if (System.IO.File.Exists(filePathNDResult))
                                        {
                                            string[] filePathNDResultSplit = studentCredential[j].NdResultUrl.Split('/');
                                            string nDResultFile = filePathNDResultSplit[4];

                                            if (string.IsNullOrEmpty(nDResultFile))
                                            {
                                                continue;
                                            }

                                            FileInfo fileInfo = new FileInfo(nDResultFile);
                                            string fileExtension = fileInfo.Extension;
                                            string newFileName = "ND_Result" + fileExtension;
                                            System.IO.File.Copy(filePathNDResult, Server.MapPath(Path.Combine(studentTempFolderPath, newFileName)), true);
                                        }
                                    }
                                    //check for ND Certificate
                                    if (!String.IsNullOrEmpty(studentCredential[j].CertificateUrl))
                                    {
                                        string filePathNDCertificateResult = System.Web.HttpContext.Current.Server.MapPath(studentCredential[j].CertificateUrl);
                                        if (System.IO.File.Exists(filePathNDCertificateResult))
                                        {
                                            string[] filePathNDCertificateSplit = studentCredential[j].CertificateUrl.Split('/');
                                            string nDCertificateFile = filePathNDCertificateSplit[4];

                                            if (string.IsNullOrEmpty(nDCertificateFile))
                                            {
                                                continue;
                                            }

                                            FileInfo fileInfo = new FileInfo(nDCertificateFile);
                                            string fileExtension = fileInfo.Extension;
                                            string newFileName = "ND_Certificate" + fileExtension;
                                            System.IO.File.Copy(filePathNDCertificateResult, Server.MapPath(Path.Combine(studentTempFolderPath, newFileName)), true);
                                        }
                                    }
                                    //check for IT Letter of Completion
                                    if (!String.IsNullOrEmpty(studentCredential[j].ITLetterOfCompletion))
                                    {
                                        string filePathNDItLetterOfCompletion = System.Web.HttpContext.Current.Server.MapPath(studentCredential[j].CertificateUrl);
                                        if (System.IO.File.Exists(filePathNDItLetterOfCompletion))
                                        {
                                            string[] filePathNDItLetterOfCompletionSplit = studentCredential[j].ITLetterOfCompletion.Split('/');
                                            string nDItLetterFile = filePathNDItLetterOfCompletionSplit[4];

                                            if (string.IsNullOrEmpty(nDItLetterFile))
                                            {
                                                continue;
                                            }

                                            FileInfo fileInfo = new FileInfo(nDItLetterFile);
                                            string fileExtension = fileInfo.Extension;
                                            string newFileName = "ItLetter" + fileExtension;
                                            System.IO.File.Copy(filePathNDItLetterOfCompletion, Server.MapPath(Path.Combine(studentTempFolderPath, newFileName)), true);
                                        }
                                    }
                                }
                                
                            }

                        }
                        string zipFileName = "Credentials_" + DateTime.Now.ToString().Replace("/", "").Replace(":", "").Replace(" ", "") + ".zip";
                        using (ZipFile zip = new ZipFile())
                        {
                            string file = Server.MapPath(tempFolder);
                            zip.AddDirectory(file, "");
                            
                            zip.Save(file + zipFileName);
                        }

                        string savedFile = tempFolder + zipFileName;

                        return File(Server.MapPath(savedFile), "application/zip", zipFileName);
                        

                    }
                    

                }
                return RedirectToAction("DownloadHNDApplicantScannedResultSingle");
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public ActionResult GetMissingUploads()
        {
            SupportViewModel viewModel = new SupportViewModel();
            try
            {
                ViewBag.Session = viewModel.AllSessionSelectList;
                ViewBag.Programme = viewModel.ProgrammeSelectListItem;
                ViewBag.Department = new SelectList(new List<Department>(), "Id", "Name");
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult GetMissingUploads(SupportViewModel viewModel)
        {
            
            try
            {
                ApplicantLogic applicantLogic = new ApplicantLogic();
                viewModel.MissingDocumentsList = applicantLogic.GetApplicantsCredentialUploads(viewModel.Programme, viewModel.Session);
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }
            SupportViewModel viewmodel = new SupportViewModel();
            ViewBag.Session = viewmodel.AllSessionSelectList;
            ViewBag.Programme = viewmodel.ProgrammeSelectListItem;
            return View(viewModel);
        }
        public ActionResult SchoolFessDefualters()
        {
            SupportViewModel viewModel = new SupportViewModel();
            try
            {
                ViewBag.Session = viewModel.SessionSelectList;
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View(viewModel);
        }
        [HttpPost]
        public ActionResult SchoolFessDefualters(SupportViewModel model)
        {
            SupportViewModel viewModel = new SupportViewModel();
            CourseRegistrationLogic courseRegistrationLogic = new CourseRegistrationLogic();
            PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
            PaymentLogic paymentLogic = new PaymentLogic();



            try
            {
                ViewBag.Session = viewModel.SessionSelectList;
                model.StudentInfoList = new List<StudentInfo>();
                var registeredCourseBySession=courseRegistrationLogic.GetModelsBy(f => f.Session_Id == model.Session.Id).ToList();
                if (registeredCourseBySession?.Count > 0)
                {
                    RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                    for(int i=0; i < registeredCourseBySession.Count; i++)
                    {
                        long personId = registeredCourseBySession[i].Student.Id;
                        
                        var listOfSchoolFeesPaidInvoices=remitaPaymentLogic.GetModelsBy(f => f.PAYMENT.Person_Id == personId && f.PAYMENT.Session_Id == model.Session.Id && (f.PAYMENT.Fee_Type_Id == 3 || f.PAYMENT.Fee_Type_Id == 10) 
                        && (f.Status.Contains("01") || f.Status.Contains("998") || f.Description.Contains("manual"))).ToList();
                        if (listOfSchoolFeesPaidInvoices.Count <= 0)
                        {
                            var payment = paymentLogic.GetModelsBy(f => f.Person_Id == personId && f.Session_Id == model.Session.Id && (f.Fee_Type_Id == 3 || f.Fee_Type_Id == 10)).FirstOrDefault();
                            if (payment?.Id > 0)
                            {
                                var etranzactRecord = paymentEtranzactLogic.GetModelsBy(f => f.Payment_Id == payment.Id).FirstOrDefault();
                                if (etranzactRecord == null)
                                {
                                    StudentInfo studentInfo = new StudentInfo()
                                    {
                                        DepartmentName = registeredCourseBySession[i].Department.Name,
                                        FullName = registeredCourseBySession[i].Student.FullName,
                                        Level = registeredCourseBySession[i].Level.Name,
                                        MatricNo = registeredCourseBySession[i].Student.MatricNumber,
                                        ProgrammeName = registeredCourseBySession[i].Programme.Name,
                                        Session = registeredCourseBySession[i].Session.Name
                                    };
                                    model.StudentInfoList.Add(studentInfo);
                                }
                            }
                            
                            
                            
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View(model);
        }
        public ActionResult VerifyRRRUsingRemitaApi()
        {

            try
            {

            }
            catch (Exception ex)
            {

                throw ex;
            }
            return View();
        }
        [HttpPost]
        public ActionResult VerifyRRRUsingRemitaApi(SupportViewModel viewModel)
        {

            try
            {

                if (viewModel.InvoiceNumber != null)
                {
                    //GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();
                    //GeneralAudit generalAudit = new GeneralAudit()
                    //{
                    //    Action = "Verified RRR for" + viewModel.InvoiceNumber,
                    //    Operation = "PULL",
                    //    TableNames = "ACCEPTANCE REPORT"
                    //};
                    //generalAuditLogic.CreateGeneralAudit(generalAudit);

                    RemitaSettings settings = new RemitaSettings();
                    RemitaSettingsLogic settingsLogic = new RemitaSettingsLogic();
                    //RemitaResponse remitaResponse = new RemitaResponse();

                    settings = settingsLogic.GetModelBy(s => s.Payment_SettingId == 1);
                    //string remitaVerifyUrl = ConfigurationManager.AppSettings["RemitaVerifyUrl"].ToString();
                    RemitaPayementProcessor remitaPayementProcessor = new RemitaPayementProcessor(settings.Api_key);
                    RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                    if (!viewModel.InvoiceNumber.Contains("FPI"))
                    {

                        
                        var remita = remitaPaymentLogic.GetModelsBy(g => g.RRR == viewModel.InvoiceNumber).LastOrDefault();
                        if (remita != null)
                        {
                            viewModel.RemitaPayment = remitaPayementProcessor.GetStatus(remita.OrderId);
                            //viewModel.RemitaPayment= remitaPaymentLogic.GetModelsBy(g => g.RRR == viewModel.InvoiceNumber).LastOrDefault();
                        }
                        else
                        {
                            SetMessage("Payment Does not Exist! ", Message.Category.Information);
                            return View();
                        }

                    }
                    else
                    {
                        PaymentLogic paymentLogic = new PaymentLogic();
                        var payment = paymentLogic.GetModelsBy(g => g.Invoice_Number == viewModel.InvoiceNumber).LastOrDefault();
                        if (payment != null)
                        {
                            var remita = remitaPaymentLogic.GetModelsBy(g => g.Payment_Id == payment.Id).FirstOrDefault();
                            if(remita!=null && remita.payment?.Id > 0)
                            {
                                viewModel.RemitaPayment = remitaPayementProcessor.GetStatus(remita.OrderId);
                                //viewModel.RemitaPayment = remitaPaymentLogic.GetModelsBy(g => g.Payment_Id == payment.Id).LastOrDefault();
                            }
                            
                        }
                        else
                        {
                            SetMessage("Payment Does not Exist! ", Message.Category.Information);
                            return View();
                        }

                    }
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
            return View(viewModel);
        }
        public ActionResult ClearRRR()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ClearRRR(SupportViewModel viewModel)
        {
            SupportViewModel supportViewModel = new SupportViewModel(); 
            if (!string.IsNullOrEmpty(viewModel.InvoiceNumber) && !string.IsNullOrEmpty(viewModel.AccessCode))
            {
                if (viewModel.AccessCode != "02@ilaro")
                {
                    SetMessage("You are not authorized to do this", Message.Category.Information);
                    return View();
                }
                RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                var remitaPayment=remitaPaymentLogic.GetModelsBy(f => f.RRR == viewModel.InvoiceNumber.Trim()).FirstOrDefault();
                if (remitaPayment == null)
                {
                    SetMessage("RRR not found", Message.Category.Information);
                    return View();
                }
                supportViewModel.RemitaPayment = remitaPayment;
                if (remitaPayment?.payment?.Id>0 && (!remitaPayment.Status.Contains("01:") || !remitaPayment.Status.Contains("00:")))
                {
                    RemitaSettings settings = new RemitaSettings();
                    RemitaSettingsLogic settingsLogic = new RemitaSettingsLogic();

                    settings = settingsLogic.GetModelBy(s => s.Payment_SettingId == 1);
                    RemitaPayementProcessor remitaPayementProcessor = new RemitaPayementProcessor(settings.Api_key);
                    supportViewModel.RemitaPayment = remitaPayementProcessor.GetStatus(remitaPayment.OrderId);
                }
            }
            return View(supportViewModel);
        }
        public JsonResult SaveClearedRRR(long id, string accessCode)
        {
            JsonResultModel result = new JsonResultModel();
            try
            {
                if (id<=0 )
                {
                    result.IsError = false;
                    result.Message = "No payment Id";
                    return Json(result);
                }
                if (string.IsNullOrEmpty(accessCode))
                {
                    result.IsError = false;
                    result.Message = "Please enter authorization code";
                    return Json(result);
                }
                if ("02@ilaro"!=accessCode)
                {
                    result.IsError = false;
                    result.Message = "Authorization code does not match";
                    return Json(result);
                }
                PaymentLogic paymentLogic = new PaymentLogic();
                RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                var remitaPayment = remitaPaymentLogic.GetModelsBy(f => f.Payment_Id == id).FirstOrDefault();
                if (remitaPayment?.payment?.Id > 0 && (remitaPayment.Status.Contains("01:") || remitaPayment.Status.Contains("00:")))
                {
                    result.IsError = false;
                    result.Message = "This RRR cannot cleared because it has been processed";
                    return Json(result);
                }
                    var response=paymentLogic.ClearRRR(id);
                if(response)
                {
                    //create audit
                    GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();
                    string Action = "CLEAR RRR";
                    string Operation = "Cleared RRR " + remitaPayment.RRR;
                    string Table = "Payment,OnlinePayment and Remitatable";
                    generalAuditLogic.CreateGeneralAudit(Action, Operation, Table);

                    result.IsError = false;
                    result.Message = "RRR cleared successfullly";
                    return Json(result);
                }
                

            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Message = ex.Message;
            }
            return Json(result);
        }

        public ActionResult UploadApplicantJambRecord()
        {
            try
            {
                ApplicantSetupViewModel viewmodel = new ApplicantSetupViewModel();
                viewmodel.ProgrammeSelectListItem = Utility.PopulateAllProgrammeSelectListItem();
                viewmodel.SessionSelectListItem = Utility.PopulateAdmissionSessionSelectListItem();
                viewmodel.AdmissionListTypeSelectListItem = Utility.PopulateAdmissionListTypeSelectListItem();

                ViewBag.ProgrammeId = viewmodel.ProgrammeSelectListItem;
                ViewBag.SessionId = viewmodel.SessionSelectListItem;
                ViewBag.AdmissionListTypeId = viewmodel.AdmissionListTypeSelectListItem;
                ViewBag.DepartmentId = new SelectList(new List<Department>(), ID, NAME);
                ViewBag.DepartmentOptionId = new SelectList(new List<DepartmentOption>(), ID, NAME);

            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            return View();
        }
        [HttpPost]
        public ActionResult UploadApplicantJambRecord(ApplicantSetupViewModel vmodel)
        {
            try
            {
                DepartmentOptionLogic departmentOptionLogic = new DepartmentOptionLogic();

                if (vmodel.CurrentSession == null)
                {

                    SetMessage("Please specify session and department!", Message.Category.Error);
                    KeepApplicationFormInvoiceGenerationDropDownState(vmodel);
                    return View(vmodel);

                }

                KeepApplicationFormInvoiceGenerationDropDownState(vmodel);

                List<JambRecord> jambRecords = new List<JambRecord>();
                List<FailedUploads> failedUploads = new List<FailedUploads>();
                int sn = 0;


                string pathForSaving = Server.MapPath("~/Content/ExcelUploads");
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
                        string newFile = "JambRecord" + "__";
                        string newFileName = newFile + DateTime.Now.ToString().Replace("/", "").Replace(":", "").Replace(" ", "") + fileExtension;

                        savedFileName = Path.Combine(pathForSaving, newFileName);
                        hpf.SaveAs(savedFileName);
                    }

                    IExcelManager excelManager = new ExcelManager();
                    DataSet dsJambRecordList = excelManager.ReadExcel(savedFileName);

                    if (dsJambRecordList != null && dsJambRecordList.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < dsJambRecordList.Tables[0].Rows.Count; i++)
                        {
                            List<string> uploadFailReason = new List<string>();
                            JambRecord jambRecord = new JambRecord();
                            FailedUploads failedUpload = new FailedUploads();
                            SexLogic sexLogic = new SexLogic();
                            DepartmentLogic departmentLogic = new DepartmentLogic();
                            StateLogic stateLogic = new StateLogic();
                            Sex sexModel = null;
                            State personState = null;
                            Department department = null;
                            bool isUpload = true;

                            var regNumber = dsJambRecordList.Tables[0].Rows[i][0].ToString().Trim();
                            var sex = dsJambRecordList.Tables[0].Rows[i][2].ToString().Trim();
                            var _state = dsJambRecordList.Tables[0].Rows[i][3].ToString().Trim();
                            var _course = dsJambRecordList.Tables[0].Rows[i][5].ToString().Trim();
                            var _jambScore = dsJambRecordList.Tables[0].Rows[i][4].ToString().Trim();
                            var _subject1 = dsJambRecordList.Tables[0].Rows[i][7].ToString().Trim();
                            var _subject1_score = dsJambRecordList.Tables[0].Rows[i][8].ToString().Trim();
                            var _subject2 = dsJambRecordList.Tables[0].Rows[i][9].ToString().Trim();
                            var _subject2_score = dsJambRecordList.Tables[0].Rows[i][10].ToString().Trim();
                            var _subject3 = dsJambRecordList.Tables[0].Rows[i][11].ToString().Trim();
                            var _subject3_score = dsJambRecordList.Tables[0].Rows[i][12].ToString().Trim();
                            var _subject4 = "English Language";
                            var _subject4_score = dsJambRecordList.Tables[0].Rows[i][13].ToString().Trim();
                            short vOut = Convert.ToInt16(_jambScore);
                            short def = 0;


                            if (!string.IsNullOrEmpty(sex))
                            {

                                if (sex == "M")
                                {
                                    sexModel = new Sex() { Id = 1, Name = "Male" };
                                }
                                else
                                {
                                    sexModel = new Sex() { Id = 2, Name = "Female" };
                                }
                                isUpload = true;
                            }

                            if (string.IsNullOrEmpty(regNumber))
                            {
                                uploadFailReason.Add("JAMB registration number column was left blank.");
                                isUpload = false;
                            }

                            if (string.IsNullOrEmpty(_subject1_score) || string.IsNullOrEmpty(_subject2_score) || string.IsNullOrEmpty(_subject3_score) || string.IsNullOrEmpty(_subject4_score))
                            {
                                uploadFailReason.Add("One or more of the JAMB score column was left blank.");
                                isUpload = false;
                            }
                            if (!string.IsNullOrEmpty(_subject1) && !string.IsNullOrEmpty(_subject2) && !string.IsNullOrEmpty(_subject3) && !string.IsNullOrEmpty(_subject4))
                            {
                                var sub1 = ResolveOlevelSubjects(_subject1);
                                var sub2 = ResolveOlevelSubjects(_subject2);
                                var sub3 = ResolveOlevelSubjects(_subject3);
                                var sub4 = ResolveOlevelSubjects(_subject4);
                                if (sub1 != null)
                                {
                                    _subject1 = sub1.Id.ToString();
                                }
                                else
                                {
                                    uploadFailReason.Add("The subject " + _subject1 + " was not found on the server.");
                                    isUpload = false;

                                }
                                if (sub2 != null)
                                {
                                    _subject2 = sub2.Id.ToString();
                                }
                                else
                                {
                                    uploadFailReason.Add("The subject " + _subject2 + " was not found on the server.");
                                    isUpload = false;

                                }
                                if (sub3 != null)
                                {
                                    _subject3 = sub3.Id.ToString();
                                }
                                else
                                {
                                    uploadFailReason.Add("The subject " + _subject3 + " was not found on the server.");
                                    isUpload = false;

                                }
                                if (sub4 != null)
                                {
                                    _subject4 = sub4.Id.ToString();
                                }
                                else
                                {
                                    uploadFailReason.Add("The subject " + _subject4 + " was not found on the server.");
                                    isUpload = false;

                                }
                            }
                            else
                            {
                                uploadFailReason.Add("One or more of the JAMB subject column was left blank.");
                                isUpload = false;
                            }
                            if (!string.IsNullOrEmpty(_subject1_score) && !string.IsNullOrEmpty(_subject2_score) && !string.IsNullOrEmpty(_subject3_score) && !string.IsNullOrEmpty(_subject4_score))
                            {
                                var score1 = Convert.ToInt16(_subject1_score);
                                var score2 = Convert.ToInt16(_subject2_score);
                                var score3 = Convert.ToInt16(_subject3_score);
                                var score4 = Convert.ToInt16(_subject4_score);
                                var sum_score = score1 + score2 + score3 + score4;
                                if (sum_score != vOut)
                                {
                                    uploadFailReason.Add("Disparity between total sum of subject score and aggregate score provided");
                                    isUpload = false;
                                }

                            }

                            if (!string.IsNullOrEmpty(_course))
                            {
                                department = departmentLogic.GetModelBy(x => x.Department_Name == _course);
                                if (department == null)
                                {
                                    uploadFailReason.Add("Specified Course was not found on the server.");
                                    isUpload = false;

                                }
                            }
                            else
                            {
                                uploadFailReason.Add("Applicant Course was not specified");
                                isUpload = false;
                            }
                            if (!string.IsNullOrEmpty(_state))
                            {
                                personState = stateLogic.GetModelBy(x => x.State_Name.ToUpper() == _state);
                                if (personState == null)
                                {
                                    uploadFailReason.Add("Applicant state was not correctly formatted");
                                    isUpload = false;
                                }
                            }
                            else
                            {
                                uploadFailReason.Add("Applicant state was not specified");
                                isUpload = false;
                            }
                            if (isUpload)
                            {
                                jambRecord.JambRegistrationNumber = dsJambRecordList.Tables[0].Rows[i][0].ToString();
                                jambRecord.CandidateName = dsJambRecordList.Tables[0].Rows[i][1].ToString();
                                jambRecord.Sex = sexModel != null ? sexModel : null;
                                jambRecord.State = personState;
                                jambRecord.Course = department;
                                jambRecord.TotalJambScore = vOut > 0 ? vOut : def;
                                jambRecord.Subject1 = _subject1;
                                jambRecord.Subject2 = _subject2;
                                jambRecord.Subject3 = _subject3;
                                jambRecord.Subject4 = _subject4;
                                jambRecord.Score1 = Convert.ToInt16(_subject1_score);
                                jambRecord.Score2 = Convert.ToInt16(_subject2_score);
                                jambRecord.Score3 = Convert.ToInt16(_subject3_score);
                                jambRecord.Score4 = Convert.ToInt16(_subject4_score);
                                jambRecord.Session = vmodel.CurrentSession;
                                //jambRecord.FirstChoiceInstitution = "FEDPO-ADO";


                                jambRecords.Add(jambRecord);
                            }
                            else
                            {
                                sn += 1;
                                failedUpload.SN = sn.ToString();
                                failedUpload.JAMBRegNumber = dsJambRecordList.Tables[0].Rows[i][0].ToString();
                                failedUpload.Fullname = dsJambRecordList.Tables[0].Rows[i][1].ToString();
                                failedUpload.Reason = string.Join(" | ", uploadFailReason);
                                failedUploads.Add(failedUpload);
                                vmodel.IsUploadFailed = true;
                            }


                        }

                        vmodel.JambRecordList = jambRecords;
                        vmodel.FailedUploadList = failedUploads;
                        TempData["ApplicantSetupViewModel"] = vmodel;

                        return View(vmodel);
                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            KeepApplicationFormInvoiceGenerationDropDownState(vmodel);
            return View(vmodel);
        }



        public ActionResult UploadCandidateOLevel()
        {
            try
            {
                ApplicantSetupViewModel viewmodel = new ApplicantSetupViewModel();
                viewmodel.ProgrammeSelectListItem = Utility.PopulateAllProgrammeSelectListItem();
                viewmodel.SessionSelectListItem = Utility.PopulateAdmissionSessionSelectListItem();

                ViewBag.ProgrammeId = viewmodel.ProgrammeSelectListItem;
                ViewBag.SessionId = viewmodel.SessionSelectListItem;


            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            return View();
        }
        [HttpPost]
        public ActionResult UploadCandidateOLevel(ApplicantSetupViewModel vmodel)
        {
            try
            {

                if (vmodel.CurrentSession == null)
                {

                    SetMessage("Please specify session and department!", Message.Category.Error);
                    KeepApplicationFormInvoiceGenerationDropDownState(vmodel);
                    return View(vmodel);

                }

                KeepApplicationFormInvoiceGenerationDropDownState(vmodel);

                List<JambRecord> jambRecords = new List<JambRecord>();
                List<FailedJambOlevelUploads> failedUploads = new List<FailedJambOlevelUploads>();
                int sn = 0;
                int successCount = 0;
                int failCount = 0;
                int updatedCount = 0;


                string pathForSaving = Server.MapPath("~/Content/ExcelUploads");
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
                        string newFile = "Admission" + "__";
                        string newFileName = newFile + DateTime.Now.ToString().Replace("/", "").Replace(":", "").Replace(" ", "") + fileExtension;

                        savedFileName = Path.Combine(pathForSaving, newFileName);
                        hpf.SaveAs(savedFileName);
                    }

                    IExcelManager excelManager = new ExcelManager();
                    DataSet dsJambRecordList = excelManager.ReadExcel(savedFileName);

                    if (dsJambRecordList != null && dsJambRecordList.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < dsJambRecordList.Tables[0].Rows.Count; i++)
                        {
                            List<string> uploadFailReason = new List<string>();
                            bool isUpload = true;

                            //JambRecord jambRecord = new JambRecord();
                            SexLogic sexLogic = new SexLogic();
                            JambRecordLogic jambRecordLogic = new JambRecordLogic();
                            JambOlevelLogic jambOlevelLogic = new JambOlevelLogic();
                            JambOlevelDetail jambOlevelDetail = new JambOlevelDetail();
                            JambOlevelDetailLogic jambOlevelDetailLogic = new JambOlevelDetailLogic();
                            JambRecord jambRecord = new JambRecord();
                            JambOlevel jambOlevel = new JambOlevel();
                            StateLogic stateLogic = new StateLogic();
                            OLevelType oLevelType = new OLevelType();
                            OLevelTypeLogic oLevelTypeLogic = new OLevelTypeLogic();
                            var regNumber = dsJambRecordList.Tables[0].Rows[i][0].ToString().Trim();
                            var _subject = dsJambRecordList.Tables[0].Rows[i][1].ToString().Trim();
                            var _grade = dsJambRecordList.Tables[0].Rows[i][2].ToString().Trim();
                            var _examYear = dsJambRecordList.Tables[0].Rows[i][4].ToString().Trim();
                            var _examType = dsJambRecordList.Tables[0].Rows[i][5].ToString().Trim();
                            var _examNumber = dsJambRecordList.Tables[0].Rows[i][6].ToString().Trim();

                            if (!string.IsNullOrEmpty(regNumber))
                            {
                                jambRecord = jambRecordLogic.GetModelBy(x => x.Jamb_Registration_Number == regNumber && x.Session_Id == vmodel.CurrentSession.Id);
                                if (jambRecord != null && jambRecord.Id > 0)
                                {
                                    jambOlevel = jambOlevelLogic.GetModelBy(x => x.Jamb_Record_Id == jambRecord.Id);
                                    if (jambOlevel == null || jambOlevel.Id < 0)
                                    {
                                        jambOlevel = new JambOlevel();
                                        jambOlevel.JambRecord = jambRecord;
                                        jambOlevel.ExamSitting = new OLevelExamSitting() { Id = 1 };
                                        jambOlevel.Exam_Number = _examNumber;
                                    }
                                }
                                else
                                {
                                    uploadFailReason.Add("JAMB Details were not found.");
                                    isUpload = false;
                                }

                            }
                            else
                            {
                                uploadFailReason.Add("JAMB registration number column was left blank.");
                                isUpload = false;
                            }

                            if (!string.IsNullOrEmpty(_examType))
                            {
                                string[] resolveExamType = _examType.Split(' ');
                                var _exType = "";
                                if (resolveExamType != null && resolveExamType.ToList().Count > 1)
                                {
                                    _exType = resolveExamType[0];
                                }
                                else
                                {
                                    _exType = _examType;
                                }
                                oLevelType = oLevelTypeLogic.GetModelsBy(x => x.O_Level_Type_Short_Name.Contains(_exType)).FirstOrDefault();
                                if (oLevelType != null && oLevelType.Id > 0)
                                {
                                    jambOlevel.ExamType = oLevelType;
                                }
                                else
                                {
                                    uploadFailReason.Add("Exam type was not found on server.");
                                    isUpload = false;
                                }
                            }
                            else
                            {
                                uploadFailReason.Add("Exam type was not specified.");
                                isUpload = false;
                            }
                            if (!string.IsNullOrEmpty(_examYear))
                            {
                                jambOlevel.Exam_Year = Convert.ToInt16(_examYear);
                            }
                            else
                            {
                                uploadFailReason.Add("Exam year was not specified.");
                                isUpload = false;
                            }

                            if (!string.IsNullOrEmpty(_subject))
                            {
                                var sub1 = ResolveOlevelSubjects(_subject);

                                if (sub1 != null)
                                {
                                    jambOlevelDetail.OLevelSubject = sub1;
                                }
                                else
                                {
                                    uploadFailReason.Add("The subject " + _subject + " was not found on the server.");
                                    isUpload = false;

                                }
                            }
                            else
                            {
                                uploadFailReason.Add("One or more Subject was not specified.");
                                isUpload = false;

                            }
                            if (!string.IsNullOrEmpty(_grade))
                            {
                                var parsedGrade = _grade == "A/R" ? "AR" : _grade;
                                var grad = ResolveOlevelGrades(parsedGrade);

                                if (grad != null)
                                {
                                    jambOlevelDetail.OLevelGrade = grad;

                                }
                                else
                                {
                                    uploadFailReason.Add("The grade " + _grade + " was not found on the server.");
                                    isUpload = false;

                                }
                            }
                            else
                            {
                                uploadFailReason.Add("One or more Grade was not specified.");
                                isUpload = false;

                            }
                            if (isUpload)
                            {
                                var _doesJambOlevelExist = jambOlevelLogic.GetModelBy(x => x.JAMB_RECORD.Jamb_Registration_Number == regNumber && x.JAMB_RECORD.Session_Id == vmodel.CurrentSession.Id);
                                if (_doesJambOlevelExist == null)
                                {
                                    using (TransactionScope transaction = new TransactionScope())
                                    {
                                        var saveJambOlevel = jambOlevelLogic.Create(jambOlevel);
                                        jambOlevelDetail.JambOlevel = saveJambOlevel;
                                        jambOlevelDetailLogic.Create(jambOlevelDetail);

                                        transaction.Complete();
                                        successCount++;
                                    }
                                }
                                else
                                {
                                    var isSubjectAdded = jambOlevelDetailLogic.GetModelBy(x => x.Subject_Id == jambOlevelDetail.OLevelSubject.Id && x.Jamb_O_Level_Id == _doesJambOlevelExist.Id);
                                    if (isSubjectAdded == null)
                                    {
                                        jambOlevelDetail.JambOlevel = _doesJambOlevelExist;
                                        jambOlevelDetailLogic.Create(jambOlevelDetail);
                                        updatedCount++;
                                    }
                                    else if (isSubjectAdded != null)
                                    {
                                        isSubjectAdded.OLevelGrade = jambOlevelDetail.OLevelGrade;
                                        jambOlevelDetailLogic.Modify(isSubjectAdded);
                                        updatedCount++;
                                    }
                                    OLevelResult oLevelResult = new OLevelResult();
                                    OLevelResultLogic oLevelResultLogic = new OLevelResultLogic();
                                    OLevelResultDetail oLevelResultDetail = new OLevelResultDetail();
                                    OLevelResultDetailLogic oLevelResultDetailLogic = new OLevelResultDetailLogic();
                                    oLevelResult = oLevelResultLogic.GetModelsBy(x => x.Exam_Number == _examNumber && x.O_Level_Exam_Sitting_Id == 1).LastOrDefault();
                                    if (oLevelResult != null)
                                    {
                                        oLevelResultDetail = oLevelResultDetailLogic.GetModelsBy(x => x.O_Level_Subject_Id == jambOlevelDetail.OLevelSubject.Id && x.Applicant_O_Level_Result_Id == oLevelResult.Id).FirstOrDefault();
                                        if (oLevelResultDetail != null)
                                        {
                                            oLevelResultDetail.Grade = jambOlevelDetail.OLevelGrade;
                                            oLevelResultDetailLogic.Modify(oLevelResultDetail);
                                        }
                                    }
                                    //updatedCount++;
                                }

                            }
                            else
                            {
                                FailedJambOlevelUploads failedUpload = new FailedJambOlevelUploads();
                                failedUpload.JAMBRegNumber = regNumber;
                                failedUpload.Reason = string.Join(" | ", uploadFailReason);
                                failCount++;
                                failedUpload.SN = failCount.ToString();
                                failedUploads.Add(failedUpload);
                                vmodel.IsUploadFailed = true;
                            }

                        }

                        //vmodel.JambRecordList = jambRecords;
                        vmodel.FailedOlevelUploadList = failedUploads;
                        TempData["CandidateOLevelViewModel"] = vmodel;

                    }
                }
                SetMessage(successCount + " Succesfully uploaded, " + updatedCount + " Updated, " + failCount + " Failed uploads.", Message.Category.Warning);
                return View(vmodel);

            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            KeepApplicationFormInvoiceGenerationDropDownState(vmodel);
            return View(vmodel);
        }



        private void KeepApplicationFormInvoiceGenerationDropDownState(ApplicantSetupViewModel viewModel)
        {
            try
            {


                ViewBag.ProgrammeId = new SelectList(viewModel.ProgrammeSelectListItem, VALUE, TEXT);
                ViewBag.DepartmentId = new SelectList(new List<Department>(), ID, NAME);
                ViewBag.DepartmentOptionId = new SelectList(new List<DepartmentOption>(), ID, NAME);
                ViewBag.SessionId = new SelectList(Utility.PopulateAdmissionSessionSelectListItem(), VALUE, TEXT);
                ViewBag.AdmissionListTypeId = viewModel.AdmissionListTypeSelectListItem;


            }
            catch (Exception ex)
            {
                throw ex;
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
        public OLevelSubject ResolveOlevelSubjects(string subjectName)
        {
            try
            {
                OLevelSubjectLogic subjectLogic = new OLevelSubjectLogic();
                var doesExist = subjectLogic.GetModelBy(x => x.O_Level_Subject_Name == subjectName);
                if (doesExist != null && doesExist.Id > 0)
                {
                    return doesExist;
                }
                return null;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public OLevelGrade ResolveOlevelGrades(string gradeName)
        {
            try
            {
                OLevelGradeLogic gradeLogic = new OLevelGradeLogic();
                var doesExist = gradeLogic.GetModelBy(x => x.O_Level_Grade_Name == gradeName);
                if (doesExist != null && doesExist.Id > 0)
                {
                    return doesExist;
                }
                return null;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public ActionResult ProblemOlevelUploads()
        {
            try
            {
                ApplicantSetupViewModel vModel = (ApplicantSetupViewModel)TempData["CandidateOLevelViewModel"];
                GridView gv = new GridView();
                List<FailedJambOlevelUploads> sample = new List<FailedJambOlevelUploads>();
                FailedUploads _failed = new FailedUploads();
                if (vModel != null && vModel.FailedOlevelUploadList.Count > 0)
                {
                    foreach (FailedJambOlevelUploads item in vModel.FailedOlevelUploadList)
                    {
                        sample.Add(item);
                    }
                }
                string filename = "Failed OLevel Uploads";
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
                return RedirectToAction("UploadApplicantJambRecord");
            }

            return RedirectToAction("UploadApplicantJambRecord");
        }

        public ActionResult ProblemUploads()
        {
            try
            {
                ApplicantSetupViewModel vModel = (ApplicantSetupViewModel)TempData["ApplicantSetupViewModel"];
                GridView gv = new GridView();
                List<FailedUploads> sample = new List<FailedUploads>();
                FailedUploads _failed = new FailedUploads();
                if (vModel != null && vModel.FailedUploadList.Count > 0)
                {
                    foreach (FailedUploads item in vModel.FailedUploadList)
                    {
                        sample.Add(item);
                    }
                }

                string filename = "Problem Uploads";
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
                return RedirectToAction("UploadApplicantJambRecord");
            }

            return RedirectToAction("UploadApplicantJambRecord");
        }

        [HttpPost]
        public ActionResult SaveJambRecordList(ApplicantSetupViewModel vmodel)
        {
            try
            {
                vmodel = (ApplicantSetupViewModel)TempData["ApplicantSetupViewModel"];
                if (vmodel.JambRecordList != null && vmodel.JambRecordList.Count > 0)
                {
                    using (TransactionScope transaction = new TransactionScope())
                    {

                        for (int i = 0; i < vmodel.JambRecordList.Count; i++)
                        {
                            var regNumber = vmodel.JambRecordList[i].JambRegistrationNumber;
                            JambRecord jambRecord = new JambRecord();
                            JambRecordLogic jambRecordLogic = new JambRecordLogic();
                            jambRecord = jambRecordLogic.GetModelBy(x => x.Jamb_Registration_Number == regNumber);
                            if (jambRecord == null)
                            {
                                jambRecord = vmodel.JambRecordList[i];
                                jambRecordLogic.Create(jambRecord);
                            }
                            else
                            {
                                jambRecord = vmodel.JambRecordList[i];
                                jambRecordLogic.Modify(jambRecord);
                            }
                        }
                        transaction.Complete();
                    }

                    SetMessage("List was uploaded successfully", Message.Category.Information);
                    return RedirectToAction("UploadApplicantJambRecord");
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            KeepApplicationFormInvoiceGenerationDropDownState(vmodel);
            return View("UploadApplicantJambRecord");
        }

        public ActionResult SampleJambRecordUpload()
        {
            try
            {
                GridView gv = new GridView();
                List<SampleJambRecordUpload> sample = new List<SampleJambRecordUpload>();
                sample.Add(new SampleJambRecordUpload()
                {
                    //SN = "1",
                    JAMBRegNumber = "12345678EG",
                    Fullname = "O. Godspeed Miracle",
                    Sex = "M",
                    State_Id = "DELTA",
                    JambScore = "300",
                    Course_Name = "Accountancy",
                    LGA_Name = "WARRI-CENTRAL",
                    Subject1_Id = "Literature In English",
                    Subject1_Score = "70",
                    Subject2_Id = "Mathematics",
                    Subject2_Score = "70",
                    Subject3_Id = "Commerce",
                    Subject3_Score = "70",
                    //Subject4_Id = "Literature In English",
                    EngScore = "90"
                });

                sample.Add(new SampleJambRecordUpload()
                {
                    //SN = "1",
                    JAMBRegNumber = "12345678AL",
                    Fullname = "Olamide Kaidosarat Rita",
                    Sex = "M",
                    State_Id = "EKITI",
                    JambScore = "300",
                    Course_Name = "Library and Information Science",
                    LGA_Name = "ADO",
                    Subject1_Id = "Mathematics",
                    Subject1_Score = "70",
                    Subject2_Id = "Commerce",
                    Subject2_Score = "70",
                    Subject3_Id = "Government",
                    Subject3_Score = "70",
                    //Subject4_Id = "Commerce",
                    EngScore = "90"
                });

                string filename = "Sample JAMB Record Upload";
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
                return RedirectToAction("UploadApplicantJambRecord");
            }

            return RedirectToAction("UploadApplicantJambRecord");
        }

        public ActionResult SampleJambOlevelRecordUpload()
        {
            try
            {
                GridView gv = new GridView();
                List<SampleJambOlevelUpload> sample = new List<SampleJambOlevelUpload>();
                sample.Add(new SampleJambOlevelUpload()
                {
                    RegNum = "12345678EG",
                    SubjectName = "English Language",
                    Grade = "A1",
                    ExamSeries = "School Exam",
                    ExamYear = "2021",
                    ExamType = "WAEC Only",
                    ExamNumber = "8374839GM",
                    DateCreated = "-",
                    InstName = "FEDERAL POLYTECHNIC, ILARO",

                });

                sample.Add(new SampleJambOlevelUpload()
                {
                    //SN = "1",
                    RegNum = "12345678EG",
                    SubjectName = "Government",
                    Grade = "A1",
                    ExamSeries = "School Exam",
                    ExamYear = "2021",
                    ExamType = "WAEC Only",
                    ExamNumber = "8374839GM",
                    DateCreated = "-",
                    InstName = "FEDERAL POLYTECHNIC, ILARO",

                });

                string filename = "Sample JAMB Record Upload";
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
                return RedirectToAction("UploadApplicantJambRecord");
            }

            return RedirectToAction("UploadApplicantJambRecord");
        }

        public ActionResult VerifyPaymentMaster()
        {
            SupportViewModel viewModel = new SupportViewModel();
            try
            {
                //ViewBag.FeeType = viewModel.FeeTypeSelectListAlt;
                ViewBag.FeeType = viewModel.FeeTypeSelectList;
                ViewBag.Session = viewModel.SessionSelectList;
                ViewBag.Programme = viewModel.ProgrammeSelectListItem;
                ViewBag.Level = viewModel.LevelSelectList;
                ViewBag.Department = new SelectList(new List<Department>(), ID, NAME);
                //ViewBag.Department = viewModel.DepartmentSelectListItem;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult VerifyPaymentMaster(SupportViewModel viewModel)
        {
            try
            {
                TempData["PaymentConfirmation"] = viewModel;
                //SupportViewModel vModel = (SupportViewModel)TempData["PaymentConfirmation"];
                ViewBag.FeeType = viewModel.FeeTypeSelectList;
                ViewBag.Session = viewModel.SessionSelectList;
                ViewBag.Programme = viewModel.ProgrammeSelectListItem;
                ViewBag.Level = viewModel.LevelSelectList;
                viewModel.DepartmentSelectListItem = Utility.PopulateDepartmentSelectListItem(viewModel.Programme);
                ViewBag.Department = viewModel.DepartmentSelectListItem;
                //ViewBag.Department = new SelectList(new List<Department>(), ID, NAME);

                var _DateFrom = Convert.ToDateTime(viewModel.DateFrom);
                var _DateTo = Convert.ToDateTime(viewModel.DateTo);

                var date__init = Convert.ToDateTime("01/01/0001");
                if ((_DateFrom == date__init || _DateTo == date__init) && (viewModel.FeeType.Id == 18 || viewModel.FeeType.Id == 14))
                {
                    SetMessage("Select date range to continue", Message.Category.Error);
                    return View(viewModel);
                }
                RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                PaymentLogic paymentLogic = new PaymentLogic();
                List<FeePaymentVerification> payments = paymentLogic.GetPaymentConfirmation(viewModel.Session, viewModel.Programme, viewModel.FeeType, viewModel.Department, viewModel.Level, _DateFrom, _DateTo);
                viewModel.PaymentVerificationList = payments;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View(viewModel);
        }

        public JsonResult VerifyPaymentSingle(string pmid)
        {
            try
            {
                var paymentId = Convert.ToInt32(pmid);
                RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                RemitaPayment remitaPayment = remitaPaymentLogic.GetModelBy(x => x.Payment_Id == paymentId);
                PaymentLogic paymentLogic = new PaymentLogic();
                PaymentVerificationLogic paymentVerificationLogic = new PaymentVerificationLogic();
                UserLogic userLogic = new UserLogic();
                User user = userLogic.GetModelsBy(x => x.User_Name == User.Identity.Name).FirstOrDefault();
                string client = Request.LogonUserIdentity.Name + " (" + HttpContext.Request.UserHostAddress + ")";
                remitaPayment = remitaPaymentLogic.GetModelsBy(f => f.Payment_Id == paymentId).LastOrDefault();
 
                PaymentVerification paymentVerification = new PaymentVerification();
                paymentVerification.User = user;
                paymentVerification.Payment = remitaPayment.payment;
                paymentVerification.DateVerified = DateTime.Now;
                paymentVerification.Comment = client;
                paymentVerificationLogic.Create(paymentVerification);

                return Json(new { statusCode = 200, message = "Verification sucessful" }, "text/html", JsonRequestBehavior.AllowGet);

                
            }
            catch (Exception ex)
            {
                return Json(new { statusCode = 400, message = "Verification unsucessful" + ex }, "text/html", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult VerifyPaymentBulk(int verifyAll)
        {
            try
            {
                SupportViewModel vModel = (SupportViewModel)TempData["PaymentConfirmation"];
                if(vModel != null && vModel.PaymentVerificationList != null && vModel.PaymentVerificationList.Count > 0)
                {
                    RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                    RemitaPayment remitaPayment = new RemitaPayment();
                    PaymentLogic paymentLogic = new PaymentLogic();
                    PaymentVerificationLogic paymentVerificationLogic = new PaymentVerificationLogic();
                    UserLogic userLogic = new UserLogic();
                    foreach (var item in vModel.PaymentVerificationList)
                    {
                        
                        var isVerified = paymentVerificationLogic.GetModelBy(x => x.Payment_Id == item.paymentId);
                        if(isVerified == null)
                        {
                            User user = userLogic.GetModelsBy(x => x.User_Name == User.Identity.Name).FirstOrDefault();
                            string client = Request.LogonUserIdentity.Name + " (" + HttpContext.Request.UserHostAddress + ")";
                            remitaPayment = remitaPaymentLogic.GetModelsBy(f => f.Payment_Id == item.paymentId).LastOrDefault();
                            PaymentVerification paymentVerification = new PaymentVerification();
                            paymentVerification.User = user;
                            paymentVerification.Payment = remitaPayment.payment;
                            paymentVerification.DateVerified = DateTime.Now;
                            paymentVerification.Comment = client;
                            paymentVerificationLogic.Create(paymentVerification);
                        }
                                             
                    }
                    
                }
               
                return Json(new { statusCode = 200, message = "Verification sucessful" }, "text/html", JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { statusCode = 400, message = "Verification unsucessful" + ex }, "text/html", JsonRequestBehavior.AllowGet);
                throw ex;
            }
        }


        public ActionResult VerifiedPaymentsReport()
        {
            SupportViewModel viewModel = new SupportViewModel();
            try
            {
                ViewBag.FeeType = viewModel.FeeTypeSelectList;
                ViewBag.Session = viewModel.SessionSelectList;
                ViewBag.Programme = viewModel.ProgrammeSelectListItem;
                ViewBag.School = viewModel.FacultySelectListItem;
                ViewBag.Level = viewModel.LevelSelectList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View(viewModel);
        }
        [HttpPost]
        public ActionResult VerifiedPaymentsReport(SupportViewModel viewModel)
        {
            try
            {
                ViewBag.FeeType = viewModel.FeeTypeSelectList;
                ViewBag.Session = viewModel.SessionSelectList;
                ViewBag.Programme = viewModel.ProgrammeSelectListItem;
                ViewBag.School = viewModel.FacultySelectListItem;
                ViewBag.Level = viewModel.LevelSelectList;
                PaymentLogic paymentLogic = new PaymentLogic();
                List<FeePaymentVerification> confirmPayments = new List<FeePaymentVerification>();
                var _DateFrom = Convert.ToDateTime(viewModel.DateFrom);
                var _DateTo = Convert.ToDateTime(viewModel.DateTo);
                if (viewModel.Level != null && viewModel.Level.Id > 0)
                {
                    confirmPayments = paymentLogic.GetVerifiedPayments(viewModel.Department.Faculty, viewModel.Level, viewModel.Session, viewModel.Programme, viewModel.FeeType, viewModel.Department, _DateFrom, _DateTo);
                }
                else
                {
                    confirmPayments = paymentLogic.GetVerifiedAcceptance(viewModel.Department.Faculty, viewModel.Session, viewModel.Programme, viewModel.FeeType, viewModel.Department, _DateFrom, _DateTo);
                }
                viewModel.PaymentVerificationList = confirmPayments;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View(viewModel);
        }
    }
}
