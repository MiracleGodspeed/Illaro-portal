using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Entity.Model
{
    public class ResponseModel
    {
        public int statusCode { get; set; } = 000;
        public string message { get; set; } = "not resolved";
        public List<FeedbackStore> FeedbackList { get; set; }
    }
}
