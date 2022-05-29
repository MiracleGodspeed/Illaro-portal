using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Entity.Model;
using Abundance_Nk.Model.Translator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Business
{
    public class DrivingExperienceLogic: BusinessBaseLogic<DrivingExperience, DRIVING_EXPERIENCE>
    {
        public DrivingExperienceLogic()
        {
            translator = new DrivingExperienceTranslator();
        }

        public bool Modify(DrivingExperience drivingExperience)
        {
            try
            {
                Expression<Func<DRIVING_EXPERIENCE, bool>> selector = f => f.Person_Id == drivingExperience.PersonId;
                DRIVING_EXPERIENCE entity = GetEntityBy(selector);
                if (entity != null && entity.Person_Id > 0)
                {
                    if(drivingExperience.LicenseNumber != null)
                    {
                        entity.License_Number = drivingExperience.LicenseNumber;
                    }
                    if(!drivingExperience.FacialMarks)
                    {
                        entity.Facial_Marks = false;
                    }
                    else
                    {
                        entity.Facial_Marks = true;
                    }
                    if(drivingExperience.Height > 0)
                    {
                        entity.Height = drivingExperience.Height;

                    }
                    if(drivingExperience.Licence_Type_Id > 0)
                    {
                        entity.Licence_Type_Id = drivingExperience.Licence_Type_Id;

                    }
                    if(drivingExperience.IssuedDate != null)
                    {
                        entity.Issued_Date = drivingExperience.IssuedDate;

                    }
                    if(drivingExperience.ExpiryDate != null)
                    {
                        entity.Expiry_Date = drivingExperience.ExpiryDate;

                    }
                    if(drivingExperience.YearsOfExperience > 0)
                    {
                        entity.Years_Of_Experience = drivingExperience.YearsOfExperience;

                    }

                    if (drivingExperience.Application_Form_Id > 0)
                    {
                        entity.Application_Form_Id = drivingExperience.Application_Form_Id;

                    }

                    int modifiedRecordCount = Save();

                    if (modifiedRecordCount > 0)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}
