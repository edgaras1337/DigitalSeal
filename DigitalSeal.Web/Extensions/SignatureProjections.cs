using DigitalSeal.Core.Models.Signature;
using DigitalSeal.Web.Models.ViewModels.Signature;

namespace DigitalSeal.Web.Extensions
{
    public static class SignatureProjections
    {
        public static SignModel ToModel(this SignRequest request)
        {
            return new()
            {
                DocIds = [.. request.DocIds],
                Reason = request.Reason,
                Contact = request.Contact,
                Location = request.Location,
                Position = request.Position,
                Page = request.Page
            };
        }
    }
}