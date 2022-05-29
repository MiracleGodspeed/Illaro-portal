using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Abundance_Nk.Model.Model;
using Abundance_Nk.Model.Entity;

namespace Abundance_Nk.Model.Translator
{
    public class DepartmentTranslator : TranslatorBase<Department, DEPARTMENT>
    {
        private FacultyTranslator facultyTranslator;

        public DepartmentTranslator()
        {
            facultyTranslator = new FacultyTranslator();
        }

        public override Department TranslateToModel(DEPARTMENT entity)
        {
            try
            {
                Department department = null;
                if (entity != null)
                {
                    department = new Department();
                    department.Id = entity.Department_Id;
                    department.Name = entity.Department_Name.ToLower();
                    department.Code = entity.Department_Code;
                    department.Faculty = facultyTranslator.Translate(entity.FACULTY);
                    department.Active = entity.Active;
                }

                return department;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override DEPARTMENT TranslateToEntity(Department department)
        {
            try
            {
                DEPARTMENT entity = null;
                if (department != null)
                {
                    entity = new DEPARTMENT();
                    entity.Department_Id = department.Id;
                    entity.Department_Name = department.Name;
                    entity.Department_Code = department.Code;
                    entity.Faculty_Id = department.Faculty.Id;
                    entity.Active = department.Active;
                    
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
