using DigitalSeal.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalSeal.Data.Repositories
{
    internal class RSAKeyRepository : Repository<RSAKey>, IRSAKeyRepository
    {
        public RSAKeyRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<RSAKey?> GetByUserId(int userId)
        {
            return await Context.RSAKeys.FirstOrDefaultAsync(x => x.UserId == userId);
        }
    }

    public interface IRSAKeyRepository : IRepository<RSAKey>
    {
        Task<RSAKey?> GetByUserId(int userId);
    }
}
