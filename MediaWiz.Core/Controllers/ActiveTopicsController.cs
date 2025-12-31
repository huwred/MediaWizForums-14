using Examine;
using Examine.Search;
using MediaWiz.Forums.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
    public class ActiveTopicsController : RenderController
    {
        private readonly IPublishedContentQuery _publishedContentQuery;
        private readonly IExamineManager _examineManager;
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly ServiceContext _serviceContext;

        public ActiveTopicsController(ILogger<ActiveTopicsController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor, IVariationContextAccessor variationContextAccessor, ServiceContext context, IPublishedContentQuery publishedContentQuery, IExamineManager examineManager)
            : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            _variationContextAccessor = variationContextAccessor;
            _serviceContext = context;
            _publishedContentQuery = publishedContentQuery;
            _examineManager = examineManager;

        }

        /// <summary>
        /// Added date range query
        /// </summary>
        [HttpGet]
        public IActionResult Index(
            [FromQuery(Name = "page")] int page, 
            [FromQuery(Name = "TopicsSince")] string query, 
            [FromQuery(Name = "showonly")] string filter)
        {
            ISearchResults results = null;
            var today = DateTime.Now;
            long min = today.Ticks;
            long max = today.Ticks;
            query ??= "7d";

            switch (query)
            {
                case "30m" :
                    min = today.AddMinutes(-30).Ticks;
                    break;
                case "60m" :
                    min = today.AddHours(-1).Ticks;
                    break;
                case "1d" :
                    min = today.AddDays(-1).Ticks;
                    break;
                case "7d" :
                    min = today.AddDays(-7).Ticks;
                    break;
                case "1m" :
                    min = today.AddMonths(-1).Ticks;
                    break;
                case "1y" :
                    min = today.AddYears(-1).Ticks;
                    break;
            }

            int pageIndex = page - 1;
            if(pageIndex < 0) {pageIndex = 0;}
            int pageSize = CurrentPage.Value<int>("intPageSize");
            if(filter != null)
            {

                if (_examineManager.TryGetIndex("ForumIndex", out var index))
                {
                    var searcher = index.Searcher;

                    var examineQuery = searcher.CreateQuery(IndexTypes.Content)
                    .Field("__NodeTypeAlias", "forumPost")
                    .And().Field("postType", "Topic");

                    if (filter == "noreply")
                    {
                        examineQuery = examineQuery.And().RangeQuery<int>(new string[] { "replies" }, -1, 0);
                    }
                    if (filter == "unsolved")
                    {
                        examineQuery = examineQuery.And().Field("answered", "0");
                    }
                    if (filter == "solved")
                    {
                        examineQuery = examineQuery.And().Field("answered", "1");
                    }
                    results = examineQuery.OrderByDescending(new SortableField[] { new SortableField("lastTicks") }).Execute();
                }
            }
            else
            {
                if (_examineManager.TryGetIndex("ForumIndex", out var index))
                {
                    var searcher = index.Searcher;

                    var examineQuery = searcher.CreateQuery(IndexTypes.Content)
                    .Field("__NodeTypeAlias", "forumPost")
                    .And().Field("postType", "Topic")
                        .And().RangeQuery<long>(new string[] { "lastTicks" }, min, max)
                        .OrderByDescending(new SortableField[] { new SortableField("lastTicks") });
                
                    results = examineQuery.Execute();
                }
            }


            if (results != null)
            {
                var pagedResults = results.Skip(pageIndex * pageSize).Take(pageSize);
                var totalResults = results.TotalItemCount;
                var ids = pagedResults.Select(x => x.Id);
                var pagedResultsAsContent = _publishedContentQuery.Content(ids);

                SearchViewModel searchPageViewModel = new SearchViewModel(CurrentPage, new PublishedValueFallback(_serviceContext, _variationContextAccessor))
                {
                    //do the search
                    query = query?.ToString(),
                    searchIn = "",
                    TotalResults = totalResults,
                    PagedResult = pagedResultsAsContent,
                    Forums = GetForumsAllowingPosts().ToDictionary(x => x.Id, x => x.Name)
                };
                //then return the custom model:
                return CurrentTemplate(searchPageViewModel);
            }
            return CurrentTemplate(new SearchViewModel(CurrentPage, new PublishedValueFallback(_serviceContext, _variationContextAccessor)));
        }
        private IEnumerable<IPublishedContent> GetForumsAllowingPosts()
        {
            if (_examineManager.TryGetIndex("ExternalIndex", out var externalIndex) == false)
            {
                return Enumerable.Empty<IPublishedContent>();
            }

            var searcher = externalIndex.Searcher;

            // Booleans may be indexed as "1" or "true" depending on data type/converter,
            // so we match both and both possible field casings.
            var query = searcher.CreateQuery(IndexTypes.Content)
                .GroupedOr(new[] { "__NodeTypeAlias" }, new[] { "forum" })
                .And()
                .GroupedOr(new[] { "isActive", "isActive" }, new[] { "1", "true" })
                .And()
                .GroupedOr(new[] { "postAtRoot", "postAtRoot" }, new[] { "1", "true" });

            var results = query.Execute();

            var ids = results.Select(x => x.Id);
            return _publishedContentQuery.Content(ids).WhereNotNull();
        }
        public IActionResult Sort([FromQuery(Name = "page")] int page, [FromQuery(Name = "query")] string query)
        {
            ISearchResults results = null;
            if (string.IsNullOrWhiteSpace(query))
            {
                query = "updated";
            }

            int pageIndex = page - 1;
            int pageSize = CurrentPage.Value<int>("intPageSize");

            if (_examineManager.TryGetIndex("ForumIndex", out var index))
            {
                var searcher = index.Searcher;
                var test = searcher.Search("* AND -postType");
                var examineQuery = searcher.CreateQuery(IndexTypes.Content)
                    .Field("postType", "Topic")
                    .OrderByDescending(new SortableField[] { new SortableField("lastTicks") });
                    //.Execute(/*maxResults: pageSize*(pageIndex + 1)*/);

                results = examineQuery.Execute();
            }

            if (results != null)
            {
                var pagedResults = results.Skip(pageIndex * pageSize).Take(pageSize);
                var totalResults = results.TotalItemCount;
                var pagedResultsAsContent = _publishedContentQuery.Content(pagedResults.Select(x => x.Id));

                SearchViewModel searchPageViewModel = new SearchViewModel(CurrentPage, new PublishedValueFallback(_serviceContext, _variationContextAccessor))
                {
                    //do the search
                    query = query,
                    searchIn = "",
                    TotalResults = totalResults,
                    PagedResult = pagedResultsAsContent
                };
                //then return the custom model:
                return CurrentTemplate(searchPageViewModel);
            }
            return CurrentTemplate(new SearchViewModel(CurrentPage, new PublishedValueFallback(_serviceContext, _variationContextAccessor)));
        }

    }
}