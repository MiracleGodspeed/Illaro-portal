using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Model
{
    public class ShortFall
    {
        public long Id { get; set; }
        public Payment Payment { get; set; }
        public double Amount { get; set; }
    }
}
