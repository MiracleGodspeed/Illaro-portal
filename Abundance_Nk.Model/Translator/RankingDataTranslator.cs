using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Model.Entity;

namespace Abundance_Nk.Model.Translator
{
    public class RankingDataTranslator : TranslatorBase<RankingData, RANKING_DATA>
    {
        private PersonTranslator personTranslator;
       
        public RankingDataTranslator()
        {
            personTranslator = new PersonTranslator();
        }


        public override RankingData TranslateToModel(RANKING_DATA entity)
        {
            try
            {
                RankingData model = null;
                if (entity != null)
                {
                    model = new RankingData();
                    model.Id = entity.Id;
                    model.Person = personTranslator.Translate(entity.PERSON);
                    model.Qualified = entity.Qualified;
                    model.Reason = entity.Reason;
                    model.Subj1 = entity.Subj1;
                    model.Subj2 = entity.Subj2;
                    model.Subj3 = entity.Subj3;
                    model.Subj4 = entity.Subj4;
                    model.Subj5 = entity.Subj5;
                    model.Total = entity.Total;
                }

                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override RANKING_DATA TranslateToEntity(RankingData model)
        {
            try
            {
                RANKING_DATA entity = null;
                if (model != null)
                {
                    entity = new RANKING_DATA();
                    entity.Id = model.Id;
                    entity.Person_id = model.Person.Id;
                    entity.Qualified = model.Qualified;
                    entity.Reason = model.Reason;
                    entity.Subj1 = model.Subj1;
                    entity.Subj2 = model.Subj2;
                    entity.Subj3 = model.Subj3;
                    entity.Subj4 = model.Subj4;
                    entity.Subj5 = model.Subj5;
                    entity.Total = model.Total;
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
