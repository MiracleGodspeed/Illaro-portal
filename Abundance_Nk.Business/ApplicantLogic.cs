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
    public class ApplicantLogic : BusinessBaseLogic<Applicant, APPLICANT>
    {
        private ApplicantClearanceLogic applicantClearedLogic;
        private StudentMatricNumberAssignmentLogic studentMatricNumberAssignmentLogic;
        private StudentLogic studentLogic;
        public ApplicantLogic()
        {
            translator = new ApplicantTranslator();
            applicantClearedLogic = new ApplicantClearanceLogic();
            studentMatricNumberAssignmentLogic = new StudentMatricNumberAssignmentLogic();
            studentLogic = new StudentLogic();
        }

        public bool Modify(Applicant applicant)
        {
            try
            {
                Expression<Func<APPLICANT, bool>> selector = a => a.Person_Id == applicant.Person.Id;
                APPLICANT entity = GetEntitiesBy(selector).LastOrDefault();

                if (entity != null)
                {
                    if (applicant.ApplicationForm != null)
                    {
                        entity.Application_Form_Id = applicant.ApplicationForm.Id; 
                    }
                    if (applicant.Person != null)
                    {
                        entity.Person_Id = applicant.Person.Id;
                    } 

                    int modifiedRecordCount = Save();

                    if (modifiedRecordCount > 0)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<ApplicationFormView> GetBy(ApplicantStatus.Status status, Faculty faculty)
        {
            try
            {
                List<ApplicationFormView> forms = (from a in repository.GetBy<VW_PROSPECTIVE_STUDENT>(a => a.Rejected == false && a.Faculty_Id == faculty.Id && a.Applicant_Status_Id == (int)status)
                                                   //orderby a.Date_Submitted
                                                   select new ApplicationFormView
                                                   {
                                                       PersonId = a.Person_Id,
                                                       FormId = a.Application_Form_Id,
                                                       FormNumber = a.Application_Form_Number,
                                                       Name = a.Name,
                                                       DepartmentId = a.Department_Id,
                                                       DepartmentName = a.Department_Name,
                                                       ProgrammeName = a.Programme_Name,
                                                       ExamSerialNumber = a.Application_Exam_Serial_Number,
                                                       ExamNumber = a.Application_Exam_Number,
                                                       FacultyId = a.Faculty_Id,
                                                       FacultyName = a.Faculty_Name,
                                                       SessionId = a.Session_Id,
                                                       //LevelId = a.Level_Id,
                                                       //LevelName = a.Level_Name,
                                                       ProgrammeId = a.Programme_Id,
                                                       IsSelected = false,
                                                   }).ToList();

                SeRejectReason(forms);

                return forms;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<ApplicationFormView> GetBy(string applicationFormNo)
        {
            try
            {
                List<ApplicationFormView> forms = (from a in repository.GetBy<VW_PROSPECTIVE_STUDENT>(a => a.Application_Form_Number.ToLower() == applicationFormNo.ToLower())
                                                   select new ApplicationFormView
                                                   {
                                                       PersonId = a.Person_Id,
                                                       FormId = a.Application_Form_Id,
                                                       FormNumber = a.Application_Form_Number,
                                                       Name = a.Name,
                                                       DepartmentId = a.Department_Id,
                                                       DepartmentName = a.Department_Name,
                                                       ProgrammeName = a.Programme_Name,
                                                       ExamSerialNumber = a.Application_Exam_Serial_Number,
                                                       ExamNumber = a.Application_Exam_Number,
                                                       FacultyId = a.Faculty_Id,
                                                       FacultyName = a.Faculty_Name,
                                                       SessionId = a.Session_Id,
                                                       //LevelId = a.Level_Id,
                                                       //LevelName = a.Level_Name,
                                                       ProgrammeId = a.Programme_Id,

                                                       IsSelected = false,
                                                   }).Take(50).ToList();

                SeRejectReason(forms);

                return forms;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ApplicationFormView GetBy(long applicationFormId)
        {
            try
            {
                ApplicationFormView applicant = (from a in repository.GetBy<VW_PROSPECTIVE_STUDENT>(a => a.Application_Form_Id == applicationFormId)
                                                 select new ApplicationFormView
                                                 {
                                                     PersonId = a.Person_Id,
                                                     FormId = a.Application_Form_Id,
                                                     FormNumber = a.Application_Form_Number,
                                                     Name = a.Name,
                                                     DepartmentId = a.Department_Id,
                                                     DepartmentName = a.Department_Name,
                                                     ProgrammeName = a.Programme_Name,
                                                     ExamSerialNumber = a.Application_Exam_Serial_Number,
                                                     ExamNumber = a.Application_Exam_Number,
                                                     FacultyId = a.Faculty_Id,
                                                     FacultyName = a.Faculty_Name,
                                                     SessionId = a.Session_Id,
                                                     //LevelId = a.Level_Id,
                                                     //LevelName = a.Level_Name,
                                                     ProgrammeId = a.Programme_Id,

                                                     IsSelected = false,
                                                 }).FirstOrDefault();

                return applicant;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ApplicationFormView GetBy(Person person)
        {
            try
            {
                ApplicationFormView applicant = (from a in repository.GetBy<VW_PROSPECTIVE_STUDENT>(a => a.Person_Id == person.Id)
                                                 select new ApplicationFormView
                                                 {
                                                     PersonId = a.Person_Id,
                                                     FormId = a.Application_Form_Id,
                                                     FormNumber = a.Application_Form_Number,
                                                     Name = a.Name,
                                                     DepartmentId = a.Department_Id,
                                                     DepartmentName = a.Department_Name,
                                                     ProgrammeName = a.Programme_Name,
                                                     ExamSerialNumber = a.Application_Exam_Serial_Number,
                                                     ExamNumber = a.Application_Exam_Number,
                                                     FacultyId = a.Faculty_Id,
                                                     FacultyName = a.Faculty_Name,
                                                     SessionId = a.Session_Id,
                                                     //LevelId = a.Level_Id,
                                                     //LevelName = a.Level_Name,
                                                     ProgrammeId = a.Programme_Id,

                                                     IsSelected = false,
                                                 }).FirstOrDefault();

                return applicant;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Applicant GetApplicantsBy(string Application_Form_Number)
        {
            try
            {
                Expression<Func<APPLICANT, bool>> selector = a => a.APPLICATION_FORM.Application_Form_Number == Application_Form_Number;
                return GetModelBy(selector);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void SeRejectReason(List<ApplicationFormView> forms)
        {
            try
            {
                if (forms != null && forms.Count > 0)
                {
                    AdmissionCriteriaLogic admissionCriteriaLogic = new AdmissionCriteriaLogic();
                    foreach (ApplicationFormView form in forms)
                    {
                        AppliedCourse appliedCourse = new AppliedCourse();
                        appliedCourse.Person = new Person() { Id = form.PersonId };
                        appliedCourse.Programme = new Programme() { Id = form.ProgrammeId, Name = form.ProgrammeName };
                        appliedCourse.Department = new Department() { Id = form.DepartmentId, Name = form.DepartmentName };

                        form.RejectReason = admissionCriteriaLogic.EvaluateApplication(appliedCourse);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Applicant GetBy(ApplicationForm form)
        {
            try
            {
                Expression<Func<APPLICANT, bool>> selector = a => a.Application_Form_Id == form.Id;
                return GetModelBy(selector);
            }
            catch (Exception)
            {
                throw;
            }
        }
     
        public bool UpdateStatus(ApplicationForm form, ApplicantStatus.Status status)
        {
            try
            {
                Expression<Func<APPLICANT, bool>> selector = a => a.Application_Form_Id == form.Id;
                APPLICANT entity = GetEntityBy(selector);

                if (entity == null)
                {
                    throw new Exception(NoItemFound);
                }

                entity.Applicant_Status_Id = (int)status;
                              
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

        public bool Clear(List<ApplicationFormView> applicants, User clearedBy)
        {
            try
            {
                if (applicants == null)
                {
                    throw new Exception("List of Applicants to clear is empty! Please select at least one applicant from the list.");
                }

                using (TransactionScope transaction = new TransactionScope())
                {
                    foreach (ApplicationFormView applicant in applicants)
                    {
                        //change applicant status
                        ApplicationForm applicationForm = new ApplicationForm() { Id = applicant.FormId };
                      
                        //assign matric no to applicant
                        Faculty faculty = new Faculty() { Id = applicant.FacultyId };
                        Department department = new Department() { Id = applicant.DepartmentId };
                        Session session = new Session() { Id = applicant.SessionId };
                        Level level = new Level() { Id = applicant.LevelId };
                        Programme programme = new Programme() { Id = applicant.ProgrammeId };
                        StudentMatricNumberAssignment startMatricNo = studentMatricNumberAssignmentLogic.GetBy(faculty, department,programme, level, session);
                        if (startMatricNo != null)
                        {
                            long studentNumber = 0;
                            string matricNumber = "";

                            if (startMatricNo.Used)
                            {
                                string[] matricNoArray = startMatricNo.MatricNoStartFrom.Split('/');

                                studentNumber = GetNextStudentNumber(faculty, department, level, session);
                                matricNoArray[matricNoArray.Length - 1] = UtilityLogic.PaddNumber(studentNumber, 4);
                                matricNumber = string.Join("/", matricNoArray);
                            }
                            else
                            {
                                matricNumber = startMatricNo.MatricNoStartFrom;
                                studentNumber = startMatricNo.MatricSerialNoStartFrom;
                                bool markedAsUsed = studentMatricNumberAssignmentLogic.MarkAsUsed(startMatricNo);
                            }

                            Person person = new Person() { Id = applicant.PersonId };
                            bool matricAssigned = studentLogic.AssignMatricNumber(person, studentNumber, matricNumber);
                        }
                        else
                        {
                            throw new Exception(applicant.LevelName + " for " + applicant.DepartmentName + " for the current academic session has not been set! Please contact your system administrator.");
                        }
                    }

                    transaction.Complete();
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public long GetNextStudentNumber(Faculty faculty, Department department, Level level, Session session)
        {
            try
            {
                long newStudentNumber = 0;
                List<ApplicationFormView> applicationForms = (from a in repository.GetBy<VW_ASSIGNED_MATRIC_NUMBER>(a => a.Faculty_Id == faculty.Id && a.Department_Id == department.Id && a.Level_Id == level.Id && a.Session_Id == session.Id)
                                                              select new ApplicationFormView
                                                              {
                                                                  FormId = a.Application_Form_Id.Value,
                                                                  StudentNumber =(long) a.Student_Number,
                                                                  MatricNumber = a.Matric_Number,
                                                                  PersonId = a.Person_Id,
                                                              }).ToList();

                if (applicationForms != null && applicationForms.Count > 0)
                {
                    long rawMaxStudentNumber = applicationForms.Max(s => s.StudentNumber);
                    newStudentNumber = rawMaxStudentNumber + 1;
                }

                return newStudentNumber;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<ApplicantResult> GetHNDApplicants(Programme programme, Department department, Session session, bool checkSchool)
        {
            try
            {
                List<ApplicantResult> applicantResults = new List<ApplicantResult>();

                if (checkSchool)
                {
                    applicantResults = (from sr in repository.GetBy<VW_APPLICANT_DETAILS_HND>(x => x.Programme_Id == programme.Id && x.Department_Id == department.Id && x.Session_Id == session.Id && x.Previous_School_Name.Contains("ILARO"))
                                        select new ApplicantResult()
                                        {
                                            OLevelType = sr.O_Level_Type_Name,
                                            OLevelYear = sr.Exam_Year.ToString(),
                                            NumberOfSittings = sr.O_Level_Exam_Sitting_Id,
                                            SubjectName = sr.O_Level_Subject_Name,
                                            Grade = sr.O_Level_Grade_Name,
                                            Name = sr.Name,
                                            Programme = sr.Programme_Name,
                                            Department = sr.Department_Name,
                                            Session = sr.Session_Name,
                                            ApplicationFormNumber = sr.Application_Form_Number,
                                            Sex = sr.Sex_Name,
                                            State = sr.State_Name,
                                            LocalGovernment = sr.Local_Government_Name,
                                            PreviousCourse = sr.Previous_Course,
                                            PreviousEducationPeriod = sr.Previous_Education_Start_Date.Year + " - " + sr.Previous_Education_End_Date.Year,
                                            PreviousEducationResultGrade = sr.Level_of_Pass,
                                            PreviousSchoolName = sr.Previous_School_Name,
                                            EducationalQualificationName = sr.Educational_Qualification_Name,
                                            ITDurationName = sr.IT_Duration_Name,
                                            RejectReason = sr.Reject_Reason,
                                            Rejected = sr.Rejected,
                                            RejectedStr = sr.Rejected.ToString(),
                                            ExamNumber = sr.Application_Exam_Number,
                                            ScreeningScore = sr.RAW_SCORE,
                                            PersonId = sr.Person_Id,
                                            DepartmentOption = sr.Department_Option_Name
                                        }).ToList();
                }
                else
                {
                    applicantResults = (from sr in repository.GetBy<VW_APPLICANT_DETAILS_HND>(x => x.Programme_Id == programme.Id && x.Department_Id == department.Id && x.Session_Id == session.Id)
                                        select new ApplicantResult()
                                        {
                                            OLevelType = sr.O_Level_Type_Name,
                                            OLevelYear = sr.Exam_Year.ToString(),
                                            NumberOfSittings = sr.O_Level_Exam_Sitting_Id,
                                            SubjectName = sr.O_Level_Subject_Name,
                                            Grade = sr.O_Level_Grade_Name,
                                            Name = sr.Name,
                                            Programme = sr.Programme_Name,
                                            Department = sr.Department_Name,
                                            Session = sr.Session_Name,
                                            ApplicationFormNumber = sr.Application_Form_Number,
                                            Sex = sr.Sex_Name,
                                            State = sr.State_Name,
                                            LocalGovernment = sr.Local_Government_Name,
                                            PreviousCourse = sr.Previous_Course,
                                            PreviousEducationPeriod = sr.Previous_Education_Start_Date.Year + " - " + sr.Previous_Education_End_Date.Year,
                                            PreviousEducationResultGrade = sr.Level_of_Pass,
                                            PreviousSchoolName = sr.Previous_School_Name,
                                            EducationalQualificationName = sr.Educational_Qualification_Name,
                                            ITDurationName = sr.IT_Duration_Name,
                                            RejectReason = sr.Reject_Reason,
                                            Rejected = sr.Rejected,
                                            RejectedStr = sr.Rejected.ToString(),
                                            ExamNumber = sr.Application_Exam_Number,
                                            ScreeningScore = sr.RAW_SCORE,
                                            PersonId = sr.Person_Id,
                                            DepartmentOption = sr.Department_Option_Name
                                        }).ToList(); 
                }
                

                List<ApplicantResult> masterList = new List<ApplicantResult>();
                List<long> regNumbers = applicantResults.Select(r => r.PersonId).Distinct().ToList();
                for (int i = 0; i < regNumbers.Count; i++)
                {
                    List<ApplicantResult> results = applicantResults.Where(r => r.PersonId == regNumbers[i]).ToList();
                    string firstOLevelType = results.FirstOrDefault().OLevelType;
                    string firstOLevelYear = results.FirstOrDefault().OLevelYear;
                    int firstOlevelSitting = results.FirstOrDefault().NumberOfSittings;
                    string OLevelResults = "";
                    int checkSittingChange = 0;
                    string oLevelType = "";
                    string oLevelYear = "";
                    string sitting = "";

                    for (int j = 0; j < results.Count; j++)
                    {
                        if (results[j].NumberOfSittings != firstOlevelSitting)
                        {
                            checkSittingChange += 1;
                            results[j].Sitting = "Two Sittings";
                            sitting = results[j].Sitting;
                            results[j].OLevelType = firstOLevelType + " | " + results[j].OLevelType;
                            oLevelType = results[j].OLevelType;
                            results[j].OLevelYear = firstOLevelYear + " | " + results[j].OLevelYear;
                            oLevelYear = results[j].OLevelYear;
                        }
                        else
                        {
                            if (oLevelType != "" && oLevelYear != "" && sitting != "")
                            {
                                results[j].Sitting = sitting;
                                results[j].OLevelYear = oLevelYear;
                                results[j].OLevelType = oLevelType;
                            }
                            else
                            {
                                results[j].Sitting = "One Sitting";
                            }
                        }

                        if (checkSittingChange == 1)
                        {
                            OLevelResults += "==== 2ND RESULT ====> ";
                        }
                        OLevelResults += results[j].SubjectName + " : " + results[j].Grade + " | ";
                        results[j].OLevelResults = OLevelResults;
                    }

                    results.LastOrDefault().OLevelResults = OLevelResults;
                }

                masterList = applicantResults.GroupBy(a => a.PersonId).Select(a => a.Last()).ToList();

                return masterList.OrderBy(m => m.PersonId).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<ApplicantResult> GetHNDApplicants(Programme programme, Department department, DepartmentOption departmentOption, Session session, bool checkSchool)
        {
            try
            {
                List<ApplicantResult> applicantResults = new List<ApplicantResult>();

                if (checkSchool)
                {
                    applicantResults = (from sr in repository.GetBy<VW_APPLICANT_DETAILS_HND>(x => x.Programme_Id == programme.Id && x.Department_Id == department.Id && x.Department_Option_Id == departmentOption.Id && x.Session_Id == session.Id && x.Previous_School_Name.Contains("ILARO"))
                                        select new ApplicantResult()
                                        {
                                            OLevelType = sr.O_Level_Type_Name,
                                            OLevelYear = sr.Exam_Year.ToString(),
                                            NumberOfSittings = sr.O_Level_Exam_Sitting_Id,
                                            SubjectName = sr.O_Level_Subject_Name,
                                            Grade = sr.O_Level_Grade_Name,
                                            Name = sr.Name,
                                            Programme = sr.Programme_Name,
                                            Department = sr.Department_Name,
                                            Session = sr.Session_Name,
                                            ApplicationFormNumber = sr.Application_Form_Number,
                                            Sex = sr.Sex_Name,
                                            State = sr.State_Name,
                                            LocalGovernment = sr.Local_Government_Name,
                                            PreviousCourse = sr.Previous_Course,
                                            PreviousEducationPeriod = sr.Previous_Education_Start_Date.Year + " - " + sr.Previous_Education_End_Date.Year,
                                            PreviousEducationResultGrade = sr.Level_of_Pass,
                                            PreviousSchoolName = sr.Previous_School_Name,
                                            EducationalQualificationName = sr.Educational_Qualification_Name,
                                            ITDurationName = sr.IT_Duration_Name,
                                            RejectReason = sr.Reject_Reason,
                                            Rejected = sr.Rejected,
                                            RejectedStr = sr.Rejected.ToString(),
                                            ExamNumber = sr.Application_Exam_Number,
                                            ScreeningScore = sr.RAW_SCORE,
                                            PersonId = sr.Person_Id,
                                            DepartmentOption = sr.Department_Option_Name,
                                            CertificateStatus = Convert.ToBoolean(sr.Certificate_Status) ? "Collected" : "Not Collected",
                                            ConvocationStatus = Convert.ToBoolean(sr.Convocation_Status) ? "Present" : "Absent"
                                        }).ToList();
                }
                else
                {
                    applicantResults = (from sr in repository.GetBy<VW_APPLICANT_DETAILS_HND>(x => x.Programme_Id == programme.Id && x.Department_Id == department.Id && x.Department_Option_Id == departmentOption.Id && x.Session_Id == session.Id)
                                        select new ApplicantResult()
                                        {
                                            OLevelType = sr.O_Level_Type_Name,
                                            OLevelYear = sr.Exam_Year.ToString(),
                                            NumberOfSittings = sr.O_Level_Exam_Sitting_Id,
                                            SubjectName = sr.O_Level_Subject_Name,
                                            Grade = sr.O_Level_Grade_Name,
                                            Name = sr.Name,
                                            Programme = sr.Programme_Name,
                                            Department = sr.Department_Name,
                                            Session = sr.Session_Name,
                                            ApplicationFormNumber = sr.Application_Form_Number,
                                            Sex = sr.Sex_Name,
                                            State = sr.State_Name,
                                            LocalGovernment = sr.Local_Government_Name,
                                            PreviousCourse = sr.Previous_Course,
                                            PreviousEducationPeriod = sr.Previous_Education_Start_Date.Year + " - " + sr.Previous_Education_End_Date.Year,
                                            PreviousEducationResultGrade = sr.Level_of_Pass,
                                            PreviousSchoolName = sr.Previous_School_Name,
                                            EducationalQualificationName = sr.Educational_Qualification_Name,
                                            ITDurationName = sr.IT_Duration_Name,
                                            RejectReason = sr.Reject_Reason,
                                            Rejected = sr.Rejected,
                                            RejectedStr = sr.Rejected.ToString(),
                                            ExamNumber = sr.Application_Exam_Number,
                                            ScreeningScore = sr.RAW_SCORE,
                                            PersonId = sr.Person_Id,
                                            DepartmentOption = sr.Department_Option_Name,
                                            CertificateStatus = Convert.ToBoolean(sr.Certificate_Status) ? "Collected" : "Not Collected",
                                            ConvocationStatus = Convert.ToBoolean(sr.Convocation_Status) ? "Present" : "Absent"
                                        }).ToList();
                }


                List<ApplicantResult> masterList = new List<ApplicantResult>();
                List<long> regNumbers = applicantResults.Select(r => r.PersonId).Distinct().ToList();
                for (int i = 0; i < regNumbers.Count; i++)
                {
                    List<ApplicantResult> results = applicantResults.Where(r => r.PersonId == regNumbers[i]).ToList();
                    string firstOLevelType = results.FirstOrDefault().OLevelType;
                    string firstOLevelYear = results.FirstOrDefault().OLevelYear;
                    int firstOlevelSitting = results.FirstOrDefault().NumberOfSittings;
                    string OLevelResults = "";
                    int checkSittingChange = 0;
                    string oLevelType = "";
                    string oLevelYear = "";
                    string sitting = "";

                    for (int j = 0; j < results.Count; j++)
                    {
                        if (results[j].NumberOfSittings != firstOlevelSitting)
                        {
                            checkSittingChange += 1;
                            results[j].Sitting = "Two Sittings";
                            sitting = results[j].Sitting;
                            results[j].OLevelType = firstOLevelType + " | " + results[j].OLevelType;
                            oLevelType = results[j].OLevelType;
                            results[j].OLevelYear = firstOLevelYear + " | " + results[j].OLevelYear;
                            oLevelYear = results[j].OLevelYear;
                        }
                        else
                        {
                            if (oLevelType != "" && oLevelYear != "" && sitting != "")
                            {
                                results[j].Sitting = sitting;
                                results[j].OLevelYear = oLevelYear;
                                results[j].OLevelType = oLevelType;
                            }
                            else
                            {
                                results[j].Sitting = "One Sitting";
                            }
                        }

                        if (checkSittingChange == 1)
                        {
                            OLevelResults += "==== 2ND RESULT ====> ";
                        }
                        OLevelResults += results[j].SubjectName + " : " + results[j].Grade + " | ";
                        results[j].OLevelResults = OLevelResults;
                    }

                    results.LastOrDefault().OLevelResults = OLevelResults;
                }

                masterList = applicantResults.GroupBy(a => a.PersonId).Select(a => a.Last()).ToList();

                return masterList.OrderBy(m => m.PersonId).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<ApplicantResult> GetApplicantDetails(Programme programme, Department department, Session session, DateTime? dateFrom, DateTime? dateTo)
        {
            try
            {
                List<ApplicantResult> applicantResults = new List<ApplicantResult>();

                if (dateFrom == null || dateTo == null)
                {
                    applicantResults = (from sr in repository.GetBy<VW_PUTME_RESULT_DETAILS>(x => x.Programme_Id == programme.Id && x.Department_Id == department.Id && x.Session_Id == session.Id)
                                        select new ApplicantResult()
                                        {
                                            OLevelType = sr.O_Level_Type_Name,
                                            OLevelYear = sr.Exam_Year.ToString(),
                                            NumberOfSittings = sr.O_Level_Exam_Sitting_Id,
                                            SubjectName = sr.O_Level_Subject_Name,
                                            Grade = sr.O_Level_Grade_Name,
                                            Name = sr.Name,
                                            Programme = sr.Programme_Name,
                                            Department = sr.Department_Name,
                                            Session = sr.Session_Name,
                                            ApplicationFormNumber = sr.Application_Form_Number,
                                            Sex = sr.Sex_Name,
                                            State = sr.State_Name,
                                            LocalGovernment = sr.Local_Government_Name,
                                            RejectReason = sr.Reject_Reason,
                                            Rejected = sr.Rejected,
                                            RejectedStr = sr.Rejected.ToString(),
                                            ExamNumber = sr.Application_Exam_Number,
                                            ScreeningScore = sr.RAW_SCORE,
                                            PersonId = sr.Person_Id,
                                            DepartmentOption = sr.Department_Option_Name,
                                            ScannedCopyUrl = sr.Scanned_Copy_Url,
                                            JambRegNumber = sr.Applicant_Jamb_Registration_Number
                                        }).ToList();
                }
                else
                {
                    applicantResults = (from sr in repository.GetBy<VW_PUTME_RESULT_DETAILS>(x => x.Programme_Id == programme.Id && x.Department_Id == department.Id && x.Session_Id == session.Id && 
                                                                                            x.Date_Submitted >= dateFrom && x.Date_Submitted <= dateTo)
                                        select new ApplicantResult()
                                        {
                                            OLevelType = sr.O_Level_Type_Name,
                                            OLevelYear = sr.Exam_Year.ToString(),
                                            NumberOfSittings = sr.O_Level_Exam_Sitting_Id,
                                            SubjectName = sr.O_Level_Subject_Name,
                                            Grade = sr.O_Level_Grade_Name,
                                            Name = sr.Name,
                                            Programme = sr.Programme_Name,
                                            Department = sr.Department_Name,
                                            Session = sr.Session_Name,
                                            ApplicationFormNumber = sr.Application_Form_Number,
                                            Sex = sr.Sex_Name,
                                            State = sr.State_Name,
                                            LocalGovernment = sr.Local_Government_Name,
                                            RejectReason = sr.Reject_Reason,
                                            Rejected = sr.Rejected,
                                            RejectedStr = sr.Rejected.ToString(),
                                            ExamNumber = sr.Application_Exam_Number,
                                            ScreeningScore = sr.RAW_SCORE,
                                            PersonId = sr.Person_Id,
                                            DepartmentOption = sr.Department_Option_Name,
                                            ScannedCopyUrl = sr.Scanned_Copy_Url,
                                            JambRegNumber = sr.Applicant_Jamb_Registration_Number
                                        }).ToList();
                }

                
                List<ApplicantResult> masterList = new List<ApplicantResult>();
                List<long> regNumbers = applicantResults.Select(r => r.PersonId).Distinct().ToList();
                for (int i = 0; i < regNumbers.Count; i++)
                {
                    List<ApplicantResult> results = applicantResults.Where(r => r.PersonId == regNumbers[i]).ToList();
                    string firstOLevelType = results.FirstOrDefault().OLevelType;
                    string firstOLevelYear = results.FirstOrDefault().OLevelYear;
                    int firstOlevelSitting = results.FirstOrDefault().NumberOfSittings;
                    string OLevelResults = "";
                    int checkSittingChange = 0;
                    string oLevelType = "";
                    string oLevelYear = "";
                    string sitting = "";

                    for (int j = 0; j < results.Count; j++)
                    {
                        if (results[j].NumberOfSittings != firstOlevelSitting)
                        {
                            checkSittingChange += 1;
                            results[j].Sitting = "Two Sittings";
                            sitting = results[j].Sitting;
                            results[j].OLevelType = firstOLevelType + " | " + results[j].OLevelType;
                            oLevelType = results[j].OLevelType;
                            results[j].OLevelYear = firstOLevelYear + " | " + results[j].OLevelYear;
                            oLevelYear = results[j].OLevelYear;
                        }
                        else
                        {
                            if (oLevelType != "" && oLevelYear != "" && sitting != "")
                            {
                                results[j].Sitting = sitting;
                                results[j].OLevelYear = oLevelYear;
                                results[j].OLevelType = oLevelType;
                            }
                            else
                            {
                                results[j].Sitting = "One Sitting";
                            }
                        }

                        if (checkSittingChange == 1)
                        {
                            OLevelResults += "==== 2ND RESULT ====> ";
                        }
                        OLevelResults += results[j].SubjectName + " : " + results[j].Grade + " | ";
                        results[j].OLevelResults = OLevelResults;
                    }

                    results.LastOrDefault().OLevelResults = OLevelResults;
                }

                masterList = applicantResults.GroupBy(a => a.PersonId).Select(a => a.Last()).ToList();

                return masterList.OrderBy(m => m.PersonId).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<ApplicantResult> GetScannedOlevelResultsHND(Programme programme, Department department, Session session, DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                List<ApplicantResult> applicantResults = new List<ApplicantResult>();

                    applicantResults = (from sr in repository.GetBy<VW_APPLICANT_DETAILS_HND>(x => x.Programme_Id == programme.Id && x.Department_Id == department.Id && x.Session_Id == session.Id &&
                                             (x.Date_Submitted >= dateFrom && x.Date_Submitted <= dateTo))
                                        select new ApplicantResult()
                                        {
                                            OLevelType = sr.O_Level_Type_Name,
                                            OLevelYear = sr.Exam_Year.ToString(),
                                            NumberOfSittings = sr.O_Level_Exam_Sitting_Id,
                                            SubjectName = sr.O_Level_Subject_Name,
                                            Grade = sr.O_Level_Grade_Name,
                                            Name = sr.Name,
                                            Programme = sr.Programme_Name,
                                            Department = sr.Department_Name,
                                            Session = sr.Session_Name,
                                            ApplicationFormNumber = sr.Application_Form_Number,
                                            Sex = sr.Sex_Name,
                                            State = sr.State_Name,
                                            LocalGovernment = sr.Local_Government_Name,
                                            PreviousCourse = sr.Previous_Course,
                                            PreviousEducationPeriod = sr.Previous_Education_Start_Date.Year + " - " + sr.Previous_Education_End_Date.Year,
                                            PreviousEducationResultGrade = sr.Level_of_Pass,
                                            PreviousSchoolName = sr.Previous_School_Name,
                                            EducationalQualificationName = sr.Educational_Qualification_Name,
                                            ITDurationName = sr.IT_Duration_Name,
                                            RejectReason = sr.Reject_Reason,
                                            Rejected = sr.Rejected,
                                            RejectedStr = sr.Rejected.ToString(),
                                            ExamNumber = sr.Application_Exam_Number,
                                            ScreeningScore = sr.RAW_SCORE,
                                            PersonId = sr.Person_Id,
                                            DepartmentOption = sr.Department_Option_Name ,
                                            ScannedCopyUrl = sr.Scanned_Copy_Url,
                                            CertificateUrl = sr.Certificate_Copy_Url,
                                            NdResultUrl = sr.Result_Copy_Url,
                                            ITLetterOfCompletion=sr.IT_Letter_Of_Completion
                                        }).ToList();

                return applicantResults;
                

            }
            catch (Exception)
            {
                
                throw;
            }
        }
        public List<ApplicantResult> GetRejectedHNDApplicants(Programme programme, Department department, Session session)
        {
            try
            {
                List<ApplicantResult> applicantResults = new List<ApplicantResult>();

                
                applicantResults = (from sr in repository.GetBy<VW_APPLICANT_DETAILS_HND>(x => x.Programme_Id == programme.Id && x.Department_Id == department.Id && x.Session_Id == session.Id && x.Rejected)
                                    select new ApplicantResult()
                                    {
                                        OLevelType = sr.O_Level_Type_Name,
                                        OLevelYear = sr.Exam_Year.ToString(),
                                        NumberOfSittings = sr.O_Level_Exam_Sitting_Id,
                                        SubjectName = sr.O_Level_Subject_Name,
                                        Grade = sr.O_Level_Grade_Name,
                                        Name = sr.Name,
                                        Programme = sr.Programme_Name,
                                        Department = sr.Department_Name,
                                        Session = sr.Session_Name,
                                        ApplicationFormNumber = sr.Application_Form_Number,
                                        Sex = sr.Sex_Name,
                                        State = sr.State_Name,
                                        LocalGovernment = sr.Local_Government_Name,
                                        PreviousCourse = sr.Previous_Course,
                                        PreviousEducationPeriod = sr.Previous_Education_Start_Date.Year + " - " + sr.Previous_Education_End_Date.Year,
                                        PreviousEducationResultGrade = sr.Level_of_Pass,
                                        PreviousSchoolName = sr.Previous_School_Name,
                                        EducationalQualificationName = sr.Educational_Qualification_Name,
                                        ITDurationName = sr.IT_Duration_Name,
                                        RejectReason = sr.Reject_Reason,
                                        Rejected = sr.Rejected,
                                        RejectedStr = sr.Rejected.ToString() ,
                                        ExamNumber = sr.Application_Exam_Number,
                                        ScreeningScore = sr.RAW_SCORE,
                                        PersonId = sr.Person_Id,
                                        DepartmentOption = sr.Department_Option_Name
                                    }).ToList();

                List<ApplicantResult> masterList = new List<ApplicantResult>();
                List<long> regNumbers = applicantResults.Select(r => r.PersonId).Distinct().ToList();
                for (int i = 0; i < regNumbers.Count; i++)
                {
                    List<ApplicantResult> results = applicantResults.Where(r => r.PersonId == regNumbers[i]).ToList();
                    string firstOLevelType = results.FirstOrDefault().OLevelType;
                    string firstOLevelYear = results.FirstOrDefault().OLevelYear;
                    int firstOlevelSitting = results.FirstOrDefault().NumberOfSittings;
                    string OLevelResults = "";
                    int checkSittingChange = 0;
                    string oLevelType = "";
                    string oLevelYear = "";
                    string sitting = "";

                    for (int j = 0; j < results.Count; j++)
                    {
                        if (results[j].NumberOfSittings != firstOlevelSitting)
                        {
                            checkSittingChange += 1;
                            results[j].Sitting = "Two Sittings";
                            sitting = results[j].Sitting;
                            results[j].OLevelType = firstOLevelType + " | " + results[j].OLevelType;
                            oLevelType = results[j].OLevelType;
                            results[j].OLevelYear = firstOLevelYear + " | " + results[j].OLevelYear;
                            oLevelYear = results[j].OLevelYear;
                        }
                        else
                        {
                            if (oLevelType != "" && oLevelYear != "" && sitting != "")
                            {
                                results[j].Sitting = sitting;
                                results[j].OLevelYear = oLevelYear;
                                results[j].OLevelType = oLevelType;
                            }
                            else
                            {
                                results[j].Sitting = "One Sitting";
                            }
                        }

                        if (checkSittingChange == 1)
                        {
                            OLevelResults += "==== 2ND RESULT ====> ";
                        }
                        OLevelResults += results[j].SubjectName + " : " + results[j].Grade + " | ";
                        results[j].OLevelResults = OLevelResults;
                    }

                    results.LastOrDefault().OLevelResults = OLevelResults;
                }

                masterList = applicantResults.GroupBy(a => a.PersonId).Select(a => a.Last()).ToList();

                return masterList.OrderBy(m => m.PersonId).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<ApplicantResult> GetRejectedApplicants(Programme programme, Department department, Session session)
        {
            try
            {
                List<ApplicantResult> applicantResults = new List<ApplicantResult>();


                applicantResults = (from sr in repository.GetBy<VW_PUTME_RESULT_DETAILS>(x => x.Programme_Id == programme.Id && x.Department_Id == department.Id && x.Session_Id == session.Id && x.Rejected)
                                    select new ApplicantResult()
                                    {
                                        OLevelType = sr.O_Level_Type_Name,
                                        OLevelYear = sr.Exam_Year.ToString(),
                                        NumberOfSittings = sr.O_Level_Exam_Sitting_Id,
                                        SubjectName = sr.O_Level_Subject_Name,
                                        Grade = sr.O_Level_Grade_Name,
                                        Name = sr.Name,
                                        Programme = sr.Programme_Name,
                                        Department = sr.Department_Name,
                                        Session = sr.Session_Name,
                                        ApplicationFormNumber = sr.Application_Form_Number,
                                        Sex = sr.Sex_Name,
                                        State = sr.State_Name,
                                        LocalGovernment = sr.Local_Government_Name,
                                        RejectReason = sr.Reject_Reason,
                                        Rejected = sr.Rejected,
                                        RejectedStr = sr.Rejected.ToString(),
                                        ExamNumber = sr.Application_Exam_Number,
                                        ScreeningScore = sr.RAW_SCORE,
                                        PersonId = sr.Person_Id,
                                        DepartmentOption = sr.Department_Option_Name
                                    }).ToList();

                List<ApplicantResult> masterList = new List<ApplicantResult>();
                List<long> regNumbers = applicantResults.Select(r => r.PersonId).Distinct().ToList();
                for (int i = 0; i < regNumbers.Count; i++)
                {
                    List<ApplicantResult> results = applicantResults.Where(r => r.PersonId == regNumbers[i]).ToList();
                    string firstOLevelType = results.FirstOrDefault().OLevelType;
                    string firstOLevelYear = results.FirstOrDefault().OLevelYear;
                    int firstOlevelSitting = results.FirstOrDefault().NumberOfSittings;
                    string OLevelResults = "";
                    int checkSittingChange = 0;
                    string oLevelType = "";
                    string oLevelYear = "";
                    string sitting = "";

                    for (int j = 0; j < results.Count; j++)
                    {
                        if (results[j].NumberOfSittings != firstOlevelSitting)
                        {
                            checkSittingChange += 1;
                            results[j].Sitting = "Two Sittings";
                            sitting = results[j].Sitting;
                            results[j].OLevelType = firstOLevelType + " | " + results[j].OLevelType;
                            oLevelType = results[j].OLevelType;
                            results[j].OLevelYear = firstOLevelYear + " | " + results[j].OLevelYear;
                            oLevelYear = results[j].OLevelYear;
                        }
                        else
                        {
                            if (oLevelType != "" && oLevelYear != "" && sitting != "")
                            {
                                results[j].Sitting = sitting;
                                results[j].OLevelYear = oLevelYear;
                                results[j].OLevelType = oLevelType;
                            }
                            else
                            {
                                results[j].Sitting = "One Sitting";
                            }
                        }

                        if (checkSittingChange == 1)
                        {
                            OLevelResults += "==== 2ND RESULT ====> ";
                        }
                        OLevelResults += results[j].SubjectName + " : " + results[j].Grade + " | ";
                        results[j].OLevelResults = OLevelResults;
                    }

                    results.LastOrDefault().OLevelResults = OLevelResults;
                }

                masterList = applicantResults.GroupBy(a => a.PersonId).Select(a => a.Last()).ToList();

                return masterList.OrderBy(m => m.PersonId).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<MissingDocuments> GetApplicantsCredentialUploads(Programme programme,Session session)
        {
            try
            {
                List<ApplicantResult> applicantResults = new List<ApplicantResult>();


                applicantResults = (from sr in repository.GetBy<VW_APPLICANT_UPLOAD>(x => x.Programme_Id == programme.Id && x.Session_Id == session.Id && (x.ResultUrl!=null || x.CertificateUrl!=null || x.OLevelResult!=null))
                                    select new ApplicantResult()
                                    {
                                        OLevelType = sr.O_Level_Type_Name,
                                        NumberOfSittings = sr.O_Level_Exam_Sitting_Id,
                                        Name = sr.Name,
                                        Programme = sr.Programme_Name,
                                        Department = sr.Department_Name,
                                        Session = sr.Session_Name,
                                        ApplicationFormNumber = sr.Application_Form_Number,
                                        NdResultUrl=sr.ResultUrl,
                                        OLevelResults=sr.OLevelResult,
                                        DepartmentOption = sr.Department_Option_Name,
                                        PhoneNo=sr.Mobile_Phone,
                                        Email=sr.Email
                                    }).ToList();

                List<MissingDocuments> missingDocumentsList = new List<MissingDocuments>();
                if (applicantResults?.Count > 0)
                {
                    foreach(var item in applicantResults)
                    {
                        MissingDocuments missingDocuments = new MissingDocuments();
                        //check for olevel Result
                        if (!String.IsNullOrEmpty(item.OLevelResults))
                        {
                            string filePathOlevel = System.Web.HttpContext.Current.Server.MapPath(item.OLevelResults);
                            if (!System.IO.File.Exists(filePathOlevel))
                            {
                                missingDocuments.OlevelType = item.OLevelType;
                                missingDocuments.Olevel = "No found";
                            }
                        }
                        //check for ND Result
                        if (!String.IsNullOrEmpty(item.NdResultUrl))
                        {
                            string filePathNDResult = System.Web.HttpContext.Current.Server.MapPath(item.NdResultUrl);
                            if (!System.IO.File.Exists(filePathNDResult))
                            {
                                missingDocuments.NDResult = "No found";
                            }
                        }
                        //check for ND Certificate
                        if (!String.IsNullOrEmpty(item.CertificateUrl))
                        {
                            string filePathCertificate = System.Web.HttpContext.Current.Server.MapPath(item.CertificateUrl);
                            if (!System.IO.File.Exists(filePathCertificate))
                            {
                                missingDocuments.Certificate = "No found";
                            }
                        }

                        missingDocuments.ApplicationNumber = item.ApplicationFormNumber;
                        missingDocuments.Department = item.Department;
                        missingDocuments.Name = item.Name;
                        missingDocuments.Email = item.Email;
                        missingDocuments.PhoneNo = item.PhoneNo;
                        missingDocuments.OlevelType = item.OLevelType;
                        if (item.NumberOfSittings == 1)
                        {
                            missingDocuments.OlevelSitting = "First Sitting";
                        }
                        else if (item.NumberOfSittings == 2)
                        {
                            missingDocuments.OlevelSitting = "Second Sitting";
                        }
                        missingDocumentsList.Add(missingDocuments);
                    }
                }
                return missingDocumentsList.OrderBy(f => f.Department).ToList();
               
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<ApplicantResult> GetApplicantsCredentialUploadsForBulkDownload(Programme programme, Session session,Department department, DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                List<ApplicantResult> applicantResults = new List<ApplicantResult>();


                applicantResults = (from sr in repository.GetBy<VW_APPLICANT_UPLOAD>(x => x.Programme_Id == programme.Id && x.Session_Id == session.Id && x.Department_Id==department.Id
                                    && (x.ResultUrl != null || x.CertificateUrl != null || x.OLevelResult != null) && (x.Date_Submitted >= dateFrom && x.Date_Submitted <= dateTo))
                
                                    select new ApplicantResult()
                                    {
                                        OLevelType = sr.O_Level_Type_Name,
                                        NumberOfSittings = sr.O_Level_Exam_Sitting_Id,
                                        Name = sr.Name,
                                        Programme = sr.Programme_Name,
                                        Department = sr.Department_Name,
                                        Session = sr.Session_Name,
                                        ApplicationFormNumber = sr.Application_Form_Number,
                                        NdResultUrl = sr.ResultUrl,
                                        OLevelResults = sr.OLevelResult,
                                        DepartmentOption = sr.Department_Option_Name,
                                        PhoneNo = sr.Mobile_Phone,
                                        Email = sr.Email,
                                        PersonId=sr.Person_Id,
                                        ProgrammeCode=sr.Programme_Short_Name,
                                        DepartmentCode=sr.Department_Code,
                                        ITLetterOfCompletion=sr.IT_Letter_Of_Completion
                                    }).ToList();
                return applicantResults.OrderBy(f => f.Department).ToList();

            }
            catch (Exception)
            {
                throw;
            }
        }

    }


}

