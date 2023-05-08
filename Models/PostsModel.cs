using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Models
{
    public class PostsModel
    {
        [Key]
        public string PostId { get; set; }
        public string Image { get; set; }
        public string Body { get; set; }
        public string userId { get; set; }
        public DateTime CreatedAt { get; set; }


    }
}
