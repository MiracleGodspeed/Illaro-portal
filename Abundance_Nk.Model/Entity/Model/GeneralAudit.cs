using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Model
{
    public class GeneralAudit
    {
        public long Id { get; set; }
        public string TableNames { get; set; }
        public string InitialValues { get; set; }
        public string CurrentValues { get; set; }
        public User User { get; set; }
        public string Operation { get; set; }
        public string Action { get; set; }
        public System.DateTime Time { get; set; }
        public string Client { get; set; }
        public string Username { get; set; }
        public bool IsSuperAdmin { get; set; }
        public string Email { get; set; }
        public string OldEmail { get; set; }
        public string Role { get; set; }
        public int RoleId { get; set; }
        public string Date { get; set; }
        public long UserId { get; set; }
        public string FirstName { get; set; }
        public string OldFirstName { get; set; }
        public string LastName { get; set; }
        public string OldLastName { get; set; }
        public string OtherName { get; set; }
        public string OldOtherName { get; set; }
        public string Title { get; set; }
        public string OldTitle { get; set; }
        public string ContactAddress { get; set; }
        public string OldContactAddress { get; set; }
        public string MobilePhone { get; set; }
        public string OldMobilePhone { get; set; }
        public string DOB { get; set; }
        public string OldDOB { get; set; }
        public string Sex { get; set; }
        public string OldSex { get; set; }
        public string State { get; set; }
        public string OldState { get; set; }
        public string LGA { get; set; }
        public string OldLGA { get; set; }
        public string HomeTown { get; set; }
        public string OldHomeTown { get; set; }
        public string HomeAddress { get; set; }
        public string OldHomeAddress { get; set; }
        public string Religion { get; set; }
        public string OldReligion { get; set; }
        public string Programme { get; set; }
        public string OldProgramme { get; set; }
        public string Department { get; set; }
        public string OldDepartment { get; set; }
        public string DepartmentOption { get; set; }
        public string OldDepartmentOption { get; set; }
        public string ApplicationForm { get; set; }
        public string OldApplicationForm { get; set; }
    }
}
