using Abundance_Nk.Model.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Entity.Model
{
    public class LiveLecturesAttendance
    {
        public long Id { get; set; }
        public LiveLectures LiveLectures { get; set; }
        public CourseRegistrationDetail CourseRegistrationDetail { get; set; }
        public Student Student { get; set; }
    }
}
