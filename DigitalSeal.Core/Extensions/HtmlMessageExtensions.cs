using DigitalSeal.Core.Models.Config.Email;
using DigitalSeal.Core.Models.Notifications;
using DigitalSeal.Data.Models;

namespace DigitalSeal.Core.Extensions
{
    public static class HtmlMessageExtensions
    {
        public static UserNotification AsTextNotification(this HtmlMessage message, int orgId, int senderId, int receiverId)
        {
            return new()
            {
                //CreatedDate = DateTime.UtcNow,
                OrganizationId = orgId,
                ReceiverId = receiverId,
                SenderId = senderId,
                Title = message.Title,
                PlainTextNotification = new PlainTextNotification
                {
                    Content = message.Content
                }
            };
        }

        public static EmailMessage AsEmailMessage(this HtmlMessage message, params string[] receivers)
            => new(message.Title, message.Content, receivers);
    }
}
