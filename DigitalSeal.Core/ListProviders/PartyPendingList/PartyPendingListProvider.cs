using DigitalSeal.Core.ListProviders.Models;
using DigitalSeal.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace DigitalSeal.Core.ListProviders.PartyPendingList
{
    public class PartyPendingListProvider : ListProvider<PartyPendingListRequest, PartyPendingListModel>, IPartyPendingListProvider
    {
        public PartyPendingListProvider(ILogger<PartyPendingListProvider> logger, AppDbContext context, IStringLocalizer? localizer = null)
            : base(logger, context, localizer)
        {
        }

        protected override GridRowSelectionMode SelectionMode => GridRowSelectionMode.None;

        protected override async Task<GridListResponse<PartyPendingListModel>> GetListAsync(PartyPendingListRequest request)
        {
            List<User> users = await Context.Users
                .Include(x => x.Parties)
                .Where(u => !u.Parties.Any(p => p.OrganizationId == request.OrgId) &&
                    u.ReceivedNotifications.Any(n => n.OrganizationId == request.OrgId &&
                        n.InviteNotification != null &&
                        n.InviteNotification.Type == (int)InviteType.Organization &&
                        n.InviteNotification.State == (int)InviteNotificationState.Pending))
                .ToListAsync();

            return new(users.Select(CreateRowModel));
        }

        private static RowModel<PartyPendingListModel> CreateRowModel(User user)
        {
            var model = new PartyPendingListModel
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
            };
            return new(model, false);
        }
    }
}
