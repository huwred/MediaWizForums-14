using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaWiz.Forums.TagHelpers
{ 
    /// <summary>
    /// A custom <see cref="TagHelper"/> that stores the inner content of the `<storecontent>` tag into a temporary storage,
    /// allowing it to be retrieved and reused later in the same request.
    /// </summary>
    /// <remarks>The content is stored in the <see cref="ViewContext.TempData"/> dictionary under a predefined key. If
    /// the <c>asp-key</c> attribute is specified, the content is stored in a sub-key corresponding to the provided value.
    /// If no key is specified, the content is stored in a default list. This tag helper is useful for scenarios where
    /// content needs to be dynamically aggregated or reused across different parts of a view.</remarks>
    [HtmlTargetElement("storecontent")]
    public class StoreContentTagHelper : TagHelper
    {
        private const string KeyAttributeName = "asp-key";
        private const string _storageKey = "storecontent";
        private const string _defaultListKey = "DefaultKey";

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        [HtmlAttributeName(KeyAttributeName)]
        public required string Key { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.SuppressOutput();
            TagHelperContent childContent = await output.GetChildContentAsync();

            var storageProvider = ViewContext!.TempData;
            Dictionary<string, List<HtmlString>> storage;
            List<HtmlString> defaultList;

            if (!storageProvider.ContainsKey(_storageKey) || !(storageProvider[_storageKey] is Dictionary<string,List<HtmlString>>))
            {
                storage = new Dictionary<string, List<HtmlString>>();
                storageProvider[_storageKey] = storage;
                defaultList = new List<HtmlString>();
                storage.Add(_defaultListKey, defaultList);
            }
            else
            {
                storage = ViewContext.TempData[_storageKey] as Dictionary<string, List<HtmlString>>;
                if (storage!.ContainsKey(_defaultListKey))
                {
                    defaultList = storage[_defaultListKey];

                }
                else
                {
                    defaultList = new List<HtmlString>();
                    storage.Add(_defaultListKey, defaultList);
                }
            }

            if (string.IsNullOrEmpty(Key))
            {
                defaultList.Add(new HtmlString(childContent.GetContent()));
            }
            else
            {
                if(storage.ContainsKey(Key))
                {
                    storage[Key].Add(new HtmlString(childContent.GetContent()));
                }
                else
                {
                    storage.Add(Key, new List<HtmlString>() { new HtmlString(childContent.GetContent()) });
                }
            }
        }
    } 
    }