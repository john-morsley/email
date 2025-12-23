namespace Morsley.UK.Email.API.TagHelpers;

[HtmlTargetElement("secret-alert")]
public class SecretAlertTagHelper : TagHelper
{
    public string? Value { get; set; }
    public string Label { get; set; } = "Secret";

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        if (!string.IsNullOrEmpty(Value))
        {
            output.Attributes.SetAttribute("class", "alert alert-dark");
            output.Content.SetHtmlContent($"<strong>{Label}:</strong> {Value}");
        }
        else
        {
            output.Attributes.SetAttribute("class", "alert alert-warning");
            output.Content.SetHtmlContent("Secret not found or empty");
        }
    }
}
