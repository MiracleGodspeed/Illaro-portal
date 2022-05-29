using Abundance_Nk.Model.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Abundance_Nk.Web.Areas.Common.Models
{
    public class StudentIDModel
    {
        public string Name { get; set; }
        public string MatricNo { get; set; }
        public string BloodGroup { get; set; }
        public string Department { get; set; }
        public string Programme { get; set; }
        public string PassportUrl { get; set; }
        public int GraduationYear { get; set; }
        public long PersonId { get; set; }
        public List<PaymentView> Payments { get; set; }
    }

}