using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abundance_Nk.Model.Model;

namespace Abundance_Nk.Model.Entity.Model
{
    public class DepartmentCapacity
    {
        public int Id { get; set; }
        public Programme Programme { get; set; }
        public Department Department { get; set; }
        public int Capacity { get; set; }
        public Session Session { get; set; }
        public bool Activated { get; set; }
    }
}
