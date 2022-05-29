using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Entity.Model
{
    public class AlumniRecord
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string OtherName { get; set; }
        public string PhoneNo { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }
        public string WorkPlace { get; set; }
        public string GraduationYear { get; set; }
        public string DepartmentName { get; set; }
        public string ProgrammeName { get; set; }
        public string MatricNumber { get; set; }
        public string AlumniNumber { get; set; }
    }
    public class StudentRecordDTO
    {
        public string StudentName { get; set; }
        public string MatricNumber { get; set; }
        public string Email { get; set; }
        public string Passport { get; set; }
        public string Gender { get; set; }

    }
}
