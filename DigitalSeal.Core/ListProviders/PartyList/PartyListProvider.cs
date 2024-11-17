using DigitalSeal.Core.ListProviders.Models;
using DigitalSeal.Core.Services;
using DigitalSeal.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace DigitalSeal.Core.ListProviders.PartyList
{
    public class PartyListProvider : ListProvider<PartyListRequest, PartyListModel>, IPartyListProvider
    {
        private readonly ICurrentUserProvider _currUserProvider;
        public PartyListProvider(ILogger<PartyListProvider> logger, AppDbContext context, ICurrentUserProvider currUserProvider,
            IStringLocalizer? localizer = null)
            : base(logger, context, localizer)
        {
            _currUserProvider = currUserProvider;
        }

        protected override async Task<GridListResponse<PartyListModel>> GetListAsync(
            PartyListRequest request)
        {
            List<Party> parties = await Context.Parties
                .Include(x => x.User)
                .Where(x => x.OrganizationId == request.OrgId)
                .ToListAsync();

            return new(parties.Select(CreateRowModel));
        }

        private RowModel<PartyListModel> CreateRowModel(Party party)
        {
            User user = party.User;
            var model = new PartyListModel
            {
                Id = party.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
            };
            bool isOwner = user.Id == _currUserProvider.CurrentUserId;
            return new(model, !isOwner);
        }
    }
}
