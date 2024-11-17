using MailKit.Net.Smtp;

namespace DigitalSeal.Core.Services
{
    public interface ISmtpClientWrapper
    {
        ISmtpClient CreateClient();
    }
}
