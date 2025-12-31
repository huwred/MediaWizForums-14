using System.Collections.Generic;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace MediaWiz.Forums.ViewModels
{
    public class SearchViewModel : PublishedContentWrapped
    {

        public string query { get; set; }
        public string searchIn { get; set; }
        public string searchForum { get; set; }
        public string searchAuthor { get; set; }
        public string searchWhere { get; set; }
        public string searchWhen { get; set; }
        public string searchDate { get; set; }

        public long TotalResults { get; set; }
        public Dictionary<int,string> Forums { get; set; }
        public IEnumerable<ForumSearchResult> RawResult { get; set; }
        public IEnumerable<IPublishedContent> PagedResult { get; set; }

        public SearchViewModel(IPublishedContent content, IPublishedValueFallback publishedValueFallback) : base(content, publishedValueFallback)
        {
        }
    }

}