using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Entity.Model;
using Abundance_Nk.Model.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Business
{
    public class AggregateSheetLogic : BusinessBaseLogic<StudentResult, STUDENT_RESULT>
    {
        StudentLevelLogic studentLevelLogic;
        SessionSemesterLogic sessionSemesterLogic;
        SessionLogic sessionLogic;
        public AggregateSheetLogic()
        {
            studentLevelLogic = new StudentLevelLogic();
            sessionSemesterLogic = new SessionSemesterLogic();
            sessionLogic = new SessionLogic();
        }

        public List<AggregateSheet> GetMaterSheetDetailsByMode(SessionSemester sessionSemester, Level level, Programme programme, Department department, CourseMode courseMode)
        {
            try
            {
                if (sessionSemester == null || sessionSemester.Id <= 0 || level == null || level.Id <= 0 || programme == null || programme.Id <= 0 || department == null || department.Id <= 0)
                {
                    throw new Exception("One or more criteria to get Mater Result Sheet not set! Please check your input criteria selection and try again.");
                }

                List<AggregateSheet> AggregateResults = new List<AggregateSheet>();

                //Get all registered students in class.
                List<StudentLevel> studentsInLevel = studentLevelLogic.GetBy(level, programme, department, sessionSemester.Session);
                if (studentsInLevel != null & studentsInLevel.Count > 0)
                {
                    //Get All Results For Each Student throughout their programme
                    foreach(StudentLevel studentLevel in studentsInLevel)
                    {
                       var studentResults = GetStudentRecords(studentLevel);
                        AggregateResults.AddRange(studentResults);
                    }

                }

                return AggregateResults;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Result> GetMaterSheetDetailsByMode(SessionSemester sessionSemester, Level level, Programme programme, Department department)
        {
            try
            {
                if (sessionSemester == null || sessionSemester.Id <= 0 || level == null || level.Id <= 0 || programme == null || programme.Id <= 0 || department == null || department.Id <= 0)
                {
                    throw new Exception("One or more criteria to get Mater Result Sheet not set! Please check your input criteria selection and try again.");
                }

                sessionSemester = sessionSemesterLogic.GetBy(sessionSemester.Id);

                List<Result> FormattedResult = new List<Result>();
                List<AggregateSheet> AggregateResults = new List<AggregateSheet>();

                //Get all registered students in class.
                List<StudentLevel> studentsInLevel = studentLevelLogic.GetBy(level, programme, department, sessionSemester.Session);
                if (studentsInLevel != null & studentsInLevel.Count > 0)
                {
                    //Get All Results For Each Student throughout their programme
                    foreach (StudentLevel studentLevel in studentsInLevel)
                    {
                        var studentResults = GetStudentResults(studentLevel);
                        if (studentResults != null && studentResults.Count > 0)
                        {
                            string Remark = "";
                            AggregateStanding studentStanding = GetCGPA(sessionSemester, studentResults);

                            if ((studentLevel.Level.Id == (int)Levels.NDI || studentLevel.Level.Id == (int)Levels.NDI) && sessionSemester.Semester.Id == (int)Semesters.FirstSemester)
                            {
                                decimal previousCGPA = studentStanding.CGPA;
                                Remark = GetFirstYearFirstSemeterRemark(studentStanding.CGPA);
                            }

                            if ((studentLevel.Level.Id == (int)Levels.NDI || studentLevel.Level.Id == (int)Levels.NDI) && sessionSemester.Semester.Id == (int)Semesters.SecondSemester)
                            {
                                decimal previousCGPA = studentStanding.CGPA;
                                Remark = GetSecondYearFirstSemeterRemark(studentStanding.FirstSemesterGPA, studentStanding.SecondSemesterGPA);
                            }

                            if ((studentLevel.Level.Id == (int)Levels.NDII || studentLevel.Level.Id == (int)Levels.HNDII) && sessionSemester.Semester.Id == (int)Semesters.FirstSemester)
                            {
                                decimal previousCGPA = Decimal.Round((studentStanding.FirstSemesterGPA + studentStanding.SecondSemesterGPA) / 2, 3);
                                Remark = GetFirstYearSecondSemeterRemark(previousCGPA, studentStanding.CGPA);
                            }

                            if ((studentLevel.Level.Id == (int)Levels.NDII || studentLevel.Level.Id == (int)Levels.HNDII) && sessionSemester.Semester.Id == (int)Semesters.SecondSemester)
                            {
                                decimal previousCGPA = Decimal.Round((studentStanding.FirstSemesterGPA + studentStanding.SecondSemesterGPA + studentStanding.ThirdSemesterGPA) / 4, 3);
                                Remark = GetSecondYearFirstSemeterRemark(studentStanding.FirstSemesterGPA, studentStanding.SecondSemesterGPA);
                            }


                            foreach (Result xResult in studentResults)
                            {
                                if (xResult.SemesterId == sessionSemester.Semester.Id && xResult.SessionId == sessionSemester.Session.Id)
                                {
                                    if (studentStanding != null && studentStanding.CGPA > 0)
                                    {
                                        string COs = "";
                                        xResult.GPA = studentStanding.GPA;
                                        xResult.CGPA = studentStanding.CGPA;
                                        if (!String.IsNullOrEmpty(xResult.Remark))
                                        {
                                            COs = xResult.Remark;
                                        }
                                        if (!String.IsNullOrEmpty(Remark))
                                        {
                                            xResult.Remark = Remark + COs;
                                        }

                                        FormattedResult.Add(xResult);
                                    }

                                }
                            }

                        }


                    }

                }

                return FormattedResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        public List<Result> GetStudentResults(StudentLevel studentLevel)
        {
            try
            {
                List<Result> AggregateResults = new List<Result>();
                var results = (from sr in repository.GetBy<VW_STUDENT_RESULT_2>(x => x.Person_Id == studentLevel.Student.Id)
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
                                   SessionId = sr.Session_Id,
                                   GPCU = sr.Grade_Point * sr.Course_Unit,
                                   TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                   SessionName = sr.Session_Name,
                                   CourseModeId = sr.Course_Mode_Id,
                                   Activated = sr.Activated,
                                   FacultyName = sr.Faculty_Name,
                                   SemesterId = sr.Semester_Id,
                                   WGP = sr.WGP,
                                   DepartmentId = sr.Department_Id,
                                   ProgrammeId = sr.Programme_Id,
                                   LevelId = sr.Level_Id,


                               }).ToList();

                if (results != null && results.Count > 0)
                {
                    var SessionsRegistered = results.Select(a => a.SessionId).Distinct().ToList();
                    if (SessionsRegistered != null && SessionsRegistered.Count > 0)
                    {
                        foreach (int SessionId in SessionsRegistered)
                        {
                            var FirstSemesterResults = results.Where(a => a.SessionId == SessionId && a.SemesterId == (int)Semesters.FirstSemester).ToList();
                            var SecondSemesterResults = results.Where(a => a.SessionId == SessionId && a.SemesterId == (int)Semesters.SecondSemester).ToList();

                            if (FirstSemesterResults != null && FirstSemesterResults.Count > 0)
                            {

                                foreach (var course in FirstSemesterResults)
                                {
                                    Result aggregateCourse = new Result();
                                    aggregateCourse.CourseId = course.CourseId;
                                    aggregateCourse.ExamScore = course.ExamScore ?? 0;
                                    aggregateCourse.TestScore = course.TestScore ?? 0;
                                    aggregateCourse.TotalScore = course.TotalScore ?? 0;
                                    aggregateCourse.Score = course.Score ?? 0;
                                    aggregateCourse.Grade = course.Grade;
                                    aggregateCourse.CourseCode = course.CourseCode;
                                    aggregateCourse.CourseUnit = course.CourseUnit;
                                    aggregateCourse.GradePoint = course.GradePoint;
                                    aggregateCourse.WGP = course.WGP;
                                    aggregateCourse.GPCU = course.GPCU;

                                    aggregateCourse.StudentId = results.FirstOrDefault().StudentId;
                                    aggregateCourse.Name = results.FirstOrDefault().Name;
                                    aggregateCourse.MatricNumber = results.FirstOrDefault().MatricNumber;
                                    aggregateCourse.ProgrammeName = results.FirstOrDefault().ProgrammeName;
                                    aggregateCourse.DepartmentName = results.FirstOrDefault().DepartmentName;
                                    aggregateCourse.FacultyName = results.FirstOrDefault().FacultyName;
                                    aggregateCourse.LevelName = results.FirstOrDefault().LevelName;
                                    aggregateCourse.Sex = results.FirstOrDefault().Sex;
                                    aggregateCourse.MobilePhone = results.FirstOrDefault().MobilePhone;

                                    aggregateCourse.SessionId = FirstSemesterResults.FirstOrDefault().SessionId;
                                    aggregateCourse.ProgrammeId = FirstSemesterResults.FirstOrDefault().ProgrammeId ?? 0;
                                    aggregateCourse.DepartmentId = FirstSemesterResults.FirstOrDefault().DepartmentId;
                                    aggregateCourse.SemesterId = FirstSemesterResults.FirstOrDefault().SemesterId;
                                    aggregateCourse.LevelId = FirstSemesterResults.FirstOrDefault().LevelId;

                                    aggregateCourse.Semestername = FirstSemesterResults.FirstOrDefault().Semestername;
                                    aggregateCourse.SessionName = FirstSemesterResults.FirstOrDefault().SessionName;
                                    aggregateCourse.TotalSemesterCourseUnit = FirstSemesterResults.Sum(c => c.CourseUnit);
                                    aggregateCourse.UnitOutstanding = FirstSemesterResults.Where(c => c.Score < 40).ToList().Sum(d => d.CourseUnit); 
                                    aggregateCourse.UnitPassed = FirstSemesterResults.Where(c => c.Score > 40).ToList().Sum(d => d.CourseUnit);

                                    decimal SumGPCU = FirstSemesterResults.Sum(a => a.WGP) ?? 0;
                                    if (aggregateCourse.TotalSemesterCourseUnit > 0 && SumGPCU > 0)
                                    {
                                        aggregateCourse.GPA = SumGPCU / aggregateCourse.TotalSemesterCourseUnit;
                                    }
                                    
                                    aggregateCourse.Activated = results.FirstOrDefault().Activated ?? true;

                                    if (FirstSemesterResults.Where(c => c.Score < 40).ToList().Count > 0)
                                    {
                                        string carryOvers = "";
                                        carryOvers = GetCarryOverRemark(FirstSemesterResults);
                                        aggregateCourse.Remark = carryOvers;
                                    }

                                    AggregateResults.Add(aggregateCourse);
                                }
                            }

                            if (SecondSemesterResults != null && SecondSemesterResults.Count > 0)
                            {
                                foreach (var course in SecondSemesterResults)
                                {
                                    Result aggregateCourse = new Result();
                                    aggregateCourse.CourseId = course.CourseId;
                                    aggregateCourse.ExamScore = course.ExamScore ?? 0;
                                    aggregateCourse.TestScore = course.TestScore ?? 0;
                                    aggregateCourse.TotalScore = course.TotalScore ?? 0;
                                    aggregateCourse.Score = course.Score ?? 0;
                                    aggregateCourse.Grade = course.Grade;
                                    aggregateCourse.CourseCode = course.CourseCode;
                                    aggregateCourse.CourseUnit = course.CourseUnit;
                                    aggregateCourse.GradePoint = course.GradePoint;
                                    aggregateCourse.WGP = course.WGP;
                                    aggregateCourse.GPCU = course.GPCU;

                                    aggregateCourse.StudentId = results.FirstOrDefault().StudentId;
                                    aggregateCourse.Name = results.FirstOrDefault().Name;
                                    aggregateCourse.MatricNumber = results.FirstOrDefault().MatricNumber;
                                    aggregateCourse.ProgrammeName = results.FirstOrDefault().ProgrammeName;
                                    aggregateCourse.DepartmentName = results.FirstOrDefault().DepartmentName;
                                    aggregateCourse.FacultyName = results.FirstOrDefault().FacultyName;
                                    aggregateCourse.LevelName = results.FirstOrDefault().LevelName;
                                    aggregateCourse.Sex = results.FirstOrDefault().Sex;
                                    aggregateCourse.MobilePhone = results.FirstOrDefault().MobilePhone;

                                    aggregateCourse.SessionId = SecondSemesterResults.FirstOrDefault().SessionId;
                                    aggregateCourse.ProgrammeId = SecondSemesterResults.FirstOrDefault().ProgrammeId ?? 0;
                                    aggregateCourse.DepartmentId = SecondSemesterResults.FirstOrDefault().DepartmentId;
                                    aggregateCourse.SemesterId = SecondSemesterResults.FirstOrDefault().SemesterId;
                                    aggregateCourse.LevelId = SecondSemesterResults.FirstOrDefault().LevelId;

                                    aggregateCourse.Semestername = SecondSemesterResults.FirstOrDefault().Semestername;
                                    aggregateCourse.SessionName = SecondSemesterResults.FirstOrDefault().SessionName;
                                    aggregateCourse.TotalSemesterCourseUnit = SecondSemesterResults.Sum(c => c.CourseUnit);
                                    aggregateCourse.UnitOutstanding = SecondSemesterResults.Where(c => c.Score < 40).ToList().Sum(d => d.CourseUnit); ;
                                    aggregateCourse.UnitPassed = SecondSemesterResults.Where(c => c.Score > 40).ToList().Sum(d => d.CourseUnit);
                                    aggregateCourse.GPCU = SecondSemesterResults.Sum(a => a.WGP);
                                    //aggregateCourse.WGP = SecondSemesterResults.Sum(a => a.WGP);

                                    decimal SumGPCU = SecondSemesterResults.Sum(a => a.WGP) ?? 0;
                                    if (aggregateCourse.TotalSemesterCourseUnit > 0 && SumGPCU > 0)
                                    {
                                        aggregateCourse.GPA = SumGPCU / aggregateCourse.TotalSemesterCourseUnit;
                                    }

                                    aggregateCourse.Activated = results.FirstOrDefault().Activated ?? true;

                                    if (SecondSemesterResults.Where(c => c.Score < 40).ToList().Count > 0)
                                    {
                                        string carryOvers = "";
                                        carryOvers = GetCarryOverRemark(SecondSemesterResults);
                                        aggregateCourse.Remark = carryOvers;
                                    }

                                    AggregateResults.Add(aggregateCourse);
                                }
                            }


                        }
                    }

                }

                return AggregateResults;
            }
            catch (Exception ex)
            {

                throw ex;
            }
            
        }
        
        public List<AggregateSheet> GetStudentRecords(StudentLevel studentLevel)
        {
            List<AggregateSheet> AggregateResults = new List<AggregateSheet>();
            var  results = (from sr in repository.GetBy<VW_STUDENT_RESULT_2>(x => x.Person_Id == studentLevel.Student.Id)
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
                                           SessionId = sr.Session_Id,
                                           GPCU = sr.Grade_Point * sr.Course_Unit,
                                           TotalSemesterCourseUnit = sr.Total_Semester_Course_Unit,
                                           SessionName = sr.Session_Name,
                                           CourseModeId = sr.Course_Mode_Id,
                                           Activated = sr.Activated,
                                           FacultyName = sr.Faculty_Name,
                                           SemesterId = sr.Semester_Id,
                                           WGP = sr.WGP,
                                           DepartmentId = sr.Department_Id,
                                           ProgrammeId = sr.Programme_Id,
                                           LevelId = sr.Level_Id,
                                           

                                       }).ToList();

            if (results != null && results.Count > 0)
            {
                var SessionsRegistered = results.Select(a => a.SessionId).Distinct().ToList();
                if(SessionsRegistered != null && SessionsRegistered.Count > 0)
                {
                    foreach (int SessionId in SessionsRegistered)
                    {
                        AggregateSheet FirstSemesterAggregateSheet = new AggregateSheet();
                        AggregateSheet SecondSemesterAggregateSheet = new AggregateSheet();
                        var FirstSemesterResults = results.Where(a => a.SessionId == SessionId && a.SemesterId == (int)Semesters.FirstSemester).ToList();
                        var SecondSemesterResults = results.Where(a => a.SessionId == SessionId && a.SemesterId == (int)Semesters.SecondSemester).ToList();

                        if(FirstSemesterResults != null && FirstSemesterResults.Count > 0)
                        {

                            FirstSemesterAggregateSheet.PersonId = results.FirstOrDefault().StudentId;
                            FirstSemesterAggregateSheet.Fullname = results.FirstOrDefault().Name;
                            FirstSemesterAggregateSheet.MatricNumber = results.FirstOrDefault().MatricNumber;
                            FirstSemesterAggregateSheet.Programme = results.FirstOrDefault().ProgrammeName;
                            FirstSemesterAggregateSheet.Department = results.FirstOrDefault().DepartmentName;
                            FirstSemesterAggregateSheet.Faculty = results.FirstOrDefault().FacultyName;
                            FirstSemesterAggregateSheet.Level = results.FirstOrDefault().LevelName;
                            FirstSemesterAggregateSheet.Gender = results.FirstOrDefault().Sex;
                            FirstSemesterAggregateSheet.Phone = results.FirstOrDefault().MobilePhone;

                            FirstSemesterAggregateSheet.SessionId = FirstSemesterResults.FirstOrDefault().SessionId;
                            FirstSemesterAggregateSheet.ProgrammeId = FirstSemesterResults.FirstOrDefault().ProgrammeId ?? 0;
                            FirstSemesterAggregateSheet.DepartmentId = FirstSemesterResults.FirstOrDefault().DepartmentId;
                            FirstSemesterAggregateSheet.SemesterId = FirstSemesterResults.FirstOrDefault().SemesterId;
                            FirstSemesterAggregateSheet.LevelId = FirstSemesterResults.FirstOrDefault().LevelId;

                            FirstSemesterAggregateSheet.Semester = FirstSemesterResults.FirstOrDefault().Semestername;
                            FirstSemesterAggregateSheet.Session = FirstSemesterResults.FirstOrDefault().SessionName;
                            FirstSemesterAggregateSheet.TotalUnitAttempted = FirstSemesterResults.Sum(c => c.CourseUnit);
                            FirstSemesterAggregateSheet.TotalUnitOutstanding = FirstSemesterResults.Where(c => c.TotalScore < 40).ToList().Sum(d => d.CourseUnit); ;
                            FirstSemesterAggregateSheet.TotalUnitPassed = FirstSemesterResults.Where(c => c.TotalScore < 40).ToList().Sum(d => d.CourseUnit);
                            FirstSemesterAggregateSheet.TotalWeightedGradePoint = FirstSemesterResults.Sum(c => c.WGP) ?? 0 ;
                            if (FirstSemesterAggregateSheet.TotalUnitAttempted > 0 && FirstSemesterAggregateSheet.TotalWeightedGradePoint > 0)
                            {
                                FirstSemesterAggregateSheet.GPA = FirstSemesterAggregateSheet.TotalWeightedGradePoint / FirstSemesterAggregateSheet.TotalUnitAttempted;

                            }
                            else
                            {
                                FirstSemesterAggregateSheet.GPA = 0;

                            }
                            FirstSemesterAggregateSheet.Activated = results.FirstOrDefault().Activated ?? true;

                            if(FirstSemesterResults.Where(c => c.TotalScore < 40).ToList().Count > 0)
                            {
                                string carryOvers = "";
                                carryOvers = GetCarryOverRemark(FirstSemesterResults);
                                FirstSemesterAggregateSheet.Remark = carryOvers;
                            }

                            FirstSemesterAggregateSheet.Courses = new List<AggregateCourse>();

                            foreach(var course in  FirstSemesterResults)
                            {
                                AggregateCourse aggregateCourse = new AggregateCourse();
                                aggregateCourse.courseId = course.CourseId;
                                aggregateCourse.ExamScore = course.ExamScore ?? 0;
                                aggregateCourse.TestScore = course.TestScore ?? 0;
                                aggregateCourse.TotalScore = course.TotalScore ?? 0;
                                aggregateCourse.Grade = course.Grade;
                                aggregateCourse.CourseCode = course.CourseCode;
                                aggregateCourse.courseUnit = course.CourseUnit;
                                aggregateCourse.WGP = course.WGP ?? 0;
                                FirstSemesterAggregateSheet.Courses.Add(aggregateCourse);
                            }
                        }

                        if (SecondSemesterResults != null && SecondSemesterResults.Count > 0)
                        {

                            SecondSemesterAggregateSheet.PersonId = results.FirstOrDefault().StudentId;
                            SecondSemesterAggregateSheet.Fullname = results.FirstOrDefault().Name;
                            SecondSemesterAggregateSheet.MatricNumber = results.FirstOrDefault().MatricNumber;
                            SecondSemesterAggregateSheet.Programme = results.FirstOrDefault().ProgrammeName;
                            SecondSemesterAggregateSheet.Department = results.FirstOrDefault().DepartmentName;
                            SecondSemesterAggregateSheet.Faculty = results.FirstOrDefault().FacultyName;
                            SecondSemesterAggregateSheet.Level = results.FirstOrDefault().LevelName;
                            SecondSemesterAggregateSheet.Gender = results.FirstOrDefault().Sex;
                            SecondSemesterAggregateSheet.Phone = results.FirstOrDefault().MobilePhone;

                            SecondSemesterAggregateSheet.SessionId = SecondSemesterResults.FirstOrDefault().SessionId;
                            SecondSemesterAggregateSheet.ProgrammeId = SecondSemesterResults.FirstOrDefault().ProgrammeId ?? 0;
                            SecondSemesterAggregateSheet.DepartmentId = SecondSemesterResults.FirstOrDefault().DepartmentId;
                            SecondSemesterAggregateSheet.SemesterId = SecondSemesterResults.FirstOrDefault().SemesterId;
                            SecondSemesterAggregateSheet.LevelId = SecondSemesterResults.FirstOrDefault().LevelId;

                            SecondSemesterAggregateSheet.Semester = SecondSemesterResults.FirstOrDefault().Semestername;
                            SecondSemesterAggregateSheet.Session = SecondSemesterResults.FirstOrDefault().SessionName;
                            SecondSemesterAggregateSheet.TotalUnitAttempted = SecondSemesterResults.Sum(c => c.CourseUnit);
                            SecondSemesterAggregateSheet.TotalUnitOutstanding = SecondSemesterResults.Where(c => c.TotalScore < 40).ToList().Sum(d => d.CourseUnit); ;
                            SecondSemesterAggregateSheet.TotalUnitPassed = SecondSemesterResults.Where(c => c.TotalScore < 40).ToList().Sum(d => d.CourseUnit);
                            SecondSemesterAggregateSheet.TotalWeightedGradePoint = SecondSemesterResults.Sum(c => c.WGP) ?? 0; ;
                            if (SecondSemesterAggregateSheet.TotalUnitAttempted > 0 && SecondSemesterAggregateSheet.TotalWeightedGradePoint > 0)
                            {
                                SecondSemesterAggregateSheet.GPA = SecondSemesterAggregateSheet.TotalWeightedGradePoint / SecondSemesterAggregateSheet.TotalUnitAttempted;

                            }
                            else
                            {
                                SecondSemesterAggregateSheet.GPA = 0;

                            }
                            SecondSemesterAggregateSheet.Activated = results.FirstOrDefault().Activated ?? true;
                            if (SecondSemesterResults.Where(c => c.TotalScore < 40).ToList().Count > 0)
                            {
                                string carryOvers = "";
                                carryOvers = GetCarryOverRemark(SecondSemesterResults);
                                SecondSemesterAggregateSheet.Remark = carryOvers;
                            }
                            SecondSemesterAggregateSheet.Courses = new List<AggregateCourse>();

                            foreach (var course in SecondSemesterResults)
                            {
                                AggregateCourse aggregateCourse = new AggregateCourse();
                                aggregateCourse.courseId = course.CourseId;
                                aggregateCourse.ExamScore = course.ExamScore ?? 0;
                                aggregateCourse.TestScore = course.TestScore ?? 0;
                                aggregateCourse.TotalScore = course.TotalScore ?? 0;
                                aggregateCourse.Grade = course.Grade;
                                aggregateCourse.CourseCode = course.CourseCode;
                                aggregateCourse.courseUnit = course.CourseUnit;
                                aggregateCourse.WGP = course.WGP ?? 0;
                                SecondSemesterAggregateSheet.Courses.Add(aggregateCourse);
                            }
                        }

                        AggregateResults.Add(FirstSemesterAggregateSheet);
                        AggregateResults.Add(SecondSemesterAggregateSheet);
                    }
                }
               
            }
            
            return AggregateResults;
        }

        private static string GetCarryOverRemark(List<Result> FirstSemesterResults)
        {
            string carryOvers = "-";
            try
            {
                
                foreach (var failedCourse in FirstSemesterResults.Where(c => c.Score < 40).ToList())
                {
                    carryOvers += "|" + failedCourse.CourseCode;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return carryOvers;
        }
        
        //Get the semester number for the session of a student
        //Pass a semester & matric number and it would know if its first semester or fourth semester based on matric number
        private int GetSemesterWeight(SessionSemester sessionSemester, string MatricNumber)
        {
            try
            {
                if (!string.IsNullOrEmpty(MatricNumber))
                {
                    string[] MatricNumberArray = MatricNumber.Split('/');
                    if (MatricNumberArray != null && MatricNumberArray.Length > 2)
                    {
                        if (MatricNumberArray[2] == sessionSemester.Session.Name.Substring(2, 2) && sessionSemester.Semester.Id == (int)Semesters.FirstSemester)
                        {
                            return 1;
                        }
                        else if (Convert.ToInt32(MatricNumberArray[2]) == Convert.ToInt32(sessionSemester.Session.Name.Substring(2, 2)) && sessionSemester.Semester.Id == (int)Semesters.SecondSemester)
                        {
                            return 2;
                        }
                        else if (Math.Abs(Convert.ToInt32(MatricNumberArray[2]) - Convert.ToInt32(sessionSemester.Session.Name.Substring(2, 2))) == 1 && sessionSemester.Semester.Id == (int)Semesters.FirstSemester)
                        {
                            return 3;
                        }
                        else if (Math.Abs(Convert.ToInt32(MatricNumberArray[2]) - Convert.ToInt32(sessionSemester.Session.Name.Substring(2, 2))) == 1 && sessionSemester.Semester.Id == (int)Semesters.SecondSemester)
                        {
                            return 4;
                        }
                    }
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
            return 0;
        }

        private AggregateStanding GetCGPA(SessionSemester sessionSemester,List<Result> results)
        {
           

            try
            {
                AggregateStanding aggregateStanding = new AggregateStanding();
                aggregateStanding.CGPA = 0;
                decimal GPA = 0;
                decimal CGPA = 0;
                int semesterWeight = GetSemesterWeight(sessionSemester, results.FirstOrDefault().MatricNumber);
                int FirstSemesterSessionId = 0;
                int SecondSemesterSessionId = 0;
                int ThirdSemesterSessionId = 0;
                int FourthSemesterSessionId = 0;

                decimal FirstSemesterGPA = 0;
                decimal SecondSemesterGPA = 0;
                decimal ThirdSemesterGPA = 0;
                decimal FourthSemesterGPA = 0;

                if (semesterWeight == 1)
                {
                    FirstSemesterSessionId = sessionSemester.Session.Id;
                    var FirstSemesterResults = results.Where(s => s.SessionId == FirstSemesterSessionId && s.SemesterId == (int)Semesters.FirstSemester).ToList();
                    if (FirstSemesterResults != null && FirstSemesterResults.Count > 0)
                    {
                        FirstSemesterGPA = FirstSemesterResults.FirstOrDefault().GPA ?? 0;
                    }
                    GPA = Decimal.Round(FirstSemesterGPA, 2);
                    CGPA = Decimal.Round(FirstSemesterGPA, 2);
                }
                else if (semesterWeight == 2)
                {
                    FirstSemesterSessionId = sessionSemester.Session.Id;
                    SecondSemesterSessionId = sessionSemester.Session.Id;

                    var FirstSemesterResults = results.Where(s => s.SessionId == FirstSemesterSessionId && s.SemesterId == (int)Semesters.FirstSemester).ToList();
                    if (FirstSemesterResults != null && FirstSemesterResults.Count > 0)
                    {
                        FirstSemesterGPA = FirstSemesterResults.FirstOrDefault().GPA ?? 0;
                    }
                    var SecondSemesterResults = results.Where(s => s.SessionId == SecondSemesterSessionId && s.SemesterId == (int)Semesters.SecondSemester).ToList();
                    if(SecondSemesterResults != null && SecondSemesterResults.Count > 0)
                    {
                       SecondSemesterGPA = SecondSemesterResults.FirstOrDefault().GPA ?? 0;
                       GPA = Decimal.Round(SecondSemesterGPA, 2);
                    }
                    CGPA = Decimal.Round((Decimal.Round(FirstSemesterGPA, 2) + Decimal.Round(SecondSemesterGPA, 2)) / semesterWeight, 2);
                }
                else if (semesterWeight == 3)
                {
                    FirstSemesterSessionId = sessionLogic.GetPreviousSession(sessionSemester.Session).Id;
                    SecondSemesterSessionId = FirstSemesterSessionId;
                    ThirdSemesterSessionId = sessionSemester.Session.Id;

                    var FirstSemesterResults = results.Where(s => s.SessionId == FirstSemesterSessionId && s.SemesterId == (int)Semesters.FirstSemester).ToList();
                    if (FirstSemesterResults != null && FirstSemesterResults.Count > 0)
                    {
                        FirstSemesterGPA = FirstSemesterResults.FirstOrDefault().GPA ?? 0;
                    }
                    var SecondSemesterResults = results.Where(s => s.SessionId == SecondSemesterSessionId && s.SemesterId == (int)Semesters.SecondSemester).ToList();
                    if (SecondSemesterResults != null && SecondSemesterResults.Count > 0)
                    {
                        SecondSemesterGPA = SecondSemesterResults.FirstOrDefault().GPA ?? 0;
                    }
                    var ThirdSemesterResults = results.Where(s => s.SessionId == ThirdSemesterSessionId && s.SemesterId == (int)Semesters.FirstSemester).ToList();
                    if (ThirdSemesterResults != null && ThirdSemesterResults.Count > 0)
                    {
                        ThirdSemesterGPA = ThirdSemesterResults.FirstOrDefault().GPA ?? 0;
                        GPA = Decimal.Round(ThirdSemesterGPA, 2);
                    }

                    CGPA = Decimal.Round((Decimal.Round(FirstSemesterGPA, 2) + Decimal.Round(SecondSemesterGPA, 2) + Decimal.Round(ThirdSemesterGPA, 2)) / semesterWeight, 2);
                }
                else if (semesterWeight == 4)
                {
                    FirstSemesterSessionId = sessionLogic.GetPreviousSession(sessionSemester.Session).Id;
                    SecondSemesterSessionId = FirstSemesterSessionId;
                    ThirdSemesterSessionId = sessionSemester.Session.Id;
                    FourthSemesterSessionId = ThirdSemesterSessionId;

                    var FirstSemesterResults = results.Where(s => s.SessionId == FirstSemesterSessionId && s.SemesterId == (int)Semesters.FirstSemester).ToList();
                    if (FirstSemesterResults != null && FirstSemesterResults.Count > 0)
                    {
                        FirstSemesterGPA = FirstSemesterResults.FirstOrDefault().GPA ?? 0;
                    }
                    var SecondSemesterResults = results.Where(s => s.SessionId == SecondSemesterSessionId && s.SemesterId == (int)Semesters.SecondSemester).ToList();
                    if (SecondSemesterResults != null && SecondSemesterResults.Count > 0)
                    {
                        SecondSemesterGPA = SecondSemesterResults.FirstOrDefault().GPA ?? 0;
                    }
                    var ThirdSemesterResults = results.Where(s => s.SessionId == ThirdSemesterSessionId && s.SemesterId == (int)Semesters.FirstSemester).ToList();
                    if (ThirdSemesterResults != null && ThirdSemesterResults.Count > 0)
                    {
                        ThirdSemesterGPA = ThirdSemesterResults.FirstOrDefault().GPA ?? 0;
                    }

                    var FourthSemesterResult = results.Where(s => s.SessionId == FourthSemesterSessionId && s.SemesterId == (int)Semesters.SecondSemester).ToList();
                    if (FourthSemesterResult != null && FourthSemesterResult.Count > 0)
                    {
                        FourthSemesterGPA = FourthSemesterResult.FirstOrDefault().GPA ?? 0;
                        GPA = Decimal.Round(FourthSemesterGPA, 2);
                    }
                    
                    CGPA = Decimal.Round((Decimal.Round(FirstSemesterGPA, 2) + Decimal.Round(SecondSemesterGPA, 2) + Decimal.Round(ThirdSemesterGPA, 2) + Decimal.Round(FourthSemesterGPA, 2)) / semesterWeight, 2);
                }
                else
                {

                }

                aggregateStanding.FirstSemesterGPA = FirstSemesterGPA;
                aggregateStanding.SecondSemesterGPA = SecondSemesterGPA;
                aggregateStanding.ThirdSemesterGPA = ThirdSemesterGPA;
                aggregateStanding.FourthSemesterGPA = FourthSemesterGPA;
                aggregateStanding.GPA = GPA;
                aggregateStanding.CGPA = CGPA;



                return aggregateStanding;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        
        private string GetFirstYearFirstSemeterRemark(decimal? currentCGPA)
        {
            string remark = null;
            try
            {
                if (currentCGPA != null)
                {
                    if (currentCGPA < (decimal)2.0)
                    {
                        remark = "WITHRADWN ";
                    }
                    else if (currentCGPA < (decimal)2.0)
                    {
                        remark = "PROBATION ";
                    }
                    else if (currentCGPA < (decimal)2.0)
                    {
                        remark = "PROBATION ";
                    }
                    
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return remark;
        }

        private string GetFirstYearSecondSemeterRemark(decimal? firstSemesterGPA, decimal? currentCGPA)
        {
            string remark = null;
            try
            {
                if (firstSemesterGPA != null && currentCGPA != null)
                {
                    if (firstSemesterGPA < (decimal)2.0 && currentCGPA < (decimal)2.0)
                    {
                        remark = "WITHRADWN ";
                    }
                    else if (firstSemesterGPA < (decimal)2.0)
                    {
                        remark = "PROBATION ";
                    }
                    else if (firstSemesterGPA < (decimal)2.0)
                    {
                        remark = "PROBATION ";
                    }

                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return remark;
        }

        private string GetSecondYearFirstSemeterRemark(decimal? firstSemesterGPA, decimal? secondSemesterGPA)
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
                    
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return remark;
        }

        private bool StudentIsClass()
        {
            return false;
        }
    }
}
