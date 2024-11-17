using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Microsoft.Extensions.Localization;
using DigitalSeal.Core.Services;
using Microsoft.Extensions.Logging;
using DigitalSeal.Core.ListProviders.Models;
using DigitalSeal.Data.Models;
using DigitalSeal.Core.Models.Signature;
using DigitalSeal.Core.Utilities;

namespace DigitalSeal.Core.ListProviders.DocList
{
    public class DocListProvider : ListProvider<DocListRequest, DocListModel>, IDocListProvider
    {
        private readonly ICurrentUserProvider _currUserProvider;
        private readonly ISignatureService _signingService;
        public DocListProvider(ILogger<DocListProvider> logger, AppDbContext context, IStringLocalizer<DocListProvider> localizer, 
            ICurrentUserProvider currUserProvider, ISignatureService signingService) 
            : base(logger, context, localizer)
        {
            _currUserProvider = currUserProvider;
            _signingService = signingService;
        }

        protected override async Task<GridListResponse<DocListModel>> GetListAsync(DocListRequest request)
        {
            DocumentCategory category = request.Category;
            IQueryable<Document> docQuery = Context.Documents;
            if (category == DocumentCategory.Pending)
            {
                docQuery = docQuery.Include(doc => doc.FileContent);
            }

            Party currParty = await _currUserProvider.GetCurrentPartyAsync();

            List<Document> documents = await docQuery
                    //.Include(doc => doc.DocumentParties.Where(docParty => docParty.Has(DocumentPermissions.Owner)))
                    //.Include(doc => doc.DocumentParties.Where(docParty => PermissionHelper.Has(docParty.Permission, DocumentPermissions.Owner)))
                    .Include(doc => doc.DocumentParties.Where(docParty => ((DocPermission)docParty.Permission).HasFlag(DocPermission.Owner)))
                        .ThenInclude(docParty => docParty.Party)
                            .ThenInclude(party => party.User)
                    .Where(FilterByCategory(category, currParty.Id))
                    .OrderByDescending(doc => doc.Id)
                    .ToListAsync();

            if (category == DocumentCategory.Pending)
            {
                for (int i = documents.Count - 1; i >= 0; i--)
                {
                    if (IsSigned(documents[i]))
                    {
                        documents.RemoveAt(i);
                    }
                }
            }

            return new GridListResponse<DocListModel>(documents.Select(doc => CreateRowModel(doc, request.TimeZone)));
        }

        private static Expression<Func<Document, bool>> FilterByCategory(DocumentCategory category, int partyID)
        {
            return category switch
            {
                DocumentCategory.All or DocumentCategory.Pending => doc => doc.DocumentParties.Any(docParty => docParty.PartyId == partyID),
                DocumentCategory.Personal => doc => doc.DocumentParties.Any(docParty =>
                    ((DocPermission)docParty.Permission).HasFlag(DocPermission.Owner) && docParty.PartyId == partyID),
                DocumentCategory.Involved => doc => doc.DocumentParties.Any(docParty =>
                    !((DocPermission)docParty.Permission).HasFlag(DocPermission.Owner) && docParty.PartyId == partyID),
                _ => doc => doc.DocumentParties.Any(docParty => docParty.PartyId == partyID),
            };
        }

        private bool IsSigned(Document document)
        {
            byte[]? content = document?.FileContent?.Content;
            if (content == null)
            {
                return false;
            }

            IList<VerifySignaturesResult> signResult = _signingService.VerifySignatures(content, _currUserProvider.CurrentUserId);
            return signResult.Any(sRes => sRes.Status == SignatureVerificationStatus.VerificationPassed);
        }
        
        private static RowModel<DocListModel> CreateRowModel(Document document, string? timeZone)
        {
            var model = new DocListModel
            {
                Id = document.Id,
                CreatedDate = DateHelper.ConvertAndFormat(document.Created, timeZone),
                Name = document.Name,
                Author = UserHelper.FormatUserName(document.DocumentParties.Single(docParty =>
                    docParty.IsAuthor).Party.User)
            };

            return new(model);
        }
    }
}
