using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Abundance_Nk.Model.Model;
using Abundance_Nk.Model.Entity;

namespace Abundance_Nk.Model.Translator
{
    public class UserTranslator : TranslatorBase<User, USER>
    {
        private RoleTranslator roleTranslator;
        private SecurityQuestionTranslator securityQuestionTranslator;

        public UserTranslator()
        {
            roleTranslator = new RoleTranslator();
            securityQuestionTranslator = new SecurityQuestionTranslator();
        }

        public override User TranslateToModel(USER entity)
        {
            try
            {
                User model = null;
                if (entity != null)
                {
                    model = new User();
                    model.Id = entity.User_Id;
                    model.Username = entity.User_Name;
                    model.Password = entity.Password;
                    model.Email = entity.Email;
                    model.SecurityQuestion = securityQuestionTranslator.Translate(entity.SECURITY_QUESTION);
                    model.SecurityAnswer = entity.Security_Answer;
                    model.Role = roleTranslator.Translate(entity.ROLE);
                    model.LastLoginDate = entity.LastLoginDate;
                    model.Activated = entity.Activated;
                    model.PasswordChanged = entity.PasswordChanged;
                    model.SuperAdmin = entity.Super_Admin;
                    model.SignatureUrl = entity.Signature_Url;
                    model.ProfileImageUrl = entity.Profile_Image_Url;
                    model.Archieved = entity.Archive;
                }

                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override USER TranslateToEntity(User model)
        {
            try
            {
                USER entity = null;
                if (model != null)
                {
                    entity = new USER();
                    entity.User_Id = model.Id;
                    entity.User_Name = model.Username;
                    entity.Password = model.Password;
                    entity.Email = model.Email;
                    entity.Security_Question_Id = model.SecurityQuestion.Id;
                    entity.Security_Answer = model.SecurityAnswer;
                    entity.Role_Id = model.Role.Id;
                    entity.LastLoginDate = model.LastLoginDate;
                    entity.Activated = model.Activated;
                    entity.PasswordChanged = model.PasswordChanged;
                    entity.Super_Admin = model.SuperAdmin;
                    entity.Signature_Url = model.SignatureUrl;
                    entity.Profile_Image_Url = model.ProfileImageUrl;
                    entity.Archive = model.Archieved;
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
