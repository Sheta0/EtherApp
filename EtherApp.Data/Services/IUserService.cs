﻿using EtherApp.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherApp.Data.Services
{
    public interface IUserService
    {
        Task<User> GetUserByIdAsync(int id);
        Task UpdateUserProfilePicture (int loggedInUserId, string profilePictureUrl);
    }
}
