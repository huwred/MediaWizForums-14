using Examine.Lucene;
using global::Umbraco.Cms.Core.Services;
using global::Umbraco.Cms.Infrastructure.Examine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace MediaWiz.Forums.Indexing
{

    public class ForumMemberIndex : UmbracoExamineIndex
    {
        public ForumMemberIndex(
            ILoggerFactory loggerFactory,
            string name,
            IOptionsMonitor<LuceneDirectoryIndexOptions> indexOptions,
            IHostingEnvironment hostingEnvironment,
            IRuntimeState runtimeState)
            : base(loggerFactory,
            name,
            indexOptions,
            hostingEnvironment,
            runtimeState)
        {
        }
    }
}
