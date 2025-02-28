﻿using EtherApp.Controllers.Base;
using EtherApp.Data.Helpers.Enums;
using EtherApp.Data.Models;
using EtherApp.Data.Services;
using EtherApp.ViewModels.Stories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtherApp.Controllers
{
    [Authorize]
    public class StoriesController : BaseController
    {
        private readonly IStoriesService _storiesService;
        private readonly IFilesService _filesService;
        public StoriesController(IStoriesService storiesService, IFilesService filesService)
        {
            _storiesService = storiesService;
            _filesService = filesService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateStory(StoryVM storyVM)
        {

            var loggedInUser = GetUserId();
            if (loggedInUser is null) return RedirectToLogin();

            var imageUploadPath = await _filesService.UploadImageAsync(storyVM.Image, ImageFileType.StoryImage);

            var newStory = new Story
            {
                DateCreated = DateTime.Now,
                IsDeleted = false,
                ImageUrl = imageUploadPath,
                UserId = loggedInUser.Value
            };

            await _storiesService.CreateStoryAsync(newStory);

            return RedirectToAction("Index", "Home");
        }
    }
}
