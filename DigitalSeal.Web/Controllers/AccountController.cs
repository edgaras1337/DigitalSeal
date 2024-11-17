using AspNetCoreHero.ToastNotification.Abstractions;
using DigitalSeal.Web.Models.ConfiurationModels;
using DigitalSeal.Web.Models.ViewModels.Account;
using DigitalSeal.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Security.Claims;
using DigitalSeal.Web.Attributes;
using DigitalSeal.Data.Models;
using DigitalSeal.Core.Services;
using DigitalSeal.Web.Extensions;

namespace DigitalSeal.Web.Controllers
{
    [AllowAnonymous]
    [Route("account")]
    public class AccountController : BaseDSController
    {
        private readonly AppSignInManager _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly LoginOptions _loginOptions;
        private readonly IAccountService _accountService;
        private readonly INotificationService _notificationService;
        private readonly ICurrentUserProvider _currUserProvider;
        public AccountController(
            AppSignInManager signInManager,
            UserManager<User> userManager,
            INotyfService notyf,
            IOptions<LoginOptions> loginOptions,
            IAccountService accountService,
            INotificationService notificationService,
            ICurrentUserProvider currUserProvider)
            : base(notyf)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _loginOptions = loginOptions.Value;
            _accountService = accountService;
            _notificationService = notificationService;
            _currUserProvider = currUserProvider;
        }

        private const string ReturnUrlKey = nameof(ReturnUrlKey);
        private bool IsLoginPersistant => _loginOptions.IsPersistant;

        [HttpGet("login")]
        [RequireAnonymous]
        public IActionResult Login(string? returnUrl)
        {
            TempData[ReturnUrlKey] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost("login")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginForm(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            User? user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return ErrorResult(View(nameof(Login), model), "Invalid email or password");
            }

            if (!user.EmailConfirmed)
            {
                return RedirectToEmailConfirmPage(user);
            }

            if (user.TwoFactorEnabled)
            {
                return await Init2FALoginAsync(user, model.ReturnUrl);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, IsLoginPersistant, false);
            if (result.Succeeded)
            {
                return Redirect(model.ReturnUrl ?? "/");
            }

            return ErrorResult(View(model), "Invalid email or password");
        }

        [HttpGet("login/2FA")]
        [RequireAnonymous]
        public IActionResult Login2FA(string? returnUrl) => View(new Login2FAViewModel { ReturnUrl = returnUrl });

        [HttpPost("login/2FA")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login2FA(Login2FAViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.TwoFactorSignInAsync("Email", model.Code, IsLoginPersistant, false);
            if (result.Succeeded)
            {
                return Redirect(model.ReturnUrl ?? "/");
            }

            return ErrorResult(View(), "Invalid code, please try again");
        }

        [HttpGet("login/oauth")]
        public async Task LoginOAuth()
        {
            string? returnUrl = TempData[ReturnUrlKey]?.ToString();
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(OAuthCallback), CurrentControllerName, new { returnUrl })
            });
        }

        [HttpGet("login/oauth/callback")]
        public async Task<IActionResult> OAuthCallback(string? returnUrl)
        {
            AuthenticateResult oauthResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!oauthResult.Succeeded)
            {
                return ErrorResult(RedirectToAction(nameof(Login)), "Identification error");
            }

            string email = oauthResult.Principal.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
            User? user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                var textInfo = CultureInfo.CurrentCulture.TextInfo;

                // Finish up registration
                return RedirectToAction(nameof(Register), new RegisterViewModel
                {
                    Email = email!,
                    FirstName = textInfo.ToTitleCase(oauthResult.Principal.FindFirst(ClaimTypes.GivenName)?.Value ?? ""),
                    LastName = textInfo.ToTitleCase(oauthResult.Principal.FindFirst(ClaimTypes.Surname)?.Value ?? ""),
                    IsExternalLogin = true,
                });
            }

            if (!user.EmailConfirmed)
            {
                return RedirectToEmailConfirmPage(user);
            }

            if (user.TwoFactorEnabled)
            {
                return await Init2FALoginAsync(user, returnUrl);
            }

            await _signInManager.SignInAsync(user, IsLoginPersistant);

            return Redirect(returnUrl ?? "/");
        }

        [HttpGet("register")]
        [RequireAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [RequireAnonymous]
        [HttpPost("register")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterForm(RegisterViewModel model)
        {
            User? existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return ErrorResult(View(nameof(Register), model), "User with this email already exists!");
            }

            (IdentityResult identityResult, User? createdUser) = await _accountService.RegisterAsync(model);

            if (identityResult.Succeeded && createdUser != null)
            {
                return RedirectToEmailConfirmPage(createdUser);
            }

            AddErrorsNotif(identityResult, "Registration failed");
            return View(nameof(Register), model);
        }

        //[HttpGet("create-organization")]
        //public IActionResult CreateOrganization(CreateOrganizationModel model)
        //    => View(model);

        //[HttpPost]
        //public async Task<IActionResult> CreateOrganizationForm(CreateOrganizationModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(nameof(CreateOrganization), model);
        //    }

        //    // TODO: Add verifications, that a user does not have an organization
        //    await _accountService.CreateInitialOrganization(model.OrganizationName);
        //    return Redirect(model.ReturnUrl ?? "/");
        //}

        [HttpGet("confirm-email/send")]
        public async Task<IActionResult> EmailConfirmation(EmailConfirmationModel model)
        {
            User? user = model.Type switch
            {
                EmailConfirmationType.Registration => await _userManager.FindByEmailAsync(model.Email),
                EmailConfirmationType.ChangeEmail => await _userManager.FindByIdAsync(CurrentUserId.ToString()),
                _ => null
            };

            if (user == null)
            {
                return ErrorResult(RedirectToAction(nameof(Login)), "User account not found");
            }

            await _accountService.SendEmailConfirmationAsync(
                user, model.Email, model.Type, CreateEmailConfirmationUrl);

            return View(model);
        }

        [HttpGet("confirm-email/validate")]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailModel model)
        {
            User? user;
            if (CurrentUserId > 0)
            {
                user = await _userManager.FindByIdAsync(CurrentUserId.ToString());
                if (user == null)
                {
                    return ErrorResult(RedirectToAction(nameof(Login)), "User account not found");
                }

                user.Email = model.Email;
                user.EmailConfirmed = false;
                await _userManager.UpdateAsync(user);
            }
            else
            {
                user = await _userManager.FindByEmailAsync(model.Email);
            }

            if (user == null)
            {
                return ErrorResult(RedirectToAction(nameof(Login)), "User account not found");
            }

            if (user.EmailConfirmed)
            {
                return RedirectHome();
            }

            //// If the token is null show the page with information, that the confirmation email has been sent.
            //if (string.IsNullOrEmpty(model.Token))
            //{
            //    return View();
            //}

            IdentityResult result = await _userManager.ConfirmEmailAsync(user, model.Token);
            if (!result.Succeeded)
            {
                return ErrorResult(RedirectToAction(nameof(Login)), "Email confirmation failed!");
            }

            string successMsg = "Thank you for confirming your email!";

            if (user.TwoFactorEnabled)
            {
                return SuccessResult(await Init2FALoginAsync(user), successMsg);
            }

            await _signInManager.SignInAsync(user, IsLoginPersistant);

            return SuccessResult(RedirectHome(), successMsg);
        }

        [HttpGet("forgot-password")]
        [RequireAnonymous]
        public IActionResult ForgotPassword() => View();

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPasswordForm(ForgotPasswordViewModel model)
        {
            string email = model.Email;
            User? user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return ErrorResult(BadRequest(), "User not found");
            }

            if (user.IsOAuthAuthenticated)
            {
                return WarningResult(BadRequest(), "This user is authenticated through a 3rd party provider");
            }

            await _accountService.SendPasswordResetEmailAsync(user,
                passResetModel => Url.Action(nameof(PasswordReset), CurrentControllerName, passResetModel, Request.Scheme)!);

            model.IsSent = true;

            return View(nameof(ForgotPassword), model);
        }

        [HttpGet("password-reset")]
        [RequireAnonymous]
        public IActionResult PasswordReset(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return RedirectHome();
            }

            return View(new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            });
        }

        [HttpPost("password-reset")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PasswordResetForm(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            User? user = await _userManager.FindByEmailAsync(model.Email ?? string.Empty);
            if (user == null)
            {
                return ErrorResult(View(), "User not found");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token ?? string.Empty, model.Password);
            if (result.Succeeded)
            {
                return SuccessResult(RedirectToAction(nameof(Login)), "Password reset successfully!");
            }

            AddErrorsNotif(result, "Password reset failed");
            return View(model);
        }

        [Authorize]
        [HttpGet("sign-out")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        [Authorize]
        [HttpGet("notification-count")]
        public async Task<IActionResult> GetNotificationCount()
        {
            int count = await _notificationService.GetUnseenCount(CurrentUserId);
            return Ok(count);
        }

        [Authorize]
        [HttpPost("see-notifications")]
        public async Task<IActionResult> SeeNotifications()
        {
            await _notificationService.MarkSeenAsync(CurrentUserId);
            return Ok();
        }

        [Authorize]
        [HttpGet("validate-timezone")]
        public async Task<IActionResult> ValidateTimezone(string clientTimezone)
        {
            User user = await _currUserProvider.GetCurrentUserAsync();
            string? lastTimezone = user.LastTimeZone;
            if (clientTimezone == lastTimezone)
            {
                return Ok(false);
            }

            user.LastTimeZone = clientTimezone;
            await _userManager.UpdateAsync(user);
            return Ok(true);
        }

        private async Task<IActionResult> Init2FALoginAsync(User user, string? returnUrl = null)
        {
            await _accountService.Init2FALoginAsync(user, IsLoginPersistant);
            return RedirectToAction(nameof(Login2FA), new Login2FAViewModel { ReturnUrl = returnUrl });
        }

        private RedirectToActionResult RedirectToEmailConfirmPage(User user)
            => RedirectToAction(nameof(EmailConfirmation), new EmailConfirmationModel(user.Email!));

        private void AddErrorsNotif(IdentityResult result, string title)
        {
            IEnumerable<string> errors = result.Errors.Select(x => x.Description);
            string message = Core.Exceptions.ValidationException.Error(title, errors).FormatMessage();
            Notyf.Error(message);
        }

        private string CreateEmailConfirmationUrl(ConfirmEmailModel model)
        {
            return Url.Action(
                    nameof(ConfirmEmail),
                    CurrentControllerName,
                    model, Request.Scheme) ?? string.Empty;
        }
    }
}
