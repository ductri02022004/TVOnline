using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;

namespace TVOnline.Helper
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailMessage = new MimeMessage();
            
            emailMessage.From.Add(new MailboxAddress(
                _configuration["EmailSettings:SenderName"],
                _configuration["EmailSettings:SenderEmail"]));
            
            emailMessage.To.Add(MailboxAddress.Parse(email));
            emailMessage.Subject = subject;

            var builder = new BodyBuilder();
            builder.HtmlBody = htmlMessage;
            emailMessage.Body = builder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(
                    _configuration["EmailSettings:SmtpServer"],
                    int.Parse(_configuration["EmailSettings:Port"]),
                    false);

                await client.AuthenticateAsync(
                    _configuration["EmailSettings:SenderEmail"],
                    _configuration["EmailSettings:Password"]);

                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }
    }
}
