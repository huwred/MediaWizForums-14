using Microsoft.AspNetCore.Razor.TagHelpers;

/// <summary>
/// A <see cref="TagHelper"/> that conditionally adds the "selected" attribute to an HTML <c>&lt;option&gt;</c>
/// element based on the specified value.
/// </summary>
/// <remarks>This tag helper is used to dynamically mark an <c>&lt;option&gt;</c> element as selected by
/// comparing its "value" attribute to the value of the <see cref="Selected"/> property. If the values match, the
/// "selected" attribute is added to the element.</remarks>
[HtmlTargetElement("option", Attributes = "selected-val")]
public class OptionSelectTagHelper : TagHelper
{
    /// <summary>
    /// Specifies a value to check to see if this option is selected.
    /// </summary>
    [HtmlAttributeName("selected-val")]
    public string Selected { get; set; }
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var tag = output.Attributes["value"];
        if (Selected != null && Selected == (string)tag.Value)
        {
            output.Attributes.Add("selected", "selected");
        }

    }
}
