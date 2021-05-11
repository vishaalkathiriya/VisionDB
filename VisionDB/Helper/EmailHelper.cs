using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace VisionDB.Helper
{
    public class EmailHelper
    {
        const string Host = "smtp.mailgun.org"; //todo: move to config file
        const int Port = 25;
        const string Username = "postmaster@rsff13bf2d44704e0f8fa6e72a15696336.mailgun.org"; //todo: move to config file
        const string Password = "4zg8obaskas2"; //todo: move to config file

        internal void SendEmail(string ToEmailAddress, string Subject, string Message, string SenderEmailAddress, bool IsBodyHtml)
        {
            SmtpClient client = new SmtpClient(Host, Port);
            client.Credentials = new System.Net.NetworkCredential(Username, Password);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;

            MailMessage email = new MailMessage(SenderEmailAddress, ToEmailAddress, Subject, Message);
            email.IsBodyHtml = IsBodyHtml;
            client.Send(email);
        }

        internal void SendEmail(string ToEmailAddress, string Subject, string Message, string SenderEmailAddress, Stream file, string fileName, bool IsBodyHtml)
        {
            SmtpClient client = new SmtpClient(Host, Port);
            client.Credentials = new System.Net.NetworkCredential(Username, Password);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;

            MailMessage email = new MailMessage(SenderEmailAddress, ToEmailAddress, Subject, Message);
            Attachment attachment = new Attachment(file, fileName);
            email.Attachments.Add(attachment);
            email.IsBodyHtml = IsBodyHtml;
            client.Send(email);
        }  
    }
}