using DigitalSeal.Data.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalSeal.Data.Models
{
    public class UserNotification : BaseAuditableEntity
    {
        //public int Id { get; set; }

        [StringLength(64)]
        public string UniqueCode { get; set; } = string.Empty;

        public bool IsSeen { get; set; }

        [StringLength(256)]
        public string Title { get; set; } = string.Empty;

        //public DateTime CreatedDate { get; set; }

        [ForeignKey(nameof(Sender))]
        public int? SenderId { get; set; }
        public User Sender { get; set; } = null!;

        [ForeignKey(nameof(Receiver))]
        public int ReceiverId { get; set; }
        public User Receiver { get; set; } = null!;

        [ForeignKey(nameof(Organization))]
        public int OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;

        public PlainTextNotification PlainTextNotification { get; set; } = null!;
        public InviteNotification InviteNotification { get; set; } = null!;
    }
}
