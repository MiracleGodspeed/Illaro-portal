using Abundance_Nk.Business;
using Abundance_Nk.Web.Api.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
//using System.Net.Http;
using System.Web.Http;
//using System.Web.Mvc;

namespace Abundance_Nk.Web.Api
{
    public class StaffController : ApiController
    {
        private StaffLogic staffLogic;
        private string Key = "jhfh8882-0909-ilaro";
        private UserLogic userLogic;
        public StaffController()
        {
            staffLogic = new StaffLogic();
            userLogic = new UserLogic();
        }
        [HttpGet]
        public IHttpActionResult StaffDetail(string key)
        {
            List<StaffDetailDto> staffDetailDtos = new List<StaffDetailDto>();
            if (key == Key)
            {
                staffDetailDtos=staffLogic.GetAll().Select(f => new StaffDetailDto
                {
                    Email = f.Email,
                    FirstName = f.FirstName,
                    LastName = f.LastName,
                    Password = f.User.Password,
                    Username = f.User.Username,
                    Gender = f.Sex?.Id > 0 ? f.Sex.Name : "N/A",
                }).ToList();
                return Ok(new { Output = staffDetailDtos });
            }
            else
            {
                return Ok(new { Output = "Not Authorized" });
            }

        }
        [HttpGet]
        public IHttpActionResult StaffLogin(string username, string password)
        {
            StaffDetailDto staffDetailDto = new StaffDetailDto();
            Response response = new Response();
            response.StaffDetailDto = new StaffDetailDto();
            if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
            {
                var user = userLogic.GetModelsBy(f => f.User_Name == username && f.Password == password).FirstOrDefault();
                if (user?.Id > 0)
                {
                    response.Validated = true;
                  response.StaffDetailDto=  staffLogic.GetModelsBy(f=>f.User_Id==user.Id).Select(f => new StaffDetailDto
                  {
                        Email = f.Email,
                        FirstName = f.FirstName,
                        LastName = f.LastName,
                        Password = f.User.Password,
                        Username = f.User.Username,
                        Gender = f.Sex?.Id > 0 ? f.Sex.Name : "N/A",
                    }).FirstOrDefault();
                    return Ok(new { Output = response });
                }
                else
                {
                    return Ok(new { Output = response });
                }
            }
            else
            {
                return Ok(new { Output = response });
            }
        }
    }
}
