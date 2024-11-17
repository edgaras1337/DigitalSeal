using System.ComponentModel.DataAnnotations;

namespace DigitalSeal.Web.Models.ViewModels.Account
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [Display(Name = "Password")]
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

        public bool IsExternalLogin { get; set; }
    }
}
