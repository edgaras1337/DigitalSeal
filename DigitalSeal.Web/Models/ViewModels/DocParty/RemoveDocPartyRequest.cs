using System.ComponentModel.DataAnnotations;

namespace DigitalSeal.Web.Models.ViewModels.DocParty
{
    public class RemoveDocPartyRequest
    {
        public int DocId { get; set; }

        [MinLength(1, ErrorMessage = "General.MinLength.Parties")]
        public int[] PartyIds { get; set; } = [];
    }
}
