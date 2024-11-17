namespace DigitalSeal.Web.Models.ViewModels.Settings
{
    // TODO: Should use custom attributes, to localize the messages.
    public record UpdateUserRequest(
        bool Is2FAEnabled,
        string? Email,
        string? CurrentPassword,
        string? NewPassword,
        string? ConfirmPassword);
}
