using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web_Phim.Models; // Gọi thư mục chứa các bảng CSDL
// Thêm thư viện cần thiết cho Transaction
using System.Data.Entity;

namespace Web_Phim.Controllers
{
    public class PaymentController : Controller
    {
        // Khởi tạo kết nối CSDL (Thay 'QuanLyPhimEntities' bằng tên trong file Model.edmx của bạn)
        private QuanLyPhimHQTCSDLEntities db = new QuanLyPhimHQTCSDLEntities();

        // 1. GET: Hiển thị trang xác nhận thông tin vé & chọn ngân hàng
        public ActionResult Index(long? lichChieuId, string seatIds)
        {   
            // Kiểm tra dữ liệu bắt buộc (Được truyền từ URL)
            if (lichChieuId == null || string.IsNullOrEmpty(seatIds))
            {
                return RedirectToAction("Index", "Home");
            }
            // Kiểm tra đăng nhập
            if (Session["UserID"] == null) return RedirectToAction("Login", "Account");

            // --- Lấy dữ liệu từ CSDL và tính toán ---
            var lichChieu = db.Lich_Chieu.Find(lichChieuId.Value);

            if (lichChieu == null) return HttpNotFound();
            var phim = db.Phims.Find(lichChieu.PhimID);
            var phong = db.Phong_Chieu.Find(lichChieu.PhongID);
            var rap = db.Rap_Chieu.Find(phong.RapID);

            // Tách chuỗi ghế "1,5,9" thành danh sách số để xử lý
            var listGheId = seatIds.Split(',').Select(long.Parse).ToList();

            // Tìm thông tin chi tiết của các ghế này trong CSDL
            var listGheDb = db.Ghe_Ngoi.Where(g => listGheId.Contains(g.GheID)).ToList();

            // QUAN TRỌNG: Tính toán lại tổng tiền tại Server 
            decimal tongTien = 0;
            foreach (var ghe in listGheDb)
            {
                decimal giaGoc = lichChieu.GiaVe;

                // Logic tính tiền: VIP thêm 20k, Đôi nhân 2
                if (ghe.LoaiGhe == "VIP") tongTien += giaGoc + 20000;
                else if (ghe.LoaiGhe == "Doi") tongTien += giaGoc * 2;
                else tongTien += giaGoc;
            }

            // Đóng gói dữ liệu vào Model để gửi sang View
            var model = new PaymentInfoViewModel
            {
                LichChieuID = lichChieuId.Value,
                SeatIds = seatIds,
                TongTien = tongTien,
                TenPhim = phim.TenPhim,
                TenRap = rap.TenRap + " - " + phong.TenPhong,
                SuatChieu = lichChieu.ThoiGianBatDau.ToString("HH:mm dd/MM/yyyy"),
                SoLuongVe = listGheId.Count
            };

            // LƯU VÀO SESSION để Action POST ProcessPayment có thể đọc
            Session["PaymentInfoViewModel"] = model;

            return View(model); // View này là trang bạn thấy có @model Web_Phim.Models.PaymentInfoViewModel
        }

        // 2. POST: Xử lý khi ấn nút "THANH TOÁN"
        // Hàm này nhận yêu cầu từ Ajax
        [HttpPost]

        public ActionResult ProcessPayment(long lichChieuId, string seatIds, string paymentMethod)
        {
            // 1. Kiểm tra tính hợp lệ (Cache Session)
            var cachedModel = Session["PaymentInfoViewModel"] as PaymentInfoViewModel;
            if (cachedModel == null || cachedModel.LichChieuID != lichChieuId || cachedModel.SeatIds != seatIds)
            {
                return Json(new { success = false, message = "Phiên giao dịch hết hạn. Vui lòng đặt lại." });
            }

            try
            {
                int userId = (int)Session["UserID"];
                decimal tongTien = cachedModel.TongTien;

                // --- GỌI STORED PROCEDURE (Thay thế hoàn toàn Transaction thủ công cũ) ---
                // Hàm này sẽ trả về ID của đơn đặt vé vừa tạo (theo câu lệnh SELECT cuối SP)
                var result = db.sp_ThanhToanVaDatVe(userId, lichChieuId, seatIds, tongTien, paymentMethod).FirstOrDefault();

                long newDonDatVeID = result.HasValue ? result.Value : 0;

                // --- XỬ LÝ KẾT QUẢ ---
                Session.Remove("PaymentInfoViewModel"); // Xóa cache

                // Chuẩn bị dữ liệu hiển thị Bill
                var listGheId = seatIds.Split(',').Select(long.Parse).ToList();
                var tenGhes = db.Ghe_Ngoi.Where(g => listGheId.Contains(g.GheID)).Select(g => g.MaGhe).ToArray();

                var billModel = new BillViewModel
                {
                    DonDatVeID = newDonDatVeID, // ID lấy từ Procedure trả về
                    TongTien = tongTien,
                    ThoiGianDat = DateTime.Now,
                    TenPhim = cachedModel.TenPhim,
                    TenRap = cachedModel.TenRap,
                    DanhSachGhe = string.Join(", ", tenGhes),
                    PhuongThuc = paymentMethod
                };

                TempData["BillInfo"] = billModel;
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Lỗi sẽ được Procedure ném ra (RAISERROR) và C# bắt được tại đây
                // Ví dụ: "Tài khoản chưa có thông tin khách hàng"
                return Json(new { success = false, message = "Lỗi thanh toán: " + ex.Message });
            }
        }

        // 3. GET: Trang hiển thị hóa đơn thành công
        public ActionResult Success()
        {
            if (TempData["BillInfo"] == null) return RedirectToAction("TrangChu", "Home");

            var model = (BillViewModel)TempData["BillInfo"];
            return View(model);
        }
    }
}