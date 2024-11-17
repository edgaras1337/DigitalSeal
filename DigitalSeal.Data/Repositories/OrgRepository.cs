using DigitalSeal.Data.Models;
using LanguageExt;
using Microsoft.EntityFrameworkCore;

namespace DigitalSeal.Data.Repositories
{
    internal class OrgRepository : Repository<Organization>, IOrgRepository
    {
        public OrgRepository(AppDbContext context) 
            : base(context)
        {
        }

        public async Task<Organization?> GetByIdAsync(int orgId, int userId)
        {
            return await Context.Organizations
                .Include(x => x.Parties.Where(p => p.UserId == userId))
                .FirstOrDefaultAsync(x => x.Id == orgId && x.Parties.Any(p => p.UserId == userId));
        }

        public async Task<List<Organization>> GetByIdsAsync(IEnumerable<int> orgIds, int userId, 
            bool includeRelatedData = false)
        {
            IQueryable<Organization> query = Context.Organizations
                .Where(x => orgIds.Contains(x.Id) && x.Parties.Any(p => p.UserId == userId));

            if (includeRelatedData)
            {
                query = query
                    .Include(x => x.Parties)
                    .ThenInclude(x => x.DocumentParties)
                        .ThenInclude(x => x.Document);
            }

            return await query.ToListAsync();
        }

        //private IQueryable<Organization> GetByUser()
        //{
        //    return Context.Organizations
        //        .Include(x => x.Parties.Where(p => p.UserId == userId))
        //}

        public async Task<int> GetUserPersonalOrgCount(int userId)
        {
            return await Context.Parties
                .Where(party => party.UserId == userId && 
                    ((PartyPermission)party.Permission).HasFlag(PartyPermission.Owner))
                .CountAsync();
        }

        public async Task<Organization?> GetWithOwnerAsync(int orgId, int userId)
        {
            return await Context.Organizations
                .Include(org => org.Parties.Where(party => party.UserId == userId || 
                        ((PartyPermission)party.Permission).HasFlag(PartyPermission.Owner)))
                    .ThenInclude(party => party.User)
                .FirstOrDefaultAsync(org => org.Id == orgId && org.Parties.Any(party => party.UserId == userId));
        }
    }

    public interface IOrgRepository : IRepository<Organization>
    {
        Task<Organization?> GetByIdAsync(int orgId, int userId);
        Task<List<Organization>> GetByIdsAsync(IEnumerable<int> orgIds, int userId, bool includeRelatedData = false);
        Task<int> GetUserPersonalOrgCount(int userId);
        Task<Organization?> GetWithOwnerAsync(int orgId, int userId);
    }
}
