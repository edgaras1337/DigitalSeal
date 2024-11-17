using DigitalSeal.Core.Extensions;
using DigitalSeal.Core.Models.Notifications;
using DigitalSeal.Core.Services;
using DigitalSeal.Core.Utilities;
using DigitalSeal.Data.Models;
using DigitalSeal.Web.Controllers;
using DigitalSeal.Web.Models.ViewModels.DocEdit;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace DigitalSeal.Web.Services
{
    public class MessageCreator : IMessageCreator
    {
        private readonly LinkGenerator _linkGenerator;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public MessageCreator(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor)
        {
            _linkGenerator = linkGenerator;
            _httpContextAccessor = httpContextAccessor;
        }

        public HtmlMessage DeadlineChanged(Document doc)
        {
            string daysLeft = GetDaysLeftString(doc.Deadline);
            string docDeadlineStr = DateHelper.FormatDateTimeDropSeconds(doc.Deadline);
            string content = CreateTag("p", $"Deadline of the document #{doc.Id} {doc.Name} was changed to: {docDeadlineStr} ({daysLeft}).");
            return new()
            {
                Title = "Document deadline changed",
                Content = content
            };
        }

        public HtmlMessage DocPartyAdded(Document doc)
        {
            string docUrl = GetDocUrl(doc.Id);
            string daysLeft = GetDaysLeftString(doc.Deadline);
            string deadline = DateHelper.FormatDateTimeDropSeconds(doc.Deadline);

            string content = CreateParagraphWithLink(
                $"You were invited to sign document #{doc.Id} {doc.Name}, until {deadline} ({daysLeft}).",
                "Click here to view the document", docUrl);

            return new()
            {
                Title = "You were invited to sign a document",
                Content = content
            };
        }

        private static string CreateParagraphWithLink(string pContent, string aContent, string link)
        {
            var content = new TagBuilder("p");
            content.InnerHtml.AppendHtmlLine(pContent);
            content.InnerHtml.AppendHtml(CreateTag("a", aContent, ("href", link)));
            return content.ToHtmlString();
        }

        public HtmlMessage DocPartyRemoved(Document doc)
        {
            string message = $"You have been removed from document #{doc.Id} {doc.Name} and no longer need to sign it.";
            return new()
            {
                Title = "Removed from document parties",
                Content = CreateTag("p", message),
            };
        }

        public HtmlMessage DocumentSigned(Document doc, string userFullName)
        {
            string docUrl = GetDocUrl(doc.Id);
            string content = CreateParagraphWithLink(
                $"Document #{doc.Id} {doc.Name} has been signed by {userFullName}.",
                "Click here to view the document", docUrl);

            return new()
            {
                Title = $"Document #{doc.Id} {doc.Name} signed",
                Content = content
            };
        }

        public HtmlMessage DocumentSigningCompleted(Document doc, SignStatus status)
        {
            if (status != SignStatus.Signed && status != SignStatus.SignedLate)
                throw new ArgumentException("Invalid document status", nameof(status));

            string docUrl = GetDocUrl(doc.Id);
            string statusMessage = status == SignStatus.Signed ? "on time" : "late";

            string content = CreateParagraphWithLink(
                $"Document #{doc.Id} {doc.Name} signing has been completed {statusMessage}.",
                "Click here to view the document", docUrl);
            return new HtmlMessage
            {
                Title = $"Document #{doc.Id} {doc.Name} signing completed",
                Content = content
            };
        }


        private static string CreateTag(string tagName, string content, params (string, string)[] attribs)
        {
            var pTag = new TagBuilder(tagName);
            pTag.InnerHtml.AppendHtmlLine(content);
            foreach (var attrib in attribs)
                pTag.Attributes.Add(attrib.Item1, attrib.Item2);
            return pTag.ToHtmlString();
        }

        private string GetDocUrl(int docId) => _linkGenerator.GetUriByAction(_httpContextAccessor.HttpContext!,
            nameof(DocEditController.Index),
            StringHelper.ControllerName(nameof(DocEditController)),
            new DocEditPageModel { DocId = docId }) ?? "/";

        private static string GetDaysLeftString(DateTime targetDate)
        {
            TimeSpan timeLeft = targetDate - DateTime.UtcNow;

            int days = timeLeft.Days;
            int hours = timeLeft.Hours;

            string result = $"{days} days";
            if (hours > 0)
                result += $" and {hours} hours";

            return result;
        }
    }
}
