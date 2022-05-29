using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Entity.Model;
using Abundance_Nk.Model.Translator;

namespace Abundance_Nk.Business
{
    public class CourseRegistrationStatusLogic : BusinessBaseLogic<CourseRegistrationStatus, COURSE_REGISTRATION_STATUS>
    {
        public CourseRegistrationStatusLogic()
        {
            translator = new CourseRegistrationStatusTranslator();
        }
        public bool Modify(CourseRegistrationStatus model)
        {
            try
            {
                COURSE_REGISTRATION_STATUS entity = GetEntityBy(r => r.Course_Registration_Status_Id == model.Id);

                if (entity == null)
                {
                    return false;
                }

                entity.Active = model.Active;

                if (model.Programme != null)
                {
                    entity.Programme_Id = model.Programme.Id;
                }
                if (model.Department != null)
                {
                    entity.Department_Id = model.Department.Id;
                }


                if (model.Session != null)
                {
                    entity.Session_Id = model.Session.Id;
                }

                int modifiedRecordCount = Save();
                if (modifiedRecordCount <= 0)
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
