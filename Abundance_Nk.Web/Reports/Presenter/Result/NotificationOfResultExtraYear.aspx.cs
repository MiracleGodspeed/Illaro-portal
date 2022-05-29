using Abundance_Nk.Business;
using Abundance_Nk.Model.Model;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Entity.Model;
using Abundance_Nk.Web.Models;

namespace Abundance_Nk.Web.Reports.Presenter.Result
{
    public partial class NotificationOfResultExtraYear : System.Web.UI.Page
    {
        public Level SelectedLevel
        {
            get
            {
                return new Level
                {
                    Id = Convert.ToInt32(ddlLevel.SelectedValue),
                    Name = ddlLevel.SelectedItem.Text
                };
            }
            set { ddlLevel.SelectedValue = value.Id.ToString(); }
        }
        public Session SelectedSession
        {
            get
            {
                return new Session
                {
                    Id = Convert.ToInt32(ddlSession.SelectedValue),
                    Name = ddlSession.SelectedItem.Text
                };
            }
            set { ddlSession.SelectedValue = value.Id.ToString(); }
        }
        public Semester SelectedSemester
        {
            get
            {
                return new Semester
                {
                    Id = Convert.ToInt32(ddlSemester.SelectedValue),
                    Name = ddlSemester.SelectedItem.Text
                };
            }
            set { ddlLevel.SelectedValue = value.Id.ToString(); }
        }
        public string Message
        {
            get { return lblMessage.Text; }
            set { lblMessage.Text = value; }
        }
        public ReportViewer Viewer
        {
            get { return rv; }
            set { rv = value; }
        }

        public int ReportType
        {
            get { throw new NotImplementedException(); }
        }
        public Programme Programme
        {
            get { return new Programme() { Id = Convert.ToInt32(ddlProgramme.SelectedValue), Name = ddlProgramme.SelectedItem.Text }; }
            set { ddlProgramme.SelectedValue = value.Id.ToString(); }
        }

        public Department Department
        {
            get { return new Department() { Id = Convert.ToInt32(ddlDepartment.SelectedValue), Name = ddlDepartment.SelectedItem.Text }; }
            set { ddlDepartment.SelectedValue = value.Id.ToString(); }
        }
        public Student Student
        {
            get { return new Student() { Id = Convert.ToInt32(ddlStudent.SelectedValue), Name = ddlStudent.SelectedItem.Text }; }
            set { ddlStudent.SelectedValue = value.Id.ToString(); }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                Message = "";

                if (!IsPostBack)
                {
                    PopulateAllDropDown();
                    ddlSession.Visible = true;
                    ddlLevel.Visible = true;
                    ddlSemester.Visible = true;
                    btnDisplayReport.Visible = true;

                    ddlStudent.Visible = false;
                    ddlDepartment.Visible = false;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message;
            }
        }

        private void DisplayReportBy(Session session, Semester semester, Student student, Department department, Level level, Programme programme)
        {
            try
            {
                List<Abundance_Nk.Model.Model.Result> resultList = null;
                StudentResultLogic studentResultLogic = new StudentResultLogic();
                StudentAcademicInformationLogic studentAcademicInformationLogic = new StudentAcademicInformationLogic();
                
                var sessionSemesterLogic = new SessionSemesterLogic();
                SessionSemester sessionSemester = sessionSemesterLogic.GetBySessionSemester(SelectedSemester.Id, SelectedSession.Id);

                string graduationDate = "";

                StudentAcademicInformation studentAcademicInformation = studentAcademicInformationLogic.GetModelsBy(s => s.Person_Id == student.Id).LastOrDefault();
                if (studentAcademicInformation != null && studentAcademicInformation.GraduationDate != null)
                {
                    int day = studentAcademicInformation.GraduationDate.Value.Day;
                    int month = studentAcademicInformation.GraduationDate.Value.Month;
                    string monthStr = DateTimeFormatInfo.CurrentInfo.GetMonthName(month);
                    //graduationDate = monthStr + ", " + studentAcademicInformation.GraduationDate.Value.Year;
                    graduationDate = day + "" + studentAcademicInformationLogic.DayTerm(day) + " " + monthStr + ", " + studentAcademicInformation.GraduationDate.Value.Year;
                }

                Model.Model.Result result = studentResultLogic.ViewProcessedStudentResultExtraYear(student.Id, sessionSemester, level, programme, department);

                if(result != null && result.Remark.Contains("CO-"))
                {
                    lblMessage.Text = "Student has outstanding courses.";
                    return;
                }
                else if(result == null)
                {
                    lblMessage.Text = "Student details not found.";
                    return;
                }

                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();

                STUDENT_LEVEL studentLevelEntity = studentLevelLogic.GetEntitiesBy(s => s.Person_Id == student.Id && s.Department_Id == department.Id && s.Programme_Id == programme.Id).LastOrDefault();
                
                if (studentLevelEntity == null)
                {
                    lblMessage.Text = "Student's record was not found, kindly contact system administrator.";
                    return;
                }

                if (studentLevelEntity.STUDENT.Activated != null && !studentLevelEntity.STUDENT.Activated.Value)
                {
                    lblMessage.Text = "Student was disabled, kindly contact school ICT (Records)";
                    return;
                }

                result.StudentTypeName = GetGraduatingDegree(programme.Id);
                result.GraduationStatus = GetGraduationStatus(result.CGPA);
                result.GraduationDate = graduationDate;
                result.MatricNumber = studentLevelEntity.STUDENT.Matric_Number;
                result.Name = studentLevelEntity.STUDENT.PERSON.Last_Name + " " + studentLevelEntity.STUDENT.PERSON.First_Name + " " + studentLevelEntity.STUDENT.PERSON.Other_Name;
                result.Surname = studentLevelEntity.STUDENT.PERSON.Last_Name;
                result.SessionName = session.Name;
                //result.DepartmentName = department.Name;
                result.DepartmentName = GetDepartmentOptionName(studentLevelEntity.Person_Id) == "" ? department.Name : department.Name + " " + "(" + GetDepartmentOptionName(studentLevelEntity.Person_Id) + ")";
                result.PassportUrl = studentLevelEntity.STUDENT.PERSON.Image_File_Url;

                resultList = new List<Model.Model.Result>();
                resultList.Add(result);

                string bind_dsStudentPaymentSummary = "dsNotificationOfResult";
                //string reportPath = @"Reports\Result\NotificationOfResult.rdlc";
                string reportPath = @"Reports\Result\NotificationOfResultReviewd.rdlc";

                rv.Reset();
                rv.LocalReport.DisplayName = "Notification of Result ";
                rv.LocalReport.ReportPath = reportPath;
                rv.LocalReport.EnableExternalImages = true;

                string appRoot = ConfigurationManager.AppSettings["AppRoot"];

                if (resultList.Count > 0)
                {
                    for(int i = 0; i < resultList.Count; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(resultList[i].PassportUrl))
                        {
                            resultList[i].PassportUrl = appRoot + resultList[i].PassportUrl;
                        }
                        else
                        {
                            resultList[i].PassportUrl = appRoot + Utility.DEFAULT_AVATAR;
                        }
                    }

                    rv.ProcessingMode = ProcessingMode.Local;
                    rv.LocalReport.DataSources.Add(new ReportDataSource(bind_dsStudentPaymentSummary.Trim(), resultList));
                    rv.LocalReport.Refresh();
                }

            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message + ex.InnerException.Message;
            }
        }

        private string GetGraduationStatus(decimal? CGPA)
        {
            string title = null;
            try
            {
                if (CGPA >= 3.5M && CGPA <= 4.0M)
                {
                    title = "DISTINCTION";
                }
                else if (CGPA >= 3.0M && CGPA <= 3.49M)
                {
                    title = "UPPER CREDIT";
                }
                else if (CGPA >= 2.5M && CGPA <= 2.99M)
                {
                    title = "LOWER CREDIT";
                }
                else if (CGPA >= 2.0M && CGPA <= 2.49M)
                {
                    title = "PASS";
                }
                else if (CGPA < 2.0M)
                {
                    title = "POOR";
                }
            }
            catch (Exception)
            {

                throw;
            }
            return title;
        }

        private string GetGraduatingDegree(int? progId)
        {
            try
            {
                if (progId == 1 || progId == 2)
                {
                    return "NATIONAL DIPLOMA";
                }
                else
                {
                    return "HIGHER NATIONAL DIPLOMA";
                }
            }
            catch (Exception)
            {
                
                throw;
            }
        }
        private List<Abundance_Nk.Model.Model.Result> GetResultList(Session session, Semester semester, Student student, Department department, Level level, Programme programme)
        {
            try
            {
                List<Abundance_Nk.Model.Model.Result> filteredResult = new List<Model.Model.Result>();
                StudentResultLogic studentResultLogic = new StudentResultLogic();
                List<string> resultList = studentResultLogic.GetProcessedResutBy(session, semester, level, department, programme).Select(p => p.MatricNumber).AsParallel().Distinct().ToList();
                List<Abundance_Nk.Model.Model.Result> result = studentResultLogic.GetProcessedResutBy(session, semester, level, department, programme);
                foreach (string item in resultList)
                {
                    Abundance_Nk.Model.Model.Result resultItem = result.Where(p => p.MatricNumber == item).FirstOrDefault();
                    filteredResult.Add(resultItem);
                }

                return filteredResult.OrderBy(p => p.Name).ToList();
            }
            catch (Exception)
            {

                throw;
            }

        }
        private void PopulateAllDropDown()
        {
            try
            {
                Utility.BindDropdownItem(ddlSession, Utility.GetAllSessions(), Utility.ID, Utility.NAME);
                Utility.BindDropdownItem(ddlLevel, Utility.GetAllLevels(), Utility.ID, Utility.NAME);
                Utility.BindDropdownItem(ddlSemester, Utility.GetAllSemesters(), Utility.ID, Utility.NAME);
                Utility.BindDropdownItem(ddlProgramme, Utility.GetAllProgrammes(), Utility.ID, Utility.NAME);
            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message;
            }
        }
        private bool InvalidUserInput()
        {
            try
            {
                if (SelectedLevel == null || SelectedLevel.Id <= 0)
                {
                    lblMessage.Text = "Please select Level";
                    return true;
                }

                if (SelectedSession == null || SelectedSession.Id <= 0)
                {
                    lblMessage.Text = " Session not set! Please contact your system administrator.";
                    return true;
                }
                else if (Programme == null || Programme.Id <= 0)
                {
                    lblMessage.Text = "Please select Programme";
                    return true;
                }
                else if (Department == null || Department.Id <= 0)
                {
                    lblMessage.Text = "Please select Department";
                    return true;
                }

                if (SelectedSemester == null || SelectedSemester.Id <= 0)
                {
                    lblMessage.Text = " Semester not set! Please contact your system administrator.";
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }
        protected void btnDisplayReport_Click(object sender, EventArgs e)
        {
            try
            {
                if (InvalidUserInput())
                {
                    return;
                }

                StudentLogic studentLogic = new StudentLogic();
                Student currentStudent = studentLogic.GetBy(Student.Id);
                if (currentStudent == null || currentStudent.Id <= 0)
                {
                    lblMessage.Text = "No student record found";
                    return;
                }


                DisplayReportBy(SelectedSession, SelectedSemester, currentStudent, Department, SelectedLevel, Programme );

            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message;
            }
        }
        protected void ddlProgramme_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (Programme != null && Programme.Id > 0)
                {
                    PopulateDepartmentDropdownByProgramme(Programme);
                }
                else
                {
                    ddlDepartment.Visible = false;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message;
            }
        }

        private void PopulateDepartmentDropdownByProgramme(Programme programme)
        {
            try
            {
                List<Department> departments = Utility.GetDepartmentByProgramme(programme);
                if (departments != null && departments.Count > 0)
                {
                    Utility.BindDropdownItem(ddlDepartment, Utility.GetDepartmentByProgramme(programme), Utility.ID, Utility.NAME);
                    ddlDepartment.Visible = true;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message;
            }
        }
        protected void ddlDepartment_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (InvalidUserInput())
                {
                    return;
                }

                rv.LocalReport.DataSources.Clear();

                SessionLogic sessionLogic = new SessionLogic();
                Session session = sessionLogic.GetModelBy(p => p.Session_Id == SelectedSession.Id);
                string[] sessionItems = session.Name.Split('/');
                string sessionNameStr = sessionItems[0];
               
                string currentSessionSuffix = sessionNameStr.Substring(2, 2);
                string yearTwoSessionSuffix = Convert.ToString((Convert.ToInt32(currentSessionSuffix) - 1));

                currentSessionSuffix = "/" + currentSessionSuffix + "/";
                yearTwoSessionSuffix = "/" + yearTwoSessionSuffix + "/";

                CourseRegistrationLogic courseRegistrationLogic = new CourseRegistrationLogic();
                List<Student> students = new List<Student>();
                List<Student> studentList = Utility.GetStudentsBy(SelectedLevel, Programme, Department, SelectedSession);

                for (int i = 0; i < studentList.Count; i++)
                {
                    if (studentList[i].MatricNumber.Contains(currentSessionSuffix) || studentList[i].MatricNumber.Contains("/17/") || studentList[i].MatricNumber.Contains(yearTwoSessionSuffix) || studentList[i].MatricNumber.Contains("/16/") || studentList[i].MatricNumber.Contains("/15/") || studentList[i].MatricNumber.Contains("/14/") ||
                           studentList[i].MatricNumber.Contains("/13/") || studentList[i].MatricNumber.Contains("/12/") || studentList[i].MatricNumber.Contains("/11/") || studentList[i].MatricNumber.Contains("/18/"))
                    {
                        //Filter People who aren't supposed to be in list by virtue of course registration for the session
                        string matric = studentList[i].MatricNumber;
                        var courseReg = courseRegistrationLogic.GetModelsBy(a => a.STUDENT.Matric_Number == matric && a.Session_Id == SelectedSession.Id);
                        if (courseReg != null)
                        {
                            students.Add(studentList[i]);
                        }
                    }
                    else
                    {
                        //Do Nothing


                    }
                }

                if (students != null && students.Count > 0)
                {
                    students.Insert(0, new Student() { Id = 0, FirstName = "-- Select Student --" });
                }

                if (students != null && students.Count > 0)
                {
                    Utility.BindDropdownItem(ddlStudent, students, Utility.ID, "FirstName");
                    ddlStudent.Visible = true;
                    rv.Visible = true;
                }
                else
                {
                    ddlStudent.Visible = false;
                    rv.Visible = false;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message;
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
    }
}