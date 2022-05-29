using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Entity.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Translator
{
    public class JambOlevelTranslator : TranslatorBase<JambOlevel, JAMB_O_LEVEL>
    {
        private JambRecordTranslator jambRecordTranslator;
        private OLevelExamSittingTranslator examSittingTranslator;
        private OLevelTypeTranslator examTypeTranslator;

        public JambOlevelTranslator()
        {
            jambRecordTranslator = new JambRecordTranslator();
            examSittingTranslator = new OLevelExamSittingTranslator();
            examTypeTranslator = new OLevelTypeTranslator();
        }


        public override JambOlevel TranslateToModel(JAMB_O_LEVEL entity)
        {
            try
            {
                JambOlevel model = null;
                if (entity != null)
                {
                    model = new JambOlevel();
                    model.Id = entity.Id;
                    model.JambRecord = jambRecordTranslator.Translate(entity.JAMB_RECORD);
                    model.ExamSitting = examSittingTranslator.Translate(entity.O_LEVEL_EXAM_SITTING);
                    model.ExamType = examTypeTranslator.Translate(entity.O_LEVEL_TYPE);
                    model.Exam_Number = entity.Exam_Number;
                    model.Exam_Year = entity.Exam_Year;

                }

                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }



        public override JAMB_O_LEVEL TranslateToEntity(JambOlevel model)
        {
            try
            {
                JAMB_O_LEVEL entity = null;
                if (model != null)
                {
                    entity = new JAMB_O_LEVEL();
                    entity.Id = model.Id;
                    entity.Jamb_Record_Id = model.JambRecord.Id;
                    entity.Exam_Sitting = model.ExamSitting.Id;
                    entity.Exam_Type_Id = model.ExamType.Id;
                    entity.Exam_Number = model.Exam_Number;
                    entity.Exam_Year = model.Exam_Year;
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
