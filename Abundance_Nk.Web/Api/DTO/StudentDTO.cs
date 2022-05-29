using Abundance_Nk.Model.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Abundance_Nk.Web.Api.DTO
{
    public class StudentDTO
    {
        public string StudentName { get; set; }
        public string MatricNumber { get; set; }
        public string Password { get; set; }
        public string Passport { get; set; }
        public List<RegisteredCourse> RegisteredCourses { get; set; }
    }
    public class RegisteredCourse
    {
        public long CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }

    }
    public class CourseAllocationDTO
    {
        public string StaffEmail { get; set; }
        public string StaffPassword { get; set; }
        public Session Session { get; set; }
        public RegisteredCourse Course { get; set; }
    }
    public class ElearningDTO
    {
        public long EcontentTopicId { get; set; }
        public string Topic { get; set; }
        public string TopicDescription { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<ECourseContent> ECourseContent { get; set; }

    }
    public class ECourseContent
    {
        public long EcontentId { get; set; }
        public string PDFUrl { get; set; }
        public string VideoUrl { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime StopDate { get; set; }
        public RegisteredCourse Course { get; set; }
    }
    public class Studentdetails
    {
        public string FullName { get; set; }
        public string MatricNo { get; set; }
        public string programmeName { get; set; }
        public string DepartmentName { get; set; }
        public string Level { get; set; }
       
        public string ImageFileUrl { get; set; }
      
        public string Sex { get; set; }
        public string ContactAddress { get; set; }
        public string Email { get; set; }
        public string MobilePhone { get; set; }
        public string DateOfBirth { get; set; }
        public string State { get; set; }
        public string HomeTown { get; set; }

        public string HomeAddress { get; set; }

        public string Nationality { get; set; }
        public string Religion { get; set; }
       


      
        public string ApplicantSponsorName { get; set; }
        public string ApplicantSponsorContactAddress { get; set; }
        public string ApplicantSponsorMobilePhone { get; set; }
        public string Relationship { get; set; }


     

        public string SchoolName { get; set; }

        public string PreviousCourse { get; set; }

        public string StartDate { get; set; }
        public string EndDate { get; set; }

        public string ITDuration { get; set; }
        public string ResultGrade { get; set; }
        public string Qualification { get; set; }
     
        public string ResultCopyUrl { get; set; }
        public string CertificateCopyUrl { get; set; }
        public string ITLetterOfCompletion { get; set; }



        public List<PaymentBreakDown> payments { get; set; }


    }
    public class PaymentBreakDown
    {
        public string PaymentMode { get; set; }
        public string PaymentType { get; set; }
        public string FeeType { get; set; }
        public string TransactionDate { get; set; }
        public string Session { get; set; }
        public string InvoiceNumber { get; set; }
        public string TransactionAmount { get; set; }
        public string TransactionDescription { get; set; }
        public string RRR { get; set; }
       

    }
}