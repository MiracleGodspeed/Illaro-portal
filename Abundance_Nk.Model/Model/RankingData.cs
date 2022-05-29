﻿using System;

namespace Abundance_Nk.Model.Model
{
    public class RankingData
    {
        public Int64 Id {get; set;}
        public Person Person { get; set; }
        public string Subj1 { get; set; }
        public string Subj2 { get; set; }
        public string Subj3 { get; set; }
        public string Subj4 { get; set; }
        public string Subj5 { get; set; }
        public decimal? Total { get; set; }
        public bool? Qualified { get; set; }
        public string Reason { get; set; }

    }
}
