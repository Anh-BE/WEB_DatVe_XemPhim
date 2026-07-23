using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web_Phim.Models;

namespace Web_Phim.Controllers
{
    public class Admin_QuanLyController : Controller
    {
        QuanLyPhimHQTCSDLEntities db = new QuanLyPhimHQTCSDLEntities();
        // GET: Admin_QuanLy
        public ActionResult TrangQuanLy()
        {
            return View();
        }
        public ActionResult TrangQuanLy_Phim()
        {
            return View();
        }
        public ActionResult ThemPhim()
        {
            ViewBag.TheLoaiList = new SelectList(db.TheLoais.ToList(), "MaTheLoai", "TenTheLoai");
            return View();
        }


        // =========================
        // POST: Thêm Phim
        // =========================
        [HttpPost]
        public ActionResult ThemPhim(Phim model, HttpPostedFileBase PosterFile)
        {
            ViewBag.TheLoaiList = new SelectList(db.TheLoais, "MaTheLoai", "TenTheLoai");

            if (!ModelState.IsValid)
                return View(model);

            // Xử lý poster upload
            string fileName = "no-poster.png";

            if (PosterFile != null && PosterFile.ContentLength > 0)
            {
                fileName = Path.GetFileName(PosterFile.FileName);
                string path = Path.Combine(Server.MapPath("~/Assets/Images"), fileName);
                PosterFile.SaveAs(path);
            }

            // Gọi Procedure để thêm phim
            db.Database.ExecuteSqlCommand(
            "EXEC sp_ThemPhim @TenPhim, @MoTa, @DaoDien, @MaTheLoai, @ThoiLuong, @Poster, @TrangThai, @PhanLoaiDoTuoi, @NgayKhoiChieu",

             new SqlParameter("@TenPhim", model.TenPhim),
             new SqlParameter("@MoTa", (object)model.MoTa ?? DBNull.Value),
             new SqlParameter("@DaoDien", (object)model.DaoDien ?? DBNull.Value),
             new SqlParameter("@MaTheLoai", model.MaTheLoai),
             new SqlParameter("@ThoiLuong", model.ThoiLuong),
             new SqlParameter("@Poster", fileName),
             new SqlParameter("@TrangThai", model.TrangThai ?? "Dang Chieu"),
             new SqlParameter("@PhanLoaiDoTuoi", model.PhanLoaiDoTuoi),

             new SqlParameter("@NgayKhoiChieu", SqlDbType.Date)
             {
                 Value = (object)model.NgayKhoiChieu ?? DBNull.Value
             }
 );


            TempData["Success"] = "Thêm phim thành công!";
            return RedirectToAction("TrangQuanLy_Phim", "Admin_QuanLy");
        }
        public ActionResult SuaPhim(long id)
        {
            var phim = db.Phims.Find(id);
            if (phim == null)
                return HttpNotFound();

            ViewBag.TheLoaiList = new SelectList(db.TheLoais, "MaTheLoai", "TenTheLoai", phim.MaTheLoai);
            return View(phim);
        }

        [HttpPost]
        public ActionResult SuaPhim(Phim model, HttpPostedFileBase PosterFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.TheLoaiList = new SelectList(db.TheLoais.ToList(), "MaTheLoai", "TenTheLoai");
                return View(model);
            }

            // 1. Lấy poster cũ từ DB
            var phimCu = db.Phims.Find(model.PhimID);
            if (phimCu == null)
            {
                return HttpNotFound();
            }

            // 2. Xử lý poster mới (nếu có)
            string posterFileName = phimCu.Poster; // mặc định giữ poster cũ

            if (PosterFile != null && PosterFile.ContentLength > 0)
            {
                posterFileName = Path.GetFileName(PosterFile.FileName);
                string path = Path.Combine(Server.MapPath("~/Assets/Images"), posterFileName);
                PosterFile.SaveAs(path);
            }

            // 3. Gọi PROCEDURE sửa phim
            db.Database.ExecuteSqlCommand(
                "EXEC sp_SuaPhim @PhimID, @TenPhim, @MoTa, @DaoDien, @NgayKhoiChieu, @ThoiLuong, @PhanLoaiDoTuoi, @Poster, @MaTheLoai",
                new SqlParameter("@PhimID", model.PhimID),
                new SqlParameter("@TenPhim", model.TenPhim),
                new SqlParameter("@MoTa", (object)model.MoTa ?? DBNull.Value),
                new SqlParameter("@DaoDien", (object)model.DaoDien ?? DBNull.Value),
                new SqlParameter("@NgayKhoiChieu", model.NgayKhoiChieu),
                new SqlParameter("@ThoiLuong", model.ThoiLuong),
                new SqlParameter("@PhanLoaiDoTuoi", model.PhanLoaiDoTuoi),
                new SqlParameter("@Poster", posterFileName), // 🔥 BẮT BUỘC PHẢI TRUYỀN
               
                new SqlParameter("@MaTheLoai", model.MaTheLoai)
            );

            TempData["Success"] = "Sửa phim thành công!";
            return RedirectToAction("DanhSachPhim_Admin", "Admin_QuanLy");
        }
        public ActionResult XoaPhim(long id)
        {
            try
            {
                db.Database.ExecuteSqlCommand(
                    "EXEC sp_XoaMemPhim_Transaction @PhimID",
                    new SqlParameter("@PhimID", id)
                );

                TempData["Success"] = "Đã ngừng chiếu phim !";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
            }

            return RedirectToAction("DanhSachPhim_Admin");
        }
        public JsonResult GetNgayKhoiChieu(long phimId)
        {
            var ngay = db.Phims
                .Where(p => p.PhimID == phimId)
                .Select(p => p.NgayKhoiChieu)
                .FirstOrDefault();

            if (ngay == null)
                return Json(null, JsonRequestBehavior.AllowGet);

            return Json(ngay.Value.ToString("yyyy-MM-ddTHH:mm"),
                JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetLichChieuTheoPhong(long phongId)
        {
            var ds = db.Lich_Chieu
                .Where(lc => lc.PhongID == phongId)
                .OrderBy(lc => lc.ThoiGianBatDau)
                .ToList()
                .Select(lc => new
                {
                    Phim = lc.Phim.TenPhim,
                    BatDau = lc.ThoiGianBatDau.ToString("yyyy-MM-dd HH:mm"),
                    KetThuc = lc.ThoiGianBatDau
                        .AddMinutes(lc.Phim.ThoiLuong + 15)
                        .ToString("yyyy-MM-dd HH:mm"),
                    DinhDang = lc.DinhDang,
                    GiaVe = lc.GiaVe
                });

            return Json(ds, JsonRequestBehavior.AllowGet);
        }


        private void LoadDropDown_LichChieu()
        {
            // Phim đang chiếu
            ViewBag.PhimList = new SelectList(
                db.Phims.Where(p => p.TrangThai == "Dang Chieu")
                        .OrderBy(p => p.TenPhim)
                        .ToList(),
                "PhimID", "TenPhim"
            );

            // Phòng chiếu + tên rạp
            ViewBag.PhongList = new SelectList(
                db.Phong_Chieu
                  .Select(pc => new {
                      pc.PhongID,
                      TenPhong = pc.TenPhong + " - " + pc.Rap_Chieu.TenRap
                  }).ToList(),
                "PhongID", "TenPhong"
            );

            // Định dạng
            ViewBag.DinhDangList = new SelectList(new[] {
         new SelectListItem { Value = "2D", Text = "2D" },
         new SelectListItem { Value = "3D", Text = "3D" }
    }, "Value", "Text");
        }


        // ==========================
        // GET: Thêm Lịch Chiếu
        // ==========================
        [HttpGet]
        public ActionResult ThemLichChieu()
        {
            LoadDropDown_LichChieu();
            return View();
        }
        public JsonResult GetLichChieuTheoPhim(long phimId)
        {
            var ds = db.Lich_Chieu
                .Where(lc => lc.PhimID == phimId)
                .OrderBy(lc => lc.ThoiGianBatDau)
                .ToList()
                .Select(lc => new
                {
                    Phong = lc.Phong_Chieu.TenPhong + " - " + lc.Phong_Chieu.Rap_Chieu.TenRap,
                    ThoiGian = lc.ThoiGianBatDau.ToString("yyyy-MM-dd HH:mm"),  // 🔥 FIX CHUẨN
                    DinhDang = lc.DinhDang,
                    GiaVe = lc.GiaVe
                });

            return Json(ds, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemLichChieu(Lich_Chieu model)
        {
            LoadDropDown_LichChieu();

            var phim = db.Phims.Find(model.PhimID);
            if (phim == null)
            {
                ModelState.AddModelError("", "Không tìm thấy phim.");
                return View(model);
            }

            // Không được chọn trước ngày khởi chiếu
            if (model.ThoiGianBatDau < phim.NgayKhoiChieu)
            {
                ModelState.AddModelError("ThoiGianBatDau",
                    "Thời gian bắt đầu phải từ ngày khởi chiếu (" +
                    phim.NgayKhoiChieu.Value.ToString("dd/MM/yyyy HH:mm") + ") trở đi.");
                return View(model);
            }

            // GỌI PROC 1 LẦN DUY NHẤT – TRẢ KẾT QUẢ CHO MVC
            var result = db.Database.SqlQuery<ProcResult>(
                "EXEC sp_ThemLichChieu_Transaction @PhimID, @PhongID, @ThoiGianBatDau, @GiaVe, @DinhDang",
                new SqlParameter("@PhimID", model.PhimID),
                new SqlParameter("@PhongID", model.PhongID),
                new SqlParameter("@ThoiGianBatDau", model.ThoiGianBatDau),
                new SqlParameter("@GiaVe", model.GiaVe),
                new SqlParameter("@DinhDang", model.DinhDang ?? "2D")
            ).FirstOrDefault();

            // Nếu lỗi → trả về VIEW
            if (result.Result == 0)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            // Thành công → redirect
            TempData["Success"] = result.Message;
            return RedirectToAction("DanhSachPhim_Admin");
        }







        public ActionResult DanhSachPhim_Admin()
        {   
            List<Phim> dsphim = db.Phims
            
            .Include("Lich_Chieu")
            .Where(p => p.TrangThai == "Dang Chieu")
            .OrderByDescending(p => p.NgayKhoiChieu)
            .ToList();

            return View(dsphim);
        }



    }

}

