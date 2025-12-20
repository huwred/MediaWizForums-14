using Examine;
using MediaWiz.Forums.Events;
using MediaWiz.Forums.Indexing;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.Examine;

namespace MediaWiz.Forums.Composers
{
    public class ForumIndexComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.ConfigureOptions<ConfigureForumIndexOptions>();
            builder.Services.AddExamineLuceneIndex<ForumIndex, ConfigurationEnabledDirectoryFactory>("ForumIndex");
            builder.Services.AddSingleton<ForumIndexValueSetBuilder>();

            builder.Services.AddSingleton<IIndexPopulator, ForumIndexPopulator>();

            builder.Services.AddExamineLuceneIndex<ForumMemberIndex, ConfigurationEnabledDirectoryFactory>("ForumMemberIndex");
            builder.Services.ConfigureOptions<ConfigureForumMemberIndexOptions>();
            builder.Services.AddSingleton<ForumMemberIndexValueSetBuilder>();
            builder.Services.AddSingleton<IIndexPopulator, ForumMemberIndexPopulator>();
            builder.AddNotificationHandler<MemberCacheRefresherNotification, ForumMemberIndexingNotificationHandler>();

        }
    }
}
