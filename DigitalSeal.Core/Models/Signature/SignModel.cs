using DigitalSeal.Data.Models;

namespace DigitalSeal.Core.Models.Signature
{
    //public record SignModel(
    //    string? Reason, 
    //    string? Location, 
    //    string? Contact, 
    //    SignaturePosition Position, 
    //    SignaturePage Page, 
    //    List<int> DocIds);

    public class SignModel
    {
        public string? Reason { get; set; }
        public string? Location { get; set; }
        public string? Contact { get; set; }
        public SignaturePosition Position { get; set; }
        public SignaturePage Page { get; set; }
        public List<int> DocIds { get; set; } = [];
    }
}
