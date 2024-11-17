using DigitalSeal.Core.Models.Document;
using DigitalSeal.Web.Models.ViewModels.DocList;

namespace DigitalSeal.Web.Extensions
{
    public static class DocListProjections
    {
        public static CreateDocumentModel ToModel(this CreateDocumentRequest request)
            => new(request.File!);
    }
}