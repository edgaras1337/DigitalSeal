using DigitalSeal.Web.Models.ViewModels.Account;
using Microsoft.AspNetCore.Identity;
using DigitalSeal.Core.Services;
using DigitalSeal.Core.Models.Config.Email;
using DigitalSeal.Data.Models;

namespace DigitalSeal.Web.Services
{
    public class AccountService : IAccountService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        private readonly AppSignInManager _signInManager;
        private readonly IUserCertificateProvider _userCertificateProvider;

        public AccountService(
            AppDbContext context,
            UserManager<User> userManager,
            IEmailService emailService,
            AppSignInManager signInManager,
            IUserCertificateProvider userCertificateProvider)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _signInManager = signInManager;
            _userCertificateProvider = userCertificateProvider;
        }

        public async Task<RegisterResult> RegisterAsync(RegisterViewModel model)
        {
            User user = CreateUser(model);

            IdentityResult result = (model.IsExternalLogin) ?
                await _userManager.CreateAsync(user) :
                await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return new(result);

            await AddUserRole(user);
            //await AddClaimsAsync(user);
            await AddOrganizationAsync(user);

            int userId = user.Id;
            await _userCertificateProvider.AddRSAKeyAsync(userId);
            await _userCertificateProvider.AddNewCertificateAsync(userId);

            return new(result, user);
        }

        public async Task Init2FALoginAsync(User user, bool isPersistent)
        {
            await _signInManager.SignOutAsync();
            await _signInManager.SignInOrTwoFactorAsync(user, isPersistent);

            string token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
            string emailBody = $"Enter this code to confirm login: {token}";
            var emailMessage = new EmailMessage("Login Confirmation", emailBody, true, user.Email);
            await _emailService.SendEmailAsync(emailMessage);
        }

        public async Task SendEmailConfirmationAsync(User user, string emailToConfirm,
            EmailConfirmationType ConfirmationType, Func<ConfirmEmailModel, string> createUrl)
        {
            var registrationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmEmailModel = new ConfirmEmailModel(emailToConfirm, registrationToken);
            string confirmationUrl = createUrl(confirmEmailModel);
            var emailMsg = new EmailMessage("Registration confirmation", $"<a href='{confirmationUrl}'>Click here to finish up registration</a>", true, confirmEmailModel.Email);
            await _emailService.SendEmailAsync(emailMsg);
        }

        public async Task SendPasswordResetEmailAsync(User user, Func<ResetPasswordViewModel, string> createUrl)
        {
            string token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetPassModel = new ResetPasswordViewModel
            {
                Email = user.Email,
                Token = token,
            };

            string resetUrl = createUrl(resetPassModel);
            string urlMessage = $"<a href='{resetUrl}'>Click here to reset your password</a>";

            var emailMessage = new EmailMessage("Password Reset", urlMessage, true, user.Email!);
            await _emailService.SendEmailAsync(emailMessage);
        }

        //public async Task CreateInitialOrganization(string name)
        //{
        //    var organization = new Organization { Name = name };

        //    var userOrg = new Party
        //    {
        //        UserId = _currUserProvider.CurrentUserId,
        //        OrganizationId = organization.Id,
        //        Permission = (int)PartyPermission.Owner,
        //        IsCurrent = true,
        //        Organization = organization,
        //    };

        //    await _context.Parties.AddAsync(userOrg);
        //}

        private async Task AddUserRole(User user) =>
            await _userManager.AddToRoleAsync(user, nameof(RoleCode.User));

        private async Task AddOrganizationAsync(User user)
        {
            var organization = new Organization
            {
                Name = $"{user.FirstName}'s Organization",
            };

            var userOrg = new Party
            {
                UserId = user.Id,
                OrganizationId = organization.Id,
                Permission = (int)PartyPermission.Owner,
                IsCurrent = true,
                Organization = organization,
            };

            await _context.Parties.AddAsync(userOrg);
        }

        private static User CreateUser(RegisterViewModel model) => new()
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            UserName = model.Email,
            TwoFactorEnabled = true,
            IsOAuthAuthenticated = model.IsExternalLogin,
        };

        //private async Task AddClaimsAsync(User user)
        //{
        //    await UserManager.AddClaimAsync(user, new Claim(nameof(user.FirstName), user.FirstName));
        //    await UserManager.AddClaimAsync(user, new Claim(nameof(user.LastName), user.LastName));
        //}
    }
}
