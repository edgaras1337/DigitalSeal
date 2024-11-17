using DigitalSeal.Core.Extensions;
using DigitalSeal.Core.Models.Config.Email;
using DigitalSeal.Core.Models.Notifications;
using DigitalSeal.Data.Models;
using DigitalSeal.Data.Repositories;

namespace DigitalSeal.Core.Services
{
    public class DocPartyNotificationService : IDocPartyNotificationService
    {
        private readonly IUserNotificationRepository _userNotificationRepository;
        private readonly IEmailService _emailService;
        private readonly IMessageCreator _notificationService;
        public DocPartyNotificationService(
            IUserNotificationRepository userNotificationRepository,
            IEmailService emailService,
            IMessageCreator notificationService)
        {
            _userNotificationRepository = userNotificationRepository;
            _emailService = emailService;
            _notificationService = notificationService;
        }

        public void InformAddedParty(Document doc, int orgId, int currentUserId, int addedUserId)
        {
            HtmlMessage message = _notificationService.DocPartyAdded(doc);
            UserNotification addedNotification = message.AsTextNotification(orgId, currentUserId, addedUserId);
            _userNotificationRepository.Add(addedNotification);
        }

        public void InformRemovedParty(Document doc, int orgId, Party currParty, DocumentParty removedParty)
        {
            HtmlMessage message = _notificationService.DocPartyRemoved(doc);
            UserNotification notification = message.AsTextNotification(orgId, currParty.UserId, removedParty.Party.UserId);
            _userNotificationRepository.Add(notification);
        }

        public async Task SendDocPartyAddedEmailsAsync(Document doc, List<string> emails)
        {
            HtmlMessage message = _notificationService.DocPartyAdded(doc);
            var emailMessage = new EmailMessage(message.Title, message.Content, true, [.. emails]);
            await _emailService.SendEmailAsync(emailMessage);
        }
    }
}
