using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Models
{
    public class CommentsModel
    {
        [Key]
        public string CommentId { get; set; }
        public string Text { get; set; }
        public string UserId { get; set; }
        public string PostId { get; set; }
    }
}
