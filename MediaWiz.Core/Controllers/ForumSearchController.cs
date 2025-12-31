using Examine;
using Examine.Lucene.Providers;
using Examine.Lucene.Search;
using Examine.Search;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using MediaWiz.Forums.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Extensions;

namespace MediaWiz.Forums.Controllers
{

    public class ForumSearchController : RenderController
    {
        private readonly IPublishedContentQuery _publishedContentQuery;
        private readonly IExamineManager _examineManager;
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly ServiceContext _serviceContext;
        private readonly ForumSearchService _searchService;

        public ForumSearchController(ILogger<SearchPageController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor, IVariationContextAccessor variationContextAccessor, ServiceContext context,IPublishedContentQuery publishedContentQuery
            , ForumSearchService searchService)
            : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            _variationContextAccessor = variationContextAccessor;
            _serviceContext = context;
            _publishedContentQuery = publishedContentQuery;
            _searchService = searchService;
        }
        public override IActionResult Index()
        {

            // you are in control here!
            // create our ViewModel based on the PublishedContent of the current request:
            // set our custom properties
            SearchViewModel searchPageViewModel = new SearchViewModel(CurrentPage,
                new PublishedValueFallback(_serviceContext, _variationContextAccessor))
            {
                //do the search
                query = "",
                searchIn = "",
                TotalResults = 0,
                RawResult = null,
                Forums = GetForumsAllowingPosts().ToDictionary(x => x.Id, x => x.Name)
            };

            
            // return our custom ViewModel
            return CurrentTemplate(searchPageViewModel);

        }

        [HttpGet]
        public IActionResult Index(
            [FromQuery(Name = "page")] int page, 
            [FromQuery(Name = "searchIn")] string searchIn, 
            [FromQuery(Name = "query")] string term, 
            [FromQuery(Name = "searchForum")] string forumid,
            [FromQuery(Name = "searchAuthor")] string author,
            [FromQuery(Name = "searchWhere")] string where, 
            [FromQuery(Name = "searchWhen")] string when, 
            [FromQuery(Name = "searchDate")] string date,
            [FromQuery(Name = "searchPhrase")] string phrase = "any")
        {
            if (String.IsNullOrWhiteSpace(term))
            {
                SearchViewModel pageViewModel = new SearchViewModel(CurrentPage,
                    new PublishedValueFallback(_serviceContext, _variationContextAccessor))
                {
                    //do the search
                    query = "",
                    searchIn = searchIn,
                    searchWhere = where,
                    searchForum = forumid,
                    searchAuthor = author,
                    searchWhen = when,
                    searchDate = date,
                    TotalResults = 0,
                    RawResult = null,
                    Forums = GetForumsAllowingPosts().ToDictionary(x => x.Id, x => x.Name)
                };
                return CurrentTemplate(pageViewModel);
            }

            var textFields = new List<string>();

            switch (searchIn)
            {
                case "Subject":
                    textFields.Add("subject");
                    break;
                case "Message":
                    textFields.Add("message");
                    break;
                case "Both":
                    textFields.Add("message");
                    textFields.Add("subject");
                    break;
                default:
                    textFields.Add("message");
                    searchIn = "Message";
                    break;
            }

            if (page == 0) page = 1;
            int pageIndex = page - 1;
            int pageSize = CurrentPage.Value<int>("intPageSize");

            DateTime? parsedDate = null;
            if (DateTime.TryParseExact(date, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime convDate))
            {
                parsedDate = convDate;
            }

            WhereTopic? searchwhere()
            {
                switch (where)
                {
                    case "open":
                        return WhereTopic.Open;
                    case "closed":
                        return WhereTopic.Closed;
                    case "solved":
                        return WhereTopic.Solved;
                    default:
                        return null;
                }
            }

            var results = _searchService.SearchPosts(
                textFields.ToArray(),
                queryTerms: term,
                phrasetype: PhraseType.Phrase,
                wheretopic: searchwhere(),
                author: author,
                forum: forumid,
                from: when == "after" ? parsedDate : null,
                to: when == "before" ? parsedDate : null,
                page: 1,
                pageSize: 20
            );

            var totalResults = results.Count();
            var pagedResults = results.Skip(pageIndex * pageSize).Take(pageSize);

            SearchViewModel searchPageViewModel = new SearchViewModel(CurrentPage, new PublishedValueFallback(_serviceContext, _variationContextAccessor))
            {
                query = term,
                searchIn = searchIn,
                searchWhere = where,
                searchForum = forumid,
                searchAuthor = author,
                searchWhen = when,
                searchDate = date,
                TotalResults = totalResults,
                RawResult = pagedResults,
                Forums = GetForumsAllowingPosts().ToDictionary(x => x.Id, x => x.Name)
            };
            //then return the custom model:
            return CurrentTemplate(searchPageViewModel);
        }
        private IEnumerable<IPublishedContent> GetForumsAllowingPosts()
        {
            var ids = _searchService.GetForumsAllowingPosts().Select(x => x.NodeId);
            return _publishedContentQuery.Content(ids).WhereNotNull();
        }
    }
}