using Abundance_Nk.Business;
using Abundance_Nk.Model.Model;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Mvc;

namespace Abundance_Nk.Web.Models
{
    /// <summary>
    /// Custom authorization attribute for setting per-method accessibility 
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public class SetPermissionsAttribute : AuthorizeAttribute
    {
        private string _controllerTypeName = string.Empty;
        private string _controllerName = string.Empty;
        private string _controllerActionName = string.Empty;

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            _controllerTypeName = filterContext.Controller.GetType().FullName;
            _controllerActionName = filterContext.RouteData.GetRequiredString("action");
            _controllerName = filterContext.RouteData.GetRequiredString("controller");

            // The following line calls the AuthorizeCore method, below.
            base.OnAuthorization(filterContext);
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            
            UserLogic userLogic = new UserLogic();
            MenuInRoleLogic menuInRoleLogic = new MenuInRoleLogic();
            
            base.AuthorizeCore(httpContext);
            
            var currentUserName = httpContext.User.Identity.Name;

            //Get User 
            var user = userLogic.GetModelBy(u => u.User_Name == currentUserName);
            if (user != null && user.Id > 0)
            {
                //Check if action is in menu_in_role
                var exists = menuInRoleLogic.GetModelsBy(m => m.Role_Id == user.Role.Id && m.MENU.Action == _controllerActionName && m.MENU.Controller == _controllerName);
                if (exists != null && exists.Count > 0)
                {
                    return true;
                }
            }


            return false;
        }
    }
}