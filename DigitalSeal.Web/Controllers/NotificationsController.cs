using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using DigitalSeal.Web.Models.ViewModels.Notifications;
using DigitalSeal.Data.Models;
using DigitalSeal.Core.Services;
using DigitalSeal.Web.Models.ViewModels.OrgEdit;

namespace DigitalSeal.Web.Controllers
{
    [Route("notifications")]
    public class NotificationsController : BaseDSController
    {
        private readonly INotificationService _notifService;
        private readonly ICurrentUserProvider _currUserProvider;
        public NotificationsController(
            INotyfService notyf, 
            INotificationService notifService, 
            ICurrentUserProvider currUserProvider)
            : base(notyf)
        {
            _notifService = notifService;
            _currUserProvider = currUserProvider;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string returnUrl = "/")
        {
            int userId = _currUserProvider.CurrentUserId;
            List<UserNotification> notifs = await _notifService.GetUserNotificationsAsync(userId);
            Task markTask = _notifService.MarkSeenAsync(notifs);

            var viewModel = new NotificationViewModel();
            notifs[0].IsSeen = false;
            viewModel.Notifications.AddRange(notifs.Select(notif => CreateModel(notif, returnUrl)));

            await markTask;

            return View(viewModel);
        }

        private BaseNotificationModel CreateModel(UserNotification notif, string returnUrl)
        {
            if (notif.PlainTextNotification != null)
            {
                return new PlainTextNotificationModel(notif);
            }
            else
            {
                if (notif.InviteNotification != null)
                {
                    return CreateInviteNotification(notif, returnUrl);
                }
                else
                {
                    return new BaseNotificationModel(notif);
                }
            }
        }

        private InvitationNotificationModel CreateInviteNotification(UserNotification notif, string returnUrl)
        {
            var inviteNotif = new InvitationNotificationModel(notif);

            var orgEditPageModel = new OrgEditPageModel
            {
                OrgId = notif.OrganizationId,
                ReturnUrl = returnUrl
            };

            string orgEditUrl = Url.Action(nameof(OrgEditController.Index), 
                ControllerName(nameof(OrgEditController)), orgEditPageModel)!;

            string notifListUrl = Url.Action(nameof(Index), CurrentControllerName, returnUrl)!;

            inviteNotif.AcceptLink = CreateResponseLink(notif, true, orgEditUrl);
            inviteNotif.DeclineLink = CreateResponseLink(notif, false, notifListUrl);

            return inviteNotif;
        }

        private string CreateResponseLink(UserNotification notif, bool isAccept, string returnUrl)
        {
            var pageModel = new RespondToOrgInviteRequest
            {
                InviteId = notif.Id,
                IsAccept = isAccept,
                ReturnUrl = returnUrl
            };

            return Url.Action(nameof(OrgEditController.RespondToInvitation),
                ControllerName(nameof(OrgEditController)), pageModel)!;
        }
    }
}
