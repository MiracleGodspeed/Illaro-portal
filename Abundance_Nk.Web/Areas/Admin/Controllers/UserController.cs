using System.EnterpriseServices.Internal;
using Abundance_Nk.Web.Areas.Admin.ViewModels;
using Abundance_Nk.Business;
using Abundance_Nk.Web.Controllers;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using Abundance_Nk.Model.Entity.Model;
using System.Web.Script.Serialization;
using System.IO;

namespace Abundance_Nk.Web.Areas.Admin.Controllers
{
    public class UserController : BaseController
    {
        private UserViewModel viewModel;
        private User user;
        private Person person;
        private PersonType personType;
        private UserLogic userLogic;
        private PersonLogic personLogic;
        private PersonTypeLogic personTypeLogic;
        public UserController()
        {
            viewModel = new UserViewModel();
            user = new User();
            person = new Person();
            personType = new PersonType();
            personTypeLogic = new PersonTypeLogic();
            userLogic = new UserLogic();
            personLogic = new PersonLogic();
        }


       
        public ActionResult Index()
        {
            try
            {
                List<User> userList = new List<User>();
                userList = userLogic.GetModelsBy(p => (p.Role_Id == 8 || p.Role_Id == 10 || p.Role_Id == 12) && p.Archive==false);
                viewModel.Users = userList;

                if (!User.IsInRole("Admin"))
                {
                    viewModel.Users = viewModel.Users.Where(u => u.Role.Id == 10).ToList();
                }
            }
            catch (Exception e)
            {
                SetMessage("Error Occured " + e.Message, Message.Category.Error);
            }
            return View(viewModel);
        }

        [Authorize(Roles = "Admin,Sub-Admin,lloydant")]
        public ActionResult CreateUser()
        {
            
                ViewBag.SexList = viewModel.SexSelectList;
                ViewBag.RoleList = viewModel.RoleSelectList;
                ViewBag.SecurityQuestionList = viewModel.SecurityQuestionSelectList;
            
            return View();
        }
        [HttpPost]
        public ActionResult CreateUser(UserViewModel viewModel)
        {
            try
            {
                if (viewModel != null)
                {
                    List<User> users = new List<User>();
                    UserLogic userLogic = new UserLogic();
                    users = userLogic.GetModelsBy(p => p.User_Name == viewModel.User.Username);
                    if (users.Count > 0)
                    {
                        SetMessage("Error: Staff with this Name already exist", Message.Category.Error);
                        return RedirectToAction("CreateUser");
                    }

                    //Role role = new Role() { Id = 8 };
                    //viewModel.User.Role = role;
                    viewModel.User.LastLoginDate = DateTime.Now;
                    viewModel.User.Activated = true;
                    userLogic.Create(viewModel.User);
                    GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();

                    string Action = "CREATED";
                    string Operation = "Created the user  " + viewModel.User.Username;
                    string Table = "User Table";
                    generalAuditLogic.CreateGeneralAudit(Action, Operation, Table);

                   

                    SetMessage("User Created Succesfully", Message.Category.Information);
                    return RedirectToAction("Index", "User");
                }
                else
                {
                    SetMessage("Input is null", Message.Category.Error);
                    return RedirectToAction("CreateUser");
                }
                
            }
            catch (Exception e)
            {
                SetMessage("Error Occured " + e.Message, Message.Category.Error);
            }

            ViewBag.SexList = viewModel.SexSelectList;
            ViewBag.RoleList = viewModel.RoleSelectList;
            ViewBag.SecurityQuestionList = viewModel.SecurityQuestionSelectList;
            return View("CreateUser",viewModel);
        }
        public ActionResult GetUserDetails()
        {     
            return View();
        }
        [HttpPost]
        public ActionResult GetUserDetails(UserViewModel viewModel)
        {
            try
            {
                if (viewModel.User.Username != null)
                {
                    userLogic = new UserLogic();
                    List<User> users = userLogic.GetModelsBy(u => u.User_Name == viewModel.User.Username);
                    if (users.Count > 1)
                    {
                        SetMessage("There are more than one user with this username!", Message.Category.Error);
                        return View(viewModel);
                    }
                    if (users.Count == 0)
                    {
                        SetMessage("There is no user with this username!", Message.Category.Error);
                        return View(viewModel);
                    }

                    Model.Model.User user = users.FirstOrDefault();

                    if (user.Role.Id != 10 && !User.IsInRole("Admin"))
                    {
                        return View();
                    }

                    return RedirectToAction("ViewUserDetails", new { @id = user.Id });
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occured " + ex.Message, Message.Category.Error);
            }

            return View();
        }
        public ActionResult ViewUserDetails(int? id)
        {
            try
            {
                viewModel = null;
                if (id != null)
                {
                    user = userLogic.GetModelBy(p => p.User_Id == id);
                    viewModel = new UserViewModel();
                    viewModel.User = user;
                    return View(viewModel);
                }
                else
                {
                    SetMessage("Error Occured: Select a User", Message.Category.Error);
                    return RedirectToAction("Index");
               }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occured " + ex.Message, Message.Category.Error);
            }
            return View();
        }
        public ActionResult EditUser(int? id)
        {
            viewModel = null;
            try
            {
                if (id != null)
                {
                    TempData["userId"] = id;
                    user = userLogic.GetModelBy(p => p.User_Id == id);
                    viewModel = new UserViewModel();
                    if (user.Activated != true)
                    {
                        user.ActivatedCheck = false;
                    }
                    else
                    {
                        user.ActivatedCheck = true;
                    }

                    viewModel.User = user;
                }
                else
                {
                    SetMessage("Select a User", Message.Category.Error);
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occured " + ex.Message, Message.Category.Error);
            }

            ViewBag.RoleList = viewModel.RoleSelectList;
            ViewBag.SecurityQuestionList = viewModel.SecurityQuestionSelectList;
            return View(viewModel);
        }
        [HttpPost]
        public ActionResult EditUser(UserViewModel viewModel)
        {
            try
            {
                if (viewModel != null)
                {
                    //Role role = new Role() { Id = 8 };
                    //viewModel.User.Role = role;

                    foreach (string file in Request.Files)
                    {
                        HttpPostedFileBase hpf = Request.Files[file];
                        if (hpf.ContentLength == 0)
                        {
                            continue;
                        }
                        FileInfo fileInfo = new FileInfo(hpf.FileName);
                        string pathForSaving = Server.MapPath("~/Content/User/SignatureImage");
                        string fileName = (int)TempData["userId"] + "__" + fileInfo.Name;

                        if (CreateFolderIfNeeded(pathForSaving))
                        {
                            DeleteFileIfExist(pathForSaving, fileName);
                            string savedFileName = Path.Combine(pathForSaving, fileName);
                            hpf.SaveAs(savedFileName);
                            viewModel.User.SignatureUrl = "/Content/User/SignatureImage/" + fileName;
                        }
                    }

                    if (viewModel.User.ActivatedCheck)
                    {
                        viewModel.User.Activated = true;
                    }
                    else
                    {
                        viewModel.User.Activated = false;
                    }

                    viewModel.User.Id = (int)TempData["userId"];
                    userLogic.Update(viewModel.User);
                    GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();

                    string Action = "MODIFY";
                    string Operation = "Modified the user  " + viewModel.User.Username;
                    string Table = "User Table";
                    generalAuditLogic.CreateGeneralAudit(Action, Operation, Table);

                    SetMessage("User Edited Successfully", Message.Category.Information);
                }
                else
                {
                    SetMessage("Input is null", Message.Category.Warning);
                    return RedirectToAction("EditUser");
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occured " + ex.Message, Message.Category.Error);
            }
            return RedirectToAction("Index");
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
        public ActionResult EditUserRole()
        {
            try
            {                
                viewModel = new UserViewModel();
                ViewBag.Role = viewModel.RoleSelectList;
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult EditUserRole(UserViewModel viewModel)
        {
            try
            {
                if (viewModel.User.Role != null)
                {
                    UserLogic userLogic = new UserLogic();
                    StaffLogic staffLogic = new StaffLogic();
                    viewModel.Users = userLogic.GetModelsBy(u => u.Role_Id == viewModel.User.Role.Id && u.Archive == false);

                    if (!User.IsInRole("Admin"))
                    {
                        viewModel.Users = viewModel.Users.Where(u => u.Role.Id == 10 ).ToList();
                    }

                    //viewModel.Users.ForEach(u =>
                    //{
                    //    u.Department = staffLogic.GetStaffDepartment(u);
                    //});
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            ViewBag.Role = viewModel.RoleSelectList;
            return View(viewModel);
        }
        public ActionResult EditRole(string id)
        {
            try
            {
                int userId = Convert.ToInt32(id);
                UserLogic userLogic = new UserLogic();

                viewModel = new UserViewModel();
                viewModel.User = userLogic.GetModelBy(u => u.User_Id == userId);

                ViewBag.Role = new SelectList(viewModel.RoleSelectList, "Value", "Text", viewModel.User.Role.Id);
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult EditRole(UserViewModel viewModel)
        {
            try
            {
                if (viewModel.User != null)
                {
                    UserLogic userLogic = new UserLogic();
                    bool isUserModified = userLogic.Modify(viewModel.User);

                    if (isUserModified)
                    {
                        GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();

                        string Action = "MODIFY";
                        string Operation = "Modified the user Role for " + viewModel.User.Username;
                        string Table = "User Table";
                        generalAuditLogic.CreateGeneralAudit(Action, Operation, Table);


                        SetMessage("Operation Successful!", Message.Category.Information);
                        return RedirectToAction("EditRoleByUserName"); 
                    }
                    else
                    {
                        SetMessage("No item was Modified", Message.Category.Information);
                        return RedirectToAction("EditRoleByUserName");
                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return RedirectToAction("EditRoleByUserName");
        }
        public ActionResult EditRoleByUserName()
        {
            try
            {
                viewModel = new UserViewModel();
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult EditRoleByUserName(UserViewModel viewModel)
        {
            try
            {
                if (viewModel.User.Username != null)
                {
                    UserLogic userLogic = new UserLogic();
                    User user = userLogic.GetModelBy(u => u.User_Name == viewModel.User.Username);
                    viewModel.User = user;
                    if (user == null)
                    {
                        SetMessage("User does not exist!", Message.Category.Error);
                        return View(viewModel);
                    }

                    if (user.Role.Id != 10 && !User.IsInRole("Admin"))
                    {
                        viewModel.User = null;
                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            ViewBag.Role = viewModel.RoleSelectList;
            return View(viewModel);
        }
        public ActionResult ChangeCourseStaffRole()
        {
            try
            {
                viewModel = new UserViewModel();
                ViewBag.Departments = viewModel.DepartmentSelectList;
                ViewBag.CurrentSession = viewModel.CurrentSessionSelectList;

            }
            catch (Exception ex)
            {
                 SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult ChangeCourseStaffRole(UserViewModel viewModel)
        {
            try
            {
                if (viewModel.User.Username != null)
                {
                    UserLogic userLogic = new UserLogic();
                    CourseAllocationLogic courseAllocationLogic = new CourseAllocationLogic();
                    StaffDepartmentLogic staffDepartmentLogic = new StaffDepartmentLogic();
                    StaffLogic staffLogic = new StaffLogic();
                    SessionSemesterLogic sessionSemesterLogic = new SessionSemesterLogic();
                    CourseAllocation courseAllocation = new CourseAllocation();

                    User user = userLogic.GetModelBy(u => u.User_Name == viewModel.User.Username);
                    if (user == null)
                    {
                        SetMessage("User Does Not Exist", Message.Category.Error);
                        ViewBag.Departments = viewModel.DepartmentSelectList;
                        ViewBag.CurrentSession = viewModel.CurrentSessionSelectList;
                        return View(viewModel);
                    }

                    courseAllocation = courseAllocationLogic.GetModelsBy(ca => ca.User_Id == user.Id && ca.Session_Id == viewModel.Session.Id).LastOrDefault();
                    if (courseAllocation == null)
                    {
                        Staff staff = staffLogic.GetModelsBy(s => s.User_Id == user.Id).LastOrDefault();
                        if (staff != null)
                        {
                            StaffDepartment staffDepartment = staffDepartmentLogic.GetModelsBy(s => s.Staff_Id == staff.Id).LastOrDefault();
                            if (staffDepartment != null)
                            {
                                StaffDepartment existingStaffDepartmentHOD = staffDepartmentLogic.GetModelsBy(s => s.IsHead && s.Department_Id == viewModel.Department.Id && s.SESSION_SEMESTER.Session_Id == viewModel.Session.Id && s.Staff_Id != staff.Id).LastOrDefault();
                                if (existingStaffDepartmentHOD != null)
                                {
                                    SetMessage("Another person has been set as the HOD for this department this session!", Message.Category.Error);
                                    ViewBag.Departments = viewModel.DepartmentSelectList;
                                    ViewBag.CurrentSession = viewModel.CurrentSessionSelectList;
                                    return RedirectToAction("ChangeCourseStaffRole");
                                }

                                if (!viewModel.RemoveHOD)
                                {
                                    staffDepartment.IsHead = true;
                                    staffDepartment.SessionSemester= sessionSemesterLogic.GetModelsBy(s => s.Session_Id == viewModel.Session.Id).LastOrDefault();
                                    staffDepartment.Department = viewModel.Department;
                                    staffDepartmentLogic.Modify(staffDepartment);
                                }
                                else
                                {
                                    staffDepartment.IsHead = false;
                                    staffDepartmentLogic.Modify(staffDepartment);
                                }

                                SetMessage("Operation Successful!", Message.Category.Information);
                                ViewBag.Departments = viewModel.DepartmentSelectList;
                                ViewBag.CurrentSession = viewModel.CurrentSessionSelectList;
                                return RedirectToAction("ChangeCourseStaffRole");
                            }
                            else if (viewModel.Department != null)
                            {
                                StaffDepartment existingStaffDepartmentHOD = staffDepartmentLogic.GetModelsBy(s => s.IsHead && s.Department_Id == viewModel.Department.Id && s.SESSION_SEMESTER.Session_Id == viewModel.Session.Id && s.Staff_Id != staff.Id).LastOrDefault();
                                if (existingStaffDepartmentHOD != null)
                                {
                                    SetMessage("Another person has been set as the HOD for this department this session!", Message.Category.Error);
                                    ViewBag.Departments = viewModel.DepartmentSelectList;
                                    ViewBag.CurrentSession = viewModel.CurrentSessionSelectList;
                                    return RedirectToAction("ChangeCourseStaffRole");
                                }


                                staffDepartment = new StaffDepartment();
                                staffDepartment.DateEntered = DateTime.Now;
                                staffDepartment.Department = viewModel.Department;
                                staffDepartment.IsHead = true;
                                staffDepartment.SessionSemester = sessionSemesterLogic.GetModelsBy(s => s.Session_Id == viewModel.Session.Id).LastOrDefault();
                                staffDepartment.Staff = staff;

                                staffDepartmentLogic.Create(staffDepartment);

                                SetMessage("Operation Successful!", Message.Category.Information);
                                ViewBag.Departments = viewModel.DepartmentSelectList;
                                ViewBag.CurrentSession = viewModel.CurrentSessionSelectList;
                                return RedirectToAction("ChangeCourseStaffRole");
                            }
                            else
                            {
                                SetMessage("Staff has not been allocated any course", Message.Category.Error);
                                ViewBag.Departments = viewModel.DepartmentSelectList;
                                ViewBag.CurrentSession = viewModel.CurrentSessionSelectList;
                                return View(viewModel);
                            }
                        }
                        else
                        {
                            SetMessage("User has not filled staff profile!", Message.Category.Error);
                            ViewBag.Departments = viewModel.DepartmentSelectList;
                            ViewBag.CurrentSession = viewModel.CurrentSessionSelectList;
                            return View(viewModel);
                        }
                    }

                    viewModel.CourseAllocation = courseAllocation;
                    viewModel.User = user;
                    RetainDropDownState(viewModel);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            ViewBag.Departments = viewModel.DepartmentSelectList;
            ViewBag.CurrentSession = viewModel.CurrentSessionSelectList;
            return View(viewModel); 
        }
        public ActionResult SaveCourseStaffRole(UserViewModel viewModel)
        {
            try
            {
                UserLogic userLogic = new UserLogic();
                CourseAllocationLogic courseAllocationLogic = new CourseAllocationLogic();
                User user = new User();
                List<CourseAllocation> courseAllocations = new List<CourseAllocation>();

                user = userLogic.GetModelBy(u => u.User_Id == viewModel.User.Id);
                courseAllocations = courseAllocationLogic.GetModelsBy(ca => ca.User_Id == user.Id);
                //if (courseAllocations.FirstOrDefault().HodDepartment.Id != viewModel.CourseAllocation.HodDepartment.Id || courseAllocations.FirstOrDefault().Programme.Id != viewModel.CourseAllocation.Programme.Id || courseAllocations.FirstOrDefault().Level.Id != viewModel.CourseAllocation.Level.Id || courseAllocations.FirstOrDefault().Session.Id != viewModel.CourseAllocation.Session.Id || courseAllocations.FirstOrDefault().Semester.Id != viewModel.CourseAllocation.Semester.Id)
                //{
                //    SetMessage("The User has not been allocated any course in this Programme, Department, Level, Session and Semester", Message.Category.Error); 
                //    RetainDropDownState(viewModel);
                //    return View("ChangeCourseStaffRole");
                //}
                using (TransactionScope scope = new TransactionScope())
                {
                    user.Role = viewModel.User.Role;
                    userLogic.Modify(user);

                    for (int i = 0; i < courseAllocations.Count; i++)
                    {
                        if (user.Role.Id != 12)
                        {
                            courseAllocations[i].HodDepartment = null;
                            courseAllocations[i].IsHOD = null;
                            courseAllocationLogic.Modify(courseAllocations[i]);
                        }
                        else
                        {
                            courseAllocations[i].HodDepartment = viewModel.CourseAllocation.HodDepartment;
                            courseAllocations[i].IsHOD = true;
                            courseAllocationLogic.Modify(courseAllocations[i]);
                        }
                    }

                    scope.Complete();
                    SetMessage("Operation Successful!", Message.Category.Information);
                }
            }
            catch (Exception)
            {
                throw;
            }

            return RedirectToAction("ChangeCourseStaffRole");
        }
        public void RetainDropDownState(UserViewModel viewModel)
        {
            try
            {
                if (viewModel.CourseAllocation != null)
                {
                    if (viewModel.CourseAllocation.Programme != null)
                    {
                        ViewBag.Programme = new SelectList(viewModel.ProgrammeSelectList, "Value", "Text", viewModel.CourseAllocation.Programme.Id);
                    }
                    else
                    {
                        ViewBag.Programme = viewModel.ProgrammeSelectList;
                    }

                    if (viewModel.CourseAllocation.Department != null && viewModel.CourseAllocation.Programme != null)
                    {
                        DepartmentLogic departmentLogic = new DepartmentLogic();
                        ViewBag.Department = new SelectList(departmentLogic.GetBy(viewModel.CourseAllocation.Programme), "Id", "Name", viewModel.CourseAllocation.Department.Id);
                    }
                    else
                    {
                        ViewBag.Department = new SelectList(new List<Department>(), "Id", "Name");
                    }

                    if (viewModel.CourseAllocation.Semester != null && viewModel.CourseAllocation.Session != null)
                    {
                        SessionSemesterLogic sessionSemesterLogic = new SessionSemesterLogic();
                        List<SessionSemester> sessionSemesterList = sessionSemesterLogic.GetModelsBy(p => p.Session_Id == viewModel.CourseAllocation.Session.Id);

                        List<Semester> semesters = new List<Semester>();
                        foreach (SessionSemester sessionSemester in sessionSemesterList)
                        {
                            semesters.Add(sessionSemester.Semester);
                        }

                        ViewBag.Semester = new SelectList(semesters, "Id", "Name", viewModel.CourseAllocation.Session.Id);
                    }
                    else
                    {
                        ViewBag.Semester = new SelectList(new List<Semester>(), "Id", "Name");
                    }

                    if (viewModel.CourseAllocation.Session != null)
                    {
                        ViewBag.Session = new SelectList(viewModel.SessionSelectList, "Value", "Text", viewModel.CourseAllocation.Session.Id);
                    }
                    else
                    {
                        ViewBag.Session = viewModel.SessionSelectList;
                    }

                    if (viewModel.CourseAllocation.Level != null)
                    {
                        ViewBag.Level = new SelectList(viewModel.LevelSelectList, "Value", "Text", viewModel.CourseAllocation.Level.Id);
                    }
                    else
                    {
                        ViewBag.Level = viewModel.LevelSelectList;
                    }
                }
                if (viewModel.User != null)
                {
                    if (viewModel.User.Role != null)
                    {
                        ViewBag.Role = new SelectList(viewModel.RoleSelectList, "Value", "Text", viewModel.User.Role.Id);
                    }
                    else
                    {
                        ViewBag.Role = viewModel.RoleSelectList;
                    }
                }
            }
            catch (Exception)
            {   
                throw;
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

                return Json(new SelectList(departments, "Id", "Name"), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
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
        [Authorize(Roles = "Admin,Sub-Admin,lloydant")]
        public ActionResult DeleteStaff()
        {
            try
            {
                viewModel = new UserViewModel();
            }
            catch (Exception ex)
            {
                SetMessage("Error" + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult DeleteStaff(UserViewModel viewModel)
        {
            try
            {
                UserLogic userLogic = new UserLogic();
                List<User> users = new List<User>();
                if (viewModel.User != null)
                {

                    users = userLogic.GetModelsBy(x => x.User_Name.Contains(viewModel.User.Username) && x.Role_Id != 1).OrderBy(x => x.Username).ToList();
                    viewModel.Users = users;
                }
                
            }
            catch (Exception ex)
            {
                SetMessage("Error" + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        [Authorize(Roles = "Admin,Sub-Admin,lloydant")]
        public ActionResult DeleteStaffAction(int? id)
        {
            try
            {
                UserLogic userLogic = new UserLogic();
                Model.Model.User user = new User();
                List<CourseAllocation> courseAllocations = new List<CourseAllocation>();
                CourseAllocationLogic courseAllocationLogic = new CourseAllocationLogic();
                bool isDeleted = false;
                if (id != null)
                {
                    user = userLogic.GetModelBy(x => x.User_Id == id);
                    courseAllocations = courseAllocationLogic.GetModelsBy(x => x.User_Id == user.Id);
                    if (courseAllocations.Count > 0)
                    {
                        SetMessage("Cannot Delete User. Staff has course allocated to him/her", Message.Category.Error);
                        return RedirectToAction("DeleteStaff");
                    }

                    isDeleted = userLogic.Delete(u => u.User_Id == user.Id);
                }
                if (isDeleted == true)
                {
                    SetMessage("Deleted Successfully", Message.Category.Information); 
                }
                else
                {
                    SetMessage("Error", Message.Category.Error);  
                }

            }
            catch (Exception ex)
            {
                SetMessage("Error" + ex.Message, Message.Category.Error);
            }

            return RedirectToAction("DeleteStaff");
        }
        [Authorize(Roles = "Admin,Sub-Admin,lloydant")]
        public ActionResult GeneralAudit()
        {
            viewModel = new UserViewModel();
            try
            {
                ViewBag.Role = viewModel.RoleSelectList;
                ViewBag.Session = viewModel.SessionSelectList;
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        public ContentResult GetAudits(string dateFrom, string dateTo, string username, int? roleId, bool admission, bool application, int? sessionId)
        {
            MyJsonResult result = new MyJsonResult();
            JavaScriptSerializer serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            try
            {
                GeneralAuditLogic auditLogic = new GeneralAuditLogic();
                PersonAuditLogic personAuditLogic = new PersonAuditLogic();
                AdmissionListAuditLogic admissionListAuditLogic = new AdmissionListAuditLogic();
                AppliedCourseAuditLogic appliedCourseAuditLogic = new AppliedCourseAuditLogic();

                result.Audits = auditLogic.GetAudits();

                List<GeneralAudit> admissionAudit = new List<Model.Model.GeneralAudit>();
                List<GeneralAudit> applicationAudit = new List<Model.Model.GeneralAudit>();
                List<GeneralAudit> personAudit = new List<Model.Model.GeneralAudit>();

                personAudit = personAuditLogic.GetAudits();
                admissionAudit = admission ? admissionListAuditLogic.GetAudits(Convert.ToInt32(sessionId)) : null;
                applicationAudit = application ? appliedCourseAuditLogic.GetAudits() : null;

                if (!string.IsNullOrEmpty(dateFrom) && !string.IsNullOrEmpty(dateTo))
                {
                    DateTime DateFrom = new DateTime();
                    DateTime DateTo = new DateTime();

                    if (!DateTime.TryParse(dateFrom, out DateFrom))
                        DateFrom = DateTime.Now;
                    if (!DateTime.TryParse(dateTo, out DateTo))
                        DateTo = DateTime.Now;

                    DateTo = DateTo.AddHours(23.99);

                    result.Audits = result.Audits.Where(r => r.Time >= DateFrom && r.Time <= DateTo).ToList();
                    personAudit = personAudit.Where(r => r.Time >= DateFrom && r.Time <= DateTo).ToList();
                    if (admissionAudit != null)
                        admissionAudit = admissionAudit.Where(r => r.Time >= DateFrom && r.Time <= DateTo).ToList();
                    if (applicationAudit != null)
                        applicationAudit = applicationAudit.Where(r => r.Time >= DateFrom && r.Time <= DateTo).ToList();
                }
                if (!string.IsNullOrEmpty(username))
                {
                    result.Audits = result.Audits.Where(r => r.Username.ToLower().Contains(username.ToLower())).ToList();
                    personAudit = personAudit.Where(r => r.Username.ToLower().Contains(username.ToLower())).ToList();
                    if (admissionAudit != null)
                        admissionAudit = admissionAudit.Where(r => r.Username.ToLower().Contains(username.ToLower())).ToList();
                    if (applicationAudit != null)
                        applicationAudit = applicationAudit.Where(r => r.Username.ToLower().Contains(username.ToLower())).ToList();
                }
                if (roleId != null && roleId > 0)
                {
                    result.Audits = result.Audits.Where(r => r.RoleId == roleId).ToList();
                    personAudit = personAudit.Where(r => r.RoleId == roleId).ToList();
                    if(admissionAudit != null)
                        admissionAudit = admissionAudit.Where(r => r.RoleId == roleId).ToList();
                    if (applicationAudit != null)
                        applicationAudit = applicationAudit.Where(r => r.RoleId == roleId).ToList();
                }

                result.Audits.AddRange(personAudit);
                if (admissionAudit != null)
                    result.Audits.AddRange(admissionAudit);
                if (applicationAudit != null)
                    result.Audits.AddRange(applicationAudit);

                result.IsError = false;

                result.Audits = result.Audits.Where(u => !u.IsSuperAdmin).ToList();
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Message = ex.Message;
            }

            var newResult = serializer.Serialize(result);

            var returnData = new ContentResult
            {
                Content = newResult,
                ContentType = "application/json"
            };

            return returnData;
        }
        public JsonResult DeleteUser(string id)
        {
            JsonResultModel result = new JsonResultModel();
            try
            {
                if (!String.IsNullOrEmpty(id))
                {
                    var userId = Convert.ToInt64(id);
                    GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();
                   
                    User LoggedInuser = userLogic.GetModelBy(p => p.User_Name == User.Identity.Name);
                    string client = Request.LogonUserIdentity.Name + " (" + HttpContext.Request.UserHostAddress + ")";
                    var user = userLogic.GetModelBy(f => f.User_Id == userId);
                    if (user?.Id > 0)
                    {
                        user.Archieved = true;
                        user.Activated = false;
                        userLogic.Modify(user);
                        result.IsError = false;
                        result.Message = "Operation Successful";
                        GeneralAudit generalAudit = new GeneralAudit();

                        generalAudit.Action = "Archived User";
                        generalAudit.Operation = "Archived User" + " :" + user.Username + "of Id:" + user.Id;
                        generalAudit.User = LoggedInuser;
                        generalAudit.Client = client;
                        generalAudit.TableNames = "User Table";
                        generalAudit.Time = DateTime.Now;
                        generalAuditLogic.Create(generalAudit);
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }



                }


            }
            catch (Exception ex)
            {

                result.IsError = true;
                result.Message = ex.Message;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetAllArchivedUsers()
        {
            try
            {
                
                List<User> userList = new List<User>();
                userList = userLogic.GetModelsBy(p => (p.Role_Id == 8 || p.Role_Id == 10 || p.Role_Id == 12) && p.Archive == true);
                viewModel.Users = userList;
            }
            catch (Exception e)
            {
                SetMessage("Error Occured " + e.Message, Message.Category.Error);
            }
            return View(viewModel);
        }
        public JsonResult ReenableUser(string id)
        {
            JsonResultModel result = new JsonResultModel();
            try
            {
                if (!String.IsNullOrEmpty(id))
                {
                    GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();
                    var userId = Convert.ToInt64(id);
                    User LoggedInuser = userLogic.GetModelBy(p => p.User_Name == User.Identity.Name);
                    string client = Request.LogonUserIdentity.Name + " (" + HttpContext.Request.UserHostAddress + ")";
                    var user = userLogic.GetModelBy(f => f.User_Id == userId);
                    if (user?.Id > 0)
                    {
                        user.Archieved = false;
                        user.Activated = true;
                        userLogic.Modify(user);
                        result.IsError = false;
                        result.Message = "Operation Successful";
                        GeneralAudit generalAudit = new GeneralAudit();

                        generalAudit.Action = "Re-Enable User";
                        generalAudit.Operation = "Re-Enable User" + " :" + user.Username + "of Id:"+ user.Id ;
                        generalAudit.User = LoggedInuser;
                        generalAudit.Client = client;
                        generalAudit.TableNames = "User Table";
                        generalAudit.Time = DateTime.Now;
                        generalAuditLogic.Create(generalAudit);
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }



                }


            }
            catch (Exception ex)
            {

                result.IsError = true;
                result.Message = ex.Message;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}