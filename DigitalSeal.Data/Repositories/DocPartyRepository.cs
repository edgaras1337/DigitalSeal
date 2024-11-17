using DigitalSeal.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalSeal.Data.Repositories
{
    internal class DocPartyRepository : Repository<DocumentParty>, IDocPartyRepository
    {
        public DocPartyRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<List<DocumentParty>> GetByDocIdAsync(int docId)
        {
            return await Context.DocumentParties
                .Where(x => x.DocId == docId)
                .ToListAsync();
        }

        public async Task AddAsync(IEnumerable<DocumentParty> parties)
            => await Context.DocumentParties.AddRangeAsync(parties);
    }

    public interface IDocPartyRepository : IRepository<DocumentParty>
    {
        Task<List<DocumentParty>> GetByDocIdAsync(int docId);
    }
}
