using Abundance_Nk.Model.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Entity.Model
{
    public class ClearanceLog
    {
        public long Id { get; set; }
        public string Remarks { get; set; }
        public Nullable<System.DateTime> DateCleared { get; set; }
        public string Client { get; set; }
        public bool Closed { get; set; }
        public Student Student { get; set; }
        public ClearanceStatus ClearanceStatus { get; set; }
        public ClearanceUnit ClearanceUnit { get; set; }
        public User User { get; set; }
        public bool IsDisputed { get; set; }
    }
}
