using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Model.Entity;

namespace Abundance_Nk.Model.Translator
{
    public class PrincipalOfficersTranslator : TranslatorBase<PrincipalOfficers, PRINCIPAL_OFFICERS>
    {
        public override PrincipalOfficers TranslateToModel(PRINCIPAL_OFFICERS entity)
        {
            try
            {
                PrincipalOfficers model = null;
                if (entity != null)
                {
                    model = new PrincipalOfficers();
                    model.Id = entity.Id;
                    model.PhoneNo = entity.PhoneNo;
                    model.Active = entity.Active;
                }

                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override PRINCIPAL_OFFICERS TranslateToEntity(PrincipalOfficers model)
        {
            try
            {
                PRINCIPAL_OFFICERS entity = null;
                if (model != null)
                {
                    entity = new PRINCIPAL_OFFICERS();
                    entity.Id = model.Id;
                    entity.PhoneNo = model.PhoneNo;
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
