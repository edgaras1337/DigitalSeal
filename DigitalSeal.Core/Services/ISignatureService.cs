using DigitalSeal.Core.Models.Signature;
using DigitalSeal.Data.Models;
using LanguageExt.Common;

namespace DigitalSeal.Core.Services
{
    /// <summary>
    /// Service class that provides document signing and signature verification functionality.
    /// </summary>
    public interface ISignatureService
    {
        /// <summary>
        /// Signs given document with an electronic signature.
        /// </summary>
        /// <param name="model">Data needed for signing.</param>
        /// <returns>Signing result.</returns>
        Task<Result<bool>> SignDocumentsAsync(SignModel model);

        /// <summary>
        /// Verifies document's signatures. If userID is specified, only that user's signatures are verified.
        /// </summary>
        /// <param name="pdfContent">Content of the document.</param>
        /// <param name="partyId">User, whose signatures will be checked. Parameter is optional.</param>
        /// <returns>Signature result.</returns>
        List<VerifySignaturesResult> VerifySignatures(byte[] pdfContent, int partyId = 0);

        bool HasValidSignatures(Document doc, int partyId);

        Dictionary<SignaturePage, List<SignaturePosition>> GetAvailableSignaturePositions(List<Document> docs);

        Task<Result<List<Document>>> GetSigneableDocumentsAsync(IEnumerable<int> docIds);

        SignStatus GetDocumentStatus(Document doc);
    }
}
