using Abundance_Nk.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Abundance_Nk.Web.Api
{
    public class AlumniRecordController : ApiController
    {
        private TranscriptRequestLogic transcriptRequestLogic;
        private StudentLogic studentLogic;

        public AlumniRecordController()
        {
            transcriptRequestLogic = new TranscriptRequestLogic();
            studentLogic = new StudentLogic();
        }
        public HttpResponseMessage GetAllAlumni()
        {
            var allAlumni=transcriptRequestLogic.GetAlumniRecord();
            return Request.CreateResponse(allAlumni);

        }
        public HttpResponseMessage GetByName(string name)
        {
            var allumniByName = transcriptRequestLogic.GetAlumniRecordByName(name);
            return Request.CreateResponse(allumniByName);
        }
        public HttpResponseMessage GetAllStudentRecord()
        {
            var studentRecord = studentLogic.GetAllStudentRecord();
                return Request.CreateResponse(studentRecord);
        }
    }
}
