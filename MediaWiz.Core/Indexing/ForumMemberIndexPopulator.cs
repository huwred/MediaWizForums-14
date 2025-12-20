using Examine;
using global::Umbraco.Cms.Core.Services;
using global::Umbraco.Cms.Infrastructure.Examine;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Models;

namespace MediaWiz.Forums.Indexing
{
    public class ForumMemberIndexPopulator : IndexPopulator
    {
        private readonly IMemberService _contentService;
        private readonly ForumMemberIndexValueSetBuilder _forumMemberIndexValueSetBuilder;

        public ForumMemberIndexPopulator(IMemberService contentService, ForumMemberIndexValueSetBuilder forumMemberIndexValueSetBuilder)
        {
            _contentService = contentService;
            _forumMemberIndexValueSetBuilder = forumMemberIndexValueSetBuilder;
            RegisterIndex("ForumMemberIndex");
        }

        protected override void PopulateIndexes(IReadOnlyList<IIndex> indexes)
        {
            foreach (IIndex index in indexes)
            {
                IMember[] roots = _contentService.GetAllMembers().ToArray();
                index.IndexItems(_forumMemberIndexValueSetBuilder.GetValueSets(roots));

                //foreach (IMember root in roots)
                //{
                //    const int pageSize = 10000;
                //    var pageIndex = 0;
                //    IMember[] descendants;
                //    do
                //    {
                //        descendants = _contentService.GetPagedDescendants(root.Id, pageIndex, pageSize, out _).ToArray();
                //        IEnumerable<ValueSet> valueSets = _forumMemberIndexValueSetBuilder.GetValueSets(descendants);
                //        index.IndexItems(valueSets);

                //        pageIndex++;
                //    }
                //    while (descendants.Length == pageSize);
                //}
            }
        }
    }
}
