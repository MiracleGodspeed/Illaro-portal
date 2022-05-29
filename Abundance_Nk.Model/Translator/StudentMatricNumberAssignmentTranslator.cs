using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Abundance_Nk.Model.Model;
using Abundance_Nk.Model.Entity;

namespace Abundance_Nk.Model.Translator
{
    public class StudentMatricNumberAssignmentTranslator : TranslatorBase<StudentMatricNumberAssignment, STUDENT_MATRIC_NUMBER_ASSIGNMENT>
    {
        private LevelTranslator levelTranslator;
        private SessionTranslator sessionTranslator;
        private DepartmentTranslator departmentTranslator;
        private FacultyTranslator facultyTranslator;
        private ProgrammeTranslator programmeTranslator;
        
        public StudentMatricNumberAssignmentTranslator()
        {
            levelTranslator = new LevelTranslator();
            sessionTranslator = new SessionTranslator();
            departmentTranslator = new DepartmentTranslator();
            facultyTranslator = new FacultyTranslator();
            programmeTranslator = new ProgrammeTranslator();
        }

        public override StudentMatricNumberAssignment TranslateToModel(STUDENT_MATRIC_NUMBER_ASSIGNMENT entity)
        {
            try
            {
                StudentMatricNumberAssignment model = null;
                if (entity != null)
                {
                    model = new StudentMatricNumberAssignment();
                    model.Faculty = facultyTranslator.Translate(entity.FACULTY);
                    model.Level = levelTranslator.Translate(entity.LEVEL);
                    model.Department = departmentTranslator.Translate(entity.DEPARTMENT);
                    model.Programme = programmeTranslator.Translate(entity.PROGRAMME);
                    model.Session = sessionTranslator.Translate(entity.SESSION);
                    model.MatricSerialNoStartFrom = entity.Matric_Serial_Number_Start_From;
                    model.MatricNoStartFrom = entity.Matric_Number_Start_From;
                    model.Used = entity.Used;
                }

                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override STUDENT_MATRIC_NUMBER_ASSIGNMENT TranslateToEntity(StudentMatricNumberAssignment model)
        {
            try
            {
                STUDENT_MATRIC_NUMBER_ASSIGNMENT entity = null;
                if (model != null)
                {
                    entity = new STUDENT_MATRIC_NUMBER_ASSIGNMENT();
                    entity.Faculty_Id = model.Faculty.Id;
                    entity.Level_Id = model.Level.Id;
                    entity.Department_Id = model.Department.Id;
                    entity.Programme_Id = model.Programme.Id;
                    entity.Session_Id = model.Session.Id;
                    entity.Matric_Serial_Number_Start_From = model.MatricSerialNoStartFrom;
                    entity.Matric_Number_Start_From = model.MatricNoStartFrom;
                    entity.Used = model.Used;
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
