using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;


namespace Abundance_Nk.Model.Model
{
    public class InstitutionChoice
    {
        [Display(Name = "Choice")]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
