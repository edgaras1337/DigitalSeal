using DigitalSeal.Core.Models.Config.Email;
using LanguageExt.Pretty;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace DigitalSeal.Core.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailOptions _emailOptions;
        private readonly ISmtpClientWrapper _smtpClientWrapper;
        public EmailService(IOptions<EmailOptions> emailOptions, ISmtpClientWrapper smtpClientWrapper)
        {
            _emailOptions = emailOptions.Value;
            _smtpClientWrapper = smtpClientWrapper;
        }

        public async Task SendEmailAsync(EmailMessage emailMessage)
        {
            using ISmtpClient client = _smtpClientWrapper.CreateClient();
            try
            {
                await client.ConnectAsync(_emailOptions.SmtpServer, _emailOptions.Port, true);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                await client.AuthenticateAsync(_emailOptions.Username, _emailOptions.Password);

                MimeMessage mimeMessage = CreateMessage(emailMessage);
                await client.SendAsync(mimeMessage);
            }
            catch
            {
                throw;
            }
            finally
            {
                await client.DisconnectAsync(true);
                client.Dispose();
            }
        }

        private MimeMessage CreateMessage(EmailMessage message)
        {
            var textFormat = message.IsHtmlContent ? TextFormat.Html : TextFormat.Text;
            var email = new MimeMessage
            {
                Subject = message.Subject,
                Body = new TextPart(textFormat)
                {
                    Text = message.Content
                }
            };
            email.From.Add(new MailboxAddress("Digital Seal", _emailOptions.From));
            email.To.AddRange(message.To);
            return email;
        }
    }
}
