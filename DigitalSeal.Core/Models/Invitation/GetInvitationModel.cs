using DigitalSeal.Data.Models;

namespace DigitalSeal.Core.Models.Invitation
{
    public record GetInvitationModel(int InviteId, int ReceiverId, InviteType Type);
}
