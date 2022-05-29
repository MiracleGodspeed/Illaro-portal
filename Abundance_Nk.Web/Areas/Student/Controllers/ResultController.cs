using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using Abundance_Nk.Web.Areas.Student.ViewModels;
using Abundance_Nk.Web.Controllers;
using Abundance_Nk.Business;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Web.Models;

namespace Abundance_Nk.Web.Areas.Student.Controllers
{
    [AllowAnonymous]
    public class ResultController : BaseController
    {
        private ResultViewModel viewModel;
        private StudentLogic studentLogic;
        private StudentLevelLogic studentLevelLogic;
        public ResultController()
        {
            viewModel = new ResultViewModel();
            studentLogic = new StudentLogic();
        }

        public ActionResult Check()
        {
            try
            {
                //return RedirectToAction("Index","Home",new{Area =""});
                viewModel = new ResultViewModel();
                ViewBag.Session = viewModel.SessionSelectList;
                ViewBag.Semester = new SelectList(new List<Semester>(), "Id", "Name");

                if (System.Web.HttpContext.Current.Session["student"] != null)
                {
                    studentLogic = new StudentLogic();
                    Model.Model.Student student = System.Web.HttpContext.Current.Session["student"] as Model.Model.Student;
                    student = studentLogic.GetBy(student.Id);

                    viewModel.MatricNumber = student.MatricNumber;
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }
            
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Check(ResultViewModel vModel)
        {
            try
            {
                //SetMessage("Result checking has been closed, check back later!", Message.Category.Error);
                //ViewBag.Session = viewModel.SessionSelectList;
                //ViewBag.Semester = new SelectList(new List<Semester>(), "Id", "Name");
                //return View(vModel);

                //SetMessage("Visit Exams & Records for your statement of result!", Message.Category.Error);
                //return View(vModel);
                if (ModelState.IsValid)
                {
                    Model.Model.Student student = System.Web.HttpContext.Current.Session["student"] as Model.Model.Student;
                    student = studentLogic.GetBy(student.Id);
                    if (student != null && student.Id > 0)
                    {
                        vModel.MatricNumber = student.MatricNumber;
                        if (student.Activated == false)
                        {
                            SetMessage("Result not available!", Message.Category.Error);
                            ViewBag.Session = viewModel.SessionSelectList;
                            ViewBag.Semester = new SelectList(new List<Semester>(), "Id", "Name");
                            return View(vModel);
                        }

                        StudentResultStatus studentResultStatus = new StudentResultStatus();
                        StudentResultStatusLogic studentResultStatusLogic = new StudentResultStatusLogic();
                        studentLevelLogic = new StudentLevelLogic();
                        StudentLevel studentLevel = studentLevelLogic.GetBy(student.Id);
                        //
                        studentResultStatus = studentResultStatusLogic.GetModelBy(s => s.Faculty_Id == studentLevel.Department.Faculty.Id && s.Programme_Id == studentLevel.Programme.Id && s.Session_Id == vModel.Session.Id && s.RAndDCApproval &&
                            s.DRAcademicsApproval && s.RegistrarApproval && s.Semester_Id == vModel.Semester.Id);
                        
                        if (studentResultStatus == null || studentResultStatus.Id <= 0)
                        {
                            SetMessage("Result for your school hasn't been released!", Message.Category.Error);
                            ViewBag.Session = viewModel.SessionSelectList;
                            ViewBag.Semester = new SelectList(new List<Semester>(), "Id", "Name");
                            return View(vModel);
                        }
                        var hasCompletedSchoolFees=HascompletedSchoolFees(student.Id);
                        if (!hasCompletedSchoolFees)
                        {
                            SetMessage("Please ensure you have completed all your school fees payment!", Message.Category.Error);
                            ViewBag.Session = viewModel.SessionSelectList;
                            ViewBag.Semester = new SelectList(new List<Semester>(), "Id", "Name");
                            return View(vModel);
                        }
                        CourseEvaluationAnswerLogic courseEvaluationAnswerLogic = new CourseEvaluationAnswerLogic();
                        List<CourseEvaluationAnswer> courseEvaluationAnswers = courseEvaluationAnswerLogic.GetModelsBy(a => a.Person_Id == student.Id && a.Semester_Id == vModel.Semester.Id && a.Session_Id == vModel.Session.Id);
                        if (courseEvaluationAnswers != null && courseEvaluationAnswers.Count > 0)
                        {
                            if (studentResultStatus.Id  > 0)
                            {
                               return RedirectToAction("Statement", new { sid = Utility.Encrypt(student.Id.ToString()) });
                            }
                            else
                            {
                                 SetMessage("Result for your school hasn't been released!", Message.Category.Error);
                            }
                        }
                        else
                        {
                             return RedirectToAction("SemesterResult", new { sid = Utility.Encrypt(student.Id.ToString()), sesid = vModel.Session.Id, semid = vModel.Semester.Id});
                        }
                    }
                    else
                    {
                        SetMessage("Invalid Matric Number!", Message.Category.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            ViewBag.Session = viewModel.SessionSelectList;
            ViewBag.Semester = new SelectList(new List<Semester>(), "Id", "Name");
            return View(vModel);
        }

        public ActionResult Statement(string sid)
        {
            long Id = Convert.ToInt64(Utility.Decrypt(sid));
            ViewBag.StudentId = Id;

            return View();
        }

        public ActionResult SemesterResult(string sid, int sesid, int semid)
        {
            viewModel = new ResultViewModel();
            if (sid == null)
            {
                 return RedirectToAction("Check");
            }
            try
            {
                long Id = Convert.ToInt64(Utility.Decrypt(sid));
                studentLevelLogic = new StudentLevelLogic();
                StudentLevel studentLevel = new StudentLevel();
                if (Id > 0)
                {
                    //studentLevel = studentLevelLogic.GetBy(Id);
                    studentLevel = studentLevelLogic.GetModelsBy(s => s.Person_Id == Id && s.Session_Id == sesid).LastOrDefault();
                }
                
                CourseLogic courseLogic = new CourseLogic();
                CourseEvaluationQuestionLogic courseEvaluationQuestionLogic = new CourseEvaluationQuestionLogic();
                ViewBag.ScoreId = viewModel.ScoreSelectListItem;
                viewModel.StudentLevel = studentLevel;
                viewModel.CourseEvaluationQuestionsForSectionOne = courseEvaluationQuestionLogic.GetModelsBy(a => a.Section == 1 && a.Activated == true);
                viewModel.CourseEvaluationQuestionsForSectionTwo = courseEvaluationQuestionLogic.GetModelsBy(a => a.Section == 2 && a.Activated == true);
                viewModel.CourseEvaluations = new List<CourseEvaluation>();
                viewModel.CourseEvaluationsTwo = new List<CourseEvaluation>();
                if (studentLevel.DepartmentOption != null)
                    viewModel.Courses = courseLogic.GetBy(studentLevel.Programme, studentLevel.Department, studentLevel.DepartmentOption, studentLevel.Level, new Semester() { Id = semid }, true);
                else
                    viewModel.Courses = courseLogic.GetBy(studentLevel.Programme, studentLevel.Department, studentLevel.Level, new Semester() { Id = semid }, true);

                viewModel.Session = new Session(){Id = sesid};
                viewModel.Semester = new Semester(){ Id = semid};

                TempData["ViewModel"] = viewModel;

                if (viewModel.Courses != null && viewModel.Courses.Count > 0)
                {
                    foreach (Course course in viewModel.Courses)
                    {
                        CourseEvaluation courseEvaluation = new CourseEvaluation();
                        courseEvaluation.Course = course;
                        courseEvaluation.CourseEvaluationQuestion = viewModel.CourseEvaluationQuestionsForSectionOne;
                        viewModel.CourseEvaluations.Add(courseEvaluation);
                    }

                    foreach (Course course in viewModel.Courses)
                    {
                        CourseEvaluation courseEvaluation = new CourseEvaluation();
                        courseEvaluation.Course = course;
                        courseEvaluation.CourseEvaluationQuestion = viewModel.CourseEvaluationQuestionsForSectionTwo;
                        viewModel.CourseEvaluationsTwo.Add(courseEvaluation);
                    }
                }

            }
            catch (Exception)
            {
                    
                throw;
            }

            return View(viewModel);
        }
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult SemesterResult(FormCollection resultViewModel)
        {
            try
            {
                ResultViewModel viewModel = (ResultViewModel) TempData["ViewModel"];

                long studentId = Convert.ToInt64(resultViewModel["StudentLevel.Student.Id"]);
                System.Text.StringBuilder st = new System.Text.StringBuilder();
                List<string> answeredQuestions = new List<string>();
                List<string> answeredQuestionsSection2 = new List<string>();
                foreach (string key in resultViewModel.Keys)
                {
                    if (key.StartsWith("Question_"))
                    {
                       answeredQuestions.Add(key);
                    }
                     if (key.StartsWith("SectionQuestion_"))
                    {
                       answeredQuestionsSection2.Add(key);
                    }
                }
                using (TransactionScope scope = new TransactionScope())
                {
                    foreach (string s in answeredQuestions)
                    {
                        CourseEvaluationAnswerLogic courseEvaluationAnswerLogic = new CourseEvaluationAnswerLogic();
                        CourseEvaluationAnswer courseEvaluationAnswer = new CourseEvaluationAnswer();
                        
                        int courseIndex = s.IndexOf("Course_Id_");
                        string Coursestring = s.Substring(courseIndex);
                        string CourseId = s.Substring(courseIndex).Replace("Course_Id_","");
                        string QuestionId = s.Replace("Question_", "").Replace(Coursestring,"");
                            

                        courseEvaluationAnswer.CourseEvaluationQuestion = new CourseEvaluationQuestion();
                        courseEvaluationAnswer.CourseEvaluationQuestion.Id = Convert.ToInt32(QuestionId);
                        courseEvaluationAnswer.Course = new Course();
                        courseEvaluationAnswer.Course.Id = Convert.ToInt32(CourseId);
                        courseEvaluationAnswer.Score = 1;
                        courseEvaluationAnswer.Semester = new Semester(){Id = viewModel.Semester.Id};
                        courseEvaluationAnswer.Session = new Session(){Id = viewModel.Session.Id};
                        courseEvaluationAnswer.Student = new Model.Model.Student();
                        courseEvaluationAnswer.Student = studentLogic.GetBy(studentId);
                        courseEvaluationAnswerLogic.Create(courseEvaluationAnswer);
                    }

                     foreach (string s in answeredQuestionsSection2)
                     {
                        int value = 0;
                        Int32.TryParse(resultViewModel[s], out value);
                        if (value > 0)
                        {
                            CourseEvaluationAnswerLogic courseEvaluationAnswerLogic = new CourseEvaluationAnswerLogic();
                            CourseEvaluationAnswer courseEvaluationAnswer = new CourseEvaluationAnswer();
                            int courseIndex = s.IndexOf("Course_Id_");
                            string Coursestring = s.Substring(courseIndex);
                            string CourseId = s.Substring(courseIndex).Replace("Course_Id_","");
                            string QuestionId = s.Replace("SectionQuestion_", "").Replace(Coursestring,"");
                            
                            courseEvaluationAnswer.CourseEvaluationQuestion = new CourseEvaluationQuestion();
                            courseEvaluationAnswer.CourseEvaluationQuestion.Id = Convert.ToInt32(QuestionId);
                            courseEvaluationAnswer.Course = new Course();
                            courseEvaluationAnswer.Course.Id = Convert.ToInt32(CourseId);
                            courseEvaluationAnswer.Score = value;
                            courseEvaluationAnswer.Semester = new Semester() { Id = viewModel.Semester.Id };
                            courseEvaluationAnswer.Session = new Session() { Id = viewModel.Session.Id };
                            courseEvaluationAnswer.Student = new Model.Model.Student();
                            courseEvaluationAnswer.Student = studentLogic.GetBy(studentId);
                            courseEvaluationAnswerLogic.Create(courseEvaluationAnswer);
                        }

                       
                    }
                    scope.Complete();
                      SetMessage("Thank you for filling the survey, You may now proceed to check your result!", Message.Category.Information);
                         
                    return RedirectToAction("Check");
                }
                
            }
            catch (Exception)
            {
                    
                throw;
            }

            return View();
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
                foreach (SessionSemester sessionSemester in sessionSemesterList)
                {
                    semesters.Add(sessionSemester.Semester);
                }

                return Json(new SelectList(semesters, "Id", "Name"), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool HascompletedSchoolFees(long studentId)
        {
            if (studentId>0)
            {
                    RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                    PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
                    PaymentLogic paymentLogic = new PaymentLogic();

                    // check for students in second year
                    CourseRegistrationLogic courseRegistrationLogic = new CourseRegistrationLogic();
                    var studentRegistrations = courseRegistrationLogic.GetModelsBy(f => f.STUDENT.Person_Id == studentId);
                    if (studentRegistrations?.Count > 0)
                    {
                        foreach (var item in studentRegistrations)
                        {
                            var paidSchoolFees = remitaPaymentLogic.GetModelsBy(f => f.PAYMENT.Person_Id == item.Student.Id && f.PAYMENT.Session_Id == item.Session.Id &&
                            (f.PAYMENT.Fee_Type_Id == 3 || f.PAYMENT.Fee_Type_Id == 10) && (f.Status.Contains("01") || f.Description.Contains("manual"))).FirstOrDefault();
                            if (paidSchoolFees == null)
                            {
                                var payment = paymentLogic.GetModelsBy(f => f.Person_Id == item.Student.Id && f.Session_Id == item.Session.Id && (f.Fee_Type_Id == 3 || f.Fee_Type_Id == 10)).FirstOrDefault();
                                if (payment?.Id > 0)
                                {
                                    var etranzactRecord = paymentEtranzactLogic.GetModelsBy(f => f.Payment_Id == payment.Id).FirstOrDefault();
                                    if (etranzactRecord?.ConfirmationNo != null)
                                    {

                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                                else
                                {
                                    return false;
                                }



                            }
                        }
                    }
                    else
                    {
                        return false;
                    }

                    return true;
                
               

            }
            return false;
        }

    }
}