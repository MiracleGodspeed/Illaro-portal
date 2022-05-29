using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Entity.Model;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Model.Translator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Business
{

    public class ApplicantApplicationApprovalLogic : BusinessBaseLogic<ApplicantApplicationApproval, APPLICANT_APPLICATION_APPROVAL>
    {
        //private ApplicationFormLogic applicationFormLogic;
        //private UserLogic userLogic;
        public ApplicantApplicationApprovalLogic()
        {
            translator = new ApplicantApplicationApprovalTranslator();
        }
        public bool Modify(ApplicantApplicationApproval model)
        {
            try
            {
                Expression<Func<APPLICANT_APPLICATION_APPROVAL, bool>> selector = a => a.Application_Form_Id == model.ApplicationForm.Id;
                APPLICANT_APPLICATION_APPROVAL entity = GetEntitiesBy(selector).FirstOrDefault();

                if (entity != null)
                {
                   
                    if (model.User?.Id>0)
                    {
                        entity.User_Id = model.User.Id;
                    }
                    entity.Clearance_Code = model.ClearanceCode;
                    entity.Is_Approved = model.IsApproved;
                    entity.Remarks = model.Remark;
                    entity.Date_Treated = model.DateTreated;
                    int modifiedRecordCount = Save();

                    if (modifiedRecordCount > 0)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
