using Google.Apis.Auth.OAuth2;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MimeKit;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace TowerScaffolding.Services
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            return PostMessage(email, subject, message);
            //Execute(Options.SendGridKey, subject, message, email);
        }

        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Route("SendNotification")]
        public async Task PostMessage(string email, string subject, string message)
        {
            var apiKey = _configuration.GetSection("SENDGRID_API_KEY").Value;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("Accountants@towerscaffolding.co.nz", "Tower Scaffolding Accountants");
            var to = new EmailAddress(email, "");
            
            var msg = MailHelper.CreateSingleEmail(from, to, subject, "PlainText", message);
            var response = await client.SendEmailAsync(msg);
        }
    }
}
