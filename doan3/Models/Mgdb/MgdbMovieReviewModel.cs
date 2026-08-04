using System;
using System.Collections.Generic;

namespace doan3.Models.Mgdb
{
    /// <summary>
    /// Model lưu trữ đánh giá và bình luận phim trong MongoDB (Collection: 'movie_reviews')
    /// </summary>
    public class MgdbMovieReviewModel
    {
        public string Id { get; set; }
        public int MovieId { get; set; }
        public string MovieTitle { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public int Rating { get; set; } // 1 - 5 Sao
        public string Content { get; set; }
        public List<string> Tags { get; set; }
        public int LikesCount { get; set; }
        public string Status { get; set; } // "Approved", "Pending", "Rejected"
        public DateTime CreatedAt { get; set; }

        public MgdbMovieReviewModel()
        {
            Tags = new List<string>();
            Status = "Approved";
            CreatedAt = DateTime.UtcNow;
            LikesCount = 0;
        }
    }

    /// <summary>
    /// DTO chứa kết quả Thống kê Aggregation Pipeline cho Đánh giá phim
    /// </summary>
    public class MgdbMovieRatingStats
    {
        public int MovieId { get; set; }
        public string MovieTitle { get; set; }
        public double AvgRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalLikes { get; set; }
    }
}
