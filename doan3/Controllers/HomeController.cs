using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using doan3.Models;
using doan3.Models.Cass;
using doan3.Models.Cass.DTO;
using System.Data.Entity;

namespace doan3.Controllers
{
    public class HomeController : Controller
    {
        LTW_DatVeXemPhimEntities db = new LTW_DatVeXemPhimEntities();

        // =====================================================================
        // GET: /Home/LandingPage
        // =====================================================================
        public ActionResult LandingPage()
        {
            ViewBag.HideHeader = true;
            try
            {
                var neo4jService = new Neo4jService();
                string username = Session["username"] as string ?? "";
                ViewBag.TopHotMovies = neo4jService.GetTopBookedMovies(4, username);
            }
            catch { }
            return View();
        }

        // =====================================================================
        // GET: /Home/GioiThieu
        // =====================================================================
        public ActionResult GioiThieu()
        {
            ViewBag.Message = "Câu chuyện về Movana Cinema";
            return View();
        }

        // =====================================================================
        // GET: /Home/ChinhSachTichDiem
        // =====================================================================
        public ActionResult ChinhSachTichDiem()
        {
            return View();
        }

        // =====================================================================
        // GET: /Home/PhimDangChieu
        // =====================================================================
        public ActionResult PhimDangChieu()
        {
            List<Phim> dsphim = db.Phims
                .Include(p => p.Lich_Chieu)
                .Where(p => p.TrangThai == "Dang Chieu")
                .OrderByDescending(t => t.NgayKhoiChieu)
                .ToList();

            try
            {
                var topBooked    = HttpRuntime.Cache["TopBookedNeo4j"]    as List<Neo4jMovieViewModel>;
                var topFavorites = HttpRuntime.Cache["TopFavoritesNeo4j"] as List<Neo4jMovieViewModel>;

                if (topBooked == null || topFavorites == null)
                {
                    var neo4jService  = new Neo4jService();
                    neo4jService.SeedInitialData(db);
                    var userSession   = Session["USER_SESSION"] as UserLogin;
                    string username   = userSession?.UserName ?? "";

                    topBooked    = neo4jService.GetTopBookedMovies(4, username);
                    topFavorites = neo4jService.GetTopFavoriteMovies(4, username);

                    for (int i = 0; i < topBooked.Count; i++)
                    {
                        var item    = topBooked[i];
                        var sqlPhim = dsphim.FirstOrDefault(p => p.PhimID == item.MovieId)
                                     ?? (i < dsphim.Count ? dsphim[i] : null);
                        if (sqlPhim != null)
                        {
                            item.Title  = sqlPhim.TenPhim;
                            item.Poster = sqlPhim.Poster;
                        }
                    }

                    HttpRuntime.Cache.Insert("TopBookedNeo4j",    topBooked,    null,
                        DateTime.Now.AddMinutes(5), System.Web.Caching.Cache.NoSlidingExpiration);
                    HttpRuntime.Cache.Insert("TopFavoritesNeo4j", topFavorites, null,
                        DateTime.Now.AddMinutes(5), System.Web.Caching.Cache.NoSlidingExpiration);
                }

                ViewBag.TopBookedNeo4j    = topBooked;
                ViewBag.TopFavoritesNeo4j = topFavorites;
            }
            catch { }

            return View(dsphim);
        }

        // =====================================================================
        // GET: /Home/PhimTheoTheLoai?MATHELOAI=
        // Log Cassandra: VIEW_MOVIE_BY_GENRE SUCCESS
        // =====================================================================
        public ActionResult PhimTheoTheLoai(int MATHELOAI)
        {
            TheLoai theloai = db.TheLoais.SingleOrDefault(t => t.MaTheLoai == MATHELOAI);
            if (theloai == null) return HttpNotFound();

            List<Phim> dsPhim = db.Phims
                .Where(p => p.MaTheLoai == MATHELOAI && p.TrangThai == "Dang Chieu")
                .OrderBy(p => p.ThoiLuong)
                .ToList();

            ViewBag.TenTheLoai = theloai.TenTheLoai;

            var sessionUser = Session["USER_SESSION"] as UserLogin;
            CassandraFeaturesService.GhiNhatKyHoatDong(new NhatKyHoatDongDTO
            {
                Username        = sessionUser?.UserName,
                HanhDong        = "VIEW_MOVIE_BY_GENRE",
                KetQua          = "SUCCESS",
                ChiTiet         = "Xem phim theo the loai: " + theloai.TenTheLoai + " (MaTheLoai=" + MATHELOAI + ")",
                ControllerName  = "Home",
                ActionName      = "PhimTheoTheLoai",
                RequestMethod   = Request.HttpMethod,
                Browser         = Request.Browser.Browser + " " + Request.Browser.Version,
                Device          = Request.Browser.IsMobileDevice ? "Mobile" : "Desktop",
                HeDieuHanh      = Request.Browser.Platform,
                IpAddress       = Request.UserHostAddress
            });

            return View(dsPhim);
        }

        // =====================================================================
        // GET: /Home/PhimTheo_Rap?IDRap=
        // Log Cassandra: VIEW_CINEMA SUCCESS
        // =====================================================================
        public ActionResult PhimTheo_Rap(long IDRap)
        {
            Rap_Chieu rapchieu = db.Rap_Chieu.SingleOrDefault(t => t.RapID == IDRap);
            if (rapchieu == null) return HttpNotFound();

            ViewBag.TenRap = rapchieu.TenRap;

            var phim = db.Lich_Chieu
                         .Where(lc => lc.Phong_Chieu.RapID == IDRap && lc.TrangThai == "Hoat Dong")
                         .Select(lc => lc.Phim)
                         .Distinct()
                         .ToList();

            var sessionUser = Session["USER_SESSION"] as UserLogin;
            CassandraFeaturesService.GhiNhatKyHoatDong(new NhatKyHoatDongDTO
            {
                Username        = sessionUser?.UserName,
                HanhDong        = "VIEW_CINEMA",
                KetQua          = "SUCCESS",
                ChiTiet         = "Xem phim theo rap: " + rapchieu.TenRap + " (RapID=" + IDRap + ")",
                ControllerName  = "Home",
                ActionName      = "PhimTheo_Rap",
                RequestMethod   = Request.HttpMethod,
                Browser         = Request.Browser.Browser + " " + Request.Browser.Version,
                Device          = Request.Browser.IsMobileDevice ? "Mobile" : "Desktop",
                HeDieuHanh      = Request.Browser.Platform,
                IpAddress       = Request.UserHostAddress
            });

            return View(phim);
        }

        // =====================================================================
        // GET: /Home/ChiTietPhim?id=
        // Log Cassandra: VIEW_MOVIE_DETAIL SUCCESS
        // =====================================================================
        public ActionResult ChiTietPhim(int id)
        {
            var phim = db.Phims
                         .Include(p => p.TheLoai)
                         .SingleOrDefault(p => p.PhimID == id);

            if (phim == null)
            {
                Response.StatusCode = 404;
                return View("Error");
            }

            var sessionUser = Session["USER_SESSION"] as UserLogin;
            CassandraFeaturesService.GhiNhatKyHoatDong(new NhatKyHoatDongDTO
            {
                Username        = sessionUser?.UserName,
                HanhDong        = "VIEW_MOVIE_DETAIL",
                KetQua          = "SUCCESS",
                ChiTiet         = "Xem chi tiet phim: " + phim.TenPhim + " (PhimID=" + phim.PhimID + ")",
                ControllerName  = "Home",
                ActionName      = "ChiTietPhim",
                RequestMethod   = Request.HttpMethod,
                Browser         = Request.Browser.Browser + " " + Request.Browser.Version,
                Device          = Request.Browser.IsMobileDevice ? "Mobile" : "Desktop",
                HeDieuHanh      = Request.Browser.Platform,
                IpAddress       = Request.UserHostAddress
            });

            return View(phim);
        }

        // =====================================================================
        // GET: /Home/TimKiemPhim  — hiển thị trang tìm kiếm
        // =====================================================================
        public ActionResult TimKiemPhim()
        {
            var tatCaPhim = db.Phims.ToList();
            return View(tatCaPhim);
        }

        // =====================================================================
        // POST: /Home/TimKiemPhim
        // Log Cassandra: SEARCH_MOVIE SUCCESS (tìm có kết quả)
        //                SEARCH_MOVIE FAILED  (không tìm thấy kết quả nào)
        // =====================================================================
        [HttpPost]
        public ActionResult TimKiemPhim(string tenphim)
        {
            var tatCaPhim = db.Phims.ToList();

            if (string.IsNullOrEmpty(tenphim))
            {
                ViewBag.Message = "Vui lòng nhập tên phim!";
                ViewBag.Result  = null;
                return View("TimKiemPhim", tatCaPhim);
            }

            var ketQua = db.Phims
                           .Where(p => p.TenPhim.Contains(tenphim))
                           .ToList();

            bool coKetQua = ketQua != null && ketQua.Count > 0;

            if (coKetQua)
            {
                ViewBag.Message = "Kết quả tìm kiếm cho: " + tenphim;
                ViewBag.Result  = ketQua;
            }
            else
            {
                ViewBag.Message = "Không tìm thấy phim nào có tên: " + tenphim;
                ViewBag.Result  = null;
            }

            var sessionUser = Session["USER_SESSION"] as UserLogin;

            // SEARCH_MOVIE SUCCESS khi có ít nhất 1 kết quả, FAILED khi 0 kết quả
            CassandraFeaturesService.GhiNhatKyHoatDong(new NhatKyHoatDongDTO
            {
                Username        = sessionUser?.UserName,
                HanhDong        = "SEARCH_MOVIE",
                KetQua          = coKetQua ? "SUCCESS" : "FAILED",
                ChiTiet         = "Tim kiem: \"" + tenphim + "\" - " + (ketQua?.Count ?? 0) + " ket qua",
                ControllerName  = "Home",
                ActionName      = "TimKiemPhim",
                RequestMethod   = Request.HttpMethod,
                Browser         = Request.Browser.Browser + " " + Request.Browser.Version,
                Device          = Request.Browser.IsMobileDevice ? "Mobile" : "Desktop",
                HeDieuHanh      = Request.Browser.Platform,
                IpAddress       = Request.UserHostAddress
            });

            return View("TimKiemPhim", tatCaPhim);
        }
    }
}
