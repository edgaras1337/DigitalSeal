using DigitalSeal.Core.Attributes;

namespace DigitalSeal.Core.ListProviders.DocPartyList
{
    public class DocPartyModel
    {
        [GridKey, GridColumnHidden]
        public int PartyId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string SignatureStatus { get; set; } = string.Empty;
    }

}
