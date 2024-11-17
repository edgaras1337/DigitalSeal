using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalSeal.Data.Models
{
    public enum InviteNotificationState { Pending, Accepted, Declined };
    public enum InviteType { Document, Organization };

    [PrimaryKey(nameof(UserNotificationId))]
    public class InviteNotification
    {
        [ForeignKey(nameof(UserNotification))]
        public int UserNotificationId { get; set; }
        public UserNotification UserNotification { get; set; } = null!;

        //[StringLength(256)]
        //public string AcceptLink { get; set; } = string.Empty;

        //[StringLength(256)]
        //public string DeclineLink { get; set; } = string.Empty;

        public int State { get; set; }
        public int Type { get; set; }
    }
}
