using Remedy.Models;
using Remedy.Services.Interfaces;
using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Remedy.Services
{
    public class BTEmailService : IEmailSender
    {
        private readonly MailSettings _mailSettings;

        public BTEmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailSender = _mailSettings.EmailAddress ?? Environment.GetEnvironmentVariable("Email");
            var host = _mailSettings.EmailHost ?? Environment.GetEnvironmentVariable("Host");
            var port = _mailSettings.EmailPort != 0 ? _mailSettings.EmailPort : int.Parse(Environment.GetEnvironmentVariable("Port")!);
            string password = _mailSettings.EmailPassword ?? Environment.GetEnvironmentVariable("Password")!;

            MimeMessage newEmail = new();

            //Add all email address to the "TO" for the email.
            newEmail.Sender = MailboxAddress.Parse(emailSender);
            newEmail.To.Add(MailboxAddress.Parse(email));
            newEmail.Subject = subject;

            //Add the body to the email.
            var builder = new BodyBuilder { HtmlBody = htmlMessage };
            newEmail.Body = builder.ToMessageBody();

            //Send the email.
            try
            {
                using SmtpClient smtpClient = new();

                await smtpClient.ConnectAsync(host, port, SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(emailSender, password);
                await smtpClient.SendAsync(newEmail);

                await smtpClient.DisconnectAsync(true);
            }
            catch
            {
                throw;
            }
        }
    }
}
