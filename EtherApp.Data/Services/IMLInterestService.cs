using EtherApp.Data.Models;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherApp.Data.Services
{
    public interface IMLInterestService
    {
        Task<ITransformer> BuildAndTrainModel();
        Task<List<Post>> GetRecommendedPostsForUser(int userId, int maxPosts = 10);
        Task<List<User>> GetSimilarUsers(int userId, int maxUsers = 10);
    }
}
