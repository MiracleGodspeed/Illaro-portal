using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Abundance_Nk.Model.Model
{
    public class StudentPayment : Payment
    {
        public Student Student { get; set; }
        public Level Level { get; set; }
        public SessionSemester SessionSemester { get; set; }

    }


}
