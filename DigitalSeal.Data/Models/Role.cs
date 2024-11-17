using Microsoft.AspNetCore.Identity;

namespace DigitalSeal.Data.Models
{
    public enum RoleCode { Admin, User }

    public class Role : IdentityRole<int>
    {
        public Role()
            : base()
        {
        }

        public Role(string roleName)
            : base(roleName)
        {
        }

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
