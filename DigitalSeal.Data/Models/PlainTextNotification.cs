using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalSeal.Data.Models
{
    [PrimaryKey(nameof(UserNotificationId))]
    public class PlainTextNotification
    {
        [ForeignKey(nameof(UserNotification))]
        public int UserNotificationId { get; set; }
        public UserNotification UserNotification { get; set; } = null!;

        [StringLength(512)]
        public string Content { get; set; } = string.Empty;
    }
}
