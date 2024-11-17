using Microsoft.AspNetCore.Html;
using System.Text.Encodings.Web;

namespace DigitalSeal.Core.Extensions
{
    public static class TagBuilderExtensions
    {
        public static string ToHtmlString(this IHtmlContent htmlContent)
        {
            using var sw = new StringWriter();
            htmlContent.WriteTo(sw, HtmlEncoder.Default);
            return sw.ToString();
        }
    }
}
