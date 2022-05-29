using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Model
{
    public class HostelRequest
    {
        public int Id { get; set; }
        public Student Student { get; set; }
        public Programme Programme { get; set; }
        public Department Department { get; set; }
        public Level Level { get; set; }
        public Session Session { get; set; }
        public bool Approved { get; set; }
        public bool Remove { get; set; }
        public bool? Expired { get; set; }
        public string Reason { get; set; }
        public DateTime RequestDate { get; set; }
        public Person Person { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public Hostel Hostel { get; set; }
        public Payment Payment { get; set; }
        public User User { get; set; }
    }
}
