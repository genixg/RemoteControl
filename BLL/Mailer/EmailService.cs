using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;

namespace RemoteControl.BLL.Mailer
{    public class EmailService
    {
        private string _host { get; set; }
        private int _port { get; set; }
        private bool _ssl { get; set; }
        private string _login { get; set; }
        private string _password { get; set; }
        private string _nameFrom { get; set; }
        private string _emailFrom { get; set; }

        public EmailService(string host, int port, bool ssl, string login, string password, string nameFrom, string emailFrom)
        {
            _host = host;
            _port = port;
            _ssl = ssl;
            _login = login;
            _password = password;
            _nameFrom = nameFrom;
            _emailFrom = emailFrom;
        }

        public bool SendEmailAsync(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress(_nameFrom, _emailFrom));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };

            using (var client = new SmtpClient())
            {
                try
                {
                    client.Connect(_host, _port, _ssl);
                    client.Authenticate(_login, _password);
                    client.Send(emailMessage);

                    client.Disconnect(true);
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }

            return true;
        }
    }
}
