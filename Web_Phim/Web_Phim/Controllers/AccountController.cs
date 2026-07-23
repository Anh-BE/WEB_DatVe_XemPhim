using System;
using System.Linq;
using System.Web.Mvc;
using Web_Phim.Models;

namespace Web_Phim.Controllers
{
    public class AccountController : Controller
    {
        QuanLyPhimHQTCSDLEntities db = new QuanLyPhimHQTCSDLEntities();

        // GET: Login
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Gọi Stored Procedure sp_KiemTraDangNhap
                // Lưu ý: Proc trả về 1 bảng, ta dùng SqlQuery để lấy dòng đầu tiên
                var userCheck = db.Database.SqlQuery<UserCheckResult>(
                    "EXEC sp_KiemTraDangNhap @UserName, @Password",
                    new System.Data.SqlClient.SqlParameter("UserName", model.UserName),
                    new System.Data.SqlClient.SqlParameter("Password", model.Password)
                ).FirstOrDefault();

                if (userCheck != null && userCheck.TrangThaiDangNhap == 1)
                {
                    // Lưu Session
                    Session["UserID"] = userCheck.UserID;
                    Session["UserName"] = userCheck.UserName;
                    Session["HoTen"] = userCheck.HoTen;
                    Session["Role"] = userCheck.TenNhomQuyen;

                    return RedirectToAction("TrangChu", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                }
            }
            return View(model);
        }

        // GET: Register
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Gọi Stored Procedure sp_DangKyKhachHang
                    db.Database.ExecuteSqlCommand(
                        "EXEC sp_DangKyKhachHang @UserName, @Password, @HoTen, @Email, @SoDienThoai",
                        new System.Data.SqlClient.SqlParameter("@UserName", model.UserName),
                        new System.Data.SqlClient.SqlParameter("@Password", model.Password),
                        new System.Data.SqlClient.SqlParameter("@HoTen", model.FullName),
                        new System.Data.SqlClient.SqlParameter("@Email", model.Email),
                        new System.Data.SqlClient.SqlParameter("@SoDienThoai", model.Phone)
                    );

                    TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    // Lỗi từ SQL (Trigger hoặc Proc) sẽ được bắt tại đây
                    // Ví dụ: UserName trùng, Email trùng...
                    ModelState.AddModelError("", "Đăng ký thất bại: " + ex.Message);
                }
            }
            return View(model);
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("TrangChu", "Home");
        }

        // Class phụ để hứng kết quả từ sp_KiemTraDangNhap
        private class UserCheckResult
        {
            public int UserID { get; set; }
            public string UserName { get; set; }
            public string HoTen { get; set; }
            public string TenNhomQuyen { get; set; }
            public int TrangThaiDangNhap { get; set; }
        }
    }
}