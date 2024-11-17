using System.ComponentModel.DataAnnotations;

namespace DigitalSeal.Web.Models.ViewModels.Account
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; } = string.Empty;
        public bool IsSent { get; set; }
    }
}
