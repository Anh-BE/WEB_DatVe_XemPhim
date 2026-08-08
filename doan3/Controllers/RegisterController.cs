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
            username = username?.Trim();
            email = email?.Trim();
            phone = phone?.Trim();
            name = name?.Trim();

            string loiKiemTra = KiemTraDuLieuDauVao(username, email, phone);
            if (!string.IsNullOrEmpty(loiKiemTra))
            {
                ViewBag.Error = loiKiemTra;

                try
                {
                    doan3.Models.Cass.CassandraFeaturesService.GhiNhatKyHoatDong(
                        new doan3.Models.Cass.DTO.NhatKyHoatDongDTO
                        {
                            Username = username,
                            HanhDong = "Dang ky",
                            ChiTiet = "That bai: " + loiKiemTra,
                            ControllerName = "Register",
                            ActionName = "DangKy",
                            RequestMethod = Request.HttpMethod,
                            Browser = Request.Browser.Browser + " " + Request.Browser.Version,
                            Device = Request.Browser.IsMobileDevice ? "Mobile" : "Desktop",
                            HeDieuHanh = Request.Browser.Platform,
                            IpAddress = Request.UserHostAddress,
                            KetQua = "Failed"
                        });
                }
                catch
                {
                }

                return View("Index_DangKy");
            }

            try
            {
                int newUserId = TaoTaiKhoanNguoiDung(username, password, name);
                TaoThongTinKhachHang(newUserId, name, email, phone, ngaysinh);

                try
                {
                    doan3.Models.Cass.CassandraFeaturesService.GhiNhatKyHoatDong(
                        new doan3.Models.Cass.DTO.NhatKyHoatDongDTO
                        {
                            Username = username,
                            HanhDong = "Dang ky",
                            ChiTiet = "Dang ky tai khoan thanh cong",
                            ControllerName = "Register",
                            ActionName = "DangKy",
                            RequestMethod = Request.HttpMethod,
                            Browser = Request.Browser.Browser + " " + Request.Browser.Version,
                            Device = Request.Browser.IsMobileDevice ? "Mobile" : "Desktop",
                            HeDieuHanh = Request.Browser.Platform,
                            IpAddress = Request.UserHostAddress,
                            KetQua = "Success"
                        });
                }
                catch
                {
                }

                TempData["SuccessMessage"] = "Đăng ký tài khoản thành công! Mời bạn đăng nhập.";
                return RedirectToAction("Index_DangNhap", "Login");
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                var errorMessages = dbEx.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => x.ErrorMessage);
                ViewBag.Error = "Lỗi dữ liệu: " + string.Join("; ", errorMessages);
                return View("Index_DangKy");
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (ex.InnerException != null)
                {
                    msg += " -> " + ex.InnerException.Message;
                }
                ViewBag.Error = "Đã xảy ra lỗi trong quá trình đăng ký: " + msg;
                return View("Index_DangKy");
            }
        }

        // ==========================================================
        // 3. ACTION: AJAX CHECK USERNAME (Dùng cho Client check nhanh)
        // ==========================================================
        public JsonResult CheckUserName(string username)
        {
            bool isAvailable = !KiemTraUserNameTonTai(username?.Trim());
            return Json(isAvailable, JsonRequestBehavior.AllowGet);
        }

        private string KiemTraDuLieuDauVao(string username, string email, string phone)
        {
            if (KiemTraUserNameTonTai(username))
                return "Tên đăng nhập này đã được sử dụng.";

            if (!KiemTraGmailHopLe(email))
                return "Địa chỉ Email không hợp lệ. Phải là Gmail chuẩn (dạng example@gmail.com, từ 6-30 ký tự).";

            if (KiemTraEmailTonTai(email))
                return "Email này đã được đăng ký cho tài khoản khác.";

            if (string.IsNullOrEmpty(phone) || !KiemTraDinhDangSoDienThoai(phone))
                return "Số điện thoại không hợp lệ. Phải gồm đúng 10 chữ số (ví dụ: 0987654321).";

            if (KiemTraSoDienThoaiTonTai(phone))
                return "Số điện thoại này đã được đăng ký cho tài khoản khác.";

            return null; 
        }

        private bool KiemTraGmailHopLe(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            string pattern = @"^[a-zA-Z0-9.]{6,30}@(gmail\.com|googlemail\.com)$";
            if (!Regex.IsMatch(email.Trim(), pattern, RegexOptions.IgnoreCase))
            {
                return false;
            }

            string usernamePart = email.Trim().Split('@')[0];
            if (usernamePart.StartsWith(".") || usernamePart.EndsWith(".") || usernamePart.Contains(".."))
            {
                return false;
            }

            return true;
        }

        private bool KiemTraDinhDangSoDienThoai(string phone)
        {
            if (string.IsNullOrEmpty(phone)) return false;
            string cleanPhone = phone.Trim().Replace(" ", "").Replace("-", "");
            return Regex.IsMatch(cleanPhone, @"^\d{10}$");
        }

        private bool KiemTraUserNameTonTai(string username)
        {
            if (string.IsNullOrEmpty(username)) return false;
            return db.NguoiDungs.Any(u => u.UserName.ToLower() == username.ToLower());
        }

        private bool KiemTraEmailTonTai(string email)
        {
            if (string.IsNullOrEmpty(email)) return false;
            return db.Khach_Hang.Any(e => e.Email.ToLower() == email.ToLower());
        }

        private bool KiemTraSoDienThoaiTonTai(string phone)
        {
            if (string.IsNullOrEmpty(phone)) return false;
            string cleanPhone = phone.Trim().Replace(" ", "").Replace("-", "");
            return db.Khach_Hang.Any(s => s.SoDienThoai == cleanPhone);
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
            string cleanPhone = phone != null ? phone.Trim().Replace(" ", "").Replace("-", "") : null;
            var newCustomer = new Khach_Hang
            {
                UserID = userId,
                TenDayDu = name,
                Email = email != null ? email.Trim() : null,
                SoDienThoai = cleanPhone,
                Ngaysinh = ngaysinh,
                DiemThanhVien = 0,
                NgayTaoTaiKhoan = DateTime.Now
            };

            db.Khach_Hang.Add(newCustomer);
            db.SaveChanges();
        }
    }
}