using EtherApp.Data.Helpers;
using EtherApp.Data.ML;
using EtherApp.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtherApp.Data.Services
{
    public class MLInterestService : IMLInterestService
    {
        private readonly AppDBContext _context;
        private readonly MLContext _mlContext;
        private ITransformer _model;
        private PredictionEngine<UserPostData, UserPostPrediction> _predictionEngine;

        public MLInterestService(AppDBContext context)
        {
            _context = context;
            _mlContext = new MLContext(seed: 0);
        }

        public async Task<ITransformer> BuildAndTrainModel()
        {
            // Get all user interactions with posts (likes, comments, favorites)
            var userInteractions = await GetUserPostInteractionsAsync();

            // Convert to IDataView
            var data = _mlContext.Data.LoadFromEnumerable(userInteractions);

            // Split data for training and testing
            var trainTestData = _mlContext.Data.TrainTestSplit(data, testFraction: 0.2);

            // Build the training pipeline
            var pipeline = _mlContext.Transforms.Conversion.MapValueToKey(
                    outputColumnName: "UserIdEncoded",
                    inputColumnName: "UserId")
                .Append(_mlContext.Transforms.Conversion.MapValueToKey(
                    outputColumnName: "PostIdEncoded",
                    inputColumnName: "PostId"))
                // Add text feature extraction for post content
                .Append(_mlContext.Transforms.Text.FeaturizeText(
                    outputColumnName: "ContentFeatures",
                    inputColumnName: "PostContent"))
                // Use Matrix Factorization for recommendations
                .Append(_mlContext.Recommendation().Trainers.MatrixFactorization(
                    labelColumnName: "Label",
                    matrixColumnIndexColumnName: "UserIdEncoded",
                    matrixRowIndexColumnName: "PostIdEncoded",
                    approximationRank: 100,
                    learningRate: 0.01,
                    numberOfIterations: 20));

            Console.WriteLine("Training the model...");
            _model = pipeline.Fit(trainTestData.TrainSet);

            // Evaluate the model
            var predictions = _model.Transform(trainTestData.TestSet);
            var metrics = _mlContext.Regression.Evaluate(predictions);

            Console.WriteLine($"Root Mean Squared Error: {metrics.RootMeanSquaredError}");
            Console.WriteLine($"RSquared: {metrics.RSquared}");

            // Create prediction engine
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<UserPostData, UserPostPrediction>(_model);

            return _model;
        }

        public async Task<List<Post>> GetRecommendedPostsForUser(int userId, int maxPosts = 10)
        {
            // Ensure model is trained
            if (_model == null)
            {
                await BuildAndTrainModel();
            }

            // Get all posts the user hasn't interacted with
            var userInteractions = await GetUserInteractionsForUser(userId);
            var interactedPostIds = userInteractions.Select(i => (int)i.PostId).ToList();

            var allPosts = await _context.Posts
                .Where(p => !p.IsPrivate && !interactedPostIds.Contains(p.Id) && p.UserId != userId)
                .Include(p => p.User)
                .Include(p => p.Like)
                .Include(p => p.Favorites)
                .Include(p => p.Comment)
                .ThenInclude(c => c.User)
                .ToListAsync();

            // Score each post for this user
            var scoredPosts = new List<(Post Post, float Score)>();

            foreach (var post in allPosts)
            {
                var prediction = _predictionEngine.Predict(new UserPostData
                {
                    UserId = (uint)userId,
                    PostId = (uint)post.Id,
                    PostContent = post.Content
                });

                scoredPosts.Add((post, prediction.Score));
            }

            // Return top N posts by predicted score
            return scoredPosts
                .OrderByDescending(p => p.Score)
                .Take(maxPosts)
                .Select(p => p.Post)
                .ToList();
        }

        public async Task<List<User>> GetSimilarUsers(int userId, int maxUsers = 10)
        {
            // Find users with similar interaction patterns
            var userVectors = await BuildUserVectorsAsync();

            if (!userVectors.ContainsKey(userId))
            {
                return new List<User>();
            }

            var currentUserVector = userVectors[userId];
            var similarities = new Dictionary<int, double>();

            foreach (var otherUser in userVectors)
            {
                if (otherUser.Key == userId) continue;

                // Calculate cosine similarity between user vectors
                similarities[otherUser.Key] = CosineSimilarity(currentUserVector, otherUser.Value);
            }

            // Get top similar users
            var topUserIds = similarities
                .OrderByDescending(kv => kv.Value)
                .Take(maxUsers)
                .Select(kv => kv.Key)
                .ToList();

            return await _context.Users
                .Where(u => topUserIds.Contains(u.Id))
                .ToListAsync();
        }

        private async Task<List<UserPostData>> GetUserPostInteractionsAsync()
        {
            var interactions = new List<UserPostData>();

            // Get likes
            var likes = await _context.Likes
                .Include(l => l.Post)
                .ToListAsync();

            foreach (var like in likes)
            {
                interactions.Add(new UserPostData
                {
                    UserId = (uint)like.UserId,
                    PostId = (uint)like.PostId,
                    Label = 1.0f, // High interest
                    PostContent = like.Post.Content
                });
            }

            // Get comments (stronger interest indicator)
            var comments = await _context.Comments
                .Include(c => c.Post)
                .ToListAsync();

            foreach (var comment in comments)
            {
                interactions.Add(new UserPostData
                {
                    UserId = (uint)comment.UserId,
                    PostId = (uint)comment.PostId,
                    Label = 1.5f, // Even higher interest
                    PostContent = comment.Post.Content
                });
            }

            // Get favorites
            var favorites = await _context.Favorites
                .Include(f => f.Post)
                .ToListAsync();

            foreach (var favorite in favorites)
            {
                interactions.Add(new UserPostData
                {
                    UserId = (uint)favorite.UserId,
                    PostId = (uint)favorite.PostId,
                    Label = 2.0f, // Highest interest
                    PostContent = favorite.Post.Content
                });
            }

            // Add negative samples (posts user has seen but not interacted with)
            // This would require tracking post views, which might not be available

            return interactions;
        }

        private async Task<List<UserPostData>> GetUserInteractionsForUser(int userId)
        {
            return (await GetUserPostInteractionsAsync())
                .Where(i => i.UserId == userId)
                .ToList();
        }

        private async Task<Dictionary<int, float[]>> BuildUserVectorsAsync()
        {
            // Extract hashtags and keywords from each user's posts and interactions
            var userVectors = new Dictionary<int, Dictionary<string, float>>();

            // Process posts
            var posts = await _context.Posts
                .Include(p => p.User)
                .ToListAsync();

            foreach (var post in posts)
            {
                // Get hashtags and keywords from post
                var terms = HashtagsHelper.GetHashtags(post.Content)
                    .Select(h => h.TrimStart('#').ToLowerInvariant())
                    .ToList();

                // Add to user's vector
                if (!userVectors.ContainsKey(post.UserId))
                {
                    userVectors[post.UserId] = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
                }

                foreach (var term in terms)
                {
                    if (userVectors[post.UserId].ContainsKey(term))
                    {
                        userVectors[post.UserId][term] += 2.0f; // Author's terms count more
                    }
                    else
                    {
                        userVectors[post.UserId][term] = 2.0f;
                    }
                }
            }

            // Process interactions (likes, comments, favorites)
            var interactions = await GetUserPostInteractionsAsync();

            foreach (var interaction in interactions)
            {
                var userId = (int)interaction.UserId;
                var terms = HashtagsHelper.GetHashtags(interaction.PostContent)
                    .Select(h => h.TrimStart('#').ToLowerInvariant())
                    .ToList();

                if (!userVectors.ContainsKey(userId))
                {
                    userVectors[userId] = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
                }

                foreach (var term in terms)
                {
                    if (userVectors[userId].ContainsKey(term))
                    {
                        userVectors[userId][term] += interaction.Label * 0.5f;
                    }
                    else
                    {
                        userVectors[userId][term] = interaction.Label * 0.5f;
                    }
                }
            }

            // Convert dictionaries to vectors
            var allTerms = userVectors
                .SelectMany(u => u.Value.Keys)
                .Distinct()
                .ToList();

            var termIndex = allTerms
                .Select((term, index) => (term, index))
                .ToDictionary(x => x.term, x => x.index);

            var result = new Dictionary<int, float[]>();

            foreach (var user in userVectors)
            {
                var vector = new float[allTerms.Count];

                foreach (var term in user.Value)
                {
                    vector[termIndex[term.Key]] = term.Value;
                }

                result[user.Key] = vector;
            }

            return result;
        }

        private double CosineSimilarity(float[] vector1, float[] vector2)
        {
            double dotProduct = 0.0;
            double norm1 = 0.0;
            double norm2 = 0.0;

            for (int i = 0; i < vector1.Length; i++)
            {
                dotProduct += vector1[i] * vector2[i];
                norm1 += vector1[i] * vector1[i];
                norm2 += vector2[i] * vector2[i];
            }

            return dotProduct / (Math.Sqrt(norm1) * Math.Sqrt(norm2));
        }
    }
}
