using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Abundance_Nk.Web.Api.DTO
{
    public class CourseRegistrationDto
    {
        public long CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public long CourseAllocationId { get; set; }
    
    }
}