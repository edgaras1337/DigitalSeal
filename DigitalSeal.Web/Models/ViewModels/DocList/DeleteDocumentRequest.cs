using System.ComponentModel.DataAnnotations;

namespace DigitalSeal.Web.Models.ViewModels.DocList
{
    public class DeleteDocumentRequest
    {
        [MinLength(1, ErrorMessage = "General.MinLength.Documents")]
        public int[] DocIds { get; set; } = [];
    }
}
