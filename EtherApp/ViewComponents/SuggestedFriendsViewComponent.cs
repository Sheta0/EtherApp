using EtherApp.Data.Services;
using EtherApp.ViewModels.Friends;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EtherApp.ViewComponents
{
    public class SuggestedFriendsViewComponent(IFriendsService friendsService) : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var loggedInUser = ((ClaimsPrincipal)User).FindFirstValue(ClaimTypes.NameIdentifier);
            var loggedInUserId = int.Parse(loggedInUser);
            var suggestedFriends = await friendsService.GetSuggestedFriendsAsync(loggedInUserId);
            var suggestedFriendsVM = suggestedFriends.Select(u => new UserWithFriendsCountVM
            {
                UserId = u.User.Id,
                FullName = u.User.FullName,
                ProfilePictureUrl = u.User.ProfilePictureUrl,
                FriendsCount = u.FriendsCount
            }).ToList();
            return View(suggestedFriendsVM);
        }
    }
}
