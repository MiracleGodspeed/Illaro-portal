using Abundance_Nk.Business;
using Abundance_Nk.Model.Entity.Model;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Web.Areas.Admin.ViewModels;
using Abundance_Nk.Web.Areas.Student.ViewModels;
using Abundance_Nk.Web.Controllers;
using Abundance_Nk.Web.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace Abundance_Nk.Web.Areas.Admin.Controllers
{
    [AllowAnonymous]
    public class ChangeCourseController : BaseController
    {
        ChangeCourseViewModel viewModel;
        // GET: Student/ChangeCourse
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SearchResult(ChangeCourseViewModel model)
        {
            viewModel = new ChangeCourseViewModel();
            try
            {
                ApplicationFormLogic applicationFormLogic = new ApplicationFormLogic();
                AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
                PersonLogic personLogic = new PersonLogic();
                ProgrammeLogic programmeLogic = new ProgrammeLogic();
                DepartmentLogic departmentLogic = new DepartmentLogic();
                ApplicationForm applicationForm = applicationFormLogic.GetModelBy(p => p.Application_Form_Number == model.ApplicationFormNumber);
                if (applicationForm != null)
                {
                    Person person = personLogic.GetModelBy(p => p.Person_Id == applicationForm.Person.Id);
                    List<Person> persons = personLogic.GetModelsBy(p => p.First_Name == person.FirstName && p.Last_Name == person.LastName && p.Date_Of_Birth == person.DateOfBirth && p.Date_Entered == person.DateEntered);
                    viewModel.Payment = isStudentAlreadyExist(persons);
                    AppliedCourse studentAppliedCourse = appliedCourseLogic.GetModelBy(p => p.Application_Form_Id == applicationForm.Id);
                    model.AppliedCourse = studentAppliedCourse;

                    getOldSchoolFees(model);

                    AppliedCourse appliedCourse = appliedCourseLogic.GetModelBy(p => p.Person_Id == person.Id);
                    viewModel.ApplicationForm = applicationForm;
                    viewModel.Person = person;
                    viewModel.AppliedCourse = appliedCourse;
                    ViewBag.ProgrammeId = viewModel.ProgrammeSelectList;
                    ViewBag.Departments = new SelectList(new List<Department>(), Utility.ID, Utility.NAME);

                    PopulateDropdownlist();

                    return View(viewModel);
                }
                else
                {
                    SetMessage("Invalid ApplicationFormNumber Form Number!", Message.Category.Error);
                }
                return RedirectToAction("Index");

            }
            catch (Exception ex)
            {
                SetMessage("Error Occured !" + ex.Message, Message.Category.Error);
                return RedirectToAction("Index");
            }
        }

        private void getOldSchoolFees(ChangeCourseViewModel model)
        {
            try
            {
                int levelId = GetLevel(model.AppliedCourse.Programme.Id);
                List<FeeDetail> oldSchoolFees = new List<FeeDetail>();
                FeeDetailLogic feeDetailLogic = new FeeDetailLogic();
                SessionLogic sessionLogic = new SessionLogic();
                Session session = sessionLogic.GetModelBy(p => p.Activated == true);
                oldSchoolFees = feeDetailLogic.GetModelsBy(p => p.Department_Id == model.AppliedCourse.Department.Id && p.Programme_Id == model.AppliedCourse.Programme.Id && p.Level_Id == levelId && p.Fee_Type_Id == (int)FeeTypes.SchoolFees && p.Session_Id == session.Id);
                decimal amount = oldSchoolFees.Sum(p => p.Fee.Amount);
                model.OldSchoolFees = amount;
                TempData["SchoolFees"] = amount;
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

                // List<Level> levels = null;
                List<Department> departments = null;
                Programme programme = new Programme() { Id = Convert.ToInt32(id) };
                if (programme.Id > 0)
                {
                    DepartmentLogic departmentLogic = new DepartmentLogic();
                    departments = departmentLogic.GetBy(programme);

                    //LevelLogic levelLogic = new LevelLogic();
                    //if (programme.Id <= 2)
                    //{
                    //    levels = levelLogic.GetONDs();
                    //}
                    //else if (programme.Id == 3 || programme.Id == 4)
                    //{
                    //    levels = levelLogic.GetHNDs();
                    //}
                    //else if (programme.Id == 5)
                    //{
                    //    levels = levelLogic.GetCertificateCourse();
                    //}
                }
                return Json(new { Departments = departments }, "json", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult GenerateInvoice(ChangeCourseViewModel model)
        {
            try
            {
                model.OldSchoolFees = (decimal)TempData["SchoolFees"];
                DepartmentLogic departmentLogic = new DepartmentLogic();
                ProgrammeLogic programmeLogic = new ProgrammeLogic();
                OLevelResultLogic olevelResultLogic = new OLevelResultLogic();
                OLevelResultDetailLogic olevelResultDetailLogic = new OLevelResultDetailLogic();
                List<OLevelResult> olevelResult = olevelResultLogic.GetModelsBy(p => p.Person_Id == model.Person.Id);
                Department department = departmentLogic.GetModelBy(p => p.Department_Id == model.AppliedCourse.Department.Id);
                Programme programme = programmeLogic.GetModelBy(p => p.Programme_Id == model.AppliedCourse.Programme.Id);

                PaymentLogic paymentLogic = new PaymentLogic();
                List<Payment> oldPayments = paymentLogic.GetModelsBy(p => p.Person_Id == model.Person.Id);
                int feeTypeID = getFeeTypeId(oldPayments);

                if (model != null)
                {
                    using (TransactionScope scope = new TransactionScope())
                    {

                        model.AppliedCourse.Programme = programme;
                        model.AppliedCourse.Department = department;
                        CreatePerson(model);
                        model.Payment = CreatePayment(model, feeTypeID);
                        CreateAppliedCourse(model);
                        model.ApplicationForm = CreateApplicationForm(model);
                        foreach (OLevelResult item in olevelResult)
                        {
                            item.Person = model.Person;
                            item.ApplicationForm = model.ApplicationForm;
                            OLevelResult olevelResultNew = new OLevelResult();
                            olevelResultNew = olevelResultLogic.Create(item);
                            olevelResultNew.Person = model.Person;
                            olevelResultNew.ApplicationForm = model.ApplicationForm;
                            List<OLevelResultDetail> olevelResultDetail = olevelResultDetailLogic.GetModelsBy(p => p.Applicant_O_Level_Result_Id == item.Id);
                            CreateOLevelResultDetail(olevelResultNew, olevelResultDetail);

                        }

                        scope.Complete();
                    }

                    decimal FeeStatus = checkFeesPaid(oldPayments, model);

                    if (FeeStatus == 1)
                    {


                        List<FeeDetail> newPaymentFeeDetail = new List<FeeDetail>();

                        FeeDetailLogic feeDetailLogic = new FeeDetailLogic();
                        int LevelId = GetLevel(model.AppliedCourse.Programme.Id);
                        SessionLogic sessionLogic = new SessionLogic();
                        Session session = sessionLogic.GetModelBy(p => p.Session_Id == model.Session.Id);
                        newPaymentFeeDetail = feeDetailLogic.GetModelsBy(p => p.Department_Id == model.AppliedCourse.Department.Id && p.Programme_Id == model.AppliedCourse.Programme.Id && p.Level_Id == LevelId && p.Session_Id == session.Id);
                        TempData["FeeDetail"] = newPaymentFeeDetail;
                        return RedirectToAction("ShortFallInvoice", "Credential", new { Area = "Common", pmid = model.Payment.Id, });
                    }
                    else
                    {
                        if (FeeStatus > 0)
                        {
                            ShortFall shortFall = new ShortFall();
                            ShortFallLogic shortFallLogic = new ShortFallLogic();
                            shortFall.Payment = model.Payment;
                            shortFall.Amount = (double)FeeStatus;
                            shortFall = shortFallLogic.Create(shortFall);
                            return RedirectToAction("ShortFallInvoice", "Credential", new { Area = "Common", pmid = model.Payment.Id, amount = FeeStatus });

                        }
                        else
                        {
                            List<FeeDetail> newPaymentFeeDetail = new List<FeeDetail>();

                            FeeDetailLogic feeDetailLogic = new FeeDetailLogic();
                            int LevelId = GetLevel(model.AppliedCourse.Programme.Id);
                            SessionLogic sessionLogic = new SessionLogic();
                            Session session = sessionLogic.GetModelBy(p => p.Session_Id == model.Session.Id);
                            newPaymentFeeDetail = feeDetailLogic.GetModelsBy(p => p.Department_Id == model.AppliedCourse.Department.Id && p.Programme_Id == model.AppliedCourse.Programme.Id && p.Level_Id == LevelId && p.Session_Id == session.Id);
                            TempData["FeeDetail"] = newPaymentFeeDetail;
                            return RedirectToAction("ShortFallInvoice", "Credential", new { Area = "Common", pmid = model.Payment.Id, });
                           
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occured !" + ex.Message, Message.Category.Error);
                
            }
            return RedirectToAction("Index");
        }

        private int getFeeTypeId(List<Payment> oldPayments)
        {
            try
            {
                int schoolFeesStatus = 0;
                int acceptanceFeeStatus = 0;
                int otherFeeStatus = 0;

                foreach (Payment oldPayment in oldPayments)
                {

                    if (oldPayment.FeeType.Id == (int)FeeTypes.AcceptanceFee)
                    {
                        acceptanceFeeStatus += 3;//use schoolFeeType
                    }
                    else if (oldPayment.FeeType.Id == (int)FeeTypes.SchoolFees)
                    {
                        schoolFeesStatus += 12;//use shortFallFeeType
                    }
                    else
                    {
                        otherFeeStatus = oldPayment.FeeType.Id;
                    }
                }
                if (schoolFeesStatus > 0 && acceptanceFeeStatus > 0)
                {
                    return 12;
                }
                else if (acceptanceFeeStatus > 0)
                {
                    return 3;
                }
                else
                {
                    return otherFeeStatus;
                }
            
            }
            catch (Exception)
            {

                throw;
            }
        }

        private decimal checkFeesPaid(List<Payment> oldPayments, ChangeCourseViewModel model)
        {
            try
            {
                int schoolFeesStatus = 0;
                int acceptanceFeeStatus = 0;
                Person person = null;

                // Payment payment = oldPayments.Where(p => p.FeeType.Id == (int)FeeTypes.SchoolFees).SingleOrDefault();

                foreach (Payment oldPayment in oldPayments)
                {
                    person = oldPayment.Person;

                    if (oldPayment.FeeType.Id == (int)FeeTypes.AcceptanceFee)
                    {
                        acceptanceFeeStatus += 1;
                    }
                    if (oldPayment.FeeType.Id == (int)(int)FeeTypes.SchoolFees)
                    {
                        schoolFeesStatus += 1;
                    }
                }
                TempData["OldPerson"] = person;
                TempData.Keep("OldPerson");
                if (schoolFeesStatus > 0 && acceptanceFeeStatus > 0)
                {
                    // Payment newPayment = model.Payment;
                    List<FeeDetail> newPaymentFeeDetail = new List<FeeDetail>();
                    FeeDetailLogic feeDetailLogic = new FeeDetailLogic();
                    int LevelId = GetLevel(model.AppliedCourse.Programme.Id);
                    SessionLogic sessionLogic = new SessionLogic();
                    Session session = sessionLogic.GetModelBy(p => p.Activated == true);
                    newPaymentFeeDetail = feeDetailLogic.GetModelsBy(p => p.Department_Id == model.AppliedCourse.Department.Id && p.Programme_Id == model.AppliedCourse.Programme.Id && p.Level_Id == LevelId && p.Session_Id == session.Id);

                    CourseRegistration courseRegistration = new CourseRegistration();
                    CourseRegistrationLogic courseRegistrationLogic = new CourseRegistrationLogic();
                    List<CourseRegistrationDetail> courseRegistrationDetailList = new List<CourseRegistrationDetail>();
                    CourseRegistrationDetailLogic courseRegistrationDetaillogic = new CourseRegistrationDetailLogic();
                    courseRegistration = courseRegistrationLogic.GetModelBy(p => p.Person_Id == person.Id);
                    if (courseRegistration != null)
                    {
                        using (TransactionScope scope = new TransactionScope())
                        {
                            bool courseRegistrationDetailDeleteStatus = courseRegistrationDetaillogic.Delete(p => p.Student_Course_Registration_Id == courseRegistration.Id);
                            bool courseRegistrationDeleteStatus = courseRegistrationLogic.Delete(p => p.Person_Id == person.Id);
                            scope.Complete();
                        }
                    }


                    decimal newSchoolFees = newPaymentFeeDetail.Sum(p => p.Fee.Amount);
                    decimal shortFall = newSchoolFees - model.OldSchoolFees;
                    return shortFall;
                }
                else if (acceptanceFeeStatus > 0)
                {
                    return 1;
                }
                return 0;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public enum FeeStatus
        {
            OneFee = 1,
            TwoFees = 2
        }

        private Person CreatePerson(ChangeCourseViewModel viewModel)
        {
            try
            {
                PersonLogic personLogic = new PersonLogic();
                Person person = personLogic.GetModelBy(p => p.Person_Id == viewModel.Person.Id);
                person = personLogic.Create(person);
                if (person != null && person.Id > 0)
                {
                    viewModel.Person = person;
                }

                return person;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Payment CreatePayment(ChangeCourseViewModel viewModel, int feeTypeID)
        {
            try
            {
                PaymentMode paymentMode = new PaymentMode { Id = (int)PaymentModes.Full };
                PaymentType paymentType = new PaymentType { Id = (int)Paymenttypes.OnlinePayment };
                FeeType feeType = new FeeType { Id = feeTypeID };
                Session session = new Session { Id = viewModel.Session != null ? viewModel.Session.Id : (int)Sessions._20182019 };
                Payment payment = new Payment();
                PaymentLogic paymentLogic = new PaymentLogic();
                payment.Person = viewModel.Person;
                payment.PaymentMode = paymentMode;
                payment.PaymentType = paymentType;
                payment.PersonType = viewModel.Person.Type;
                payment.FeeType = feeType;
                payment.Session = session;
                payment.DatePaid = DateTime.Now;
                payment = paymentLogic.Create(payment);
                OnlinePayment newOnlinePayment = null;
                if (payment != null)
                {
                    PaymentChannel channel = new PaymentChannel() { Id = (int)PaymentChannel.Channels.Etranzact };
                    OnlinePaymentLogic onlinePaymentLogic = new OnlinePaymentLogic();
                    OnlinePayment onlinePayment = new OnlinePayment();
                    onlinePayment.Channel = channel;
                    onlinePayment.Payment = payment;

                    newOnlinePayment = onlinePaymentLogic.Create(onlinePayment);
                }


                return payment;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public ApplicationForm CreateApplicationForm(ChangeCourseViewModel viewModel)
        {
            try
            {
                ApplicationFormLogic applicationFormLogic = new ApplicationFormLogic();
                ApplicationForm applicationForm = new ApplicationForm();
                ApplicationProgrammeFeeLogic appProgrammeFeeLogic = new ApplicationProgrammeFeeLogic();
                ApplicationProgrammeFee applicationProgrammeFee = new ApplicationProgrammeFee();
                applicationProgrammeFee = appProgrammeFeeLogic.GetModelBy(p => p.Programme_Id == viewModel.AppliedCourse.Programme.Id && p.Fee_Type_Id == (int)FeeTypes.ShortFall);
                ApplicationFormSetting appFormSetting = new ApplicationFormSetting { Id = 1 };
                applicationForm.Person = viewModel.Person;
                applicationForm.Payment = viewModel.Payment;
                applicationForm.DateSubmitted = DateTime.Now;
                applicationForm.Release = false;
                applicationForm.Rejected = false;
                applicationForm.Setting = appFormSetting;
                applicationForm.ProgrammeFee = applicationProgrammeFee;
                applicationForm = applicationFormLogic.Create(applicationForm, viewModel.AppliedCourse);
                return applicationForm;
            }
            catch (Exception)
            {
                
                throw;
            }
         
        }

        public AppliedCourse CreateAppliedCourse(ChangeCourseViewModel viewModel)
        {
            try
            {
                AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
                AppliedCourse appliedCourse = new AppliedCourse();
                AppliedCourse newAppliedCourse = new AppliedCourse();
                appliedCourse.Person = viewModel.Person;
                appliedCourse.Programme = viewModel.AppliedCourse.Programme;
                appliedCourse.Department = viewModel.AppliedCourse.Department;
                return appliedCourseLogic.Create(appliedCourse);
            }
            catch (Exception)
            {
                
                throw;
            }
           

        }

        public OLevelResultDetail CreateOLevelResultDetail(OLevelResult model, List<OLevelResultDetail> models)
        {
            try
            {
                OlevelResultdDetailsAudit olevelResultdDetailsAudit = new OlevelResultdDetailsAudit();
                olevelResultdDetailsAudit.Action = "Modify";
                olevelResultdDetailsAudit.Operation = "Modify O level (ChangeCourse Controller)";
                olevelResultdDetailsAudit.Client =  Request.LogonUserIdentity.Name + " (" + HttpContext.Request.UserHostAddress + ")";
                UserLogic loggeduser = new UserLogic();
                olevelResultdDetailsAudit.User = loggeduser.GetModelBy(u => u.User_Name == User.Identity.Name);

                OLevelResultDetailLogic olevelResultDetailLogic = new OLevelResultDetailLogic();
                OLevelResultDetail olevelResultDetail = null;
                for (int i = 0; i < models.Count; i++)
                {
                    models[i].Header = model;
                    olevelResultDetail = olevelResultDetailLogic.Create(models[i],olevelResultdDetailsAudit);
                }
                return olevelResultDetail;
            }
            catch (Exception)
            {
                
                throw;
            }
         
        }

        public Payment isStudentAlreadyExist(List<Person> persons)
        {
            try
            {
                PaymentLogic paymentLogic = new PaymentLogic();
                Payment payment = null;
                foreach (Person p in persons)
                {
                    payment = paymentLogic.GetModelBy(pl => pl.Person_Id == p.Id && pl.Fee_Type_Id == (int)FeeTypes.ShortFall);
                    if (payment != null)
                    {
                        return payment;
                    }
                }
                return payment;
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
        public ActionResult CreateShortFall()
        {
            viewModel = new ChangeCourseViewModel();
            ViewBag.Session = viewModel.SessionSelectList;
            
            return View(viewModel);
        }
        [HttpPost]
        public ActionResult CreateShortFall(ChangeCourseViewModel model)
        {
            viewModel = new ChangeCourseViewModel();
            try
            {
                ApplicationFormLogic applicationFormLogic = new ApplicationFormLogic();
                AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
                StudentLogic studentLogic = new StudentLogic();
                PersonLogic personLogic = new PersonLogic();
                ApplicationForm applicationForm = applicationFormLogic.GetModelBy(p => p.Application_Form_Number == model.ApplicationFormNumber);
                List<Abundance_Nk.Model.Model.Student> student = studentLogic.GetModelsBy(p => p.Matric_Number == model.ApplicationFormNumber);
                if (applicationForm != null)
                {
                    Person person = personLogic.GetModelBy(p => p.Person_Id == applicationForm.Person.Id);                 
                    AppliedCourse studentAppliedCourse = appliedCourseLogic.GetModelBy(p => p.Application_Form_Id == applicationForm.Id);
                    model.AppliedCourse = studentAppliedCourse;
                    viewModel.ApplicationForm = applicationForm;
                    viewModel.Person = person;
                    viewModel.ApplicationFormNumber = model.ApplicationFormNumber;
                    viewModel.AppliedCourse = studentAppliedCourse;
                    ViewBag.ProgrammeId = viewModel.ProgrammeSelectList;
                    ViewBag.Departments = new SelectList(new List<Department>(), Utility.ID, Utility.NAME);

                    PopulateDropdownlist();

                    return View(viewModel);
                }
                else if (student.Count == 1)
                {
                    Abundance_Nk.Model.Model.Student studnetItem = student[0];
                    if (studnetItem != null)
                    {
                        Person person = personLogic.GetModelBy(p => p.Person_Id == studnetItem.Id);
                        TempData["person"] = person;
                        viewModel.Person = person;
                        ViewBag.ProgrammeId = viewModel.ProgrammeSelectList;
                        ViewBag.Departments = new SelectList(new List<Department>(), Utility.ID, Utility.NAME);

                        PopulateDropdownlist();

                        return View(viewModel);
                    }
                }
                else
                {
                    SetMessage("Invalid ApplicationFormNumber Form Number!", Message.Category.Error);
                }
                return RedirectToAction("Index");

            }
            catch (Exception ex)
            {
                SetMessage("Error Occured !" + ex.Message, Message.Category.Error);
                return RedirectToAction("Index");
            }
        }

        private void PopulateDropdownlist()
        {
            try
            {
                if (viewModel.Session != null && viewModel.Session.Id > 0)
                    ViewBag.Session = new SelectList(viewModel.SessionSelectList, "Value", "Text", viewModel.Session.Id);
                else
                    ViewBag.Session = viewModel.SessionSelectList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public ActionResult CreateShortFallInvoice(ChangeCourseViewModel model)
        {
            try
            {
                ApplicationFormLogic applicationFormLogic = new ApplicationFormLogic();
                ApplicationForm applicationForm = new ApplicationForm();           
                ShortFall shortFall = new ShortFall();
                ShortFallLogic shortFallLogic = new ShortFallLogic();
                applicationForm = applicationFormLogic.GetModelBy(p => p.Application_Form_Number == model.ApplicationFormNumber);
                if (applicationForm != null)
                {
                    model.Person = applicationForm.Person;
                }
                else
                {
                    model.Person = (Person)TempData["person"];
                }
                
                PaymentLogic paymentLogic = new PaymentLogic();
                Payment payment = CreatePayment(model, (int)FeeTypes.ShortFall);
                shortFall.Payment = payment;
                shortFall.Amount = (double)model.ShortFallAmount;
                shortFall = shortFallLogic.Create(shortFall);

                RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                RemitaPayment remitaPayment = remitaPaymentLogic.GetBy(payment.Id);

                if (remitaPayment == null)
                {
                    //Get Payment Specific Setting
                    RemitaSettingsLogic settingsLogic = new RemitaSettingsLogic();
                    RemitaSettings settings = settingsLogic.GetBy(9);
                    RemitaPayementProcessor remitaPayementProcessor = new RemitaPayementProcessor(settings.Api_key);
                    string remitaBaseUrl = ConfigurationManager.AppSettings["RemitaBaseUrl"].ToString();
                    remitaPayment = remitaPayementProcessor.GenerateRRRCard(payment.InvoiceNumber, remitaBaseUrl, "SHORTFALL", settings, model.ShortFallAmount,"RRGEN");
                }


                return RedirectToAction("ShortFallInvoice", "Credential", new { Area = "Common", pmid = payment.Id, amount = model.ShortFallAmount });
            }
            catch (Exception ex)
            {
                SetMessage("Error Occured !" + ex.Message, Message.Category.Error);
                return RedirectToAction("CreateShortFall");
                throw;
            }
   
            
        }


    }
}