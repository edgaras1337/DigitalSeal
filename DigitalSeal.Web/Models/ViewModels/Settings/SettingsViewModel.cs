using System.ComponentModel.DataAnnotations;

namespace DigitalSeal.Web.Models.ViewModels.Settings
{
    // TODO: Localize
    public class SettingsViewModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Field is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; } = string.Empty;
        public bool Is2FAEnabled { get; set; }

        [Required(ErrorMessage = "Field is required")]

        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [Required(ErrorMessage = "Field is required")]
        [RegularExpression("^(?=.*[A-Z])(?=.*\\d).{6,}$",
            ErrorMessage = 
                "Passwords must be at least 6 characters<br/>" +
                "Passwords must have at least one digit ('0'-'9')<br/>" +
                "Passwords must have at least one uppercase ('A'-'Z')")]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [Required(ErrorMessage = "Field is required")]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }

        public bool HasPassword { get; set; }
        public bool IsAdmin { get; set; }
    }
}
