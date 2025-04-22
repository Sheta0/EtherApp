using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EtherApp.Data.ML
{
    public class UserPostData
    {
        [KeyType(count: 262144)]
        public uint UserId { get; set; }

        [KeyType(count: 262144)]
        public uint PostId { get; set; }

        public float Label { get; set; } // Represents user's interest level (likes, comments, etc.)

        public string PostContent { get; set; }
    }

    public class UserPostPrediction
    {
        public float Score { get; set; }
    }

    public class UserSimilarityPrediction
    {
        public uint UserId { get; set; }
        public uint SimilarUserId { get; set; }
        public float Score { get; set; }
    }

    public class ContentFeatures
    {
        public uint PostId { get; set; }
        [VectorType(100)]
        public float[] Features { get; set; }
    }
}
