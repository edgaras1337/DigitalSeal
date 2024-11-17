using DigitalSeal.Core.Models.Document;
using DigitalSeal.Data.Models;
using LanguageExt.Common;
using Microsoft.AspNetCore.Http;

namespace DigitalSeal.Core.Services
{
    public interface IDocService
    {
        Task<Result<bool>> CreateDocumentAsync(CreateDocumentModel formFile);
        Task<Result<bool>> DeleteAsync(int[] docIds);
        Task<Document?> GetByIdAsync(int docId, bool includeRelatedData = false, bool includeFileContent = false);
        Task<byte[]?> GetContentAsync(int docId);
        Task<Result<string>> GetStatusAsync(int docId);
        Task<Result<UpdateDocumentResult>> UpdateAsync(UpdateDocumentModel model);
    }
}