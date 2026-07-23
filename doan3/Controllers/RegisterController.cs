using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using doan3.Models;

namespace doan3.Controllers
{
    public class RegisterController : Controller
    {
        private LTW_DatVeXemPhimEntities db = new LTW_DatVeXemPhimEntities();
        private const int CustomerGroupID = 2; // Nhóm Khách hàng

        // ==========================================================
        // 1. ACTION: HIỂN THỊ TRANG ĐĂNG KÝ
        // ==========================================================
        public ActionResult Index_DangKy()
        {
            return View();
        }

        // ==========================================================
        // 2. ACTION: XỬ LÝ ĐĂNG KÝ
        // ==========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangKy(string username, string password, string name, string email, string phone, DateTime? ngaysinh)
        {
            
            string loiKiemTra = KiemTraDuLieuDauVao(username, email, phone);
            if (!string.IsNullOrEmpty(loiKiemTra))
            {
                ViewBag.Error = loiKiemTra;
                return View("Index_DangKy");
            }

            
            try
            {
      
                int newUserId = TaoTaiKhoanNguoiDung(username, password, name);

                
                TaoThongTinKhachHang(newUserId, name, email, phone, ngaysinh);

                TempData["SuccessMessage"] = "Đăng ký tài khoản thành công! Mời bạn đăng nhập.";
                return RedirectToAction("Index_DangNhap", "Login");
            }
            catch (Exception)
            {
              
                ViewBag.Error = "Đã xảy ra lỗi hệ thống trong quá trình đăng ký. Vui lòng thử lại sau.";
                return View("Index_DangKy");
            }
        }

        // ==========================================================
        // 3. ACTION: AJAX CHECK USERNAME (Dùng cho Client check nhanh)
        // ==========================================================
        public JsonResult CheckUserName(string username)
        {
            bool isAvailable = !KiemTraUserNameTonTai(username);
            return Json(isAvailable, JsonRequestBehavior.AllowGet);
        }


    

        private string KiemTraDuLieuDauVao(string username, string email, string phone)
        {
            if (KiemTraUserNameTonTai(username))
                return "Tên đăng nhập này đã được sử dụng.";

            if (KiemTraEmailTonTai(email))
                return "Email này đã được đăng ký cho tài khoản khác.";

           
            if (!KiemTraDinhDangSoDienThoai(phone))
                return "Số điện thoại không hợp lệ. Phải có đúng 10 chữ số và không chứa ký tự chữ.";

           
            if (KiemTraSoDienThoaiTonTai(phone))
                return "Số điện thoại này đã được đăng ký cho tài khoản khác.";

            return null; 
        }

        private bool KiemTraDinhDangSoDienThoai(string phone)
        {
            if (string.IsNullOrEmpty(phone)) return false;
            
            return Regex.IsMatch(phone, @"^\d{10}$");
        }

        private bool KiemTraUserNameTonTai(string username)
        {
            return db.NguoiDungs.Any(u => u.UserName == username);
        }

        private bool KiemTraEmailTonTai(string email)
        {
            return db.Khach_Hang.Any(e => e.Email == email);
        }

        private bool KiemTraSoDienThoaiTonTai(string phone)
        {
            return db.Khach_Hang.Any(s => s.SoDienThoai == phone);
        }

        private int TaoTaiKhoanNguoiDung(string username, string password, string name)
        {
            var newUser = new NguoiDung
            {
                UserName = username,
                Password = password, 
                GroupID = CustomerGroupID,
                Name = name
            };

            db.NguoiDungs.Add(newUser);
            db.SaveChanges(); 

            return newUser.UserID;
        }

        private void TaoThongTinKhachHang(int userId, string name, string email, string phone, DateTime? ngaysinh)
        {
            var newCustomer = new Khach_Hang
            {
                UserID = userId,
                TenDayDu = name,
                Email = email,
                SoDienThoai = phone,
                Ngaysinh = ngaysinh,
                DiemThanhVien = 0,
                NgayTaoTaiKhoan = DateTime.Now
            };

            db.Khach_Hang.Add(newCustomer);
            db.SaveChanges();
        }
    }
}