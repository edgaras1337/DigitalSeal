using DigitalSeal.Core.Models.Document;
using DigitalSeal.Web.Models.ViewModels.DocEdit;

namespace DigitalSeal.Web.Extensions
{
    public static class DocEditProjections 
    {
        public static UpdateDocumentModel ToModel(this UpdateDocumentRequest request)
        {
            DateTime? deadline = string.IsNullOrEmpty(request.Deadline) ? 
                null : DateTime.Parse(request.Deadline);
            return new(request.DocId, request.Name, deadline);
        }
    }
}