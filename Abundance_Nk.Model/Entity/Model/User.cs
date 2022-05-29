using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Model
{
    public class User
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public SecurityQuestion SecurityQuestion { get; set; }
        public string SecurityAnswer { get; set; }
        public Role Role { get; set; }
        public DateTime LastLoginDate { get; set; }
        public bool? Activated { get; set; }
        public bool ActivatedCheck { get; set; }
        public bool? PasswordChanged { get; set; }
        public bool? SuperAdmin { get; set; }
        public string SignatureUrl { get; set; }
        public string ProfileImageUrl { get; set; }
        public string Department { get; set; }
        public bool Archieved { get; set; }
    }


}
