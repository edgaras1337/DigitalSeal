using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using DigitalSeal.Core.Exceptions;
using DigitalSeal.Web.Extensions;
using AspNetCoreHero.ToastNotification.Abstractions;

namespace DigitalSeal.Web.Filters
{
    public class ValidationFilter : IAsyncActionFilter
    {
        private readonly INotyfService _notyf;
        public ValidationFilter(INotyfService notyf)
        {
            _notyf = notyf;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ModelState.IsValid)
            {
                AddErrorToNotyf(context);
            }
            else
            {
                await next();
            }
        }

        private void AddErrorToNotyf(ActionExecutingContext context)
        {
            IEnumerable<string> messages = context.ModelState
                .SelectMany(x => x.Value!.Errors.Select(y => y.ErrorMessage));
            var ex = ValidationException.Error(messages);
            context.Result = new BadRequestObjectResult(ex);
            _notyf.Error(ex.FormatMessage());

        }
    }
}
