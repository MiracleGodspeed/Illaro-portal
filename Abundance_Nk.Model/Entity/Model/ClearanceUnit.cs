using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Entity.Model
{
    public class ClearanceUnit
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public string StampUnit { get; set; }
        public string status { get; set; }
        public long LogId { get; set; }
    }
}
