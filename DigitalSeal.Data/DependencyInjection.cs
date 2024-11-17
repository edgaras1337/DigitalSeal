using DigitalSeal.Data.Interceptors;
using DigitalSeal.Data.Models;
using DigitalSeal.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalSeal.Data
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDataServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();

            string? connectionString = configuration.GetConnectionString("Local");

            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                options.UseSqlServer(connectionString);
            });

            AddRepositories(services);

            return services;
        }

        private static void AddRepositories(IServiceCollection services)
        {
            services.AddScoped<IDocRepository, DocRepository>();
            services.AddScoped<IDocPartyRepository, DocPartyRepository>();
            services.AddScoped<IPartyRepository, PartyRepository>();
            services.AddScoped<IRSAKeyRepository, RSAKeyRepository>();
            services.AddScoped<IUserNotificationRepository, UserNotificationRepository>();
            services.AddScoped<IX509CertificateRepository, X509CertificateRepository>();
            services.AddScoped<ISignatureInfoRepository, SignatureInfoRepository>();
            services.AddScoped<IOrgRepository, OrgRepository>();
        }
    }
}
