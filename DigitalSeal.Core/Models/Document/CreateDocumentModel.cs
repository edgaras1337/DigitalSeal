using Microsoft.AspNetCore.Http;

namespace DigitalSeal.Core.Models.Document
{
    public record CreateDocumentModel(IFormFile FormFile);
}
