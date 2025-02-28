using EtherApp.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherApp.Data.Services
{
    public class UserService : IUserService
    {
        private readonly AppDBContext _appDbContext;
        public UserService(AppDBContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public async Task<User> GetUserByIdAsync(int loggedInUserId)
        {
            return await _appDbContext.Users.FirstOrDefaultAsync(n => n.Id == loggedInUserId) ?? new User() ;
        }

        public async Task UpdateUserProfilePicture(int loggedInUserId, string profilePictureUrl)
        {
            var user = _appDbContext.Users.FirstOrDefault(u => u.Id == loggedInUserId);

            if (user != null)
            {
                user.ProfilePictureUrl = profilePictureUrl;
                _appDbContext.Users.Update(user);
                await _appDbContext.SaveChangesAsync();
            }
        }
    }
}
