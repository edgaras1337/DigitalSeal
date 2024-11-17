using DigitalSeal.Core.ListProviders.Models;
using DigitalSeal.Core.Models.Signature;
using DigitalSeal.Core.Services;
using DigitalSeal.Core.Utilities;
using DigitalSeal.Data.Models;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace DigitalSeal.Core.ListProviders.Signatures
{
    public class SignaturesListProvider : ListProvider<SignaturesListRequest, SignaturesListModel>, ISignaturesListProvider
    {
        private readonly ISignatureService _signatureService;
        private readonly IDocService _docService;
        public SignaturesListProvider(ILogger<SignaturesListProvider> logger, AppDbContext context, ISignatureService signatureService, 
            IDocService docService, IStringLocalizer<SignaturesListProvider> localizer) 
            : base(logger, context, localizer)
        {
            _signatureService = signatureService;
            _docService = docService;
        }

        protected override GridRowSelectionMode SelectionMode => GridRowSelectionMode.None;

        protected override async Task<GridListResponse<SignaturesListModel>> GetListAsync(SignaturesListRequest request)
        {
            Document? doc = await _docService.GetByIdAsync(request.DocId, true);
            if (doc == null)
            {
                return new([]);
            }

            byte[] fileContent = doc.FileContent.Content;

            IList<VerifySignaturesResult> result = _signatureService.VerifySignatures(fileContent);
            return new(result.Select((res, i) => CreateRowModel(res, i, request.TimeZone)));
        }

        private RowModel<SignaturesListModel> CreateRowModel(VerifySignaturesResult result, int idx, string? timeZone)
        {
            var model = new SignaturesListModel
            {
                DummyId = idx,
                Status = Localizer[result.Status.ToString()],
                SignedBy = result.SignedBy,
                SignTime = DateHelper.ConvertAndFormat(result.SignTime, timeZone),
                Reason = result.Reason,
                Location = result.Location,
                Contact = result.Contact,
            };

            return new(model, false);
        }
    }
}
