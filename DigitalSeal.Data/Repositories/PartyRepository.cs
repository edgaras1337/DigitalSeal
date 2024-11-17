using DigitalSeal.Data.Models;
using LanguageExt;
using Microsoft.EntityFrameworkCore;

namespace DigitalSeal.Data.Repositories
{
    internal class PartyRepository : Repository<Party>, IPartyRepository
    {
        public PartyRepository(AppDbContext context) 
            : base(context)
        {
        }

        public async Task<int> GetIdOfCurrentAsync(int userId)
        {
            return await Context.Parties
                .AsNoTracking()
                .Include(party => party.User)
                .Where(party => party.UserId == userId && party.IsCurrent)
                .Select(party => party.Id)
                .SingleAsync();
        }

        public async Task<Party> GetCurrentAsync(int userId)
        {
            return await Context.Parties
                .Include(party => party.User)
                .SingleAsync(party => party.UserId == userId && party.IsCurrent);
        }


        public async Task<List<Party>> GetAsync(IEnumerable<int> partyIds, int orgId)
        {
            return await Context.Parties
                .Include(party => party.User)
                .Where(party => partyIds.Contains(party.Id) && party.OrganizationId == orgId)
                .ToListAsync();
        }

        public async Task<Party?> GetAsync(int orgId, int userId)
        {
            return await Context.Parties
                .FirstOrDefaultAsync(party => party.OrganizationId == orgId && party.UserId == userId);
        }

        public async Task<List<Party>> GetAsync(int orgId)
        {
            return await Context.Parties
                .Where(party => party.OrganizationId == orgId)
                .ToListAsync();
        }

        public async Task<Party> GetAnyOwnerPartyAsync(int userId)
        {
            return await Context.Parties
                .FirstAsync(party => party.UserId == userId && 
                    ((PartyPermission)party.Permission).HasFlag(PartyPermission.Owner));
        }

        public async Task<List<Party>> GetAnyNonCurrentAsync(IEnumerable<int> userIds)
        {
            //return await Context.Users
            //    .Where(user => userIds.Contains(user.Id))
            //    .Include(user => user.Parties.Where(party => !party.IsCurrent))
            //    .SelectMany(user => user.Parties)
            //    .ToListAsync();

            List<Party> nonCurrentParties = await Context.Parties
                .Where(party => userIds.Contains(party.UserId) && !party.IsCurrent)
                .ToListAsync();

            return nonCurrentParties
                .GroupBy(party => party.UserId)
                .Select(group => group.First())
                .ToList();
        }
    }

    public interface IPartyRepository : IRepository<Party>
    {
        Task<int> GetIdOfCurrentAsync(int userId);
        Task<Party> GetCurrentAsync(int userId);
        Task<List<Party>> GetAsync(IEnumerable<int> partyIds, int orgId);
        Task<Party?> GetAsync(int orgId, int userId);
        Task<List<Party>> GetAsync(int orgId);
        Task<Party> GetAnyOwnerPartyAsync(int userId);
        Task<List<Party>> GetAnyNonCurrentAsync(IEnumerable<int> userIds);
    }
}
