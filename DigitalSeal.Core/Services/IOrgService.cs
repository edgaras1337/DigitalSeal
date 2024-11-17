using DigitalSeal.Core.Models.Organization;
using LanguageExt.Common;

namespace DigitalSeal.Core.Services
{
    public interface IOrgService
    {
        Task<Result<CreateOrgResponse>> CreateAsync(CreateOrgModel model);
        Task<Result<bool>> DeleteAsync(int[] orgIds);
        Task<OrganizationModel?> GetOrganizationWithOwnerAsync(int orgID);
        Task<Result<bool>> InviteUsersAsync(InviteUsersModel model);
        Task<Result<bool>> KickUsersAsync(KickUsersModel model);
        Task<Result<bool>> LeaveAsync(int orgId);
        Task<Result<bool>> RespondToInviteAsync(RespondToOrgInviteModel model);
        Task<Result<bool>> SwitchAsync(int orgId);
        Task<Result<UpdateOrgResponse>> UpdateAsync(UpdateOrgModel model);
    }
}