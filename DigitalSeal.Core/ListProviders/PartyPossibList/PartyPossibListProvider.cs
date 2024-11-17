using DigitalSeal.Core.ListProviders.Models;
using DigitalSeal.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace DigitalSeal.Core.ListProviders.PartyPossibList
{
    public class PartyPossibListProvider : ListProvider<PartyPossibListRequest, PartyPossibListModel>, IPartyPossibListProvider
    {
        public PartyPossibListProvider(ILogger<PartyPossibListProvider> logger, AppDbContext context, IStringLocalizer? localizer = null)
            : base(logger, context, localizer)
        {
        }

        protected override async Task<GridListResponse<PartyPossibListModel>> GetListAsync(PartyPossibListRequest request)
        {
            List<User> users = await Context.Users
                .Include(x => x.Parties)
                .Where(u => !u.Parties.Any(p => p.OrganizationId == request.OrgId) &&
                    !u.ReceivedNotifications.Any(n => n.OrganizationId == request.OrgId &&
                        n.InviteNotification != null &&
                        n.InviteNotification.Type == (int)InviteType.Organization &&
                        n.InviteNotification.State == (int)InviteNotificationState.Pending))
                .ToListAsync();

            return new(users.Select(CreateRowModel));
        }

        private static RowModel<PartyPossibListModel> CreateRowModel(User user)
        {
            var model = new PartyPossibListModel
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
            };
            return new(model);
        }
    }
}
