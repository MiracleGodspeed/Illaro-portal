using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Model
{
    public class Course
    {
        public long Id { get; set; }
        public CourseType Type { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public Level Level { get; set; }
        public Department Department { get; set; }
        public DepartmentOption DepartmentOption { get; set; }
        public int Unit { get; set; }
        public Semester Semester { get; set; }
        public bool IsRegistered { get; set; }
        public bool isCarryOverCourse { get; set; }
        public bool? Activated { get; set; }
    }


}
