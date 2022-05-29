using Abundance_Nk.Model.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Entity.Model
{
    public class DrivingExperience
    {
        public long Id { get; set; }
        public int Licence_Type_Id { get; set; }
        public long Application_Form_Id { get; set; }
        public LicenseType LicenseType {get; set;}
        public ApplicationForm ApplicationForm { get; set; }
        public Person Person { get; set; }
        public long PersonId { get; set; }
        public DateTime? IssuedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        [Display(Name = "Height(meters)")]
        public decimal? Height { get; set; }
        public bool FacialMarks { get; set; }
        [Display(Name = "Years Of Experience")]
        public int YearsOfExperience { get; set; }
        [Display(Name = "License Number")]
        public string LicenseNumber { get; set; }

    }
}
