using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Entity.Model
{
    public class OdFelStudentFormat
    {
        public string Last_Name { get; set; }
        public string First_Name { get; set; }
        public string Other_Name { get; set; }
        public string FullName { get; set; }
        public string Matric_Number { get; set; }
        public string Application_Form_Number { get; set; }
        public int Department_Id { get; set; }
        public int Programme_Id { get; set; }
        public int Session_Id { get; set; }
        public int Level_Id { get; set; }
        public string Department_Name { get; set; }
        public string Programme_Name { get; set; }
        public byte Sex_Id { get; set; }
        public string Sex_Name { get; set; }
    }
}
