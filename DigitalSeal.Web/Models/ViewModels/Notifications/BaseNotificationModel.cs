using DigitalSeal.Core.Utilities;
using DigitalSeal.Data.Models;

namespace DigitalSeal.Web.Models.ViewModels.Notifications
{
    public class BaseNotificationModel
    {
        public int NotificationID { get; set; }
        public bool IsSeen { get; set; }
        public string? Title { get; set; }
        public string? SentDate { get; set; }
        public string? SenderName { get; set; }

        public BaseNotificationModel(UserNotification notification)
        {
            NotificationID = notification.Id;
            IsSeen = notification.IsSeen;
            Title = notification.Title;
            SentDate = DateHelper.FormatDateTime(notification.Created);
            User? sender = notification.Sender;
            SenderName = UserHelper.FormatUserName(sender);
        }

        public BaseNotificationModel()
        {
        }
    }
}
