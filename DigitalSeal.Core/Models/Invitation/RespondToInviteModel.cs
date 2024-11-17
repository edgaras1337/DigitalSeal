using DigitalSeal.Data.Models;

namespace DigitalSeal.Core.Models.Invitation
{
    public record RespondToInviteModel(UserNotification Notification, bool IsAccept);
}
