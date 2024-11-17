using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using DigitalSeal.Web.Models;
using LanguageExt.Common;
using System.Net;
using DigitalSeal.Core.Utilities;
using DigitalSeal.Core.Exceptions;
using DigitalSeal.Web.Extensions;

namespace DigitalSeal.Web.Controllers
{
    public enum ResponseState { Success, Warning, Error }

    public class BaseDSController : Controller
    {
        private readonly INotyfService _notyf;
        public BaseDSController(INotyfService notyf)
        {
            _notyf = notyf;
        }

        protected virtual async Task<GridResponse<TModel>> CreateGridResponseAsync<TEntity, TModel>(IList<TEntity> data, 
            Func<TEntity, Task<TModel>> createModelAsync) where TModel : class
        {
            var response = new GridResponse<TModel>
            {
                DataRows = new List<TModel>(data.Count)
            };

            foreach (TEntity entity in data)
                response.DataRows.Add(await createModelAsync(entity));

            return response;
        }

        //protected virtual async Task<IList<TModel>> MapListAsync<TEntity, TModel>(IList<TEntity> data,
        //    Func<TEntity, Task<TModel>> createModelAsync) where TModel : class
        //{
        //    var dataRows = new List<TModel>(data.Count);
        //    foreach (TEntity entity in data)
        //        dataRows.Add(await createModelAsync(entity));
        //    return dataRows;
        //}

        //protected virtual IList<TModel> MapList<TEntity, TModel>(IList<TEntity> data,
        //    Func<TEntity, TModel> createModel) where TModel : class
        //{
        //    var dataRows = new List<TModel>(data.Count);
        //    foreach (TEntity entity in data)
        //        dataRows.Add(createModel(entity));
        //    return dataRows;
        //}

        //protected IActionResult MatchResult<TResult>(DataResult<TResult> result)
        //{
        //    return result.Match((data, msg) => msg == null ? Ok(data) : SuccessResult(Ok(data), msg),
        //        (msg) => WarningResult(BadRequest(), msg),
        //        (msg) => ErrorResult(BadRequest(), msg));
        //}

        //protected void AddNotyf<TResult>(DataResult<TResult> result)
        //{
        //    switch (result.State)
        //    {
        //        case ResponseState.Success:
        //            Notyf.Success(result.Message);
        //            break;
        //        case ResponseState.Warning:
        //            Notyf.Warning(result.Message);
        //            break;
        //        case ResponseState.Error:
        //            Notyf.Error(result.Message);
        //            break;
        //    }
        //}

        //protected async Task<IActionResult> MatchResultAsync<TResult>(Task<DataResult<TResult>> resultTask)
        //{
        //    var result = await resultTask;
        //    return result.Match((data, msg) => msg == null ? Ok(data) : SuccessResult(Ok(data), msg),
        //        (msg) => WarningResult(BadRequest(), msg),
        //        (msg) => ErrorResult(BadRequest(), msg));
        //}

        protected IActionResult CreateNotyFromModelState()
        {
            string modelErrors = string.Join('\n', ModelState);
            return ErrorResult(BadRequest(), modelErrors);
        }

        protected INotyfService Notyf => _notyf;

        protected IActionResult SuccessResult(IActionResult result, string message) => 
            ResultWithNotification(result, ResponseState.Success, message);
        protected IActionResult WarningResult(IActionResult result, string message) => 
            ResultWithNotification(result, ResponseState.Warning, message);
        protected IActionResult ErrorResult(IActionResult result, string message) => 
            ResultWithNotification(result, ResponseState.Error, message);

        protected IActionResult ResultWithNotification(IActionResult result, ResponseState state, string message)
        {
            if (_notyf == null)
                return result;

            switch (state)
            {
                case ResponseState.Success:
                    _notyf.Success(message);
                    break;
                case ResponseState.Warning:
                    _notyf.Warning(message);
                    break;
                case ResponseState.Error:
                    _notyf.Error(message);
                    break;
                default:
                    break;
            }

            return result;
        }

        protected IActionResult RedirectHome() => 
            RedirectToAction(nameof(DocListController.Index), ControllerName(nameof(DocListController)));

        protected int CurrentUserId => AuthHelper.GetCurrentUserID(HttpContext) ?? 0;

        public static string ControllerName(string name) => StringHelper.ControllerName(name);

        public string CurrentControllerName => ControllerName(ControllerContext.ActionDescriptor.ControllerTypeInfo.Name);

        protected string HomeUrl => Url.Action(nameof(DocListController.Index), ControllerName(nameof(DocListController))) ?? "/";

        //protected IActionResult MatchResult<TResult>(Result<TResult> result, string? successMessage = null)
        //{
        //    return result.Match<IActionResult>(result =>
        //    {
        //        if (!string.IsNullOrEmpty(successMessage))
        //            Notyf.Success(successMessage);

        //        return new OkObjectResult(result);
        //    }, exception =>
        //    {
        //        if (exception is ValidationException validationException)
        //        {
        //            if (validationException.Level == ValidationException.ExceptionLevel.Warning)
        //                Notyf.Warning(validationException.FormatMessage());
        //            else
        //                Notyf.Error(validationException.FormatMessage());

        //            return new BadRequestObjectResult(validationException);
        //        }

        //        return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        //    });
        //}

        protected IActionResult MatchResult<TResult>(Result<TResult> result, string? successMessage = null)
        {
            return MatchResult(result, successResult =>
            {
                if (!string.IsNullOrEmpty(successMessage))
                    Notyf.Success(successMessage);

                return new OkObjectResult(successResult);
            }, exception => new BadRequestObjectResult(exception));
        }

        protected IActionResult MatchResult<TResult>(Result<TResult> result, Func<TResult, IActionResult> successResult, 
            Func<ValidationException, IActionResult> failResult)
        {
            return result.Match(successResult, exception =>
            {
                if (exception is ValidationException validationException)
                {
                    AddMessagesFromException(validationException);
                    return failResult(validationException);
                }

                return CreateInternalServerError();
            });
        }

        protected Task<IActionResult> MatchResultAsync<TResult>(Result<TResult> result, 
            Func<TResult, Task<IActionResult>> successResult,
            Func<ValidationException, IActionResult> failResult)
        {
            return result.Match(successResult, exception =>
            {
                if (exception is ValidationException validationException)
                {
                    AddMessagesFromException(validationException);
                    return Task.FromResult(failResult(validationException));
                }

                return Task.FromResult(CreateInternalServerError());
            });
        }

        private static IActionResult CreateInternalServerError() 
            => new StatusCodeResult((int)HttpStatusCode.InternalServerError);

        private void AddMessagesFromException(ValidationException validationException)
        {
            if (validationException.Level == ValidationException.ExceptionLevel.Warning)
            {
                Notyf.Warning(validationException.FormatMessage());
            }
            else
            {
                Notyf.Error(validationException.FormatMessage());
            }
        }
    }
}
