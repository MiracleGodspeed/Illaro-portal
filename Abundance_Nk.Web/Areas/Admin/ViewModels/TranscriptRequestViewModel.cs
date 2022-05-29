using Abundance_Nk.Model.Model;
using Abundance_Nk.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Abundance_Nk.Web.Areas.Admin.ViewModels
{
    public class TranscriptRequestViewModel
    {
        public TranscriptRequestViewModel()
        {
            TranscriptStatusSelectItem = Utility.PopulateTranscriptStatusSelectListItem();
            DepartmentSelectListItem = Utility.PopulateDepartmentSelectListItem();
        }
        public List<GroupTranscriptByYear> GroupTranscriptByYears { get; set; }
        public List<GroupTranscriptByMonth> GroupTranscriptByMonths { get; set; }
        public List<TranscriptRequest> TranscriptRequests { get; set; }
        public List<SelectListItem> TranscriptStatusSelectItem { get; set; }
        public TranscriptStatus TranscriptStatus { get; set; }
        public List<SelectListItem> DepartmentSelectListItem { get; set; }
        //public List<TranscriptIncidentLog> TranscriptIncidentLogs { get; set; }
        //public TranscriptIncidentLog TranscriptIncidentLog { get; set; }
        public TranscriptRequest TranscriptRequest { get; set; }
        public string MatricNo { get; set; }
        public List<StudentLevel> StudentLevels { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public bool ShowClosedTicket { get; set; }
        public bool Active { get; set; }
        public string RequestType { get; set; }
        public Model.Model.Student Student { get; set; } 
        public StudentLevel StudentLevel { get; set; }


    }
    public class GroupTranscriptByYear
    {
        public int Year { get; set; }
        public int TranscriptCount { get; set; }
    }
    public class GroupTranscriptByMonth
    {
        public int TranscriptCount { get; set; }
        public string Month { get; set; }
        public int intMonth { get; set; }
        public int Year { get; set; }
    }
}
