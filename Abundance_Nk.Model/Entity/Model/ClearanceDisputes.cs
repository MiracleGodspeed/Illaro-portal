using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Entity.Model
{
    public class ClearanceDisputes
    {
        public long Id { get; set; }
        public string Remark { get; set; }
        public string Attachment { get; set; }
        public bool IsStudent { get; set; }
        public DateTime DateSent { get; set; }

        public ClearanceLog ClearanceLog { get; set; }
    }
}
