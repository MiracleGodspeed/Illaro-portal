using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Abundance_Nk.Web.Api.DTO
{
    public class CourseTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class AssignmentUpload
    {
        public long PersonId { get; set; }
        public string AssignmentText { get; set; }
        public int AssignmentId { get; set; }
        public HttpPostedFileBase file { get; set; }

    }
}