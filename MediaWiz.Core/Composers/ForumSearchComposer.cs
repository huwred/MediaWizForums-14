using Examine;
using Examine.Lucene.Providers;
using Microsoft.Extensions.DependencyInjection;
using System;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;



namespace MediaWiz.Forums.Composers
{
    public class ForumSearchComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddSingleton<ISynonymProvider, InMemorySynonymProvider>();

            builder.Services.AddSingleton<ForumSearchService>(sp =>
            {
                var examine = sp.GetRequiredService<IExamineManager>();

                if (!examine.TryGetIndex("ForumIndex", out var index))
                    throw new InvalidOperationException("ForumIndex not found in Examine");

                var searcher = (BaseLuceneSearcher)index.Searcher;

                return new ForumSearchService(
                    searcher,
                    sp.GetRequiredService<ISynonymProvider>(),
                    debugMode: true
                );
            });
        }
    }
}