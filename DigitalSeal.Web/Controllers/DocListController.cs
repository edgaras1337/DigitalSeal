using AspNetCoreHero.ToastNotification.Abstractions;
using DigitalSeal.Web.Models.ViewModels.DocList;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using DigitalSeal.Web.Utilities;
using LanguageExt.Common;
using DigitalSeal.Core.Services;
using DigitalSeal.Core.ListProviders.DocList;
using DigitalSeal.Web.Extensions;
using DigitalSeal.Web.Models.ViewModels;

namespace DigitalSeal.Web.Controllers
{
    public class DocListController : BaseDSController
    {
        private readonly IDocService _docService;
        private readonly IDocListProvider _docListProvider;
        private readonly IStringLocalizer<DocListController> _localizer;
        public DocListController(
            INotyfService notyf, 
            IDocService docService, 
            IDocListProvider docListProvider,
            IStringLocalizer<DocListController> docListLocalizer) 
            : base(notyf)
        {
            _docService = docService;
            _docListProvider = docListProvider;
            _localizer = docListLocalizer;
        }

        [HttpGet]
        public IActionResult Index()
        {
            string columnDefs = _docListProvider.CreateColumnDefs();
            IDictionary<string, string> categories = EnumLocalizer.Localize<DocumentCategory>(_localizer);

            var viewModel = new ListPageModel
            {
                ColumnDefs = columnDefs,
                FilterPanel = new FilterPanelModel(categories)
            };
            return View(viewModel);
        }
        
        [HttpGet("documents")]
        public async Task<IActionResult?> GetDocuments(DocListRequest request)
            => Ok(await _docListProvider.CreateListAsync(request));

        [HttpPost("documents")]
        public async Task<IActionResult> CreateDocument([FromForm] CreateDocumentRequest request)
        {
            Result<bool> result = await _docService.CreateDocumentAsync(request.ToModel());
            return MatchResult(result, _localizer["DocCreated"]);
        }

        [HttpDelete("documents")]
        public async Task<IActionResult> DeleteDocuments(DeleteDocumentRequest request)
        {
            Result<bool> result = await _docService.DeleteAsync(request.DocIds);
            return MatchResult(result, _localizer["DocsDeleted"]);
        }
    }
}