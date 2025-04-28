using EtherApp.Controllers.Base;
using EtherApp.Data.Services;
using EtherApp.ViewModels.Recommendations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtherApp.Controllers
{
    [Authorize]
    public class RecommendationsController : BaseController
    {
        private readonly IMLInterestService _mlInterestService;

        public RecommendationsController(IMLInterestService mlInterestService)
        {
            _mlInterestService = mlInterestService;
        }

        public async Task<IActionResult> Index()
        {
            var loggedInUser = GetUserId();
            if (loggedInUser is null) return RedirectToLogin();

            var recommendedPosts = await _mlInterestService.GetRecommendedPostsForUser(loggedInUser.Value);

            var similarUsers = await _mlInterestService.GetSimilarUsers(loggedInUser.Value);

            var viewModel = new RecommendationsVM
            {
                RecommendedPosts = recommendedPosts,
                SimilarUsers = similarUsers
            };

            return View(viewModel);
        }
    }
}
