using EtherApp.Data.Dtos;
using EtherApp.Data.Helpers.Constants;
using EtherApp.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherApp.Data.Services
{
    public class FriendsService(AppDBContext context) : IFriendsService
    {
        public async Task SendRequestAsync(int senderId, int receiverId)
        {
            var request = new FriendRequest
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Status = FriendRequestStatus.Pending,
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow
            };
            await context.FriendRequests.AddAsync(request);
            await context.SaveChangesAsync();
        }

        public async Task<FriendRequest> UpdateRequestAsync(int requestId, string newStatus)
        {
            var request = await context.FriendRequests.FindAsync(requestId);
            if (request != null && request.Status == FriendRequestStatus.Pending)
            {
                request.Status = newStatus;
                request.DateUpdated = DateTime.UtcNow;
                context.FriendRequests.Update(request);
                await context.SaveChangesAsync();
            }

            if (newStatus == FriendRequestStatus.Accepted)
            {
                var friendship = new Friendship
                {
                    SenderId = request.SenderId,
                    ReceiverId = request.ReceiverId,
                    DateCreated = DateTime.UtcNow
                };
                await context.Friendships.AddAsync(friendship);
                await context.SaveChangesAsync();
            }

            return request;
        }

        public async Task RemoveFriendAsync(int friendshipId)
        {
            var friendship = await context.Friendships.FindAsync(friendshipId);
            if (friendship != null)
            {
                context.Friendships.Remove(friendship);
                await context.SaveChangesAsync();

                var requests = await context.FriendRequests
                    .Where(fr => (fr.SenderId == friendship.SenderId && fr.ReceiverId == friendship.ReceiverId) ||
                                               (fr.SenderId == friendship.ReceiverId && fr.ReceiverId == friendship.SenderId)).ToListAsync();
                if (requests.Any())
                {
                    context.FriendRequests.RemoveRange(requests);
                    await context.SaveChangesAsync();
                }
            }
        }

        public async Task<List<UserWithFriendsCountDto>> GetSuggestedFriendsAsync(int userId)
        {
            var existingFriendIds = await context.Friendships
                .Where(f => f.SenderId == userId || f.ReceiverId == userId)
                .Select(f => f.SenderId == userId ? f.ReceiverId : f.SenderId)
                .ToListAsync();

            var pendingRequestIds = await context.FriendRequests
                .Where(fr => (fr.SenderId == userId || fr.ReceiverId == userId) && fr.Status == FriendRequestStatus.Pending)
                .Select(fr => fr.SenderId == userId ? fr.ReceiverId : fr.SenderId)
                .ToListAsync();

            var suggestedFriends = await context.Users
                .Where(u => u.Id != userId && !existingFriendIds.Contains(u.Id) && !pendingRequestIds.Contains(u.Id))
                .Select(u => new UserWithFriendsCountDto
                {
                    User = u,
                    FriendsCount = context.Friendships.Count(f => f.SenderId == u.Id || f.ReceiverId == u.Id)
                })
                .Take(5)
                .ToListAsync();

            return suggestedFriends ?? new List<UserWithFriendsCountDto>();
        }
        public async Task<List<FriendRequest>> GetSentFriendRequestsAsync(int userId)
        {
            var sentRequests = await context.FriendRequests
                .Include(fr => fr.Receiver)
                .Include(fr => fr.Sender)
                .Where(fr => fr.SenderId == userId && fr.Status == FriendRequestStatus.Pending)
                .ToListAsync();
            return sentRequests ?? new List<FriendRequest>();
        }

        public async Task<List<FriendRequest>> GetReceivedFriendRequestsAsync(int userId)
        {
            var receivedRequests = await context.FriendRequests
                .Include(fr => fr.Receiver)
                .Include(fr => fr.Sender)
                .Where(fr => fr.ReceiverId == userId && fr.Status == FriendRequestStatus.Pending)
                .ToListAsync();
            return receivedRequests ?? new List<FriendRequest>();
        }

        public async Task<List<Friendship>> GetUserFriendsAsync(int userId)
        {
            var friends = await context.Friendships
                .Include(f => f.Receiver)
                .Include(f => f.Sender)
                .Where(f => f.SenderId == userId || f.ReceiverId == userId)
                .ToListAsync();
            return friends ?? new List<Friendship>();
        }
    }
}
