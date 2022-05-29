using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Translator;
using System.Linq.Expressions;

namespace Abundance_Nk.Business
{
   public class PersonAuditLogic : BusinessBaseLogic<PersonAudit, PERSON_AUDIT>
    {
       public PersonAuditLogic()
        {
            translator = new PersonAuditTranslator();
        }


       public bool Add(Person OldDetails, Person newDetails, User userLoggedIn)
       {
           try
           {
              
               PERSON_AUDIT personAuditEntity = new PERSON_AUDIT();

               if (OldDetails.FirstName != null)
               {
                   personAuditEntity.First_Name = OldDetails.FirstName;
               }
               if (OldDetails.LastName != null)
               {
                   personAuditEntity.Last_Name = OldDetails.LastName;
               }
               if (OldDetails.OtherName != null)
               {
                   personAuditEntity.Other_Name = OldDetails.OtherName;
               }
               if (OldDetails.ContactAddress != null)
               {
                   personAuditEntity.Contact_Address = OldDetails.ContactAddress;
               }
               if (OldDetails.Email != null)
               {
                   personAuditEntity.Email = OldDetails.Email;
               }
               if (OldDetails.MobilePhone != null)
               {
                   personAuditEntity.Mobile_Phone = OldDetails.MobilePhone;
               }
               if (OldDetails.SignatureFileUrl != null)
               {
                   personAuditEntity.Signature_File_Url = OldDetails.SignatureFileUrl;
               }
               if (OldDetails.ImageFileUrl != null)
               {
                   personAuditEntity.Image_File_Url = OldDetails.ImageFileUrl;
               }
               if (OldDetails.DateOfBirth != null)
               {
                   personAuditEntity.Date_Of_Birth = OldDetails.DateOfBirth;
               }
               if (OldDetails.HomeTown != null)
               {
                   personAuditEntity.Home_Town = OldDetails.HomeTown;
               }
               if (OldDetails.HomeAddress != null)
               {
                   personAuditEntity.Home_Address = OldDetails.HomeAddress;
               }
               if (OldDetails.DateEntered != null)
               {
                   personAuditEntity.Date_Entered = OldDetails.DateEntered;
               }
               if (OldDetails.Initial != null)
               {
                   personAuditEntity.Initial = OldDetails.Initial;
               }
               if (OldDetails.Title != null)
               {
                   personAuditEntity.Title = OldDetails.Title;
               }

               if (OldDetails.Role != null && OldDetails.Role.Id > 0)
               {
                   personAuditEntity.Role_Id = OldDetails.Role.Id;
               }
               if (OldDetails.Nationality != null && OldDetails.Nationality.Id > 0)
               {
                   personAuditEntity.Nationality_Id = OldDetails.Nationality.Id;
               }
               if (OldDetails.State != null && !string.IsNullOrEmpty(OldDetails.State.Id))
               {
                   personAuditEntity.State_Id = OldDetails.State.Id;
               }
               if (OldDetails.Type != null && OldDetails.Type.Id > 0)
               {
                   personAuditEntity.Person_Type_Id = OldDetails.Type.Id;
               }
               if (OldDetails.Religion != null)
               {
                   personAuditEntity.Religion_Id = OldDetails.Religion.Id;
               }
               if (OldDetails.LocalGovernment != null)
               {
                   personAuditEntity.Local_Government_Id = OldDetails.LocalGovernment.Id;
               }
               if (OldDetails.Sex != null)
               {
                   personAuditEntity.Sex_Id = OldDetails.Sex.Id;
               }


              
               return true;
           }
           catch (Exception)
           {
               throw;
           }
       }
        public List<GeneralAudit> GetAudits()
        {
            List<GeneralAudit> audits = new List<GeneralAudit>();
            try
            {
                audits = (from s in repository.GetBy<PERSON_AUDIT>(s => s.User_Id > 0)
                          select new GeneralAudit
                          {
                              Time = s.Time,
                              Action = s.Action,
                              Operation = s.Operation,
                              Client = s.Client,
                              UserId = s.User_Id,
                              Email = s.Email,
                              OldEmail = s.Old_Email,
                              Date = s.Time.ToLongDateString(),
                              FirstName = s.First_Name,
                              OldFirstName = s.Old_First_Name,
                              LastName = s.Last_Name,
                              OldLastName = s.Old_Last_Name,
                              Title = s.Title,
                              OldTitle = s.Old_Title,
                              ContactAddress = s.Contact_Address,
                              OldContactAddress = s.Old_Contact_Address,
                              MobilePhone = s.Mobile_Phone,
                              OldMobilePhone = s.Old_Mobile_Phone,
                              DOB = s.Date_Of_Birth != null ? s.Date_Of_Birth.ToString() : null,
                              OldDOB = s.Old_Date_Of_Birth != null ? s.Old_Date_Of_Birth.ToString() : null,
                              Sex = s.SEX != null ? s.SEX.Sex_Name : null,
                              OldSex = s.SEX1 != null ? s.SEX1.Sex_Name : null,
                              State = s.STATE != null ? s.STATE.State_Name : null,
                              OldState = s.STATE1 != null ? s.STATE1.State_Name : null,
                              LGA = s.LOCAL_GOVERNMENT != null ? s.LOCAL_GOVERNMENT.Local_Government_Name : null,
                              OldLGA = s.LOCAL_GOVERNMENT1 != null ? s.LOCAL_GOVERNMENT1.Local_Government_Name : null,
                              HomeAddress = s.Home_Address,
                              OldHomeAddress = s.Old_Home_Address,
                              HomeTown = s.Home_Town,
                              OldHomeTown = s.Old_Home_Town,
                              Religion = s.RELIGION != null ? s.RELIGION.Religion_Name : null,
                              OldReligion = s.RELIGION1 != null ? s.RELIGION1.Religion_Name : null
                          }).ToList();

                UserLogic userLogic = new UserLogic();
                
                List<USER> users = userLogic.GetEntitiesBy(u => u.User_Id > 0);

                audits.ForEach(p =>
                {
                    AssignAuditValues(p, users);
                });

                return audits;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static void AssignAuditValues(GeneralAudit p, List<USER> users)
        {
            try
            {
                USER user = users.Where(u => u.User_Id == p.UserId).LastOrDefault();
                p.Username = user.User_Name;
                p.RoleId = user.Role_Id;
                p.Role = user.ROLE.Role_Name;
                p.IsSuperAdmin = user.Super_Admin == true;

                if (p.Email != null && p.Email != p.OldEmail)
                {
                    if (!string.IsNullOrEmpty(p.Email))
                        p.CurrentValues += p.Email + ", ";
                    if (!string.IsNullOrEmpty(p.OldEmail))
                        p.InitialValues += p.OldEmail + ", ";
                }
                if (p.FirstName != null && p.FirstName != p.OldFirstName)
                {
                    if (!string.IsNullOrEmpty(p.FirstName))
                        p.CurrentValues += p.FirstName + ", ";
                    if (!string.IsNullOrEmpty(p.OldFirstName))
                        p.InitialValues += p.OldFirstName + ", ";
                }
                if (p.LastName != null && p.LastName != p.OldLastName)
                {
                    if (!string.IsNullOrEmpty(p.LastName))
                        p.CurrentValues += p.LastName + ", ";
                    if (!string.IsNullOrEmpty(p.OldLastName))
                        p.InitialValues += p.OldLastName + ", ";
                }
                if (p.Title != null && p.Title != p.OldTitle)
                {
                    if (!string.IsNullOrEmpty(p.Title))
                        p.CurrentValues += p.Title + ", ";
                    if (!string.IsNullOrEmpty(p.OldTitle))
                        p.InitialValues += p.OldTitle + ", ";
                }
                if (p.ContactAddress != null && p.ContactAddress != p.OldContactAddress)
                {
                    if (!string.IsNullOrEmpty(p.ContactAddress))
                        p.CurrentValues += p.ContactAddress + ", ";
                    if (!string.IsNullOrEmpty(p.OldContactAddress))
                        p.InitialValues += p.OldContactAddress + ", ";
                }
                if (p.MobilePhone != null && p.MobilePhone != p.OldMobilePhone)
                {
                    if (!string.IsNullOrEmpty(p.MobilePhone))
                        p.CurrentValues += p.MobilePhone + ", ";
                    if (!string.IsNullOrEmpty(p.OldMobilePhone))
                        p.InitialValues += p.OldMobilePhone + ", ";
                }
                if (p.MobilePhone != null && p.MobilePhone != p.OldMobilePhone)
                {
                    if (!string.IsNullOrEmpty(p.MobilePhone))
                        p.CurrentValues += p.MobilePhone + ", ";
                    if (!string.IsNullOrEmpty(p.OldMobilePhone))
                        p.InitialValues += p.OldMobilePhone + ", ";
                }
                if (p.DOB != null && p.DOB != p.OldDOB)
                {
                    if (!string.IsNullOrEmpty(p.DOB))
                        p.CurrentValues += p.DOB + ", ";
                    if (!string.IsNullOrEmpty(p.OldDOB))
                        p.InitialValues += p.OldDOB + ", ";
                }
                if (p.Sex != null && p.Sex != p.OldSex)
                {
                    if (!string.IsNullOrEmpty(p.Sex))
                        p.CurrentValues += p.Sex + ", ";
                    if (!string.IsNullOrEmpty(p.OldSex))
                        p.InitialValues += p.OldSex + ", ";
                }
                if (p.State != null && p.State != p.OldState)
                {
                    if (!string.IsNullOrEmpty(p.State))
                        p.CurrentValues += p.State + ", ";
                    if (!string.IsNullOrEmpty(p.OldState))
                        p.InitialValues += p.OldState + ", ";
                }
                if (p.LGA != null && p.LGA != p.OldLGA)
                {
                    if (!string.IsNullOrEmpty(p.LGA))
                        p.CurrentValues += p.LGA + ", ";
                    if (!string.IsNullOrEmpty(p.OldLGA))
                        p.InitialValues += p.OldLGA + ", ";
                }
                if (p.HomeAddress != null && p.HomeAddress != p.OldHomeAddress)
                {
                    if (!string.IsNullOrEmpty(p.HomeAddress))
                        p.CurrentValues += p.HomeAddress + ", ";
                    if (!string.IsNullOrEmpty(p.OldHomeAddress))
                        p.InitialValues += p.OldHomeAddress + ", ";
                }
                if (p.HomeTown != null && p.HomeTown != p.OldHomeTown)
                {
                    if (!string.IsNullOrEmpty(p.HomeTown))
                        p.CurrentValues += p.HomeTown + ", ";
                    if (!string.IsNullOrEmpty(p.OldHomeTown))
                        p.InitialValues += p.OldHomeTown + ", ";
                }
                if (p.Religion != null && p.Religion != p.OldReligion)
                {
                    if (!string.IsNullOrEmpty(p.Religion))
                        p.CurrentValues += p.Religion + ", ";
                    if (!string.IsNullOrEmpty(p.OldReligion))
                        p.InitialValues += p.OldReligion + ", ";
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
