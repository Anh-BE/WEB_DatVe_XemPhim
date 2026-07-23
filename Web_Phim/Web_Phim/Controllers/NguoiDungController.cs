using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web_Phim.Models;
using System.Data.Entity;

namespace Web_Phim.Controllers
{
    public class NguoiDungController : Controller
    {
        private QuanLyPhimHQTCSDLEntities db = new QuanLyPhimHQTCSDLEntities();

        // ==========================================
        // 1. GET: Hiển thị trang Thông tin cá nhân
        // ==========================================
        public ActionResult ThongTinNguoiDung()
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Account");

            int userId = Convert.ToInt32(Session["UserID"]);

            var khachHang = db.Khach_Hang.Include("User_")
                                         .FirstOrDefault(k => k.UserID == userId);

            if (khachHang == null)
            {
                TempData["Error"] = "Lỗi: Tài khoản không có hồ sơ khách hàng.";
                return RedirectToAction("TrangChu", "Home");
            }

            var model = new PersonalInfoViewModel
            {
                UserID = userId,
                KhachHangID = khachHang.KhachHangID,
                UserName = khachHang.User_.UserName,
                DiemThanhVien = khachHang.DiemThanhVien ?? 0,

                // Thông tin cá nhân
                TenDayDu = khachHang.TenDayDu,
                Email = khachHang.Email,
                SoDienThoai = khachHang.SoDienThoai,

                // Các trường mật khẩu để trống khi mới load trang
                MatKhauCu = null,
                MatKhauMoi = null,
                XacNhanMatKhau = null
            };

            return View(model);
        }

        // ==========================================
        // 2. POST: Chỉ Cập nhật Thông tin cá nhân (Tên, Email, SĐT)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CapNhatThongTin(PersonalInfoViewModel model)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Account");

            // Bỏ qua validate các trường mật khẩu vì form này không gửi mật khẩu
            if (ModelState.IsValid)
            {
                try
                {
                    int userId = Convert.ToInt32(Session["UserID"]);

                    var khachHang = db.Khach_Hang.FirstOrDefault(k => k.UserID == userId);
                    if (khachHang != null)
                    {
                        khachHang.TenDayDu = model.TenDayDu;
                        khachHang.Email = model.Email;
                        khachHang.SoDienThoai = model.SoDienThoai;

                        db.SaveChanges(); // Lưu xuống DB

                        Session["HoTen"] = model.TenDayDu; // Cập nhật Session
                        TempData["Success"] = "Cập nhật hồ sơ thành công!";
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Lỗi cập nhật: " + ex.Message;
                }
            }
            else
            {
                TempData["Error"] = "Dữ liệu nhập vào không hợp lệ.";
            }

            return RedirectToAction("ThongTinNguoiDung");
        }

        // ==========================================
        // 3. POST: Hàm MỚI chuyên xử lý Đổi Mật Khẩu (Từ Modal)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DoiMatKhau(int UserID, string MatKhauCu, string MatKhauMoi, string XacNhanMatKhau)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Account");

            // 1. Kiểm tra xác nhận mật khẩu
            if (MatKhauMoi != XacNhanMatKhau)
            {
                TempData["Error"] = "Mật khẩu xác nhận không khớp!";
                return RedirectToAction("ThongTinNguoiDung");
            }

            try
            {
                // 2. Gọi Stored Procedure (3 tham số: ID, Cũ, Mới)
                // Procedure này sẽ tự kiểm tra Mật khẩu cũ có đúng không
                db.sp_DoiMatKhauCaNhann(UserID, MatKhauCu, MatKhauMoi);

                TempData["Success"] = "Đổi mật khẩu thành công!";
            }
            catch (Exception ex)
            {
                // Bắt lỗi từ SQL (ví dụ: Mật khẩu cũ sai, Mật khẩu mới quá ngắn...)
                var sqlError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                TempData["Error"] = sqlError;
            }

            return RedirectToAction("ThongTinNguoiDung");
        }



    }
}