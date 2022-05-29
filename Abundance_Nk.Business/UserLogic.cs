using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Translator;
using System.Linq.Expressions;
using System.Web;

namespace Abundance_Nk.Business
{
    public class UserLogic : BusinessBaseLogic<User, USER>
    {
       public UserLogic()
       {
           translator = new UserTranslator();
       }

        public bool ValidateUser(string Username, string Password)
       {
           try
           {
               Expression<Func<USER, bool>> selector = p => p.User_Name == Username && p.Password == Password && p.Activated == true && p.Archive==false;
               User UserDetails = GetModelBy(selector);
               if (UserDetails != null && UserDetails.Password != null)
               {
                   UpdateLastLogin(UserDetails);
                   return true;
               }
               else
               {
                   return false;
               }
           }
           catch (Exception)
           {
               throw;
           }
       }

        public bool UpdateLastLogin(User user)
        {
            try
            {
                Expression<Func<USER, bool>> selector = p => p.User_Name == user.Username && p.Password == user.Password && p.User_Id==user.Id;
                USER userEntity = GetEntityBy(selector);
                if (userEntity == null || userEntity.User_Id <= 0)
                {
                    throw new Exception(NoItemFound);
                }

                userEntity.User_Name = user.Username;
                userEntity.Password = user.Password;
                userEntity.Email = user.Email;
                userEntity.Role_Id = user.Role.Id;
                userEntity.Security_Question_Id = user.SecurityQuestion.Id;
                userEntity.Security_Answer = user.SecurityAnswer;
                userEntity.LastLoginDate = DateTime.Now;
                userEntity.Activated = user.Activated;
                userEntity.PasswordChanged = user.PasswordChanged;
                userEntity.Super_Admin = user.SuperAdmin;
                userEntity.Signature_Url = user.SignatureUrl;
                userEntity.Profile_Image_Url = user.ProfileImageUrl;
                userEntity.Archive = user.Archieved;

                int modifiedRecordCount = Save();
                if (modifiedRecordCount <= 0)
                {
                    throw new Exception(NoItemModified);
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool ChangeUserPassword(User user)
        {
            try
            {
                Expression<Func<USER, bool>> selector = p => p.User_Name == user.Username;
                USER userEntity = GetEntityBy(selector);
                if (userEntity == null || userEntity.User_Id <= 0)
                {
                    throw new Exception(NoItemFound);
                }

                userEntity.User_Name = user.Username;
                userEntity.Password = user.Password;
                userEntity.Email = user.Email;
                userEntity.Role_Id = user.Role.Id;
                userEntity.Security_Question_Id = user.SecurityQuestion.Id;
                userEntity.Security_Answer = user.SecurityAnswer;
                userEntity.LastLoginDate = user.LastLoginDate;
                userEntity.Activated = user.Activated;
                userEntity.PasswordChanged = user.PasswordChanged;
                userEntity.Super_Admin = user.SuperAdmin;
                userEntity.Signature_Url = user.SignatureUrl;
                userEntity.Profile_Image_Url = user.ProfileImageUrl;
                userEntity.Archive = user.Archieved;

                int modifiedRecordCount = Save();
                if (modifiedRecordCount <= 0)
                {
                    throw new Exception(NoItemModified);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public User Update(User model)
        {
            try
            {
                Expression<Func<USER, bool>> selector = a => a.User_Id == model.Id;
                USER entity = GetEntityBy(selector);

                entity.User_Name = model.Username;
                entity.Password = model.Password;
                entity.Email = model.Email;
                entity.Activated = model.Activated;
                entity.PasswordChanged = model.PasswordChanged;
                entity.Super_Admin = model.SuperAdmin;
                entity.Signature_Url = model.SignatureUrl;
                entity.Profile_Image_Url = model.ProfileImageUrl;
                entity.Archive = model.Archieved;
                if (model.Role != null)
                {
                    entity.Role_Id = model.Role.Id;  
                }   
                entity.Security_Answer = model.SecurityAnswer;
                entity.Security_Question_Id = model.SecurityQuestion.Id;

                int modifiedRecordCount = Save();
                if (modifiedRecordCount <= 0)
                {
                    throw new Exception(NoItemModified);
                }

                return model;
            }
            catch (Exception)
            {

                throw;
            }
        }

       public bool Modify(User model)
        {
            try
            {
                Expression<Func<USER, bool>> selector = u => u.User_Id == model.Id;
                USER entity = GetEntityBy(selector);
                if (entity == null)
                {
                    throw new Exception(NoItemFound);
                }

                entity.User_Id = model.Id;
                if (model.Password != null)
                {
                    entity.Password = model.Password;  
                }
                
                if (model.Role != null)
                {
                    entity.Role_Id = model.Role.Id;
                }
                if (model.Activated != null)
                {
                    entity.Activated = model.Activated;
                }
                if (model.PasswordChanged != null)
                {
                    entity.PasswordChanged = model.PasswordChanged;
                }
                if (model.SuperAdmin != null)
                {
                    entity.Super_Admin = model.SuperAdmin;
                }
                if (model.SignatureUrl != null)
                {
                    entity.Signature_Url = model.SignatureUrl;
                }
                if (model.ProfileImageUrl != null)
                {
                    entity.Profile_Image_Url = model.ProfileImageUrl;
                }
                entity.Archive = model.Archieved;
                int modifiedRecordCount = Save();
                if (modifiedRecordCount <= 0)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
