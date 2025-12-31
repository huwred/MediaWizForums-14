using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace MediaWiz.Forums.Events
{

    public class ForumPostBeforeSaveNotification : INotification
    {
        public IContent Post { get; }
        public ForumPostBeforeSaveNotification(IContent post)
        {
            Post = post;
        }

    }
    public class ForumPostAfterSaveNotification : INotification
    {
        public IContent Post { get; }
        public OperationResult SaveResult { get; }

        public ForumPostAfterSaveNotification(IContent post, OperationResult saveresult)
        {
            Post = post;
            SaveResult = saveresult;
        }
    }
}