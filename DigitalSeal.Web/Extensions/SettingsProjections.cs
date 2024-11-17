using DigitalSeal.Core.Models.Settings;
using DigitalSeal.Web.Models.ViewModels.Settings;

namespace DigitalSeal.Web.Extensions
{
    public static class SettingsProjections
    {
        public static UpdateUserModel ToModel(this UpdateUserRequest request)
            => new(
                request.Is2FAEnabled, request.Email, request.CurrentPassword, 
                request.NewPassword, request.ConfirmPassword);
    }
}