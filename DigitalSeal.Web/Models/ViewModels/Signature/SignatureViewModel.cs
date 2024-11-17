using DigitalSeal.Data.Models;

namespace DigitalSeal.Web.Models.ViewModels.Signature
{
    public class SignatureViewModel : SignRequest
    {
        public string[] DocNames { get; set; } = [];
        public Dictionary<SignaturePage, List<SignaturePosition>>? HiddenPositionsByPage { get; set; }
    }
}
