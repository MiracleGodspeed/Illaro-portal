using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Abundance_Nk.Web.Api.DTO
{
    public class AssignmentByCategoryDTO
    {
        public List<AssignmentDto> SubmittedAssignment { get; set; }
        public List<AssignmentDto> NotSubmittedAssignment { get; set; }
    }
}