using DigitalSeal.Core.Models.Organization;
using DigitalSeal.Web.Models.ViewModels.OrgEdit;

namespace DigitalSeal.Web.Extensions
{
    public static class OrgEditProjections
    {
        public static CreateOrgModel ToModel(this CreateOrgRequest request)
            => new(request.Name);

        public static UpdateOrgModel ToModel(this UpdateOrgRequest request)
            => new(request.OrgId, request.Name);

        public static InviteUsersModel ToModel(this InviteUsersRequest request)
            => new(request.OrgId, request.UserIds);

        public static KickUsersModel ToModel(this KickUsersRequest request)
            => new(request.OrgId, request.PartyIds);

        public static RespondToOrgInviteModel ToModel(this RespondToOrgInviteRequest request)
            => new(request.InviteId, request.IsAccept);
    }
}