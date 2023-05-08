using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Models;

namespace SocialMediaApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FollowsModel>().HasKey(table => new {
                table.FollowingUserId,
                table.FollowedUserId
            });

            modelBuilder.Entity<LikesModel>().HasKey(table => new {
                table.PostId,
                table.UserId
            });

        }
        public DbSet<PostsModel> Posts { get; set; }
        public DbSet<CommentsModel> Comments { get; set; }
        public DbSet<LikesModel> Likes { get; set; }
        public DbSet<FollowsModel> Follows { get; set; }
        
    }
}
