using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace LibraryManagement.Services.Auth
{
    public class EmailService
    {
        public void SeedEmail(string to, string subject, string body)
        {
            SmtpClient smtpClient = new SmtpClient("library.com", 587);
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential("mailaddress", "ukss cuag wvoz wyqx");

            MailMessage mailMessage = new MailMessage("mailaddress", to, subject, body);

            smtpClient.Send(mailMessage);



        }
    }
}
