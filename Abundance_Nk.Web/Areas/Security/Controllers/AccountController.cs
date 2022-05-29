using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

using System.Web.Security;
using Abundance_Nk.Web.Models;
using Abundance_Nk.Web.Controllers;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Business;
using Abundance_Nk.Model.Entity.Model;
using System.IO;

namespace Abundance_Nk.Web.Areas.Security.Controllers
{
	
	public class AccountController : BaseController
	{
		public ActionResult Home()
		{
			return View();
		}

		public ActionResult ChangePassword()
		{
			ManageUserViewModel manageUserviewModel = new ManageUserViewModel();
			
			try
			{
				ViewBag.UserId = User.Identity.Name;
				manageUserviewModel.Username = User.Identity.Name;
			}
			catch (Exception)
			{
				throw;
			}
			return View(manageUserviewModel);
		}
		[HttpPost]
		public ActionResult ChangePassword(ManageUserViewModel manageUserviewModel)
		{
			try
			{
				if (ModelState.IsValid)
				{
					UserLogic userLogic = new UserLogic();
					Abundance_Nk.Model.Model.User LoggedInUser = new Model.Model.User();
					LoggedInUser = userLogic.GetModelBy(u => u.User_Name == manageUserviewModel.Username && u.Password == manageUserviewModel.OldPassword);
					if (LoggedInUser != null)
					{
						LoggedInUser.Password = manageUserviewModel.NewPassword;
						userLogic.ChangeUserPassword(LoggedInUser);
						TempData["Message"] = "Password Changed successfully! Please keep password in a safe place";
						return RedirectToAction("Home", "Account", new { Area = "Security" });
					}
					else
					{
						SetMessage("Please log off and log in then try again.", Message.Category.Error);
					}
				   
					return View(manageUserviewModel);
				}
			}
			catch (Exception)
			{
				throw;
			}
			return View();
		}

		[AllowAnonymous]
		public ActionResult Login(string ReturnUrl)
		{
			ViewBag.ReturnUrl = ReturnUrl;
			return View();
		}

		[HttpPost]
		[AllowAnonymous]
		public ActionResult Login(LoginViewModel viewModel, string returnUrl)
		{
			try
			{
				if (viewModel.UserName.Contains("/"))
				{
					StudentLogic studentLogic = new StudentLogic();
					if (studentLogic.ValidateUser(viewModel.UserName, viewModel.Password))
					{
						FormsAuthentication.SetAuthCookie(viewModel.UserName, false);
						var student = studentLogic.GetBy(viewModel.UserName);
                        student=CheckExistenceOfPassport(student);
                        Session["student"] = student;
						if (string.IsNullOrEmpty(returnUrl))
						{
							return RedirectToAction("Index", "Home", new {Area = "Student"});
						}
						return RedirectToLocal(returnUrl);
					}
				}
				else
				{
					UserLogic userLogic = new UserLogic();
					if (userLogic.ValidateUser(viewModel.UserName, viewModel.Password))
					{
						FormsAuthentication.SetAuthCookie(viewModel.UserName, false);

                        Model.Model.User user = userLogic.GetModelsBy(u => u.User_Name == viewModel.UserName && u.Archive==false).LastOrDefault();
                        if (user != null && user.Archieved)
                        {
                            SetMessage("Account No Longer Exist!", Message.Category.Error);
                            return View();
                            //return RedirectToAction("ModifyPassword");
                        }

						if (string.IsNullOrEmpty(returnUrl))
						{
							return RedirectToAction("Index", "Profile", new {Area = "Admin"});
						}
						else
						{
							return RedirectToLocal(returnUrl);
						}

					}
				}
			}
			catch (Exception ex)
			{
				SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
			}

			SetMessage("Invalid Username or Password!", Message.Category.Error);
			return View();
		}

        [AllowAnonymous]
        [HttpPost]
		public ActionResult LogOff()
		{
			FormsAuthentication.SignOut();
			System.Web.HttpContext.Current.Session.Clear();
			return RedirectToAction("Login", "Account", new { Area = "Security" });
		}

		private ActionResult RedirectToLocal(string returnUrl)
		{
			if (Url.IsLocalUrl(returnUrl))
			{
				return Redirect(returnUrl);
			}
			else
			{
				return RedirectToAction("Index", "Home");
			}
		}

	    public ActionResult ModifyPassword()
	    {
	        return View();
	    }
        public JsonResult ChangeUserPassword(string password)
        {
            JsonResultModel result = new JsonResultModel();
            try
            {
                if (!string.IsNullOrEmpty(password))
                {
                    UserLogic userLogic = new UserLogic();

                    Model.Model.User user = userLogic.GetModelsBy(u => u.User_Name == User.Identity.Name).LastOrDefault();
                    
                    if (user != null)
                    {
                        user.Password = password;
                        user.PasswordChanged = true;

                        userLogic.Modify(user);

                        result.IsError = false;
                        result.Message = "Operation Successful!";
                    }
                    else
                    {
                        result.IsError = true;
                        result.Message = "User not found!";
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
                result.Message = ex.Message;

                return Json(result, JsonRequestBehavior.AllowGet);
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public Model.Model.Student CheckExistenceOfPassport(Model.Model.Student student)
        {
            string correctImagePath = string.Empty;
            string correctImageServerPath = string.Empty;
            string searchImagePath = string.Empty;
            if (!string.IsNullOrEmpty(student.ImageFileUrl))
            {
                string imageFilePath = System.Web.HttpContext.Current.Server.MapPath(student.ImageFileUrl);
                if (!System.IO.File.Exists(imageFilePath))
                {
                    SetCorrectImagePath(student, out correctImagePath, out correctImageServerPath);
                    if (imageFilePath.Contains("Junk"))
                        searchImagePath = imageFilePath.Replace("Junk", "Student");
                    else
                        searchImagePath = imageFilePath.Replace("Student", "Junk");
                    string folderPath = Path.GetDirectoryName(searchImagePath);
                    string correctFolderPath = Path.GetFileName(correctImagePath);
                    string mainFileName = student.Id.ToString() + "__";

                    SearchAndCopyFileIfExist(folderPath, mainFileName, correctImageServerPath);
                    student.ImageFileUrl = correctImagePath;
                    PersonLogic personLogic = new PersonLogic();
                    var person=personLogic.GetModelsBy(f => f.Person_Id == student.Id).FirstOrDefault();
                    if (person?.Id > 0)
                    {
                        person.ImageFileUrl = student.ImageFileUrl;
                        personLogic.ModifyImageUrl(person);
                    }

                    //System.IO.File.Move(junkFilePath, pathForSaving);
                }
            }
            return student;
        }
        private void SearchAndCopyFileIfExist(string folderPath, string fileName, string correctImageServerPath)
        {
            try
            {
                string wildCard = fileName + "*.*";
                IEnumerable<string> files = Directory.EnumerateFiles(folderPath, wildCard, SearchOption.TopDirectoryOnly);

                if (files != null && files.Count() > 0)
                {
                    foreach (string file in files)
                    {
                        if (System.IO.File.Exists(file))
                        {
                            System.IO.File.Copy(file, correctImageServerPath);
                            break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void SetCorrectImagePath(Model.Model.Student student, out string savePath, out string saveServerPath)
        {

            try
            {
                if (student.ImageFileUrl.Contains("Junk"))
                {
                    savePath = student.ImageFileUrl.Replace("Junk", "Student");
                    saveServerPath = Server.MapPath(savePath);
                }
                else
                {
                    savePath = student.ImageFileUrl;
                    saveServerPath = Server.MapPath(savePath);
                }


            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}