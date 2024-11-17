using DigitalSeal.Data.Common;
using System.ComponentModel.DataAnnotations;

namespace DigitalSeal.Data.Models
{
    public class Organization : BaseAuditableEntity
    {
        //public int Id { get; set; }

        [StringLength(256)]
        public string Name { get; set; } = string.Empty;

        //public DateTime CreatedDate { get; set; }

        public ICollection<Party> Parties { get; set; } = [];
        public ICollection<UserNotification> UserNotifications { get; set; } = [];
    }
}
