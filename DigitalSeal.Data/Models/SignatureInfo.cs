using DigitalSeal.Data.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalSeal.Data.Models
{
    public class SignatureInfo : BaseEntity
    {
        //[Key]
        //public int Id { get; set; }
        public int SignDisplayPage { get; set; }
        public int SignDisplayLocation { get; set; }
        public DateTime SignDate { get; set; }

        [ForeignKey(nameof(DocumentParty))]
        public int DocumentPartyId { get; set; }
        public DocumentParty? DocumentParty { get; set; }
    }
}
