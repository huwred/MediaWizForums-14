using Examine;
using global::Umbraco.Cms.Infrastructure.Examine;
using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Models;

namespace MediaWiz.Forums.Indexing
{
    public class ForumMemberIndexValueSetBuilder : IValueSetBuilder<IMember>
    {
        public IEnumerable<ValueSet> GetValueSets(params IMember[] members)
        {
            foreach (IMember content in members)
            {
                var indexValues = new Dictionary<string, object>
                {
                    // this is a special field used to display the content name in the Examine dashboard
                    [UmbracoExamineFieldNames.NodeNameFieldName] = content.Name!,
                    ["name"] = content.Name!,
                    // add the fields you want in the index
                    ["id"] = content.Id,
                    ["joinedDate"] = content.GetValue<DateTime?>("joinedDate"),
                    ["postCount"] = content.GetValue<int>("postCount"),
                    ["lastLogin"] = content.LastLoginDate,
                    ["hasVerifiedAccount"] = content.GetValue<bool>("hasVerifiedAccount") ? 1 : 0
                };

                yield return new ValueSet(content.Id.ToString(), IndexTypes.Content, content.ContentType.Alias, indexValues);
            }
        }

        // filter out all content types except "product"
        private bool CanAddToIndex(IContent content) => content.ContentType.Alias == "forumMember";
    }
}
