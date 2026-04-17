namespace MediaWiz.Forums.Models
{
    public class ForgotPasswordViewModel
    {
        public ForumForgotPasswordModel Model { get; set; }
        public string RequestSentTitle { get; set; }
        public string RequestSentMessage { get; set; }
        public string SendButtonText { get; set; }
    }
}