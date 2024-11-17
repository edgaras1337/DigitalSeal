using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using DigitalSeal.Core.Services;
using Microsoft.Extensions.Logging;
using DigitalSeal.Core.ListProviders.Models;
using DigitalSeal.Data.Models;
using DigitalSeal.Core.Models.Signature;

namespace DigitalSeal.Core.ListProviders.DocPartyList
{
    public class DocPartyListProvider : ListProvider<DocPartyListRequest, DocPartyModel>, IDocPartyListProvider
    {
        private readonly ICurrentUserProvider _currUserProvider;
        private readonly IStringLocalizer _localizer;
        private readonly ISignatureService _signingService;

        public DocPartyListProvider(
            ILogger<DocPartyListProvider> logger, 
            AppDbContext context, 
            ICurrentUserProvider currUserProvider,
            IStringLocalizer<DocPartyListProvider> localizer, 
            ISignatureService signingService) 
            : base(logger, context)
        {
            _currUserProvider = currUserProvider;
            _localizer = localizer;
            _signingService = signingService;
        }

        protected override async Task<GridListResponse<DocPartyModel>> GetListAsync(DocPartyListRequest request)
        {
            Party currParty = await _currUserProvider.GetCurrentPartyAsync();
            Document? document = Context.Documents
                .AsNoTracking()
                .Include(x => x.FileContent)
                .FirstOrDefault(x => x.Id == request.DocId && x.DocumentParties.Any(x => x.PartyId == currParty.Id));
            if (document == null)
            {
                Logger.LogInformation("Document (ID: {id}) not found", request.DocId);
                return new([]);
            }


            int userId = currParty.UserId;
            List<DocumentParty> parties = await Context.DocumentParties
                .AsNoTracking()
                .Include(docParty => docParty.Party)
                    .ThenInclude(party => party.User)
                //.ThenInclude(user => user!.UserRoles)
                //    .ThenInclude(userRole => userRole.Role)
                .Include(docParty => docParty.SignatureInfos)
                .Where(docParty => docParty.DocId == request.DocId)
                .OrderBy(docParty => docParty.Id != userId)
                .ThenBy(docParty => docParty.Id)
                .ToListAsync();

            bool isAuthor = parties.Any(docParty => docParty.Party.UserId == userId && docParty.IsAuthor);
            List<RowModel<DocPartyModel>> partyList = parties.Select(docParty => CreateRowModel(docParty, document, userId, isAuthor)).ToList();

            return new GridListResponse<DocPartyModel>(partyList);
        }

        private RowModel<DocPartyModel> CreateRowModel(DocumentParty docParty, Document doc, int userId, bool isCurrentUserAuthor)
        {
            SignStatus status = GetPartySigningStatus(doc, docParty);
            bool isSelectable = isCurrentUserAuthor && docParty.Party.UserId != userId;
            return new RowModel<DocPartyModel>(CreateDocPartyModel(docParty, status), isSelectable, statusStyle[status]);
        }

        private readonly Dictionary<SignStatus, GridRowStyle> statusStyle = new()
        {
            [SignStatus.Signed] = GridRowStyle.Positive,
            [SignStatus.InProgress] = GridRowStyle.Primary,
            [SignStatus.SignedLate] = GridRowStyle.Warning,
            [SignStatus.NotSigned] = GridRowStyle.Negative,
        };

        private DocPartyModel CreateDocPartyModel(DocumentParty docParty, SignStatus status)
        {
            return new()
            {
                PartyId = docParty.PartyId,
                Email = docParty.Party.User.Email,
                FirstName = docParty.Party.User.FirstName,
                LastName = docParty.Party.User.LastName,
                SignatureStatus = _localizer[status.ToString()]
            };
        }

        private SignStatus GetPartySigningStatus(Document document, DocumentParty docParty)
        {
            byte[] content = document.FileContent.Content;
            VerifySignaturesResult? firstValid = GetFirstValidSignature(content, docParty.PartyId);

            if (firstValid == null)
            {
                return DateTime.UtcNow < document.Deadline ? 
                    SignStatus.InProgress : 
                    SignStatus.NotSigned;
            }

            return IsSignLate(firstValid, document) ? SignStatus.SignedLate : SignStatus.Signed;
        }

        private VerifySignaturesResult? GetFirstValidSignature(byte[] docContent, int userId)
        {
            IList<VerifySignaturesResult> signatureResults = _signingService.VerifySignatures(docContent, userId);

            VerifySignaturesResult? firstValid = signatureResults
                .OrderBy(s => s.SignTime)
                .FirstOrDefault(sRes => sRes.Status == SignatureVerificationStatus.VerificationPassed);

            return firstValid;
        }

        private static bool IsSignLate(VerifySignaturesResult signResult, Document document)
            => signResult.SignTime > document.Deadline;
    }
}
