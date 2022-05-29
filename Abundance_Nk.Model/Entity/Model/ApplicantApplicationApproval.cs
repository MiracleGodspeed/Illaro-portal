using Abundance_Nk.Model.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Model
{
    public class ApplicantApplicationApproval
    {
        public ApplicationForm ApplicationForm { get; set; }
        public User User { get; set; }
        public string Remark { get; set; }
        public DateTime DateTreated { get; set; }
        public bool IsApproved { get; set; }
        public string ClearanceCode { get; set; }


        
    }
}
