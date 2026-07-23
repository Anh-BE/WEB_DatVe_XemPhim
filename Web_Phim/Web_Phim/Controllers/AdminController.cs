using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web_Phim.Models;


namespace Web_Phim.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        private QuanLyPhimHQTCSDLEntities db = new QuanLyPhimHQTCSDLEntities();

        // GET: /Admin/QuanLyNguoiDung
        public ActionResult QuanLyNguoiDung()
        {
            // Bước 1: Gọi thủ tục thống kê theo quyền
            // Lưu ý: Nếu dùng Entity Framework, cần gọi hàm theo cách của EF (ví dụ: db.Database.SqlQuery<T>).
            // Dưới đây là cách mô phỏng gọi stored procedure và map kết quả.

            // 1. Lấy dữ liệu thống kê (sp_ThongKeNguoiDungTheoQuyen)
            var thongKeQuyen = db.Database.SqlQuery<ThongKeQuyenViewModel>(
                "EXEC sp_ThongKeNguoiDungTheoQuyen"
            ).ToList();

            // 2. Lấy danh sách người dùng chi tiết (sp_LayDanhSachChiTietNguoiDung)
            var danhSachNguoiDung = db.Database.SqlQuery<NguoiDungChiTietViewModel>(
                "EXEC sp_LayDanhSachChiTietNguoiDung"
            ).ToList();

            // Tạo ViewModel tổng hợp để truyền vào View
            var model = new QuanLyNguoiDungTongHopViewModel
            {
                ThongKeTheoQuyen = thongKeQuyen,
                DanhSachChiTiet = danhSachNguoiDung
            };

            return View(model);
        }


        public ActionResult ThemNhanVien()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemNhanVien(ThemNhanVienViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Khai báo tham số SQL
                    var pUserName = new SqlParameter("@UserName", model.UserName);
                    var pPassword = new SqlParameter("@Password", model.Password); // Nên mã hóa password tại đây nếu cần
                    var pHoTen = new SqlParameter("@HoTen", model.HoTen);

                    // THAM SỐ MỚI THÊM
                    var pEmail = new SqlParameter("@Email", model.Email);
                    var pPhone = new SqlParameter("@SoDienThoai", model.SoDienThoai);

                    // THAM SỐ CỐ ĐỊNH: 2 LÀ STAFF (NHÂN VIÊN)
                    var pGroupID = new SqlParameter("@GroupID", 2);

                    // 2. Gọi Stored Procedure cập nhật mới
                    // Lưu ý: Tên Proc và thứ tự tham số phải khớp với SQL bạn đã chạy
                    var result = db.Database.SqlQuery<SqlResultViewModel>(
                        "EXEC sp_ThemNhanVien @UserName, @Password, @HoTen, @Email, @SoDienThoai, @GroupID",
                        pUserName, pPassword, pHoTen, pEmail, pPhone, pGroupID
                    ).FirstOrDefault();

                    // 3. Kiểm tra kết quả trả về từ SQL (Nếu Proc có Select Message trả về)
                    // Nếu Proc của bạn chỉ Insert mà không Select về, hãy dùng db.Database.ExecuteSqlCommand(...) thay thế.
                    if (result != null)
                    {
                        TempData["SuccessMessage"] = result.Message;
                        return RedirectToAction("QuanLyNguoiDung");
                    }
                    else
                    {
                        // Trường hợp chạy ExecuteSqlCommand thành công nhưng không trả về data
                        // Hoặc logic fallback
                        TempData["SuccessMessage"] = "Thêm nhân viên thành công!";
                        return RedirectToAction("QuanLyNguoiDung");
                    }
                }
                catch (Exception ex)
                {
                    // Bắt lỗi từ SQL (ví dụ: Trùng tên đăng nhập, trùng Email...)
                    ModelState.AddModelError("", "Lỗi từ hệ thống: " + ex.Message);
                }
            }

            // Nếu thất bại, trả về View để nhập lại
            return View(model);
        }
        public ActionResult XoaUser(int id)
        {
            // Lấy thông tin chi tiết người dùng để hiển thị trên trang xác nhận
            // Giả định bạn có một Stored Procedure hoặc LINQ để lấy 1 người dùng theo ID

            var userToDelete = db.User_.Find(id); // Sử dụng Entity Framework Find nếu có

            if (userToDelete == null)
            {
                return HttpNotFound();
            }

            return View(userToDelete); // Trả về Model User_ để hiển thị HoTen, UserName
        }
        [HttpPost, ActionName("XoaUser")]
        [ValidateAntiForgeryToken] // Rất quan trọng cho bảo mật
        public ActionResult XacNhanXoa(int id)
        {
            try
            {
                // 1. Khai báo tham số SQL
                var pUserID = new SqlParameter("@UserID", id);

                // 2. Gọi Stored Procedure và hứng kết quả bằng SqlResultViewModel
                // (Giả định bạn đã tạo lớp SqlResultViewModel trong thư mục Models)
                var result = db.Database.SqlQuery<SqlResultViewModel>(
                    "EXEC sp_XoaNguoiDung @UserID",
                    pUserID
                ).FirstOrDefault();

                // 3. Xử lý kết quả
                if (result != null && result.Result == 1)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction("QuanLyNguoiDung");
                }
                else
                {
                    // Nếu có lỗi từ SQL (ví dụ: RAISERROR)
                    TempData["ErrorMessage"] = result?.Message ?? "Lỗi không xác định khi xóa người dùng.";
                    return RedirectToAction("QuanLyNguoiDung");
                }
            }
            catch (Exception ex)
            {
                // Lỗi hệ thống hoặc lỗi kết nối
                TempData["ErrorMessage"] = "Lỗi hệ thống: " + ex.Message;
                return RedirectToAction("QuanLyNguoiDung");
            }
        }
        public ActionResult BaoCaoPhanHang()
        {
            try
            {
                // Gọi Stored Procedure
                var data = db.Database.SqlQuery<BaoCaoChiTieuViewModel>("EXEC sp_BaoCaoChiTieuKhachHangg_Cursor").ToList();
                return View(data);
            }
            catch (Exception ex)
            {
                // 1. Ghi lỗi vào TempData để debug
                TempData["ErrorMessage"] = "Lỗi hệ thống: " + ex.Message;

                // 2. Thay vì chuyển sang Index (không tồn tại), hãy chuyển về trang Quản Lý Người Dùng
                return RedirectToAction("QuanLyNguoiDung");
            }
        }

    }
}
