using DigitalSeal.Data.Models;

namespace DigitalSeal.Data.Repositories
{
    internal class SignatureInfoRepository : Repository<SignatureInfo>, ISignatureInfoRepository
    {
        public SignatureInfoRepository(AppDbContext context) 
            : base(context)
        {
        }
    }

    public interface ISignatureInfoRepository : IRepository<SignatureInfo> 
    { 
    }
}
