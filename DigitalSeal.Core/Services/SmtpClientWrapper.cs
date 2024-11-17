using MailKit.Net.Smtp;

namespace DigitalSeal.Core.Services
{
    public class SmtpClientWrapper : ISmtpClientWrapper
    {
        public ISmtpClient CreateClient() => new SmtpClient();
    }
}
