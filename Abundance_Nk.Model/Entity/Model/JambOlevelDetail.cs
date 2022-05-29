using Abundance_Nk.Model.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Entity.Model
{
    public class JambOlevelDetail
    {
        public long Id { get; set; }
        public JambOlevel JambOlevel { get; set; }
        public OLevelSubject OLevelSubject { get; set; }
        public OLevelGrade OLevelGrade { get; set; }
    }
}
