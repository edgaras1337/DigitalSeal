namespace DigitalSeal.Core.Models.Settings
{
    public record UpdateUserModel(
        bool Is2FAEnabled,
        string? Email,
        string? CurrentPassword,
        string? NewPassword,
        string? ConfirmPassword);
}
