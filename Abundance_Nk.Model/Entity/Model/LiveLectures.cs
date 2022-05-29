using Abundance_Nk.Model.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Entity.Model
{
    public class LiveLectures
    {
        public long Id { get; set; }
        public string Topic { get; set; }
        public string Agenda { get; set; }
        public int Duration { get; set; }
        public int? Time { get; set; }
        public string Start_Meeting_Url { get; set; }
        public string Join_Meeting_Url { get; set; }
        public DateTime? LectureDate { get; set; }
        public DateTime DateCreated { get; set; }
        public CourseAllocation CourseAllocation { get; set; }
        public Programme Programme { get; set; }
        public Department Department { get; set; }
        public Session Session { get; set; }
        public Level Level { get; set; }
    }
}
