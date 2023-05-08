using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Areas.Identity.Data;
using SocialMediaApp.Data;
using SocialMediaApp.Models;
using System.Drawing.Drawing2D;
using System.Dynamic;

namespace SocialMediaApp.Controllers
{
    [Authorize]
    public class MyController : Controller
    {
        UserManager<SocialMediaAppUser> _userManager;
        ApplicationDbContext _context;
        AmazonS3Client client;
        public MyController(ApplicationDbContext context, UserManager<SocialMediaAppUser> userManager) 
        {
            _context = context;
            _userManager = userManager;
            client = new AmazonS3Client("AKIA4FGG5GBJPYTJUBO5", "pZDs8WIkdcvrFabrkVG9ZzkZzbm3RfmVBj6MOPWd", Amazon.RegionEndpoint.APSouth1);
        }
        public IActionResult Index()
        {
            ViewData["userEmail"] = _userManager.GetUserName(this.User);
            return View();
        }

        public IActionResult ProfileEdit()
        {
            SocialMediaAppUser data = _userManager.Users.Where(user => user.Id ==_userManager.GetUserId(this.User)).First();
            if (data.profilePicURL != null)
            {
                var profilePicRequest = new GetPreSignedUrlRequest()
                {
                    BucketName = "socialmediaappbucker",
                    Key = data.profilePicURL,
                    Expires = DateTime.Now.AddSeconds(50)
                };
                data.profilePicURL = client.GetPreSignedURL(profilePicRequest);
            }
            if (data.coverPicURL != null)
            {
                var coverPicRequest = new GetPreSignedUrlRequest()
                {
                    BucketName = "socialmediaappbucker",
                    Key = data.coverPicURL,
                    Expires = DateTime.Now.AddSeconds(50)
                };
                data.coverPicURL = client.GetPreSignedURL(coverPicRequest);
            }
            return View(data);
        }
        [HttpPost]
        public async Task<IActionResult> EditProfile()
        {
            SocialMediaAppUser userdata = _userManager.Users.Where(user => user.Id == _userManager.GetUserId(this.User)).First();
            userdata.Bio = Request.Form["about"];
            userdata.fullName = Request.Form["fullName"];
            userdata.Address = Request.Form["address"];
            userdata.Country = Request.Form["country"];
            userdata.Education = Request.Form["education"];
            userdata.Work = Request.Form["work"];
            await _userManager.UpdateAsync(userdata);
            return RedirectToAction("Index","User");
        }

        [HttpPost]
        public async Task<IActionResult> UploadProfilePic()
        {
            var file = Request.Form.Files[0];
            string fname = $"pic/profilePic/{_userManager.GetUserId(this.User)}/{DateTime.Now:yyyyMMddhhmmss}{file.FileName}";
            var user = _userManager.Users.Where(user => user.Id == _userManager.GetUserId(this.User)).First();
            user.profilePicURL = fname;
            var objectRequest = new PutObjectRequest()
            {
                BucketName = "socialmediaappbucker",
                Key = fname,
                InputStream = file.OpenReadStream(),

            };
            var objResponse = await client.PutObjectAsync(objectRequest);
            await _userManager.UpdateAsync(user);
            return RedirectToAction("Index","User");
        }

        [HttpPost]
        public async Task<IActionResult> UploadCoverPic()
        {
            var file = Request.Form.Files[0];
            string fname = $"pic/coverPic/{_userManager.GetUserId(this.User)}/{DateTime.Now:yyyyMMddhhmmss}{file.FileName}";
            var user = _userManager.Users.Where(user => user.Id == _userManager.GetUserId(this.User)).First();
            user.coverPicURL = fname;
            var objectRequest = new PutObjectRequest()
            {
                BucketName = "socialmediaappbucker",
                Key = fname,
                InputStream = file.OpenReadStream(),

            };
            var objResponse = await client.PutObjectAsync(objectRequest);
            await _userManager.UpdateAsync(user);
            return RedirectToAction("Index", "User");
        }

        // Followers
        public IActionResult Following() {
            var followingData = _context.Follows.Where(follow => follow.FollowedUserId == _userManager.GetUserId(this.User)).ToList();
            var allUsers = _userManager.Users.ToList();
            var result = allUsers.Join(followingData,
                user => user.Id,
                follow => follow.FollowingUserId,
                (user, follow) =>
                new UserDto() {
                    Id = user.Id,
                    profilePicURL = user.profilePicURL,
                    coverPicURL = user.coverPicURL,
                    uname = user.uname,
                    fullName = user.fullName
                }).ToList();
            result.ForEach(user =>
            {
                if (user.profilePicURL != null)
                {
                    var profilePicRequest = new GetPreSignedUrlRequest()
                    {
                        BucketName = "socialmediaappbucker",
                        Key = user.profilePicURL,
                        Expires = DateTime.Now.AddSeconds(50)
                    };
                    user.profilePicURL = client.GetPreSignedURL(profilePicRequest);
                }
                if (user.coverPicURL != null)
                {
                    var profilePicRequest = new GetPreSignedUrlRequest()
                    {
                        BucketName = "socialmediaappbucker",
                        Key = user.coverPicURL,
                        Expires = DateTime.Now.AddSeconds(50)
                    };
                    user.coverPicURL = client.GetPreSignedURL(profilePicRequest);
                }
            });
            return View(result);
        }
        //Following
        public IActionResult Followers()
        {
			/*            var followersData = _context.Follows.Where(follow => follow.FollowingUserId == _userManager.GetUserId(this.User)).ToList();
						return View(followersData);*/
			var user = _userManager.Users.Where(user => user.Id == _userManager.GetUserId(this.User)).ToList();
			dynamic dataModel = new ExpandoObject();
			var allUsers = _userManager.Users.ToList().Except(user);
			string currentUsreId = _userManager.GetUserId(this.User);
			var followData = _context.Follows.Where(follow => follow.FollowingUserId == currentUsreId).ToList();
			ViewData["count"] = followData.Count;
            ViewData["currentUserId"] = currentUsreId;
			var usersList = allUsers.Join(followData,
				user => user.Id,
				follow => follow.FollowedUserId,
				(user, follow) =>
				new UserDto()
				{
					Id = user.Id,
					profilePicURL = user.profilePicURL,
					coverPicURL = user.coverPicURL,
					uname = user.uname,
					fullName = user.fullName
				}).ToList();
			usersList.ToList().ForEach(user =>
			{
				if (user.profilePicURL != null)
				{
					var profilePicRequest = new GetPreSignedUrlRequest()
					{
						BucketName = "socialmediaappbucker",
						Key = user.profilePicURL,
						Expires = DateTime.Now.AddSeconds(50)
					};
					user.profilePicURL = client.GetPreSignedURL(profilePicRequest);
				}
				if (user.coverPicURL != null)
				{
					var profilePicRequest = new GetPreSignedUrlRequest()
					{
						BucketName = "socialmediaappbucker",
						Key = user.coverPicURL,
						Expires = DateTime.Now.AddSeconds(50)
					};
					user.coverPicURL = client.GetPreSignedURL(profilePicRequest);
				}
			});
            dataModel.followData = followData;
			dataModel.users = usersList;
            dataModel.user = user[0];
			return View(dataModel);
		}
    }
}
