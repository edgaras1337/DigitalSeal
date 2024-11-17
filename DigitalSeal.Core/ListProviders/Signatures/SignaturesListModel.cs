using DigitalSeal.Core.Attributes;
using DigitalSeal.Core.Models.Signature;

namespace DigitalSeal.Core.ListProviders.Signatures
{
    public record SignaturesListModel
    {
        [GridKey, GridColumnHidden]
        public int DummyId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string SignedBy { get; set; } = string.Empty;
        public string SignTime { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public string? Location { get; set; }
        public string? Contact { get; set; }
    }
}
