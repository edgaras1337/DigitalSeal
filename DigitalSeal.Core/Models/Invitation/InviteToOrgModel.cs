namespace DigitalSeal.Core.Models.Invitation
{
    public record InviteToOrgModel(int OrgId, int SenderId, int[] InvitedUserIds);
}
