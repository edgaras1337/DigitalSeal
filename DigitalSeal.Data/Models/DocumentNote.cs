using DigitalSeal.Data.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalSeal.Data.Models
{
    public class DocumentNote : BaseAuditableEntity
    {
        //public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        //public DateTime CreatedDate { get; set; }

        [ForeignKey(nameof(DocumentParty))]
        public int DocumentPartyId { get; set; }
        public DocumentParty DocumentParty { get; set; } = null!;
    }
}
