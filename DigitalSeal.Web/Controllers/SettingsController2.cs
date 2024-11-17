using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigitalSeal.Web.Models;
using DigitalSeal.Web.Models.ViewModels.Settings;
using DigitalSeal.Data.Models;

namespace DigitalSeal.Web.Controllers
{
    [Route("settings")]
    public class SettingsController2 : BaseDSController
    {
        //private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly SignInManager<User> _signInManager;
        public SettingsController2(INotyfService notyf, UserManager<User> userManager,
            RoleManager<Role> roleManager, SignInManager<User> signInManager)
            : base(notyf)
        {
            //_unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            //SettingsViewModel? viewModel = await CreateSettingsViewModel();
            //return View(viewModel);
            return null;
        }

        [HttpPost("profile")]
        public async Task<IActionResult> UpdateProfile(SettingsViewModel model)
        {
            return null;
            //if (!ModelState.IsValid)
            //    return View(nameof(Index), model);

            //User? user = await GetCurrentUserAsync();
            //if (user == null)
            //    return RedirectToAction(ControllerName(nameof(AccountController.Login)), nameof(AccountController));

            //user.TwoFactorEnabled = model.Is2FAEnabled;

            //bool needSignOut = UpdateEmail(user, model);
            //(needSignOut, IActionResult? updateError) = await UpdatePasswordAsync(user, model);
            //if (updateError != null) return updateError;
            //await UpdateUserAsync(user);

            //if (!needSignOut)
            //    return RedirectToAction(nameof(Index));

            //await _signInManager.SignOutAsync();
            //return RedirectToAction(ControllerName(nameof(AccountController.Login)), nameof(AccountController));
        }


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

        public async Task<IActionResult> DeactivateAccount() => throw new NotImplementedException();

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
        //    User? user = await _userManager.FindByIdAsync(CurrentUserID.ToString());
        //    if (user == null)
        //        await _signInManager.SignOutAsync();
        //    return user;
        //}
    }
}
