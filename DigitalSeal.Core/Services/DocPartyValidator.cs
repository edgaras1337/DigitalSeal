using DigitalSeal.Core.Exceptions;
using DigitalSeal.Data.Models;
using DigitalSeal.Data.Repositories;
using LanguageExt.Pretty;
using Microsoft.Extensions.Localization;

namespace DigitalSeal.Core.Services
{
    public class DocPartyValidator : IDocPartyValidator
    {
        private readonly IStringLocalizer<DocPartyService> _localizer;
        private readonly ISignatureService _signatureService;
        private readonly IDocPartyRepository _docPartyRepository;
        public DocPartyValidator(
            IStringLocalizer<DocPartyService> localizer, 
            ISignatureService signatureService, 
            IDocPartyRepository docPartyRepository)
        {
            _localizer = localizer;
            _signatureService = signatureService;
            _docPartyRepository = docPartyRepository;

        }

        public bool CanAdd(Party partyToAdd, int orgId, List<DocumentParty> existingParties,
            out ValidationException? ex)
        {
            ex = null;
            if (partyToAdd.OrganizationId != orgId)
            {
                ex = ValidationException.Error(_localizer["PartyNotFound"]);
            }
            else if (existingParties.Any(x => x.PartyId == partyToAdd.Id))
            {
                ex = ValidationException.Error(_localizer["DocPartyExists"]);
            }

            return ex == null;
        }

        public async Task<ValidationException?> CanAddAsync(IEnumerable<Party> parties, int docId, int orgId)
        {
            List<DocumentParty> currentDocParties = await _docPartyRepository.GetByDocIdAsync(docId);

            foreach (Party party in parties)
            {
                if (!CanAdd(party, orgId, currentDocParties, out ValidationException? ex))
                {
                    return ex;
                }
            }

            return null;
        }

        public bool CanRemove(DocumentParty? docParty, Document doc, out ValidationException? ex)
        {
            ex = null;
            if (docParty == null)
            {
                ex = ValidationException.Error(_localizer["PartyNotFound"]);
            }
            else if (docParty.Has(DocPermission.Owner))
            {
                ex = ValidationException.Error(_localizer["RemoveOwner"]);
            }
            else if (_signatureService.HasValidSignatures(doc, docParty.PartyId))
            {
                ex = ValidationException.Error(_localizer["RemoveSigned"]);
            }

            return ex == null;
        }

        public bool CanModify(Document? document, int userId, out ValidationException? ex, bool checkDeadline = true)
        {
            ex = null;
            if (document == null)
            {
                ex = ValidationException.Error(_localizer["DocNotFound"]);
            }
            else if (!IsAuthor(document, userId))
            {
                ex = ValidationException.Error(_localizer["NoPermission"]);
            }
            else if (checkDeadline && document.Deadline <= DateTime.UtcNow)
            {
                ex = ValidationException.Error(_localizer["DocDeadlineMissed"]);
            }

            return ex == null;
        }

        public bool IsAuthor(Document document, int userId) =>
             document.DocumentParties.Any(docParty => docParty.IsAuthor &&
             docParty.Party.UserId == userId);
    }
}
