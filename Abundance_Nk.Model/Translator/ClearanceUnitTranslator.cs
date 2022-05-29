using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Entity.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Translator
{

    public class ClearanceUnitTranslator : TranslatorBase<ClearanceUnit, CLEARANCE_UNITS>
    {
        public override ClearanceUnit TranslateToModel(CLEARANCE_UNITS entity)
        {
            try
            {
                ClearanceUnit model = null;
                if (entity != null)
                {
                    model = new ClearanceUnit();
                    model.Id = entity.Id;
                    model.Name = entity.Name;
                    model.Active = entity.Active;
                    model.StampUnit = entity.Stamp_Unit;
                }

                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override CLEARANCE_UNITS TranslateToEntity(ClearanceUnit model)
        {
            try
            {
                CLEARANCE_UNITS entity = null;
                if (model != null)
                {
                    entity = new CLEARANCE_UNITS();
                    entity.Id = model.Id;
                    entity.Name = model.Name;
                    entity.Active = model.Active;
                    entity.Stamp_Unit = model.StampUnit;
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
