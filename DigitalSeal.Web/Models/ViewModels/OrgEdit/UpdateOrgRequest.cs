using System.ComponentModel.DataAnnotations;

namespace DigitalSeal.Web.Models.ViewModels.OrgEdit
{
    public class UpdateOrgRequest
    {
        public int OrgId { get; set; }

        [Required(ErrorMessage = "General.Required.OrgName")]
        public string Name { get; set; } = string.Empty;
    }
}
