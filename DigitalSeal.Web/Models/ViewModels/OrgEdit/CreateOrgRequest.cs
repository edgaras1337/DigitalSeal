using System.ComponentModel.DataAnnotations;

namespace DigitalSeal.Web.Models.ViewModels.OrgEdit
{
    public class CreateOrgRequest
    {
        [Required(ErrorMessage = "General.Required.OrgName")]
        public string Name { get; set; } = string.Empty;
        public string? ReturnUrl { get; set; }
    }
}
