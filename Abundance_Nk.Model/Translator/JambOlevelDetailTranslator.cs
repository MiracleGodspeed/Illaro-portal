using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Entity.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Translator
{
    public class JambOlevelDetailTranslator : TranslatorBase<JambOlevelDetail, JAMB_O_LEVEL_DETAIL>
    {
        private JambOlevelTranslator jambOlevelTranslator;
        private OLevelGradeTranslator oLevelGradeTranslator;
        private OLevelSubjectTranslator oLevelSubjectTranslator;

        public JambOlevelDetailTranslator()
        {
            jambOlevelTranslator = new JambOlevelTranslator();
            oLevelGradeTranslator = new OLevelGradeTranslator();
            oLevelSubjectTranslator = new OLevelSubjectTranslator();
        }

        public override JambOlevelDetail TranslateToModel(JAMB_O_LEVEL_DETAIL entity)
        {
            try
            {
                JambOlevelDetail model = null;
                if (entity != null)
                {
                    model = new JambOlevelDetail();
                    model.Id = entity.Id;
                    model.JambOlevel = jambOlevelTranslator.Translate(entity.JAMB_O_LEVEL);
                    model.OLevelSubject = oLevelSubjectTranslator.Translate(entity.O_LEVEL_SUBJECT);
                    model.OLevelGrade = oLevelGradeTranslator.Translate(entity.O_LEVEL_GRADE);
                }

                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override JAMB_O_LEVEL_DETAIL TranslateToEntity(JambOlevelDetail model)
        {
            try
            {
                JAMB_O_LEVEL_DETAIL entity = null;
                if (model != null)
                {
                    entity = new JAMB_O_LEVEL_DETAIL();
                    entity.Id = model.Id;
                    entity.Jamb_O_Level_Id = model.JambOlevel.Id;
                    entity.Subject_Id = model.OLevelSubject.Id;
                    entity.Grade_Id = model.OLevelGrade.Id;
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
