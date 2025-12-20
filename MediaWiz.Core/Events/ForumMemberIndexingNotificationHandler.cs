using Examine;
using MediaWiz.Forums.Indexing;
using System;
using System.Collections.Generic;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure;
using Umbraco.Cms.Infrastructure.Search;
using Umbraco.Extensions;

namespace MediaWiz.Forums.Events;

public class ForumMemberIndexingNotificationHandler : INotificationHandler<MemberCacheRefresherNotification>
{
    private readonly IRuntimeState _runtimeState;
    private readonly IUmbracoIndexingHandler _umbracoIndexingHandler;
    private readonly IExamineManager _examineManager;
    private readonly IMemberService _memberService;
    private readonly ForumMemberIndexValueSetBuilder _forumMemberIndexValueSetBuilder;

    public ForumMemberIndexingNotificationHandler(
        IRuntimeState runtimeState,
        IUmbracoIndexingHandler umbracoIndexingHandler,
        IExamineManager examineManager,
        IMemberService memberService,
        ForumMemberIndexValueSetBuilder forumMemberIndexValueSetBuilder)
    {
        _runtimeState = runtimeState;
        _umbracoIndexingHandler = umbracoIndexingHandler;
        _examineManager = examineManager;
        _memberService = memberService;
        _forumMemberIndexValueSetBuilder = forumMemberIndexValueSetBuilder;
    }

    private bool NotificationHandlingIsDisabled()
    {
        // Only handle events when the site is running.
        if (_runtimeState.Level != RuntimeLevel.Run)
        {
            return true;
        }

        if (_umbracoIndexingHandler.Enabled == false)
        {
            return true;
        }

        if (Suspendable.ExamineEvents.CanIndex == false)
        {
            return true;
        }

        return false;
    }

    private MemberCacheRefresher.JsonPayload[] GetNotificationPayloads(MemberCacheRefresherNotification notification)
    {
        if (notification.MessageType != MessageType.RefreshByPayload ||
            notification.MessageObject is not MemberCacheRefresher.JsonPayload[] payloads)
        {
            throw new NotSupportedException();
        }

        return payloads;
    }

    public void Handle(MemberCacheRefresherNotification notification)
    {
        if (NotificationHandlingIsDisabled())
        {
            return;
        }

        if (!_examineManager.TryGetIndex("ForumMemberIndex", out IIndex index))
        {
            throw new InvalidOperationException("Could not obtain the Forum Member index");
        }

        MemberCacheRefresher.JsonPayload[] payloads = GetNotificationPayloads(notification);

        foreach (MemberCacheRefresher.JsonPayload payload in payloads)
        {
            // Remove
            if (payload.Removed)
            {
                index.DeleteFromIndex(payload.Id.ToString());
            }
            // Reindex
            else 
            {
                IMember content = _memberService.GetById(payload.Id);
                if (content == null || content.Trashed)
                {
                    index.DeleteFromIndex(payload.Id.ToString());
                    continue;
                }

                IEnumerable<ValueSet> valueSets = _forumMemberIndexValueSetBuilder.GetValueSets(content);
                index.IndexItems(valueSets);
            }
        }
    }
}
