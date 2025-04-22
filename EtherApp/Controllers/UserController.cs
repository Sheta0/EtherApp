using EtherApp.Controllers.Base;
using EtherApp.Data.Models;
using EtherApp.Data.Services;
using EtherApp.ViewModels.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EtherApp.Controllers
{
    public class UserController(IUserService userService) : BaseController
    {
        public async Task<IActionResult> Details(int userId)
        {
            var loggedInUserId = GetUserId();

            if (!loggedInUserId.HasValue)
            {
                return BadRequest("Logged in user ID is required.");
            }

            var user = await userService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var userPosts = await userService.GetUserPosts(userId, loggedInUserId.Value);

            var viewModel = new UserDetailsVM
            {
                User = user,
                Posts = userPosts
            };

            return View(viewModel);
        }
    }
}
