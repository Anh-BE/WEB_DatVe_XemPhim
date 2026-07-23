using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Web_Phim.Models;

namespace Web_Phim.Controllers
{
    public class BookingController : Controller
    {
        QuanLyPhimHQTCSDLEntities db = new QuanLyPhimHQTCSDLEntities();

        // GET: Booking/SelectCinema?id=1
        public ActionResult SelectCinema(long id)
        {
            var rawData = db.Database.SqlQuery<LichChieuDTO>(
                "EXEC sp_LayLichChieuTheoPhim @PhimID",
                new System.Data.SqlClient.SqlParameter("@PhimID", id)
            ).ToList();

            if (rawData.Count == 0) return View("EmptySchedule");

            var model = new MovieBookingViewModel
            {
                PhimID = id,
                TenPhim = rawData.First().TenPhim,
                Poster = rawData.First().Poster,
                CacNgayChieu = rawData.Select(x => x.ThoiGianBatDau.Date).Distinct().OrderBy(x => x).ToList(),
                // Lấy danh sách các thành phố có rạp chiếu phim này
                DanhSachThanhPho = rawData.Select(x => x.ThanhPho).Distinct().ToList(),
                LichChieuTheoNgay = new Dictionary<string, List<RapChieuViewModel>>()
            };

            foreach (var ngay in model.CacNgayChieu)
            {
                var keyNgay = ngay.ToString("ddMMyyyy");
                var suatTrongNgay = rawData.Where(x => x.ThoiGianBatDau.Date == ngay).ToList();

                // Chú ý: Thêm x.ThanhPho vào GroupBy và Select
                var danhSachRap = suatTrongNgay
                    .GroupBy(x => new { x.RapID, x.TenRap, x.DiaChi, x.ThanhPho })
                    .Select(g => new RapChieuViewModel
                    {
                        RapID = g.Key.RapID,
                        TenRap = g.Key.TenRap,
                        DiaChi = g.Key.DiaChi,
                        ThanhPho = g.Key.ThanhPho, // Gán giá trị
                        DanhSachSuatChieu = g.Select(s => new SuatChieuItem
                        {
                            LichChieuID = s.LichChieuID,
                            GioChieu = s.ThoiGianBatDau.ToString("HH:mm"),
                            DinhDang = s.DinhDang,
                            GiaVe = s.GiaVe
                        }).OrderBy(s => s.GioChieu).ToList()
                    }).ToList();

                model.LichChieuTheoNgay.Add(keyNgay, danhSachRap);
            }

            return View(model);
        }
        // GET: Booking/GetTicketSelection?lichChieuId=1
        public ActionResult GetTicketSelection(long lichChieuId)
        {
            // 1. Lấy thông tin tóm tắt phim/rạp
            var summary = db.Database.SqlQuery<BookingSummaryViewModel>(
                "EXEC sp_LayThongTinTomTatBooking @LichChieuID",
                new System.Data.SqlClient.SqlParameter("@LichChieuID", lichChieuId)
            ).FirstOrDefault();

            if (summary == null) return HttpNotFound();

            // 2. Lấy danh sách loại vé và giá
            var ticketTypes = db.Database.SqlQuery<TicketOptionViewModel>(
                "EXEC sp_LayDanhSachGiaVeTheoLichChieu @LichChieuID",
                new System.Data.SqlClient.SqlParameter("@LichChieuID", lichChieuId)
            ).ToList();

            summary.DanhSachLoaiVe = ticketTypes;

            return PartialView("_TicketSelection", summary);
        }
        // GET: Booking/GetSeatMap?lichChieuId=1&qty=3
        public ActionResult GetSeatMap(long lichChieuId, int qtyNorm, int qtyCouple)
        {
            var seats = db.Database.SqlQuery<SeatViewModel>(
                "EXEC sp_LayDanhSachGheVoiTrangThai @LichChieuID",
                new System.Data.SqlClient.SqlParameter("@LichChieuID", lichChieuId)
            ).ToList();

            foreach (var seat in seats) { seat.HangGhe = seat.MaGhe.Substring(0, 1); }

            var model = new SeatMapViewModel
            {
                DanhSachGhe = seats,
                SoLuongGheDon = qtyNorm,   // Gán vào đây
                SoLuongGheDoi = qtyCouple  // Gán vào đây
            };

            return PartialView("_SeatMap", model);
        }

        // Thêm vào BookingController.cs


        [HttpPost]
        public ActionResult ConfirmSeats(long lichChieuId, string seatIds)
        {
            if (Session["UserID"] == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập!" });
            }

            // Lấy UserID từ Session (Đây là ID tài khoản, ví dụ: 1, 2, 3...)
            int userId = int.Parse(Session["UserID"].ToString());

            try
            {
                // Gọi Procedure MỚI (Truyền @UserID)
                db.Database.ExecuteSqlCommand(
                    "EXEC sp_KhoaGheDeThanhToan @LichChieuID, @UserID, @ListGheID",
                    new System.Data.SqlClient.SqlParameter("@LichChieuID", lichChieuId),
                    new System.Data.SqlClient.SqlParameter("@UserID", userId), // Đổi tên tham số thành @UserID
                    new System.Data.SqlClient.SqlParameter("@ListGheID", seatIds)
                );

                return Json(new { success = true, message = "Giữ ghế thành công!" });
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Json(new { success = false, message = msg });
            }
        }
    }
}