using DigitalSeal.Core.ListProviders.Models;
using DigitalSeal.Core.Services;
using DigitalSeal.Core.Utilities;
using DigitalSeal.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DigitalSeal.Core.ListProviders.OrgList
{
    public class OrgListProvider : ListProvider<OrgListRequest, OrgListModel>, IOrgListProvider
    {
        private readonly ICurrentUserProvider _currUserProvider;

        public OrgListProvider(ILogger<OrgListProvider> logger, AppDbContext context, ICurrentUserProvider currUserProvider) : base(logger, context)
        {
            _currUserProvider = currUserProvider;
        }

        protected override async Task<GridListResponse<OrgListModel>> GetListAsync(OrgListRequest request)
        {
            int userID = _currUserProvider.CurrentUserId;

            IQueryable<Party> partyQuery = Context.Parties
                .AsNoTracking()
                .Include(x => x.Organization)
                .Include(x => x.User)
                .OrderByDescending(x => x.IsCurrent)
                .Where(x => x.UserId == userID);

            if (request.Category == OrgCategory.Personal)
            {
                partyQuery = partyQuery.Where(x => ((PartyPermission)x.Permission).HasFlag(PartyPermission.Owner));
                return CreateResponse(await partyQuery.ToListAsync(), null);
            }
            else if (request.Category == OrgCategory.NonPersonal)
            {
                partyQuery = partyQuery.Where(x => !((PartyPermission)x.Permission).HasFlag(PartyPermission.Owner));
            }

            List<Party> parties = await partyQuery.ToListAsync();

            HashSet<int> nonPersonalOrgIDs = parties
                .Where(x => x.Has(PartyPermission.Owner))
                .Select(x => x.Id)
                .ToHashSet();

            List<Party>? nonPersonalOrgs = null;
            if (nonPersonalOrgIDs.Count != 0)
            {
                nonPersonalOrgs = await Context.Parties
                    .Where(x => nonPersonalOrgIDs.Contains(x.Id))
                    .ToListAsync();
            }

            return CreateResponse(parties, nonPersonalOrgs);
        }

        private static GridListResponse<OrgListModel> CreateResponse(List<Party> parties, List<Party>? nonPersonalOrgs = null)
        {
            IEnumerable<RowModel<OrgListModel>> rows = parties.Select(party =>
            {
                OrgListModel model = CreateModel(party, nonPersonalOrgs);
                GridRowStyle style = party.IsCurrent ? GridRowStyle.Positive : GridRowStyle.Default;

                return new RowModel<OrgListModel>(model, style: style);
            });
            return new(rows);
        }

        private static OrgListModel CreateModel(Party party, List<Party>? owners = null)
        {
            Party? owner = null;
            if (party.Has(PartyPermission.Owner))
            {
                owner = party;
            }
            else if (owners != null)
            {
                owner = owners.FirstOrDefault(x => x.OrganizationId == party.OrganizationId);
            }

            string ownerName = owner != null ?
                UserHelper.FormatUserName(owner?.User ?? party.User) :
                string.Empty;

            return new()
            {
                Id = party.OrganizationId,
                Name = party.Organization.Name,
                IsCurrent = party.IsCurrent,
                IsOwner = party.Has(PartyPermission.Owner),
                Owner = ownerName,
            };
        }
    }
}
