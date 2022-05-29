using Abundance_Nk.Model.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Entity.Model
{
    public class AggregateSheet
    {
        public string MatricNumber { get; set; }
        public string Fullname { get; set; }
        public string Programme { get; set; }
        public string Faculty { get; set; }
        public string Department { get; set; }
        public string Level { get; set; }
        public string Gender { get; set; }
        public string Phone { get; set; }
        public string Session { get; set; }
        public string Semester { get; set; }
        public List<AggregateCourse> Courses { get; set; }
        public int TotalUnitAttempted { get; set; }
        public int TotalUnitPassed { get; set; }
        public int TotalUnitOutstanding { get; set; }
        public decimal TotalWeightedGradePoint { get; set; }
        public decimal GPA { get; set; }
        public bool Activated { get; set; }
        public string Remark { get; set; }
        public long PersonId { get; set; }
        public int ProgrammeId { get; set; }
        public int FacultyId { get; set; }
        public int DepartmentId { get; set; }
        public int LevelId { get; set; }
        public int SessionId { get; set; }
        public int SemesterId { get; set; }
    }

    public class AggregateCourse
    {
        public long courseId { get; set; }
        public int courseUnit { get; set; }
        public decimal ExamScore { get; set; }
        public decimal TestScore { get; set; }
        public decimal TotalScore { get; set; }
        public string Grade  { get; set; }
        public string CourseCode { get; set; }
        public decimal WGP { get; set; }
    }
    
    public class AggregateStanding
    {
        public decimal FirstSemesterGPA { get; set; }
        public decimal SecondSemesterGPA { get; set; }
        public decimal ThirdSemesterGPA { get; set; }
        public decimal FourthSemesterGPA { get; set; }
        public decimal GPA { get; set; }
        public decimal CGPA { get; set; }
    }
}
