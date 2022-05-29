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

    public class ClearanceStatusTranslator : TranslatorBase<ClearanceStatus, CLEARANCE_STATUS>
    {
        public override ClearanceStatus TranslateToModel(CLEARANCE_STATUS entity)
        {
            try
            {
                ClearanceStatus model = null;
                if (entity != null)
                {
                    model = new ClearanceStatus();
                    model.Id = entity.Id;
                    model.Name = entity.Name;
                    model.Active = entity.Active;
                }

                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override CLEARANCE_STATUS TranslateToEntity(ClearanceStatus model)
        {
            try
            {
                CLEARANCE_STATUS entity = null;
                if (model != null)
                {
                    entity = new CLEARANCE_STATUS();
                    entity.Id = model.Id;
                    entity.Name = model.Name;
                    entity.Active = model.Active;
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
