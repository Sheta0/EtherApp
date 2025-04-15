using EtherApp.Controllers.Base;
using EtherApp.Data.Models;
using EtherApp.Data.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EtherApp.Controllers
{
    public class UserController(IUserService userService) : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Details(int userId)
        {
            var loggedInUserId = GetUserId();

            
            if (!loggedInUserId.HasValue)
            {
                return BadRequest("Logged in user ID is required.");
            }

            var userPosts = await userService.GetUserPosts(userId, loggedInUserId.Value);

            return View(userPosts);
        }
    }
}
