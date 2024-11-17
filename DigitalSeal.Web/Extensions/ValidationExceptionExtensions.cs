using DigitalSeal.Core.Exceptions;
using DigitalSeal.Core.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DigitalSeal.Web.Extensions
{
    public static class ValidationExceptionExtensions
    {
        public static string FormatMessage(this ValidationException ex)
        {
            if (ex.Errors != null && ex.Errors.Count == 1)
                return ex.Errors[0];

            string output = "";
            if (!string.IsNullOrEmpty(ex.Message))
            {
                var header = new TagBuilder("h4");
                header.InnerHtml.Append(ex.Message);
                output += header.ToHtmlString();
            }

            if (ex.Errors != null)
            {
                foreach (string error in ex.Errors)
                {
                    var pTag = new TagBuilder("div");
                    pTag.InnerHtml.Append(error);
                    output += pTag.ToHtmlString();
                }
            }

            return output;



            //string? wrappedTitle = !string.IsNullOrEmpty(ex.Message) ? $"<h4>{ex.Message}</h4>" : null;

            //var sb = new StringBuilder(wrappedTitle);
            //if (ex.Errors != null)
            //{
            //    foreach (string error in ex.Errors)
            //        sb.Append($"<p>{error}</p>");
            //}
            //return sb.ToString();
        }
    }
}
