using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Translator;
using System.Linq.Expressions;

namespace Abundance_Nk.Business
{
    public class AppliedCourseAuditLogic : BusinessBaseLogic<AppliedCourseAudit, APPLICANT_APPLIED_COURSE_AUDIT>
    {
        public AppliedCourseAuditLogic()
        {
            translator = new AppliedCourseAuditTranslator ();
        }
        public List<GeneralAudit> GetAudits()
        {
            List<GeneralAudit> audits = new List<GeneralAudit>();
            try
            {
                audits = (from s in repository.GetAll<VW_APPLIED_COURSE_AUDIT>()
                          select new GeneralAudit
                          {
                              Time = s.Time,
                              Action = s.Action,
                              Operation = s.Operation + " : " + s.Name + ", " + s.Mobile_Phone,
                              Client = s.Client,
                              UserId = s.User_Id,
                              Programme = s.Programme_Name,
                              OldProgramme = s.Old_Programme_Name,
                              Department = s.Department_Name,
                              OldDepartment = s.Old_Department_Name,
                              DepartmentOption = s.Department_Option_Name,
                              OldDepartmentOption = s.Old_Department_Option_Name,
                              ApplicationForm = s.Application_Form_Number,
                              OldApplicationForm = s.Old_Application_Form_Number,
                              Username = s.User_Name,
                              RoleId = s.Role_Id,
                              Role = s.Role_Name,
                              IsSuperAdmin = s.Super_Admin != null ? s.Super_Admin.Value : false
                          }).ToList();
                
                audits.ForEach(p =>
                {
                    AssignAuditValues(p);
                });

                return audits;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void AssignAuditValues(GeneralAudit p)
        {
            try
            {
                if (p.Programme != null && p.Programme != p.OldProgramme)
                {
                    if (!string.IsNullOrEmpty(p.Programme))
                        p.CurrentValues += p.Programme + ", ";
                    if (!string.IsNullOrEmpty(p.OldProgramme))
                        p.InitialValues += p.OldProgramme + ", ";
                }
                if (p.Department != null && p.Department != p.OldDepartment)
                {
                    if (!string.IsNullOrEmpty(p.Department))
                        p.CurrentValues += p.Department + ", ";
                    if (!string.IsNullOrEmpty(p.OldDepartment))
                        p.InitialValues += p.OldDepartment + ", ";
                }
                if (p.DepartmentOption != null && p.DepartmentOption != p.OldDepartmentOption)
                {
                    if (!string.IsNullOrEmpty(p.Department))
                        p.CurrentValues += p.Department + ", ";
                    if (!string.IsNullOrEmpty(p.OldDepartmentOption))
                        p.InitialValues += p.OldDepartmentOption + ", ";
                }
                if (p.ApplicationForm != null && p.ApplicationForm != p.OldApplicationForm)
                {
                    if (!string.IsNullOrEmpty(p.ApplicationForm))
                        p.CurrentValues += p.ApplicationForm + ", ";
                    if (!string.IsNullOrEmpty(p.OldApplicationForm))
                        p.InitialValues += p.OldApplicationForm + ", ";
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
