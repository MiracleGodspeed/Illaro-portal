using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Abundance_Nk.Model.Model;
using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Translator;
using System.Transactions;
using System.Linq.Expressions;
using System.Web.Helpers;
using Abundance_Nk.Model.Entity.Model;

namespace Abundance_Nk.Business
{
    public class StudentResultLogic : BusinessBaseLogic<StudentResult, STUDENT_RESULT>
    {
        private StudentResultDetailLogic studentResultDetailLogic;

        public StudentResultLogic()
        {
            translator = new StudentResultTranslator();
            studentResultDetailLogic = new StudentResultDetailLogic();
        }

        public List<StudentResult> GetBy(Level level, Programme programme, Department department, SessionSemester sessionSemester)
        {
            try
            {
                Expression<Func<STUDENT_RESULT, bool>> selector = sr => sr.Level_Id == level.Id && sr.Programme_Id == programme.Id && sr.Department_Id == department.Id && sr.Session_Semester_Id == sessionSemester.Id;
                return GetModelsBy(selector);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int GetTotalMaximumObtainableScore(Level level, Programme programme, Department department, SessionSemester sessionSemester)
        {
            try
            {
                Expression<Func<STUDENT_RESULT, bool>> selector = sr => sr.Level_Id == level.Id && sr.Programme_Id == programme.Id && sr.Department_Id == department.Id && sr.Session_Semester_Id == sessionSemester.Id;
                List<StudentResult> studentResults = GetModelsBy(selector) ;
                return studentResults.Sum(s => s.MaximumObtainableScore);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool Save(StudentResult resultHeader)
        {
            try
            {
                using (TransactionScope transaction = new TransactionScope())
                {
                    if (resultHeader.Results != null && resultHeader.Results.Count > 0)
                    {
                        Add(resultHeader);
                        transaction.Complete();
                    }
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public StudentResult Add(StudentResult resultHeader, CourseRegistrationDetailAudit courseRegistrationDetailAudit)
        {
            try
            {
                StudentResult newResultHeader = base.Create(resultHeader);
                if (newResultHeader == null || newResultHeader.Id == 0)
                {
                    throw new Exception("Result Header add opeartion failed! Please try again");
                }

                resultHeader.Id = newResultHeader.Id;

                List<StudentResultDetail> results = SetHeader(resultHeader);
                int rowsAdded = studentResultDetailLogic.Create(results);
                if (rowsAdded == 0)
                {
                    throw new Exception("Result Header was succesfully added, but Result Detail Add operation failed! Please try again");
                }

                CourseRegistrationDetailLogic courseRegistrationDetailLogic = new CourseRegistrationDetailLogic();
                if (courseRegistrationDetailLogic.UpdateCourseRegistrationScore(results, courseRegistrationDetailAudit))
                {
                    return resultHeader;
                }
                else
                {
                    throw new Exception("Registered course failed on update! Please try again");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private List<StudentResultDetail> SetHeader(StudentResult resultHeader)
        {
            try
            {
                foreach (StudentResultDetail result in resultHeader.Results)
                {
                    result.Header = resultHeader;
                }

                return resultHeader.Results;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<Result> GetTranscriptBy(Student student)
        {
            try
            {
                if (student == null || student.Id <= 0)
                {
                    throw new Exception("Student not set! Please select student and try again.");
                }

                List<Result> results = (from sr in repository.GetBy<VW_STUDENT_RESULT_2>(x => x.Person_Id == student.Id && x.Special_Case==null)
                                        select new Result
                                        {
                                            StudentId = sr.Person_Id,
                                            Sex = sr.Sex_Name,
                                            Name = sr.Name,
                                            MatricNumber = sr.Matric_Number,
                                            CourseId = sr.Course_Id,
                                            CourseCode = sr.Course_Code,
                                            CourseName = sr.Course_Name,
                                            CourseUnit = sr.Course_Unit,
                                            FacultyName = sr.Faculty_Name,
                                            TestScore = sr.Test_Score,
                                            ExamScore = sr.Exam_Score,
                                            Score = sr.Total_Score,
                                            Grade = sr.Grade,
                                            GradePoint = sr.Grade_Point,
                                            Email = sr.Email,
                                            Address = sr.Contact_Address,
                                            MobilePhone = sr.Mobile_Phone,
                                            PassportUrl = sr.Image_File_Url,
                                            GPCU = sr.Grade_Point * sr.Course_Unit,
                                            TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,

                                            SessionName = sr.Session_Name,
                                            Semestername = sr.Semester_Name,
                                            LevelName = sr.Level_Name,
                                            ProgrammeName = sr.Programme_Name,
                                            DepartmentName = sr.Department_Name,
                                            SessionSemesterId = sr.Session_Semester_Id,
                                            SessionSemesterSequenceNumber = sr.Sequence_Number,
                                            SessionId = sr.Session_Id,
                                            SemesterId = sr.Semester_Id,
                                            LevelId = sr.Level_Id,
                                            ProgrammeId = sr.Programme_Id,
                                            DepartmentId = sr.Department_Id
                                        }).ToList();

                List<int> distinctSessions = results.Select(s => s.SessionId).Distinct().ToList();
                List<int> distinctLevels = results.Select(s => s.LevelId).Distinct().ToList();

                Semester firstSemester = new Semester() { Id = (int)Semesters.FirstSemester };
                Semester secondSemester = new Semester() { Id = (int)Semesters.SecondSemester };

                decimal firstSemesterCGPA = 0M;
                decimal secondSemesterCGPA = 0M;

                string remark = "";

                SessionSemester sessionSemester = null;

                for (int i = 0; i < distinctSessions.Count; i++)
                {
                    int currentSessonId = distinctSessions[i];

                    List<Result> currentSessionFirstSemesterResults = results.Where(s => s.SessionId == currentSessonId && s.SemesterId == firstSemester.Id).ToList();
                    if (currentSessionFirstSemesterResults.Count > 0)
                    {
                        firstSemesterCGPA = Math.Round(Convert.ToDecimal(currentSessionFirstSemesterResults.Sum(s => s.GPCU) / currentSessionFirstSemesterResults.Sum(s => s.CourseUnit)), 2);

                        List<string> carryOverCourses = new List<string>();

                        int levelId = results.Where(l => l.SessionId == currentSessonId).LastOrDefault().LevelId;
                        if (levelId == (int)Levels.NDII || levelId == (int)Levels.HNDII)
                        {
                            sessionSemester = new SessionSemester() { Session = new Session() { Id = currentSessonId }, Semester = firstSemester };
                            carryOverCourses = GetSecondYearCarryOverCourses(sessionSemester, new Level() { Id = levelId }, new Programme() { Id = results.LastOrDefault().ProgrammeId ?? 0 }, new Department() { Id = results.LastOrDefault().DepartmentId  }, student);

                        }
                        else
                        {
                            sessionSemester = new SessionSemester() { Session = new Session() { Id = currentSessonId }, Semester = firstSemester };
                            carryOverCourses = GetFirstYearCarryOverCourses(sessionSemester, new Level() { Id = levelId }, new Programme() { Id = results.LastOrDefault().ProgrammeId ?? 0 }, new Department() { Id = results.LastOrDefault().DepartmentId }, student);

                        }

                        remark = GetGraduationStatus(firstSemesterCGPA, carryOverCourses);

                        for (int j = 0; j < currentSessionFirstSemesterResults.Count; j++)
                        {
                            currentSessionFirstSemesterResults[j].Remark = remark;
                        }
                    }

                    List<Result> currentSessionSecondSemesterResults = results.Where(s => s.SessionId == currentSessonId && s.SemesterId == secondSemester.Id).ToList();
                    if (currentSessionSecondSemesterResults.Count > 0)
                    {
                        secondSemesterCGPA = Math.Round(Convert.ToDecimal(currentSessionSecondSemesterResults.Sum(s => s.GPCU) / currentSessionSecondSemesterResults.Sum(s => s.CourseUnit)), 2);

                        List<string> carryOverCourses = new List<string>();

                        int levelId = results.Where(l => l.SessionId == currentSessonId).LastOrDefault().LevelId;
                        if (levelId == (int)Levels.NDII || levelId == (int)Levels.HNDII)
                        {
                            sessionSemester = new SessionSemester() { Session = new Session() { Id = currentSessonId }, Semester = secondSemester };
                            carryOverCourses = GetSecondYearCarryOverCourses(sessionSemester, new Level() { Id = levelId }, new Programme() { Id = results.LastOrDefault().ProgrammeId ?? 0 }, new Department() { Id = results.LastOrDefault().DepartmentId  }, student);

                        }
                        else
                        {
                            sessionSemester = new SessionSemester() { Session = new Session() { Id = currentSessonId }, Semester = secondSemester };
                            carryOverCourses = GetFirstYearCarryOverCourses(sessionSemester, new Level() { Id = levelId }, new Programme() { Id = results.LastOrDefault().ProgrammeId ?? 0 }, new Department() { Id = results.LastOrDefault().DepartmentId }, student);

                        }

                        remark = GetGraduationStatus(secondSemesterCGPA, carryOverCourses);
                        for (int j = 0; j < currentSessionSecondSemesterResults.Count; j++)
                        {
                            currentSessionSecondSemesterResults[j].Remark = remark;
                        }
                    }
                }

                return results;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public List<Result> GetTranscriptBy(Student student, Session session, Semester semester)
        {
            try
            {
                if (student == null || student.Id <= 0)
                {
                    throw new Exception("Student not set! Please select student and try again.");
                }

                List<Result> results = (from sr in repository.GetBy<VW_STUDENT_RESULT_2>(x => x.Person_Id == student.Id && x.Semester_Id == semester.Id && x.Session_Id == session.Id)
                                        select new Result
                                        {
                                            StudentId = sr.Person_Id,
                                            Sex = sr.Sex_Name,
                                            Name = sr.Name,
                                            MatricNumber = sr.Matric_Number,
                                            CourseId = sr.Course_Id,
                                            CourseCode = sr.Course_Code,
                                            CourseName = sr.Course_Name,
                                            CourseUnit = sr.Course_Unit,
                                            FacultyName = sr.Faculty_Name,
                                            TestScore = sr.Test_Score,
                                            ExamScore = sr.Exam_Score,
                                            Score = sr.Total_Score,
                                            Grade = sr.Grade,
                                            GradePoint = sr.Grade_Point,
                                            Email = sr.Email,
                                            Address = sr.Contact_Address,
                                            MobilePhone = sr.Mobile_Phone,
                                            PassportUrl = sr.Image_File_Url,
                                            GPCU = sr.Grade_Point * sr.Course_Unit,
                                            TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,

                                            SessionName = sr.Session_Name,
                                            Semestername = sr.Semester_Name,
                                            LevelName = sr.Level_Name,
                                            ProgrammeName = sr.Programme_Name,
                                            DepartmentName = sr.Department_Name,
                                            SessionSemesterId = sr.Session_Semester_Id,
                                            SessionSemesterSequenceNumber = sr.Sequence_Number,
                                            SessionId = sr.Session_Id,
                                            SemesterId = sr.Semester_Id,
                                            LevelId = sr.Level_Id,
                                            ProgrammeId = sr.Programme_Id,
                                            DepartmentId = sr.Department_Id
                                        }).ToList();

                List<int> distinctSessions = results.Select(s => s.SessionId).Distinct().ToList();
                List<int> distinctLevels = results.Select(s => s.LevelId).Distinct().ToList();

                Semester firstSemester = new Semester() { Id = (int)Semesters.FirstSemester };
                Semester secondSemester = new Semester() { Id = (int)Semesters.SecondSemester };

                decimal firstSemesterCGPA = 0M;
                decimal secondSemesterCGPA = 0M;

                string remark = "";

                SessionSemester sessionSemester = null;

                for (int i = 0; i < distinctSessions.Count; i++)
                {
                    int currentSessonId = distinctSessions[i];

                    List<Result> currentSessionFirstSemesterResults = results.Where(s => s.SessionId == currentSessonId && s.SemesterId == firstSemester.Id).ToList();
                    if (currentSessionFirstSemesterResults.Count > 0)
                    {
                        firstSemesterCGPA = Math.Round(Convert.ToDecimal(currentSessionFirstSemesterResults.Sum(s => s.GPCU) / currentSessionFirstSemesterResults.Sum(s => s.CourseUnit)), 2);

                        List<string> carryOverCourses = new List<string>();

                        int levelId = results.Where(l => l.SessionId == currentSessonId).LastOrDefault().LevelId;
                        if (levelId == (int)Levels.NDII || levelId == (int)Levels.HNDII)
                        {
                            sessionSemester = new SessionSemester() { Session = new Session() { Id = currentSessonId }, Semester = firstSemester };
                            carryOverCourses = GetSecondYearCarryOverCourses(sessionSemester, new Level() { Id = levelId }, new Programme() { Id = results.LastOrDefault().ProgrammeId ?? 0 }, new Department() { Id = results.LastOrDefault().DepartmentId }, student);

                        }
                        else
                        {
                            sessionSemester = new SessionSemester() { Session = new Session() { Id = currentSessonId }, Semester = firstSemester };
                            carryOverCourses = GetFirstYearCarryOverCourses(sessionSemester, new Level() { Id = levelId }, new Programme() { Id = results.LastOrDefault().ProgrammeId ?? 0 }, new Department() { Id = results.LastOrDefault().DepartmentId }, student);

                        }

                        remark = GetGraduationStatus(firstSemesterCGPA, carryOverCourses);

                        for (int j = 0; j < currentSessionFirstSemesterResults.Count; j++)
                        {
                            currentSessionFirstSemesterResults[j].Remark = remark;
                        }
                    }

                    List<Result> currentSessionSecondSemesterResults = results.Where(s => s.SessionId == currentSessonId && s.SemesterId == secondSemester.Id).ToList();
                    if (currentSessionSecondSemesterResults.Count > 0)
                    {
                        secondSemesterCGPA = Math.Round(Convert.ToDecimal(currentSessionSecondSemesterResults.Sum(s => s.GPCU) / currentSessionSecondSemesterResults.Sum(s => s.CourseUnit)), 2);

                        List<string> carryOverCourses = new List<string>();

                        int levelId = results.Where(l => l.SessionId == currentSessonId).LastOrDefault().LevelId;
                        if (levelId == (int)Levels.NDII || levelId == (int)Levels.HNDII)
                        {
                            sessionSemester = new SessionSemester() { Session = new Session() { Id = currentSessonId }, Semester = secondSemester };
                            carryOverCourses = GetSecondYearCarryOverCourses(sessionSemester, new Level() { Id = levelId }, new Programme() { Id = results.LastOrDefault().ProgrammeId ?? 0 }, new Department() { Id = results.LastOrDefault().DepartmentId }, student);

                        }
                        else
                        {
                            sessionSemester = new SessionSemester() { Session = new Session() { Id = currentSessonId }, Semester = secondSemester };
                            carryOverCourses = GetFirstYearCarryOverCourses(sessionSemester, new Level() { Id = levelId }, new Programme() { Id = results.LastOrDefault().ProgrammeId ?? 0 }, new Department() { Id = results.LastOrDefault().DepartmentId }, student);

                        }

                        remark = GetGraduationStatus(secondSemesterCGPA, carryOverCourses);
                        for (int j = 0; j < currentSessionSecondSemesterResults.Count; j++)
                        {
                            currentSessionSecondSemesterResults[j].Remark = remark;
                        }
                    }
                }

                return results;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<StatementOfResultSummary> GetStatementOfResultSummaryBy(SessionSemester sessionSemester, Level level, Programme programme, Department department, Student student)
        {
            try
            {
                if (level == null || level.Id <= 0 || programme == null || programme.Id <= 0 || department == null || department.Id <= 0 || student == null || student.Id <= 0)
                {
                    throw new Exception("One or more criteria to get Master Result Sheet not set! Please check your input criteria selection and try again.");
                }

                SessionSemesterLogic sessionSemesterLogic = new SessionSemesterLogic();
                SessionSemester ss = sessionSemesterLogic.GetBy(sessionSemester.Id);


                //List<Result> results = (from sr in repository.GetBy<VW_STUDENT_RESULT_SUMMARY>(x => x.Programme_Id == programme.Id && x.Department_Id == department.Id && x.Person_Id == student.Id)
                //                        select new Result
                //                        {
                //                            StudentId = sr.Person_Id,
                //                            Sex = sr.Sex_Name,
                //                            Name = sr.Name,
                //                            MatricNumber = sr.Matric_Number,
                //                            CourseUnit = (int)sr.Course_Unit,
                //                            FacultyName = sr.Faculty_Name,
                //                            TestScore = sr.Test_Score,
                //                            ExamScore = sr.Exam_Score,
                //                            Score = sr.Total_Score,
                //                            SessionSemesterId = sr.Session_Semester_Id,
                //                            SessionSemesterSequenceNumber = sr.Session_Semester_Sequence_Number,
                //                            GradePoint = sr.Grade_Point,
                //                            GPA = sr.GPA,
                //                            WGP = sr.WGP,
                //                            UnitPassed = sr.Unit_Passed,
                //                            UnitOutstanding = sr.Unit_Outstanding,
                //                            GPCU = sr.Grade_Point * sr.Course_Unit,
                //                            TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                //                            SemesterId = sr.Semester_Id,
                //                            SessionId = sr.Session_Id,
                //                            LevelId = sr.Level_Id
                //                        }).ToList();

                List<Result> studentResults = (from sr in repository.GetBy<VW_STUDENT_RESULT_2>(x => x.Person_Id == student.Id && x.Session_Semester_Id <= sessionSemester.Id)
                                                 select new Result
                                                 {
                                                     StudentId = sr.Person_Id,
                                                     Sex = sr.Sex_Name,
                                                     Name = sr.Name,
                                                     MatricNumber = sr.Matric_Number,
                                                     CourseUnit = (int)sr.Course_Unit,
                                                     TotalSemesterCourseUnit = (int)sr.Total_Semester_Course_Unit,
                                                     FacultyName = sr.Faculty_Name,
                                                     TestScore = sr.Test_Score,
                                                     ExamScore = sr.Exam_Score,
                                                     Score = sr.Total_Score,
                                                     SessionSemesterId = sr.Session_Semester_Id,
                                                     GradePoint = sr.Grade_Point,
                                                     UnitOutstanding = (int)sr.Course_Unit,
                                                     GPCU = sr.Grade_Point * sr.Course_Unit,
                                                     WGP = sr.WGP,
                                                     SemesterId = sr.Semester_Id,
                                                     CourseId = sr.Course_Id,
                                                     CourseModeId = sr.Course_Mode_Id,
                                                     SpecialCase = sr.Special_Case,
                                                     SessionId = sr.Session_Id,
                                                     LevelId = sr.Level_Id,
                                                     TotalScore=(sr.Exam_Score + sr.Test_Score)
                                                 }).ToList();

                
                List<Result> currentStudentResults = studentResults.Where(k => k.SessionSemesterId == sessionSemester.Id).ToList();
                List<Result> otherYearStudentResults = new List<Result>();
                studentResults.ForEach(r =>
                {
                    //if (r.SessionSemesterId != sessionSemester.Id && r.LevelId == currentStudentResults.FirstOrDefault().LevelId)
                    if (r.SessionSemesterId != sessionSemester.Id)
                        otherYearStudentResults.Add(r);
                });

                List<Result> otherCarryOverResults = otherYearStudentResults.Where(x => x.isCarryOver()).Distinct().ToList();
                List<Result> otherSpecialCases = otherYearStudentResults.Where(y => y.isSpecialCase()).ToList();


                List<Result> currentCarryOverResults = currentStudentResults.Where(x => x.isCarryOver()).ToList();
                List<Result> currentSpecialCases = currentStudentResults.Where(z => z.isSpecialCase()).ToList();
                currentCarryOverResults=RemoveExistingCarryOverFromList(otherCarryOverResults, currentCarryOverResults);



                List<StatementOfResultSummary> resultSummaries = new List<StatementOfResultSummary>();

                //if (results != null && results.Count > 0)
                    if (studentResults != null && studentResults.Count > 0)
                    {
                    //Result currentSemesterResult = results.Where(r => r.SessionSemesterId == sessionSemester.Id).SingleOrDefault();
                    Result currentSemesterResult = studentResults.Where(r => r.SessionSemesterId == sessionSemester.Id).FirstOrDefault();
                    Result previousSemesterResult = new Result();

                    //Get previous semester
                    //if in seccond year first semester, get all semesters before that
                    List<Result> otheResults = new List<Result>();
                    SessionSemester previousSessionSemester = sessionSemesterLogic.GetPreviousSession(sessionSemester.Id);
                    
                    if (level.Id == (int)Levels.NDI || level.Id == (int)Levels.HNDI)
                    {
                        if (sessionSemester.Semester.Id == 2)
                        {
                            //otheResults = results.Where(r => r.SessionSemesterId == previousSessionSemester.Id).ToList();
                            otheResults = studentResults.Where(r => r.SessionSemesterId == previousSessionSemester.Id).ToList();
                        }
                        //consider programmes that do 3 semesters per session
                        else if ((programme.Id == 2 || programme.Id == 5) && (ss.Session.Id >= 9 &&  (level.Id == 1 || level.Id == 3)) && sessionSemester.Semester.Id == 3)
                        {
                            var firstPreviousSessionSemesterId = previousSessionSemester.Id - 1;
                            var firstPreviousSessionSemester = sessionSemesterLogic.GetModelsBy(d => d.Session_Semester_Id == firstPreviousSessionSemesterId).FirstOrDefault();
                            //otheResults = results.Where(r => r.SessionSemesterId == previousSessionSemester.Id || r.SessionSemesterId== firstPreviousSessionSemester.Id).ToList();
                            otheResults = studentResults.Where(r => r.SessionSemesterId == previousSessionSemester.Id || r.SessionSemesterId == firstPreviousSessionSemester.Id).ToList();
                        }
                        else
                        {
                            if (otheResults.Count == 0)
                            {
                                Result previousSemester = new Result()
                                {
                                    StudentId = studentResults.FirstOrDefault().StudentId,
                                    Sex = studentResults.FirstOrDefault().Sex,
                                    Name = studentResults.FirstOrDefault().Name,
                                    MatricNumber = studentResults.FirstOrDefault().MatricNumber,
                                    CourseUnit = 0,
                                    FacultyName = studentResults.FirstOrDefault().FacultyName,
                                    TestScore = 0,
                                    ExamScore = 0,
                                    Score = 0,
                                    SessionSemesterId = studentResults.FirstOrDefault().SessionSemesterId,
                                    SessionSemesterSequenceNumber = studentResults.FirstOrDefault().SessionSemesterSequenceNumber,
                                    GradePoint = 0,
                                    GPA = 0,
                                    WGP = 0,
                                    UnitPassed = 0,
                                    UnitOutstanding = 0,
                                    GPCU = 0,
                                    TotalSemesterCourseUnit = 0,
                                    SemesterId = 0,
                                    LevelId = 0
                                };
                                otheResults.Add(previousSemester);
                            }
                        }
                    }
                    else
                    {
                        List<Result> otheResultsFirstYear = new List<Result>();
                        //otheResultsFirstYear = results.Where(r => r.LevelId == (int)Levels.NDI || r.LevelId == (int)Levels.HNDI).ToList();
                        otheResultsFirstYear = studentResults.Where(r => r.LevelId == (int)Levels.NDI || r.LevelId == (int)Levels.HNDI).ToList();

                        if (sessionSemester.Semester.Id == (int)Semesters.FirstSemester)
                        {
                           // otheResults = results.Where(r => r.LevelId == (int)Levels.NDI && r.SessionSemesterId == previousSessionSemester.Id).ToList();
                           if(IsCarryOverStudent(student, ss.Session))
                                //otheResults = results.Where(r => (r.LevelId == (int)Levels.HNDII || r.LevelId == (int)Levels.NDII) && r.SessionId != ss.Session.Id).ToList();
                                otheResults = studentResults.Where(r => (r.LevelId == (int)Levels.HNDII || r.LevelId == (int)Levels.NDII) && r.SessionId != ss.Session.Id).ToList();
                        }
                        else
                        {
                            //otheResults = results.Where(r => (r.LevelId == (int)Levels.HNDII || r.LevelId == (int)Levels.NDII) && r.SessionSemesterId == previousSessionSemester.Id).ToList();
                            otheResults = studentResults.Where(r => (r.LevelId == (int)Levels.HNDII || r.LevelId == (int)Levels.NDII) && r.SessionSemesterId <= previousSessionSemester.Id).ToList();

                            if (IsCarryOverStudent(student, ss.Session))
                            {
                                //List<Result> otherResultsNDII = results.Where(r => (r.LevelId == (int)Levels.HNDII || r.LevelId == (int)Levels.NDII) && r.SessionId != ss.Session.Id).ToList();
                                List<Result> otherResultsNDII = studentResults.Where(r => (r.LevelId == (int)Levels.HNDII || r.LevelId == (int)Levels.NDII) && r.SessionId != ss.Session.Id).ToList();
                                if (otherResultsNDII != null && otherResultsNDII.Count > 0)
                                    otheResults.AddRange(otherResultsNDII);
                            }
                        }
                        otheResults.AddRange(otheResultsFirstYear);
                    }

                    otheResults = otheResults.Where(r => r.GPCU != null).ToList();
                    //capture all other attempted course irrespective of passing or not


                    List<Result> allOtherResult = studentResults.Where(s => s.SessionSemesterId != sessionSemester.Id).ToList();

                    for (int i = 0; i < allOtherResult.Count(); i++)
                    {
                        var thisResult = allOtherResult[i];
                        if (thisResult.ContainsResult(currentStudentResults) && thisResult.isCarryOver())
                        {
                            var otherResultToModify = otheResults.Where(r => r.LevelId == thisResult.LevelId && r.SessionId == thisResult.SessionId).LastOrDefault();
                            if (otherResultToModify != null)
                            {
                                //otherResultToModify.UnitOutstanding -= thisResult.CourseUnit;
                            }
                        }
                            
                    }

                    bool passedCarryOverCourseOthers = false;
                    int unitToDeductCarryOverOthers = 0;

                    if (otherCarryOverResults.Count > 0)
                    {
                        List<long> passedCarryOverCourses = studentResults.Count(r => (r.TestScore + r.ExamScore) >= 40) > 0 ? studentResults.Where(r => (r.TestScore + r.ExamScore) >= 40 && 
                                                                    r.SpecialCase == null).Select(c => c.CourseId).ToList() : new List<long>();

                        if (passedCarryOverCourses.Count > 0)
                        {
                            List<long> duplicateCO = new List<long>();

                            List<long> checkedCourseId = new List<long>();

                            for (int i = 0; i < otherCarryOverResults.Count; i++)
                            {
                                Result cResult = otherCarryOverResults[i];
                                
                                if (passedCarryOverCourses.Contains(cResult.CourseId))
                                {
                                    passedCarryOverCourseOthers = true;
                                    //check how many times the course appeared in the carryover list
                                    var listOfCourseIds=otherCarryOverResults.Select(f => f.CourseId).ToList();
                                    int count=TimesCourseExistInCarryoverList(listOfCourseIds, cResult.CourseId);
                                    if (!checkedCourseId.Contains(cResult.CourseId))
                                        unitToDeductCarryOverOthers += (studentResults.LastOrDefault(s => s.CourseId == cResult.CourseId).CourseUnit) * count;
                                }
                                else
                                {
                                    if (otherCarryOverResults.Count(c => c.CourseId == cResult.CourseId) > 1)
                                    {
                                        if (!duplicateCO.Contains(cResult.CourseId) && 
                                            ((otherCarryOverResults.FirstOrDefault(c => c.CourseId == cResult.CourseId).ExamScore + 
                                            otherCarryOverResults.FirstOrDefault(c => c.CourseId == cResult.CourseId).TestScore) < 40 ||
                                            (otherCarryOverResults.LastOrDefault(c => c.CourseId == cResult.CourseId).ExamScore +
                                            otherCarryOverResults.LastOrDefault(c => c.CourseId == cResult.CourseId).TestScore) < 40))
                                        {
                                            duplicateCO.Add(cResult.CourseId);
                                            passedCarryOverCourseOthers = false;
                                            // Remove an instance of the course
                                            //unitToDeductCarryOverOthers -= studentResults.LastOrDefault(s => s.CourseId == cResult.CourseId).CourseUnit;
                                        }
                                    }
                                }

                                checkedCourseId.Add(cResult.CourseId);
                            }
                        }
                    }

                    bool passedSpecialCaseOthers = false;
                    int unitToDeductSpecialCaseOthers = 0;

                    if (otherSpecialCases.Count > 0)
                    {
                        List<long> passedSpecialCaseCourses = studentResults.Count(r => (r.TestScore + r.ExamScore) >= 40) > 0 ? studentResults.Where(r => (r.TestScore + r.ExamScore) >= 40 && 
                                                                                            r.SpecialCase == null).Select(c => c.CourseId).ToList() : new List<long>();

                        if (passedSpecialCaseCourses.Count > 0)
                        {
                            List<long> duplicateCO = new List<long>();

                            for (int i = 0; i < otherSpecialCases.Count; i++)
                            {
                                Result cResult = otherSpecialCases[i];

                                List<long> checkedCourseId = new List<long>();

                                if (passedSpecialCaseCourses.Contains(cResult.CourseId))
                                {
                                    passedSpecialCaseOthers = true;
                                    //check how many times the course appeared in the carryover list
                                    var listOfCourseIds = otherSpecialCases.Select(f => f.CourseId).ToList();
                                    int count = TimesCourseExistInCarryoverList(listOfCourseIds, cResult.CourseId);
                                    if (!checkedCourseId.Contains(cResult.CourseId))
                                        unitToDeductSpecialCaseOthers += (studentResults.LastOrDefault(s => s.CourseId == cResult.CourseId).CourseUnit) * count; ;
                                }
                                else
                                {
                                    if (otherSpecialCases.Count(c => c.CourseId == cResult.CourseId) > 1)
                                    {
                                        if (!duplicateCO.Contains(cResult.CourseId) && 
                                            ((otherSpecialCases.FirstOrDefault(c => c.CourseId == cResult.CourseId).ExamScore +
                                            otherSpecialCases.FirstOrDefault(c => c.CourseId == cResult.CourseId).TestScore) < 40 ||
                                            (otherSpecialCases.LastOrDefault(c => c.CourseId == cResult.CourseId).ExamScore +
                                            otherSpecialCases.LastOrDefault(c => c.CourseId == cResult.CourseId).TestScore) < 40))
                                        {
                                            duplicateCO.Add(cResult.CourseId);
                                            passedSpecialCaseOthers = true;
                                            unitToDeductSpecialCaseOthers += studentResults.LastOrDefault(s => s.CourseId == cResult.CourseId).CourseUnit;
                                        }
                                    }
                                }

                                checkedCourseId.Add(cResult.CourseId);
                            }
                        }
                    }

                    bool passedCarryOverCourseCurrent = false;
                    int unitToDeductCarryOverCurrent = 0;

                    if (currentCarryOverResults.Count > 0)
                    {
                        List<long> passedCarryOverCourses = studentResults.Count(r => (r.TestScore + r.ExamScore) >= 40) > 0 ? studentResults.Where(r => (r.TestScore + r.ExamScore) >= 40 
                                                                    && r.SpecialCase == null).Select(c => c.CourseId).ToList() : new List<long>();

                        if (passedCarryOverCourses.Count > 0)
                        {
                            List<long> duplicateCO = new List<long>();

                            for (int i = 0; i < currentCarryOverResults.Count; i++)
                            {
                                Result cResult = currentCarryOverResults[i];

                                List<long> checkedCourseId = new List<long>();

                                if (passedCarryOverCourses.Contains(cResult.CourseId))
                                {
                                    passedCarryOverCourseCurrent = true;
                                    if (!checkedCourseId.Contains(cResult.CourseId))
                                        unitToDeductCarryOverCurrent += currentStudentResults.LastOrDefault(s => s.CourseId == cResult.CourseId).CourseUnit;
                                }
                                else
                                {
                                    if (currentCarryOverResults.Count(c => c.CourseId == cResult.CourseId) > 1)
                                    {
                                        if (!duplicateCO.Contains(cResult.CourseId) &&
                                            ((currentCarryOverResults.FirstOrDefault(c => c.CourseId == cResult.CourseId).ExamScore +
                                            currentCarryOverResults.FirstOrDefault(c => c.CourseId == cResult.CourseId).TestScore) < 40 ||
                                            (currentCarryOverResults.LastOrDefault(c => c.CourseId == cResult.CourseId).ExamScore +
                                            currentCarryOverResults.LastOrDefault(c => c.CourseId == cResult.CourseId).TestScore) < 40))
                                        {
                                            duplicateCO.Add(cResult.CourseId);
                                            passedCarryOverCourseCurrent = true;
                                            unitToDeductCarryOverCurrent += currentStudentResults.LastOrDefault(s => s.CourseId == cResult.CourseId).CourseUnit;
                                    
                                        }
                                    }
                                }

                                checkedCourseId.Add(cResult.CourseId);
                            }
                        }
                    }

                    bool passedSpecialCaseCurrent = false;
                    int unitToDeductSpecialCaseCurrent = 0;

                    if (currentSpecialCases.Count > 0)
                    {
                        List<long> passedSpecialCaseCourses = studentResults.Count(r => (r.TestScore + r.ExamScore) >= 40) > 0 ? studentResults.Where(r => (r.TestScore + r.ExamScore) >= 40 &&
                                                                                            r.SpecialCase != null).Select(c => c.CourseId).ToList() : new List<long>();

                        if (passedSpecialCaseCourses.Count > 0)
                        {
                            List<long> duplicateCO = new List<long>();

                            for (int i = 0; i < currentSpecialCases.Count; i++)
                            {
                                Result cResult = currentSpecialCases[i];

                                List<long> checkedCourseId = new List<long>();

                                if (passedSpecialCaseCourses.Contains(cResult.CourseId))
                                {
                                    passedSpecialCaseCurrent = true;
                                    if (!checkedCourseId.Contains(cResult.CourseId))
                                        unitToDeductSpecialCaseCurrent += currentStudentResults.LastOrDefault(s => s.CourseId == cResult.CourseId).CourseUnit;
                                }
                                else
                                {
                                    if (currentSpecialCases.Count(c => c.CourseId == cResult.CourseId) > 1)
                                    {
                                        if (!duplicateCO.Contains(cResult.CourseId) && 
                                            ((currentSpecialCases.FirstOrDefault(c => c.CourseId == cResult.CourseId).ExamScore +
                                            currentSpecialCases.FirstOrDefault(c => c.CourseId == cResult.CourseId).TestScore) < 40 ||
                                            (currentSpecialCases.LastOrDefault(c => c.CourseId == cResult.CourseId).ExamScore +
                                            currentSpecialCases.LastOrDefault(c => c.CourseId == cResult.CourseId).TestScore) < 40))
                                        {
                                            duplicateCO.Add(cResult.CourseId);
                                            passedSpecialCaseCurrent = true;
                                            unitToDeductSpecialCaseCurrent += currentStudentResults.LastOrDefault(s => s.CourseId == cResult.CourseId).CourseUnit;
                                        }
                                     }
                                }

                                checkedCourseId.Add(cResult.CourseId);
                            }
                        }
                    }
                    
                    //currentSemesterResult = CheckForSpecialCase()

                    StatementOfResultSummary unitsAttempted = new StatementOfResultSummary();
                    StatementOfResultSummary wgp = new StatementOfResultSummary();
                    StatementOfResultSummary gpa = new StatementOfResultSummary();
                    StatementOfResultSummary unitPassed = new StatementOfResultSummary();
                    StatementOfResultSummary unitsOutstanding = new StatementOfResultSummary();

                    unitsAttempted.Item = "UNITS ATTEMPTED";
                    wgp.Item = "WEIGHT GRADE POINTS";
                    gpa.Item = "GRADE POINT AVERAGE";
                    unitPassed.Item = "UNITS PASSED";
                    unitsOutstanding.Item = "UNITS OUTSTANDING";

                    int otherResultsSumCourseUnit = 0, otherResultsSumUnitPassed = 0, otherResultsSumUnitOutstanding = 0, currentSumUnitOutstanding = 0;

                    if (otheResults.Count > 0 )
                    {
                        otherResultsSumCourseUnit = otheResults.Where(f => f.SpecialCase == null).Sum(r => r.CourseUnit);
                        
                        int totalOutStandingUnit = 0;
                        if (otherCarryOverResults.Count > 0)
                        {
                            var distictCourseId=otherCarryOverResults.GroupBy(k=>k.CourseId).ToList();

                            for(int i=0; i<distictCourseId.Count; i++)
                            {
                                var courseId = distictCourseId[i].Key;
                                totalOutStandingUnit += otheResults.Where(f => f.CourseId == courseId).Select(g => g.CourseUnit).FirstOrDefault();
                            }
                        }
                        
                        //otherResultsSumUnitOutstanding = Convert.ToInt32(otheResults.Sum(r => r.UnitOutstanding));
                        otherResultsSumUnitOutstanding = (totalOutStandingUnit - unitToDeductCarryOverOthers);
                        // otherResultsSumUnitPassed = Convert.ToInt32(otheResults.Sum(r => r.UnitPassed));
                        otherResultsSumUnitPassed = otherResultsSumCourseUnit - otherResultsSumUnitOutstanding;
                        if (passedCarryOverCourseOthers && otherResultsSumUnitOutstanding != 0)
                        {
                            otherResultsSumUnitOutstanding = otherResultsSumUnitOutstanding - unitToDeductCarryOverOthers;
                        }
                        if (passedSpecialCaseOthers && otherResultsSumUnitOutstanding != 0)
                        {
                            otherResultsSumUnitOutstanding = otherResultsSumUnitOutstanding - unitToDeductSpecialCaseOthers;
                        }

                        unitsAttempted.PreviousSemester = otherResultsSumCourseUnit.ToString();
                        wgp.PreviousSemester = otheResults.Sum(r => r.WGP).ToString();
                        if (otherResultsSumCourseUnit != 0)
                        {
                            previousSemesterResult.GPA = Math.Round(Convert.ToDecimal(otheResults.Sum(r => r.WGP)) / otherResultsSumCourseUnit, 2);
                        }
                        else
                        {
                            previousSemesterResult.GPA = Math.Round(Convert.ToDecimal(otheResults.Sum(r => r.WGP)), 2);
                        }
                        gpa.PreviousSemester = Math.Round((decimal)previousSemesterResult.GPA, 2).ToString();

                        unitPassed.PreviousSemester = otherResultsSumUnitPassed.ToString();
                        unitsOutstanding.PreviousSemester = otherResultsSumUnitOutstanding< 0 ? "0": otherResultsSumUnitOutstanding.ToString();
                    }

                    //if (currentSemesterResult != null)
                    if (currentStudentResults != null && currentStudentResults.Count > 0)
                    {
                        //currentSumUnitOutstanding = Convert.ToInt32(currentSemesterResult.UnitOutstanding);
                        currentSumUnitOutstanding=currentStudentResults.Where(r => r.Score < 40).Sum(c => c.CourseUnit);

                        //currentSumUnitOutstanding = Convert.ToInt32(currentCarryOverResults.Select(d => d.UnitOutstanding).Sum());


                        if (passedCarryOverCourseCurrent && currentSumUnitOutstanding != 0)
                        {
                            currentSumUnitOutstanding = currentSumUnitOutstanding - unitToDeductCarryOverCurrent;
                        }
                        if (passedSpecialCaseCurrent && currentSumUnitOutstanding != 0)
                        {
                            currentSumUnitOutstanding = currentSumUnitOutstanding - unitToDeductSpecialCaseCurrent;
                        }

                        //unitsAttempted.CurrentSemester = currentSemesterResult.CourseUnit.ToString();
                        unitsAttempted.CurrentSemester = currentStudentResults.Where(f=>f.SpecialCase == null).Sum(f=>f.CourseUnit).ToString();
                        //wgp.CurrentSemester = currentSemesterResult.WGP.ToString(); 
                        wgp.CurrentSemester = currentStudentResults.Where(f => f.SpecialCase == null).Sum(f=>f.WGP).ToString();
                        //gpa.CurrentSemester = currentSemesterResult.GPA!=null?(Math.Round((decimal)currentSemesterResult.GPA, 2).ToString()):"0";

                        if (currentStudentResults.Where(f => f.SpecialCase == null).Sum(f => f.CourseUnit) == 0)
                            gpa.CurrentSemester = "0";
                        else
                        gpa.CurrentSemester = Math.Round(((decimal)currentStudentResults.Where(f => f.SpecialCase == null).Sum(g => g.GPCU) / currentStudentResults.Where(f => f.SpecialCase == null).Sum(f => f.CourseUnit)),2).ToString();
                        //unitPassed.CurrentSemester = currentSemesterResult.UnitPassed.ToString();
                        unitPassed.CurrentSemester = currentStudentResults.Where(r=>r.Score>=40).Sum(c=>c.CourseUnit).ToString();
                        //unitsOutstanding.CurrentSemester = currentSumUnitOutstanding<0 ? "0" : currentSumUnitOutstanding.ToString();
                        unitsOutstanding.CurrentSemester = currentStudentResults.Where(r => r.Score < 40).Sum(c => c.CourseUnit).ToString();
                    }

                    //unitsAttempted.AllSemester = currentSemesterResult != null && otheResults.Count <= 0 ? unitsAttempted.CurrentSemester : Convert.ToString(results.Where(r => r.GPCU != null).Sum(r => r.CourseUnit));
                    //wgp.AllSemester = currentSemesterResult != null && otheResults.Count <= 0 ? wgp.CurrentSemester : results.Where(r => r.GPCU != null).Sum(r => r.WGP).ToString();
                    //gpa.AllSemester = currentSemesterResult != null && otheResults.Count <= 0 ? gpa.CurrentSemester :Math.Round(Convert.ToDecimal(results.Where(r => r.GPCU != null).Sum(r => r.WGP)) / results.Where(r => r.GPCU != null).Sum(r => r.CourseUnit), 2).ToString();
                    //unitPassed.AllSemester = currentSemesterResult != null && otheResults.Count <= 0 ? unitPassed.CurrentSemester : results.Where(r => r.GPCU != null).Sum(r => r.UnitPassed).ToString();
                    //unitsOutstanding.AllSemester = currentSemesterResult != null && otheResults.Count <= 0 ? currentSumUnitOutstanding.ToString() :Convert.ToString(otherResultsSumUnitOutstanding + currentSumUnitOutstanding);

                    Result AllSemester = new Result();
                    decimal wgpSum = 0;
                    foreach(var rx in currentStudentResults.Where(r => r.WGP != null && r.SpecialCase==null))
                    {
                        wgpSum += rx.WGP ?? 0;
                    }
                    wgp.AllSemester = Convert.ToString(currentStudentResults.Where(g=>g.SpecialCase==null).Sum(r => r.WGP) + otheResults.Where(g => g.SpecialCase == null).Sum(r => r.WGP));
                    //unitsAttempted.AllSemester =  Convert.ToString(currentStudentResults.Where(r => r.GPCU != null && r.SpecialCase == null).Sum(r => r.CourseUnit) + otheResults.Where(r => r.GPCU != null &&  r.SpecialCase==null).Sum(r => r.CourseUnit));
                    unitsAttempted.AllSemester = Convert.ToString(currentStudentResults.Where(r => r.GPCU != null && r.SpecialCase == null).Sum(r => r.CourseUnit) + otherResultsSumCourseUnit);
                    if (Convert.ToDecimal(unitsAttempted.AllSemester) == 0)
                        gpa.AllSemester = "0";
                    else
                    gpa.AllSemester = Math.Round(Convert.ToDecimal(wgp.AllSemester) / Convert.ToDecimal(unitsAttempted.AllSemester), 2).ToString();
                    //unitPassed.AllSemester = Convert.ToString(currentStudentResults.Where(r => r.GPCU != null && (r.ExamScore + r.TestScore) >= 40).Sum(r => r.CourseUnit) + otheResults.Where(r => r.GPCU != null).Sum(r => r.UnitPassed));
                    unitPassed.AllSemester = Convert.ToString(currentStudentResults.Where(r => r.Score >= 40).Sum(c => c.CourseUnit) + otherResultsSumUnitPassed);
                    currentSumUnitOutstanding = currentStudentResults.Where(r => r.Score < 40).Sum(c => c.CourseUnit);
                    var outstanding=(otherResultsSumUnitOutstanding + currentSumUnitOutstanding) < 0 ? 0 : (otherResultsSumUnitOutstanding + currentSumUnitOutstanding);
                    unitsOutstanding.AllSemester = Convert.ToString(outstanding);


                    resultSummaries.Add(unitsAttempted);
                    resultSummaries.Add(wgp);
                    resultSummaries.Add(gpa);
                    resultSummaries.Add(unitPassed);
                    resultSummaries.Add(unitsOutstanding);
                }

                return resultSummaries;

            }
            catch (Exception)
            {
                throw;
            }
        }
        private bool IsCarryOverStudent(Student student, Session session)
        {
            bool extraYearStatus = false;
            try
            {
                StudentExtraYearLogic extraYearLogic = new StudentExtraYearLogic();
                StudentExtraYearSession extraYearSession = extraYearLogic.GetModelsBy(s => s.Person_Id == student.Id && s.Session_Id == session.Id).LastOrDefault();
                if (extraYearSession != null)
                {
                    extraYearStatus = true;
                }
            }
            catch (Exception)
            {
                throw;
            }

            return extraYearStatus;
        }

        //private decimal getCGPA(long studentId, int levelId, int departmentId, int programmeId, int semesterId, int sessionId)
        public decimal getCGPA(long studentId, int levelId, int departmentId, int programmeId, int semesterId, int sessionId)
        {
            Result overallResult = new Result();
            try
            {
                StudentLogic studentLogic = new StudentLogic();
                StudentResultLogic studentResultLogic = new StudentResultLogic();

                Session session = new Session() { Id = sessionId };
                Programme programme = new Programme() { Id = programmeId };
                Department department = new Department() { Id = departmentId };
                Level level = new Level() { Id = levelId };

                Abundance_Nk.Model.Model.Student studentCheck = studentLogic.GetModelBy(p => p.Person_Id == studentId);

                if (levelId == (int)Levels.NDI || levelId == (int)Levels.HNDI)
                {
                    if (semesterId == (int)Semesters.FirstSemester)
                    {
                        Semester firstSemester = new Semester() { Id = (int)Semesters.FirstSemester };
                        List<Result> result = null;
                        if (studentCheck.Activated == true || studentCheck.Activated == null)
                        {
                            result = studentResultLogic.GetStudentProcessedResultBy(session, level, department, studentCheck, firstSemester, programme);
                        }
                        else
                        {
                            result = studentResultLogic.GetDeactivatedStudentProcessedResultBy(session, level, department, studentCheck, firstSemester, programme);
                        }
                        List<Result> modifiedResultList = new List<Result>();

                        int totalSemesterCourseUnit = 0;
                        foreach (Result resultItem in result)
                        {
                            decimal WGP = 0;

                            if (resultItem.SpecialCase != null)
                            {
                                resultItem.GPCU = 0;
                                if (totalSemesterCourseUnit == 0)
                                {
                                    totalSemesterCourseUnit = (int)resultItem.TotalSemesterCourseUnit - resultItem.CourseUnit;
                                    resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                                    resultItem.Grade = "-";
                                }
                                else
                                {
                                    totalSemesterCourseUnit -= resultItem.CourseUnit;
                                    resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                                    resultItem.Grade = "-";
                                }
                            }
                            if (totalSemesterCourseUnit > 0)
                            {
                                resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                            }

                            modifiedResultList.Add(resultItem);
                        }

                        decimal? firstSemesterGPCUSum = modifiedResultList.Sum(p => p.GPCU);
                        int? firstSemesterTotalSemesterCourseUnit = 0;
                        overallResult = modifiedResultList.FirstOrDefault();
                        firstSemesterTotalSemesterCourseUnit = modifiedResultList.Min(p => p.TotalSemesterCourseUnit) ?? 0;
                        decimal? firstSemesterGPA = 0M;
                        if (firstSemesterGPCUSum != null && firstSemesterGPCUSum > 0)
                        {
                            firstSemesterGPA = firstSemesterGPCUSum / firstSemesterTotalSemesterCourseUnit;
                        }

                        if (firstSemesterGPA != null && firstSemesterGPA > 0)
                        {
                            overallResult.GPA = Decimal.Round((decimal)firstSemesterGPA, 2);
                        }
                        if (firstSemesterGPA != null && firstSemesterGPA > 0)
                        {
                            overallResult.CGPA = Decimal.Round((decimal)firstSemesterGPA, 2);
                        }
                    }
                    else
                    {
                        List<Result> result = null;
                        Semester firstSemester = new Semester() { Id = (int)Semesters.FirstSemester };
                        if (studentCheck.Activated == true || studentCheck.Activated == null)
                        {
                            result = studentResultLogic.GetStudentProcessedResultBy(session, level, department, studentCheck, firstSemester, programme);
                        }
                        else
                        {
                            result = studentResultLogic.GetDeactivatedStudentProcessedResultBy(session, level, department, studentCheck, firstSemester, programme);
                        }
                        List<Result> firstSemesterModifiedResultList = new List<Result>();

                        int totalFirstSemesterCourseUnit = 0;
                        foreach (Result resultItem in result)
                        {
                            decimal WGP = 0;

                            if (resultItem.SpecialCase != null)
                            {
                                resultItem.GPCU = 0;
                                if (totalFirstSemesterCourseUnit == 0)
                                {
                                    totalFirstSemesterCourseUnit = (int)resultItem.TotalSemesterCourseUnit - resultItem.CourseUnit;
                                    resultItem.TotalSemesterCourseUnit = totalFirstSemesterCourseUnit;
                                    resultItem.Grade = "-";
                                }
                                else
                                {
                                    totalFirstSemesterCourseUnit -= resultItem.CourseUnit;
                                    resultItem.TotalSemesterCourseUnit = totalFirstSemesterCourseUnit;
                                    resultItem.Grade = "-";
                                }

                            }
                            if (totalFirstSemesterCourseUnit > 0)
                            {
                                resultItem.TotalSemesterCourseUnit = totalFirstSemesterCourseUnit;
                            }
                            firstSemesterModifiedResultList.Add(resultItem);
                        }

                        decimal? firstSemesterGPCUSum = firstSemesterModifiedResultList.Sum(p => p.GPCU);
                        int? firstSemesterTotalSemesterCourseUnit = 0;
                        overallResult = firstSemesterModifiedResultList.FirstOrDefault();
                        firstSemesterTotalSemesterCourseUnit = firstSemesterModifiedResultList.Min(p => p.TotalSemesterCourseUnit) ?? 0;
                        decimal? firstSemesterGPA = 0M;
                        if (firstSemesterGPCUSum != null && firstSemesterGPCUSum > 0)
                        {
                            firstSemesterGPA = firstSemesterGPCUSum / firstSemesterTotalSemesterCourseUnit;
                        }

                        if (firstSemesterGPA != null && firstSemesterGPA > 0)
                        {
                            overallResult.GPA = Decimal.Round((decimal)firstSemesterGPA, 2);
                        }

                        Semester secondSemester = new Semester() { Id = (int)Semesters.SecondSemester };
                        List<Result> secondSemesterResult = null;
                        if (studentCheck.Activated == true || studentCheck.Activated == null)
                        {
                            secondSemesterResult = studentResultLogic.GetStudentProcessedResultBy(session, level, department, studentCheck, secondSemester, programme);
                        }
                        else
                        {
                            secondSemesterResult = studentResultLogic.GetDeactivatedStudentProcessedResultBy(session, level, department, studentCheck, secondSemester, programme);
                        }
                        List<Result> secondSemesterModifiedResultList = new List<Result>();

                        int totalSecondSemesterCourseUnit = 0;
                        foreach (Result resultItem in secondSemesterResult)
                        {

                            decimal WGP = 0;

                            if (resultItem.SpecialCase != null)
                            {

                                resultItem.GPCU = 0;
                                if (totalSecondSemesterCourseUnit == 0)
                                {
                                    totalSecondSemesterCourseUnit = (int)resultItem.TotalSemesterCourseUnit - resultItem.CourseUnit;
                                    resultItem.TotalSemesterCourseUnit = totalSecondSemesterCourseUnit;
                                    resultItem.Grade = "-";
                                }
                                else
                                {
                                    totalSecondSemesterCourseUnit -= resultItem.CourseUnit;
                                    resultItem.TotalSemesterCourseUnit = totalSecondSemesterCourseUnit;
                                    resultItem.Grade = "-";
                                }

                            }
                            if (totalSecondSemesterCourseUnit > 0)
                            {
                                resultItem.TotalSemesterCourseUnit = totalSecondSemesterCourseUnit;
                            }
                            secondSemesterModifiedResultList.Add(resultItem);
                        }
                        decimal? secondSemesterGPCUSum = secondSemesterModifiedResultList.Sum(p => p.GPCU);
                        Result secondSemesterStudentResult = secondSemesterModifiedResultList.FirstOrDefault();
                        overallResult = secondSemesterStudentResult;
                        if (secondSemesterGPCUSum != null && secondSemesterGPCUSum > 0)
                        {
                            overallResult.GPA = Decimal.Round((decimal)(secondSemesterGPCUSum / secondSemesterStudentResult.TotalSemesterCourseUnit), 2);
                        }
                        if (firstSemesterGPCUSum > 0 || secondSemesterGPCUSum > 0)
                        {
                            overallResult.CGPA = Decimal.Round((decimal)((firstSemesterGPCUSum + secondSemesterGPCUSum) / (secondSemesterModifiedResultList.Min(p => p.TotalSemesterCourseUnit) + firstSemesterTotalSemesterCourseUnit)), 2);
                        }
                    }
                }
                else
                {
                    decimal firstYearFirstSemesterGPCUSum = 0;
                    int firstYearFirstSemesterTotalCourseUnit = 0;
                    decimal firstYearSecondSemesterGPCUSum = 0;
                    int firstYearSecondSemesterTotalCourseUnit = 0;
                    decimal secondYearFirstSemesterGPCUSum = 0;
                    int secondYearFirstSemesterTotalCourseUnit = 0;
                    decimal secondYearSecondSemesterGPCUSum = 0;
                    int secondYearSecondSemesterTotalCourseUnit = 0;

                    Result firstYearFirstSemester = GetFirstYearFirstSemesterResultInfo(level.Id, department, programme, studentCheck);
                    Result firstYearSecondSemester = GetFirstYearSecondSemesterResultInfo(level.Id, department, programme, studentCheck);
                    if (semesterId == (int)Semesters.FirstSemester)
                    {
                        List<Result> result = null;
                        Semester firstSemester = new Semester() { Id = (int)Semesters.FirstSemester };

                        if (studentCheck.Activated == true || studentCheck.Activated == null)
                        {
                            result = studentResultLogic.GetStudentProcessedResultBy(session, level, department, studentCheck, firstSemester, programme);
                        }
                        else
                        {
                            result = studentResultLogic.GetDeactivatedStudentProcessedResultBy(session, level, department, studentCheck, firstSemester, programme);
                        }
                        List<Result> modifiedResultList = new List<Result>();
                        int totalSemesterCourseUnit = 0;
                        foreach (Result resultItem in result)
                        {

                            decimal WGP = 0;

                            if (resultItem.SpecialCase != null)
                            {

                                resultItem.GPCU = 0;
                                if (totalSemesterCourseUnit == 0)
                                {
                                    totalSemesterCourseUnit = (int)resultItem.TotalSemesterCourseUnit - resultItem.CourseUnit;
                                    resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                                    resultItem.Grade = "-";
                                }
                                else
                                {
                                    totalSemesterCourseUnit -= resultItem.CourseUnit;
                                    resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                                    resultItem.Grade = "-";
                                }

                            }
                            if (totalSemesterCourseUnit > 0)
                            {
                                resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                            }
                            modifiedResultList.Add(resultItem);
                        }
                        Result firstYearFirstSemesterResult = new Result();
                        decimal? firstSemesterGPCUSum = modifiedResultList.Sum(p => p.GPCU);
                        int? secondYearfirstSemesterTotalSemesterCourseUnit = 0;
                        secondYearfirstSemesterTotalSemesterCourseUnit = modifiedResultList.Min(p => p.TotalSemesterCourseUnit) ?? 0;
                        firstYearFirstSemesterResult.TotalSemesterCourseUnit = secondYearfirstSemesterTotalSemesterCourseUnit;
                        firstYearFirstSemesterResult.GPCU = firstSemesterGPCUSum;
                        decimal? firstSemesterGPA = 0M;
                        if (firstSemesterGPCUSum != null && firstSemesterGPCUSum > 0)
                        {
                            firstSemesterGPA = firstSemesterGPCUSum / secondYearfirstSemesterTotalSemesterCourseUnit;
                        }

                        overallResult = modifiedResultList.FirstOrDefault();
                        if (firstSemesterGPA != null && firstSemesterGPA > 0)
                        {
                            overallResult.GPA = Decimal.Round((decimal)firstSemesterGPA, 2);
                        }
                        if (firstSemesterGPCUSum != null && firstYearFirstSemester != null && firstYearSecondSemester != null)
                        {
                            if ((firstSemesterGPCUSum + firstYearFirstSemester.GPCU + firstYearSecondSemester.GPCU) > 0 && firstYearSecondSemester.TotalSemesterCourseUnit != null && firstYearFirstSemester.TotalSemesterCourseUnit != null && secondYearfirstSemesterTotalSemesterCourseUnit != null)
                            {
                                firstYearFirstSemester.TotalSemesterCourseUnit = firstYearFirstSemester.TotalSemesterCourseUnit ?? 0;
                                firstYearSecondSemester.TotalSemesterCourseUnit = firstYearSecondSemester.TotalSemesterCourseUnit ?? 0;
                                overallResult.CGPA = Decimal.Round((decimal)((firstSemesterGPCUSum + firstYearFirstSemester.GPCU + firstYearSecondSemester.GPCU) / (firstYearSecondSemester.TotalSemesterCourseUnit + firstYearFirstSemester.TotalSemesterCourseUnit + secondYearfirstSemesterTotalSemesterCourseUnit)), 2);
                            }
                        }
                    }
                    else if (semesterId == (int)Semesters.SecondSemester)
                    {

                        List<Result> result = null;
                        Semester firstSemester = new Semester() { Id = (int)Semesters.FirstSemester };

                        if (studentCheck.Activated == true || studentCheck.Activated == null)
                        {
                            result = studentResultLogic.GetStudentProcessedResultBy(session, level, department, studentCheck, firstSemester, programme);
                        }
                        else
                        {
                            result = studentResultLogic.GetDeactivatedStudentProcessedResultBy(session, level, department, studentCheck, firstSemester, programme);
                        }
                        List<Result> modifiedResultList = new List<Result>();
                        int totalSemesterCourseUnit = 0;
                        foreach (Result resultItem in result)
                        {

                            decimal WGP = 0;

                            if (resultItem.SpecialCase != null)
                            {

                                resultItem.GPCU = 0;
                                if (totalSemesterCourseUnit == 0)
                                {
                                    totalSemesterCourseUnit = (int)resultItem.TotalSemesterCourseUnit - resultItem.CourseUnit;
                                    resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                                    resultItem.Grade = "-";
                                }
                                else
                                {
                                    totalSemesterCourseUnit -= resultItem.CourseUnit;
                                    resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                                    resultItem.Grade = "-";
                                }

                            }
                            if (totalSemesterCourseUnit > 0)
                            {
                                resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                            }
                            modifiedResultList.Add(resultItem);
                        }
                        Result secondYearFirstSemesterResult = new Result();
                        decimal? secondYearfirstSemesterGPCUSum = modifiedResultList.Sum(p => p.GPCU);
                        int? secondYearfirstSemesterTotalSemesterCourseUnit = 0;
                        secondYearfirstSemesterTotalSemesterCourseUnit = modifiedResultList.Min(p => p.TotalSemesterCourseUnit) ?? 0;
                        secondYearFirstSemesterResult.TotalSemesterCourseUnit = secondYearfirstSemesterTotalSemesterCourseUnit;
                        secondYearFirstSemesterResult.GPCU = secondYearfirstSemesterGPCUSum;
                        decimal? firstSemesterGPA = 0M;
                        if (secondYearfirstSemesterGPCUSum != null && secondYearfirstSemesterGPCUSum > 0)
                        {
                            firstSemesterGPA = secondYearfirstSemesterGPCUSum / secondYearfirstSemesterTotalSemesterCourseUnit;
                        }



                        //Second semester second year

                        List<Result> secondSemesterResult = null;
                        Semester secondSemester = new Semester() { Id = (int)Semesters.SecondSemester };
                        if (studentCheck.Activated == true || studentCheck.Activated == null)
                        {
                            secondSemesterResult = studentResultLogic.GetStudentProcessedResultBy(session, level, department, studentCheck, secondSemester, programme);
                        }
                        else
                        {
                            secondSemesterResult = studentResultLogic.GetDeactivatedStudentProcessedResultBy(session, level, department, studentCheck, secondSemester, programme);
                        }
                        List<Result> secondSemesterModifiedResultList = new List<Result>();
                        int totalSecondSemesterCourseUnit = 0;
                        foreach (Result resultItem in secondSemesterResult)
                        {

                            decimal WGP = 0;
                            if (resultItem.SpecialCase != null)
                            {

                                resultItem.GPCU = 0;
                                if (totalSecondSemesterCourseUnit == 0)
                                {
                                    totalSecondSemesterCourseUnit = (int)resultItem.TotalSemesterCourseUnit - resultItem.CourseUnit;
                                    resultItem.TotalSemesterCourseUnit = totalSecondSemesterCourseUnit;
                                    resultItem.Grade = "-";
                                }
                                else
                                {
                                    totalSecondSemesterCourseUnit -= resultItem.CourseUnit;
                                    resultItem.TotalSemesterCourseUnit = totalSecondSemesterCourseUnit;
                                    resultItem.Grade = "-";
                                }

                            }
                            if (totalSecondSemesterCourseUnit > 0)
                            {
                                resultItem.TotalSemesterCourseUnit = totalSecondSemesterCourseUnit;
                            }
                            secondSemesterModifiedResultList.Add(resultItem);
                        }
                        Result secondYearSecondtSemesterResult = new Result();
                        decimal? secondYearSecondtSemesterGPCUSum = secondSemesterModifiedResultList.Sum(p => p.GPCU);
                        int? secondYearSecondSemesterTotalSemesterCourseUnit = 0;
                        secondYearSecondSemesterTotalSemesterCourseUnit = secondSemesterModifiedResultList.Min(p => p.TotalSemesterCourseUnit) ?? 0;
                        secondYearSecondtSemesterResult.TotalSemesterCourseUnit = secondYearSecondSemesterTotalSemesterCourseUnit;
                        secondYearSecondtSemesterResult.GPCU = secondYearSecondtSemesterGPCUSum;
                        decimal? secondYearSecondSmesterGPA = 0M;
                        if (secondYearSecondtSemesterGPCUSum != null && secondYearSecondtSemesterGPCUSum > 0)
                        {
                            secondYearSecondSmesterGPA = secondYearSecondtSemesterGPCUSum / secondYearSecondSemesterTotalSemesterCourseUnit;
                        }

                        overallResult = secondSemesterModifiedResultList.FirstOrDefault();
                        if (secondYearSecondSmesterGPA != null && secondYearSecondSmesterGPA > 0)
                        {
                            overallResult.GPA = Decimal.Round((decimal)secondYearSecondSmesterGPA, 2);
                        }
                        if (secondYearfirstSemesterGPCUSum != null && firstYearFirstSemester != null && firstYearSecondSemester != null)
                        {
                            firstYearFirstSemester.TotalSemesterCourseUnit = firstYearFirstSemester.TotalSemesterCourseUnit ?? 0;
                            firstYearSecondSemester.TotalSemesterCourseUnit = firstYearSecondSemester.TotalSemesterCourseUnit ?? 0;
                            overallResult.CGPA = Decimal.Round((decimal)((secondYearfirstSemesterGPCUSum + firstYearFirstSemester.GPCU + firstYearSecondSemester.GPCU + secondYearSecondtSemesterGPCUSum) / (firstYearSecondSemester.TotalSemesterCourseUnit + firstYearFirstSemester.TotalSemesterCourseUnit + secondYearfirstSemesterTotalSemesterCourseUnit + secondYearSecondSemesterTotalSemesterCourseUnit)), 2);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return Convert.ToDecimal(overallResult.CGPA);
        }
        public Result GetFirstYearSecondSemesterResultInfo(int levelId, Department department, Programme programme, Model.Model.Student student)
        {
            try
            {
                List<Result> result = null;
                StudentLogic studentLogic = new StudentLogic();
                StudentResultLogic studentResultLogic = new StudentResultLogic();
                Abundance_Nk.Model.Model.Student studentCheck = studentLogic.GetModelBy(p => p.Person_Id == student.Id);
                Semester semester = new Semester() { Id = (int)Semesters.SecondSemester  };
                Level level = null;
                if (levelId == (int)Levels.NDII)
                {
                    level = new Level() { Id = (int)Levels.NDI };
                }
                else
                {
                    level = new Level() { Id = (int)Levels.HNDI };
                }
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                StudentLevel studentLevel = studentLevelLogic.GetModelsBy(p => p.Person_Id == studentCheck.Id && p.Level_Id == level.Id && p.Department_Id == department.Id && p.Programme_Id == programme.Id).FirstOrDefault();

                // if student level is null create it for the student

                if (studentCheck.Activated == true || studentCheck.Activated == null)
                {
                    result = studentResultLogic.GetStudentProcessedResultBy(studentLevel.Session, level, studentLevel.Department, studentCheck, semester, studentLevel.Programme);
                }
                else
                {
                    result = studentResultLogic.GetDeactivatedStudentProcessedResultBy(studentLevel.Session, level, studentLevel.Department, studentCheck, semester, studentLevel.Programme);
                }
                List<Result> modifiedResultList = new List<Result>();
                int totalSemesterCourseUnit = 0;
                foreach (Result resultItem in result)
                {

                    decimal WGP = 0;

                    if (resultItem.SpecialCase != null)
                    {

                        resultItem.GPCU = 0;
                        if (totalSemesterCourseUnit == 0)
                        {
                            totalSemesterCourseUnit = (int)resultItem.TotalSemesterCourseUnit - resultItem.CourseUnit;
                            resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                            resultItem.Grade = "-";
                        }
                        else
                        {
                            totalSemesterCourseUnit -= resultItem.CourseUnit;
                            resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                            resultItem.Grade = "-";
                        }

                    }
                    if (totalSemesterCourseUnit > 0)
                    {
                        resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                    }
                    modifiedResultList.Add(resultItem);
                }
                Result firstYearFirstSemesterResult = new Result();
                decimal? firstSemesterGPCUSum = modifiedResultList.Sum(p => p.GPCU);
                int? firstSemesterTotalSemesterCourseUnit = 0;
                firstSemesterTotalSemesterCourseUnit = modifiedResultList.Min(p => p.TotalSemesterCourseUnit);
                firstYearFirstSemesterResult.TotalSemesterCourseUnit = firstSemesterTotalSemesterCourseUnit;
                firstYearFirstSemesterResult.GPCU = firstSemesterGPCUSum;
                return firstYearFirstSemesterResult;

            }
            catch (Exception)
            {

                throw;
            }
        }
        public Result GetFirstYearFirstSemesterResultInfo(int levelId, Department department, Programme programme, Model.Model.Student student)
        {
            try
            {
                List<Result> result = null;
                StudentLogic studentLogic = new StudentLogic();
                StudentResultLogic studentResultLogic = new StudentResultLogic();
                Abundance_Nk.Model.Model.Student studentCheck = studentLogic.GetModelBy(p => p.Person_Id == student.Id);


                Semester semester = new Semester() { Id = (int)Semesters.FirstSemester};
                Level level = null;
                if (levelId == (int)Levels.NDII)
                {
                    level = new Level() { Id = (int)Levels.NDI };
                }
                else
                {
                    level = new Level() { Id = (int)Levels.HNDI };
                }
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                StudentLevel studentLevel = studentLevelLogic.GetModelsBy(p => p.Person_Id == studentCheck.Id && p.Level_Id == level.Id && p.Department_Id == department.Id && p.Programme_Id == programme.Id).FirstOrDefault();
                if (studentCheck.Activated == true || studentCheck.Activated == null)
                {
                    result = studentResultLogic.GetStudentProcessedResultBy(studentLevel.Session, level, studentLevel.Department, studentCheck, semester, studentLevel.Programme);
                }
                else
                {
                    result = studentResultLogic.GetDeactivatedStudentProcessedResultBy(studentLevel.Session, level, studentLevel.Department, studentCheck, semester, studentLevel.Programme);
                }
                List<Result> modifiedResultList = new List<Result>();
                int totalSemesterCourseUnit = 0;
                foreach (Result resultItem in result)
                {

                    decimal WGP = 0;

                    if (resultItem.SpecialCase != null)
                    {

                        resultItem.GPCU = 0;
                        if (totalSemesterCourseUnit == 0)
                        {
                            totalSemesterCourseUnit = (int)resultItem.TotalSemesterCourseUnit - resultItem.CourseUnit;
                            resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                            resultItem.Grade = "-";
                        }
                        else
                        {
                            totalSemesterCourseUnit -= resultItem.CourseUnit;
                            resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                            resultItem.Grade = "-";
                        }

                    }
                    if (totalSemesterCourseUnit > 0)
                    {
                        resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                    }
                    modifiedResultList.Add(resultItem);
                }
                Result firstYearFirstSemesterResult = new Result();
                decimal? firstSemesterGPCUSum = modifiedResultList.Sum(p => p.GPCU);
                int? firstSemesterTotalSemesterCourseUnit = 0;
                firstSemesterTotalSemesterCourseUnit = modifiedResultList.Min(p => p.TotalSemesterCourseUnit);
                firstYearFirstSemesterResult.TotalSemesterCourseUnit = firstSemesterTotalSemesterCourseUnit;
                firstYearFirstSemesterResult.GPCU = firstSemesterGPCUSum;
                return firstYearFirstSemesterResult;

            }
            catch (Exception)
            {

                throw;
            }
        }
        public List<Result> GetProcessedResutBy(Session session, Semester semester, Level level, Department department, Programme programme)
        {
            try
            {
                if (session == null || session.Id < 0 || level == null || level.Id <= 0 || department == null || department.Id <= 0 || programme == null || programme.Id <= 0)
                {
                    throw new Exception("One or more criteria to get Result not set! Please check your input criteria selection and try again.");
                }
                SessionLogic sessionLogic = new SessionLogic();
                Session sessions = sessionLogic.GetModelBy(p => p.Session_Id == session.Id);
                string[] sessionItems = sessions.Name.Split('/');
                string sessionNameStr = sessionItems[0];
                int sessionNameInt = Convert.ToInt32(sessionNameStr);
                if (sessionNameInt >= 2015)
                {

                    List<Result> results = (from sr in repository.GetBy<VW_STUDENT_RESULT_2>(p => p.Programme_Id == programme.Id && p.Session_Id == session.Id && p.Level_Id == level.Id && p.Department_Id == department.Id && p.Semester_Id == semester.Id && p.Activated != false)
                                            select new Result
                                            {
                                                StudentId = sr.Person_Id,
                                                Sex = sr.Sex_Name,
                                                Name = sr.Name,
                                                MatricNumber = sr.Matric_Number,
                                                CourseId = sr.Course_Id,
                                                CourseCode = sr.Course_Code,
                                                CourseName = sr.Course_Name,
                                                CourseUnit = sr.Course_Unit,
                                                FacultyName = sr.Faculty_Name,
                                                TestScore = sr.Test_Score,
                                                ExamScore = sr.Exam_Score,
                                                Score = sr.Total_Score,
                                                Grade = sr.Grade,
                                                GradePoint = sr.Grade_Point,
                                                Email = sr.Email,
                                                SpecialCase = sr.Special_Case,
                                                Address = sr.Contact_Address,
                                                MobilePhone = sr.Mobile_Phone,
                                                PassportUrl = sr.Image_File_Url,
                                                GPCU = sr.Grade_Point * sr.Course_Unit,
                                                TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit ?? 0,
                                                Student_Type_Id = sr.Student_Type_Id,
                                                SessionName = sr.Session_Name,
                                                Semestername = sr.Semester_Name,
                                                LevelName = sr.Level_Name,
                                                WGP = sr.WGP,
                                                Activated = sr.Activated,
                                                Reason = sr.Reason,
                                                RejectCategory = sr.Reject_Category,
                                                firstname_middle = sr.Othernames,
                                                ProgrammeName = sr.Programme_Name,
                                                Surname = sr.Last_Name,
                                                Firstname = sr.First_Name,
                                                Othername = sr.Other_Name,
                                                TotalScore = sr.Total_Score,
                                                SessionSemesterId = sr.Session_Semester_Id,
                                                SessionSemesterSequenceNumber = sr.Sequence_Number,
                                            }).ToList();

                    return results;
                }
                else
                {

                    List<Result> results = (from sr in repository.GetBy<VW_STUDENT_RESULT_OLD_GRADING_SYSTEM>(p => p.Programme_Id == programme.Id && p.Session_Id == session.Id && p.Level_Id == level.Id && p.Department_Id == department.Id && p.Semester_Id == semester.Id && p.Activated != false)
                                            select new Result
                                            {
                                                StudentId = sr.Person_Id,
                                                Sex = sr.Sex_Name,
                                                Name = sr.Name,
                                                MatricNumber = sr.Matric_Number,
                                                CourseId = sr.Course_Id,
                                                CourseCode = sr.Course_Code,
                                                CourseName = sr.Course_Name,
                                                CourseUnit = sr.Course_Unit,
                                                FacultyName = sr.Faculty_Name,
                                                TestScore = sr.Test_Score,
                                                ExamScore = sr.Exam_Score,
                                                Score = sr.Total_Score,
                                                Grade = sr.Grade,
                                                GradePoint = sr.Grade_Point,
                                                Email = sr.Email,
                                                SpecialCase = sr.Special_Case,
                                                Address = sr.Contact_Address,
                                                MobilePhone = sr.Mobile_Phone,
                                                PassportUrl = sr.Image_File_Url,
                                                GPCU = sr.Grade_Point * sr.Course_Unit,
                                                TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                                Student_Type_Id = sr.Student_Type_Id,
                                                SessionName = sr.Session_Name,
                                                Semestername = sr.Semester_Name,
                                                LevelName = sr.Level_Name,
                                                WGP = sr.WGP,
                                                Activated = sr.Activated,
                                                Reason = sr.Reason,
                                                RejectCategory = sr.Reject_Category,
                                                firstname_middle = sr.Othernames,
                                                ProgrammeName = sr.Programme_Name,
                                                Surname = sr.Last_Name,
                                                Firstname = sr.First_Name,
                                                Othername = sr.Other_Name,
                                                TotalScore = sr.Total_Score,
                                                SessionSemesterId = sr.Session_Semester_Id,
                                                SessionSemesterSequenceNumber = sr.Sequence_Number,
                                            }).ToList();

                    return results;
                }




            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<Result> GetStudentProcessedResultBy(Session session, Level level, Department department, Student student, Semester semester, Programme programme)
        {
            try
            {

                if (session == null || session.Id < 0 || level == null || level.Id <= 0 || department == null || department.Id <= 0 || student == null || student.Id <= 0 || semester == null || semester.Id <= 0 || programme == null || programme.Id <= 0)
                {
                    throw new Exception("One or more criteria to get Result not set! Please check your input criteria selection and try again.");
                }
                SessionLogic sessionLogic = new SessionLogic();
                SESSION sessions = sessionLogic.GetEntityBy(p => p.Session_Id == session.Id);
                string[] sessionItems = sessions.Session_Name.Split('/');
                string sessionNameStr = sessionItems[0];
                int sessionNameInt = Convert.ToInt32(sessionNameStr);

                List<Result> results = (from sr in repository.GetBy<VW_STUDENT_RESULT_2>(p => p.Person_Id == student.Id && p.Programme_Id == programme.Id && p.Session_Id == session.Id && 
                                        p.Level_Id == level.Id && p.Department_Id == department.Id && p.Semester_Id == semester.Id && (p.Activated == true || p.Activated == null))
                                        select new Result
                                        {
                                            StudentId = sr.Person_Id,
                                            Sex = sr.Sex_Name,
                                            Name = sr.Name,
                                            MatricNumber = sr.Matric_Number,
                                            CourseId = sr.Course_Id,
                                            CourseCode = sr.Course_Code,
                                            CourseName = sr.Course_Name,
                                            CourseUnit = sr.Course_Unit,
                                            FacultyName = sr.Faculty_Name,
                                            DepartmentName = GetDepartmentOptionName(sr.Person_Id)==""? sr.Department_Name: sr.Department_Name + " " +"(" + GetDepartmentOptionName(sr.Person_Id)+")",
                                            ProgrammeId = sr.Programme_Id,
                                            TestScore = sr.Test_Score,
                                            ExamScore = sr.Exam_Score,
                                            Score = sr.Total_Score,
                                            Grade = sr.Grade,
                                            GradePoint = sr.Grade_Point,
                                            Email = sr.Email,
                                            Address = sr.Contact_Address,
                                            MobilePhone = sr.Mobile_Phone,
                                            PassportUrl = sr.Image_File_Url,
                                            GPCU = sr.Grade_Point * sr.Course_Unit,
                                            TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                            Student_Type_Id = sr.Student_Type_Id,
                                            SessionName = sr.Session_Name,
                                            Semestername = sr.Semester_Name,
                                            LevelName = sr.Level_Name,
                                            WGP = sr.WGP,
                                            SpecialCase = sr.Special_Case,
                                            Activated = sr.Activated,
                                            Reason = sr.Reason,
                                            RejectCategory = sr.Reject_Category,
                                            firstname_middle = sr.Othernames,
                                            ProgrammeName = sr.Programme_Name,
                                            Surname = sr.Last_Name,
                                            Firstname = sr.First_Name,
                                            Othername = sr.Other_Name,
                                            TotalScore = sr.Total_Score,
                                            SessionSemesterId = sr.Session_Semester_Id,
                                            SessionSemesterSequenceNumber = sr.Sequence_Number,
                                            SessionId = sr.Session_Id
                                        }).ToList();

                return results;


            }
            catch (Exception)
            {
                throw;
            }
        }
        public string GetDepartmentOptionName(long personId)
        {
            var optionName = "";
            try
            {
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                var level=studentLevelLogic.GetModelsBy(f => f.Person_Id == personId).LastOrDefault();
                if (level != null && level.DepartmentOption != null)
                {
                    optionName = level.DepartmentOption.Name;
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return optionName;
        }
        public List<Result> GetStudentProcessedResultAll(Department department, Student student, Programme programme, SessionSemester sessionSemester)
        {
            try
            {
                if (department == null || department.Id <= 0 || student == null || student.Id <= 0 || programme == null || programme.Id <= 0)
                {
                    throw new Exception("One or more criteria to get Result not set! Please check your input criteria selection and try again.");
                }

                List<Result> results = (from sr in repository.GetBy<VW_STUDENT_RESULT_2>(p => p.Person_Id == student.Id && p.Programme_Id == programme.Id && p.Department_Id == department.Id &&
                                            p.Activated != false && p.Session_Semester_Id <= sessionSemester.Id && p.Special_Case != "ABSENT" && p.Special_Case != "SICK")
                                        select new Result
                                        {
                                            StudentId = sr.Person_Id,
                                            Sex = sr.Sex_Name,
                                            Name = sr.Name,
                                            MatricNumber = sr.Matric_Number,
                                            CourseId = sr.Course_Id,
                                            CourseCode = sr.Course_Code,
                                            CourseName = sr.Course_Name,
                                            CourseUnit = sr.Course_Unit,
                                            FacultyName = sr.Faculty_Name,
                                            DepartmentName = sr.Department_Name,
                                            ProgrammeId = sr.Programme_Id,
                                            TestScore = sr.Test_Score,
                                            ExamScore = sr.Exam_Score,
                                            Score = sr.Total_Score,
                                            Grade = sr.Grade,
                                            GradePoint = sr.Grade_Point,
                                            Email = sr.Email,
                                            Address = sr.Contact_Address,
                                            MobilePhone = sr.Mobile_Phone,
                                            PassportUrl = sr.Image_File_Url,
                                            GPCU = sr.Grade_Point * sr.Course_Unit,
                                            TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                            Student_Type_Id = sr.Student_Type_Id,
                                            SessionName = sr.Session_Name,
                                            Semestername = sr.Semester_Name,
                                            LevelName = sr.Level_Name,
                                            WGP = sr.WGP,
                                            SpecialCase = sr.Special_Case,
                                            Activated = sr.Activated,
                                            Reason = sr.Reason,
                                            RejectCategory = sr.Reject_Category,
                                            firstname_middle = sr.Othernames,
                                            ProgrammeName = sr.Programme_Name,
                                            Surname = sr.Last_Name,
                                            Firstname = sr.First_Name,
                                            Othername = sr.Other_Name,
                                            TotalScore = sr.Total_Score,
                                            SessionSemesterId = sr.Session_Semester_Id,
                                            SessionSemesterSequenceNumber = sr.Sequence_Number,
                                            SessionId = sr.Session_Id
                                        }).ToList();

                return results;


            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<Result> GetStudentProcessedResultUploaded(Department department, Student student, Programme programme)
        {
            try
            {
                if (department == null || department.Id <= 0 || student == null || student.Id <= 0 || programme == null || programme.Id <= 0)
                {
                    throw new Exception("One or more criteria to get Result not set! Please check your input criteria selection and try again.");
                }

                List<Result> results = (from sr in repository.GetBy<VW_STUDENT_RESULT_2>(p => p.Person_Id == student.Id && p.Programme_Id == programme.Id && p.Department_Id == department.Id &&
                                            p.Grade_Point != null)
                                        select new Result
                                        {
                                            StudentId = sr.Person_Id,
                                            Sex = sr.Sex_Name,
                                            Name = sr.Name,
                                            MatricNumber = sr.Matric_Number,
                                            CourseId = sr.Course_Id,
                                            CourseCode = sr.Course_Code,
                                            CourseName = sr.Course_Name,
                                            CourseUnit = sr.Course_Unit,
                                            FacultyName = sr.Faculty_Name,
                                            DepartmentName = sr.Department_Name,
                                            ProgrammeId = sr.Programme_Id,
                                            TestScore = sr.Test_Score,
                                            ExamScore = sr.Exam_Score,
                                            Score = sr.Total_Score,
                                            Grade = sr.Grade,
                                            GradePoint = sr.Grade_Point,
                                            Email = sr.Email,
                                            Address = sr.Contact_Address,
                                            MobilePhone = sr.Mobile_Phone,
                                            PassportUrl = sr.Image_File_Url,
                                            GPCU = sr.Grade_Point * sr.Course_Unit,
                                            TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                            Student_Type_Id = sr.Student_Type_Id,
                                            SessionName = sr.Session_Name,
                                            Semestername = sr.Semester_Name,
                                            LevelName = sr.Level_Name,
                                            WGP = sr.WGP,
                                            SpecialCase = sr.Special_Case,
                                            Activated = sr.Activated,
                                            Reason = sr.Reason,
                                            RejectCategory = sr.Reject_Category,
                                            firstname_middle = sr.Othernames,
                                            ProgrammeName = sr.Programme_Name,
                                            Surname = sr.Last_Name,
                                            Firstname = sr.First_Name,
                                            Othername = sr.Other_Name,
                                            TotalScore = sr.Total_Score,
                                            SessionSemesterId = sr.Session_Semester_Id,
                                            SessionSemesterSequenceNumber = sr.Sequence_Number,
                                            SessionId = sr.Session_Id
                                        }).ToList();

                return results;


            }
            catch (Exception)
            {
                throw;
            }
        }
        
        public List<Result> GetDeactivatedProcessedResutBy(Session session, Semester semester, Level level, Department department, Programme programme)
        {
            try
            {
                if (session == null || session.Id < 0 || level == null || level.Id <= 0 || department == null || department.Id <= 0 || programme == null || programme.Id <= 0)
                {
                    throw new Exception("One or more criteria to get Result not set! Please check your input criteria selection and try again.");
                }
                SessionLogic sessionLogic = new SessionLogic();
                Session sessions = sessionLogic.GetModelBy(p => p.Session_Id == session.Id);
                string[] sessionItems = sessions.Name.Split('/');
                string sessionNameStr = sessionItems[0];
                int sessionNameInt = Convert.ToInt32(sessionNameStr);


                List<Result> results = (from sr in repository.GetBy<VW_STUDENT_RESULT_OLD_GRADING_SYSTEM>(p => p.Programme_Id == programme.Id && p.Session_Id == session.Id && p.Level_Id == level.Id && p.Department_Id == department.Id && p.Semester_Id == semester.Id && p.Activated == false)
                                        select new Result
                                        {
                                            StudentId = sr.Person_Id,
                                            Sex = sr.Sex_Name,
                                            Name = sr.Name,
                                            MatricNumber = sr.Matric_Number,
                                            CourseId = sr.Course_Id,
                                            CourseCode = sr.Course_Code,
                                            CourseName = sr.Course_Name,
                                            CourseUnit = sr.Course_Unit,
                                            FacultyName = sr.Faculty_Name,
                                            TestScore = sr.Test_Score,
                                            ExamScore = sr.Exam_Score,
                                            Score = sr.Total_Score,
                                            Grade = sr.Grade,
                                            GradePoint = sr.Grade_Point,
                                            Email = sr.Email,
                                            SpecialCase = sr.Special_Case,
                                            Address = sr.Contact_Address,
                                            MobilePhone = sr.Mobile_Phone,
                                            PassportUrl = sr.Image_File_Url,
                                            GPCU = sr.Grade_Point * sr.Course_Unit,
                                            TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                            Student_Type_Id = sr.Student_Type_Id,
                                            SessionName = sr.Session_Name,
                                            Semestername = sr.Semester_Name,
                                            LevelName = sr.Level_Name,
                                            WGP = sr.WGP,
                                            Activated = sr.Activated,
                                            Reason = sr.Reason,
                                            RejectCategory = sr.Reject_Category,
                                            firstname_middle = sr.Othernames,
                                            ProgrammeName = sr.Programme_Name,
                                            Surname = sr.Last_Name,
                                            Firstname = sr.First_Name,
                                            Othername = sr.Other_Name,
                                            TotalScore = sr.Total_Score,
                                            SessionSemesterId = sr.Session_Semester_Id,
                                            SessionSemesterSequenceNumber = sr.Sequence_Number,
                                        }).ToList();

                return results;



            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<Result> GetDeactivatedStudentProcessedResultBy(Session session, Level level, Department department, Student student, Semester semester, Programme programme)
        {
            try
            {
                if (session == null || session.Id < 0 || level == null || level.Id <= 0 || department == null || department.Id <= 0 || student == null || student.Id <= 0 || semester == null || semester.Id <= 0 || programme == null || programme.Id <= 0)
                {
                    throw new Exception("One or more criteria to get Result not set! Please check your input criteria selection and try again.");
                }
                SessionLogic sessionLogic = new SessionLogic();
                SESSION sessions = sessionLogic.GetEntityBy(p => p.Session_Id == session.Id);
                string[] sessionItems = sessions.Session_Name.Split('/');
                string sessionNameStr = sessionItems[0];
                int sessionNameInt = Convert.ToInt32(sessionNameStr);

                List<Result> results = (from sr in repository.GetBy<VW_STUDENT_RESULT_2>(p => p.Person_Id == student.Id && p.Programme_Id == programme.Id && p.Session_Id == session.Id && p.Level_Id == level.Id && p.Department_Id == department.Id && p.Semester_Id == semester.Id && p.Activated == false)
                                        select new Result
                                        {
                                            StudentId = sr.Person_Id,
                                            Sex = sr.Sex_Name,
                                            Name = sr.Name,
                                            MatricNumber = sr.Matric_Number,
                                            CourseId = sr.Course_Id,
                                            CourseCode = sr.Course_Code,
                                            CourseName = sr.Course_Name,
                                            CourseUnit = sr.Course_Unit,
                                            FacultyName = sr.Faculty_Name,
                                            DepartmentName = sr.Department_Name,
                                            ProgrammeId = sr.Programme_Id,
                                            TestScore = sr.Test_Score,
                                            ExamScore = sr.Exam_Score,
                                            Score = sr.Total_Score,
                                            Grade = sr.Grade,
                                            GradePoint = sr.Grade_Point,
                                            Email = sr.Email,
                                            Address = sr.Contact_Address,
                                            MobilePhone = sr.Mobile_Phone,
                                            PassportUrl = sr.Image_File_Url,
                                            GPCU = sr.Grade_Point * sr.Course_Unit,
                                            TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                            Student_Type_Id = sr.Student_Type_Id,
                                            SessionName = sr.Session_Name,
                                            Semestername = sr.Semester_Name,
                                            LevelName = sr.Level_Name,
                                            WGP = sr.WGP,
                                            SpecialCase = sr.Special_Case,
                                            Activated = sr.Activated,
                                            Reason = sr.Reason,
                                            RejectCategory = sr.Reject_Category,
                                            firstname_middle = sr.Othernames,
                                            ProgrammeName = sr.Programme_Name,
                                            Surname = sr.Last_Name,
                                            Firstname = sr.First_Name,
                                            Othername = sr.Other_Name,
                                            TotalScore = sr.Total_Score,
                                            SessionSemesterId = sr.Session_Semester_Id,
                                            SessionSemesterSequenceNumber = sr.Sequence_Number,
                                        }).ToList();

                return results;

            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<Result> GetDeactivatedStudentProcessedResultAll(Department department, Student student, Programme programme)
        {
            try
            {
                if (department == null || department.Id <= 0 || student == null || student.Id <= 0 || programme == null || programme.Id <= 0)
                {
                    throw new Exception("One or more criteria to get Result not set! Please check your input criteria selection and try again.");
                }

                List<Result> results = (from sr in repository.GetBy<VW_STUDENT_RESULT_2>(p => p.Person_Id == student.Id && p.Programme_Id == programme.Id && p.Department_Id == department.Id
                                        && p.Activated == false && p.Special_Case != "ABSENT" && p.Special_Case != "SICK")
                                        select new Result
                                        {
                                            StudentId = sr.Person_Id,
                                            Sex = sr.Sex_Name,
                                            Name = sr.Name,
                                            MatricNumber = sr.Matric_Number,
                                            CourseId = sr.Course_Id,
                                            CourseCode = sr.Course_Code,
                                            CourseName = sr.Course_Name,
                                            CourseUnit = sr.Course_Unit,
                                            FacultyName = sr.Faculty_Name,
                                            DepartmentName = sr.Department_Name,
                                            ProgrammeId = sr.Programme_Id,
                                            TestScore = sr.Test_Score,
                                            ExamScore = sr.Exam_Score,
                                            Score = sr.Total_Score,
                                            Grade = sr.Grade,
                                            GradePoint = sr.Grade_Point,
                                            Email = sr.Email,
                                            Address = sr.Contact_Address,
                                            MobilePhone = sr.Mobile_Phone,
                                            PassportUrl = sr.Image_File_Url,
                                            GPCU = sr.Grade_Point * sr.Course_Unit,
                                            TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                            Student_Type_Id = sr.Student_Type_Id,
                                            SessionName = sr.Session_Name,
                                            Semestername = sr.Semester_Name,
                                            LevelName = sr.Level_Name,
                                            WGP = sr.WGP,
                                            SpecialCase = sr.Special_Case,
                                            Activated = sr.Activated,
                                            Reason = sr.Reason,
                                            RejectCategory = sr.Reject_Category,
                                            firstname_middle = sr.Othernames,
                                            ProgrammeName = sr.Programme_Name,
                                            Surname = sr.Last_Name,
                                            Firstname = sr.First_Name,
                                            Othername = sr.Other_Name,
                                            TotalScore = sr.Total_Score,
                                            SessionSemesterId = sr.Session_Semester_Id,
                                            SessionSemesterSequenceNumber = sr.Sequence_Number,
                                        }).ToList();

                return results;

            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<Result> GetStudentResultBy(SessionSemester sessionSemester, Level level, Programme programme, Department department, Student student)
        {
            try
            {
                if (sessionSemester == null || sessionSemester.Id <= 0 || level == null || level.Id <= 0 || programme == null || programme.Id <= 0 || department == null || department.Id <= 0 || student == null || student.Id <= 0)
                {
                    throw new Exception("One or more criteria to get Mater Result Sheet not set! Please check your input criteria selection and try again.");
                }

                SessionSemesterLogic sessionSemesterLogic = new Business.SessionSemesterLogic();
                SessionSemester ss = sessionSemesterLogic.GetBy(sessionSemester.Id);
                SessionLogic sessionLogic = new SessionLogic();
                Session sessions = sessionLogic.GetModelBy(p => p.Session_Id == ss.Session.Id);
                string[] sessionItems = ss.Session.Name.Split('/');
                string sessionNameStr = sessionItems[0];
                int sessionNameInt = Convert.ToInt32(sessionNameStr);

                string currentSessionSuffix = sessionNameStr.Substring(2, 2);
                string yearTwoSessionSuffix = Convert.ToString((Convert.ToInt32(currentSessionSuffix) - 1));

                currentSessionSuffix = "/" + currentSessionSuffix + "/";
                yearTwoSessionSuffix = "/" + yearTwoSessionSuffix + "/";

                string identifier = null;
                string departmentCode = GetDepartmentCode(department.Id);
                string levels = GetLevelName(level.Id);
                string semesterCode = GetSemesterCodeBy(ss.Semester.Id);
                string sessionCode = GetSessionCodeBy(ss.Session.Name);
                identifier = departmentCode + levels + semesterCode + sessionCode;

                List<Result> results = (from sr in repository.GetBy<VW_STUDENT_RESULT_2>(x => x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id && 
                    x.Programme_Id == programme.Id && x.Department_Id == department.Id && x.Person_Id == student.Id)
                                        select new Result
                                        {
                                            StudentId = sr.Person_Id,
                                            Sex = sr.Sex_Name,
                                            Name = sr.Name,
                                            MatricNumber = sr.Matric_Number,
                                            CourseId = sr.Course_Id,
                                            CourseCode = sr.Course_Code,
                                            CourseName = sr.Course_Name,
                                            CourseUnit = sr.Course_Unit,
                                            FacultyName = sr.Faculty_Name,
                                            ProgrammeName = sr.Programme_Name,
                                            DepartmentName = sr.Department_Name,
                                            TestScore = sr.Test_Score,
                                            ExamScore = sr.Exam_Score,
                                            Score = sr.Total_Score,
                                            Grade = sr.Grade,
                                            GradePoint = sr.Grade_Point,
                                            LevelName = sr.Level_Name,
                                            SessionName = sr.Session_Name,
                                            Semestername = sr.Semester_Name,
                                            Email = sr.Email,
                                            Address = sr.Contact_Address,
                                            MobilePhone = sr.Mobile_Phone,
                                            PassportUrl = sr.Image_File_Url,

                                            GPCU = sr.Grade_Point * sr.Course_Unit,
                                            TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                            SpecialCase = sr.Special_Case
                                        }).ToList();

                List<string> carryOverCourses = new List<string>();
                if (level.Id == (int)Levels.NDII || level.Id == (int)Levels.HNDII)
                {
                    carryOverCourses = GetSecondYearCarryOverCourses(ss, level, programme, department, student);
                }
                else
                {
                    carryOverCourses = GetFirstYearCarryOverCourses(ss, level, programme, department, student);
                }

                decimal CGPA = 0M;
                if (student.MatricNumber.Contains(currentSessionSuffix) || student.MatricNumber.Contains(yearTwoSessionSuffix))
                    CGPA = getCGPA(student.Id, level.Id, department.Id, programme.Id, ss.Semester.Id, ss.Session.Id);
                else
                {
                    Result allSemester = GetAllSemesterResultInfo(programme, department, student, ss);
                    allSemester.TotalSemesterCourseUnit = allSemester.TotalSemesterCourseUnit ?? 0;
                    allSemester.GPCU = allSemester.GPCU ?? 0;

                    CGPA = Decimal.Round((decimal)(allSemester.GPCU / allSemester.TotalSemesterCourseUnit), 2);
                }
                //decimal CGPA = getCGPA(student.Id, level.Id, department.Id, programme.Id, ss.Semester.Id, ss.Session.Id);
                // decimal? CGPA = GetOverallCGPAForLevel(student.Id, programme, department);

                //Ensure all passed carried over course code are removed from the carryoverCourse List
                if (carryOverCourses.Count > 0)
                {
                    List<string> PassedCarriedOverCourseCode = new List<string>();
                    
                    var AllpassedCourseCode=GetStudentAllPassedCourseCode(department, student, programme);
                    for(int i = 0; i < carryOverCourses.Count; i++)
                    {
                        var code = carryOverCourses[i];
                        if (AllpassedCourseCode.Contains(code))
                        {

                            PassedCarriedOverCourseCode.Add(code);
                        }
                    }
                    foreach (var item in PassedCarriedOverCourseCode) carryOverCourses.Remove(item);
                }

                string remark = GetGraduationStatus(CGPA, carryOverCourses);

                for (int i = 0; i < results.Count; i++)
                {
                    CheckForSpecialCase(results[i], Convert.ToInt32(results[i].TotalSemesterCourseUnit), ss.Semester.Id);

                    results[i].CGPA = CGPA;
                    results[i].Identifier = identifier;
                    results[i].Remark = remark;
                }

                return results;

            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<Result> GetStudentResultByCourse(SessionSemester sessionSemester, Level level, Programme programme, Department department, Course course)
        {
            try
            {
                if (sessionSemester == null || sessionSemester.Id <= 0 || level == null || level.Id <= 0 || programme == null || programme.Id <= 0 || department == null || department.Id <= 0 || course == null || course.Id <= 0)
                {
                    throw new Exception("One or more criteria to get Mater Result Sheet not set! Please check your input criteria selection and try again.");
                }

                SessionSemesterLogic sessionSemesterLogic = new Business.SessionSemesterLogic();
                SessionSemester ss = sessionSemesterLogic.GetBy(sessionSemester.Id);

                SessionLogic sessionLogic = new SessionLogic();
                Session sessions = sessionLogic.GetModelBy(p => p.Session_Id == ss.Session.Id);
                string[] sessionItems = ss.Session.Name.Split('/');
                string sessionNameStr = sessionItems[0];
                int sessionNameInt = Convert.ToInt32(sessionNameStr);

                List<Result> results = (from sr in repository.GetBy<VW_STUDENT_RESULT_2>(x => x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id && x.Level_Id == level.Id && x.Programme_Id == programme.Id && x.Department_Id == department.Id && x.Course_Id == course.Id)
                                        select new Result
                                        {
                                            StudentId = sr.Person_Id,
                                            Sex = sr.Sex_Name,
                                            Name = sr.Name,
                                            MatricNumber = sr.Matric_Number,
                                            CourseId = sr.Course_Id,
                                            CourseCode = sr.Course_Code,
                                            CourseName = sr.Course_Name,
                                            CourseUnit = sr.Course_Unit,
                                            FacultyName = sr.Faculty_Name,
                                            TestScore = sr.Test_Score,
                                            ExamScore = sr.Exam_Score,
                                            Score = sr.Total_Score,
                                            Grade = sr.Grade,
                                            GradePoint = sr.Grade_Point,

                                            GPCU = sr.Grade_Point * sr.Course_Unit,
                                            TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                        }).ToList();

                return results;




            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<Result> GetMaterSheetDetailsBy(SessionSemester sessionSemester, Level level, Programme programme, Department department)
        {
            try
            {
                if (sessionSemester == null || sessionSemester.Id <= 0 || level == null || level.Id <= 0 || programme == null || programme.Id <= 0 || department == null || department.Id <= 0)
                {
                    throw new Exception("One or more criteria to get Mater Result Sheet not set! Please check your input criteria selection and try again.");
                }

                SessionSemesterLogic sessionSemesterLogic = new Business.SessionSemesterLogic();
                SessionSemester ss = sessionSemesterLogic.GetBy(sessionSemester.Id);
                SessionLogic sessionLogic = new SessionLogic();
                Session sessions = sessionLogic.GetModelBy(p => p.Session_Id == ss.Session.Id);
                string[] sessionItems = ss.Session.Name.Split('/');
                string sessionNameStr = sessionItems[0];
                int sessionNameInt = Convert.ToInt32(sessionNameStr);
                List<Result> results = null;

                string identifier = null;
                string departmentCode = GetDepartmentCode(department.Id);
                string levels = GetLevelName(level.Id);
                string semesterCode = GetSemesterCodeBy(ss.Semester.Id);
                string sessionCode = GetSessionCodeBy(ss.Session.Name);
                identifier = departmentCode + levels + semesterCode + sessionCode;


                results = (from sr in repository.GetBy<VW_STUDENT_RESULT_2>(x => x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id && x.Level_Id == level.Id && x.Programme_Id == programme.Id && x.Department_Id == department.Id && (x.Activated == true || x.Activated == null))
                           select new Result
                           {
                               StudentId = sr.Person_Id,
                               Sex = sr.Sex_Name,
                               Name = sr.Name,
                               MatricNumber = sr.Matric_Number,
                               CourseId = sr.Course_Id,
                               CourseCode = sr.Course_Code,
                               CourseName = sr.Course_Name,
                               CourseUnit = sr.Course_Unit,
                               SpecialCase = sr.Special_Case,
                               TestScore = sr.Test_Score,
                               ExamScore = sr.Exam_Score,
                               Score = sr.Total_Score,
                               Grade = sr.Grade,
                               GradePoint = sr.Grade_Point,
                               DepartmentName = sr.Department_Name,
                               ProgrammeName = sr.Programme_Name,
                               LevelName = sr.Level_Name,
                               Semestername = sr.Semester_Name,
                               GPCU = sr.Grade_Point * sr.Course_Unit,
                               TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                           }).ToList();

                sessionSemester = sessionSemesterLogic.GetModelBy(p => p.Session_Semester_Id == sessionSemester.Id);
                List<Result> masterSheetResult = new List<Result>();
                for (int i = 0; i < results.Count; i++)
                {
                    Result resultItem = results[i];

                    resultItem.Identifier = identifier;
                    Result result = ViewProcessedStudentResult(resultItem.StudentId, sessionSemester, level, programme, department);
                    masterSheetResult.Add(result);
                }

                for (int i = 0; i < masterSheetResult.Count; i++)
                {
                    Result result = masterSheetResult[i];

                    List<Result> studentResults = results.Where(p => p.StudentId == result.StudentId).ToList();
                    for (int j = 0; j < studentResults.Count; j++)
                    {
                        Result resultItem = studentResults[j];

                        resultItem.Identifier = identifier;
                        resultItem.CGPA = result.CGPA;
                        resultItem.Remark = result.Remark;
                        resultItem.GPA = result.GPA;
                    }
                }

                for (int i = 0; i < results.Count; i++)
                {
                    results[i].LevelName = levels;
                }

                return results.OrderBy(a => a.MatricNumber).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public  async Task<List<Result>> GetMaterSheetDetailsByMode(SessionSemester sessionSemester, Level level, Programme programme, Department department, CourseMode courseMode)
        {
            try
            {
                if (sessionSemester == null || sessionSemester.Id <= 0 || level == null || level.Id <= 0 || programme == null || programme.Id <= 0 || department == null || department.Id <= 0)
                {
                    throw new Exception("One or more criteria to get Mater Result Sheet not set! Please check your input criteria selection and try again.");
                }

                SessionSemesterLogic sessionSemesterLogic = new Business.SessionSemesterLogic();
                sessionSemester = sessionSemesterLogic.GetBy(sessionSemester.Id);
                

                SessionLogic sessionLogic = new SessionLogic();
                //Session sessions = sessionLogic.GetModelBy(p => p.Session_Id == sessionSemester.Session.Id);
                string[] sessionItems = sessionSemester.Session.Name.Split('/');
                string sessionNameStr = sessionItems[0];
                int sessionNameInt = Convert.ToInt32(sessionNameStr);
                List<Result> results = null;

                string currentSessionSuffix = sessionNameStr.Substring(2, 2);
                string yearTwoSessionSuffix = Convert.ToString((Convert.ToInt32(currentSessionSuffix) - 1));

                currentSessionSuffix = "/" + currentSessionSuffix + "/";
                yearTwoSessionSuffix = "/" + yearTwoSessionSuffix + "/";

                string identifier = null;
                string departmentCode = GetDepartmentCode(department.Id);
                string levels = GetLevelName(level.Id);
                string semesterCode = GetSemesterCodeBy(sessionSemester.Semester.Id);
                string sessionCode = GetSessionCodeBy(sessionSemester.Session.Name);
                identifier = departmentCode + levels + semesterCode + sessionCode;

                //StudentDefermentLogic defermentLogic = new StudentDefermentLogic();
                //List<StudentDeferementLog> deferementLogs = defermentLogic.GetAll();

                if (courseMode == null)
                {
                    if (sessionSemester.Session.Id == (int)Sessions._20152016)
                    {
                         results = (from sr in await  repository.GetByAsync<VW_STUDENT_RESULT_2>(x => x.Session_Id == sessionSemester.Session.Id && x.Semester_Id == sessionSemester.Semester.Id && x.Level_Id == level.Id && x.Programme_Id == programme.Id && x.Department_Id == department.Id && (x.Activated == true || x.Activated == null))
                                   select new Result
                                   {
                                       StudentId = sr.Person_Id,
                                       Sex = sr.Sex_Name,
                                       Name = sr.Name,
                                       MatricNumber = sr.Matric_Number,
                                       CourseId = sr.Course_Id,
                                       CourseCode = sr.Course_Code,
                                       CourseName = sr.Course_Name,
                                       CourseUnit = sr.Course_Unit,
                                       SpecialCase = sr.Special_Case,
                                       TestScore = sr.Test_Score,
                                       ExamScore = sr.Exam_Score,
                                       Score = sr.Total_Score,
                                       Grade = sr.Grade,
                                       GradePoint = sr.Grade_Point,
                                       DepartmentName = sr.Department_Name,
                                       ProgrammeName = sr.Programme_Name,
                                       LevelName = sr.Level_Name,
                                       Semestername = sr.Semester_Name,
                                       GPCU = sr.Grade_Point * sr.Course_Unit,
                                       TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                       SessionName = sr.Session_Name,
                                       CourseModeId = sr.Course_Mode_Id
                                   }).ToList();

                    }
                    else
                    {
                        if (level.Id == (int)Levels.NDI || level.Id == (int)Levels.HNDI)
                        {
                            results = (from sr in await repository.GetByAsync<VW_STUDENT_RESULT_2>(x => x.Session_Id == sessionSemester.Session.Id && x.Semester_Id == sessionSemester.Semester.Id && x.Level_Id == level.Id && x.Programme_Id == programme.Id && x.Department_Id == department.Id && (x.Activated == true || x.Activated == null) && x.Course_Mode_Id == (int)CourseModes.FirstAttempt)
                                       select new Result
                                       {
                                           StudentId = sr.Person_Id,
                                           Sex = sr.Sex_Name,
                                           Name = sr.Name,
                                           MatricNumber = sr.Matric_Number,
                                           CourseId = sr.Course_Id,
                                           CourseCode = sr.Course_Code,
                                           CourseName = sr.Course_Name,
                                           CourseUnit = sr.Course_Unit,
                                           SpecialCase = sr.Special_Case,
                                           TestScore = sr.Test_Score,
                                           ExamScore = sr.Exam_Score,
                                           Score = sr.Total_Score,
                                           Grade = sr.Grade,
                                           GradePoint = sr.Grade_Point,
                                           DepartmentName = sr.Department_Name,
                                           ProgrammeName = sr.Programme_Name,
                                           LevelName = sr.Level_Name,
                                           Semestername = sr.Semester_Name,
                                           GPCU = sr.Grade_Point * sr.Course_Unit,
                                           TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                           SessionName = sr.Session_Name,
                                           CourseModeId = sr.Course_Mode_Id
                                       }).ToList();

                            List<Result> resultList = new List<Result>();

                            for (int i = 0; i < results.Count; i++)
                            {
                                if (results[i].MatricNumber.Contains(currentSessionSuffix))
                                {
                                    resultList.Add(results[i]);
                                }
                                else
                                {
                                    //if (deferementLogs.Where(s => s.Student.MatricNumber == results[i].MatricNumber) != null)
                                    //{
                                    //    resultList.Add(results[i]);
                                    //}
                                    //Do Nothing
                                }
                            }

                            results = new List<Result>();
                            results = resultList;
                        }
                        else
                        {
                            results = (from sr in await repository.GetByAsync<VW_STUDENT_RESULT_2>(x => x.Session_Id == sessionSemester.Session.Id && x.Semester_Id == sessionSemester.Semester.Id && 
                                       x.Programme_Id == programme.Id && x.Department_Id == department.Id && (x.Activated == true || x.Activated == null) && 
                                       (x.Level_Id == level.Id || x.Course_Mode_Id == (int)CourseModes.CarryOver))
                                       select new Result
                                       {
                                           StudentId = sr.Person_Id,
                                           Sex = sr.Sex_Name,
                                           Name = sr.Name,
                                           MatricNumber = sr.Matric_Number,
                                           CourseId = sr.Course_Id,
                                           CourseCode = sr.Course_Code,
                                           CourseName = sr.Course_Name,
                                           CourseUnit = sr.Course_Unit,
                                           SpecialCase = sr.Special_Case,
                                           TestScore = sr.Test_Score,
                                           ExamScore = sr.Exam_Score,
                                           Score = sr.Total_Score,
                                           Grade = sr.Grade,
                                           GradePoint = sr.Grade_Point,
                                           DepartmentName = sr.Department_Name,
                                           ProgrammeName = sr.Programme_Name,
                                           LevelName = sr.Level_Name,
                                           Semestername = sr.Semester_Name,
                                           GPCU = sr.Grade_Point * sr.Course_Unit,
                                           TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                           SessionName = sr.Session_Name,
                                           CourseModeId = sr.Course_Mode_Id
                                       }).ToList();

                            List<Result> resultList = new List<Result>();

                            for (int i = 0; i < results.Count; i++)
                            {
                                if (results[i].MatricNumber.Contains(yearTwoSessionSuffix))
                                {
                                    resultList.Add(results[i]);
                                }
                                else
                                {
                                    //if (deferementLogs.Where(s => s.Student.MatricNumber == results[i].MatricNumber) != null)
                                    //{
                                    //    resultList.Add(results[i]);
                                    //}
                                    //Do Nothing
                                }
                            }

                            results = new List<Result>();
                            results = resultList;
                        }
                    }
                    
                    List<long> students = results.Select(s => s.StudentId).Distinct().ToList();

                    List<Result> masterSheetResult = new List<Result>();

                    for (int i = 0; i < students.Count; i++)
                    {
                        long studentId = students[i];
                        if(studentId== 112385)
                        {
                            
                        }
                        TotalUnitsOmitted = 0;
                        Result result = ViewProcessedStudentResult(studentId, sessionSemester, level, programme, department);
                        result.UnitOutstanding = TotalUnitsOmitted;
                        AssignAndAddToMasterSheet(identifier, result, results.Where(s => s.StudentId == studentId).ToList(), masterSheetResult);
                        
                    }

                    StudentExtraYearLogic extraYearLogic = new StudentExtraYearLogic();
                    List<long> extraYear = extraYearLogic.GetEntitiesBy(e => e.Session_Id == sessionSemester.Session.Id).Select(e => e.Person_Id).ToList();

                    List<Result> classResult = (from sr in await repository.GetByAsync<VW_STUDENT_RESULT_2>(x => x.Programme_Id == programme.Id && x.Department_Id == department.Id && 
                                                    (x.Activated == true || x.Activated == null))
                                                  select new Result
                                                  {
                                                      StudentId = sr.Person_Id,
                                                      Sex = sr.Sex_Name,
                                                      Name = sr.Name,
                                                      MatricNumber = sr.Matric_Number,
                                                      CourseId = sr.Course_Id,
                                                      CourseCode = sr.Course_Code,
                                                      CourseName = sr.Course_Name,
                                                      CourseUnit = sr.Course_Unit,
                                                      SpecialCase = sr.Special_Case,
                                                      TestScore = sr.Test_Score,
                                                      ExamScore = sr.Exam_Score,
                                                      Score = sr.Total_Score,
                                                      Grade = sr.Grade,
                                                      GradePoint = sr.Grade_Point,
                                                      DepartmentName = sr.Department_Name,
                                                      ProgrammeName = sr.Programme_Name,
                                                      LevelName = sr.Level_Name,
                                                      Semestername = sr.Semester_Name,
                                                      GPCU = sr.Grade_Point * sr.Course_Unit,
                                                      TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                                      SessionName = sr.Session_Name,
                                                      CourseModeId = sr.Course_Mode_Id
                                                  }).ToList();

            

                    for (int i = 0; i < results.Count; i++)
                    {
                        results[i].LevelName = levels;
                        results[i].CourseMode = "Extensive";
                    }
                }
                else if (courseMode.Id == (int)CourseModes.FirstAttempt)
                {

                    results = (from sr in await repository.GetByAsync<VW_STUDENT_RESULT_2>(x => x.Session_Id == sessionSemester.Session.Id && x.Semester_Id == sessionSemester.Semester.Id && x.Level_Id == level.Id && x.Programme_Id == programme.Id && x.Department_Id == department.Id && (x.Activated == true || x.Activated == null) &&  x.Course_Mode_Id == courseMode.Id)
                               select new Result
                               {
                                   StudentId = sr.Person_Id,
                                   Sex = sr.Sex_Name,
                                   Name = sr.Name,
                                   MatricNumber = sr.Matric_Number,
                                   CourseId = sr.Course_Id,
                                   CourseCode = sr.Course_Code,
                                   CourseName = sr.Course_Name,
                                   CourseUnit = sr.Course_Unit,
                                   SpecialCase = sr.Special_Case,
                                   TestScore = sr.Test_Score,
                                   ExamScore = sr.Exam_Score,
                                   Score = sr.Total_Score,
                                   Grade = sr.Grade,
                                   GradePoint = sr.Grade_Point,
                                   DepartmentName = sr.Department_Name,
                                   ProgrammeName = sr.Programme_Name,
                                   LevelName = sr.Level_Name,
                                   Semestername = sr.Semester_Name,
                                   GPCU = sr.Grade_Point * sr.Course_Unit,
                                   TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                               }).ToList();

                    List<Result> resultList = new List<Result>();

                    if (level.Id == (int)Levels.HNDI || level.Id == (int)Levels.NDI)
                    {
                        for (int i = 0; i < results.Count; i++)
                        {
                            if (results[i].MatricNumber.Contains("/17/"))
                            {
                                resultList.Add(results[i]);
                            }
                            else
                            {
                                //Do Nothing
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < results.Count; i++)
                        {
                            if (results[i].MatricNumber.Contains("/16/"))
                            {
                                resultList.Add(results[i]);
                            }
                            else
                            {
                                //Do Nothing
                            }
                        }
                    }

                    results = new List<Result>();
                    results = resultList;
                    
                    List<Result> masterSheetResult = new List<Result>();
                    for (int i = 0; i < results.Count; i++)
                    {
                        Result resultItem = results[i];
                        resultItem.Identifier = identifier;
                        Result result = ViewProcessedStudentResult(resultItem.StudentId, sessionSemester, level, programme, department);
                        masterSheetResult.Add(result);
                    }

                    for (int i = 0; i < masterSheetResult.Count; i++)
                    {
                        Result result = masterSheetResult[i];
                        List<Result> studentResults = results.Where(p => p.StudentId == result.StudentId).ToList();
                        for (int j = 0; j < studentResults.Count; j++)
                        {
                            Result resultItem = studentResults[j];
                            resultItem.Identifier = identifier;
                            resultItem.CGPA = result.CGPA;
                            resultItem.Remark = result.Remark;
                            resultItem.GPA = result.GPA;
                        }
                    }

                    for (int i = 0; i < results.Count; i++)
                    {
                        results[i].LevelName = levels;
                        results[i].CourseMode = "First Attempt";
                    }
                }
                else if (courseMode.Id == (int)CourseModes.CarryOver)
                {
                    results = (from sr in await repository.GetByAsync<VW_STUDENT_RESULT_2>(x => x.Session_Id == sessionSemester.Session.Id && x.Semester_Id == sessionSemester.Semester.Id && x.Level_Id == level.Id && x.Programme_Id == programme.Id && x.Department_Id == department.Id && (x.Activated == true || x.Activated == null) && x.Course_Mode_Id == courseMode.Id)
                               select new Result
                               {
                                   StudentId = sr.Person_Id,
                                   Sex = sr.Sex_Name,
                                   Name = sr.Name,
                                   MatricNumber = sr.Matric_Number,
                                   CourseId = sr.Course_Id,
                                   CourseCode = sr.Course_Code,
                                   CourseName = sr.Course_Name,
                                   CourseUnit = sr.Course_Unit,
                                   SpecialCase = sr.Special_Case,
                                   TestScore = sr.Test_Score,
                                   ExamScore = sr.Exam_Score,
                                   Score = sr.Total_Score,
                                   Grade = sr.Grade,
                                   GradePoint = sr.Grade_Point,
                                   DepartmentName = sr.Department_Name,
                                   ProgrammeName = sr.Programme_Name,
                                   LevelName = sr.Level_Name,
                                   Semestername = sr.Semester_Name,
                                   GPCU = sr.Grade_Point * sr.Course_Unit,
                                   TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                               }).ToList();

                    List<Result> resultList = new List<Result>();

                    for (int i = 0; i < results.Count; i++)
                    {
                        if (results[i].MatricNumber.Contains("/15/"))
                        {
                            resultList.Add(results[i]);
                        }
                        else
                        {
                            //Do Nothing
                        }
                    }

                    results = new List<Result>();
                    results = resultList;
                    
                    List<Result> masterSheetResult = new List<Result>();
                    for (int i = 0; i < results.Count; i++)
                    {
                        Result resultItem = results[i];

                        resultItem.Identifier = identifier;
                        Result result = ViewProcessedStudentResult(resultItem.StudentId, sessionSemester, level, programme, department);
                        masterSheetResult.Add(result);
                    }

                    for (int i = 0; i < masterSheetResult.Count; i++)
                    {
                        Result result = masterSheetResult[i];

                        List<Result> studentResults = results.Where(p => p.StudentId == result.StudentId).ToList();
                        for (int j = 0; j < studentResults.Count; j++)
                        {
                            Result resultItem = studentResults[j];

                            resultItem.Identifier = identifier;
                            resultItem.CGPA = result.CGPA;
                            resultItem.Remark = result.Remark;
                            resultItem.GPA = result.GPA;
                        }
                    }

                    for (int i = 0; i < results.Count; i++)
                    {
                        results[i].LevelName = levels;
                        results[i].CourseMode = "Carry Over";
                    }
                }
                else if (courseMode.Id == (int)CourseModes.ExtraYear)
                {
                    results = (from sr in await repository.GetByAsync<VW_STUDENT_RESULT_2>(x => x.Session_Id == sessionSemester.Session.Id && x.Semester_Id == sessionSemester.Semester.Id && x.Level_Id == level.Id && x.Programme_Id == programme.Id && x.Department_Id == department.Id && (x.Activated == true || x.Activated == null))
                               select new Result
                               {
                                   StudentId = sr.Person_Id,
                                   Sex = sr.Sex_Name,
                                   Name = sr.Name,
                                   MatricNumber = sr.Matric_Number,
                                   CourseId = sr.Course_Id,
                                   CourseCode = sr.Course_Code,
                                   CourseName = sr.Course_Name,
                                   CourseUnit = sr.Course_Unit,
                                   SpecialCase = sr.Special_Case,
                                   TestScore = sr.Test_Score,
                                   ExamScore = sr.Exam_Score,
                                   Score = sr.Total_Score,
                                   Grade = sr.Grade,
                                   GradePoint = sr.Grade_Point,
                                   DepartmentName = sr.Department_Name,
                                   ProgrammeName = sr.Programme_Name,
                                   LevelName = sr.Level_Name,
                                   Semestername = sr.Semester_Name,
                                   GPCU = sr.Grade_Point * sr.Course_Unit,
                                   TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                               }).ToList();

                    List<Result> resultList = new List<Result>();

                    for (int i = 0; i < results.Count; i++)
                    {
                        if (results[i].MatricNumber.Contains("/16/") || results[i].MatricNumber.Contains("/17/"))
                        {
                            //Do Nothing
                        }
                        else
                        {
                            resultList.Add(results[i]);
                        }
                    }

                    results = new List<Result>();
                    results = resultList;
                    
                    List<Result> masterSheetResult = new List<Result>();
                    for (int i = 0; i < results.Count; i++)
                    {
                        Result resultItem = results[i];

                        resultItem.Identifier = identifier;
                        if(resultItem.StudentId== 82249)
                        {
                            int k = 0;
                        }
                        Result result = ViewProcessedStudentResult(resultItem.StudentId, sessionSemester, level, programme, department);
                        masterSheetResult.Add(result);
                    }

                    for (int i = 0; i < masterSheetResult.Count; i++)
                    {
                        Result result = masterSheetResult[i];
                        List<Result> studentResults = results.Where(p => p.StudentId == result.StudentId).ToList();
                        for (int j = 0; j < studentResults.Count; j++)
                        {
                            Result resultItem = studentResults[j];
                            resultItem.Identifier = identifier;
                            resultItem.CGPA = result.CGPA;
                            resultItem.Remark = result.Remark;
                            resultItem.GPA = result.GPA;
                        }
                    }

                    for (int i = 0; i < results.Count; i++)
                    {
                        results[i].LevelName = levels;
                        results[i].CourseMode = "Extra Year";
                    }
                }

                return results.OrderBy(a => a.MatricNumber).ThenBy(a => a.CourseModeId).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CheckSpecialCaseOutstanding(List<Result> classResult, long studentId, List<Result> studentResults)
        {
            try
            {
                List<Result> specialCases = classResult.Where(r => r.StudentId == studentId && r.SpecialCase != null).ToList();
                if (specialCases == null || specialCases.Count <= 0)
                    return;

                bool cleardSpecialCase = true;
                string remark = "OUTSTANDING: ";

                for (int i = 0; i < specialCases.Count; i++)
                {
                    Result specialCase = specialCases[i];

                    Result hasPassedSpecialCase = classResult.LastOrDefault(r => r.StudentId == studentId && r.CourseId == specialCase.CourseId && 
                                                                            ((r.TestScore ?? 0) + (r.ExamScore ?? 0) >= 40));
                    if (hasPassedSpecialCase == null)
                    {
                        cleardSpecialCase = false;
                        remark += specialCase.CourseCode + " - " + specialCase.SpecialCase + " | ";
                    }
                }

                if (!cleardSpecialCase)
                {
                    studentResults.ForEach(r =>
                    {
                        r.Remark = remark;
                    });
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool CheckSpecialCase(List<Result> classResult, long studentId)
        {
            try
            {
                List<Result> specialCases = classResult.Where(r => r.StudentId == studentId && r.SpecialCase != null).ToList();
                if (specialCases == null || specialCases.Count <= 0)
                    return false;

                bool hasSpecialCase = false;

                for (int i = 0; i < specialCases.Count; i++)
                {
                    Result specialCase = specialCases[i];

                    Result hasPassedSpecialCase = classResult.LastOrDefault(r => r.StudentId == studentId && r.CourseId == specialCase.CourseId &&
                                                                            ((r.TestScore ?? 0) + (r.ExamScore ?? 0) >= 40));
                    if (hasPassedSpecialCase == null)
                    {
                        hasSpecialCase = true;
                    }
                }

                return hasSpecialCase;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<Result> GetMaterSheetDetailsByModeCapacity(SessionSemester sessionSemester, Level level, Programme programme, Department department, CourseMode courseMode)
        {
            try
            {
                if (sessionSemester == null || sessionSemester.Id <= 0 || level == null || level.Id <= 0 || programme == null || programme.Id <= 0 || department == null || department.Id <= 0)
                {
                    throw new Exception("One or more criteria to get Mater Result Sheet not set! Please check your input criteria selection and try again.");
                }

                SessionSemesterLogic sessionSemesterLogic = new Business.SessionSemesterLogic();
                SessionSemester ss = sessionSemesterLogic.GetBy(sessionSemester.Id);
                SessionLogic sessionLogic = new SessionLogic();
                Session sessions = sessionLogic.GetModelBy(p => p.Session_Id == ss.Session.Id);
                string[] sessionItems = ss.Session.Name.Split('/');
                string sessionNameStr = sessionItems[0];
                int sessionNameInt = Convert.ToInt32(sessionNameStr);
                List<Result> results = null;

                List<Result> masterSheetResult = new List<Result>();

                string currentSessionSuffix = sessionNameStr.Substring(2, 2);
                string yearTwoSessionSuffix = Convert.ToString((Convert.ToInt32(currentSessionSuffix) - 1));

                currentSessionSuffix = "/" + currentSessionSuffix + "/";
                yearTwoSessionSuffix = "/" + yearTwoSessionSuffix + "/";

                string identifier = null;
                string departmentCode = GetDepartmentCode(department.Id);
                string levels = GetLevelName(level.Id);
                string semesterCode = GetSemesterCodeBy(ss.Semester.Id);
                string sessionCode = GetSessionCodeBy(ss.Session.Name);
                identifier = departmentCode + levels + semesterCode + sessionCode;

                //StudentDefermentLogic defermentLogic = new StudentDefermentLogic();
                //List<StudentDeferementLog> deferementLogs = defermentLogic.GetAll();

                if (courseMode == null)
                {
                    if (ss.Session.Id == (int)Sessions._20152016)
                    {
                        results = (from sr in repository.GetBy<VW_STUDENT_RESULT_2>(x => x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id && x.Level_Id == level.Id && x.Programme_Id == programme.Id && x.Department_Id == department.Id && (x.Activated == true || x.Activated == null))
                                   select new Result
                                   {
                                       StudentId = sr.Person_Id,
                                       Sex = sr.Sex_Name,
                                       Name = sr.Name,
                                       MatricNumber = sr.Matric_Number,
                                       CourseId = sr.Course_Id,
                                       CourseCode = sr.Course_Code,
                                       CourseName = sr.Course_Name,
                                       CourseUnit = sr.Course_Unit,
                                       SpecialCase = sr.Special_Case,
                                       TestScore = sr.Test_Score,
                                       ExamScore = sr.Exam_Score,
                                       Score = sr.Total_Score,
                                       Grade = sr.Grade,
                                       GradePoint = sr.Grade_Point,
                                       DepartmentName = sr.Department_Name,
                                       ProgrammeName = sr.Programme_Name,
                                       LevelName = sr.Level_Name,
                                       Semestername = sr.Semester_Name,
                                       GPCU = sr.Grade_Point * sr.Course_Unit,
                                       TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                       SessionName = sr.Session_Name,
                                       CourseModeId = sr.Course_Mode_Id
                                   }).ToList();

                    }
                    else
                    {
                        if (level.Id == (int)Levels.NDI || level.Id == (int)Levels.HNDI)
                        {
                            results = (from sr in repository.GetBy<VW_STUDENT_RESULT_2>(x => x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id && x.Level_Id == level.Id && x.Programme_Id == programme.Id && x.Department_Id == department.Id && (x.Activated == true || x.Activated == null) && x.Course_Mode_Id == (int)CourseModes.FirstAttempt)
                                       select new Result
                                       {
                                           StudentId = sr.Person_Id,
                                           Sex = sr.Sex_Name,
                                           Name = sr.Name,
                                           MatricNumber = sr.Matric_Number,
                                           CourseId = sr.Course_Id,
                                           CourseCode = sr.Course_Code,
                                           CourseName = sr.Course_Name,
                                           CourseUnit = sr.Course_Unit,
                                           SpecialCase = sr.Special_Case,
                                           TestScore = sr.Test_Score,
                                           ExamScore = sr.Exam_Score,
                                           Score = sr.Total_Score,
                                           Grade = sr.Grade,
                                           GradePoint = sr.Grade_Point,
                                           DepartmentName = sr.Department_Name,
                                           ProgrammeName = sr.Programme_Name,
                                           LevelName = sr.Level_Name,
                                           Semestername = sr.Semester_Name,
                                           GPCU = sr.Grade_Point * sr.Course_Unit,
                                           TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                           SessionName = sr.Session_Name,
                                           CourseModeId = sr.Course_Mode_Id
                                       }).ToList();

                            List<Result> resultList = new List<Result>();

                            for (int i = 0; i < results.Count; i++)
                            {
                                if (results[i].MatricNumber.Contains(currentSessionSuffix))
                                {
                                    resultList.Add(results[i]);
                                }
                                else
                                {
                                    //if (deferementLogs.Where(s => s.Student.MatricNumber == results[i].MatricNumber) != null)
                                    //{
                                    //    resultList.Add(results[i]);
                                    //}
                                    //Do Nothing
                                }
                            }

                            results = new List<Result>();
                            results = resultList;
                        }
                        else
                        {
                            results = (from sr in repository.GetBy<VW_STUDENT_RESULT_2>(x => x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id && x.Programme_Id == programme.Id && x.Department_Id == department.Id && (x.Activated == true || x.Activated == null))
                                       select new Result
                                       {
                                           StudentId = sr.Person_Id,
                                           Sex = sr.Sex_Name,
                                           Name = sr.Name,
                                           MatricNumber = sr.Matric_Number,
                                           CourseId = sr.Course_Id,
                                           CourseCode = sr.Course_Code,
                                           CourseName = sr.Course_Name,
                                           CourseUnit = sr.Course_Unit,
                                           SpecialCase = sr.Special_Case,
                                           TestScore = sr.Test_Score,
                                           ExamScore = sr.Exam_Score,
                                           Score = sr.Total_Score,
                                           Grade = sr.Grade,
                                           GradePoint = sr.Grade_Point,
                                           DepartmentName = sr.Department_Name,
                                           ProgrammeName = sr.Programme_Name,
                                           LevelName = sr.Level_Name,
                                           Semestername = sr.Semester_Name,
                                           GPCU = sr.Grade_Point * sr.Course_Unit,
                                           TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                           SessionName = sr.Session_Name,
                                           CourseModeId = sr.Course_Mode_Id
                                       }).ToList();

                            List<Result> resultList = new List<Result>();

                            for (int i = 0; i < results.Count; i++)
                            {
                                if (results[i].MatricNumber.Contains(yearTwoSessionSuffix))
                                {
                                    resultList.Add(results[i]);
                                }
                                else
                                {
                                    //if (deferementLogs.Where(s => s.Student.MatricNumber == results[i].MatricNumber) != null)
                                    //{
                                    //    resultList.Add(results[i]);
                                    //}
                                    //Do Nothing
                                }
                            }

                            results = new List<Result>();
                            results = resultList;
                        }
                    }


                    //sessionSemester = sessionSemesterLogic.GetModelBy(p => p.Session_Semester_Id == sessionSemester.Id);
                    //List<Result> masterSheetResult = new List<Result>();
                    //for (int i = 0; i < results.Count; i++)
                    //{
                    //    Result resultItem = results[i];
                    //    resultItem.Identifier = identifier;
                    //    TotalUnitsOmitted = 0;
                    //    Result result = ViewProcessedStudentResult(resultItem.StudentId, sessionSemester, level, programme, department);
                    //    result.UnitOutstanding = TotalUnitsOmitted;

                    //    masterSheetResult.Add(result);
                    //   // masterSheetResult.Add(resultItem);
                    //}

                    sessionSemester = sessionSemesterLogic.GetModelBy(p => p.Session_Semester_Id == sessionSemester.Id);

                    List<long> students = new List<long>();

                    DepartmentCapacityLogic capacityLogic = new DepartmentCapacityLogic();
                    DepartmentCapacity capacity = capacityLogic.GetModelsBy(c => c.Programme_Id == programme.Id && c.Department_Id == department.Id && c.Session_Id == ss.Session.Id && c.Activated).LastOrDefault();
                    if (results.Count > 0)
                    {
                        if (capacity != null)
                        {
                            students = results.Select(s => s.StudentId).Distinct().Take(capacity.Capacity).ToList();
                        }
                        else
                        {
                            students = results.Select(s => s.StudentId).Distinct().ToList();
                        }
                    }
                   

                    for (int i = 0; i < students.Count; i++)
                    {
                        //Result resultItem = results[i];
                        //resultItem.Identifier = identifier;
                        long studentId = students[i];
                        TotalUnitsOmitted = 0;
                        Result result = ViewProcessedStudentResult(studentId, sessionSemester, level, programme, department);
                        result.UnitOutstanding = TotalUnitsOmitted;

                        AssignAndAddToMasterSheet(identifier, result, results.Where(s => s.StudentId == studentId).ToList(), masterSheetResult);

                        //masterSheetResult.Add(result);
                    }

                    StudentExtraYearLogic extraYearLogic = new StudentExtraYearLogic();
                    List<long> extraYear = extraYearLogic.GetEntitiesBy(e => e.Session_Id == ss.Session.Id).Select(e => e.Person_Id).ToList();

                    for (int i = 0; i < masterSheetResult.Count; i++)
                    {
                        Result result = masterSheetResult[i];

                        List<Result> studentResults = masterSheetResult.Where(p => p.StudentId == result.StudentId).ToList();
                        for (int j = 0; j < studentResults.Count; j++)
                        {
                            Result resultItem = studentResults[j];

                            resultItem.Identifier = identifier;
                            resultItem.CGPA = result.CGPA;
                            resultItem.Remark = result.Remark;
                            resultItem.GPA = result.GPA;
                            resultItem.UnitOutstanding = result.UnitOutstanding;

                            resultItem.SessionId = ss.Session.Id;

                            int totalSemesterCourseUnit = 0;
                            CheckForSpecialCase(resultItem, totalSemesterCourseUnit, (int)Semesters.FirstSemester);
                            //if (totalSemesterCourseUnit > 0)
                            //{
                            //    resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                            //}
                            if (extraYear.Contains(resultItem.StudentId))
                            {
                                resultItem.Remark = "";
                            }
                        }
                    }

                    for (int i = 0; i < masterSheetResult.Count; i++)
                    {
                        masterSheetResult[i].LevelName = levels;
                        masterSheetResult[i].CourseMode = "Extensive";
                    }
                }

                return masterSheetResult.OrderBy(a => a.MatricNumber).ThenBy(a => a.CourseModeId).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void AssignAndAddToMasterSheet(string identifier, Result result, List<Result> results, List<Result> masterSheetResult)
        {
            try
            {
                if (result != null)
                {
                    for (int i = 0; i < results.Count; i++)
                    {
                        results[i].CGPA = result.CGPA;
                        //results[i].FirstSemesterGPA = result.FirstSemesterGPA;
                        results[i].GPA = result.GPA;
                        //results[i].GPCU = result.GPCU;
                        results[i].Identifier = identifier;
                        results[i].TotalSemesterCourseUnit = result.TotalSemesterCourseUnit;
                        results[i].Remark = result.Remark;
                        results[i].UnitOutstanding = result.UnitOutstanding;
                        results[i].UnitPassed = result.UnitPassed;

                        masterSheetResult.Add(results[i]);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void AssignAndAddToMasterSheetExtraYear(string identifier, Result result, List<Result> results, List<Result> masterSheetResult)
        {
            try
            {
                if (result != null)
                {
                    for (int i = 0; i < results.Count; i++)
                    {
                        results[i].CGPA = result.CGPA;
                        //results[i].FirstSemesterGPA = result.FirstSemesterGPA;
                        results[i].GPA = result.GPA;
                        //results[i].GPCU = result.GPCU;
                        results[i].Identifier = identifier;
                        results[i].TotalSemesterCourseUnit = result.TotalSemesterCourseUnit;
                        results[i].Remark = result.Remark;
                        results[i].UnitOutstanding = result.UnitOutstanding;
                        results[i].UnitPassed = result.TotalSemesterCourseUnit - result.UnitOutstanding;
                        results[i].WGP = result.WGP;

                        masterSheetResult.Add(results[i]);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<Result> GetMaterSheetDetailsByOptions(SessionSemester sessionSemester, Level level, Programme programme, Department department, DepartmentOption departmentOption)
        {
            try
            {
                if (sessionSemester == null || sessionSemester.Id <= 0 || level == null || level.Id <= 0 || programme == null || programme.Id <= 0 || department == null || department.Id <= 0 || departmentOption == null || departmentOption.Id <= 0)
                {
                    throw new Exception("One or more criteria to get Mater Result Sheet not set! Please check your input criteria selection and try again.");
                }

                SessionSemesterLogic sessionSemesterLogic = new Business.SessionSemesterLogic();
                SessionSemester ss = sessionSemesterLogic.GetBy(sessionSemester.Id);
                SessionLogic sessionLogic = new SessionLogic();
                Session sessions = sessionLogic.GetModelBy(p => p.Session_Id == ss.Session.Id);
                string[] sessionItems = ss.Session.Name.Split('/');
                string sessionNameStr = sessionItems[0];
                int sessionNameInt = Convert.ToInt32(sessionNameStr);
                List<Result> results = null;

                string identifier = null;
                string departmentCode = GetDepartmentCode(department.Id);
                string levels = GetLevelName(level.Id);
                string semesterCode = GetSemesterCodeBy(ss.Semester.Id);
                string sessionCode = GetSessionCodeBy(ss.Session.Name);
                identifier = departmentCode + levels + semesterCode + sessionCode;
                
                results = (from sr in repository.GetBy<VW_STUDENT_RESULT_WITH_OPTIONS>(x => x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id && x.Level_Id == level.Id && x.Programme_Id == programme.Id && x.Department_Id == department.Id && x.Department_Option_Id == departmentOption.Id && (x.Activated != false || x.Activated == null))
                           select new Result
                           {
                               StudentId = sr.Person_Id,
                               Sex = sr.Sex_Name,
                               Name = sr.Name,
                               MatricNumber = sr.Matric_Number,
                               CourseId = sr.Course_Id,
                               CourseCode = sr.Course_Code,
                               CourseName = sr.Course_Name,
                               CourseUnit = (int)sr.Course_Unit,
                               SpecialCase = sr.Special_Case,
                               TestScore = sr.Test_Score,
                               ExamScore = sr.Exam_Score,
                               Score = sr.Total_Score,
                               Grade = sr.Grade,
                               GradePoint = sr.Grade_Point,
                               DepartmentName = sr.Department_Name,
                               ProgrammeName = sr.Programme_Name,
                               LevelName = sr.Level_Name,
                               Semestername = sr.Semester_Name,
                               GPCU = sr.Grade_Point * sr.Course_Unit,
                               TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                           }).ToList();



                sessionSemester = sessionSemesterLogic.GetModelBy(p => p.Session_Semester_Id == sessionSemester.Id);
                //  List<Result> studentsResult = GetResultList(sessionSemester, level, programme, department).ToList(); 
                List<Result> masterSheetResult = new List<Result>();
                for (int i = 0; i < results.Count; i++)
                {
                    Result resultItem = results[i];
                    resultItem.Identifier = identifier;
                    Result result = ViewProcessedStudentResult(resultItem.StudentId, sessionSemester, level, programme, department);
                    masterSheetResult.Add(result);
                }
                //foreach (Result resultItem in results)
                //{
                //    resultItem.Identifier = identifier;
                //    Result result = ViewProcessedStudentResult(resultItem.StudentId, sessionSemester, level, programme, department);
                //    masterSheetResult.Add(result);
                //}

                for (int i = 0; i < masterSheetResult.Count; i++)
                {
                    Result result = masterSheetResult[i];
                    List<Result> studentResults = results.Where(p => p.StudentId == result.StudentId).ToList();
                    for (int j = 0; j < studentResults.Count; j++)
                    {
                        Result resultItem = studentResults[j];
                        resultItem.Identifier = identifier;
                        resultItem.CGPA = result.CGPA;
                        resultItem.Remark = result.Remark;
                        resultItem.GPA = result.GPA;
                    }
                }

                

                for (int i = 0; i < results.Count; i++)
                {
                    results[i].LevelName = levels;
                }

                return results.OrderBy(a => a.MatricNumber).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<List<Result>> GetMaterSheetDetailsByOptionsAndMode(SessionSemester sessionSemester, Level level, Programme programme, Department department, DepartmentOption departmentOption, CourseMode courseMode)
        {
            try
            {
                if (sessionSemester == null || sessionSemester.Id <= 0 || level == null || level.Id <= 0 || programme == null || programme.Id <= 0 || department == null || department.Id <= 0 || departmentOption == null || departmentOption.Id <= 0)
                {
                    throw new Exception("One or more criteria to get Mater Result Sheet not set! Please check your input criteria selection and try again.");
                }

                SessionSemesterLogic sessionSemesterLogic = new Business.SessionSemesterLogic();
                SessionSemester ss = sessionSemesterLogic.GetBy(sessionSemester.Id);
                SessionLogic sessionLogic = new SessionLogic();
                Session sessions = sessionLogic.GetModelBy(p => p.Session_Id == ss.Session.Id);
                string[] sessionItems = ss.Session.Name.Split('/');
                string sessionNameStr = sessionItems[0];
                int sessionNameInt = Convert.ToInt32(sessionNameStr);
                List<Result> results = null;

                string currentSessionSuffix = sessionNameStr.Substring(2, 2);
                string yearTwoSessionSuffix = Convert.ToString((Convert.ToInt32(currentSessionSuffix) - 1));

                currentSessionSuffix = "/" + currentSessionSuffix + "/";
                yearTwoSessionSuffix = "/" + yearTwoSessionSuffix + "/";

                string identifier = null;
                string departmentCode = GetDepartmentCode(department.Id);
                string levels = GetLevelName(level.Id);
                string semesterCode = GetSemesterCodeBy(ss.Semester.Id);
                string sessionCode = GetSessionCodeBy(ss.Session.Name);
                identifier = departmentCode + levels + semesterCode + sessionCode;


                //StudentDefermentLogic defermentLogic = new StudentDefermentLogic();
                //List<StudentDeferementLog> deferementLogs = defermentLogic.GetAll();

                DepartmentOptionLogic departmentOptionLogic = new DepartmentOptionLogic();
                DepartmentOption option = departmentOptionLogic.GetModelBy(d => d.Department_Option_Id == departmentOption.Id);

                if (courseMode == null)
                {
                    if (ss.Session.Id == (int)Sessions._20152016)
                    {
                        results = (from sr in await repository.GetByAsync<VW_STUDENT_RESULT_WITH_OPTIONS>(
                                     x =>
                                         x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id &&
                                         x.Level_Id == level.Id && x.Programme_Id == programme.Id &&
                                         x.Department_Id == department.Id &&
                                         x.Department_Option_Id == departmentOption.Id &&
                                         (x.Activated != false || x.Activated == null))
                                   select new Result
                                   {
                                       StudentId = sr.Person_Id,
                                       Sex = sr.Sex_Name,
                                       Name = sr.Name,
                                       MatricNumber = sr.Matric_Number,
                                       CourseId = sr.Course_Id,
                                       CourseCode = sr.Course_Code,
                                       CourseName = sr.Course_Name,
                                       CourseUnit = (int)sr.Course_Unit,
                                       SpecialCase = sr.Special_Case,
                                       TestScore = sr.Test_Score,
                                       ExamScore = sr.Exam_Score,
                                       Score = sr.Total_Score,
                                       Grade = sr.Grade,
                                       GradePoint = sr.Grade_Point,
                                       DepartmentName = sr.Department_Name,
                                       ProgrammeName = sr.Programme_Name,
                                       LevelName = sr.Level_Name,
                                       Semestername = sr.Semester_Name,
                                       GPCU = sr.Grade_Point * sr.Course_Unit,
                                       TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                       DepartmentOptionId = sr.Department_Option_Id,
                                       DepartmentOptionName = sr.Department_Option_Name,
                                       SessionName = sr.Session_Name,
                                       CourseModeId = sr.Course_Mode_Id
                                   }).ToList();
                    }
                    else
                    {
                        if (level.Id == (int)Levels.NDI || level.Id == (int)Levels.HNDI)
                        {
                            results =
                                (from sr in await
                                     repository.GetByAsync<VW_STUDENT_RESULT_WITH_OPTIONS>(
                                         x =>
                                             x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id &&
                                             x.Level_Id == level.Id && x.Programme_Id == programme.Id &&
                                             x.Department_Id == department.Id &&
                                             x.Department_Option_Id == departmentOption.Id &&
                                             (x.Activated != false || x.Activated == null))
                                 select new Result
                                 {
                                     StudentId = sr.Person_Id,
                                     Sex = sr.Sex_Name,
                                     Name = sr.Name,
                                     MatricNumber = sr.Matric_Number,
                                     CourseId = sr.Course_Id,
                                     CourseCode = sr.Course_Code,
                                     CourseName = sr.Course_Name,
                                     CourseUnit = (int)sr.Course_Unit,
                                     SpecialCase = sr.Special_Case,
                                     TestScore = sr.Test_Score,
                                     ExamScore = sr.Exam_Score,
                                     Score = sr.Total_Score,
                                     Grade = sr.Grade,
                                     GradePoint = sr.Grade_Point,
                                     DepartmentName = sr.Department_Name,
                                     ProgrammeName = sr.Programme_Name,
                                     LevelName = sr.Level_Name,
                                     Semestername = sr.Semester_Name,
                                     GPCU = sr.Grade_Point * sr.Course_Unit,
                                     TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                     DepartmentOptionId = sr.Department_Option_Id,
                                     DepartmentOptionName = sr.Department_Option_Name,
                                     SessionName = sr.Session_Name,
                                     CourseModeId = sr.Course_Mode_Id
                                 }).ToList();

                            List<Result> resultList = new List<Result>();

                            for (int i = 0; i < results.Count; i++)
                            {
                                if (results[i].MatricNumber.Contains(currentSessionSuffix))
                                {
                                    resultList.Add(results[i]);
                                }
                                else
                                {
                                    //if (deferementLogs.Where(s => s.Student.MatricNumber == results[i].MatricNumber) != null)
                                    //{
                                    //    resultList.Add(results[i]);
                                    //}
                                    //Do Nothing
                                }
                            }

                            results = new List<Result>();
                            results = resultList;

                            //for (int i = 0; i < results.Count; i++)
                            //{
                            //    if (results[i].MatricNumber.Contains(currentSessionSuffix) || results[i].MatricNumber.Contains(yearTwoSessionSuffix))
                            //    {
                            //        //Do Nothing
                            //    }
                            //    else
                            //    {
                            //        resultList.Add(results[i]);
                            //    }
                            //}

                            //results = new List<Result>();
                            //results = resultList;

                        }
                        else
                        {
                            results =
                                (from sr in await
                                     repository.GetByAsync<VW_STUDENT_RESULT_WITH_OPTIONS>(
                                         x =>
                                             x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id &&
                                             x.Programme_Id == programme.Id && x.Department_Id == department.Id &&
                                             x.Department_Option_Id == departmentOption.Id &&
                                             (x.Activated != false || x.Activated == null))
                                 select new Result
                                 {
                                     StudentId = sr.Person_Id,
                                     Sex = sr.Sex_Name,
                                     Name = sr.Name,
                                     MatricNumber = sr.Matric_Number,
                                     CourseId = sr.Course_Id,
                                     CourseCode = sr.Course_Code,
                                     CourseName = sr.Course_Name,
                                     CourseUnit = (int)sr.Course_Unit,
                                     SpecialCase = sr.Special_Case,
                                     TestScore = sr.Test_Score,
                                     ExamScore = sr.Exam_Score,
                                     Score = sr.Total_Score,
                                     Grade = sr.Grade,
                                     GradePoint = sr.Grade_Point,
                                     DepartmentName = sr.Department_Name,
                                     ProgrammeName = sr.Programme_Name,
                                     LevelName = sr.Level_Name,
                                     Semestername = sr.Semester_Name,
                                     GPCU = sr.Grade_Point * sr.Course_Unit,
                                     TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                     DepartmentOptionId = sr.Department_Option_Id,
                                     DepartmentOptionName = sr.Department_Option_Name,
                                     SessionName = sr.Session_Name,
                                     CourseModeId = sr.Course_Mode_Id
                                 }).ToList();

                            //List<Result> resultList = new List<Result>();

                            //for (int i = 0; i < results.Count; i++)
                            //{
                            //    if (results[i].MatricNumber.Contains("/17/") || results[i].MatricNumber.Contains("/13/") || results[i].MatricNumber.Contains("/14/") || results[i].MatricNumber.Contains("/15/") || results[i].MatricNumber.Contains("/12/") || results[i].MatricNumber.Contains("/11/"))
                            //    {
                            //        //Do Nothing
                            //    }
                            //    else
                            //    {
                            //        resultList.Add(results[i]);
                            //    }
                            //}

                            //results = new List<Result>();
                            //results = resultList;
                            List<Result> resultList = new List<Result>();

                            for (int i = 0; i < results.Count; i++)
                            {
                                if (results[i].MatricNumber.Contains(yearTwoSessionSuffix))
                                {
                                    resultList.Add(results[i]);
                                }
                                else
                                {
                                    //if (deferementLogs.Where(s => s.Student.MatricNumber == results[i].MatricNumber) != null)
                                    //{
                                    //    resultList.Add(results[i]);
                                    //}
                                    //Do Nothing
                                }
                            }

                            results = new List<Result>();
                            results = resultList;
                        }

                    }
                    sessionSemester = sessionSemesterLogic.GetModelBy(p => p.Session_Semester_Id == sessionSemester.Id);
                    //  List<Result> studentsResult = GetResultList(sessionSemester, level, programme, department).ToList(); 

                    List<long> students = results.Select(s => s.StudentId).Distinct().ToList();

                    List<Result> masterSheetResult = new List<Result>();

                    for (int i = 0; i < students.Count; i++)
                    {
                        long studentId = students[i];
                        TotalUnitsOmitted = 0;
                        Result result = ViewProcessedStudentResult(studentId, sessionSemester, level, programme, department);
                        result.UnitOutstanding = TotalUnitsOmitted;

                        AssignAndAddToMasterSheet(identifier, result, results.Where(s => s.StudentId == studentId).ToList(), masterSheetResult);

                        //masterSheetResult.Add(result);
                    }

                    //for (int i = 0; i < results.Count; i++)
                    //{
                    //    Result resultItem = results[i];
                    //    resultItem.Identifier = identifier;
                    //    TotalUnitsOmitted = 0;
                    //    Result result = ViewProcessedStudentResult(resultItem.StudentId, sessionSemester, level, programme, department);
                    //    result.UnitOutstanding = TotalUnitsOmitted;
                    //    masterSheetResult.Add(result);
                    //}

                    StudentExtraYearLogic extraYearLogic = new StudentExtraYearLogic();
                    List<long> extraYear = extraYearLogic.GetEntitiesBy(e => e.Session_Id == ss.Session.Id).Select(e => e.Person_Id).ToList();

                    List<Result> classResult = (from sr in await repository.GetByAsync<VW_STUDENT_RESULT_2>(x => x.Programme_Id == programme.Id && x.Department_Id == department.Id &&
                                                    (x.Activated == true || x.Activated == null))
                                                select new Result
                                                {
                                                    StudentId = sr.Person_Id,
                                                    Sex = sr.Sex_Name,
                                                    Name = sr.Name,
                                                    MatricNumber = sr.Matric_Number,
                                                    CourseId = sr.Course_Id,
                                                    CourseCode = sr.Course_Code,
                                                    CourseName = sr.Course_Name,
                                                    CourseUnit = sr.Course_Unit,
                                                    SpecialCase = sr.Special_Case,
                                                    TestScore = sr.Test_Score,
                                                    ExamScore = sr.Exam_Score,
                                                    Score = sr.Total_Score,
                                                    Grade = sr.Grade,
                                                    GradePoint = sr.Grade_Point,
                                                    DepartmentName = sr.Department_Name,
                                                    ProgrammeName = sr.Programme_Name,
                                                    LevelName = sr.Level_Name,
                                                    Semestername = sr.Semester_Name,
                                                    GPCU = sr.Grade_Point * sr.Course_Unit,
                                                    TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                                    SessionName = sr.Session_Name,
                                                    CourseModeId = sr.Course_Mode_Id
                                                }).ToList();

                    for (int i = 0; i < masterSheetResult.Count; i++)
                    {
                        Result result = masterSheetResult[i];
                        List<Result> studentResults = results.Where(p => p.StudentId == result.StudentId).ToList();
                        for (int j = 0; j < studentResults.Count; j++)
                        {
                            Result resultItem = studentResults[j];
                            resultItem.Identifier = identifier;
                            resultItem.CGPA = result.CGPA;
                            resultItem.Remark = result.Remark;
                            resultItem.GPA = result.GPA;
                            resultItem.UnitOutstanding = result.UnitOutstanding;

                            resultItem.SessionId = ss.Session.Id;

                            int totalSemesterCourseUnit = 0;
                            CheckForSpecialCase(resultItem, totalSemesterCourseUnit, (int)Semesters.FirstSemester);
                            //if (totalSemesterCourseUnit > 0)
                            //{
                            //    resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                            //}
                            if (extraYear.Contains(resultItem.StudentId))
                            {
                                resultItem.Remark = "";
                            }

                            if ((level.Id == (int)Levels.HNDII || level.Id == (int)Levels.NDII) && ss.Semester.Id == (int)Semesters.SecondSemester)
                                CheckSpecialCaseOutstanding(classResult, result.StudentId, studentResults);

                            resultItem.DepartmentOptionName = option.Name;
                        }
                    }

                    for (int i = 0; i < results.Count; i++)
                    {
                        results[i].LevelName = levels;
                        results[i].CourseMode = "Extensive";
                    }
                }
                else if (courseMode.Id == (int)CourseModes.FirstAttempt)
                {
                    results = (from sr in await repository.GetByAsync<VW_STUDENT_RESULT_WITH_OPTIONS>(x => x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id && x.Level_Id == level.Id && x.Programme_Id == programme.Id && x.Department_Id == department.Id && x.Department_Option_Id == departmentOption.Id && (x.Activated != false || x.Activated == null) && x.Course_Mode_Id == courseMode.Id)
                               select new Result
                               {
                                   StudentId = sr.Person_Id,
                                   Sex = sr.Sex_Name,
                                   Name = sr.Name,
                                   MatricNumber = sr.Matric_Number,
                                   CourseId = sr.Course_Id,
                                   CourseCode = sr.Course_Code,
                                   CourseName = sr.Course_Name,
                                   CourseUnit = (int)sr.Course_Unit,
                                   SpecialCase = sr.Special_Case,
                                   TestScore = sr.Test_Score,
                                   ExamScore = sr.Exam_Score,
                                   Score = sr.Total_Score,
                                   Grade = sr.Grade,
                                   GradePoint = sr.Grade_Point,
                                   DepartmentName = sr.Department_Name,
                                   ProgrammeName = sr.Programme_Name,
                                   LevelName = sr.Level_Name,
                                   Semestername = sr.Semester_Name,
                                   GPCU = sr.Grade_Point * sr.Course_Unit,
                                   TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                   SessionName = sr.Session_Name
                               }).ToList();

                    List<Result> resultList = new List<Result>();

                    if (level.Id == (int)Levels.NDI || level.Id == (int)Levels.HNDI)
                    {
                        for (int i = 0; i < results.Count; i++)
                        {
                            if (results[i].MatricNumber.Contains("/17/"))
                            {
                                resultList.Add(results[i]);
                            }
                            else
                            {
                                //Do Nothing
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < results.Count; i++)
                        {
                            if (results[i].MatricNumber.Contains("/16/"))
                            {
                                resultList.Add(results[i]);
                            }
                            else
                            {
                                //Do Nothing
                            }
                        }
                    }

                    results = new List<Result>();
                    results = resultList;

                    sessionSemester = sessionSemesterLogic.GetModelBy(p => p.Session_Semester_Id == sessionSemester.Id);
                    //  List<Result> studentsResult = GetResultList(sessionSemester, level, programme, department).ToList(); 
                    List<Result> masterSheetResult = new List<Result>();
                    for (int i = 0; i < results.Count; i++)
                    {
                        Result resultItem = results[i];
                        resultItem.Identifier = identifier;
                        Result result = ViewProcessedStudentResult(resultItem.StudentId, sessionSemester, level, programme, department);
                        masterSheetResult.Add(result);
                    }

                    for (int i = 0; i < masterSheetResult.Count; i++)
                    {
                        Result result = masterSheetResult[i];
                        List<Result> studentResults = results.Where(p => p.StudentId == result.StudentId).ToList();
                        for (int j = 0; j < studentResults.Count; j++)
                        {
                            Result resultItem = studentResults[j];
                            resultItem.Identifier = identifier;
                            resultItem.CGPA = result.CGPA;
                            resultItem.Remark = result.Remark;
                            resultItem.GPA = result.GPA;
                        }
                    }

                    for (int i = 0; i < results.Count; i++)
                    {
                        results[i].LevelName = levels;
                        results[i].CourseMode = "First Attempt";
                    }
                }
                else if (courseMode.Id == (int)CourseModes.CarryOver)
                {
                    results = (from sr in await repository.GetByAsync<VW_STUDENT_RESULT_WITH_OPTIONS>(x => x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id && x.Level_Id == level.Id && x.Programme_Id == programme.Id && x.Department_Id == department.Id && x.Department_Option_Id == departmentOption.Id && (x.Activated != false || x.Activated == null) && x.Course_Mode_Id == courseMode.Id)
                               select new Result
                               {
                                   StudentId = sr.Person_Id,
                                   Sex = sr.Sex_Name,
                                   Name = sr.Name,
                                   MatricNumber = sr.Matric_Number,
                                   CourseId = sr.Course_Id,
                                   CourseCode = sr.Course_Code,
                                   CourseName = sr.Course_Name,
                                   CourseUnit = (int)sr.Course_Unit,
                                   SpecialCase = sr.Special_Case,
                                   TestScore = sr.Test_Score,
                                   ExamScore = sr.Exam_Score,
                                   Score = sr.Total_Score,
                                   Grade = sr.Grade,
                                   GradePoint = sr.Grade_Point,
                                   DepartmentName = sr.Department_Name,
                                   ProgrammeName = sr.Programme_Name,
                                   LevelName = sr.Level_Name,
                                   Semestername = sr.Semester_Name,
                                   GPCU = sr.Grade_Point * sr.Course_Unit,
                                   TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                   SessionName = sr.Session_Name
                               }).ToList();

                    List<Result> resultList = new List<Result>();

                    for (int i = 0; i < results.Count; i++)
                    {
                        if (results[i].MatricNumber.Contains("/15/"))
                        {
                            resultList.Add(results[i]);
                        }
                        else
                        {
                            //Do Nothing
                        }
                    }

                    results = new List<Result>();
                    results = resultList;

                    sessionSemester = sessionSemesterLogic.GetModelBy(p => p.Session_Semester_Id == sessionSemester.Id);
                    //  List<Result> studentsResult = GetResultList(sessionSemester, level, programme, department).ToList(); 
                    List<Result> masterSheetResult = new List<Result>();
                    for (int i = 0; i < results.Count; i++)
                    {
                        Result resultItem = results[i];
                        resultItem.Identifier = identifier;
                        Result result = ViewProcessedStudentResult(resultItem.StudentId, sessionSemester, level, programme, department);
                        masterSheetResult.Add(result);
                    }

                    for (int i = 0; i < masterSheetResult.Count; i++)
                    {
                        Result result = masterSheetResult[i];
                        List<Result> studentResults = results.Where(p => p.StudentId == result.StudentId).ToList();
                        for (int j = 0; j < studentResults.Count; j++)
                        {
                            Result resultItem = studentResults[j];
                            resultItem.Identifier = identifier;
                            resultItem.CGPA = result.CGPA;
                            resultItem.Remark = result.Remark;
                            resultItem.GPA = result.GPA;
                        }
                    }

                    for (int i = 0; i < results.Count; i++)
                    {
                        results[i].LevelName = levels;
                        results[i].CourseMode = "Carry Over";
                    }
                }
                else if (courseMode.Id == (int)CourseModes.ExtraYear)
                {
                    results = (from sr in await repository.GetByAsync<VW_STUDENT_RESULT_WITH_OPTIONS>(x => x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id && x.Level_Id == level.Id && x.Programme_Id == programme.Id && x.Department_Id == department.Id && x.Department_Option_Id == departmentOption.Id && (x.Activated != false || x.Activated == null))
                               select new Result
                               {
                                   StudentId = sr.Person_Id,
                                   Sex = sr.Sex_Name,
                                   Name = sr.Name,
                                   MatricNumber = sr.Matric_Number,
                                   CourseId = sr.Course_Id,
                                   CourseCode = sr.Course_Code,
                                   CourseName = sr.Course_Name,
                                   CourseUnit = (int)sr.Course_Unit,
                                   SpecialCase = sr.Special_Case,
                                   TestScore = sr.Test_Score,
                                   ExamScore = sr.Exam_Score,
                                   Score = sr.Total_Score,
                                   Grade = sr.Grade,
                                   GradePoint = sr.Grade_Point,
                                   DepartmentName = sr.Department_Name,
                                   ProgrammeName = sr.Programme_Name,
                                   LevelName = sr.Level_Name,
                                   Semestername = sr.Semester_Name,
                                   GPCU = sr.Grade_Point * sr.Course_Unit,
                                   TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                   SessionName = sr.Session_Name
                               }).ToList();

                    List<Result> resultList = new List<Result>();

                    for (int i = 0; i < results.Count; i++)
                    {
                        if (results[i].MatricNumber.Contains("/16/") || results[i].MatricNumber.Contains("/17/"))
                        {
                            //Do Nothing
                        }
                        else
                        {
                            resultList.Add(results[i]);
                        }
                    }

                    results = new List<Result>();
                    results = resultList;

                    sessionSemester = sessionSemesterLogic.GetModelBy(p => p.Session_Semester_Id == sessionSemester.Id);
                    //  List<Result> studentsResult = GetResultList(sessionSemester, level, programme, department).ToList(); 
                    List<Result> masterSheetResult = new List<Result>();
                    for (int i = 0; i < results.Count; i++)
                    {
                        Result resultItem = results[i];
                        resultItem.Identifier = identifier;
                        Result result = ViewProcessedStudentResult(resultItem.StudentId, sessionSemester, level, programme, department);
                        masterSheetResult.Add(result);
                    }

                    for (int i = 0; i < masterSheetResult.Count; i++)
                    {
                        Result result = masterSheetResult[i];
                        List<Result> studentResults = results.Where(p => p.StudentId == result.StudentId).ToList();
                        for (int j = 0; j < studentResults.Count; j++)
                        {
                            Result resultItem = studentResults[j];
                            resultItem.Identifier = identifier;
                            resultItem.CGPA = result.CGPA;
                            resultItem.Remark = result.Remark;
                            resultItem.GPA = result.GPA;
                        }
                    }

                    for (int i = 0; i < results.Count; i++)
                    {
                        results[i].LevelName = levels;
                        results[i].CourseMode = "Extra Year";
                    }
                }

                return results.OrderBy(a => a.MatricNumber).ThenBy(a => a.CourseModeId).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<Result> GetMaterSheetDetailsByOptionsAndModeCapacity(SessionSemester sessionSemester, Level level, Programme programme, Department department, DepartmentOption departmentOption, CourseMode courseMode)
        {
            try
            {
                if (sessionSemester == null || sessionSemester.Id <= 0 || level == null || level.Id <= 0 || programme == null || programme.Id <= 0 || department == null || department.Id <= 0 || departmentOption == null || departmentOption.Id <= 0)
                {
                    throw new Exception("One or more criteria to get Mater Result Sheet not set! Please check your input criteria selection and try again.");
                }

                SessionSemesterLogic sessionSemesterLogic = new Business.SessionSemesterLogic();
                SessionSemester ss = sessionSemesterLogic.GetBy(sessionSemester.Id);
                SessionLogic sessionLogic = new SessionLogic();
                Session sessions = sessionLogic.GetModelBy(p => p.Session_Id == ss.Session.Id);
                string[] sessionItems = ss.Session.Name.Split('/');
                string sessionNameStr = sessionItems[0];
                int sessionNameInt = Convert.ToInt32(sessionNameStr);
                List<Result> results = null;

                List<Result> masterSheetResult = new List<Result>();

                string currentSessionSuffix = sessionNameStr.Substring(2, 2);
                string yearTwoSessionSuffix = Convert.ToString((Convert.ToInt32(currentSessionSuffix) - 1));

                currentSessionSuffix = "/" + currentSessionSuffix + "/";
                yearTwoSessionSuffix = "/" + yearTwoSessionSuffix + "/";

                string identifier = null;
                string departmentCode = GetDepartmentCode(department.Id);
                string levels = GetLevelName(level.Id);
                string semesterCode = GetSemesterCodeBy(ss.Semester.Id);
                string sessionCode = GetSessionCodeBy(ss.Session.Name);
                identifier = departmentCode + levels + semesterCode + sessionCode;


                //StudentDefermentLogic defermentLogic = new StudentDefermentLogic();
                //List<StudentDeferementLog> deferementLogs = defermentLogic.GetAll();

                DepartmentOptionLogic departmentOptionLogic = new DepartmentOptionLogic();
                DepartmentOption option = departmentOptionLogic.GetModelBy(d => d.Department_Option_Id == departmentOption.Id);

                if (courseMode == null)
                {
                    if (ss.Session.Id == (int)Sessions._20152016)
                    {
                        results = (from sr in repository.GetBy<VW_STUDENT_RESULT_WITH_OPTIONS>(
                                     x =>
                                         x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id &&
                                         x.Level_Id == level.Id && x.Programme_Id == programme.Id &&
                                         x.Department_Id == department.Id &&
                                         x.Department_Option_Id == departmentOption.Id &&
                                         (x.Activated != false || x.Activated == null))
                                   select new Result
                                   {
                                       StudentId = sr.Person_Id,
                                       Sex = sr.Sex_Name,
                                       Name = sr.Name,
                                       MatricNumber = sr.Matric_Number,
                                       CourseId = sr.Course_Id,
                                       CourseCode = sr.Course_Code,
                                       CourseName = sr.Course_Name,
                                       CourseUnit = (int)sr.Course_Unit,
                                       SpecialCase = sr.Special_Case,
                                       TestScore = sr.Test_Score,
                                       ExamScore = sr.Exam_Score,
                                       Score = sr.Total_Score,
                                       Grade = sr.Grade,
                                       GradePoint = sr.Grade_Point,
                                       DepartmentName = sr.Department_Name,
                                       ProgrammeName = sr.Programme_Name,
                                       LevelName = sr.Level_Name,
                                       Semestername = sr.Semester_Name,
                                       GPCU = sr.Grade_Point * sr.Course_Unit,
                                       TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                       DepartmentOptionId = sr.Department_Option_Id,
                                       DepartmentOptionName = sr.Department_Option_Name,
                                       SessionName = sr.Session_Name,
                                       CourseModeId = sr.Course_Mode_Id
                                   }).ToList();
                    }
                    else
                    {
                        if (level.Id == (int)Levels.NDI || level.Id == (int)Levels.HNDI)
                        {
                            results =
                                (from sr in
                                     repository.GetBy<VW_STUDENT_RESULT_WITH_OPTIONS>(
                                         x =>
                                             x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id &&
                                             x.Level_Id == level.Id && x.Programme_Id == programme.Id &&
                                             x.Department_Id == department.Id &&
                                             x.Department_Option_Id == departmentOption.Id &&
                                             (x.Activated != false || x.Activated == null))
                                 select new Result
                                 {
                                     StudentId = sr.Person_Id,
                                     Sex = sr.Sex_Name,
                                     Name = sr.Name,
                                     MatricNumber = sr.Matric_Number,
                                     CourseId = sr.Course_Id,
                                     CourseCode = sr.Course_Code,
                                     CourseName = sr.Course_Name,
                                     CourseUnit = (int)sr.Course_Unit,
                                     SpecialCase = sr.Special_Case,
                                     TestScore = sr.Test_Score,
                                     ExamScore = sr.Exam_Score,
                                     Score = sr.Total_Score,
                                     Grade = sr.Grade,
                                     GradePoint = sr.Grade_Point,
                                     DepartmentName = sr.Department_Name,
                                     ProgrammeName = sr.Programme_Name,
                                     LevelName = sr.Level_Name,
                                     Semestername = sr.Semester_Name,
                                     GPCU = sr.Grade_Point * sr.Course_Unit,
                                     TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                     DepartmentOptionId = sr.Department_Option_Id,
                                     DepartmentOptionName = sr.Department_Option_Name,
                                     SessionName = sr.Session_Name,
                                     CourseModeId = sr.Course_Mode_Id
                                 }).ToList();

                            List<Result> resultList = new List<Result>();

                            for (int i = 0; i < results.Count; i++)
                            {
                                if (results[i].MatricNumber.Contains(currentSessionSuffix))
                                {
                                    resultList.Add(results[i]);
                                }
                                else
                                {
                                    //if (deferementLogs.Where(s => s.Student.MatricNumber == results[i].MatricNumber) != null)
                                    //{
                                    //    resultList.Add(results[i]);
                                    //}
                                    //Do Nothing
                                }
                            }

                            results = new List<Result>();
                            results = resultList;

                            //for (int i = 0; i < results.Count; i++)
                            //{
                            //    if (results[i].MatricNumber.Contains(currentSessionSuffix) || results[i].MatricNumber.Contains(yearTwoSessionSuffix))
                            //    {
                            //        //Do Nothing
                            //    }
                            //    else
                            //    {
                            //        resultList.Add(results[i]);
                            //    }
                            //}

                            //results = new List<Result>();
                            //results = resultList;

                        }
                        else
                        {
                            results =
                                (from sr in
                                     repository.GetBy<VW_STUDENT_RESULT_WITH_OPTIONS>(
                                         x =>
                                             x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id &&
                                             x.Programme_Id == programme.Id && x.Department_Id == department.Id &&
                                             x.Department_Option_Id == departmentOption.Id &&
                                             (x.Activated != false || x.Activated == null))
                                 select new Result
                                 {
                                     StudentId = sr.Person_Id,
                                     Sex = sr.Sex_Name,
                                     Name = sr.Name,
                                     MatricNumber = sr.Matric_Number,
                                     CourseId = sr.Course_Id,
                                     CourseCode = sr.Course_Code,
                                     CourseName = sr.Course_Name,
                                     CourseUnit = (int)sr.Course_Unit,
                                     SpecialCase = sr.Special_Case,
                                     TestScore = sr.Test_Score,
                                     ExamScore = sr.Exam_Score,
                                     Score = sr.Total_Score,
                                     Grade = sr.Grade,
                                     GradePoint = sr.Grade_Point,
                                     DepartmentName = sr.Department_Name,
                                     ProgrammeName = sr.Programme_Name,
                                     LevelName = sr.Level_Name,
                                     Semestername = sr.Semester_Name,
                                     GPCU = sr.Grade_Point * sr.Course_Unit,
                                     TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                     DepartmentOptionId = sr.Department_Option_Id,
                                     DepartmentOptionName = sr.Department_Option_Name,
                                     SessionName = sr.Session_Name,
                                     CourseModeId = sr.Course_Mode_Id
                                 }).ToList();

                            //List<Result> resultList = new List<Result>();

                            //for (int i = 0; i < results.Count; i++)
                            //{
                            //    if (results[i].MatricNumber.Contains("/17/") || results[i].MatricNumber.Contains("/13/") || results[i].MatricNumber.Contains("/14/") || results[i].MatricNumber.Contains("/15/") || results[i].MatricNumber.Contains("/12/") || results[i].MatricNumber.Contains("/11/"))
                            //    {
                            //        //Do Nothing
                            //    }
                            //    else
                            //    {
                            //        resultList.Add(results[i]);
                            //    }
                            //}

                            //results = new List<Result>();
                            //results = resultList;
                            List<Result> resultList = new List<Result>();

                            for (int i = 0; i < results.Count; i++)
                            {
                                if (results[i].MatricNumber.Contains(yearTwoSessionSuffix))
                                {
                                    resultList.Add(results[i]);
                                }
                                else
                                {
                                    //if (deferementLogs.Where(s => s.Student.MatricNumber == results[i].MatricNumber) != null)
                                    //{
                                    //    resultList.Add(results[i]);
                                    //}
                                    //Do Nothing
                                }
                            }

                            results = new List<Result>();
                            results = resultList;
                        }

                    }
                    sessionSemester = sessionSemesterLogic.GetModelBy(p => p.Session_Semester_Id == sessionSemester.Id);
                    //  List<Result> studentsResult = GetResultList(sessionSemester, level, programme, department).ToList(); 

                     List<long> students = new List<long>();

                    DepartmentCapacityLogic capacityLogic = new DepartmentCapacityLogic();
                    DepartmentCapacity capacity = capacityLogic.GetModelsBy(c => c.Programme_Id == programme.Id && c.Department_Id == department.Id && c.Session_Id == ss.Session.Id && c.Activated).LastOrDefault();
                    if (results.Count > 0)
                    {
                        if (capacity != null)
                        {
                            students = results.Select(s => s.StudentId).Distinct().Take(capacity.Capacity).ToList();
                        }
                        else
                        {
                            students = results.Select(s => s.StudentId).Distinct().ToList();
                        }
                    }

                    for (int i = 0; i < students.Count; i++)
                    {
                        long studentId = students[i];
                        TotalUnitsOmitted = 0;
                        Result result = ViewProcessedStudentResult(studentId, sessionSemester, level, programme, department);
                        result.UnitOutstanding = TotalUnitsOmitted;

                        AssignAndAddToMasterSheet(identifier, result, results.Where(s => s.StudentId == studentId).ToList(), masterSheetResult);

                        //masterSheetResult.Add(result);
                    }


                    StudentExtraYearLogic extraYearLogic = new StudentExtraYearLogic();
                    List<long> extraYear = extraYearLogic.GetEntitiesBy(e => e.Session_Id == ss.Session.Id).Select(e => e.Person_Id).ToList();

                    for (int i = 0; i < masterSheetResult.Count; i++)
                    {
                        Result result = masterSheetResult[i];
                        List<Result> studentResults = masterSheetResult.Where(p => p.StudentId == result.StudentId).ToList();
                        for (int j = 0; j < studentResults.Count; j++)
                        {
                            Result resultItem = studentResults[j];
                            resultItem.Identifier = identifier;
                            resultItem.CGPA = result.CGPA;
                            resultItem.Remark = result.Remark;
                            resultItem.GPA = result.GPA;
                            resultItem.UnitOutstanding = result.UnitOutstanding;

                            resultItem.SessionId = ss.Session.Id;

                            int totalSemesterCourseUnit = 0;
                            CheckForSpecialCase(resultItem, totalSemesterCourseUnit, (int)Semesters.FirstSemester);
                            //if (totalSemesterCourseUnit > 0)
                            //{
                            //    resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                            //}
                            if (extraYear.Contains(resultItem.StudentId))
                            {
                                resultItem.Remark = "";
                            }

                            resultItem.DepartmentOptionName = option.Name;
                        }
                    }

                    for (int i = 0; i < masterSheetResult.Count; i++)
                    {
                        masterSheetResult[i].LevelName = levels;
                        masterSheetResult[i].CourseMode = "Extensive";
                    }
                }

                return masterSheetResult.OrderBy(a => a.MatricNumber).ThenBy(a => a.CourseModeId).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<Result> GetMaterSheetDetailsAltBy(SessionSemester sessionSemester, Level level, Programme programme, Department department, CourseMode courseMode)
        {
            try
            {
                if (sessionSemester == null || sessionSemester.Id <= 0 || level == null || level.Id <= 0 || programme == null || programme.Id <= 0 || department == null || department.Id <= 0 || courseMode == null || courseMode.Id <= 0)
                {
                    throw new Exception("One or more criteria to get Mater Result Sheet not set! Please check your input criteria selection and try again.");
                }

                SessionSemesterLogic sessionSemesterLogic = new Business.SessionSemesterLogic();
                SessionSemester ss = sessionSemesterLogic.GetBy(sessionSemester.Id);
                SessionLogic sessionLogic = new SessionLogic();
                Session sessions = sessionLogic.GetModelBy(p => p.Session_Id == ss.Session.Id);
                string[] sessionItems = ss.Session.Name.Split('/');
                string sessionNameStr = sessionItems[0];
                int sessionNameInt = Convert.ToInt32(sessionNameStr);
                List<Result> results = null;

                string identifier = null;
                string departmentCode = GetDepartmentCode(department.Id);
                string levels = GetLevelName(level.Id);
                string semesterCode = GetSemesterCodeBy(ss.Semester.Id);
                string sessionCode = GetSessionCodeBy(ss.Session.Name);
                identifier = departmentCode + levels + semesterCode + sessionCode;




                results = (from sr in repository.GetBy<VW_STUDENT_RESULT_2>(x => x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id && x.Level_Id == level.Id && x.Programme_Id == programme.Id && x.Department_Id == department.Id)
                           select new Result
                           {
                               StudentId = sr.Person_Id,
                               Sex = sr.Sex_Name,
                               Name = sr.Name,
                               MatricNumber = sr.Matric_Number,
                               CourseId = sr.Course_Id,
                               CourseCode = sr.Course_Code,
                               CourseName = sr.Course_Name,
                               CourseUnit = sr.Course_Unit,
                               SpecialCase = sr.Special_Case,
                               TestScore = sr.Test_Score,
                               ExamScore = sr.Exam_Score,
                               Score = sr.Total_Score,
                               Grade = sr.Grade,
                               GradePoint = sr.Grade_Point,
                               DepartmentName = sr.Department_Name,
                               ProgrammeName = sr.Programme_Name,
                               LevelName = sr.Level_Name,
                               Semestername = sr.Semester_Name,
                               GPCU = sr.Grade_Point * sr.Course_Unit,
                               TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                               CourseModeId = sr.Course_Mode_Id,
                               CourseModeName = sr.Course_Mode_Name
                           }).ToList();



                sessionSemester = sessionSemesterLogic.GetModelBy(p => p.Session_Semester_Id == sessionSemester.Id);
                // List<Result> studentsResult = GetResultList(sessionSemester, level, programme, department).ToList();
                List<Result> masterSheetResult = new List<Result>();
                foreach (Result resultItem in results)
                {
                    resultItem.Identifier = identifier;
                    Result result = ViewProcessedStudentResult(resultItem.StudentId, sessionSemester, level, programme, department);
                    masterSheetResult.Add(result);
                }


                foreach (Result result in masterSheetResult)
                {
                    List<Result> studentResults = results.Where(p => p.StudentId == result.StudentId).ToList();
                    foreach (Result resultItem in studentResults)
                    {
                        resultItem.Identifier = identifier;
                        resultItem.CGPA = result.CGPA;
                        resultItem.Remark = result.Remark;
                        resultItem.GPA = result.GPA;

                    }

                }

                List<Result> resultsMode = (from sr in repository.GetBy<VW_STUDENT_RESULT_2>(x => x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id && x.Level_Id == level.Id && x.Programme_Id == programme.Id && x.Department_Id == department.Id && x.Course_Mode_Id == courseMode.Id)
                                            select new Result
                                            {
                                                StudentId = sr.Person_Id,
                                                Sex = sr.Sex_Name,
                                                Name = sr.Name,
                                                MatricNumber = sr.Matric_Number,
                                                CourseId = sr.Course_Id,
                                                CourseCode = sr.Course_Code,
                                                CourseName = sr.Course_Name,
                                                CourseUnit = sr.Course_Unit,
                                                SpecialCase = sr.Special_Case,
                                                TestScore = sr.Test_Score,
                                                ExamScore = sr.Exam_Score,
                                                Score = sr.Total_Score,
                                                Grade = sr.Grade,
                                                GradePoint = sr.Grade_Point,
                                                DepartmentName = sr.Department_Name,
                                                ProgrammeName = sr.Programme_Name,
                                                LevelName = sr.Level_Name,
                                                Semestername = sr.Semester_Name,
                                                GPCU = sr.Grade_Point * sr.Course_Unit,
                                                TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                                CourseModeId = sr.Course_Mode_Id,
                                                CourseModeName = sr.Course_Mode_Name
                                            }).ToList();

                List<Result> ResultsAlt = new List<Result>();
                List<string> RegNumbers = resultsMode.Select(r => r.MatricNumber).Distinct().ToList();

                for (int i = 0; i < RegNumbers.Count; i++)
                {
                    List<Result> thisResult = results.Where(r => r.MatricNumber == RegNumbers[i]).ToList();
                    for (int j = 0; j < thisResult.Count; j++)
                    {
                        ResultsAlt.Add(thisResult[j]);
                    }
                }

                return ResultsAlt.OrderBy(a => a.MatricNumber).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        private List<Result> GetResultList(SessionSemester sessionSemester, Level level, Programme programme, Department department)
        {
            try
            {
                List<Result> filteredResult = new List<Result>();
                StudentResultLogic studentResultLogic = new StudentResultLogic();
                List<string> resultList = studentResultLogic.GetProcessedResutBy(sessionSemester.Session, sessionSemester.Semester, level, department, programme).Select(p => p.MatricNumber).AsParallel().Distinct().ToList();
                List<Result> result = studentResultLogic.GetProcessedResutBy(sessionSemester.Session, sessionSemester.Semester, level, department, programme);
                foreach (string item in resultList)
                {
                    Result resultItem = result.Where(p => p.MatricNumber == item).FirstOrDefault();
                    filteredResult.Add(resultItem);
                }
                return filteredResult.ToList();
            }
            catch (Exception)
            {

                throw;
            }

        }
        //public Result ViewProcessedStudentResult(long PersonId, SessionSemester sessionSemester, Level level, Programme programme, Department department)
        //{
        //    Result ProcessedResult = new Result();
        //    string Remark = null;
        //    try
        //    {

        //        if (PersonId > 0)
        //        {
        //            Abundance_Nk.Model.Model.Student student = new Model.Model.Student() { Id = PersonId };
        //            StudentLogic studentLogic = new StudentLogic();
        //            StudentResultLogic studentResultLogic = new StudentResultLogic();
        //            if (sessionSemester.Semester != null && sessionSemester.Session != null && programme != null && department != null && level != null)
        //            {
        //                if (level.Id == (int)Levels.NDI || level.Id == (int)Levels.HNDI)
        //                {
        //                    Abundance_Nk.Model.Model.Student studentCheck = studentLogic.GetModelBy(p => p.Person_Id == PersonId);
        //                    if (sessionSemester.Semester.Id == (int)Semesters.FirstSemester)
        //                    {
        //                        List<Result> result = null;
        //                        if (studentCheck.Activated == true || studentCheck.Activated == null)
        //                        {
        //                            result = studentResultLogic.GetStudentProcessedResultBy(sessionSemester.Session, level, department, student, sessionSemester.Semester, programme);
        //                        }

        //                        List<Result> modifiedResultList = new List<Result>();
        //                        int totalSemesterCourseUnit = 0;
        //                        foreach (Result resultItem in result)
        //                        {
        //                            decimal WGP = 0;
        //                            totalSemesterCourseUnit = CheckForSpecialCase(resultItem, totalSemesterCourseUnit);
        //                            if (totalSemesterCourseUnit > 0)
        //                            {
        //                                resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
        //                            }
        //                            modifiedResultList.Add(resultItem);
        //                        }
        //                        decimal firstSemesterGPCUSum = modifiedResultList.Sum(p => p.GPCU) ?? 0;
        //                        int? firstSemesterTotalSemesterCourseUnit = 0;
        //                        Result firstYearFirstSemesterResult = modifiedResultList.FirstOrDefault();
        //                        firstSemesterTotalSemesterCourseUnit = modifiedResultList.Min(p => p.TotalSemesterCourseUnit) ?? 0;
        //                        decimal firstSemesterGPA = firstSemesterGPCUSum / firstSemesterTotalSemesterCourseUnit ?? 0;
        //                        firstYearFirstSemesterResult.GPA = Decimal.Round(firstSemesterGPA, 2);
        //                        firstYearFirstSemesterResult.CGPA = Decimal.Round(firstSemesterGPA, 2);
        //                        firstYearFirstSemesterResult.GPCU = firstSemesterGPCUSum;
        //                        firstYearFirstSemesterResult.TotalSemesterCourseUnit = firstSemesterTotalSemesterCourseUnit;
        //                        Remark = GetGraduationStatus(firstYearFirstSemesterResult.CGPA, GetFirstYearCarryOverCourses(sessionSemester, level, programme, department, student));
        //                        firstYearFirstSemesterResult.Remark = Remark;
        //                        ProcessedResult = firstYearFirstSemesterResult;

        //                    }
        //                    else
        //                    {
        //                        List<Result> result = null;
        //                        Semester firstSemester = new Semester() { Id = (int)Semesters.FirstSemester };
        //                        if (studentCheck.Activated == true || studentCheck.Activated == null)
        //                        {
        //                            result = studentResultLogic.GetStudentProcessedResultBy(sessionSemester.Session, level, department, student, firstSemester, programme);
        //                        }
        //                        else
        //                        {
        //                            result = studentResultLogic.GetDeactivatedStudentProcessedResultBy(sessionSemester.Session, level, department, student, firstSemester, programme);
        //                        }
        //                        List<Result> firstSemesterModifiedResultList = new List<Result>();
        //                        int totalFirstSemesterCourseUnit = 0;
        //                        foreach (Result resultItem in result)
        //                        {
        //                           decimal WGP = 0;
        //                           totalFirstSemesterCourseUnit = CheckForSpecialCase(resultItem, totalFirstSemesterCourseUnit);
        //                           if (totalFirstSemesterCourseUnit > 0)
        //                            {
        //                                resultItem.TotalSemesterCourseUnit = totalFirstSemesterCourseUnit;
        //                            }
        //                            firstSemesterModifiedResultList.Add(resultItem);
        //                        }


        //                        decimal firstSemesterGPCUSum = firstSemesterModifiedResultList.Sum(p => p.GPCU) ?? 0;
        //                        int? firstSemesterTotalSemesterCourseUnit = 0;
        //                        Result firstYearFirstSemesterResult = firstSemesterModifiedResultList.FirstOrDefault() ;
        //                        firstSemesterTotalSemesterCourseUnit = firstSemesterModifiedResultList.Min(p => p.TotalSemesterCourseUnit) ?? 0;
        //                        decimal firstSemesterGPA = firstSemesterGPCUSum / firstSemesterTotalSemesterCourseUnit ?? 0;
        //                        firstYearFirstSemesterResult.GPA = Decimal.Round(firstSemesterGPA,2);

        //                        Semester secondSemester = new Semester() { Id = (int)Semesters.SecondSemester };
        //                        List<Result> secondSemesterResult = null;
        //                        if (studentCheck.Activated == true || studentCheck.Activated == null)
        //                        {
        //                            secondSemesterResult = studentResultLogic.GetStudentProcessedResultBy(sessionSemester.Session, level, department, student, secondSemester, programme);
        //                        }
        //                        else
        //                        {
        //                            secondSemesterResult = studentResultLogic.GetDeactivatedStudentProcessedResultBy(sessionSemester.Session, level, department, student, secondSemester, programme);
        //                        }
        //                        List<Result> secondSemesterModifiedResultList = new List<Result>();

        //                        int totalSecondSemesterCourseUnit = 0;
        //                        foreach (Result resultItem in secondSemesterResult)
        //                        {

        //                            decimal WGP = 0;

        //                            totalSecondSemesterCourseUnit = CheckForSpecialCase(resultItem, totalSecondSemesterCourseUnit);

        //                            if (totalSecondSemesterCourseUnit > 0)
        //                            {
        //                                resultItem.TotalSemesterCourseUnit = totalSecondSemesterCourseUnit;
        //                            }
        //                            secondSemesterModifiedResultList.Add(resultItem);
        //                        }
        //                        decimal? secondSemesterGPCUSum = secondSemesterModifiedResultList.Sum(p => p.GPCU);
        //                        Result secondSemesterStudentResult = secondSemesterModifiedResultList.FirstOrDefault();

        //                        secondSemesterStudentResult.GPA = Decimal.Round((decimal)(secondSemesterGPCUSum / (decimal)(secondSemesterModifiedResultList.Min(p => p.TotalSemesterCourseUnit))), 2);
        //                        secondSemesterStudentResult.CGPA = Decimal.Round((decimal)((firstSemesterGPCUSum + secondSemesterGPCUSum) / (secondSemesterModifiedResultList.Min(p => p.TotalSemesterCourseUnit) + firstSemesterTotalSemesterCourseUnit)), 2);
        //                        if (secondSemesterStudentResult.GPA < (decimal)2.0 && firstYearFirstSemesterResult.GPA < (decimal)2.0)
        //                        {
        //                            Remark = GetGraduationStatus(firstYearFirstSemesterResult.CGPA, firstYearFirstSemesterResult.GPA, secondSemesterStudentResult.GPA, GetFirstYearCarryOverCourses(sessionSemester, level, programme, department, student));
        //                        }
        //                        else
        //                        {
        //                            Remark = GetGraduationStatus(firstYearFirstSemesterResult.CGPA, GetFirstYearCarryOverCourses(sessionSemester, level, programme, department, student));
        //                        }
        //                        secondSemesterStudentResult.Remark = Remark;
        //                        ProcessedResult = secondSemesterStudentResult;

        //                    }

        //                }
        //                else
        //                {
        //                    decimal firstYearFirstSemesterGPCUSum = 0;
        //                    int firstYearFirstSemesterTotalCourseUnit = 0;
        //                    decimal firstYearSecondSemesterGPCUSum = 0;
        //                    int firstYearSecondSemesterTotalCourseUnit = 0;
        //                    decimal secondYearFirstSemesterGPCUSum = 0;
        //                    int secondYearFirstSemesterTotalCourseUnit = 0;
        //                    decimal secondYearSecondSemesterGPCUSum = 0;
        //                    int secondYearSecondSemesterTotalCourseUnit = 0;

        //                    Result firstYearFirstSemester = GetFirstYearFirstSemesterResultInfo(sessionSemester,  level, programme,  department, student);
        //                    Result firstYearSecondSemester = GetFirstYearSecondSemesterResultInfo(sessionSemester, level, programme, department, student);
        //                    if (sessionSemester.Semester.Id == 1)
        //                    {
        //                        List<Result> result = null;
        //                        Abundance_Nk.Model.Model.Student studentCheck = studentLogic.GetModelBy(p => p.Person_Id == student.Id);
        //                        Semester semester = new Semester() { Id = (int)Semesters.FirstSemester };
        //                        if (student.Activated == true || studentCheck.Activated == null)
        //                        {
        //                            result = studentResultLogic.GetStudentProcessedResultBy(sessionSemester.Session, level, department, student, semester, programme);
        //                        }
        //                        else
        //                        {
        //                            result = studentResultLogic.GetDeactivatedStudentProcessedResultBy(sessionSemester.Session, level, department, student, semester, programme);
        //                        }
        //                        List<Result> modifiedResultList = new List<Result>();
        //                        int totalSemesterCourseUnit = 0;
        //                        foreach (Result resultItem in result)
        //                        {
        //                            decimal WGP = 0;
        //                            totalSemesterCourseUnit = CheckForSpecialCase(resultItem, totalSemesterCourseUnit);
        //                            if (totalSemesterCourseUnit > 0)
        //                            {
        //                                resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
        //                            }
        //                            modifiedResultList.Add(resultItem);
        //                        }
        //                        Result secondYearFirstSemesterResult = new Result();
        //                        decimal firstSemesterGPCUSum = modifiedResultList.Sum(p => p.GPCU) ?? 0;
        //                        int secondYearfirstSemesterTotalSemesterCourseUnit = 0;
        //                        secondYearFirstSemesterResult = modifiedResultList.FirstOrDefault();
        //                        secondYearfirstSemesterTotalSemesterCourseUnit = modifiedResultList.Min(p => p.TotalSemesterCourseUnit) ?? 0;
        //                        secondYearFirstSemesterResult.TotalSemesterCourseUnit = secondYearfirstSemesterTotalSemesterCourseUnit;
        //                        secondYearFirstSemesterResult.GPCU = firstSemesterGPCUSum;
        //                        decimal firstSemesterGPA = firstSemesterGPCUSum / secondYearfirstSemesterTotalSemesterCourseUnit;
        //                        secondYearFirstSemesterResult.GPA = Decimal.Round(firstSemesterGPA,2);
        //                        if (firstYearFirstSemester != null && firstYearFirstSemester.GPCU != null && firstYearSecondSemester != null && firstYearSecondSemester.GPCU != null)
        //                        {
        //                           secondYearFirstSemesterResult.CGPA = Decimal.Round((decimal)((firstSemesterGPCUSum + firstYearFirstSemester.GPCU + firstYearSecondSemester.GPCU) / (firstYearSecondSemester.TotalSemesterCourseUnit + firstYearFirstSemester.TotalSemesterCourseUnit + secondYearfirstSemesterTotalSemesterCourseUnit)), 2);
        //                        }
        //                        else
        //                        {
        //                            secondYearFirstSemesterResult.CGPA = secondYearFirstSemesterResult.GPA;
        //                        }
        //                        List<string> secondYearFirstSemetserCarryOverCourses = GetSecondYearCarryOverCourses(sessionSemester, level, programme, department, student);
        //                        secondYearFirstSemesterResult.Remark = GetGraduationStatus(secondYearFirstSemesterResult.CGPA, secondYearFirstSemetserCarryOverCourses);

        //                        ProcessedResult = secondYearFirstSemesterResult;

        //                    }
        //                    else if (sessionSemester.Semester.Id == (int)Semesters.SecondSemester)
        //                    {

        //                        List<Result> result = null;
        //                        Abundance_Nk.Model.Model.Student studentCheck = studentLogic.GetModelBy(p => p.Person_Id == student.Id);
        //                        Semester semester = new Semester() { Id = (int)Semesters.FirstSemester };

        //                        if (student.Activated == true || studentCheck.Activated == null)
        //                        {
        //                            result = studentResultLogic.GetStudentProcessedResultBy(sessionSemester.Session, level, department, student, semester, programme);
        //                        }
        //                        else
        //                        {
        //                            result = studentResultLogic.GetDeactivatedStudentProcessedResultBy(sessionSemester.Session, level, department, student, semester, programme);
        //                        }
        //                        List<Result> modifiedResultList = new List<Result>();
        //                        int totalSemesterCourseUnit = 0;
        //                        foreach (Result resultItem in result)
        //                        {
        //                            decimal WGP = 0;
        //                            totalSemesterCourseUnit = CheckForSpecialCase(resultItem, totalSemesterCourseUnit);
        //                            if (totalSemesterCourseUnit > 0)
        //                            {
        //                                resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
        //                            }
        //                            modifiedResultList.Add(resultItem);
        //                        }
        //                        Result secondYearFirstSemesterResult = new Result();
        //                        decimal secondYearfirstSemesterGPCUSum = modifiedResultList.Sum(p => p.GPCU) ?? 0;
        //                        int secondYearfirstSemesterTotalSemesterCourseUnit = 0;
        //                        secondYearfirstSemesterTotalSemesterCourseUnit = modifiedResultList.Min(p => p.TotalSemesterCourseUnit) ?? 0;
        //                        secondYearFirstSemesterResult.TotalSemesterCourseUnit = secondYearfirstSemesterTotalSemesterCourseUnit;
        //                        secondYearFirstSemesterResult.GPCU = secondYearfirstSemesterGPCUSum;
        //                        decimal firstSemesterGPA = secondYearfirstSemesterGPCUSum / secondYearfirstSemesterTotalSemesterCourseUnit;
        //                        secondYearFirstSemesterResult.GPA = Decimal.Round(firstSemesterGPA, 2);

        //                        //Second semester second year

        //                        List<Result> secondSemesterResult = null;
        //                        Semester secondSemester = new Semester() { Id = (int)Semesters.SecondSemester };
        //                        if (student.Activated == true || studentCheck.Activated == null)
        //                        {
        //                            secondSemesterResult = studentResultLogic.GetStudentProcessedResultBy(sessionSemester.Session, level, department, student, secondSemester, programme);
        //                        }
        //                        else
        //                        {
        //                            secondSemesterResult = studentResultLogic.GetDeactivatedStudentProcessedResultBy(sessionSemester.Session, level, department, student, secondSemester, programme);
        //                        }
        //                        List<Result> secondSemesterModifiedResultList = new List<Result>();
        //                        int totalSecondSemesterCourseUnit = 0;
        //                        foreach (Result resultItem in secondSemesterResult)
        //                        {
        //                           decimal WGP = 0;
        //                           totalSecondSemesterCourseUnit = CheckForSpecialCase(resultItem, totalSecondSemesterCourseUnit);
        //                            if (totalSecondSemesterCourseUnit > 0)
        //                            {
        //                               resultItem.TotalSemesterCourseUnit = totalSecondSemesterCourseUnit;
        //                            }
        //                            secondSemesterModifiedResultList.Add(resultItem);
        //                        }
        //                        Result secondYearSecondtSemesterResult = new Result();
        //                        decimal secondYearSecondtSemesterGPCUSum = secondSemesterModifiedResultList.Sum(p => p.GPCU) ?? 0;
        //                        int secondYearSecondSemesterTotalSemesterCourseUnit = 0;
        //                        secondYearSecondSemesterTotalSemesterCourseUnit = secondSemesterModifiedResultList.Min(p => p.TotalSemesterCourseUnit) ?? 0;
        //                        secondYearSecondtSemesterResult = secondSemesterModifiedResultList.FirstOrDefault();
        //                        secondYearSecondtSemesterResult.TotalSemesterCourseUnit = secondYearSecondSemesterTotalSemesterCourseUnit;
        //                        secondYearSecondtSemesterResult.GPCU = secondYearSecondtSemesterGPCUSum;
        //                        decimal secondYearSecondSmesterGPA = secondYearSecondtSemesterGPCUSum / secondYearSecondSemesterTotalSemesterCourseUnit;
        //                        secondYearSecondtSemesterResult.GPA = Decimal.Round(secondYearSecondSmesterGPA, 2);
        //                        secondYearSecondtSemesterResult.CGPA = Decimal.Round((decimal)((secondYearfirstSemesterGPCUSum + firstYearFirstSemester.GPCU + firstYearSecondSemester.GPCU + secondYearSecondtSemesterGPCUSum) / (firstYearSecondSemester.TotalSemesterCourseUnit + firstYearFirstSemester.TotalSemesterCourseUnit + secondYearfirstSemesterTotalSemesterCourseUnit + secondYearSecondSemesterTotalSemesterCourseUnit)), 2);
        //                        List<string> secondYearSecondSemetserCarryOverCourses = GetSecondYearCarryOverCourses(sessionSemester, level, programme, department, student);
        //                        if (secondYearSecondtSemesterResult.GPA < (decimal)2.0 && secondYearFirstSemesterResult.GPA < (decimal)2.0)
        //                        {
        //                            secondYearSecondtSemesterResult.Remark = GetGraduationStatus(secondYearFirstSemesterResult.CGPA, secondYearFirstSemesterResult.GPA, secondYearSecondtSemesterResult.GPA, secondYearSecondSemetserCarryOverCourses);
        //                        }
        //                        else
        //                        {
        //                            secondYearSecondtSemesterResult.Remark = GetGraduationStatus(secondYearFirstSemesterResult.CGPA, secondYearSecondSemetserCarryOverCourses);
        //                        }

        //                        ProcessedResult = secondYearSecondtSemesterResult;
        //                    }



        //                }
        //            }
        //        }

        //    }
        //    catch (Exception )
        //    {


        //    }
        //    return ProcessedResult;
        //}

        public Result ViewProcessedStudentResult(long PersonId, SessionSemester sessionSemester, Level level, Programme programme, Department department)
        {
            Result ProcessedResult = new Result();
            string Remark = null;
            try
            {
                if (PersonId > 0)
                {
                    Abundance_Nk.Model.Model.Student student = new Model.Model.Student() { Id = PersonId };
                    StudentLogic studentLogic = new StudentLogic();
                    GetStudent(studentLogic, student);
                    StudentResultLogic studentResultLogic = new StudentResultLogic();

                    if (sessionSemester.Semester != null && sessionSemester.Session != null && programme != null && department != null && level != null)
                    {
                        if (level.Id == (int)Levels.NDI || level.Id == (int)Levels.HNDI)
                        {
                            Abundance_Nk.Model.Model.Student studentCheck = new Student() { Id = PersonId };
                            GetStudent(studentLogic, studentCheck);
                            if (sessionSemester.Semester.Id == (int)Semesters.FirstSemester)
                            {
                                List<Result> result = null;
                                if (studentCheck.Activated == true || studentCheck.Activated == null)
                                {
                                    result = studentResultLogic.GetStudentProcessedResultBy(sessionSemester.Session, level, department, student, sessionSemester.Semester, programme);
                                }

                                List<Result> modifiedResultList = new List<Result>();
                                int totalSemesterCourseUnit = 0;
                                for (int i = 0; i < result.Count; i++)
                                {
                                    Result resultItem = result[i];
                                    decimal WGP = 0;
                                    totalSemesterCourseUnit = CheckForSpecialCase(resultItem, totalSemesterCourseUnit, (int)Semesters.FirstSemester);
                                    if (totalSemesterCourseUnit > 0)
                                    {
                                        resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                                    }
                                    modifiedResultList.Add(resultItem);
                                }

                                decimal firstSemesterGPCUSum = modifiedResultList.Sum(p => p.GPCU) ?? 0;
                                int? firstSemesterTotalSemesterCourseUnit = 0;
                                Result firstYearFirstSemesterResult = modifiedResultList.FirstOrDefault();
                                firstSemesterTotalSemesterCourseUnit = modifiedResultList.Min(p => p.TotalSemesterCourseUnit) ?? 0;
                                decimal firstSemesterGPA = 0M;
                                if (firstSemesterTotalSemesterCourseUnit != 0)
                                {
                                    firstSemesterGPA = firstSemesterGPCUSum / firstSemesterTotalSemesterCourseUnit ?? 0;
                                }
                                firstYearFirstSemesterResult.GPA = Decimal.Round(firstSemesterGPA, 2);
                                firstYearFirstSemesterResult.CGPA = Decimal.Round(firstSemesterGPA, 2);
                                firstYearFirstSemesterResult.GPCU = firstSemesterGPCUSum;
                                firstYearFirstSemesterResult.TotalSemesterCourseUnit = firstSemesterTotalSemesterCourseUnit;
                                Remark = GetGraduationStatus(firstYearFirstSemesterResult.CGPA, GetFirstYearCarryOverCourses(sessionSemester, level, programme, department, student));
                                firstYearFirstSemesterResult.Remark = Remark;
                                ProcessedResult = firstYearFirstSemesterResult;

                            }
                            else
                            {
                                List<Result> result = null;
                                Semester firstSemester = new Semester() { Id = (int)Semesters.FirstSemester };
                                if (studentCheck.Activated == true || studentCheck.Activated == null)
                                {
                                    result = studentResultLogic.GetStudentProcessedResultBy(sessionSemester.Session, level, department, student, firstSemester, programme);
                                }
                                else
                                {
                                    result = studentResultLogic.GetDeactivatedStudentProcessedResultBy(sessionSemester.Session, level, department, student, firstSemester, programme);
                                }
                                List<Result> firstSemesterModifiedResultList = new List<Result>();
                                int totalFirstSemesterCourseUnit = 0;
                                for (int i = 0; i < result.Count; i++)
                                {
                                    Result resultItem = result[i];
                                    decimal WGP = 0;
                                    totalFirstSemesterCourseUnit = CheckForSpecialCase(resultItem, totalFirstSemesterCourseUnit, (int)Semesters.FirstSemester);
                                    if (totalFirstSemesterCourseUnit > 0)
                                    {
                                        resultItem.TotalSemesterCourseUnit = totalFirstSemesterCourseUnit;
                                    }
                                    firstSemesterModifiedResultList.Add(resultItem);

                                }


                                decimal firstSemesterGPCUSum = firstSemesterModifiedResultList.Sum(p => p.GPCU) ?? 0;
                                int? firstSemesterTotalSemesterCourseUnit = 0;
                                Result firstYearFirstSemesterResult = firstSemesterModifiedResultList.FirstOrDefault();
                                firstSemesterTotalSemesterCourseUnit = firstSemesterModifiedResultList.Min(p => p.TotalSemesterCourseUnit) ?? 0;
                                decimal firstSemesterGPA = 0M;
                                if (firstSemesterGPCUSum > 0 && firstSemesterTotalSemesterCourseUnit > 0)
                                {
                                    firstSemesterGPA = firstSemesterGPCUSum / firstSemesterTotalSemesterCourseUnit ?? 0;
                                }

                                firstYearFirstSemesterResult = firstYearFirstSemesterResult == null ? new Result() : firstYearFirstSemesterResult;

                                firstYearFirstSemesterResult.GPA = Decimal.Round(firstSemesterGPA, 2);
                                firstYearFirstSemesterResult.CGPA = Decimal.Round(firstSemesterGPA, 2);

                                Semester secondSemester = new Semester() { Id = (int)Semesters.SecondSemester };
                                List<Result> secondSemesterResult = null;
                                if (studentCheck.Activated == true || studentCheck.Activated == null)
                                {
                                    secondSemesterResult = studentResultLogic.GetStudentProcessedResultBy(sessionSemester.Session, level, department, student, secondSemester, programme);
                                }
                                else
                                {
                                    secondSemesterResult = studentResultLogic.GetDeactivatedStudentProcessedResultBy(sessionSemester.Session, level, department, student, secondSemester, programme);
                                }
                                List<Result> secondSemesterModifiedResultList = new List<Result>();

                                int totalSecondSemesterCourseUnit = 0;
                                for (int i = 0; i < secondSemesterResult.Count; i++)
                                {
                                    Result resultItem = secondSemesterResult[i];
                                    decimal WGP = 0;

                                    totalSecondSemesterCourseUnit = CheckForSpecialCase(resultItem, totalSecondSemesterCourseUnit, (int)Semesters.SecondSemester);

                                    if (totalSecondSemesterCourseUnit > 0)
                                    {
                                        resultItem.TotalSemesterCourseUnit = totalSecondSemesterCourseUnit;
                                    }
                                    secondSemesterModifiedResultList.Add(resultItem);
                                }
                                
                                decimal? secondSemesterGPCUSum = secondSemesterModifiedResultList.Sum(p => p.GPCU);
                                Result secondSemesterStudentResult = secondSemesterModifiedResultList.FirstOrDefault();

                                if (secondSemesterGPCUSum > 0)
                                {
                                    secondSemesterStudentResult.GPA = Decimal.Round((decimal)(secondSemesterGPCUSum / (decimal)(secondSemesterModifiedResultList.Min(p => p.TotalSemesterCourseUnit))), 2);
                                }

                                if (firstSemesterGPCUSum > 0 || secondSemesterGPCUSum > 0)
                                {
                                    secondSemesterStudentResult.CGPA = Decimal.Round((decimal)((firstSemesterGPCUSum + secondSemesterGPCUSum) / (secondSemesterModifiedResultList.Min(p => p.TotalSemesterCourseUnit) + firstSemesterTotalSemesterCourseUnit)), 2);

                                }
                                else
                                {
                                    secondSemesterStudentResult.CGPA = secondSemesterStudentResult.GPA;
                                }

                                if (secondSemesterStudentResult.CGPA < (decimal)2.0 && firstYearFirstSemesterResult.GPA < (decimal)2.0)
                                {
                                    Remark = GetGraduationStatus(secondSemesterStudentResult.CGPA, firstYearFirstSemesterResult.CGPA ?? 0, secondSemesterStudentResult.CGPA ?? 0, GetFirstYearCarryOverCourses(sessionSemester, level, programme, department, student));
                                }
                                else
                                {
                                    Remark = GetGraduationStatus(secondSemesterStudentResult.CGPA, GetFirstYearCarryOverCourses(sessionSemester, level, programme, department, student));
                                }

                                secondSemesterStudentResult.Remark = Remark;
                                ProcessedResult = secondSemesterStudentResult;

                            }

                        }
                        else
                        {
                            decimal firstYearFirstSemesterGPCUSum = 0;
                            int firstYearFirstSemesterTotalCourseUnit = 0;
                            decimal firstYearSecondSemesterGPCUSum = 0;
                            int firstYearSecondSemesterTotalCourseUnit = 0;
                            decimal secondYearFirstSemesterGPCUSum = 0;
                            int secondYearFirstSemesterTotalCourseUnit = 0;
                            decimal secondYearSecondSemesterGPCUSum = 0;
                            int secondYearSecondSemesterTotalCourseUnit = 0;

                            Result firstYearFirstSemester = GetFirstYearFirstSemesterResultInfo(sessionSemester, level, programme, department, student);
                            Result firstYearSecondSemester = GetFirstYearSecondSemesterResultInfo(sessionSemester, level, programme, department, student);
                            if (sessionSemester.Semester.Id == (int)Semesters.FirstSemester)
                            {
                                List<Result> result = null;
                                Abundance_Nk.Model.Model.Student studentCheck = studentLogic.GetModelBy(p => p.Person_Id == student.Id);
                                Semester semester = new Semester() { Id = (int)Semesters.FirstSemester };

                                if (student.Activated == true || studentCheck.Activated == null)
                                {
                                    result = studentResultLogic.GetStudentProcessedResultBy(sessionSemester.Session, level, department, student, semester, programme);
                                }
                                else
                                {
                                    result = studentResultLogic.GetDeactivatedStudentProcessedResultBy(sessionSemester.Session, level, department, student, semester, programme);
                                }

                                List<Result> modifiedResultList = new List<Result>();
                                int totalSemesterCourseUnit = 0;

                                for (int i = 0; i < result.Count; i++)
                                {
                                    Result resultItem = result[i];
                                    decimal WGP = 0;
                                    totalSemesterCourseUnit = CheckForSpecialCase(resultItem, totalSemesterCourseUnit, (int)Semesters.FirstSemester);
                                    if (totalSemesterCourseUnit > 0)
                                    {
                                        resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                                    }
                                    modifiedResultList.Add(resultItem);
                                }

                                Result secondYearFirstSemesterResult = new Result();
                                decimal firstSemesterGPCUSum = modifiedResultList.Sum(p => p.GPCU) ?? 0;
                                int secondYearfirstSemesterTotalSemesterCourseUnit = 0;
                                secondYearFirstSemesterResult = modifiedResultList.FirstOrDefault();
                                secondYearfirstSemesterTotalSemesterCourseUnit = modifiedResultList.Min(p => p.TotalSemesterCourseUnit) ?? 0;
                                secondYearFirstSemesterResult.TotalSemesterCourseUnit = secondYearfirstSemesterTotalSemesterCourseUnit;
                                secondYearFirstSemesterResult.GPCU = firstSemesterGPCUSum;

                                decimal firstSemesterGPA = 0M;

                                if (firstSemesterGPCUSum > 0 && secondYearfirstSemesterTotalSemesterCourseUnit > 0)
                                {
                                    firstSemesterGPA = firstSemesterGPCUSum / secondYearfirstSemesterTotalSemesterCourseUnit;
                                }

                                secondYearFirstSemesterResult.GPA = Decimal.Round(firstSemesterGPA, 2);

                                if (firstYearFirstSemester != null && firstYearFirstSemester.GPCU != null && firstYearSecondSemester != null && firstYearSecondSemester.GPCU != null)
                                {
                                    secondYearFirstSemesterResult.CGPA = Decimal.Round((decimal)((firstSemesterGPCUSum + firstYearFirstSemester.GPCU + firstYearSecondSemester.GPCU) / (firstYearSecondSemester.TotalSemesterCourseUnit + firstYearFirstSemester.TotalSemesterCourseUnit + secondYearfirstSemesterTotalSemesterCourseUnit)), 2);
                                }
                                else
                                {
                                    secondYearFirstSemesterResult.CGPA = secondYearFirstSemesterResult.GPA;
                                }

                                List<string> secondYearFirstSemetserCarryOverCourses = GetSecondYearCarryOverCourses(sessionSemester, level, programme, department, student);
                                //secondYearFirstSemesterResult.Remark = GetGraduationStatus(secondYearFirstSemesterResult.CGPA, secondYearFirstSemetserCarryOverCourses);

                                decimal firstYearCGPA = Convert.ToDecimal((firstYearSecondSemester.GPCU + firstYearFirstSemester.GPCU) / (firstYearFirstSemester.TotalSemesterCourseUnit + firstYearSecondSemester.TotalSemesterCourseUnit));
                                secondYearFirstSemesterResult.Remark = GetSecondYearFirstSemeterGraduationStatus(firstYearCGPA, secondYearFirstSemesterResult.CGPA, secondYearFirstSemetserCarryOverCourses);

                                ProcessedResult = secondYearFirstSemesterResult;

                            }
                            else if (sessionSemester.Semester.Id == (int)Semesters.SecondSemester)
                            {

                                List<Result> result = null;
                                Abundance_Nk.Model.Model.Student studentCheck = studentLogic.GetModelBy(p => p.Person_Id == student.Id);
                                Semester semester = new Semester() { Id = (int)Semesters.FirstSemester };

                                if (student.Activated == true || studentCheck.Activated == null)
                                {
                                    result = studentResultLogic.GetStudentProcessedResultBy(sessionSemester.Session, level, department, student, semester, programme);
                                }
                                else
                                {
                                    result = studentResultLogic.GetDeactivatedStudentProcessedResultBy(sessionSemester.Session, level, department, student, semester, programme);
                                }
                                List<Result> modifiedResultList = new List<Result>();
                                int totalSemesterCourseUnit = 0;
                                for (int i = 0; i < result.Count; i++)
                                {
                                    Result resultItem = result[i];
                                    decimal WGP = 0;
                                    totalSemesterCourseUnit = CheckForSpecialCase(resultItem, totalSemesterCourseUnit, (int)Semesters.FirstSemester);
                                    if (totalSemesterCourseUnit > 0)
                                    {
                                        resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                                    }
                                    modifiedResultList.Add(resultItem);
                                }
                                //foreach (Result resultItem in result)
                                //{
                                //    decimal WGP = 0;
                                //    totalSemesterCourseUnit = CheckForSpecialCase(resultItem, totalSemesterCourseUnit, (int)Semesters.FirstSemester);
                                //    if (totalSemesterCourseUnit > 0)
                                //    {
                                //        resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                                //    }
                                //    modifiedResultList.Add(resultItem);
                                //}
                                Result secondYearFirstSemesterResult = new Result();
                                decimal secondYearfirstSemesterGPCUSum = modifiedResultList.Sum(p => p.GPCU) ?? 0;
                                int secondYearfirstSemesterTotalSemesterCourseUnit = 0;
                                secondYearfirstSemesterTotalSemesterCourseUnit = modifiedResultList.Min(p => p.TotalSemesterCourseUnit) ?? 0;
                                secondYearFirstSemesterResult.TotalSemesterCourseUnit = secondYearfirstSemesterTotalSemesterCourseUnit;
                                secondYearFirstSemesterResult.GPCU = secondYearfirstSemesterGPCUSum;
                                decimal firstSemesterGPA = 0M;
                                if (secondYearfirstSemesterGPCUSum > 0 && secondYearfirstSemesterTotalSemesterCourseUnit > 0)
                                {
                                    firstSemesterGPA = secondYearfirstSemesterGPCUSum / secondYearfirstSemesterTotalSemesterCourseUnit;
                                }

                                secondYearFirstSemesterResult.GPA = Decimal.Round(firstSemesterGPA, 2);

                                //Second semester second year

                                List<Result> secondSemesterResult = null;
                                Semester secondSemester = new Semester() { Id = (int)Semesters.SecondSemester };
                                if (student.Activated == true || studentCheck.Activated == null)
                                {
                                    secondSemesterResult = studentResultLogic.GetStudentProcessedResultBy(sessionSemester.Session, level, department, student, secondSemester, programme);
                                }
                                else
                                {
                                    secondSemesterResult = studentResultLogic.GetDeactivatedStudentProcessedResultBy(sessionSemester.Session, level, department, student, secondSemester, programme);
                                }
                                List<Result> secondSemesterModifiedResultList = new List<Result>();
                                int totalSecondSemesterCourseUnit = 0;
                                for (int i = 0; i < secondSemesterResult.Count; i++)
                                {
                                    Result resultItem = secondSemesterResult[i];
                                    decimal WGP = 0;
                                    totalSecondSemesterCourseUnit = CheckForSpecialCase(resultItem, totalSecondSemesterCourseUnit, (int)Semesters.SecondSemester);
                                    if (totalSecondSemesterCourseUnit > 0)
                                    {
                                        resultItem.TotalSemesterCourseUnit = totalSecondSemesterCourseUnit;
                                    }
                                    secondSemesterModifiedResultList.Add(resultItem);
                                }
                                //foreach (Result resultItem in secondSemesterResult)
                                //{
                                //    decimal WGP = 0;
                                //    totalSecondSemesterCourseUnit = CheckForSpecialCase(resultItem, totalSecondSemesterCourseUnit, (int)Semesters.SecondSemester);
                                //    if (totalSecondSemesterCourseUnit > 0)
                                //    {
                                //        resultItem.TotalSemesterCourseUnit = totalSecondSemesterCourseUnit;
                                //    }
                                //    secondSemesterModifiedResultList.Add(resultItem);
                                //}
                                Result secondYearSecondtSemesterResult = new Result();
                                decimal secondYearSecondtSemesterGPCUSum = secondSemesterModifiedResultList.Sum(p => p.GPCU) ?? 0;
                                int secondYearSecondSemesterTotalSemesterCourseUnit = 0;
                                secondYearSecondSemesterTotalSemesterCourseUnit = secondSemesterModifiedResultList.Min(p => p.TotalSemesterCourseUnit) ?? 0;
                                secondYearSecondtSemesterResult = secondSemesterModifiedResultList.FirstOrDefault();
                                secondYearSecondtSemesterResult.TotalSemesterCourseUnit = secondYearSecondSemesterTotalSemesterCourseUnit;
                                secondYearSecondtSemesterResult.GPCU = secondYearSecondtSemesterGPCUSum;
                                decimal secondYearSecondSmesterGPA = 0M;
                                if (secondYearSecondtSemesterGPCUSum > 0 && secondYearSecondSemesterTotalSemesterCourseUnit > 0)
                                {
                                    secondYearSecondSmesterGPA = secondYearSecondtSemesterGPCUSum / secondYearSecondSemesterTotalSemesterCourseUnit;
                                }
                                secondYearSecondtSemesterResult.GPA = Decimal.Round(secondYearSecondSmesterGPA, 2);

                                firstYearFirstSemester.TotalSemesterCourseUnit = firstYearFirstSemester.TotalSemesterCourseUnit ?? 0;
                                firstYearSecondSemester.TotalSemesterCourseUnit = firstYearSecondSemester.TotalSemesterCourseUnit ?? 0;

                                decimal previousCGPA = 0M;

                                secondYearSecondtSemesterResult.CGPA = Decimal.Round((decimal)((secondYearfirstSemesterGPCUSum + firstYearFirstSemester.GPCU + firstYearSecondSemester.GPCU + secondYearSecondtSemesterGPCUSum) / (firstYearSecondSemester.TotalSemesterCourseUnit + firstYearFirstSemester.TotalSemesterCourseUnit + secondYearfirstSemesterTotalSemesterCourseUnit + secondYearSecondSemesterTotalSemesterCourseUnit)), 2);
                                previousCGPA = Decimal.Round((decimal)((secondYearfirstSemesterGPCUSum + firstYearFirstSemester.GPCU + firstYearSecondSemester.GPCU) / (firstYearSecondSemester.TotalSemesterCourseUnit + firstYearFirstSemester.TotalSemesterCourseUnit + secondYearfirstSemesterTotalSemesterCourseUnit)), 2);
                                List<string> secondYearSecondSemetserCarryOverCourses = GetSecondYearCarryOverCourses(sessionSemester, level, programme, department, student);
                                if (secondYearSecondtSemesterResult.CGPA < (decimal)2.0 && previousCGPA < (decimal)2.0)
                                {
                                    secondYearSecondtSemesterResult.Remark = GetGraduationStatus(secondYearSecondtSemesterResult.CGPA, previousCGPA, secondYearSecondtSemesterResult.CGPA, secondYearSecondSemetserCarryOverCourses);
                                }
                                else
                                {
                                    secondYearSecondtSemesterResult.Remark = GetGraduationStatus(secondYearSecondtSemesterResult.CGPA, secondYearSecondSemetserCarryOverCourses);
                                }

                                ProcessedResult = secondYearSecondtSemesterResult;
                            }



                        }
                    }
                }

            }
            catch (Exception)
            {


            }
            return ProcessedResult;
        }
        public Result ViewProcessedStudentResultExtraYear(long PersonId, SessionSemester sessionSemester, Level level, Programme programme, Department department)
        {
            Result ProcessedResult = new Result();
            string Remark = null;
            try
            {
                if (PersonId > 0)
                {
                    if(PersonId== 43648)
                    {
                        var now = 0;
                    }
                    Abundance_Nk.Model.Model.Student student = new Model.Model.Student() { Id = PersonId };
                    StudentLogic studentLogic = new StudentLogic();

                    GetStudent(studentLogic, student);

                    StudentResultLogic studentResultLogic = new StudentResultLogic();
                    if (sessionSemester.Semester != null && sessionSemester.Session != null && programme != null && department != null && level != null)
                    {
                        decimal otherSemesterGPCUSum = 0;
                        int otherSemesterTotalCourseUnit = 0;
                        decimal currentSemesterGPCUSum = 0;
                        int currentSemesterTotalCourseUnit = 0;

                        Result allSemester = GetAllSemesterResultInfo(programme, department, student, sessionSemester);

                        List<Result> result = null;
                        Abundance_Nk.Model.Model.Student studentCheck = studentLogic.GetModelBy(p => p.Person_Id == student.Id);

                        if (student.Activated == true || studentCheck.Activated == null)
                        {
                            result = studentResultLogic.GetStudentProcessedResultBy(sessionSemester.Session, level, department, student, sessionSemester.Semester, programme);
                        }
                        else
                        {
                            result = studentResultLogic.GetDeactivatedStudentProcessedResultBy(sessionSemester.Session, level, department, student, sessionSemester.Semester, programme);
                        }

                        List<Result> modifiedResultList = new List<Result>();
                        int totalSemesterCourseUnit = 0;
                        for (int i = 0; i < result.Count; i++)
                        {
                            Result resultItem = result[i];
                            decimal WGP = 0;
                            totalSemesterCourseUnit = CheckForSpecialCase(resultItem, totalSemesterCourseUnit, (int)Semesters.FirstSemester);
                            if (totalSemesterCourseUnit > 0)
                            {
                                resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                            }

                            modifiedResultList.Add(resultItem);
                        }

                        Result currentSemesterResult = new Result();
                        currentSemesterGPCUSum = modifiedResultList.Sum(p => p.GPCU) ?? 0;
                        int currentSemesterTotalSemesterCourseUnit = 0;
                        currentSemesterTotalSemesterCourseUnit = modifiedResultList.Min(p => p.TotalSemesterCourseUnit) ?? 0;
                        currentSemesterResult.TotalSemesterCourseUnit = currentSemesterTotalSemesterCourseUnit;
                        currentSemesterResult.GPCU = currentSemesterGPCUSum;
                        decimal currentSemesterGPA = 0M;
                        if (currentSemesterGPCUSum > 0 && currentSemesterTotalSemesterCourseUnit > 0)
                        {
                            currentSemesterGPA = currentSemesterGPCUSum / currentSemesterTotalSemesterCourseUnit;
                        }

                        currentSemesterResult.GPA = Decimal.Round(currentSemesterGPA, 2);

                        allSemester.TotalSemesterCourseUnit = allSemester.TotalSemesterCourseUnit ?? 0;
                        allSemester.GPCU = allSemester.GPCU ?? 0;

                        currentSemesterResult.CGPA = allSemester.TotalSemesterCourseUnit>0? Decimal.Round((decimal)(allSemester.GPCU / allSemester.TotalSemesterCourseUnit), 2):0;

                        List<string> currentSemetserCarryOverCourses = GetSecondYearCarryOverCourses(sessionSemester, level, programme, department, student);

                        List<string> coursesWithoutResult = GetCoursesWithoutResult(student, currentSemetserCarryOverCourses);

                        if (coursesWithoutResult.Count > 0 )
                        {
                            currentSemetserCarryOverCourses.AddRange(coursesWithoutResult);
                        }

                        otherSemesterTotalCourseUnit = Convert.ToInt32(allSemester.TotalSemesterCourseUnit) - Convert.ToInt32(result.Sum(r => r.CourseUnit));
                        otherSemesterGPCUSum = Convert.ToDecimal(allSemester.GPCU) - Convert.ToDecimal(result.Sum(r => r.GPCU));

                        decimal otherSemesterCGPA = otherSemesterTotalCourseUnit > 0 ? otherSemesterGPCUSum / otherSemesterTotalCourseUnit : 0;

                        if (otherSemesterCGPA < 2.0M && currentSemesterGPA < 2.0M)
                        {
                            Remark = GetGraduationStatus(currentSemesterGPA, otherSemesterCGPA, currentSemesterGPA, currentSemetserCarryOverCourses);
                        }
                        else
                        {
                            Remark = GetGraduationStatus(currentSemesterResult.CGPA, currentSemetserCarryOverCourses);
                        }

                        currentSemesterResult.Remark = Remark;

                        currentSemesterResult.WGP = allSemester.GPCU;
                        currentSemesterResult.TotalSemesterCourseUnit = allSemester.TotalSemesterCourseUnit;
                        
                        ProcessedResult = currentSemesterResult;
                    }
                }

            }
            catch (Exception)
            {

            }

            return ProcessedResult;
        }

        public decimal? GetOverallCGPA(long PersonId, Programme programme, Department department)
        {
            decimal? cgpa = 0M;
            try
            {
                if (PersonId > 0)
                {
                    Abundance_Nk.Model.Model.Student student = new Model.Model.Student() { Id = PersonId };
                    StudentLogic studentLogic = new StudentLogic();

                    GetStudent(studentLogic, student);

                    if (programme != null && department != null)
                    {
                        decimal allResultGPCUSum = 0;

                        List<Result> result = GetStudentProcessedResultUploaded(department, student, programme);

                        //List<Result> modifiedResultList = new List<Result>();
                        //int totalSemesterCourseUnit = 0;
                        //for (int i = 0; i < result.Count; i++)
                        //{
                        //    Result resultItem = result[i];
                        //    decimal WGP = 0;
                        //    totalSemesterCourseUnit = CheckForSpecialCase(resultItem, totalSemesterCourseUnit, (int)Semesters.FirstSemester);
                        //    if (totalSemesterCourseUnit > 0)
                        //    {
                        //        resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                        //    }

                        //    modifiedResultList.Add(resultItem);
                        //}

                        Result allResult = new Result();
                        allResultGPCUSum = result.Sum(p => p.GPCU) ?? 0;
                        int allResultTotalSemesterCourseUnit = 0;
                        allResultTotalSemesterCourseUnit = result.Sum(p => p.CourseUnit);
                        allResult.TotalSemesterCourseUnit = allResultTotalSemesterCourseUnit;
                        allResult.GPCU = allResultGPCUSum;
                        decimal allResultCGPA = 0M;
                        if (allResultGPCUSum > 0 && allResultTotalSemesterCourseUnit > 0)
                        {
                            allResultCGPA = allResultGPCUSum / allResultTotalSemesterCourseUnit;
                        }

                        allResult.CGPA = Decimal.Round(allResultCGPA, 2);

                        cgpa = allResult.CGPA;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return cgpa;
        }

        public decimal? GetOverallCGPAForLevel(long PersonId, Programme programme, Department department, Level level,SessionSemester session)
        {
            decimal? cgpa = 0M;
            try
            {
                if (PersonId > 0)
                {
                    Abundance_Nk.Model.Model.Student student = new Model.Model.Student() { Id = PersonId };
                    StudentLogic studentLogic = new StudentLogic();

                    GetStudent(studentLogic, student);

                    if (programme != null && department != null)
                    {
                        decimal allResultGPCUSum = 0;

                        List<Result> result = GetProcessedResutBy(session.Session, session.Semester,level,department,programme);

                        //List<Result> modifiedResultList = new List<Result>();
                        //int totalSemesterCourseUnit = 0;
                        //for (int i = 0; i < result.Count; i++)
                        //{
                        //    Result resultItem = result[i];
                        //    decimal WGP = 0;
                        //    totalSemesterCourseUnit = CheckForSpecialCase(resultItem, totalSemesterCourseUnit, (int)Semesters.FirstSemester);
                        //    if (totalSemesterCourseUnit > 0)
                        //    {
                        //        resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                        //    }

                        //    modifiedResultList.Add(resultItem);
                        //}

                        Result allResult = new Result();
                        allResultGPCUSum = result.Sum(p => p.GPCU) ?? 0;
                        int allResultTotalSemesterCourseUnit = 0;
                        allResultTotalSemesterCourseUnit = result.Sum(p => p.CourseUnit);
                        allResult.TotalSemesterCourseUnit = allResultTotalSemesterCourseUnit;
                        allResult.GPCU = allResultGPCUSum;
                        decimal allResultCGPA = 0M;
                        if (allResultGPCUSum > 0 && allResultTotalSemesterCourseUnit > 0)
                        {
                            allResultCGPA = allResultGPCUSum / allResultTotalSemesterCourseUnit;
                        }

                        allResult.CGPA = Decimal.Round(allResultCGPA, 2);

                        cgpa = allResult.CGPA;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return cgpa;
        }

        private List<string> GetCoursesWithoutResult(Student student, List<string> carryOverCourses)
        {
            List<string> courseList = new List<string>();
            try
            {
                if (student != null)
                {
                    CourseRegistrationDetailLogic courseRegistrationDetailLogic = new CourseRegistrationDetailLogic();

                    List<CourseRegistrationDetail> courseRegistrationDetailsWithout = courseRegistrationDetailLogic.GetModelsBy(c => c.STUDENT_COURSE_REGISTRATION.STUDENT.Person_Id == student.Id && c.Test_Score == null && c.Exam_Score == null);
                    for (int i = 0; i < courseRegistrationDetailsWithout.Count; i++)
                    {
                        CourseRegistrationDetail courseRegistrationDetail = courseRegistrationDetailsWithout[i];

                        if (carryOverCourses.Contains(courseRegistrationDetail.Course.Code))
                        {
                            continue;
                        }

                        CourseRegistrationDetail passedCourse = courseRegistrationDetailLogic.GetModelBy(c => c.STUDENT_COURSE_REGISTRATION.STUDENT.Person_Id == student.Id && c.Course_Id == courseRegistrationDetail.Course.Id && (c.Test_Score + c.Exam_Score) >= 40);
                        if (passedCourse == null && courseRegistrationDetail.Course.Unit > 0)
                        {
                            TotalUnitsOmitted = TotalUnitsOmitted + Convert.ToInt32(courseRegistrationDetailsWithout[i].CourseUnit);
                            courseList.Add(courseRegistrationDetailsWithout[i].Course.Code);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return courseList;
        }
        private void GetStudent(StudentLogic studentLogic, Student student)
        {
            try
            {
                STUDENT studentEntity = studentLogic.GetEntityBy(s => s.Person_Id == student.Id);
                student.Activated = studentEntity.Activated;
                student.ApplicationForm = new ApplicationForm() { Id = studentEntity.Application_Form_Id ?? 0 };
                student.BloodGroup = new BloodGroup() { Id = studentEntity.Blood_Group_Id ?? 0 };
                student.Category = new StudentCategory() { Id = studentEntity.Student_Category_Id };
                student.Genotype = new Genotype() { Id = studentEntity.Genotype_Id ?? 0 };
                student.MaritalStatus = new MaritalStatus() { Id = studentEntity.Marital_Status_Id ?? 0 };
                student.MatricNumber = studentEntity.Matric_Number;
                student.Number = studentEntity.Student_Number;
                student.Reason = studentEntity.Reason;
                student.RejectCategory = studentEntity.Reject_Category;
                student.SchoolContactAddress = studentEntity.School_Contact_Address;
                student.Status = new StudentStatus() { Id = studentEntity.Student_Status_Id };
                student.Title = new Title() { Id = studentEntity.Title_Id ?? 0 };
                student.Type = new StudentType() { Id = studentEntity.Student_Type_Id };
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static int CheckForSpecialCase(Result resultItem, int totalSemesterCourseUnit, int semesterId)
        {
            if (resultItem.SpecialCase != null)
            {
                resultItem.GPCU = 0;
                if (totalSemesterCourseUnit == 0)
                {
                    totalSemesterCourseUnit = (int)resultItem.TotalSemesterCourseUnit - resultItem.CourseUnit;
                    resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                    if (resultItem.UnitOutstanding == null)
                    {
                        resultItem.UnitOutstanding = resultItem.CourseUnit;
                    }
                    else
                    {
                        resultItem.UnitOutstanding += resultItem.CourseUnit;
                    }
                    //resultItem.Grade = "-";
                    if (resultItem.SpecialCase == "SICK")
                    {
                        resultItem.Grade = "S";
                    }
                    if (resultItem.SpecialCase == "ABSENT")
                    {
                        resultItem.Grade = "ABS";
                    }
                }
                else
                {
                    totalSemesterCourseUnit -= resultItem.CourseUnit;
                    resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                    //resultItem.UnitOutstanding += resultItem.CourseUnit;
                    if (resultItem.UnitOutstanding == null)
                    {
                        resultItem.UnitOutstanding = resultItem.CourseUnit;
                    }
                    else
                    {
                        resultItem.UnitOutstanding += resultItem.CourseUnit;
                    }
                    //resultItem.UnitOutstanding += resultItem.CourseUnit;
                    //resultItem.Grade = "-";
                    if (resultItem.SpecialCase == "SICK")
                    {
                        resultItem.Grade = "S";
                    }
                    if (resultItem.SpecialCase == "ABSENT")
                    {
                        resultItem.Grade = "ABS";
                    }
                }

            }

            //Check for deferment
            StudentDefermentLogic defermentLogic = new StudentDefermentLogic();
            if (defermentLogic.isStudentDefered(resultItem))
            {
                resultItem.GPCU = 0;
                if (totalSemesterCourseUnit == 0)
                {
                    totalSemesterCourseUnit = (int)resultItem.TotalSemesterCourseUnit - resultItem.CourseUnit;
                    resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;

                    resultItem.UnitOutstanding += resultItem.CourseUnit;
                    resultItem.Grade = "-";
                    resultItem.GPA = null;
                    resultItem.CGPA = null;
                    //resultItem.CourseUnit = 0;
                    resultItem.Remark = "DEFERMENT";
                }
                else
                {
                    totalSemesterCourseUnit -= resultItem.CourseUnit;
                    resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;

                    resultItem.UnitOutstanding += resultItem.CourseUnit;
                    resultItem.Grade = "-";
                    resultItem.GPA = null;
                    resultItem.CGPA = null;
                    //resultItem.CourseUnit = 0;
                    resultItem.Remark = "DEFERMENT";
                }
            }

            //Check for rustication
            if (defermentLogic.isStudentRusticated(resultItem, semesterId))
            {
                resultItem.GPCU = 0;
                if (totalSemesterCourseUnit == 0)
                {
                    totalSemesterCourseUnit = (int)resultItem.TotalSemesterCourseUnit - resultItem.CourseUnit;
                    resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;

                    resultItem.UnitOutstanding += resultItem.CourseUnit;
                    resultItem.Grade = "-";
                    resultItem.GPA = null;
                    resultItem.CGPA = null;
                    //resultItem.CourseUnit = 0;
                    resultItem.Remark = "RUSTICATION";
                }
                else
                {
                    totalSemesterCourseUnit -= resultItem.CourseUnit;
                    resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;

                    resultItem.UnitOutstanding += resultItem.CourseUnit;
                    resultItem.Grade = "-";
                    resultItem.GPA = null;
                    resultItem.CGPA = null;
                    //resultItem.CourseUnit = 0;
                    resultItem.Remark = "RUSTICATION";
                }
            }

            return totalSemesterCourseUnit;
        }

        public int TotalUnitsOmitted { get; set; }
        public List<string> GetFirstYearCarryOverCourses(SessionSemester sessionSemester, Level lvl, Programme programme, Department department, Student student)
        {
            try
            {
                List<CourseRegistrationDetail> courseRegistrationdetails = new List<CourseRegistrationDetail>();
                CourseRegistrationDetailLogic courseRegistrationDetailLogic = new CourseRegistrationDetailLogic();
                List<string> courseCodes = new List<string>();
                if (lvl.Id == (int)Levels.NDI || lvl.Id == (int)Levels.HNDI)
                {
                    List<CourseRegistrationDetail> nullCourseRegistrationDetails = new List<CourseRegistrationDetail>();
                    List<CourseRegistrationDetail> nullCourseRegistrationDetailsLessThanPassMark = new List<CourseRegistrationDetail>();

                    nullCourseRegistrationDetails = courseRegistrationDetailLogic.GetModelsBy(crd => crd.STUDENT_COURSE_REGISTRATION.Session_Id == sessionSemester.Session.Id &&
                        crd.STUDENT_COURSE_REGISTRATION.Department_Id == department.Id && crd.STUDENT_COURSE_REGISTRATION.Programme_Id == programme.Id &&
                        crd.STUDENT_COURSE_REGISTRATION.Person_Id == student.Id && (crd.Test_Score == null || crd.Exam_Score == null) && crd.Special_Case == null);
                    if (nullCourseRegistrationDetails.Count > 0)
                    {
                        for (int i = 0; i < nullCourseRegistrationDetails.Count; i++)
                        {
                            if (nullCourseRegistrationDetails[i].ExamScore == null && nullCourseRegistrationDetails[i].TestScore < (int)Grades.PassMark)
                            {
                                nullCourseRegistrationDetailsLessThanPassMark.Add(nullCourseRegistrationDetails[i]);
                            }
                            else if (nullCourseRegistrationDetails[i].TestScore == null && nullCourseRegistrationDetails[i].ExamScore < (int)Grades.PassMark)
                            {
                                nullCourseRegistrationDetailsLessThanPassMark.Add(nullCourseRegistrationDetails[i]);
                            }
                        }
                    }

                    courseRegistrationdetails = courseRegistrationDetailLogic.GetModelsBy(crd => crd.STUDENT_COURSE_REGISTRATION.Session_Id == sessionSemester.Session.Id &&
                        crd.STUDENT_COURSE_REGISTRATION.Department_Id == department.Id && crd.STUDENT_COURSE_REGISTRATION.Programme_Id == programme.Id &&
                        crd.STUDENT_COURSE_REGISTRATION.Person_Id == student.Id && (crd.Test_Score + crd.Exam_Score) < (int)Grades.PassMark);
                    if (nullCourseRegistrationDetailsLessThanPassMark.Count > 0)
                    {
                        courseRegistrationdetails.AddRange(nullCourseRegistrationDetailsLessThanPassMark);
                    }

                    if (sessionSemester.Semester.Id == (int)Semesters.FirstSemester)
                    {
                        courseRegistrationdetails = courseRegistrationdetails.Where(p => p.Semester.Id == (int)Semesters.FirstSemester).ToList();
                        if (courseRegistrationdetails.Count > 0)
                        {
                            for (int i = 0; i < courseRegistrationdetails.Count; i++)
                            {
                                CourseRegistrationDetail courseRegistrationDetail = courseRegistrationdetails[i];
                                //if (courseRegistrationDetail.SpecialCase != null)
                                //{
                                    courseCodes.Add(courseRegistrationDetail.Course.Code);
                                    TotalUnitsOmitted += courseRegistrationDetail.Course.Unit;
                               // }
                            }
                        }
                    }
                    else
                    {
                        if (courseRegistrationdetails.Count > 0)
                        {
                            for (int i = 0; i < courseRegistrationdetails.Count; i++)
                            {
                                CourseRegistrationDetail courseRegistrationDetail = courseRegistrationdetails[i];
                                //if (courseRegistrationDetail.SpecialCase == null)
                                //{
                                    courseCodes.Add(courseRegistrationDetail.Course.Code);
                                    TotalUnitsOmitted += courseRegistrationDetail.Course.Unit;
                                //}
                            }
                        }
                    }

                }
                //Ensure all passed carried over course code are removed from the carryoverCourse List
                if (courseCodes.Count > 0)
                {
                    List<string> PassedCarriedOverCourseCode = new List<string>();

                    var AllpassedCourseCode = GetStudentAllPassedCourseCode(department, student, programme);
                    for (int i = 0; i < courseCodes.Count; i++)
                    {
                        var code = courseCodes[i];
                        if (AllpassedCourseCode.Contains(code))
                        {

                            PassedCarriedOverCourseCode.Add(code);
                            TotalUnitsOmitted -= courseRegistrationdetails.Where(f => f.Course.Code == code).Select(f => f.Course.Unit).FirstOrDefault();
                            
                        }
                    }
                    foreach (var item in PassedCarriedOverCourseCode) courseCodes.Remove(item);
                }
                return courseCodes;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public List<string> GetOtherCarryOverCourses(SessionSemester sessionSemester, Level lvl, Programme programme, Department department, Student student)
        {
            try
            {
                List<CourseRegistrationDetail> courseRegistrationdetails = new List<CourseRegistrationDetail>();
                CourseRegistrationDetailLogic courseRegistrationDetailLogic = new CourseRegistrationDetailLogic();
                List<string> courseCodes = new List<string>();

                List<CourseRegistrationDetail> nullCourseRegistrationDetails = new List<CourseRegistrationDetail>();
                List<CourseRegistrationDetail> nullCourseRegistrationDetailsLessThanPassMark = new List<CourseRegistrationDetail>();

                nullCourseRegistrationDetails = courseRegistrationDetailLogic.GetModelsBy(crd => crd.STUDENT_COURSE_REGISTRATION.Session_Id == sessionSemester.Session.Id &&
                    crd.STUDENT_COURSE_REGISTRATION.Department_Id == department.Id && crd.STUDENT_COURSE_REGISTRATION.Programme_Id == programme.Id &&
                    crd.STUDENT_COURSE_REGISTRATION.Person_Id == student.Id && (crd.Test_Score == null || crd.Exam_Score == null) && crd.Special_Case == null);
                if (nullCourseRegistrationDetails.Count > 0)
                {
                    for (int i = 0; i < nullCourseRegistrationDetails.Count; i++)
                    {
                        if (nullCourseRegistrationDetails[i].ExamScore == null && nullCourseRegistrationDetails[i].TestScore < (int)Grades.PassMark)
                        {
                            nullCourseRegistrationDetailsLessThanPassMark.Add(nullCourseRegistrationDetails[i]);
                        }
                        else if (nullCourseRegistrationDetails[i].TestScore == null && nullCourseRegistrationDetails[i].ExamScore < (int)Grades.PassMark)
                        {
                            nullCourseRegistrationDetailsLessThanPassMark.Add(nullCourseRegistrationDetails[i]);
                        }
                    }
                }

                courseRegistrationdetails = courseRegistrationDetailLogic.GetModelsBy(crd => crd.STUDENT_COURSE_REGISTRATION.Session_Id == sessionSemester.Session.Id &&
                    crd.STUDENT_COURSE_REGISTRATION.Department_Id == department.Id && crd.STUDENT_COURSE_REGISTRATION.Programme_Id == programme.Id &&
                    crd.STUDENT_COURSE_REGISTRATION.Person_Id == student.Id && (crd.Test_Score + crd.Exam_Score) < (int)Grades.PassMark);
                if (nullCourseRegistrationDetailsLessThanPassMark.Count > 0)
                {
                    courseRegistrationdetails.AddRange(nullCourseRegistrationDetailsLessThanPassMark);
                }

                if (sessionSemester.Semester.Id == (int)Semesters.FirstSemester)
                {
                    courseRegistrationdetails = courseRegistrationdetails.Where(p => p.Semester.Id == (int)Semesters.FirstSemester).ToList();
                    if (courseRegistrationdetails.Count > 0)
                    {
                        for (int i = 0; i < courseRegistrationdetails.Count; i++)
                        {
                            CourseRegistrationDetail courseRegistrationDetail = courseRegistrationdetails[i];
                            //if (courseRegistrationDetail.SpecialCase == null)
                            //{
                                courseCodes.Add(courseRegistrationDetail.Course.Code);
                                TotalUnitsOmitted += courseRegistrationDetail.Course.Unit;
                            //}
                        }
                    }
                }
                else
                {
                    if (courseRegistrationdetails.Count > 0)
                    {
                        for (int i = 0; i < courseRegistrationdetails.Count; i++)
                        {
                            CourseRegistrationDetail courseRegistrationDetail = courseRegistrationdetails[i];
                            //if (courseRegistrationDetail.SpecialCase == null)
                           // {
                                courseCodes.Add(courseRegistrationDetail.Course.Code);
                                TotalUnitsOmitted += courseRegistrationDetail.Course.Unit;
                            //}
                        }
                    }
                }
                //Ensure all passed carried over course code are removed from the carryoverCourse List
                if (courseCodes.Count > 0)
                {
                    List<string> PassedCarriedOverCourseCode = new List<string>();

                    var AllpassedCourseCode = GetStudentAllPassedCourseCode(department, student, programme);
                    for (int i = 0; i < courseCodes.Count; i++)
                    {
                        var code = courseCodes[i];
                        if (AllpassedCourseCode.Contains(code))
                        {

                            PassedCarriedOverCourseCode.Add(code);
                            TotalUnitsOmitted -= courseRegistrationdetails.Where(f => f.Course.Code == code).Select(f => f.Course.Unit).FirstOrDefault();
                        }
                    }
                    foreach (var item in PassedCarriedOverCourseCode) courseCodes.Remove(item);
                }

                return courseCodes;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public List<string> GetSecondYearCarryOverCourses(SessionSemester sessionSemester, Level lvl, Programme programme, Department department, Student student)
        {
            try
            {
                List<CourseRegistrationDetail> courseRegistrationdetails = new List<CourseRegistrationDetail>();
                CourseRegistrationDetailLogic courseRegistrationDetailLogic = new CourseRegistrationDetailLogic();
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                List<string> courseCodes = courseCodes = new List<string>();
                List<string> firstYearCarryOverCourseCodes = null;
                StudentLevel studentLevel = null;
                if (lvl.Id == 2)
                {
                    //studentLevel = studentLevelLogic.GetModelsBy(p => p.Person_Id == student.Id && p.Level_Id == 1 && p.Department_Id == department.Id && p.Programme_Id == programme.Id).FirstOrDefault();//ND1
                    //GetStudentLevel(studentLevelLogic, studentLevel, student, new Level(){Id = 1}, department, programme );
                    STUDENT_LEVEL studentLevelEntity = studentLevelLogic.GetEntitiesBy(p => p.Person_Id == student.Id && p.Level_Id == 1 && p.Department_Id == department.Id && p.Programme_Id == programme.Id).FirstOrDefault();
                    if (studentLevelEntity != null)
                    {
                        studentLevel = new StudentLevel();
                        studentLevel.Id = studentLevelEntity.Student_Level_Id;
                        studentLevel.Department = department;
                        if (studentLevelEntity.Department_Option_Id != null && studentLevelEntity.Department_Option_Id > 0)
                        {
                            studentLevel.DepartmentOption = new DepartmentOption() { Id = studentLevelEntity.Department_Option_Id ?? 0 };
                        }
                        else
                        {
                            studentLevel.DepartmentOption = null;
                        }

                        studentLevel.Level = new Level() { Id = 1 };
                        studentLevel.Programme = programme;
                        studentLevel.Session = new Session() { Id = studentLevelEntity.Session_Id };
                        studentLevel.Student = student;
                    }
                    if (studentLevel != null)
                    {
                        SessionSemester ss = new SessionSemester();
                        ss.Session = studentLevel.Session;
                        ss.Semester = new Semester() { Id = 2 };// Second semester to get all carry over for first year
                        firstYearCarryOverCourseCodes = GetFirstYearCarryOverCourses(ss, studentLevel.Level, studentLevel.Programme, studentLevel.Department, studentLevel.Student);
                    }
                }
                else if (lvl.Id == 4)
                {
                    //studentLevel = studentLevelLogic.GetModelsBy(p => p.Person_Id == student.Id && p.Level_Id == 3 && p.Department_Id == department.Id && p.Programme_Id == programme.Id).FirstOrDefault(); //HND1
                    STUDENT_LEVEL studentLevelEntity = studentLevelLogic.GetEntitiesBy(p => p.Person_Id == student.Id && p.Level_Id == 3 && p.Department_Id == department.Id && p.Programme_Id == programme.Id).FirstOrDefault();
                    if (studentLevelEntity != null)
                    {
                        studentLevel = new StudentLevel();
                        studentLevel.Id = studentLevelEntity.Student_Level_Id;
                        studentLevel.Department = department;
                        if (studentLevelEntity.Department_Option_Id != null && studentLevelEntity.Department_Option_Id > 0)
                        {
                            studentLevel.DepartmentOption = new DepartmentOption() { Id = studentLevelEntity.Department_Option_Id ?? 0 };
                        }
                        else
                        {
                            studentLevel.DepartmentOption = null;
                        }

                        studentLevel.Level = new Level() { Id = 3 };
                        studentLevel.Programme = programme;
                        studentLevel.Session = new Session() { Id = studentLevelEntity.Session_Id };
                        studentLevel.Student = student;
                    }
                    if (studentLevel != null)
                    {
                        SessionSemester ss = new SessionSemester();
                        ss.Session = studentLevel.Session;
                        ss.Semester = new Semester() { Id = 2 }; // Second semester to get all carry over for first year
                        firstYearCarryOverCourseCodes = GetFirstYearCarryOverCourses(ss, studentLevel.Level, studentLevel.Programme, studentLevel.Department, studentLevel.Student);
                    }
                }

                //Check other session, for extra year students
                if (sessionSemester != null && sessionSemester.Session != null && !string.IsNullOrEmpty(sessionSemester.Session.Name))
                {
                    StudentLogic studentLogic = new StudentLogic();
                    student = !string.IsNullOrEmpty(student.MatricNumber) ? student : studentLogic.GetModelBy(s => s.Person_Id == student.Id);
                    string[] sessionItems = sessionSemester.Session.Name.Split('/');
                    string sessionNameStr = sessionItems[0];

                    string currentSessionSuffix = "/" + sessionNameStr.Substring(2, 2) + "/";
                    string yearTwoSessionSuffix = "/" + Convert.ToString((Convert.ToInt32(sessionNameStr.Substring(2, 2)) - 1)) + "/";

                    if (student.MatricNumber.Contains(currentSessionSuffix) || student.MatricNumber.Contains(yearTwoSessionSuffix))
                    {
                        //do nothing
                    }
                    else
                    {
                        int[] yearOneLevels = { (int)Levels.NDI, (int)Levels.HNDI };
                        List<StudentLevel> otherStudentLevels = studentLevelLogic.GetModelsBy(s => s.Person_Id == student.Id && !yearOneLevels.Contains(s.Level_Id) &&
                            s.Session_Id != sessionSemester.Session.Id);

                        Semester firstSemester = new Semester() { Id = 1 };
                        Semester secondSemester = new Semester() { Id = 2 };

                        List<string> otherCourseCodesCompared = new List<string>();

                        for (int i = 0; i < otherStudentLevels.Count; i++)
                        {
                            StudentLevel otherStudentLevel = otherStudentLevels[i];
                            List<string> otherCourseCodes = new List<string>();

                            List<string> otherCourseCodesFirstSemester =
                                GetOtherCarryOverCourses(
                                    new SessionSemester()
                                    {
                                        Semester = firstSemester,
                                        Session = otherStudentLevel.Session
                                    }, otherStudentLevel.Level,
                                    otherStudentLevel.Programme, otherStudentLevel.Department, student);

                            List<string> otherCourseCodesSecondSemester =
                                GetOtherCarryOverCourses(
                                    new SessionSemester()
                                    {
                                        Semester = secondSemester,
                                        Session = otherStudentLevel.Session
                                    }, otherStudentLevel.Level,
                                    otherStudentLevel.Programme, otherStudentLevel.Department, student);

                            if (otherCourseCodesFirstSemester.Any())
                            {
                                otherCourseCodes.AddRange(otherCourseCodesFirstSemester);
                            }
                            if (otherCourseCodesSecondSemester.Any())
                            {
                                otherCourseCodes.AddRange(otherCourseCodesSecondSemester);
                            }

                            List<string> otherCourseCodesComparedToAdd =
                                CompareCourses(otherCourseCodes, firstYearCarryOverCourseCodes, new SessionSemester() { Session = otherStudentLevel.Session }, lvl, programme, department, student);
                            if(otherCourseCodesComparedToAdd != null && otherCourseCodesComparedToAdd .Count > 0 ){
                                otherCourseCodesCompared.AddRange(otherCourseCodesComparedToAdd);
                            }
                        }

                        firstYearCarryOverCourseCodes = otherCourseCodesCompared;
                    }
                }


                if (lvl.Id == 2 || lvl.Id == 4)
                {
                    List<CourseRegistrationDetail> nullCourseRegistrationDetails = new List<CourseRegistrationDetail>();
                    List<CourseRegistrationDetail> nullCourseRegistrationDetailsLessThanPassMark = new List<CourseRegistrationDetail>();

                    nullCourseRegistrationDetails = courseRegistrationDetailLogic.GetModelsBy(crd => crd.STUDENT_COURSE_REGISTRATION.Session_Id == sessionSemester.Session.Id &&
                        crd.STUDENT_COURSE_REGISTRATION.Department_Id == department.Id && crd.STUDENT_COURSE_REGISTRATION.Programme_Id == programme.Id &&
                        crd.STUDENT_COURSE_REGISTRATION.Person_Id == student.Id && (crd.Test_Score == null || crd.Exam_Score == null) && crd.Special_Case == null);
                    if (nullCourseRegistrationDetails.Count > 0)
                    {
                        for (int i = 0; i < nullCourseRegistrationDetails.Count; i++)
                        {
                            if (nullCourseRegistrationDetails[i].ExamScore == null && nullCourseRegistrationDetails[i].TestScore < (int)Grades.PassMark)
                            {
                                nullCourseRegistrationDetailsLessThanPassMark.Add(nullCourseRegistrationDetails[i]);
                            }
                            else if (nullCourseRegistrationDetails[i].TestScore == null && nullCourseRegistrationDetails[i].ExamScore < (int)Grades.PassMark)
                            {
                                nullCourseRegistrationDetailsLessThanPassMark.Add(nullCourseRegistrationDetails[i]);
                            }
                        }
                    }

                    courseRegistrationdetails = courseRegistrationDetailLogic.GetModelsBy(crd => crd.STUDENT_COURSE_REGISTRATION.Session_Id == sessionSemester.Session.Id &&
                        crd.STUDENT_COURSE_REGISTRATION.Department_Id == department.Id && crd.STUDENT_COURSE_REGISTRATION.Programme_Id == programme.Id &&
                        crd.STUDENT_COURSE_REGISTRATION.Person_Id == student.Id && (crd.Test_Score + crd.Exam_Score) < (int)Grades.PassMark);
                    //crd.STUDENT_COURSE_REGISTRATION.Person_Id == student.Id && (crd.Test_Score + crd.Exam_Score) < (int)Grades.PassMark && crd.Special_Case == null);
                    if (nullCourseRegistrationDetailsLessThanPassMark.Count > 0)
                    {
                        courseRegistrationdetails.AddRange(nullCourseRegistrationDetailsLessThanPassMark);
                    }

                    if (sessionSemester.Semester.Id == 1)
                    {
                        courseRegistrationdetails = courseRegistrationdetails.Where(p => p.Semester.Id == 1).ToList();
                        if (courseRegistrationdetails.Count > 0)
                        {
                            for (int i = 0; i < courseRegistrationdetails.Count; i++)
                            {
                                CourseRegistrationDetail courseRegistrationDetail = courseRegistrationdetails[i];
                               // if (courseRegistrationDetail.SpecialCase == null)
                               // {
                                    courseCodes.Add(courseRegistrationDetail.Course.Code);
                                    TotalUnitsOmitted += courseRegistrationDetail.Course.Unit;
                               // }
                            }
                          
                        }
                    }
                    else
                    {
                        if (courseRegistrationdetails.Count > 0)
                        {
                            for (int i = 0; i < courseRegistrationdetails.Count; i++)
                            {
                                CourseRegistrationDetail courseRegistrationDetail = courseRegistrationdetails[i];
                                //if (courseRegistrationDetail.SpecialCase != null)
                               // {
                                    courseCodes.Add(courseRegistrationDetail.Course.Code);
                                    TotalUnitsOmitted += courseRegistrationDetail.Course.Unit;
                                //}
                            }
                           
                        }
                    }

                }
                //compare courses
                courseCodes = CompareCourses(courseCodes, firstYearCarryOverCourseCodes, sessionSemester, lvl, programme, department, student);
                //Ensure all passed carried over course code are removed from the carryoverCourse List
                if (courseCodes.Count > 0)
                {
                    List<string> PassedCarriedOverCourseCode = new List<string>();

                    var AllpassedCourseCode = GetStudentAllPassedCourseCode(department, student, programme);
                    for (int i = 0; i < courseCodes.Count; i++)
                    {
                        var code = courseCodes[i];
                        if (AllpassedCourseCode.Contains(code))
                        {

                            PassedCarriedOverCourseCode.Add(code);
                            TotalUnitsOmitted -= courseRegistrationdetails.Where(f => f.Course.Code == code).Select(f => f.Course.Unit).FirstOrDefault();
                        }
                    }
                    foreach (var item in PassedCarriedOverCourseCode) courseCodes.Remove(item);
                }
                
                
                return courseCodes;
            }
            catch (Exception)
            {

                throw;
            }
        }
        private List<string> CompareCourses(List<string> courseCodes, List<string> firstYearCarryOverCourseCodes, SessionSemester sessionSemester, Level lvl, Programme programme, Department department, Student student)
        {

            try
            {
                CourseRegistrationDetailLogic courseRegistrationDetail = new CourseRegistrationDetailLogic();

                if (courseCodes != null && firstYearCarryOverCourseCodes != null)
                {
                    int firstYearCount = firstYearCarryOverCourseCodes.Count;
                    for (int i = 0; i < firstYearCount; i++)
                    {
                        if (courseCodes.Contains(firstYearCarryOverCourseCodes[i]))
                        {
                            string code = firstYearCarryOverCourseCodes[i];
                            //courseCodes.Add(firstYearCarryOverCourseCodes[i]);
                            //firstYearCarryOverCourseCodes.RemoveAt(i);
                            STUDENT_COURSE_REGISTRATION_DETAIL course = courseRegistrationDetail.GetEntitiesBy(p => p.COURSE.Course_Code == code && p.STUDENT_COURSE_REGISTRATION.Person_Id == student.Id
                                && p.STUDENT_COURSE_REGISTRATION.Session_Id == sessionSemester.Session.Id).LastOrDefault();
                            if (course != null)
                            {
                                TotalUnitsOmitted -= course.Course_Unit ?? 0;
                            }
                        }
                        else
                        {
                            string Coursecode = firstYearCarryOverCourseCodes[i];
                            CourseRegistrationDetail course = courseRegistrationDetail.GetModelsBy(p => p.COURSE.Course_Code == Coursecode && p.STUDENT_COURSE_REGISTRATION.Person_Id == student.Id &&
                                p.STUDENT_COURSE_REGISTRATION.Session_Id == sessionSemester.Session.Id &&  (p.Test_Score + p.Exam_Score) >= (int)Grades.PassMark ).LastOrDefault();
                            if (course != null)
                            {
                                int courseUnitToRemove = course.CourseUnit ?? 0;
                                TotalUnitsOmitted -= courseUnitToRemove;
                                //firstYearCarryOverCourseCodes.RemoveAt(i);
                                TotalUnitsOmitted = TotalUnitsOmitted < 0 ? 0 : TotalUnitsOmitted;
                            }
                            else
                            {
                                courseCodes.Add(firstYearCarryOverCourseCodes[i]);
                            }
                        }
                    }

                }
            }
            catch (Exception)
            {

                throw;
            }
            return courseCodes;
        }
        private Result GetFirstYearFirstSemesterResultInfo(SessionSemester sessionSemester, Level lvl, Programme programme, Department department, Model.Model.Student student)
        {
            try
            {
                List<Result> result = null;
                Result firstYearFirstSemesterResult = new Result();
                StudentLogic studentLogic = new StudentLogic();
                StudentResultLogic studentResultLogic = new StudentResultLogic();
                Abundance_Nk.Model.Model.Student studentCheck = new Student() { Id = student.Id };
                //Abundance_Nk.Model.Model.Student studentCheck = studentLogic.GetModelBy(p => p.Person_Id == student.Id);
                GetStudent(studentLogic, studentCheck);

                Semester semester = new Semester() { Id = 1 };
                Level level = null;
                if (lvl.Id == 2)
                {
                    level = new Level() { Id = 1 };
                }
                else
                {
                    level = new Level() { Id = 3 };
                }
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                StudentLevel studentLevel = null;
                //StudentLevel studentLevel = studentLevelLogic.GetModelsBy(p => p.Person_Id == student.Id && p.Level_Id == level.Id && p.Department_Id == department.Id && p.Programme_Id == programme.Id).FirstOrDefault();
                studentLevel = GetStudentLevel(studentLevelLogic, studentLevel, student, level, department, programme);
                if (studentLevel != null && studentLevel.Session != null)
                {
                    if (student.Activated == true || studentCheck.Activated == null)
                    {
                        result = studentResultLogic.GetStudentProcessedResultBy(studentLevel.Session, level,
                            studentLevel.Department, student, semester, studentLevel.Programme);
                    }
                    else
                    {
                        result = studentResultLogic.GetDeactivatedStudentProcessedResultBy(studentLevel.Session, level,
                            studentLevel.Department, student, semester, studentLevel.Programme);
                    }


                    List<Result> modifiedResultList = new List<Result>();
                    int totalSemesterCourseUnit = 0;
                    foreach (Result resultItem in result)
                    {
                        decimal WGP = 0;
                        if (resultItem.SpecialCase != null)
                        {

                            resultItem.GPCU = 0;
                            if (totalSemesterCourseUnit == 0)
                            {
                                totalSemesterCourseUnit = (int)resultItem.TotalSemesterCourseUnit - resultItem.CourseUnit;
                                resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                                resultItem.Grade = "-";
                            }
                            else
                            {
                                totalSemesterCourseUnit -= resultItem.CourseUnit;
                                resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                                resultItem.Grade = "-";
                            }

                        }
                        if (totalSemesterCourseUnit > 0)
                        {
                            resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                        }
                        modifiedResultList.Add(resultItem);
                    }

                    decimal? firstSemesterGPCUSum = modifiedResultList.Sum(p => p.GPCU);
                    int? firstSemesterTotalSemesterCourseUnit = 0;
                    firstSemesterTotalSemesterCourseUnit = modifiedResultList.Min(p => p.TotalSemesterCourseUnit);
                    firstYearFirstSemesterResult.TotalSemesterCourseUnit = firstSemesterTotalSemesterCourseUnit;
                    firstYearFirstSemesterResult.GPCU = firstSemesterGPCUSum;

                }
                return firstYearFirstSemesterResult;
            }
            catch (Exception)
            {

                throw;
            }

        }
        //private Result GetAllSemesterResultInfo(Programme programme, Department department, Model.Model.Student student, SessionSemester sessionSemester)
        public Result GetAllSemesterResultInfo(Programme programme, Department department, Model.Model.Student student, SessionSemester sessionSemester)
        {
            try
            {
                List<Result> result = null;
                Result allSemesterResult = new Result();
                StudentLogic studentLogic = new StudentLogic();
                StudentResultLogic studentResultLogic = new StudentResultLogic();
                Abundance_Nk.Model.Model.Student studentCheck = new Student() { Id = student.Id };

                GetStudent(studentLogic, studentCheck);

                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                StudentLevel studentLevel = null;

                studentLevel = studentLevelLogic.GetModelsBy(s => s.Person_Id == student.Id).LastOrDefault();

                if (studentLevel != null && studentLevel.Session != null)
                {
                    if (student.Activated == true || studentCheck.Activated == null)
                    {
                        result = studentResultLogic.GetStudentProcessedResultAll(studentLevel.Department, student, studentLevel.Programme, sessionSemester);
                    }
                    else
                    {
                        result = studentResultLogic.GetDeactivatedStudentProcessedResultAll(studentLevel.Department, student, studentLevel.Programme);
                    }

                    //List<Result> modifiedResultList = new List<Result>();
                    //int totalSemesterCourseUnit = 0;
                    //foreach (Result resultItem in result)
                    //{
                    //    decimal WGP = 0;
                    //    if (resultItem.SpecialCase != null)
                    //    {

                    //        resultItem.GPCU = 0;
                    //        if (totalSemesterCourseUnit == 0)
                    //        {
                    //            totalSemesterCourseUnit = (int)resultItem.TotalSemesterCourseUnit - resultItem.CourseUnit;
                    //            resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                    //            resultItem.Grade = "-";
                    //        }
                    //        else
                    //        {
                    //            totalSemesterCourseUnit -= resultItem.CourseUnit;
                    //            resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                    //            resultItem.Grade = "-";
                    //        }

                    //    }
                    //    if (totalSemesterCourseUnit > 0)
                    //    {
                    //        resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                    //    }
                    //    modifiedResultList.Add(resultItem);
                    //}

                    //decimal? allSemesterGPCUSum = modifiedResultList.Sum(p => p.GPCU);
                    //int? allSemesterTotalSemesterCourseUnit = 0;
                    //allSemesterTotalSemesterCourseUnit = modifiedResultList.Min(p => p.TotalSemesterCourseUnit);

                    allSemesterResult.TotalSemesterCourseUnit = result.Where(s => s.WGP != null).Sum(s => s.CourseUnit);
                    allSemesterResult.GPCU = result.Sum(s => s.GPCU) ;
                }

                return allSemesterResult;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private StudentLevel GetStudentLevel(StudentLevelLogic studentLevelLogic, StudentLevel studentLevel, Student student, Level level, Department department, Programme programme)
        {
            try
            {
                STUDENT_LEVEL studentLevelEntity = studentLevelLogic.GetEntitiesBy(p => p.Person_Id == student.Id && p.Level_Id == level.Id && p.Department_Id == department.Id && p.Programme_Id == programme.Id).FirstOrDefault();
                if (studentLevelEntity != null)
                {
                    studentLevel = new StudentLevel();
                    studentLevel.Id = studentLevelEntity.Student_Level_Id;
                    studentLevel.Department = department;

                    if (studentLevelEntity.Department_Option_Id != null && studentLevelEntity.Department_Option_Id > 0)
                    {
                        studentLevel.DepartmentOption = new DepartmentOption() { Id = studentLevelEntity.Department_Option_Id ?? 0 };
                    }
                    else
                    {
                        studentLevel.DepartmentOption = null;
                    }

                    studentLevel.Level = level;
                    studentLevel.Programme = programme;
                    studentLevel.Session = new Session() { Id = studentLevelEntity.Session_Id };
                    studentLevel.Student = student;
                }
                else
                {
                    studentLevel = null;
                }

                return studentLevel;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private Result GetFirstYearSecondSemesterResultInfo(SessionSemester sessionSemester, Level lvl, Programme programme, Department department, Model.Model.Student student)
        {
            try
            {
                Result firstYearFirstSemesterResult = new Result();
                List<Result> modifiedResultList = new List<Result>();
                List<Result> result = null;
                StudentLogic studentLogic = new StudentLogic();
                StudentResultLogic studentResultLogic = new StudentResultLogic();
                Abundance_Nk.Model.Model.Student studentCheck = studentLogic.GetModelBy(p => p.Person_Id == student.Id);
                Semester semester = new Semester() { Id = 2 };
                Level level = null;
                if (lvl.Id == 2)
                {
                    level = new Level() { Id = 1 };
                }
                else
                {
                    level = new Level() { Id = 3 };
                }

                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                StudentLevel studentLevel = null;
                studentLevel = GetStudentLevel(studentLevelLogic, studentLevel, student, level, department, programme);
                //StudentLevel studentLevel = studentLevelLogic.GetModelsBy(p => p.Person_Id == student.Id && p.Level_Id == level.Id && p.Department_Id == department.Id && p.Programme_Id == programme.Id).FirstOrDefault();
                if (studentLevel != null && studentLevel.Session != null)
                {

                    if (student.Activated == true || studentCheck.Activated == null)
                    {
                        result = studentResultLogic.GetStudentProcessedResultBy(studentLevel.Session, level, studentLevel.Department, student, semester, studentLevel.Programme);
                    }
                    else
                    {
                        result = studentResultLogic.GetDeactivatedStudentProcessedResultBy(studentLevel.Session, level, studentLevel.Department, student, semester, studentLevel.Programme);
                    }

                    int totalSemesterCourseUnit = 0;
                    foreach (Result resultItem in result)
                    {

                        decimal WGP = 0;

                        if (resultItem.SpecialCase != null)
                        {

                            resultItem.GPCU = 0;
                            if (totalSemesterCourseUnit == 0)
                            {
                                totalSemesterCourseUnit = (int)resultItem.TotalSemesterCourseUnit - resultItem.CourseUnit;
                                resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                                resultItem.Grade = "-";
                            }
                            else
                            {
                                totalSemesterCourseUnit -= resultItem.CourseUnit;
                                resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                                resultItem.Grade = "-";
                            }
                        }
                        if (totalSemesterCourseUnit > 0)
                        {
                            resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                        }
                        modifiedResultList.Add(resultItem);
                    }

                    decimal? firstSemesterGPCUSum = modifiedResultList.Sum(p => p.GPCU);
                    int? firstSemesterTotalSemesterCourseUnit = 0;
                    firstSemesterTotalSemesterCourseUnit = modifiedResultList.Min(p => p.TotalSemesterCourseUnit);
                    firstYearFirstSemesterResult.TotalSemesterCourseUnit = firstSemesterTotalSemesterCourseUnit;
                    firstYearFirstSemesterResult.GPCU = firstSemesterGPCUSum;

                }
                return firstYearFirstSemesterResult;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public List<string> GetCarryOverCourseCodes(SessionSemester ss, Level lvl, Programme programme, Department department, Model.Model.Student student)
        {
            try
            {
                List<string> courseCodes = new List<string>();
                List<CourseRegistrationDetail> courseRegistrationdetails = new List<CourseRegistrationDetail>();
                CourseRegistrationDetailLogic courseRegistrationDetailLogic = new CourseRegistrationDetailLogic();

                StudentLevel studentLevel = new StudentLevel();
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();


                Level level = null;
                if (lvl.Id == 1 || lvl.Id == 3)
                {
                    level = new Level() { Id = 1 };
                    courseCodes = GetFirstYearCarryOverCourses(ss, lvl, programme, department, student);

                }
                else
                {
                    level = new Level() { Id = 3 };
                }
                courseRegistrationdetails = courseRegistrationDetailLogic.GetModelsBy(crd => crd.STUDENT_COURSE_REGISTRATION.Session_Id == ss.Session.Id && crd.STUDENT_COURSE_REGISTRATION.Department_Id == department.Id && crd.STUDENT_COURSE_REGISTRATION.Programme_Id == programme.Id && crd.STUDENT_COURSE_REGISTRATION.Person_Id == student.Id && (crd.Test_Score + crd.Exam_Score) < 40 && crd.Special_Case == null);

                if (courseRegistrationdetails != null)
                {
                    if (ss.Semester.Id == 1)
                    {
                        courseRegistrationdetails = courseRegistrationdetails.Where(p => p.Semester.Id == 1).ToList();
                        if (courseRegistrationdetails.Count > 0)
                        {
                            foreach (CourseRegistrationDetail courseRegistrationDetail in courseRegistrationdetails)
                            {
                                if (courseRegistrationDetail.SpecialCase == null)
                                {
                                    courseCodes.Add(courseRegistrationDetail.Course.Code);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (courseRegistrationdetails.Count > 0)
                        {
                            foreach (CourseRegistrationDetail courseRegistrationDetail in courseRegistrationdetails)
                            {
                                if (courseRegistrationDetail.SpecialCase == null)
                                {
                                    courseCodes.Add(courseRegistrationDetail.Course.Code);
                                }
                            }
                        }
                    }
                }
                return courseCodes;
            }
            catch (Exception)
            {

                throw;
            }

        }
        private string GetGraduationStatus(decimal? CGPA, List<string> courseCodes)
        {
            string remark = null;
            try
            {
                if (courseCodes.Count == 0 && CGPA != null)
                {
                    if (CGPA >= (decimal)3.5 && CGPA <= (decimal)4.0)
                    {
                        remark = "RHL; PASSED: DISTINCTION";
                    }
                    else if (CGPA >= (decimal)3.25 && CGPA <= (decimal)3.49)
                    {
                        remark = "DHL; PASSED: UPPER CREDIT";
                    }
                    else if (CGPA >= (decimal)3.0 && CGPA < (decimal)3.25)
                    {
                        remark = "PAS; PASSED: UPPER CREDIT";
                    }
                    else if (CGPA >= (decimal)2.5 && CGPA <= (decimal)2.99)
                    {
                        remark = "PAS; PASSED: LOWER CREDIT";
                    }
                    else if (CGPA >= (decimal)2.0 && CGPA <= (decimal)2.49)
                    {
                        remark = "PAS; PASSED: PASS";
                    }
                    else if (CGPA < (decimal)2.0)
                    {
                        remark = "PROBATION";
                    }
                }
                else
                {
                    if (CGPA < (decimal)2.0)
                    {
                        remark = "PROBATION / CO-";
                        for (int i = 0; i < courseCodes.Count(); i++)
                        {
                            remark += ("|" + courseCodes[i]);
                        }
                    }
                    else
                    {
                        remark = "CO-";
                        for (int i = 0; i < courseCodes.Count(); i++)
                        {
                            remark += ("|" + courseCodes[i]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return remark;
        }

        private string GetGraduationStatus(decimal? CGPA, decimal? previousCGPA, decimal? currentCGPA, List<string> courseCodes)
        {
            string remark = null;
            try
            {
                if (previousCGPA != null && currentCGPA != null)
                {
                    if (previousCGPA < (decimal)2.0 && currentCGPA < (decimal)2.0)
                    {
                        remark = "WITHRADWN ";
                    }
                    else if (previousCGPA < (decimal)2.0)
                    {
                        remark = "PROBATION ";
                    }
                    else if (currentCGPA < (decimal)2.0)
                    {
                        remark = "PROBATION ";
                    }
                    if (courseCodes.Count != 0)
                    {
                        remark += "CO-";
                        for (int i = 0; i < courseCodes.Count(); i++)
                        {
                            remark += ("|" + courseCodes[i]);
                        }
                    }
                }

            }
            catch (Exception)
            {
            }
            return remark;
        }
        private string GetSecondYearFirstSemeterGraduationStatus(decimal? firstSemesterGPA, decimal? secondSemesterGPA, List<string> courseCodes)
        {
            string remark = null;
            try
            {
                if (firstSemesterGPA != null && secondSemesterGPA != null)
                {
                    if (firstSemesterGPA < (decimal)2.0 && secondSemesterGPA < (decimal)2.0)
                    {
                        remark = "WITHRADWN ";
                    }
                    //else if (firstSemesterGPA < (decimal)2.0)
                    //{
                    //    remark = "PROBATION ";
                    //}
                    //else if (secondSemesterGPA < (decimal)2.0)
                    //{
                    //    remark = "PROBATION ";
                    //}
                    else if (secondSemesterGPA >= (decimal)3.5 && secondSemesterGPA <= (decimal)4.0)
                    {
                        remark = "RHL; PASSED: DISTINCTION";
                    }
                    else if (secondSemesterGPA >= (decimal)3.25 && secondSemesterGPA <= (decimal)3.49)
                    {
                        remark = "DHL; PASSED: UPPER CREDIT";
                    }
                    else if (secondSemesterGPA >= (decimal)3.0 && secondSemesterGPA < (decimal)3.25)
                    {
                        remark = "PAS; PASSED: UPPER CREDIT";
                    }
                    else if (secondSemesterGPA >= (decimal)2.5 && secondSemesterGPA <= (decimal)2.99)
                    {
                        remark = "PAS; PASSED: LOWER CREDIT";
                    }
                    else if (secondSemesterGPA >= (decimal)2.0 && secondSemesterGPA <= (decimal)2.49)
                    {
                        remark = "PAS; PASSED: PASS";
                    }
                    else if (secondSemesterGPA < (decimal)2.0)
                    {
                        remark = "PROBATION ";
                    }

                    if (courseCodes.Count != 0)
                    {
                        remark += "CO-";
                        for (int i = 0; i < courseCodes.Count(); i++)
                        {
                            remark += ("|" + courseCodes[i]);
                        }
                    }
                }
                else if (firstSemesterGPA == null && secondSemesterGPA != null)
                {
                    if (secondSemesterGPA >= (decimal)3.5 && secondSemesterGPA <= (decimal)4.0)
                    {
                        remark = "RHL; PASSED: DISTINCTION";
                    }
                    else if (secondSemesterGPA >= (decimal)3.25 && secondSemesterGPA <= (decimal)3.49)
                    {
                        remark = "DHL; PASSED: UPPER CREDIT";
                    }
                    else if (secondSemesterGPA >= (decimal)3.0 && secondSemesterGPA < (decimal)3.25)
                    {
                        remark = "PAS; PASSED: UPPER CREDIT";
                    }
                    else if (secondSemesterGPA >= (decimal)2.5 && secondSemesterGPA <= (decimal)2.99)
                    {
                        remark = "PAS; PASSED: LOWER CREDIT";
                    }
                    else if (secondSemesterGPA >= (decimal)2.0 && secondSemesterGPA <= (decimal)2.49)
                    {
                        remark = "PAS; PASSED: PASS";
                    }
                    else if (secondSemesterGPA < (decimal)2.0)
                    {
                        remark = "PROBATION ";
                    }

                    if (courseCodes.Count != 0)
                    {
                        remark += "CO-";
                        for (int i = 0; i < courseCodes.Count(); i++)
                        {
                            remark += ("|" + courseCodes[i]);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return remark;
        }

        public List<UploadedCourseFormat> GetUploadedCourses(Session session, Semester semester, Programme programme, Department department, Level level)
        {
            try
            {
                if (session == null || session.Id <= 0 || semester == null || semester.Id <= 0 || programme == null || programme.Id <= 0 || department == null || department.Id <= 0 || level == null || level.Id <= 0)
                {
                    throw new Exception("One or more criteria to get the uploaded courses is not set! Please check your input criteria selection and try again.");
                }

                List<UploadedCourseFormat> uploadedCourses = (from uc in repository.GetBy<VW_UPLOADED_COURSES>(x => x.Session_Id == session.Id && x.Semester_Id == semester.Id && x.Department_Id == department.Id && x.Level_Id == level.Id)
                                                              select new UploadedCourseFormat
                                                              {
                                                                  Level = uc.Level_Name,
                                                                  Department = uc.Department_Name,
                                                                  CourseCode = uc.Course_Code,
                                                                  CourseTitle = uc.Course_Name,
                                                                  DepartmentId = uc.Department_Id,
                                                                  SessionId = uc.Session_Id,
                                                                  SemesterId = uc.Semester_Id,
                                                                  LevelId = uc.Level_Id,
                                                                  CourseId = uc.Course_Id
                                                              }).ToList();

                StudentExamRawScoreSheetResultLogic rawScoreLogic = new StudentExamRawScoreSheetResultLogic();
                CourseAllocationLogic courseAllocationLogic = new CourseAllocationLogic();
                List<UploadedCourseFormat> masterUploadedCourseFormats = new List<UploadedCourseFormat>();

                for (int i = 0; i < uploadedCourses.Count; i++)
                {
                    UploadedCourseFormat currentUploadedCourseFormat = uploadedCourses[i];

                    CourseAllocation courseAllocation = courseAllocationLogic.GetModelsBy(c => c.Session_Id == currentUploadedCourseFormat.SessionId && c.Semester_Id == currentUploadedCourseFormat.SemesterId && c.Course_Id == currentUploadedCourseFormat.CourseId && c.Department_Id == currentUploadedCourseFormat.DepartmentId && c.Level_Id == currentUploadedCourseFormat.LevelId && c.Programme_Id == programme.Id).LastOrDefault();

                    if (courseAllocation != null)
                    {
                        List<UploadedCourseFormat> specifiCourseFormats = uploadedCourses.Where(u => u.CourseId == currentUploadedCourseFormat.CourseId && u.DepartmentId == currentUploadedCourseFormat.DepartmentId && u.LevelId == currentUploadedCourseFormat.LevelId && u.SemesterId == currentUploadedCourseFormat.SemesterId && u.SessionId == currentUploadedCourseFormat.SessionId).ToList();
                        for (int k = 0; k < specifiCourseFormats.Count; k++)
                        {
                            specifiCourseFormats[k].Programme = courseAllocation.Programme.Name;
                            specifiCourseFormats[k].LecturerName = courseAllocation.User.Username;
                            specifiCourseFormats[k].ProgrammeId = courseAllocation.Programme.Id;
                            specifiCourseFormats[k].CourseAllocationId = courseAllocation.Id;
                        }
                    }
                }

                masterUploadedCourseFormats = uploadedCourses.Where(u => u.ProgrammeId == programme.Id).ToList();

                return masterUploadedCourseFormats.OrderBy(uc => uc.Department).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<UploadedCourseFormat> GetUploadedAlternateCourses(Session session, Semester semester)
        {
            try
            {
                if (session == null || session.Id <= 0 || semester == null || semester.Id <= 0)
                {
                    throw new Exception("One or more criteria to get the uploaded courses is not set! Please check your input criteria selection and try again.");
                }
                List<UploadedCourseFormat> uploadedCourses = (from uc in repository.GetBy<VW_STUDENT_RESULT_RAW_SCORE_SHEET_UNREGISTERED>(x => x.Session_Id == session.Id && x.Semester_Id == semester.Id)
                                                              select new UploadedCourseFormat
                                                              {
                                                                  Programme = uc.Programme_Name,
                                                                  Level = uc.Level_Name,
                                                                  Department = uc.Department_Name,
                                                                  CourseCode = uc.Course_Code,
                                                                  CourseTitle = uc.Course_Name,
                                                                  LecturerName = "",
                                                                  ProgrammeId = uc.Programme_Id,
                                                                  DepartmentId = 1,
                                                                  SessionId = uc.Session_Id,
                                                                  SemesterId = uc.Semester_Id,
                                                                  LevelId = uc.Level_Id,
                                                                  CourseId = uc.Course_Id
                                                              }).ToList();

                return uploadedCourses.OrderBy(uc => uc.Programme).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetIdentifierBy(STUDENT_EXAM_RAW_SCORE_SHEET_RESULT_NOT_REGISTERED rawscoresheetItem)
        {
            try
            {
                string identifier = null;
                string departmentCode = rawscoresheetItem.COURSE.DEPARTMENT.Department_Code;
                string level = rawscoresheetItem.LEVEL.Level_Name;
                string semesterCode = GetSemesterCodeBy(rawscoresheetItem.Semester_Id);
                string sessionCode = GetSessionCodeBy(rawscoresheetItem.SESSION.Session_Name);
                identifier = departmentCode + level + semesterCode + sessionCode;
                return identifier;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private string GetSessionCodeBy(string sessionName)
        {
            try
            {
                string sessionCode = null;
                string[] sessionArray = sessionName.Split('/');
                string sessionYear = sessionArray[1];
                sessionCode = sessionYear.Substring(2, 2);
                return sessionCode;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private string GetSemesterCodeBy(int semesterId)
        {
            try
            {
                if (semesterId == 1)
                {
                    return "F";
                }
                else
                {
                    return "S";
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private string GetDepartmentCode(int departmentid)
        {
            string code = "";
            try
            {
                DepartmentLogic departmentLogic = new DepartmentLogic();
                code = departmentLogic.GetModelBy(m => m.Department_Id == departmentid).Code;
            }
            catch (Exception)
            {

                throw;
            }
            return code;
        }
        private string GetLevelName(int levelId)
        {
            string code = "";
            try
            {
                LevelLogic levelLogic = new LevelLogic();
                code = levelLogic.GetModelBy(m => m.Level_Id == levelId).Name;
            }
            catch (Exception)
            {

                throw;
            }
            return code;
        }
        public List<ResultUpdateModel> GetResultUpdates(Session session, Semester semester, Programme programme, Department department, Level level, Course course)
        {
            try
            {
                if (session == null || session.Id <= 0 || semester == null || semester.Id <= 0 || programme == null || programme.Id <= 0 || department == null || department.Id <= 0 || level == null || level.Id <= 0 || course == null || course.Id <= 0)
                {
                    throw new Exception("One or more criteria to get the uploaded courses is not set! Please check your input criteria selection and try again.");
                }
                List<ResultUpdateModel> resultUpdates = (from uc in repository.GetBy<VW_RESULT_UPDATES>(x => x.Session_Id == session.Id && x.Semester_Id == semester.Id && x.Programme_Id == programme.Id && x.Department_Id == department.Id && x.Level_Id == level.Id && x.Course_Id == course.Id)
                                                         select new ResultUpdateModel
                                                         {
                                                             Programme = uc.Programme_Name,
                                                             Level = uc.Level_Name,
                                                             Department = uc.Department_Name,
                                                             CourseCode = uc.Course_Code,
                                                             CourseTitle = uc.Course_Name,
                                                             StaffName = uc.User_Name,
                                                             Session = uc.Session_Name,
                                                             Semester = uc.Semester_Name,
                                                             MatricNumber = uc.Matric_Number,
                                                             LastModifiedDate = uc.Date_Uploaded.ToLongDateString(),
                                                             UserId = uc.User_Id
                                                         }).ToList();

                StaffLogic staffLogic = new StaffLogic();
                for (int i = 0; i < resultUpdates.Count; i++)
                {
                    ResultUpdateModel currentModel = resultUpdates[i];
                    Staff staff = staffLogic.GetModelBy(s => s.User_Id == currentModel.UserId);
                    if (staff != null)
                    {
                        resultUpdates[i].StaffName = staff.FullName;
                    }
                }

                return resultUpdates.OrderBy(r => r.MatricNumber).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<List<Result>> GetGraduationList(SessionSemester sessionSemester, Level level, Programme programme, Department department, int type)
        {
            List<Result> masterSheetResult = new List<Result>();
            try
            {
                if (sessionSemester == null || sessionSemester.Id <= 0 || level == null || level.Id <= 0 || programme == null || programme.Id <= 0 || department == null || department.Id <= 0)
                {
                    throw new Exception("One or more criteria to get Mater Result Sheet not set! Please check your input criteria selection and try again.");
                }

                SessionSemesterLogic sessionSemesterLogic = new Business.SessionSemesterLogic();
                SessionSemester ss = sessionSemesterLogic.GetBy(sessionSemester.Id);
                SessionLogic sessionLogic = new SessionLogic();
                Session sessions = sessionLogic.GetModelBy(p => p.Session_Id == ss.Session.Id);
                string[] sessionItems = ss.Session.Name.Split('/');
                string sessionNameStr = sessionItems[0];
                int sessionNameInt = Convert.ToInt32(sessionNameStr);
                List<Result> results = null;

                string currentSessionSuffix = sessionNameStr.Substring(2, 2); // 18
                string yearTwoSessionSuffix = Convert.ToString((Convert.ToInt32(currentSessionSuffix) - 1)); //17

                currentSessionSuffix = "/" + currentSessionSuffix + "/";
                yearTwoSessionSuffix = "/" + yearTwoSessionSuffix + "/";

                string identifier = null;
                string departmentCode = GetDepartmentCode(department.Id);
                string levels = GetLevelName(level.Id);
                string semesterCode = GetSemesterCodeBy(ss.Semester.Id);
                string sessionCode = GetSessionCodeBy(ss.Session.Name);
                identifier = departmentCode + levels + semesterCode + sessionCode;


                if (ss.Session.Id == (int)Sessions._20152016)
                {
                    results = (from sr in await repository.GetByAsync<VW_STUDENT_RESULT_2>(x => x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id && x.Level_Id == level.Id && x.Programme_Id == programme.Id && x.Department_Id == department.Id && (x.Activated == true || x.Activated == null))
                               select new Result
                               {
                                   StudentId = sr.Person_Id,
                                   Sex = sr.Sex_Name,
                                   Name = sr.Name,
                                   MatricNumber = sr.Matric_Number,
                                   CourseId = sr.Course_Id,
                                   CourseCode = sr.Course_Code,
                                   CourseName = sr.Course_Name,
                                   CourseUnit = sr.Course_Unit,
                                   SpecialCase = sr.Special_Case,
                                   TestScore = sr.Test_Score,
                                   ExamScore = sr.Exam_Score,
                                   Score = sr.Total_Score,
                                   Grade = sr.Grade,
                                   GradePoint = sr.Grade_Point,
                                   DepartmentName = sr.Department_Name,
                                   ProgrammeName = sr.Programme_Name,
                                   LevelName = sr.Level_Name,
                                   Semestername = sr.Semester_Name,
                                   GPCU = sr.Grade_Point * sr.Course_Unit,
                                   TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                   SessionName = sr.Session_Name
                               }).ToList();

                }
                else
                {
                    results = (from sr in await repository.GetByAsync<VW_STUDENT_RESULT_2>(x => x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id && x.Level_Id == level.Id && x.Programme_Id == programme.Id && x.Department_Id == department.Id  && (x.Activated == true || x.Activated == null))
                               select new Result
                               {
                                   StudentId = sr.Person_Id,
                                   Sex = sr.Sex_Name,
                                   Name = sr.Name.ToUpper(),
                                   MatricNumber = sr.Matric_Number,
                                   CourseId = sr.Course_Id,
                                   CourseCode = sr.Course_Code,
                                   CourseName = sr.Course_Name,
                                   CourseUnit = sr.Course_Unit,
                                   SpecialCase = sr.Special_Case,
                                   TestScore = sr.Test_Score,
                                   ExamScore = sr.Exam_Score,
                                   Score = sr.Total_Score,
                                   Grade = sr.Grade,
                                   GradePoint = sr.Grade_Point,
                                   DepartmentName = sr.Department_Name,
                                   ProgrammeName = sr.Programme_Name,
                                   LevelName = sr.Level_Name,
                                   Semestername = sr.Semester_Name,
                                   GPCU = sr.Grade_Point * sr.Course_Unit,
                                   TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                   SessionName = sr.Session_Name
                               }).ToList();

                    List<Result> resultList = new List<Result>();

                    for (int i = 0; i < results.Count; i++)
                    {
                        if (results[i].MatricNumber.Contains(yearTwoSessionSuffix))
                        {
                            resultList.Add(results[i]);
                        }
                        else
                        {
                            //Do Nothing
                        }

                        //if (results[i].MatricNumber.Contains("/16/") || results[i].MatricNumber.Contains("/13/") || results[i].MatricNumber.Contains("/14/"))
                        //{
                        //    //Do Nothing
                        //}
                        //else
                        //{
                        //    resultList.Add(results[i]);
                        //}
                    }

                    results = new List<Result>();
                    results = resultList;
                }

                List<Result> classResult = (from sr in await repository.GetByAsync<VW_STUDENT_RESULT_2>(x => x.Programme_Id == programme.Id && x.Department_Id == department.Id &&
                                                   (x.Activated == true || x.Activated == null))
                                            select new Result
                                            {
                                                StudentId = sr.Person_Id,
                                                Sex = sr.Sex_Name,
                                                Name = sr.Name,
                                                MatricNumber = sr.Matric_Number,
                                                CourseId = sr.Course_Id,
                                                CourseCode = sr.Course_Code,
                                                CourseName = sr.Course_Name,
                                                CourseUnit = sr.Course_Unit,
                                                SpecialCase = sr.Special_Case,
                                                TestScore = sr.Test_Score,
                                                ExamScore = sr.Exam_Score,
                                                Score = sr.Total_Score,
                                                Grade = sr.Grade,
                                                GradePoint = sr.Grade_Point,
                                                DepartmentName = sr.Department_Name,
                                                ProgrammeName = sr.Programme_Name,
                                                LevelName = sr.Level_Name,
                                                Semestername = sr.Semester_Name,
                                                GPCU = sr.Grade_Point * sr.Course_Unit,
                                                TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                                SessionName = sr.Session_Name,
                                                CourseModeId = sr.Course_Mode_Id
                                            }).ToList();

                List<long> distinctStudents = results.Select(r => r.StudentId).Distinct().ToList();
                for (int i = 0; i < distinctStudents.Count; i++)
                {
                    long currentStudentId = distinctStudents[i];
                    Result currentStudentResult = results.Where(r => r.StudentId == currentStudentId).LastOrDefault();
                    Result confirmedResult = GetStudentResultDetails(currentStudentId, level.Id, department.Id, programme.Id, ss.Semester.Id, ss.Session.Id);

                    bool hasSpecialCaseOutstanding = CheckSpecialCase(classResult, currentStudentId);

                    if (confirmedResult != null && !confirmedResult.Remark.Contains("CO-") && !hasSpecialCaseOutstanding)
                    {
                        currentStudentResult.CGPA = confirmedResult.CGPA;
                        currentStudentResult.Remark = confirmedResult.Remark;
                        currentStudentResult.Identifier = identifier;
                        currentStudentResult.Count = 1;
                        masterSheetResult.Add(currentStudentResult);
                    }

                }

                switch (type)
                {
                    case 1:
                        return masterSheetResult.OrderBy(a => a.Remark).ToList();
                        break;
                    case 2:
                        return masterSheetResult.OrderByDescending(a => a.CGPA).ToList();
                        break;
                    case 3:
                        return masterSheetResult.OrderBy(a => a.MatricNumber).ToList();
                        break;
                    case 4:
                        return masterSheetResult.OrderBy(a => a.Name).ToList();
                        break;
                }
            }
            catch (Exception)
            {
                throw;
            }

            return masterSheetResult.OrderBy(a => a.MatricNumber).ToList();
        }
        public List<Result> GetGraduationListFirstYear(SessionSemester sessionSemester, Level level, Programme programme, Department department, int type)
        {
            List<Result> masterSheetResult = new List<Result>();
            try
            {
                if (sessionSemester == null || sessionSemester.Id <= 0 || level == null || level.Id <= 0 || programme == null || programme.Id <= 0 || department == null || department.Id <= 0)
                {
                    throw new Exception("One or more criteria to get Mater Result Sheet not set! Please check your input criteria selection and try again.");
                }

                SessionSemesterLogic sessionSemesterLogic = new Business.SessionSemesterLogic();
                SessionSemester ss = sessionSemesterLogic.GetBy(sessionSemester.Id);
                SessionLogic sessionLogic = new SessionLogic();
                Session sessions = sessionLogic.GetModelBy(p => p.Session_Id == ss.Session.Id);
                string[] sessionItems = ss.Session.Name.Split('/');
                string sessionNameStr = sessionItems[0];
                int sessionNameInt = Convert.ToInt32(sessionNameStr);
                List<Result> results = null;

                string currentSessionSuffix = sessionNameStr.Substring(2, 2);
                string yearTwoSessionSuffix = Convert.ToString((Convert.ToInt32(currentSessionSuffix) - 1));

                currentSessionSuffix = "/" + currentSessionSuffix + "/";
                yearTwoSessionSuffix = "/" + yearTwoSessionSuffix + "/";

                string identifier = null;
                string departmentCode = GetDepartmentCode(department.Id);
                string levels = GetLevelName(level.Id);
                string semesterCode = GetSemesterCodeBy(ss.Semester.Id);
                string sessionCode = GetSessionCodeBy(ss.Session.Name);
                identifier = departmentCode + levels + semesterCode + sessionCode;


                if (ss.Session.Id == (int)Sessions._20152016)
                {
                    results = (from sr in repository.GetBy<VW_STUDENT_RESULT_2>(x => x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id && x.Level_Id == level.Id && x.Programme_Id == programme.Id && x.Department_Id == department.Id && (x.Activated == true || x.Activated == null))
                               select new Result
                               {
                                   StudentId = sr.Person_Id,
                                   Sex = sr.Sex_Name,
                                   Name = sr.Name,
                                   MatricNumber = sr.Matric_Number,
                                   CourseId = sr.Course_Id,
                                   CourseCode = sr.Course_Code,
                                   CourseName = sr.Course_Name,
                                   CourseUnit = sr.Course_Unit,
                                   SpecialCase = sr.Special_Case,
                                   TestScore = sr.Test_Score,
                                   ExamScore = sr.Exam_Score,
                                   Score = sr.Total_Score,
                                   Grade = sr.Grade,
                                   GradePoint = sr.Grade_Point,
                                   DepartmentName = sr.Department_Name,
                                   ProgrammeName = sr.Programme_Name,
                                   LevelName = sr.Level_Name,
                                   Semestername = sr.Semester_Name,
                                   GPCU = sr.Grade_Point * sr.Course_Unit,
                                   TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                   SessionName = sr.Session_Name
                               }).ToList();

                }
                else
                {
                    results = (from sr in repository.GetBy<VW_STUDENT_RESULT_2>(x => x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id && x.Programme_Id == programme.Id && x.Department_Id == department.Id && x.Level_Id == level.Id && (x.Activated == true || x.Activated == null))
                               select new Result
                               {
                                   StudentId = sr.Person_Id,
                                   Sex = sr.Sex_Name,
                                   Name = sr.Name.ToUpper(),
                                   MatricNumber = sr.Matric_Number,
                                   CourseId = sr.Course_Id,
                                   CourseCode = sr.Course_Code,
                                   CourseName = sr.Course_Name,
                                   CourseUnit = sr.Course_Unit,
                                   SpecialCase = sr.Special_Case,
                                   TestScore = sr.Test_Score,
                                   ExamScore = sr.Exam_Score,
                                   Score = sr.Total_Score,
                                   Grade = sr.Grade,
                                   GradePoint = sr.Grade_Point,
                                   DepartmentName = sr.Department_Name,
                                   ProgrammeName = sr.Programme_Name,
                                   LevelName = sr.Level_Name,
                                   Semestername = sr.Semester_Name,
                                   GPCU = sr.Grade_Point * sr.Course_Unit,
                                   TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                   SessionName = sr.Session_Name
                               }).ToList();

                    List<Result> resultList = new List<Result>();

                    for (int i = 0; i < results.Count; i++)
                    {
                        if (results[i].MatricNumber.Contains(currentSessionSuffix))
                        {
                            resultList.Add(results[i]);
                        }
                        else
                        {
                            //Do Nothing
                        }

                        //if (results[i].MatricNumber.Contains("/16/") || results[i].MatricNumber.Contains("/13/") || results[i].MatricNumber.Contains("/14/"))
                        //{
                        //    //Do Nothing
                        //}
                        //else
                        //{
                        //    resultList.Add(results[i]);
                        //}
                    }

                    results = new List<Result>();
                    results = resultList;
                }

                List<Result> classResult = (from sr in repository.GetBy<VW_STUDENT_RESULT_2>(x => x.Programme_Id == programme.Id && x.Department_Id == department.Id &&
                                                   (x.Activated == true || x.Activated == null))
                                            select new Result
                                            {
                                                StudentId = sr.Person_Id,
                                                Sex = sr.Sex_Name,
                                                Name = sr.Name,
                                                MatricNumber = sr.Matric_Number,
                                                CourseId = sr.Course_Id,
                                                CourseCode = sr.Course_Code,
                                                CourseName = sr.Course_Name,
                                                CourseUnit = sr.Course_Unit,
                                                SpecialCase = sr.Special_Case,
                                                TestScore = sr.Test_Score,
                                                ExamScore = sr.Exam_Score,
                                                Score = sr.Total_Score,
                                                Grade = sr.Grade,
                                                GradePoint = sr.Grade_Point,
                                                DepartmentName = sr.Department_Name,
                                                ProgrammeName = sr.Programme_Name,
                                                LevelName = sr.Level_Name,
                                                Semestername = sr.Semester_Name,
                                                GPCU = sr.Grade_Point * sr.Course_Unit,
                                                TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                                SessionName = sr.Session_Name,
                                                CourseModeId = sr.Course_Mode_Id
                                            }).ToList();

                List<long> distinctStudents = results.Select(r => r.StudentId).Distinct().ToList();
                for (int i = 0; i < distinctStudents.Count; i++)
                {
                    long currentStudentId = distinctStudents[i];
                    Result currentStudentResult = results.Where(r => r.StudentId == currentStudentId).LastOrDefault();
                    //Result confirmedResult = GetStudentResultDetails(currentStudentId, level.Id, department.Id, programme.Id, ss.Semester.Id, ss.Session.Id);

                    Result confirmedResult = ViewProcessedStudentResult(currentStudentId, ss, level, programme, department);
                    //confirmedResult.UnitOutstanding = TotalUnitsOmitted;

                    bool hasSpecialCaseOutstanding = CheckSpecialCase(classResult, currentStudentId);

                    if (confirmedResult != null && !confirmedResult.Remark.Contains("CO-") && !hasSpecialCaseOutstanding)
                    {
                        currentStudentResult.CGPA = confirmedResult.CGPA;
                        currentStudentResult.Remark = confirmedResult.Remark;
                        currentStudentResult.Identifier = identifier;
                        currentStudentResult.Count = 1;
                        masterSheetResult.Add(currentStudentResult);
                    }

                }

                switch (type)
                {
                    case 1:
                        return masterSheetResult.OrderBy(a => a.Remark).ToList();
                        break;
                    case 2:
                        return masterSheetResult.OrderByDescending(a => a.CGPA).ToList();
                        break;
                    case 3:
                        return masterSheetResult.OrderBy(a => a.MatricNumber).ToList();
                        break;
                    case 4:
                        return masterSheetResult.OrderBy(a => a.Name).ToList();
                        break;
                }
            }
            catch (Exception)
            {
                throw;
            }

            return masterSheetResult.OrderBy(a => a.MatricNumber).ToList();
        }
        public List<Result> GetGraduationListExtraYear(SessionSemester sessionSemester, Level level, Programme programme, Department department, int type)
        {
            List<Result> masterSheetResult = new List<Result>();
            try
            {
                if (sessionSemester == null || sessionSemester.Id <= 0 || level == null || level.Id <= 0 || programme == null || programme.Id <= 0 || department == null || department.Id <= 0)
                {
                    throw new Exception("One or more criteria to get Mater Result Sheet not set! Please check your input criteria selection and try again.");
                }

                SessionSemesterLogic sessionSemesterLogic = new Business.SessionSemesterLogic();
                SessionSemester ss = sessionSemesterLogic.GetBy(sessionSemester.Id);
                SessionLogic sessionLogic = new SessionLogic();
                Session sessions = sessionLogic.GetModelBy(p => p.Session_Id == ss.Session.Id);
                string[] sessionItems = ss.Session.Name.Split('/');
                string sessionNameStr = sessionItems[0];
                int sessionNameInt = Convert.ToInt32(sessionNameStr);
                List<Result> results = null;

                string currentSessionSuffix = sessionNameStr.Substring(2, 2);
                string yearTwoSessionSuffix = Convert.ToString((Convert.ToInt32(currentSessionSuffix) - 1));

                currentSessionSuffix = "/" + currentSessionSuffix + "/";
                yearTwoSessionSuffix = "/" + yearTwoSessionSuffix + "/";

                string identifier = null;
                string departmentCode = GetDepartmentCode(department.Id);
                string levels = GetLevelName(level.Id);
                string semesterCode = GetSemesterCodeBy(ss.Semester.Id);
                string sessionCode = GetSessionCodeBy(ss.Session.Name);
                string extraYearIdentifiersuffix = null;
                if (programme.Id == (int)Programmes.NDPartTime)
                {
                    extraYearIdentifiersuffix = "YC2";
                }
                else
                {
                    extraYearIdentifiersuffix = levels.Substring(0, 1) + "C2";
                }
                identifier = departmentCode + extraYearIdentifiersuffix + semesterCode + sessionCode;

                StudentDefermentLogic defermentLogic = new StudentDefermentLogic();
                List<StudentDeferementLog> deferementLogs = defermentLogic.GetAll();

                if (ss.Session.Id == (int)Sessions._20152016)
                {
                    results = (from sr in repository.GetBy<VW_STUDENT_RESULT_2>(x => x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id && x.Level_Id == level.Id && x.Programme_Id == programme.Id && x.Department_Id == department.Id && (x.Activated == true || x.Activated == null))
                               select new Result
                               {
                                   StudentId = sr.Person_Id,
                                   Sex = sr.Sex_Name,
                                   Name = sr.Name,
                                   MatricNumber = sr.Matric_Number,
                                   CourseId = sr.Course_Id,
                                   CourseCode = sr.Course_Code,
                                   CourseName = sr.Course_Name,
                                   CourseUnit = sr.Course_Unit,
                                   SpecialCase = sr.Special_Case,
                                   TestScore = sr.Test_Score,
                                   ExamScore = sr.Exam_Score,
                                   Score = sr.Total_Score,
                                   Grade = sr.Grade,
                                   GradePoint = sr.Grade_Point,
                                   DepartmentName = sr.Department_Name,
                                   ProgrammeName = sr.Programme_Name,
                                   LevelName = sr.Level_Name,
                                   Semestername = sr.Semester_Name,
                                   GPCU = sr.Grade_Point * sr.Course_Unit,
                                   TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                   SessionName = sr.Session_Name
                               }).ToList();

                }
                else
                {
                    results = (from sr in repository.GetBy<VW_STUDENT_RESULT_2>(x => x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id && x.Programme_Id == programme.Id 
                               && x.Department_Id == department.Id && x.Level_Id == level.Id && (x.Activated == true || x.Activated == null))
                               select new Result
                               {
                                   StudentId = sr.Person_Id,
                                   Sex = sr.Sex_Name,
                                   Name = sr.Name.ToUpper(),
                                   MatricNumber = sr.Matric_Number,
                                   CourseId = sr.Course_Id,
                                   CourseCode = sr.Course_Code,
                                   CourseName = sr.Course_Name,
                                   CourseUnit = sr.Course_Unit,
                                   SpecialCase = sr.Special_Case,
                                   TestScore = sr.Test_Score,
                                   ExamScore = sr.Exam_Score,
                                   Score = sr.Total_Score,
                                   Grade = sr.Grade,
                                   GradePoint = sr.Grade_Point,
                                   DepartmentName = sr.Department_Name,
                                   ProgrammeName = sr.Programme_Name,
                                   LevelName = sr.Level_Name,
                                   Semestername = sr.Semester_Name,
                                   GPCU = sr.Grade_Point * sr.Course_Unit,
                                   TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                   SessionName = sr.Session_Name
                               }).ToList();

                    List<Result> resultList = new List<Result>();

                    for (int i = 0; i < results.Count; i++)
                    {
                        //if (results[i].MatricNumber.Contains(yearTwoSessionSuffix))
                        //{
                        //    resultList.Add(results[i]);
                        //}
                        //else
                        //{
                        //    //Do Nothing
                        //}

                        if (results[i].MatricNumber.Contains(currentSessionSuffix) || results[i].MatricNumber.Contains(yearTwoSessionSuffix) || results[i].MatricNumber.Contains("/14/") ||
                                                    results[i].MatricNumber.Contains("/13/") || results[i].MatricNumber.Contains("/12/") || results[i].MatricNumber.Contains("/11/"))
                        {
                            //Do Nothing
                        }
                        else
                        {
                            if (deferementLogs.LastOrDefault(s => s.Student.MatricNumber == results[i].MatricNumber && s.Rusticated) == null)
                            {
                                resultList.Add(results[i]);
                            }
                        }
                    }

                    results = new List<Result>();
                    results = resultList;
                }
                //Get students with more carry over semesters
                List<long> studentsWithMoreSemesters = new List<long>();
                if (ss.Semester.Id == 1)
                {
                    studentsWithMoreSemesters = (from sr in repository.GetBy<VW_STUDENT_RESULT_2>(x => x.Session_Id == ss.Session.Id && x.Semester_Id == (int)Semesters.SecondSemester && x.Programme_Id == programme.Id
                               && x.Department_Id == department.Id && x.Level_Id == level.Id && (x.Activated == true || x.Activated == null))
                                                 select new Result
                                                 {
                                                     StudentId = sr.Person_Id,

                                                 }).Select(f => f.StudentId).Distinct().ToList();
                }
                List<long> distinctStudents = results.Select(r => r.StudentId).Distinct().ToList();
                for (int i = 0; i < distinctStudents.Count; i++)
                {
                    
                    long currentStudentId = distinctStudents[i];
                    //remove this student from the graduation list of first semester
                    //if((currentStudentId== 87904 || currentStudentId == 85518) && sessionSemester?.Id>0 && sessionSemester.Id == 13)
                    //{
                    //    continue;
                    //}
                    //remove this student from the graduation list of first semester
                    if (studentsWithMoreSemesters.Contains(currentStudentId))
                        continue;
                    Result currentStudentResult = results.Where(r => r.StudentId == currentStudentId).LastOrDefault();
                    //Result confirmedResult = GetStudentResultDetails(currentStudentId, level.Id, department.Id, programme.Id, ss.Semester.Id, ss.Session.Id);
                    Result result = ViewProcessedStudentResultExtraYear(currentStudentId, sessionSemester, level, programme, department);
                    if (result != null && result.Remark!=null && !result.Remark.Contains("CO-") && !result.Remark.Contains("PROBATION"))
                    {
                        currentStudentResult.CGPA = result.CGPA;
                        currentStudentResult.Remark = result.Remark;
                        currentStudentResult.Identifier = identifier;

                        currentStudentResult.Count = 1;

                        masterSheetResult.Add(currentStudentResult);
                    }

                }
                switch (type)
                {
                    case 1:
                        return masterSheetResult.OrderBy(a => a.Remark).ToList();
                        break;
                    case 2:
                        return masterSheetResult.OrderByDescending(a => a.CGPA).ToList();
                        break;
                    case 3:
                        return masterSheetResult.OrderBy(a => a.MatricNumber).ToList();
                        break;
                    case 4:
                        return masterSheetResult.OrderBy(a => a.Name).ToList();
                        break;
                }
            }
            catch (Exception)
            {
                throw;
            }

            return masterSheetResult.OrderBy(a => a.MatricNumber).ToList();
        }
        public List<Result> GetGraduationListByCapacity(SessionSemester sessionSemester, Level level, Programme programme, Department department, int type)
        {
            List<Result> masterSheetResult = new List<Result>();
            try
            {
                if (sessionSemester == null || sessionSemester.Id <= 0 || level == null || level.Id <= 0 || programme == null || programme.Id <= 0 || department == null || department.Id <= 0)
                {
                    throw new Exception("One or more criteria to get Mater Result Sheet not set! Please check your input criteria selection and try again.");
                }

                SessionSemesterLogic sessionSemesterLogic = new Business.SessionSemesterLogic();
                SessionSemester ss = sessionSemesterLogic.GetBy(sessionSemester.Id);
                SessionLogic sessionLogic = new SessionLogic();
                Session sessions = sessionLogic.GetModelBy(p => p.Session_Id == ss.Session.Id);
                string[] sessionItems = ss.Session.Name.Split('/');
                string sessionNameStr = sessionItems[0];
                int sessionNameInt = Convert.ToInt32(sessionNameStr);
                List<Result> results = null;

                string currentSessionSuffix = sessionNameStr.Substring(2, 2);
                string yearTwoSessionSuffix = Convert.ToString((Convert.ToInt32(currentSessionSuffix) - 1));

                currentSessionSuffix = "/" + currentSessionSuffix + "/";
                yearTwoSessionSuffix = "/" + yearTwoSessionSuffix + "/";

                string identifier = null;
                string departmentCode = GetDepartmentCode(department.Id);
                string levels = GetLevelName(level.Id);
                string semesterCode = GetSemesterCodeBy(ss.Semester.Id);
                string sessionCode = GetSessionCodeBy(ss.Session.Name);
                identifier = departmentCode + levels + semesterCode + sessionCode;


                if (ss.Session.Id == (int)Sessions._20152016)
                {
                    results = (from sr in repository.GetBy<VW_STUDENT_RESULT_2>(x => x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id && x.Level_Id == level.Id && x.Programme_Id == programme.Id && x.Department_Id == department.Id && (x.Activated == true || x.Activated == null))
                               select new Result
                               {
                                   StudentId = sr.Person_Id,
                                   Sex = sr.Sex_Name,
                                   Name = sr.Name,
                                   MatricNumber = sr.Matric_Number,
                                   CourseId = sr.Course_Id,
                                   CourseCode = sr.Course_Code,
                                   CourseName = sr.Course_Name,
                                   CourseUnit = sr.Course_Unit,
                                   SpecialCase = sr.Special_Case,
                                   TestScore = sr.Test_Score,
                                   ExamScore = sr.Exam_Score,
                                   Score = sr.Total_Score,
                                   Grade = sr.Grade,
                                   GradePoint = sr.Grade_Point,
                                   DepartmentName = sr.Department_Name,
                                   ProgrammeName = sr.Programme_Name,
                                   LevelName = sr.Level_Name,
                                   Semestername = sr.Semester_Name,
                                   GPCU = sr.Grade_Point * sr.Course_Unit,
                                   TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                   SessionName = sr.Session_Name
                               }).ToList();

                }
                else
                {
                    results = (from sr in repository.GetBy<VW_STUDENT_RESULT_2>(x => x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id && x.Programme_Id == programme.Id && x.Department_Id == department.Id && x.Level_Id == level.Id && (x.Activated == true || x.Activated == null))
                               select new Result
                               {
                                   StudentId = sr.Person_Id,
                                   Sex = sr.Sex_Name,
                                   Name = sr.Name.ToUpper(),
                                   MatricNumber = sr.Matric_Number,
                                   CourseId = sr.Course_Id,
                                   CourseCode = sr.Course_Code,
                                   CourseName = sr.Course_Name,
                                   CourseUnit = sr.Course_Unit,
                                   SpecialCase = sr.Special_Case,
                                   TestScore = sr.Test_Score,
                                   ExamScore = sr.Exam_Score,
                                   Score = sr.Total_Score,
                                   Grade = sr.Grade,
                                   GradePoint = sr.Grade_Point,
                                   DepartmentName = sr.Department_Name,
                                   ProgrammeName = sr.Programme_Name,
                                   LevelName = sr.Level_Name,
                                   Semestername = sr.Semester_Name,
                                   GPCU = sr.Grade_Point * sr.Course_Unit,
                                   TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                   SessionName = sr.Session_Name
                               }).ToList();

                    List<Result> resultList = new List<Result>();

                    for (int i = 0; i < results.Count; i++)
                    {
                        if (results[i].MatricNumber.Contains(yearTwoSessionSuffix))
                        {
                            resultList.Add(results[i]);
                        }
                        else
                        {
                            //Do Nothing
                        }
                        //if (results[i].MatricNumber.Contains("/16/") || results[i].MatricNumber.Contains("/13/") || results[i].MatricNumber.Contains("/14/"))
                        //{
                        //    //Do Nothing
                        //}
                        //else
                        //{
                        //    resultList.Add(results[i]);
                        //}
                    }

                    results = new List<Result>();
                    results = resultList;
                }

                List<long> distinctStudents = new List<long>();

                DepartmentCapacityLogic capacityLogic = new DepartmentCapacityLogic();
                DepartmentCapacity capacity = capacityLogic.GetModelsBy(c => c.Programme_Id == programme.Id && c.Department_Id == department.Id && c.Session_Id == ss.Session.Id && c.Activated).LastOrDefault();
                if (capacity != null)
                {
                    distinctStudents = results.Select(r => r.StudentId).Distinct().Take(capacity.Capacity).ToList();
                }
                else
                {
                    distinctStudents = results.Select(r => r.StudentId).Distinct().ToList();
                }
                
                for (int i = 0; i < distinctStudents.Count; i++)
                {
                    long currentStudentId = distinctStudents[i];
                    Result currentStudentResult = results.Where(r => r.StudentId == currentStudentId).LastOrDefault();
                    Result confirmedResult = GetStudentResultDetails(currentStudentId, level.Id, department.Id, programme.Id, ss.Semester.Id, ss.Session.Id);
                    if (confirmedResult != null)
                    {
                        currentStudentResult.CGPA = confirmedResult.CGPA;
                        currentStudentResult.Remark = confirmedResult.Remark;
                        currentStudentResult.Identifier = identifier;
                        currentStudentResult.Count = 1;
                        masterSheetResult.Add(currentStudentResult);
                    }

                }

                switch (type)
                {
                    case 1:
                        return masterSheetResult.OrderBy(a => a.Remark).ToList();
                        break;
                    case 2:
                        return masterSheetResult.OrderByDescending(a => a.CGPA).ToList();
                        break;
                    case 3:
                        return masterSheetResult.OrderBy(a => a.MatricNumber).ToList();
                        break;
                    case 4:
                        return masterSheetResult.OrderBy(a => a.Name).ToList();
                        break;
                }
            }
            catch (Exception)
            {
                throw;
            }

            return masterSheetResult.OrderBy(a => a.MatricNumber).ToList();
        }
        public List<Result> GetDiplomaClassList(SessionSemester sessionSemester, Level level, Programme programme, Department department)
        {
            List<Result> masterSheetResult = new List<Result>();
            try
            {
                if (sessionSemester == null || sessionSemester.Id <= 0 || level == null || level.Id <= 0 || programme == null || programme.Id <= 0 || department == null || department.Id <= 0)
                {
                    throw new Exception("One or more criteria to get Mater Result Sheet not set! Please check your input criteria selection and try again.");
                }

                SessionSemesterLogic sessionSemesterLogic = new Business.SessionSemesterLogic();
                SessionSemester ss = sessionSemesterLogic.GetBy(sessionSemester.Id);
                SessionLogic sessionLogic = new SessionLogic();
                Session sessions = sessionLogic.GetModelBy(p => p.Session_Id == ss.Session.Id);
                string[] sessionItems = ss.Session.Name.Split('/');
                string sessionNameStr = sessionItems[0];
                int sessionNameInt = Convert.ToInt32(sessionNameStr);
                List<Result> results = null;

                string identifier = null;
                string departmentCode = GetDepartmentCode(department.Id);
                string levels = GetLevelName(level.Id);
                string semesterCode = GetSemesterCodeBy(ss.Semester.Id);
                string sessionCode = GetSessionCodeBy(ss.Session.Name);
                identifier = departmentCode + levels + semesterCode + sessionCode;


                if (ss.Session.Id == (int)Sessions._20152016)
                {
                    results = (from sr in repository.GetBy<VW_STUDENT_RESULT_2>(x => x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id && x.Level_Id == level.Id && x.Programme_Id == programme.Id && x.Department_Id == department.Id && (x.Activated == true || x.Activated == null))
                               select new Result
                               {
                                   StudentId = sr.Person_Id,
                                   Sex = sr.Sex_Name,
                                   Name = sr.Name,
                                   MatricNumber = sr.Matric_Number,
                                   CourseId = sr.Course_Id,
                                   CourseCode = sr.Course_Code,
                                   CourseName = sr.Course_Name,
                                   CourseUnit = sr.Course_Unit,
                                   SpecialCase = sr.Special_Case,
                                   TestScore = sr.Test_Score,
                                   ExamScore = sr.Exam_Score,
                                   Score = sr.Total_Score,
                                   Grade = sr.Grade,
                                   GradePoint = sr.Grade_Point,
                                   DepartmentName = sr.Department_Name,
                                   ProgrammeName = sr.Programme_Name,
                                   LevelName = sr.Level_Name,
                                   Semestername = sr.Semester_Name,
                                   GPCU = sr.Grade_Point * sr.Course_Unit,
                                   TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                   SessionName = sr.Session_Name
                               }).ToList();

                }
                else
                {
                    results = (from sr in repository.GetBy<VW_STUDENT_RESULT_2>(x => x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id && x.Programme_Id == programme.Id && x.Department_Id == department.Id && x.Level_Id == level.Id && (x.Activated == true || x.Activated == null))
                               select new Result
                               {
                                   StudentId = sr.Person_Id,
                                   Sex = sr.Sex_Name,
                                   Name = sr.Name.ToUpper(),
                                   MatricNumber = sr.Matric_Number,
                                   CourseId = sr.Course_Id,
                                   CourseCode = sr.Course_Code,
                                   CourseName = sr.Course_Name,
                                   CourseUnit = sr.Course_Unit,
                                   SpecialCase = sr.Special_Case,
                                   TestScore = sr.Test_Score,
                                   ExamScore = sr.Exam_Score,
                                   Score = sr.Total_Score,
                                   Grade = sr.Grade,
                                   GradePoint = sr.Grade_Point,
                                   DepartmentName = sr.Department_Name,
                                   ProgrammeName = sr.Programme_Name,
                                   LevelName = sr.Level_Name,
                                   Semestername = sr.Semester_Name,
                                   GPCU = sr.Grade_Point * sr.Course_Unit,
                                   TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                   SessionName = sr.Session_Name
                               }).ToList();

                    //List<Result> resultList = new List<Result>();

                    //for (int i = 0; i < results.Count; i++)
                    //{
                    //    if (results[i].MatricNumber.Contains("/16/") || results[i].MatricNumber.Contains("/13/") || results[i].MatricNumber.Contains("/14/"))
                    //    {
                    //        //Do Nothing
                    //    }
                    //    else
                    //    {
                    //        resultList.Add(results[i]);
                    //    }
                    //}

                    //results = new List<Result>();
                    // results = resultList;
                }


                List<long> distinctStudents = results.Select(r => r.StudentId).Distinct().ToList();
                for (int i = 0; i < distinctStudents.Count; i++)
                {
                    long currentStudentId = distinctStudents[i];
                    Result currentStudentResult = results.Where(r => r.StudentId == currentStudentId).LastOrDefault();
                    //Result confirmedResult = GetStudentResultDetails(currentStudentId, level.Id, department.Id, programme.Id, ss.Semester.Id, ss.Session.Id);
                    Result confirmedResult = GetStudentResultBy(ss, level, programme, department, new Student() { Id = currentStudentId }).LastOrDefault();
                    if (confirmedResult != null)
                    {
                        currentStudentResult.CGPA = confirmedResult.CGPA;
                        currentStudentResult.Remark = confirmedResult.Remark;
                        currentStudentResult.Identifier = identifier;
                        currentStudentResult.Count = 1;
                        masterSheetResult.Add(currentStudentResult);
                    }

                }

            }
            catch (Exception)
            {
                throw;
            }

            return masterSheetResult.OrderBy(a => a.MatricNumber).ToList();
        }
        public async Task<List<Result>> GetGraduationListByOption(SessionSemester sessionSemester, Level level, Programme programme, Department department, DepartmentOption departmentOption, int type)
        {
            List<Result> masterSheetResult = new List<Result>();
            try
            {
                if (sessionSemester == null || sessionSemester.Id <= 0 || level == null || level.Id <= 0 || programme == null || programme.Id <= 0 || department == null || department.Id <= 0)
                {
                    throw new Exception("One or more criteria to get Mater Result Sheet not set! Please check your input criteria selection and try again.");
                }

                SessionSemesterLogic sessionSemesterLogic = new Business.SessionSemesterLogic();
                SessionSemester ss = sessionSemesterLogic.GetBy(sessionSemester.Id);
                SessionLogic sessionLogic = new SessionLogic();
                Session sessions = sessionLogic.GetModelBy(p => p.Session_Id == ss.Session.Id);
                string[] sessionItems = ss.Session.Name.Split('/');
                string sessionNameStr = sessionItems[0];
                int sessionNameInt = Convert.ToInt32(sessionNameStr);
                List<Result> results = null;

                string currentSessionSuffix = sessionNameStr.Substring(2, 2);
                string yearTwoSessionSuffix = Convert.ToString((Convert.ToInt32(currentSessionSuffix) - 1));

                currentSessionSuffix = "/" + currentSessionSuffix + "/";
                yearTwoSessionSuffix = "/" + yearTwoSessionSuffix + "/";

                string identifier = null;
                string departmentCode = GetDepartmentCode(department.Id);
                string levels = GetLevelName(level.Id);
                string semesterCode = GetSemesterCodeBy(ss.Semester.Id);
                string sessionCode = GetSessionCodeBy(ss.Session.Name);
                identifier = departmentCode + levels + semesterCode + sessionCode;


                DepartmentOptionLogic departmentOptionLogic = new DepartmentOptionLogic();
                DepartmentOption option = departmentOptionLogic.GetModelBy(d => d.Department_Option_Id == departmentOption.Id);

                if (ss.Session.Id == (int)Sessions._20152016)
                {
                    results = (from sr in await repository.GetByAsync<VW_STUDENT_RESULT_WITH_OPTIONS>(
                                 x =>
                                     x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id &&
                                     x.Level_Id == level.Id && x.Programme_Id == programme.Id &&
                                     x.Department_Id == department.Id &&
                                     x.Department_Option_Id == departmentOption.Id &&
                                     (x.Activated != false || x.Activated == null))
                               select new Result
                               {
                                   StudentId = sr.Person_Id,
                                   Sex = sr.Sex_Name,
                                   Name = sr.Name,
                                   MatricNumber = sr.Matric_Number,
                                   CourseId = sr.Course_Id,
                                   CourseCode = sr.Course_Code,
                                   CourseName = sr.Course_Name,
                                   CourseUnit = (int)sr.Course_Unit,
                                   SpecialCase = sr.Special_Case,
                                   TestScore = sr.Test_Score,
                                   ExamScore = sr.Exam_Score,
                                   Score = sr.Total_Score,
                                   Grade = sr.Grade,
                                   GradePoint = sr.Grade_Point,
                                   DepartmentName = sr.Department_Name,
                                   ProgrammeName = sr.Programme_Name,
                                   LevelName = sr.Level_Name,
                                   Semestername = sr.Semester_Name,
                                   GPCU = sr.Grade_Point * sr.Course_Unit,
                                   TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                   DepartmentOptionId = sr.Department_Option_Id,
                                   DepartmentOptionName = sr.Department_Option_Name,
                                   SessionName = sr.Session_Name
                               }).ToList();
                }
                else
                {
                    results =
                        (from sr in await
                             repository.GetByAsync<VW_STUDENT_RESULT_WITH_OPTIONS>(
                                 x =>
                                     x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id &&
                                     x.Level_Id == level.Id &&
                                     x.Programme_Id == programme.Id && x.Department_Id == department.Id &&
                                     x.Department_Option_Id == departmentOption.Id &&
                                     (x.Activated != false || x.Activated == null))
                         select new Result
                         {
                             StudentId = sr.Person_Id,
                             Sex = sr.Sex_Name,
                             Name = sr.Name.ToUpper(),
                             MatricNumber = sr.Matric_Number,
                             CourseId = sr.Course_Id,
                             CourseCode = sr.Course_Code,
                             CourseName = sr.Course_Name,
                             CourseUnit = (int)sr.Course_Unit,
                             SpecialCase = sr.Special_Case,
                             TestScore = sr.Test_Score,
                             ExamScore = sr.Exam_Score,
                             Score = sr.Total_Score,
                             Grade = sr.Grade,
                             GradePoint = sr.Grade_Point,
                             DepartmentName = sr.Department_Name,
                             ProgrammeName = sr.Programme_Name,
                             LevelName = sr.Level_Name,
                             Semestername = sr.Semester_Name,
                             GPCU = sr.Grade_Point * sr.Course_Unit,
                             TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                             DepartmentOptionId = sr.Department_Option_Id,
                             DepartmentOptionName = sr.Department_Option_Name,
                             SessionName = sr.Session_Name
                         }).ToList();

                    List<Result> resultList = new List<Result>();

                    for (int i = 0; i < results.Count; i++)
                    {
                        if (results[i].MatricNumber.Contains(yearTwoSessionSuffix))
                        {
                            resultList.Add(results[i]);
                        }
                        else
                        {
                            //Do Nothing
                        }
                        //if (results[i].MatricNumber.Contains("/16/") || results[i].MatricNumber.Contains("/13/") || results[i].MatricNumber.Contains("/14/"))
                        //{
                        //    //Do Nothing
                        //}
                        //else
                        //{
                        //    resultList.Add(results[i]);
                        //}
                    }

                    results = new List<Result>();
                    results = resultList;
                }

                List<Result> classResult = (from sr in await repository.GetByAsync<VW_STUDENT_RESULT_2>(x => x.Programme_Id == programme.Id && x.Department_Id == department.Id &&
                                                  (x.Activated == true || x.Activated == null))
                                            select new Result
                                            {
                                                StudentId = sr.Person_Id,
                                                Sex = sr.Sex_Name,
                                                Name = sr.Name,
                                                MatricNumber = sr.Matric_Number,
                                                CourseId = sr.Course_Id,
                                                CourseCode = sr.Course_Code,
                                                CourseName = sr.Course_Name,
                                                CourseUnit = sr.Course_Unit,
                                                SpecialCase = sr.Special_Case,
                                                TestScore = sr.Test_Score,
                                                ExamScore = sr.Exam_Score,
                                                Score = sr.Total_Score,
                                                Grade = sr.Grade,
                                                GradePoint = sr.Grade_Point,
                                                DepartmentName = sr.Department_Name,
                                                ProgrammeName = sr.Programme_Name,
                                                LevelName = sr.Level_Name,
                                                Semestername = sr.Semester_Name,
                                                GPCU = sr.Grade_Point * sr.Course_Unit,
                                                TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                                SessionName = sr.Session_Name,
                                                CourseModeId = sr.Course_Mode_Id
                                            }).ToList();

                List<long> distinctStudents = results.Select(r => r.StudentId).Distinct().ToList();
                for (int i = 0; i < distinctStudents.Count; i++)
                {
                    long currentStudentId = distinctStudents[i];
                    Result currentStudentResult = results.Where(r => r.StudentId == currentStudentId).LastOrDefault();
                    Result confirmedResult = GetStudentResultDetails(currentStudentId, level.Id, department.Id, programme.Id, ss.Semester.Id, ss.Session.Id);

                    bool hasSpecialCaseOutstanding = CheckSpecialCase(classResult, currentStudentId);

                    if (confirmedResult != null && !confirmedResult.Remark.Contains("CO-") && !hasSpecialCaseOutstanding)
                    {
                        currentStudentResult.CGPA = confirmedResult.CGPA;
                        currentStudentResult.Remark = confirmedResult.Remark;
                        currentStudentResult.Identifier = identifier;

                        currentStudentResult.Count = 1;

                        masterSheetResult.Add(currentStudentResult);
                    }

                }

                switch (type)
                {
                    case 1:
                        return masterSheetResult.OrderBy(a => a.Remark).ToList();
                        break;
                    case 2:
                        return masterSheetResult.OrderByDescending(a => a.CGPA).ToList();
                        break;
                    case 3:
                        return masterSheetResult.OrderBy(a => a.MatricNumber).ToList();
                        break;
                    case 4:
                        return masterSheetResult.OrderBy(a => a.Name).ToList();
                        break;
                }
            }
            catch (Exception)
            {
                throw;
            }

            return masterSheetResult.OrderBy(a => a.MatricNumber).ToList();
        }
        public List<Result> GetGraduationListByOptionFirstYear(SessionSemester sessionSemester, Level level, Programme programme, Department department, DepartmentOption departmentOption, int type)
        {
            List<Result> masterSheetResult = new List<Result>();
            try
            {
                if (sessionSemester == null || sessionSemester.Id <= 0 || level == null || level.Id <= 0 || programme == null || programme.Id <= 0 || department == null || department.Id <= 0)
                {
                    throw new Exception("One or more criteria to get Mater Result Sheet not set! Please check your input criteria selection and try again.");
                }

                SessionSemesterLogic sessionSemesterLogic = new Business.SessionSemesterLogic();
                SessionSemester ss = sessionSemesterLogic.GetBy(sessionSemester.Id);
                SessionLogic sessionLogic = new SessionLogic();
                Session sessions = sessionLogic.GetModelBy(p => p.Session_Id == ss.Session.Id);
                string[] sessionItems = ss.Session.Name.Split('/');
                string sessionNameStr = sessionItems[0];
                int sessionNameInt = Convert.ToInt32(sessionNameStr);
                List<Result> results = null;

                string currentSessionSuffix = sessionNameStr.Substring(2, 2);
                string yearTwoSessionSuffix = Convert.ToString((Convert.ToInt32(currentSessionSuffix) - 1));

                currentSessionSuffix = "/" + currentSessionSuffix + "/";
                yearTwoSessionSuffix = "/" + yearTwoSessionSuffix + "/";

                string identifier = null;
                string departmentCode = GetDepartmentCode(department.Id);
                string levels = GetLevelName(level.Id);
                string semesterCode = GetSemesterCodeBy(ss.Semester.Id);
                string sessionCode = GetSessionCodeBy(ss.Session.Name);
                identifier = departmentCode + levels + semesterCode + sessionCode;


                DepartmentOptionLogic departmentOptionLogic = new DepartmentOptionLogic();
                DepartmentOption option = departmentOptionLogic.GetModelBy(d => d.Department_Option_Id == departmentOption.Id);

                if (ss.Session.Id == (int)Sessions._20152016)
                {
                    results = (from sr in repository.GetBy<VW_STUDENT_RESULT_WITH_OPTIONS>(
                                 x =>
                                     x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id &&
                                     x.Level_Id == level.Id && x.Programme_Id == programme.Id &&
                                     x.Department_Id == department.Id &&
                                     x.Department_Option_Id == departmentOption.Id &&
                                     (x.Activated != false || x.Activated == null))
                               select new Result
                               {
                                   StudentId = sr.Person_Id,
                                   Sex = sr.Sex_Name,
                                   Name = sr.Name,
                                   MatricNumber = sr.Matric_Number,
                                   CourseId = sr.Course_Id,
                                   CourseCode = sr.Course_Code,
                                   CourseName = sr.Course_Name,
                                   CourseUnit = (int)sr.Course_Unit,
                                   SpecialCase = sr.Special_Case,
                                   TestScore = sr.Test_Score,
                                   ExamScore = sr.Exam_Score,
                                   Score = sr.Total_Score,
                                   Grade = sr.Grade,
                                   GradePoint = sr.Grade_Point,
                                   DepartmentName = sr.Department_Name,
                                   ProgrammeName = sr.Programme_Name,
                                   LevelName = sr.Level_Name,
                                   Semestername = sr.Semester_Name,
                                   GPCU = sr.Grade_Point * sr.Course_Unit,
                                   TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                   DepartmentOptionId = sr.Department_Option_Id,
                                   DepartmentOptionName = sr.Department_Option_Name,
                                   SessionName = sr.Session_Name
                               }).ToList();
                }
                else
                {
                    results =
                        (from sr in
                             repository.GetBy<VW_STUDENT_RESULT_WITH_OPTIONS>(
                                 x =>
                                     x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id &&
                                     x.Level_Id == level.Id &&
                                     x.Programme_Id == programme.Id && x.Department_Id == department.Id &&
                                     x.Department_Option_Id == departmentOption.Id &&
                                     (x.Activated != false || x.Activated == null))
                         select new Result
                         {
                             StudentId = sr.Person_Id,
                             Sex = sr.Sex_Name,
                             Name = sr.Name.ToUpper(),
                             MatricNumber = sr.Matric_Number,
                             CourseId = sr.Course_Id,
                             CourseCode = sr.Course_Code,
                             CourseName = sr.Course_Name,
                             CourseUnit = (int)sr.Course_Unit,
                             SpecialCase = sr.Special_Case,
                             TestScore = sr.Test_Score,
                             ExamScore = sr.Exam_Score,
                             Score = sr.Total_Score,
                             Grade = sr.Grade,
                             GradePoint = sr.Grade_Point,
                             DepartmentName = sr.Department_Name,
                             ProgrammeName = sr.Programme_Name,
                             LevelName = sr.Level_Name,
                             Semestername = sr.Semester_Name,
                             GPCU = sr.Grade_Point * sr.Course_Unit,
                             TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                             DepartmentOptionId = sr.Department_Option_Id,
                             DepartmentOptionName = sr.Department_Option_Name,
                             SessionName = sr.Session_Name
                         }).ToList();

                    List<Result> resultList = new List<Result>();

                    for (int i = 0; i < results.Count; i++)
                    {
                        if (results[i].MatricNumber.Contains(currentSessionSuffix))
                        {
                            resultList.Add(results[i]);
                        }
                        else
                        {
                            //Do Nothing
                        }
                        //if (results[i].MatricNumber.Contains("/16/") || results[i].MatricNumber.Contains("/13/") || results[i].MatricNumber.Contains("/14/"))
                        //{
                        //    //Do Nothing
                        //}
                        //else
                        //{
                        //    resultList.Add(results[i]);
                        //}
                    }

                    results = new List<Result>();
                    results = resultList;
                }

                List<Result> classResult = (from sr in repository.GetBy<VW_STUDENT_RESULT_2>(x => x.Programme_Id == programme.Id && x.Department_Id == department.Id &&
                                                  (x.Activated == true || x.Activated == null))
                                            select new Result
                                            {
                                                StudentId = sr.Person_Id,
                                                Sex = sr.Sex_Name,
                                                Name = sr.Name,
                                                MatricNumber = sr.Matric_Number,
                                                CourseId = sr.Course_Id,
                                                CourseCode = sr.Course_Code,
                                                CourseName = sr.Course_Name,
                                                CourseUnit = sr.Course_Unit,
                                                SpecialCase = sr.Special_Case,
                                                TestScore = sr.Test_Score,
                                                ExamScore = sr.Exam_Score,
                                                Score = sr.Total_Score,
                                                Grade = sr.Grade,
                                                GradePoint = sr.Grade_Point,
                                                DepartmentName = sr.Department_Name,
                                                ProgrammeName = sr.Programme_Name,
                                                LevelName = sr.Level_Name,
                                                Semestername = sr.Semester_Name,
                                                GPCU = sr.Grade_Point * sr.Course_Unit,
                                                TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                                SessionName = sr.Session_Name,
                                                CourseModeId = sr.Course_Mode_Id
                                            }).ToList();

                List<long> distinctStudents = results.Select(r => r.StudentId).Distinct().ToList();
                for (int i = 0; i < distinctStudents.Count; i++)
                {
                    long currentStudentId = distinctStudents[i];
                    Result currentStudentResult = results.Where(r => r.StudentId == currentStudentId).LastOrDefault();
                    //Result confirmedResult = GetStudentResultDetails(currentStudentId, level.Id, department.Id, programme.Id, ss.Semester.Id, ss.Session.Id);
                    Result confirmedResult = ViewProcessedStudentResult(currentStudentId, sessionSemester, level, programme, department);

                    bool hasSpecialCaseOutstanding = CheckSpecialCase(classResult, currentStudentId);

                    if (confirmedResult != null && !confirmedResult.Remark.Contains("CO-") && !hasSpecialCaseOutstanding)
                    {
                        currentStudentResult.CGPA = confirmedResult.CGPA;
                        currentStudentResult.Remark = confirmedResult.Remark;
                        currentStudentResult.Identifier = identifier;

                        currentStudentResult.Count = 1;

                        masterSheetResult.Add(currentStudentResult);
                    }

                }

                switch (type)
                {
                    case 1:
                        return masterSheetResult.OrderBy(a => a.Remark).ToList();
                        break;
                    case 2:
                        return masterSheetResult.OrderByDescending(a => a.CGPA).ToList();
                        break;
                    case 3:
                        return masterSheetResult.OrderBy(a => a.MatricNumber).ToList();
                        break;
                    case 4:
                        return masterSheetResult.OrderBy(a => a.Name).ToList();
                        break;
                }
            }
            catch (Exception)
            {
                throw;
            }

            return masterSheetResult.OrderBy(a => a.MatricNumber).ToList();
        }
        public List<Result> GetGraduationListByOptionExtraYear(SessionSemester sessionSemester, Level level, Programme programme, Department department, DepartmentOption departmentOption, int type)
        {
            List<Result> masterSheetResult = new List<Result>();
            try
            {
                if (sessionSemester == null || sessionSemester.Id <= 0 || level == null || level.Id <= 0 || programme == null || programme.Id <= 0 || department == null || department.Id <= 0)
                {
                    throw new Exception("One or more criteria to get Mater Result Sheet not set! Please check your input criteria selection and try again.");
                }

                SessionSemesterLogic sessionSemesterLogic = new Business.SessionSemesterLogic();
                SessionSemester ss = sessionSemesterLogic.GetBy(sessionSemester.Id);
                SessionLogic sessionLogic = new SessionLogic();
                Session sessions = sessionLogic.GetModelBy(p => p.Session_Id == ss.Session.Id);
                string[] sessionItems = ss.Session.Name.Split('/');
                string sessionNameStr = sessionItems[0];
                int sessionNameInt = Convert.ToInt32(sessionNameStr);
                List<Result> results = null;

                string currentSessionSuffix = sessionNameStr.Substring(2, 2);
                string yearTwoSessionSuffix = Convert.ToString((Convert.ToInt32(currentSessionSuffix) - 1));

                currentSessionSuffix = "/" + currentSessionSuffix + "/";
                yearTwoSessionSuffix = "/" + yearTwoSessionSuffix + "/";

                string identifier = null;
                string departmentCode = GetDepartmentCode(department.Id);
                string levels = GetLevelName(level.Id);
                string semesterCode = GetSemesterCodeBy(ss.Semester.Id);
                string sessionCode = GetSessionCodeBy(ss.Session.Name);
                string extraYearIdentifiersuffix = null;
                if (programme.Id == (int)Programmes.NDPartTime)
                {
                    extraYearIdentifiersuffix = "YC2";
                }
                else
                {
                    extraYearIdentifiersuffix = levels.Substring(0, 1) + "C2";
                }
                identifier = departmentCode + extraYearIdentifiersuffix + semesterCode + sessionCode;

                StudentDefermentLogic defermentLogic = new StudentDefermentLogic();
                List<StudentDeferementLog> deferementLogs = defermentLogic.GetAll();

                DepartmentOptionLogic departmentOptionLogic = new DepartmentOptionLogic();
                DepartmentOption option = departmentOptionLogic.GetModelBy(d => d.Department_Option_Id == departmentOption.Id);

                if (ss.Session.Id == (int)Sessions._20152016)
                {
                    results = (from sr in repository.GetBy<VW_STUDENT_RESULT_WITH_OPTIONS>(
                                 x =>
                                     x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id &&
                                     x.Level_Id == level.Id && x.Programme_Id == programme.Id &&
                                     x.Department_Id == department.Id &&
                                     x.Department_Option_Id == departmentOption.Id &&
                                     (x.Activated != false || x.Activated == null))
                               select new Result
                               {
                                   StudentId = sr.Person_Id,
                                   Sex = sr.Sex_Name,
                                   Name = sr.Name,
                                   MatricNumber = sr.Matric_Number,
                                   CourseId = sr.Course_Id,
                                   CourseCode = sr.Course_Code,
                                   CourseName = sr.Course_Name,
                                   CourseUnit = (int)sr.Course_Unit,
                                   SpecialCase = sr.Special_Case,
                                   TestScore = sr.Test_Score,
                                   ExamScore = sr.Exam_Score,
                                   Score = sr.Total_Score,
                                   Grade = sr.Grade,
                                   GradePoint = sr.Grade_Point,
                                   DepartmentName = sr.Department_Name,
                                   ProgrammeName = sr.Programme_Name,
                                   LevelName = sr.Level_Name,
                                   Semestername = sr.Semester_Name,
                                   GPCU = sr.Grade_Point * sr.Course_Unit,
                                   TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                   DepartmentOptionId = sr.Department_Option_Id,
                                   DepartmentOptionName = sr.Department_Option_Name,
                                   SessionName = sr.Session_Name
                               }).ToList();
                }
                else
                {
                    results =
                        (from sr in
                             repository.GetBy<VW_STUDENT_RESULT_WITH_OPTIONS>(
                                 x =>
                                     x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id &&
                                     x.Level_Id == level.Id &&
                                     x.Programme_Id == programme.Id && x.Department_Id == department.Id &&
                                     x.Department_Option_Id == departmentOption.Id &&
                                     (x.Activated != false || x.Activated == null))
                         select new Result
                         {
                             StudentId = sr.Person_Id,
                             Sex = sr.Sex_Name,
                             Name = sr.Name.ToUpper(),
                             MatricNumber = sr.Matric_Number,
                             CourseId = sr.Course_Id,
                             CourseCode = sr.Course_Code,
                             CourseName = sr.Course_Name,
                             CourseUnit = (int)sr.Course_Unit,
                             SpecialCase = sr.Special_Case,
                             TestScore = sr.Test_Score,
                             ExamScore = sr.Exam_Score,
                             Score = sr.Total_Score,
                             Grade = sr.Grade,
                             GradePoint = sr.Grade_Point,
                             DepartmentName = sr.Department_Name,
                             ProgrammeName = sr.Programme_Name,
                             LevelName = sr.Level_Name,
                             Semestername = sr.Semester_Name,
                             GPCU = sr.Grade_Point * sr.Course_Unit,
                             TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                             DepartmentOptionId = sr.Department_Option_Id,
                             DepartmentOptionName = sr.Department_Option_Name,
                             SessionName = sr.Session_Name
                         }).ToList();

                    List<Result> resultList = new List<Result>();

                    for (int i = 0; i < results.Count; i++)
                    {
                        //if (results[i].MatricNumber.Contains(yearTwoSessionSuffix))
                        //{
                        //    resultList.Add(results[i]);
                        //}
                        //else
                        //{
                        //    //Do Nothing
                        //}
                        if (results[i].MatricNumber.Contains(currentSessionSuffix) || results[i].MatricNumber.Contains(yearTwoSessionSuffix) || results[i].MatricNumber.Contains("/14/") ||
                             results[i].MatricNumber.Contains("/13/") || results[i].MatricNumber.Contains("/12/") || results[i].MatricNumber.Contains("/11/"))
                        {
                            //Do Nothing
                        }
                        else
                        {
                            if (deferementLogs.LastOrDefault(s => s.Student.MatricNumber == results[i].MatricNumber) == null)
                            {
                                resultList.Add(results[i]);
                            }
                        }
                    }

                    results = new List<Result>();
                    results = resultList;
                }

                //Get students with more carry over semesters
                List<long> studentsWithMoreSemesters = new List<long>();
                if (ss.Semester.Id == 1)
                {
                    studentsWithMoreSemesters = (from sr in repository.GetBy<VW_STUDENT_RESULT_2>(x => x.Session_Id == ss.Session.Id && x.Semester_Id == (int)Semesters.SecondSemester && x.Programme_Id == programme.Id
                               && x.Department_Id == department.Id && x.Level_Id == level.Id && (x.Activated == true || x.Activated == null))
                                                 select new Result
                                                 {
                                                     StudentId = sr.Person_Id,

                                                 }).Select(f => f.StudentId).Distinct().ToList();
                }
                List<long> distinctStudents = results.Select(r => r.StudentId).Distinct().ToList();
                for (int i = 0; i < distinctStudents.Count; i++)
                {
                    long currentStudentId = distinctStudents[i];
                    //remove this student from the graduation list of first semester
                    //if ((currentStudentId == 80575) && sessionSemester?.Id > 0 && sessionSemester.Id == 13)
                    //{
                    //    continue;
                    //}

                    if (studentsWithMoreSemesters.Contains(currentStudentId))
                        continue;
                    Result currentStudentResult = results.Where(r => r.StudentId == currentStudentId).LastOrDefault();
                    //Result confirmedResult = GetStudentResultDetails(currentStudentId, level.Id, department.Id, programme.Id, ss.Semester.Id, ss.Session.Id);
                    Result result = ViewProcessedStudentResultExtraYear(currentStudentId, sessionSemester, level, programme, department);
                    if (result != null && !result.Remark.Contains("CO-") && !result.Remark.Contains("PROBATION"))
                    {
                        currentStudentResult.CGPA = result.CGPA;
                        currentStudentResult.Remark = result.Remark;
                        currentStudentResult.Identifier = identifier;

                        currentStudentResult.Count = 1;

                        masterSheetResult.Add(currentStudentResult);
                    }

                }

                switch (type)
                {
                    case 1:
                        return masterSheetResult.OrderBy(a => a.Remark).ToList();
                        break;
                    case 2:
                        return masterSheetResult.OrderByDescending(a => a.CGPA).ToList();
                        break;
                    case 3:
                        return masterSheetResult.OrderBy(a => a.MatricNumber).ToList();
                        break;
                    case 4:
                        return masterSheetResult.OrderBy(a => a.Name).ToList();
                        break;
                }
            }
            catch (Exception)
            {
                throw;
            }

            return masterSheetResult.OrderBy(a => a.MatricNumber).ToList();
        }
        public List<Result> GetGraduationListByOptionCapacity(SessionSemester sessionSemester, Level level, Programme programme, Department department, DepartmentOption departmentOption, int type)
        {
            List<Result> masterSheetResult = new List<Result>();
            try
            {
                if (sessionSemester == null || sessionSemester.Id <= 0 || level == null || level.Id <= 0 || programme == null || programme.Id <= 0 || department == null || department.Id <= 0)
                {
                    throw new Exception("One or more criteria to get Mater Result Sheet not set! Please check your input criteria selection and try again.");
                }

                SessionSemesterLogic sessionSemesterLogic = new Business.SessionSemesterLogic();
                SessionSemester ss = sessionSemesterLogic.GetBy(sessionSemester.Id);
                SessionLogic sessionLogic = new SessionLogic();
                Session sessions = sessionLogic.GetModelBy(p => p.Session_Id == ss.Session.Id);
                string[] sessionItems = ss.Session.Name.Split('/');
                string sessionNameStr = sessionItems[0];
                int sessionNameInt = Convert.ToInt32(sessionNameStr);
                List<Result> results = null;

                string currentSessionSuffix = sessionNameStr.Substring(2, 2);
                string yearTwoSessionSuffix = Convert.ToString((Convert.ToInt32(currentSessionSuffix) - 1));

                currentSessionSuffix = "/" + currentSessionSuffix + "/";
                yearTwoSessionSuffix = "/" + yearTwoSessionSuffix + "/";

                string identifier = null;
                string departmentCode = GetDepartmentCode(department.Id);
                string levels = GetLevelName(level.Id);
                string semesterCode = GetSemesterCodeBy(ss.Semester.Id);
                string sessionCode = GetSessionCodeBy(ss.Session.Name);
                identifier = departmentCode + levels + semesterCode + sessionCode;


                DepartmentOptionLogic departmentOptionLogic = new DepartmentOptionLogic();
                DepartmentOption option = departmentOptionLogic.GetModelBy(d => d.Department_Option_Id == departmentOption.Id);

                if (ss.Session.Id == (int)Sessions._20152016)
                {
                    results = (from sr in repository.GetBy<VW_STUDENT_RESULT_WITH_OPTIONS>(
                                 x =>
                                     x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id &&
                                     x.Level_Id == level.Id && x.Programme_Id == programme.Id &&
                                     x.Department_Id == department.Id &&
                                     x.Department_Option_Id == departmentOption.Id &&
                                     (x.Activated != false || x.Activated == null))
                               select new Result
                               {
                                   StudentId = sr.Person_Id,
                                   Sex = sr.Sex_Name,
                                   Name = sr.Name,
                                   MatricNumber = sr.Matric_Number,
                                   CourseId = sr.Course_Id,
                                   CourseCode = sr.Course_Code,
                                   CourseName = sr.Course_Name,
                                   CourseUnit = (int)sr.Course_Unit,
                                   SpecialCase = sr.Special_Case,
                                   TestScore = sr.Test_Score,
                                   ExamScore = sr.Exam_Score,
                                   Score = sr.Total_Score,
                                   Grade = sr.Grade,
                                   GradePoint = sr.Grade_Point,
                                   DepartmentName = sr.Department_Name,
                                   ProgrammeName = sr.Programme_Name,
                                   LevelName = sr.Level_Name,
                                   Semestername = sr.Semester_Name,
                                   GPCU = sr.Grade_Point * sr.Course_Unit,
                                   TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                   DepartmentOptionId = sr.Department_Option_Id,
                                   DepartmentOptionName = sr.Department_Option_Name,
                                   SessionName = sr.Session_Name
                               }).ToList();
                }
                else
                {
                    results =
                        (from sr in
                             repository.GetBy<VW_STUDENT_RESULT_WITH_OPTIONS>(
                                 x =>
                                     x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id &&
                                     x.Level_Id == level.Id &&
                                     x.Programme_Id == programme.Id && x.Department_Id == department.Id &&
                                     x.Department_Option_Id == departmentOption.Id &&
                                     (x.Activated != false || x.Activated == null))
                         select new Result
                         {
                             StudentId = sr.Person_Id,
                             Sex = sr.Sex_Name,
                             Name = sr.Name.ToUpper(),
                             MatricNumber = sr.Matric_Number,
                             CourseId = sr.Course_Id,
                             CourseCode = sr.Course_Code,
                             CourseName = sr.Course_Name,
                             CourseUnit = (int)sr.Course_Unit,
                             SpecialCase = sr.Special_Case,
                             TestScore = sr.Test_Score,
                             ExamScore = sr.Exam_Score,
                             Score = sr.Total_Score,
                             Grade = sr.Grade,
                             GradePoint = sr.Grade_Point,
                             DepartmentName = sr.Department_Name,
                             ProgrammeName = sr.Programme_Name,
                             LevelName = sr.Level_Name,
                             Semestername = sr.Semester_Name,
                             GPCU = sr.Grade_Point * sr.Course_Unit,
                             TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                             DepartmentOptionId = sr.Department_Option_Id,
                             DepartmentOptionName = sr.Department_Option_Name,
                             SessionName = sr.Session_Name
                         }).ToList();

                    List<Result> resultList = new List<Result>();

                    for (int i = 0; i < results.Count; i++)
                    {
                        if (results[i].MatricNumber.Contains(yearTwoSessionSuffix))
                        {
                            resultList.Add(results[i]);
                        }
                        else
                        {
                            //Do Nothing
                        }
                    }

                    results = new List<Result>();
                    results = resultList;
                }

                List<long> distinctStudents = new List<long>();

                DepartmentCapacityLogic capacityLogic = new DepartmentCapacityLogic();
                DepartmentCapacity capacity = capacityLogic.GetModelsBy(c => c.Programme_Id == programme.Id && c.Department_Id == department.Id && c.Session_Id == ss.Session.Id && c.Activated).LastOrDefault();
                if (capacity != null)
                {
                    distinctStudents = results.Select(r => r.StudentId).Distinct().Take(capacity.Capacity).ToList();
                }
                else
                {
                    distinctStudents = results.Select(r => r.StudentId).Distinct().ToList();
                }
                
                for (int i = 0; i < distinctStudents.Count; i++)
                {
                    long currentStudentId = distinctStudents[i];
                    Result currentStudentResult = results.Where(r => r.StudentId == currentStudentId).LastOrDefault();
                    Result confirmedResult = GetStudentResultDetails(currentStudentId, level.Id, department.Id, programme.Id, ss.Semester.Id, ss.Session.Id);
                    if (confirmedResult != null)
                    {
                        currentStudentResult.CGPA = confirmedResult.CGPA;
                        currentStudentResult.Remark = confirmedResult.Remark;
                        currentStudentResult.Identifier = identifier;

                        currentStudentResult.Count = 1;

                        masterSheetResult.Add(currentStudentResult);
                    }

                }

                switch (type)
                {
                    case 1:
                        return masterSheetResult.OrderBy(a => a.Remark).ToList();
                        break;
                    case 2:
                        return masterSheetResult.OrderByDescending(a => a.CGPA).ToList();
                        break;
                    case 3:
                        return masterSheetResult.OrderBy(a => a.MatricNumber).ToList();
                        break;
                    case 4:
                        return masterSheetResult.OrderBy(a => a.Name).ToList();
                        break;
                }
            }
            catch (Exception)
            {
                throw;
            }

            return masterSheetResult.OrderBy(a => a.MatricNumber).ToList();
        }
        public List<Result> GetDiplomaClassListByOption(SessionSemester sessionSemester, Level level, Programme programme, Department department, DepartmentOption departmentOption)
        {
            List<Result> masterSheetResult = new List<Result>();
            try
            {
                if (sessionSemester == null || sessionSemester.Id <= 0 || level == null || level.Id <= 0 || programme == null || programme.Id <= 0 || department == null || department.Id <= 0)
                {
                    throw new Exception("One or more criteria to get Mater Result Sheet not set! Please check your input criteria selection and try again.");
                }

                SessionSemesterLogic sessionSemesterLogic = new Business.SessionSemesterLogic();
                SessionSemester ss = sessionSemesterLogic.GetBy(sessionSemester.Id);
                SessionLogic sessionLogic = new SessionLogic();
                Session sessions = sessionLogic.GetModelBy(p => p.Session_Id == ss.Session.Id);
                string[] sessionItems = ss.Session.Name.Split('/');
                string sessionNameStr = sessionItems[0];
                int sessionNameInt = Convert.ToInt32(sessionNameStr);
                List<Result> results = null;

                string currentSessionSuffix = sessionNameStr.Substring(2, 2);
                string yearTwoSessionSuffix = Convert.ToString((Convert.ToInt32(currentSessionSuffix) - 1));

                currentSessionSuffix = "/" + currentSessionSuffix + "/";
                yearTwoSessionSuffix = "/" + yearTwoSessionSuffix + "/";

                string identifier = null;
                string departmentCode = GetDepartmentCode(department.Id);
                string levels = GetLevelName(level.Id);
                string semesterCode = GetSemesterCodeBy(ss.Semester.Id);
                string sessionCode = GetSessionCodeBy(ss.Session.Name);
                identifier = departmentCode + levels + semesterCode + sessionCode;


                DepartmentOptionLogic departmentOptionLogic = new DepartmentOptionLogic();
                DepartmentOption option = departmentOptionLogic.GetModelBy(d => d.Department_Option_Id == departmentOption.Id);

                if (ss.Session.Id == (int)Sessions._20152016)
                {
                    results = (from sr in repository.GetBy<VW_STUDENT_RESULT_WITH_OPTIONS>(
                                 x =>
                                     x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id &&
                                     x.Level_Id == level.Id && x.Programme_Id == programme.Id &&
                                     x.Department_Id == department.Id &&
                                     x.Department_Option_Id == departmentOption.Id &&
                                     (x.Activated != false || x.Activated == null))
                               select new Result
                               {
                                   StudentId = sr.Person_Id,
                                   Sex = sr.Sex_Name,
                                   Name = sr.Name,
                                   MatricNumber = sr.Matric_Number,
                                   CourseId = sr.Course_Id,
                                   CourseCode = sr.Course_Code,
                                   CourseName = sr.Course_Name,
                                   CourseUnit = (int)(int)sr.Course_Unit,
                                   SpecialCase = sr.Special_Case,
                                   TestScore = sr.Test_Score,
                                   ExamScore = sr.Exam_Score,
                                   Score = sr.Total_Score,
                                   Grade = sr.Grade,
                                   GradePoint = sr.Grade_Point,
                                   DepartmentName = sr.Department_Name,
                                   ProgrammeName = sr.Programme_Name,
                                   LevelName = sr.Level_Name,
                                   Semestername = sr.Semester_Name,
                                   GPCU = sr.Grade_Point * sr.Course_Unit,
                                   TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                   DepartmentOptionId = sr.Department_Option_Id,
                                   DepartmentOptionName = sr.Department_Option_Name,
                                   SessionName = sr.Session_Name
                               }).ToList();
                }
                else
                {
                    results =
                        (from sr in
                             repository.GetBy<VW_STUDENT_RESULT_WITH_OPTIONS>(
                                 x =>
                                     x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id &&
                                     x.Level_Id == level.Id &&
                                     x.Programme_Id == programme.Id && x.Department_Id == department.Id &&
                                     x.Department_Option_Id == departmentOption.Id &&
                                     (x.Activated != false || x.Activated == null))
                         select new Result
                         {
                             StudentId = sr.Person_Id,
                             Sex = sr.Sex_Name,
                             Name = sr.Name.ToUpper(),
                             MatricNumber = sr.Matric_Number,
                             CourseId = sr.Course_Id,
                             CourseCode = sr.Course_Code,
                             CourseName = sr.Course_Name,
                             CourseUnit = (int)sr.Course_Unit,
                             SpecialCase = sr.Special_Case,
                             TestScore = sr.Test_Score,
                             ExamScore = sr.Exam_Score,
                             Score = sr.Total_Score,
                             Grade = sr.Grade,
                             GradePoint = sr.Grade_Point,
                             DepartmentName = sr.Department_Name,
                             ProgrammeName = sr.Programme_Name,
                             LevelName = sr.Level_Name,
                             Semestername = sr.Semester_Name,
                             GPCU = sr.Grade_Point * sr.Course_Unit,
                             TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                             DepartmentOptionId = sr.Department_Option_Id,
                             DepartmentOptionName = sr.Department_Option_Name,
                             SessionName = sr.Session_Name
                         }).ToList();

                    List<Result> resultList = new List<Result>();

                    for (int i = 0; i < results.Count; i++)
                    {
                        if (results[i].MatricNumber.Contains(yearTwoSessionSuffix))
                        {
                            resultList.Add(results[i]);
                        }
                        else
                        {
                            //Do Nothing
                        }
                    }

                    results = new List<Result>();
                    results = resultList;
                }


                List<long> distinctStudents = results.Select(r => r.StudentId).Distinct().ToList();
                for (int i = 0; i < distinctStudents.Count; i++)
                {
                    long currentStudentId = distinctStudents[i];
                    Result currentStudentResult = results.Where(r => r.StudentId == currentStudentId).LastOrDefault();
                    //Result confirmedResult = GetStudentResultDetails(currentStudentId, level.Id, department.Id, programme.Id, ss.Semester.Id, ss.Session.Id);
                    Result confirmedResult = GetStudentResultBy(ss, level, programme, department, new Student() { Id = currentStudentId }).LastOrDefault();
                    if (confirmedResult != null)
                    {
                        currentStudentResult.CGPA = confirmedResult.CGPA;
                        currentStudentResult.Remark = confirmedResult.Remark;
                        currentStudentResult.Identifier = identifier;

                        currentStudentResult.Count = 1;

                        masterSheetResult.Add(currentStudentResult);
                    }

                }

            }
            catch (Exception)
            {
                throw;
            }

            return masterSheetResult.OrderBy(a => a.MatricNumber).ToList();
        }
        private Result GetStudentResultDetails(long studentId, int levelId, int departmentId, int programmeId, int semesterId, int sessionId)
        {
            Result overallResult = new Result();
            try
            {
                StudentLogic studentLogic = new StudentLogic();
                StudentResultLogic studentResultLogic = new StudentResultLogic();
                SessionSemesterLogic sessionSemesterLogic = new SessionSemesterLogic();

                Session session = new Session() { Id = sessionId };
                Programme programme = new Programme() { Id = programmeId };
                Department department = new Department() { Id = departmentId };
                Level level = new Level() { Id = levelId };
                SessionSemester sessionSemester = new SessionSemester();
                List<string> carryOverCourses = new List<string>();

                Abundance_Nk.Model.Model.Student studentCheck = studentLogic.GetModelBy(p => p.Person_Id == studentId);

                decimal firstYearFirstSemesterGPCUSum = 0;
                int firstYearFirstSemesterTotalCourseUnit = 0;
                decimal firstYearSecondSemesterGPCUSum = 0;
                int firstYearSecondSemesterTotalCourseUnit = 0;
                decimal secondYearFirstSemesterGPCUSum = 0;
                int secondYearFirstSemesterTotalCourseUnit = 0;
                decimal secondYearSecondSemesterGPCUSum = 0;
                int secondYearSecondSemesterTotalCourseUnit = 0;

                Result firstYearFirstSemester = new Result();
                Result firstYearSecondSemester = new Result();
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                STUDENT_LEVEL studentLevelEntity = new STUDENT_LEVEL();
                if (programmeId > 2)
                {
                    studentLevelEntity = studentLevelLogic.GetEntitiesBy(s => s.Level_Id == 3 && s.Person_Id == studentId && s.Department_Id == departmentId && s.Programme_Id == programmeId).LastOrDefault();
                }
                else
                {
                    studentLevelEntity = studentLevelLogic.GetEntitiesBy(s => s.Level_Id == 1 && s.Person_Id == studentId && s.Department_Id == departmentId && s.Programme_Id == programmeId).LastOrDefault();
                }

                if (studentLevelEntity != null)
                {
                    firstYearFirstSemester = GetFirstYearFirstSemesterResultInfo(level.Id, department, programme, studentCheck);
                    firstYearSecondSemester = GetFirstYearSecondSemesterResultInfo(level.Id, department, programme, studentCheck);
                }
                else
                {
                    return null;
                }

                if (semesterId == 1)
                {
                    List<Result> result = null;
                    Semester firstSemester = new Semester() { Id = 1 };
                    sessionSemester = sessionSemesterLogic.GetModelBy(s => s.Session_Id == sessionId && s.Semester_Id == firstSemester.Id);

                    carryOverCourses = GetSecondYearCarryOverCourses(sessionSemester, level, programme, department, studentCheck);
                    if (carryOverCourses.Count > 0)
                    {
                        return null;
                    }

                    if (studentCheck.Activated == true || studentCheck.Activated == null)
                    {
                        result = studentResultLogic.GetStudentProcessedResultBy(session, level, department, studentCheck, firstSemester, programme);
                    }
                    else
                    {
                        // result = studentResultLogic.GetDeactivatedStudentProcessedResultBy(session, level, department, studentCheck, firstSemester, programme);
                        return null;
                    }
                    List<Result> modifiedResultList = new List<Result>();
                    int totalSemesterCourseUnit = 0;
                    foreach (Result resultItem in result)
                    {
                        decimal WGP = 0;

                        if (resultItem.SpecialCase != null)
                        {
                            resultItem.GPCU = 0;
                            if (totalSemesterCourseUnit == 0)
                            {
                                totalSemesterCourseUnit = (int)resultItem.TotalSemesterCourseUnit - resultItem.CourseUnit;
                                resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                                resultItem.Grade = "-";
                            }
                            else
                            {
                                totalSemesterCourseUnit -= resultItem.CourseUnit;
                                resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                                resultItem.Grade = "-";
                            }

                        }
                        if (totalSemesterCourseUnit > 0)
                        {
                            resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                        }

                        modifiedResultList.Add(resultItem);
                    }

                    Result firstYearFirstSemesterResult = new Result();
                    decimal? firstSemesterGPCUSum = modifiedResultList.Sum(p => p.GPCU);
                    int? secondYearfirstSemesterTotalSemesterCourseUnit = 0;
                    secondYearfirstSemesterTotalSemesterCourseUnit = modifiedResultList.Min(p => p.TotalSemesterCourseUnit);
                    firstYearFirstSemesterResult.TotalSemesterCourseUnit = secondYearfirstSemesterTotalSemesterCourseUnit;
                    firstYearFirstSemesterResult.GPCU = firstSemesterGPCUSum;
                    decimal? firstSemesterGPA = 0M;
                    if (firstSemesterGPCUSum != null && firstSemesterGPCUSum > 0)
                    {
                        firstSemesterGPA = firstSemesterGPCUSum / secondYearfirstSemesterTotalSemesterCourseUnit;
                    }

                    overallResult = modifiedResultList.FirstOrDefault();
                    if (firstSemesterGPA != null && firstSemesterGPA > 0)
                    {
                        overallResult.GPA = Decimal.Round((decimal)firstSemesterGPA, 2);
                    }
                    if (firstSemesterGPCUSum != null && firstYearFirstSemester != null && firstYearSecondSemester != null)
                    {
                        if ((firstSemesterGPCUSum + firstYearFirstSemester.GPCU + firstYearSecondSemester.GPCU) > 0 && firstYearSecondSemester.TotalSemesterCourseUnit != null && firstYearFirstSemester.TotalSemesterCourseUnit != null && secondYearfirstSemesterTotalSemesterCourseUnit != null)
                        {
                            firstYearFirstSemester.TotalSemesterCourseUnit = firstYearFirstSemester.TotalSemesterCourseUnit ?? 0;
                            firstYearSecondSemester.TotalSemesterCourseUnit = firstYearSecondSemester.TotalSemesterCourseUnit ?? 0;
                            overallResult.CGPA = Decimal.Round((decimal)((firstSemesterGPCUSum + firstYearFirstSemester.GPCU + firstYearSecondSemester.GPCU) / (firstYearSecondSemester.TotalSemesterCourseUnit + firstYearFirstSemester.TotalSemesterCourseUnit + secondYearfirstSemesterTotalSemesterCourseUnit)), 2);
                        }
                    }

                    overallResult.Remark = GetGraduationStatus(overallResult.CGPA, carryOverCourses);
                }
                else if (semesterId == 2)
                {

                    List<Result> result = null;
                    Semester firstSemester = new Semester() { Id = 1 };


                    if (studentCheck.Activated == true || studentCheck.Activated == null)
                    {
                        result = studentResultLogic.GetStudentProcessedResultBy(session, level, department, studentCheck, firstSemester, programme);
                    }
                    else
                    {
                        //result = studentResultLogic.GetDeactivatedStudentProcessedResultBy(session, level, department, studentCheck, firstSemester, programme);
                        return null;
                    }
                    List<Result> modifiedResultList = new List<Result>();
                    int totalSemesterCourseUnit = 0;
                    foreach (Result resultItem in result)
                    {

                        decimal WGP = 0;

                        if (resultItem.SpecialCase != null)
                        {

                            resultItem.GPCU = 0;
                            if (totalSemesterCourseUnit == 0)
                            {
                                totalSemesterCourseUnit = (int)resultItem.TotalSemesterCourseUnit - resultItem.CourseUnit;
                                resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                                resultItem.Grade = "-";
                            }
                            else
                            {
                                totalSemesterCourseUnit -= resultItem.CourseUnit;
                                resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                                resultItem.Grade = "-";
                            }

                        }
                        if (totalSemesterCourseUnit > 0)
                        {
                            resultItem.TotalSemesterCourseUnit = totalSemesterCourseUnit;
                        }
                        modifiedResultList.Add(resultItem);
                    }
                    Result secondYearFirstSemesterResult = new Result();
                    decimal? secondYearfirstSemesterGPCUSum = modifiedResultList.Sum(p => p.GPCU);
                    int? secondYearfirstSemesterTotalSemesterCourseUnit = 0;
                    secondYearfirstSemesterTotalSemesterCourseUnit = modifiedResultList.Min(p => p.TotalSemesterCourseUnit);
                    secondYearFirstSemesterResult.TotalSemesterCourseUnit = secondYearfirstSemesterTotalSemesterCourseUnit;
                    secondYearFirstSemesterResult.GPCU = secondYearfirstSemesterGPCUSum;
                    decimal? firstSemesterGPA = 0M;
                    if (secondYearfirstSemesterGPCUSum != null && secondYearfirstSemesterGPCUSum > 0)
                    {
                        firstSemesterGPA = secondYearfirstSemesterGPCUSum / secondYearfirstSemesterTotalSemesterCourseUnit;
                    }



                    //Second semester second year

                    List<Result> secondSemesterResult = null;


                    Semester secondSemester = new Semester() { Id = 2 };

                    sessionSemester = sessionSemesterLogic.GetModelBy(s => s.Session_Id == sessionId && s.Semester_Id == secondSemester.Id);

                    carryOverCourses = GetSecondYearCarryOverCourses(sessionSemester, level, programme, department, studentCheck);
                    if (carryOverCourses.Count > 0)
                    {
                        return null;
                    }

                    if (studentCheck.Activated == true || studentCheck.Activated == null)
                    {
                        secondSemesterResult = studentResultLogic.GetStudentProcessedResultBy(session, level, department, studentCheck, secondSemester, programme);
                    }
                    else
                    {
                        //secondSemesterResult = studentResultLogic.GetDeactivatedStudentProcessedResultBy(session, level, department, studentCheck, secondSemester, programme);
                        return null;
                    }
                    List<Result> secondSemesterModifiedResultList = new List<Result>();
                    int totalSecondSemesterCourseUnit = 0;
                    foreach (Result resultItem in secondSemesterResult)
                    {

                        decimal WGP = 0;

                        if (resultItem.SpecialCase != null)
                        {

                            resultItem.GPCU = 0;
                            if (totalSecondSemesterCourseUnit == 0)
                            {
                                totalSecondSemesterCourseUnit = (int)resultItem.TotalSemesterCourseUnit - resultItem.CourseUnit;
                                resultItem.TotalSemesterCourseUnit = totalSecondSemesterCourseUnit;
                                resultItem.Grade = "-";
                            }
                            else
                            {
                                totalSecondSemesterCourseUnit -= resultItem.CourseUnit;
                                resultItem.TotalSemesterCourseUnit = totalSecondSemesterCourseUnit;
                                resultItem.Grade = "-";
                            }

                        }
                        if (totalSecondSemesterCourseUnit > 0)
                        {
                            resultItem.TotalSemesterCourseUnit = totalSecondSemesterCourseUnit;
                        }
                        secondSemesterModifiedResultList.Add(resultItem);
                    }
                    Result secondYearSecondtSemesterResult = new Result();
                    decimal? secondYearSecondtSemesterGPCUSum = secondSemesterModifiedResultList.Sum(p => p.GPCU);
                    int? secondYearSecondSemesterTotalSemesterCourseUnit = 0;
                    secondYearSecondSemesterTotalSemesterCourseUnit = secondSemesterModifiedResultList.Min(p => p.TotalSemesterCourseUnit);
                    secondYearSecondtSemesterResult.TotalSemesterCourseUnit = secondYearSecondSemesterTotalSemesterCourseUnit;
                    secondYearSecondtSemesterResult.GPCU = secondYearSecondtSemesterGPCUSum;
                    decimal? secondYearSecondSmesterGPA = 0M;
                    if (secondYearSecondtSemesterGPCUSum != null && secondYearSecondtSemesterGPCUSum > 0)
                    {
                        secondYearSecondSmesterGPA = secondYearSecondtSemesterGPCUSum / secondYearSecondSemesterTotalSemesterCourseUnit;
                    }

                    overallResult = secondSemesterModifiedResultList.FirstOrDefault();
                    if (secondYearSecondSmesterGPA != null && secondYearSecondSmesterGPA > 0)
                    {
                        overallResult.GPA = Decimal.Round((decimal)secondYearSecondSmesterGPA, 2);
                    }
                    if (secondYearfirstSemesterGPCUSum != null && firstYearFirstSemester != null && firstYearSecondSemester != null)
                    {
                        firstYearFirstSemester.TotalSemesterCourseUnit = firstYearFirstSemester.TotalSemesterCourseUnit ?? 0;
                        firstYearSecondSemester.TotalSemesterCourseUnit = firstYearSecondSemester.TotalSemesterCourseUnit ?? 0;
                        firstYearFirstSemester.GPCU = firstYearFirstSemester.GPCU ?? 0;
                        firstYearSecondSemester.GPCU = firstYearSecondSemester.GPCU ?? 0;
                        overallResult.CGPA = Decimal.Round((decimal)((secondYearfirstSemesterGPCUSum + firstYearFirstSemester.GPCU + firstYearSecondSemester.GPCU + secondYearSecondtSemesterGPCUSum) / (firstYearSecondSemester.TotalSemesterCourseUnit + firstYearFirstSemester.TotalSemesterCourseUnit + secondYearfirstSemesterTotalSemesterCourseUnit + secondYearSecondSemesterTotalSemesterCourseUnit)), 2);
                    }

                    overallResult.Remark = GetGraduationStatus(overallResult.CGPA, carryOverCourses);
                }
            }
            catch (Exception)
            {
                throw;
            }

            return overallResult;
        }
        public List<Result> GetMaterSheetDetailsByOptionsExtraYear(SessionSemester sessionSemester, Level level, Programme programme, Department department, DepartmentOption departmentOption)
        {
            try
            {
                if (sessionSemester == null || sessionSemester.Id <= 0 || programme == null || programme.Id <= 0 || level == null || level.Id <= 0 || department == null || department.Id <= 0 || departmentOption == null || departmentOption.Id <= 0)
                {
                    throw new Exception("One or more criteria to get Mater Result Sheet not set! Please check your input criteria selection and try again.");
                }

                SessionSemesterLogic sessionSemesterLogic = new Business.SessionSemesterLogic();
                SessionSemester ss = sessionSemesterLogic.GetBy(sessionSemester.Id);
                SessionLogic sessionLogic = new SessionLogic();
                Session sessions = sessionLogic.GetModelBy(p => p.Session_Id == ss.Session.Id);
                string[] sessionItems = ss.Session.Name.Split('/');
                string sessionNameStr = sessionItems[0];
                int sessionNameInt = Convert.ToInt32(sessionNameStr);
                List<Result> results = null;

                string currentSessionSuffix = sessionNameStr.Substring(2, 2);
                string yearTwoSessionSuffix = Convert.ToString((Convert.ToInt32(currentSessionSuffix) - 1));

                currentSessionSuffix = "/" + currentSessionSuffix + "/";
                yearTwoSessionSuffix = "/" + yearTwoSessionSuffix + "/";

                string levels = GetLevelName(level.Id);
                string identifier = null;
                string departmentCode = GetDepartmentCode(department.Id);

                string semesterCode = GetSemesterCodeBy(ss.Semester.Id);
                string sessionCode = GetSessionCodeBy(ss.Session.Name);
                string extraYearIdentifiersuffix = null;
                if (programme.Id == (int)Programmes.NDPartTime)
                {
                    extraYearIdentifiersuffix = "YC2";
                }
                else
                {
                    extraYearIdentifiersuffix = levels.Substring(0, 1) + "C2";
                }
                identifier = departmentCode + extraYearIdentifiersuffix + semesterCode + sessionCode;

                StudentDefermentLogic defermentLogic = new StudentDefermentLogic();
                List<StudentDeferementLog> deferementLogs = defermentLogic.GetAll();

                DepartmentOptionLogic departmentOptionLogic = new DepartmentOptionLogic();
                DepartmentOption option = departmentOptionLogic.GetModelBy(d => d.Department_Option_Id == departmentOption.Id);

                if (ss.Session.Id == (int)Sessions._20152016)
                {
                    results = (from sr in repository.GetBy<VW_STUDENT_RESULT_WITH_OPTIONS>(
                                 x =>
                                     x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id &&
                                     x.Programme_Id == programme.Id &&
                                     x.Department_Id == department.Id &&
                                     x.Department_Option_Id == departmentOption.Id &&
                                     (x.Activated != false || x.Activated == null))
                               select new Result
                               {
                                   StudentId = sr.Person_Id,
                                   Sex = sr.Sex_Name,
                                   Name = sr.Name,
                                   MatricNumber = sr.Matric_Number,
                                   CourseId = sr.Course_Id,
                                   CourseCode = sr.Course_Code,
                                   CourseName = sr.Course_Name,
                                   CourseUnit = (int)(int)sr.Course_Unit,
                                   SpecialCase = sr.Special_Case,
                                   TestScore = sr.Test_Score,
                                   ExamScore = sr.Exam_Score,
                                   Score = sr.Total_Score,
                                   Grade = sr.Grade,
                                   GradePoint = sr.Grade_Point,
                                   DepartmentName = sr.Department_Name,
                                   ProgrammeName = sr.Programme_Name,
                                   LevelName = sr.Level_Name,
                                   Semestername = sr.Semester_Name,
                                   GPCU = sr.Grade_Point * sr.Course_Unit,
                                   TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                   DepartmentOptionId = sr.Department_Option_Id,
                                   DepartmentOptionName = sr.Department_Option_Name,
                                   SessionName = sr.Session_Name,
                                   CourseModeId = sr.Course_Mode_Id,
                                   LevelId = sr.Level_Id
                               }).ToList();

                    List<Result> resultList = new List<Result>();

                    for (int i = 0; i < results.Count; i++)
                    {
                        if (results[i].MatricNumber.Contains(currentSessionSuffix) || results[i].MatricNumber.Contains(yearTwoSessionSuffix) || results[i].MatricNumber.Contains("/14/") ||
                            results[i].MatricNumber.Contains("/13/") || results[i].MatricNumber.Contains("/12/") || results[i].MatricNumber.Contains("/11/"))
                        {
                            //Do Nothing
                        }
                        else
                        {
                            if (deferementLogs.LastOrDefault(s => s.Student.MatricNumber == results[i].MatricNumber) == null)
                            {
                                resultList.Add(results[i]);
                            }
                        }
                    }

                    results = new List<Result>();
                    results = resultList;
                }
                else
                {
                    results =
                        (from sr in
                             repository.GetBy<VW_STUDENT_RESULT_WITH_OPTIONS>(
                                 x =>
                                     x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id &&
                                     x.Programme_Id == programme.Id &&
                                     x.Department_Id == department.Id &&
                                     x.Department_Option_Id == departmentOption.Id &&
                                     (x.Activated != false || x.Activated == null))
                         select new Result
                         {
                             StudentId = sr.Person_Id,
                             Sex = sr.Sex_Name,
                             Name = sr.Name,
                             MatricNumber = sr.Matric_Number,
                             CourseId = sr.Course_Id,
                             CourseCode = sr.Course_Code,
                             CourseName = sr.Course_Name,
                             CourseUnit = (int)sr.Course_Unit,
                             SpecialCase = sr.Special_Case,
                             TestScore = sr.Test_Score,
                             ExamScore = sr.Exam_Score,
                             Score = sr.Total_Score,
                             Grade = sr.Grade,
                             GradePoint = sr.Grade_Point,
                             DepartmentName = sr.Department_Name,
                             ProgrammeName = sr.Programme_Name,
                             LevelName = sr.Level_Name,
                             Semestername = sr.Semester_Name,
                             GPCU = sr.Grade_Point * sr.Course_Unit,
                             TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                             DepartmentOptionId = sr.Department_Option_Id,
                             DepartmentOptionName = sr.Department_Option_Name,
                             SessionName = sr.Session_Name,
                             CourseModeId = sr.Course_Mode_Id,
                             LevelId = sr.Level_Id
                         }).ToList();

                    List<Result> resultList = new List<Result>();

                    if (level.Id == (int)Levels.HNDI || level.Id == (int)Levels.NDI)
                    {
                        for (int i = 0; i < results.Count; i++)
                        {
                            if (results[i].MatricNumber.Contains(currentSessionSuffix) || results[i].MatricNumber.Contains(yearTwoSessionSuffix) || results[i].MatricNumber.Contains("/14/") ||
                                results[i].MatricNumber.Contains("/13/") || results[i].MatricNumber.Contains("/12/") || results[i].MatricNumber.Contains("/11/"))
                            {
                                if (deferementLogs.LastOrDefault(s => s.Student.MatricNumber == results[i].MatricNumber) != null && level.Id == results[i].LevelId)
                                {
                                    resultList.Add(results[i]);
                                }
                            }
                            else
                            {
                                if (deferementLogs.LastOrDefault(s => s.Student.MatricNumber == results[i].MatricNumber) != null && level.Id == results[i].LevelId)
                                {
                                    resultList.Add(results[i]);
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < results.Count; i++)
                        {
                            if (results[i].MatricNumber.Contains(currentSessionSuffix) || results[i].MatricNumber.Contains(yearTwoSessionSuffix) || results[i].MatricNumber.Contains("/14/") ||
                                results[i].MatricNumber.Contains("/13/") || results[i].MatricNumber.Contains("/12/") || results[i].MatricNumber.Contains("/11/"))
                            {
                                if (deferementLogs.LastOrDefault(s => s.Student.MatricNumber == results[i].MatricNumber) != null && level.Id == results[i].LevelId)
                                {
                                    resultList.Add(results[i]);
                                }
                            }
                            else
                            {
                                if (deferementLogs.LastOrDefault(s => s.Student.MatricNumber == results[i].MatricNumber) == null)
                                {
                                    resultList.Add(results[i]);
                                }
                                else if (deferementLogs.LastOrDefault(s => s.Student.MatricNumber == results[i].MatricNumber) != null && level.Id == results[i].LevelId)
                                {
                                    resultList.Add(results[i]);
                                }
                            }
                        }
                    }

                    results = new List<Result>();
                    results = resultList;
                }
                sessionSemester = sessionSemesterLogic.GetModelBy(p => p.Session_Semester_Id == sessionSemester.Id);
                //  List<Result> studentsResult = GetResultList(sessionSemester, level, programme, department).ToList(); 

                List<long> students = results.Select(s => s.StudentId).Distinct().ToList();

                List<Result> masterSheetResult = new List<Result>();

                for (int i = 0; i < students.Count; i++)
                {
                    long studentId = students[i];
                    TotalUnitsOmitted = 0;

                    Result result = null;
                    if (deferementLogs.LastOrDefault(s => s.Student.MatricNumber == results[i].MatricNumber) == null)
                        result = ViewProcessedStudentResultExtraYear(studentId, sessionSemester, level, programme, department);
                    else
                        result = ViewProcessedStudentResult(studentId, sessionSemester, level, programme, department);

                    result.UnitOutstanding = TotalUnitsOmitted;

                    AssignAndAddToMasterSheetExtraYear(identifier, result, results.Where(s => s.StudentId == studentId).ToList(), masterSheetResult);

                }

                for (int i = 0; i < masterSheetResult.Count; i++)
                {
                    Result result = masterSheetResult[i];
                    List<Result> studentResults = results.Where(p => p.StudentId == result.StudentId).ToList();
                    for (int j = 0; j < studentResults.Count; j++)
                    {
                        Result resultItem = studentResults[j];
                        resultItem.Identifier = identifier;
                        resultItem.CGPA = result.CGPA;
                        resultItem.Remark = result.Remark;
                        resultItem.GPA = result.GPA;
                        resultItem.UnitOutstanding = result.UnitOutstanding;
                        resultItem.WGP = result.WGP;
                        resultItem.TotalSemesterCourseUnit = result.TotalSemesterCourseUnit;
                        resultItem.UnitPassed = result.UnitPassed;

                        resultItem.SessionId = ss.Session.Id;

                        //int totalSemesterCourseUnit = 0;
                        //CheckForSpecialCase(resultItem, totalSemesterCourseUnit, (int)Semesters.FirstSemester);
                        
                        resultItem.DepartmentOptionName = option.Name;
                    }
                }

                for (int i = 0; i < results.Count; i++)
                {
                    results[i].LevelName = levels;
                    results[i].CourseMode = "Extra Year";
                }

                return results.OrderBy(a => a.MatricNumber).ThenBy(a => a.CourseModeId).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<Result> GetMaterSheetDetailsExtraYear(SessionSemester sessionSemester, Level level, Programme programme, Department department)
        {
            try
            {
                if (sessionSemester == null || sessionSemester.Id <= 0 || level == null || level.Id <= 0 || programme == null || programme.Id <= 0 || department == null || department.Id <= 0)
                {
                    throw new Exception("One or more criteria to get Mater Result Sheet not set! Please check your input criteria selection and try again.");
                }

                SessionSemesterLogic sessionSemesterLogic = new SessionSemesterLogic();
                SessionSemester ss = sessionSemesterLogic.GetBy(sessionSemester.Id);
                string[] sessionItems = ss.Session.Name.Split('/');
                string sessionNameStr = sessionItems[0];
                List<Result> results = null;

                string currentSessionSuffix = sessionNameStr.Substring(2, 2);
                string yearTwoSessionSuffix = Convert.ToString((Convert.ToInt32(currentSessionSuffix) - 1));

                currentSessionSuffix = "/" + currentSessionSuffix + "/";
                yearTwoSessionSuffix = "/" + yearTwoSessionSuffix + "/";

                string identifier = null;
                string departmentCode = GetDepartmentCode(department.Id);
                string levels = GetLevelName(level.Id);
                string semesterCode = GetSemesterCodeBy(ss.Semester.Id);
                string sessionCode = GetSessionCodeBy(ss.Session.Name);
                string extraYearIdentifiersuffix = null;
                if (programme.Id == (int)Programmes.NDPartTime)
                {
                    extraYearIdentifiersuffix = "YC2";
                }
                else
                {
                    extraYearIdentifiersuffix = levels.Substring(0, 1) + "C2";
                }
                identifier = departmentCode + extraYearIdentifiersuffix + semesterCode + sessionCode;

                StudentDefermentLogic defermentLogic = new StudentDefermentLogic();
                List<StudentDeferementLog> deferementLogs = defermentLogic.GetAll();

                if (ss.Session.Id == (int)Sessions._20152016)
                {
                    results = (from sr in repository.GetBy<VW_STUDENT_RESULT_2>(x => x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id && x.Programme_Id == programme.Id &&
                               x.Department_Id == department.Id && (x.Activated != false))
                               select new Result
                               {
                                   StudentId = sr.Person_Id,
                                   Sex = sr.Sex_Name,
                                   Name = sr.Name,
                                   MatricNumber = sr.Matric_Number,
                                   CourseId = sr.Course_Id,
                                   CourseCode = sr.Course_Code,
                                   CourseName = sr.Course_Name,
                                   CourseUnit = sr.Course_Unit,
                                   SpecialCase = sr.Special_Case,
                                   TestScore = sr.Test_Score,
                                   ExamScore = sr.Exam_Score,
                                   Score = sr.Total_Score,
                                   Grade = sr.Grade,
                                   GradePoint = sr.Grade_Point,
                                   DepartmentName = sr.Department_Name,
                                   ProgrammeName = sr.Programme_Name,
                                   LevelName = sr.Level_Name,
                                   Semestername = sr.Semester_Name,
                                   GPCU = sr.Grade_Point * sr.Course_Unit,
                                   TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                   SessionName = sr.Session_Name,
                                   CourseModeId = sr.Course_Mode_Id,
                                   LevelId = sr.Level_Id
                               }).ToList();

                    List<Result> resultList = new List<Result>();

                    for (int i = 0; i < results.Count; i++)
                    {
                        if (results[i].MatricNumber.Contains(currentSessionSuffix) || results[i].MatricNumber.Contains(yearTwoSessionSuffix) || results[i].MatricNumber.Contains("/14/") ||
                            results[i].MatricNumber.Contains("/13/") || results[i].MatricNumber.Contains("/12/") || results[i].MatricNumber.Contains("/11/"))
                        {
                            //Do Nothing
                        }
                        else
                        {
                            if (deferementLogs.LastOrDefault(s => s.Student.MatricNumber == results[i].MatricNumber) == null)
                            {
                                resultList.Add(results[i]);
                            }
                        }
                    }

                    results = new List<Result>();
                    results = resultList;
                }
                else
                {
                    results = (from sr in repository.GetBy<VW_STUDENT_RESULT_2>(x => x.Session_Id == ss.Session.Id && x.Semester_Id == ss.Semester.Id && x.Programme_Id == programme.Id
                               && x.Department_Id == department.Id && (x.Activated != false))
                               select new Result
                               {
                                   StudentId = sr.Person_Id,
                                   Sex = sr.Sex_Name,
                                   Name = sr.Name,
                                   MatricNumber = sr.Matric_Number,
                                   CourseId = sr.Course_Id,
                                   CourseCode = sr.Course_Code,
                                   CourseName = sr.Course_Name,
                                   CourseUnit = sr.Course_Unit,
                                   SpecialCase = sr.Special_Case,
                                   TestScore = sr.Test_Score,
                                   ExamScore = sr.Exam_Score,
                                   Score = sr.Total_Score,
                                   Grade = sr.Grade,
                                   GradePoint = sr.Grade_Point,
                                   DepartmentName = sr.Department_Name,
                                   ProgrammeName = sr.Programme_Name,
                                   LevelName = sr.Level_Name,
                                   Semestername = sr.Semester_Name,
                                   GPCU = sr.Grade_Point * sr.Course_Unit,
                                   TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                   SessionName = sr.Session_Name,
                                   CourseModeId = sr.Course_Mode_Id,
                                   LevelId = sr.Level_Id
                               }).ToList();

                    List<Result> resultList = new List<Result>();

                    if (level.Id == (int)Levels.HNDI || level.Id == (int)Levels.NDI)
                    {
                        for (int i = 0; i < results.Count; i++)
                        {
                            if (results[i].MatricNumber.Contains(currentSessionSuffix) || results[i].MatricNumber.Contains(yearTwoSessionSuffix) || results[i].MatricNumber.Contains("/14/") || 
                                results[i].MatricNumber.Contains("/13/") || results[i].MatricNumber.Contains("/12/") || results[i].MatricNumber.Contains("/11/"))
                            {
                                if (deferementLogs.LastOrDefault(s => s.Student.MatricNumber == results[i].MatricNumber) != null && level.Id == results[i].LevelId)
                                {
                                    resultList.Add(results[i]);
                                }
                            }
                            else
                            {
                                if (deferementLogs.LastOrDefault(s => s.Student.MatricNumber == results[i].MatricNumber) != null && level.Id == results[i].LevelId)
                                {
                                    resultList.Add(results[i]);
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < results.Count; i++)
                        {
                            if (results[i].MatricNumber.Contains(currentSessionSuffix) || results[i].MatricNumber.Contains(yearTwoSessionSuffix) || results[i].MatricNumber.Contains("/14/") ||
                                results[i].MatricNumber.Contains("/13/") || results[i].MatricNumber.Contains("/12/") || results[i].MatricNumber.Contains("/11/"))
                            {
                                if (deferementLogs.LastOrDefault(s => s.Student.MatricNumber == results[i].MatricNumber) != null && level.Id == results[i].LevelId)
                                {
                                    resultList.Add(results[i]);
                                }
                            }
                            else
                            {
                                if (deferementLogs.LastOrDefault(s => s.Student.MatricNumber == results[i].MatricNumber) == null)
                                {
                                    resultList.Add(results[i]);
                                }
                                else if (deferementLogs.LastOrDefault(s => s.Student.MatricNumber == results[i].MatricNumber) != null && level.Id == results[i].LevelId)
                                {
                                    resultList.Add(results[i]);
                                }
                            }
                        }
                    }
                    
                    results = new List<Result>();
                    results = resultList;
                }

                sessionSemester = sessionSemesterLogic.GetModelBy(p => p.Session_Semester_Id == sessionSemester.Id);
                List<long> students = results.Select(s => s.StudentId).Distinct().ToList();

                List<Result> masterSheetResult = new List<Result>();
                //students = new List<long>();
                //students.Add(43648);
                for (int i = 0; i < students.Count; i++)
                {
                    long studentId = students[i];
                    TotalUnitsOmitted = 0;
                    Result result = null;
                    var defered=deferementLogs.Where(s => s.Student.Id == studentId).LastOrDefault();
                    if(defered?.Id>0)
                        result = ViewProcessedStudentResult(studentId, sessionSemester, level, programme, department);
                    
                    else
                        result = ViewProcessedStudentResultExtraYear(studentId, sessionSemester, level, programme, department);

                    result.UnitOutstanding = TotalUnitsOmitted;

                    AssignAndAddToMasterSheetExtraYear(identifier, result, results.Where(s => s.StudentId == studentId).ToList(), masterSheetResult);
                }

                for (int i = 0; i < masterSheetResult.Count; i++)
                {
                    Result result = masterSheetResult[i];

                    List<Result> studentResults = results.Where(p => p.StudentId == result.StudentId).ToList();
                    for (int j = 0; j < studentResults.Count; j++)
                    {
                        Result resultItem = studentResults[j];

                        resultItem.Identifier = identifier;
                        resultItem.CGPA = result.CGPA;
                        resultItem.Remark = result.Remark;
                        resultItem.GPA = result.GPA;
                        resultItem.UnitOutstanding = result.UnitOutstanding;
                        resultItem.WGP = result.WGP;
                        resultItem.TotalSemesterCourseUnit = result.TotalSemesterCourseUnit;
                        resultItem.UnitPassed = result.UnitPassed;

                        resultItem.SessionId = ss.Session.Id;

                        //int totalSemesterCourseUnit = 0;
                        //CheckForSpecialCase(resultItem, totalSemesterCourseUnit, (int)Semesters.FirstSemester);
                    }
                }

                for (int i = 0; i < results.Count; i++)
                {
                    results[i].LevelName = levels;
                    results[i].CourseMode = "Extra Year";
                }

                return results.OrderBy(a => a.MatricNumber).ThenBy(a => a.CourseModeId).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public int TimesCourseExistInCarryoverList(List<long> CourseCodes,  long courseCode)
        {
            int count = 0;
            if (CourseCodes.Count > 0)
            {
                for(int i=0; i < CourseCodes.Count; i++)
                {
                    if (CourseCodes[i]==(courseCode))
                        count += 1;
                }
            }
            return count;
        }
        public List<string> GetStudentAllPassedCourseCode(Department department, Student student, Programme programme)
        {
            try
            {
                if (department == null || department.Id <= 0 || student == null || student.Id <= 0 || programme == null || programme.Id <= 0)
                {
                    throw new Exception("One or more criteria to get Result not set! Please check your input criteria selection and try again.");
                }

                List<Result> results = (from sr in repository.GetBy<VW_STUDENT_RESULT_2>(p => p.Person_Id == student.Id && p.Programme_Id == programme.Id && p.Department_Id == department.Id &&
                                            p.Activated != false && (p.Test_Score + p.Exam_Score)>=40)
                                        select new Result
                                        {
                                            CourseCode = sr.Course_Code

                                        }).ToList();
                var CourseCode=results.Select(f => f.CourseCode);

                return CourseCode.ToList();


            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<Result> RemoveExistingCarryOverFromList(List<Result> OtherYearResult, List<Result> CurrentYearResult)
        {
            try
            {
                if(CurrentYearResult!=null && CurrentYearResult.Count > 0 && OtherYearResult!=null && OtherYearResult.Count>0)
                {
                    for(int i=0; i< CurrentYearResult.Count; i++)
                    {
                        var courseId = CurrentYearResult[i].CourseId;
                        var contains = OtherYearResult.Where(f => f.CourseId == courseId).ToList();
                        if (contains!=null && contains.Count > 0)
                        {
                            CurrentYearResult.Remove(CurrentYearResult[i]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return CurrentYearResult;
        }
    }

}
