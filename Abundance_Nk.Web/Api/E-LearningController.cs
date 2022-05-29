using Abundance_Nk.Business;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Web.Api.DTO;
using Abundance_Nk.Web.Areas.Admin.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;

namespace Abundance_Nk.Web.Api
{
   
    public class E_LearningController : ApiController
    {
        private StudentLogic studentLogic;
        private StudentLevelLogic studentLevelLogic;

        public E_LearningController()
        {
            studentLogic = new StudentLogic();
            studentLevelLogic = new StudentLevelLogic();
        }
        

        [HttpGet]
        public IHttpActionResult LoginStudent(string matricNo,string Password)
        {
            //var PersonId = studentLogic.GetModelBy(x => x.Matric_Number == matricNo && x.Password_hash == Password);
            var studentLevel=studentLevelLogic.GetModelsBy(f => f.STUDENT.Matric_Number == matricNo && f.STUDENT.Password_hash == Password).LastOrDefault();
           
            return Ok(new { OutPut = studentLevel });
        }
        [HttpGet]
        public IHttpActionResult RegisteredCourses(long PersonId, int Semester)
        {
           List<CourseRegistrationDetail> courseRegistrationDetail = new List<CourseRegistrationDetail>();
            CourseAllocationLogic courseAllocationLogic = new CourseAllocationLogic();
            SessionLogic sessionLogic = new SessionLogic();
            CourseRegistrationDetailLogic courseRegistrationDetailLogic = new CourseRegistrationDetailLogic();
            StudentLogic studentLogic = new StudentLogic();
            var activeSession = sessionLogic.GetModelsBy(f => f.Activated == true).LastOrDefault();
            Model.Model.Student student = studentLogic.GetBy(PersonId);
            List<CourseRegistrationDto> CourseRegistrationDto = new List<CourseRegistrationDto>();
            
            if (activeSession?.Id > 0 && student?.Id > 0)
            {
              
                    courseRegistrationDetail = courseRegistrationDetailLogic.GetModelsBy(f => f.STUDENT_COURSE_REGISTRATION.Session_Id == activeSession.Id && f.STUDENT_COURSE_REGISTRATION.Person_Id == PersonId && f.Semester_Id == Semester);
                if (courseRegistrationDetail?.Count > 0)
                {
                    var departmentId = courseRegistrationDetail.FirstOrDefault().CourseRegistration.Department.Id;
                    var programmeId = courseRegistrationDetail.FirstOrDefault().CourseRegistration.Programme.Id;
                    var levelId= courseRegistrationDetail.FirstOrDefault().CourseRegistration.Level.Id;
                    var semesterId= courseRegistrationDetail.FirstOrDefault().Semester.Id;
                    var courseAllocations=courseAllocationLogic.GetModelsBy(f => f.Department_Id == departmentId && f.Programme_Id == programmeId && f.Level_Id == levelId && f.Session_Id == activeSession.Id && f.Semester_Id == semesterId);
                    foreach (var item in courseRegistrationDetail)
                    {
                        CourseRegistrationDto courseRegistrationDtos = new CourseRegistrationDto();
                        courseRegistrationDtos.CourseCode = item.Course.Code;
                        courseRegistrationDtos.CourseName = item.Course.Name;
                        courseRegistrationDtos.CourseId = item.Course.Id;
                        courseRegistrationDtos.CourseAllocationId = courseAllocations.Where(g => g.Course.Id == item.Course.Id).Select(f => f.Id).FirstOrDefault();
                        CourseRegistrationDto.Add(courseRegistrationDtos);

                    }
                    

                }
                    return Ok(new { Output = CourseRegistrationDto });
               
            }
            return Ok(new { Output = "Student does not exist" });
          
        }
        [HttpGet]
        public IHttpActionResult ContentType(long PersonId, long CourseId)
        {
            CourseRegistrationLogic courseRegistrationLogic = new CourseRegistrationLogic();
            var CourseRegId = courseRegistrationLogic.GetModelsBy(x => x.Person_Id == PersonId &&  x.SESSION.Activated==true).FirstOrDefault();
            
            CourseAllocationLogic courseAllocationLogic = new CourseAllocationLogic();
            var CourseAllocationId = courseAllocationLogic.GetModelBy(x => x.Course_Id == CourseId && x.Programme_Id == CourseRegId.Programme.Id && x.Department_Id == CourseRegId.Department.Id && x.SESSION.Activated == true);
            EContentTypeLogic eContentTypeLogic = new EContentTypeLogic();
            var content = eContentTypeLogic.GetModelsBy(x => x.Course_Allocation_Id == CourseAllocationId.Id);
            List<CourseTypeDto> CourseTypeDto = new List<CourseTypeDto>();
            foreach (var item in content)
            {
                CourseTypeDto courseTypeDtos = new CourseTypeDto();
                courseTypeDtos.Id = item.Id;
                courseTypeDtos.Name = item.Name;
                CourseTypeDto.Add(courseTypeDtos);
            }
            return Ok(new { Output = CourseTypeDto });
        }
        [HttpGet]
        public IHttpActionResult CourseContentDetails(int ContentId,int CourseId)
        {
            ECourseLogic eCourseLogic = new ECourseLogic();
            var Coursedetails = eCourseLogic.GetModelsBy(x => x.E_Content_Type_Id == ContentId && x.Course_Id == CourseId);
            List<CourseContentDto> CourseContentDto = new List<CourseContentDto>();
            foreach(var item in Coursedetails)
            {
                CourseContentDto courseContentDto = new CourseContentDto();
                courseContentDto.Id = item.Id;
                courseContentDto.Url = item.Url;
                courseContentDto.VideoUrl = item.VideoUrl;
                courseContentDto.StartTime = item.EContentType.StartTime;
                courseContentDto.StopTime = item.EContentType.EndTime;
                courseContentDto.LiveStreamingLink = item.LiveStreamLink;
                CourseContentDto.Add(courseContentDto);
            }
            return Ok(new { Output = CourseContentDto });
        }
        [HttpGet]
        public IHttpActionResult GetStudentsBy(int programmeId, int departmentId , int levelId)
        {
            List<StudentDTO> studentDTOList = new List<StudentDTO>();
            StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
             var studentLevelList=studentLevelLogic.GetModelsBy(f => f.Department_Id == departmentId && f.Programme_Id == programmeId && f.Level_Id == levelId && f.SESSION.Activated==true);
            if (studentLevelList?.Count > 0)
            {
                foreach(var item in studentLevelList)
                {
                    StudentDTO studentDTO = new StudentDTO();
                    studentDTO.StudentName = item.Student.FullName;
                    studentDTO.Password = item.Student.PasswordHash;
                    studentDTO.Passport = item.Student.ImageFileUrl;
                    studentDTO.MatricNumber = item.Student.MatricNumber;
                    studentDTO.RegisteredCourses = GetRegisteredCourse(item.Student, item.Level);
                    studentDTOList.Add(studentDTO);
                }
            }
            return Ok(new { Output = studentDTOList });
        }
        [HttpGet]
        public IHttpActionResult CourseAllocation(int programmeId, int departmentId, int levelId)
        {
            List<CourseAllocationDTO> courseAllocationDTOs = new List<CourseAllocationDTO>();
            try
            {
                
                var activeSession=GetActiveSession();
                if (activeSession == null)
                {
                    return Ok(new { Output = "No Active Session" });
                }
                CourseAllocationLogic courseAllocationLogic = new CourseAllocationLogic();
                var courseAllocationList=courseAllocationLogic.GetModelsBy(f => f.Programme_Id == programmeId && f.Department_Id == departmentId && f.Level_Id == levelId && f.Session_Id == activeSession.Id);
                if (courseAllocationList?.Count > 0)
                {
                    foreach(var item in courseAllocationList)
                    {
                        CourseAllocationDTO courseAllocationDTO = new CourseAllocationDTO();
                        courseAllocationDTO.Session = item.Session;
                        courseAllocationDTO.StaffEmail = item.User.Username;
                        courseAllocationDTO.StaffPassword = item.User.Password;
                        courseAllocationDTO.Course = GetAllocatedCourse(item.User,courseAllocationList,item);
                        courseAllocationDTOs.Add(courseAllocationDTO);
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return Ok(new { Output = courseAllocationDTOs });
        }
        [HttpGet]
        public IHttpActionResult Department()
        {
            List<Department> departments = new List<Department>();
            try
            {
                DepartmentLogic departmentLogic = new DepartmentLogic();
                departments = departmentLogic.GetModelsBy(f => f.Active);
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return Ok(new { Output = departments });
        }
        [HttpGet]
        public IHttpActionResult Faculty()
        {
            List<Faculty> facultys = new List<Faculty>();
            try
            {
                FacultyLogic facultyLogic = new FacultyLogic();
                facultys = facultyLogic.GetAll();
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return Ok(new { Output = facultys });
        }
        [HttpGet]
        public IHttpActionResult Programme()
        {
            List<Programme> programmes = new List<Programme>();
            try
            {
                ProgrammeLogic programmeLogic = new ProgrammeLogic();
                programmes = programmeLogic.GetModelsBy(f => f.Activated == true);
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return Ok(new { Output = programmes });
        }

        [HttpGet]
        public IHttpActionResult GetElearning(long courseAllocationId)
        {
            List<ElearningDTO> elearningDTOList = new List<ElearningDTO>();
            try
            {
                EContentTypeLogic eContentTypeLogic = new EContentTypeLogic();
                var eContentType=eContentTypeLogic.GetModelsBy(g => g.IsDelete == false && g.Course_Allocation_Id == courseAllocationId);
                if (eContentType?.Count > 0)
                {
                    foreach(var item in eContentType)
                    {
                        ElearningDTO elearningDTO = new ElearningDTO();
                        elearningDTO.EcontentTopicId = item.Id;
                        elearningDTO.EndDate = item.EndTime;
                        elearningDTO.StartDate = item.StartTime;
                        elearningDTO.TopicDescription = item.Description;
                        elearningDTO.Topic = item.Name;
                        elearningDTO.ECourseContent = GetECourseContent(item);
                        elearningDTOList.Add(elearningDTO);
                    }
                }
                
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return Ok(new { Output = elearningDTOList });
        }

        public List<RegisteredCourse> GetRegisteredCourse(Student student, Level level)
        {
            List<RegisteredCourse> registeredCourseList = new List<RegisteredCourse>();
            try
            {
                CourseRegistrationDetailLogic courseRegistrationDetailLogic = new CourseRegistrationDetailLogic();
                var courseRegistrationDetails=courseRegistrationDetailLogic.GetModelsBy(f => f.STUDENT_COURSE_REGISTRATION.Person_Id == student.Id && f.STUDENT_COURSE_REGISTRATION.SESSION.Activated == true && f.STUDENT_COURSE_REGISTRATION.Level_Id == level.Id);
                if (courseRegistrationDetails?.Count > 0)
                {
                    foreach(var item in courseRegistrationDetails)
                    {
                        RegisteredCourse registeredCourse = new RegisteredCourse();
                        registeredCourse.CourseCode = item.Course.Code;
                        registeredCourse.CourseId = item.Course.Id;
                        registeredCourse.CourseName = item.Course.Name;
                        registeredCourseList.Add(registeredCourse);
                    }
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
            return registeredCourseList;
        }
        public RegisteredCourse GetAllocatedCourse(User user, List<CourseAllocation> courseAllocationList , CourseAllocation courseAllocation)
        {
            RegisteredCourse registeredCourse = new RegisteredCourse();
            try
            {
                if(user?.Id>0 && courseAllocation?.Id>0 && courseAllocationList?.Count > 0)
                {
                    var userCourseAllocation = courseAllocationList.Where(f => f.User.Id == user.Id && f.Id == courseAllocation.Id).FirstOrDefault();
                    if (userCourseAllocation?.Id > 0)
                    {
                        
                            
                            registeredCourse.CourseCode = userCourseAllocation.Course.Code;
                            registeredCourse.CourseId = userCourseAllocation.Course.Id;
                            registeredCourse.CourseName = userCourseAllocation.Course.Name;
                            

                    }
                }
           

            }
            catch (Exception ex)
            {

                throw ex;
            }
            return registeredCourse;
        }
        public List<ECourseContent> GetECourseContent(EContentType eContentType)
        {
            List<ECourseContent> eCourseContentList = new List<ECourseContent>();
            try
            {
                if (eContentType?.Id > 0 )
                {
                    ECourseLogic eCourseLogic = new ECourseLogic();
                    var econtent=eCourseLogic.GetModelsBy(g => g.E_Content_Type_Id == eContentType.Id);
                    if (econtent?.Count > 0)
                    {
                        foreach(var item in econtent)
                        {
                            ECourseContent eCourseContent = new ECourseContent();
                            eCourseContent.EcontentId = item.Id;
                            eCourseContent.VideoUrl = item.VideoUrl;
                            eCourseContent.PDFUrl = "https://applications.federalpolyilaro.edu.ng" + (item.Url.Contains('~')? item.Url.Remove(0,1): item.Url);
                            eCourseContent.Course = new RegisteredCourse();
                            eCourseContent.Course.CourseCode = item.Course.Code;
                            eCourseContent.Course.CourseId = item.Course.Id;
                            eCourseContent.Course.CourseName = item.Course.Name;
                            eCourseContent.StartDate = item.EContentType.StartTime;
                            eCourseContent.StopDate = item.EContentType.EndTime;
                            eCourseContentList.Add(eCourseContent);
                        }

                    }
                }


            }
            catch (Exception ex)
            {

                throw ex;
            }
            return eCourseContentList;
        }
        public Session GetActiveSession()
        {
            SessionLogic sessionLogic = new SessionLogic();
            var ActiveSession=sessionLogic.GetModelsBy(f => f.Activated == true).LastOrDefault();
            return ActiveSession;
        }
        [HttpGet]
        public IHttpActionResult EnterChatRoomwithCourseId(long courseId, long personId)
        {
            ChatModel chatModel = new ChatModel();
            try
            {
                if (courseId>0 && personId>0)
                {


                    
                    CourseRegistrationDetailLogic courseRegistrationDetailLogic = new CourseRegistrationDetailLogic();
                    CourseAllocationLogic courseAllocationLogic = new CourseAllocationLogic();
                    EChatTopicLogic eChatTopicLogic = new EChatTopicLogic();
                    EChatResponseLogic eChatResponseLogic = new EChatResponseLogic();
                    var courseregistrations = courseRegistrationDetailLogic.GetModelBy(f => f.Course_Id == courseId && f.STUDENT_COURSE_REGISTRATION.Person_Id==personId && f.STUDENT_COURSE_REGISTRATION.SESSION.Activated==true);
                    if (courseregistrations?.Id > 0)
                    {

                        var courseAllocation = courseAllocationLogic.GetModelsBy(s => s.Department_Id == courseregistrations.CourseRegistration.Department.Id &&
                                                           s.Programme_Id == courseregistrations.CourseRegistration.Programme.Id && s.Level_Id == courseregistrations.CourseRegistration.Level.Id
                                                           && s.Session_Id == courseregistrations.CourseRegistration.Session.Id && s.Course_Id == courseregistrations.Course.Id).FirstOrDefault();
                        if (courseAllocation?.Id > 0)
                        {
                            StudentLogic studentLogic = new StudentLogic();
                            Student student = studentLogic.GetBy(personId);
                            var existingChatTopic = eChatTopicLogic.GetModelsBy(f => f.Course_Allocation_Id == courseAllocation.Id).LastOrDefault();
                            if (existingChatTopic == null)
                            {
                                EChatTopic eChatTopic = new EChatTopic()
                                {
                                    CourseAllocation = courseAllocation,
                                    Active = true
                                };
                                eChatTopicLogic.Create(eChatTopic);
                            }
                            chatModel.EChatBoards=eChatResponseLogic.LoadChatBoard(student, null, courseAllocation.Id);
                            chatModel.StudentName = student.FullName;
                            chatModel.MatricNumber = student.MatricNumber;
                            chatModel.CourseCode = courseAllocation.Course.Code;
                            chatModel.CourseTitle = courseAllocation.Course.Name;


                        }

                    }
                }
            }
                            catch (Exception ex)
            {

                throw ex;
            }
            return Ok(new { Output = chatModel });
        }
        [HttpGet]
        public IHttpActionResult EnterChatRoom(long courseAllocationId, long personId)
        {
            ChatModel chatModel = new ChatModel();
            try
            {
                if (courseAllocationId > 0 && personId > 0)
                {



                    CourseRegistrationDetailLogic courseRegistrationDetailLogic = new CourseRegistrationDetailLogic();
                    CourseAllocationLogic courseAllocationLogic = new CourseAllocationLogic();
                    EChatTopicLogic eChatTopicLogic = new EChatTopicLogic();
                    EChatResponseLogic eChatResponseLogic = new EChatResponseLogic();
                    var courseAllocation=courseAllocationLogic.GetModelsBy(f => f.Course_Allocation_Id == courseAllocationId).FirstOrDefault();
                    if (courseAllocation?.Id > 0)
                    {
                        StudentLogic studentLogic = new StudentLogic();
                        Student student = studentLogic.GetBy(personId);
                        var existingChatTopic = eChatTopicLogic.GetModelsBy(f => f.Course_Allocation_Id == courseAllocation.Id).LastOrDefault();
                        if (existingChatTopic == null)
                        {
                            EChatTopic eChatTopic = new EChatTopic()
                            {
                                CourseAllocation = courseAllocation,
                                Active = true
                            };
                            eChatTopicLogic.Create(eChatTopic);
                        }
                        chatModel.EChatBoards = eChatResponseLogic.LoadChatBoard(student, null, courseAllocation.Id);
                        chatModel.StudentName = student.FullName;
                        chatModel.MatricNumber = student.MatricNumber;
                        chatModel.CourseCode = courseAllocation.Course.Code;
                        chatModel.CourseTitle = courseAllocation.Course.Name;


                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return Ok(new { Output = chatModel });
        }
        [HttpGet]
        public IHttpActionResult SaveChatResponse(long courseAllocationId, string response, long personId)
        {
            try
            {
                EChatResponseLogic eChatResponseLogic = new EChatResponseLogic();
                StudentLogic studentLogic = new StudentLogic();
                Student Student = studentLogic.GetBy(personId);
                if (Student != null && courseAllocationId > 0 && response != null && response != " ")
                {

                    eChatResponseLogic.SaveChatResponse(courseAllocationId, Student, response, null);

                    return Ok(new { Output = "Response Saved" });
                }
            }
            catch (Exception)
            {

                throw;
            }
            return Ok(new { Output = "Error" });
            
        }
        [HttpGet]
        public IHttpActionResult Assignment(int CourseId)
        { 
            EAssignmentLogic eAssignmentLogic = new EAssignmentLogic();
           
            CourseAllocationLogic courseAllocationLogic = new CourseAllocationLogic();
            List<AssignmentDto> AssignmentList = new List<AssignmentDto>();
            var Assignmentsobject = eAssignmentLogic.GetModelsBy(x => x.Course_Id == CourseId);
            foreach(var item in Assignmentsobject)
            {
                AssignmentDto assignmentDto = new AssignmentDto();
                var Lecturer = courseAllocationLogic.GetModelBy(x => x.Course_Allocation_Id == item.CourseAllocation.Id);
                assignmentDto.Lecturer = Lecturer.User.Email;
                assignmentDto.Id = item.Id;
                assignmentDto.Instructions = item.Instructions;
                assignmentDto.URL = item.URL;
                assignmentDto.Assignment = item.Assignment;
                assignmentDto.AssignmentinText = item.AssignmentinText;
                assignmentDto.DueDate = item.DueDate;
                AssignmentList.Add(assignmentDto);

            }
            return Ok(new { Output = AssignmentList});
       }
        public async Task<string> AssigmentSubmissionAsync(long personId, int AssignmentId, int Semester, string AssignmentInText)
        {
            var ctx = HttpContext.Current;
            var root = ctx.Server.MapPath("/Content/ELearning/Submission/");
            var provider = new MultipartFormDataStreamProvider(root);
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }

            var sessionActive = GetActiveSession();

            StudentLogic studentLogic = new StudentLogic();
            EAssignmentSubmissionLogic eAssignmentSubmissionLogic = new EAssignmentSubmissionLogic();

            var student = studentLogic.GetModelsBy(u => u.Person_Id == personId).LastOrDefault();
            if (student != null)
            {


                await Request.Content.ReadAsMultipartAsync(provider);

                foreach (var file in provider.FileData)
                {


                    var name = file.Headers.ContentDisposition.FileName;
                    name = name.Trim('"');
                    var NameOfStudent = student.Name.Replace("/", "_").Replace(" ", "_") + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + DateTime.Now.Millisecond + name;

                    var localFilename = file.LocalFileName;
                    var filepath = Path.Combine(root, NameOfStudent);
                    File.Move(localFilename, filepath);

                    var existingSubmission = eAssignmentSubmissionLogic.GetModelsBy(s => s.STUDENT.Person_Id == personId && s.Assignment_Id == AssignmentId).LastOrDefault();
                    if (existingSubmission != null)
                    {
                        existingSubmission.AssignmentContent = "~/Content/ELearning/Submission/" + NameOfStudent;
                        existingSubmission.DateSubmitted = DateTime.Now;

                        eAssignmentSubmissionLogic.Modify(existingSubmission);
                    }
                    else
                    {
                        EAssignmentLogic eAssignmentLogic = new EAssignmentLogic();
                        var Assignment = eAssignmentLogic.GetModelBy(x => x.Id == AssignmentId);
                        EAssignmentSubmission eAssignmentSubmission = new EAssignmentSubmission();
                        eAssignmentSubmission.AssignmentContent = "~/Content/ELearning/Submission/" + NameOfStudent;
                        eAssignmentSubmission.DateSubmitted = DateTime.Now;
                        eAssignmentSubmission.EAssignment = Assignment;
                        eAssignmentSubmission.Student = student;
                        eAssignmentSubmission.TextSubmission = AssignmentInText;

                        eAssignmentSubmissionLogic.Create(eAssignmentSubmission);
                    }

                    return "File Uploaded";


                }



            }
    
            return "student does not exist";



        }
        [HttpGet]
        public IHttpActionResult AssignmentByCategory(long PersonId)
        {
            List<CourseRegistrationDetail> courseRegistrationDetail = new List<CourseRegistrationDetail>();
            CourseRegistrationDetailLogic courseRegistrationDetailLogic = new CourseRegistrationDetailLogic();
            StudentLogic studentLogic = new StudentLogic();
            Model.Model.Student student = studentLogic.GetBy(PersonId);
            List<AssignmentByCategoryDTO> AssignmentByCategoryList = new List<AssignmentByCategoryDTO>();
            List<AssignmentDto> AssignmentListSubmitted = new List<AssignmentDto>();
            List<AssignmentDto> AssignmentListNotsubmitted = new List<AssignmentDto>();

            if (student?.Id > 0)
            {
                courseRegistrationDetail = courseRegistrationDetailLogic.GetModelsBy(f => f.STUDENT_COURSE_REGISTRATION.SESSION.Activated == true && f.STUDENT_COURSE_REGISTRATION.Person_Id == PersonId);

                if (courseRegistrationDetail?.Count > 0)
                {
                    foreach (var items in courseRegistrationDetail)
                    {
                        EAssignmentLogic eAssignmentLogic = new EAssignmentLogic();

                        var Assignmentsobject = eAssignmentLogic.GetModelsBy(x => x.Course_Id == items.Course.Id && x.COURSE_ALLOCATION.Session_Id == items.CourseRegistration.Session.Id && x.COURSE_ALLOCATION.Programme_Id == items.CourseRegistration.Programme.Id &&
                        x.COURSE_ALLOCATION.Department_Id == items.CourseRegistration.Department.Id && x.COURSE_ALLOCATION.Level_Id == items.CourseRegistration.Level.Id);
                        if (Assignmentsobject?.Count > 0)
                        {
                            foreach (var item in Assignmentsobject)
                            {
                                AssignmentDto assignmentDto = new AssignmentDto();

                                assignmentDto.Lecturer = item.CourseAllocation.User.Username;
                                assignmentDto.Id = item.Id;
                                assignmentDto.Instructions = item.Instructions;
                                assignmentDto.URL = item.URL;
                                assignmentDto.Assignment = item.Assignment;
                                assignmentDto.AssignmentinText = item.AssignmentinText;
                                assignmentDto.DueDate = item.DueDate;
                                assignmentDto.Semester = item.CourseAllocation.Semester.Name;
                                assignmentDto.CourseName = item.Course.Name;
                                assignmentDto.CourseCode = item.Course.Code;
                                EAssignmentSubmissionLogic eAssignmentSubmissionLogic = new EAssignmentSubmissionLogic();
                                var submittedAssignment = eAssignmentSubmissionLogic.GetModelsBy(x => x.Assignment_Id == item.Id && x.Student_Id == student.Id).FirstOrDefault();
                                if (submittedAssignment?.Id > 0)
                                {
                                    assignmentDto.AssignmentSubmission = new AssignmentSubmission();
                                    assignmentDto.AssignmentSubmission.SubmittedAssignmentScore = item.Publish && submittedAssignment.Score!=null ? Decimal.Truncate((decimal)submittedAssignment.Score) + "/" + item.MaxScore : "";
                                    assignmentDto.AssignmentSubmission.SubmittedAssignmentText = submittedAssignment.TextSubmission;
                                    assignmentDto.AssignmentSubmission.SubmittedAssignmentUrl = submittedAssignment.AssignmentContent;
                                    AssignmentListSubmitted.Add(assignmentDto);
                                }
                                else
                                {
                                    AssignmentListNotsubmitted.Add(assignmentDto);
                                }


                            } //AssignmentList.Add(assignmentDto);

                        }
                        //   return Ok(new { Output = "No Assignment" });

                    }
                    AssignmentByCategoryDTO assignmentByCategoryDTO = new AssignmentByCategoryDTO()
                    {
                        SubmittedAssignment = AssignmentListSubmitted,
                        NotSubmittedAssignment = AssignmentListNotsubmitted
                    };
                    
                    return Ok(new { Output = assignmentByCategoryDTO });

                }

            }
            return Ok(new { Output = "null" });
        }
        
        [System.Web.Http.AcceptVerbs("GET","POST")]
        [System.Web.Http.HttpPost]
        public HttpResponseMessage PostAssignmentAnswer(long personId, int AssignmentId, string AssignmentInText)
        {
            try
            {
                StudentLogic studentLogic = new StudentLogic();
                EAssignmentSubmissionLogic eAssignmentSubmissionLogic = new EAssignmentSubmissionLogic();
                EAssignmentLogic eAssignmentLogic = new EAssignmentLogic();
                var Assignment = eAssignmentLogic.GetModelBy(x => x.Id == AssignmentId);
                string savedFileName = String.Empty;
                var student = studentLogic.GetModelsBy(u => u.Person_Id == personId).LastOrDefault();
                if (student != null && Assignment!=null)
                {
                    var existingSubmission = eAssignmentSubmissionLogic.GetModelsBy(s => s.STUDENT.Person_Id == personId && s.Assignment_Id == AssignmentId).LastOrDefault();
                    if (existingSubmission == null)
                    {
                        var httpRequest = HttpContext.Current.Request;
                        var root = HttpContext.Current.Server.MapPath("/Content/ELearning/Submission/");
                        if (httpRequest.Files.Count >= 1)
                        {
                            foreach (string file in httpRequest.Files)
                            {
                                var postedFile = httpRequest.Files[file];
                                string extension = Path.GetExtension(postedFile.FileName);
                                var NameOfStudent = student.Name.Replace("/", "_").Replace(" ", "_") + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + DateTime.Now.Millisecond + extension;
                                savedFileName = Path.Combine(root, NameOfStudent);
                                postedFile.SaveAs(savedFileName);
                                EAssignmentSubmission eAssignmentSubmission = new EAssignmentSubmission();
                               
                                eAssignmentSubmission.AssignmentContent = "~/Content/ELearning/Submission/" + NameOfStudent;

                                eAssignmentSubmission.DateSubmitted = DateTime.Now;
                                eAssignmentSubmission.EAssignment = Assignment;
                                eAssignmentSubmission.Student = student;
                                eAssignmentSubmission.TextSubmission = AssignmentInText;

                                eAssignmentSubmissionLogic.Create(eAssignmentSubmission);
                                return Request.CreateResponse(HttpStatusCode.Created);
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.NoContent);
                        }
                        
                        
                    }
                    else
                    {
                        return Request.CreateResponse(new { Output = "Already Submitted Assignment" });
                    }
                

                }
                return Request.CreateResponse(new { Output = "Student Don't Exist" });
                
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        [HttpPost]
        public IHttpActionResult UploadAssignment()
        {
            try
            {


                var PersonId= Convert.ToInt64(HttpContext.Current.Request.Params["PersonId"]);
                var AssignmentId= Convert.ToInt32(HttpContext.Current.Request.Params["AssignmentId"]);
                var AssignmentText= HttpContext.Current.Request.Params["AssignmentText"];
                var file= HttpContext.Current.Request.Files["file"];
                if (file != null && AssignmentId > 0 && PersonId > 0)
                {
                    StudentLogic studentLogic = new StudentLogic();
                    EAssignmentSubmissionLogic eAssignmentSubmissionLogic = new EAssignmentSubmissionLogic();
                    EAssignmentLogic eAssignmentLogic = new EAssignmentLogic();
                    var Assignment = eAssignmentLogic.GetModelBy(x => x.Id == AssignmentId);
                    string savedFileName = String.Empty;
                    var student = studentLogic.GetModelsBy(u => u.Person_Id == PersonId).LastOrDefault();
                    if (student != null && Assignment != null)
                    {
                        if (Assignment != null)
                        {
                            var existingSubmission = eAssignmentSubmissionLogic.GetModelsBy(s => s.STUDENT.Person_Id == PersonId && s.Assignment_Id == AssignmentId).LastOrDefault();
                            if (existingSubmission == null)
                            {

                                var root = HttpContext.Current.Server.MapPath("/Content/ELearning/Submission/");
                                if (file.ContentLength > 0)
                                {

                                    string extension = Path.GetExtension(file.FileName);
                                    var NameOfStudent = student.Name.Replace("/", "_").Replace(" ", "_") + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + DateTime.Now.Millisecond + extension;
                                    savedFileName = Path.Combine(root, NameOfStudent);
                                    file.SaveAs(savedFileName);
                                    EAssignmentSubmission eAssignmentSubmission = new EAssignmentSubmission();

                                    eAssignmentSubmission.AssignmentContent = "~/Content/ELearning/Submission/" + NameOfStudent;

                                    eAssignmentSubmission.DateSubmitted = DateTime.Now;
                                    eAssignmentSubmission.EAssignment = Assignment;
                                    eAssignmentSubmission.Student = student;
                                    eAssignmentSubmission.TextSubmission = AssignmentText;

                                    eAssignmentSubmissionLogic.Create(eAssignmentSubmission);
                                    return Ok(HttpStatusCode.Created);

                                }
                                else
                                {
                                    return Ok(HttpStatusCode.NoContent);
                                }


                            }
                            else
                            {
                                return Ok(new { Output = "Already Submitted Assignment" });
                            }
                        }

                        return Ok(new { Output = "Invalid Assignment Id" });

                    }
                    return Ok(new { Output = "Student Don't Exist" });

                }

                return Ok(new { Output = "Please, supply all the required fields" });
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }



    }

}
