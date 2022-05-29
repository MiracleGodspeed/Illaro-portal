using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Model
{
   public class StudentResultStatus
    {
       public int Id { get; set; }
       public Faculty Faculty { get; set; }
        public Programme Programme { get; set; }
        public Session Session { get; set; }
        public Semester Semester { get; set; }
        public bool RAndDCApproval { get; set; }
        public bool DRAcademicsApproval { get; set; }
        public bool RegistrarApproval { get; set; }
    }
}
