using System;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace MediaWiz.Forums.Extensions
{
    public static class Extensions
    {
        public static string FirstCharToUpper(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
            };

        public static async Task<string> GetOrCreateDictionaryValue(this IDictionaryItemService dictionaryService, string key, string defaultValue, string isoCode = null)
        {
            var languageCode = isoCode ?? System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
            if (languageCode.StartsWith("en_"))
            {
                languageCode = "en";
            }

            var dictionaryItem = await dictionaryService.GetAsync(key);
            if (dictionaryItem == null)
            {
                // Try to find the item by splitting the key
                var parts = key.Split('.');
                IDictionaryItem currentItem = null;

                foreach (var part in parts)
                {
                    var partKey = currentItem is null ? part : $"{currentItem.ItemKey}.{part}";
                    currentItem = await dictionaryService.GetAsync(partKey);
                    if (currentItem == null)
                        break;
                }

                dictionaryItem = currentItem;
            }

            if(dictionaryItem == null)
            {
                ILanguage lang = new Language(languageCode, System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
                var newitem = new DictionaryItem(key);
                newitem.AddOrUpdateDictionaryValue(lang, defaultValue);
                await dictionaryService.CreateAsync(newitem, Guid.NewGuid());
                return defaultValue;
            }

            var currentValue = dictionaryItem.Translations?.FirstOrDefault(it => it.LanguageIsoCode == languageCode);
            if (!string.IsNullOrWhiteSpace(currentValue?.Value))
                return currentValue.Value;
            return $"[{key}]";
        }
    }
}
