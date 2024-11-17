using DigitalSeal.Core.Exceptions;
using DigitalSeal.Core.Models.DocParty;
using DigitalSeal.Data.Models;
using DigitalSeal.Data.Repositories;
using LanguageExt.Common;

namespace DigitalSeal.Core.Services
{
    public static class ResultExtensions
    {
        public static T GetResult<T>(this Result<T> result)
        {
            return result.Match(
                successObject => successObject, 
                exception => throw exception);
        }

        public static Exception GetException<T>(this Result<T> result)
        {
            return result.Match(
                success => throw new InvalidOperationException("Failure result expected."),
                exception => exception);
        }
    }

    public class DocPartyService : IDocPartyService
    {
        private readonly IPartyRepository _partyRepository;
        private readonly IDocPartyRepository _docPartyRepository;
        private readonly IDocService _docService;
        private readonly ICurrentUserProvider _currUserProvider;
        private readonly IDocPartyValidator _validator;
        private readonly IDocPartyNotificationService _notificationService;
        public DocPartyService(
            IPartyRepository partyRepository, 
            IDocPartyRepository docPartyRepository,
            IDocService docService, 
            ICurrentUserProvider currUserProvider, 
            IDocPartyValidator validator,
            IDocPartyNotificationService notificationService)
        {
            _partyRepository = partyRepository;
            _docPartyRepository = docPartyRepository;
            _docService = docService;
            _currUserProvider = currUserProvider;
            _validator = validator;
            _notificationService = notificationService;
        }

        public async Task<Result<bool>> AddAsync(AddDocPartyModel model)
        {
            Result<Document> docRes = await GetModifiableDocAsync(model.DocId);
            if (docRes.IsFaulted)
            {
                return docRes.Map(_ => false);
            }
            Document doc = docRes.GetResult();

            Party currParty = await _currUserProvider.GetCurrentPartyAsync();
            int orgId = currParty.OrganizationId;

            List<Party> partiesToAdd = await _partyRepository.GetAsync(model.PartyIds, orgId);

            ValidationException? addForbidden = await _validator.CanAddAsync(partiesToAdd, doc.Id, orgId);
            if (addForbidden != null)
            {
                return new(addForbidden);
            }

            var emails = new List<string>(partiesToAdd.Count);
            foreach (Party party in partiesToAdd)
            {
                AddNewDocParty(model.DocId, party.Id);
                _notificationService.InformAddedParty(doc, orgId, currParty.UserId, party.UserId);

                emails.Add(party.User.Email);
            }

            await _docPartyRepository.SaveChangesAsync();

            // TODO: Consider awaiting it.
            _ = _notificationService.SendDocPartyAddedEmailsAsync(doc, emails);

            return true;
        }

        public async Task<Result<bool>> RemoveAsync(RemoveDocPartyModel model)
        {
            Result<Document> docRes = await GetModifiableDocAsync(model.DocId);
            if (docRes.IsFaulted)
            {
                return docRes.Map(_ => false);
            }
            Document doc = docRes.GetResult();

            Party currParty = await _currUserProvider.GetCurrentPartyAsync();
            int orgId = currParty.OrganizationId;

            IEnumerable<DocumentParty> partiesToRemove = doc.DocumentParties
                .Where(x => model.PartyIds.Contains(x.PartyId));

            foreach (DocumentParty docParty in partiesToRemove)
            {
                if (!_validator.CanRemove(docParty, doc, out ValidationException? removeForbidden))
                {
                    return new(removeForbidden);
                }
            }

            foreach (DocumentParty partyToRemove in partiesToRemove)
            {
                _docPartyRepository.Remove(partyToRemove);
                _notificationService.InformRemovedParty(doc, orgId, currParty, partyToRemove);
            }

            await _docPartyRepository.SaveChangesAsync();

            return true;
        }

        
        public async Task<Result<Document>> GetModifiableDocAsync(int docId)
        {
            Document? doc = await _docService.GetByIdAsync(docId, includeRelatedData: true);
            if (doc == null)
            {
                // TODO: Localize.
                return new(ValidationException.Error("Document not found"));
            }

            int userId = _currUserProvider.CurrentUserId;
            if (_validator.CanModify(doc, userId, out ValidationException? modifyForbidden))
            {
                return new(doc);
            }
            return new(modifyForbidden);
        }


        private void AddNewDocParty(int docId, int partyId)
        {
            DocumentParty docParty = CreateDocParty(docId, partyId);
            _docPartyRepository.Add(docParty);
        }

        private static DocumentParty CreateDocParty(int docId, int partyId, PartyPermission permission = PartyPermission.Read)
        {
            return new()
            {
                DocId = docId,
                PartyId = partyId,
                Permission = (int)permission
            };
        }
    }
    //public record GetModifiableDocResult(Document? Doc, ValidationException? Ex);
}
