using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Model.Translator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Abundance_Nk.Business
{
    public class ResultProcessingLogic : BusinessBaseLogic<StudentResult, STUDENT_RESULT>
    {
        private StudentResultDetailLogic studentResultDetailLogic;

        public ResultProcessingLogic()
        {
            translator = new StudentResultTranslator();
            studentResultDetailLogic = new StudentResultDetailLogic();
        }

        public async Task<List<Result>> GetGraduationList(Programme programme, Department department, SessionSemester sessionSemester, int type, DepartmentOption departmentOption)
        {
            List<Result> results = new List<Result>();
            List<Result> graduatingStudents = new List<Result>();
            try
            {
                Level level = null;
                LevelLogic levelLogic = new LevelLogic();
                ProgrammeLogic programmeLogic = new ProgrammeLogic();
                SessionLogic sessionLogic = new SessionLogic();
                DepartmentLogic departmentLogic = new DepartmentLogic();
                SemesterLogic semesterLogic = new SemesterLogic();
                SessionSemesterLogic sessionSemesterLogic = new Business.SessionSemesterLogic();
                SessionSemester ss = sessionSemesterLogic.GetBy(sessionSemester.Id);
                programme = programmeLogic.GetModelBy(g => g.Programme_Id == programme.Id);
                department = departmentLogic.GetModelBy(f => f.Department_Id == department.Id);
                Session session = sessionLogic.GetModelBy(f => f.Session_Id == ss.Session.Id);
                Semester semester = semesterLogic.GetModelBy(f => f.Semester_Id == ss.Semester.Id);

                if (programme.Id == 1 || programme.Id == 2) //ND fulltime or ND Part time
                {
                    level = levelLogic.GetModelBy(g => g.Level_Id == 2);

                }
                else if (programme.Id == 3 || programme.Id == 4 || programme.Id == 5) //HND fulltime or HND Part time or HND Weekend
                {
                    level = levelLogic.GetModelBy(g => g.Level_Id == 4);
                }
                else
                {
                    throw new Exception("This Programme has not been setup for graduation list.");
                }

                string[] sessionItems = session.Name.Split('/');
                string sessionNameStr = sessionItems[0];
                int sessionNameInt = Convert.ToInt32(sessionNameStr);

                string currentSessionSuffix = sessionNameStr.Substring(2, 2);
                string yearTwoSessionSuffix = Convert.ToString((Convert.ToInt32(currentSessionSuffix) - 1));

                currentSessionSuffix = "/" + currentSessionSuffix + "/";
                yearTwoSessionSuffix = "/" + yearTwoSessionSuffix + "/";

                string semesterCode = GetSemesterCodeBy(ss.Semester.Id);
                string sessionCode = GetSessionCodeBy(ss.Session.Name);
                string identifier = department.Code + level.Name + semesterCode + sessionCode;

                var classStudents = await GetStudentsForGraduationList(programme, department, session, level, departmentOption);
                var distinctMatricNo = classStudents.Where(s => s.MatricNumber.Contains(yearTwoSessionSuffix)).OrderBy(a => a.MatricNumber).Select(f => f.MatricNumber).Distinct().ToList();
                //var allResults = await GetAllStudentResult(distinctMatricNo);
                var allResults = new List<Result>();

                foreach (var matricNo in distinctMatricNo)
                {
                    bool isExtraYear = false;
                    if (matricNo == "PN/INS/18/0058")
                    {
                        var good = 0;
                    }
                    List<string> matricNos = new List<string>();
                    matricNos.Add(matricNo);
                    //confirm that student has completed school fees before computing his/her graduation list
                    //var hasPaidCompletely=HascompletedSchoolFees(matricNo);
                    //if (!hasPaidCompletely)
                        //continue;
                    var currentStudentResult = classStudents.Where(f => f.MatricNumber == matricNo).LastOrDefault();
                    decimal firstYearFirstSemesterGPA = 0M;
                    decimal firstYearSecondSemesterGPA = 0M;
                    decimal firstYearThirdSemesterGPA = 0M;
                    decimal secondYearFirstSemesterGPA = 0M;
                    decimal secondYearSecondSemesterGPA = 0M;
                    decimal secondYearThirdSemesterGPA = 0M;

                    List<decimal> TotalCoursePoints = new List<decimal>();
                    List<int> TotalCourseUnits = new List<int>();
                    List<Course> oustandingCourses = new List<Course>();
                    allResults.AddRange(await GetAllStudentResult(matricNos));
                    var carryOvers = allResults.Where(f => f.MatricNumber == matricNo && f.CourseUnit > 0 && ((f.TotalScore < 40 || f.SpecialCase != null) || (f.TotalScore == null && f.SpecialCase == null))).ToList();
                    if (carryOvers != null && carryOvers.Count > 0)
                    {

                        foreach (var carryOverCourse in carryOvers)
                        {
                            //Filter off Extra-year student
                            var specificCourse = carryOvers.Where(f => f.CourseId == carryOverCourse.CourseId).ToList();
                            if (specificCourse?.Count >= 2)
                            {
                                isExtraYear = true;
                            }
                            else
                            {
                                var passedCarryOver = allResults.Where(f => f.CourseId == carryOverCourse.CourseId && f.MatricNumber == matricNo && f.TotalScore >= 40).FirstOrDefault();
                                if (passedCarryOver == null)
                                {
                                    if (!CheckForExistingCarryOver(oustandingCourses, carryOverCourse.CourseId))
                                    {
                                        oustandingCourses.Add(new Course() { Code = carryOverCourse.CourseCode, Id = carryOverCourse.CourseId, Unit = carryOverCourse.CourseUnit });
                                    }
                                    //oustandingCourses.Add(new Course() { Code = carryOverCourse.CourseCode, Id = carryOverCourse.CourseId, Unit = carryOverCourse.CourseUnit });
                                }
                            }

                        }

                    }
                    if (oustandingCourses.Count == 0 && !isExtraYear)
                    {
                        int firstYearLevel = level.Id - 1;
                        var firstYearFirstSemesterResult = allResults.Where(f => f.MatricNumber == matricNo && f.SemesterId == 1 && f.LevelId == firstYearLevel).ToList();
                        if (firstYearFirstSemesterResult != null && firstYearFirstSemesterResult.Count > 0)
                        {
                            TotalCourseUnits.Add(firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                            TotalCoursePoints.Add((firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                            firstYearFirstSemesterGPA = ((firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                        }



                        var firstYearSecondSemesterResult = allResults.Where(f => f.MatricNumber == matricNo && f.SemesterId == 2 && f.LevelId == firstYearLevel).ToList();
                        if (firstYearSecondSemesterResult != null && firstYearSecondSemesterResult.Count > 0)
                        {

                            TotalCourseUnits.Add(firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                            TotalCoursePoints.Add((firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                            firstYearSecondSemesterGPA = ((firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                        }


                        if (programme.Id == 2 || programme.Id == 5 && (session.Id >= 9 && (firstYearLevel == 1 || firstYearLevel == 3)))
                        {
                            var firstYearThirdSemesterResult = allResults.Where(f => f.MatricNumber == matricNo && f.SemesterId == 3 && f.LevelId == firstYearLevel).ToList();
                            if (firstYearThirdSemesterResult != null && firstYearThirdSemesterResult.Count > 0)
                            {

                                TotalCourseUnits.Add(firstYearThirdSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                TotalCoursePoints.Add((firstYearThirdSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                                firstYearThirdSemesterGPA = ((firstYearThirdSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / firstYearThirdSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                            }

                        }

                        var secondYearFirstSemesterResult = allResults.Where(f => f.MatricNumber == matricNo && f.SemesterId == 1 && f.LevelId == level.Id).ToList();
                        if (secondYearFirstSemesterResult != null && secondYearFirstSemesterResult.Count > 0)
                        {
                            TotalCourseUnits.Add(secondYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                            TotalCoursePoints.Add((secondYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                            secondYearFirstSemesterGPA = ((secondYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / secondYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                        }



                        var secondYearSecondSemesterResult = allResults.Where(f => f.MatricNumber == matricNo && f.SemesterId == 2 && f.LevelId == level.Id).ToList();
                        if (secondYearSecondSemesterResult != null && secondYearSecondSemesterResult.Count > 0)
                        {
                            TotalCourseUnits.Add(secondYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                            TotalCoursePoints.Add((secondYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                            secondYearSecondSemesterGPA = ((secondYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / secondYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                        }


                        decimal CGPA = Math.Round(TotalCoursePoints.Sum() / TotalCourseUnits.Sum(), 2);

                        //decimal CGPA = Math.Round((firstYearFirstSemesterGPA + firstYearSecondSemesterGPA + secondYearFirstSemesterGPA + secondYearSecondSemesterGPA) / 4, 2);
                        //Account for Third Semester for ND parttime and HND Evening
                        if ((programme.Id == 2 || programme.Id == 5) && (session.Id >= 9 && (level.Id == 2 || level.Id == 4)))
                        {
                            var secondYearThirdSemesterResult = allResults.Where(f => f.MatricNumber == matricNo && f.SemesterId == 3 && f.LevelId == level.Id).ToList();
                            if (secondYearThirdSemesterResult != null && secondYearThirdSemesterResult.Count > 0)
                            {
                                TotalCourseUnits.Add(secondYearThirdSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                TotalCoursePoints.Add((secondYearThirdSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                                secondYearThirdSemesterGPA = ((secondYearThirdSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / secondYearThirdSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                //CGPA = Math.Round((firstYearFirstSemesterGPA + firstYearSecondSemesterGPA + secondYearFirstSemesterGPA + secondYearSecondSemesterGPA + firstYearThirdSemesterGPA + secondYearThirdSemesterGPA) / 6, 2);
                                CGPA = Math.Round(TotalCoursePoints.Sum() / TotalCourseUnits.Sum(), 2);

                            }

                        }

                        string Remark = GetGraduationStatus(CGPA);

                        currentStudentResult.CGPA = CGPA;
                        currentStudentResult.Remark = Remark;
                        currentStudentResult.Identifier = identifier;
                        currentStudentResult.Count = 1;
                        currentStudentResult.Semestername = semester.Name;
                        currentStudentResult.Name = currentStudentResult.Name.ToUpper();
                        if (departmentOption != null && departmentOption.Id > 0)
                        {
                            DepartmentOptionLogic departmentOptionLogic = new DepartmentOptionLogic();
                            var option = departmentOptionLogic.GetModelBy(f => f.Department_Option_Id == departmentOption.Id);
                            if (option != null)
                            {
                                currentStudentResult.DepartmentName = currentStudentResult.DepartmentName + " (" + option.Name + ")";
                            }

                        }

                        graduatingStudents.Add(currentStudentResult);
                    }

                }

                switch (type)
                {
                    case 1:
                        return graduatingStudents.OrderBy(a => a.Remark).ToList();
                    case 2:
                        return graduatingStudents.OrderByDescending(a => a.CGPA).ToList();
                    case 3:
                        return graduatingStudents.OrderBy(a => a.MatricNumber).ToList();
                    case 4:
                        return graduatingStudents.OrderBy(a => a.Name).ToList();
                    default:
                        return graduatingStudents.OrderBy(a => a.MatricNumber).ToList();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private async Task<List<Result>> GetAllStudentResult(List<string> distinctMatricNo)
        {
            try
            {
                List<Result> allResult = (from sr in await repository.GetByAsync<VW_STUDENT_RESULT_2>(p => distinctMatricNo.Contains(p.Matric_Number))
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
                                              SessionId = sr.Session_Id,
                                              LevelId = sr.Level_Id,
                                              SemesterId = sr.Semester_Id,
                                              CourseModeId = sr.Course_Mode_Id
                                          }).ToList();
                return allResult;
            }
            catch (Exception ex)
            {

                throw ex;
            }
            
        }

        private async Task<List<Result>> GetAllStudentResultBySession(List<string> distinctMatricNo, Session session)
        {
            List<Result> allResult = (from sr in await repository.GetByAsync<VW_STUDENT_RESULT_2>(p => distinctMatricNo.Contains(p.Matric_Number) && p.Session_Id == session.Id)
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
                                          SessionId = sr.Session_Id,
                                          LevelId = sr.Level_Id,
                                          SemesterId = sr.Semester_Id,
                                          CourseModeId = sr.Course_Mode_Id
                                      }).ToList();
            return allResult;
        }

        private async Task<List<Result>> GetStudentResults(long PersonId, int LevelId,int sessionId)
        {
            var results = new List<Result>();
            if (LevelId == 1 || LevelId == 3)
            {
                results = (from sr in await repository.GetByAsync<VW_STUDENT_RESULT_2>(p => p.Person_Id == PersonId && p.Total_Score != null && (p.Activated == true || p.Activated == null || (p.Activated == false && p.Reason.Contains("System deactivated"))) && p.Level_Id == LevelId)
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
                               SessionId = sr.Session_Id,
                               LevelId = sr.Level_Id,
                               SemesterId = sr.Semester_Id,
                               CourseModeId = sr.Course_Mode_Id
                           }).ToList();
            }
            else
            {
                results = (from sr in await repository.GetByAsync<VW_STUDENT_RESULT_2>(p => p.Person_Id == PersonId && p.Total_Score!=null && (p.Activated == true || p.Activated == null || (p.Activated == false && p.Reason.Contains("System deactivated"))) && p.Session_Id<=sessionId)
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
                               SessionId = sr.Session_Id,
                               LevelId = sr.Level_Id,
                               SemesterId = sr.Semester_Id,
                               CourseModeId = sr.Course_Mode_Id
                           }).ToList();
            }


            return results;
        }

        private async Task<List<Result>> GetStudents(Programme programme, Department department, Session session, Level level, DepartmentOption departmentOption)
        {
            List<Result> results = null;
            if (departmentOption != null && departmentOption.Id > 0)
            {
                results = (from sr in await repository.GetByAsync<VW_STUDENT_RESULT_WITH_OPTIONS>(p => p.Programme_Id == programme.Id && p.Session_Id == session.Id &&
                                                   p.Level_Id == level.Id && p.Department_Id == department.Id && p.Department_Option_Id == departmentOption.Id &&  (p.Activated == true || p.Activated == null || (p.Activated == false && p.Reason.Contains("System deactivated")) ))
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
                               SessionId = sr.Session_Id,
                               LevelId = sr.Level_Id,
                               SemesterId = sr.Semester_Id,
                               CourseModeId = sr.Course_Mode_Id
                           }).ToList();
            }
            else
            {
                results = (from sr in await repository.GetByAsync<VW_STUDENT_RESULT_2>(p => p.Programme_Id == programme.Id && p.Session_Id == session.Id &&
                                                p.Level_Id == level.Id && p.Department_Id == department.Id && (p.Activated == true || p.Activated == null || (p.Activated == false && p.Reason.Contains("System deactivated"))))
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
                               SessionId = sr.Session_Id,
                               LevelId = sr.Level_Id,
                               SemesterId = sr.Semester_Id,
                               CourseModeId = sr.Course_Mode_Id
                           }).ToList();
            }

            return results;
        }
        private async Task<List<Result>> GetStudentsForGraduationList(Programme programme, Department department, Session session, Level level, DepartmentOption departmentOption)
        {
            List<Result> results = null;
            if (departmentOption != null && departmentOption.Id > 0)
            {
                results = (from sr in await repository.GetByAsync<VW_STUDENT_RESULT_WITH_OPTIONS>(p => p.Programme_Id == programme.Id && p.Session_Id == session.Id &&
                                                   p.Level_Id == level.Id && p.Department_Id == department.Id && p.Department_Option_Id == departmentOption.Id && (p.Activated == true || p.Activated == null))
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
                               SessionId = sr.Session_Id,
                               LevelId = sr.Level_Id,
                               SemesterId = sr.Semester_Id,
                               CourseModeId = sr.Course_Mode_Id
                           }).ToList();
            }
            else
            {
                results = (from sr in await repository.GetByAsync<VW_STUDENT_RESULT_2>(p => p.Programme_Id == programme.Id && p.Session_Id == session.Id &&
                                                p.Level_Id == level.Id && p.Department_Id == department.Id && (p.Activated == true || p.Activated == null))
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
                               SessionId = sr.Session_Id,
                               LevelId = sr.Level_Id,
                               SemesterId = sr.Semester_Id,
                               CourseModeId = sr.Course_Mode_Id
                           }).ToList();
            }

            return results;
        }
        [AsyncTimeout(300000)]
        public async Task<List<Result>> GetAggregateSheetPartTime(SessionSemester sessionSemester, Level level, Programme programme, Department department, CourseMode courseMode, DepartmentOption departmentOption)
        {
            List<Result> results = new List<Result>();

            try
            {


                LevelLogic levelLogic = new LevelLogic();
                ProgrammeLogic programmeLogic = new ProgrammeLogic();
                SessionLogic sessionLogic = new SessionLogic();
                DepartmentLogic departmentLogic = new DepartmentLogic();
                SessionSemesterLogic sessionSemesterLogic = new Business.SessionSemesterLogic();
                SemesterLogic semesterLogic = new SemesterLogic();
                CourseModeLogic courseModeLogic = new CourseModeLogic();
                SessionSemester ss = sessionSemesterLogic.GetBy(sessionSemester.Id);
                programme = programmeLogic.GetModelBy(g => g.Programme_Id == programme.Id);
                department = departmentLogic.GetModelBy(f => f.Department_Id == department.Id);
                Session session = sessionLogic.GetModelBy(f => f.Session_Id == ss.Session.Id);
                level = levelLogic.GetModelBy(g => g.Level_Id == level.Id);

                Semester semester = semesterLogic.GetModelBy(f => f.Semester_Id == ss.Semester.Id);
                
                if (departmentOption?.Id > 0)
                {
                    DepartmentOptionLogic departmentOptionLogic = new DepartmentOptionLogic();
                    departmentOption = departmentOptionLogic.GetModelBy(f => f.Department_Option_Id == departmentOption.Id);
                }
                
                List<string> withdrawnStudentMatricNo = new List<string>();
                if (courseMode == null)
                {
                    courseMode = courseModeLogic.GetAll().FirstOrDefault();
                }
                else
                {
                    courseMode = courseModeLogic.GetModelBy(c => c.Course_Mode_Id == courseMode.Id);
                }

                string[] sessionItems = session.Name.Split('/');
                string sessionNameStr = sessionItems[0];
                int sessionNameInt = Convert.ToInt32(sessionNameStr);

                string currentSessionSuffix = sessionNameStr.Substring(2, 2); // 18
                string yearTwoSessionSuffix = Convert.ToString((Convert.ToInt32(currentSessionSuffix) - 1)); //17


                currentSessionSuffix = "/" + currentSessionSuffix + "/";
                yearTwoSessionSuffix = "/" + yearTwoSessionSuffix + "/";

                string previousSessionName = Convert.ToString((sessionNameInt - 1)) + "/" + Convert.ToString((sessionNameInt));
                Session previousSession = sessionLogic.GetModelsBy(s => s.Session_Name == previousSessionName.Trim()).LastOrDefault();
                if (previousSession == null)
                {
                    throw new Exception("Cannot Find Previous Session");
                }

                string semesterCode = GetSemesterCodeBy(ss.Semester.Id);
                string sessionCode = GetSessionCodeBy(ss.Session.Name);
                string identifier = department.Code + level.Name + semesterCode + sessionCode;

                var classStudents = await GetStudents(programme, department, session, level, departmentOption);
                var classStudents2 = classStudents.Where(c => c.CourseModeId == courseMode.Id).ToList();
                List<string> distinctMatricNo = new List<string>();
                if (level.Id == 1 || level.Id == 3)
                {
                    distinctMatricNo = classStudents2.Where(s => s.MatricNumber.Contains(currentSessionSuffix)).OrderBy(a => a.MatricNumber).Select(f => f.MatricNumber).Distinct().ToList();
                }
                else
                {
                    distinctMatricNo = classStudents2.Where(s => s.MatricNumber.Contains(yearTwoSessionSuffix)).OrderBy(a => a.MatricNumber).Select(f => f.MatricNumber).Distinct().ToList();
                }

                var previousSessionResults = await GetAllStudentResultBySession(distinctMatricNo, previousSession);

                foreach (var matricNo in distinctMatricNo)
                {
                    if(matricNo== "PN/MKT/17/1378")
                    {

                    }
                    List<Probation> probationList = new List<Probation>();
                    var currentStudentResult = classStudents.Where(f => f.MatricNumber == matricNo && (f.TotalScore != null || f.SpecialCase!=null)).ToList();
                    var previousSessionStudentResults = previousSessionResults.Where(f => f.MatricNumber == matricNo && (f.TotalScore != null || f.SpecialCase != null)).ToList();

                    decimal firstYearFirstSemesterGPA = 0M;
                    decimal firstYearSecondSemesterGPA = 0M;
                    decimal firstYearThirdSemesterGPA = 0M;
                    decimal secondYearFirstSemesterGPA = 0M;
                    decimal secondYearSecondSemesterGPA = 0M;
                    decimal secondYearThirdSemesterGPA = 0M;
                    decimal currentSessionSemesterGPA = 0M;
                    decimal currentSessionSemesterCGPA = 0M;
                    decimal firstYearSecondSemesterCGPA = 0M;
                    decimal secondYearSecondSemesterCGPA = 0M;
                    decimal firstYearThirdSemesterCGPA = 0M;
                    decimal secondYearThirdSemesterCGPA = 0M;
                    decimal secondYearFirstSemesterCGPA = 0M;


                    List<Result> firstYearThirdSemesterResultHolder = null;
                    int TotalSemesterCourseUnit = 0;
                    int UnitOutstanding = 0;
                    int UnitPassed = 0;
                    List<decimal> TotalCoursePoints = new List<decimal>();
                    List<int> TotalCourseUnits = new List<int>();
                    List<Course> oustandingCourses = new List<Course>();
                    //Get outsatnding for previous session
                    if (previousSessionStudentResults != null && previousSessionStudentResults.Count > 0)
                    {
                        var carryOvers = previousSessionStudentResults.Where(f => f.MatricNumber == matricNo && f.CourseUnit > 0 && ((f.TotalScore < 40 || f.SpecialCase != null))).ToList();
                        if (carryOvers != null && carryOvers.Count > 0)
                        {
                            foreach (var carryOverCourse in carryOvers)
                            {
                                var passedCarryOver = previousSessionStudentResults.Where(f => f.CourseId == carryOverCourse.CourseId && f.MatricNumber == matricNo && f.TotalScore >= 40).FirstOrDefault();
                                var passedCarryOverInCurrentSession = new Result();
                                if (currentStudentResult != null && currentStudentResult.Count > 0)
                                {
                                    passedCarryOverInCurrentSession = currentStudentResult.Where(f => f.CourseId == carryOverCourse.CourseId && f.MatricNumber == matricNo && f.TotalScore >= 40).FirstOrDefault();
                                }

                                if (passedCarryOver == null && passedCarryOverInCurrentSession == null)
                                {
                                    oustandingCourses.Add(new Course() { Code = carryOverCourse.CourseCode, Id = carryOverCourse.CourseId, Unit = carryOverCourse.CourseUnit });
                                }
                            }

                        }
                    }
                    //Get outstanding for current session
                    if (currentStudentResult != null && currentStudentResult.Count > 0)
                    {
                        var carryOvers = new List<Result>();
                        //Only show the outstanding of first semester of the selected level if first year and first semester is selected
                        if ((level.Id == 1 || level.Id == 3) && semester.Id == 1)
                        {
                            carryOvers = currentStudentResult.Where(f => f.MatricNumber == matricNo && f.CourseUnit > 0 && ((f.TotalScore < 40 || f.SpecialCase != null)) && f.LevelId == level.Id && f.SemesterId == semester.Id).ToList();
                        }
                        else
                        {
                            carryOvers = currentStudentResult.Where(f => f.MatricNumber == matricNo && f.CourseUnit > 0 && ((f.TotalScore < 40 || f.SpecialCase != null))).ToList();
                        }

                        if (carryOvers != null && carryOvers.Count > 0)
                        {
                            foreach (var carryOverCourse in carryOvers)
                            {
                                var passedCarryOver = currentStudentResult.Where(f => f.CourseId == carryOverCourse.CourseId && f.MatricNumber == matricNo && f.TotalScore >= 40).FirstOrDefault();
                                if (passedCarryOver == null)
                                {
                                    if (!CheckForExistingCarryOver(oustandingCourses, carryOverCourse.CourseId))
                                    {
                                        oustandingCourses.Add(new Course() { Code = carryOverCourse.CourseCode, Id = carryOverCourse.CourseId, Unit = carryOverCourse.CourseUnit });
                                    }
                                    //oustandingCourses.Add(new Course() { Code = carryOverCourse.CourseCode, Id = carryOverCourse.CourseId, Unit = carryOverCourse.CourseUnit });
                                }
                            }

                        }
                    }
                    var currentSessionSemester = currentStudentResult.Where(f => f.MatricNumber == matricNo && f.SemesterId == semester.Id).ToList();
                    if (currentSessionSemester != null && currentSessionSemester.Count > 0)
                    {

                        TotalCourseUnits.Add(currentSessionSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                        TotalCoursePoints.Add((currentSessionSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                        if ((currentSessionSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) > 0)
                        {
                            currentSessionSemesterGPA = ((currentSessionSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / currentSessionSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                        }

                        TotalSemesterCourseUnit = currentSessionSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(s => s.CourseUnit);
                        UnitOutstanding = currentSessionSemester.Where(c => (c.TotalScore < 40 || c.SpecialCase != null) || (c.TotalScore == null && c.SpecialCase == null)).Sum(c => c.CourseUnit);
                        UnitPassed = currentSessionSemester.Where(c => c.TotalScore >= 40).Sum(c => c.CourseUnit);

                        if (level.Id == 1 || level.Id == 3)
                        {
                            if (semester.Id == 1)
                            {
                                firstYearFirstSemesterGPA = currentSessionSemesterGPA;
                                //Get list of GPA within the probation range

                                if (firstYearFirstSemesterGPA < (decimal)2.0)
                                {
                                    Probation probation = new Probation();
                                    probation.CGPA = firstYearFirstSemesterGPA;
                                    probation.Semester = new Semester() { Id = 1 };
                                    probation.Level = level;
                                    probationList.Add(probation);
                                }

                                currentSessionSemesterCGPA = Math.Round(firstYearFirstSemesterGPA, 2);
                            }
                            else if (semester.Id == 2)
                            {
                                var firstYearFirstSemester = currentStudentResult.Where(f => f.MatricNumber == matricNo && f.SemesterId == 1).ToList();
                                if (firstYearFirstSemester != null && firstYearFirstSemester.Count > 0)
                                {
                                    TotalCourseUnits.Add(firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                    TotalCoursePoints.Add((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                                    if ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) > 0)
                                    {
                                        firstYearFirstSemesterGPA = ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                    }

                                }
                                //Get list of CGPA within the probation range
                                if (firstYearFirstSemesterGPA < (decimal)2.0)
                                {
                                    Probation probation = new Probation();
                                    probation.CGPA = firstYearFirstSemesterGPA;
                                    probation.Semester = new Semester() { Id = 1 };
                                    probation.Level = level;
                                    probationList.Add(probation);
                                }
                                firstYearSecondSemesterGPA = currentSessionSemesterGPA;
                                if (TotalCourseUnits.Sum() > 0)
                                    currentSessionSemesterCGPA = Math.Round(TotalCoursePoints.Sum() / TotalCourseUnits.Sum(), 2);
                                firstYearSecondSemesterCGPA = currentSessionSemesterCGPA;
                                //Get list of CGPA within the probation range
                                if (firstYearSecondSemesterCGPA < (decimal)2.0)
                                {
                                    Probation probation = new Probation();
                                    probation.CGPA = firstYearSecondSemesterCGPA;
                                    probation.Semester = new Semester() { Id = 2 };
                                    probation.Level = level;
                                    probationList.Add(probation);
                                }



                            }
                            else if (semester.Id == 3)
                            {
                                if ((session.Id >= 9 && (level.Id == 1 || level.Id == 3)))
                                {
                                    var firstYearFirstSemester = currentStudentResult.Where(f => f.MatricNumber == matricNo && f.SemesterId == 1).ToList();
                                    if (firstYearFirstSemester != null && firstYearFirstSemester.Count > 0)
                                    {
                                        TotalCourseUnits.Add(firstYearFirstSemester.Sum(x => x.CourseUnit));
                                        TotalCoursePoints.Add((firstYearFirstSemester.Sum(x => x.GPCU) ?? 0));
                                        if ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) > 0)
                                        {
                                            firstYearFirstSemesterGPA = ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                        }

                                    }
                                    //Get list of CGPA within the probation range
                                    if (firstYearFirstSemesterGPA < (decimal)2.0)
                                    {
                                        Probation probation = new Probation();
                                        probation.CGPA = firstYearFirstSemesterGPA;
                                        probation.Semester = new Semester() { Id = 1 };
                                        probation.Level = level;
                                        probationList.Add(probation);
                                    }
                                    var firstYearSecondSemester = currentStudentResult.Where(f => f.MatricNumber == matricNo && f.SemesterId == 2).ToList();
                                    if (firstYearSecondSemester != null && firstYearSecondSemester.Count > 0)
                                    {

                                        TotalCourseUnits.Add(firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                        TotalCoursePoints.Add((firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                                        if ((firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) > 0)
                                        {
                                            firstYearSecondSemesterGPA = ((firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                        }
                                        if ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) > 0)
                                        {
                                            //Get the CGPA of both First year first and second semesters
                                            firstYearSecondSemesterCGPA = ((firstYearFirstSemester.Sum(x => x.GPCU) ?? 0) + (firstYearSecondSemester.Sum(x => x.GPCU) ?? 0)) / ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)));
                                        }

                                    }
                                    //Get list of CGPA within the probation range
                                    if (firstYearSecondSemesterCGPA < (decimal)2.0)
                                    {
                                        Probation probation = new Probation();
                                        probation.CGPA = firstYearSecondSemesterCGPA;
                                        probation.Semester = new Semester() { Id = 2 };
                                        probation.Level = level;
                                        probationList.Add(probation);
                                    }
                                    firstYearThirdSemesterGPA = currentSessionSemesterGPA;
                                    if (TotalCourseUnits.Sum() > 0)
                                        currentSessionSemesterCGPA = Math.Round(TotalCoursePoints.Sum() / TotalCourseUnits.Sum(), 2);
                                    firstYearThirdSemesterCGPA = currentSessionSemesterCGPA;
                                    //Get list of GPA within the probation range
                                    if (firstYearThirdSemesterCGPA < (decimal)2.0)
                                    {
                                        Probation probation = new Probation();
                                        probation.CGPA = firstYearThirdSemesterCGPA;
                                        probation.Semester = new Semester() { Id = 3 };
                                        probation.Level = level;
                                        probationList.Add(probation);
                                    }



                                }
                                else
                                {
                                    throw new Exception("Selected Session-Semester is not valid for selected Programme");
                                }
                            }
                        }
                        else
                        {
                            var firstYearLevelId = level.Id - 1;
                            if (previousSessionResults != null && previousSessionResults.Count > 0)
                            {
                                if (semester.Id == 1)
                                {

                                    var firstYearFirstSemester = previousSessionResults.Where(f => f.MatricNumber == matricNo && f.SemesterId == 1).ToList();
                                    if (firstYearFirstSemester != null && firstYearFirstSemester.Count > 0)
                                    {
                                        TotalCourseUnits.Add(firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                        TotalCoursePoints.Add((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                                        if ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) > 0)
                                        {
                                            firstYearFirstSemesterGPA = ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                        }

                                        //Get list of CGPA within the probation range
                                        if (firstYearFirstSemesterGPA < (decimal)2.0)
                                        {
                                            Probation probation = new Probation();
                                            probation.CGPA = firstYearFirstSemesterGPA;
                                            probation.Semester = new Semester() { Id = 1 };
                                            probation.Level = new Level() { Id = firstYearLevelId };
                                            probationList.Add(probation);
                                        }

                                        var firstYearSecondSemester = previousSessionResults.Where(f => f.MatricNumber == matricNo && f.SemesterId == 2).ToList();

                                        if (firstYearSecondSemester != null && firstYearSecondSemester.Count > 0)
                                        {
                                            TotalCourseUnits.Add(firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                            TotalCoursePoints.Add((firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                                            if ((firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) > 0)
                                            {
                                                firstYearSecondSemesterGPA = ((firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                            }

                                            if (((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit))) > 0)
                                            {
                                                //Get the CGPA of both First year first and second semesters
                                                firstYearSecondSemesterCGPA = ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0)) / ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)));
                                            }

                                            //Get list of GPA within the probation range
                                            if (firstYearSecondSemesterCGPA < (decimal)2.0)
                                            {
                                                Probation probation = new Probation();
                                                probation.CGPA = firstYearSecondSemesterCGPA;
                                                probation.Semester = new Semester() { Id = 2 };
                                                probation.Level = new Level() { Id = firstYearLevelId };
                                                probationList.Add(probation);
                                            }

                                            secondYearFirstSemesterGPA = currentSessionSemesterGPA;


                                            //Account for third semester for selected Programmes(ND Part Time and HND Evening)
                                            if ((session.Id >= 9 && (level.Id == 2 || level.Id == 4)))
                                            {
                                                var firstYearThirdSemester = previousSessionResults.Where(f => f.MatricNumber == matricNo && f.SemesterId == 3).ToList();

                                                if (firstYearThirdSemester != null && firstYearThirdSemester.Count > 0)
                                                {
                                                    TotalCourseUnits.Add(firstYearThirdSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                                    TotalCoursePoints.Add((firstYearThirdSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                                                    if ((firstYearThirdSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) > 0)
                                                    {
                                                        firstYearThirdSemesterGPA = ((firstYearThirdSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / firstYearThirdSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                                    }

                                                    if (((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearThirdSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit))) > 0)
                                                    {
                                                        //Get the CGPA of both First year first,second and Third semesters
                                                        firstYearThirdSemesterCGPA = ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (firstYearThirdSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0)) / ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearThirdSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)));
                                                    }


                                                    //Get list of CGPA within the probation range
                                                    if (firstYearThirdSemesterCGPA < (decimal)2.0)
                                                    {
                                                        Probation probation = new Probation();
                                                        probation.CGPA = firstYearThirdSemesterCGPA;
                                                        probation.Semester = new Semester() { Id = 3 };
                                                        probation.Level = new Level() { Id = firstYearLevelId };
                                                        probationList.Add(probation);
                                                    }
                                                    secondYearFirstSemesterGPA = currentSessionSemesterGPA;

                                                    //currentSessionSemesterCGPA = Math.Round(((firstYearFirstSemesterGPA + firstYearSecondSemesterGPA + firstYearThirdSemesterGPA + secondYearFirstSemesterGPA) / 4), 2);
                                                    if ((TotalCourseUnits.Sum()) > 0)
                                                    {
                                                        currentSessionSemesterCGPA = Math.Round(TotalCoursePoints.Sum() / TotalCourseUnits.Sum(), 2);
                                                    }

                                                    secondYearFirstSemesterCGPA = currentSessionSemesterCGPA;
                                                    //Get list of CGPA within the probation range
                                                    if (secondYearFirstSemesterCGPA < (decimal)2.0)
                                                    {
                                                        Probation probation = new Probation();
                                                        probation.CGPA = secondYearFirstSemesterCGPA;
                                                        probation.Semester = new Semester() { Id = 1 };
                                                        probation.Level = level;
                                                        probationList.Add(probation);
                                                    }


                                                }
                                            }
                                            else
                                            {
                                                if ((TotalCourseUnits.Sum()) > 0)
                                                {
                                                    currentSessionSemesterCGPA = Math.Round(TotalCoursePoints.Sum() / TotalCourseUnits.Sum(), 2);
                                                }
                                                //Get list of CGPA within the probation range
                                                if (currentSessionSemesterCGPA < (decimal)2.0)
                                                {
                                                    Probation probation = new Probation();
                                                    probation.CGPA = currentSessionSemesterCGPA;
                                                    probation.Semester = new Semester() { Id = 1 };
                                                    probation.Level = level;
                                                    probationList.Add(probation);
                                                }

                                                //currentSessionSemesterCGPA = Math.Round(((firstYearFirstSemesterGPA + firstYearSecondSemesterGPA + secondYearFirstSemesterGPA) / 3), 2);
                                            }

                                        }

                                    }

                                }
                                else if (semester.Id == 2)
                                {
                                    var firstYearFirstSemester = previousSessionResults.Where(f => f.MatricNumber == matricNo && f.SemesterId == 1).ToList();
                                    if (firstYearFirstSemester != null && firstYearFirstSemester.Count > 0)
                                    {
                                        TotalCourseUnits.Add(firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                        TotalCoursePoints.Add((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                                        if ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) > 0)
                                        {
                                            firstYearFirstSemesterGPA = ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                        }



                                        //Get list of CGPA within the probation range
                                        if (firstYearFirstSemesterGPA < (decimal)2.0)
                                        {
                                            Probation probation = new Probation();
                                            probation.CGPA = firstYearFirstSemesterGPA;
                                            probation.Semester = new Semester() { Id = 1 };
                                            probation.Level = new Level() { Id = firstYearLevelId };
                                            probationList.Add(probation);
                                        }
                                        var firstYearSecondSemester = previousSessionResults.Where(f => f.MatricNumber == matricNo && f.SemesterId == 2).ToList();

                                        if (firstYearSecondSemester != null && firstYearSecondSemester.Count > 0)
                                        {

                                            TotalCourseUnits.Add(firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                            TotalCoursePoints.Add((firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                                            if ((firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) > 0)
                                            {
                                                firstYearSecondSemesterGPA = ((firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                            }

                                            if (((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit))) > 0)
                                            {
                                                //Get the CGPA of both First year first and second semesters
                                                firstYearSecondSemesterCGPA = ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0)) / ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)));
                                            }


                                            //Get list of CGPA within the probation range
                                            if (firstYearSecondSemesterCGPA < (decimal)2.0)
                                            {
                                                Probation probation = new Probation();
                                                probation.CGPA = firstYearSecondSemesterCGPA;
                                                probation.Semester = new Semester() { Id = 2 };
                                                probation.Level = new Level() { Id = firstYearLevelId };
                                                probationList.Add(probation);
                                            }
                                            //Account for third semester for selected Programmes(ND Part Time and HND Evening)
                                            if ((session.Id >= 9 && (level.Id == 2 || level.Id == 4)))
                                            {
                                                var firstYearThirdSemester = previousSessionResults.Where(f => f.MatricNumber == matricNo && f.SemesterId == 3).ToList();

                                                if (firstYearThirdSemester != null && firstYearThirdSemester.Count > 0)
                                                {
                                                    //assign firstYearThirdSemesterResult to a global holder;
                                                    firstYearThirdSemesterResultHolder = firstYearThirdSemester;
                                                    TotalCourseUnits.Add(firstYearThirdSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                                    TotalCoursePoints.Add((firstYearThirdSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                                                    if ((firstYearThirdSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) > 0)
                                                    {
                                                        firstYearThirdSemesterGPA = ((firstYearThirdSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / firstYearThirdSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                                    }

                                                    if (((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearThirdSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit))) > 0)
                                                    {
                                                        //Get the CGPA of both First year first,second and Third semesters
                                                        firstYearThirdSemesterCGPA = ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (firstYearThirdSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0)) / ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearThirdSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)));
                                                    }


                                                    //Get list of CGPA within the probation range
                                                    if (firstYearThirdSemesterCGPA < (decimal)2.0)
                                                    {
                                                        Probation probation = new Probation();
                                                        probation.CGPA = firstYearThirdSemesterCGPA;
                                                        probation.Semester = new Semester() { Id = 3 };
                                                        probation.Level = new Level() { Id = firstYearLevelId };
                                                        probationList.Add(probation);
                                                    }
                                                }
                                            }


                                            var secondYearFirstSemester = currentStudentResult.Where(f => f.MatricNumber == matricNo && f.SemesterId == 1).ToList();
                                            if (secondYearFirstSemester != null && secondYearFirstSemester.Count > 0)
                                            {
                                                TotalCourseUnits.Add(secondYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                                TotalCoursePoints.Add((secondYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                                                if ((secondYearFirstSemester.Sum(x => x.CourseUnit)) > 0)
                                                {
                                                    secondYearFirstSemesterGPA = ((secondYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / secondYearFirstSemester.Sum(x => x.CourseUnit));
                                                }


                                                //Get the CGPA of both First year first,second and Third semesters
                                                if (firstYearThirdSemesterResultHolder != null)
                                                {
                                                    if ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearThirdSemesterResultHolder.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (secondYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) > 0)
                                                    {
                                                        secondYearFirstSemesterCGPA = ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (firstYearThirdSemesterResultHolder.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (secondYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0)) / ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearThirdSemesterResultHolder.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (secondYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)));
                                                    }

                                                }
                                                else
                                                {
                                                    if (((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (secondYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit))) > 0)
                                                    {
                                                        secondYearFirstSemesterCGPA = ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (secondYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0)) / ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (secondYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)));
                                                    }

                                                }


                                                //Get list of CGPA within the probation range
                                                if (secondYearFirstSemesterCGPA < (decimal)2.0)
                                                {
                                                    Probation probation = new Probation();
                                                    probation.CGPA = secondYearFirstSemesterCGPA;
                                                    probation.Semester = new Semester() { Id = 1 };
                                                    probation.Level = level;
                                                    probationList.Add(probation);
                                                }
                                                secondYearSecondSemesterGPA = currentSessionSemesterGPA;

                                                //currentSessionSemesterCGPA = Math.Round(((firstYearFirstSemesterGPA + firstYearSecondSemesterGPA + secondYearFirstSemesterGPA + secondYearSecondSemesterGPA) / 4), 2);
                                                if ((TotalCourseUnits.Sum() > 0))
                                                {
                                                    currentSessionSemesterCGPA = Math.Round(TotalCoursePoints.Sum() / TotalCourseUnits.Sum(), 2);
                                                }

                                                if ((session.Id >= 9 && (level.Id == 1 || level.Id == 3)))
                                                {
                                                    //currentSessionSemesterCGPA = Math.Round(((firstYearFirstSemesterGPA + firstYearSecondSemesterGPA + firstYearThirdSemesterGPA + secondYearFirstSemesterGPA + secondYearSecondSemesterGPA) / 5), 2);

                                                    if ((TotalCourseUnits.Sum() > 0))
                                                    {
                                                        currentSessionSemesterCGPA = Math.Round(TotalCoursePoints.Sum() / TotalCourseUnits.Sum(), 2);
                                                    }

                                                }
                                                secondYearSecondSemesterCGPA = currentSessionSemesterCGPA;
                                                //Get list of CGPA within the probation range
                                                if (secondYearSecondSemesterCGPA < (decimal)2.0)
                                                {
                                                    Probation probation = new Probation();
                                                    probation.CGPA = secondYearSecondSemesterCGPA;
                                                    probation.Semester = new Semester() { Id = 2 };
                                                    probation.Level = level;
                                                    probationList.Add(probation);
                                                }

                                            }

                                        }
                                    }
                                }
                                else
                                {
                                    if ((session.Id >= 9 && (level.Id == 2 || level.Id == 4)))
                                    {
                                        var firstYearFirstSemester = previousSessionResults.Where(f => f.MatricNumber == matricNo && f.SemesterId == 1).ToList();
                                        if (firstYearFirstSemester != null && firstYearFirstSemester.Count > 0)
                                        {
                                            TotalCourseUnits.Add(firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                            TotalCoursePoints.Add((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                                            if ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) > 0)
                                            {
                                                firstYearFirstSemesterGPA = ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                            }

                                            //Get list of CGPA within the probation range
                                            if (firstYearFirstSemesterGPA < (decimal)2.0)
                                            {
                                                Probation probation = new Probation();
                                                probation.CGPA = firstYearFirstSemesterGPA;
                                                probation.Semester = new Semester() { Id = 1 };
                                                probation.Level = new Level() { Id = firstYearLevelId };
                                                probationList.Add(probation);
                                            }
                                            var firstYearSecondSemester = previousSessionResults.Where(f => f.MatricNumber == matricNo && f.SemesterId == 2).ToList();

                                            if (firstYearSecondSemester != null && firstYearSecondSemester.Count > 0)
                                            {
                                                TotalCourseUnits.Add(firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                                TotalCoursePoints.Add((firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                                                if ((firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) > 0)
                                                {
                                                    firstYearSecondSemesterGPA = ((firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                                }

                                                if ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) > 0)
                                                {
                                                    //Get the CGPA of both First year first and second semesters
                                                    firstYearSecondSemesterCGPA = ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0)) / ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)));
                                                }


                                                //Get list of CGPA within the probation range
                                                if (firstYearSecondSemesterCGPA < (decimal)2.0)
                                                {
                                                    Probation probation = new Probation();
                                                    probation.CGPA = firstYearSecondSemesterCGPA;
                                                    probation.Semester = new Semester() { Id = 2 };
                                                    probation.Level = new Level() { Id = firstYearLevelId };
                                                    probationList.Add(probation);
                                                }
                                                var firstYearThirdSemester = previousSessionResults.Where(f => f.MatricNumber == matricNo && f.SemesterId == 3).ToList();

                                                if (firstYearThirdSemester != null && firstYearThirdSemester.Count > 0)
                                                {
                                                    //assign firstYearThirdSemesterResult to a global holder;
                                                    firstYearThirdSemesterResultHolder = firstYearThirdSemester;

                                                    TotalCourseUnits.Add(firstYearThirdSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                                    TotalCoursePoints.Add(firstYearThirdSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0);
                                                    if ((firstYearThirdSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) > 0)
                                                    {
                                                        firstYearThirdSemesterGPA = ((firstYearThirdSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / firstYearThirdSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                                    }

                                                    if (((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearThirdSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit))) > 0)
                                                    {
                                                        //Get the CGPA of both First year first,second and Third semesters
                                                        firstYearThirdSemesterCGPA = ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (firstYearThirdSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0)) / ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearThirdSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)));
                                                    }


                                                    //Get list of CGPA within the probation range
                                                    if (firstYearThirdSemesterCGPA < (decimal)2.0)
                                                    {
                                                        Probation probation = new Probation();
                                                        probation.CGPA = firstYearThirdSemesterGPA;
                                                        probation.Semester = new Semester() { Id = 3 };
                                                        probation.Level = new Level() { Id = firstYearLevelId };
                                                        probationList.Add(probation);
                                                    }

                                                }


                                                var secondYearFirstSemester = currentStudentResult.Where(f => f.MatricNumber == matricNo && f.SemesterId == 1).ToList();
                                                if (secondYearFirstSemester != null && secondYearFirstSemester.Count > 0)
                                                {
                                                    TotalCourseUnits.Add(secondYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                                    TotalCoursePoints.Add((secondYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));

                                                    secondYearFirstSemesterGPA = ((secondYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / secondYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));

                                                    //Get the CGPA of both First year (first,second and Third) and Second year first  semesters
                                                    if (firstYearThirdSemesterResultHolder != null)
                                                    {
                                                        if (((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearThirdSemesterResultHolder.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (secondYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit))) > 0)
                                                        {
                                                            secondYearFirstSemesterCGPA = ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (firstYearThirdSemesterResultHolder.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (secondYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0)) / ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearThirdSemesterResultHolder.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (secondYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)));
                                                        }

                                                    }
                                                    else
                                                    {
                                                        if ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (secondYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) > 0)
                                                        {
                                                            secondYearFirstSemesterCGPA = ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (secondYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0)) / ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (secondYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)));
                                                        }

                                                    }


                                                    //Get list of CGPA within the probation range
                                                    if (secondYearFirstSemesterCGPA < (decimal)2.0)
                                                    {
                                                        Probation probation = new Probation();
                                                        probation.CGPA = secondYearFirstSemesterCGPA;
                                                        probation.Semester = new Semester() { Id = 1 };
                                                        probation.Level = level;
                                                        probationList.Add(probation);
                                                    }
                                                    var secondYearSecondSemester = currentStudentResult.Where(f => f.MatricNumber == matricNo && f.SemesterId == 2).ToList();
                                                    if (secondYearSecondSemester != null && secondYearSecondSemester.Count > 0)
                                                    {
                                                        TotalCourseUnits.Add(secondYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                                        TotalCoursePoints.Add((secondYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                                                        if (secondYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit) > 0)
                                                        {
                                                            secondYearSecondSemesterGPA = ((secondYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / secondYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                                        }


                                                        //Get the CGPA of both First year (first,second and Third) and Second year first and Second semesters
                                                        if (firstYearThirdSemesterResultHolder != null)
                                                        {
                                                            if (((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearThirdSemesterResultHolder.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (secondYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (secondYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit))) > 0)
                                                            {
                                                                secondYearSecondSemesterCGPA = ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (firstYearThirdSemesterResultHolder.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (secondYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (secondYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0)) / ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearThirdSemesterResultHolder.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (secondYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (secondYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)));
                                                            }

                                                        }
                                                        else
                                                        {
                                                            if ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (secondYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (secondYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) > 0)
                                                            {
                                                                secondYearSecondSemesterCGPA = ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (secondYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (secondYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0)) / ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (secondYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (secondYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)));
                                                            }


                                                        }

                                                        //Get list of GPA within the probation range
                                                        if (secondYearSecondSemesterCGPA < (decimal)2.0)
                                                        {
                                                            Probation probation = new Probation();
                                                            probation.CGPA = secondYearSecondSemesterCGPA;
                                                            probation.Semester = new Semester() { Id = 2 };
                                                            probation.Level = level;
                                                            probationList.Add(probation);
                                                        }
                                                    }
                                                    secondYearThirdSemesterGPA = currentSessionSemesterGPA;
                                                    if ((TotalCourseUnits.Sum()) > 0)
                                                    {
                                                        currentSessionSemesterCGPA = Math.Round(TotalCoursePoints.Sum() / TotalCourseUnits.Sum(), 2);
                                                    }

                                                    secondYearThirdSemesterCGPA = currentSessionSemesterCGPA;
                                                    //Get list of GPA within the probation range
                                                    if (secondYearThirdSemesterCGPA < (decimal)2.0)
                                                    {
                                                        Probation probation = new Probation();
                                                        probation.CGPA = secondYearThirdSemesterCGPA;
                                                        probation.Semester = new Semester() { Id = 3 };
                                                        probation.Level = level;
                                                        probationList.Add(probation);
                                                    }
                                                    //currentSessionSemesterCGPA = Math.Round(((firstYearFirstSemesterGPA + firstYearSecondSemesterGPA + firstYearThirdSemesterGPA + secondYearFirstSemesterGPA + secondYearSecondSemesterGPA + secondYearThirdSemesterGPA) / 6), 2);



                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception("Selected Session-Semester is not valid for selected Programme");
                                    }

                                }
                            }
                        }
                        //bool isWithdrawn = isWithdrawal(probationList);
                        PartialRemark partialRemark = isWithdrawalBasedOnProgramme(currentSessionSemesterCGPA, oustandingCourses, probationList, programme.Id,level,session);
                        //string remark = GetSemesterRemarks(currentSessionSemesterCGPA, oustandingCourses, programme.Id, semester.Id, isWithdrawn);
                        string remark = GetSemesterRemarksWithWithdrawal(currentSessionSemesterCGPA, oustandingCourses, programme.Id, partialRemark);

                        //if (isWithdrawn)
                        //{
                        //    withdrawnStudentMatricNo.Add(matricNo);

                        //}
                        if (remark != null && remark.Contains("WITHDRAWN"))
                        {
                            withdrawnStudentMatricNo.Add(matricNo);

                        }
                        var studentCourses = classStudents.Where(c => c.MatricNumber == matricNo && c.SemesterId == semester.Id && (c.CourseModeId == 1 || c.CourseModeId == 2)).ToList();
                        var unitOutStanding=oustandingCourses.Sum(a => a.Unit);
                        for (int i = 0; i < studentCourses.Count; i++)
                        {
                            studentCourses[i].GPA = currentSessionSemesterGPA;
                            studentCourses[i].CGPA = currentSessionSemesterCGPA;
                            studentCourses[i].Remark = remark;
                            studentCourses[i].Identifier = identifier;
                            studentCourses[i].TotalSemesterCourseUnit = TotalSemesterCourseUnit;
                            studentCourses[i].UnitOutstanding = unitOutStanding;
                            studentCourses[i].UnitPassed = UnitPassed;
                            if (studentCourses[i].SpecialCase == "SICK")
                            {
                                studentCourses[i].Grade = "S";
                            }
                            if (studentCourses[i].SpecialCase == "ABSENT")
                            {
                                studentCourses[i].Grade = "ABS";
                            }
                            if (departmentOption != null && departmentOption.Id > 0)
                            {

                                    studentCourses[i].DepartmentName = studentCourses[i].DepartmentName + " (" + departmentOption.Name + ")";
                            }

                            results.Add(studentCourses[i]);
                        }
                    }



                }
                if (withdrawnStudentMatricNo != null && withdrawnStudentMatricNo.Count > 0)
                {
                    DeactivateWithdrawnStudent(withdrawnStudentMatricNo);
                }


            }
            catch (Exception ex)
            {

                throw ex;
            }

            return results.OrderBy(f => f.MatricNumber).ThenBy(f => f.CourseModeId).ToList();
        }
        public async Task<List<Result>> GetAggregateSheetFullTime(SessionSemester sessionSemester, Level level, Programme programme, Department department, CourseMode courseMode, DepartmentOption departmentOption)
        {
            List<Result> results = new List<Result>();

            try
            {


                LevelLogic levelLogic = new LevelLogic();
                ProgrammeLogic programmeLogic = new ProgrammeLogic();
                SessionLogic sessionLogic = new SessionLogic();
                DepartmentLogic departmentLogic = new DepartmentLogic();
                SessionSemesterLogic sessionSemesterLogic = new Business.SessionSemesterLogic();
                SemesterLogic semesterLogic = new SemesterLogic();
                CourseModeLogic courseModeLogic = new CourseModeLogic();
                SessionSemester ss = sessionSemesterLogic.GetBy(sessionSemester.Id);
                programme = programmeLogic.GetModelBy(g => g.Programme_Id == programme.Id);
                department = departmentLogic.GetModelBy(f => f.Department_Id == department.Id);
                Session session = sessionLogic.GetModelBy(f => f.Session_Id == ss.Session.Id);
                level = levelLogic.GetModelBy(g => g.Level_Id == level.Id);
                Semester semester = semesterLogic.GetModelBy(f => f.Semester_Id == ss.Semester.Id);
                List<string> withdrawnStudentMatricNo = new List<string>();
                if (departmentOption?.Id > 0)
                {
                    DepartmentOptionLogic departmentOptionLogic = new DepartmentOptionLogic();
                    departmentOption = departmentOptionLogic.GetModelBy(f => f.Department_Option_Id == departmentOption.Id);
                }
                if (courseMode == null)
                {
                    courseMode = courseModeLogic.GetAll().FirstOrDefault();
                }
                else
                {
                    courseMode = courseModeLogic.GetModelBy(c => c.Course_Mode_Id == courseMode.Id);
                }

                string[] sessionItems = session.Name.Split('/');
                string sessionNameStr = sessionItems[0];
                int sessionNameInt = Convert.ToInt32(sessionNameStr);

                string currentSessionSuffix = sessionNameStr.Substring(2, 2); // 18
                string yearTwoSessionSuffix = Convert.ToString((Convert.ToInt32(currentSessionSuffix) - 1)); //17


                currentSessionSuffix = "/" + currentSessionSuffix + "/";
                yearTwoSessionSuffix = "/" + yearTwoSessionSuffix + "/";

                string previousSessionName = Convert.ToString((sessionNameInt - 1)) + "/" + Convert.ToString((sessionNameInt));
                Session previousSession = sessionLogic.GetModelsBy(s => s.Session_Name == previousSessionName.Trim()).LastOrDefault();
                if (previousSession == null)
                {
                    throw new Exception("Cannot Find Previous Session");
                }

                string semesterCode = GetSemesterCodeBy(ss.Semester.Id);
                string sessionCode = GetSessionCodeBy(ss.Session.Name);
                string identifier = department.Code + level.Name + semesterCode + sessionCode;

                var classStudents = await GetStudents(programme, department, session, level, departmentOption);
                var classStudents2 = classStudents.Where(c => c.CourseModeId == courseMode.Id).ToList();
                List<string> distinctMatricNo = new List<string>();
                if (level.Id == 1 || level.Id == 3)
                {
                    distinctMatricNo = classStudents2.Where(s => s.MatricNumber.Contains(currentSessionSuffix)).OrderBy(a => a.MatricNumber).Select(f => f.MatricNumber).Distinct().ToList();
                }
                else
                {
                    distinctMatricNo = classStudents2.Where(s => s.MatricNumber.Contains(yearTwoSessionSuffix)).OrderBy(a => a.MatricNumber).Select(f => f.MatricNumber).Distinct().ToList();
                }

                var previousSessionResults = await GetAllStudentResultBySession(distinctMatricNo, previousSession);

                foreach (var matricNo in distinctMatricNo)
                {
                    //if(matricNo!= "N/OTM/19/4765")
                    //{
                    //    continue;
                    //}
                    List<Probation> probationList = new List<Probation>();
                    var currentStudentResult = classStudents.Where(f => f.MatricNumber == matricNo && (f.TotalScore != null || f.SpecialCase != null)).ToList();
                    var previousSessionStudentResults = previousSessionResults.Where(f => f.MatricNumber == matricNo && (f.TotalScore != null || f.SpecialCase != null)).ToList();

                    decimal firstYearFirstSemesterGPA = 0M;
                    decimal firstYearSecondSemesterGPA = 0M;
                    decimal secondYearFirstSemesterGPA = 0M;
                    decimal secondYearSecondSemesterGPA = 0M;
                    decimal currentSessionSemesterGPA = 0M;
                    decimal currentSessionSemesterCGPA = 0M;
                    decimal firstYearSecondSemesterCGPA = 0M;
                    decimal secondYearSecondSemesterCGPA = 0M;
                    decimal secondYearFirstSemesterCGPA = 0M;

                    int TotalSemesterCourseUnit = 0;
                    int UnitOutstanding = 0;
                    int UnitPassed = 0;
                    List<decimal> TotalCoursePoints = new List<decimal>();
                    List<int> TotalCourseUnits = new List<int>();
                    List<Course> oustandingCourses = new List<Course>();
                    //Get outsatnding for previous session
                    if (previousSessionStudentResults != null && previousSessionStudentResults.Count > 0)
                    {
                        var carryOvers = previousSessionStudentResults.Where(f => f.MatricNumber == matricNo && f.CourseUnit > 0 && ((f.TotalScore < 40 || f.SpecialCase != null))).ToList();
                        if (carryOvers != null && carryOvers.Count > 0)
                        {
                            foreach (var carryOverCourse in carryOvers)
                            {
                                var passedCarryOver = previousSessionStudentResults.Where(f => f.CourseId == carryOverCourse.CourseId && f.MatricNumber == matricNo && f.TotalScore >= 40).FirstOrDefault();
                                var passedCarryOverInCurrentSession = new Result();
                                if (currentStudentResult != null && currentStudentResult.Count > 0)
                                {
                                    passedCarryOverInCurrentSession = currentStudentResult.Where(f => f.CourseId == carryOverCourse.CourseId && f.MatricNumber == matricNo && f.TotalScore >= 40).FirstOrDefault();
                                }

                                if (passedCarryOver == null && passedCarryOverInCurrentSession == null)
                                {
                                    oustandingCourses.Add(new Course() { Code = carryOverCourse.CourseCode, Id = carryOverCourse.CourseId, Unit = carryOverCourse.CourseUnit });
                                }
                            }

                        }
                    }
                    //Get outstanding for current session
                    if (currentStudentResult != null && currentStudentResult.Count > 0)
                    {
                        var carryOvers = new List<Result>();
                        //Only show the outstanding of first semester of the selected level if first year and first semester is selected
                        if ((level.Id == 1 || level.Id == 3) && semester.Id == 1)
                        {
                            carryOvers = currentStudentResult.Where(f => f.MatricNumber == matricNo && f.CourseUnit > 0 && ((f.TotalScore < 40 || f.SpecialCase != null)) && f.LevelId == level.Id && f.SemesterId == semester.Id).ToList();
                        }
                        else
                        {
                            carryOvers = currentStudentResult.Where(f => f.MatricNumber == matricNo && f.CourseUnit > 0 && ((f.TotalScore < 40 || f.SpecialCase != null))).ToList();
                        }

                        if (carryOvers != null && carryOvers.Count > 0)
                        {
                            foreach (var carryOverCourse in carryOvers)
                            {
                                var passedCarryOver = currentStudentResult.Where(f => f.CourseId == carryOverCourse.CourseId && f.MatricNumber == matricNo && f.TotalScore >= 40).FirstOrDefault();
                                if (passedCarryOver == null)
                                {
                                    if (!CheckForExistingCarryOver(oustandingCourses, carryOverCourse.CourseId))
                                    {
                                        oustandingCourses.Add(new Course() { Code = carryOverCourse.CourseCode, Id = carryOverCourse.CourseId, Unit = carryOverCourse.CourseUnit });
                                    }
                                    //oustandingCourses.Add(new Course() { Code = carryOverCourse.CourseCode, Id = carryOverCourse.CourseId, Unit = carryOverCourse.CourseUnit });
                                }
                            }

                        }
                    }
                    var currentSessionSemester = currentStudentResult.Where(f => f.MatricNumber == matricNo && f.SemesterId == semester.Id).ToList();
                    if (currentSessionSemester != null && currentSessionSemester.Count > 0)
                    {

                        TotalCourseUnits.Add(currentSessionSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                        TotalCoursePoints.Add((currentSessionSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                        if ((currentSessionSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) > 0)
                        {
                            currentSessionSemesterGPA = ((currentSessionSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / currentSessionSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                        }

                        TotalSemesterCourseUnit = currentSessionSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(s => s.CourseUnit);
                        UnitOutstanding = currentSessionSemester.Where(c => (c.TotalScore < 40 || c.SpecialCase != null) || (c.TotalScore == null && c.SpecialCase == null)).Sum(c => c.CourseUnit);
                        UnitPassed = currentSessionSemester.Where(c => c.TotalScore >= 40).Sum(c => c.CourseUnit);

                        if (level.Id == 1 || level.Id == 3)
                        {
                            if (semester.Id == 1)
                            {
                                firstYearFirstSemesterGPA = currentSessionSemesterGPA;
                                //Get list of GPA within the probation range

                                if (firstYearFirstSemesterGPA < (decimal)2.0)
                                {
                                    Probation probation = new Probation();
                                    probation.CGPA = firstYearFirstSemesterGPA;
                                    probation.Semester = new Semester() { Id = 1 };
                                    probation.Level = level;
                                    probationList.Add(probation);
                                }

                                currentSessionSemesterCGPA = Math.Round(firstYearFirstSemesterGPA, 2);
                            }
                            else if (semester.Id == 2)
                            {
                                var firstYearFirstSemester = currentStudentResult.Where(f => f.MatricNumber == matricNo && f.SemesterId == 1).ToList();
                                if (firstYearFirstSemester != null && firstYearFirstSemester.Count > 0)
                                {
                                    TotalCourseUnits.Add(firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                    TotalCoursePoints.Add((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                                    if ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) > 0)
                                    {
                                        firstYearFirstSemesterGPA = ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                    }

                                }
                                //Get list of CGPA within the probation range
                                if (firstYearFirstSemesterGPA < (decimal)2.0)
                                {
                                    Probation probation = new Probation();
                                    probation.CGPA = firstYearFirstSemesterGPA;
                                    probation.Semester = new Semester() { Id = 1 };
                                    probation.Level = level;
                                    probationList.Add(probation);
                                }
                                firstYearSecondSemesterGPA = currentSessionSemesterGPA;
                                if (TotalCourseUnits.Sum() > 0)
                                    currentSessionSemesterCGPA = Math.Round(TotalCoursePoints.Sum() / TotalCourseUnits.Sum(), 2);
                                //else
                                //    currentSessionSemesterCGPA = 0;
                                //firstYearSecondSemesterCGPA = currentSessionSemesterCGPA;
                                //Get list of CGPA within the probation range
                                if (firstYearSecondSemesterCGPA < (decimal)2.0)
                                {
                                    Probation probation = new Probation();
                                    probation.CGPA = firstYearSecondSemesterCGPA;
                                    probation.Semester = new Semester() { Id = 2 };
                                    probation.Level = level;
                                    probationList.Add(probation);
                                }



                            }

                        }
                        else
                        {
                            var firstYearLevelId = level.Id - 1;
                            if (previousSessionResults != null && previousSessionResults.Count > 0)
                            {
                                if (semester.Id == 1)
                                {

                                    var firstYearFirstSemester = previousSessionResults.Where(f => f.MatricNumber == matricNo && f.SemesterId == 1).ToList();
                                    if (firstYearFirstSemester != null && firstYearFirstSemester.Count > 0)
                                    {
                                        TotalCourseUnits.Add(firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                        TotalCoursePoints.Add((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                                        if ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) > 0)
                                        {
                                            firstYearFirstSemesterGPA = ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                        }

                                        //Get list of CGPA within the probation range
                                        if (firstYearFirstSemesterGPA < (decimal)2.0)
                                        {
                                            Probation probation = new Probation();
                                            probation.CGPA = firstYearFirstSemesterGPA;
                                            probation.Semester = new Semester() { Id = 1 };
                                            probation.Level = new Level() { Id = firstYearLevelId };
                                            probationList.Add(probation);
                                        }

                                        var firstYearSecondSemester = previousSessionResults.Where(f => f.MatricNumber == matricNo && f.SemesterId == 2).ToList();

                                        if (firstYearSecondSemester != null && firstYearSecondSemester.Count > 0)
                                        {
                                            TotalCourseUnits.Add(firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                            TotalCoursePoints.Add((firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                                            if ((firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) > 0)
                                            {
                                                firstYearSecondSemesterGPA = ((firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                            }

                                            if (((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit))) > 0)
                                            {
                                                //Get the CGPA of both First year first and second semesters
                                                firstYearSecondSemesterCGPA = ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0)) / ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)));
                                            }

                                            //Get list of GPA within the probation range
                                            if (firstYearSecondSemesterCGPA < (decimal)2.0)
                                            {
                                                Probation probation = new Probation();
                                                probation.CGPA = firstYearSecondSemesterCGPA;
                                                probation.Semester = new Semester() { Id = 2 };
                                                probation.Level = new Level() { Id = firstYearLevelId };
                                                probationList.Add(probation);
                                            }

                                            secondYearFirstSemesterGPA = currentSessionSemesterGPA;
                                            if ((TotalCourseUnits.Sum()) > 0)
                                            {
                                                currentSessionSemesterCGPA = Math.Round(TotalCoursePoints.Sum() / TotalCourseUnits.Sum(), 2);
                                            }
                                            //Get list of GPA within the probation range
                                            if (currentSessionSemesterCGPA < (decimal)2.0)
                                            {
                                                Probation probation = new Probation();
                                                probation.CGPA = currentSessionSemesterCGPA;
                                                probation.Semester = new Semester() { Id = 1 };
                                                probation.Level = new Level() { Id = level.Id };
                                                probationList.Add(probation);
                                            }


                                        }

                                    }

                                }
                                else if (semester.Id == 2)
                                {
                                    var firstYearFirstSemester = previousSessionResults.Where(f => f.MatricNumber == matricNo && f.SemesterId == 1).ToList();
                                    if (firstYearFirstSemester != null && firstYearFirstSemester.Count > 0)
                                    {
                                        TotalCourseUnits.Add(firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                        TotalCoursePoints.Add((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                                        if ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) > 0)
                                        {
                                            firstYearFirstSemesterGPA = ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                        }



                                        //Get list of CGPA within the probation range
                                        if (firstYearFirstSemesterGPA < (decimal)2.0)
                                        {
                                            Probation probation = new Probation();
                                            probation.CGPA = firstYearFirstSemesterGPA;
                                            probation.Semester = new Semester() { Id = 1 };
                                            probation.Level = new Level() { Id = firstYearLevelId };
                                            probationList.Add(probation);
                                        }
                                        var firstYearSecondSemester = previousSessionResults.Where(f => f.MatricNumber == matricNo && f.SemesterId == 2).ToList();

                                        if (firstYearSecondSemester != null && firstYearSecondSemester.Count > 0)
                                        {

                                            TotalCourseUnits.Add(firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                            TotalCoursePoints.Add((firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                                            if ((firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) > 0)
                                            {
                                                firstYearSecondSemesterGPA = ((firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                            }

                                            if (((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit))) > 0)
                                            {
                                                //Get the CGPA of both First year first and second semesters
                                                firstYearSecondSemesterCGPA = ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0)) / ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)));
                                            }


                                            //Get list of CGPA within the probation range
                                            if (firstYearSecondSemesterCGPA < (decimal)2.0)
                                            {
                                                Probation probation = new Probation();
                                                probation.CGPA = firstYearSecondSemesterCGPA;
                                                probation.Semester = new Semester() { Id = 2 };
                                                probation.Level = new Level() { Id = firstYearLevelId };
                                                probationList.Add(probation);
                                            }



                                            var secondYearFirstSemester = currentStudentResult.Where(f => f.MatricNumber == matricNo && f.SemesterId == 1).ToList();
                                            if (secondYearFirstSemester != null && secondYearFirstSemester.Count > 0)
                                            {
                                                TotalCourseUnits.Add(secondYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                                TotalCoursePoints.Add((secondYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                                                if ((secondYearFirstSemester.Sum(x => x.CourseUnit)) > 0)
                                                {
                                                    secondYearFirstSemesterGPA = ((secondYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / secondYearFirstSemester.Sum(x => x.CourseUnit));
                                                }


                                                //Get the CGPA of both First year first, and second semester

                                                if (((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (secondYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit))) > 0)
                                                {
                                                    secondYearFirstSemesterCGPA = ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (secondYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0)) / ((firstYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (secondYearFirstSemester.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)));
                                                }
                                                //Get list of CGPA within the probation range
                                                if (secondYearFirstSemesterCGPA < (decimal)2.0)
                                                {
                                                    Probation probation = new Probation();
                                                    probation.CGPA = secondYearFirstSemesterCGPA;
                                                    probation.Semester = new Semester() { Id = 1 };
                                                    probation.Level = level;
                                                    probationList.Add(probation);
                                                }
                                                secondYearSecondSemesterGPA = currentSessionSemesterGPA;

                                                //currentSessionSemesterCGPA = Math.Round(((firstYearFirstSemesterGPA + firstYearSecondSemesterGPA + secondYearFirstSemesterGPA + secondYearSecondSemesterGPA) / 4), 2);
                                                if ((TotalCourseUnits.Sum() > 0))
                                                {
                                                    currentSessionSemesterCGPA = Math.Round(TotalCoursePoints.Sum() / TotalCourseUnits.Sum(), 2);
                                                }

                                                if ((programme.Id == 2 || programme.Id == 5) && (session.Id >= 9 && (level.Id == 1 || level.Id == 3)))
                                                {
                                                    //currentSessionSemesterCGPA = Math.Round(((firstYearFirstSemesterGPA + firstYearSecondSemesterGPA + firstYearThirdSemesterGPA + secondYearFirstSemesterGPA + secondYearSecondSemesterGPA) / 5), 2);

                                                    if ((TotalCourseUnits.Sum() > 0))
                                                    {
                                                        currentSessionSemesterCGPA = Math.Round(TotalCoursePoints.Sum() / TotalCourseUnits.Sum(), 2);
                                                    }

                                                }
                                                secondYearSecondSemesterCGPA = currentSessionSemesterCGPA;
                                                //Get list of CGPA within the probation range
                                                if (secondYearSecondSemesterCGPA < (decimal)2.0)
                                                {
                                                    Probation probation = new Probation();
                                                    probation.CGPA = secondYearSecondSemesterCGPA;
                                                    probation.Semester = new Semester() { Id = 2 };
                                                    probation.Level = level;
                                                    probationList.Add(probation);
                                                }

                                            }

                                        }
                                    }
                                }
                                else
                                {
                                    
                                        throw new Exception("Selected Session-Semester is not valid for selected Programme");
                                }
                            }
                        }
                        //bool isWithdrawn = isWithdrawal(probationList);
                        PartialRemark partialRemark = isWithdrawalBasedOnProgramme(currentSessionSemesterCGPA, oustandingCourses, probationList, programme.Id, level,session);
                        //string remark = GetSemesterRemarks(currentSessionSemesterCGPA, oustandingCourses, programme.Id, semester.Id, isWithdrawn);
                        string remark = GetSemesterRemarksWithWithdrawal(currentSessionSemesterCGPA, oustandingCourses, programme.Id, partialRemark);

                        if (remark != null && remark.Contains("WITHDRAWN"))
                        {
                            withdrawnStudentMatricNo.Add(matricNo);

                        }
                        var studentCourses = classStudents.Where(c => c.MatricNumber == matricNo && c.SemesterId == semester.Id && (c.CourseModeId == 1 || c.CourseModeId == 2)).ToList();
                        var unitOutStanding = oustandingCourses.Sum(a => a.Unit);
                        for (int i = 0; i < studentCourses.Count; i++)
                        {
                            studentCourses[i].GPA = currentSessionSemesterGPA;
                            studentCourses[i].CGPA = currentSessionSemesterCGPA;
                            studentCourses[i].Remark = remark;
                            studentCourses[i].Identifier = identifier;
                            studentCourses[i].TotalSemesterCourseUnit = TotalSemesterCourseUnit;
                            studentCourses[i].UnitOutstanding = unitOutStanding;
                            studentCourses[i].UnitPassed = UnitPassed;
                            if (studentCourses[i].SpecialCase == "SICK")
                            {
                                studentCourses[i].Grade = "S";
                            }
                            if (studentCourses[i].SpecialCase == "ABSENT")
                            {
                                studentCourses[i].Grade = "ABS";
                            }
                            if (departmentOption != null && departmentOption.Id > 0)
                            {

                                studentCourses[i].DepartmentName = studentCourses[i].DepartmentName + " (" + departmentOption.Name + ")";
                            }

                            results.Add(studentCourses[i]);
                        }
                    }



                }
                if (withdrawnStudentMatricNo != null && withdrawnStudentMatricNo.Count > 0)
                {
                    DeactivateWithdrawnStudent(withdrawnStudentMatricNo);
                }


            }
            catch (Exception ex)
            {

                throw ex;
            }

            return results.OrderBy(f => f.MatricNumber).ThenBy(f => f.CourseModeId).ToList();
        }

        public async Task<Result> GetNotificationSheet(Student student, SessionSemester sessionSemester, Level level, Programme programme, Department department)
        {
            Result result = null;
            try
            {
                LevelLogic levelLogic = new LevelLogic();
                StudentLogic studentLogic = new StudentLogic();
                ProgrammeLogic programmeLogic = new ProgrammeLogic();
                SessionLogic sessionLogic = new SessionLogic();
                DepartmentLogic departmentLogic = new DepartmentLogic();
                SessionSemesterLogic sessionSemesterLogic = new Business.SessionSemesterLogic();
                SemesterLogic semesterLogic = new SemesterLogic();
                SessionSemester ss = sessionSemesterLogic.GetBy(sessionSemester.Id);
                programme = programmeLogic.GetModelBy(g => g.Programme_Id == programme.Id);
                department = departmentLogic.GetModelBy(f => f.Department_Id == department.Id);
                Session session = sessionLogic.GetModelBy(f => f.Session_Id == ss.Session.Id);
                level = levelLogic.GetModelBy(g => g.Level_Id == level.Id);
                Semester semester = semesterLogic.GetModelBy(f => f.Semester_Id == ss.Semester.Id);
                student = studentLogic.GetBy(student.Id);
                List<decimal> TotalCoursePoints = new List<decimal>();
                List<int> TotalCourseUnits = new List<int>();
                var allResults = await GetStudentResults(student.Id, level.Id, session.Id);
                if (allResults != null && allResults.Count > 0)
                {
                    decimal firstYearFirstSemesterGPA = 0M;
                    decimal firstYearSecondSemesterGPA = 0M;
                    decimal firstYearThirdSemesterGPA = 0M;
                    decimal secondYearFirstSemesterGPA = 0M;
                    decimal secondYearSecondSemesterGPA = 0M;
                    decimal secondYearThirdSemesterGPA = 0M;
                    decimal CGPA = 0M;


                    var carryOvers = allResults.Where(f => f.CourseUnit > 0 && (f.TotalScore < 40 || f.SpecialCase != null) || (f.TotalScore == null && f.SpecialCase == null)).ToList();
                    if (carryOvers != null && carryOvers.Count > 0)
                    {
                        foreach (var carryOverCourse in carryOvers)
                        {
                            var passedCarryOver = allResults.Where(f => f.CourseId == carryOverCourse.CourseId && f.TotalScore >= 40).FirstOrDefault();
                            if (passedCarryOver == null)
                            {
                                // throw new Exception("Student has Outstanding Courses");
                                return null;
                            }
                        }
                    }

                    int firstYearLevel = level.Id - 1;
                    var firstYearFirstSemesterResult = allResults.Where(f => f.SemesterId == 1 && f.LevelId == firstYearLevel).ToList();
                    if (firstYearFirstSemesterResult != null && firstYearFirstSemesterResult.Count > 0)
                    {
                        TotalCourseUnits.Add(firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                        TotalCoursePoints.Add((firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                        firstYearFirstSemesterGPA = ((firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));

                        var firstYearSecondSemesterResult = allResults.Where(f => f.SemesterId == 2 && f.LevelId == firstYearLevel).ToList();
                        if (firstYearSecondSemesterResult?.Count>0)
                        {
                            TotalCourseUnits.Add(firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                            TotalCoursePoints.Add((firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));

                            firstYearSecondSemesterGPA = ((firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                            if(session.Id>=9 && (programme.Id==2 || programme.Id == 4 || programme.Id == 5))
                            {
                                var firstYearThirdSemesterResult = allResults.Where(f => f.SemesterId == 3 && f.LevelId == firstYearLevel).ToList();
                                if (firstYearThirdSemesterResult?.Count>0)
                                {
                                    TotalCourseUnits.Add(firstYearThirdSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                    TotalCoursePoints.Add((firstYearThirdSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));

                                    firstYearThirdSemesterGPA = ((firstYearThirdSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / firstYearThirdSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                }
                                }
                            var secondYearFirstSemesterResult = allResults.Where(f => f.SemesterId == 1 && f.LevelId == level.Id).ToList();
                            if (secondYearFirstSemesterResult != null && secondYearFirstSemesterResult.Count > 0)
                            {
                                TotalCourseUnits.Add(secondYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                TotalCoursePoints.Add((secondYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));

                                secondYearFirstSemesterGPA = ((secondYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / secondYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                var secondYearSecondSemesterResult = allResults.Where(f => f.SemesterId == 2 && f.LevelId == level.Id).ToList();
                                if (secondYearSecondSemesterResult?.Count>0)
                                {
                                    TotalCourseUnits.Add(secondYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                    TotalCoursePoints.Add((secondYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));

                                    secondYearSecondSemesterGPA = ((secondYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / secondYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                    //CGPA = Math.Round((firstYearFirstSemesterGPA + firstYearSecondSemesterGPA + secondYearFirstSemesterGPA + secondYearSecondSemesterGPA) / 4, 2);

                                    if (session.Id >= 9 && (programme.Id == 2 || programme.Id == 4 || programme.Id == 5))
                                    {
                                        var secondearThirdSemesterResult = allResults.Where(f => f.SemesterId == 3 && f.LevelId == level.Id).ToList();
                                        if (secondearThirdSemesterResult?.Count>0)
                                        {
                                            TotalCourseUnits.Add(secondearThirdSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                            TotalCoursePoints.Add((secondearThirdSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));

                                            secondYearThirdSemesterGPA = ((secondearThirdSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / secondearThirdSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                        }
                                    }

                                }

                            }

                        }

                    }
                    CGPA = Math.Round(TotalCoursePoints.Sum() / TotalCourseUnits.Sum(), 2);

                    result = allResults.FirstOrDefault();
                    result.CGPA = CGPA;
                    result.Remark = GetGraduationStatus(CGPA);


                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
            return result;
        }
        public async Task<List<Result>> GetStatementSheet(SessionSemester sessionSemester, Level level, Programme programme, Department department, Student student)
        {
            List<Result> results = new List<Result>();
            List<Probation> probationList = new List<Probation>();
            List<string> withdrawnStudentMatricNo = new List<string>();
            Result result = null;
            bool isExtraYear = false;
            try
            {
                SessionSemesterLogic sessionSemesterLogic = new Business.SessionSemesterLogic();
                DepartmentLogic departmentLogic = new DepartmentLogic();
                LevelLogic levelLogic = new LevelLogic();
                StudentLogic studentLogic = new StudentLogic();
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
                department = departmentLogic.GetModelBy(f => f.Department_Id == department.Id);
                level = levelLogic.GetModelBy(g => g.Level_Id == level.Id);
                string semesterCode = GetSemesterCodeBy(ss.Semester.Id);
                string sessionCode = GetSessionCodeBy(ss.Session.Name);
                identifier = department.Code + level.Name + semesterCode + sessionCode;
                student = studentLogic.GetBy(student.Id);
                var allResults = await GetStudentResults(student.Id, level.Id,sessions.Id);
                allResults = allResults.Where(d => !(d.TotalScore == null && d.SpecialCase == null)).ToList();
                var sessionCount = allResults.GroupBy(f => f.SessionId).ToList();
                List<int> studentExtraYearSessionId = new List<int>();
                
                isExtraYear = allResults.GroupBy(f => f.SessionId).ToList().Count > 2 ? true : false;
                if (isExtraYear)
                {
                    StudentExtraYearLogic studentExtraYearLogic = new StudentExtraYearLogic();
                    var sessionIds=studentExtraYearLogic.GetModelsBy(f => f.Person_Id == student.Id).Select(f=>f.Session.Id).ToList();
                    studentExtraYearSessionId.AddRange(sessionIds);
                }
                if (allResults != null && allResults.Count > 0)
                {
                    decimal firstYearFirstSemesterGPA = 0M;
                    decimal firstYearSecondSemesterGPA = 0M;
                    decimal secondYearFirstSemesterGPA = 0M;
                    decimal secondYearSecondSemesterGPA = 0M;
                    decimal firstYearThirdSemesterGPA = 0M;
                    decimal secondYearThirdSemesterGPA = 0M;
                    decimal firstYearSecondSemesterCGPA = 0M;
                    decimal secondYearSecondSemesterCGPA = 0M;
                    decimal firstYearThirdSemesterCGPA = 0M;
                    decimal secondYearThirdSemesterCGPA = 0M;
                    decimal secondYearFirstSemesterCGPA = 0M;
                    List<Result> firstYearThirdSemesterResultHolder = null;
                    List<Result> secondYearThirdSemesterResultHolder = null;


                    decimal CGPA = 0M;
                    List<Course> oustandingCourses = new List<Course>();
                    List<decimal> TotalCoursePoints = new List<decimal>();
                    List<int> TotalCourseUnits = new List<int>();
                    var carryOvers = allResults.Where(f => f.CourseUnit > 0 && !(f.TotalScore == null && f.SpecialCase == null) && ((f.TotalScore < 40 || f.SpecialCase != null) || (f.TotalScore == null && f.SpecialCase == null))).ToList();
                    if (carryOvers != null && carryOvers.Count > 0)
                    {
                        foreach (var carryOverCourse in carryOvers)
                        {
                            var passedCarryOver = allResults.Where(f => f.CourseId == carryOverCourse.CourseId && f.TotalScore >= 40).FirstOrDefault();
                            if (passedCarryOver == null)
                            {
                                if (!CheckForExistingCarryOver(oustandingCourses, carryOverCourse.CourseId))
                                {
                                    oustandingCourses.Add(new Course() { Code = carryOverCourse.CourseCode, Id = carryOverCourse.CourseId, Unit = carryOverCourse.CourseUnit });
                                }

                            }
                        }
                    }
                    int firstYearLevel = level.Id - 1;
                    if (level.Id == 1 || level.Id == 3)
                    {
                        if (ss.Semester.Id == 1)
                        {
                            var firstYearFirstSemesterResult = allResults.Where(f => f.SemesterId == 1 && f.LevelId == level.Id && f.SpecialCase != "SICK" && f.SpecialCase != "ABSENT").ToList();
                            if (firstYearFirstSemesterResult != null && firstYearFirstSemesterResult.Count > 0)
                            {
                                TotalCourseUnits.Add(firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                TotalCoursePoints.Add((firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));

                                firstYearFirstSemesterGPA = ((firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));

                                if (firstYearFirstSemesterGPA < (decimal)2.0)
                                {
                                    Probation probation = new Probation();
                                    probation.CGPA = firstYearFirstSemesterGPA;
                                    probation.Semester = new Semester() { Id = 1 };
                                    probation.Level = level;
                                    probationList.Add(probation);
                                }

                                CGPA = Math.Round(firstYearFirstSemesterGPA, 2);

                            }
                        }
                        else if (ss.Semester.Id == 2)
                        {


                            var firstYearFirstSemesterResult = allResults.Where(f => f.SemesterId == 1 && f.LevelId == level.Id && f.SpecialCase != "SICK" && f.SpecialCase != "ABSENT").ToList();
                            if (firstYearFirstSemesterResult != null && firstYearFirstSemesterResult.Count > 0)
                            {
                                TotalCourseUnits.Add(firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                TotalCoursePoints.Add((firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                                
                                firstYearFirstSemesterGPA = ((firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));

                                if (firstYearFirstSemesterGPA < (decimal)2.0)
                                {
                                    Probation probation = new Probation();
                                    probation.CGPA = firstYearFirstSemesterGPA;
                                    probation.Semester = new Semester() { Id = 1 };
                                    probation.Level = level;
                                    probationList.Add(probation);
                                }
                            }
                            var firstYearSecondSemesterResult = allResults.Where(f => f.SemesterId == 2 && f.LevelId == level.Id && f.SpecialCase != "SICK" && f.SpecialCase != "ABSENT").ToList();
                            if (firstYearSecondSemesterResult?.Count > 0)
                            {
                                TotalCourseUnits.Add(firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                TotalCoursePoints.Add((firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                               
                                firstYearSecondSemesterGPA = ((firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                               
                                //Get the CGPA of both First year first and second semesters
                                firstYearSecondSemesterCGPA = ((firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0)) / ((firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)));
                                //Get list of CGPA within the probation range
                                if (firstYearSecondSemesterCGPA < (decimal)2.0)
                                {
                                    Probation probation = new Probation();
                                    probation.CGPA = firstYearSecondSemesterCGPA;
                                    probation.Semester = new Semester() { Id = 2 };
                                    probation.Level = level;
                                    probationList.Add(probation);
                                }
                            }
                            CGPA = Math.Round(TotalCoursePoints.Sum() / TotalCourseUnits.Sum(), 2);
                            //CGPA = Math.Round((firstYearFirstSemesterGPA + firstYearSecondSemesterGPA) / 2, 2);

                        }
                        else if (ss.Semester.Id == 3)
                        {
                            if ((programme.Id == 2 || programme.Id == 5) && (ss.Session.Id >= 9 && (level.Id == 1 || level.Id == 3)))
                            {
                                var firstYearFirstSemesterResult = allResults.Where(f => f.SemesterId == 1 && f.LevelId == level.Id && f.SpecialCase != "SICK" && f.SpecialCase != "ABSENT").ToList();
                                if (firstYearFirstSemesterResult?.Count > 0)
                                {
                                    TotalCourseUnits.Add(firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                    TotalCoursePoints.Add((firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                                    
                                    firstYearFirstSemesterGPA = ((firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                    //Get list of CGPA within the probation range
                                    if (firstYearFirstSemesterGPA < (decimal)2.0)
                                    {
                                        Probation probation = new Probation();
                                        probation.CGPA = firstYearFirstSemesterGPA;
                                        probation.Semester = new Semester() { Id = 1 };
                                        probation.Level = level;
                                        probationList.Add(probation);
                                    }
                                }
                                var firstYearSecondSemesterResult = allResults.Where(f => f.SemesterId == 2 && f.LevelId == level.Id && f.SpecialCase != "SICK" && f.SpecialCase != "ABSENT").ToList();
                                if (firstYearSecondSemesterResult?.Count > 0)
                                {
                                    TotalCourseUnits.Add(firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                    TotalCoursePoints.Add((firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));

                                    firstYearSecondSemesterGPA = ((firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                    //Get the CGPA of both First year first and second semesters
                                    firstYearSecondSemesterCGPA = ((firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0)) / ((firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)));

                                    //Get list of CGPA within the probation range
                                    if (firstYearSecondSemesterCGPA < (decimal)2.0)
                                    {
                                        Probation probation = new Probation();
                                        probation.CGPA = firstYearSecondSemesterCGPA;
                                        probation.Semester = new Semester() { Id = 2 };
                                        probation.Level = level;
                                        probationList.Add(probation);
                                    }
                                }
                                var firstYearThirdSemesterResult = allResults.Where(f => f.SemesterId == 3 && f.LevelId == level.Id && f.SpecialCase != "SICK" && f.SpecialCase != "ABSENT").ToList();
                                if (firstYearThirdSemesterResult?.Count > 0)
                                {
                                    TotalCourseUnits.Add(firstYearThirdSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                    TotalCoursePoints.Add((firstYearThirdSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));

                                    firstYearThirdSemesterGPA = ((firstYearThirdSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / firstYearThirdSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                    CGPA = Math.Round(TotalCoursePoints.Sum() / TotalCourseUnits.Sum(), 2);
                                    firstYearThirdSemesterCGPA = CGPA;
                                    //Get list of GPA within the probation range
                                    if (firstYearThirdSemesterCGPA < (decimal)2.0)
                                    {
                                        Probation probation = new Probation();
                                        probation.CGPA = firstYearThirdSemesterCGPA;
                                        probation.Semester = new Semester() { Id = 3 };
                                        probation.Level = level;
                                        probationList.Add(probation);
                                    }

                                }
                            }
                            //CGPA = Math.Round((firstYearFirstSemesterGPA + firstYearSecondSemesterGPA + firstYearThirdSemesterGPA) / 3, 2);


                        }
                        else
                        {
                            throw new Exception("Selected Session-Semester is not valid for selected Programme");
                        }


                    }
                    else
                    {
                        var firstYearFirstSemesterResult = allResults.Where(f => f.SemesterId == 1 && f.LevelId == firstYearLevel && f.SpecialCase != "SICK" && f.SpecialCase != "ABSENT").ToList();
                        if (firstYearFirstSemesterResult != null && firstYearFirstSemesterResult.Count > 0)
                        {
                            TotalCourseUnits.Add(firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                            TotalCoursePoints.Add((firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                            
                              firstYearFirstSemesterGPA = ((firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));

                            //Get list of CGPA within the probation range
                            if (firstYearFirstSemesterGPA < (decimal)2.0)
                            {
                                Probation probation = new Probation();
                                probation.CGPA = firstYearFirstSemesterGPA;
                                probation.Semester = new Semester() { Id = 1 };
                                probation.Level = new Level() { Id = firstYearLevel };
                                probationList.Add(probation);
                            }
                        }
                        var firstYearSecondSemesterResult = allResults.Where(f => f.SemesterId == 2 && f.LevelId == firstYearLevel && f.SpecialCase != "SICK" && f.SpecialCase != "ABSENT").ToList();
                        if (firstYearSecondSemesterResult?.Count > 0)
                        {
                            TotalCourseUnits.Add(firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                            TotalCoursePoints.Add((firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                           
                                firstYearSecondSemesterGPA = ((firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));

                            //Get the CGPA of both First year first and second semesters
                           
                            firstYearSecondSemesterCGPA = ((firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0)) / ((firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)));

                            //Get list of CGPA within the probation range
                            if (firstYearSecondSemesterCGPA < (decimal)2.0)
                            {
                                Probation probation = new Probation();
                                probation.CGPA = firstYearSecondSemesterCGPA;
                                probation.Semester = new Semester() { Id = 2 };
                                probation.Level = new Level() { Id = firstYearLevel };
                                probationList.Add(probation);
                            }
                        }
                        if ((programme.Id == 2 || programme.Id == 5) && (ss.Session.Id >= 9 && (level.Id == 2 || level.Id == 4)))
                        {
                            var firstYearThirdSemesterResult = allResults.Where(f => f.SemesterId == 3 && f.LevelId == firstYearLevel && f.SpecialCase != "SICK" && f.SpecialCase != "ABSENT").ToList();
                            if (firstYearThirdSemesterResult != null && firstYearThirdSemesterResult.Count > 0)
                            {
                                //assign firstYearThirdSemesterResult to a global holder;
                                firstYearThirdSemesterResultHolder = firstYearThirdSemesterResult;

                                TotalCourseUnits.Add(firstYearThirdSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                TotalCoursePoints.Add((firstYearThirdSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                                
                                firstYearThirdSemesterGPA = ((firstYearThirdSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / firstYearThirdSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                
                                    //Get the CGPA of both First year first,second and third semesters
                                    firstYearThirdSemesterCGPA = ((firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (firstYearThirdSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0)) / ((firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearThirdSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)));
                                if (firstYearThirdSemesterCGPA < (decimal)2.0)
                                {
                                    Probation probation = new Probation();
                                    probation.CGPA = firstYearThirdSemesterCGPA;
                                    probation.Semester = new Semester() { Id = 3 };
                                    probation.Level = new Level() { Id = firstYearLevel };
                                    probationList.Add(probation);
                                }
                            }
                        }
                        var secondYearFirstSemesterResult = allResults.Where(f => f.SemesterId == 1 && f.LevelId == level.Id && f.SpecialCase != "SICK" && f.SpecialCase != "ABSENT").ToList();
                        if (ss.Semester.Id == 1)
                        {
                            if (secondYearFirstSemesterResult != null && secondYearFirstSemesterResult.Count > 0)
                            {
                                TotalCourseUnits.Add(secondYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                TotalCoursePoints.Add((secondYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                                
                                secondYearFirstSemesterGPA = ((secondYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / secondYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));





                                //var secondYearSecondSemesterResult = allResults.Where(f => f.SemesterId == 2 && f.LevelId == level.Id).ToList();

                                var secondYearSecondSemesterResult = new List<Result>();
                                var secondYearThirdSemesterResult = new List<Result>();
                                //get all result for extra year student
                                if (isExtraYear)
                                {
                                    secondYearSecondSemesterResult = allResults.Where(f => f.SemesterId == 2 && f.LevelId == level.Id && f.SpecialCase != "SICK" && f.SpecialCase != "ABSENT").ToList();
                                    if (secondYearSecondSemesterResult?.Count > 0)
                                    {
                                        TotalCourseUnits.Add(secondYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                        TotalCoursePoints.Add((secondYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                                    }

                                    if ((programme.Id == 2 || programme.Id == 5) && (ss.Session.Id >= 9 && (level.Id == 2 || level.Id == 4)))
                                    {
                                        secondYearThirdSemesterResult = allResults.Where(f => f.SemesterId == 3 && f.LevelId == level.Id && f.SpecialCase != "SICK" && f.SpecialCase != "ABSENT").ToList();
                                        if (secondYearThirdSemesterResult != null && secondYearThirdSemesterResult.Count > 0)
                                        {
                                            //assign firstYearThirdSemesterResult to a global holder;
                                            secondYearThirdSemesterResultHolder = secondYearThirdSemesterResult;

                                            TotalCourseUnits.Add(secondYearThirdSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                            TotalCoursePoints.Add((secondYearThirdSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));
                                            
                                        }
                                    }

                                }

                                //CGPA = Math.Round((firstYearFirstSemesterGPA + firstYearSecondSemesterGPA + secondYearFirstSemesterGPA) / 3, 2);
                                CGPA = Math.Round(TotalCoursePoints.Sum() / TotalCourseUnits.Sum(), 2);
                                if (programme.Id == 2 || programme.Id == 5 && (ss.Session.Id >= 9 && (level.Id == 1 || level.Id == 3)))
                                {
                                    CGPA = Math.Round(TotalCoursePoints.Sum() / TotalCourseUnits.Sum(), 2);
                                    //CGPA = Math.Round((firstYearFirstSemesterGPA + firstYearSecondSemesterGPA + secondYearFirstSemesterGPA + firstYearThirdSemesterGPA) / 4, 2);
                                }

                                secondYearFirstSemesterCGPA = CGPA;

                                //Get list of CGPA within the probation range
                                if (secondYearFirstSemesterCGPA < (decimal)2.0)
                                {
                                    Probation probation = new Probation();
                                    probation.CGPA = secondYearFirstSemesterCGPA;
                                    probation.Semester = new Semester() { Id = 1 };
                                    probation.Level = level;
                                    probationList.Add(probation);
                                }


                            }
                        }
                        else if (ss.Semester.Id == 2)
                        {
                            if (secondYearFirstSemesterResult != null && secondYearFirstSemesterResult.Count > 0)
                            {
                                TotalCourseUnits.Add(secondYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                TotalCoursePoints.Add((secondYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));

                                secondYearFirstSemesterGPA = ((secondYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / secondYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));

                                //Get the CGPA of both First year first,second,Third and second year first semesters
                                if (firstYearThirdSemesterResultHolder != null)
                                {
                                    secondYearFirstSemesterCGPA = ((firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (firstYearThirdSemesterResultHolder.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (secondYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0)) / ((firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearThirdSemesterResultHolder.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (secondYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)));
                                }
                                else
                                {
                                    secondYearFirstSemesterCGPA = ((firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (secondYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0)) / ((firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (secondYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)));
                                }


                                //Get list of CGPA within the probation range
                                if (secondYearFirstSemesterCGPA < (decimal)2.0)
                                {
                                    Probation probation = new Probation();
                                    probation.CGPA = secondYearFirstSemesterCGPA;
                                    probation.Semester = new Semester() { Id = 1 };
                                    probation.Level = level;
                                    probationList.Add(probation);
                                }
                            }
                            var secondYearSecondSemesterResult = allResults.Where(f => f.SemesterId == 2 && f.LevelId == level.Id && f.SpecialCase != "SICK" && f.SpecialCase != "ABSENT").ToList();
                            if (secondYearSecondSemesterResult?.Count > 0)
                            {
                                TotalCourseUnits.Add(secondYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                TotalCoursePoints.Add((secondYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));

                                secondYearSecondSemesterGPA = ((secondYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / secondYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                CGPA = Math.Round(TotalCoursePoints.Sum() / TotalCourseUnits.Sum(), 2);
                                secondYearSecondSemesterCGPA = CGPA;

                                //Get list of CGPA within the probation range
                                if (secondYearSecondSemesterCGPA < (decimal)2.0)
                                {
                                    Probation probation = new Probation();
                                    probation.CGPA = secondYearSecondSemesterCGPA;
                                    probation.Semester = new Semester() { Id = 2 };
                                    probation.Level = level;
                                    probationList.Add(probation);
                                }

                                //CGPA = Math.Round((firstYearFirstSemesterGPA + firstYearSecondSemesterGPA + secondYearFirstSemesterGPA + secondYearSecondSemesterGPA) / 4, 2);

                            }
                            //get all result for extra year student
                            if (isExtraYear)
                            {
                               
                                if ((programme.Id == 2 || programme.Id == 5) && (ss.Session.Id >= 9 && (level.Id == 2 || level.Id == 4)))
                                {
                                    var secondYearThirdSemesterResult = allResults.Where(f => f.SemesterId == 3 && f.LevelId == level.Id && f.SpecialCase != "SICK" && f.SpecialCase != "ABSENT").ToList();
                                    if (secondYearThirdSemesterResult != null && secondYearThirdSemesterResult.Count > 0)
                                    {
                                        //assign firstYearThirdSemesterResult to a global holder;
                                        secondYearThirdSemesterResultHolder = secondYearThirdSemesterResult;

                                        TotalCourseUnits.Add(secondYearThirdSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                        TotalCoursePoints.Add((secondYearThirdSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));

                                    }
                                    CGPA = Math.Round(TotalCoursePoints.Sum() / TotalCourseUnits.Sum(), 2);
                                    secondYearSecondSemesterCGPA = CGPA;
                                }

                            }





                        }
                        else if (ss.Semester.Id == 3)
                        {
                            if ((programme.Id == 2 || programme.Id == 5) && (ss.Session.Id >= 9 && (level.Id == 2 || level.Id == 4)))
                            {

                                if (secondYearFirstSemesterResult != null && secondYearFirstSemesterResult.Count > 0)
                                {
                                    TotalCourseUnits.Add(secondYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                    TotalCoursePoints.Add((secondYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));

                                    secondYearFirstSemesterGPA = ((secondYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / secondYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));

                                    //Get the CGPA of both First year first,second,Third and second year first and second semesters
                                    if (firstYearThirdSemesterResultHolder != null)
                                    {
                                        secondYearFirstSemesterCGPA = ((firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (firstYearThirdSemesterResultHolder.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (secondYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0)) / ((firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearThirdSemesterResultHolder.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (secondYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)));
                                    }
                                    else
                                    {
                                        secondYearFirstSemesterCGPA = ((firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (secondYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0)) / ((firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (secondYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)));
                                    }


                                    //Get list of CGPA within the probation range
                                    if (secondYearFirstSemesterCGPA < (decimal)2.0)
                                    {
                                        Probation probation = new Probation();
                                        probation.CGPA = secondYearFirstSemesterCGPA;
                                        probation.Semester = new Semester() { Id = 2 };
                                        probation.Level = level;
                                        probationList.Add(probation);
                                    }
                                }
                                var secondYearSecondSemesterResult = allResults.Where(f => f.SemesterId == 2 && f.LevelId == level.Id && f.SpecialCase != "SICK" && f.SpecialCase != "ABSENT").ToList();
                                if (secondYearSecondSemesterResult != null && secondYearSecondSemesterResult.Count > 0)
                                {
                                    TotalCourseUnits.Add(secondYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));
                                    TotalCoursePoints.Add((secondYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0));

                                    secondYearSecondSemesterGPA = ((secondYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) / secondYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit));

                                    //Get the CGPA of both First year first,second,Third and second year first and second semesters
                                    if (firstYearThirdSemesterResultHolder != null)
                                    {
                                        secondYearSecondSemesterCGPA = ((firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (firstYearThirdSemesterResultHolder.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (secondYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (secondYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0)) / ((firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearThirdSemesterResultHolder.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (secondYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (secondYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)));
                                    }
                                    else
                                    {
                                        secondYearSecondSemesterCGPA = ((firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (secondYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0) + (secondYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.GPCU) ?? 0)) / ((firstYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (firstYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (secondYearFirstSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)) + (secondYearSecondSemesterResult.Where(g => g.SpecialCase != "SICK" && g.SpecialCase != "ABSENT").Sum(x => x.CourseUnit)));
                                    }


                                    //Get list of CGPA within the probation range
                                    if (secondYearSecondSemesterCGPA < (decimal)2.0)
                                    {
                                        Probation probation = new Probation();
                                        probation.CGPA = secondYearSecondSemesterCGPA;
                                        probation.Semester = new Semester() { Id = 2 };
                                        probation.Level = level;
                                        probationList.Add(probation);
                                    }
                                }
                                var secondYearThirdSemesterResult = allResults.Where(f => f.SemesterId == 3 && f.LevelId == level.Id && f.SpecialCase != "SICK" && f.SpecialCase != "ABSENT").ToList();
                                if (secondYearThirdSemesterResult?.Count > 0)
                                {
                                    TotalCourseUnits.Add(secondYearThirdSemesterResult.Sum(x => x.CourseUnit));
                                    TotalCoursePoints.Add((secondYearThirdSemesterResult.Sum(x => x.GPCU) ?? 0));
                                    secondYearThirdSemesterGPA = ((secondYearThirdSemesterResult.Sum(x => x.GPCU) ?? 0) / secondYearThirdSemesterResult.Sum(x => x.CourseUnit));
                                    CGPA = Math.Round(TotalCoursePoints.Sum() / TotalCourseUnits.Sum(), 2);
                                    secondYearThirdSemesterCGPA = CGPA;
                                    //Get list of CGPA within the probation range
                                    if (secondYearThirdSemesterCGPA < (decimal)2.0)
                                    {
                                        Probation probation = new Probation();
                                        probation.CGPA = secondYearThirdSemesterCGPA;
                                        probation.Semester = new Semester() { Id = 3 };
                                        probation.Level = level;
                                        probationList.Add(probation);
                                    }

                                    //CGPA = Math.Round((firstYearFirstSemesterGPA + firstYearSecondSemesterGPA + firstYearThirdSemesterGPA + secondYearFirstSemesterGPA + secondYearSecondSemesterGPA + secondYearThirdSemesterGPA) / 6, 2);

                                }



                            }
                            else
                            {
                                throw new Exception("Selected Session-Semester is not valid for selected Programme");
                            }
                        }





                    }

                    var currentSessionSemester = allResults.Where(d => d.SessionSemesterId == sessionSemester.Id).ToList();

                    //bool isWithdrawn = isWithdrawal(probationList);
                    //if (isWithdrawn)
                    //{
                    //    withdrawnStudentMatricNo.Add(student.MatricNumber);

                    //}
                    PartialRemark partialRemark = isWithdrawalBasedOnProgramme(CGPA, oustandingCourses, probationList, programme.Id, level, sessions);
                    if (currentSessionSemester != null && currentSessionSemester.Count > 0)
                    {
                        for (int i = 0; i < currentSessionSemester.Count; i++)
                        {
                            result = currentSessionSemester[i];
                            result.CGPA = CGPA;
                            //result.Remark = GetSemesterRemarks(CGPA, oustandingCourses, programme.Id, ss.Semester.Id, isWithdrawn);
                            if (currentSessionSemester[i].SpecialCase == "SICK")
                            {
                                result.Grade = "S";
                            }
                            if (currentSessionSemester[i].SpecialCase == "ABSENT")
                            {
                                result.Grade = "ABS";
                            }


                            result.Remark = GetSemesterRemarksWithWithdrawal(CGPA, oustandingCourses, programme.Id, partialRemark);
                            result.Identifier = identifier;
                            results.Add(result);
                        }
                    }
                    if (results.FirstOrDefault().Remark != null && results.FirstOrDefault().Remark.Contains("WITHDRAWN"))
                    {
                        withdrawnStudentMatricNo.Add(student.MatricNumber);

                    }


                }
                if (withdrawnStudentMatricNo != null && withdrawnStudentMatricNo.Count > 0)
                {
                    DeactivateWithdrawnStudent(withdrawnStudentMatricNo);
                }

            }
            
            catch (Exception ex)
            {

                throw ex;
            }
            return results;
        }

        private string GetGraduationStatus(decimal? CGPA)
        {
            string remark = null;
            try
            {
                if (CGPA != null)
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
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return remark;
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
        public string GetDepartmentOptionName(long personId)
        {
            var optionName = "";
            try
            {
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                var level = studentLevelLogic.GetModelsBy(f => f.Person_Id == personId).LastOrDefault();
                if (level != null && level.DepartmentOption != null)
                {
                    optionName = level.DepartmentOption.Name;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return optionName;
        }
        private string GetSemesterRemarks(decimal? CGPA, List<Course> course, int programmeId, int semesterId, bool isWithdrawal)
        {
            string remark = null;
            try
            {
                if (course.Count == 0 && CGPA != null)
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
                    else if (isWithdrawal)
                    {
                        remark = "WITHDRAWN";
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
                        if (isWithdrawal)
                        {
                            remark = "WITHDRAWN / CO-";
                            for (int i = 0; i < course.Count(); i++)
                            {
                                remark += ("|" + course[i].Code);
                            }
                        }
                        else
                        {
                            remark = "PROBATION / CO-";
                            for (int i = 0; i < course.Count(); i++)
                            {
                                remark += ("|" + course[i].Code);
                            }
                        }

                    }
                    else
                    {
                        remark = "CO-";
                        for (int i = 0; i < course.Count(); i++)
                        {
                            remark += ("|" + course[i].Code);
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

        private string GetSemesterRemarksWithWithdrawal(decimal? CGPA, List<Course> course, int programmeId, PartialRemark partialRemark)
        {
            string remark = null;
            try
            {
                if (course.Count == 0 && CGPA != null && ((partialRemark != null && partialRemark.Remark != "WITHDRAWN") || (partialRemark == null)))
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
                        remark = partialRemark.Remark;
                    }

                }
                else if (course.Count != 0 && CGPA != null && ((partialRemark != null && partialRemark.Remark != "WITHDRAWN") || (partialRemark == null)))
                {
                    if (CGPA >= (decimal)3.5 && CGPA <= (decimal)4.0)
                    {
                        remark = "RHL; PASSED: DISTINCTION" + "/CO-";
                        for (int i = 0; i < course.Count(); i++)
                        {
                            remark += ("|" + course[i].Code);
                        }
                    }
                    else if (CGPA >= (decimal)3.25 && CGPA <= (decimal)3.49)
                    {
                        remark = "DHL; PASSED: UPPER CREDIT" + "/CO-";
                        for (int i = 0; i < course.Count(); i++)
                        {
                            remark += ("|" + course[i].Code);
                        }
                    }
                    else if (CGPA >= (decimal)3.0 && CGPA < (decimal)3.25)
                    {
                        remark = "PAS; PASSED: UPPER CREDIT" + "/CO-";
                        for (int i = 0; i < course.Count(); i++)
                        {
                            remark += ("|" + course[i].Code);
                        }
                    }
                    else if (CGPA >= (decimal)2.5 && CGPA <= (decimal)2.99)
                    {
                        remark = "PAS; PASSED: LOWER CREDIT" + "/CO-";
                        for (int i = 0; i < course.Count(); i++)
                        {
                            remark += ("|" + course[i].Code);
                        }
                    }
                    else if (CGPA >= (decimal)2.0 && CGPA <= (decimal)2.49)
                    {
                        remark = "PAS; PASSED: PASS" + "/CO-";
                        for (int i = 0; i < course.Count(); i++)
                        {
                            remark += ("|" + course[i].Code);
                        }
                    }
                    else if (CGPA < (decimal)2.0)
                    {
                        remark = partialRemark.Remark + "/CO-";
                        for (int i = 0; i < course.Count(); i++)
                        {
                            remark += ("|" + course[i].Code);
                        }
                    }
                }
                else
                {

                    if (partialRemark.Remark == "WITHDRAWN")
                    {
                        //remark = "WITHDRAWN / CO-";
                        remark = partialRemark.Remark + "/CO-";
                        for (int i = 0; i < course.Count(); i++)
                        {
                            remark += ("|" + course[i].Code);
                        }
                    }
                    else
                    {
                        remark = partialRemark.Remark + "/CO-";
                        for (int i = 0; i < course.Count(); i++)
                        {
                            remark += ("|" + course[i].Code);
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

        public bool isWithdrawal(List<Probation> probationList)
        {
            if (probationList.Count > 1)
            {
                if (probationList.Count == 2)
                {
                    if (probationList[0].Level.Id == probationList[1].Level.Id)
                    {
                        return true;
                    }
                }
                else
                {
                    for (int j = 0; j <= probationList.Count - 1; j++)
                    {
                        if (probationList[j].Level.Id == probationList[j + 1].Level.Id)
                        {
                            //Takes care of the next semester in the same level
                            if (probationList[j].Semester.Id == probationList[j + 1].Semester.Id - 1)
                            {
                                return true;
                            }

                        }
                        else
                        {
                            //Takes care of 3rd semester and First semester the next year/different level
                            if (probationList[j].Semester.Id == 3 && probationList[j].Semester.Id == probationList[j + 1].Semester.Id + 2)
                            {
                                return true;
                            }
                            //Takes care of 2nd semester and First semester the next year/different level
                            else if (probationList[j].Semester.Id == probationList[j + 1].Semester.Id + 1)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        public PartialRemark isWithdrawalBasedOnProgramme(decimal? CGPA, List<Course> course, List<Probation> probationList, int ProgrammeId,Level level, Session session)
        {
            PartialRemark partialRemark = new PartialRemark();
            if (probationList.Count > 1)
            {
                if (probationList.Count == 2)
                {
                    if (probationList[0].Level.Id == probationList[1].Level.Id)
                    {
                        if ((ProgrammeId == 2 || ProgrammeId == 5 || ProgrammeId == 4) && (((level.Id == 1 || level.Id == 3) && session.Id >= 9) || (session.Id > 9)))
                        {
                            partialRemark.Remark = "PROBATION II";

                            return partialRemark;
                        }
                        else
                        {
                            partialRemark.Remark = "WITHDRAWN";

                            return partialRemark;
                        }

                    }
                    else if(probationList[0].Level.Id != probationList[1].Level.Id && (((probationList[0].Level.Id - probationList[1].Level.Id) <= 1) || ((probationList[1].Level.Id - probationList[0].Level.Id) <= 1)))
                    {
                        
                        if ((ProgrammeId == 2 || ProgrammeId == 5 || ProgrammeId == 4) && (((level.Id == 1 || level.Id == 3) && session.Id >= 9) || (session.Id > 9)))
                        {
                            partialRemark.Remark = "PROBATION II";

                            return partialRemark;
                        }
                        else if((probationList[0].Semester.Id==2 && probationList[1].Semester.Id == 1) && (ProgrammeId != 2 || ProgrammeId != 5 || ProgrammeId != 4))
                        {
                            //take care of next semester of next level considering the last level last semester
                            partialRemark.Remark = "WITHDRAWN";

                            return partialRemark;
                        }
                    }
                }
                else
                {
                    for (int j = 0; j <= probationList.Count - 1; j++)
                    {
                        if ((j + 1) <= (probationList.Count - 1))
                        {
                            if (probationList[j].Level.Id == probationList[j + 1].Level.Id)
                            {
                                //Takes care of the next semester in the same level
                                if (probationList[j].Semester.Id == probationList[j + 1].Semester.Id - 1)
                                {

                                    partialRemark.Remark = "WITHDRAWN";
                                    return partialRemark;
                                    //return true;
                                }

                            }
                            else
                            {
                                //Takes care of 3rd semester and First semester the next year/different level
                                if (probationList[j].Semester.Id == 3 && probationList[j].Semester.Id == probationList[j + 1].Semester.Id + 2)
                                {
                                    if (j > 0 && ((probationList[j - 1].Semester.Id + 1) == probationList[j].Semester.Id) && (ProgrammeId == 2 || ProgrammeId == 5 || ProgrammeId == 4) && (((level.Id == 1 || level.Id == 3) && session.Id >= 9) || (session.Id > 9)))
                                    {
                                        partialRemark.Remark = "WITHDRAWN";
                                        return partialRemark;
                                    }
                                    else if ((ProgrammeId != 2 && ProgrammeId != 5 && ProgrammeId != 4))
                                    {
                                        partialRemark.Remark = "WITHDRAWN";
                                        return partialRemark;
                                    }

                                }
                                //Takes care of 2nd semester and First semester the next year/different level
                                else if (probationList[j].Semester.Id == probationList[j + 1].Semester.Id + 1)
                                {
                                    partialRemark.Remark = "WITHDRAWN";
                                    return partialRemark;
                                }
                            }
                        }
                        
                    }
                }
            }
            else if (probationList.Count == 1)
            {
                //if ((ProgrammeId == 2 || ProgrammeId == 5))
                if ((ProgrammeId == 2 || ProgrammeId == 5 || ProgrammeId == 4) && (((level.Id == 1 || level.Id == 3) && session.Id >= 9) || (session.Id > 9)))
                {
                    partialRemark.Remark = "PROBATION I";

                    return partialRemark;
                }
                else
                {
                    partialRemark.Remark = "PROBATION";

                    return partialRemark;
                }
            }
            return partialRemark;


        }

        public bool CheckForExistingCarryOver(List<Course> outStandings, long courseId)
        {
            try
            {
                if (outStandings != null && outStandings.Count > 0)
                {
                    for (int i = 0; i < outStandings.Count; i++)
                    {
                        long Id = outStandings[i].Id;
                        if (courseId == Id)
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return false;
        }
        public void DeactivateWithdrawnStudent(List<string> matricNos)
        {
            try
            {
                if (matricNos != null && matricNos.Count > 0)
                {
                    foreach (var matricNo in matricNos)
                    {
                        StudentLogic studentLogic = new StudentLogic();
                        var student = studentLogic.GetModelsBy(f => f.Matric_Number == matricNo).FirstOrDefault();
                        if (student != null)
                        {

                            student.Activated = false;
                            student.Reason = "System deactivated Student For Having Withdrawn";
                            studentLogic.Modify(student);
                            GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();

                            string Action = "CREATE";
                            string Operation = "System deactivated Student" + student.MatricNumber + "For Having Withdrawn";
                            string Table = "Student Table";
                            generalAuditLogic.CreateGeneralAudit(Action, Operation, Table);
                        }
                    }
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        //check if student has completed school fees
        public bool HascompletedSchoolFees(string MatricNo)
        {
            if (!string.IsNullOrEmpty(MatricNo))
            {
                StudentLogic studentLogic = new StudentLogic();
                var student=studentLogic.GetModelsBy(f => f.Matric_Number == MatricNo).FirstOrDefault();
                if (student?.Id > 0)
                {
                    RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                    PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
                    PaymentLogic paymentLogic = new PaymentLogic();

                    // check for students in second year
                    CourseRegistrationLogic courseRegistrationLogic = new CourseRegistrationLogic();
                    var studentRegistrations = courseRegistrationLogic.GetModelsBy(f => f.STUDENT.Person_Id == student.Id);
                    if (studentRegistrations?.Count > 0)
                    {
                        foreach (var item in studentRegistrations)
                        {
                            var paidSchoolFees = remitaPaymentLogic.GetModelsBy(f => f.PAYMENT.Person_Id == item.Student.Id && f.PAYMENT.Session_Id == item.Session.Id &&
                            (f.PAYMENT.Fee_Type_Id == 3 || f.PAYMENT.Fee_Type_Id == 10) && (f.Status.Contains("01") || f.Description.Contains("manual"))).FirstOrDefault();
                            if (paidSchoolFees == null)
                            {
                                var payment = paymentLogic.GetModelsBy(f => f.Person_Id == item.Student.Id && f.Session_Id == item.Session.Id && (f.Fee_Type_Id == 3 || f.Fee_Type_Id == 10)).FirstOrDefault();
                                if (payment?.Id > 0)
                                {
                                    var etranzactRecord = paymentEtranzactLogic.GetModelsBy(f => f.Payment_Id == payment.Id).FirstOrDefault();
                                    if (etranzactRecord?.ConfirmationNo != null)
                                    {

                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                                else
                                {
                                    return false;
                                }



                            }
                        }
                    }
                    else
                    {
                        return false;
                    }

                    return true;
                }
                else
                {
                    return false;
                }
                
            }
            return false;
        }

    }
}
