using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Entity.Model;
using Abundance_Nk.Model.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Translator
{
    public class ApplicantApplicationApprovalTranslator : TranslatorBase<ApplicantApplicationApproval, APPLICANT_APPLICATION_APPROVAL>
    {
        private ApplicationFormTranslator applicationFormTranslator;
        private UserTranslator userTranslator;

        public ApplicantApplicationApprovalTranslator()
        {
            applicationFormTranslator = new ApplicationFormTranslator();
            userTranslator = new UserTranslator();
        }
        public override ApplicantApplicationApproval TranslateToModel(APPLICANT_APPLICATION_APPROVAL entity)
        {
            try
            {
                ApplicantApplicationApproval model = null;
                if (entity != null)
                {
                    model = new ApplicantApplicationApproval();
                    model.ApplicationForm = applicationFormTranslator.Translate(entity.APPLICATION_FORM);
                    model.User = userTranslator.Translate(entity.USER);
                    model.IsApproved = entity.Is_Approved;
                    model.DateTreated = entity.Date_Treated;
                    model.Remark = entity.Remarks;
                    model.ClearanceCode = entity.Clearance_Code;
                }

                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override APPLICANT_APPLICATION_APPROVAL TranslateToEntity(ApplicantApplicationApproval model)
        {
            try
            {
                APPLICANT_APPLICATION_APPROVAL entity = null;
                if (model != null)
                {
                    entity = new APPLICANT_APPLICATION_APPROVAL();
                    entity.Application_Form_Id = model.ApplicationForm.Id;
                    entity.User_Id = model.User.Id;
                    entity.Remarks = model.Remark;
                    entity.Date_Treated = model.DateTreated;
                    entity.Is_Approved = model.IsApproved;
                    entity.Clearance_Code = model.ClearanceCode;
                }

                return entity;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
