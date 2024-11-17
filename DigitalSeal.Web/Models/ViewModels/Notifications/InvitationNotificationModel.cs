using DigitalSeal.Data.Models;

namespace DigitalSeal.Web.Models.ViewModels.Notifications
{
    public class InvitationNotificationModel : BaseNotificationModel
    {
        public string? AcceptLink { get; set; }
        public string? DeclineLink { get; set; }
        public InviteNotificationState State { get; set; }

        public InvitationNotificationModel(UserNotification notification)
            : base(notification)
        {
            InviteNotification? inviteNotif = notification.InviteNotification;
            if (inviteNotif != null)
            {
                //AcceptLink = acceptLink;
                //DeclineLink = declineLink;
                State = (InviteNotificationState)inviteNotif.State;
            }
        }

        public InvitationNotificationModel()
        {
        }
    }
}
