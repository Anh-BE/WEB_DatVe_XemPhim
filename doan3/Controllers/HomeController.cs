using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using doan3.Models;
using System.Data.Entity;

namespace doan3.Controllers
{
    public class HomeController : Controller
    {
        LTW_DatVeXemPhimEntities db = new LTW_DatVeXemPhimEntities();

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
