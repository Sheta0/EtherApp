using EtherApp.Controllers.Base;
using EtherApp.Data.Helpers.Constants;
using EtherApp.Data.Models;
using EtherApp.Data.Services;
using EtherApp.ViewModels.Friends;
using Microsoft.AspNetCore.Mvc;

namespace EtherApp.Controllers
{
    public class FriendsController(IFriendsService friendsService) : BaseController
    {
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return RedirectToLogin();

            var friendsData = new FriendshipVM()
            {
                FriendRequests = await friendsService.GetSentFriendRequestsAsync(userId.Value),
                ReceivedRequests = await friendsService.GetReceivedFriendRequestsAsync(userId.Value),
                Friends = await friendsService.GetUserFriendsAsync(userId.Value)
            };

            ViewBag.CurrentUserId = userId.Value;
            return View(friendsData);
        }

        [HttpPost]
        public async Task<IActionResult> SendRequest(int receiverId)
        {
            var senderId = GetUserId();
            if (!senderId.HasValue)
                return RedirectToLogin();
            await friendsService.SendRequestAsync(senderId.Value, receiverId);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRequest(int requestId, string status)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return RedirectToLogin();
            await friendsService.UpdateRequestAsync(requestId, status);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFriend(int friendshipId)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return RedirectToLogin();
            await friendsService.RemoveFriendAsync(friendshipId);
            return RedirectToAction("Index");
        }


    }
}
