using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;

namespace doan3.Models.Mgdb
{
    /// <summary>
    /// Service giao tiếp trực tiếp với MongoDB Docker Container (CinemaNoSQL Database)
    /// </summary>
    public class MgdbService
    {
        private static readonly string ConnectionString = ConfigurationManager.AppSettings["MongoConnectionString"] 
            ?? "mongodb://admin:adminpassword@localhost:27017";

        private static readonly string DatabaseName = ConfigurationManager.AppSettings["MongoDatabaseName"] 
            ?? "CinemaNoSQL";

        private static IMongoDatabase GetDatabase()
        {
            var client = new MongoClient(ConnectionString);
            return client.GetDatabase(DatabaseName);
        }

        private static IMongoCollection<BsonDocument> ReviewsCollection => GetDatabase().GetCollection<BsonDocument>("movie_reviews");
        private static IMongoCollection<BsonDocument> FeedbacksCollection => GetDatabase().GetCollection<BsonDocument>("customer_feedbacks");

        // ===============================================================================
        // 1. CÁC TÍNH NĂNG MONGODB CHO COLLECTION 'movie_reviews'
        // ===============================================================================

        /// <summary>
        /// Lấy danh sách đánh giá của 1 phim (Lọc status Approved, Sắp xếp Mới nhất hoặc Nhiêu Like nhất)
        /// </summary>
        public static List<MgdbMovieReviewModel> GetReviewsByMovie(int movieId, string sortBy = "newest")
        {
            var list = new List<MgdbMovieReviewModel>();
            try
            {
                var filter = Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Eq("movieId", movieId),
                    Builders<BsonDocument>.Filter.Eq("status", "Approved")
                );

                var sort = sortBy == "likes" 
                    ? Builders<BsonDocument>.Sort.Descending("likesCount") 
                    : Builders<BsonDocument>.Sort.Descending("createdAt");

                var docs = ReviewsCollection.Find(filter).Sort(sort).ToList();

                foreach (var doc in docs)
                {
                    list.Add(MapDocToReviewModel(doc));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error Mgdb GetReviewsByMovie: " + ex.Message);
            }
            return list;
        }

        /// <summary>
        /// Tạo một bài đánh giá phim mới (CRUD: Create)
        /// </summary>
        public static bool AddReview(MgdbMovieReviewModel review)
        {
            try
            {
                var doc = new BsonDocument
                {
                    { "movieId", review.MovieId },
                    { "movieTitle", review.MovieTitle ?? "Phim" },
                    { "userId", review.UserId },
                    { "username", review.Username ?? "NguoiDung" },
                    { "rating", review.Rating },
                    { "content", review.Content ?? "" },
                    { "tags", new BsonArray(review.Tags ?? new List<string>()) },
                    { "likesCount", 0 },
                    { "status", "Approved" },
                    { "createdAt", DateTime.UtcNow }
                };

                ReviewsCollection.InsertOne(doc);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error Mgdb AddReview: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Tăng 1 lượt thích (Like) cho bài đánh giá phim (CRUD: Update)
        /// </summary>
        public static bool LikeReview(string reviewId)
        {
            try
            {
                if (!ObjectId.TryParse(reviewId, out ObjectId objId)) return false;

                var filter = Builders<BsonDocument>.Filter.Eq("_id", objId);
                var update = Builders<BsonDocument>.Update.Inc("likesCount", 1);
                var result = ReviewsCollection.UpdateOne(filter, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error Mgdb LikeReview: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// MongoDB Aggregation Pipeline: Tính điểm trung bình (avgRating) và thống kê review theo bộ phim
        /// </summary>
        public static MgdbMovieRatingStats GetMovieRatingStats(int movieId)
        {
            var stats = new MgdbMovieRatingStats { MovieId = movieId, AvgRating = 5.0, TotalReviews = 0, TotalLikes = 0 };
            try
            {
                var pipeline = new[]
                {
                    new BsonDocument("$match", new BsonDocument { { "movieId", movieId }, { "status", "Approved" } }),
                    new BsonDocument("$group", new BsonDocument
                    {
                        { "_id", "$movieId" },
                        { "movieTitle", new BsonDocument("$first", "$movieTitle") },
                        { "avgRating", new BsonDocument("$avg", "$rating") },
                        { "totalReviews", new BsonDocument("$sum", 1) },
                        { "totalLikes", new BsonDocument("$sum", "$likesCount") }
                    })
                };

                var result = ReviewsCollection.Aggregate<BsonDocument>(pipeline).FirstOrDefault();
                if (result != null)
                {
                    stats.MovieTitle = result.Contains("movieTitle") ? result["movieTitle"].AsString : "";
                    stats.AvgRating = result.Contains("avgRating") && !result["avgRating"].IsBsonNull ? Math.Round(result["avgRating"].AsDouble, 1) : 5.0;
                    stats.TotalReviews = result.Contains("totalReviews") ? result["totalReviews"].AsInt32 : 0;
                    stats.TotalLikes = result.Contains("totalLikes") ? result["totalLikes"].AsInt32 : 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error Mgdb GetMovieRatingStats: " + ex.Message);
            }
            return stats;
        }

        // ===============================================================================
        // 2. CÁC TÍNH NĂNG MONGODB CHO COLLECTION 'customer_feedbacks'
        // ===============================================================================

        /// <summary>
        /// Lấy tất cả phản hồi / khiếu nại của 1 người dùng
        /// </summary>
        public static List<MgdbCustomerFeedbackModel> GetFeedbacksByUser(string username)
        {
            var list = new List<MgdbCustomerFeedbackModel>();
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("username", username);
                var docs = FeedbacksCollection.Find(filter).SortByDescending(d => d["createdAt"]).ToList();

                foreach (var doc in docs)
                {
                    list.Add(MapDocToFeedbackModel(doc));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error Mgdb GetFeedbacksByUser: " + ex.Message);
            }
            return list;
        }

        /// <summary>
        /// Gửi 1 yêu cầu hỗ trợ / khiếu nại mới (CRUD: Create)
        /// </summary>
        public static bool AddFeedback(MgdbCustomerFeedbackModel feedback)
        {
            try
            {
                var doc = new BsonDocument
                {
                    { "userId", feedback.UserId },
                    { "username", feedback.Username ?? "NguoiDung" },
                    { "email", feedback.Email ?? "" },
                    { "category", feedback.Category ?? "Khác" },
                    { "subject", feedback.Subject ?? "Hỗ trợ" },
                    { "content", feedback.Content ?? "" },
                    { "imageUrls", new BsonArray(feedback.ImageUrls ?? new List<string>()) },
                    { "status", "New" },
                    { "conversations", new BsonArray() },
                    { "createdAt", DateTime.UtcNow }
                };

                FeedbacksCollection.InsertOne(doc);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error Mgdb AddFeedback: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Admin trả lời phản hồi và cập nhật trạng thái (CRUD: Update)
        /// </summary>
        public static bool ReplyFeedback(string feedbackId, string replyMessage, string sender = "Admin")
        {
            try
            {
                if (!ObjectId.TryParse(feedbackId, out ObjectId objId)) return false;

                var filter = Builders<BsonDocument>.Filter.Eq("_id", objId);
                var conversationDoc = new BsonDocument
                {
                    { "sender", sender },
                    { "message", replyMessage },
                    { "createdAt", DateTime.UtcNow }
                };

                var update = Builders<BsonDocument>.Update
                    .Set("status", "Resolved")
                    .Push("conversations", conversationDoc);

                var result = FeedbacksCollection.UpdateOne(filter, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error Mgdb ReplyFeedback: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// MongoDB Aggregation Pipeline: Thống kê số lượng phản hồi theo chuyên mục (category)
        /// </summary>
        public static List<MgdbFeedbackCategoryStats> GetFeedbackCategoryStats()
        {
            var list = new List<MgdbFeedbackCategoryStats>();
            try
            {
                var pipeline = new[]
                {
                    new BsonDocument("$group", new BsonDocument
                    {
                        { "_id", "$category" },
                        { "totalTickets", new BsonDocument("$sum", 1) },
                        { "resolvedCount", new BsonDocument("$sum", new BsonDocument("$cond", new BsonArray { new BsonDocument("$eq", new BsonArray { "$status", "Resolved" }), 1, 0 })) },
                        { "pendingCount", new BsonDocument("$sum", new BsonDocument("$cond", new BsonArray { new BsonDocument("$ne", new BsonArray { "$status", "Resolved" }), 1, 0 })) }
                    })
                };

                var docs = FeedbacksCollection.Aggregate<BsonDocument>(pipeline).ToList();
                foreach (var doc in docs)
                {
                    list.Add(new MgdbFeedbackCategoryStats
                    {
                        Category = doc["_id"].AsString,
                        TotalTickets = doc["totalTickets"].AsInt32,
                        ResolvedCount = doc["resolvedCount"].AsInt32,
                        PendingCount = doc["pendingCount"].AsInt32
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error Mgdb GetFeedbackCategoryStats: " + ex.Message);
            }
            return list;
        }

        // ===============================================================================
        // HELPER MAPPING METHODS
        // ===============================================================================

        private static MgdbMovieReviewModel MapDocToReviewModel(BsonDocument doc)
        {
            var model = new MgdbMovieReviewModel
            {
                Id = doc["_id"].ToString(),
                MovieId = doc.Contains("movieId") ? doc["movieId"].AsInt32 : 0,
                MovieTitle = doc.Contains("movieTitle") ? doc["movieTitle"].AsString : "",
                UserId = doc.Contains("userId") ? doc["userId"].AsInt32 : 0,
                Username = doc.Contains("username") ? doc["username"].AsString : "",
                Rating = doc.Contains("rating") ? doc["rating"].AsInt32 : 5,
                Content = doc.Contains("content") ? doc["content"].AsString : "",
                LikesCount = doc.Contains("likesCount") ? doc["likesCount"].AsInt32 : 0,
                Status = doc.Contains("status") ? doc["status"].AsString : "Approved"
            };

            if (doc.Contains("tags") && doc["tags"].IsBsonArray)
            {
                model.Tags = doc["tags"].AsBsonArray.Select(t => t.AsString).ToList();
            }

            return model;
        }

        private static MgdbCustomerFeedbackModel MapDocToFeedbackModel(BsonDocument doc)
        {
            var model = new MgdbCustomerFeedbackModel
            {
                Id = doc["_id"].ToString(),
                UserId = doc.Contains("userId") ? doc["userId"].AsInt32 : 0,
                Username = doc.Contains("username") ? doc["username"].AsString : "",
                Email = doc.Contains("email") ? doc["email"].AsString : "",
                Category = doc.Contains("category") ? doc["category"].AsString : "Khác",
                Subject = doc.Contains("subject") ? doc["subject"].AsString : "",
                Content = doc.Contains("content") ? doc["content"].AsString : "",
                Status = doc.Contains("status") ? doc["status"].AsString : "New"
            };

            if (doc.Contains("conversations") && doc["conversations"].IsBsonArray)
            {
                foreach (var conv in doc["conversations"].AsBsonArray)
                {
                    if (conv.IsBsonDocument)
                    {
                        var cDoc = conv.AsBsonDocument;
                        model.Conversations.Add(new MgdbFeedbackConversation
                        {
                            Sender = cDoc.Contains("sender") ? cDoc["sender"].AsString : "Admin",
                            Message = cDoc.Contains("message") ? cDoc["message"].AsString : ""
                        });
                    }
                }
            }

            return model;
        }
    }
}
