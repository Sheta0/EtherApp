using EtherApp.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EtherApp.Controllers
{
    [Authorize]
    public class FavoritesController : Controller
    {
        private readonly IPostsService _postsService;

        public FavoritesController(IPostsService postsService)
        {
            _postsService = postsService;
        }

        public async Task<IActionResult> Index()
        {
            var loggedInUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var myFavoritePosts = await _postsService.GetAllFavoritedPostsAsync(int.Parse(loggedInUser));

            return View(myFavoritePosts);
        }
    }
}
