using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Abundance_Nk.Web.Api.DTO
{
    public class AssignmentDto { 
    
        public long Id { get; set; }
        public string Assignment { get; set; }
        public string URL { get; set; }
        public string Instructions { get; set; }
        public DateTime DateSet { get; set; }
        public DateTime DueDate { get; set; }
        public string AssignmentinText { get; set; }
       public string Lecturer { get; set; }
        public string Semester { get; set; }
        public string CourseName { get; set; }
        public string CourseCode { get; set; }
        public AssignmentSubmission AssignmentSubmission { get; set; }
    }
    public class AssignmentSubmission
    {
        public string SubmittedAssignmentUrl { get; set; }
        public string SubmittedAssignmentText { get; set; }
        public string SubmittedAssignmentScore { get; set; }

    }
}