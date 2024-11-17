namespace DigitalSeal.Core.Models.Signature
{
    public enum SignatureVerificationStatus
    {
        VerificationPassed,
        VerificationFailed,
        //NotSigned
    }

    public class VerifySignaturesResult
    {
        public SignatureVerificationStatus Status { get; set; }
        public string SignedBy { get; set; } = string.Empty;
        public int SignerPartyId { get; set; }
        public DateTime SignTime { get; set; }
        public string? Reason { get; set; }
        public string? Location { get; set; }
        public string? Contact { get; set; }
    }
}
