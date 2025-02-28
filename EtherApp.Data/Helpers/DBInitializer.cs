using EtherApp.Data.Helpers.Constants;
using EtherApp.Data.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherApp.Data.Helpers
{
    public static class DBInitializer
    {
        public static async Task SeedUsersAndRolesAsync(UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            // Roles
            if (!roleManager.Roles.Any())
            {
                foreach (var role in AppRoles.AllRoles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole<int>(role));
                    }
                }

            }

            // Users with roles
            if (!userManager.Users.Any(n => !string.IsNullOrEmpty(n.Email)))
            {
                var userPassword = "P@ssw0rd";
                var user = new User()
                {
                    UserName = "sheta0.9",
                    Email = "theimpossible000@gmail.com",
                    FullName = "Ahmed Sheta",
                    ProfilePictureUrl = "~/images/avatar/profile.jpg",
                    EmailConfirmed = true
                };

                var userResult = await userManager.CreateAsync(user, userPassword);

                if (userResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, AppRoles.User);
                }


                var Admin = new User()
                {
                    UserName = "admin.admin",
                    Email = "ahmedsheta834@gmail.com",
                    FullName = "Sheta Admin",
                    ProfilePictureUrl = "~/images/avatar/profile.jpg",
                    EmailConfirmed = true
                };

                var adminResult = await userManager.CreateAsync(Admin, userPassword);

                if (adminResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(Admin, AppRoles.Admin);
                }
            }
        }
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
