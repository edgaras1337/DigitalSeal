using System.ComponentModel.DataAnnotations;

namespace DigitalSeal.Web.Models.ViewModels.OrgList
{
    public class DeleteOrgsRequest
    {
        [MinLength(1, ErrorMessage = "General.MinLength.Organizations")]
        public int[] OrgIds { get; set; } = [];
    }
}
