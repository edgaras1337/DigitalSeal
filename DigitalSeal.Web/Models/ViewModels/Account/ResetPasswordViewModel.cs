using System.ComponentModel.DataAnnotations;

namespace DigitalSeal.Web.Models.ViewModels.Account
{
    public class ResetPasswordViewModel
    {
        public string? Email { get; set; }
        public string? Token { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [Display(Name = "New Password")]
        [DataType(DataType.Password)]
        [RegularExpression("^(?=.*[A-Z])(?=.*\\d).{6,}$",
            ErrorMessage =
                "Passwords must be at least 6 characters<br/>" +
                "Passwords must have at least one digit ('0'-'9')<br/>" +
                "Passwords must have at least one uppercase ('A'-'Z')")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
