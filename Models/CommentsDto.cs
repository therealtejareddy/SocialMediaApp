using SocialMediaApp.Areas.Identity.Data;

namespace SocialMediaApp.Models
{
    public class CommentsDto
    {
        public string CommentId { get; set; }
        public string Text { get; set; }
        public string UserId { get; set; }
        public SocialMediaAppUser User { get; set; }
        public string PostId { get; set; }
    }
}
