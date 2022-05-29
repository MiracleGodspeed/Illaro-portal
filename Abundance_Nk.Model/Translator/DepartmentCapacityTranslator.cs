using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Entity.Model;

namespace Abundance_Nk.Model.Translator
{
    public class DepartmentCapacityTranslator : TranslatorBase<DepartmentCapacity, DEPARTMENT_CAPACITY>
    {
        private ProgrammeTranslator programmeTranslator;
        private DepartmentTranslator departmentTranslator;
        private SessionTranslator sessionTranslator;

        public DepartmentCapacityTranslator()
        {
            programmeTranslator = new ProgrammeTranslator();
            departmentTranslator = new DepartmentTranslator();
            sessionTranslator = new SessionTranslator();
        }

        public override DepartmentCapacity TranslateToModel(DEPARTMENT_CAPACITY entity)
        {
            try
            {
                DepartmentCapacity model = null;
                if (entity != null)
                {
                    model = new DepartmentCapacity();
                    model.Activated = entity.Activated;
                    model.Capacity = entity.Capacity;
                    model.Department = departmentTranslator.Translate(entity.DEPARTMENT);
                    model.Programme = programmeTranslator.Translate(entity.PROGRAMME);
                    model.Session = sessionTranslator.Translate(entity.SESSION);
                    model.Id = entity.Department_Capacity_Id;
                }

                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override DEPARTMENT_CAPACITY TranslateToEntity(DepartmentCapacity model)
        {
            try
            {
                DEPARTMENT_CAPACITY entity = null;
                if (model != null)
                {
                    entity = new DEPARTMENT_CAPACITY();
                    entity.Activated = model.Activated;
                    entity.Capacity = model.Capacity;
                    entity.Department_Capacity_Id = model.Id;
                    entity.Department_Id = model.Department.Id;
                    entity.Programme_Id = model.Programme.Id;
                    entity.Session_Id = model.Session.Id;
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
