using DigitalSeal.Core.Models.DocParty;
using DigitalSeal.Web.Models.ViewModels.DocParty;

namespace DigitalSeal.Web.Extensions
{
    public static class DocPartyProjections
    {
        public static AddDocPartyModel ToModel(this AddDocPartyRequest request)
            => new(request.DocId, request.PartyIds);

        public static RemoveDocPartyModel ToModel(this RemoveDocPartyRequest request)
            => new(request.DocId, request.PartyIds);
    }
}