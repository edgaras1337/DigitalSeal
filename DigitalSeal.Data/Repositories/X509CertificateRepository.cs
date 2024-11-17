using DigitalSeal.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSeal.Data.Repositories
{
    internal class X509CertificateRepository : Repository<X509Certificate>, IX509CertificateRepository
    {
        public X509CertificateRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<X509Certificate?> GetLastForUserAsync(int userId)
        {
            return await Context.X509Certificates
                .OrderByDescending(cert => cert.ExpirationDate)
                .FirstOrDefaultAsync(cert => cert.UserId == userId);
        }
    }

    public interface IX509CertificateRepository : IRepository<X509Certificate>
    {
        Task<X509Certificate?> GetLastForUserAsync(int userId);
    }
}
