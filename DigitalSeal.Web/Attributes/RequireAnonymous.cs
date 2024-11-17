using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using DigitalSeal.Web.Controllers;
using DigitalSeal.Core.Utilities;

namespace DigitalSeal.Web.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RequireAnonymous : Attribute, IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context?.HttpContext.User.Identity?.IsAuthenticated ?? false)
                context.Result = new RedirectToActionResult(nameof(DocListController.Index), StringHelper.ControllerName<DocListController>(), null);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
