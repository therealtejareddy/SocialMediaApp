using SocialMediaApp.Areas.Identity.Data;

namespace SocialMediaApp.Models
{
    public class PostDto
    {
        public string PostId { get; set; }
        public string userId { get; set;}
        public string Body { get; set; }
        public DateTime? createdAt { get; set; }
        public string Image { get; set; }
        public SocialMediaAppUser user { get; set; }
        public int likesCount { get; set; }
        public bool liked { get; set; }

        public List<CommentsModel> comments { get; set; }
    }
}
