using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Model.Translator;

using Abundance_Nk.Business;
using Abundance_Nk.Model.Entity.Model;

//using Abundance_Nk.Web.Areas.Admin.ViewModels;
//using Abundance_Nk.Web.Controllers;
//using Abundance_Nk.Web.Models;

using System.Web;
using System.Transactions;
using System.Web.Mvc;

namespace Abundance_Nk.Business
{
    public class GeneralAuditLogic : BusinessBaseLogic<GeneralAudit, GENERAL_AUDIT>
    {
        public GeneralAuditLogic()
        {
            translator = new GeneralAuditTranslator();
        }

        public List<GeneralAudit> GetAudits()
        {
            try
            {
                List<GeneralAudit> audits = new List<GeneralAudit>();
                audits = (from s in repository.GetBy<VW_GENERAL_AUDIT>(s => s.User_Id > 0)
                          select new GeneralAudit
                          {
                              Time = s.Time,
                              Action = s.Action,
                              Operation = s.Operation,
                              InitialValues = s.Initial_Values,
                              CurrentValues = s.Current_Values,
                              Client = s.Client,
                              Username = s.User_Name,
                              Email = s.Email,
                              Role = s.Role_Name,
                              RoleId = s.Role_Id,
                              Date = s.Time.ToLongDateString()
                          }).ToList();

                return audits;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GeneralAudit CreateGeneralAudit(string Action, string Operation, String Table)
        {
               // GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();
                GeneralAudit generalAudit = new GeneralAudit();
                UserLogic userLogic = new UserLogic();

                User user = userLogic.GetModelBy(p => p.User_Name == HttpContext.Current.User.Identity.Name.ToString());
                string client = user.Username + " (" + HttpContext.Current.Request.UserHostAddress + ")";

                generalAudit.Action = Action;
                generalAudit.Client = client;
                generalAudit.CurrentValues = "-";
                generalAudit.InitialValues = "-";
                generalAudit.Operation = Operation;
                generalAudit.TableNames = Table;
                generalAudit.Time = DateTime.Now;
                generalAudit.User = user;

              base.Create(generalAudit);

            return generalAudit;
        }

        public GeneralAudit CreateGeneralAudit(GeneralAudit generalAudit)
        {
            try
            {
                if (generalAudit != null)
                {
                    UserLogic userLogic = new UserLogic();

                    User user = userLogic.GetModelBy(p => p.User_Name == HttpContext.Current.User.Identity.Name.ToString());
                    string client = user.Username + " (" + HttpContext.Current.Request.UserHostAddress + ")";
                    generalAudit.User = user;
                    generalAudit.Client = client;
                    generalAudit.Time = DateTime.Now;

                    generalAudit = base.Create(generalAudit);
                }

                return generalAudit;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
