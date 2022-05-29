using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Entity.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Translator
{
    public class DrivingExperienceTranslator : TranslatorBase<DrivingExperience, DRIVING_EXPERIENCE>
    {
        private LicenseTypeTranslator licenceTypeTranslator;
        private ApplicationFormTranslator applicationFormTranslator;
        private PersonTranslator personTranslator;
        public DrivingExperienceTranslator()
        {
            licenceTypeTranslator = new LicenseTypeTranslator();
            applicationFormTranslator = new ApplicationFormTranslator();
            personTranslator = new PersonTranslator();
        }
        public override DrivingExperience TranslateToModel(DRIVING_EXPERIENCE entity)
        {
            try
            {
                DrivingExperience model = null;
                if (entity != null)
                {
                    model = new DrivingExperience();
                    model.Id = entity.Driving_Experience_Id;
                    model.LicenseNumber = entity.License_Number;
                    model.LicenseType = licenceTypeTranslator.Translate(entity.LICENSE_TYPE);
                    model.FacialMarks = entity.Facial_Marks;
                    model.Height = entity.Height;
                    model.IssuedDate = entity.Issued_Date;
                    model.ExpiryDate = entity.Expiry_Date;
                    model.ApplicationForm = applicationFormTranslator.Translate(entity.APPLICATION_FORM);
                    model.YearsOfExperience = entity.Years_Of_Experience;
                    //model.Person = personTranslator.Translate(entity.PERSON);
                    model.PersonId = entity.Person_Id;
                    
                   
                }

                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override DRIVING_EXPERIENCE TranslateToEntity(DrivingExperience model)
        {
            try
            {
                DRIVING_EXPERIENCE entity = null;
                if (model != null)
                {
                    entity = new DRIVING_EXPERIENCE();
                    entity.Driving_Experience_Id = model.Id;
                    entity.License_Number = model.LicenseNumber;
                    entity.Licence_Type_Id = model.Licence_Type_Id;
                    entity.Issued_Date = model.IssuedDate;
                    entity.Expiry_Date = model.ExpiryDate;
                    entity.Height = model.Height;
                    entity.Years_Of_Experience = model.YearsOfExperience;
                    entity.Facial_Marks = model.FacialMarks;
                    if(model.Application_Form_Id > 0)
                    {
                        entity.Application_Form_Id = model.Application_Form_Id;
                    }
                    //entity.PERSON = personTranslator.Translate(model.Person);
                    entity.Person_Id = model.PersonId;

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
