using Abundance_Nk.Model.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Entity.Model
{
    public class JambOlevel
    {
        public long Id { get; set; }
        public string Exam_Number { get; set; }
        public int? Exam_Year { get; set; }
        public JambRecord JambRecord { get; set; }
        public OLevelExamSitting ExamSitting { get; set; }
        public OLevelType ExamType { get; set; }
    }
}
