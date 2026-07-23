using Web_Phim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Web_Phim.Controllers
{
    public class HomeController : Controller
    {
        QuanLyPhimHQTCSDLEntities db = new QuanLyPhimHQTCSDLEntities();
        public ActionResult TrangChu()
        {
            var dsPhimViewModel = db.Phims
                .Where(p => p.TrangThai == "Dang Chieu")
                .OrderByDescending(t => t.NgayKhoiChieu)
                .Select(p => new PhimViewModel // p chính là Phim
                {
                    // 2. Gán các giá trị từ Phim
                    PhimID = p.PhimID,
                    TenPhim = p.TenPhim,
                    Poster = p.Poster,
                    ThoiLuong = p.ThoiLuong,
                    NgayKhoiChieu = p.NgayKhoiChieu,

                    // 3. Tính toán giá vé thấp nhất
                    //    p.Lich_Chieus là một danh sách các suất chiếu của phim đó
                    GiaVeThapNhat = p.Lich_Chieu.Min(lc => (decimal?)lc.GiaVe) ?? 0m
                })
                .ToList();
            return View(dsPhimViewModel);
        }
        public ActionResult PhimTheoTheLoai(int MATHELOAI)
        {
            TheLoai theloai = db.TheLoais.SingleOrDefault(t => t.MaTheLoai == MATHELOAI);
            if (theloai == null)
                return HttpNotFound();

            // 2. Lấy danh sách phim và TÍNH TOÁN GIÁ VÉ (dùng ViewModel)
            var dsPhimViewModel = db.Phims
                .Where(p => p.TrangThai == "Dang Chieu")
                .Where(p => p.MaTheLoai == MATHELOAI) // Lọc theo thể loại
                .Select(p => new PhimViewModel // Chuyển đổi sang ViewModel
                {
                    PhimID = p.PhimID,
                    TenPhim = p.TenPhim,
                    Poster = p.Poster,
                    ThoiLuong = p.ThoiLuong,
                    NgayKhoiChieu = p.NgayKhoiChieu,
                    GiaVeThapNhat = p.Lich_Chieu.Min(lc => (decimal?)lc.GiaVe) ?? 0m
                })
                .OrderByDescending(p => p.NgayKhoiChieu)
                .ToList();
            // 3. Gửi tên thể loại sang View
            ViewBag.TenTheLoai = theloai.TenTheLoai;
            return View("TrangChu", dsPhimViewModel);
        }
        [ChildActionOnly] // Đánh dấu đây là Action chỉ được gọi từ View
        public ActionResult _TheLoaiDropdown()
        {
            var dsTheLoai = db.TheLoais.OrderBy(t => t.TenTheLoai).ToList();
            return PartialView("_TheLoaiDropdown", dsTheLoai);
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";
            return View();
        }
        public ActionResult ChiTietPhim(int id)
        {
            if (id <= 0)
                return HttpNotFound();

            var phim = db.Phims
                         .Include("TheLoai")    // Dùng chuỗi để tránh lỗi Include null
                         .SingleOrDefault(p => p.PhimID == id);

            if (phim == null)
                return HttpNotFound();

            return View(phim);
        }




    }
}