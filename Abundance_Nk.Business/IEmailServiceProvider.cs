using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace Abundance_Nk.Business
{
    public interface IEmailServiceProvider
    {
        IRestResponse Send(string email, string emailBody);
        string TemplateSetUp<T>(T model, string templateFilePath) where T : class;

    }
}
