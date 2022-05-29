using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Model
{
    public class StudentStatisticReport
    {
       
        public int TotalSex { get; set; }
        public int TotalFemale { get; set; }
        public int TotalMale { get; set; }
        public int TotalGenderUnknown { get; set; }
        public string State { get; set; }
        public int TotalCount { get; set; }
        public string ProgrammeName { get; set; }
        public string DepartmentName { get; set; }
        public int ProgrammeId { get; set; }
        public string Genotype { get; set; }
        public string SessionName { get; set; }
        public string LevelName { get; set; }
    }
}
