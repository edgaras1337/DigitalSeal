using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalSeal.Data.Models
{
    public class RSAKey
    {
        [Key, ForeignKey(nameof(User))]
        public int UserId { get; set; }

        [Required]
        public byte[] EncryptedXmlString { get; set; } = [];

        public User User { get; set; } = null!;
    }
}
