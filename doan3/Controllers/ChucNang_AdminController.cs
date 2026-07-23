using doan3.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace doan3.Controllers
{
    public class ChucNang_AdminController : Controller
    {
        LTW_DatVeXemPhimEntities db = new LTW_DatVeXemPhimEntities();
        // GET: /ChucNang_Admin/
        public ActionResult QuanLyAdmin()
        {


            return View();
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
        public ActionResult ThemPhim()
        {
            // Dropdown thể loại phim
            ViewBag.MaTheLoai = new SelectList(
                db.TheLoais
                  .OrderBy(t => t.TenTheLoai)
                  .ToList(),
                "MaTheLoai",
                "TenTheLoai"
            );

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemPhim(
           [Bind(Include = "TenPhim,MoTa,NgayKhoiChieu,ThoiLuong,DaoDien,PhanLoaiDoTuoi,TrangThai,MaTheLoai")]
    Phim phim,
           HttpPostedFileBase fileUpload
       )
        {
            ViewBag.MaTheLoai = new SelectList(
                db.TheLoais.OrderBy(t => t.TenTheLoai),
                "MaTheLoai",
                "TenTheLoai",
                phim.MaTheLoai
            );

            if (!ModelState.IsValid)
                return View(phim);

            string fileName = "no-image.png";

            if (fileUpload != null && fileUpload.ContentLength > 0)
            {
                fileName = Path.GetFileName(fileUpload.FileName);

                string folderPath = Server.MapPath("~/Content/HinhAnhSP/");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string path = Path.Combine(folderPath, fileName);
                fileUpload.SaveAs(path);
            }

        
            phim.Poster = fileName;

            if (string.IsNullOrEmpty(phim.TrangThai))
                phim.TrangThai = "Dang Chieu";

            db.Phims.Add(phim);
            db.SaveChanges();

            return RedirectToAction("DanhSachPhim_Admin");
        }
        
        public ActionResult Edit(long? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var phim = db.Phims.Find(id);
            if (phim == null)
                return HttpNotFound();

           
            ViewBag.MaTheLoai = new SelectList(
                db.TheLoais.OrderBy(t => t.TenTheLoai),
                "MaTheLoai",
                "TenTheLoai",
                phim.MaTheLoai
            );

            
            ViewBag.TrangThai = new SelectList(
                new[]
                {
            new { Value = "Dang Chieu", Text = "Đang chiếu" },
            new { Value = "Ngung Chieu", Text = "Ngừng chiếu" }
                },
                "Value",
                "Text",
                phim.TrangThai
            );

            return View(phim);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            long PhimID,
            string TenPhim,
            string MoTa,
            DateTime? NgayKhoiChieu,
            int? ThoiLuong,
            string DaoDien,
            string PhanLoaiDoTuoi,
            string TrangThai,
            int? MaTheLoai,
            HttpPostedFileBase PosterFile)
        {
            var phim = db.Phims.Find(PhimID);
            if (phim == null)
            {
                return HttpNotFound();
            }

            // ràng buộc ngày không được quá khứ
            if (NgayKhoiChieu.HasValue && NgayKhoiChieu.Value.Date < DateTime.Today)
            {
                ModelState.AddModelError("NgayKhoiChieu", "Ngày khởi chiếu không được nhỏ hơn hôm nay");
            }

            if (ModelState.IsValid)
            {
                phim.TenPhim = TenPhim;
                phim.MoTa = MoTa;
                phim.NgayKhoiChieu = NgayKhoiChieu;
                phim.ThoiLuong = ThoiLuong;
                phim.DaoDien = DaoDien;
                phim.PhanLoaiDoTuoi = PhanLoaiDoTuoi;
                phim.TrangThai = TrangThai;
                phim.MaTheLoai = MaTheLoai;

                if (PosterFile != null && PosterFile.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(PosterFile.FileName);
                    string path = Path.Combine(Server.MapPath("~/Content/HinhAnhSP"), fileName);
                    PosterFile.SaveAs(path);
                    phim.Poster = fileName;
                }

                db.SaveChanges();
                return RedirectToAction("DanhSachPhim_Admin");
            }

            ViewBag.MaTheLoai = new SelectList(
                db.TheLoais.OrderBy(t => t.TenTheLoai),
                "MaTheLoai",
                "TenTheLoai",
                phim.MaTheLoai
            );

            ViewBag.TrangThai = new SelectList(
                new[]
                {
            new { Value = "Dang Chieu", Text = "Đang chiếu" },
            new { Value = "Ngung Chieu", Text = "Ngừng chiếu" }
                },
                "Value",
                "Text",
                phim.TrangThai
            );

            return View(phim);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Xoa(int id)
        {
            var phim = db.Phims.Find(id);
            if (phim == null)
                return HttpNotFound();

            phim.TrangThai = "Ngung Chieu";
            db.SaveChanges();

            return RedirectToAction("DanhSachPhim_Admin");
        }

        public ActionResult ThemLichChieu(long? phimId)
        {
            var vm = new ThemLichChieuViewModel
            {
                DanhSachPhim = db.Phims.ToList(),
                DanhSachRap = db.Rap_Chieu.ToList(),
                DanhSachPhong = new List<Phong_Chieu>(),
                LichChieuHienTai = new List<Lich_Chieu>(),
                PhimID = phimId // Giữ lại ID phim khi load lại trang
            };

            if (phimId.HasValue)
            {
                LoadFormData(vm);
            }

            return View(vm);
        }
        [HttpPost]
       
        public ActionResult XemLichChieu(ThemLichChieuViewModel vm)
        {
            if (vm.PhimID.HasValue)
            {
                // Chuyển hướng sang GET để đồng bộ tham số trên URL
                return RedirectToAction("ThemLichChieu", new { phimId = vm.PhimID });
            }
            return RedirectToAction("ThemLichChieu");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemLichChieuPost(ThemLichChieuViewModel vm)
        {
            
            LoadFormData(vm);

            
            if (!vm.PhimID.HasValue || !vm.RapID.HasValue || !vm.PhongID.HasValue)
            {
                ModelState.AddModelError("", "Vui lòng chọn đầy đủ Phim, Rạp và Phòng.");
                return View("ThemLichChieu", vm);
            }

            if (!vm.ThoiGianBatDau.HasValue || vm.ThoiGianBatDau < DateTime.Now)
            {
                ModelState.AddModelError("", "Thời gian chiếu phải lớn hơn thời gian hiện tại.");
                return View("ThemLichChieu", vm);
            }

            if (KiemTraTrungLich(vm))
            {
                ModelState.AddModelError("", "Phòng này đã có lịch chiếu khác trùng thời gian.");
                return View("ThemLichChieu", vm);
            }

            try
            {
                var lichMoi = new Lich_Chieu
                {
                    PhimID = vm.PhimID.Value,
                    PhongID = vm.PhongID.Value,
                    ThoiGianBatDau = vm.ThoiGianBatDau.Value,
                    DinhDang = vm.DinhDang,
                    TrangThai = "Hoat Dong" 
                };

                db.Lich_Chieu.Add(lichMoi);
                db.SaveChanges();

                TempData["Success"] = "Thêm lịch chiếu thành công!";
                return RedirectToAction("ThemLichChieu", new { phimId = vm.PhimID });
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                
                foreach (var error in ex.EntityValidationErrors)
                {
                    foreach (var ve in error.ValidationErrors)
                    {
                        ModelState.AddModelError("", $"Lỗi cột '{ve.PropertyName}': {ve.ErrorMessage}");
                    }
                }
                return View("ThemLichChieu", vm);
            }
        }
        public JsonResult GetPhongTheoRap(long rapId)
        {
            var data = db.Phong_Chieu
                .Where(p => p.RapID == rapId)
                .Select(p => new { p.PhongID, p.TenPhong })
                .ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        private void LoadFormData(ThemLichChieuViewModel vm)
        {
            vm.DanhSachPhim = db.Phims.ToList();
            vm.DanhSachRap = db.Rap_Chieu.ToList();

            if (vm.PhimID.HasValue)
            {
                vm.LichChieuHienTai = db.Lich_Chieu
                    .Where(lc => lc.PhimID == vm.PhimID && lc.TrangThai == "Hoat Dong")
                    .OrderBy(lc => lc.ThoiGianBatDau)
                    .ToList();
            }
        }

        private bool KiemTraTrungLich(ThemLichChieuViewModel vm)
        {
            var phim = db.Phims.Find(vm.PhimID.Value);
            DateTime batDauMoi = vm.ThoiGianBatDau.Value;
            DateTime ketThucMoi = batDauMoi.AddMinutes((phim.ThoiLuong ?? 0) + 15);

            return db.Lich_Chieu.Any(lc =>
                lc.PhongID == vm.PhongID &&
                lc.TrangThai == "Hoat Dong" &&
                batDauMoi < DbFunctions.AddMinutes(lc.ThoiGianBatDau, (db.Phims.Where(p => p.PhimID == lc.PhimID).Select(p => p.ThoiLuong).FirstOrDefault() ?? 0) + 15) &&
                ketThucMoi > lc.ThoiGianBatDau
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XoaLichChieu(long lichChieuId, long phimId)
        {
            var lich = db.Lich_Chieu.Find(lichChieuId);

            if (lich == null)
            {
                TempData["Error"] = "Lịch chiếu không tồn tại";
                return RedirectToAction("ThemLichChieu");
            }

            lich.TrangThai = "Da Xoa";
            db.SaveChanges();

            TempData["Success"] = "Đã xóa lịch chiếu";

            return RedirectToAction("ThemLichChieu", new { phimId = phimId });
        }

    }
}
