using DigitalSeal.Core.Exceptions;
using DigitalSeal.Core.Models.Invitation;
using DigitalSeal.Data.Models;
using DigitalSeal.Data.Repositories;
using LanguageExt.Common;
using Microsoft.Extensions.Localization;

namespace DigitalSeal.Core.Services
{
    public class InvitationService : IInvitationService
    {
        private readonly IUserNotificationRepository _userNotificationRepository;
        private readonly IStringLocalizer<InvitationService> _localizer;
        public InvitationService(
            IUserNotificationRepository userNotificationRepository, 
            IStringLocalizer<InvitationService> localizer)
        {
            _userNotificationRepository = userNotificationRepository;
            _localizer = localizer;
        }

        // TODO: Make an email version message.
        public async Task InviteToOrganizationAsync(InviteToOrgModel model)
        {
            foreach (int userId in model.InvitedUserIds)
            {
                _userNotificationRepository.Add(new UserNotification
                {
                    //CreatedDate = DateTime.UtcNow,
                    OrganizationId = model.OrgId,
                    SenderId = userId,
                    ReceiverId = userId,
                    Title = _localizer["InvitedToOrg"],
                    InviteNotification = new InviteNotification
                    {
                        Type = (int)InviteType.Organization,
                    }
                });
            }
            await _userNotificationRepository.SaveChangesAsync();
        }

        public async Task<Result<bool>> RespondToInvitationAsync(RespondToInviteModel model)
        {
            InviteNotification invite = model.Notification.InviteNotification;
            invite.State = (int)(model.IsAccept ? InviteNotificationState.Accepted : InviteNotificationState.Declined);
            await _userNotificationRepository.SaveChangesAsync();

            // TODO: Will have to add a notification that tells the inviter that the user responded.
            return true;
        }

        // TODO: test innvalid notif
        public async Task<Result<UserNotification>> GetInvitationAsync(GetInvitationModel model)
        {
            UserNotification? inviteNotif = await _userNotificationRepository.GetByInviteIdAsync(model.InviteId);
            if (IsValidInvitation(inviteNotif, model.ReceiverId, model.Type))
            {
                return inviteNotif!;
            }

            return new(ValidationException.Error(_localizer["InviteNotFound"]));
        }

        private static bool IsValidInvitation(UserNotification? inviteNotif, int receiverId, InviteType type)
        {
            return inviteNotif != null &&
                inviteNotif.ReceiverId == receiverId;// ||
                //inviteNotif.InviteNotification.Type != (int)type;
        }
    }
}
