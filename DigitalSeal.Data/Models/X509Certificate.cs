using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DigitalSeal.Data.Models
{
    public class X509Certificate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public byte[] EncryptedCertificateBlob { get; set; } = [];

        [Required]
        public DateTime ExpirationDate { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
