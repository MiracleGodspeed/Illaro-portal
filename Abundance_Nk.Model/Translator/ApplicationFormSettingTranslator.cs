using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Abundance_Nk.Model.Model;
using Abundance_Nk.Model.Entity;

namespace Abundance_Nk.Model.Translator
{
    public class ApplicationFormSettingTranslator : TranslatorBase<ApplicationFormSetting, APPLICATION_FORM_SETTING>
    {
        private UserTranslator userTranslator;
        private PersonTypeTranslator personTypeTranslator;
        private PaymentTypeTranslator paymentTypeTranslator;
        private PaymentModeTranslator paymentModeTranslator;
        private SessionTranslator sessionTranslator;

        public ApplicationFormSettingTranslator()
        {
            userTranslator = new UserTranslator();
            personTypeTranslator = new PersonTypeTranslator();
            paymentTypeTranslator = new PaymentTypeTranslator();
            paymentModeTranslator = new PaymentModeTranslator();
            sessionTranslator = new SessionTranslator();
        }

        public override ApplicationFormSetting TranslateToModel(APPLICATION_FORM_SETTING entity)
        {
            try
            {
                ApplicationFormSetting model = null;
                if (entity != null)
                {
                    model = new ApplicationFormSetting();
                    model.Id = entity.Application_Form_Setting_Id;
                    model.PersonType = personTypeTranslator.Translate(entity.PERSON_TYPE);
                    model.PaymentType = paymentTypeTranslator.Translate(entity.PAYMENT_TYPE);
                    model.PaymentMode = paymentModeTranslator.Translate(entity.PAYMENT_MODE);
                    model.Session = sessionTranslator.Translate(entity.SESSION);
                    model.StartDate = entity.Start_Date;
                    model.EndDate = entity.End_Date;
                    model.DateEntered = entity.Date_Entered;
                    model.EnteredBy = userTranslator.Translate(entity.USER);
                    model.ExamDate = entity.Exam_Date;
                    model.ExamVenue = entity.Exam_Venue;
                    model.ExamTime = entity.Exam_Time;
                    model.RegistrationEndDate = entity.RegistrationEndDate;
                    model.RegistrationEndTime = entity.RegistrationEndTime;
                    if (model.RegistrationEndTime.HasValue)
                    {
                        model.RegistrationEndTimeString = DateTime.Today.Add(model.RegistrationEndTime.Value).ToString("hh:mm tt");
                    }
                }

                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override APPLICATION_FORM_SETTING TranslateToEntity(ApplicationFormSetting model)
        {
            try
            {
                APPLICATION_FORM_SETTING entity = null;
                if (model != null)
                {
                    entity = new APPLICATION_FORM_SETTING();
                    entity.Application_Form_Setting_Id = model.Id;
                    entity.Person_Type_Id = model.PersonType.Id;
                    entity.Payment_Type_Id = model.PaymentType.Id;
                    entity.Payment_Mode_Id = model.PaymentMode.Id;
                    entity.Session_Id = model.Session.Id;
                    entity.Start_Date = model.StartDate;
                    entity.End_Date = model.EndDate;
                    entity.Date_Entered = model.DateEntered;
                    entity.Entered_By_User_Id = model.EnteredBy.Id;
                    entity.Exam_Date = model.ExamDate;
                    entity.Exam_Venue = model.ExamVenue;
                    entity.Exam_Time = model.ExamTime;
                    entity.RegistrationEndDate = model.RegistrationEndDate;
                    entity.RegistrationEndTime = model.RegistrationEndTime;
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
