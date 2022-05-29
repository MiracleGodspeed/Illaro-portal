using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Abundance_Nk.Model.Entity.Model;
using Abundance_Nk.Web.Controllers;
using Abundance_Nk.Web.Areas.Student.ViewModels;
using Abundance_Nk.Business;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Web.Models;
using System.Web.Security;
using System.IO;

namespace Abundance_Nk.Web.Areas.Student.Controllers
{
	[AllowAnonymous]
	public class CourseRegistrationController : BaseController
	{
		private CourseLogic courseLogic;
		private StudentLogic studentLogic;
		private CourseRegistrationViewModel viewModel;
	    private CourseRegistrationStatusLogic _registrationStatusLogic;

		public CourseRegistrationController()
		{
			courseLogic = new CourseLogic();
			studentLogic = new StudentLogic();
			viewModel = new CourseRegistrationViewModel();
		}

		public ActionResult Logon()
		{
			return View(viewModel);
		}

		[HttpPost]
		public ActionResult Logon(CourseRegistrationViewModel vModel)
		{
			try
			{
				if (ModelState.IsValid)
				{
					Model.Model.Student student = studentLogic.GetBy(vModel.MatricNumber);
					if (student != null && student.Id > 0)
					{
						return RedirectToAction("Form", new { sid = student.Id });
					}

					SetMessage("Invalid Matric Number or PIN!", Message.Category.Error);
				}
			}
			catch (Exception ex)
			{
				SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
			}

			return View(vModel);
		}

		public ActionResult Form(string sid, int ssid)
		{
			try
			{
                bool hasPaidFees = false;
                //SetMessage("Registration has closed! ", Message.Category.Error);
                //return RedirectToAction("CheckStatus", "Admission", new { area = "Applicant"});

                long StudentId = Convert.ToInt64(Utility.Decrypt(sid));
				SessionSemesterLogic sessionSemesterLogic = new SessionSemesterLogic();
				SessionSemester sessionSemester = null;
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                StudentLevel studentLevel = new StudentLevel();

                if (ssid > 0)
				{
					sessionSemester = sessionSemesterLogic.GetModelBy(s => s.Session_Semester_Id == ssid);
                }
				else
				{
					studentLevel = studentLevelLogic.GetModelsBy(s => s.Person_Id == StudentId).LastOrDefault();
					if (studentLevel == null)
					{
					    SetMessage("No Student Level record for this session! ", Message.Category.Error);
                        return RedirectToAction("SelectCourseRegistrationSession", "Registration", new { Area = "Student" });
                        //return View("Form", viewModel);
					}

					sessionSemester = sessionSemesterLogic.GetModelsBy(s => s.Session_Id == studentLevel.Session.Id).LastOrDefault();
				}

                StudentLogic studentLogic = new StudentLogic();
                Model.Model.Student student = studentLogic.GetModelBy(s => s.Person_Id == StudentId);
                studentLevel = studentLevelLogic.GetModelsBy(s => s.Person_Id == StudentId).LastOrDefault();
                TempData["StudentId"] = student.Id;
                
                if(studentLevel != null && (studentLevel.Programme.Id == (int)Programmes.NDDistance) || studentLevel.Programme.Id == (int)Programmes.HNDDistance)
                {
                    hasPaidFees = Utility.HasCompletedSchoolFeesDistantLearning(student, sessionSemester.Session);
                    if (!hasPaidFees)
                    {
                        hasPaidFees = Utility.HasCompletedSchoolFirstInstallment(student, sessionSemester.Session);

                    }

                }
                else
                {
                    hasPaidFees = Utility.HasCompletedSchoolFees(student, sessionSemester.Session);
                }

                //Check if student medical report has been uploaded
                if (student.StudentMedicalReport == null)
                {
                    var getStudentLevel = studentLevelLogic.GetModelsBy(x => x.Person_Id == StudentId).LastOrDefault();
                    if(getStudentLevel.Level.Id == (int)Levels.HNDI || getStudentLevel.Level.Id == (int)Levels.NDI)
                    {
                        viewModel.IsMedicalReportUploaded = false;
                    }
                }

                if (hasPaidFees)
                {
                    PopulateCourseRegistrationForm(StudentId, sessionSemester);
                }
                else
                {
                    //this student repeated 100level and was asked to pay as returning student (29900) which is below the suppose amount (50400) for new students
                    if (student?.Id > 0 && sessionSemester.Session?.Id > 0 && (student.Id == 149264 || student.Id == 148467) && sessionSemester.Session.Id == 11)
                    {
                        PopulateCourseRegistrationForm(StudentId, sessionSemester);
                    }
                    else
                    {
                        SetMessage("You have not paid fees for this session! ", Message.Category.Error);
                        return RedirectToAction("SelectCourseRegistrationSession", "Registration", new { Area = "Student" });
                    }
                    
                }
			}
			catch (Exception ex)
			{
				SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
			}

			TempData["CourseRegistrationViewModel"] = viewModel;
			return View("Form", viewModel);
		}


        [HttpPost]
        public ActionResult Form(CourseRegistrationViewModel viewModel)
        {
            string message = null;

            try
            {
                TempData.Keep("StudentId");
                CourseLogic courseLogic = new CourseLogic();

                string operation = "INSERT";
                string action = "REGISTRATION :COURSE FORM";
                string client = Request.LogonUserIdentity.Name + " (" + HttpContext.Request.UserHostAddress + ")";
                var courseRegistrationDetailAudit = new CourseRegistrationDetailAudit();
                courseRegistrationDetailAudit.Action = action;
                courseRegistrationDetailAudit.Operation = operation;
                courseRegistrationDetailAudit.Client = client;
                UserLogic loggeduser = new UserLogic();
                courseRegistrationDetailAudit.User = loggeduser.GetModelBy(u => u.User_Id == 1);

                List<CourseRegistrationDetail> selectedFirstSemesterCourseRegistrationDetails = null;
                List<CourseRegistrationDetail> selectedSecondSemesterCourseRegistrationDetails = null;
                List<CourseRegistrationDetail> selectedThirdSemesterCourseRegistrationDetails = null;
                List<CourseRegistrationDetail> courseRegistrationDetails = new List<CourseRegistrationDetail>();

                if (viewModel.CarryOverExist)
                {
                    List<CourseRegistrationDetail> selectedCarryOverCourseRegistrationDetails = new List<CourseRegistrationDetail>();
                    selectedCarryOverCourseRegistrationDetails = GetSelectedCourses(viewModel.CarryOverCourses);
                    courseRegistrationDetails.AddRange(selectedCarryOverCourseRegistrationDetails);
                }

                viewModel.RegisteredCourse.Student = viewModel.Student;

                CourseRegistrationLogic courseRegistrationLogic = new CourseRegistrationLogic();
                if (viewModel.CourseAlreadyRegistered) //modify
                {
                    selectedFirstSemesterCourseRegistrationDetails = viewModel.FirstSemesterCourses;
                    selectedSecondSemesterCourseRegistrationDetails = viewModel.SecondSemesterCourses;
                    selectedThirdSemesterCourseRegistrationDetails = viewModel.ThirdSemesterCourses;

                    //courseRegistrationDetails = selectedFirstSemesterCourseRegistrationDetails;
                    courseRegistrationDetails.AddRange(selectedFirstSemesterCourseRegistrationDetails);
                    courseRegistrationDetails.AddRange(selectedSecondSemesterCourseRegistrationDetails);
                    if ((viewModel.StudentLevel.Programme.Id == (int)Programmes.NDPartTime || viewModel.StudentLevel.Programme.Id == (int)Programmes.HNDWeekend || viewModel.StudentLevel.Programme.Id == (int)Programmes.NDDistance || viewModel.StudentLevel.Programme.Id == (int)Programmes.HNDDistance) && selectedThirdSemesterCourseRegistrationDetails != null)
                        courseRegistrationDetails.AddRange(selectedThirdSemesterCourseRegistrationDetails);

                    for (int i = 0; i < courseRegistrationDetails.Count; i++)
                    {
                        CourseRegistrationDetail courseRegistrationDetail = courseRegistrationDetails[i];
                        courseRegistrationDetails[i].CourseUnit = courseLogic.GetModelBy(c => c.Course_Id == courseRegistrationDetail.Course.Id).Unit;
                    }

                    viewModel.RegisteredCourse.Details = courseRegistrationDetails;

                    for (int i = 0; i < viewModel.RegisteredCourse.Details.Count; i++)
                    {
                        viewModel.RegisteredCourse.Details[i].ExamScore = null;
                        viewModel.RegisteredCourse.Details[i].TestScore = null;
                    }

                    courseRegistrationDetailAudit.Operation = "MODIFY: COURSE FORM";
                    bool modified = courseRegistrationLogic.Modify(viewModel.RegisteredCourse, courseRegistrationDetailAudit);
                    if (modified)
                    {
                        message = "Selected courses has been successfully modified.";
                    }
                    else
                    {
                        message = "Course Registration modification Failed! Please try again.";
                    }
                }
                else //insert
                {


                    selectedFirstSemesterCourseRegistrationDetails = GetSelectedCourses(viewModel.FirstSemesterCourses);
                    courseRegistrationDetails.AddRange(selectedFirstSemesterCourseRegistrationDetails);

                    if (viewModel.SecondSemesterCourses != null && viewModel.SecondSemesterCourses.Count > 0)
                    {
                        selectedSecondSemesterCourseRegistrationDetails = GetSelectedCourses(viewModel.SecondSemesterCourses);
                        courseRegistrationDetails.AddRange(selectedSecondSemesterCourseRegistrationDetails);

                    }
                    if (viewModel.ThirdSemesterCourses != null && viewModel.ThirdSemesterCourses.Count > 0)
                    {
                        selectedThirdSemesterCourseRegistrationDetails = GetSelectedCourses(viewModel.ThirdSemesterCourses);
                        courseRegistrationDetails.AddRange(selectedThirdSemesterCourseRegistrationDetails);

                    }

                    //courseRegistrationDetails = selectedFirstSemesterCourseRegistrationDetails;
                    //courseRegistrationDetails.AddRange(selectedFirstSemesterCourseRegistrationDetails);
                    //courseRegistrationDetails.AddRange(selectedSecondSemesterCourseRegistrationDetails);
                    if ((viewModel.StudentLevel.Programme.Id == (int)Programmes.NDPartTime || viewModel.StudentLevel.Programme.Id == (int)Programmes.HNDWeekend) && selectedThirdSemesterCourseRegistrationDetails != null)
                        courseRegistrationDetails.AddRange(selectedThirdSemesterCourseRegistrationDetails);

                    for (int i = 0; i < courseRegistrationDetails.Count; i++)
                    {
                        CourseRegistrationDetail courseRegistrationDetail = courseRegistrationDetails[i];
                        courseRegistrationDetails[i].CourseUnit = courseLogic.GetModelBy(c => c.Course_Id == courseRegistrationDetail.Course.Id).Unit;
                    }

                    viewModel.RegisteredCourse.Details = courseRegistrationDetails;

                    for (int i = 0; i < viewModel.RegisteredCourse.Details.Count; i++)
                    {
                        viewModel.RegisteredCourse.Details[i].ExamScore = null;
                        viewModel.RegisteredCourse.Details[i].TestScore = null;
                    }

                    //viewModel.RegisteredCourse.Student = new Model.Model.Student() { Id = viewModel.Student.Id };


                    viewModel.RegisteredCourse.Level = new Level() { Id = viewModel.StudentLevel.Level.Id };
                    viewModel.RegisteredCourse.Programme = new Programme() { Id = viewModel.StudentLevel.Programme.Id };
                    viewModel.RegisteredCourse.Department = new Department() { Id = viewModel.StudentLevel.Department.Id };
                    viewModel.RegisteredCourse.Session = new Session() { Id = viewModel.CurrentSessionSemester.SessionSemester.Session.Id };
                    CourseRegistration oldCourseRegistration = courseRegistrationLogic.GetBy(viewModel.RegisteredCourse.Student, viewModel.RegisteredCourse.Level, viewModel.RegisteredCourse.Programme,
                            viewModel.RegisteredCourse.Department, viewModel.RegisteredCourse.Session);

                    CourseRegistration courseRegistration = new CourseRegistration();

                    if (oldCourseRegistration != null)
                    {
                        CourseRegistrationDetailLogic courseRegistrationDetailLogic = new CourseRegistrationDetailLogic();

                        courseRegistrationDetailLogic.Delete(c => c.Student_Course_Registration_Id == oldCourseRegistration.Id);
                        //courseRegistrationLogic.Delete(a => a.Student_Course_Registration_Id == oldCourseRegistration.Id);

                        for (int i = 0; i < viewModel.RegisteredCourse.Details.Count; i++)
                        {
                            viewModel.RegisteredCourse.Details[i].CourseRegistration = oldCourseRegistration;
                        }

                        courseRegistrationLogic.Modify(viewModel.RegisteredCourse, courseRegistrationDetailAudit);

                        courseRegistration = oldCourseRegistration;
                    }
                    else
                    {
                        courseRegistration = courseRegistrationLogic.Create(viewModel.RegisteredCourse, courseRegistrationDetailAudit);
                    }


                    if (courseRegistration != null)
                    {
                        message = "Selected courses has been successfully registered.";
                    }
                    else
                    {
                        message = "Course Registration Failed! Please try again.";
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    message = "Error Occurred! " + ex.Message + ". Please try again.";
                }
                else
                {
                    if (ex.InnerException != null)
                    {
                        message = "Error Occurred! " + ex.Message + ". Please try again." + ex.InnerException.ToString();
                    }
                    else
                    {
                        message = "Error Occurred! " + ex.Message + ". Please try again.";
                    }
                }
            }

            return Json(new { message = message }, "text/html", JsonRequestBehavior.AllowGet);
        }



        private void PopulateCourseRegistrationForm(long sid, SessionSemester sessionSemester)
		{
			try
			{
				List<Course> firstSemesterCourses = null;
				List<Course> secondSemesterCourses = null;
                List<Course> thirdSemesterCourses = null;
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                bool HasPaidSchoolFees = false;
                bool HasPaidFirstInstallment = false;
                bool HasPaidSecondInstallment = false;
                bool HasPaidThirdInstallment = false;

                viewModel.Student = studentLogic.GetBy(sid);
				if(viewModel.Student != null && viewModel.Student.Id > 0)
				{
					CourseMode firstAttempt = new CourseMode() { Id = 1 };
					CourseMode carryOver = new CourseMode() { Id = 2 };
					Semester firstSemester = new Semester() { Id = 1 };
					Semester secondSemester = new Semester() { Id = 2 };
                    Semester thirdSemester = new Semester() { Id = 3 };

                    //close registration, to be removed later
                    //throw new Exception("Course Registration has been closed.");

                    //check number of sessions registered
                    if (Utility.HasMoreThanFourRegistrations(new Person { Id = viewModel.Student.Id }))
                    {
                        throw new Exception("You have registered for more than four sessions, hence can no longer register any other session.");
                    }

                    //check if has completed school fees payment
                    var studentLvl = studentLevelLogic.GetModelsBy(x => x.Person_Id == viewModel.Student.Id).LastOrDefault();
                    if (studentLvl != null && (studentLvl.Programme.Id == (int)Programmes.NDDistance || studentLvl.Programme.Id == (int)Programmes.HNDDistance))
                    {
                            HasPaidSchoolFees = Utility.HasCompletedSchoolFeesDistantLearning(viewModel.Student, sessionSemester.Session);
                            HasPaidFirstInstallment = Utility.HasCompletedSchoolFirstInstallment(viewModel.Student, sessionSemester.Session);
                            HasPaidSecondInstallment = Utility.HasCompletedSchoolSecondInstallment(viewModel.Student, sessionSemester.Session);
                            HasPaidThirdInstallment = Utility.HasCompletedSchoolThirdInstallment(viewModel.Student, sessionSemester.Session);                      

                    }
                    else if (!Utility.HasCompletedSchoolFees(viewModel.Student, sessionSemester.Session))
                    {
                        //this student repeated 100level and was asked to pay as returning student (29900) which is below the suppose amount (50400) for new students
                        if (viewModel.Student?.Id > 0 && sessionSemester.Session?.Id > 0 && (viewModel.Student.Id == 149264 || viewModel.Student.Id == 148467) && sessionSemester.Session.Id == 11)
                        {

                        }
                        else
                        {
                            throw new Exception("You have not completed your fees, kindly complete your fees before proceeding.");
                        }

                    }
                    
                    //CurrentSessionSemesterLogic currentSessionSemesterLogic = new CurrentSessionSemesterLogic();
                    //viewModel.CurrentSessionSemester = currentSessionSemesterLogic.GetCurrentSessionSemester();
                    viewModel.CurrentSessionSemester = new CurrentSessionSemester(){ SessionSemester = sessionSemester};

					//viewModel.StudentLevel = studentLevelLogic.GetBy(viewModel.Student,viewModel.CurrentSessionSemester.SessionSemester.Session);
					viewModel.StudentLevel = studentLevelLogic.GetModelsBy(s => s.Person_Id == viewModel.Student.Id && s.Session_Id == viewModel.CurrentSessionSemester.SessionSemester.Session.Id).LastOrDefault();

				    if (!CanRegister(viewModel.StudentLevel))
				    {
                        throw new Exception("Course registration has not been opened for this session");
				    }


                    //Distant Learning populate Course logic
                    if (viewModel.StudentLevel != null && (studentLvl.Programme.Id == (int)Programmes.NDDistance || studentLvl.Programme.Id == (int)Programmes.HNDDistance))
                    {
                        //If Full payment was made, initialize all courses(first, second and third semester) for registration
                        if (HasPaidSchoolFees || (HasPaidFirstInstallment && HasPaidSecondInstallment && HasPaidThirdInstallment))
                        {
                            if (viewModel.StudentLevel != null && viewModel.StudentLevel.Id > 0 && viewModel.StudentLevel.DepartmentOption == null)
                            {
                                firstSemesterCourses = courseLogic.GetBy(viewModel.StudentLevel.Programme, viewModel.StudentLevel.Department, viewModel.StudentLevel.Level, firstSemester, true);
                                secondSemesterCourses = courseLogic.GetBy(viewModel.StudentLevel.Programme, viewModel.StudentLevel.Department, viewModel.StudentLevel.Level, secondSemester, true);
                                thirdSemesterCourses = courseLogic.GetBy(viewModel.StudentLevel.Programme, viewModel.StudentLevel.Department, viewModel.StudentLevel.Level, thirdSemester, true);
                                SetMinimumAndMaximumCourseUnit(firstSemester, secondSemester, viewModel.StudentLevel.Department, viewModel.StudentLevel.Level, viewModel.StudentLevel.DepartmentOption, viewModel.StudentLevel.Programme);
                                //SetMinimumAndMaximumCourseUnit(firstSemester, secondSemester, viewModel.StudentLevel.Department, viewModel.StudentLevel.Level, viewModel.StudentLevel.DepartmentOption);
                            }
                            else if (viewModel.StudentLevel != null && viewModel.StudentLevel.Id > 0 && viewModel.StudentLevel.DepartmentOption != null && viewModel.StudentLevel.DepartmentOption.Id > 0)
                            {
                                firstSemesterCourses = courseLogic.GetBy(viewModel.StudentLevel.Programme, viewModel.StudentLevel.Department, viewModel.StudentLevel.DepartmentOption, viewModel.StudentLevel.Level, firstSemester, true);
                                secondSemesterCourses = courseLogic.GetBy(viewModel.StudentLevel.Programme, viewModel.StudentLevel.Department, viewModel.StudentLevel.DepartmentOption, viewModel.StudentLevel.Level, secondSemester, true);
                                thirdSemesterCourses = courseLogic.GetBy(viewModel.StudentLevel.Programme, viewModel.StudentLevel.Department, viewModel.StudentLevel.DepartmentOption, viewModel.StudentLevel.Level, thirdSemester, true);
                                SetMinimumAndMaximumCourseUnit(firstSemester, secondSemester, viewModel.StudentLevel.Department, viewModel.StudentLevel.Level, viewModel.StudentLevel.DepartmentOption, viewModel.StudentLevel.Programme);
                                //SetMinimumAndMaximumCourseUnit(firstSemester, secondSemester, viewModel.StudentLevel.Department, viewModel.StudentLevel.Level, viewModel.StudentLevel.DepartmentOption);
                            }
                        }
                        //If payments were made in installments, initialize courses(according to installment/semester) paid for registration
                        else
                        {
                            //First Installment
                            if (HasPaidFirstInstallment && !HasPaidSecondInstallment && !HasPaidThirdInstallment)
                            {
                                if (viewModel.StudentLevel != null && viewModel.StudentLevel.Id > 0 && viewModel.StudentLevel.DepartmentOption == null)
                                {
                                    firstSemesterCourses = courseLogic.GetBy(viewModel.StudentLevel.Programme, viewModel.StudentLevel.Department, viewModel.StudentLevel.Level, firstSemester, true);                                   
                                    SetMinimumAndMaximumCourseUnitAlt(firstSemester, null, null, viewModel.StudentLevel.Department, viewModel.StudentLevel.Level, viewModel.StudentLevel.DepartmentOption, viewModel.StudentLevel.Programme);                                  
                                }
                                else if (viewModel.StudentLevel != null && viewModel.StudentLevel.Id > 0 && viewModel.StudentLevel.DepartmentOption != null && viewModel.StudentLevel.DepartmentOption.Id > 0)
                                {
                                    firstSemesterCourses = courseLogic.GetBy(viewModel.StudentLevel.Programme, viewModel.StudentLevel.Department, viewModel.StudentLevel.DepartmentOption, viewModel.StudentLevel.Level, firstSemester, true);

                                    SetMinimumAndMaximumCourseUnitAlt(firstSemester, null, null, viewModel.StudentLevel.Department, viewModel.StudentLevel.Level, viewModel.StudentLevel.DepartmentOption, viewModel.StudentLevel.Programme);
                                   
                                }
                            }
                            //Second Installment
                            if (HasPaidFirstInstallment && HasPaidSecondInstallment && !HasPaidThirdInstallment)
                            {
                                if (viewModel.StudentLevel != null && viewModel.StudentLevel.Id > 0 && viewModel.StudentLevel.DepartmentOption == null)
                                {
                                    firstSemesterCourses = courseLogic.GetBy(viewModel.StudentLevel.Programme, viewModel.StudentLevel.Department, viewModel.StudentLevel.Level, firstSemester, true);
                                    secondSemesterCourses = courseLogic.GetBy(viewModel.StudentLevel.Programme, viewModel.StudentLevel.Department, viewModel.StudentLevel.Level, secondSemester, true);
                                    SetMinimumAndMaximumCourseUnitAlt(firstSemester, secondSemester, null, viewModel.StudentLevel.Department, viewModel.StudentLevel.Level, viewModel.StudentLevel.DepartmentOption, viewModel.StudentLevel.Programme);
                                }
                                else if (viewModel.StudentLevel != null && viewModel.StudentLevel.Id > 0 && viewModel.StudentLevel.DepartmentOption != null && viewModel.StudentLevel.DepartmentOption.Id > 0)
                                {
                                    firstSemesterCourses = courseLogic.GetBy(viewModel.StudentLevel.Programme, viewModel.StudentLevel.Department, viewModel.StudentLevel.DepartmentOption, viewModel.StudentLevel.Level, firstSemester, true);

                                    secondSemesterCourses = courseLogic.GetBy(viewModel.StudentLevel.Programme, viewModel.StudentLevel.Department, viewModel.StudentLevel.DepartmentOption, viewModel.StudentLevel.Level, secondSemester, true);

                                    SetMinimumAndMaximumCourseUnitAlt(firstSemester, secondSemester, null, viewModel.StudentLevel.Department, viewModel.StudentLevel.Level, viewModel.StudentLevel.DepartmentOption, viewModel.StudentLevel.Programme);

                                }
                            }
                            //Third Installment
                            if (HasPaidFirstInstallment && HasPaidSecondInstallment && HasPaidThirdInstallment)
                            {
                                if (viewModel.StudentLevel != null && viewModel.StudentLevel.Id > 0 && viewModel.StudentLevel.DepartmentOption == null)
                                {
                                    firstSemesterCourses = courseLogic.GetBy(viewModel.StudentLevel.Programme, viewModel.StudentLevel.Department, viewModel.StudentLevel.Level, firstSemester, true);
                                    secondSemesterCourses = courseLogic.GetBy(viewModel.StudentLevel.Programme, viewModel.StudentLevel.Department, viewModel.StudentLevel.Level, secondSemester, true);
                                    thirdSemesterCourses = courseLogic.GetBy(viewModel.StudentLevel.Programme, viewModel.StudentLevel.Department, viewModel.StudentLevel.Level, thirdSemester, true);

                                    SetMinimumAndMaximumCourseUnitAlt(firstSemester, secondSemester, thirdSemester, viewModel.StudentLevel.Department, viewModel.StudentLevel.Level, viewModel.StudentLevel.DepartmentOption, viewModel.StudentLevel.Programme);
                                }
                                else if (viewModel.StudentLevel != null && viewModel.StudentLevel.Id > 0 && viewModel.StudentLevel.DepartmentOption != null && viewModel.StudentLevel.DepartmentOption.Id > 0)
                                {
                                    firstSemesterCourses = courseLogic.GetBy(viewModel.StudentLevel.Programme, viewModel.StudentLevel.Department, viewModel.StudentLevel.DepartmentOption, viewModel.StudentLevel.Level, firstSemester, true);
                                    secondSemesterCourses = courseLogic.GetBy(viewModel.StudentLevel.Programme, viewModel.StudentLevel.Department, viewModel.StudentLevel.DepartmentOption, viewModel.StudentLevel.Level, secondSemester, true);

                                    thirdSemesterCourses = courseLogic.GetBy(viewModel.StudentLevel.Programme, viewModel.StudentLevel.Department, viewModel.StudentLevel.DepartmentOption, viewModel.StudentLevel.Level, thirdSemester, true);

                                    SetMinimumAndMaximumCourseUnitAlt(firstSemester, secondSemester, thirdSemester, viewModel.StudentLevel.Department, viewModel.StudentLevel.Level, viewModel.StudentLevel.DepartmentOption, viewModel.StudentLevel.Programme);

                                }
                            }
                        }

                    }
                    else
                    {
                        if (viewModel.StudentLevel != null && viewModel.StudentLevel.Id > 0 && viewModel.StudentLevel.DepartmentOption == null)
                        {
                            firstSemesterCourses = courseLogic.GetBy(viewModel.StudentLevel.Programme, viewModel.StudentLevel.Department, viewModel.StudentLevel.Level, firstSemester, true);
                            secondSemesterCourses = courseLogic.GetBy(viewModel.StudentLevel.Programme, viewModel.StudentLevel.Department, viewModel.StudentLevel.Level, secondSemester, true);
                            thirdSemesterCourses = courseLogic.GetBy(viewModel.StudentLevel.Programme, viewModel.StudentLevel.Department, viewModel.StudentLevel.Level, thirdSemester, true);
                            SetMinimumAndMaximumCourseUnit(firstSemester, secondSemester, viewModel.StudentLevel.Department, viewModel.StudentLevel.Level, viewModel.StudentLevel.DepartmentOption, viewModel.StudentLevel.Programme);
                            //SetMinimumAndMaximumCourseUnit(firstSemester, secondSemester, viewModel.StudentLevel.Department, viewModel.StudentLevel.Level, viewModel.StudentLevel.DepartmentOption);
                        }
                        else if (viewModel.StudentLevel != null && viewModel.StudentLevel.Id > 0 && viewModel.StudentLevel.DepartmentOption != null && viewModel.StudentLevel.DepartmentOption.Id > 0)
                        {
                            firstSemesterCourses = courseLogic.GetBy(viewModel.StudentLevel.Programme, viewModel.StudentLevel.Department, viewModel.StudentLevel.DepartmentOption, viewModel.StudentLevel.Level, firstSemester, true);
                            secondSemesterCourses = courseLogic.GetBy(viewModel.StudentLevel.Programme, viewModel.StudentLevel.Department, viewModel.StudentLevel.DepartmentOption, viewModel.StudentLevel.Level, secondSemester, true);
                            thirdSemesterCourses = courseLogic.GetBy(viewModel.StudentLevel.Programme, viewModel.StudentLevel.Department, viewModel.StudentLevel.DepartmentOption, viewModel.StudentLevel.Level, thirdSemester, true);
                            SetMinimumAndMaximumCourseUnit(firstSemester, secondSemester, viewModel.StudentLevel.Department, viewModel.StudentLevel.Level, viewModel.StudentLevel.DepartmentOption, viewModel.StudentLevel.Programme);
                            //SetMinimumAndMaximumCourseUnit(firstSemester, secondSemester, viewModel.StudentLevel.Department, viewModel.StudentLevel.Level, viewModel.StudentLevel.DepartmentOption);
                        }

                    }

                    //get courses if already registered
                    CourseRegistrationLogic courseRegistrationLogic = new CourseRegistrationLogic();
					CourseRegistrationDetailLogic courseRegistrationDetailLogic = new CourseRegistrationDetailLogic();
					CourseRegistration courseRegistration = courseRegistrationLogic.GetBy(viewModel.Student,viewModel.StudentLevel.Level,viewModel.StudentLevel.Programme,viewModel.StudentLevel.Department,viewModel.CurrentSessionSemester.SessionSemester.Session);
					if(courseRegistration != null && courseRegistration.Id > 0)
					{
						viewModel.RegisteredCourse = courseRegistration;
						if(courseRegistration.Details != null && courseRegistration.Details.Count > 0)
						{
							//split registered courses by semester
							List<CourseRegistrationDetail> firstSemesterRegisteredCourseDetails = courseRegistration.Details.Where(rc => rc.Semester.Id == firstSemester.Id).ToList();
							List<CourseRegistrationDetail> secondSemesterRegisteredCourseDetails = courseRegistration.Details.Where(rc => rc.Semester.Id == secondSemester.Id).ToList();
                            List<CourseRegistrationDetail> thirdSemesterRegisteredCourseDetails = courseRegistration.Details.Where(rc => rc.Semester.Id == thirdSemester.Id).ToList();
                            List<CourseRegistrationDetail> firstSemesterCarryOverRegisteredCourseDetails = courseRegistration.Details.Where(rc => rc.Semester.Id == firstSemester.Id && rc.Mode.Id == carryOver.Id).ToList();
							List<CourseRegistrationDetail> secondSemesterCarryOverRegisteredCourseDetails = courseRegistration.Details.Where(rc => rc.Semester.Id == secondSemester.Id && rc.Mode.Id == carryOver.Id).ToList();
                            List<CourseRegistrationDetail> thirdSemesterCarryOverRegisteredCourseDetails = courseRegistration.Details.Where(rc => rc.Semester.Id == thirdSemester.Id && rc.Mode.Id == carryOver.Id).ToList();

                            //get registered courses
                            viewModel.FirstSemesterCourses = GetRegisteredCourse(courseRegistration,firstSemesterCourses,firstSemester,firstSemesterRegisteredCourseDetails,firstAttempt);
							viewModel.SecondSemesterCourses = GetRegisteredCourse(courseRegistration,secondSemesterCourses,secondSemester,secondSemesterRegisteredCourseDetails,firstAttempt);
                            viewModel.ThirdSemesterCourses = GetRegisteredCourse(courseRegistration, thirdSemesterCourses, thirdSemester, thirdSemesterRegisteredCourseDetails, firstAttempt);

                            //get carry over courses
                            List<Course> firstSemesterCarryOverCourses = courseRegistrationDetailLogic.GetCarryOverCoursesBy(courseRegistration,firstSemester);
							List<Course> secondSemesterCarryOverCourses = courseRegistrationDetailLogic.GetCarryOverCoursesBy(courseRegistration,secondSemester);
                            List<Course> thirdSemesterCarryOverCourses = courseRegistrationDetailLogic.GetCarryOverCoursesBy(courseRegistration, thirdSemester);
                            viewModel.FirstSemesterCarryOverCourses = GetRegisteredCourse(courseRegistration,firstSemesterCarryOverCourses,firstSemester,firstSemesterCarryOverRegisteredCourseDetails,carryOver);
                            viewModel.SecondSemesterCarryOverCourses = GetRegisteredCourse(courseRegistration, secondSemesterCarryOverCourses, secondSemester, secondSemesterCarryOverRegisteredCourseDetails, carryOver);
                            viewModel.thirdSemesterCarryOverCourses = GetRegisteredCourse(courseRegistration, thirdSemesterCarryOverCourses, thirdSemester, thirdSemesterCarryOverRegisteredCourseDetails, carryOver);

                            if (viewModel.FirstSemesterCarryOverCourses != null && viewModel.FirstSemesterCarryOverCourses.Count > 0)
							{
								viewModel.CarryOverExist = true;
								viewModel.CarryOverCourses.AddRange(viewModel.FirstSemesterCarryOverCourses);
								viewModel.TotalFirstSemesterCarryOverCourseUnit = viewModel.FirstSemesterCarryOverCourses.Where(c => c.Semester.Id == firstSemester.Id).Sum(u => u.Course.Unit);
							}
							if(viewModel.SecondSemesterCarryOverCourses != null && viewModel.SecondSemesterCarryOverCourses.Count > 0)
							{
								viewModel.CarryOverExist = true;
								viewModel.CarryOverCourses.AddRange(viewModel.SecondSemesterCarryOverCourses);
								viewModel.TotalSecondSemesterCarryOverCourseUnit = viewModel.SecondSemesterCarryOverCourses.Where(c => c.Semester.Id == secondSemester.Id).Sum(u => u.Course.Unit);
							}

                            if (viewModel.thirdSemesterCarryOverCourses != null && viewModel.thirdSemesterCarryOverCourses.Count > 0)
                            {
                                viewModel.CarryOverExist = true;
                                viewModel.CarryOverCourses.AddRange(viewModel.thirdSemesterCarryOverCourses);
                                viewModel.TotalThirdSemesterCarryOverCourseUnit = viewModel.thirdSemesterCarryOverCourses.Where(c => c.Semester.Id == secondSemester.Id).Sum(u => u.Course.Unit);
                            }

                            //set total selected course units
                            if(viewModel.StudentLevel != null && (viewModel.StudentLevel.Programme.Id == (int)Programmes.HNDDistance || viewModel.StudentLevel.Programme.Id == (int)Programmes.NDDistance))
                            {
                                if (HasPaidSchoolFees)
                                {
                                    viewModel.SumOfFirstSemesterSelectedCourseUnit = SumSemesterSelectedCourseUnit(firstSemesterRegisteredCourseDetails);
                                    viewModel.SumOfSecondSemesterSelectedCourseUnit = SumSemesterSelectedCourseUnit(secondSemesterRegisteredCourseDetails);
                                    viewModel.SumOfThirdSemesterSelectedCourseUnit = SumSemesterSelectedCourseUnit(thirdSemesterRegisteredCourseDetails);
                                    viewModel.CourseAlreadyRegistered = true;
                                }
                                else if (HasPaidFirstInstallment && !HasPaidSecondInstallment && !HasPaidThirdInstallment)
                                {
                                    viewModel.SumOfFirstSemesterSelectedCourseUnit = SumSemesterSelectedCourseUnit(firstSemesterRegisteredCourseDetails);
                                    viewModel.CourseAlreadyRegistered = true;


                                }
                                else if (HasPaidFirstInstallment && HasPaidSecondInstallment && !HasPaidThirdInstallment)
                                {
                                    viewModel.SumOfSecondSemesterSelectedCourseUnit = SumSemesterSelectedCourseUnit(secondSemesterRegisteredCourseDetails);
                                    viewModel.CourseAlreadyRegistered = true;


                                }
                                else if (HasPaidFirstInstallment && HasPaidSecondInstallment && HasPaidThirdInstallment)
                                {
                                    viewModel.SumOfThirdSemesterSelectedCourseUnit = SumSemesterSelectedCourseUnit(thirdSemesterRegisteredCourseDetails);
                                    viewModel.CourseAlreadyRegistered = true;


                                }
                            }
                            else
                            {
                                viewModel.SumOfFirstSemesterSelectedCourseUnit = SumSemesterSelectedCourseUnit(firstSemesterRegisteredCourseDetails);
                                viewModel.SumOfSecondSemesterSelectedCourseUnit = SumSemesterSelectedCourseUnit(secondSemesterRegisteredCourseDetails);
                                viewModel.SumOfThirdSemesterSelectedCourseUnit = SumSemesterSelectedCourseUnit(thirdSemesterRegisteredCourseDetails);
                                viewModel.CourseAlreadyRegistered = true;
                            }
						}
						else
						{
							viewModel.FirstSemesterCourses = GetUnregisteredCourseDetail(firstSemesterCourses,firstSemester);
							viewModel.SecondSemesterCourses = GetUnregisteredCourseDetail(secondSemesterCourses,secondSemester);
                            viewModel.ThirdSemesterCourses = GetUnregisteredCourseDetail(thirdSemesterCourses, thirdSemester);
                            viewModel.CourseAlreadyRegistered = false;
							//get carry over courses
							viewModel.CarryOverCourses = courseRegistrationDetailLogic.GetCarryOverBy(viewModel.Student,viewModel.CurrentSessionSemester.SessionSemester.Session);
							if(viewModel.CarryOverCourses != null && viewModel.CarryOverCourses.Count > 0)
							{
								viewModel.CarryOverExist = true;
								viewModel.TotalFirstSemesterCarryOverCourseUnit = viewModel.CarryOverCourses.Where(c => c.Semester.Id == firstSemester.Id).Sum(u => u.Course.Unit);
								viewModel.TotalSecondSemesterCarryOverCourseUnit = viewModel.CarryOverCourses.Where(c => c.Semester.Id == secondSemester.Id).Sum(u => u.Course.Unit);
                                viewModel.TotalThirdSemesterCarryOverCourseUnit = viewModel.CarryOverCourses.Where(c => c.Semester.Id == thirdSemester.Id).Sum(u => u.Course.Unit);

                                if (viewModel.TotalFirstSemesterCarryOverCourseUnit <= viewModel.FirstSemesterMaximumUnit && viewModel.TotalSecondSemesterCarryOverCourseUnit <= viewModel.SecondSemesterMaximumUnit && viewModel.TotalThirdSemesterCarryOverCourseUnit <= viewModel.ThirdSemesterMaximumUnit)
								{
									foreach(CourseRegistrationDetail carryOverCourse in viewModel.CarryOverCourses)
									{
										carryOverCourse.Course.IsRegistered = true;
									}
								}
							}
						}
					}
					else
					{
						viewModel.FirstSemesterCourses = GetUnregisteredCourseDetail(firstSemesterCourses,firstSemester);
						viewModel.SecondSemesterCourses = GetUnregisteredCourseDetail(secondSemesterCourses,secondSemester);
                        viewModel.ThirdSemesterCourses = GetUnregisteredCourseDetail(thirdSemesterCourses, thirdSemester);
                        viewModel.CourseAlreadyRegistered = false;
						//get carry over courses
						viewModel.CarryOverCourses = courseRegistrationDetailLogic.GetCarryOverBy(viewModel.Student,viewModel.CurrentSessionSemester.SessionSemester.Session);
						if(viewModel.CarryOverCourses != null && viewModel.CarryOverCourses.Count > 0)
						{
							viewModel.CarryOverExist = true;
							viewModel.TotalFirstSemesterCarryOverCourseUnit = viewModel.CarryOverCourses.Where(c => c.Semester.Id == firstSemester.Id).Sum(u => u.Course.Unit);
							viewModel.TotalSecondSemesterCarryOverCourseUnit = viewModel.CarryOverCourses.Where(c => c.Semester.Id == secondSemester.Id).Sum(u => u.Course.Unit);
                            viewModel.TotalThirdSemesterCarryOverCourseUnit = viewModel.CarryOverCourses.Where(c => c.Semester.Id == thirdSemester.Id).Sum(u => u.Course.Unit);

                            if (viewModel.TotalFirstSemesterCarryOverCourseUnit <= viewModel.FirstSemesterMaximumUnit && viewModel.TotalSecondSemesterCarryOverCourseUnit <= viewModel.SecondSemesterMaximumUnit)
							{
								foreach(CourseRegistrationDetail carryOverCourse in viewModel.CarryOverCourses)
								{
									carryOverCourse.Course.IsRegistered = true;
								}
							}
						}
					}


					//}
				}
			}
			catch(Exception)
			{
				throw;
			}
		}
        private void PopulateCourseRegistrationFormForReprint(long sid, SessionSemester sessionSemester)
        {
            try
            {
                List<Course> firstSemesterCourses = null;
                List<Course> secondSemesterCourses = null;
                List<Course> thirdSemesterCourses = null;

                viewModel.Student = studentLogic.GetBy(sid);
                if (viewModel.Student != null && viewModel.Student.Id > 0)
                {
                    CourseMode firstAttempt = new CourseMode() { Id = 1 };
                    CourseMode carryOver = new CourseMode() { Id = 2 };
                    Semester firstSemester = new Semester() { Id = 1 };
                    Semester secondSemester = new Semester() { Id = 2 };
                    Semester thirdSemester = new Semester() { Id = 3 };
                    
                    //CurrentSessionSemesterLogic currentSessionSemesterLogic = new CurrentSessionSemesterLogic();
                    //viewModel.CurrentSessionSemester = currentSessionSemesterLogic.GetCurrentSessionSemester();
                    viewModel.CurrentSessionSemester = new CurrentSessionSemester() { SessionSemester = sessionSemester };

                    StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                    //viewModel.StudentLevel = studentLevelLogic.GetBy(viewModel.Student,viewModel.CurrentSessionSemester.SessionSemester.Session);
                    viewModel.StudentLevel = studentLevelLogic.GetModelsBy(s => s.Person_Id == viewModel.Student.Id && s.Session_Id == viewModel.CurrentSessionSemester.SessionSemester.Session.Id).LastOrDefault();
                    
                    if (viewModel.StudentLevel != null && viewModel.StudentLevel.Id > 0 && viewModel.StudentLevel.DepartmentOption == null)
                    {
                        firstSemesterCourses = courseLogic.GetBy(viewModel.StudentLevel.Programme, viewModel.StudentLevel.Department, viewModel.StudentLevel.Level, firstSemester, true);
                        secondSemesterCourses = courseLogic.GetBy(viewModel.StudentLevel.Programme, viewModel.StudentLevel.Department, viewModel.StudentLevel.Level, secondSemester, true);
                        thirdSemesterCourses = courseLogic.GetBy(viewModel.StudentLevel.Programme, viewModel.StudentLevel.Department, viewModel.StudentLevel.Level, thirdSemester, true);
                        SetMinimumAndMaximumCourseUnit(firstSemester, secondSemester, viewModel.StudentLevel.Department, viewModel.StudentLevel.Level, viewModel.StudentLevel.DepartmentOption, viewModel.StudentLevel.Programme);
                        //SetMinimumAndMaximumCourseUnit(firstSemester, secondSemester, viewModel.StudentLevel.Department, viewModel.StudentLevel.Level, viewModel.StudentLevel.DepartmentOption);
                    }
                    else if (viewModel.StudentLevel != null && viewModel.StudentLevel.Id > 0 && viewModel.StudentLevel.DepartmentOption != null && viewModel.StudentLevel.DepartmentOption.Id > 0)
                    {
                        firstSemesterCourses = courseLogic.GetBy(viewModel.StudentLevel.Programme, viewModel.StudentLevel.Department, viewModel.StudentLevel.DepartmentOption, viewModel.StudentLevel.Level, firstSemester, true);
                        secondSemesterCourses = courseLogic.GetBy(viewModel.StudentLevel.Programme, viewModel.StudentLevel.Department, viewModel.StudentLevel.DepartmentOption, viewModel.StudentLevel.Level, secondSemester, true);
                        thirdSemesterCourses = courseLogic.GetBy(viewModel.StudentLevel.Programme, viewModel.StudentLevel.Department, viewModel.StudentLevel.DepartmentOption, viewModel.StudentLevel.Level, thirdSemester, true);
                        SetMinimumAndMaximumCourseUnit(firstSemester, secondSemester, viewModel.StudentLevel.Department, viewModel.StudentLevel.Level, viewModel.StudentLevel.DepartmentOption, viewModel.StudentLevel.Programme);
                        //SetMinimumAndMaximumCourseUnit(firstSemester, secondSemester, viewModel.StudentLevel.Department, viewModel.StudentLevel.Level, viewModel.StudentLevel.DepartmentOption);
                    }

                    //get courses if already registered
                    CourseRegistrationLogic courseRegistrationLogic = new CourseRegistrationLogic();
                    CourseRegistrationDetailLogic courseRegistrationDetailLogic = new CourseRegistrationDetailLogic();
                    CourseRegistration courseRegistration = courseRegistrationLogic.GetBy(viewModel.Student, viewModel.StudentLevel.Level, viewModel.StudentLevel.Programme, viewModel.StudentLevel.Department, viewModel.CurrentSessionSemester.SessionSemester.Session);
                    if (courseRegistration != null && courseRegistration.Id > 0)
                    {
                        viewModel.RegisteredCourse = courseRegistration;
                        if (courseRegistration.Details != null && courseRegistration.Details.Count > 0)
                        {
                            //split registered courses by semester
                            List<CourseRegistrationDetail> firstSemesterRegisteredCourseDetails = courseRegistration.Details.Where(rc => rc.Semester.Id == firstSemester.Id && rc.Mode.Id == firstAttempt.Id).ToList();
                            List<CourseRegistrationDetail> secondSemesterRegisteredCourseDetails = courseRegistration.Details.Where(rc => rc.Semester.Id == secondSemester.Id && rc.Mode.Id == firstAttempt.Id).ToList();
                            List<CourseRegistrationDetail> thirdSemesterRegisteredCourseDetails = courseRegistration.Details.Where(rc => rc.Semester.Id == thirdSemester.Id && rc.Mode.Id == firstAttempt.Id).ToList();

                            List<CourseRegistrationDetail> firstSemesterCarryOverRegisteredCourseDetails = courseRegistration.Details.Where(rc => rc.Semester.Id == firstSemester.Id && rc.Mode.Id == carryOver.Id).ToList();
                            List<CourseRegistrationDetail> secondSemesterCarryOverRegisteredCourseDetails = courseRegistration.Details.Where(rc => rc.Semester.Id == secondSemester.Id && rc.Mode.Id == carryOver.Id).ToList();
                            List<CourseRegistrationDetail> thirdSemesterCarryOverRegisteredCourseDetails = courseRegistration.Details.Where(rc => rc.Semester.Id == thirdSemester.Id && rc.Mode.Id == carryOver.Id).ToList();

                            //get registered courses
                            viewModel.FirstSemesterCourses = firstSemesterRegisteredCourseDetails;
                            viewModel.SecondSemesterCourses = secondSemesterRegisteredCourseDetails;
                            viewModel.ThirdSemesterCourses = thirdSemesterRegisteredCourseDetails;

                            //get carry over courses
                            viewModel.FirstSemesterCarryOverCourses = firstSemesterCarryOverRegisteredCourseDetails;
                            viewModel.SecondSemesterCarryOverCourses = secondSemesterCarryOverRegisteredCourseDetails;
                            viewModel.thirdSemesterCarryOverCourses = thirdSemesterCarryOverRegisteredCourseDetails;

                            if (viewModel.FirstSemesterCarryOverCourses != null && viewModel.FirstSemesterCarryOverCourses.Count > 0)
                            {
                                viewModel.CarryOverExist = true;
                                viewModel.CarryOverCourses.AddRange(viewModel.FirstSemesterCarryOverCourses);
                                viewModel.TotalFirstSemesterCarryOverCourseUnit = viewModel.FirstSemesterCarryOverCourses.Where(c => c.Semester.Id == firstSemester.Id).Sum(u => u.Course.Unit);
                            }
                            if (viewModel.SecondSemesterCarryOverCourses != null && viewModel.SecondSemesterCarryOverCourses.Count > 0)
                            {
                                viewModel.CarryOverExist = true;
                                viewModel.CarryOverCourses.AddRange(viewModel.SecondSemesterCarryOverCourses);
                                viewModel.TotalSecondSemesterCarryOverCourseUnit = viewModel.SecondSemesterCarryOverCourses.Where(c => c.Semester.Id == secondSemester.Id).Sum(u => u.Course.Unit);
                            }

                            if (viewModel.thirdSemesterCarryOverCourses != null && viewModel.thirdSemesterCarryOverCourses.Count > 0)
                            {
                                viewModel.CarryOverExist = true;
                                viewModel.CarryOverCourses.AddRange(viewModel.thirdSemesterCarryOverCourses);
                                viewModel.TotalThirdSemesterCarryOverCourseUnit = viewModel.thirdSemesterCarryOverCourses.Where(c => c.Semester.Id == secondSemester.Id).Sum(u => u.Course.Unit);
                            }

                            //set total selected course units
                            viewModel.SumOfFirstSemesterSelectedCourseUnit = SumSemesterSelectedCourseUnit(firstSemesterRegisteredCourseDetails);
                            viewModel.SumOfSecondSemesterSelectedCourseUnit = SumSemesterSelectedCourseUnit(secondSemesterRegisteredCourseDetails);
                            viewModel.SumOfThirdSemesterSelectedCourseUnit = SumSemesterSelectedCourseUnit(thirdSemesterRegisteredCourseDetails);
                            viewModel.CourseAlreadyRegistered = true;
                            //mark registered carryover course as registered
                            if (viewModel.CarryOverCourses?.Count > 0)
                            {
                                foreach (CourseRegistrationDetail carryOverCourse in viewModel.CarryOverCourses)
                                {
                                    carryOverCourse.Course.IsRegistered = true;
                                }
                            }
                        }
                        else
                        {
                            viewModel.FirstSemesterCourses = GetUnregisteredCourseDetail(firstSemesterCourses, firstSemester);
                            viewModel.SecondSemesterCourses = GetUnregisteredCourseDetail(secondSemesterCourses, secondSemester);
                            viewModel.ThirdSemesterCourses = GetUnregisteredCourseDetail(thirdSemesterCourses, thirdSemester);
                            viewModel.CourseAlreadyRegistered = false;
                            //get carry over courses
                            viewModel.CarryOverCourses = courseRegistrationDetailLogic.GetCarryOverBy(viewModel.Student, viewModel.CurrentSessionSemester.SessionSemester.Session);
                            if (viewModel.CarryOverCourses != null && viewModel.CarryOverCourses.Count > 0)
                            {
                                viewModel.CarryOverExist = true;
                                viewModel.TotalFirstSemesterCarryOverCourseUnit = viewModel.CarryOverCourses.Where(c => c.Semester.Id == firstSemester.Id).Sum(u => u.Course.Unit);
                                viewModel.TotalSecondSemesterCarryOverCourseUnit = viewModel.CarryOverCourses.Where(c => c.Semester.Id == secondSemester.Id).Sum(u => u.Course.Unit);
                                viewModel.TotalThirdSemesterCarryOverCourseUnit = viewModel.CarryOverCourses.Where(c => c.Semester.Id == thirdSemester.Id).Sum(u => u.Course.Unit);

                                if (viewModel.TotalFirstSemesterCarryOverCourseUnit <= viewModel.FirstSemesterMaximumUnit && viewModel.TotalSecondSemesterCarryOverCourseUnit <= viewModel.SecondSemesterMaximumUnit && viewModel.TotalThirdSemesterCarryOverCourseUnit <= viewModel.ThirdSemesterMaximumUnit)
                                {
                                    foreach (CourseRegistrationDetail carryOverCourse in viewModel.CarryOverCourses)
                                    {
                                        carryOverCourse.Course.IsRegistered = true;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        viewModel.FirstSemesterCourses = GetUnregisteredCourseDetail(firstSemesterCourses, firstSemester);
                        viewModel.SecondSemesterCourses = GetUnregisteredCourseDetail(secondSemesterCourses, secondSemester);
                        viewModel.ThirdSemesterCourses = GetUnregisteredCourseDetail(thirdSemesterCourses, thirdSemester);
                        viewModel.CourseAlreadyRegistered = false;
                        //get carry over courses
                        viewModel.CarryOverCourses = courseRegistrationDetailLogic.GetCarryOverBy(viewModel.Student, viewModel.CurrentSessionSemester.SessionSemester.Session);
                        if (viewModel.CarryOverCourses != null && viewModel.CarryOverCourses.Count > 0)
                        {
                            viewModel.CarryOverExist = true;
                            viewModel.TotalFirstSemesterCarryOverCourseUnit = viewModel.CarryOverCourses.Where(c => c.Semester.Id == firstSemester.Id).Sum(u => u.Course.Unit);
                            viewModel.TotalSecondSemesterCarryOverCourseUnit = viewModel.CarryOverCourses.Where(c => c.Semester.Id == secondSemester.Id).Sum(u => u.Course.Unit);
                            viewModel.TotalThirdSemesterCarryOverCourseUnit = viewModel.CarryOverCourses.Where(c => c.Semester.Id == thirdSemester.Id).Sum(u => u.Course.Unit);

                            if (viewModel.TotalFirstSemesterCarryOverCourseUnit <= viewModel.FirstSemesterMaximumUnit && viewModel.TotalSecondSemesterCarryOverCourseUnit <= viewModel.SecondSemesterMaximumUnit)
                            {
                                foreach (CourseRegistrationDetail carryOverCourse in viewModel.CarryOverCourses)
                                {
                                    carryOverCourse.Course.IsRegistered = true;
                                }
                            }
                        }
                    }


                    //}
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private bool CanRegister(StudentLevel studentLevel)
        {
            try
            {
                if (studentLevel == null)
                    return false;

                _registrationStatusLogic = new CourseRegistrationStatusLogic();
                CourseRegistrationStatus registrationStatus = _registrationStatusLogic.GetModelsBy(c => c.Department_Id == studentLevel.Department.Id && c.Programme_Id == studentLevel.Programme.Id &&
                                                                c.Session_Id == studentLevel.Session.Id && c.Active).LastOrDefault();
                return registrationStatus != null;
            }
            catch (Exception)
            {
                throw;
            }
        }

		
		private List<CourseRegistrationDetail> GetSelectedCourses(List<CourseRegistrationDetail> coursesToRegister)
		{
			try
			{
				List<CourseRegistrationDetail> selectedCourseDetails = null;

				if (coursesToRegister != null && coursesToRegister.Count > 0)
				{
					selectedCourseDetails = coursesToRegister.Where(c => c.Course.IsRegistered == true).ToList();
				}

				return selectedCourseDetails;
			}
			catch (Exception)
			{
				throw;
			}
		}

		private int SumSemesterSelectedCourseUnit(List<CourseRegistrationDetail> semesterRegisteredCourseDetails)
		{
			try
			{
				int totalRegisteredCourseUnit = 0;
				if (semesterRegisteredCourseDetails != null && semesterRegisteredCourseDetails.Count > 0)
				{
					totalRegisteredCourseUnit = semesterRegisteredCourseDetails.Sum(c => c.CourseUnit) ?? 0;
				}

				return totalRegisteredCourseUnit;
			}
			catch (Exception)
			{
				throw;
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
                else
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

		private List<CourseRegistrationDetail> GetUnregisteredCourseDetail(List<Course> courses, Semester semester)
		{
			try
			{
				List<CourseRegistrationDetail> courseRegistrationDetails = null;
				if (courses != null && courses.Count > 0)
				{
					courseRegistrationDetails = new List<CourseRegistrationDetail>();
					foreach (Course course in courses)
					{
						CourseRegistrationDetail courseRegistrationDetail = new CourseRegistrationDetail();
						courseRegistrationDetail.Course = course;
						courseRegistrationDetail.Semester = semester;
						courseRegistrationDetail.Course.IsRegistered = false;
						courseRegistrationDetail.Mode = new CourseMode() { Id = 1 };

						courseRegistrationDetails.Add(courseRegistrationDetail);
					}
				}

				return courseRegistrationDetails;
			}
			catch (Exception)
			{
				throw;
			}
		}

		//private void SetMinimumAndMaximumCourseUnit(Semester firstSemester, Semester secondSemester, Department departmemt, Level level, DepartmentOption departmentOption)
		//{
		//	try
		//	{
		//		CourseUnitLogic courseUnitLogic = new CourseUnitLogic();
		//		CourseUnit firstSemesterCourseUnit = courseUnitLogic.GetBy(departmemt, level, firstSemester, departmentOption);
		//		if (firstSemesterCourseUnit != null && firstSemesterCourseUnit.Id > 0)
		//		{
		//			viewModel.FirstSemesterMinimumUnit = firstSemesterCourseUnit.MinimumUnit;
		//			viewModel.FirstSemesterMaximumUnit = firstSemesterCourseUnit.MaximumUnit;
		//		}

		//		CourseUnit secondSemesterCourseUnit = courseUnitLogic.GetBy(departmemt, level, secondSemester, departmentOption);
		//		if (secondSemesterCourseUnit != null && secondSemesterCourseUnit.Id > 0)
		//		{
		//			viewModel.SecondSemesterMinimumUnit = secondSemesterCourseUnit.MinimumUnit;
		//			viewModel.SecondSemesterMaximumUnit = secondSemesterCourseUnit.MaximumUnit;
		//		}

  //              CourseUnit thirdSemesterCourseUnit = courseUnitLogic.GetBy(departmemt, level, new Semester() { Id = 3 }, departmentOption);
  //              if (thirdSemesterCourseUnit != null && thirdSemesterCourseUnit.Id > 0)
  //              {
  //                  viewModel.ThirdSemesterMinimumUnit = thirdSemesterCourseUnit.MinimumUnit;
  //                  viewModel.ThirdSemesterMaximumUnit = thirdSemesterCourseUnit.MaximumUnit;
  //              }
  //          }
		//	catch (Exception ex)
		//	{
		//		SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
		//	}
		//}
        private void SetMinimumAndMaximumCourseUnit(Semester firstSemester, Semester secondSemester, Department departmemt, Level level, DepartmentOption departmentOption, Programme programme)
        {
            try
            {
                CourseUnitLogic courseUnitLogic = new CourseUnitLogic();
                CourseUnit firstSemesterCourseUnit = courseUnitLogic.GetBy(departmemt, level, firstSemester, departmentOption, programme);
                if (firstSemesterCourseUnit != null && firstSemesterCourseUnit.Id > 0)
                {
                    viewModel.FirstSemesterMinimumUnit = firstSemesterCourseUnit.MinimumUnit;
                    viewModel.FirstSemesterMaximumUnit = firstSemesterCourseUnit.MaximumUnit;
                }

                CourseUnit secondSemesterCourseUnit = courseUnitLogic.GetBy(departmemt, level, secondSemester, departmentOption, programme);
                if (secondSemesterCourseUnit != null && secondSemesterCourseUnit.Id > 0)
                {
                    viewModel.SecondSemesterMinimumUnit = secondSemesterCourseUnit.MinimumUnit;
                    viewModel.SecondSemesterMaximumUnit = secondSemesterCourseUnit.MaximumUnit;
                }

                CourseUnit thirdSemesterCourseUnit = courseUnitLogic.GetBy(departmemt, level, new Semester() { Id = 3 }, departmentOption, programme);
                if (thirdSemesterCourseUnit != null && thirdSemesterCourseUnit.Id > 0)
                {
                    viewModel.ThirdSemesterMinimumUnit = thirdSemesterCourseUnit.MinimumUnit;
                    viewModel.ThirdSemesterMaximumUnit = thirdSemesterCourseUnit.MaximumUnit;
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
        }




        private void SetMinimumAndMaximumCourseUnitAlt(Semester firstSemester, Semester secondSemester, Semester thirdSemester, Department departmemt, Level level, DepartmentOption departmentOption, Programme programme)
        {
            try
            {
                CourseUnitLogic courseUnitLogic = new CourseUnitLogic();
                CourseUnit firstSemesterCourseUnit = new CourseUnit();
                CourseUnit secondSemesterCourseUnit = new CourseUnit();
                CourseUnit thirdSemesterCourseUnit = new CourseUnit();

                if (firstSemester != null && firstSemester.Id > 0)
                {
                    firstSemesterCourseUnit = courseUnitLogic.GetBy(departmemt, level, firstSemester, departmentOption, programme);
                    if (firstSemesterCourseUnit != null && firstSemesterCourseUnit.Id > 0)
                    {
                        viewModel.FirstSemesterMinimumUnit = firstSemesterCourseUnit.MinimumUnit;
                        viewModel.FirstSemesterMaximumUnit = firstSemesterCourseUnit.MaximumUnit;
                    }
                }



                if (secondSemester != null && secondSemester.Id > 0)
                {
                    secondSemesterCourseUnit = courseUnitLogic.GetBy(departmemt, level, secondSemester, departmentOption, programme);
                    if (secondSemesterCourseUnit != null && secondSemesterCourseUnit.Id > 0)
                    {
                        viewModel.SecondSemesterMinimumUnit = secondSemesterCourseUnit.MinimumUnit;
                        viewModel.SecondSemesterMaximumUnit = secondSemesterCourseUnit.MaximumUnit;
                    }
                }


                if (thirdSemester != null && thirdSemester.Id > 0)
                {
                    thirdSemesterCourseUnit = courseUnitLogic.GetBy(departmemt, level, thirdSemester, departmentOption, programme);
                    if (thirdSemesterCourseUnit != null && thirdSemesterCourseUnit.Id > 0)
                    {
                        viewModel.ThirdSemesterMinimumUnit = thirdSemesterCourseUnit.MinimumUnit;
                        viewModel.ThirdSemesterMaximumUnit = thirdSemesterCourseUnit.MaximumUnit;
                    }
                }

                    
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
        }



        public ActionResult CourseFormPrintOut(string sesid, long sid)
		{
			try
			{
				SessionSemesterLogic sessionSemesterLogic = new SessionSemesterLogic();
				StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                HostelAllocationLogic hostelAllocationLogic = new HostelAllocationLogic();
				SessionSemester sessionSemester = null;
                viewModel.barcodeImageUrl = "http://applications.federalpolyilaro.edu.ng/Student/CourseRegistration/CourseFormPrintOut?sesid=" + sesid+ "&sid=" + sid;
                if (sesid != null)
				{
					int sessionId = Convert.ToInt32(sesid);
					sessionSemester = sessionSemesterLogic.GetModelsBy(s => s.Session_Id == sessionId).LastOrDefault();
				}
				else
				{
					StudentLevel studentLevel = studentLevelLogic.GetBy(sid);
					sessionSemester = sessionSemesterLogic.GetModelsBy(s => s.Session_Id == studentLevel.Session.Id).LastOrDefault();
                    viewModel.HostelAllocation= hostelAllocationLogic.GetModelsBy(s => s.Session_Id == studentLevel.Session.Id && s.Session_Id == sid).LastOrDefault();
				}

				PopulateCourseRegistrationForm(sid, sessionSemester);
				
			}
			catch (Exception ex)
			{
				SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
			}

			return View(viewModel);
		}
        
        //[HttpPost]
        //public ActionResult RegisterCourse(List<int> firstSemesterCourseIds, List<int> secondSemesterCourseIds, long studentId, int programmeId, int departmentId, int sessionId, int levelId)
        //{
        //    try
        //    {
        //        CourseRegistration courseRegistration = new CourseRegistration();
        //        List<CourseRegistrationDetail> courseRegistrationDetails = new List<CourseRegistrationDetail>();

        //        if (firstSemesterCourseIds != null && firstSemesterCourseIds.Count > 0 && secondSemesterCourseIds != null && secondSemesterCourseIds.Count > 0)
        //        {
        //            courseRegistration.Student = new Model.Model.Student() { Id = studentId };
        //            courseRegistration.Level = new Level() { Id = levelId };
        //            courseRegistration.Programme = new Programme() { Id = programmeId };
        //            courseRegistration.Department = new Department() { Id = departmentId };
        //            courseRegistration.Session = new Session() { Id = sessionId };

        //            foreach (int id in firstSemesterCourseIds)
        //            {
        //                CourseRegistrationDetail courseRegistrationDetail = new CourseRegistrationDetail();
        //                courseRegistrationDetail.Course = new Course() { Id = id };
        //                courseRegistrationDetail.Mode = new CourseMode() { Id = 1 };
        //                courseRegistrationDetail.Semester = new Semester() { Id = 1 };
        //                courseRegistrationDetails.Add(courseRegistrationDetail);
        //            }

        //            foreach (int id in secondSemesterCourseIds)
        //            {
        //                CourseRegistrationDetail courseRegistrationDetail = new CourseRegistrationDetail();
        //                courseRegistrationDetail.Course = new Course() { Id = id };
        //                courseRegistrationDetail.Mode = new CourseMode() { Id = 1 };
        //                courseRegistrationDetail.Semester = new Semester() { Id = 2 };
        //                courseRegistrationDetails.Add(courseRegistrationDetail);
        //            }
        //        }

        //        courseRegistration.Details = courseRegistrationDetails;
        //        CourseRegistrationLogic courseRegistrationLogic = new CourseRegistrationLogic();
        //        CourseRegistration CourseRegistration = courseRegistrationLogic.Create(courseRegistration);
        //        if (CourseRegistration != null)
        //        {
        //            SetMessage("Selected Course has been successfully registered", Message.Category.Error);
        //        }
        //        else
        //        {
        //            SetMessage("Selected Course Registration failed!", Message.Category.Error);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
        //    }

        //    return View(new CourseRegistrationViewModel());
        //    //return PartialView("_ApplicationFormsGrid", null);
        //}
        
        public ActionResult CourseRegistrations()
        {
            CourseRegistrationViewModel viewModel = new CourseRegistrationViewModel();
            try
            {
                if (System.Web.HttpContext.Current.Session["student"] != null)
                {
                    StudentLogic studentLogic = new StudentLogic();

                    Model.Model.Student student = System.Web.HttpContext.Current.Session["student"] as Model.Model.Student;
                    
                    if (student != null && student.Id > 0)
                    {
                        CourseRegistrationLogic courseRegistrationLogic = new CourseRegistrationLogic();
                        viewModel.CourseRegistrations = courseRegistrationLogic.GetModelsBy(c => c.Person_Id == student.Id);

                        if (viewModel.CourseRegistrations == null || viewModel.CourseRegistrations.Count <= 0)
                        {
                            SetMessage("No course registration record found.", Message.Category.Information);
                        }
                    }
                }
                else
                {
                    FormsAuthentication.SignOut();
                    System.Web.HttpContext.Current.Response.Redirect("/Security/Account/Login");

                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }

        public ActionResult ReprintCourseForm(string cId)
        {
            try
            {
                long courseRegId = Convert.ToInt64(Utility.Decrypt(cId));

                SessionSemesterLogic sessionSemesterLogic = new SessionSemesterLogic();
                CourseRegistrationLogic courseRegistrationLogic = new CourseRegistrationLogic();
                HostelAllocationLogic hostelAllocationLogic = new HostelAllocationLogic();

                CourseRegistration courseRegistration = courseRegistrationLogic.GetModelBy(s => s.Student_Course_Registration_Id == courseRegId);
                viewModel.barcodeImageUrl = "http://applications.federalpolyilaro.edu.ng/Student/CourseRegistration/ReprintCourseForm?cId=" + cId;
                SessionSemester sessionSemester = sessionSemesterLogic.GetModelsBy(s => s.Session_Id == courseRegistration.Session.Id).LastOrDefault();
                viewModel.HostelAllocation = hostelAllocationLogic.GetModelsBy(s => s.Session_Id == courseRegistration.Session.Id && s.Student_Id == courseRegistration.Student.Id).LastOrDefault();
                PopulateCourseRegistrationFormForReprint(courseRegistration.Student.Id, sessionSemester);
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            return View("CourseFormPrintOut", viewModel);
        }

        [HttpPost]
        public ActionResult UploadMedicalReport(CourseRegistrationViewModel model)
        {
            var studentId = Convert.ToString(TempData["StudentId"]);
            HttpPostedFileBase file = model.MyMedicalReport;
            var personId = model.Student.Id;
            bool isUploaded;
            string imageUrl = null;
            string imageUrlDisplay = null;
            string path = null;

            StudentLogic studentLogic = new StudentLogic();
            if (file != null && file.ContentLength != 0)
            {
                FileInfo fileInfo = new FileInfo(file.FileName);
                string fileExtension = fileInfo.Extension;
                string newFile = personId + "_medical_report_";
                string newFileName = newFile + DateTime.Now.ToString().Replace("/", "").Replace(":", "").Replace(" ", "") + fileExtension;

               
                    decimal sizeAllowed = 500;
                    string invalidFileMessage = InvalidFilePdf(file.ContentLength, fileExtension, sizeAllowed);
                if (!string.IsNullOrEmpty(invalidFileMessage))
                {
                    return RedirectToAction("SelectCourseRegistrationSession", "Registration");

                }

                string pathForSaving = Server.MapPath("~/Content/Student/Credential");
                    if (this.CreateFolderIfNeeded(pathForSaving))
                    {
                        DeleteFileIfExist(pathForSaving, newFile);

                        file.SaveAs(Path.Combine(pathForSaving, newFileName));

                        isUploaded = true;

                        path = Path.Combine(pathForSaving, newFileName);
                    if (path != null)
                    {

                        imageUrl = "/Content/Student/Credential/" + newFileName;
                        var studentDetails = studentLogic.GetModelBy(x => x.Person_Id == personId);
                        studentDetails.StudentMedicalReport = imageUrl;
                        studentLogic.Modify(studentDetails);
                        SetMessage("Medical Report was uploaded successfully", Message.Category.Information);

                        //imageUrlDisplay = appRoot + imageUrl + "?t=" + DateTime.Now;

                    }
                }
              
            }

            //return View(model);
            return RedirectToAction("SelectCourseRegistrationSession", "Registration");
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
                    SetMessage("File type '" + fileExtension + "' is invalid! ", Message.Category.Error);
                }
                else if (actualFileSizeToUpload > (maximumFileSize / oneKiloByte))
                {
                    SetMessage("Your file size of " + actualFileSizeToUpload.ToString("0.#") + " Kb is too large, maximum allowed size is " + (maximumFileSize / oneKiloByte) + " Kb", Message.Category.Error);
                }

                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool InvalidFileTypePdf(string extension)
        {
            extension = extension.ToLower();
            switch (extension)
            {
                case ".pdf":
                    return false;
                case ".jpg":
                    return false;
                case ".jpeg":
                    return false;
                default:
                    return true;
            }
        }
    }
}