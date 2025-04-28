using EtherApp.Controllers.Base;
using EtherApp.Data.Helpers.Enums;
using EtherApp.Data.Models;
using EtherApp.Data.Services;
using EtherApp.ViewModels.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtherApp.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPostsService _postsService;
        private readonly IHashtagsService _hashtagsService;
        private readonly IFilesService _filesService;

        public HomeController(ILogger<HomeController> logger, IPostsService postsService, IHashtagsService hashtagsService, IFilesService filesService)
        {
            _logger = logger;
            _postsService = postsService;
            _hashtagsService = hashtagsService;
            _filesService = filesService;
        }

        public async Task<IActionResult> Index()
        {
            var loggedInUser = GetUserId();
            if(loggedInUser is null) return RedirectToLogin();
            var allPosts = await _postsService.GetAllPostsAsync(loggedInUser.Value);
            return View(allPosts);
        }

        public async Task<IActionResult> PostDetails(int postId)
        {
            var post = await _postsService.GetPostByIdAsync(postId);
            return View(post);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost(PostVM post)
        {
            var loggedInUser = GetUserId();
            if (loggedInUser is null) return RedirectToLogin();

            post.Content ??= string.Empty;

            var imageUploadPath = await _filesService.UploadImageAsync(post.Image, ImageFileType.PostImage);

            if (string.IsNullOrWhiteSpace(post.Content) && string.IsNullOrEmpty(imageUploadPath))
            {
                TempData["ErrorMessage"] = "Please provide either text content or an image for your post.";
                return RedirectToAction("Index");
            }

            var newPost = new Post
            {
                Content = post.Content,
                DateCreated = DateTime.Now,
                DateUpdated = DateTime.Now,
                ImageUrl = imageUploadPath,
                NrOfReports = 0,
                UserId = loggedInUser.Value
            };

            await _postsService.CreatePostAsync(newPost);

            // Only process hashtags if content is not empty
            if (!string.IsNullOrWhiteSpace(post.Content))
            {
                await _hashtagsService.ProcessHashtagsForNewPostAsync(post.Content, loggedInUser.Value);
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePostLike(PostLikeVM postLikeVM)
        {
            var loggedInUser = GetUserId();
            if (loggedInUser is null) return RedirectToLogin();

            await _postsService.TogglePostLikeAsync(postLikeVM.PostId, loggedInUser.Value);
            
            var post = await _postsService.GetPostByIdAsync(postLikeVM.PostId);

            return PartialView("Home/_Post", post);
        }

        [HttpPost]
        public async Task<IActionResult> TogglePostFavorite(PostFavoriteVM postFavoriteVM)
        {
            var loggedInUser = GetUserId();
            if (loggedInUser is null) return RedirectToLogin();

            await _postsService.TogglePostFavoriteAsync(postFavoriteVM.PostId, loggedInUser.Value);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var post = await _postsService.GetPostByIdAsync(postFavoriteVM.PostId);
                return PartialView("Home/_Post", post);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> TogglePostVisibility(PostVisibilityVM postVisibilityVM)
        {
            var loggedInUser = GetUserId();
            if (loggedInUser is null) return RedirectToLogin();

            await _postsService.TogglePostVisibilityAsync(postVisibilityVM.PostId, loggedInUser.Value);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddPostComment(PostCommentVM postCommentVM)
        {
            var loggedInUser = GetUserId();
            if (loggedInUser is null) return RedirectToLogin();

            var newComment = new Comment()
            {
                UserId = loggedInUser.Value,
                PostId = postCommentVM.PostId,
                Content = postCommentVM.Content,
                DateCreated = DateTime.Now,
                DateUpdated = DateTime.Now,
            };

            await _postsService.AddPostCommontAsync(newComment);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var post = await _postsService.GetPostByIdAsync(postCommentVM.PostId);
                return PartialView("Home/_Post", post);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddPostReport(PostReportVM postReportVM)
        {
            var loggedInUser = GetUserId();
            if (loggedInUser is null) return RedirectToLogin();

            await _postsService.ReportPostAsync(postReportVM.PostId, loggedInUser.Value);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemovePostComment(RemoveCommentVM removeCommentVM)
        {
            await _postsService.DeletePostCommentAsync(removeCommentVM.CommentId);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> PostDelete(PostDeleteVM postDeleteVM)
        {

            var postRemoved = await _postsService.DeletePostAsync(postDeleteVM.PostId);
            await _hashtagsService.ProcessHashtagsForRemovedPostAsync(postRemoved.Content, postRemoved.UserId);


            return RedirectToAction("Index");
        }

    }
}