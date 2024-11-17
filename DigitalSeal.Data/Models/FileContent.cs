using DigitalSeal.Data.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalSeal.Data.Models
{
    // TODO: This BaseEntity introduces Id. Check if it causes any problems.
    public class FileContent : BaseAuditableEntity
    {
        [ForeignKey(nameof(Document))]
        public int DocId { get; set; }
        public Document? Document { get; set; }

        public byte[] Content { get; set; } = [];
    }
}
