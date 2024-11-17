using System.ComponentModel.DataAnnotations;

namespace DigitalSeal.Web.Models.ViewModels.Signature
{
    public class SignaturePageModel : BasePageModel
    {
        [MinLength(1, ErrorMessage = "General.MinLength.Documents")]
        public int[] DocIds { get; set; } = [];
    }
}
