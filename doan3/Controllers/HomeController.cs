using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using doan3.Models;
using System.Data.Entity;
using doan3.Models.Cassandra;

namespace doan3.Controllers
{
    public class HomeController : Controller
    {
        LTW_DatVeXemPhimEntities db = new LTW_DatVeXemPhimEntities();

        public ActionResult Index()
        {
            var rs = CassandraService.Session.Execute("SELECT now() FROM system.local");

            return View();
        }

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

        public ActionResult GioiThieu()
        {
            ViewBag.Message = "Câu chuyện về Movana Cinema";
            return View();
        }
        public ActionResult ChinhSachTichDiem()
        {
            return View();
        }
        public ActionResult PhimDangChieu()
        {
            List<Phim> dsphim = db.Phims
                          .Include(p => p.Lich_Chieu)
                          .Where(p => p.TrangThai == "Dang Chieu" )
                          .OrderByDescending(t => t.NgayKhoiChieu)
                          .ToList();

            // TÍNH NĂNG NEO4J: LOAD BẢNG XẾP HẠNG TOP PHIM TRỰC TIẾP TRÊN TRANG CHỦ (CACHE TỐI ƯU 5 PHÚT)
            try
            {
                var topBooked = HttpRuntime.Cache["TopBookedNeo4j"] as List<Neo4jMovieViewModel>;
                var topFavorites = HttpRuntime.Cache["TopFavoritesNeo4j"] as List<Neo4jMovieViewModel>;

                if (topBooked == null || topFavorites == null)
                {
                    var neo4jService = new Neo4jService();
                    neo4jService.SeedInitialData(db);
                    var userSession = Session["USER_SESSION"] as UserLogin;
                    string username = userSession != null ? userSession.UserName : "";

                    topBooked = neo4jService.GetTopBookedMovies(4, username);
                    topFavorites = neo4jService.GetTopFavoriteMovies(4, username);

                    // Đồng bộ 100% Tên phim và File ảnh Poster từ SQL Server
                    for (int i = 0; i < topBooked.Count; i++)
                    {
                        var item = topBooked[i];
                        var sqlPhim = dsphim.FirstOrDefault(p => p.PhimID == item.MovieId) 
                                     ?? (i < dsphim.Count ? dsphim[i] : null);

                        if (sqlPhim != null)
                        {
                            item.Title = sqlPhim.TenPhim;
                            item.Poster = sqlPhim.Poster; // VD: 001.png, 002.png...
                        }
                    }

                    HttpRuntime.Cache.Insert("TopBookedNeo4j", topBooked, null, DateTime.Now.AddMinutes(5), System.Web.Caching.Cache.NoSlidingExpiration);
                    HttpRuntime.Cache.Insert("TopFavoritesNeo4j", topFavorites, null, DateTime.Now.AddMinutes(5), System.Web.Caching.Cache.NoSlidingExpiration);
                }

                ViewBag.TopBookedNeo4j = topBooked;
                ViewBag.TopFavoritesNeo4j = topFavorites;
            }
            catch { }

            return View(dsphim);
        }
        public ActionResult PhimTheoTheLoai(int MATHELOAI)
        {
            
            TheLoai theloai = db.TheLoais
                .SingleOrDefault(t => t.MaTheLoai == MATHELOAI);
            if (theloai == null)    
                return HttpNotFound();

           
            List<Phim> dsPhim = db.Phims
                                 .Where(p => p.MaTheLoai == MATHELOAI)
                                 .Where(p => p.TrangThai == "Dang Chieu")
                                 .OrderBy(p => p.ThoiLuong)
                                 .ToList();

            
            ViewBag.TenTheLoai = theloai.TenTheLoai;

            return View(dsPhim);
        }
        public ActionResult PhimTheo_Rap(long IDRap)
        {
            
            Rap_Chieu rapchieu = db.Rap_Chieu.SingleOrDefault(t => t.RapID == IDRap);

            if (rapchieu == null)
            {
                return HttpNotFound();
            }

            
            ViewBag.TenRap = rapchieu.TenRap;

         
            var phim = db.Lich_Chieu
                         .Where(lc => lc.Phong_Chieu.RapID == IDRap && lc.TrangThai == "Hoat Dong")
                         .Select(lc => lc.Phim)
                         .Distinct()
                         .ToList();

            return View(phim);
        }
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

            return View(phim);
        }

        public ActionResult TimKiemPhim()
        {
          
            var tatCaPhim = db.Phims.ToList();
            return View(tatCaPhim);
        }

        [HttpPost]
        public ActionResult TimKiemPhim(string tenphim)
        {
            
            var tatCaPhim = db.Phims.ToList();

            if (string.IsNullOrEmpty(tenphim))
            {
                ViewBag.Message = "Vui lòng nhập tên phim!";
                ViewBag.Result = null;
                var sessionUser = Session["USER_SESSION"] as UserLogin;

                if (sessionUser != null)
                {
                    CassandraService.LogUserActivity(
                        sessionUser.UserID,
                        "SEARCH",
                        null,
                        null,
                        null,
                        "Tìm kiếm phim: " + tenphim,
                        Request.UserHostAddress,
                        Request.UserAgent
                    );
                }
                return View("TimKiemPhim", tatCaPhim);
            }

            
            var ketQua = db.Phims
                           .Where(p => p.TenPhim.Contains(tenphim))
                           .ToList();

            
            if (ketQua == null || ketQua.Count == 0)
            {
                ViewBag.Message = "Không tìm thấy phim nào có tên: " + tenphim;
                ViewBag.Result = null; 
            }
            else
            {
                ViewBag.Message = "Kết quả tìm kiếm cho: " + tenphim;
                ViewBag.Result = ketQua;
            }

         
            return View("TimKiemPhim", tatCaPhim);
        }










    }
}
