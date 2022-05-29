using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Entity.Model
{
    public class FeedbackStore
    {
        public long Id { get; set; }
        public LiveLectures LiveLectures { get; set; }
        public string Comments { get; set; }
        public bool Active { get; set; }

    }
}
