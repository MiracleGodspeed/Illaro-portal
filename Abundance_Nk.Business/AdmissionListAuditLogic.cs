using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Translator;


namespace Abundance_Nk.Business
{
    public class AdmissionListAuditLogic: BusinessBaseLogic<AdmissionListAudit,ADMISSION_LIST_AUDIT>
    {
        public AdmissionListAuditLogic()
        {
            translator = new AdmissionListAuditTranslator();
        }
        public List<GeneralAudit> GetAudits(int sessionId)
        {
            List<GeneralAudit> audits = new List<GeneralAudit>();
            try
            {
                audits = (from s in repository.GetBy<VW_ADMISSION_AUDIT>(s => s.Session_Id == sessionId)
                          select new GeneralAudit
                          {
                              Time = s.Time,
                              Action = s.Action,
                              Operation = s.Operation + " : " + s.Application_Form_Number,
                              Client = s.Client,
                              UserId = s.User_Id,
                              Username = s.User_Name,
                              RoleId = s.Role_Id,
                              Role = s.Role_Name,
                              IsSuperAdmin = s.Super_Admin != null ? s.Super_Admin.Value : false
                          }).ToList();
                
                return audits;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
