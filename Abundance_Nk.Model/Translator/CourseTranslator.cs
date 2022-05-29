using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Abundance_Nk.Model.Model;
using Abundance_Nk.Model.Entity;


namespace Abundance_Nk.Model.Translator
{
    public class CourseTranslator : TranslatorBase<Course, COURSE>
    {
        private LevelTranslator levelTranslator;
        private SemesterTranslator semesterTranslator;
        private DepartmentTranslator departmentTranslator;
        private DepartmentOptionTranslator departmentOptionTranslator;
        private CourseTypeTranslator courseTypeTranslator;
        private ProgrammeTranslator programmeTranslator;

        public CourseTranslator()
        {
            levelTranslator = new LevelTranslator();
            semesterTranslator = new SemesterTranslator();
            courseTypeTranslator = new CourseTypeTranslator();
            departmentTranslator = new DepartmentTranslator();
            departmentOptionTranslator = new DepartmentOptionTranslator();
            programmeTranslator = new ProgrammeTranslator();
        }

        public override Course TranslateToModel(COURSE entity)
        {
            try
            {
                Course model = null;
                if (entity != null)
                {
                    model = new Course();
                    model.Id = entity.Course_Id;
                    model.Name = entity.Course_Name;
                    model.Type = courseTypeTranslator.Translate(entity.COURSE_TYPE);
                    model.Level = levelTranslator.Translate(entity.LEVEL);
                    model.Department = departmentTranslator.Translate(entity.DEPARTMENT);
                    model.Programme = programmeTranslator.Translate(entity.PROGRAMME);
                    model.DepartmentOption = departmentOptionTranslator.Translate(entity.DEPARTMENT_OPTION);
                    model.Unit = entity.Course_Unit;
                    model.Semester = semesterTranslator.Translate(entity.SEMESTER);
                    model.Code = entity.Course_Code;
                    model.Activated = entity.Activated;
                }

                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override COURSE TranslateToEntity(Course model)
        {
            try
            {
                COURSE entity = null;
                if (model != null)
                {
                    entity = new COURSE();
                    entity.Course_Id = model.Id;
                    entity.Course_Name = model.Name;
                    entity.Course_Type_Id = model.Type.Id;
                    entity.Level_Id = model.Level.Id;
                    entity.Department_Id = model.Department.Id;
                    entity.Programme_Id = model.Programme.Id;
                    if (model.DepartmentOption != null && model.DepartmentOption.Id > 0)
                    {
                       entity.Department_Option_Id = model.DepartmentOption.Id;
                    }
                    entity.Course_Unit = model.Unit;
                    entity.Semester_Id = model.Semester.Id;
                    entity.Course_Code = model.Code;
                    entity.Activated = model.Activated;
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
