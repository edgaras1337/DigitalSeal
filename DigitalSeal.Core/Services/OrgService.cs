using DigitalSeal.Core.Exceptions;
using DigitalSeal.Core.Models.Invitation;
using DigitalSeal.Core.Models.Organization;
using DigitalSeal.Data.Models;
using DigitalSeal.Data.Repositories;
using LanguageExt.Common;
using Microsoft.Extensions.Localization;

namespace DigitalSeal.Core.Services
{
    internal class OrgService : IOrgService
    {
        private readonly IOrgRepository _orgRepository;
        private readonly IPartyRepository _partyRepository;
        private readonly IDocRepository _docRepository;
        private readonly ICurrentUserProvider _currUserProvider;
        private readonly IInvitationService _invitationService;
        private readonly IStringLocalizer<OrgService> _localizer;

        public OrgService(
            IOrgRepository orgRepository,
            IPartyRepository partyRepository,
            IDocRepository docRepository,
            ICurrentUserProvider currUserProvider,
            IInvitationService invitationService,
            IStringLocalizer<OrgService> localizer)
        {
            _orgRepository = orgRepository;
            _partyRepository = partyRepository;
            _docRepository = docRepository;
            _currUserProvider = currUserProvider;
            _invitationService = invitationService;
            _localizer = localizer;
        }

        public async Task<Result<CreateOrgResponse>> CreateAsync(CreateOrgModel model)
        {
            var organization = new Organization
            {
                Name = model.Name,
            };

            var ownerParty = new Party
            {
                OrganizationId = organization.Id,
                UserId = _currUserProvider.CurrentUserId,
                Permission = (int)PartyPermission.Owner,
            };

            organization.Parties.Add(ownerParty);

            await _orgRepository.AddAsync(organization);
            await _orgRepository.SaveChangesAsync();

            return new CreateOrgResponse(organization.Id);
        }

        public async Task<Result<UpdateOrgResponse>> UpdateAsync(UpdateOrgModel model)
        {
            string name = model.Name.Trim();
            int userId = _currUserProvider.CurrentUserId;
            Organization? org = await _orgRepository.GetByIdAsync(model.OrgId, userId);
            if (org == null)
            {
                return new(ValidationException.Error(_localizer["OrgNotFound"]));
            }

            Party party = org.Parties.First();
            if (!party.Has(PartyPermission.Owner) && !party.Has(PartyPermission.Delete))
            {
                return new(ValidationException.Error(_localizer["NoPermission"]));
            }

            org.Name = name;
            await _orgRepository.SaveChangesAsync();

            return new UpdateOrgResponse(name);
        }

        public async Task<Result<bool>> DeleteAsync(int[] orgIds)
        {
            int userId = _currUserProvider.CurrentUserId;

            List<Organization> organizations = await _orgRepository.GetByIdsAsync(
                orgIds, userId, includeRelatedData: true);

            int partyCount = await _orgRepository.GetUserPersonalOrgCount(userId);

            List<int> usersThatNeedSwitchOrgIds = [];
            bool foundAny = false;
            foreach (Organization organization in organizations)
            {
                ICollection<Party> parties = organization.Parties;

                Party party = parties.First(x => x.UserId == userId);

                int orgId = organization.Id;
                string orgName = organization.Name;

                string? errMessage = null;
                if (!party.Has(PartyPermission.Owner) && !party.Has(PartyPermission.Update))
                {
                    errMessage = _localizer["DeteleNoPermission", orgName];
                }
                else if (party.IsCurrent)
                {
                    errMessage = _localizer["DeleteCurrent", orgName];
                }
                else if (partyCount <= 1)
                {
                    errMessage = _localizer["DeleteLast", orgName];
                }

                if (errMessage != null)
                {
                    return new(ValidationException.Error(errMessage));
                }

                IEnumerable<int> partiesWithCurrentOrg = parties
                    .Where(x => x.IsCurrent && x.UserId != userId)
                    .Select(x => x.UserId);

                usersThatNeedSwitchOrgIds.AddRange(partiesWithCurrentOrg);

                foreach (DocumentParty docParty in parties.SelectMany(x => x.DocumentParties))
                {
                    _docRepository.Remove(docParty.Document);
                }

                if (!foundAny)
                {
                    foundAny = true;
                }

                _orgRepository.Remove(organization);
            }

            await _orgRepository.SaveChangesAsync();

            return true;
        }

        public async Task<Result<bool>> SwitchAsync(int orgId)
        {
            int userId = _currUserProvider.CurrentUserId;

            Party? partyToSelect = await _partyRepository.GetAsync(orgId, userId);
            if (partyToSelect == null)
            {
                return new(ValidationException.Error(_localizer["OrgNotFound"]));
            }

            Party currentParty = await _currUserProvider.GetCurrentPartyAsync();
            currentParty.IsCurrent = false;
            partyToSelect.IsCurrent = true;

            await _orgRepository.SaveChangesAsync();

            return true;
        }

        public async Task<Result<bool>> InviteUsersAsync(InviteUsersModel model)
        {
            int userId = _currUserProvider.CurrentUserId;
            Organization? org = await _orgRepository.GetByIdAsync(model.OrgId, userId);
            if (org == null)
            {
                return new(ValidationException.Error(_localizer["OrgNotFound"]));
            }

            var inviteToOrgModel = new InviteToOrgModel(model.OrgId, userId, model.UserIds);
            await _invitationService.InviteToOrganizationAsync(inviteToOrgModel);
            return true;
        }

        public async Task<Result<bool>> RespondToInviteAsync(Models.Organization.RespondToOrgInviteModel model)
        {
            int userId = _currUserProvider.CurrentUserId;

            var inviteModel = new GetInvitationModel(model.InviteId, userId, InviteType.Organization);
            Result<UserNotification> notifResult = await _invitationService.GetInvitationAsync(inviteModel);

            UserNotification notif = null!;
            Result<bool> result = notifResult.Map(success =>
            {
                notif = success;
                return true;
            });

            if (result.IsFaulted)
            {
                return result;
            }

            int orgId = notif.OrganizationId;
            if (model.IsAccept)
            {
                Party? existingParty = await _partyRepository.GetAsync(orgId, userId);
                if (existingParty != null)
                {
                    return new(ValidationException.Error(_localizer["AlreadyExists"]));
                }
            }

            var serviceReq = new RespondToInviteModel(notif, model.IsAccept);
            result = await _invitationService.RespondToInvitationAsync(serviceReq);
            if (result.IsFaulted)
            {
                return result;
            }

            if (!model.IsAccept)
            {
                return true;
            }

            var newParty = new Party
            {
                UserId = userId,
                Permission = (int)PartyPermission.Read,
                OrganizationId = orgId
            };

            await _partyRepository.AddAsync(newParty);
            await _partyRepository.SaveChangesAsync();

            return true;
        }

        public async Task<Result<bool>> KickUsersAsync(KickUsersModel model)
        {
            Party owner = await _currUserProvider.GetCurrentPartyAsync();
            int orgId = model.OrgId;
            if (owner.OrganizationId != orgId || !owner.Has(PartyPermission.Owner))
            {
                return new(ValidationException.Error(_localizer["NoPermission"]));
            }

            List<Party> parties = await _partyRepository.GetAsync(orgId);

            if (parties.Count == 0)
            {
                return new(ValidationException.Error(_localizer["OrgNotFound"]));
            }

            List<Document> docs = await GetPartyDocumentsAsync(parties.Select(x => x.Id).ToList());

            foreach (Party party in parties)
            {
                if (party.Has(PartyPermission.Owner))
                {
                    return new(ValidationException.Error(_localizer["KickOwner"]));
                }

                if (party.IsCurrent)
                {
                    Party nextParty = await _partyRepository.GetAnyOwnerPartyAsync(party.UserId);

                    nextParty.IsCurrent = true;
                }
            }

            _partyRepository.RemoveRange(parties);
            _docRepository.RemoveRange(docs);
            await _orgRepository.SaveChangesAsync();

            return true;
        }

        public async Task<Result<bool>> LeaveAsync(int orgId)
        {
            int userId = _currUserProvider.CurrentUserId;
            Party? party = await _partyRepository.GetAsync(orgId, userId);
            if (party == null)
            {
                return new(ValidationException.Error(_localizer["OrgNotFound"]));
            }

            if (party.IsCurrent)
            {
                return new(ValidationException.Error(_localizer["LeaveCurrent"]));
            }

            if (party.Has(PartyPermission.Owner))
            {
                return new(ValidationException.Error(_localizer["KickOwned"]));
            }

            List<Document> docs = await GetPartyDocumentsAsync([party.Id]);

            _docRepository.RemoveRange(docs);
            _partyRepository.Remove(party);
            await _orgRepository.SaveChangesAsync();

            return true;
        }

        public async Task<OrganizationModel?> GetOrganizationWithOwnerAsync(int orgId)
        {
            int userId = _currUserProvider.CurrentUserId;
            Organization? org = await _orgRepository.GetWithOwnerAsync(orgId, userId);

            if (org == null)
            {
                return null;
            }

            Party owner = org.Parties.First(x => x.Has(PartyPermission.Owner));

            var model = new OrganizationModel
            {
                Id = org.Id,
                Name = org.Name,
                Owner = new()
                {
                    Id = owner.User.Id,
                    FirstName = owner.User.FirstName,
                    LastName = owner.User.LastName,
                }
            };

            return model;
        }


        private async Task<List<Document>> GetPartyDocumentsAsync(List<int> partyIds)
        {
            List<Document> ownedDocs = await _docRepository.GetOwnedAsync(
                partyIds.Select(x => new PartyId(x)).ToList());
            return ownedDocs;
        }
    }
}
