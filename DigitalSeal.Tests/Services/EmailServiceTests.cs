using System;
using System.Threading.Tasks;
using DigitalSeal.Core.Models.Config.Email;
using DigitalSeal.Core.Services;
using FakeItEasy;
using FluentAssertions;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using Xunit;

namespace DigitalSeal.Tests.Services
{
    public class EmailServiceTests
    {
        private readonly IEmailService _emailService;
        private readonly ISmtpClientWrapper _smtpClientWrapper;
        private readonly EmailOptions _emailOptions;
        private readonly ISmtpClient _smtpClient;
        public EmailServiceTests()
        {
            _emailOptions = new EmailOptions
            {
                From = "sender@example.com",
                SmtpServer = "smtp.example.com",
                Port = 465,
                Username = "user@example.com",
                Password = "password",
            };

            _smtpClient = A.Fake<ISmtpClient>();

            _smtpClientWrapper = A.Fake<ISmtpClientWrapper>();
            A.CallTo(() => _smtpClientWrapper.CreateClient()).Returns(_smtpClient);

            _emailService = new EmailService(Options.Create(_emailOptions), _smtpClientWrapper);
        }

        [Fact]
        public async Task SendEmailAsync_ShouldSendEmailSuccessfully()
        {
            // Arrange
            var emailMessage = new EmailMessage("Test subject", "Test content", "recipient@test.com");

            // Act
            await _emailService.SendEmailAsync(emailMessage);

            // Assert
            A.CallTo(() => _smtpClient.ConnectAsync(A<string?>.That.IsEqualTo(_emailOptions.SmtpServer),
                A<int>.That.IsEqualTo(_emailOptions.Port),
                true, A<CancellationToken>.Ignored))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _smtpClient.AuthenticateAsync(A<string?>.That.IsEqualTo(_emailOptions.Username),
                A<string?>.That.IsEqualTo(_emailOptions.Password), A<CancellationToken>.Ignored))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _smtpClient.SendAsync(A<MimeMessage>.Ignored, 
                A<CancellationToken>.Ignored, A<MailKit.ITransferProgress>.Ignored))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _smtpClient.DisconnectAsync(true, A<CancellationToken>.Ignored))
                .MustHaveHappenedOnceExactly();
        }
    }
}
