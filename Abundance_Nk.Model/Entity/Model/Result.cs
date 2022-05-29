using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace Abundance_Nk.Model.Model
{
    public class Result
    {
        public long StudentId { get; set; }
        public string Sex { get; set; }
        public string Name { get; set; }
        public string MatricNumber { get; set; }
        public string LevelName { get; set; }
        public long SessionSemesterId { get; set; }
        public int SessionSemesterSequenceNumber { get; set; }
        public string SessionName { get; set; }
        public string Semestername { get; set; }
        public string DepartmentName { get; set; }
        public string PassportUrl { get; set; }
        public string TypeName { get; set; }
        public string ProgrammeName { get; set; }
        public long CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public int CourseUnit { get; set; }
        public decimal? Score { get; set; }
        public double? AggregatedScore { get; set; }
        public string Grade { get; set; }
        public decimal? MaxSoreObtainable { get; set; }
        public string FacultyName { get; set; }
        public decimal? TestScore { get; set; }
        public decimal? ExamScore { get; set; }
        public decimal? GradePoint { get; set; }
        public decimal? GPCU { get; set; }
        public decimal? CGPA { get; set; }
        public decimal? GPA { get; set; }
        public decimal? WGP { get; set; }
        public int? ProgrammeId { get; set; }
        public int? UnitPassed { get; set; }
        public int? UnitOutstanding { get; set; }
        public int? TotalSemesterCourseUnit { get; set; }
        public int SessionId { get; set; }
        public int LevelId { get; set; }
        public string Address { get; set; }
        public string MobilePhone { get; set; }
        public string Email { get; set; }
        public int Student_Type_Id { get; set; }
        public string Surname { get; set; }
        public string Firstname { get; set; }
        public string Othername { get; set; }
        public decimal? TotalScore { get; set; }
        public string firstname_middle { get; set; }
        public string Reason { get; set; }
        public bool? Activated { get; set; }
        public string RejectCategory { get; set; }
        public string GraduationStatus { get; set; }
        public string StudentTypeName { get; set; }
        public string SpecialCase { get; set; }
        public string Remark { get; set; }
        public string Identifier { get; set; }
        public int CourseModeId { get; set; }
        public string CourseModeName { get; set; }
        public decimal FirstSemesterGPA { get; set; }
        public decimal SecondSemesterGPA { get; set; }
        public decimal ThirdSemesterGPA { get; set; }
        public decimal FourthSemesterGPA { get; set; }
        public string CourseMode { get; set; }
        public string DepartmentOptionName { get; set; }
        public int? DepartmentOptionId { get; set; }
        public int SemesterId { get; set; } 
        public int Count { get; set; }
        public string TranscriptLevel { get; set; }
        public string TranscriptSession { get; set; }
        public string TranscriptSemester { get; set; }
        public int DepartmentId { get; set; }
        public string GraduationDate { get; set; }

        public bool isCarryOver()
        {
            if (((TestScore ?? 0) + (ExamScore ?? 0) < 40)  && SpecialCase == null)
                return true;
            else
                return false;
        }

        public bool isSpecialCase()
        {
            return SpecialCase != null;
        }

        public bool ContainsResult(List<Result> results)
        {
            try
            {
                return results.Where(r => r.CourseId == CourseId).LastOrDefault() != null;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
    public class Probation
    {
        public Level Level { get; set; }
        public Semester Semester { get; set; }
        public decimal CGPA { get; set; }
    }
    public class PartialRemark
    {
        public string Remark { get; set; }
        //public int Count { get; set; }
    }
}
