using DigitalSeal.Core.Models.Config.Email;
using MailKit.Net.Smtp;

namespace DigitalSeal.Core.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailMessage message);
    }
}
