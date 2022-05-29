﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Translator;
using System.Linq.Expressions;
using System.Collections;

namespace Abundance_Nk.Business
{
    public class RankingDataLogic : BusinessBaseLogic<RankingData, RANKING_DATA>
    {
        private OLevelResultLogic oLevelResultLogic;
        private OLevelResultDetailLogic oLevelResultDetailLogic;
        private AdmissionCriteriaForOLevelSubjectLogic admissionCriteriaForOLevelSubjectLogic;
        private AdmissionCriteriaForOLevelTypeLogic admissionCriteriaForOLevelTypeLogic;
        private AdmissionCriteriaForOLevelSubjectAlternativeLogic admissionCriteriaForOLevelSubjectAlternativeLogic;

        public RankingDataLogic()
        {
            translator = new RankingDataTranslator();

            admissionCriteriaForOLevelTypeLogic = new AdmissionCriteriaForOLevelTypeLogic();
            admissionCriteriaForOLevelSubjectLogic = new AdmissionCriteriaForOLevelSubjectLogic();
            admissionCriteriaForOLevelSubjectAlternativeLogic = new AdmissionCriteriaForOLevelSubjectAlternativeLogic();
            oLevelResultDetailLogic = new OLevelResultDetailLogic();
            oLevelResultLogic = new OLevelResultLogic();
        }

        public List<AppliedCourse> GetAppliedCourses()
        {
            AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
            return appliedCourseLogic.GetModelsBy(a => a.Application_Form_Id != null);
        }


        public string RankAllStudents(APPLICANT_APPLIED_COURSE appliedCourse)
        {

            try
            {
                //  AppliedCourse applicantAppliedCourse
               
                //int count = appliedCourses.Count;
                string rejectReason = null;

                //for (int i = 0; i < count; i++)
                //{
                    APPLICANT_APPLIED_COURSE applicantAppliedCourse = appliedCourse;
                    PERSON applicant = applicantAppliedCourse.PERSON;
                    PROGRAMME programme = applicantAppliedCourse.PROGRAMME;
                    DEPARTMENT department = applicantAppliedCourse.DEPARTMENT;

                    //ADMISSION CRITERIA
                    //get admission criteria
                    Expression<Func<ADMISSION_CRITERIA, bool>> selector = ac => ac.Department_Id == department.Department_Id;
                    AdmissionCriteria admissionCriteria = new AdmissionCriteria();
                    AdmissionCriteriaLogic admissionCriteriaLogic = new AdmissionCriteriaLogic();
                    admissionCriteria = admissionCriteriaLogic.GetModelBy(selector);

                    if (!(admissionCriteria == null || admissionCriteria.Id <= 0))
                    {
                        //throw new Exception("No Admission Criteria found for " + programme.Programme_Name + " in " + department.Department_Name);

                        //get admission criteria olevel type
                        Expression<Func<ADMISSION_CRITERIA_FOR_O_LEVEL_TYPE, bool>> otSelector = ac => ac.Admission_Criteria_Id == admissionCriteria.Id;
                        List<ADMISSION_CRITERIA_FOR_O_LEVEL_TYPE> requiredOlevelTypes = admissionCriteriaForOLevelTypeLogic.GetEntitiesBy(otSelector);

                        //get admission criteria olevel subject
                        Expression<Func<ADMISSION_CRITERIA_FOR_O_LEVEL_SUBJECT, bool>> scSelector = s => s.Admission_Criteria_Id == admissionCriteria.Id && s.Is_Compulsory == true;
                        Expression<Func<ADMISSION_CRITERIA_FOR_O_LEVEL_SUBJECT, bool>> osSelector = s => s.Admission_Criteria_Id == admissionCriteria.Id && s.Is_Compulsory == false;
                        List<ADMISSION_CRITERIA_FOR_O_LEVEL_SUBJECT> compulsorySubjects = admissionCriteriaForOLevelSubjectLogic.GetEntitiesBy(scSelector);
                        List<ADMISSION_CRITERIA_FOR_O_LEVEL_SUBJECT> otherSubjects = admissionCriteriaForOLevelSubjectLogic.GetEntitiesBy(osSelector);



                        //APPLICANT
                        //get applicant result
                        Expression<Func<APPLICANT_O_LEVEL_RESULT, bool>> firstSittingOlevelSelector = f => f.Person_Id == applicant.Person_Id && f.O_Level_Exam_Sitting_Id == 1;
                        Expression<Func<APPLICANT_O_LEVEL_RESULT, bool>> secondSittingOlevelSelector = s => s.Person_Id == applicant.Person_Id && s.O_Level_Exam_Sitting_Id == 2;
                        APPLICANT_O_LEVEL_RESULT firstSittingOlevelResult = oLevelResultLogic.GetEntityBy(firstSittingOlevelSelector);
                        APPLICANT_O_LEVEL_RESULT secondSittingOlevelResult = oLevelResultLogic.GetEntityBy(secondSittingOlevelSelector);


                        //get applicant olevel subjects
                        List<APPLICANT_O_LEVEL_RESULT_DETAIL> firstSittingResultDetails = null;
                        List<APPLICANT_O_LEVEL_RESULT_DETAIL> secondSittingResultDetails = null;
                        if (firstSittingOlevelResult != null && firstSittingOlevelResult.Applicant_O_Level_Result_Id > 0)
                        {
                            Expression<Func<APPLICANT_O_LEVEL_RESULT_DETAIL, bool>> firstSittingSubjectSelector = fs => fs.Applicant_O_Level_Result_Id == firstSittingOlevelResult.Applicant_O_Level_Result_Id;
                            firstSittingResultDetails = oLevelResultDetailLogic.GetEntitiesBy(firstSittingSubjectSelector);
                        }

                        if (secondSittingOlevelResult != null && secondSittingOlevelResult.Applicant_O_Level_Result_Id > 0)
                        {
                            Expression<Func<APPLICANT_O_LEVEL_RESULT_DETAIL, bool>> secondSittingSubjectSelector = ss => ss.Applicant_O_Level_Result_Id == secondSittingOlevelResult.Applicant_O_Level_Result_Id;
                            secondSittingResultDetails = oLevelResultDetailLogic.GetEntitiesBy(secondSittingSubjectSelector);
                        }

                        if ((firstSittingResultDetails == null || firstSittingResultDetails.Count <= 0) && (secondSittingResultDetails == null || secondSittingResultDetails.Count <= 0))
                        {
                            return "No O-Level result found for applicant!";
                        }

                        string oLevelTypeRejectReason = null; //CheckOlevelType(requiredOlevelTypes, firstSittingOlevelResult, secondSittingOlevelResult, applicantAppliedCourse);
                        string requiredSubjectsRejectReason = CheckRequiredOlevelSubjects(compulsorySubjects, otherSubjects, firstSittingResultDetails, secondSittingResultDetails, applicantAppliedCourse);

                        if (oLevelTypeRejectReason != null && requiredSubjectsRejectReason != null)
                        {
                            rejectReason = oLevelTypeRejectReason + ". " + requiredSubjectsRejectReason;
                        }
                        else if (oLevelTypeRejectReason == null && requiredSubjectsRejectReason != null)
                        {
                            rejectReason = requiredSubjectsRejectReason;
                        }
                        else if (oLevelTypeRejectReason != null && requiredSubjectsRejectReason == null)
                        {
                            rejectReason = oLevelTypeRejectReason;
                        }

                        if (!string.IsNullOrEmpty(rejectReason))
                        {
                            rejectReason += " Therefore did not qualify for admission.";
                        }
                    }

                    

                //}
                return rejectReason;
            }
            catch (Exception ex)
            {

                int c = 2;
                throw ex;
            }
        }

        //public string RankStudent(AppliedCourse applicantAppliedCourse)
        //{
        //    try
        //    {
        //        Person applicant = applicantAppliedCourse.Person;
        //        Programme programme = applicantAppliedCourse.Programme;
        //        Department department = applicantAppliedCourse.Department;

        //        //ADMISSION CRITERIA
        //        //get admission criteria
        //        Expression<Func<ADMISSION_CRITERIA, bool>> selector = ac => ac.Department_Id == department.Id;
        //        AdmissionCriteria admissionCriteria = new AdmissionCriteria();
        //        AdmissionCriteriaLogic admissionCriteriaLogic = new AdmissionCriteriaLogic();
        //        admissionCriteria = admissionCriteriaLogic.GetModelBy(selector);

        //        if (admissionCriteria == null || admissionCriteria.Id <= 0)
        //        {
        //            throw new Exception("No Admission Criteria found for " + programme.Name + " in " + department.Name);
        //        }

        //        //get admission criteria olevel type
        //        Expression<Func<ADMISSION_CRITERIA_FOR_O_LEVEL_TYPE, bool>> otSelector = ac => ac.Admission_Criteria_Id == admissionCriteria.Id;
        //        List<AdmissionCriteriaForOLevelType> requiredOlevelTypes = admissionCriteriaForOLevelTypeLogic.GetModelsBy(otSelector);

        //        //get admission criteria olevel subject
        //        Expression<Func<ADMISSION_CRITERIA_FOR_O_LEVEL_SUBJECT, bool>> scSelector = s => s.Admission_Criteria_Id == admissionCriteria.Id && s.Is_Compulsory == true;
        //        Expression<Func<ADMISSION_CRITERIA_FOR_O_LEVEL_SUBJECT, bool>> osSelector = s => s.Admission_Criteria_Id == admissionCriteria.Id && s.Is_Compulsory == false;
        //        List<AdmissionCriteriaForOLevelSubject> compulsorySubjects = admissionCriteriaForOLevelSubjectLogic.GetModelsBy(scSelector);
        //        List<AdmissionCriteriaForOLevelSubject> otherSubjects = admissionCriteriaForOLevelSubjectLogic.GetModelsBy(osSelector);



        //        //APPLICANT
        //        //get applicant result
        //        Expression<Func<APPLICANT_O_LEVEL_RESULT, bool>> firstSittingOlevelSelector = f => f.Person_Id == applicant.Id && f.O_Level_Exam_Sitting_Id == 1;
        //        Expression<Func<APPLICANT_O_LEVEL_RESULT, bool>> secondSittingOlevelSelector = s => s.Person_Id == applicant.Id && s.O_Level_Exam_Sitting_Id == 2;
        //        OLevelResult firstSittingOlevelResult = oLevelResultLogic.GetModelBy(firstSittingOlevelSelector);
        //        OLevelResult secondSittingOlevelResult = oLevelResultLogic.GetModelBy(secondSittingOlevelSelector);


        //        //get applicant olevel subjects
        //        List<OLevelResultDetail> firstSittingResultDetails = null;
        //        List<OLevelResultDetail> secondSittingResultDetails = null;
        //        if (firstSittingOlevelResult != null && firstSittingOlevelResult.Id > 0)
        //        {
        //            Expression<Func<APPLICANT_O_LEVEL_RESULT_DETAIL, bool>> firstSittingSubjectSelector = fs => fs.Applicant_O_Level_Result_Id == firstSittingOlevelResult.Id;
        //            firstSittingResultDetails = oLevelResultDetailLogic.GetModelsBy(firstSittingSubjectSelector);
        //        }

        //        if (secondSittingOlevelResult != null && secondSittingOlevelResult.Id > 0)
        //        {
        //            Expression<Func<APPLICANT_O_LEVEL_RESULT_DETAIL, bool>> secondSittingSubjectSelector = ss => ss.Applicant_O_Level_Result_Id == secondSittingOlevelResult.Id;
        //            secondSittingResultDetails = oLevelResultDetailLogic.GetModelsBy(secondSittingSubjectSelector);
        //        }

        //        if ((firstSittingResultDetails == null || firstSittingResultDetails.Count <= 0) && (secondSittingResultDetails == null || secondSittingResultDetails.Count <= 0))
        //        {
        //            return "No O-Level result found for applicant!";
        //        }

        //        string oLevelTypeRejectReason = null; //CheckOlevelType(requiredOlevelTypes, firstSittingOlevelResult, secondSittingOlevelResult, applicantAppliedCourse);
        //        string requiredSubjectsRejectReason = CheckRequiredOlevelSubjects(compulsorySubjects, otherSubjects, firstSittingResultDetails, secondSittingResultDetails, applicantAppliedCourse);



        //        string rejectReason = null;
        //        if (oLevelTypeRejectReason != null && requiredSubjectsRejectReason != null)
        //        {
        //            rejectReason = oLevelTypeRejectReason + ". " + requiredSubjectsRejectReason;
        //        }
        //        else if (oLevelTypeRejectReason == null && requiredSubjectsRejectReason != null)
        //        {
        //            rejectReason = requiredSubjectsRejectReason;
        //        }
        //        else if (oLevelTypeRejectReason != null && requiredSubjectsRejectReason == null)
        //        {
        //            rejectReason = oLevelTypeRejectReason;
        //        }

        //        if (!string.IsNullOrEmpty(rejectReason))
        //        {
        //            rejectReason += " Therefore did not qualify for admission.";
        //        }

        //        return rejectReason;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        private List<APPLICANT_O_LEVEL_RESULT_DETAIL> GetConsolidatedApplicantOlevelResultAboveOrEqualToC6(List<APPLICANT_O_LEVEL_RESULT_DETAIL> firstSittingResultDetails, List<APPLICANT_O_LEVEL_RESULT_DETAIL> secondSittingResultDetails)
        {
            try
            {
                List<APPLICANT_O_LEVEL_RESULT_DETAIL> firstSittingOLevelResultsAboveOrEqualToC6 = GetOLevelResultsAboveOrEqualToC6(firstSittingResultDetails);
                List<APPLICANT_O_LEVEL_RESULT_DETAIL> secondSittingOLevelResultsAboveOrEqualToC6 = GetOLevelResultsAboveOrEqualToC6(secondSittingResultDetails);

                List<APPLICANT_O_LEVEL_RESULT_DETAIL> consolidatedOLevelResultsAboveOrEqualToC6 = new List<APPLICANT_O_LEVEL_RESULT_DETAIL>();
                if (firstSittingOLevelResultsAboveOrEqualToC6 != null && firstSittingOLevelResultsAboveOrEqualToC6.Count > 0)
                {
                    if (secondSittingOLevelResultsAboveOrEqualToC6 != null && secondSittingOLevelResultsAboveOrEqualToC6.Count > 0)
                    {
                        foreach (APPLICANT_O_LEVEL_RESULT_DETAIL oLevelResultDetail in firstSittingOLevelResultsAboveOrEqualToC6)
                        {
                            APPLICANT_O_LEVEL_RESULT_DETAIL oLevelResultDetailMatch = secondSittingOLevelResultsAboveOrEqualToC6.Where(ss => ss.O_Level_Subject_Id == oLevelResultDetail.O_Level_Subject_Id).SingleOrDefault();
                            if (oLevelResultDetailMatch != null)
                            {
                                if (oLevelResultDetailMatch.O_Level_Grade_Id > oLevelResultDetail.O_Level_Grade_Id)
                                {
                                    consolidatedOLevelResultsAboveOrEqualToC6.Add(oLevelResultDetail);
                                }
                                else
                                {
                                    consolidatedOLevelResultsAboveOrEqualToC6.Add(oLevelResultDetailMatch);
                                }
                            }
                            else
                            {
                                consolidatedOLevelResultsAboveOrEqualToC6.Add(oLevelResultDetail);
                            }
                        }

                        foreach (APPLICANT_O_LEVEL_RESULT_DETAIL oLevelResultDetail in secondSittingOLevelResultsAboveOrEqualToC6)
                        {
                            APPLICANT_O_LEVEL_RESULT_DETAIL oLevelResultDetailMatch = firstSittingOLevelResultsAboveOrEqualToC6.Where(ss => ss.O_Level_Subject_Id == oLevelResultDetail.O_Level_Grade_Id).SingleOrDefault();
                            if (oLevelResultDetailMatch == null)
                            {
                                consolidatedOLevelResultsAboveOrEqualToC6.Add(oLevelResultDetail);
                            }
                        }
                    }
                    else
                    {
                        foreach (APPLICANT_O_LEVEL_RESULT_DETAIL oLevelResultDetail in firstSittingOLevelResultsAboveOrEqualToC6)
                        {
                            if (oLevelResultDetail.O_Level_Grade_Id <= 6)
                            {
                                consolidatedOLevelResultsAboveOrEqualToC6.Add(oLevelResultDetail);
                            }
                        }
                    }
                }

                return consolidatedOLevelResultsAboveOrEqualToC6;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private List<APPLICANT_O_LEVEL_RESULT_DETAIL> GetOLevelResultsAboveOrEqualToC6(List<APPLICANT_O_LEVEL_RESULT_DETAIL> resultDetails)
        {
            try
            {
                List<APPLICANT_O_LEVEL_RESULT_DETAIL> oLevelResultsAboveOrEqualToC6 = new List<APPLICANT_O_LEVEL_RESULT_DETAIL>();
                if (resultDetails != null && resultDetails.Count > 0)
                {
                    foreach (APPLICANT_O_LEVEL_RESULT_DETAIL firstSittingResultDetail in resultDetails)
                    {
                        if (firstSittingResultDetail.O_Level_Grade_Id <= 6)
                        {
                            oLevelResultsAboveOrEqualToC6.Add(firstSittingResultDetail);
                        }
                    }
                }

                return oLevelResultsAboveOrEqualToC6;
            }
            catch (Exception)
            {
                throw;
            }
        }


        //private List<OLevelResultDetail> GetConsolidatedApplicantOlevelResultAboveOrEqualToC6(List<OLevelResultDetail> firstSittingResultDetails, List<OLevelResultDetail> secondSittingResultDetails)
        //{
        //    try
        //    {
        //        List<OLevelResultDetail> firstSittingOLevelResultsAboveOrEqualToC6 = GetOLevelResultsAboveOrEqualToC6(firstSittingResultDetails);
        //        List<OLevelResultDetail> secondSittingOLevelResultsAboveOrEqualToC6 = GetOLevelResultsAboveOrEqualToC6(secondSittingResultDetails);

        //        List<OLevelResultDetail> consolidatedOLevelResultsAboveOrEqualToC6 = new List<OLevelResultDetail>();
        //        if (firstSittingOLevelResultsAboveOrEqualToC6 != null && firstSittingOLevelResultsAboveOrEqualToC6.Count > 0)
        //        {
        //            if (secondSittingOLevelResultsAboveOrEqualToC6 != null && secondSittingOLevelResultsAboveOrEqualToC6.Count > 0)
        //            {
        //                foreach (OLevelResultDetail oLevelResultDetail in firstSittingOLevelResultsAboveOrEqualToC6)
        //                {
        //                    OLevelResultDetail oLevelResultDetailMatch = secondSittingOLevelResultsAboveOrEqualToC6.Where(ss => ss.Subject.Id == oLevelResultDetail.Subject.Id).SingleOrDefault();
        //                    if (oLevelResultDetailMatch != null)
        //                    {
        //                        if (oLevelResultDetailMatch.Grade.Id > oLevelResultDetail.Grade.Id)
        //                        {
        //                            consolidatedOLevelResultsAboveOrEqualToC6.Add(oLevelResultDetail);
        //                        }
        //                        else
        //                        {
        //                            consolidatedOLevelResultsAboveOrEqualToC6.Add(oLevelResultDetailMatch);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        consolidatedOLevelResultsAboveOrEqualToC6.Add(oLevelResultDetail);
        //                    }
        //                }

        //                foreach (OLevelResultDetail oLevelResultDetail in secondSittingOLevelResultsAboveOrEqualToC6)
        //                {
        //                    OLevelResultDetail oLevelResultDetailMatch = firstSittingOLevelResultsAboveOrEqualToC6.Where(ss => ss.Subject.Id == oLevelResultDetail.Subject.Id).SingleOrDefault();
        //                    if (oLevelResultDetailMatch == null)
        //                    {
        //                        consolidatedOLevelResultsAboveOrEqualToC6.Add(oLevelResultDetail);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                foreach (OLevelResultDetail oLevelResultDetail in firstSittingOLevelResultsAboveOrEqualToC6)
        //                {
        //                    if (oLevelResultDetail.Grade.Id <= 6)
        //                    {
        //                        consolidatedOLevelResultsAboveOrEqualToC6.Add(oLevelResultDetail);
        //                    }
        //                }
        //            }
        //        }

        //        return consolidatedOLevelResultsAboveOrEqualToC6;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //private List<OLevelResultDetail> GetOLevelResultsAboveOrEqualToC6(List<OLevelResultDetail> resultDetails)
        //{
        //    try
        //    {
        //        List<OLevelResultDetail> oLevelResultsAboveOrEqualToC6 = new List<OLevelResultDetail>();
        //        if (resultDetails != null && resultDetails.Count > 0)
        //        {
        //            foreach (OLevelResultDetail firstSittingResultDetail in resultDetails)
        //            {
        //                if (firstSittingResultDetail.Grade.Id <= 6)
        //                {
        //                    oLevelResultsAboveOrEqualToC6.Add(firstSittingResultDetail);
        //                }
        //            }
        //        }

        //        return oLevelResultsAboveOrEqualToC6;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        private string CheckRequiredOlevelSubjects(List<ADMISSION_CRITERIA_FOR_O_LEVEL_SUBJECT> compulsorySubjects, List<ADMISSION_CRITERIA_FOR_O_LEVEL_SUBJECT> otherSubjects, List<APPLICANT_O_LEVEL_RESULT_DETAIL> firstSittingResultDetails, List<APPLICANT_O_LEVEL_RESULT_DETAIL> secondSittingResultDetails, APPLICANT_APPLIED_COURSE applicantAppliedCourse)
        {
            try
            {
                decimal Score = 0;
                decimal Total = 0;
                int k = 0;
                ArrayList RankSubjects = new ArrayList();
                string[] coreSubjects = new string[10];
                string[] noncoreSubjects = new string[10];

                //check for the compulsory subjects
                if ((compulsorySubjects == null || compulsorySubjects.Count <= 0) && (otherSubjects == null || otherSubjects.Count <= 0))
                {
                    throw new Exception("Subject has not been set in Admission Criteria for " + applicantAppliedCourse.PROGRAMME.Programme_Name.ToUpper() + " in " + applicantAppliedCourse.DEPARTMENT.Department_Name + "! Please contact you system administrator");
                }


                int minimumNoOfSubjectWithCreditOrAbove = compulsorySubjects[0].ADMISSION_CRITERIA.Minimum_Required_Number_Of_Subject;

                List<APPLICANT_O_LEVEL_RESULT_DETAIL> applicantConsolidatedResultsWithCreditOrAbove = GetConsolidatedApplicantOlevelResultAboveOrEqualToC6(firstSittingResultDetails, secondSittingResultDetails);
                if (applicantConsolidatedResultsWithCreditOrAbove == null || applicantConsolidatedResultsWithCreditOrAbove.Count <= 0)
                {
                    return "Required minimum number of subjects at Credit level for " + applicantAppliedCourse.DEPARTMENT.Department_Name.ToUpper() + " is " + minimumNoOfSubjectWithCreditOrAbove + " and the applicant got none, ";
                }
                else if (applicantConsolidatedResultsWithCreditOrAbove.Count < minimumNoOfSubjectWithCreditOrAbove)
                {
                    return "Required minimum number of subjects at Credit level for " + applicantAppliedCourse.DEPARTMENT.Department_Name.ToUpper() + " is " + minimumNoOfSubjectWithCreditOrAbove + " the applicant got only " + applicantConsolidatedResultsWithCreditOrAbove.Count + ".";
                }

                string rejectReason = null;


                //check for main compulsory subjects
                int compulsorySubjectMatchCount = 0;
                List<string> requiredSubjects = new List<string>();
                foreach (ADMISSION_CRITERIA_FOR_O_LEVEL_SUBJECT compulsoryOLevelSubject in compulsorySubjects)
                {
                    string alternativeSubjects = null;
                    List<APPLICANT_O_LEVEL_RESULT_DETAIL> oLevelResultDetailAlternatives = new List<APPLICANT_O_LEVEL_RESULT_DETAIL>();
                    APPLICANT_O_LEVEL_RESULT_DETAIL oLevelResultDetail = applicantConsolidatedResultsWithCreditOrAbove.Where(cs => cs.O_Level_Subject_Id == compulsoryOLevelSubject.O_Level_Subject_Id).FirstOrDefault();

                    List<ADMISSION_CRITERIA_FOR_O_LEVEL_SUBJECT_ALTERNATIVE> compulsoryOLevelSubjectAlternatives = admissionCriteriaForOLevelSubjectAlternativeLogic.GetEntitiesBy(m => m.Admission_Criteria_For_O_Level_Subject_Id == compulsoryOLevelSubject.O_Level_Subject_Id).ToList();


                    if (compulsoryOLevelSubjectAlternatives != null && compulsoryOLevelSubjectAlternatives.Count > 0)
                    {
                        foreach (ADMISSION_CRITERIA_FOR_O_LEVEL_SUBJECT_ALTERNATIVE compulsoryOLevelSubjectAlternative in compulsoryOLevelSubjectAlternatives)
                        {
                            APPLICANT_O_LEVEL_RESULT_DETAIL oLevelResultDetailAlternative = applicantConsolidatedResultsWithCreditOrAbove.Where(cs => cs.O_Level_Subject_Id == compulsoryOLevelSubjectAlternative.O_Level_Subject_Id).SingleOrDefault();
                            if (oLevelResultDetailAlternative != null)
                            {
                                oLevelResultDetailAlternatives.Add(oLevelResultDetailAlternative);
                            }
                        }
                    }

                    if (oLevelResultDetail == null && oLevelResultDetailAlternatives.Count <= 0)
                    {
                        //get all compulsory alternative subjects
                        if (compulsoryOLevelSubjectAlternatives != null && compulsoryOLevelSubjectAlternatives.Count > 0)
                        {
                            foreach (ADMISSION_CRITERIA_FOR_O_LEVEL_SUBJECT_ALTERNATIVE alternative in compulsoryOLevelSubjectAlternatives)
                            {
                                alternativeSubjects += "/" + alternative.O_LEVEL_SUBJECT.O_Level_Subject_Name;
                            }
                        }

                        if (string.IsNullOrEmpty(alternativeSubjects))
                        {
                            requiredSubjects.Add(compulsoryOLevelSubject.O_LEVEL_SUBJECT.O_Level_Subject_Name.ToUpper());
                            //rejectReason += compulsoryOLevelSubject.Subject.Name.ToUpper() + " is a compulsory requirement for " + applicantAppliedCourse.Department.Name.ToUpper() + ", but was not obtained by the applicant. Therefore did not qualify for admission.";
                        }
                        else
                        {
                            requiredSubjects.Add(" " + compulsoryOLevelSubject.O_LEVEL_SUBJECT.O_Level_Subject_Name.ToUpper() + alternativeSubjects.ToUpper());
                            //rejectReason += "Any one of " + compulsoryOLevelSubject.Subject.Name.ToUpper() + alternativeSubjects.ToUpper() + " is a compulsory requirement for " + applicantAppliedCourse.Department.Name.ToUpper() + ", but none of them was obtained by the applicant. Therefore did not qualify for admission.";
                        }
                    }
                    else
                    {

                        Score = 0;


                        if (oLevelResultDetail != null)
                        {

                            coreSubjects[compulsorySubjectMatchCount] = oLevelResultDetail.O_LEVEL_SUBJECT.O_Level_Subject_Name;
                            RankSubjects.Add(oLevelResultDetail.O_LEVEL_SUBJECT.O_Level_Subject_Name + " : " + oLevelResultDetail.O_LEVEL_GRADE.O_Level_Grade_Name + " * ");

                            if (k < compulsorySubjects.Count)
                            {
                                Score = GetWeight(oLevelResultDetail.O_Level_Grade_Id, true);
                                Total = Total + Score;
                                k++;
                            }
                        }
                        else
                        {
                            List<APPLICANT_O_LEVEL_RESULT_DETAIL> alternativeResultMatch = new List<APPLICANT_O_LEVEL_RESULT_DETAIL>();
                            alternativeResultMatch = oLevelResultDetailAlternatives.OrderBy(o => o.O_Level_Grade_Id).ToList();
                            coreSubjects[compulsorySubjectMatchCount] = alternativeResultMatch[0].O_LEVEL_SUBJECT.O_Level_Subject_Name;
                            RankSubjects.Add(alternativeResultMatch[0].O_LEVEL_SUBJECT.O_Level_Subject_Name + " : " + alternativeResultMatch[0].O_LEVEL_GRADE.O_Level_Grade_Name + " * ");

                            if (k < compulsorySubjects.Count)
                            {
                                Score = GetWeight(alternativeResultMatch[0].O_LEVEL_GRADE.O_Level_Grade_Id, true);
                                Total = Total + Score;
                                k++;
                            }
                        }


                        compulsorySubjectMatchCount++;
                    }
                }

                if (requiredSubjects.Count > 0)
                {
                    string[] sbjectsNotGot = requiredSubjects.ToArray();
                    string missingSubjects = string.Join(", ", sbjectsNotGot);
                    rejectReason += missingSubjects + " is a compulsory requirement for " + applicantAppliedCourse.DEPARTMENT.Department_Name.ToUpper() + ", but was not obtained by the applicant.";
                }


                //
                int orderSubjectMatchCount = 0;
                k = compulsorySubjects.Count;
                //List<string> otherNeededSubjects = new List<string>();
                foreach (ADMISSION_CRITERIA_FOR_O_LEVEL_SUBJECT otherSubject in otherSubjects)
                {
                    //string alternativeSubjects = null;
                    List<APPLICANT_O_LEVEL_RESULT_DETAIL> oLevelResultDetailAlternatives = new List<APPLICANT_O_LEVEL_RESULT_DETAIL>();
                    List<APPLICANT_O_LEVEL_RESULT_DETAIL> applicantConsolidatedResultsWithCreditOrAboveWithOutCompolsarySubjects = applicantConsolidatedResultsWithCreditOrAbove;

                    foreach (string subject in coreSubjects)
                    {
                        APPLICANT_O_LEVEL_RESULT_DETAIL oLevelResultDetailx = applicantConsolidatedResultsWithCreditOrAboveWithOutCompolsarySubjects.Where(s => s.O_LEVEL_SUBJECT.O_Level_Subject_Name == subject).FirstOrDefault();
                        applicantConsolidatedResultsWithCreditOrAboveWithOutCompolsarySubjects.Remove(oLevelResultDetailx);
                    }

                    applicantConsolidatedResultsWithCreditOrAboveWithOutCompolsarySubjects = applicantConsolidatedResultsWithCreditOrAboveWithOutCompolsarySubjects.OrderBy(a => a.O_Level_Grade_Id).ToList();
                    APPLICANT_O_LEVEL_RESULT_DETAIL oLevelResultDetail = applicantConsolidatedResultsWithCreditOrAboveWithOutCompolsarySubjects.Where(cs => cs.O_Level_Subject_Id == otherSubject.O_Level_Subject_Id).FirstOrDefault();
                    List<ADMISSION_CRITERIA_FOR_O_LEVEL_SUBJECT_ALTERNATIVE> otherSubjectAlternatives = admissionCriteriaForOLevelSubjectAlternativeLogic.GetEntitiesBy(m => m.Admission_Criteria_For_O_Level_Subject_Id == otherSubject.O_Level_Subject_Id).ToList();
                    if (otherSubjectAlternatives != null && otherSubjectAlternatives.Count > 0)
                    {
                        foreach (ADMISSION_CRITERIA_FOR_O_LEVEL_SUBJECT_ALTERNATIVE otherSubjectAlternative in otherSubjectAlternatives)
                        {
                            APPLICANT_O_LEVEL_RESULT_DETAIL oLevelResultDetailAlternative = applicantConsolidatedResultsWithCreditOrAboveWithOutCompolsarySubjects.Where(cs => cs.O_Level_Subject_Id == otherSubjectAlternative.O_Level_Subject_Id).FirstOrDefault();
                            if (oLevelResultDetailAlternative != null)
                            {
                                //alternativeSubjects += "/" + oLevelResultDetailAlternative.Subject.Name;
                                oLevelResultDetailAlternatives.Add(oLevelResultDetailAlternative);
                            }
                        }
                    }

                    if (oLevelResultDetail != null || oLevelResultDetailAlternatives.Count > 0)
                    {
                        //Check if subject is among alternatives and the get the best.
                        Score = 0;
                        if (oLevelResultDetail != null)
                        {

                            noncoreSubjects[orderSubjectMatchCount] = oLevelResultDetail.O_LEVEL_SUBJECT.O_Level_Subject_Name;
                            RankSubjects.Add(oLevelResultDetail.O_LEVEL_SUBJECT.O_Level_Subject_Name + " : " + oLevelResultDetail.O_LEVEL_GRADE.O_Level_Grade_Name);

                            int totalSubjectsOutstanding = minimumNoOfSubjectWithCreditOrAbove - compulsorySubjects.Count;
                            string subjectsOutstanding = totalSubjectsOutstanding > 1 ? totalSubjectsOutstanding + " subjects" : totalSubjectsOutstanding + " subject";

                            if (k < 5)
                            {
                                Score = GetWeight(oLevelResultDetail.O_Level_Grade_Id, false);
                                Total = Total + Score;
                                k++;
                            }
                        }
                        else
                        {
                            List<APPLICANT_O_LEVEL_RESULT_DETAIL> alternativeResultMatch = new List<APPLICANT_O_LEVEL_RESULT_DETAIL>();
                            alternativeResultMatch = oLevelResultDetailAlternatives.OrderBy(o => o.O_Level_Grade_Id).ToList();
                            noncoreSubjects[orderSubjectMatchCount] = alternativeResultMatch[0].O_LEVEL_SUBJECT.O_Level_Subject_Name + " : " + alternativeResultMatch[0].O_LEVEL_GRADE.O_Level_Grade_Name;
                            RankSubjects.Add(alternativeResultMatch[0].O_LEVEL_SUBJECT.O_Level_Subject_Name + " : " + alternativeResultMatch[0].O_LEVEL_GRADE.O_Level_Grade_Name);

                            int totalSubjectsOutstanding = minimumNoOfSubjectWithCreditOrAbove - compulsorySubjects.Count;
                            string subjectsOutstanding = totalSubjectsOutstanding > 1 ? totalSubjectsOutstanding + " subjects" : totalSubjectsOutstanding + " subject";

                            if (k < 5)
                            {
                                Score = GetWeight(alternativeResultMatch[0].O_Level_Grade_Id, false);
                                Total = Total + Score;
                                k++;
                            }
                        }



                        orderSubjectMatchCount++;
                    }
                }

                int totalSubjectsObtained = orderSubjectMatchCount + compulsorySubjectMatchCount;
                int otherSubjectCount = minimumNoOfSubjectWithCreditOrAbove - compulsorySubjects.Count;
                if (otherSubjectCount > orderSubjectMatchCount)
                {
                    if (orderSubjectMatchCount > 0)
                    {
                        rejectReason += "Applicant made a total of " + compulsorySubjectMatchCount + " compulsory subjects, and " + orderSubjectMatchCount + " other subject, which totals " + totalSubjectsObtained + " and does not meet the minimum requirement of " + minimumNoOfSubjectWithCreditOrAbove + " subjects for " + applicantAppliedCourse.DEPARTMENT.Department_Name.ToUpper() + ".";
                    }
                    else
                    {
                        List<string> anyOfTheseSubjects = new List<string>();
                        foreach (ADMISSION_CRITERIA_FOR_O_LEVEL_SUBJECT otherOLevelSubject in otherSubjects)
                        {
                            anyOfTheseSubjects.Add(otherOLevelSubject.O_LEVEL_SUBJECT.O_Level_Subject_Name.ToUpper());
                        }

                        string[] anyOne = anyOfTheseSubjects.ToArray();
                        int totalSubjectsOutstanding = minimumNoOfSubjectWithCreditOrAbove - compulsorySubjects.Count;
                        string subjectsOutstanding = totalSubjectsOutstanding > 1 ? totalSubjectsOutstanding + " subjects" : totalSubjectsOutstanding + " subject";
                        rejectReason += "Applicant made a total of " + compulsorySubjectMatchCount + " compulsory subjects, but did not obtain any other " + subjectsOutstanding + " from (" + string.Join(", ", anyOne) + ") at credit level to complement. Which does not meet the minimum requirement of " + minimumNoOfSubjectWithCreditOrAbove + " subjects for " + applicantAppliedCourse.DEPARTMENT.Department_Name.ToUpper() + ".";
                    }
                }


                RankSubjects.Add("NAN");
                RankSubjects.Add("NAN");
                RankSubjects.Add("NAN");
                RankSubjects.Add("NAN");
                RankSubjects.Add("NAN");

                RankingData Rank = new RankingData();
                RankingDataLogic RankLogic = new RankingDataLogic();
                Rank.Person = new Person() { Id = applicantAppliedCourse.Person_Id };
                Rank.Subj1 = RankSubjects[0].ToString();
                Rank.Subj2 = RankSubjects[1].ToString();
                Rank.Subj3 = RankSubjects[2].ToString();
                Rank.Subj4 = RankSubjects[3].ToString();
                Rank.Subj5 = RankSubjects[4].ToString();
                Rank.Reason = rejectReason;
                if (rejectReason != null)
                {
                    Rank.Qualified = false;
                }
                else
                {
                    Rank.Qualified = true;
                }
                Rank.Total = (decimal)Total;

                RankLogic.Create(Rank);
                return rejectReason;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        //private string CheckRequiredOlevelSubjects(List<AdmissionCriteriaForOLevelSubject> compulsorySubjects, List<AdmissionCriteriaForOLevelSubject> otherSubjects, List<OLevelResultDetail> firstSittingResultDetails, List<OLevelResultDetail> secondSittingResultDetails, AppliedCourse applicantAppliedCourse)
        //{
        //    try
        //    {
        //        decimal Score = 0;
        //        decimal Total = 0;
        //        int k = 0;
        //        ArrayList RankSubjects = new ArrayList();
        //        string[] coreSubjects = new string[10];
        //        string[] noncoreSubjects = new string[10];
                
        //        //check for the compulsory subjects
        //        if ((compulsorySubjects == null || compulsorySubjects.Count <= 0) && (otherSubjects == null || otherSubjects.Count <= 0))
        //        {
        //            throw new Exception("Subject has not been set in Admission Criteria for " + applicantAppliedCourse.Programme.Name.ToUpper() + " in " + applicantAppliedCourse.Department.Name + "! Please contact you system administrator");
        //        }


        //        int minimumNoOfSubjectWithCreditOrAbove = compulsorySubjects[0].MainCriteria.MinimumRequiredNumberOfSubject;

        //        List<OLevelResultDetail> applicantConsolidatedResultsWithCreditOrAbove = GetConsolidatedApplicantOlevelResultAboveOrEqualToC6(firstSittingResultDetails, secondSittingResultDetails);
        //        if (applicantConsolidatedResultsWithCreditOrAbove == null || applicantConsolidatedResultsWithCreditOrAbove.Count <= 0)
        //        {
        //            return "Required minimum number of subjects at Credit level for " + applicantAppliedCourse.Department.Name.ToUpper() + " is " + minimumNoOfSubjectWithCreditOrAbove + " and the applicant got none, ";
        //        }
        //        else if (applicantConsolidatedResultsWithCreditOrAbove.Count < minimumNoOfSubjectWithCreditOrAbove)
        //        {
        //            return "Required minimum number of subjects at Credit level for " + applicantAppliedCourse.Department.Name.ToUpper() + " is " + minimumNoOfSubjectWithCreditOrAbove + " the applicant got only " + applicantConsolidatedResultsWithCreditOrAbove.Count + ".";
        //        }

        //        string rejectReason = null;


        //        //check for main compulsory subjects
        //        int compulsorySubjectMatchCount = 0;
        //        List<string> requiredSubjects = new List<string>();
        //        foreach (AdmissionCriteriaForOLevelSubject compulsoryOLevelSubject in compulsorySubjects)
        //        {
        //            string alternativeSubjects = null;
        //            List<OLevelResultDetail> oLevelResultDetailAlternatives = new List<OLevelResultDetail>();
        //            OLevelResultDetail oLevelResultDetail = applicantConsolidatedResultsWithCreditOrAbove.Where(cs => cs.Subject.Id == compulsoryOLevelSubject.Subject.Id).SingleOrDefault();
                    
        //            List<AdmissionCriteriaForOLevelSubjectAlternative> compulsoryOLevelSubjectAlternatives = admissionCriteriaForOLevelSubjectAlternativeLogic.GetModelsBy(m => m.Admission_Criteria_For_O_Level_Subject_Id == compulsoryOLevelSubject.Id).ToList();


        //            if (compulsoryOLevelSubjectAlternatives != null && compulsoryOLevelSubjectAlternatives.Count > 0)
        //            {
        //                foreach (AdmissionCriteriaForOLevelSubjectAlternative compulsoryOLevelSubjectAlternative in compulsoryOLevelSubjectAlternatives)
        //                {
        //                    OLevelResultDetail oLevelResultDetailAlternative = applicantConsolidatedResultsWithCreditOrAbove.Where(cs => cs.Subject.Id == compulsoryOLevelSubjectAlternative.OLevelSubject.Id).SingleOrDefault();
        //                    if (oLevelResultDetailAlternative != null)
        //                    {
        //                        oLevelResultDetailAlternatives.Add(oLevelResultDetailAlternative);
        //                    }
        //                }
        //            }

        //            if (oLevelResultDetail == null && oLevelResultDetailAlternatives.Count <= 0)
        //            {
        //                //get all compulsory alternative subjects
        //                if (compulsoryOLevelSubjectAlternatives != null && compulsoryOLevelSubjectAlternatives.Count > 0)
        //                {
        //                    foreach (AdmissionCriteriaForOLevelSubjectAlternative alternative in compulsoryOLevelSubjectAlternatives)
        //                    {
        //                        alternativeSubjects += "/" + alternative.OLevelSubject.Name;
        //                    }
        //                }

        //                if (string.IsNullOrEmpty(alternativeSubjects))
        //                {
        //                    requiredSubjects.Add(compulsoryOLevelSubject.Subject.Name.ToUpper());
        //                    //rejectReason += compulsoryOLevelSubject.Subject.Name.ToUpper() + " is a compulsory requirement for " + applicantAppliedCourse.Department.Name.ToUpper() + ", but was not obtained by the applicant. Therefore did not qualify for admission.";
        //                }
        //                else
        //                {
        //                    requiredSubjects.Add(" " + compulsoryOLevelSubject.Subject.Name.ToUpper() + alternativeSubjects.ToUpper());
        //                    //rejectReason += "Any one of " + compulsoryOLevelSubject.Subject.Name.ToUpper() + alternativeSubjects.ToUpper() + " is a compulsory requirement for " + applicantAppliedCourse.Department.Name.ToUpper() + ", but none of them was obtained by the applicant. Therefore did not qualify for admission.";
        //                }
        //            }
        //            else 
        //            {
                        
        //                Score = 0;
                       

        //                if (oLevelResultDetail != null)
        //                {

        //                    coreSubjects[compulsorySubjectMatchCount] = oLevelResultDetail.Subject.Name ;
        //                    RankSubjects.Add(oLevelResultDetail.Subject.Name + " : " + oLevelResultDetail.Grade.Name + " * ");

        //                    if (k < compulsorySubjects.Count)
        //                    {
        //                        Score = GetWeight(oLevelResultDetail.Grade.Id, true);
        //                        Total = Total + Score;
        //                        k++;
        //                    }
        //                }
        //                else
        //                {
        //                    List<OLevelResultDetail> alternativeResultMatch = new List<OLevelResultDetail>();
        //                    alternativeResultMatch = oLevelResultDetailAlternatives.OrderBy(o => o.Grade.Id).ToList();
        //                    coreSubjects[compulsorySubjectMatchCount] = alternativeResultMatch[0].Subject.Name;
        //                    RankSubjects.Add(alternativeResultMatch[0].Subject.Name + " : " + alternativeResultMatch[0].Grade.Name + " * ");

        //                    if (k < compulsorySubjects.Count)
        //                    {
        //                        Score = GetWeight(alternativeResultMatch[0].Grade.Id, true);
        //                        Total = Total + Score;
        //                        k++;
        //                    }
        //                }


        //                compulsorySubjectMatchCount++;
        //            }
        //        }

        //        if (requiredSubjects.Count > 0)
        //        {
        //            string[] sbjectsNotGot = requiredSubjects.ToArray();
        //            string missingSubjects = string.Join(", ", sbjectsNotGot);
        //            rejectReason += missingSubjects + " is a compulsory requirement for " + applicantAppliedCourse.Department.Name.ToUpper() + ", but was not obtained by the applicant.";
        //        }


        //        //
        //        int orderSubjectMatchCount = 0;
        //        k = compulsorySubjects.Count;
        //        //List<string> otherNeededSubjects = new List<string>();
        //        foreach (AdmissionCriteriaForOLevelSubject otherSubject in otherSubjects)
        //        {
        //            //string alternativeSubjects = null;
        //            List<OLevelResultDetail> oLevelResultDetailAlternatives = new List<OLevelResultDetail>();
        //            List<OLevelResultDetail> applicantConsolidatedResultsWithCreditOrAboveWithOutCompolsarySubjects = applicantConsolidatedResultsWithCreditOrAbove;
                   
        //            foreach(string subject in coreSubjects)
        //            {
        //               OLevelResultDetail oLevelResultDetailx =  applicantConsolidatedResultsWithCreditOrAboveWithOutCompolsarySubjects.Where(s => s.Subject.Name == subject).SingleOrDefault();
        //               applicantConsolidatedResultsWithCreditOrAboveWithOutCompolsarySubjects.Remove(oLevelResultDetailx);
        //            }

        //            applicantConsolidatedResultsWithCreditOrAboveWithOutCompolsarySubjects = applicantConsolidatedResultsWithCreditOrAboveWithOutCompolsarySubjects.OrderBy(a => a.Grade.Id).ToList();
        //            OLevelResultDetail oLevelResultDetail = applicantConsolidatedResultsWithCreditOrAboveWithOutCompolsarySubjects.Where(cs => cs.Subject.Id == otherSubject.Subject.Id).SingleOrDefault();
        //            List<AdmissionCriteriaForOLevelSubjectAlternative> otherSubjectAlternatives = admissionCriteriaForOLevelSubjectAlternativeLogic.GetModelsBy(m => m.Admission_Criteria_For_O_Level_Subject_Id == otherSubject.Id).ToList();
        //            if (otherSubjectAlternatives != null && otherSubjectAlternatives.Count > 0)
        //            {
        //                foreach (AdmissionCriteriaForOLevelSubjectAlternative otherSubjectAlternative in otherSubjectAlternatives)
        //                {
        //                    OLevelResultDetail oLevelResultDetailAlternative = applicantConsolidatedResultsWithCreditOrAboveWithOutCompolsarySubjects.Where(cs => cs.Subject.Id == otherSubjectAlternative.OLevelSubject.Id).SingleOrDefault();
        //                    if (oLevelResultDetailAlternative != null)
        //                    {
        //                        //alternativeSubjects += "/" + oLevelResultDetailAlternative.Subject.Name;
        //                        oLevelResultDetailAlternatives.Add(oLevelResultDetailAlternative);
        //                    }
        //                }
        //            }

        //            if (oLevelResultDetail != null  || oLevelResultDetailAlternatives.Count > 0 )
        //            {
        //                //Check if subject is among alternatives and the get the best.
        //                Score = 0;
        //                if (oLevelResultDetail != null)
        //                {
                           
        //                    noncoreSubjects[orderSubjectMatchCount] = oLevelResultDetail.Subject.Name ;
        //                    RankSubjects.Add(oLevelResultDetail.Subject.Name + " : " + oLevelResultDetail.Grade.Name);

        //                    int totalSubjectsOutstanding = minimumNoOfSubjectWithCreditOrAbove - compulsorySubjects.Count;
        //                    string subjectsOutstanding = totalSubjectsOutstanding > 1 ? totalSubjectsOutstanding + " subjects" : totalSubjectsOutstanding + " subject";

        //                    if (k < 5)
        //                    {
        //                        Score = GetWeight(oLevelResultDetail.Grade.Id, false);
        //                        Total = Total + Score;
        //                        k++;
        //                    }
        //                }
        //                else
        //                {
        //                    List<OLevelResultDetail> alternativeResultMatch = new List<OLevelResultDetail>(); 
        //                    alternativeResultMatch =  oLevelResultDetailAlternatives.OrderBy(o => o.Grade.Id).ToList();
        //                    noncoreSubjects[orderSubjectMatchCount] = alternativeResultMatch[0].Subject.Name + " : " + alternativeResultMatch[0].Grade.Name;
        //                    RankSubjects.Add(alternativeResultMatch[0].Subject.Name + " : " + alternativeResultMatch[0].Grade.Name);

        //                    int totalSubjectsOutstanding = minimumNoOfSubjectWithCreditOrAbove - compulsorySubjects.Count;
        //                    string subjectsOutstanding = totalSubjectsOutstanding > 1 ? totalSubjectsOutstanding + " subjects" : totalSubjectsOutstanding + " subject";

        //                    if (k < 5)
        //                    {
        //                        Score = GetWeight(alternativeResultMatch[0].Grade.Id, false);
        //                        Total = Total + Score;
        //                        k++;
        //                    }
        //                }

                       

        //                orderSubjectMatchCount++;
        //            }
        //        }

        //        int totalSubjectsObtained = orderSubjectMatchCount + compulsorySubjectMatchCount;
        //        int otherSubjectCount = minimumNoOfSubjectWithCreditOrAbove - compulsorySubjects.Count;
        //        if (otherSubjectCount > orderSubjectMatchCount)
        //        {
        //            if (orderSubjectMatchCount > 0)
        //            {
        //                rejectReason += "Applicant made a total of " + compulsorySubjectMatchCount + " compulsory subjects, and " + orderSubjectMatchCount + " other subject, which totals " + totalSubjectsObtained + " and does not meet the minimum requirement of " + minimumNoOfSubjectWithCreditOrAbove + " subjects for " + applicantAppliedCourse.Department.Name.ToUpper() + ".";
        //            }
        //            else
        //            {
        //                List<string> anyOfTheseSubjects = new List<string>();
        //                foreach (AdmissionCriteriaForOLevelSubject otherOLevelSubject in otherSubjects)
        //                {
        //                    anyOfTheseSubjects.Add(otherOLevelSubject.Subject.Name.ToUpper());
        //                }

        //                string[] anyOne = anyOfTheseSubjects.ToArray();
        //                int totalSubjectsOutstanding = minimumNoOfSubjectWithCreditOrAbove - compulsorySubjects.Count;
        //                string subjectsOutstanding = totalSubjectsOutstanding > 1 ? totalSubjectsOutstanding + " subjects" : totalSubjectsOutstanding + " subject";
        //                rejectReason += "Applicant made a total of " + compulsorySubjectMatchCount + " compulsory subjects, but did not obtain any other " + subjectsOutstanding + " from (" + string.Join(", ", anyOne) + ") at credit level to complement. Which does not meet the minimum requirement of " + minimumNoOfSubjectWithCreditOrAbove + " subjects for " + applicantAppliedCourse.Department.Name.ToUpper() + ".";
        //            }
        //        }


        //        RankSubjects.Add("NAN");
        //        RankSubjects.Add("NAN");
        //        RankSubjects.Add("NAN");
        //        RankSubjects.Add("NAN");
        //        RankSubjects.Add("NAN");

        //        RankingData Rank = new RankingData();
        //        RankingDataLogic RankLogic = new RankingDataLogic();
        //        Rank.Person = applicantAppliedCourse.Person;
        //        Rank.Subj1 = RankSubjects[0].ToString();
        //        Rank.Subj2 = RankSubjects[1].ToString();
        //        Rank.Subj3 = RankSubjects[2].ToString();
        //        Rank.Subj4 = RankSubjects[3].ToString();
        //        Rank.Subj5 = RankSubjects[4].ToString();
        //        Rank.Reason = rejectReason;
        //        if (rejectReason != null)
        //        {
        //            Rank.Qualified = false;
        //        }
        //        else
        //        {
        //            Rank.Qualified = true;
        //        }
        //        Rank.Total = (decimal) Total;

        //        RankLogic.Create(Rank);
        //        return rejectReason;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
      
        private string CheckOlevelType(List<AdmissionCriteriaForOLevelType> requiredOlevelTypes, OLevelResult firstSittingOlevelResult, OLevelResult secondSittingOlevelResult, AppliedCourse applicantAppliedCourse)
        {
            try
            {
                string rejectReason = null;
                if (requiredOlevelTypes == null || requiredOlevelTypes.Count <= 0)
                {
                    throw new Exception("O-Level Type has not been set in Adission Criteria for " + applicantAppliedCourse.Programme.Name + " in " + applicantAppliedCourse.Department.Name + "! Please contact your system administrator.");
                }

                string applicantOlevelTypes = null;
                if (firstSittingOlevelResult != null && firstSittingOlevelResult.Id > 0)
                {
                    applicantOlevelTypes += firstSittingOlevelResult.Type.Name;

                    AdmissionCriteriaForOLevelType secondSittingAdmissionCriteriaOLevelType = null;
                    AdmissionCriteriaForOLevelType firstSittingAdmissionCriteriaOLevelType = requiredOlevelTypes.Where(ac => ac.OLevelType.Id == firstSittingOlevelResult.Type.Id).SingleOrDefault();
                    if (firstSittingAdmissionCriteriaOLevelType == null)
                    {
                        if (secondSittingOlevelResult != null && secondSittingOlevelResult.Id > 0)
                        {
                            applicantOlevelTypes += " & " + secondSittingOlevelResult.Type.Name;
                            secondSittingAdmissionCriteriaOLevelType = requiredOlevelTypes.Where(ac => ac.OLevelType.Id == secondSittingOlevelResult.Type.Id).SingleOrDefault();
                        }
                    }

                    if (firstSittingAdmissionCriteriaOLevelType == null && secondSittingAdmissionCriteriaOLevelType == null)
                    {
                        string validOlevelTypes = GetStringOfValidOlevelTypes(requiredOlevelTypes);

                        //int totalRequiredOLevelTypeCount = GetDistinctOLevelTypeCountFrom(requiredOlevelTypes);
                        //if (totalRequiredOLevelTypeCount == 1)
                        //{
                        rejectReason = "'" + applicantOlevelTypes + "' O-Level Type obtained by the applicant does not meet the O-Level Type (" + validOlevelTypes + ") requirement for " + applicantAppliedCourse.Department.Name.ToUpper();
                        //}
                        //else
                        //{
                        //    rejectReason = "'" + applicantOlevelTypes + "' O-Level Type. But required Types are " + validOlevelTypes;
                        //}
                    }
                }

                return rejectReason;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetStringOfValidOlevelTypes(List<AdmissionCriteriaForOLevelType> admissionCriteriaOLevelTypes)
        {
            try
            {
                string validOlevelTypes = null;
                if (admissionCriteriaOLevelTypes != null && admissionCriteriaOLevelTypes.Count > 0)
                {
                    string[] oLevelTypes = GetDistinctOLevelTypesFrom(admissionCriteriaOLevelTypes);
                    validOlevelTypes = string.Join(", ", oLevelTypes);


                    //for (int i = 0; i < admissionCriteriaOLevelTypes.Count; i++)
                    //{
                    //    if (admissionCriteriaOLevelTypes.Count == i - 1)
                    //    {
                    //        validOlevelTypes += admissionCriteriaOLevelTypes[i].OLevelType.ShortName;
                    //    }
                    //    else
                    //    {
                    //        validOlevelTypes += admissionCriteriaOLevelTypes[i].OLevelType.ShortName + ", ";
                    //    }
                    //}
                }

                return validOlevelTypes;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string[] GetDistinctOLevelTypesFrom(List<AdmissionCriteriaForOLevelType> admissionCriteriaOLevelTypes)
        {
            try
            {
                string[] oLevelTypes = null;
                if (admissionCriteriaOLevelTypes != null && admissionCriteriaOLevelTypes.Count > 0)
                {
                    oLevelTypes = admissionCriteriaOLevelTypes.Select(o => o.OLevelType.ShortName).Distinct().ToArray();
                }

                return oLevelTypes;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private int GetDistinctOLevelTypeCountFrom(List<AdmissionCriteriaForOLevelType> admissionCriteriaOLevelTypes)
        {
            try
            {
                string[] oLevelTypes = GetDistinctOLevelTypesFrom(admissionCriteriaOLevelTypes);
                if (oLevelTypes != null && oLevelTypes.Length > 0)
                {
                    return oLevelTypes.Length;
                }
                else
                {
                    return 0;
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        private decimal GetWeight(int grade,bool IsCompulsory)
        {
            decimal weight = 0;
            try
            {
               switch (grade)
               {
                   case 1:
                       weight = 20;
                       break;
                   case 2:
                       weight = 19;
                       break;
                   case 3:
                       weight = 18;
                       break;
                   case 4:
                       weight = 17;
                       break;
                   case 5:
                       weight = 16;
                       break;
                   case 6:
                       weight = 15;
                       break;
               }

                if (IsCompulsory)
                {
                    weight = (weight * (decimal) 1.50);
                }
               
            }
            catch (Exception ex)
            {
                
                throw;
            }
            return weight;
        }

    }
}
