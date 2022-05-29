using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Entity.Model
{
    public class LicenseType
    {
        public int Id { get; set; }
        [Display(Name = "License Type")]
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }
    }
}
