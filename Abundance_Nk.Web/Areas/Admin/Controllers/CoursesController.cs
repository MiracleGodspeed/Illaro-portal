﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Abundance_Nk.Model.Entity;
using Abundance_Nk.Business;
using Abundance_Nk.Model.Entity.Model;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Web.Controllers;
using Abundance_Nk.Web.Areas.Admin.ViewModels;
using Abundance_Nk.Web.Models;
using System.Transactions;
using System.IO;
using Abundance_Nk.Model.Translator;
using System.Web.UI.WebControls;
using Ionic.Zip;

namespace Abundance_Nk.Web.Areas.Admin.Controllers
{
    
    public class CoursesController : BaseController
    {
        private const string ID = "Id";
        private const string NAME = "Name";
        private const string VALUE = "Value";
        private const string TEXT = "Text";
        private Abundance_NkEntities db = new Abundance_NkEntities();
        private CourseViewModel viewmodel;
        private CourseRegistrationStatusLogic _registrationStatusLogic;
        private UserLogic _userLogic;
        private GeneralAuditLogic _auditLogic;
        private ProgrammeLogic _programmeLogic;
        private SessionLogic _sessionLogic;

        public CoursesController()
        {
            viewmodel = new CourseViewModel();
        }
        // GET: Admin/Courses
       public ActionResult Index()
        {

            viewmodel = new CourseViewModel();
            ViewBag.Departments = viewmodel.DepartmentSelectListItem;
            ViewBag.Programmes = viewmodel.ProgrammeSelectListItem;
            ViewBag.levels = viewmodel.levelSelectListItem;
            ViewBag.DepartmentOptionId = new SelectList(new List<DepartmentOption>(), ID, NAME);

            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Index(CourseViewModel vModel)
        {
            try
            {
                Semester firstSemester = new Semester() { Id = 1 };
                Semester secondSemester = new Semester() { Id = 2 };
                Semester thirdSemester = new Semester() { Id = 3 };
                CourseLogic courseLogic = new CourseLogic();


                if (vModel.course.DepartmentOption != null && vModel.course.DepartmentOption.Id > 0 )
                {
                    vModel.firstSemesterCourses = courseLogic.GetBy(vModel.course.Programme, vModel.course.Department, vModel.course.DepartmentOption, vModel.course.Level, firstSemester);
                    vModel.secondSemesterCourses = courseLogic.GetBy(vModel.course.Programme, vModel.course.Department, vModel.course.DepartmentOption, vModel.course.Level, secondSemester);
                    vModel.thirdSemesterCourses = courseLogic.GetBy(vModel.course.Programme, vModel.course.Department, vModel.course.DepartmentOption, vModel.course.Level, thirdSemester);
                }
                else
                {
                    vModel.firstSemesterCourses = courseLogic.GetBy(vModel.course.Programme, vModel.course.Department, vModel.course.Level, firstSemester);
                    vModel.secondSemesterCourses = courseLogic.GetBy(vModel.course.Programme, vModel.course.Department, vModel.course.Level, secondSemester);
                    vModel.thirdSemesterCourses = courseLogic.GetBy(vModel.course.Programme, vModel.course.Department, vModel.course.Level, thirdSemester);

                }
                
                
                RetainDropdown(vModel);
            }
            catch (Exception ex)
            {
                
                throw;
            }
            return View(vModel);
        }

        public void RetainDropdown(CourseViewModel vModel)
        {
            try
            {
                ViewBag.DepartmentOptionId = new SelectList(new List<DepartmentOption>(), ID, NAME);

                if (vModel.course.Department != null && vModel.course.Department.Id > 0)
                {
                    
                    vModel.DepartmentOpionSelectListItem = Utility.PopulateDepartmentOptionSelectListItem(vModel.course.Department,vModel.course.Programme );
                
                    ViewBag.Departments = new SelectList(vModel.DepartmentSelectListItem, Utility.VALUE, Utility.TEXT, vModel.course.Department.Id);
                    if (vModel.course.DepartmentOption != null && vModel.course.DepartmentOption.Id > 0)
                    {
                        ViewBag.DepartmentOptionId = new SelectList(vModel.DepartmentOpionSelectListItem, Utility.VALUE, Utility.TEXT, vModel.course.DepartmentOption.Id);

                    }
                    
                }
                else
                {
                    ViewBag.Departments = viewmodel.DepartmentSelectListItem;
                    ViewBag.DepartmentOptionId = new SelectList(vModel.DepartmentOpionSelectListItem, VALUE, TEXT);

                }
                if (vModel.course.Level != null && vModel.course.Level.Id > 0)
                {
                    ViewBag.levels = new SelectList(vModel.levelSelectListItem, Utility.VALUE, Utility.TEXT, vModel.course.Level.Id);

                }
                else
                {
                    ViewBag.levels = viewmodel.levelSelectListItem;
                }

                if (vModel.course.Programme != null && vModel.course.Programme.Id > 0)
                {
                    ViewBag.Programmes = new SelectList(vModel.ProgrammeSelectListItem, Utility.VALUE, Utility.TEXT, vModel.course.Programme.Id);

                }
                else
                {
                    ViewBag.Programmes = viewmodel.ProgrammeSelectListItem;
                }
            }
            catch (Exception ex)
            {
                
                throw;
            }
        }

        public ActionResult Edit()
        {
            viewmodel = new CourseViewModel();
            ViewBag.Departments = viewmodel.DepartmentSelectListItem;
            ViewBag.Programmes = viewmodel.ProgrammeSelectListItem;
            ViewBag.levels = viewmodel.levelSelectListItem;
            ViewBag.DepartmentOptionId = new SelectList(new List<DepartmentOption>(), ID, NAME);

            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(CourseViewModel vModel)
        {
            try
            {
                Semester firstSemester = new Semester() { Id = 1 };
                Semester secondSemester = new Semester() { Id = 2 };
                Semester thirdSemester = new Semester() { Id = 3 };
                CourseLogic courseLogic = new CourseLogic();

                if (vModel.course.DepartmentOption != null && vModel.course.DepartmentOption.Id > 0)
                {
                    vModel.firstSemesterCourses = courseLogic.GetBy(vModel.course.Programme, vModel.course.Department,vModel.course.DepartmentOption,vModel.course.Level, firstSemester);
                    vModel.secondSemesterCourses = courseLogic.GetBy(vModel.course.Programme, vModel.course.Department,vModel.course.DepartmentOption,vModel.course.Level, secondSemester);
                    vModel.thirdSemesterCourses = courseLogic.GetBy(vModel.course.Programme, vModel.course.Department, vModel.course.DepartmentOption, vModel.course.Level, thirdSemester);

                }
                else
                {
                    vModel.firstSemesterCourses = courseLogic.GetBy(vModel.course.Programme, vModel.course.Department, vModel.course.Level, firstSemester);
                    vModel.secondSemesterCourses = courseLogic.GetBy(vModel.course.Programme, vModel.course.Department, vModel.course.Level, secondSemester);
                    vModel.thirdSemesterCourses = courseLogic.GetBy(vModel.course.Programme, vModel.course.Department, vModel.course.Level, thirdSemester);

                }

                Course firstSemesterBlanks = new Course();
                firstSemesterBlanks.Unit = 0;
                firstSemesterBlanks.Id = -1;
                firstSemesterBlanks.Semester = firstSemester;
                firstSemesterBlanks.Department = vModel.course.Department;
                firstSemesterBlanks.Programme = vModel.course.Programme;
                firstSemesterBlanks.DepartmentOption = vModel.course.DepartmentOption;
                firstSemesterBlanks.Level = vModel.course.Level;

                Course secondSemesterBlanks = new Course();
                secondSemesterBlanks.Unit = 0;
                secondSemesterBlanks.Id = -1;
                secondSemesterBlanks.Semester = secondSemester;
                secondSemesterBlanks.Department = vModel.course.Department;
                secondSemesterBlanks.Programme = vModel.course.Programme;
                secondSemesterBlanks.DepartmentOption = vModel.course.DepartmentOption;
                secondSemesterBlanks.Level = vModel.course.Level;

                Course thirdSemesterBlanks = new Course();
                thirdSemesterBlanks.Unit = 0;
                thirdSemesterBlanks.Id = -1;
                thirdSemesterBlanks.Semester = thirdSemester;
                thirdSemesterBlanks.Department = vModel.course.Department;
                thirdSemesterBlanks.Programme = vModel.course.Programme;
                thirdSemesterBlanks.DepartmentOption = vModel.course.DepartmentOption;
                thirdSemesterBlanks.Level = vModel.course.Level;

                int blankCount = 5;
                if (vModel.firstSemesterCourses.Count < 1 && vModel.secondSemesterCourses.Count < 1 && vModel.thirdSemesterCourses.Count < 1)
                {
                    blankCount = 15;
                }
                for (int i = 0; i < blankCount; i++)
                {
                    vModel.firstSemesterCourses.Add(firstSemesterBlanks);
                    vModel.secondSemesterCourses.Add(secondSemesterBlanks);
                    vModel.thirdSemesterCourses.Add(thirdSemesterBlanks);
                }

                RetainDropdown(vModel);
            }
            catch (Exception ex)
            {
                SetMessage(ex.Message, Message.Category.Information);
            }
            return View(vModel);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult SaveCourseChanges(CourseViewModel vModel)
        {
            try
            {
                if (vModel.firstSemesterCourses.Count > 0)
                {
                    CourseLogic courseLogic = new CourseLogic();
                    Semester semester = new Semester() {Id =1 };
                    courseLogic.Modify(vModel.firstSemesterCourses);
                }

                if (vModel.secondSemesterCourses.Count > 0)
                {
                    CourseLogic courseLogic = new CourseLogic();
                    Semester semester = new Semester() {Id =2 };
                    courseLogic.Modify(vModel.secondSemesterCourses);
                }

                if (vModel.thirdSemesterCourses.Count > 0)
                {
                    CourseLogic courseLogic = new CourseLogic();
                    Semester semester = new Semester() { Id = 3};
                    courseLogic.Modify(vModel.thirdSemesterCourses);
                }

            }
            catch (Exception ex)
            {
                SetMessage(ex.Message, Message.Category.Error);
            }
            SetMessage("Courses were updated successfully", Message.Category.Information);
            return RedirectToAction("Edit");
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

        public ActionResult ViewStudentCourses()
        {
            CourseViewModel viewModel = new CourseViewModel();
            try
            {
                ViewBag.Session = viewModel.SessionSelectList;
                ViewBag.Level = viewModel.levelSelectListItem;
            }
            catch (Exception ex)
            {   
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult ViewStudentCourses(CourseViewModel viewModel)
        {
            StudentLogic studentLogic = new StudentLogic();
            StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
            CourseRegistrationLogic courseRegistrationLogic = new CourseRegistrationLogic();
            CourseRegistrationDetailLogic courseRegistrationDetailLogic = new CourseRegistrationDetailLogic();
            CourseLogic courseLogic = new CourseLogic();
            
            Model.Model.Student student = new Model.Model.Student();
            StudentLevel studentLevel = new StudentLevel();
            CourseRegistration courseRegistration = new CourseRegistration();
            CourseRegistrationDetail courseRegistrationDetail = new CourseRegistrationDetail();

            List<Model.Model.Student> students = new List<Model.Model.Student>();
            List<Course> courses = new List<Course>();
            List<CourseRegistration> courseRegistrations = new List<CourseRegistration>();
            List<long> courseIds = new List<long>();
            List<CourseRegistrationDetail> courseRegDetails = new List<CourseRegistrationDetail>();

            try
            {
                if (viewModel.Student.MatricNumber != null && viewModel.Session.Id > 0 && viewModel.level.Id > 0)
                {
                    students = studentLogic.GetModelsBy(s => s.Matric_Number == viewModel.Student.MatricNumber);
                    if (students.Count != 1)
                    {
                        SetMessage("Duplicate Matric Number or No Student Record", Message.Category.Error);
                        RetainStudentCourseDropDown(viewModel);
                        return View(viewModel);
                    }

                    student = students.FirstOrDefault();
                    studentLevel = studentLevelLogic.GetModelBy(sl => sl.Person_Id == student.Id && sl.Level_Id == viewModel.level.Id && sl.Session_Id == viewModel.Session.Id);
                    courses = courseLogic.GetModelsBy(c => c.Activated == true && c.Department_Id == studentLevel.Department.Id && c.Level_Id == viewModel.level.Id && c.Programme_Id == viewmodel.programme.Id);
                    courseRegistrations = courseRegistrationLogic.GetModelsBy(cr => cr.Department_Id == studentLevel.Department.Id && cr.Level_Id == viewModel.level.Id && cr.Person_Id == student.Id && cr.Programme_Id == studentLevel.Programme.Id && cr.Session_Id == viewModel.Session.Id);
                    
                    if (courseRegistrations.Count == 1)
                    {
                        courseRegistration = courseRegistrations.FirstOrDefault();
                        courseRegDetails = courseRegistrationDetailLogic.GetModelsBy(crd => crd.Student_Course_Registration_Id == courseRegistration.Id);
                        courseIds = courseRegDetails.Select(crd => crd.Course.Id).Distinct().ToList();

                        for (int i = 0; i < courses.Count; i++)
                        {
                            if (courseIds.Contains(courses[i].Id))
                            {
                                courses[i].IsRegistered = true;
                                long thisCourseId = courses[i].Id;
                                courseRegistrationDetail = courseRegDetails.Where(crd => crd.Course.Id == thisCourseId).SingleOrDefault();
                                if (courseRegistrationDetail.Mode.Id == 2)
                                {
                                    courses[i].isCarryOverCourse = true;
                                }
                            }
                            
                        }

                        viewModel.Courses = courses;
                        viewModel.Student = student;
                        viewModel.CourseRegistration = courseRegistration;
                        RetainStudentCourseDropDown(viewModel);
                        return View(viewModel);
                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            RetainStudentCourseDropDown(viewModel);
            return View(viewModel); 
        }

        public ActionResult SaveStudentCourses(CourseViewModel viewModel)
        {
            try
            {
                if (viewModel.Courses.Count > 0)
                {
                    string operation = "MODIFY";
                    string action = "ADMIN :CHANGES FROM ADMIN CONSOLE (CoursesController)";
                    string client = Request.LogonUserIdentity.Name + " (" + HttpContext.Request.UserHostAddress + ")";
                    var courseRegistrationDetailAudit = new CourseRegistrationDetailAudit();
                    courseRegistrationDetailAudit.Action = action;
                    courseRegistrationDetailAudit.Operation = operation;
                    courseRegistrationDetailAudit.Client = client;
                    UserLogic loggeduser = new UserLogic();
                    courseRegistrationDetailAudit.User = loggeduser.GetModelBy(u => u.User_Name == User.Identity.Name);

                    CourseRegistrationDetailLogic courseRegistrationDetailLogic = new CourseRegistrationDetailLogic();
                    CourseMode firstAttemptCourseMode = new CourseMode() { Id = 1};
                    CourseMode carryOverCourseMode = new CourseMode() { Id = 2};
                    CourseRegistrationDetail courseRegistrationDetail = new CourseRegistrationDetail();
                    CourseRegistrationLogic courseRegistrationLogic = new CourseRegistrationLogic();
                    CourseLogic courseLogic = new CourseLogic();

                    CourseRegistration courseRegistration = courseRegistrationLogic.GetModelBy(cr => cr.Student_Course_Registration_Id == viewModel.CourseRegistration.Id);

                    for (int i = 0; i < viewModel.Courses.Count; i++)
                    {
                        if (viewModel.Courses[i].IsRegistered)
                        {
                            long thisCourseId = viewModel.Courses[i].Id;
                            Course thisCourse = courseLogic.GetModelBy(c => c.Course_Id == thisCourseId);
                            courseRegistrationDetail = courseRegistrationDetailLogic.GetModelBy(crd => crd.Student_Course_Registration_Id == courseRegistration.Id && crd.Course_Id == thisCourseId);
                            if (courseRegistrationDetail != null)
                            {
                                if (viewModel.Courses[i].isCarryOverCourse)
                                {
                                    courseRegistrationDetail.Mode = carryOverCourseMode;
                                    courseRegistrationDetailLogic.Modify(courseRegistrationDetail,courseRegistrationDetailAudit);
                                }
                                else
                                {
                                    courseRegistrationDetail.Mode = firstAttemptCourseMode;
                                    courseRegistrationDetailLogic.Modify(courseRegistrationDetail,courseRegistrationDetailAudit);
                                }
                            }
                            else
                            {
                                courseRegistrationDetail = new CourseRegistrationDetail();
                                courseRegistrationDetail.CourseRegistration = courseRegistration;
                                courseRegistrationDetail.Course = thisCourse;
                                if (viewModel.Courses[i].isCarryOverCourse)
                                {
                                    courseRegistrationDetail.Mode = carryOverCourseMode;
                                }
                                else
                                {
                                    courseRegistrationDetail.Mode = firstAttemptCourseMode;
                                }
                                courseRegistrationDetail.CourseUnit = thisCourse.Unit;
                                courseRegistrationDetail.Semester = thisCourse.Semester;

                                courseRegistrationDetailLogic.Create(courseRegistrationDetail);
                            }
                        }
                        else
                        {
                            long thisCourseId = viewModel.Courses[i].Id;
                            courseRegistrationDetail = courseRegistrationDetailLogic.GetModelBy(crd => crd.Student_Course_Registration_Id == courseRegistration.Id && crd.Course_Id == thisCourseId);
                            if (courseRegistrationDetail != null)
                            {
                                courseRegistrationDetailLogic.Delete(crd => crd.Student_Course_Registration_Detail_Id == courseRegistrationDetail.Id);
                            }
                        }
                    }
                    
                    SetMessage("Operation Successful! ", Message.Category.Information);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return RedirectToAction("ViewStudentCourses");
        }
        private void RetainStudentCourseDropDown(CourseViewModel viewModel)
        {
            try
            {
                if (viewModel != null)
                {
                    if (viewModel.Session != null && viewModel.Session.Id > 0)
                    {
                        ViewBag.Session = new SelectList(Utility.PopulateAllSessionSelectListItem(), "Value", "Text", viewModel.Session.Id);
                    }
                    else
                    {
                        ViewBag.Session = new SelectList(Utility.PopulateAllSessionSelectListItem(), "Value", "Text");
                    }

                    if (viewModel.level != null && viewModel.level.Id > 0)
                    {
                        ViewBag.Level = new SelectList(Utility.PopulateLevelSelectListItem(), "Value", "Text", viewModel.level.Id); 
                    }
                    else
                    {
                        ViewBag.Level = new SelectList(Utility.PopulateLevelSelectListItem(), "Value", "Text"); 
                    }
                }
            }
            catch (Exception)
            {       
                throw;
            }
        }

        public ActionResult AddAndUpdateCourses()
        {
            CourseViewModel viewModel = new CourseViewModel();
            try
            { 
                ViewBag.Programme = viewModel.ProgrammeSelectListItem;
                ViewBag.Department = new SelectList(new List<Department>(), ID, NAME);
                ViewBag.DepartmentOption = new SelectList(new List<DepartmentOption>(), ID, NAME);
                ViewBag.Session = viewModel.SessionSelectList;
                ViewBag.Level = viewModel.levelSelectListItem;
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult AddAndUpdateCourses(CourseViewModel viewModel)
        {
            try
            {
                if (viewModel != null)
                { 
                    List<Course> courses = new List<Course>();
                    SemesterLogic semesterLogic = new SemesterLogic();

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
                                if (FileSet.Tables[0].Rows[i][0] == " " || FileSet.Tables[0].Rows[i][0] == "")
                                {
                                    continue;
                                }

                                string courseCode = FileSet.Tables[0].Rows[i][0].ToString().Trim();
                                string coursetitle = FileSet.Tables[0].Rows[i][1].ToString().Trim();
                                int courseUnit = Convert.ToInt32(FileSet.Tables[0].Rows[i][2].ToString().Trim());
                                int semesterId = Convert.ToInt32(FileSet.Tables[0].Rows[i][3].ToString().Trim());
                                Semester semester = semesterLogic.GetModelBy(s => s.Semester_Id == semesterId);

                                Course course = new Course();
                                course.Code = courseCode;
                                course.Name = coursetitle;
                                course.Unit = courseUnit;
                                course.Semester = semester;

                                courses.Add(course);
                            }
                        }
                    }

                    viewModel.Courses = courses;
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            DepartmentLogic departmentLogic = new DepartmentLogic();
            DepartmentOptionLogic departmentOptionLogic = new DepartmentOptionLogic();
            List<Department> departments = departmentLogic.GetBy(viewModel.programme);
            List<DepartmentOption> departmentOptions = departmentOptionLogic.GetBy(viewModel.Department, viewModel.programme);
            if (viewModel.DepartmentOption != null && viewModel.DepartmentOption.Id > 0)
            {
                ViewBag.DepartmentOption = new SelectList(departmentOptions, ID, NAME, viewModel.DepartmentOption.Id);
            }
            else
            {
                ViewBag.DepartmentOption = new SelectList(new List<DepartmentOption>(), ID, NAME);
            }

            ViewBag.Programme = viewModel.ProgrammeSelectListItem;
            ViewBag.Department = new SelectList(departments, ID, NAME, viewModel.Department.Id);
            ViewBag.Session = viewModel.SessionSelectList;
            ViewBag.Level = viewModel.levelSelectListItem;
            TempData["viewModel"] = viewModel;

            return View(viewModel);
        }

        public ActionResult SaveAddedCourses()
        {
            try
            {
                CourseViewModel viewModel = (CourseViewModel) TempData["viewModel"];
                if (viewModel != null)
                {
                    CourseLogic courseLogic = new CourseLogic();

                    Department department = new Department(){ Id = viewModel.Department.Id};
                    DepartmentOption departmentOption = null;
                    if (viewModel.DepartmentOption != null && viewModel.DepartmentOption.Id > 0)
                    {
                        departmentOption = new DepartmentOption(){ Id = viewModel.DepartmentOption.Id}; 
                    }
                    Level level = new Level(){Id = viewModel.level.Id};
                    CourseType courseType = new CourseType(){Id = 1};

                    List<Course> existingCourses = GetExistingCourses(level, department, departmentOption);

                    for (int i = 0; i < viewModel.Courses.Count; i++)
                    {
                        string courseCode = viewModel.Courses[i].Code;
                        string coursetitle = viewModel.Courses[i].Name;
                        int courseUnit = viewModel.Courses[i].Unit;
                        Semester semester = viewModel.Courses[i].Semester;
                                
                        Course checkExistingCourse = null;
                                
                        if (departmentOption != null)
                        {
                            checkExistingCourse = existingCourses.LastOrDefault(c => c.Code.Trim().Replace(" ", "") == courseCode.Trim().Replace(" ", "") && c.Department.Id == department.Id && c.DepartmentOption.Id == departmentOption.Id && c.Level.Id == level.Id && c.Semester.Id == semester.Id); 
                        }
                        else
                        {
                            checkExistingCourse = existingCourses.LastOrDefault(c => c.Code.Trim().Replace(" ", "") == courseCode.Trim().Replace(" ", "") && c.Department.Id == department.Id && c.Level.Id == level.Id && c.Semester.Id == semester.Id); 
                        }

                        if (checkExistingCourse != null)
                        {
                            checkExistingCourse.Code = courseCode.Trim().Replace(" ", "");
                            checkExistingCourse.Name = coursetitle;
                            checkExistingCourse.Unit = courseUnit;

                            courseLogic.Modify(checkExistingCourse);

                            Course thisCourse = existingCourses.LastOrDefault(c => c.Code == courseCode.Trim().Replace(" ", ""));
                            existingCourses.Remove(thisCourse);
                        }
                        else
                        {
                            checkExistingCourse = new Course();
                            checkExistingCourse.Code = courseCode.Trim().Replace(" ", "");
                            checkExistingCourse.Name = coursetitle;
                            checkExistingCourse.Unit = courseUnit;
                            checkExistingCourse.Activated = true;
                            checkExistingCourse.Department = department;
                            checkExistingCourse.DepartmentOption = departmentOption;
                            checkExistingCourse.Level = level;
                            checkExistingCourse.Type = courseType;
                            checkExistingCourse.Semester = semester;

                            courseLogic.Create(checkExistingCourse);
                        }
                    }

                    if (existingCourses.Count > 0)
                    {
                        for (int j = 0; j < existingCourses.Count; j++)
                        {
                            Course currentCourse = existingCourses[j];
                            currentCourse.Activated = false;

                            courseLogic.Modify(currentCourse);
                        }
                    } 

                SetMessage("Operation Successful! ", Message.Category.Information); 
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return RedirectToAction("AddAndUpdateCourses");
        }

        private List<Course> GetExistingCourses(Level level, Department department, DepartmentOption departmentOption)
        {
            List<Course> courses = new List<Course>();
            try
            { 
                CourseLogic courseLogic = new CourseLogic(); 

                if (departmentOption != null && departmentOption.Id > 0)
                {
                    courses = courseLogic.GetModelsBy(c => c.Department_Id == department.Id && c.Department_Option_Id == departmentOption.Id && c.Level_Id == level.Id);
                }
                else
                {
                    courses = courseLogic.GetModelsBy(c => c.Department_Id == department.Id && c.Level_Id == level.Id);
                }
            }
            catch (Exception)
            {
                throw;
            }

            return courses;
        }

        public ActionResult ActivateCourses()
        {
            CourseViewModel viewModel = new CourseViewModel();
            try
            {
                ViewBag.Programme = viewModel.ProgrammeSelectListItem;
                ViewBag.Department = new SelectList(new List<Department>(), ID, NAME);
                ViewBag.DepartmentOption = new SelectList(new List<DepartmentOption>(), ID, NAME);
                ViewBag.Semester = viewModel.SemesterSelectList;
                ViewBag.Level = viewModel.levelSelectListItem;
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult ActivateCourses(CourseViewModel viewModel)
        {
            try
            {
                if (viewModel != null)
                {
                    List<Course> courses = new List<Course>();
            
                    CourseLogic courseLogic = new CourseLogic();

                    if (viewModel.DepartmentOption != null && viewModel.DepartmentOption.Id > 0)
                    {
                        courses = courseLogic.GetModelsBy(c => c.Programme_Id == viewModel.programme.Id && c.Department_Id == viewModel.Department.Id && c.Department_Option_Id == viewModel.DepartmentOption.Id && c.Level_Id == viewModel.level.Id && c.Semester_Id == viewModel.Semester.Id);
                    }
                    else
                    {
                        courses = courseLogic.GetModelsBy(c => c.Programme_Id == viewModel.programme.Id && c.Department_Id == viewModel.Department.Id && c.Level_Id == viewModel.level.Id && c.Semester_Id == viewModel.Semester.Id);
                    }

                    for (int i = 0; i < courses.Count; i++)
                    {
                        courses[i].isActivated = courses[i].Activated.Value;
                    }

                    viewModel.Courses = courses;
                }

                DepartmentLogic departmentLogic = new DepartmentLogic();
                DepartmentOptionLogic departmentOptionLogic = new DepartmentOptionLogic();
                List<Department> departments = departmentLogic.GetBy(viewModel.programme);
                List<DepartmentOption> departmentOptions = departmentOptionLogic.GetBy(viewModel.Department, viewModel.programme);
                if (viewModel.DepartmentOption != null && viewModel.DepartmentOption.Id > 0)
                {
                    ViewBag.DepartmentOption = new SelectList(departmentOptions, ID, NAME, viewModel.DepartmentOption.Id);
                }
                else
                {
                    ViewBag.DepartmentOption = new SelectList(new List<DepartmentOption>(), ID, NAME);
                }

                ViewBag.Programme = viewModel.ProgrammeSelectListItem;
                ViewBag.Department = new SelectList(departments, ID, NAME, viewModel.Department.Id);
                ViewBag.Semester = viewModel.SemesterSelectList;
                ViewBag.Level = viewModel.levelSelectListItem;
                TempData["viewModel"] = viewModel;
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }

        public ActionResult SaveActivatedCourses(CourseViewModel viewModel)
        {
            try
            {
                if (viewModel != null)
                {
                    CourseLogic courseLogic = new CourseLogic();

                    for (int i = 0; i < viewModel.Courses.Count; i++)
                    {
                        Course courseToModify = viewModel.Courses[i];
                        Course course = courseLogic.GetModelBy(c => c.Course_Id == courseToModify.Id);
                        course.Activated = courseToModify.isActivated;

                        courseLogic.Modify(course);
                    }

                    SetMessage("Operation Successful!", Message.Category.Information);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return RedirectToAction("ActivateCourses");
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

        public ActionResult ActivateCourseRegistration()
        {
            try
            {
                PopulateAllDropDown(viewmodel);
            }
            catch (Exception ex)
            {
                SetMessage("Error!" + ex.Message, Message.Category.Error);
            }

            return View(viewmodel);
        }
        [HttpPost]
        public ActionResult ActivateCourseRegistration(CourseViewModel viewModel)
        {
            try
            {
                if (viewModel != null && viewModel.programme.Id > 0 && viewModel.Session.Id > 0)
                {
                    List<CourseRegistrationStatus> registrationStatuses = new List<CourseRegistrationStatus>();
                    CourseRegistrationStatus registrationStatus = new CourseRegistrationStatus();
                    Utility.GetDepartmentByProgramme(viewModel.programme).ForEach(d =>
                    {
                        if (d.Id > 0)
                        {
                            CreateRegistrationStatus(d, viewModel.programme, viewModel.Session, out registrationStatus);
                            registrationStatuses.Add(registrationStatus);
                        }
                    });

                    viewModel.CourseRegistrationStatusList = registrationStatuses.OrderBy(r => r.Department.Faculty.Name).ThenBy(r => r.Department.Name).ToList();
                    PopulateAllDropDown(viewModel);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error!" + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }

        private void CreateAudit(Session session, Programme programme)
        {
            try
            {
                _auditLogic = new GeneralAuditLogic();
                _userLogic = new UserLogic();
                GeneralAudit generalAudit = new GeneralAudit();

                User user = _userLogic.GetModelBy(p => p.User_Name == User.Identity.Name);
                string client = Request.LogonUserIdentity.Name + " (" + HttpContext.Request.UserHostAddress + ")";

                generalAudit.Action = "INSERT";
                generalAudit.Client = client;
                generalAudit.CurrentValues = "-";
                generalAudit.InitialValues = "-";
                generalAudit.Operation = "Added Course Registration Status for " + programme.Name + " in " + session.Name + " Session";
                generalAudit.TableNames = "COURSE REGISTRATION STATUS";
                generalAudit.Time = DateTime.Now;
                generalAudit.User = user;

                _auditLogic.Create(generalAudit);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private Session GetSession(int sessionId, SessionLogic logic )
        {
            return logic.GetModelBy(s => s.Session_Id == sessionId);
        }

        private Programme GetProgramme(int programmeId, ProgrammeLogic logic)
        {
            return logic.GetModelBy(p => p.Programme_Id == programmeId);
        }

        private void CreateRegistrationStatus(Department department, Programme programme, Session session, out CourseRegistrationStatus registrationStatus)
        {
            try
            {
                _registrationStatusLogic = new CourseRegistrationStatusLogic();
                registrationStatus = new CourseRegistrationStatus();

                programme = GetProgramme(programme.Id, new ProgrammeLogic());
                session = GetSession(session.Id, new SessionLogic());
                department = GetDepartment(department.Id, new DepartmentLogic());

                CourseRegistrationStatus existingStatus = _registrationStatusLogic.GetModelsBy(r => r.Department_Id == department.Id && r.Programme_Id == programme.Id && 
                                                                                                    r.Session_Id == session.Id).LastOrDefault();
                if (existingStatus == null)
                {
                    existingStatus = new CourseRegistrationStatus();
                    existingStatus.Department = department;
                    existingStatus.Active = false;
                    existingStatus.Programme = programme;
                    existingStatus.Session = session;

                    CourseRegistrationStatus createdStatus = _registrationStatusLogic.Create(existingStatus);
                    existingStatus.Id = createdStatus.Id;

                    CreateAudit(session, programme);
                }

                registrationStatus = existingStatus;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private Department GetDepartment(int departmentId, DepartmentLogic logic)
        {
            try
            {
                return logic.GetModelBy(p => p.Department_Id == departmentId);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void PopulateAllDropDown(CourseViewModel viewModel)
        {
            try
            {
                if (viewModel.programme != null && viewModel.programme.Id > 0)
                    ViewBag.Programme = new SelectList(viewModel.ProgrammeSelectListItem, VALUE, TEXT, viewModel.programme.Id);
                else
                    ViewBag.Programme = viewModel.ProgrammeSelectListItem;

                if (viewModel.Session != null && viewModel.Session.Id > 0)
                    ViewBag.Session = new SelectList(viewModel.SessionSelectListItem, VALUE, TEXT, viewModel.Session.Id);
                else
                    ViewBag.Session = viewModel.SessionSelectListItem;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public JsonResult ModifyCourseRegistrationStatus(int statusId, bool status)
        {
            try
            {
                if (statusId > 0)
                {
                    _registrationStatusLogic = new CourseRegistrationStatusLogic();
                    CourseRegistrationStatus existingRegistrationStatus = _registrationStatusLogic.GetModelBy(r => r.Course_Registration_Status_Id == statusId);
                    
                    if (existingRegistrationStatus != null)
                    {
                        bool prevStatus = existingRegistrationStatus.Active;
                        existingRegistrationStatus.Active = status;
                        _registrationStatusLogic.Modify(existingRegistrationStatus);

                        _auditLogic = new GeneralAuditLogic();
                        _userLogic = new UserLogic();
                        GeneralAudit generalAudit = new GeneralAudit();

                        User user = _userLogic.GetModelBy(p => p.User_Name == User.Identity.Name);
                        string client = Request.LogonUserIdentity.Name + " (" + HttpContext.Request.UserHostAddress + ")";

                        generalAudit.Action = "UPDATE";
                        generalAudit.Client = client;
                        generalAudit.CurrentValues = status.ToString();
                        generalAudit.InitialValues = prevStatus.ToString();
                        generalAudit.Operation = "Modified Course Registration Status for " + existingRegistrationStatus.Programme.Name + " in " + existingRegistrationStatus.Session.Name + " Session";
                        generalAudit.TableNames = "COURSE REGISTRATION STATUS";
                        generalAudit.Time = DateTime.Now;
                        generalAudit.User = user;

                        _auditLogic.Create(generalAudit);
                    }
                    else
                        return Json("Status not found!", JsonRequestBehavior.AllowGet);

                    return Json("Modified", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Invalid Parameter!", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ModifyCourseRegistrationStatusBulk(int progId, int sessionId, bool status)
        {
            try
            {
                if (progId > 0 && sessionId > 0)
                {
                    _registrationStatusLogic = new CourseRegistrationStatusLogic();
                    _auditLogic = new GeneralAuditLogic();
                    _userLogic = new UserLogic();

                    User user = _userLogic.GetModelBy(p => p.User_Name == User.Identity.Name);
                    string client = Request.LogonUserIdentity.Name + " (" + HttpContext.Request.UserHostAddress + ")";

                    List<CourseRegistrationStatus> existingRegistrationStatuses = _registrationStatusLogic.GetModelsBy(r => r.Programme_Id == progId && r.Session_Id == sessionId);

                    if (existingRegistrationStatuses != null && existingRegistrationStatuses.Count > 0)
                    {
                        for (int i = 0; i < existingRegistrationStatuses.Count; i++)
                        {
                            CourseRegistrationStatus existingRegistrationStatus = existingRegistrationStatuses[i];

                            bool prevStatus = existingRegistrationStatus.Active;

                            existingRegistrationStatus.Active = status;
                            _registrationStatusLogic.Modify(existingRegistrationStatus);

                            GeneralAudit generalAudit = new GeneralAudit();
                            generalAudit.Action = "UPDATE";
                            generalAudit.Client = client;
                            generalAudit.CurrentValues = status.ToString();
                            generalAudit.InitialValues = prevStatus.ToString();
                            generalAudit.Operation = "Modified Course Registration Status for " + existingRegistrationStatus.Department.Name + ", " + existingRegistrationStatus.Programme.Name + 
                                                    " in " + existingRegistrationStatus.Session.Name + " Session";
                            generalAudit.TableNames = "COURSE REGISTRATION STATUS";
                            generalAudit.Time = DateTime.Now;
                            generalAudit.User = user;

                            _auditLogic.Create(generalAudit);
                        }
                    }
                    else
                        return Json("Status not found!", JsonRequestBehavior.AllowGet);

                    return Json("200", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Invalid Parameter!", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult DownloadCoursesView()
        {
            return View();
        }
        
        public ActionResult DownloadCourses()
        {
            try
            {
                ProgrammeLogic programmeLogic = new ProgrammeLogic();
                DepartmentLogic departmentLogic = new DepartmentLogic();
                LevelLogic levelLogic = new LevelLogic();
                DepartmentOptionLogic optionLogic = new DepartmentOptionLogic();
                List<Programme> programmes = programmeLogic.GetModelsBy(p => p.Activated == true);

                string pathToSave = "~/Content/courses/" + DateTime.Now.Ticks;

                if (!Directory.Exists(Server.MapPath(pathToSave)))
                {
                    Directory.CreateDirectory(Server.MapPath(pathToSave));
                }
                
                for (int i = 0; i < programmes.Count; i++)
                {
                    Programme programme = programmes[i];

                    List<Department> departments = departmentLogic.GetBy(programme);

                    for (int j = 0; j < departments.Count; j++)
                    {
                        Department department = departments[j];

                        List<Level> levels = new List<Level>();
                        if (programme.Id > 2)
                        {
                            levels.Add(new Level { Id = 3, Name = "HND I" });
                            levels.Add(new Level { Id = 4, Name = "HND II" });
                        }
                        else
                        {
                            levels.Add(new Level { Id = 1, Name = "ND I" });
                            levels.Add(new Level { Id = 2, Name = "ND II" });
                        }

                        for (int k = 0; k < levels.Count; k++)
                        {
                            Level level = levels[k];

                            List<DepartmentOption> options = optionLogic.GetBy(department, programme);
                            
                            if (options != null && options.Count > 0)
                            {
                                for (int l = 0; l < options.Count; l++)
                                {
                                    DepartmentOption option = options[l];

                                    List<CourseExport> courses = GetCourses(programme, level, department, option);
                                    
                                    GridView gv = new GridView();
                                    DataTable dt = new DataTable();

                                    DataColumn dcEmpty = new DataColumn(" ", typeof(String));
                                    dt.Columns.Add(dcEmpty);
                                    dcEmpty = new DataColumn("  ", typeof(String));
                                    dt.Columns.Add(dcEmpty);
                                    dcEmpty = new DataColumn("   ", typeof(String));
                                    dt.Columns.Add(dcEmpty);
                                    dcEmpty = new DataColumn("    ", typeof(String));
                                    dt.Columns.Add(dcEmpty);

                                    DataRow drOption = dt.NewRow();
                                    drOption[0] = "Department Option: ";
                                    drOption[1] = option.Name;
                                    dt.Rows.InsertAt(drOption, 0);

                                    DataRow drDepartment = dt.NewRow();
                                    drDepartment[0] = "Department: ";
                                    drDepartment[1] = department.Name;
                                    dt.Rows.InsertAt(drDepartment, 0);

                                    DataRow drLevel = dt.NewRow();
                                    drLevel[0] = "Level: ";
                                    drLevel[1] = level.Name;
                                    dt.Rows.InsertAt(drLevel, 0);

                                    DataRow drProgramme = dt.NewRow();
                                    drProgramme[0] = "Programme: ";
                                    drProgramme[1] = programme.Name;
                                    dt.Rows.InsertAt(drProgramme, 0);

                                    dt.Rows.Add();

                                    DataRow drCols = dt.NewRow();
                                    drCols[0] = "Code";
                                    drCols[1] = "Title";
                                    drCols[2] = "Unit";
                                    drCols[3] = "Semester";
                                    dt.Rows.InsertAt(drCols, 5);

                                    for (int m = 0; m < courses.Count; m++)
                                    {
                                        CourseExport course = courses[m];

                                        DataRow dr = dt.NewRow();

                                        dr[0] = course.Code;
                                        dr[1] = course.Title;
                                        dr[2] = course.Unit;
                                        dr[3] = course.Semester;

                                        dt.Rows.Add(dr);
                                    }

                                    gv.DataSource = dt;
                                    gv.Caption = programme.Name + " " + level.Name + " " + department.Name.Replace("/", "_") + " " + option.Name.Replace("/", "_");
                                    gv.DataBind();

                                    string filename = programme.Name + " " + level.Name + " " + department.Name.Replace("/", "_") + " " + option.Name.Replace("/", "_") + ".xls";

                                    System.IO.StringWriter sw = new System.IO.StringWriter();
                                    System.Web.UI.HtmlTextWriter htw = new System.Web.UI.HtmlTextWriter(sw);

                                    // Render grid view control.
                                    gv.RenderControl(htw);

                                    string path = Server.MapPath(pathToSave);
                                    string savelocation = Path.Combine(path, filename);

                                    // Write the rendered content to a file.
                                    string renderedGridView = sw.ToString();
                                    System.IO.File.WriteAllText(savelocation, renderedGridView);
                                }
                            }
                            else
                            {
                                List<CourseExport> courses = GetCourses(programme, level, department, null);

                                GridView gv = new GridView();
                                DataTable dt = new DataTable();

                                DataColumn dcEmpty = new DataColumn(" ", typeof(String));
                                dt.Columns.Add(dcEmpty);
                                dcEmpty = new DataColumn("  ", typeof(String));
                                dt.Columns.Add(dcEmpty);
                                dcEmpty = new DataColumn("   ", typeof(String));
                                dt.Columns.Add(dcEmpty);
                                dcEmpty = new DataColumn("    ", typeof(String));
                                dt.Columns.Add(dcEmpty);
                                
                                DataRow drDepartment = dt.NewRow();
                                drDepartment[0] = "Department: ";
                                drDepartment[1] = department.Name;
                                dt.Rows.InsertAt(drDepartment, 0);

                                DataRow drLevel = dt.NewRow();
                                drLevel[0] = "Level: ";
                                drLevel[1] = level.Name;
                                dt.Rows.InsertAt(drLevel, 0);

                                DataRow drProgramme = dt.NewRow();
                                drProgramme[0] = "Programme: ";
                                drProgramme[1] = programme.Name;
                                dt.Rows.InsertAt(drProgramme, 0);

                                dt.Rows.Add();

                                DataRow drCols = dt.NewRow();
                                drCols[0] = "Code";
                                drCols[1] = "Title";
                                drCols[2] = "Unit";
                                drCols[3] = "Semester";
                                dt.Rows.InsertAt(drCols, 4);
                                
                                for (int m = 0; m < courses.Count; m++)
                                {
                                    CourseExport course = courses[m];

                                    DataRow dr = dt.NewRow();

                                    dr[0] = course.Code;
                                    dr[1] = course.Title;
                                    dr[2] = course.Unit;
                                    dr[3] = course.Semester;

                                    dt.Rows.Add(dr);
                                }

                                if (department.Name.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0)
                                {
                                    department.Name = departmentLogic.GetModelBy(d => d.Department_Id == department.Id).Code;
                                }
                                
                                gv.DataSource = dt;
                                gv.Caption = programme.Name + " " + level.Name + " " + department.Name.Replace("/", "_");
                                gv.DataBind();

                                string filename = programme.Name + " " + level.Name + " " + department.Name.Replace("/", "_")+ ".xls";

                                System.IO.StringWriter sw = new System.IO.StringWriter();
                                System.Web.UI.HtmlTextWriter htw = new System.Web.UI.HtmlTextWriter(sw);

                                // Render grid view control.
                                gv.RenderControl(htw);

                                string path = Server.MapPath(pathToSave);
                                string savelocation = Path.Combine(path, filename);

                                // Write the rendered content to a file.
                                string renderedGridView = sw.ToString();
                                System.IO.File.WriteAllText(savelocation, renderedGridView);
                            }
                        }
                    }
                }
                using (ZipFile zip = new ZipFile())
                {
                    string file = Server.MapPath(pathToSave);
                    zip.AddDirectory(file, "");
                    string zipFileName = "Departmental Courses";
                    zip.Save(file + zipFileName + ".zip");
                    string export = pathToSave + zipFileName + ".zip";
                    
                    return File(export, "application/zip", zipFileName + ".zip");
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error!" + ex.Message, Message.Category.Error);
            }

            return null;
        }

        private List<CourseExport> GetCourses(Programme programme, Level level, Department department, DepartmentOption option)
        {
            try
            {
                CourseLogic courseLogic = new CourseLogic();
                List<Course> courses = new List<Course>();
                List<CourseExport> courseExports = new List<CourseExport>();

                if (option != null)
                    courses = courseLogic.GetModelsBy(c => c.Programme_Id == programme.Id && c.Department_Id == department.Id && c.Level_Id == level.Id && c.Department_Option_Id == option.Id && c.Activated == true);
                else
                    courses = courseLogic.GetModelsBy(c => c.Programme_Id == programme.Id && c.Department_Id == department.Id && c.Level_Id == level.Id && c.Activated == true);

                for (int i = 0; i < courses.Count; i++)
                {
                    CourseExport courseExport = new CourseExport();
                    courseExport.Code = courses[i].Code;
                    courseExport.Semester = courses[i].Semester.Name;
                    courseExport.Title = courses[i].Name;
                    courseExport.Unit = courses[i].Unit;

                    courseExports.Add(courseExport);
                }

                return courseExports.OrderBy(s => s.Semester).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}