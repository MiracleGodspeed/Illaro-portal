using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Model;

namespace Abundance_Nk.Model.Translator
{
    public class StudentResultStatusTranslator:TranslatorBase<StudentResultStatus,STUDENT_RESULT_STATUS>
    {
        private FacultyTranslator facultyTranslator;
        private SessionTranslator sessionTranslator;
        private ProgrammeTranslator programmeTranslator;
        private SemesterTranslator semesterTranslator;

        public StudentResultStatusTranslator()
        {
            facultyTranslator = new FacultyTranslator();
            sessionTranslator = new SessionTranslator();
            programmeTranslator = new ProgrammeTranslator();
            semesterTranslator = new SemesterTranslator();
        }

        public override StudentResultStatus TranslateToModel(STUDENT_RESULT_STATUS entity)
        {
            try
            {
                StudentResultStatus model = null;
                if (entity != null)
                {
                    model = new StudentResultStatus();
                    model.Id = entity.Id;
                    model.Faculty = facultyTranslator.Translate(entity.FACULTY);
                    model.Session = sessionTranslator.Translate(entity.SESSION);
                    model.Programme = programmeTranslator.Translate(entity.PROGRAMME);
                    model.Semester = semesterTranslator.Translate(entity.SEMESTER);
                    model.RAndDCApproval = entity.RAndDCApproval;
                    model.DRAcademicsApproval = entity.DRAcademicsApproval;
                    model.RegistrarApproval = entity.RegistrarApproval;
                }

                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override STUDENT_RESULT_STATUS TranslateToEntity(StudentResultStatus model)
        {
            try
            {
                STUDENT_RESULT_STATUS entity = null;
                if (model != null)
                {
                    entity = new STUDENT_RESULT_STATUS();
                    entity.Id = model.Id;
                    if (model.Session != null)
                    {
                        entity.Session_Id = model.Session.Id;
                    }
                    if (model.Semester != null)
                    {
                        entity.Semester_Id = model.Semester.Id;
                    }
                    if (model.Faculty != null)
                    {
                        entity.Faculty_Id = model.Faculty.Id;
                    }
                    if(model.Programme != null)
                    {
                        entity.Programme_Id = model.Programme.Id;
                    }
                    entity.RegistrarApproval = model.RegistrarApproval;
                    entity.DRAcademicsApproval = model.DRAcademicsApproval;
                    entity.RAndDCApproval = model.RAndDCApproval;
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
