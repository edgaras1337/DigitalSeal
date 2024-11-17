using DigitalSeal.Core.Models.Invitation;
using DigitalSeal.Data.Models;
using LanguageExt.Common;

namespace DigitalSeal.Core.Services
{
    public interface IInvitationService
    {
        Task InviteToOrganizationAsync(InviteToOrgModel model);
        Task<Result<bool>> RespondToInvitationAsync(RespondToInviteModel model);
        Task<Result<UserNotification>> GetInvitationAsync(GetInvitationModel model);
    }
}
