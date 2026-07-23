using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using Web_Phim.Models;

namespace Web_Phim.Controllers
{
    public class HistoryController : Controller
    {
        private QuanLyPhimHQTCSDLEntities db = new QuanLyPhimHQTCSDLEntities();

        public ActionResult Index()
        {
            // BƯỚC 3.1: KIỂM TRA ĐĂNG NHẬP
            // Giả định UserID được lưu trong Session sau khi đăng nhập
            if (Session["UserID"] == null)
            {
                // Thay thế bằng logic redirect đến trang đăng nhập của bạn
                return RedirectToAction("Login", "Account");
            }

            // Lấy UserID của người dùng hiện tại
            int userId = (int)Session["UserID"];

            // BƯỚC 3.2: GỌI STORED PROCEDURE
            try
            {
                // Tạo tham số SQL cho Stored Procedure
                var userIdParam = new SqlParameter("@UserID", userId);

                // Gọi SP và map kết quả trả về vào BookingHistoryViewModel
                var historyList = db.Database.SqlQuery<BookingHistoryViewModel>(
                    "EXEC sp_LayLichSuDatVeChiTiet @UserID",
                    userIdParam
                ).ToList();

                // BƯỚC 3.3: XỬ LÝ KẾT QUẢ VÀ HIỂN THỊ
                if (historyList == null || !historyList.Any())
                {
                    ViewBag.Message = "Tài khoản của bạn chưa có lịch sử đặt vé nào.";
                    return View(new List<BookingHistoryViewModel>());
                }

                return View(historyList);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi vào hệ thống (nếu có)
                // Hoặc hiển thị trang lỗi thân thiện
                ViewBag.ErrorMessage = "Đã xảy ra lỗi khi tải lịch sử đặt vé: " + ex.Message;
                return View("Error");
            }
        }
    }
}