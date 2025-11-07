using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Communication.Email;
using Microsoft.Extensions.Configuration;
using PromoPilot.Application.Interfaces;

namespace PromoPilot.Application.Services
{
    public class AzureEmailService : IEmailService
    {
        private readonly EmailClient _emailClient;
        private readonly string _senderEmail;

        public AzureEmailService(IConfiguration config)
        {
            var connectionString = config["ACS:ConnectionString"];
            _senderEmail = config["ACS:SenderEmail"];
            _emailClient = new EmailClient(connectionString);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                var emailContent = new EmailContent(subject)
                {
                    Html = htmlBody
                };

                var recipients = new EmailRecipients(new List<EmailAddress>
        {
            new EmailAddress(toEmail)
        });

                var emailMessage = new EmailMessage(_senderEmail, recipients, emailContent);

                EmailSendOperation operation = await _emailClient.SendAsync(Azure.WaitUntil.Completed, emailMessage);

                if (operation.HasCompleted && operation.HasValue)
                {
                    Console.WriteLine($"✅ Email sent. Status: {operation.Value.Status}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Email send failed: {ex.Message}");
                // Optionally log or suppress error to avoid crashing registration
            }
        }
    }
}
