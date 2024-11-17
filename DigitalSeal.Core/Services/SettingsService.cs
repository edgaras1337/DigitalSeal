using DigitalSeal.Core.Exceptions;
using DigitalSeal.Core.Extensions;
using DigitalSeal.Core.Models.Settings;
using DigitalSeal.Data.Models;
using LanguageExt.Common;
using Microsoft.AspNetCore.Identity;

namespace DigitalSeal.Core.Services
{
    public interface ISettingsService
    {
        Task<Result<UpdateUserResult>> UpdateUserAsync(UpdateUserModel model, string? emailConfirmUrl = null);
    }

    public class SettingsService : ISettingsService
    {
        private readonly UserManager<User> _userManager;
        private readonly ICurrentUserProvider _currentUserProvider;
        public SettingsService(UserManager<User> userManager, ICurrentUserProvider currentUserProvider)
        {
            _userManager = userManager;
            _currentUserProvider = currentUserProvider;
        }

        public async Task<Result<UpdateUserResult>> UpdateUserAsync(UpdateUserModel model, string? emailConfirmUrl = null)
        {
            int currentUserId = _currentUserProvider.CurrentUserId;
            User user = (await _userManager.FindByIdAsync(currentUserId.ToString()))!;

            user.TwoFactorEnabled = model.Is2FAEnabled;

            (bool emailUpdated, ValidationException? ex) = await UpdateEmailAsync(user, model.Email);
            if (ex != null)
            {
                return new(ex);
            }

            (bool passwordUpdated, ex) = await UpdatePasswordAsync(user, model);
            if (ex != null)
            {
                return new(ex);
            }


            IdentityResult result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return new(result.ToValidationException("Profile update failed"));
            }

            bool needSignOut = emailUpdated || passwordUpdated;
            string email = emailUpdated && !string.IsNullOrEmpty(model.Email) ? model.Email : user.Email;
            return new UpdateUserResult(needSignOut, user.TwoFactorEnabled, email, emailUpdated);
        }


        private async Task<(bool, ValidationException?)> UpdateEmailAsync(User user, string? newEmail)
        {
            if (string.IsNullOrEmpty(newEmail))
            {
                return (false, null);
            }

            if (!user.Email.Equals(newEmail, StringComparison.OrdinalIgnoreCase))
            {
                User? userWithEmail = await _userManager.FindByEmailAsync(newEmail);
                if (userWithEmail != null)
                {
                    return (false, ValidationException.Error("Email is already in use"));
                }
            }

            return (false, null);
        }

        private async Task<(bool, ValidationException?)> UpdatePasswordAsync(User user, UpdateUserModel model)
        {
            if (string.IsNullOrEmpty(model.NewPassword))
                return (false, null);

            if (string.IsNullOrEmpty(model.CurrentPassword))
            {
                return (false, ValidationException.Error("Current password is required"));
            }

            IdentityResult passResult = await _userManager.ChangePasswordAsync(
                user, model.CurrentPassword, model.NewPassword);

            if (!passResult.Succeeded)
            {
                return (false, passResult.ToValidationException("Password change failed"));
            }

            return (true, null);
        }
    }
}
