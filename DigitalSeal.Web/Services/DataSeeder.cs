using Microsoft.EntityFrameworkCore;
using DigitalSeal.Core.Utilities;
using DigitalSeal.Data.Services;
using DigitalSeal.Data.Models;

namespace DigitalSeal.Web.Services
{
    public class DataSeeder : IDataSeeder
    {
        private readonly IConfiguration _config;
        public DataSeeder(IConfiguration config)
        {
            _config = config;
        }

        public static async Task EnsureDbCreated(IServiceProvider serviceProvider)
        {
            using var client = serviceProvider.GetService<AppDbContext>();
            if (client != null)
                await client.Database.EnsureCreatedAsync();
        }

        public void Seed(ModelBuilder modelBuilder)
        {
            SeedRoles(modelBuilder);
            SeedAdminAccount(modelBuilder);
        }

        private static void SeedRoles(ModelBuilder builder)
        {
            builder.Entity<Role>().HasData(
                CreateRole(1, nameof(RoleCode.Admin)),
                CreateRole(2, nameof(RoleCode.User)));
        }

        private static Role CreateRole(int id, string name) => new()
        {
            Id = id,
            Name = name,
            NormalizedName = name.ToUpper(),
        };

        private void SeedAdminAccount(ModelBuilder builder)
        {
            var admin = _config.GetSection("DefaultAdminAccount").Get<User>();
            if (admin == null)
                return;

            var password = _config.GetSection("DefaultAdminSection:Password").Value ?? "AdminPass123";
            admin.PasswordHash = AuthHelper.HashPassword(password);
            admin.NormalizedEmail = admin.Email!.ToUpper();
            admin.UserName = admin.Email;
            admin.NormalizedUserName = admin.NormalizedEmail;
            builder.Entity<User>().HasData(admin);

            var userRole = new UserRole
            {
                UserId = admin.Id,
                RoleId = 1,
            };
            builder.Entity<UserRole>().HasData(userRole);

            var defaultOrg = new Organization
            {
                Id = 1,
                Name = $"My Organization",
            };
            builder.Entity<Organization>().HasData(defaultOrg);


            var defaultUserOrg = new Party
            {
                Id = 1,
                Permission = (int)PartyPermission.Owner,
                IsCurrent = true,
                OrganizationId = 1,
                UserId = 1,
            };
            builder.Entity<Party>().HasData(defaultUserOrg);
        }
    }
}
