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
using Abundance_Nk.Model.Entity.Model;
using System.Configuration;

namespace Abundance_Nk.Business
{
    public class StudentLogic : BusinessBaseLogic<Student, STUDENT>
    {
        private PersonLogic personLogic;
        private StudentMatricNumberAssignmentLogic studentMatricNumberAssignmentLogic;
        Abundance_NkEntities abundance_NkEntities;

        public StudentLogic()
        {
            personLogic = new PersonLogic();
            translator = new StudentTranslator();
            studentMatricNumberAssignmentLogic = new StudentMatricNumberAssignmentLogic();
            abundance_NkEntities = new Abundance_NkEntities();
        }
        public bool ValidateUser(string Username, string Password)
        {
            try
            {
                Expression<Func<STUDENT, bool>> selector = p => p.Matric_Number == Username && p.Password_hash == Password && p.Activated != false;
                Student UserDetails = GetModelBy(selector);
                if (UserDetails != null && UserDetails.PasswordHash != null)
                {
                    //UpdateLastLogin(UserDetails);
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public bool ChangeUserPassword(Student student)
        {
            try
            {
                Expression<Func<STUDENT, bool>> selector = p => p.Matric_Number == student.MatricNumber;
                STUDENT userEntity = GetEntityBy(selector);
                if (userEntity == null || userEntity.Person_Id <= 0)
                {
                    throw new Exception(NoItemFound);
                }

                userEntity.Password_hash = student.PasswordHash;

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
        public Student GetBy(string matricNumber)
        {
            try
            {
                Expression<Func<STUDENT, bool>> selector = s => s.Matric_Number == matricNumber;
                return base.GetModelsBy(selector).LastOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Student GetBy(long id)
        {
            try
            {
                Expression<Func<STUDENT, bool>> selector = s => s.Person_Id == id;
                return base.GetModelBy(selector);
            }
            catch (Exception)
            {
                throw;
            }
        }


        public List<Student> GetStudentsBy(string matricNumber)
        {
            try
            {
                Expression<Func<STUDENT, bool>> selector = s => s.Matric_Number == matricNumber;
                return base.GetModelsBy(selector);
            }
            catch (Exception)
            {
                throw;
            }
        }


        public bool Modify(Student student)
        {
            try
            {
                STUDENT entity = GetEntityBy(s => s.Person_Id == student.Id);
                
                if (entity != null)
                {

                entity.Person_Id = student.Id;
                entity.Student_Number = student.Number;
                if (student.MatricNumber != null)
                {
                    entity.Matric_Number = student.MatricNumber;
                }
                
                entity.School_Contact_Address = student.SchoolContactAddress;
                entity.Activated = student.Activated;
                entity.Reason = student.Reason;
                entity.Reject_Category = student.RejectCategory;
                if (student.Type !=null && student.Type.Id > 0)
                {
                  entity.Student_Type_Id = student.Type.Id;
                }

                if (student.Category != null && student.Category.Id > 0)
                {
                  entity.Student_Category_Id = student.Category.Id;
                }

                if (student.Status != null && student.Status.Id > 0)
                {
                    entity.Student_Status_Id = student.Status.Id;
                }
                if (student.Title != null && student.Title.Id > 0)
                {
                    entity.Title_Id = student.Title.Id;
                }
                if (student.MaritalStatus != null && student.MaritalStatus.Id > 0)
                {
                    entity.Marital_Status_Id = student.MaritalStatus.Id;
                }
                if (student.BloodGroup != null && student.BloodGroup.Id > 0)
                {
                    entity.Blood_Group_Id = student.BloodGroup.Id;
                }
                if (student.Genotype != null && student.Genotype.Id > 0)
                {
                    entity.Genotype_Id = student.Genotype.Id;
                }
                if (student.ApplicationForm != null && student.ApplicationForm.Id > 0)
                {
                    entity.Application_Form_Id = student.ApplicationForm.Id;
                }
                if(student.StudentMedicalReport != null)
                {
                    entity.StudentMedicalReport = student.StudentMedicalReport;
                }
                
                int modifiedRecordCount = Save();
               

                return true;
                }
                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public long GetNextNumberFor()
        {
            try
            {
                long newSerialNumber = repository.GetMaxValueBy<STUDENT>(s => (int)s.Student_Number);
                newSerialNumber += 1;
                
                return newSerialNumber;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override Student Add(Student student)
        {
            try
            {
                Person person = personLogic.Create(student);
                student.Id = person.Id;

                Student newStudent = base.Create(student);
                if (newStudent != null)
                {
                    newStudent.FullName = person.FullName;
                }

                return newStudent;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override Student Create(Student model)
        {
            STUDENT studentEntity = GetEntityBy(s => s.Matric_Number == model.MatricNumber);
            model.Activated = true;
            model.PasswordHash = "1234567";
            if (studentEntity == null )
            {
                return base.Create(model);
            }
            return null;
        }
        public bool AssignMatricNumber(Person person, long studentNumber, string matricNumber)
        {
            try
            {
                STUDENT studentEntity = GetEntityBy(s => s.Person_Id == person.Id);
                studentEntity.Matric_Number = matricNumber;
                studentEntity.Student_Number = studentNumber;

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

        public bool AssignMatricNumber(ApplicationFormView applicant)
        {
            try
            {
                //Get admission list to be sure of student current department
                AdmissionList admissionList = new AdmissionList();
                AdmissionListLogic admissionLogic = new AdmissionListLogic();
                admissionList = admissionLogic.GetBy(applicant.FormId);
                if (admissionList == null)
                {
                    return false;
                }

                //assign matric no to applicant
                Faculty faculty = new Faculty();
                faculty = admissionList.Deprtment.Faculty;

                Department department = new Department();
                department = admissionList.Deprtment;

                DepartmentOption departmentOption = new DepartmentOption();
                departmentOption = admissionList.DepartmentOption;

                Session session = new Session() { Id = applicant.SessionId };
                Programme programme = new Programme() { Id = applicant.ProgrammeId };
                Level level;
                if (applicant.ProgrammeId == (int)Programmes.DrivingCertificate)
                {
                    level = new Level { Id = 1 };
                }
                else if (applicant.ProgrammeId == (int)Programmes.NDDistance)
                {
                    level = new Level { Id = 1 };
                }
                else if (applicant.ProgrammeId == (int)Programmes.HNDDistance)
                {
                    level = new Level { Id = 3 };
                }
                else if (applicant.ProgrammeId > 2)
                {
                    level = new Level { Id = 3 };
                }
                else
                {
                    level = new Level { Id = 1 };
                }
                var studentCategory = applicant.ProgrammeId <= 2 ? 1 : 1;
                var studentType = applicant.ProgrammeId <= 2 ? 1 : 1;

                StudentMatricNumberAssignment startMatricNo = studentMatricNumberAssignmentLogic.GetBy(faculty, department, programme, level, session);
                var insertedStudent = abundance_NkEntities.STP_INSERT_STUDENT(applicant.PersonId, applicant.FormId, studentType, studentCategory, 1, null, null, 1, 1, 1, 1, "", true, null, null, "1234567");
                if (startMatricNo != null)
                {
                    //Student student = new Student();
                    //student.Id = applicant.PersonId;
                    //student.ApplicationForm = new ApplicationForm() { Id = applicant.FormId };
                    //student.Type = new StudentType() { Id = applicant.ProgrammeId <= 2 ? 1 : 2 };
                    //student.Category = new StudentCategory() { Id = applicant.ProgrammeId <= 2 ? 1 : 1 };
                    //student.Status = new StudentStatus() { Id = 1 };
                    //student.Number = null;
                    //student.MatricNumber = null;
                    //student.Activated = true;
                    //student.PasswordHash = "1234567";
                    //Student newStudent = base.Create(student);
                    StudentLogic studentLogic = new StudentLogic();
                    var existingStudent = studentLogic.GetModelBy(g => g.Person_Id == applicant.PersonId);
                    if (insertedStudent != null && existingStudent!=null )
                    {
                        
                        
                        StudentLevel studentLevel = new StudentLevel();
                        studentLevel.Session = new Session() { Id = session.Id };
                        studentLevel.Level = new Level() { Id = applicant.ProgrammeId == (int)Programmes.HNDDistance || applicant.ProgrammeId == (int)Programmes.NDDistance ? level.Id : applicant.ProgrammeId <= 2 || applicant.ProgrammeId == 7  ? 1 :  3 };
                        studentLevel.Student = existingStudent;
                        studentLevel.Department = department;
                        studentLevel.Programme = programme;
                        if (departmentOption != null && departmentOption.Id > 0)
                        {
                            studentLevel.DepartmentOption = departmentOption;
                        }

                        StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                        StudentLevel check = studentLevelLogic.GetModelsBy(s => s.Session_Id == session.Id && s.Person_Id == existingStudent.Id).LastOrDefault();
                        if (check == null)
                        {
                            return studentLevelLogic.Create(studentLevel) != null ? true : false;
                        }
                    }
                    
                    
                }
                else
                {
                    throw new Exception(applicant.LevelName + " for " + applicant.DepartmentName + " for the current academic session has not been set! Please contact your system administrator.");
                }
                return true;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public bool UpdateMatricNumber(StudentLevel studentLevel,Student student)
        {
            try   
            {

                //StudentMatricNumberAssignment startMatricNo = studentMatricNumberAssignmentLogic.GetBy(studentLevel.Department.Faculty, studentLevel.Department, studentLevel.Programme, studentLevel.Level, studentLevel.Session);
                //if (startMatricNo != null)
                //{
                //    long studentNumber = 0;
                //    string matricNumber = "";

                //    if (startMatricNo.Used)
                //    {
                //        string[] matricNoArray = startMatricNo.MatricNoStartFrom.Split('/');
                //        studentNumber = GetNextStudentNumber(studentLevel.Department.Faculty, studentLevel.Department, studentLevel.Programme, studentLevel.Level, studentLevel.Session);
                //        matricNoArray[matricNoArray.Length - 1] = UtilityLogic.PaddNumber(studentNumber, 4);
                //        matricNumber = string.Join("/", matricNoArray);

                      
                //    }
                //    else
                //    {
                //        matricNumber = startMatricNo.MatricNoStartFrom;
                //        studentNumber = startMatricNo.MatricSerialNoStartFrom;
                //        bool markedAsUsed = studentMatricNumberAssignmentLogic.MarkAsUsed(startMatricNo);
                //    }

                    

                //    student.Status = new StudentStatus() { Id = 1 };
                //    student.Number = studentNumber;
                //    student.MatricNumber = matricNumber;
                //    return Modify(student);

                   
                //}
                //else
                //{
                //    throw new Exception(studentLevel.Level.Name + " for " + studentLevel.Department.Name + " for the current academic session has not been set! Please contact your system administrator.");
                //}
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        public long GetNextStudentNumber(Faculty faculty, Department department,Programme programme, Level level, Session session)
        {
            try
            {
                long newStudentNumber = 0;
                List<ApplicationFormView> applicationForms = (from a in repository.GetBy<VW_ASSIGNED_MATRIC_NUMBER>(a =>  a.Department_Id == department.Id && a.Programme_Id == programme.Id && a.Level_Id == level.Id && a.Session_Id == session.Id)
                                                              select new ApplicationFormView
                                                              {
                                                                  FormId = a.Application_Form_Id.Value,
                                                                  StudentNumber = (long)a.Student_Number,
                                                                  MatricNumber = a.Matric_Number,
                                                                  PersonId = a.Person_Id,
                                                              }).ToList();

                if (applicationForms != null && applicationForms.Count > 0)
                {
                    long rawMaxStudentNumber = applicationForms.Max(s => s.StudentNumber);
                    newStudentNumber = rawMaxStudentNumber + 1;
                }


                //List<ApplicationFormView> applicationForms = (from s in repository.GetBy<STUDENT_LEVEL>(s => s.DEPARTMENT.Faculty_Id == faculty.Id && s.Department_Id == department.Id && s.Programme_Id == programme.Id && s.Level_Id == level.Id && s.Session_Id == session.Id)
                //           select new ApplicationFormView
                //           {
                //               FormId = (long) s.STUDENT.Application_Form_Id,
                //               StudentNumber = (long) s.STUDENT.Student_Number,
                //               MatricNumber = s.STUDENT.Matric_Number,
                //               PersonId = s.Person_Id,

                //           }).ToList();

                //if (applicationForms != null && applicationForms.Count > 0)
                //{
                //    long rawMaxStudentNumber = applicationForms.Max(s => s.StudentNumber);
                //    newStudentNumber = rawMaxStudentNumber + 1;
                //}

                return newStudentNumber;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<StudentReport> GetStudentInformationBy(Department department, Programme programme, Session session, Level level)
        {
            try
            {
                List<StudentReport> studentReportList = new List<StudentReport>();
                List<StudentLevel> studentLevelList = new List<StudentLevel>();
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                PersonLogic personLogic = new PersonLogic();
                if (department != null && programme != null && session != null && level != null)
                {
                    studentLevelList = studentLevelLogic.GetModelsBy(p => p.Department_Id == department.Id && p.Programme_Id == programme.Id && p.Level_Id == level.Id && p.Session_Id == session.Id);
                    foreach (StudentLevel studentLevelitem in studentLevelList)
                    {
                        Person person = new Person();
                        StudentReport studentReport = new StudentReport();
                        person = personLogic.GetModelBy(p => p.Person_Id == studentLevelitem.Student.Id);
                        studentReport.Fullname = person.LastName + " " + person.FirstName + " " + person.OtherName;
                        studentReport.RegistrationNumber = studentLevelitem.Student.MatricNumber;
                        studentReport.Address = person.HomeAddress;
                        studentReport.Othernames = person.FirstName + "  " + person.OtherName;
                        studentReport.Lastname = person.LastName;
                        if (person.Sex != null)
                        {
                            studentReport.Sex = person.Sex.Name;
                        }
                        else
                        {
                            studentReport.Sex = "";
                        }
                        if (person.State != null)
                        {
                            studentReport.State = person.State.Name;
                        }
                        else
                        {
                            studentReport.State = "";
                        }
                        studentReport.MobileNumber = person.MobilePhone;
                        if (person.LocalGovernment != null)
                        {
                            studentReport.LocalGovernment = person.LocalGovernment.Name;
                        }
                        else
                        {
                            studentReport.LocalGovernment = "";
                        }

                        studentReportList.Add(studentReport);

                    }
                    return studentReportList.OrderBy(a => a.RegistrationNumber).ToList();
                }
                else
                {
                    return new List<StudentReport>();
                }


            }
            catch (Exception)
            {

                throw;
            }
        }

        public List<StudentReport> GetRegisteredStudentBy(Department department, Programme programme, Session session, Level level)
        {
            try
            {
                List<StudentReport> studentReportList = new List<StudentReport>();
                List<CourseRegistration> courseRegistrationList = new List<CourseRegistration>();
                List<StudentLevel> studentLevelList = new List<StudentLevel>();

                CourseRegistrationLogic courseLogic = new CourseRegistrationLogic();
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                PersonLogic personLogic = new PersonLogic();
                SessionLogic sessionLogic = new SessionLogic();
                ApplicantJambDetailLogic applicantJambDetailLogic = new ApplicantJambDetailLogic();
                ProgrammeLogic programmeLogic = new ProgrammeLogic();
                DepartmentLogic departmentLogic = new DepartmentLogic();
                CourseRegistrationLogic courseRegistrationLogic = new CourseRegistrationLogic();
                if (department != null && programme != null && session != null && level != null)
                {
                    department = departmentLogic.GetModelsBy(g => g.Department_Id == department.Id).FirstOrDefault();
                    programme = programmeLogic.GetModelsBy(g => g.Programme_Id == programme.Id).FirstOrDefault();
                    studentLevelList = studentLevelLogic.GetModelsBy(c => c.Department_Id == department.Id && c.Level_Id == level.Id && c.Programme_Id == programme.Id && c.Session_Id == session.Id);
                    string appRoot = ConfigurationManager.AppSettings["AppRoot"];


                    foreach (var studentLevel in studentLevelList)
                    {
                        var isExist=courseRegistrationLogic.GetModelsBy(f => f.Person_Id == studentLevel.Student.Id && f.Programme_Id == studentLevel.Programme.Id && f.Department_Id == studentLevel.Department.Id).FirstOrDefault();
                        if (isExist == null)
                            continue;
                        StudentReport studentReport = new StudentReport();
                        studentReport.Fullname = studentLevel.Student.LastName + " " + studentLevel.Student.FirstName + " " + studentLevel.Student.OtherName;
                        studentReport.RegistrationNumber = studentLevel.Student.MatricNumber;
                        studentReport.Address = studentLevel.Student.HomeAddress;
                        studentReport.Othernames = studentLevel.Student.FirstName + "  " + studentLevel.Student.OtherName;
                        studentReport.Lastname = studentLevel.Student.LastName;
                        studentReport.ProgrammeName = programme.Name;
                        studentReport.DepartmentName = department.Name;
                        studentReport.ProgrammeId = programme.Id;
                        studentReport.LevelName = studentLevel.Level.Name;
                        studentReport.SessionName = sessionLogic.GetModelsBy(f => f.Session_Id == session.Id).Select(f=>f.Name).FirstOrDefault();
                        if (studentLevel.Student.Sex != null)
                        {
                            studentReport.Sex = studentLevel.Student.Sex.Name;
                        }
                        else
                        {
                            studentReport.Sex = "";
                        }
                        if (studentLevel.Student.State != null)
                        {
                            studentReport.State = studentLevel.Student.State.Name;
                        }
                        else
                        {
                            studentReport.State = "";
                        }
                        studentReport.MobileNumber = studentLevel.Student.MobilePhone;
                        if (studentLevel.Student.LocalGovernment != null)
                        {
                            studentReport.LocalGovernment = studentLevel.Student.LocalGovernment.Name;
                        }
                        else
                        {
                            studentReport.LocalGovernment = "";
                        }
                        if (studentLevel.Student.BloodGroup?.Id>0)
                        {
                            studentReport.Bloodgroup = studentLevel.Student.BloodGroup.Name;
                        }
                        else
                        {
                            studentReport.Bloodgroup = "";
                        }
                        if (studentLevel.Student.Genotype?.Id > 0)
                        {
                            studentReport.Genotype = studentLevel.Student.Genotype.Name;
                        }
                        else
                        {
                            studentReport.Genotype = "";
                        }
                        if (studentLevel.Student.SignatureFileUrl != null)
                        {
                            studentReport.Signature = appRoot + studentLevel.Student.SignatureFileUrl;
                        }
                        else
                        {
                           studentReport.Signature = appRoot + "/Content/Images/signSample.png";
                        }
                        
                        if (programme.Id == 1)
                        {
                            var applicantJambDetail = applicantJambDetailLogic.GetModelsBy(g => g.Person_Id == studentLevel.Student.Id).FirstOrDefault();
                            studentReport.JambNumber = applicantJambDetail != null ? applicantJambDetail.JambRegistrationNumber : "";
                        }
                        studentReportList.Add(studentReport);
                        //studentReportList.OrderBy(p => p.RegistrationNumber);
                    }
                    return studentReportList.OrderBy(a => a.RegistrationNumber).ToList();
                }
                else
                {
                    return new List<StudentReport>();
                }


            }
            catch (Exception)
            {

                throw;
            }
        }

        public List<StudentReport> FetchRegisteredStudentBy(State state, Programme programme, Session session, Level level)
        {
            try
            {
                List<StudentReport> studentReportList = new List<StudentReport>();
                List<CourseRegistration> courseRegistrationList = new List<CourseRegistration>();
                List<StudentLevel> studentLevelList = new List<StudentLevel>();

                CourseRegistrationLogic courseLogic = new CourseRegistrationLogic();
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                PersonLogic personLogic = new PersonLogic();
                SessionLogic sessionLogic = new SessionLogic();
                ApplicantJambDetailLogic applicantJambDetailLogic = new ApplicantJambDetailLogic();
                ProgrammeLogic programmeLogic = new ProgrammeLogic();
                DepartmentLogic departmentLogic = new DepartmentLogic();
                CourseRegistrationLogic courseRegistrationLogic = new CourseRegistrationLogic();
                if (state != null && programme != null && session != null && level != null)
                {
                    //department = departmentLogic.GetModelsBy(g => g.Department_Id == department.Id).FirstOrDefault();
                    programme = programmeLogic.GetModelsBy(g => g.Programme_Id == programme.Id).FirstOrDefault();
                    studentLevelList = studentLevelLogic.GetModelsBy(c => c.Level_Id == level.Id && c.Programme_Id == programme.Id && c.Session_Id == session.Id && c.STUDENT.PERSON.State_Id == state.Id);
                    string appRoot = ConfigurationManager.AppSettings["AppRoot"];


                    foreach (var studentLevel in studentLevelList)
                    {
                        var isExist = courseRegistrationLogic.GetModelsBy(f => f.Person_Id == studentLevel.Student.Id && f.Programme_Id == studentLevel.Programme.Id && f.STUDENT.PERSON.State_Id == state.Id).FirstOrDefault();
                        if (isExist == null)
                            continue;
                        StudentReport studentReport = new StudentReport();
                        studentReport.Fullname = studentLevel.Student.LastName + " " + studentLevel.Student.FirstName + " " + studentLevel.Student.OtherName;
                        studentReport.RegistrationNumber = studentLevel.Student.MatricNumber;
                        studentReport.Address = studentLevel.Student.HomeAddress;
                        studentReport.Othernames = studentLevel.Student.LastName + " " + studentLevel.Student.FirstName + "  " + studentLevel.Student.OtherName;
                        //studentReport.Lastname = studentLevel.Student.LastName;
                        studentReport.ProgrammeName = programme.Name;
                        studentReport.DepartmentName = studentLevel.Department.Name;
                        studentReport.ProgrammeId = programme.Id;
                        studentReport.LevelName = studentLevel.Level.Name;
                        studentReport.SessionName = sessionLogic.GetModelsBy(f => f.Session_Id == session.Id).Select(f => f.Name).FirstOrDefault();
                        if (studentLevel.Student.Sex != null)
                        {
                            studentReport.Sex = studentLevel.Student.Sex.Name;
                        }
                        else
                        {
                            studentReport.Sex = "";
                        }
                        if (studentLevel.Student.State != null)
                        {
                            studentReport.State = studentLevel.Student.State.Name;
                        }
                        else
                        {
                            studentReport.State = "";
                        }
                        studentReport.MobileNumber = studentLevel.Student.MobilePhone;
                        if (studentLevel.Student.LocalGovernment != null)
                        {
                            studentReport.LocalGovernment = studentLevel.Student.LocalGovernment.Name;
                        }
                        else
                        {
                            studentReport.LocalGovernment = "";
                        }
                        if (studentLevel.Student.BloodGroup?.Id > 0)
                        {
                            studentReport.Bloodgroup = studentLevel.Student.BloodGroup.Name;
                        }
                        else
                        {
                            studentReport.Bloodgroup = "";
                        }
                        if (studentLevel.Student.Genotype?.Id > 0)
                        {
                            studentReport.Genotype = studentLevel.Student.Genotype.Name;
                        }
                        else
                        {
                            studentReport.Genotype = "";
                        }
                        if (studentLevel.Student.SignatureFileUrl != null)
                        {
                            studentReport.Signature = appRoot + studentLevel.Student.SignatureFileUrl;
                        }
                        else
                        {
                            studentReport.Signature = appRoot + "/Content/Images/signSample.png";
                        }

                        if (programme.Id == 1)
                        {
                            var applicantJambDetail = applicantJambDetailLogic.GetModelsBy(g => g.Person_Id == studentLevel.Student.Id).FirstOrDefault();
                            studentReport.JambNumber = applicantJambDetail != null ? applicantJambDetail.JambRegistrationNumber : "";
                        }
                        studentReportList.Add(studentReport);
                        //studentReportList.OrderBy(p => p.RegistrationNumber);
                    }
                    return studentReportList.OrderBy(a => a.DepartmentName).ToList();
                }
                else
                {
                    return new List<StudentReport>();
                }


            }
            catch (Exception)
            {

                throw;
            }
        }

        public List<StudentReport> GetRegisteredStudentByCapacity(Department department, Programme programme, Session session, Level level)
        {
            try
            {
                List<StudentReport> studentReportList = new List<StudentReport>();
                List<CourseRegistration> courseRegistrationList = new List<CourseRegistration>();

                DepartmentCapacityLogic capacityLogic = new DepartmentCapacityLogic();
                CourseRegistrationLogic courseLogic = new CourseRegistrationLogic();
                SessionLogic sessionLogic = new SessionLogic();

                if (department != null && programme != null && session != null && level != null)
                {
                    DepartmentCapacity capacity = capacityLogic.GetModelsBy(c => c.Programme_Id == programme.Id && c.Department_Id == department.Id && c.Session_Id == session.Id && c.Activated).LastOrDefault();
                    if (capacity != null)
                    {
                        if (level.Id == (int)Levels.HNDII || level.Id == (int)Levels.NDII)
                        {
                            session = sessionLogic.GetModelBy(s => s.Session_Id == session.Id);

                            string[] sessionItems = session.Name.Split('/');
                            string sessionNameStr = sessionItems[0];
                            string currentSessionSuffix = sessionNameStr.Substring(2, 2);
                            string yearTwoSessionSuffix = Convert.ToString((Convert.ToInt32(currentSessionSuffix) - 1));

                            currentSessionSuffix = "/" + currentSessionSuffix + "/";
                            yearTwoSessionSuffix = "/" + yearTwoSessionSuffix + "/";

                            List<CourseRegistration> courseRegistrationListAll = new List<CourseRegistration>();
                            List<CourseRegistration> courseRegistrationListFirstAttempt = new List<CourseRegistration>();
                            List<CourseRegistration> courseRegistrationListCarryOver = new List<CourseRegistration>();

                            courseRegistrationListAll = courseLogic.GetModelsBy(c => c.Department_Id == department.Id && c.Level_Id == level.Id && c.Programme_Id == programme.Id &&
                                                                                    c.Session_Id == session.Id).ToList();

                            courseRegistrationListFirstAttempt = courseRegistrationListAll.Where(s => s.Student.MatricNumber.Contains(yearTwoSessionSuffix)).Take(capacity.Capacity).ToList();
                            courseRegistrationListCarryOver = courseRegistrationListAll.Where(s => !s.Student.MatricNumber.Contains(yearTwoSessionSuffix)).ToList();

                            courseRegistrationList.AddRange(courseRegistrationListFirstAttempt);
                            courseRegistrationList.AddRange(courseRegistrationListCarryOver);
                        }
                        else
                        {
                            courseRegistrationList = courseLogic.GetModelsBy(c => c.Department_Id == department.Id && c.Level_Id == level.Id && c.Programme_Id == programme.Id &&
                                                                                    c.Session_Id == session.Id).Take(capacity.Capacity).ToList();
                        }
                    }
                    else
                    {
                        courseRegistrationList = courseLogic.GetModelsBy(c => c.Department_Id == department.Id && c.Level_Id == level.Id && c.Programme_Id == programme.Id && c.Session_Id == session.Id);
                    }
                    
                    foreach (CourseRegistration registration in courseRegistrationList)
                    {
                        StudentReport studentReport = new StudentReport();
                        studentReport.Fullname = registration.Student.LastName + " " + registration.Student.FirstName + " " + registration.Student.OtherName;
                        studentReport.RegistrationNumber = registration.Student.MatricNumber;
                        studentReport.Address = registration.Student.HomeAddress;
                        studentReport.Othernames = registration.Student.FirstName + "  " + registration.Student.OtherName;
                        studentReport.Lastname = registration.Student.LastName;
                        if (registration.Student.Sex != null)
                        {
                            studentReport.Sex = registration.Student.Sex.Name;
                        }
                        else
                        {
                            studentReport.Sex = "";
                        }
                        if (registration.Student.State != null)
                        {
                            studentReport.State = registration.Student.State.Name;
                        }
                        else
                        {
                            studentReport.State = "";
                        }
                        studentReport.MobileNumber = registration.Student.MobilePhone;
                        if (registration.Student.LocalGovernment != null)
                        {
                            studentReport.LocalGovernment = registration.Student.LocalGovernment.Name;
                        }
                        else
                        {
                            studentReport.LocalGovernment = "";
                        }

                        studentReportList.Add(studentReport);
                        //studentReportList.OrderBy(p => p.RegistrationNumber);

                    }
                    return studentReportList.OrderBy(a => a.RegistrationNumber).ToList();
                }
                else
                {
                    return new List<StudentReport>();
                }


            }
            catch (Exception)
            {

                throw;
            }
        }
        private long GetNextStudentNumber(StudentLevel studentLevel)
        {
            try
            {
                List<StudentLevel> studentList = new List<StudentLevel>();
                StudentLevelLogic studentLogic = new StudentLevelLogic();
                studentList = studentLogic.GetModelsBy(s => s.Department_Id == studentLevel.Department.Id && s.Level_Id == studentLevel.Level.Id && s.Programme_Id == studentLevel.Programme.Id && s.Session_Id == studentLevel.Session.Id);
                long studentNumber = (long)studentList.Max(p => p.Student.Number);
                long newNumber = studentNumber + 1;
                return newNumber;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void CheckMatricNumberDuplicate(string matricNumber)
        {
            try
            {
                Student student = new Student();
                StudentLogic studentLogic = new StudentLogic();
                List<StudentLevel> studentList = new List<StudentLevel>();
                List<StudentLevel> studentListSort = new List<StudentLevel>();
                StudentLevel studentLevel = new StudentLevel();
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                studentList = studentLevelLogic.GetModelsBy(x => x.STUDENT.Matric_Number == matricNumber);
                foreach (StudentLevel studentItem in studentList)
                {
                  if (studentList.Count() > 1)
                    {
                        studentList.RemoveAt(0);
                        for (int i = 0; i < studentList.Count(); i++)
                        {
                            studentItem.Student.Number = null;
                            // UpdateMatricNumber(studentListSort[i]);
                            using (TransactionScope scope = new TransactionScope())
                            {
                                CourseRegistrationLogic courseRegistrationLogic = new CourseRegistrationLogic();
                                CourseRegistrationDetailLogic courseRegistrationDetailLogic = new CourseRegistrationDetailLogic();
                                CourseRegistration courseRegistration = courseRegistrationLogic.GetBy(studentItem.Student, studentItem.Level, studentItem.Programme, studentItem.Department, studentItem.Session);
                                if (courseRegistration != null && courseRegistration.Id > 0)
                                {
                                    Expression<Func<STUDENT_COURSE_REGISTRATION_DETAIL, bool>> selector = cr => cr.Student_Course_Registration_Id == courseRegistration.Id;
                                    if (courseRegistrationDetailLogic.Delete(selector))
                                    {
                                        Expression<Func<STUDENT_COURSE_REGISTRATION, bool>> deleteSelector = cr => cr.Student_Course_Registration_Id == courseRegistration.Id;
                                        courseRegistrationLogic.Delete(deleteSelector);

                                    }
                                    else
                                    {
                                        Expression<Func<STUDENT_COURSE_REGISTRATION, bool>> deleteSelector = cr => cr.Student_Course_Registration_Id == courseRegistration.Id;
                                        courseRegistrationLogic.Delete(deleteSelector);
                                        scope.Complete();
                                    }
                                }

                                Expression<Func<STUDENT_LEVEL, bool>> deleteStudentLevelSelector = sl => sl.Person_Id == studentItem.Student.Id;
                                if (studentLevelLogic.Delete(deleteStudentLevelSelector))
                                {
                                    Expression<Func<STUDENT, bool>> deleteStudentSelector = sl => sl.Person_Id == studentItem.Student.Id;
                                    if (studentLogic.Delete(deleteStudentSelector))
                                    {
                                        ApplicantLogic applicantLogic = new ApplicantLogic();
                                        ApplicationFormView applicant = applicantLogic.GetBy(studentItem.Student);
                                        if (applicant != null)
                                        {
                                            bool matricNoAssigned = studentLogic.AssignMatricNumber(applicant);
                                            if (matricNoAssigned)
                                            {
                                                scope.Complete();
                                            }
                                        }
                                        else
                                        {
                                            scope.Complete();
                                        }

                                    }
                                }




                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public bool UpdateMatricNumber(StudentLevel studentLevel)
        {
            try
            {
                StudentMatricNumberAssignment startMatricNo = studentMatricNumberAssignmentLogic.GetBy(studentLevel.Department.Faculty, studentLevel.Department, studentLevel.Programme, studentLevel.Level, studentLevel.Session);
                if (startMatricNo != null)
                {
                    long studentNumber = 0;
                    string matricNumber = "";

                    if (startMatricNo.Used)
                    {
                        string[] matricNoArray = startMatricNo.MatricNoStartFrom.Split('/');
                        studentNumber = GetNextStudentNumber(studentLevel);
                        matricNoArray[matricNoArray.Length - 1] = UtilityLogic.PaddNumber(studentNumber, 4);
                        matricNumber = string.Join("/", matricNoArray);
                    }
                    else
                    {
                        matricNumber = startMatricNo.MatricNoStartFrom;
                        studentNumber = startMatricNo.MatricSerialNoStartFrom;
                        bool markedAsUsed = studentMatricNumberAssignmentLogic.MarkAsUsed(startMatricNo);
                    }


                    studentLevel.Student.Number = studentNumber;
                    studentLevel.Student.MatricNumber = matricNumber;
                    return Modify(studentLevel.Student);


                }
                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void DeleteMatricNumberDuplicate(string matricNumber)
        {
            try
            {
                Student student = new Student();
                StudentLogic studentLogic = new StudentLogic();
                List<StudentLevel> studentList = new List<StudentLevel>();
                List<StudentLevel> studentListSort = new List<StudentLevel>();
                StudentLevel studentLevel = new StudentLevel();
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                studentList = studentLevelLogic.GetModelsBy(x => x.STUDENT.Matric_Number == matricNumber);
                if (studentList != null && studentList.Count > 0)
                {
                    foreach (StudentLevel studentItem in studentList)
                    {

                        using (TransactionScope scope = new TransactionScope())
                        {
                            CourseRegistrationLogic courseRegistrationLogic = new CourseRegistrationLogic();
                            CourseRegistrationDetailLogic courseRegistrationDetailLogic = new CourseRegistrationDetailLogic();
                            CourseRegistration courseRegistration = courseRegistrationLogic.GetBy(studentItem.Student, studentItem.Level, studentItem.Programme, studentItem.Department, studentItem.Session);
                            if (courseRegistration != null && courseRegistration.Id > 0)
                            {
                                Expression<Func<STUDENT_COURSE_REGISTRATION_DETAIL, bool>> selector = cr => cr.Student_Course_Registration_Id == courseRegistration.Id;
                                if (courseRegistrationDetailLogic.Delete(selector))
                                {
                                    Expression<Func<STUDENT_COURSE_REGISTRATION, bool>> deleteSelector = cr => cr.Student_Course_Registration_Id == courseRegistration.Id;
                                    courseRegistrationLogic.Delete(deleteSelector);

                                }
                                else
                                {
                                    Expression<Func<STUDENT_COURSE_REGISTRATION, bool>> deleteSelector = cr => cr.Student_Course_Registration_Id == courseRegistration.Id;
                                    courseRegistrationLogic.Delete(deleteSelector);
                                    scope.Complete();
                                }
                            }

                            Expression<Func<STUDENT_LEVEL, bool>> deleteStudentLevelSelector = sl => sl.Person_Id == studentItem.Student.Id;
                            if (studentLevelLogic.Delete(deleteStudentLevelSelector))
                            {
                                Expression<Func<STUDENT, bool>> deleteStudentSelector = sl => sl.Person_Id == studentItem.Student.Id;
                                if (studentLogic.Delete(deleteStudentSelector))
                                {
                                    Expression<Func<ONLINE_PAYMENT, bool>> deleteOnlinePaymentSelector = sl => sl.PAYMENT.Person_Id == studentItem.Student.Id;
                                    OnlinePaymentLogic onlinePyamentLogic = new OnlinePaymentLogic();
                                    if (onlinePyamentLogic.Delete(deleteOnlinePaymentSelector))
                                    {
                                        Expression<Func<PAYMENT, bool>> deletePaymentSelector = sl => sl.Person_Id == studentItem.Student.Id;
                                        PaymentLogic paymentLogic = new PaymentLogic();
                                        paymentLogic.Delete(deletePaymentSelector);
                                    }


                                    Expression<Func<PERSON, bool>> deletePersonSelector = sl => sl.Person_Id == studentItem.Student.Id;
                                    if (personLogic.Delete(deletePersonSelector))
                                    {
                                        scope.Complete();
                                    }

                                }
                            }




                        }

                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public long legitPersonId { get; set; }
        public void DeleteDuplicateMatricNumber(string matricNumber)
        {
            try
            {
                StudentLogic studentLogic = new StudentLogic();
                List<StudentLevel> studentList = new List<StudentLevel>();
                StudentLevel studentLevel = new StudentLevel();
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                studentList = studentLevelLogic.GetModelsBy(x => x.STUDENT.Matric_Number == matricNumber);
                legitPersonId = GetLegitimateStudent(studentList);
                if (studentList != null && studentList.Count > 0)
                {
                    foreach (StudentLevel studentItem in studentList)
                    {

                        using (TransactionScope scope = new TransactionScope())
                        {
                            CourseRegistrationLogic courseRegistrationLogic = new CourseRegistrationLogic();
                            int currentSessionId = 0;
                            List<CourseRegistration> courseRegistrations = courseRegistrationLogic.GetListBy(studentItem.Student, studentItem.Programme, studentItem.Department);
                            foreach (CourseRegistration courseregistration in courseRegistrations)
                            {
                                if (courseregistration.Session.Id == 1)
                                {
                                    currentSessionId = 1;
                                }
                            }
                            if ((courseRegistrations.Count == 0 || currentSessionId == 1) && studentItem.Student.Id != legitPersonId)
                            {                                
                                if (courseRegistrations.Count != 0)
                                {
                                    List<CourseRegistrationDetail> courseRegistrationDetails = new List<CourseRegistrationDetail>();
                                    CourseRegistrationDetailLogic courseRegistrationDetailLogic = new CourseRegistrationDetailLogic();
                                    CourseRegistration courseReg = courseRegistrationLogic.GetModelBy(p => p.Person_Id == studentItem.Student.Id && p.Session_Id == currentSessionId);
                                    courseRegistrationDetails = courseRegistrationDetailLogic.GetModelsBy(crd => crd.Student_Course_Registration_Id == courseReg.Id);
                                    if (courseRegistrationDetails.Count > 0)
                                    {
                                        foreach (CourseRegistrationDetail courseRegistrationDetail in courseRegistrationDetails)
                                        {
                                            Expression<Func<STUDENT_COURSE_REGISTRATION_DETAIL, bool>> deleteCourseRegistrationDetailSelector = crd => crd.Student_Course_Registration_Id == courseReg.Id;
                                            courseRegistrationDetailLogic.Delete(deleteCourseRegistrationDetailSelector);
                                        }
                                    }

                                    Expression<Func<STUDENT_COURSE_REGISTRATION, bool>> deleteCourseRegistrationSelector = cr => cr.Student_Course_Registration_Id == courseReg.Id;
                                    courseRegistrationLogic.Delete(deleteCourseRegistrationSelector);
                                }

                                //List<STUDENT_LEVEL> studentLevels = studentLevelLogic.GetEntitiesBy(sl => sl.Person_Id == studentItem.Student.Id);
                                //if (studentLevels.Count > 1)
                                //{
                                //    studentLevelLogic.Delete(studentLevels);
                                //}
                              
                                Expression<Func<STUDENT_LEVEL, bool>> deleteStudentLevelSelector = sl => sl.Student_Level_Id == studentItem.Id;

                                if (studentLevelLogic.Delete(deleteStudentLevelSelector))
                                {
                                    CheckStudentSponsor(studentItem);
                                    CheckStudentFinanceInformation(studentItem);
                                    CheckStudentAcademicInformation(studentItem);
                                    CheckStudentResultDetails(studentItem);
                                    CheckStudentNDResult(studentItem);
                                    CheckStudentEmploymentImformation(studentItem);
                                    Expression<Func<STUDENT, bool>> deleteStudentSelector = s => s.Person_Id == studentItem.Student.Id;
                                    if (studentLogic.Delete(deleteStudentSelector))
                                    {

                                        Expression<Func<ONLINE_PAYMENT, bool>> deleteOnlinePaymentSelector = op => op.PAYMENT.Person_Id == studentItem.Student.Id;
                                        PaymentLogic paymentLogic = new PaymentLogic();
                                        PaymentEtranzactLogic paymentEtransactLogic = new PaymentEtranzactLogic();
                                        PaymentEtranzact paymentEtranzact = paymentEtransactLogic.GetModelBy(pe => pe.ONLINE_PAYMENT.PAYMENT.Person_Id == studentItem.Student.Id);
                                        if (paymentEtranzact == null)
                                        {
                                            OnlinePaymentLogic onlinePyamentLogic = new OnlinePaymentLogic();
                                            if (onlinePyamentLogic.Delete(deleteOnlinePaymentSelector))
                                            {
                                                Expression<Func<PAYMENT, bool>> deletePaymentSelector = p => p.Person_Id == studentItem.Student.Id;

                                                paymentLogic.Delete(deletePaymentSelector);
                                            }
                                        }
                                        else
                                        {
                                            List<Payment> payments = new List<Payment>();
                                            payments = paymentLogic.GetModelsBy(p => p.Person_Id == studentItem.Student.Id);
                                            Person person = new Person() { Id = legitPersonId };
                                            foreach (Payment payment in payments)
                                            {
                                                payment.Person = person;
                                                paymentLogic.Modify(payment);
                                            }
                                           
                                        }

                                        StudentExtraYearLogic studentExtraYearLogic = new StudentExtraYearLogic();
                                        Expression<Func<STUDENT_EXTRA_YEAR_SESSION, bool>> extraYearSessionSelector = eys => eys.Person_Id == studentItem.Student.Id;
                                        StudentExtraYearSession studentExtraYearSession = studentExtraYearLogic.GetModelBy(extraYearSessionSelector);
                                        if (studentExtraYearSession != null)
                                        {
                                            studentExtraYearLogic.Delete(extraYearSessionSelector);
                                        }

                                        scope.Complete();

                                    }
                                }
                            }
                            else
                            {
                                legitPersonId = studentItem.Student.Id;
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        private void CheckStudentEmploymentImformation(StudentLevel studentItem)
        {
            try
            {
                StudentEmploymentInformationLogic studentEmploymentInformationLogic = new StudentEmploymentInformationLogic();
                StudentEmploymentInformation studentEmploymentInformation = studentEmploymentInformationLogic.GetModelBy(ss => ss.Person_Id == studentItem.Student.Id);
                if (studentEmploymentInformation != null)
                {
                    Expression<Func<STUDENT_EMPLOYMENT_INFORMATION, bool>> deleteStudentEmploymentInformation = ss => ss.Person_Id == studentItem.Student.Id;
                    studentEmploymentInformationLogic.Delete(deleteStudentEmploymentInformation);
                }
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        private void CheckStudentNDResult(StudentLevel studentItem)
        {
            try
            {
                StudentNdResultLogic studentNDResultLogic = new StudentNdResultLogic();
                StudentNdResult studentNDResult = studentNDResultLogic.GetModelBy(ss => ss.Person_Id == studentItem.Student.Id);
                if (studentNDResult != null)
                {
                    Expression<Func<STUDENT_ND_RESULT, bool>> deleteStudentNDResult = ss => ss.Person_Id == studentItem.Student.Id;
                    studentNDResultLogic.Delete(deleteStudentNDResult);
                }
            }
            catch (Exception)
            {
                
                throw;
            }


        }

        private long GetLegitimateStudent(List<StudentLevel> studentList)
        {
            long personId = 0;
            try
            {
                foreach (StudentLevel studentItem in studentList)
                {
                
                    CourseRegistrationLogic courseRegistrationLogic = new CourseRegistrationLogic();
           
                    List<CourseRegistration> courseRegistrations = courseRegistrationLogic.GetListBy(studentItem.Student, studentItem.Programme, studentItem.Department);
                    foreach (CourseRegistration courseregistration in courseRegistrations)
                    {
                        if (courseRegistrations.Count >= 1 && courseregistration.Session.Id != 1 )
                        {
                            personId = courseregistration.Student.Id;
                        }
                        else if (courseRegistrations.Count == 1 && personId == 0)
                        {
                            personId = courseregistration.Student.Id;
                        }                            
                    }                    
                }
                if (personId == 0)
                {
                    personId = studentList.Where(s => s.Session.Id == 1).FirstOrDefault().Student.Id;
                }
            }
            catch (Exception)
            {                
                throw;
            }
            return personId;
        }

        private static void CheckStudentSponsor(StudentLevel studentItem)
        {
            StudentSponsorLogic studentSponsorLogic = new StudentSponsorLogic();
            StudentSponsor studentSponsor = studentSponsorLogic.GetModelBy(ss => ss.Person_Id == studentItem.Student.Id);
            if (studentSponsor != null)
            {
                Expression<Func<STUDENT_SPONSOR, bool>> deleteStudentSponsorSelector = ss => ss.Person_Id == studentItem.Student.Id;
                studentSponsorLogic.Delete(deleteStudentSponsorSelector);
            }
        }
        private static void CheckStudentFinanceInformation(StudentLevel studentItem)
        {
            StudentFinanceInformationLogic studentFinanceInformationLogic = new StudentFinanceInformationLogic();
            StudentFinanceInformation studentFinanceInformation = studentFinanceInformationLogic.GetModelBy(ss => ss.Person_Id == studentItem.Student.Id);
            if (studentFinanceInformation != null)
            {
                Expression<Func<STUDENT_FINANCE_INFORMATION, bool>> deleteStudentFinanceInfoSelector = sfi => sfi.Person_Id == studentItem.Student.Id;
                studentFinanceInformationLogic.Delete(deleteStudentFinanceInfoSelector);
            }
        }
        private static void CheckStudentAcademicInformation(StudentLevel studentItem)
        {
            StudentAcademicInformationLogic studentAcademicInformationLogic = new StudentAcademicInformationLogic();
            StudentAcademicInformation studentAcademicInformation = studentAcademicInformationLogic.GetModelBy(ss => ss.Person_Id == studentItem.Student.Id);
            if (studentAcademicInformation != null)
            {
                Expression<Func<STUDENT_ACADEMIC_INFORMATION, bool>> deleteStudentAcademicInfoSelector = sai => sai.Person_Id == studentItem.Student.Id;
                studentAcademicInformationLogic.Delete(deleteStudentAcademicInfoSelector);
            }
        }
        private static void CheckStudentResultDetails(StudentLevel studentItem)
        {
            StudentResultDetailLogic studentResultDetailLogic = new StudentResultDetailLogic();
            StudentResultDetail studentResultDetail = studentResultDetailLogic.GetModelBy(srd => srd.Person_Id == studentItem.Student.Id);
            if (studentResultDetail != null)
            {
                Expression<Func<STUDENT_RESULT_DETAIL, bool>> deleteStudentResultDetailSelector = srd => srd.Person_Id == studentItem.Student.Id;
                studentResultDetailLogic.Delete(deleteStudentResultDetailSelector);
            }
        }

        public List<StudentDetailsModel> GetStudentDetails(Programme programme, Department department, DepartmentOption option, Level level, Session session)
        {
            try
            {
                if (level == null || level.Id <= 0 || programme == null || programme.Id <= 0 || department == null || department.Id <= 0 || session == null || session.Id <= 0)
                {
                    throw new Exception("One or more criteria to get this report was not set! Please check your input criteria selection and try again.");
                }

                List<StudentDetailsModel> details = new List<StudentDetailsModel>();

                if (option != null && option.Id >= 0)
                {
                    details = (from sd in repository.GetBy<VW_STUDENT_DETAILS>(x => x.Level_Id == level.Id && x.Programme_Id == programme.Id && x.Department_Id == department.Id && x.Department_Option_Id == option.Id && x.Session_Id == session.Id)
                                                         let dateOfBirth = sd.Date_Of_Birth
                                                         where dateOfBirth != null
                                                         select new StudentDetailsModel
                                                         {
                                                             PersonId = sd.Person_Id,
                                                             BloodGroup = sd.Blood_Group_Name,
                                                             ContactAddress = sd.Contact_Address,
                                                             DateOfBirth = dateOfBirth.Value.ToLongDateString(),
                                                             DepartmentCode = sd.Department_Code,
                                                             DepartmentName = sd.Department_Name,
                                                             DepartmentOptionName = sd.Department_Option_Name,
                                                             Name = sd.Name,
                                                             Email = sd.Email,
                                                             FacultyName = sd.Faculty_Name,
                                                             ImageFileUrl = sd.Image_File_Url,
                                                             SchoolContactAddress = sd.School_Contact_Address,
                                                             SessionName = sd.Session_Name,
                                                             SexName = sd.Sex_Name,
                                                             StateName = sd.State_Name,
                                                             MatricNumber = sd.Matric_Number,
                                                             MobilePhone = sd.Mobile_Phone,
                                                             ProgrammeName = sd.Programme_Name,
                                                             ProgrammeShortName = sd.Programme_Short_Name,
                                                             LevelName = sd.Level_Name,
                                                             LocalGovernmentName = sd.Local_Government_Name,
                                                             NationalityName = sd.Nationality_Name,
                                                             HomeAddress = sd.Home_Address,
                                                             HomeTown = sd.Home_Town,
                                                             ReligionName = sd.Religion_Name,
                                                             Genotype = sd.Genotype_Name
                                                         }).ToList();
                }
                else
                {
                    details = (from sd in repository.GetBy<VW_STUDENT_DETAILS>(x => x.Level_Id == level.Id && x.Programme_Id == programme.Id && x.Department_Id == department.Id && x.Session_Id == session.Id)
                                                         let dateOfBirth = sd.Date_Of_Birth
                                                         where dateOfBirth != null
                                                         select new StudentDetailsModel
                                                         {
                                                             PersonId = sd.Person_Id,
                                                             BloodGroup = sd.Blood_Group_Name,
                                                             ContactAddress = sd.Contact_Address,
                                                             DateOfBirth = dateOfBirth.Value.ToLongDateString(),
                                                             DepartmentCode = sd.Department_Code,
                                                             DepartmentName = sd.Department_Name,
                                                             DepartmentOptionName = sd.Department_Option_Name,
                                                             Name = sd.Name,
                                                             Email = sd.Email,
                                                             FacultyName = sd.Faculty_Name,
                                                             ImageFileUrl = sd.Image_File_Url,
                                                             SchoolContactAddress = sd.School_Contact_Address,
                                                             SessionName = sd.Session_Name,
                                                             SexName = sd.Sex_Name,
                                                             StateName = sd.State_Name,
                                                             MatricNumber = sd.Matric_Number,
                                                             MobilePhone = sd.Mobile_Phone,
                                                             ProgrammeName = sd.Programme_Name,
                                                             ProgrammeShortName = sd.Programme_Short_Name,
                                                             LevelName = sd.Level_Name,
                                                             LocalGovernmentName = sd.Local_Government_Name,
                                                             NationalityName = sd.Nationality_Name,
                                                             HomeAddress = sd.Home_Address,
                                                             HomeTown = sd.Home_Town,
                                                             ReligionName = sd.Religion_Name,
                                                             Genotype = sd.Genotype_Name
                                                         }).ToList();
                }
                

                List<long> distinctPersonId = details.Select(d => d.PersonId).Distinct().ToList();
                List<StudentDetailsModel> masterDetails = new List<StudentDetailsModel>();

                for (int i = 0; i < distinctPersonId.Count; i++)
                {
                    long currentId = distinctPersonId[i];
                    masterDetails.Add(details.LastOrDefault(d => d.PersonId == currentId));
                }

                return masterDetails.OrderBy(d => d.MatricNumber).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<StudentDetailFormat> GetOldStudentCount(Session session)
        {
            try
            {
                List<StudentDetailFormat> ndIHndIStudents = new List<StudentDetailFormat>();
                List<StudentDetailFormat> ndIIHndIIStudents = new List<StudentDetailFormat>();
                List<StudentDetailFormat> masterList = new List<StudentDetailFormat>();
                List<StudentDetailFormat> newStudents = new List<StudentDetailFormat>();
                List<StudentDetailFormat> returningStudents = new List<StudentDetailFormat>();

                SessionLogic sessionLogic = new SessionLogic();
                Session currentSession = sessionLogic.GetModelsBy(s => s.Session_Id == session.Id).LastOrDefault();

                int[] ndProgrammes = { 1, 2 };
                int[] hndProgrammes = { 3, 4 };
                string[] ndProgrammeNames = { "ND Full Time", "ND Part Time" };
                string[] hndProgrammeNames = { "HND Full Time", "HND Part Time" };

                //if (currentSession != null)
                //{
                //ndIHndIStudents = (from sr in repository.GetBy<VW_NEW_STUDENT>(p => p.Admitted_Session_Id == session.Id && p.Confirmation_No != null)
                //                   select new StudentDetailFormat
                //                   {
                //                       PersonId = sr.Person_Id,
                //                       Name = sr.Name,
                //                       MatricNumber = sr.Application_Form_Number,
                //                       Programme = sr.Admitted_Programme,
                //                       Department = sr.Admitted_Department,
                //                       ProgrammeId = sr.Admitted_Programme_Id,
                //                       DepartmentId = sr.Admitted_Department_Id,
                //                       Session = sr.Admitted_Session
                //                   }).ToList();

                //List<long> personIdList = ndIHndIStudents.Select(p => p.PersonId).Distinct().ToList();

                int NewStudentTotalCountND = 0;
                int NewStudentTotalCountHND = 0;
                int ReturningStudentTotalCountND = 0;
                int ReturningStudentTotalCountHND = 0;

                //for (int i = 0; i < personIdList.Count; i++)
                //{
                //    StudentDetailFormat currentFormat = ndIHndIStudents.LastOrDefault(p => p.PersonId == personIdList[i]);
                //    if (ndProgrammes.Contains(currentFormat.ProgrammeId) && ndProgrammeNames.Contains(currentFormat.Programme))
                //    {
                //        currentFormat.Programme = currentFormat.Programme + " ND I";
                //        NewStudentTotalCountND += 1;
                //    }
                //    else if (hndProgrammes.Contains(currentFormat.ProgrammeId) && hndProgrammeNames.Contains(currentFormat.Programme))
                //    {
                //        currentFormat.Programme = currentFormat.Programme + " HND I";
                //        NewStudentTotalCountHND += 1;
                //    }
                //    else
                //    {
                //        continue;
                //    }

                //    currentFormat.Session = currentSession.Name;
                //    currentFormat.Count = 1;
                //    newStudents.Add(currentFormat);
                //}

                int totalNewStudents = newStudents.Count;

                if (session.Id == (int)Sessions._20162017)
                {
                    ndIIHndIIStudents = (from sr in repository.GetBy<VW_OLD_STUDENT>(x => x.Session_Id == currentSession.Id && (x.Admitted_Session_Id != null && x.Admitted_Session_Id != session.Id))
                                         select new StudentDetailFormat
                                         {
                                             PersonId = sr.Person_Id,
                                             Name = sr.Name,
                                             MatricNumber = sr.Matric_Number,
                                             Programme = sr.Programme_Name,
                                             Department = sr.Department_Name,
                                             ProgrammeId = sr.Programme_Id,
                                             DepartmentId = sr.Department_Id,
                                             Session = sr.Session_Name,
                                             AdmittedSession = sr.Admitted_Session,
                                             GraduationYear = sr.Year_Of_Graduation ?? 0
                                         }).ToList();
                }
                else
                {
                    ndIIHndIIStudents = (from sr in repository.GetBy<VW_OLD_STUDENT>(x => x.Session_Id == currentSession.Id && x.Admitted_Session_Id == null)
                                         select new StudentDetailFormat
                                         {
                                             PersonId = sr.Person_Id,
                                             Name = sr.Name,
                                             MatricNumber = sr.Matric_Number,
                                             Programme = sr.Programme_Name,
                                             Department = sr.Department_Name,
                                             ProgrammeId = sr.Programme_Id,
                                             DepartmentId = sr.Department_Id,
                                             Session = sr.Session_Name,
                                             AdmittedSession = sr.Admitted_Session,
                                             GraduationYear = sr.Year_Of_Graduation ?? 0
                                         }).ToList();
                }


                List<long> personIdListH = ndIIHndIIStudents.Select(p => p.PersonId).Distinct().ToList();
                for (int i = 0; i < personIdListH.Count; i++)
                {
                    StudentDetailFormat currentFormat = ndIIHndIIStudents.LastOrDefault(p => p.PersonId == personIdListH[i]);

                    if (ndProgrammes.Contains(currentFormat.ProgrammeId) && ndProgrammeNames.Contains(currentFormat.Programme))
                    {
                        currentFormat.Programme = currentFormat.Programme + " ND II";
                        ReturningStudentTotalCountND += 1;
                    }
                    else if (hndProgrammes.Contains(currentFormat.ProgrammeId) && hndProgrammeNames.Contains(currentFormat.Programme))
                    {
                        currentFormat.Programme = currentFormat.Programme + " HND II";
                        ReturningStudentTotalCountHND += 1;
                    }
                    else
                    {
                        continue;
                    }

                    currentFormat.Session = currentSession.Name;
                    currentFormat.Count = 1;
                    returningStudents.Add(currentFormat);

                }

                int totalReturningStudents = returningStudents.Count;

                masterList.AddRange(newStudents);
                masterList.AddRange(returningStudents);

                int totalCount = masterList.Count;

                for (int i = 0; i < masterList.Count; i++)
                {
                    masterList[i].NewStudentTotalCount = totalNewStudents;
                    masterList[i].ReturningStudentTotalCount = totalReturningStudents;
                    masterList[i].TotalCount = totalCount;
                    masterList[i].NewStudentTotalCountHND = NewStudentTotalCountHND;
                    masterList[i].NewStudentTotalCountND = NewStudentTotalCountND;
                    masterList[i].ReturningStudentTotalCountHND = ReturningStudentTotalCountHND;
                    masterList[i].ReturningStudentTotalCountND = ReturningStudentTotalCountND;
                }
                //}

                return masterList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<StudentDetailFormat> GetStudentCount(Session session)
        {
            try
            {
                List<StudentDetailFormat> ndIHndIStudents = new List<StudentDetailFormat>();
                List<StudentDetailFormat> ndIIHndIIStudents = new List<StudentDetailFormat>();
                List<StudentDetailFormat> masterList = new List<StudentDetailFormat>();
                List<StudentDetailFormat> newStudents = new List<StudentDetailFormat>();
                List<StudentDetailFormat> returningStudents = new List<StudentDetailFormat>();

                SessionLogic sessionLogic = new SessionLogic();
                Session currentSession = sessionLogic.GetModelsBy(s => s.Session_Id == session.Id).LastOrDefault();

                int lstSessionYear = Convert.ToInt32(currentSession.Name.Split('/').FirstOrDefault());
                string lstSessionName = (lstSessionYear - 1) + "/" + lstSessionYear;

                Session lastSession = sessionLogic.GetModelBy(s => s.Session_Name.Trim() == lstSessionName.Trim());

                int[] ndProgrammes = { 1, 2 };
                int[] hndProgrammes = { 3, 4 };
                string[] ndProgrammeNames = { "ND Full Time", "ND Part Time" };
                string[] hndProgrammeNames = { "HND Full Time", "HND Part Time" };

                //if (currentSession != null)
                //{
                ndIHndIStudents = (from sr in repository.GetBy<VW_NEW_STUDENT>(p => p.Admitted_Session_Id == session.Id && p.Confirmation_No != null)
                                   select new StudentDetailFormat
                                   {
                                       PersonId = sr.Person_Id,
                                       Name = sr.Name,
                                       MatricNumber = sr.Application_Form_Number,
                                       Programme = sr.Admitted_Programme,
                                       Department = sr.Admitted_Department,
                                       ProgrammeId = sr.Admitted_Programme_Id,
                                       DepartmentId = sr.Admitted_Department_Id,
                                       Session = sr.Admitted_Session
                                   }).ToList();

                List<long> personIdList = ndIHndIStudents.Select(p => p.PersonId).Distinct().ToList();

                int NewStudentTotalCountND = 0;
                int NewStudentTotalCountHND = 0;
                int NewStudentTotalCountOthers = 0;
                int ReturningStudentTotalCountND = 0;
                int ReturningStudentTotalCountHND = 0;
                int ReturningStudentTotalCountOthers = 0;

                for (int i = 0; i < personIdList.Count; i++)
                {
                    StudentDetailFormat currentFormat = ndIHndIStudents.LastOrDefault(p => p.PersonId == personIdList[i]);
                    if (ndProgrammes.Contains(currentFormat.ProgrammeId) && ndProgrammeNames.Contains(currentFormat.Programme))
                    {
                        currentFormat.Programme = currentFormat.Programme + " ND I";
                        NewStudentTotalCountND += 1;
                    }
                    else if (hndProgrammes.Contains(currentFormat.ProgrammeId) && hndProgrammeNames.Contains(currentFormat.Programme))
                    {
                        currentFormat.Programme = currentFormat.Programme + " HND I";
                        NewStudentTotalCountHND += 1;
                    }
                    else
                    {
                        continue;
                    }

                    currentFormat.Session = currentSession.Name;
                    currentFormat.Count = 1;
                    newStudents.Add(currentFormat);
                }

                int totalNewStudents = newStudents.Count;

                //if (session.Id == (int)Sessions._20162017)
                //{
                //    ndIIHndIIStudents = (from sr in repository.GetBy<VW_OLD_STUDENT>(x => x.Session_Id == currentSession.Id && x.Confirmation_No != null && (x.Admitted_Session_Id != null && x.Admitted_Session_Id != session.Id))
                //                         select new StudentDetailFormat
                //                         {
                //                             PersonId = sr.Person_Id,
                //                             Name = sr.Name,
                //                             MatricNumber = sr.Matric_Number,
                //                             Programme = sr.Programme_Name,
                //                             Department = sr.Department_Name,
                //                             ProgrammeId = sr.Programme_Id,
                //                             DepartmentId = sr.Department_Id,
                //                             Session = sr.Session_Name,
                //                             AdmittedSession = sr.Admitted_Session,
                //                             GraduationYear = sr.Year_Of_Graduation ?? 0
                //                         }).ToList();
                //}
                //else
                //{
                //    ndIIHndIIStudents = (from sr in repository.GetBy<VW_OLD_STUDENT>(x => x.Session_Id == currentSession.Id && x.Confirmation_No != null && x.Admitted_Session_Id == null)
                //                         select new StudentDetailFormat
                //                         {
                //                             PersonId = sr.Person_Id,
                //                             Name = sr.Name,
                //                             MatricNumber = sr.Matric_Number,
                //                             Programme = sr.Programme_Name,
                //                             Department = sr.Department_Name,
                //                             ProgrammeId = sr.Programme_Id,
                //                             DepartmentId = sr.Department_Id,
                //                             Session = sr.Session_Name,
                //                             AdmittedSession = sr.Admitted_Session,
                //                             GraduationYear = sr.Year_Of_Graduation ?? 0
                //                         }).ToList();
                //}

                int[] otherYears = { 3, 4, 5, 6, 7 };

                if (otherYears.Contains(lastSession.Id))
                {
                    ndIIHndIIStudents = (from sr in repository.GetBy<VW_OLD_STUDENT>(x => x.Session_Id == currentSession.Id && x.Confirmation_No != null && (x.Admitted_Session_Id == lastSession.Id || x.Admitted_Session_Id == null))
                                         select new StudentDetailFormat
                                         {
                                             PersonId = sr.Person_Id,
                                             Name = sr.Name,
                                             MatricNumber = sr.Matric_Number,
                                             Programme = sr.Programme_Name,
                                             Department = sr.Department_Name,
                                             ProgrammeId = sr.Programme_Id,
                                             DepartmentId = sr.Department_Id,
                                             Session = sr.Session_Name,
                                             AdmittedSession = sr.Admitted_Session,
                                             GraduationYear = sr.Year_Of_Graduation ?? 0
                                         }).ToList();
                }
                else
                {
                    ndIIHndIIStudents = (from sr in repository.GetBy<VW_NEW_STUDENT>(p => p.Admitted_Session_Id == lastSession.Id && p.Confirmation_No != null)
                                         select new StudentDetailFormat
                                         {
                                             PersonId = sr.Person_Id,
                                             Name = sr.Name,
                                             MatricNumber = sr.Application_Form_Number,
                                             Programme = sr.Admitted_Programme,
                                             Department = sr.Admitted_Department,
                                             ProgrammeId = sr.Admitted_Programme_Id,
                                             DepartmentId = sr.Admitted_Department_Id,
                                             Session = sr.Admitted_Session
                                         }).ToList();
                }

                //PaymentEtranzactLogic etranzactLogic = new PaymentEtranzactLogic();
                //List<PAYMENT_ETRANZACT> etranzact = etranzactLogic.GetEntitiesBy(e => e.ONLINE_PAYMENT.PAYMENT.Fee_Type_Id == (int)FeeTypes.SchoolFees && e.ONLINE_PAYMENT.PAYMENT.Session_Id == session.Id);

                //List<long> etranzactPins = etranzact.Select(e => e.ONLINE_PAYMENT.PAYMENT.Person_Id).ToList();

                List<long> etranzactPins = repository.GetBy<VW_CONFIRMED_PAYMENTS>(p => p.Session_Id == session.Id).Select(p => p.Person_Id).ToList();


                List<long> personIdListH = ndIIHndIIStudents.Select(p => p.PersonId).Distinct().ToList();
                for (int i = 0; i < personIdListH.Count; i++)
                {
                    StudentDetailFormat currentFormat = ndIIHndIIStudents.LastOrDefault(p => p.PersonId == personIdListH[i]);

                    if (!etranzactPins.Contains(currentFormat.PersonId))
                    {
                        continue;
                    }

                    if (ndProgrammes.Contains(currentFormat.ProgrammeId) && ndProgrammeNames.Contains(currentFormat.Programme))
                    {
                        currentFormat.Programme = currentFormat.Programme + " ND II";
                        ReturningStudentTotalCountND += 1;
                    }
                    else if (hndProgrammes.Contains(currentFormat.ProgrammeId) && hndProgrammeNames.Contains(currentFormat.Programme))
                    {
                        currentFormat.Programme = currentFormat.Programme + " HND II";
                        ReturningStudentTotalCountHND += 1;
                    }
                    else
                    {
                        continue;
                    }

                    currentFormat.Session = currentSession.Name;
                    currentFormat.Count = 1;
                    returningStudents.Add(currentFormat);

                }

                int totalReturningStudents = returningStudents.Count;

                masterList.AddRange(newStudents);
                masterList.AddRange(returningStudents);

                int totalCount = masterList.Count;

                for (int i = 0; i < masterList.Count; i++)
                {
                    masterList[i].NewStudentTotalCount = totalNewStudents;
                    masterList[i].ReturningStudentTotalCount = totalReturningStudents;
                    masterList[i].TotalCount = totalCount;
                    masterList[i].NewStudentTotalCountHND = NewStudentTotalCountHND;
                    masterList[i].NewStudentTotalCountND = NewStudentTotalCountND;
                    masterList[i].ReturningStudentTotalCountHND = ReturningStudentTotalCountHND;
                    masterList[i].ReturningStudentTotalCountND = ReturningStudentTotalCountND;
                    masterList[i].OtherStudentTotalCount = NewStudentTotalCountOthers + ReturningStudentTotalCountOthers;
                }
                //}

                return masterList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<StudentDetailFormat> GetStudentCountByAdmission(Session session)
        {
            try
            {
                List<StudentDetailFormat> ndIHndIStudents = new List<StudentDetailFormat>();
                List<StudentDetailFormat> ndIIHndIIStudents = new List<StudentDetailFormat>();
                List<StudentDetailFormat> masterList = new List<StudentDetailFormat>();
                List<StudentDetailFormat> newStudents = new List<StudentDetailFormat>();
                List<StudentDetailFormat> returningStudents = new List<StudentDetailFormat>();

                SessionLogic sessionLogic = new SessionLogic();
                Session currentSession = sessionLogic.GetModelsBy(s => s.Session_Id == session.Id).LastOrDefault();

                int lstSessionYear = Convert.ToInt32(currentSession.Name.Split('/').FirstOrDefault());
                string lstSessionName = (lstSessionYear - 1) + "/" + lstSessionYear;

                Session lastSession = sessionLogic.GetModelBy(s => s.Session_Name.Trim() == lstSessionName.Trim());

                int[] ndProgrammes = { 1, 2 };
                int[] hndProgrammes = { 3, 4 };
                string[] ndProgrammeNames = { "ND Full Time", "ND Part Time" };
                string[] hndProgrammeNames = { "HND Full Time", "HND Part Time" };

                //if (currentSession != null)
                //{
                ndIHndIStudents = (from sr in repository.GetBy<VW_ADMISSION_LIST>(p => p.Session_Id == session.Id)
                                   select new StudentDetailFormat
                                   {
                                       PersonId = sr.Person_Id,
                                       Name = sr.FullName,
                                       MatricNumber = sr.Application_Form_Number,
                                       Programme = sr.Programme_Name,
                                       Department = sr.Department_Name,
                                       ProgrammeId = sr.Programme_Id,
                                       DepartmentId = sr.Department_Id,
                                       Session = sr.Session_Name
                                   }).ToList();

                List<long> personIdList = ndIHndIStudents.Select(p => p.PersonId).Distinct().ToList();

                int NewStudentTotalCountND = 0;
                int NewStudentTotalCountHND = 0;
                int ReturningStudentTotalCountND = 0;
                int ReturningStudentTotalCountHND = 0;
                int NewStudentTotalCountOthers = 0;
                int ReturningStudentTotalCountOthers = 0;

                for (int i = 0; i < personIdList.Count; i++)
                {
                    StudentDetailFormat currentFormat = ndIHndIStudents.LastOrDefault(p => p.PersonId == personIdList[i]);
                    if (ndProgrammes.Contains(currentFormat.ProgrammeId) && ndProgrammeNames.Contains(currentFormat.Programme))
                    {
                        currentFormat.Programme = currentFormat.Programme + " ND I";
                        NewStudentTotalCountND += 1;
                    }
                    else if (hndProgrammes.Contains(currentFormat.ProgrammeId) && hndProgrammeNames.Contains(currentFormat.Programme))
                    {
                        currentFormat.Programme = currentFormat.Programme + " HND I";
                        NewStudentTotalCountHND += 1;
                    }
                   else
                    {
                        continue;
                    }

                    currentFormat.Session = currentSession.Name;
                    currentFormat.Count = 1;
                    newStudents.Add(currentFormat);
                }

                int totalNewStudents = newStudents.Count;

                //if (session.Id == (int)Sessions._20162017)
                //{
                //    ndIIHndIIStudents = (from sr in repository.GetBy<VW_OLD_STUDENT>(x => x.Session_Id == currentSession.Id && x.Confirmation_No != null && (x.Admitted_Session_Id != null && x.Admitted_Session_Id != session.Id))
                //                         select new StudentDetailFormat
                //                         {
                //                             PersonId = sr.Person_Id,
                //                             Name = sr.Name,
                //                             MatricNumber = sr.Matric_Number,
                //                             Programme = sr.Programme_Name,
                //                             Department = sr.Department_Name,
                //                             ProgrammeId = sr.Programme_Id,
                //                             DepartmentId = sr.Department_Id,
                //                             Session = sr.Session_Name,
                //                             AdmittedSession = sr.Admitted_Session,
                //                             GraduationYear = sr.Year_Of_Graduation ?? 0
                //                         }).ToList();
                //}
                //else
                //{
                //    ndIIHndIIStudents = (from sr in repository.GetBy<VW_OLD_STUDENT>(x => x.Session_Id == currentSession.Id && x.Confirmation_No != null && x.Admitted_Session_Id == null)
                //                         select new StudentDetailFormat
                //                         {
                //                             PersonId = sr.Person_Id,
                //                             Name = sr.Name,
                //                             MatricNumber = sr.Matric_Number,
                //                             Programme = sr.Programme_Name,
                //                             Department = sr.Department_Name,
                //                             ProgrammeId = sr.Programme_Id,
                //                             DepartmentId = sr.Department_Id,
                //                             Session = sr.Session_Name,
                //                             AdmittedSession = sr.Admitted_Session,
                //                             GraduationYear = sr.Year_Of_Graduation ?? 0
                //                         }).ToList();
                //}

                int[] otherYears = { 3, 4, 5, 6, 7 };

                if (otherYears.Contains(lastSession.Id))
                {
                    ndIIHndIIStudents = (from sr in repository.GetBy<VW_OLD_STUDENT>(x => x.Session_Id == currentSession.Id && x.Confirmation_No != null && (x.Admitted_Session_Id == lastSession.Id || x.Admitted_Session_Id == null))
                                         select new StudentDetailFormat
                                         {
                                             PersonId = sr.Person_Id,
                                             Name = sr.Name,
                                             MatricNumber = sr.Matric_Number,
                                             Programme = sr.Programme_Name,
                                             Department = sr.Department_Name,
                                             ProgrammeId = sr.Programme_Id,
                                             DepartmentId = sr.Department_Id,
                                             Session = sr.Session_Name,
                                             AdmittedSession = sr.Admitted_Session,
                                             GraduationYear = sr.Year_Of_Graduation ?? 0
                                         }).ToList();
                }
                else
                {
                    ndIIHndIIStudents = (from sr in repository.GetBy<VW_NEW_STUDENT>(p => p.Admitted_Session_Id == lastSession.Id && p.Confirmation_No != null)
                                         select new StudentDetailFormat
                                         {
                                             PersonId = sr.Person_Id,
                                             Name = sr.Name,
                                             MatricNumber = sr.Application_Form_Number,
                                             Programme = sr.Admitted_Programme,
                                             Department = sr.Admitted_Department,
                                             ProgrammeId = sr.Admitted_Programme_Id,
                                             DepartmentId = sr.Admitted_Department_Id,
                                             Session = sr.Admitted_Session
                                         }).ToList();
                }

                List<long> personIdListH = ndIIHndIIStudents.Select(p => p.PersonId).Distinct().ToList();
                for (int i = 0; i < personIdListH.Count; i++)
                {
                    StudentDetailFormat currentFormat = ndIIHndIIStudents.LastOrDefault(p => p.PersonId == personIdListH[i]);

                    if (ndProgrammes.Contains(currentFormat.ProgrammeId) && ndProgrammeNames.Contains(currentFormat.Programme))
                    {
                        currentFormat.Programme = currentFormat.Programme + " ND II";
                        ReturningStudentTotalCountND += 1;
                    }
                    else if (hndProgrammes.Contains(currentFormat.ProgrammeId) && hndProgrammeNames.Contains(currentFormat.Programme))
                    {
                        currentFormat.Programme = currentFormat.Programme + " HND II";
                        ReturningStudentTotalCountHND += 1;
                    }
                    else
                    {
                        continue;
                    }

                    currentFormat.Session = currentSession.Name;
                    currentFormat.Count = 1;
                    returningStudents.Add(currentFormat);

                }

                int totalReturningStudents = returningStudents.Count;

                masterList.AddRange(newStudents);
                masterList.AddRange(returningStudents);

                int totalCount = masterList.Count;

                for (int i = 0; i < masterList.Count; i++)
                {
                    masterList[i].NewStudentTotalCount = totalNewStudents;
                    masterList[i].ReturningStudentTotalCount = totalReturningStudents;
                    masterList[i].TotalCount = totalCount;
                    masterList[i].NewStudentTotalCountHND = NewStudentTotalCountHND;
                    masterList[i].NewStudentTotalCountND = NewStudentTotalCountND;
                    masterList[i].ReturningStudentTotalCountHND = ReturningStudentTotalCountHND;
                    masterList[i].ReturningStudentTotalCountND = ReturningStudentTotalCountND;
                    masterList[i].OtherStudentTotalCount = NewStudentTotalCountOthers + ReturningStudentTotalCountOthers;
                }
                //}

                return masterList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<StudentDetailFormat> GetStudentCountByAcceptance(Session session)
        {
            try
            {
                List<StudentDetailFormat> ndIHndIStudents = new List<StudentDetailFormat>();
                List<StudentDetailFormat> ndIIHndIIStudents = new List<StudentDetailFormat>();
                List<StudentDetailFormat> masterList = new List<StudentDetailFormat>();
                List<StudentDetailFormat> newStudents = new List<StudentDetailFormat>();
                List<StudentDetailFormat> returningStudents = new List<StudentDetailFormat>();

                SessionLogic sessionLogic = new SessionLogic();
                Session currentSession = sessionLogic.GetModelsBy(s => s.Session_Id == session.Id).LastOrDefault();

                int lstSessionYear = Convert.ToInt32(currentSession.Name.Split('/').FirstOrDefault());
                string lstSessionName = (lstSessionYear - 1) + "/" + lstSessionYear;

                Session lastSession = sessionLogic.GetModelBy(s => s.Session_Name.Trim() == lstSessionName.Trim());

                int[] ndProgrammes = { 1, 2 };
                int[] hndProgrammes = { 3, 4 };
                string[] ndProgrammeNames = { "ND Full Time", "ND Part Time" };
                string[] hndProgrammeNames = { "HND Full Time", "HND Part Time" };

                //if (currentSession != null)
                //{
                ndIHndIStudents = (from sr in repository.GetBy<VW_ACCEPTANCE_REPORT>(p => p.Session_Id == session.Id)
                                   select new StudentDetailFormat
                                   {
                                       PersonId = sr.Person_Id,
                                       Name = sr.SURNAME + " " + sr.FIRSTNAME + " " + sr.OTHER_NAMES,
                                       MatricNumber = sr.Application_Form_Number,
                                       Programme = sr.Programme_Name,
                                       Department = sr.Department_Name,
                                       ProgrammeId = sr.Programme_Id ,
                                       DepartmentId = sr.Department_Id,
                                       Session = sr.Session_Name
                                   }).ToList();

                List<long> personIdList = ndIHndIStudents.Select(p => p.PersonId).Distinct().ToList();

                int NewStudentTotalCountND = 0;
                int NewStudentTotalCountHND = 0;
                int ReturningStudentTotalCountND = 0;
                int ReturningStudentTotalCountHND = 0;
                int NewStudentTotalCountOthers = 0;

                int ReturningStudentTotalCountOthers = 0;

                for (int i = 0; i < personIdList.Count; i++)
                {
                    StudentDetailFormat currentFormat = ndIHndIStudents.LastOrDefault(p => p.PersonId == personIdList[i]);
                    if (ndProgrammes.Contains(currentFormat.ProgrammeId) && ndProgrammeNames.Contains(currentFormat.Programme))
                    {
                        currentFormat.Programme = currentFormat.Programme + " ND I";
                        NewStudentTotalCountND += 1;
                    }
                    else if (hndProgrammes.Contains(currentFormat.ProgrammeId) && hndProgrammeNames.Contains(currentFormat.Programme))
                    {
                        currentFormat.Programme = currentFormat.Programme + " HND I";
                        NewStudentTotalCountHND += 1;
                    }
                    else
                    {
                        continue;
                    }

                    currentFormat.Session = currentSession.Name;
                    currentFormat.Count = 1;
                    newStudents.Add(currentFormat);
                }

                int totalNewStudents = newStudents.Count;

                //if (session.Id == (int)Sessions._20162017)
                //{
                //    ndIIHndIIStudents = (from sr in repository.GetBy<VW_OLD_STUDENT>(x => x.Session_Id == currentSession.Id && x.Confirmation_No != null && (x.Admitted_Session_Id != null && x.Admitted_Session_Id != session.Id))
                //                         select new StudentDetailFormat
                //                         {
                //                             PersonId = sr.Person_Id,
                //                             Name = sr.Name,
                //                             MatricNumber = sr.Matric_Number,
                //                             Programme = sr.Programme_Name,
                //                             Department = sr.Department_Name,
                //                             ProgrammeId = sr.Programme_Id,
                //                             DepartmentId = sr.Department_Id,
                //                             Session = sr.Session_Name,
                //                             AdmittedSession = sr.Admitted_Session,
                //                             GraduationYear = sr.Year_Of_Graduation ?? 0
                //                         }).ToList();
                //}
                //else
                //{
                //    ndIIHndIIStudents = (from sr in repository.GetBy<VW_OLD_STUDENT>(x => x.Session_Id == currentSession.Id && x.Confirmation_No != null && x.Admitted_Session_Id == null)
                //                         select new StudentDetailFormat
                //                         {
                //                             PersonId = sr.Person_Id,
                //                             Name = sr.Name,
                //                             MatricNumber = sr.Matric_Number,
                //                             Programme = sr.Programme_Name,
                //                             Department = sr.Department_Name,
                //                             ProgrammeId = sr.Programme_Id,
                //                             DepartmentId = sr.Department_Id,
                //                             Session = sr.Session_Name,
                //                             AdmittedSession = sr.Admitted_Session,
                //                             GraduationYear = sr.Year_Of_Graduation ?? 0
                //                         }).ToList();
                //}

                int[] otherYears = { 3, 4, 5, 6, 7 };

                if (otherYears.Contains(lastSession.Id))
                {
                    ndIIHndIIStudents = (from sr in repository.GetBy<VW_OLD_STUDENT>(x => x.Session_Id == currentSession.Id && x.Confirmation_No != null && (x.Admitted_Session_Id == lastSession.Id || x.Admitted_Session_Id == null))
                                         select new StudentDetailFormat
                                         {
                                             PersonId = sr.Person_Id,
                                             Name = sr.Name,
                                             MatricNumber = sr.Matric_Number,
                                             Programme = sr.Programme_Name,
                                             Department = sr.Department_Name,
                                             ProgrammeId = sr.Programme_Id,
                                             DepartmentId = sr.Department_Id,
                                             Session = sr.Session_Name,
                                             AdmittedSession = sr.Admitted_Session,
                                             GraduationYear = sr.Year_Of_Graduation ?? 0
                                         }).ToList();
                }
                else
                {
                    ndIIHndIIStudents = (from sr in repository.GetBy<VW_NEW_STUDENT>(p => p.Admitted_Session_Id == lastSession.Id && p.Confirmation_No != null)
                                         select new StudentDetailFormat
                                         {
                                             PersonId = sr.Person_Id,
                                             Name = sr.Name,
                                             MatricNumber = sr.Application_Form_Number,
                                             Programme = sr.Admitted_Programme,
                                             Department = sr.Admitted_Department,
                                             ProgrammeId = sr.Admitted_Programme_Id,
                                             DepartmentId = sr.Admitted_Department_Id,
                                             Session = sr.Admitted_Session
                                         }).ToList();
                }

                List<long> personIdListH = ndIIHndIIStudents.Select(p => p.PersonId).Distinct().ToList();
                for (int i = 0; i < personIdListH.Count; i++)
                {
                    StudentDetailFormat currentFormat = ndIIHndIIStudents.LastOrDefault(p => p.PersonId == personIdListH[i]);

                    if (ndProgrammes.Contains(currentFormat.ProgrammeId) && ndProgrammeNames.Contains(currentFormat.Programme))
                    {
                        currentFormat.Programme = currentFormat.Programme + " ND II";
                        ReturningStudentTotalCountND += 1;
                    }
                    else if (hndProgrammes.Contains(currentFormat.ProgrammeId) && hndProgrammeNames.Contains(currentFormat.Programme))
                    {
                        currentFormat.Programme = currentFormat.Programme + " HND II";
                        ReturningStudentTotalCountHND += 1;
                    }
                    else
                    {
                        continue;
                    }

                    currentFormat.Session = currentSession.Name;
                    currentFormat.Count = 1;
                    returningStudents.Add(currentFormat);

                }

                int totalReturningStudents = returningStudents.Count;

                masterList.AddRange(newStudents);
                masterList.AddRange(returningStudents);

                int totalCount = masterList.Count;

                for (int i = 0; i < masterList.Count; i++)
                {
                    masterList[i].NewStudentTotalCount = totalNewStudents;
                    masterList[i].ReturningStudentTotalCount = totalReturningStudents;
                    masterList[i].TotalCount = totalCount;
                    masterList[i].NewStudentTotalCountHND = NewStudentTotalCountHND;
                    masterList[i].NewStudentTotalCountND = NewStudentTotalCountND;
                    masterList[i].ReturningStudentTotalCountHND = ReturningStudentTotalCountHND;
                    masterList[i].ReturningStudentTotalCountND = ReturningStudentTotalCountND;
                    masterList[i].OtherStudentTotalCount = NewStudentTotalCountOthers + ReturningStudentTotalCountOthers;
                }
                //}

                return masterList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<StudentDetailFormat> GetStudentCountAll(Session session)
        {
            try
            {
                List<StudentDetailFormat> ndIHndIStudents = new List<StudentDetailFormat>();
                List<StudentDetailFormat> ndIIHndIIStudents = new List<StudentDetailFormat>();
                List<StudentDetailFormat> masterList = new List<StudentDetailFormat>();
                List<StudentDetailFormat> newStudents = new List<StudentDetailFormat>();
                List<StudentDetailFormat> returningStudents = new List<StudentDetailFormat>();

                SessionLogic sessionLogic = new SessionLogic();
                Session currentSession = sessionLogic.GetModelsBy(s => s.Session_Id == session.Id).LastOrDefault();

                int lstSessionYear = Convert.ToInt32(currentSession.Name.Split('/').FirstOrDefault());
                string lstSessionName = (lstSessionYear - 1) + "/" + lstSessionYear;

                Session lastSession = sessionLogic.GetModelBy(s => s.Session_Name.Trim() == lstSessionName.Trim());

                int[] ndProgrammes = { 1, 2 };
                int[] hndProgrammes = { 3, 4 };
                string[] ndProgrammeNames = { "ND Full Time", "ND Part Time" };
                string[] hndProgrammeNames = { "HND Full Time", "HND Part Time" };

                //if (currentSession != null)
                //{
                ndIHndIStudents = (from sr in repository.GetBy<VW_NEW_STUDENT>(p => p.Admitted_Session_Id == session.Id && p.Confirmation_No != null)
                                   select new StudentDetailFormat
                                   {
                                       PersonId = sr.Person_Id,
                                       Name = sr.Name,
                                       MatricNumber = sr.Application_Form_Number,
                                       Programme = sr.Admitted_Programme,
                                       Department = sr.Admitted_Department,
                                       ProgrammeId = sr.Admitted_Programme_Id,
                                       DepartmentId = sr.Admitted_Department_Id,
                                       Session = sr.Admitted_Session
                                   }).ToList();

                List<long> personIdList = ndIHndIStudents.Select(p => p.PersonId).Distinct().ToList();

                int NewStudentTotalCountND = 0;
                int NewStudentTotalCountHND = 0;
                int ReturningStudentTotalCountND = 0;
                int ReturningStudentTotalCountHND = 0;
                int NewStudentTotalCountOthers = 0;
                int ReturningStudentTotalCountOthers = 0;

                for (int i = 0; i < personIdList.Count; i++)
                {
                    StudentDetailFormat currentFormat = ndIHndIStudents.LastOrDefault(p => p.PersonId == personIdList[i]);
                    if (ndProgrammes.Contains(currentFormat.ProgrammeId) && ndProgrammeNames.Contains(currentFormat.Programme))
                    {
                        currentFormat.Programme = currentFormat.Programme + " ND I";
                        NewStudentTotalCountND += 1;
                    }
                    else if (hndProgrammes.Contains(currentFormat.ProgrammeId) && hndProgrammeNames.Contains(currentFormat.Programme))
                    {
                        currentFormat.Programme = currentFormat.Programme + " HND I";
                        NewStudentTotalCountHND += 1;
                    }
                    else
                    {
                        continue;
                    }

                    currentFormat.Session = currentSession.Name;
                    currentFormat.Count = 1;
                    newStudents.Add(currentFormat);
                }

                int totalNewStudents = newStudents.Count;

                //if (session.Id == (int)Sessions._20162017)
                //{
                //    ndIIHndIIStudents = (from sr in repository.GetBy<VW_OLD_STUDENT>(x => x.Session_Id == currentSession.Id && x.Confirmation_No != null && (x.Admitted_Session_Id != null && x.Admitted_Session_Id != session.Id))
                //                         select new StudentDetailFormat
                //                         {
                //                             PersonId = sr.Person_Id,
                //                             Name = sr.Name,
                //                             MatricNumber = sr.Matric_Number,
                //                             Programme = sr.Programme_Name,
                //                             Department = sr.Department_Name,
                //                             ProgrammeId = sr.Programme_Id,
                //                             DepartmentId = sr.Department_Id,
                //                             Session = sr.Session_Name,
                //                             AdmittedSession = sr.Admitted_Session,
                //                             GraduationYear = sr.Year_Of_Graduation ?? 0
                //                         }).ToList();
                //}
                //else
                //{
                //    ndIIHndIIStudents = (from sr in repository.GetBy<VW_OLD_STUDENT>(x => x.Session_Id == currentSession.Id && x.Confirmation_No != null && x.Admitted_Session_Id == null)
                //                         select new StudentDetailFormat
                //                         {
                //                             PersonId = sr.Person_Id,
                //                             Name = sr.Name,
                //                             MatricNumber = sr.Matric_Number,
                //                             Programme = sr.Programme_Name,
                //                             Department = sr.Department_Name,
                //                             ProgrammeId = sr.Programme_Id,
                //                             DepartmentId = sr.Department_Id,
                //                             Session = sr.Session_Name,
                //                             AdmittedSession = sr.Admitted_Session,
                //                             GraduationYear = sr.Year_Of_Graduation ?? 0
                //                         }).ToList();
                //}

                int[] otherYears = { 3, 4, 5, 6, 7 };

                if (otherYears.Contains(lastSession.Id))
                {
                    ndIIHndIIStudents = (from sr in repository.GetBy<VW_OLD_STUDENT>(x => x.Session_Id == currentSession.Id && x.Confirmation_No != null && (x.Admitted_Session_Id == lastSession.Id || x.Admitted_Session_Id == null))
                                         select new StudentDetailFormat
                                         {
                                             PersonId = sr.Person_Id,
                                             Name = sr.Name,
                                             MatricNumber = sr.Matric_Number,
                                             Programme = sr.Programme_Name,
                                             Department = sr.Department_Name,
                                             ProgrammeId = sr.Programme_Id,
                                             DepartmentId = sr.Department_Id,
                                             Session = sr.Session_Name,
                                             AdmittedSession = sr.Admitted_Session,
                                             GraduationYear = sr.Year_Of_Graduation ?? 0
                                         }).ToList();
                }
                else
                {
                    ndIIHndIIStudents = (from sr in repository.GetBy<VW_NEW_STUDENT>(p => p.Admitted_Session_Id == lastSession.Id && p.Confirmation_No != null)
                                         select new StudentDetailFormat
                                         {
                                             PersonId = sr.Person_Id,
                                             Name = sr.Name,
                                             MatricNumber = sr.Application_Form_Number,
                                             Programme = sr.Admitted_Programme,
                                             Department = sr.Admitted_Department,
                                             ProgrammeId = sr.Admitted_Programme_Id,
                                             DepartmentId = sr.Admitted_Department_Id,
                                             Session = sr.Admitted_Session
                                         }).ToList();
                }

                //PaymentEtranzactLogic etranzactLogic = new PaymentEtranzactLogic();
                //List<PAYMENT_ETRANZACT> etranzact = etranzactLogic.GetEntitiesBy(e => e.ONLINE_PAYMENT.PAYMENT.Fee_Type_Id == (int)FeeTypes.SchoolFees && e.ONLINE_PAYMENT.PAYMENT.Session_Id == session.Id);

                //List<long> etranzactPins = etranzact.Select(e => e.ONLINE_PAYMENT.PAYMENT.Person_Id).ToList();

                //List<long> etranzactPins = repository.GetBy<VW_CONFIRMED_PAYMENTS>(p => p.Session_Id == session.Id).Select(p => p.Person_Id).ToList();


                List<long> personIdListH = ndIIHndIIStudents.Select(p => p.PersonId).Distinct().ToList();
                for (int i = 0; i < personIdListH.Count; i++)
                {
                    StudentDetailFormat currentFormat = ndIIHndIIStudents.LastOrDefault(p => p.PersonId == personIdListH[i]);

                    if (ndProgrammes.Contains(currentFormat.ProgrammeId) && ndProgrammeNames.Contains(currentFormat.Programme))
                    {
                        currentFormat.Programme = currentFormat.Programme + " ND II";
                        ReturningStudentTotalCountND += 1;
                    }
                    else if (hndProgrammes.Contains(currentFormat.ProgrammeId) && hndProgrammeNames.Contains(currentFormat.Programme))
                    {
                        currentFormat.Programme = currentFormat.Programme + " HND II";
                        ReturningStudentTotalCountHND += 1;
                    }
                    else
                    {
                        continue;
                    }

                    currentFormat.Session = currentSession.Name;
                    currentFormat.Count = 1;
                    returningStudents.Add(currentFormat);

                }

                int totalReturningStudents = returningStudents.Count;

                masterList.AddRange(newStudents);
                masterList.AddRange(returningStudents);

                int totalCount = masterList.Count;

                for (int i = 0; i < masterList.Count; i++)
                {
                    masterList[i].NewStudentTotalCount = totalNewStudents;
                    masterList[i].ReturningStudentTotalCount = totalReturningStudents;
                    masterList[i].TotalCount = totalCount;
                    masterList[i].NewStudentTotalCountHND = NewStudentTotalCountHND;
                    masterList[i].NewStudentTotalCountND = NewStudentTotalCountND;
                    masterList[i].ReturningStudentTotalCountHND = ReturningStudentTotalCountHND;
                    masterList[i].ReturningStudentTotalCountND = ReturningStudentTotalCountND;
                    masterList[i].OtherStudentTotalCount = NewStudentTotalCountOthers + ReturningStudentTotalCountOthers;
                }
                //}

                return masterList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<StudentRecordDTO> GetAllStudentRecord()
        {

            return GetAll()
                .Select(f=>new StudentRecordDTO {
                    Email=f.Email,
                    Gender=f.Sex?.Name,
                    MatricNumber=f.MatricNumber,
                    StudentName=f.FullName,
                    Passport= "https://applications.federalpolyilaro.edu.ng"+ f.ImageFileUrl
                }).ToList();
            
        }
        public List<StudentStatisticReport> GetRegisteredStudentStatisticsBy(Department department, Programme programme, Session session, Level level)
        {
            try
            {
                List<StudentStatisticReport> studentStatisticReportList = new List<Model.Model.StudentStatisticReport>();
                List<StudentReport> studentReportList = new List<StudentReport>();
                List<CourseRegistration> courseRegistrationList = new List<CourseRegistration>();
                List<StudentLevel> studentLevelList = new List<StudentLevel>();

                CourseRegistrationLogic courseLogic = new CourseRegistrationLogic();
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                PersonLogic personLogic = new PersonLogic();
                SessionLogic sessionLogic = new SessionLogic();
                ApplicantJambDetailLogic applicantJambDetailLogic = new ApplicantJambDetailLogic();
                ProgrammeLogic programmeLogic = new ProgrammeLogic();
                DepartmentLogic departmentLogic = new DepartmentLogic();
                CourseRegistrationLogic courseRegistrationLogic = new CourseRegistrationLogic();
                if (department != null && programme != null && session != null && level != null)
                {
                    department = departmentLogic.GetModelsBy(g => g.Department_Id == department.Id).FirstOrDefault();
                    programme = programmeLogic.GetModelsBy(g => g.Programme_Id == programme.Id).FirstOrDefault();
                    studentLevelList = studentLevelLogic.GetModelsBy(c => c.Department_Id == department.Id && c.Level_Id == level.Id && c.Programme_Id == programme.Id && c.Session_Id == session.Id);


                    foreach (var studentLevel in studentLevelList)
                    {
                        var isExist = courseRegistrationLogic.GetModelsBy(f => f.Person_Id == studentLevel.Student.Id && f.Programme_Id == studentLevel.Programme.Id && f.Department_Id == studentLevel.Department.Id).FirstOrDefault();
                        if (isExist == null)
                            continue;
                        StudentReport studentReport = new StudentReport();
                        
                        studentReport.RegistrationNumber = studentLevel.Student.MatricNumber;
                     
                        studentReport.ProgrammeName = programme.Name;
                        studentReport.DepartmentName = department.Name;
                        studentReport.ProgrammeId = programme.Id;
                        studentReport.LevelName = studentLevel.Level.Name;
                        studentReport.SessionName = sessionLogic.GetModelsBy(f => f.Session_Id == session.Id).Select(f => f.Name).FirstOrDefault();
                        if (studentLevel.Student.Sex != null)
                        {
                            studentReport.Sex = studentLevel.Student.Sex.Name;
                            studentReport.SexId = studentLevel.Student.Sex.Id;
                        }
                        else
                        {
                            studentReport.Sex = "";
                            studentReport.SexId = 0;
                        }
                        if (studentLevel.Student.State != null)
                        {
                            studentReport.State = studentLevel.Student.State.Name;
                        }
                        else
                        {
                            studentReport.State = "";
                        }
                        studentReport.MobileNumber = studentLevel.Student.MobilePhone;
                        if (studentLevel.Student.LocalGovernment != null)
                        {
                            studentReport.LocalGovernment = studentLevel.Student.LocalGovernment.Name;
                        }
                        else
                        {
                            studentReport.LocalGovernment = "";
                        }
                        studentReportList.Add(studentReport);
                    }
                    //group the list by State Name
                    var groupedByState=studentReportList.GroupBy(f => f.State).ToList();
                    foreach(var item in groupedByState)
                    {
                        var stateCount = studentReportList.Where(f => f.State == item.Key).ToList();
                        StudentStatisticReport studentStatisticReport = new StudentStatisticReport()
                        {
                            DepartmentName = department.Name,
                            LevelName = level.Name,
                            ProgrammeName = programme.Name,
                            ProgrammeId = programme.Id,
                            SessionName = session.Name,
                            State = item.Key,
                            TotalCount = stateCount.Count,
                            TotalFemale = stateCount.Where(f => f.SexId == 2 || f.SexId == 0).ToList().Count,
                            TotalMale = stateCount.Where(f => f.SexId == 1).ToList().Count,
                            //TotalGenderUnknown = stateCount.Where(f => f.SexId == 0).ToList().Count,
                            TotalSex = stateCount.Count,

                        };
                        studentStatisticReportList.Add(studentStatisticReport);
                    }
                    
                }
                else
                {
                    return studentStatisticReportList;
                }

                return studentStatisticReportList.OrderBy(f => f.State).ToList();
            }
            catch (Exception)
            {

                throw;
            }
        }


    }
}
