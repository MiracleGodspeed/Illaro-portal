using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Entity.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Translator
{
    public class ClearanceLogTranslator : TranslatorBase<ClearanceLog, CLEARANCE_LOG>
    {
        private UserTranslator userTranslator;
        private ClearanceStatusTranslator clearanceStatusTranslator;
        private ClearanceUnitTranslator clearanceUnitTranslator;
        private StudentTranslator studentTranslator;

        public ClearanceLogTranslator()
        {
            userTranslator = new UserTranslator();
            clearanceStatusTranslator = new ClearanceStatusTranslator();
            clearanceUnitTranslator = new ClearanceUnitTranslator();
            studentTranslator = new StudentTranslator();
        }
        public override ClearanceLog TranslateToModel(CLEARANCE_LOG entity)
        {
            try
            {
                ClearanceLog model = null;
                if (entity != null)
                {
                    model = new ClearanceLog();
                    model.Id = entity.Id;
                    model.Remarks = entity.Remarks;
                    model.Student = studentTranslator.Translate(entity.STUDENT);
                    model.ClearanceStatus = clearanceStatusTranslator.Translate(entity.CLEARANCE_STATUS);
                    model.ClearanceUnit = clearanceUnitTranslator.Translate(entity.CLEARANCE_UNITS);
                    model.User = userTranslator.Translate(entity.USER);
                    model.DateCleared = entity.Date_Cleared;
                    model.Closed = entity.Closed;
                    model.Client = entity.Client;
                }

                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override CLEARANCE_LOG TranslateToEntity(ClearanceLog model)
        {
            try
            {
                CLEARANCE_LOG entity = null;
                if (model != null)
                {
                    entity = new CLEARANCE_LOG();
                    entity.Id = model.Id;
                    entity.Client = model.Client;
                    entity.Closed = model.Closed;
                    entity.Date_Cleared = model.DateCleared;
                    entity.Remarks = model.Remarks;
                    entity.Student_Id = model.Student.Id;
                    if (model.User != null)
                    {
                        entity.User_Id = model.User.Id;
                    }
                    
                    entity.Clearance_Status_Id = model.ClearanceStatus.Id;
                    entity.Clearance_Unit_Id = model.ClearanceUnit.Id;


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
