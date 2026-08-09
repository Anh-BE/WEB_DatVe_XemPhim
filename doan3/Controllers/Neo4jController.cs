using System;
using System.Collections.Generic;
using System.Web.Mvc;
using doan3.Models;

namespace doan3.Controllers
{
    public class Neo4jController : Controller
    {
        private readonly Neo4jService _neo4jService = new Neo4jService();

        // GET: /Neo4j/
        public ActionResult Index()
        {
            var userSession = Session["USER_SESSION"] as UserLogin;
            string username = userSession != null ? userSession.UserName : (Session["username"] as string ?? "");
            
            // Tự động kiểm tra và khởi tạo dữ liệu mẫu nếu Neo4j chưa có dữ liệu
            _neo4jService.SeedInitialData();

            var recommendedMovies = _neo4jService.GetRecommendedMovies(username, 6);
            var topBooked = _neo4jService.GetTopBookedMovies(6, username);
            var topFavorites = _neo4jService.GetTopFavoriteMovies(6, username);
            var trendingGenres = _neo4jService.GetTrendingGenres(5);

            ViewBag.RecommendedMovies = recommendedMovies;
            ViewBag.TopBooked = topBooked;
            ViewBag.TopFavorites = topFavorites;
            ViewBag.TrendingGenres = trendingGenres;
            ViewBag.CurrentUsername = username;

            return View();
        }

        // POST: /Neo4j/ToggleFavorite
        [HttpPost]
        public ActionResult ToggleFavorite(int movieId, string title, string poster)
        {
            var userSession = Session["USER_SESSION"] as UserLogin;
            string username = userSession != null ? userSession.UserName : (Session["username"] as string);

            if (string.IsNullOrEmpty(username))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để sử dụng tính năng Yêu thích (❤️)!" });
            }

            bool isFavorited = _neo4jService.ToggleFavorite(username, movieId, title, poster);
            return Json(new { success = true, isFavorite = isFavorited, message = isFavorited ? "Đã thêm vào danh sách phim Yêu thích!" : "Đã bỏ Yêu thích bộ phim!" });
        }

        // POST: /Neo4j/SeedData
        [HttpPost]
        public ActionResult SeedData()
        {
            bool success = _neo4jService.SeedInitialData();
            return Json(new { success = success, message = success ? "Khởi tạo dữ liệu đồ thị Neo4j thành công!" : "Không thể kết nối tới Neo4j Server (Vui lòng kiểm tra Docker/Neo4j)." });
        }
    }
}
