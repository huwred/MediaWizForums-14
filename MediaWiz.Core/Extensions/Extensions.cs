using System;
using System.Linq;
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

        public static string GetOrCreateDictionaryValue(this IDictionaryItemService dictionaryService, string key, string defaultValue,string isoCode = null)
        {
            var languageCode = isoCode ?? System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
            if (languageCode.StartsWith("en_"))
            {
                languageCode = "en";
            }
            var dictionaryItem = dictionaryService.GetAsync(key).Result ?? key.Split('.').Aggregate((IDictionaryItem)null, (item, part) =>
            {
                var partKey = item is null ? part : $"{item.ItemKey}.{part}";
                return dictionaryService.GetAsync(partKey).Result;
            });
            if(dictionaryItem == null)
            {
                ILanguage lang = new Language(languageCode,System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
                    var newitem = new DictionaryItem(key);
                    newitem.AddOrUpdateDictionaryValue(lang,defaultValue);
                    dictionaryService.CreateAsync(newitem,Guid.NewGuid());
                return defaultValue;
            }
            var currentValue = dictionaryItem.Translations?.FirstOrDefault(it => it.LanguageIsoCode == languageCode);
            if (!string.IsNullOrWhiteSpace(currentValue?.Value))
                return currentValue.Value;
            return $"[{key}]";
        }
    }
}
