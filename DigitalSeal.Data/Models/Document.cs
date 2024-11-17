using DigitalSeal.Data.Common;
using System.ComponentModel.DataAnnotations;

namespace DigitalSeal.Data.Models
{
    public enum SignaturePage : short { First, Last }
    public enum SignaturePosition : short { TopLeft, TopRight, BottomLeft, BottomRight, TopMiddle, BottomMiddle, Hidden }

    public class Document : BaseAuditableEntity
    {
        //[Key]
        //public int Id { get; set; }

        [StringLength(256)]
        public string Name { get; set; } = string.Empty;

        [StringLength(8)]
        public string Extension { get; set; } = string.Empty;

        //public DateTime CreatedDate { get; set; }

        public DateTime Deadline { get; set; }

        public FileContent FileContent { get; set; } = null!;
        public ICollection<DocumentParty> DocumentParties { get; set; } = [];
    }
}
