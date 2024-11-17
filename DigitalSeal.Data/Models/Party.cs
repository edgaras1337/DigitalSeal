using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalSeal.Data.Models
{
    [Flags]
    public enum PartyPermission
    {
        // Full access
        Owner = 1 << 0,
        Read = 1 << 1,
        Invite = 1 << 2,
        Update = 1 << 3,
        Delete = 1 << 4,
    }

    public class Party : WithPermission<PartyPermission>
    {
        //public int Id { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        public User User { get; set; } = null!;


        [ForeignKey(nameof(Organization))]
        public int OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;

        //public bool IsOwner { get; set; }

        public bool IsCurrent { get; set; }

        public ICollection<DocumentParty> DocumentParties { get; set; } = [];
        //public ICollection<UserNotification> SentNotifications { get; set; } = [];
        //public ICollection<UserNotification> ReceivedNotifications { get; set; } = [];
    }
}
