using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Abundance_Nk.Model.Model;
using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Translator;
using System.Linq.Expressions;
using System.Transactions;

namespace Abundance_Nk.Business
{
    public class AdmissionCriteriaForOLevelSubjectLogic : BusinessBaseLogic<AdmissionCriteriaForOLevelSubject, ADMISSION_CRITERIA_FOR_O_LEVEL_SUBJECT>
    {
        public AdmissionCriteriaForOLevelSubjectLogic()
        {
            translator = new AdmissionCriteriaForOLevelSubjectTranslator();
        }

        public bool Modify (List<AdmissionCriteriaForOLevelSubject> subjects)
        {
            try
            {

                using (TransactionScope scope = new TransactionScope())
                {
                    foreach (AdmissionCriteriaForOLevelSubject subject in subjects)
                    {
                        if (subject.MinimumGrade.Id <= 0 || subject.Subject.Id <= 0)
                        {
                            continue;
                        }
                        Expression<Func<ADMISSION_CRITERIA_FOR_O_LEVEL_SUBJECT, bool>> selector = a => a.Admission_Criteria_For_O_Level_Subject_Id == subject.Id;
                        ADMISSION_CRITERIA_FOR_O_LEVEL_SUBJECT criteria = GetEntityBy(selector);

                        if (criteria == null)
                        {
                            AdmissionCriteriaForOLevelSubject admissionCriteriaForOLevelSubject = new AdmissionCriteriaForOLevelSubject();
                            admissionCriteriaForOLevelSubject.MinimumGrade = subject.MinimumGrade;
                            admissionCriteriaForOLevelSubject.Subject = subject.Subject;
                            admissionCriteriaForOLevelSubject.IsCompulsory = subject.IsCompulsory;
                            admissionCriteriaForOLevelSubject.MainCriteria = subject.MainCriteria;

                            AdmissionCriteriaForOLevelSubject newAdmissionCriteriaForOLevelSubject = Create(admissionCriteriaForOLevelSubject);
                            criteria = GetEntityBy(c => c.Admission_Criteria_For_O_Level_Subject_Id == newAdmissionCriteriaForOLevelSubject.Id);

                        }
                        else
                        {
                            if (true)
                            {

                            }
                            criteria.O_Level_Subject_Id = subject.Subject.Id;
                            criteria.Is_Compulsory = subject.IsCompulsory;
                            criteria.Minimum_O_Level_Grade_Id = subject.MinimumGrade.Id;
                            int modifiedRecordCount = Save(); 
                        }
                        

                        if (subject.Alternatives != null && subject.Alternatives[0].OLevelSubject.Id > 0)
                        {
                            AdmissionCriteriaForOLevelSubjectAlternativeLogic criteriaSubjectAlternativeLogic = new AdmissionCriteriaForOLevelSubjectAlternativeLogic();
           
                            foreach (AdmissionCriteriaForOLevelSubjectAlternative subjectAlternative in subject.Alternatives)
                            {
                                AdmissionCriteriaForOLevelSubjectAlternative criteriaAlternative = criteriaSubjectAlternativeLogic.GetModelsBy(a => a.Admission_Criteria_For_O_Level_Subject_Id == subject.Id && a.O_Level_Subject_Id == subjectAlternative.OLevelSubject.Id).FirstOrDefault();


                                if (criteriaAlternative == null)
                                {
                                    AdmissionCriteriaForOLevelSubjectAlternative criteriaAlternativeSubject = new AdmissionCriteriaForOLevelSubjectAlternative();
                                    criteriaAlternativeSubject.OLevelSubject = subjectAlternative.OLevelSubject;
                                    criteriaAlternativeSubject.Alternative = new AdmissionCriteriaForOLevelSubject(){ Id = criteria.Admission_Criteria_For_O_Level_Subject_Id};
                                    criteriaSubjectAlternativeLogic.Create(criteriaAlternativeSubject);
                                }
                                else
                                {
                                    criteriaAlternative.Alternative = subject;
                                    criteriaAlternative.OLevelSubject = subjectAlternative.OLevelSubject;

                                    criteriaSubjectAlternativeLogic.ModifyOnly(criteriaAlternative);
                                    //int modifiedRecordCount = criteriaSubjectAlternativeLogic.Save();
                                }
                            }
                        }
                        else if (subject.Alternatives != null && subject.Alternatives[0].OLevelSubject.Id == 0)
                        {
                            AdmissionCriteriaForOLevelSubjectAlternativeLogic criteriaSubjectAlternativeLogic = new AdmissionCriteriaForOLevelSubjectAlternativeLogic();
                            AdmissionCriteriaForOLevelSubjectAlternative criteriaAlternative = criteriaSubjectAlternativeLogic.GetModelsBy(a => a.Admission_Criteria_For_O_Level_Subject_Id == subject.Id).FirstOrDefault();

                            if (criteriaAlternative != null)
                            {
                                criteriaSubjectAlternativeLogic.Delete(c => c.Admission_Criteria_For_O_Level_Subject_Alternative_Id == criteriaAlternative.Id);
                            }
                        }

                        if (subject.OtherAlternatives != null && subject.OtherAlternatives[0].OLevelSubject.Id > 0)
                        {
                            AdmissionCriteriaForOLevelSubjectAlternativeLogic criteriaSubjectAlternativeLogic = new AdmissionCriteriaForOLevelSubjectAlternativeLogic();

                            foreach (AdmissionCriteriaForOLevelSubjectAlternative subjectAlternative in subject.OtherAlternatives)
                            {
                                AdmissionCriteriaForOLevelSubjectAlternative criteriaAlternative = criteriaSubjectAlternativeLogic.GetModelsBy(a => a.Admission_Criteria_For_O_Level_Subject_Id == subject.Id && a.O_Level_Subject_Id == subjectAlternative.OLevelSubject.Id).FirstOrDefault();


                                if (criteriaAlternative == null)
                                {
                                    AdmissionCriteriaForOLevelSubjectAlternative criteriaAlternativeSubject = new AdmissionCriteriaForOLevelSubjectAlternative();
                                    criteriaAlternativeSubject.OLevelSubject = subjectAlternative.OLevelSubject;
                                    criteriaAlternativeSubject.Alternative = new AdmissionCriteriaForOLevelSubject() { Id = criteria.Admission_Criteria_For_O_Level_Subject_Id };
                                    criteriaSubjectAlternativeLogic.Create(criteriaAlternativeSubject);
                                }
                                else
                                {
                                    criteriaAlternative.Alternative = subject;
                                    criteriaAlternative.OLevelSubject = subjectAlternative.OLevelSubject;

                                    criteriaSubjectAlternativeLogic.ModifyOnly(criteriaAlternative);
                                    //int modifiedRecordCount = criteriaSubjectAlternativeLogic.Save();
                                }
                            }
                        }
                        else if (subject.OtherAlternatives != null && subject.OtherAlternatives[0].OLevelSubject.Id == 0)
                        {
                            AdmissionCriteriaForOLevelSubjectAlternativeLogic criteriaSubjectAlternativeLogic = new AdmissionCriteriaForOLevelSubjectAlternativeLogic();
                            AdmissionCriteriaForOLevelSubjectAlternative criteriaAlternative = criteriaSubjectAlternativeLogic.GetModelsBy(a => a.Admission_Criteria_For_O_Level_Subject_Id == subject.Id).FirstOrDefault();

                            //if (criteriaAlternative != null)
                            //{
                            //    criteriaSubjectAlternativeLogic.Delete(c => c.Admission_Criteria_For_O_Level_Subject_Alternative_Id == criteriaAlternative.Id);
                            //}
                        }
                    }
                    scope.Complete();
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
