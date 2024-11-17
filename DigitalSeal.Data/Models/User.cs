using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DigitalSeal.Data.Models
{
    public class User : IdentityUser<int>
    {
        [StringLength(256)]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(256)]
        public string LastName { get; set; } = string.Empty;

        public bool IsOAuthAuthenticated { get; set; }

        // This will never be null, after the account is created.
        public override string Email { get; set; } = string.Empty;

        public string? LastTimeZone { get; set; }

        public ICollection<UserRole> UserRoles { get; set; } = [];
        public ICollection<X509Certificate> X509Certificates { get; set; } = [];
        public RSAKey RSAKey { get; set; } = null!;
        public ICollection<Party> Parties { get; set; } = [];

        public ICollection<UserNotification> SentNotifications { get; set; } = [];
        public ICollection<UserNotification> ReceivedNotifications { get; set; } = [];
    }
}
