using AspNetCoreHero.ToastNotification.Abstractions;
using DigitalSeal.Core.ListProviders.OrgList;
using DigitalSeal.Core.Services;
using DigitalSeal.Web.Models.ViewModels;
using DigitalSeal.Web.Models.ViewModels.OrgList;
using DigitalSeal.Web.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DigitalSeal.Web.Controllers
{
    [Route("organizations")]
    public class OrgListController : BaseDSController
    {
        private readonly IOrgListProvider _orgListProvider;
        private readonly IOrgService _orgService;
        private readonly IStringLocalizer<OrgListController> _localizer;
        public OrgListController(
            INotyfService notyf, 
            IOrgListProvider orgListProvider, 
            IOrgService orgService,
            IStringLocalizer<OrgListController> orgListLocalizer) 
            : base(notyf)
        {
            _orgListProvider = orgListProvider;
            _orgService = orgService;
            _localizer = orgListLocalizer;
        }

        [HttpGet]
        public IActionResult Index()
        {
            string columnDefs = _orgListProvider.CreateColumnDefs();
            IDictionary<string, string> categories = EnumLocalizer.Localize<OrgCategory>(_localizer);
            var model = new ListPageModel
            {
                ColumnDefs = columnDefs,
                FilterPanel = new FilterPanelModel(categories)
            };

            return View(model);
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetOrgList(OrgListRequest request)
            => Ok(await _orgListProvider.CreateListAsync(request));

        [HttpPost("set-current")]
        public async Task<IActionResult> SetAsCurrent(int orgId)
            => MatchResult(await _orgService.SwitchAsync(orgId));

        [HttpDelete]
        public async Task<IActionResult> Delete(DeleteOrgsRequest request)
            => MatchResult(await _orgService.DeleteAsync(request.OrgIds), _localizer["Deleted"]);
    }
}
