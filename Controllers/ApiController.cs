using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Data;
using SocialMediaApp.Models;

namespace SocialMediaApp.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ApiController : ControllerBase
	{
		public ApplicationDbContext _context;
		public ApiController(ApplicationDbContext context) 
		{
			_context = context;
			_context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
		}

		[HttpPost("post/{postId}/{userId}")]
		public string Post(string postId, string userId)
		{
			LikesModel likeData = new LikesModel();
			likeData.PostId = postId;
			likeData.UserId = userId;
			likeData.CreatedAt = DateTime.Now;
			if (_context.Likes.Where(like => like.PostId == likeData.PostId && like.UserId == likeData.UserId).ToList().Count == 1)
			{
				_context.Likes.Remove(likeData);
			}
			else
			{
				_context.Likes.Add(likeData);
			}
			_context.SaveChanges();
			return "Ok";
		} 
	}
}
