using Abundance_Nk.Model.Entity.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Abundance_Nk.Web.Areas.Admin.ViewModels
{
    public class StudentClearanceViewModel
    {
        public ClearanceStatus ClearanceStatus { get; set; }
        public ClearanceDisputes ClearanceDisputes { get; set; }
        public Model.Model.Student Student { get; set; }
        public Model.Model.StudentLevel StudentLevel { get; set; }
        public ClearanceLog ClearanceLog { get; set; }
        public bool ViewPanel { get; set; }
        public List<ClearanceLog> ClearanceLogs { get; set; }
        public List<ClearanceUnit> ClearanceUnits { get; set; }
        public HttpPostedFileBase MyFile { get; set; }
        public List<ClearanceDisputes> ClearanceDisputesList { get; set; }
        
    }
}