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
            ViewBag.HideHeader = false;
            List<Phim> dsphim = db.Phims.Include(p => p.TheLoai).Where(p => p.TrangThai == "Dang Chieu").ToList();
            try
            {
                var neo4jService = new Neo4jService();
                var userSession = Session["USER_SESSION"] as UserLogin;
                string username = userSession != null ? userSession.UserName : "";

                var topBooked = neo4jService.GetRecommendedMovies(username, 4);
                if (topBooked != null && topBooked.Count > 0)
                {
                    var syncedList = new List<Neo4jMovieViewModel>();
                    foreach (var item in topBooked)
                    {
                        var sqlPhim = db.Phims.Include(p => p.TheLoai).FirstOrDefault(p => p.PhimID == item.MovieId);
                        if (sqlPhim != null)
                        {
                            item.Title = sqlPhim.TenPhim;
                            if (!string.IsNullOrEmpty(sqlPhim.Poster))
                            {
                                item.Poster = sqlPhim.Poster;
                            }
                            if (sqlPhim.TheLoai != null)
                            {
                                item.GenreName = sqlPhim.TheLoai.TenTheLoai;
                            }
                        }
                        if (!syncedList.Any(x => x.MovieId == item.MovieId))
                        {
                            syncedList.Add(item);
                        }
                    }

                    // Bổ sung cho đủ 4 phim nếu chưa đủ 4 phim (Đảm bảo không trùng lặp)
                    while (syncedList.Count < 4)
                    {
                        var p = dsphim.FirstOrDefault(x => !syncedList.Any(t => t.MovieId == x.PhimID));
                        if (p == null) break;

                        syncedList.Add(new Neo4jMovieViewModel
                        {
                            MovieId = (int)p.PhimID,
                            Title = p.TenPhim,
                            Poster = p.Poster,
                            Duration = p.ThoiLuong ?? 120,
                            BookingCount = 1,
                            FavoriteCount = 1,
                            GenreName = p.TheLoai != null ? p.TheLoai.TenTheLoai : "Tổng Hợp",
                            IsFavorite = false
                        });
                    }

                    ViewBag.TopBookedNeo4j = syncedList.Take(4).ToList();
                }
                else
                {
                    ViewBag.TopBookedNeo4j = dsphim.Take(4).Select((p, idx) => new Neo4jMovieViewModel
                    {
                        MovieId = (int)p.PhimID,
                        Title = p.TenPhim,
                        Poster = p.Poster,
                        Duration = p.ThoiLuong ?? 120,
                        BookingCount = Math.Max(1, 5 - idx),
                        FavoriteCount = Math.Max(1, 4 - idx),
                        GenreName = p.TheLoai != null ? p.TheLoai.TenTheLoai : "Tổng Hợp",
                        IsFavorite = false
                    }).ToList();
                }
            }
            catch
            {
                ViewBag.TopBookedNeo4j = dsphim.Take(4).Select((p, idx) => new Neo4jMovieViewModel
                {
                    MovieId = (int)p.PhimID,
                    Title = p.TenPhim,
                    Poster = p.Poster,
                    Duration = p.ThoiLuong ?? 120,
                    BookingCount = Math.Max(1, 5 - idx),
                    FavoriteCount = Math.Max(1, 4 - idx),
                    GenreName = p.TheLoai != null ? p.TheLoai.TenTheLoai : "Tổng Hợp",
                    IsFavorite = false
                }).ToList();
            }
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
                          .Include(p => p.TheLoai)
                          .Include(p => p.Lich_Chieu)
                          .Where(p => p.TrangThai == "Dang Chieu" )
                          .OrderByDescending(t => t.NgayKhoiChieu)
                          .ToList();

            try
            {
                var neo4jService = new Neo4jService();
                var userSession = Session["USER_SESSION"] as UserLogin;
                string username = userSession != null ? userSession.UserName : "";

                var topBooked = neo4jService.GetRecommendedMovies(username, 4);
                var topFavorites = neo4jService.GetTopFavoriteMovies(4, username);

                if (topBooked != null && topBooked.Count > 0)
                {
                    var syncedList = new List<Neo4jMovieViewModel>();
                    foreach (var item in topBooked)
                    {
                        var sqlPhim = dsphim.FirstOrDefault(p => p.PhimID == item.MovieId);
                        if (sqlPhim != null)
                        {
                            item.Title = sqlPhim.TenPhim;
                            if (!string.IsNullOrEmpty(sqlPhim.Poster))
                            {
                                item.Poster = sqlPhim.Poster;
                            }
                            if (sqlPhim.TheLoai != null)
                            {
                                item.GenreName = sqlPhim.TheLoai.TenTheLoai;
                            }
                        }
                        if (!syncedList.Any(x => x.MovieId == item.MovieId))
                        {
                            syncedList.Add(item);
                        }
                    }

                    while (syncedList.Count < 4)
                    {
                        var p = dsphim.FirstOrDefault(x => !syncedList.Any(t => t.MovieId == x.PhimID));
                        if (p == null) break;

                        syncedList.Add(new Neo4jMovieViewModel
                        {
                            MovieId = (int)p.PhimID,
                            Title = p.TenPhim,
                            Poster = p.Poster,
                            Duration = p.ThoiLuong ?? 120,
                            BookingCount = 1,
                            FavoriteCount = 1,
                            GenreName = p.TheLoai != null ? p.TheLoai.TenTheLoai : "Tổng Hợp",
                            IsFavorite = false
                        });
                    }

                    topBooked = syncedList.Take(4).ToList();
                }

                if (topBooked == null || topBooked.Count == 0)
                {
                    topBooked = dsphim.Take(4).Select((p, idx) => new Neo4jMovieViewModel
                    {
                        MovieId = (int)p.PhimID,
                        Title = p.TenPhim,
                        Poster = p.Poster,
                        Duration = p.ThoiLuong ?? 120,
                        BookingCount = Math.Max(1, 5 - idx),
                        FavoriteCount = Math.Max(1, 4 - idx),
                        GenreName = p.TheLoai != null ? p.TheLoai.TenTheLoai : "Tổng Hợp",
                        IsFavorite = false
                    }).ToList();
                }

                ViewBag.TopBookedNeo4j    = topBooked;
                ViewBag.TopFavoritesNeo4j = topFavorites;
            }
            catch 
            {
                ViewBag.TopBookedNeo4j = dsphim.Take(4).Select((p, idx) => new Neo4jMovieViewModel
                {
                    MovieId = (int)p.PhimID,
                    Title = p.TenPhim,
                    Poster = p.Poster,
                    Duration = p.ThoiLuong ?? 120,
                    BookingCount = 28 - (idx * 6),
                    FavoriteCount = 15 - (idx * 3),
                    GenreName = p.TheLoai != null ? p.TheLoai.TenTheLoai : "Tổng Hợp",
                    IsFavorite = false
                }).ToList();
            }

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
