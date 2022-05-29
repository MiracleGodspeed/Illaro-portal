using System;
using System.IO;
using RazorEngine;
using RazorEngine.Templating;
using RestSharp;
using RestSharp.Authenticators;

namespace Abundance_Nk.Business
{
   public class EmailServiceProvider : IEmailServiceProvider
    {
        public IRestResponse Send(string emailAddress, string emailBody)
        {
            RestClient client = new RestClient();
            RestRequest request = new RestRequest();
            try
            {
                client.BaseUrl = new Uri("https://api.mailgun.net/v3");
                client.Authenticator = new HttpBasicAuthenticator("api", "key-06b6db8ba1fa36f7dfddcff9d7c54040");
                request.AddParameter("domain", "audme.dreamteam.com.ng", ParameterType.UrlSegment);
                request.Resource = "{domain}/messages";
                request.AddParameter("from", "Federal Poly Ilaro");
                request.AddParameter("to", emailAddress);
                request.AddParameter("subject", "Account Notification");
                request.AddParameter("html", emailBody);
                request.Method = Method.POST;
            }
            catch (Exception)
            {

                throw;
            }
            return client.Execute(request);
        }

        public string TemplateSetUp<T>(T templatemodel, string filePath) where T : class
        {
            string result = null;
            try
            {

                string fileName = Path.GetFileName(filePath);
                string folderName = Path.GetDirectoryName(filePath);
                if (folderName != null && folderName.Contains("\\"))
                {
                    folderName = folderName.Replace("\\", "");
                }
                if (!string.IsNullOrEmpty(folderName) && !string.IsNullOrEmpty(fileName))
                {
                    var templateFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folderName);
                    var templateFilePath = templateFolderPath + "/" + fileName;
                    // Model Used in your email
                    var model = templatemodel;
                    var key = DateTime.Now.Ticks.ToString();
                    //Get Template
                    var template = File.ReadAllText(templateFilePath);
                    // Generate the email body from the template file.
                    // 'templateFilePath' should contain the absolute path of your template file.
                    result = Engine.Razor.RunCompile(template, key, null, model);
                }

            }
            catch (Exception ex)
            {

                throw;
            }
            return result;
        }
    }
}
