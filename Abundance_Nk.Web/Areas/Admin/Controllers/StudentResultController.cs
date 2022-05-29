using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Abundance_Nk.Business;
using Abundance_Nk.Model.Entity.Model;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Web.Areas.Admin.ViewModels;
using Abundance_Nk.Web.Controllers;
using Abundance_Nk.Web.Models;
using Menu = Abundance_Nk.Model.Model.Menu;

namespace Abundance_Nk.Web.Areas.Admin.Controllers
{
    public class StudentResultController : BaseController
    {
        private StudentResultViewModel viewModel;
        public ActionResult CreateStudentResultStatus()
        {
            try
            {
                viewModel = new StudentResultViewModel();
                PopulateAllDropDown(viewModel);
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult CreateStudentResultStatus(StudentResultViewModel viewModel)
        {
            try
            {
                if (viewModel != null)
                {
                    LevelLogic levelLogic = new LevelLogic();
                    ProgrammeLogic programmeLogic = new ProgrammeLogic();
                    SessionLogic sessionLogic = new SessionLogic();
                    SemesterLogic semesterLogic = new SemesterLogic();

                    Level level = levelLogic.GetModelBy(l => l.Level_Id == viewModel.Level.Id);
                    Programme programme = programmeLogic.GetModelBy(p => p.Programme_Id == viewModel.Programme.Id);
                    Session session = sessionLogic.GetModelBy(s => s.Session_Id == viewModel.Session.Id);
                    Semester semester = semesterLogic.GetModelBy(s => s.Semester_Id == viewModel.Semester.Id);

                    List<Department> departments = Utility.GetDepartmentByProgramme(programme);
                    Department castedDepartment = departments.Where(d => d.Name == "-- Select Department --").FirstOrDefault();
                    departments.Remove(castedDepartment);

                    viewModel.Programme = programme;
                    viewModel.Level = level;
                    viewModel.Session = session;
                    viewModel.Semester = semester;

                    List<StudentResultStatusFormat> studentResultStatusFormats = new List<StudentResultStatusFormat>();
                    for (int i = 0; i < departments.Count; i++)
                    {
                        StudentResultStatusFormat studentResultStatusFormat = new StudentResultStatusFormat();
                        studentResultStatusFormat.Department = departments[i];
                        studentResultStatusFormat.Approved = false;

                        studentResultStatusFormats.Add(studentResultStatusFormat);
                    }


                    viewModel.StudentResultStatusFormats = studentResultStatusFormats;

                    RetainDropDown(viewModel);
                    return View(viewModel);
                }

            }
            catch (Exception ex)
            {
                SetMessage("Error!" + ex.Message, Message.Category.Error);
            }

            RetainDropDown(viewModel);
            return View(viewModel);
        }

        //public ActionResult SaveStudentResultStatus(StudentResultViewModel viewModel)
        //{
        //    try
        //    {
        //        if (viewModel != null)
        //        {
        //            Programme programme = viewModel.Programme;
        //            Level level = viewModel.Level;

        //            StudentResultStatusLogic studentResultStatusLogic = new StudentResultStatusLogic();

        //            for (int i = 0; i < viewModel.StudentResultStatusFormats.Count; i++)
        //            {
        //                StudentResultStatusFormat currentStudentResultStatusFormat = viewModel.StudentResultStatusFormats[i];

        //                List<StudentResultStatus> studentResultStatusList = studentResultStatusLogic.GetModelsBy(sr => sr.Department_Id == currentStudentResultStatusFormat.Department.Id && sr.Programme_Id == programme.Id && sr.Level_Id == level.Id && sr.Session_Id == viewModel.Session.Id);
        //                if (studentResultStatusList.Count == 0)
        //                {
        //                    if (currentStudentResultStatusFormat.Department.Id != 0)
        //                    {
        //                        StudentResultStatus studentResultStatus = new StudentResultStatus();
        //                        studentResultStatus.Programme = programme;
        //                        studentResultStatus.Level = level;
        //                        studentResultStatus.Session = viewModel.Session;
        //                        studentResultStatus.Department = currentStudentResultStatusFormat.Department;
        //                        studentResultStatus.Activated = currentStudentResultStatusFormat.Approved;

        //                        studentResultStatusLogic.Create(studentResultStatus); 
        //                    } 
                            
        //                }
        //            }

        //            SetMessage("Operation Successful! ", Message.Category.Information);
        //            return RedirectToAction("CreateStudentResultStatus");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        SetMessage("Error!" + ex.Message, Message.Category.Error);
        //    }

        //    return RedirectToAction("CreateStudentResultStatus");
        //}
        public ActionResult ViewStudentResultStatus()
        {
            try
            {
                viewModel = new StudentResultViewModel();
                PopulateAllDropDown(viewModel);
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult ViewStudentResultStatus(StudentResultViewModel viewModel)
        {
            try
            {
                if (viewModel.Session != null && viewModel.Session.Id > 0 )
                {
                    StudentResultStatusLogic studentResultStatusLogic = new StudentResultStatusLogic();
                    FacultyLogic facultyLogic = new FacultyLogic();

                    viewModel.StudentResultStatusList = studentResultStatusLogic.GetModelsBy(m => m.Session_Id == viewModel.Session.Id && m.Programme_Id == viewModel.Programme.Id && m.Semester_Id == viewModel.Semester.Id);

                    if (viewModel.StudentResultStatusList == null || viewModel.StudentResultStatusList.Count <= 0)
                    {
                        viewModel.StudentResultStatusList = new List<StudentResultStatus>();

                        viewModel.Faculties = facultyLogic.GetAll();
                        for (int i = 0; i < viewModel.Faculties.Count; i++)
                        {
                            Faculty faculty = viewModel.Faculties[i];
                            StudentResultStatus resultStatus = new StudentResultStatus();
                            resultStatus.Faculty = faculty;
                            resultStatus.Programme = new Programme() { Id = viewModel.Programme.Id };
                            resultStatus.Session = viewModel.Session;
                            resultStatus.Semester = viewModel.Semester;
                            resultStatus.RAndDCApproval = false;
                            resultStatus.DRAcademicsApproval = false;
                            resultStatus.RegistrarApproval = false;

                            viewModel.StudentResultStatusList.Add(resultStatus);

                            studentResultStatusLogic.Create(resultStatus);

                        }
                    }

                    RetainDropDown(viewModel);
                    return View(viewModel);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            RetainDropDown(viewModel);
            return View(viewModel);
        }
        public JsonResult ApproveResult(int sessionId, int facultyId, int programmeId, int semesterId)
        {
            JsonResultModel result = new JsonResultModel();
            try
            {
                if (sessionId <= 0 || facultyId <= 0)
                {
                    result.IsError = true;
                    result.Message = "Parameter not Set!";

                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                bool isAuthorized = false;

                if (User.IsInRole("R&DC"))
                {
                    isAuthorized = true;
                }
                if (User.IsInRole("Registrar"))
                {
                    isAuthorized = true;
                }
                if (User.IsInRole("DRAcademics"))
                {
                    isAuthorized = true;
                }

                if (!isAuthorized)
                {
                    result.IsError = true;
                    result.Message = "You are not authorized to approve result.";

                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                StudentResultStatusLogic resultStatusLogic = new StudentResultStatusLogic();
                GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();
                UserLogic userLogic = new UserLogic();

                User user = userLogic.GetModelBy(p => p.User_Name == User.Identity.Name);
                string client = Request.LogonUserIdentity.Name + " (" + HttpContext.Request.UserHostAddress + ")";

                List<StudentResultStatus> studentResultStatuses = resultStatusLogic.GetModelsBy(r => r.Session_Id == sessionId && r.Programme_Id == programmeId && r.Faculty_Id == facultyId && r.Semester_Id == semesterId);
                for (int i = 0; i < studentResultStatuses.Count; i++)
                {
                    StudentResultStatus resultStatus = studentResultStatuses[i];
                    GeneralAudit generalAudit = new GeneralAudit();
                    string role = "";

                    if (User.IsInRole("R&DC"))
                    {
                        resultStatus.RAndDCApproval = true;
                        role = "R&DC";
                    }
                    else if (User.IsInRole("Registrar"))
                    {
                        resultStatus.RegistrarApproval = true;
                        role = "Registrar";
                    }
                    else if (User.IsInRole("DRAcademics"))
                    {
                        resultStatus.DRAcademicsApproval = true;
                        role = "DR Academics";
                    }

                    resultStatusLogic.Modify(resultStatus);


                    generalAudit.Action = "MODIFY";
                    generalAudit.Client = client;
                    generalAudit.CurrentValues = "-";
                    generalAudit.InitialValues = "-";
                    generalAudit.Operation = "Aprroved result from " + role + " account for "+ resultStatus.Programme.Name + " " + resultStatus.Faculty.Name + " " + resultStatus.Session.Name + "Session" ;
                    generalAudit.TableNames = "STUDENT RESULT STATUS";
                    generalAudit.Time = DateTime.Now;
                    generalAudit.User = user;

                    generalAuditLogic.Create(generalAudit);
                }

                result.IsError = false;
                result.Message = "Operation Successful!";

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Message = ex.Message;

                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult CancelResult(int sessionId, int facultyId, int programmeId, int semesterId)
        {
            JsonResultModel result = new JsonResultModel();
            try
            {
                if (sessionId <= 0 || facultyId <= 0)
                {
                    result.IsError = true;
                    result.Message = "Parameter not Set!";

                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                bool isAuthorized = false;

                if (User.IsInRole("R&DC"))
                {
                    isAuthorized = true;
                }
                if (User.IsInRole("Registrar"))
                {
                    isAuthorized = true;
                }
                if (User.IsInRole("DRAcademics"))
                {
                    isAuthorized = true;
                }

                if (!isAuthorized)
                {
                    result.IsError = true;
                    result.Message = "You are not authorized to approve result.";

                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                StudentResultStatusLogic resultStatusLogic = new StudentResultStatusLogic();
                GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();
                UserLogic userLogic = new UserLogic();

                User user = userLogic.GetModelBy(p => p.User_Name == User.Identity.Name);
                string client = Request.LogonUserIdentity.Name + " (" + HttpContext.Request.UserHostAddress + ")";

                List<StudentResultStatus> studentResultStatuses = resultStatusLogic.GetModelsBy(r => r.Session_Id == sessionId && r.Programme_Id == programmeId && r.Faculty_Id == facultyId && r.Semester_Id == semesterId);
                for (int i = 0; i < studentResultStatuses.Count; i++)
                {
                    StudentResultStatus resultStatus = studentResultStatuses[i];
                    GeneralAudit generalAudit = new GeneralAudit();
                    string role = "";

                    if (User.IsInRole("R&DC"))
                    {
                        resultStatus.RAndDCApproval = false;
                        role = "R&DC";
                    }
                    else if (User.IsInRole("Registrar"))
                    {
                        resultStatus.RegistrarApproval = false;
                        role = "Registrar";
                    }
                    else if (User.IsInRole("DRAcademics"))
                    {
                        resultStatus.DRAcademicsApproval = false;
                        role = "DR Academics";
                    }

                    resultStatusLogic.Modify(resultStatus);


                    generalAudit.Action = "MODIFY";
                    generalAudit.Client = client;
                    generalAudit.CurrentValues = "-";
                    generalAudit.InitialValues = "-";
                    generalAudit.Operation = "Canceled result from " + role + " account for " + resultStatus.Faculty.Name + " " + resultStatus.Session.Name + "Session";
                    generalAudit.TableNames = "STUDENT RESULT STATUS";
                    generalAudit.Time = DateTime.Now;
                    generalAudit.User = user;

                    generalAuditLogic.Create(generalAudit);
                }

                result.IsError = false;
                result.Message = "Operation Successful!";

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Message = ex.Message;

                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        //[HttpPost]
        //public ActionResult SaveEditedStudentResultStatus(StudentResultViewModel viewModel)
        //{
        //    try
        //    {
        //        StudentResultStatusLogic studentResultStatusLogic = new StudentResultStatusLogic();
                
        //        Programme programme = viewModel.Programme;
        //        Level level = viewModel.Level;

        //        for (int i = 0; i < viewModel.StudentResultStatusList.Count; i++)
        //        {   
        //            StudentResultStatus currStudentResultStatus = viewModel.StudentResultStatusList[i];
        //            if (currStudentResultStatus.Department != null && currStudentResultStatus.Department.Id != 0)
        //            {
        //                currStudentResultStatus.Programme = programme;
        //                currStudentResultStatus.Level = level;
        //                currStudentResultStatus.Session = viewModel.Session;
        //                currStudentResultStatus.Department = currStudentResultStatus.Department;
        //                currStudentResultStatus.Activated = currStudentResultStatus.Activated;

        //                studentResultStatusLogic.Modify(currStudentResultStatus);
        //            }  
        //        }
                
        //        SetMessage("Operation Successful!", Message.Category.Information);
        //        return RedirectToAction("ViewStudentResultStatus");
        //    }
        //    catch (Exception ex)
        //    {
        //        SetMessage("Error! " + ex.Message, Message.Category.Error);
        //    }

        //    RetainDropDown(viewModel);
        //    return View("ViewStudentResultStatus", viewModel);
        //}

        //public ActionResult ConfirmDeleteResultStatus(int rid)
        //{
        //    try
        //    {
        //        viewModel = new StudentResultViewModel();
        //        if (rid > 0)
        //        {
        //            StudentResultStatusLogic studentResultStatusLogic = new StudentResultStatusLogic();
        //            viewModel.StudentResultStatus = studentResultStatusLogic.GetModelBy(x => x.Id == rid);
        //            if (viewModel.StudentResultStatus != null)
        //            {
        //                viewModel.Programme = viewModel.StudentResultStatus.Programme;
        //                viewModel.Level = viewModel.StudentResultStatus.Level;
        //                viewModel.Session = viewModel.StudentResultStatus.Session;
        //            }

        //            RetainDropDown(viewModel);
        //            return View(viewModel);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        SetMessage("Error! " + ex.Message, Message.Category.Error);
        //    }

        //    RetainDropDown(viewModel);
        //    return View(viewModel);
        //}

        //[HttpPost]
        //public ActionResult DeleteResultStatus(StudentResultViewModel viewModel)
        //{
        //    try
        //    {
        //        StudentResultStatusLogic studentResultStatusLogic = new StudentResultStatusLogic();
        //        studentResultStatusLogic.Delete(x => x.Id == viewModel.StudentResultStatus.Id);

        //        SetMessage("Operation Successful!", Message.Category.Information);
        //        return RedirectToAction("ViewStudentResultStatus");

        //    }
        //    catch (Exception ex)
        //    {
        //        SetMessage("Error! " + ex.Message, Message.Category.Error);
        //    }

        //    return RedirectToAction("ConfirmDeleteResultStatus", new { rid = viewModel.StudentResultStatus.Id });
        //}

        public void PopulateAllDropDown(StudentResultViewModel viewModel)
        {
            try
            {
                ViewBag.Programme = viewModel.ProgrammeSelectListItem;
                ViewBag.Level = viewModel.LevelSelectListItem;
                ViewBag.Session = viewModel.SessionSelectListItem;
                ViewBag.Semester = viewModel.SemesterSelectListItem;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void RetainDropDown(StudentResultViewModel viewModel)
        {
            try
            {
                if (viewModel.Programme != null)
                {
                    ViewBag.Programme = new SelectList(viewModel.ProgrammeSelectListItem, "Value", "Text", viewModel.Programme.Id);
                }
                else
                {
                    ViewBag.Programme = viewModel.ProgrammeSelectListItem;
                }
                if (viewModel.Level != null)
                {
                    ViewBag.Level = new SelectList(viewModel.LevelSelectListItem, "Value", "Text", viewModel.Level.Id);
                }
                else
                {
                    ViewBag.Level = viewModel.LevelSelectListItem;
                }
                if (viewModel.Session != null)
                {
                    ViewBag.Session = new SelectList(viewModel.SessionSelectListItem, "Value", "Text", viewModel.Session.Id);
                }
                else
                {
                    ViewBag.Session = viewModel.SessionSelectListItem;
                }

                if (viewModel.Semester != null)
                {
                    ViewBag.Semester = new SelectList(viewModel.SemesterSelectListItem, "Value", "Text", viewModel.Semester.Id);
                }
                else
                {
                    ViewBag.Semester = viewModel.SemesterSelectListItem;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}