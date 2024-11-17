using DigitalSeal.Core.Utilities;

namespace DigitalSeal.Core.ListProviders.Signatures
{
    public record SignaturesListRequest(int DocId) : TimedRequest;
}
