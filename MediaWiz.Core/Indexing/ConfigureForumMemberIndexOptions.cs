using Examine;
using Examine.Lucene;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Util;
using Microsoft.Extensions.Options;
using System;
using Umbraco.Cms.Core.Configuration.Models;

namespace MediaWiz.Forums.Indexing
{
    public class ConfigureForumMemberIndexOptions : IConfigureNamedOptions<LuceneDirectoryIndexOptions>
    {
        private readonly IOptions<IndexCreatorSettings> _settings;

        public ConfigureForumMemberIndexOptions(IOptions<IndexCreatorSettings> settings)
            => _settings = settings;

        public void Configure(string name, LuceneDirectoryIndexOptions options)
        {
            if (name?.Equals("ForumMemberIndex") is false)
            {
                return;
            }

            options.Analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48);

            options.FieldDefinitions = new(
                new("id", FieldDefinitionTypes.Integer),
                new("name", FieldDefinitionTypes.FullText),
                new("joinedDate", FieldDefinitionTypes.DateTime),
                new("postCount", FieldDefinitionTypes.Integer),
                new("resetGuid", FieldDefinitionTypes.FullText),
                //new("lastPost", FieldDefinitionTypes.DateTime),
                new("lastLogin", FieldDefinitionTypes.DateTime),
                new("hasVerifiedAccount", FieldDefinitionTypes.Integer)
            );

            options.UnlockIndex = true;

            if (_settings.Value.LuceneDirectoryFactory == LuceneDirectoryFactory.SyncedTempFileSystemDirectoryFactory)
            {
                // if this directory factory is enabled then a snapshot deletion policy is required
                options.IndexDeletionPolicy = new SnapshotDeletionPolicy(new KeepOnlyLastCommitDeletionPolicy());
            }
        }

        // not used
        public void Configure(LuceneDirectoryIndexOptions options) => throw new NotImplementedException();
    }
}
