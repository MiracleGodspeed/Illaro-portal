using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Entity.Model
{
    public class AdmissionListSummary
    {
        public int ProgrammeId { get; set; }
        public string ProgrammeName { get; set; }
        public string DepartmentName { get; set; }
        public string SessionName { get; set; }
        public int FormCount { get; set; }
    }
}
