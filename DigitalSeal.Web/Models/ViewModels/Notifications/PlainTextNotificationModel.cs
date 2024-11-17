using DigitalSeal.Data.Models;

namespace DigitalSeal.Web.Models.ViewModels.Notifications
{
    public class PlainTextNotificationModel : BaseNotificationModel
    {
        public string? Content { get; set; }

        public PlainTextNotificationModel(UserNotification notification)
            : base(notification)
        {
            Content = notification.PlainTextNotification?.Content;
        }

        public PlainTextNotificationModel()
        {
        }
    }
}
