using Amazon.S3.Model;
using Amazon.S3.Util;
using Amazon.S3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Areas.Identity.Data;
using SocialMediaApp.Data;
using SocialMediaApp.Models;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Dynamic;

namespace SocialMediaApp.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public UserManager<SocialMediaAppUser> _userManager;
        public ApplicationDbContext _context;
        AmazonS3Client client;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<SocialMediaAppUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            client = new AmazonS3Client("AKIA4FGG5GBJPYTJUBO5", "pZDs8WIkdcvrFabrkVG9ZzkZzbm3RfmVBj6MOPWd", Amazon.RegionEndpoint.APSouth1);
        }
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var followData = _context.Follows.Where(follow => follow.FollowedUserId == _userManager.GetUserId(this.User)).ToList();
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
            List<PostsModel> posts = _context.Posts.ToList();
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
                return client.GetPreSignedURL(req)+obj.Key;
            }).ToList();
            if(posts.Count > 0)
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
            var postUserJoinResult = posts.Join(followData,
                post => post.userId,
                follow => follow.FollowingUserId,
                (post, follow) => new PostDto()
                {
                    PostId = post.PostId,
                    userId = post.userId,
                    Body = post.Body,
                    createdAt = post.CreatedAt,
                    Image = post.Image,
                    user = allUsers.Where(user => user.Id == follow.FollowingUserId).First(),
                    likesCount = _context.Likes.Where(like => like.PostId == post.PostId).Count(),
                    liked = _context.Likes.Where(like => like.PostId==post.PostId && like.UserId ==_userManager.GetUserId(this.User)).ToList().Count==1,
                    comments = _context.Comments.Where(comment => comment.PostId==post.PostId).ToList()
                }).ToList();
            dynamic dataModel = new ExpandoObject();
            dataModel.posts = postUserJoinResult;
            dataModel.allUsers = allUsers;
            return View(dataModel);
        }

        [Authorize]
        [Route("SearchPost/{searchParam}")]
        [HttpGet]
        public IActionResult SearchPost(string searchParam)
        {
            var followData = _context.Follows.Where(follow => follow.FollowedUserId == _userManager.GetUserId(this.User)).ToList();
            List<PostsModel> posts = _context.Posts.Where(post => post.Body.Contains(searchParam)).ToList();
            var allUsers = _userManager.Users.ToList();
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
            posts.ForEach(post =>
            {
                if (post.Image.Length > 0)
                {
                    var imageRequest = new GetPreSignedUrlRequest()
                    {
                        BucketName = "socialmediaappbucker",
                        Key = post.Image,
                        Expires = DateTime.Now.AddMinutes(2)
                    };
                    post.Image = client.GetPreSignedURL(imageRequest);
                }
            });
            var postUserJoinResult = posts.Join(followData,
                post => post.userId,
                follow => follow.FollowingUserId,
                (post, follow) => new PostDto()
                {
                    PostId = post.PostId,
                    userId = post.userId,
                    Body = post.Body,
                    createdAt = post.CreatedAt,
                    Image = post.Image,
                    user = allUsers.Where(user => user.Id == follow.FollowingUserId).First(),
                    likesCount = _context.Likes.Where(like => like.PostId == post.PostId).Count(),
                    liked = _context.Likes.Where(like => like.PostId == post.PostId && like.UserId == _userManager.GetUserId(this.User)).ToList().Count == 1,
                    comments = _context.Comments.Where(comment => comment.PostId == post.PostId).ToList()
                }).ToList();
            dynamic dataModel = new ExpandoObject();
            dataModel.posts = postUserJoinResult;
            dataModel.allUsers = allUsers;
            return View(dataModel);
        }

        [Authorize]
        public IActionResult Search()
        {
            string searchval = Request.Form["search"];
            if(searchval.Length > 0)
            {
                if (searchval.StartsWith("#"))
                {
                    return Redirect($"/SearchPost/{searchval.Substring(1)}");
                }else if (searchval.StartsWith("@"))
                {
                    return Redirect($"/SearchUser/{searchval.Substring(1)}");
                }
            }
            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddPost()
        {
            PostsModel newPost = new PostsModel();
            newPost.PostId = Request.Form["postId"];
            newPost.userId = _userManager.GetUserId(this.User);
            newPost.Body = Request.Form["body"];
            DateTime dateData = DateTime.Now;
            if (Request.Form.Files.Count == 0 && Request.Form["body"].ToString().Length==0)
            {
                return RedirectToAction("Index");

			}
            else if(Request.Form.Files.Count == 0)
            {
                newPost.Image = "";
            }
            else
            {
                var file = Request.Form.Files[0];
                string fname = $"{_userManager.GetUserId(this.User)}/{dateData:yyyyMMddhhmmss}{file.FileName}";
                newPost.Image = fname;
                var objectRequest = new PutObjectRequest()
                {
                    BucketName = "socialmediaappbucker",
                    Key = fname,
                    InputStream = file.OpenReadStream(),

                };
                var objResponse = await client.PutObjectAsync(objectRequest);

            }
            newPost.CreatedAt = dateData;
            _context.Posts.Add(newPost);
            _context.SaveChanges();
            return RedirectToAction("Index");
            //return View(nameof(Index));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> LikePost()
        {
			/*LikesModel likeData = new LikesModel();
            likeData.PostId = Request.Form["postId"];
            likeData.UserId = _userManager.GetUserId(this.User);
            likeData.CreatedAt = DateTime.Now;
            if (_context.Likes.Where(like => like.PostId == likeData.PostId && like.UserId == likeData.UserId).ToList().Count == 1)
            {
                _context.Likes.Remove(likeData);
            }
            else
            {
                _context.Likes.Add(likeData);
            }
            _context.SaveChanges();*/
			//--------
			HttpClient hclient = new HttpClient();
			var values = new Dictionary<string, string>
              {
	              { "postId", Request.Form["postId"] },
	              { "userId", _userManager.GetUserId(this.User) }
              };

			var content = new FormUrlEncodedContent(values);

			var response = await hclient.PostAsync($"https://localhost:7254/api/Api/post/{Request.Form["postId"]}/{_userManager.GetUserId(this.User)}", content);
			var responseString = await response.Content.ReadAsStringAsync();
			//--------
			return Redirect(Request.Headers["Referer"].ToString());
        }

        [Authorize]
        [HttpPost]
        public IActionResult CommentPost()
        {
            CommentsModel comment = new CommentsModel();
            comment.PostId = Request.Form["postId"];
            comment.UserId = _userManager.GetUserId(this.User);
            comment.Text = Request.Form["commentText"];
            comment.CommentId = Guid.NewGuid().ToString();
            _context.Comments.Add(comment);
            _context.SaveChanges();
            return Redirect(Request.Headers["Referer"].ToString());
        }

        [Route("SharePost/{id}")]
        [HttpGet]
        public IActionResult SharePost(string id)
        {
            PostsModel post = _context.Posts.Where(post => post.PostId== id).First();
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
            if(post.Image!=null)
            {
                var postImageRequest = new GetPreSignedUrlRequest()
                {
                    BucketName = "socialmediaappbucker",
                    Key = post.Image,
                    Expires = DateTime.Now.AddSeconds(50)
                };
                post.Image = client.GetPreSignedURL(postImageRequest);
            }
            var postRes = new PostDto()
            {
                PostId = post.PostId,
                userId = post.userId,
                Body = post.Body,
                createdAt = post.CreatedAt,
                Image = post.Image,
                user = allUsers.Where(user => user.Id == post.userId).First(),
                likesCount = _context.Likes.Where(like => like.PostId == post.PostId).Count(),
                liked = _context.Likes.Where(like => like.PostId == post.PostId && like.UserId == _userManager.GetUserId(this.User)).ToList().Count == 1,
                comments = _context.Comments.Where(comment => comment.PostId == post.PostId).ToList()
            };
            dynamic dataModel = new ExpandoObject();
            dataModel.post = postRes;
            dataModel.allUsers = allUsers;
            return View(dataModel);
            //return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}


/*
                var bucketExist = await AmazonS3Util.DoesS3BucketExistV2Async(client, "socialmedia-s3");
                if (!bucketExist)
                {
                    var bucketRequest = new PutBucketRequest()
                    {
                        BucketName = "socialmediaappbucker",
                        UseClientRegion = true,
                    };
                    await client.PutBucketAsync(bucketRequest);
                }
 */