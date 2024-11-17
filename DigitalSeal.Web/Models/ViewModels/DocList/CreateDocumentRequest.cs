using System.ComponentModel.DataAnnotations;

namespace DigitalSeal.Web.Models.ViewModels.DocList
{
    public class CreateDocumentRequest
    {
        [Required(ErrorMessage = "General.Required.NoFileSelected")]
        public IFormFile? File { get; set; }
    }
}
