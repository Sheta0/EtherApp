using EtherApp.Data.Dtos;
using EtherApp.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherApp.Data.Services
{
    public interface IFriendsService
    {
        Task SendRequestAsync(int senderId, int receiverId);
        Task UpdateRequestAsync(int requestId, string status);
        Task RemoveFriendAsync(int friendshipId);
        Task<List<UserWithFriendsCountDto>> GetSuggestedFriendsAsync(int userId);
        Task<List<FriendRequest>> GetSentFriendRequestsAsync(int userId);
        Task<List<FriendRequest>> GetReceivedFriendRequestsAsync(int userId);
        Task<List<Friendship>> GetUserFriendsAsync(int userId);
    }
}
