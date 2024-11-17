using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DigitalSeal.Web.Models.ViewModels.Settings;
using DigitalSeal.Data.Models;
using DigitalSeal.Core.Services;
using LanguageExt.Common;
using DigitalSeal.Web.Extensions;
using DigitalSeal.Core.Models.Settings;
using DigitalSeal.Web.Models.ViewModels.Account;

namespace DigitalSeal.Web.Controllers
{
    [Route("settings")]
    public class SettingsController : BaseDSController
    {
        //private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ICurrentUserProvider _currUserProvider;

        private readonly ISettingsService _settingsService;
        public SettingsController(
            INotyfService notyf, 
            UserManager<User> userManager,
            RoleManager<Role> roleManager, 
            SignInManager<User> signInManager,
            ICurrentUserProvider currUserProvider,
            ISettingsService settingsService)
            : base(notyf)
        {
            //_unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _currUserProvider = currUserProvider;
            _settingsService = settingsService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            User? user = await _userManager.FindByIdAsync(_currUserProvider.CurrentUserId.ToString());
            if (user == null)
            {
                return ErrorResult(RedirectHome(), "User not found");
            }

            var viewModel = new SettingsViewModel
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Is2FAEnabled = user.TwoFactorEnabled,
                HasPassword = !user.IsOAuthAuthenticated,
                IsAdmin = User.IsInRole(nameof(RoleCode.Admin)),
            };

            return View(viewModel);
        }

        [HttpPost("update-profile")]
        public async Task<IActionResult> UpdateProfile(UpdateUserRequest request)
        {
            Result<UpdateUserResult> result = await _settingsService.UpdateUserAsync(request.ToModel());
            result = result.Map(res =>
            {
                if (res.EmailUpdated)
                {
                    res.EmailConfirmationUrl = Url.Action(
                        nameof(AccountController.EmailConfirmation),
                        ControllerName(nameof(AccountController)),
                        new EmailConfirmationModel(res.Email, EmailConfirmationType.ChangeEmail));
                }

                return res;
            });

            return MatchResult(result, "Profile updated successfully");
        }


        //[HttpPost("deactivate")]
        //public async Task<IActionResult> DeactivateAccount()
        //{
        //    User? user = await QueryUsersForRemoval()
        //        .FirstOrDefaultAsync(u => u.Id == CurrentUserID);
        //    if (user == null)
        //        return ErrorResult(RedirectToAction(nameof(Index)), "User not found");

        //    if (User.IsInRole(nameof(RoleCode.Admin)))
        //        return ErrorResult(RedirectToAction(nameof(Index)), "Administrator cannot deactivate his account");

        //    await DeleteUserAsync(user);

        //    await _unitOfWork.SaveChangesAsync();
        //    await _signInManager.SignOutAsync();

        //    return SuccessResult(RedirectToAction(nameof(AccountController.Login), ControllerName(nameof(AccountController))),
        //        "Account deleted successfully");
        //}


        //[Authorize(Roles = nameof(RoleCode.Admin))]
        //[HttpGet("users")]
        //public async Task<IActionResult> GetUsers()
        //{
        //    try
        //    {
        //        List<User> users = await _userManager.Users
        //            .Include(u => u.UserRoles)
        //            .ThenInclude(ur => ur.Role)
        //            .Where(user => user.Id != CurrentUserID)
        //            .Where(user => user.Id != CurrentUserID &&
        //                user.UserRoles.Any(ur => ur.Role!.Name != nameof(RoleCode.Admin)))
        //            .ToListAsync();

        //        GridResponse<UserModel> response = await CreateGridResponseAsync(users, user => Task.FromResult(new UserModel(user)));
        //        return Json(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        return ErrorResult(BadRequest(), ex.Message);
        //    }
        //}

        ////[Authorize(Roles = nameof(RoleCode.Admin))]
        ////[HttpDelete("users")]
        ////public async Task<IActionResult> DeleteUsers(int[] userIDs)
        ////{
        ////    if (userIDs == null || !userIDs.Any())
        ////        return ErrorResult(NotFound(), "No user was selected");

        ////    using IDbContextTransaction? transaction = await _unitOfWork.BeginTransactionAsync();
        ////    try
        ////    {
        ////        List<User> users = await QueryUsersForRemoval()
        ////            .Where(doc => userIDs.Contains(doc.Id))
        ////            .ToListAsync();

        ////        if (users.Any(u => u.Id == CurrentUserID))
        ////            return ErrorResult(Ok(), "Cannot delete your account");

        ////        foreach (User user in users)
        ////            await DeleteUserAsync(user);

        ////        await _unitOfWork.SaveChangesAsync();
        ////        await transaction.CommitAsync();
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        await transaction.RollbackAsync();
        ////        return ErrorResult(BadRequest(), ex.Message);
        ////    }

        ////    return SuccessResult(Ok(), "Users deleted successfully");
        ////}

        //public async Task<IActionResult> DeactivateAccount() => throw new NotImplementedException();

        ////[Authorize]
        ////[HttpPost("deactivate")]
        ////[ValidateAntiForgeryToken]
        ////public async Task<IActionResult> DeactivateAccount()
        ////{
        ////    User? user = await QueryUsersForRemoval()
        ////        .FirstOrDefaultAsync(u => u.Id == CurrentUserID);
        ////    if (user == null)
        ////        return ErrorResult(RedirectToAction(nameof(Index)), "User not found");

        ////    if (User.IsInRole(nameof(RoleCode.Admin)))
        ////        return ErrorResult(RedirectToAction(nameof(Index)), "Administrator cannot deactivate his account");

        ////    await DeleteUserAsync(user);

        ////    await _unitOfWork.SaveChangesAsync();
        ////    await _signInManager.SignOutAsync();

        ////    return SuccessResult(RedirectToAction(nameof(AccountController.Login), ControllerName(nameof(AccountController))), 
        ////        "Account deleted successfully");
        ////}

        ////private async Task<SettingsViewModel?> CreateSettingsViewModel()
        ////{
        ////    User? user = await _userManager.FindByIdAsync(CurrentUserID.ToString());
        ////    if (user == null)
        ////        return null;

        ////    return new SettingsViewModel
        ////    {
        ////        UserId = user.Id,
        ////        Email = user.Email,
        ////        FirstName = user.FirstName,
        ////        LastName = user.LastName,
        ////        Is2FAEnabled = user.TwoFactorEnabled,
        ////        HasPassword = !user.IsOAuthAuthenticated,
        ////        IsAdmin = User.IsInRole(nameof(RoleCode.Admin)),
        ////    };
        ////}

        ////private async Task DeleteUserAsync(User user)
        ////{
        ////    IEnumerable<DocumentParty> ownerParties = user.DocumentParties.Where(docParty => docParty.IsAuthor);
        ////    foreach (DocumentParty party in ownerParties)
        ////    {
        ////        Document? document = party.Document;
        ////        if (document == null)
        ////            continue;

        ////        _unitOfWork.DocumentRepository.Delete(document);
        ////    }

        ////    IEnumerable<Party> ownerOrgs = user.UserOrganizations.Where(userOrg => userOrg.IsOwner);
        ////    foreach (Party party in ownerOrgs)
        ////    {
        ////        Organization? organization = party.Organization;
        ////        if (organization == null)
        ////            continue;

        ////        _unitOfWork.OrganizationRepository.Delete(organization);
        ////    }

        ////    await _userManager.DeleteAsync(user);
        ////}

        ////private IQueryable<User> QueryUsersForRemoval()
        ////{
        ////    return _userManager.Users
        ////        .AsQueryable()
        ////        .Include(user => user.ReceivedNotifications)
        ////        .Include(user => user.SentNotifications)
        ////        .Include(user => user.ReceivedInvites)
        ////        .Include(user => user.SentInvites)
        ////        .Include(user => user.DocumentParties)
        ////            .ThenInclude(docParty => docParty.Document)
        ////        .Include(user => user.UserOrganizations)
        ////            .ThenInclude(uOrg => uOrg.Organization);
        ////}

        //private static bool UpdateEmail(User user, SettingsViewModel model)
        //{
        //    if (user.Email?.ToLower() != model.Email?.ToLower())
        //    {
        //        user.Email = model.Email;
        //        user.EmailConfirmed = false;

        //        return true;
        //    }

        //    return false;
        //}

        //private async Task<(bool, IActionResult?)> UpdatePasswordAsync(User user, SettingsViewModel model)
        //{
        //    if (string.IsNullOrEmpty(model.NewPassword))
        //        return (false, null);

        //    if (string.IsNullOrEmpty(model.CurrentPassword))
        //    {
        //        ModelState.AddModelError(string.Empty, "Current Password is required");
        //        return (false, View(nameof(Index), model));
        //    }

        //    IdentityResult passResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        //    if (!passResult.Succeeded)
        //        return (false, OnPassChangeFail(passResult, model));

        //    return (true, null);
        //}

        //private IActionResult OnPassChangeFail(IdentityResult passResult, SettingsViewModel model)
        //{
        //    foreach (var error in passResult.Errors)
        //        ModelState.AddModelError(string.Empty, error.Description);
        //    return View(nameof(Index), model);
        //}

        //private async Task UpdateUserAsync(User user)
        //{
        //    IdentityResult result = await _userManager.UpdateAsync(user);
        //    if (result.Succeeded)
        //        Notyf?.Success("Changes saved successfully");
        //    else
        //    {
        //        foreach (var error in result.Errors)
        //            ModelState.AddModelError(string.Empty, error.Description);
        //    }
        //}

        //private async Task<User?> GetCurrentUserAsync()
        //{
        //    int currentUserId = _currUserProvider.CurrentUserId;
        //    User? user = await _userManager.FindByIdAsync(currentUserId.ToString());
        //    if (user == null)
        //        await _signInManager.SignOutAsync();
        //    return user;
        //}
    }
}
