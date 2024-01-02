using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using System.Text.Encodings.Web;

namespace DataPersist.SavedViews.TagHelpers;

// Delete link button

[HtmlTargetElement("a", Attributes = CountAttributeName)]
[HtmlTargetElement("a", Attributes = MessageAttributeName)]
public class DeleteTagHelper : TagHelper
{
    private const string CountAttributeName = "related-count";
    private const string MessageAttributeName = "related-message";

    [HtmlAttributeName(CountAttributeName)]
    public string? Count { get; set; }

    [HtmlAttributeName(MessageAttributeName)]
    public string? Message { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.RemoveAll("related-count");
        output.Attributes.RemoveAll("related-message");

        if (string.IsNullOrEmpty(Count) || Count == "0")  // Can be deleted
        {
            
        }
        else // cannot be deleted
        {
            output.Attributes.SetAttribute("href", "javascript:void(0);");
            output.Attributes.Add("data-bs-toggle", "tooltip");
            output.Attributes.Add("data-bs-placement", "left");
            output.Attributes.Add("data-bs-html", "true");
            output.Attributes.Add("title", Message);
            output.RemoveClass("js-confirm", HtmlEncoder.Default);
        }
    }
}
