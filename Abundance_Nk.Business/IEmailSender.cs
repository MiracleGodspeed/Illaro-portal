using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abundance_Nk.Model.Model;

namespace Abundance_Nk.Business
{
    interface IEmailSender
    {
        void Send(EmailMessage message);
        void Send(List<EmailMessage> messages);
    }
}
