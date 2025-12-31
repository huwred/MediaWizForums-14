using MediaWiz.Forums.Extensions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MediaWiz.Forums.Models
{
    public class ForumsPostModel
    {
        public int Id { get; set; }
        public int ParentId { get; set; }

        [DisplayName("Title")]
        [RequiredIf(nameof(IsTopic), true, ErrorMessage = "Title is required for a Topic.")]
        public string Title { get; set; }

        [Required]
        [DisplayName("Message")]
        public string Body { get; set; }

        [Required] public int AuthorId { get; set; }

        public bool IsTopic { get; set; }

        public string returnPath { get; set; }
    }

    public class ForumsForumModel
    {
        public int Id { get; set; }
        public int ParentId { get; set; }

        [Required]
        [DisplayName("Title")] 
        public string Title { get; set; }

        [Required]
        [DisplayName("Introduction")]
        public string Introduction { get; set; }

        [DisplayName("Allow Posts")]
        public bool AllowPosts { get; set; }
        [DisplayName("Allow Images")]
        public bool AllowImages { get; set; }
        [DisplayName("Require Approval for Posts")]
        public bool RequireApproval { get; set; }
    }
}

