using DigitalSeal.Core.Models.Notifications;
using DigitalSeal.Data.Models;

namespace DigitalSeal.Core.Services
{
    public interface IMessageCreator
    {
        HtmlMessage DocPartyAdded(Document doc);
        HtmlMessage DocPartyRemoved(Document doc);
        HtmlMessage DeadlineChanged(Document doc);
        HtmlMessage DocumentSigned(Document doc, string userFullName);
        HtmlMessage DocumentSigningCompleted(Document doc, SignStatus status);
    }
}
