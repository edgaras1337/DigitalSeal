using DigitalSeal.Data.Models;
using DigitalSeal.Web.Models.ViewModels.Account;

namespace DigitalSeal.Web.Services
{
    public interface IAccountService
    {
        Task Init2FALoginAsync(User user, bool isPersistent);
        Task<RegisterResult> RegisterAsync(RegisterViewModel model);
        Task SendEmailConfirmationAsync(User user, string emailToConfirm, EmailConfirmationType ConfirmationType, Func<ConfirmEmailModel, string> createUrl);
        Task SendPasswordResetEmailAsync(User user, Func<ResetPasswordViewModel, string> createUrl);
    }
}