using Microsoft.EntityFrameworkCore;
using DigitalSeal.Core.Services;
using Microsoft.Extensions.Logging;
using DigitalSeal.Core.ListProviders.Models;
using DigitalSeal.Data.Models;

namespace DigitalSeal.Core.ListProviders.DocPartyPossibList
{
    public class DocPartyPossibListProvider : ListProvider<DocPartyPossibListRequest, DocPartyPossibListModel>, IDocPartyPossibListProvider
    {
        private readonly ICurrentUserProvider _currUserProvider;
        public DocPartyPossibListProvider(ILogger<DocPartyPossibListProvider> logger, AppDbContext context, 
            ICurrentUserProvider currUserProvider) 
            : base(logger, context)
        {
            _currUserProvider = currUserProvider;
        }

        protected override async Task<GridListResponse<DocPartyPossibListModel>> GetListAsync(DocPartyPossibListRequest request)
        {
            Party currParty = await _currUserProvider.GetCurrentPartyAsync();

            if (!await Context.DocumentParties.AnyAsync(x => x.PartyId == currParty.Id && x.DocId == request.DocId))
            {
                Logger.LogInformation("Document (ID: {id}) not found", request.DocId);
                return new([]);
            }

            List<Party> parties = await Context.Parties
                .Include(party => party.User)
                .Where(party => party.Id != currParty.Id &&
                    party.OrganizationId == currParty.OrganizationId &&
                    !party.DocumentParties.Any(dp => dp.DocId == request.DocId))
                .ToListAsync();

            return new GridListResponse<DocPartyPossibListModel>(parties.Select(CreateDocPossiblePartyModel).ToList());
        }

        private static RowModel<DocPartyPossibListModel> CreateDocPossiblePartyModel(Party party)
            => new(new()
            {
                PartyId = party.Id,
                Email = party.User.Email,
                FirstName = party.User.FirstName,
                LastName = party.User.LastName,
            });
    }
}
