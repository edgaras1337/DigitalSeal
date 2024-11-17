using DigitalSeal.Data.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace DigitalSeal.Web.Services
{
    public class AppSignInManager : SignInManager<User>
    {
        public AppSignInManager(UserManager<User> userManager, IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<User> claimsFactory, IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<User>> logger, IAuthenticationSchemeProvider schemes,
            IUserConfirmation<User> confirmation)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
        }

        public async Task<SignInResult> SignInOrTwoFactorAsync(User user, bool isPersistant) =>
            await base.SignInOrTwoFactorAsync(user, isPersistant);
    }
}