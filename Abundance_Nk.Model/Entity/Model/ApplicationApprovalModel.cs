using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Model
{
    public class ApplicationApprovalModel
    {
        public string FormNo { get; set; }
        public int DepartmentId { get; set; }
        public int ProgrammeId { get; set; }
        public string DepartmentName { get; set; }
        public string ProgrammeName { get; set; }
        public string OptionName { get; set; }
        public string ApplicantFullName { get; set; }
        public long FormId { get; set; }
        public long? ApproveOfficerId { get; set; }
        public string ApproveOfficerUsername { get; set; }
        public string ApproveOfficerEmail { get; set; }
        public DateTime? DateApproved { get; set; }
        public string ApprovalRemarks { get; set; }
        public int? OptionId { get; set; }
        public string ClearanceCode { get; set; }
        public bool? IsApproved { get; set; }
        public long PersonId { get; set; }
        public long? TreatedFormId { get; set; }
        public string Qualified { get; set; }
        public string SessionName { get; set; }
        public string Title { get; set; }

    }
}
