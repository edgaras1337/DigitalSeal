using DigitalSeal.Data.Models;

namespace DigitalSeal.Core.Services
{
    public interface IDocPartyNotificationService
    {
        void InformAddedParty(Document doc, int orgId, int currentUserId, int addedUserId);
        void InformRemovedParty(Document doc, int orgId, Party currParty, DocumentParty removedParty);
        Task SendDocPartyAddedEmailsAsync(Document doc, List<string> emails);
    }
}