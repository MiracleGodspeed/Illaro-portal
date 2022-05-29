using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Abundance_Nk.Web.Api.DTO
{
 
    public class Response
    {
        public StaffDetailDto StaffDetailDto { get; set; }
        public bool Validated { get; set; }
    }
}