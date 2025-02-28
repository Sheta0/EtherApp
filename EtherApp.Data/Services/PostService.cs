using EtherApp.Data.Helpers;
using EtherApp.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherApp.Data.Services
{
    public class PostService : IPostsService
    {
        private readonly AppDBContext _context;
        public PostService(AppDBContext context) 
        {
            _context = context;
        }


        public async Task<List<Post>> GetAllPostsAsync(int loggedInUserId)
        {
            var allPosts = await _context.Posts
               .Where(n => (!n.IsPrivate || n.UserId == loggedInUserId) && n.NrOfReports < 5 /*&& !n.IsDeleted*/)
               .Include(n => n.User)
               .Include(n => n.Like)
               .Include(n => n.Favorites)
               .Include(n => n.Comment).ThenInclude(n => n.User)
               .Include(n => n.Reports)
               .OrderByDescending(n => n.DateCreated)
               .ToListAsync();

            return allPosts;
        }

        public async Task<Post> GetPostByIdAsync(int postId)
        {
            var postDb = await _context.Posts
                .Include(n => n.User)
                .Include(n => n.Like)
                .Include(n => n.Favorites)
                .Include(n => n.Comment).ThenInclude(n => n.User)
                .FirstOrDefaultAsync(n => n.Id == postId);
            return postDb;
        }

        public async Task<List<Post>> GetAllFavoritedPostsAsync(int loggedInUserId)
        {
            var allFavoritedPosts = await _context.Favorites
                .Where(n => n.UserId == loggedInUserId && n.Post.Reports.Count < 5 /*&& !n.IsDeleted*/)
                .Include(f => f.Post)
                    .ThenInclude(p => p.User)
                .Include(f => f.Post)
                    .ThenInclude(p => p.Favorites)
                .Include(f => f.Post)
                    .ThenInclude(p => p.Comment)
                        .ThenInclude(c => c.User)
                .Include(f => f.Post)
                    .ThenInclude(p => p.Like)
                .Select(f => f.Post)
                .ToListAsync();

            return allFavoritedPosts;
        }

        public async Task AddPostCommontAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
        }

        public async Task<Post> CreatePostAsync(Post post)
        {
            

            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();

            return post;
        }

        public async Task<Post> DeletePostAsync(int postId)
        {
            var postDb = await _context.Posts.FirstOrDefaultAsync(n => n.Id == postId);
            
            if (postDb != null)
            {

                _context.Posts.Remove(postDb);
                await _context.SaveChangesAsync();

            }

            return postDb;
        }

        public async Task DeletePostCommentAsync(int commentId)
        {
            var commentDb = _context.Comments.FirstOrDefault(n => n.Id == commentId);

            if (commentDb != null)
            {
                _context.Comments.Remove(commentDb);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ReportPostAsync(int postId, int userId)
        {
            var newReport = new Report()
            {
                UserId = userId,
                PostId = postId,
                DateCreated = DateTime.Now
            };

            await _context.Reports.AddAsync(newReport);

            // Update the NrOfReports in the Posts table
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == postId);
            if (post != null)
            {
                post.NrOfReports++;
                _context.Posts.Update(post);
            }

            await _context.SaveChangesAsync();
        }

        public async Task TogglePostFavoriteAsync(int postId, int userId)
        {
            //check if user favorited the post
            var favorite = await _context.Favorites
                .Where(l => l.PostId == postId && l.UserId == userId)
                .FirstOrDefaultAsync();

            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
            }
            else
            {
                var newFavorite = new Favorite()
                {
                    PostId = postId,
                    UserId = userId,
                    DateCreated = DateTime.Now
                };
                await _context.Favorites.AddAsync(newFavorite);
                await _context.SaveChangesAsync();
            }
        }

        public async Task TogglePostLikeAsync(int postId, int userId)
        {
            //check if user liked the post
            var like = await _context.Likes
                .Where(l => l.PostId == postId && l.UserId == userId)
                .FirstOrDefaultAsync();

            if (like != null)
            {
                _context.Likes.Remove(like);
                await _context.SaveChangesAsync();
            }
            else
            {
                var newLike = new Like()
                {
                    PostId = postId,
                    UserId = userId
                };
                await _context.Likes.AddAsync(newLike);
                await _context.SaveChangesAsync();
            }
        }

        public async Task TogglePostVisibilityAsync(int postId, int userId)
        {
            //check if user favorited the post
            var post = await _context.Posts
                .FirstOrDefaultAsync(l => l.Id == postId && l.UserId == userId);
            if (post != null)
            {
                post.IsPrivate = !post.IsPrivate;
                _context.Posts.Update(post);
                await _context.SaveChangesAsync();
            }
        }
    }
}
