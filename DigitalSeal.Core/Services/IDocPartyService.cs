using DigitalSeal.Core.Models.DocParty;
using DigitalSeal.Data.Models;
using LanguageExt.Common;

namespace DigitalSeal.Core.Services
{
    public interface IDocPartyService
    {
        Task<Result<bool>> AddAsync(AddDocPartyModel model);
        Task<Result<bool>> RemoveAsync(RemoveDocPartyModel model);
        Task<Result<Document>> GetModifiableDocAsync(int docId);
    }
}