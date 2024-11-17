using AspNetCoreHero.ToastNotification.Abstractions;
using DigitalSeal.Core.ListProviders.PartyList;
using DigitalSeal.Core.ListProviders.PartyPendingList;
using DigitalSeal.Core.ListProviders.PartyPossibList;
using DigitalSeal.Core.Models.Organization;
using DigitalSeal.Core.Services;
using DigitalSeal.Core.Utilities;
using DigitalSeal.Data.Models;
using DigitalSeal.Web.Extensions;
using DigitalSeal.Web.Models.ViewModels.OrgEdit;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DigitalSeal.Web.Controllers
{
    [Route("organization")]
    public class OrgEditController : BaseDSController
    {
        private readonly IOrgService _orgService;
        private readonly ICurrentUserProvider _currUserProvider;
        private readonly IPartyListProvider _partyListProvider;
        private readonly IPartyPossibListProvider _partyPossibListProvider;
        private readonly IPartyPendingListProvider _partyPendingListProvider;
        private readonly IStringLocalizer<OrgEditController> _localizer;
        public OrgEditController(
            INotyfService notyf,
            IOrgService orgService,
            ICurrentUserProvider currUserProvider,
            IPartyListProvider partyListProvider,
            IPartyPossibListProvider partyPossibListProvider,
            IPartyPendingListProvider partyPendingListProvider,
            IStringLocalizer<OrgEditController> localizer)
            : base(notyf)
        {
            _orgService = orgService;
            _currUserProvider = currUserProvider;
            _partyListProvider = partyListProvider;
            _partyPossibListProvider = partyPossibListProvider;
            _partyPendingListProvider = partyPendingListProvider;
            _localizer = localizer;
        }

        [HttpGet]
        public async Task<IActionResult> Index(OrgEditPageModel model)
        {
            OrganizationModel? orgModel = await _orgService.GetOrganizationWithOwnerAsync(model.OrgId);
            if (orgModel == null)
            {
                return ErrorResult(Redirect(model.ReturnUrl), _localizer["OrgNotFound"]);
            }

            OrgEditViewModel viewModel = await CreateViewModelAsync(orgModel);
            return View(viewModel);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateOrgRequest request)
        {
            Result<CreateOrgResponse> result = await _orgService.CreateAsync(request.ToModel());
            string returnUrl = request.ReturnUrl ?? "/";
            return MatchResult(result, successResult =>
            {
                Notyf.Success(_localizer["Created"]);
                return RedirectToAction(nameof(Index), new OrgEditPageModel
                {
                    OrgId = successResult.OrgId,
                    ReturnUrl = returnUrl,
                });

            }, errorResult => Redirect(returnUrl));
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update(UpdateOrgRequest request)
        {
            Result<UpdateOrgResponse> result = await _orgService.UpdateAsync(request.ToModel());
            return MatchResult(result, _localizer["Updated"]);
        }

        [HttpPost("set-current")]
        public async Task<IActionResult> SetCurrent(int orgId)
        {
            Result<bool> result = await _orgService.SwitchAsync(orgId);
            return MatchResult(result, _localizer["Switched"]);
        }

        [HttpGet("parties/{orgId}")]
        public async Task<IActionResult> GetParties(PartyListRequest request)
            => Ok(await _partyListProvider.CreateListAsync(request));

        [HttpGet("parties/possible/{orgId}")]
        public async Task<IActionResult> GetPossibleParties(PartyPossibListRequest request)
            => Ok(await _partyPossibListProvider.CreateListAsync(request));

        [HttpGet("parties/pending/{orgId}")]
        public async Task<IActionResult> GetPossibleParties(PartyPendingListRequest request)
            => Ok(await _partyPendingListProvider.CreateListAsync(request));

        [HttpPost("parties/invite")]
        public async Task<IActionResult> InviteParties(InviteUsersRequest request)
        {
            Result<bool> result = await _orgService.InviteUsersAsync(request.ToModel());
            return MatchResult(result, _localizer["UsersInvited"]);
        }

        [HttpDelete("parties/kick")]
        public async Task<IActionResult> KickParties(KickUsersRequest request)
        {
            Result<bool> result = await _orgService.KickUsersAsync(request.ToModel());
            return MatchResult(result, _localizer["UsersRemoved"]);
        }

        [HttpDelete("leave")]
        public async Task<IActionResult> Leave(int orgId)
        {
            Result<bool> result = await _orgService.LeaveAsync(orgId);
            return MatchResult(result, _localizer["OrgLeft"]);
        }

        [HttpGet("respond")]
        public async Task<IActionResult> RespondToInvitation(RespondToOrgInviteRequest request)
        {
            Result<bool> result = await _orgService.RespondToInviteAsync(request.ToModel());
            string message = _localizer[request.IsAccept ? "InvitationAccepted" : "InvitationDeclined"];
            return MatchResult(result,
                _ => SuccessResult(Redirect(request.ReturnUrl), message),
                //err => ErrorResult(Redirect(request.ReturnUrl), err.FormatMessage()));
                _ => Redirect(request.ReturnUrl));
        }

        private async Task<OrgEditViewModel> CreateViewModelAsync(OrganizationModel model)
        {
            OrganizationModel.OwnerModel ownerModel = model.Owner;
            Party currParty = await _currUserProvider.GetCurrentPartyAsync();
            string createdDate = DateHelper.FormatDateTime(model.CreatedDate);
            bool isOwner = ownerModel.Id == currParty.UserId;
            string ownerName = UserHelper.FormatUserName(ownerModel.Id, ownerModel.FirstName, ownerModel.LastName);
            bool isCurrent = currParty.OrganizationId == model.Id;

            return new OrgEditViewModel
            {
                OrgId = model.Id,
                Name = model.Name,
                CreatedDate = createdDate,
                OwnerId = ownerModel.Id,
                IsOwner = isOwner,
                OwnerName = ownerName,
                IsCurrent = isCurrent,
                PartyColumnDefs = _partyListProvider.CreateColumnDefs(),
                PossiblePartyColumnDefs = _partyPossibListProvider.CreateColumnDefs(),
                PendingPartyColumnDefs = _partyPendingListProvider.CreateColumnDefs(),
            };
        }
    }
}
