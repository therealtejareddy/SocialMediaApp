using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Models
{
    public class LikesModel
    {
        public string? PostId { get; set; }
        public string? UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
