using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Entity.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Translator
{
    public class LicenseTypeTranslator : TranslatorBase<LicenseType, LICENSE_TYPE>
    {
        public override LicenseType TranslateToModel(LICENSE_TYPE entity)
        {
            try
            {
                LicenseType model = null;
                if (entity != null)
                {
                    model = new LicenseType();
                    model.Id = entity.License_Type_Id;
                    model.Name = entity.Name;
                    model.Description = entity.Description;
                    model.Active = entity.Active;
                }

                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public override LICENSE_TYPE TranslateToEntity(LicenseType model)
        {
            try
            {
                LICENSE_TYPE entity = null;
                if (model != null)
                {
                    entity = new LICENSE_TYPE();
                    entity.License_Type_Id = model.Id;
                    entity.Name = model.Name;
                    entity.Description = model.Description;
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
