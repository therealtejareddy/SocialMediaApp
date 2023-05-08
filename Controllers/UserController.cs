using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Areas.Identity.Data;
using SocialMediaApp.Data;
using SocialMediaApp.Models;
using System.Dynamic;
using System.Xml.Linq;

namespace SocialMediaApp.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        UserManager<SocialMediaAppUser> _userManager;
        ApplicationDbContext _context;
        AmazonS3Client client;
        public UserController(ApplicationDbContext context, UserManager<SocialMediaAppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            client = new AmazonS3Client("AKIA4FGG5GBJPYTJUBO5", "pZDs8WIkdcvrFabrkVG9ZzkZzbm3RfmVBj6MOPWd", Amazon.RegionEndpoint.APSouth1);

        }
        public async Task<IActionResult> Index()
        {
            ViewData["userEmail"] = _userManager.GetUserName(this.User);
            ViewData["followers"] = _context.Follows.Where(follow => follow.FollowedUserId == _userManager.GetUserId(this.User)).ToList().Count();
            ViewData["following"] = _context.Follows.Where(follow => follow.FollowingUserId == _userManager.GetUserId(this.User)).ToList().Count();
            string currentUserId = _userManager.GetUserId(this.User);
            List<PostsModel> posts = _context.Posts.Where(post => post.userId== currentUserId).ToList();
            var request = new ListObjectsV2Request()
            {
                BucketName = "socialmediaappbucker",
            };
            var response = await client.ListObjectsV2Async(request);
            List<string> imgURLs = response.S3Objects.Select(obj =>
            {
                var req = new GetPreSignedUrlRequest()
                {
                    BucketName = obj.BucketName,
                    Key = obj.Key,
                    Expires = DateTime.Now.AddMinutes(2)
                };
                return client.GetPreSignedURL(req) + obj.Key;
            }).ToList();
            if (posts.Count > 0)
            {
                for (int i = 0; i < posts.Count; i++)
                {
                    if (posts[i].Image.Length != 0)
                    {
                        //posts[i].Image = imgURLs[imgURLs.IndexOf(posts[i].Image)];
                        posts[i].Image = imgURLs.Find(img => img.EndsWith(posts[i].Image)).Substring(0, imgURLs.Find(img => img.EndsWith(posts[i].Image)).Length - posts[i].Image.Length);
                        //posts[i].Image = imgURLs.First();
                    }
                    else
                    {
                        posts[i].Image = "";
                    }
                }
            }
            List<SocialMediaAppUser> allUsers = _userManager.Users.ToList();
            allUsers.ForEach(eachUser =>
            {
                if (eachUser.profilePicURL != null && eachUser.profilePicURL.Length > 0)
                {
                    var profilePicRequest = new GetPreSignedUrlRequest()
                    {
                        BucketName = "socialmediaappbucker",
                        Key = eachUser.profilePicURL,
                        Expires = DateTime.Now.AddSeconds(50)
                    };
                    eachUser.profilePicURL = client.GetPreSignedURL(profilePicRequest);
                }
            });
            List<PostDto> userPosts = new List<PostDto>();
            foreach (var post in posts)
            {
                userPosts.Add(new PostDto()
                {
                    PostId = post.PostId,
                    userId = post.userId,
                    Body = post.Body,
                    createdAt = post.CreatedAt,
                    Image = post.Image,
                    likesCount = _context.Likes.Where(like => like.PostId == post.PostId).Count(),
                    liked = _context.Likes.Where(like => like.PostId == post.PostId && like.UserId == _userManager.GetUserId(this.User)).ToList().Count == 1,
                    comments = _context.Comments.Where(comment => comment.PostId == post.PostId).ToList()
                });
            }

            dynamic dataModel = new ExpandoObject();
            dataModel.postsData = userPosts;
            dataModel.allUsers = allUsers;
            dataModel.user = _userManager.Users.Where(user => user.Id == _userManager.GetUserId(this.User)).First();
            if (dataModel.user.coverPicURL!=null)
            {
                var coverPicRequest = new GetPreSignedUrlRequest()
                {
                    BucketName = "socialmediaappbucker",
                    Key = dataModel.user.coverPicURL,
                    Expires = DateTime.Now.AddSeconds(50)
                };
                dataModel.user.coverPicURL = client.GetPreSignedURL(coverPicRequest);
            }
            return View(dataModel);
        }

        [Route("GetUser/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetUser(string id)
        {
            ViewData["followers"] = _context.Follows.Where(follow => follow.FollowedUserId == id).ToList().Count();
            ViewData["following"] = _context.Follows.Where(follow => follow.FollowingUserId == id).ToList().Count();
            List<SocialMediaAppUser> allUsers = _userManager.Users.ToList();
            allUsers.ForEach(eachUser =>
            {
                if (eachUser.profilePicURL != null && eachUser.profilePicURL.Length > 0)
                {
                    var profilePicRequest = new GetPreSignedUrlRequest()
                    {
                        BucketName = "socialmediaappbucker",
                        Key = eachUser.profilePicURL,
                        Expires = DateTime.Now.AddSeconds(50)
                    };
                    eachUser.profilePicURL = client.GetPreSignedURL(profilePicRequest);
                }
            });
            var allPosts = _context.Posts.Where(post => post.userId == id).ToList();
            List<PostDto> posts = new List<PostDto>();
            foreach (var post in allPosts)
            {
                posts.Add(new PostDto()
                {
                    PostId = post.PostId,
                    userId = post.userId,
                    Body = post.Body,
                    createdAt = post.CreatedAt,
                    Image = post.Image,
                    likesCount = _context.Likes.Where(like => like.PostId == post.PostId).Count(),
                    liked = _context.Likes.Where(like => like.PostId == post.PostId && like.UserId == _userManager.GetUserId(this.User)).ToList().Count == 1,
                    comments = _context.Comments.Where(comment => comment.PostId == post.PostId).ToList()
                });
            }

            var request = new ListObjectsV2Request()
            {
                BucketName = "socialmediaappbucker",
            };
            var response = await client.ListObjectsV2Async(request);
            List<string> imgURLs = response.S3Objects.Select(obj =>
            {
                var req = new GetPreSignedUrlRequest()
                {
                    BucketName = obj.BucketName,
                    Key = obj.Key,
                    Expires = DateTime.Now.AddSeconds(50)
                };
                return client.GetPreSignedURL(req) + obj.Key;
            }).ToList();
            if (posts.Count > 0)
            {
                for (int i = 0; i < posts.Count; i++)
                {
                    if (posts[i].Image.Length != 0)
                    {
                        //posts[i].Image = imgURLs[imgURLs.IndexOf(posts[i].Image)];
                        posts[i].Image = imgURLs.Find(img => img.EndsWith(posts[i].Image)).Substring(0, imgURLs.Find(img => img.EndsWith(posts[i].Image)).Length - posts[i].Image.Length);
                        //posts[i].Image = imgURLs.First();
                    }
                    else
                    {
                        posts[i].Image = "";
                    }
                }
            }
            var thisUser = allUsers.Where(user => user.Id == id).First();
            dynamic dataModel = new ExpandoObject();
            dataModel.postsData = posts;
            dataModel.user = thisUser;
            string currentUsreId = _userManager.GetUserId(this.User);
            dataModel.followData = _context.Follows.Where(follow => follow.FollowedUserId == currentUsreId).ToList();
            dataModel.allUsers = allUsers;
            return View(dataModel);
        }

        [Route("SearchUser/{searchParam}")]
        [HttpGet]
        public IActionResult GetAll(string searchParam)
        {
            var user = _userManager.Users.Where(user => user.Id==_userManager.GetUserId(this.User)).ToList();
            dynamic dataModel = new ExpandoObject();
            var usersList = _userManager.Users.Where(user => user.UserName.StartsWith(searchParam)).ToList().Except(user);
            string currentUsreId = _userManager.GetUserId(this.User);
            dataModel.followData = _context.Follows.Where(follow => follow.FollowedUserId == currentUsreId).ToList();
            ViewData["count"] = dataModel.followData.Count;
            usersList.ToList().ForEach(user =>
            {
                if(user.profilePicURL != null)
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
            dataModel.users = usersList;
            return View(dataModel);
        }

        [HttpPost]
        public IActionResult Follow() 
        {

            FollowsModel followdata = new FollowsModel();
            followdata.FollowingUserId = Request.Form["user-id"];
            followdata.FollowedUserId = _userManager.GetUserId(this.User);
            followdata.FollowsData = DateTime.Now;
            _context.Follows.Add(followdata);
            _context.SaveChanges();
            return Redirect(Request.Headers["Referer"].ToString());
        }

        [HttpPost]
        public IActionResult UnFollow()
        {

            FollowsModel followdata = new FollowsModel();
            followdata.FollowingUserId = Request.Form["user-id"];
            followdata.FollowedUserId = _userManager.GetUserId(this.User);
            followdata.FollowsData = DateTime.Now;
            _context.Follows.Remove(followdata);
            _context.SaveChanges();
            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}
