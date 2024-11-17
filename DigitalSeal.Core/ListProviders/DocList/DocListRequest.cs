using DigitalSeal.Core.Utilities;

namespace DigitalSeal.Core.ListProviders.DocList
{
    public enum DocumentCategory { All, Personal, Involved, Pending };
    public record DocListRequest(DocumentCategory Category) : TimedRequest;
}
