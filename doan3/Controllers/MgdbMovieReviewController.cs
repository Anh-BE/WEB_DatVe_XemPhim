using System.Collections.Generic;
using System.Web.Mvc;
using doan3.Models.Mgdb;

namespace doan3.Controllers
{
    /// <summary>
    /// Controller quản lý tính năng Đánh giá & Bình luận phim (Sử dụng MongoDB Collection: 'movie_reviews')
    /// </summary>
    public class MgdbMovieReviewController : Controller
    {
        // GET: /MgdbMovieReview?movieId=101
        public ActionResult Index(int movieId = 101, string sortBy = "newest")
        {
            ViewBag.MovieId = movieId;
            ViewBag.SortBy = sortBy;

            // Lấy danh sách review từ MongoDB
            List<MgdbMovieReviewModel> reviews = MgdbService.GetReviewsByMovie(movieId, sortBy);

            // Lấy thống kê Rating bằng MongoDB Aggregation Pipeline
            MgdbMovieRatingStats stats = MgdbService.GetMovieRatingStats(movieId);
            ViewBag.MovieStats = stats;

            return View(reviews);
        }

        // POST: /MgdbMovieReview/AddReview
        [HttpPost]
        public ActionResult AddReview(int movieId, string movieTitle, int rating, string content, string tags)
        {
            var user = Session["User"] as doan3.Models.NguoiDung;
            string username = user != null ? user.UserName : "KhachHang";
            int userId = user != null ? user.UserID : 0;

            var review = new MgdbMovieReviewModel
            {
                MovieId = movieId,
                MovieTitle = string.IsNullOrEmpty(movieTitle) ? "Lật Mặt 7: Một Điều Ước" : movieTitle,
                UserId = userId,
                Username = username,
                Rating = rating,
                Content = content,
                Tags = !string.IsNullOrEmpty(tags) ? new List<string>(tags.Split(new[] { ',', ';' })) : new List<string> { "Phim hay" }
            };

            bool success = MgdbService.AddReview(review);
            if (success)
            {
                TempData["Message"] = "Đăng đánh giá thành công lên MongoDB!";
            }
            else
            {
                TempData["Error"] = "Có lỗi xảy ra khi lưu vào MongoDB!";
            }

            return RedirectToAction("Index", new { movieId = movieId });
        }

        // POST: /MgdbMovieReview/LikeReview
        [HttpPost]
        public JsonResult LikeReview(string reviewId)
        {
            bool success = MgdbService.LikeReview(reviewId);
            return Json(new { success = success });
        }
    }
}
