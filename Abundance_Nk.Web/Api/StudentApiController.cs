using Abundance_Nk.Business;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Web.Api.DTO;
using Abundance_Nk.Web.Areas.Admin.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Abundance_Nk.Web.Api
{
    public class StudentApiController : ApiController
    {
        [HttpGet]
        public IHttpActionResult GetStudentDetails(string matricNo)
        {
            StudentLogic studentLogic = new StudentLogic();
            PersonLogic personLogic = new PersonLogic();
            AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
            StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
            PreviousEducationLogic previousEducationLogic = new PreviousEducationLogic();
            SponsorLogic sponsorLogic = new SponsorLogic();

            var student = studentLogic.GetModelBy(x => x.Matric_Number == matricNo.Trim());
            var person = personLogic.GetModelBy(x => x.Person_Id == student.Id);
            var applicant = appliedCourseLogic.GetModelBy(x => x.Person_Id == student.Id);
            var studentlevel = studentLevelLogic.GetModelsBy(x => x.Person_Id == person.Id).LastOrDefault();
            var applicantPreviousEducation = previousEducationLogic.GetModelBy(x => x.Person_Id == person.Id);
            var applicantsponspor = sponsorLogic.GetModelBy(x => x.Person_Id == person.Id);

            var CheckedPreviousEducation = CheckForPreviousDetails(applicantPreviousEducation);
            if (student == null)
            {
                return Ok(new { Output = "Student Does Not Exist" });
            }
            PaymentLogic paymentLogic = new PaymentLogic();
            List<Payment> payments = paymentLogic.GetModelsBy(x => x.Person_Id == student.Id);
            List<PaymentBreakDown> paymentDto = new List<PaymentBreakDown>();
            foreach(var item in payments)
            {
                RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                var remita = remitaPaymentLogic.GetModelBy(x => x.Payment_Id == item.Id);
                if (remita != null && remita.Status == "01:")
                {
                    PaymentBreakDown paymentBreakDown = new PaymentBreakDown()
                    {
                        TransactionAmount = Convert.ToString(remita.TransactionAmount),
                        TransactionDate = Convert.ToString(remita.TransactionDate),
                        FeeType = item.FeeType.Name,
                        InvoiceNumber = item.InvoiceNumber,
                        PaymentMode = item.PaymentMode.Name,
                        PaymentType = item.PaymentType.Name,
                        Session = item.Session.Name,
                       
                        RRR = remita.RRR,
                     
                        TransactionDescription = remita.Description
                    };
                    paymentDto.Add(paymentBreakDown);
                }
            }
           
         

            Studentdetails model = new Studentdetails()
            {
               FullName = person.FullName,
               MatricNo = student.MatricNumber,
               ContactAddress = person.ContactAddress,
               DateOfBirth = Convert.ToString(person.DateOfBirth),
               DepartmentName = applicant.Department.Name,
               Email = person.Email,
               HomeAddress = person.HomeAddress,
               HomeTown= person.HomeTown,
               ImageFileUrl = "https://applications.federalpolyilaro.edu.ng"+person.ImageFileUrl,
               Level = studentlevel.Level.Name,
               MobilePhone = person.MobilePhone,
               Nationality = person.Nationality.Name,
               programmeName = studentlevel.Programme.Name,
               Religion = person.Religion.Name,
               Sex = person.Sex.Name,
               State= person.State.Name,
               ApplicantSponsorName = applicantsponspor.Name,
               ApplicantSponsorContactAddress = applicantsponspor.ContactAddress,
               ApplicantSponsorMobilePhone = applicantsponspor.MobilePhone,
               Relationship = applicantsponspor.Relationship.Name,
               PreviousCourse = CheckedPreviousEducation.Course,
               EndDate = Convert.ToString(CheckedPreviousEducation.EndDate),
               StartDate = Convert.ToString(CheckedPreviousEducation.StartDate),
               ITDuration = CheckedPreviousEducation.ITDuration.Name,
               Qualification = CheckedPreviousEducation.Qualification.Name,
               SchoolName = CheckedPreviousEducation.SchoolName,
               ResultGrade = CheckedPreviousEducation.ResultGrade.LevelOfPass,
               ResultCopyUrl = CheckedPreviousEducation.ResultCopyUrl!=null ? "https://applications.federalpolyilaro.edu.ng"+ CheckedPreviousEducation.ResultCopyUrl: CheckedPreviousEducation.ResultCopyUrl,
               CertificateCopyUrl = CheckedPreviousEducation.CertificateCopyUrl != null ? "https://applications.federalpolyilaro.edu.ng" + CheckedPreviousEducation.CertificateCopyUrl: CheckedPreviousEducation.CertificateCopyUrl,
               ITLetterOfCompletion = CheckedPreviousEducation.ITLetterOfCompletion != null ? "https://applications.federalpolyilaro.edu.ng" + CheckedPreviousEducation.CertificateCopyUrl: CheckedPreviousEducation.CertificateCopyUrl,

               payments = paymentDto

            };

            return Ok(new { Output = model });
        }



        public PreviousEducation CheckForPreviousDetails (PreviousEducation model)
        {
           // string baseUrl = "https://applications.federalpolyilaro.edu.ng";
          //  string baseUrl = "http://localhost:2600";
            string baseUrl = "~";
            string ResultUrl = baseUrl + model.ResultCopyUrl;
            string CertificateUrl = baseUrl + model.CertificateCopyUrl;
            string ItLetter = baseUrl + model.ITLetterOfCompletion;

            if (!File.Exists(HttpContext.Current.Server.MapPath(ResultUrl)))
            {
                model.ResultCopyUrl = null;
            }
            if (!File.Exists(HttpContext.Current.Server.MapPath(CertificateUrl)))
            {
                model.CertificateCopyUrl = null;
            }
            if (!File.Exists(HttpContext.Current.Server.MapPath(ItLetter)))
            {
                model.ITLetterOfCompletion = null;
            }
            return model;
        }

    }
}