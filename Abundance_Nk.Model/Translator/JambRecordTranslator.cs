using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Model;

namespace Abundance_Nk.Model.Translator
{
    public class JambRecordTranslator : TranslatorBase<JambRecord, JAMB_RECORD>
    {
        private DepartmentTranslator departmentTranslator;
        private SessionTranslator sessionTranslator;
        private StateTranslator stateTranslator;
        private SexTranslator sexTranslator;
        private OLevelSubjectTranslator subjectTranslator;

        public JambRecordTranslator()
        {
            departmentTranslator = new DepartmentTranslator();
            sessionTranslator = new SessionTranslator();
            stateTranslator = new StateTranslator();
            sexTranslator = new SexTranslator();
            subjectTranslator = new OLevelSubjectTranslator();

        }
        public override JambRecord TranslateToModel(JAMB_RECORD entity)
        {
            try
            {
                JambRecord model = null;
                if (entity != null)
                {
                    model = new JambRecord();
                    model.Id = entity.Jamb_Record_Id;
                    model.CandidateName = entity.Candidate_Name;
                    model.FirstChoiceInstitution = entity.First_Choice_Institution;
                    model.JambRegistrationNumber = entity.Jamb_Registration_Number;
                    model.Subject1 = entity.Subject1;
                    model.Subject2 = entity.Subject2;
                    model.Subject3 = entity.Subject3;
                    model.Subject4 = entity.Subject4;
                    model.TotalJambScore = entity.Total_Jamb_Score;
                    model.Score1 = entity.Score1;
                    model.Score2 = entity.Score2;
                    model.Score3 = entity.Score3;
                    model.Score4 = entity.Score4;
                    model.Course = departmentTranslator.Translate(entity.DEPARTMENT);
                    model.Session = sessionTranslator.Translate(entity.SESSION);
                    model.State = stateTranslator.Translate(entity.STATE);
                    model.Sex = sexTranslator.Translate(entity.SEX);
                }

                return model;
            }
            catch (Exception)
            {   
                throw;
            }
        }

        public override JAMB_RECORD TranslateToEntity(JambRecord model)
        {
            try
            {
                JAMB_RECORD entity = null;
                if (model != null)
                {
                    entity = new JAMB_RECORD();
                    entity.Jamb_Record_Id = model.Id;
                    entity.Candidate_Name = model.CandidateName;
                    entity.First_Choice_Institution = model.FirstChoiceInstitution;
                    entity.Jamb_Registration_Number = model.JambRegistrationNumber;
                    entity.Subject1 = model.Subject1;
                    entity.Subject2 = model.Subject2;
                    entity.Subject3 = model.Subject3;
                    entity.Subject4 = model.Subject4;
                    entity.Score1 = model.Score1;
                    entity.Score2 = model.Score2;
                    entity.Score3 = model.Score3;
                    entity.Score4 = model.Score4;
                    entity.Total_Jamb_Score = model.TotalJambScore;
                    entity.Session_Id = model.Session.Id;
                    entity.Course_Id = model.Course.Id;
                    entity.State_Id = model.State.Id;
                    entity.Sex_Id = model.Sex.Id;
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
