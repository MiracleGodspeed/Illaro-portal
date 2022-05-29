using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Entity.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Translator
{
    public class ClearanceDisputesTranslator : TranslatorBase<ClearanceDisputes, CLEARANCE_DISPUTES>
    {
        private ClearanceLogTranslator clearanceLogTranslator;


        public ClearanceDisputesTranslator()
        {
            clearanceLogTranslator = new ClearanceLogTranslator();
        }
        public override ClearanceDisputes TranslateToModel(CLEARANCE_DISPUTES entity)
        {
            try
            {
                ClearanceDisputes model = null;
                if (entity != null)
                {
                    model = new ClearanceDisputes();
                    model.Id = entity.Id;
                    model.Remark = entity.Remark;
                    model.ClearanceLog = clearanceLogTranslator.Translate(entity.CLEARANCE_LOG);
                    model.IsStudent = entity.IsStudent;
                    model.DateSent = entity.DateSent;
                    model.Attachment = entity.Attachment;
                }

                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override CLEARANCE_DISPUTES TranslateToEntity(ClearanceDisputes model)
        {
            try
            {
                CLEARANCE_DISPUTES entity = null;
                if (model != null)
                {
                    entity = new CLEARANCE_DISPUTES();
                    entity.Id = model.Id;
                    entity.Attachment = model.Attachment;
                    entity.Clearance_Log_Id = model.ClearanceLog.Id;
                    entity.DateSent = model.DateSent;
                    entity.IsStudent = model.IsStudent;
                    entity.Remark = model.Remark;


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
