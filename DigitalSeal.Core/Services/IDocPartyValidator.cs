using DigitalSeal.Core.Exceptions;
using DigitalSeal.Data.Models;

namespace DigitalSeal.Core.Services
{
    public interface IDocPartyValidator
    {
        bool CanAdd(Party partyToAdd, int orgId, List<DocumentParty> existingParties, out ValidationException? ex);
        Task<ValidationException?> CanAddAsync(IEnumerable<Party> parties, int docId, int orgId);
        bool CanRemove(DocumentParty? docParty, Document doc, out ValidationException? ex);
        bool CanModify(Document? document, int userId, out ValidationException? ex, bool checkDeadline = true);
        bool IsAuthor(Document document, int userId);
    }
}