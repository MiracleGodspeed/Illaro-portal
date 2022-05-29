using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Business
{
    public class SmartSmsLogic
    {
        private static readonly HttpClient client = new HttpClient();
        private string token = ConfigurationManager.AppSettings["SmartSmsToken"];
        private string BaseUrl = ConfigurationManager.AppSettings["SmartSmsBaseUrl"];

        private string _token;
        public SmartSmsLogic(string token)
        {
            _token = token;
        }

        public SmartSmsLogic() { }
        public async Task<string> SendSingleSmsMessage(SmartSMS smartSMS)
        {
            var dataPayload = new Dictionary<string, string>
            {

                { "sender", smartSMS.sender },
                { "to", smartSMS.to },
                { "message", smartSMS.message },
                { "token", token },
                { "type", smartSMS.type.ToString() },
                { "routing", smartSMS.routing.ToString() }
            };
            const SslProtocols _Tls12 = (SslProtocols)0x00000C00;
            const SecurityProtocolType Tls12 = (SecurityProtocolType)_Tls12;
            ServicePointManager.SecurityProtocol = Tls12;
            var content = new FormUrlEncodedContent(dataPayload);
            var response = await client.PostAsync(BaseUrl, content);

            return await response.Content.ReadAsStringAsync();




        }
    }
    public class SmartSMS
    {
        public string sender { get; set; }
        public string to { get; set; }
        public string message { get; set; }
        public int type { get; set; }
        public int routing { get; set; }
    }
}

