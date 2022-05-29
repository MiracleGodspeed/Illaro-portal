using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace Abundance_Nk.Model.Model
{
   public class Confrence
    {

        public int Confrence_Id { get; set; }
        public long Person_Id { get; set; }
        public string Department { get; set; }
        public string Institution { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Postal_Code { get; set; }
        public string Country { get; set; }
        public string Research_DocumentUrl { get; set; }
        public string Amount_Package { get; set; }
        public System.DateTime Date_Applied { get; set; }
        public string Status { get; set; }
        public Person Person { get; set; }
        public Payment Payment { get; set; }

       
    }
}
