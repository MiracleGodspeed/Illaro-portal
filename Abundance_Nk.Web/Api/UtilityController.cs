using Abundance_Nk.Business;
using Abundance_Nk.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Abundance_Nk.Web.Api
{
    public class UtilityController : ApiController
    {
        private readonly SessionLogic sessionLogic;
        private readonly ApplicationFormLogic applicationFormLogic;
        public UtilityController()
        {
            sessionLogic = new SessionLogic();
            applicationFormLogic = new ApplicationFormLogic();
        }
        [HttpGet]
        public async void SendApplicationFormNotification()
        {
            var activeApplicationSession=sessionLogic.GetModelsBy(f => f.Active_For_Application == true).LastOrDefault();
            if (activeApplicationSession?.Id > 0)
            {
                DateTime toDate = DateTime.UtcNow.Date;
                DateTime fromDate = toDate.AddDays(-7);
                var returnObject=applicationFormLogic.GetApplicationFormCountAtIntervals(fromDate, toDate, activeApplicationSession);
                if (returnObject != null)
                {
                    PrincipalOfficersLogic principalOfficersLogic = new PrincipalOfficersLogic();
                    List<string> phoneNos =principalOfficersLogic.GetModelsBy(f => f.Active).Select(f=>f.PhoneNo).ToList();
                    if (phoneNos?.Count > 0)
                    {
                        //List<string> phoneNos =new List<string> { "2348067486937", "2348064232915", "2348085761493" };
                        string message = "";
                        string to = toDate.ToString();
                        string from = fromDate.ToString();
                        string sessionId = activeApplicationSession.Id.ToString();
                        message = "Good day, the total application count, for the period " + returnObject.DateFrom + " and " + returnObject.DateTo + " is: " + returnObject.FormCount + ". Login for details. https://applications.federalpolyilaro.edu.ng/admin/support/ApplicationFormSummary?from=" + from + "&to=" + to + "&ses=" + sessionId;
                        foreach (var item in phoneNos)
                        {

                            SmartSMS smartSMS = new SmartSMS();
                            smartSMS.sender = "POLYILARO";
                            smartSMS.to = item;
                            smartSMS.message = message;
                            smartSMS.type = 0;
                            smartSMS.routing = 3;
                            SmartSmsLogic smartSmsLogic = new SmartSmsLogic();
                            await smartSmsLogic.SendSingleSmsMessage(smartSMS);
                        }
                    }
                    
                    
                }
            }
        }
    }
}
