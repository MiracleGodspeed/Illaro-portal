using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Entity.Model;

namespace Abundance_Nk.Model.Translator
{
    public class CourseRegistrationStatusTranslator : TranslatorBase<CourseRegistrationStatus, COURSE_REGISTRATION_STATUS>
    {
        private ProgrammeTranslator _programmeTranslator;
        private DepartmentTranslator _departmentTranslator;
        private SessionTranslator _sessionTranslator;

        public CourseRegistrationStatusTranslator()
        {
            _programmeTranslator = new ProgrammeTranslator();
            _departmentTranslator = new DepartmentTranslator();
            _sessionTranslator = new SessionTranslator();
        }

        public override CourseRegistrationStatus TranslateToModel(COURSE_REGISTRATION_STATUS entity)
        {
            try
            {
                CourseRegistrationStatus model = null;
                if (entity != null)
                {
                    model = new CourseRegistrationStatus();
                    model.Department = _departmentTranslator.Translate(entity.DEPARTMENT);
                    model.Active = entity.Active;
                    model.Id = entity.Course_Registration_Status_Id;
                    model.Programme = _programmeTranslator.Translate(entity.PROGRAMME);
                    model.Session = _sessionTranslator.Translate(entity.SESSION);
                }

                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override COURSE_REGISTRATION_STATUS TranslateToEntity(CourseRegistrationStatus model)
        {
            try
            {
                COURSE_REGISTRATION_STATUS entity = null;
                if (model != null)
                {
                    entity = new COURSE_REGISTRATION_STATUS();
                    entity.Active = model.Active;
                    entity.Course_Registration_Status_Id = model.Id;
                    if (model.Department != null)
                    {
                        entity.Department_Id = model.Department.Id;
                    }
                    if (model.Programme != null)
                    {
                        entity.Programme_Id = model.Programme.Id;
                    }
                    if (model.Session != null)
                    {
                        entity.Session_Id = model.Session.Id;
                    }
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
