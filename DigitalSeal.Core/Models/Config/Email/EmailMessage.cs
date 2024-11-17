using MimeKit;

namespace DigitalSeal.Core.Models.Config.Email
{
    public class EmailMessage
    {
        public IList<MailboxAddress>? To { get; set; }
        public string? Subject { get; set; }
        public string? Content { get; set; }

        public bool IsHtmlContent { get; set; }

        public EmailMessage(string subject, string content, params string[] to)
        {
            To = [.. to.Select(x => new MailboxAddress("email", x))];
            Subject = subject;
            Content = content;
        }

        public EmailMessage(string subject, string content, bool isHtmlContent, params string[] to)
            : this(subject, content, to)
        {
            IsHtmlContent = isHtmlContent;
        }
    }
}
