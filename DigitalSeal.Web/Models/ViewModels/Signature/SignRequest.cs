using DigitalSeal.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace DigitalSeal.Web.Models.ViewModels.Signature
{
    public class SignRequest
    {
        public string? Reason { get; set; }
        public string? Location { get; set; }
        public string? Contact { get; set; }
        public SignaturePosition Position { get; set; }
        public SignaturePage Page { get; set; }

        [MinLength(1, ErrorMessage = "General.MinLength.Documents")]
        public int[] DocIds { get; set; } = [];

        public string ReturnUrl { get; set; } = "/";
    }
}
