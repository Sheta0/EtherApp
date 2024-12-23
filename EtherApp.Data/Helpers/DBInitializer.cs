using EtherApp.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherApp.Data.Helpers
{
    public static class DBInitializer
    {
        public static async Task SeedAsync(AppDBContext appDBContext)
        {
            if (!appDBContext.Users.Any() && !appDBContext.Posts.Any())
            {
                var newUser = new User()
                {
                    FullName = "Ahmed Sheta",
                    ProfilePictureUrl = "~/images/avatar/profile.jpg"
                };
                await appDBContext.Users.AddAsync(newUser);
                await appDBContext.SaveChangesAsync();

                var newPostWithoutImage = new Post()
                {
                    Content = "YOOOO this new FB clone is lit lit lit lit lit 🗣️🗣️🗣️🔥",
                    ImageUrl = "",
                    NrOfReports = 0,
                    DateCreated = DateTime.Now,
                    DateUpdated = DateTime.Now,

                    UserId = newUser.Id
                };

                var newPostWithImage = new Post()
                {
                    Content = "You can post images and shit 😱😱🤯",
                    ImageUrl = "~/images/posts/gnx.png",
                    NrOfReports = 0,
                    DateCreated = DateTime.Now,
                    DateUpdated = DateTime.Now,

                    UserId = newUser.Id
                };

                await appDBContext.Posts.AddRangeAsync(newPostWithoutImage, newPostWithImage);
                await appDBContext.SaveChangesAsync();
            }
        }
    }
}
