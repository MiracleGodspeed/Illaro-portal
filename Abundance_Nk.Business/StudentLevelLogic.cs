using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Translator;
using System.Linq.Expressions;
using Abundance_Nk.Model.Entity.Model;

namespace Abundance_Nk.Business
{
    public class StudentLevelLogic : BusinessBaseLogic<StudentLevel, STUDENT_LEVEL>
    {
        public StudentLevelLogic()
        {
            translator = new StudentLevelTranslator();
        }

        public StudentLevel GetBy(long studentId)
        {
            try
            {
                SessionLogic sessionLogic = new SessionLogic();
                Expression<Func<STUDENT_LEVEL, bool>> selector = sl => sl.Person_Id == studentId ;
                List<StudentLevel> studentLevels = base.GetModelsBy(selector);
                Session session = sessionLogic.GetModelBy(p => p.Activated == true);
                if (studentLevels != null && studentLevels.Count > 0)
                {
                    int maxLevel =  studentLevels.Max(p => p.Level.Id);
                    Expression<Func<STUDENT_LEVEL, bool>> selector2 = sl => sl.Person_Id == studentId && sl.Level_Id == maxLevel && sl.Session_Id == session.Id;
                    StudentLevel CurrentLevel = base.GetModelBy(selector2);
                    if (CurrentLevel == null)
                    {
                        int minLevel = studentLevels.Min(p => p.Level.Id);
                        Expression<Func<STUDENT_LEVEL, bool>> selector3 = sl => sl.Person_Id == studentId && sl.Level_Id == minLevel;
                        StudentLevel CurrentLevelAlt = base.GetModelsBy(selector3).LastOrDefault();
                        CurrentLevel = CurrentLevelAlt;
                    }
                    return CurrentLevel;
                }

                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public StudentLevel GetExtraYearBy(long studentId)
        {
            try
            {
                SessionLogic sessionLogic = new SessionLogic();
                Expression<Func<STUDENT_LEVEL, bool>> selector = sl => sl.Person_Id == studentId;
                List<StudentLevel> studentLevels = base.GetModelsBy(selector);
                Session session = sessionLogic.GetModelBy(p => p.Activated == true);
                if (studentLevels != null && studentLevels.Count > 0)
                {
                    int maxLevel = studentLevels.Max(p => p.Level.Id);
                    Expression<Func<STUDENT_LEVEL, bool>> selector2 = sl => sl.Person_Id == studentId && sl.Level_Id == maxLevel && sl.Session_Id == session.Id;
                    StudentLevel CurrentLevel = base.GetModelBy(selector2);
                    if (CurrentLevel == null)
                    {
                        int minLevel = studentLevels.Min(p => p.Level.Id);
                        Expression<Func<STUDENT_LEVEL, bool>> selector3 = sl => sl.Person_Id == studentId && sl.Level_Id == minLevel && sl.Session_Id == session.Id;
                        StudentLevel CurrentLevelAlt = base.GetModelBy(selector3);
                        CurrentLevel = CurrentLevelAlt;
                    }
                    if (CurrentLevel == null)
                    {
                        int maxLevel2 = studentLevels.Max(p => p.Level.Id);
                        Expression<Func<STUDENT_LEVEL, bool>> selector4 = sl => sl.Person_Id == studentId && sl.Level_Id == maxLevel;
                        StudentLevel CurrentLevel2 = base.GetModelBy(selector4);
                        CurrentLevel = CurrentLevel2;
                        if (CurrentLevel2 == null)
                        {
                            int minLevel = studentLevels.Min(p => p.Level.Id);
                            Expression<Func<STUDENT_LEVEL, bool>> selector5 = sl => sl.Person_Id == studentId && sl.Level_Id == minLevel;
                            StudentLevel CurrentLevelAlt = base.GetModelBy(selector5);
                            CurrentLevel = CurrentLevelAlt;
                        }
                    }
                    return CurrentLevel;
                }

                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public StudentLevel GetBy(string MatricNumber)
        {
            try
            {
                SessionLogic sessionLogic = new SessionLogic();
                Expression<Func<STUDENT_LEVEL, bool>> selector = sl => sl.STUDENT.Matric_Number == MatricNumber;
                List<StudentLevel> studentLevels = base.GetModelsBy(selector);
                Session session = sessionLogic.GetModelBy(p => p.Activated == true);
                if (studentLevels != null && studentLevels.Count > 0)
                {
                    int maxLevel = studentLevels.Max(p => p.Level.Id);
                    Expression<Func<STUDENT_LEVEL, bool>> selector2 = sl => sl.STUDENT.Matric_Number == MatricNumber && sl.Level_Id == maxLevel && sl.Session_Id == session.Id;
                    StudentLevel CurrentLevel = base.GetModelBy(selector2);
                    if (CurrentLevel == null)
                    {
                        int minLevel = studentLevels.Min(p => p.Level.Id);
                        Expression<Func<STUDENT_LEVEL, bool>> selector3 = sl => sl.STUDENT.Matric_Number == MatricNumber && sl.Level_Id == minLevel && sl.Session_Id == session.Id;
                        StudentLevel CurrentLevelAlt = base.GetModelBy(selector3);
                        CurrentLevel = CurrentLevelAlt;
                    }
                    return CurrentLevel;
                }

                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public StudentLevel GetBy(Student student, Session session)
        {
            try
            {
                Expression<Func<STUDENT_LEVEL, bool>> selector = sl => sl.Person_Id == student.Id && sl.Session_Id == session.Id;
                return base.GetModelsBy(selector).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<StudentLevel> GetBy(Level level, Session session)
        {
            try
            {
                Expression<Func<STUDENT_LEVEL, bool>> selector = sl => sl.Level_Id == level.Id && sl.Session_Id == session.Id;
                return base.GetModelsBy(selector);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<StudentLevel> GetBy(Level level, Programme programme, Department department, Session session)
        {
            try
            {
                Expression<Func<STUDENT_LEVEL, bool>> selector = sl => sl.Level_Id == level.Id && sl.Programme_Id == programme.Id && sl.Department_Id == department.Id && sl.Session_Id == session.Id;
                return base.GetModelsBy(selector);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool Modify(StudentLevel student)
        {
            try
            {
                StudentLevel model = GetBy(student.Student.Id);
                Expression<Func<STUDENT_LEVEL, bool>> selector = sl => sl.Level_Id == model.Level.Id && sl.Person_Id == model.Student.Id;
                STUDENT_LEVEL entity = GetEntityBy(selector);
                entity.Level_Id = student.Level.Id;
                if (student.Department != null)
                {
                    entity.Department_Id = student.Department.Id;
                }
                if (student.Programme != null)
                {
                    entity.Programme_Id = student.Programme.Id;
                }
                int modifiedRecordCount = Save();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool Modify(StudentLevel student, Person person)
        {
            try
            {
                StudentLevel model = GetBy(person.Id);
                Expression<Func<STUDENT_LEVEL, bool>> selector = sl => sl.Level_Id == model.Level.Id && sl.Person_Id == model.Student.Id && sl.Session_Id == student.Session.Id;
                STUDENT_LEVEL entity = GetEntityBy(selector);

                entity.Level_Id = student.Level.Id;
                entity.Department_Id = student.Department.Id;
                if (student.DepartmentOption != null)
                {
                    entity.Department_Option_Id = student.DepartmentOption.Id;
                }
               

                int modifiedRecordCount = Save();

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool Modify(StudentLevel student, long studentLevelId)
        {
            try
            {
                Expression<Func<STUDENT_LEVEL, bool>> selector = sl => sl.Student_Level_Id == studentLevelId && sl.Person_Id == student.Student.Id;
                STUDENT_LEVEL entity = GetEntityBy(selector);
                if (student.Level != null)
                {
                    entity.Level_Id = student.Level.Id; 
                }
                if (student.Session != null)
                {
                    entity.Session_Id = student.Session.Id;
                } 
                if (student.Department != null)
                {
                    entity.Department_Id = student.Department.Id;
                }
                if (student.DepartmentOption != null)
                {
                    entity.Department_Option_Id = student.DepartmentOption.Id;
                }
                if (student.Programme != null)
                {
                    entity.Programme_Id = student.Programme.Id;
                }
                int modifiedRecordCount = Save();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool ModifyByStudentLevelId(StudentLevel student)
        {
            try
            {
                Expression<Func<STUDENT_LEVEL, bool>> selector = sl => sl.Student_Level_Id == student.Id;
                STUDENT_LEVEL entity = GetEntityBy(selector);
                if (student.Level != null)
                {
                    entity.Level_Id = student.Level.Id;
                }
                if (student.Session != null)
                {
                    entity.Session_Id = student.Session.Id;
                }
                if (student.Department != null)
                {
                    entity.Department_Id = student.Department.Id;
                }
                if (student.DepartmentOption != null)
                {
                    entity.Department_Option_Id = student.DepartmentOption.Id;
                }
                if (student.Programme != null)
                {
                    entity.Programme_Id = student.Programme.Id;
                }
                if (student.Student != null)
                {
                    entity.Person_Id = student.Student.Id;
                }
                int modifiedRecordCount = Save();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public StudentLevel GetStudentLevel(long personId)
        {
            StudentLevel studentLevel = null;
            try
            {
                Programme programme = null;
                Department department = null;
                Level level = null;

                StudentLogic studentLogic = new StudentLogic();
                DepartmentLogic departmentLogic = new DepartmentLogic();

                Model.Model.Student student = studentLogic.GetBy(personId);
                if (student != null && !string.IsNullOrEmpty(student.MatricNumber))
                {
                    string[] matricNumber = student.MatricNumber.Split('/');
                    if (matricNumber[0] == "H")
                    {
                        programme = new Programme { Id = (int)Programmes.HNDFullTime, Name = "HND Full Time" };
                        level = new Level { Id = (int)LevelList.HND2, Name = "HND II" };
                    }
                    if (matricNumber[0] == "N")
                    {
                        programme = new Programme { Id = (int)Programmes.NDFullTime, Name = "ND Full Time" };
                        level = new Level { Id = (int)LevelList.ND2, Name = "ND II" };
                    }
                    if (matricNumber[0] == "PN")
                    {
                        programme = new Programme { Id = (int)Programmes.NDPartTime, Name = "ND Part Time" };
                        level = new Level { Id = (int)LevelList.ND2, Name = "ND II" };
                    }

                    if (!string.IsNullOrEmpty(matricNumber[1]))
                    {
                        var deptCode = matricNumber[1];
                        department = departmentLogic.GetModelsBy(d => d.Department_Code == deptCode).LastOrDefault();
                        if (department == null)
                            department = new Department { Id = 1 };
                    }
                    else
                        department = new Department { Id = 1 };
                }

                studentLevel = new StudentLevel { Programme = programme, Department = department, Level = level };
            }
            catch (Exception)
            {
                throw;
            }

            return studentLevel;
        }

        public  List<StudentLevel> GetStudentLevelBy(Programme programme, Session session2015, Session session2016, Session session2017, Session session2018)
        {
            List<StudentLevel> studentLevel = new List<StudentLevel>();
            try
            {
                Expression<Func<STUDENT_LEVEL, bool>> selector = sl => (sl.Programme_Id == programme.Id && sl.Session_Id == session2015.Id) ||
                                                                        (sl.Programme_Id == programme.Id && sl.Session_Id == session2016.Id) ||
                                                                        (sl.Programme_Id == programme.Id && sl.Session_Id == session2017.Id) ||
                                                                        (sl.Programme_Id == programme.Id && sl.Session_Id == session2018.Id);
                studentLevel =  GetModelsBy(selector);
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return studentLevel;

        }

    }




}
