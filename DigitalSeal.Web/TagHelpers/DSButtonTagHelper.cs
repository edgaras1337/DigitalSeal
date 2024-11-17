using DigitalSeal.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;

namespace DigitalSeal.Web.TagHelpers
{
    [HtmlTargetElement("simple-button")]
    public class SimpleButtonTagHelper : TagHelper
    {
        public ButtonType ButtonType { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsServerAction { get; set; }

        public string ButtonStateClass => ButtonType switch
        {
            ButtonType.Primary => "primary-button",
            ButtonType.Danger => "danger-button",
            _ => "secondary-button",
        };

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            await base.ProcessAsync(context, output);
            output.TagMode = TagMode.StartTagAndEndTag;
            output.TagName = "button";
            output.AddClass("button", HtmlEncoder.Default);
            output.AddClass(ButtonStateClass, HtmlEncoder.Default);

            var titleSpan = new TagBuilder("span");
            titleSpan.InnerHtml.Append(Title);
            titleSpan.AddCssClass("title");
            output.Content.AppendHtml(titleSpan);

            if (IsServerAction)
            {
                var loadingTag = new TagBuilder("div");
                loadingTag.AddCssClass("loading-container");
                loadingTag.AddCssClass("hidden");
                var imgTag = new TagBuilder("img")
                {
                    TagRenderMode = TagRenderMode.StartTag
                };
                imgTag.Attributes["src"] = "/images/loading.svg";
                loadingTag.InnerHtml.AppendHtml(imgTag);
                output.PostContent.AppendHtml(loadingTag);
            }
        }
    }
}


//    [HtmlTargetElement("ds-button")]
//    public class DSButtonTagHelper : TagHelper
//    {
//        public ButtonState State { get; set; }
//        public string Title { get; set; } = string.Empty;
//        public bool IsServerAction { get; set; }

//        public string ButtonStateClass => State switch
//        {
//            ButtonState.Positive => "primary-button",
//            ButtonState.Negative => "danger-button",
//            _ => "secondary-button",
//        };

//        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
//        {
//            await base.ProcessAsync(context, output);
//            output.TagName = "button";
//            output.AddClass("button", HtmlEncoder.Default);
//            output.AddClass(ButtonStateClass, HtmlEncoder.Default);

//            //var titleSpan = new TagBuilder("span");
//            //titleSpan.AddCssClass("title");
//            //output.Content.AppendHtml(titleSpan);

//            output.PreContent.SetHtmlContent("<span class=\"title\">");
//            output.PostContent.SetHtmlContent("</span>");

//            if (IsServerAction)
//            {
//                var loadingTag = new TagBuilder("div");
//                loadingTag.AddCssClass("loading-container");
//                loadingTag.AddCssClass("hidden");
//                var imgTag = new TagBuilder("img");
//                imgTag.TagRenderMode = TagRenderMode.StartTag;
//                imgTag.Attributes["src"] = "/images/loading.svg";
//                loadingTag.InnerHtml.AppendHtml(imgTag);
//                output.PostContent.AppendHtml(loadingTag);
//            }

//            //output.PostContent.AppendHtml(loadingTag);
//        }
//    }
//}
