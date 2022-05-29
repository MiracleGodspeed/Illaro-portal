﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Abundance_Nk.Business;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Web.Models;

namespace Abundance_Nk.Web.Controllers
{
   // [SetPermissions()]
    public class BaseController : Controller
	{
		public BaseController()
		{
			if (System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
			{
				string username = System.Web.HttpContext.Current.User.Identity.Name;

                UserLogic userLogic = new UserLogic();
                Model.Model.User user = userLogic.GetModelsBy(u => u.User_Name == username).LastOrDefault();
                if (user != null && user.PasswordChanged != true)
                {
                    RedirectToAction("ModifyPassword", "Account", new { Area = "Security" });
                    return;
                }

			    if (username != null)
				{
					StaffLogic staffLogic = new StaffLogic();
					Staff staff = new Staff();
					staff = staffLogic.GetBy(username);
					if (staff == null)
					{
					   RedirectToAction("Index", "Profile", new {Area = "Admin"});
					}
				}

			}
		}
		protected void SetMessage(string message, Message.Category messageType)
		{
			Message msg = new Message(message, (int)messageType);
			TempData["Message"] = msg;
		}
        protected static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://google.com/generate_204"))
                    return true;
            }
            catch
            {
                return false;
            }
        }


    }




}