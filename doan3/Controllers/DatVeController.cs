using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using doan3.Models;
using doan3.Models.Cass;
using doan3.Models.Cass.DTO;

namespace doan3.Controllers
{
    public class DatVeController : Controller
    {
        private LTW_DatVeXemPhimEntities db = new LTW_DatVeXemPhimEntities();

        // =====================================================================
        // GET: /DatVe/ChonSuat?idPhim=
        // Log Cassandra: VIEW_SHOWTIME SUCCESS
        // =====================================================================
        public ActionResult ChonSuat(long idPhim)
        {
            if (Session["USER_SESSION"] == null)
                return RedirectToAction("Index_DangNhap", "Login");

            var rawData = (from lc in db.Lich_Chieu
                           join phong in db.Phong_Chieu on lc.PhongID equals phong.PhongID
                           join rap in db.Rap_Chieu on phong.RapID equals rap.RapID
                           join phim in db.Phims on lc.PhimID equals phim.PhimID
                           where lc.PhimID == idPhim && lc.TrangThai == "Hoat Dong"
                           select new LichChieuDTO
                           {
                               LichChieuID = lc.LichChieuID,
                               ThoiGianBatDau = lc.ThoiGianBatDau,
                               DinhDang = lc.DinhDang,
                               RapID = rap.RapID,
                               TenRap = rap.TenRap,
                               DiaChi = rap.DiaChi,
                               ThanhPho = rap.ThanhPho,
                               TenPhong = phong.TenPhong,
                               TenPhim = phim.TenPhim,
                               Poster = phim.Poster
                           }).ToList();

            if (!rawData.Any())
                return View("EmptySchedule");

            var model = new MovieBookingViewModel
            {
                PhimID = idPhim,
                TenPhim = rawData.First().TenPhim,
                Poster = rawData.First().Poster,
                CacNgayChieu = rawData
                    .Where(x => x.ThoiGianBatDau.HasValue)
                    .Select(x => x.ThoiGianBatDau.Value.Date)
                    .Distinct().OrderBy(x => x).ToList(),
                DanhSachThanhPho = rawData.Select(x => x.ThanhPho).Distinct().ToList(),
                LichChieuTheoNgay = new Dictionary<string, List<RapChieuViewModel>>()
            };

            foreach (var ngay in model.CacNgayChieu)
            {
                var keyNgay = ngay.ToString("ddMMyyyy");
                var suatTrongNgay = rawData
                    .Where(x => x.ThoiGianBatDau.HasValue && x.ThoiGianBatDau.Value.Date == ngay.Date)
                    .ToList();

                var danhSachRap = suatTrongNgay
                    .GroupBy(x => new { x.RapID, x.TenRap, x.DiaChi, x.ThanhPho })
                    .Select(g => new RapChieuViewModel
                    {
                        RapID = g.Key.RapID,
                        TenRap = g.Key.TenRap,
                        DiaChi = g.Key.DiaChi,
                        ThanhPho = g.Key.ThanhPho,
                        DanhSachSuatChieu = g.Select(s => new SuatChieuItem
                        {
                            LichChieuID = s.LichChieuID,
                            GioChieu = s.ThoiGianBatDau?.ToString("HH:mm"),
                            DinhDang = s.DinhDang
                        }).OrderBy(s => s.GioChieu).ToList()
                    }).ToList();

                model.LichChieuTheoNgay.Add(keyNgay, danhSachRap);
            }

            // Cassandra: VIEW_SHOWTIME SUCCESS
            var sessionUser = Session["USER_SESSION"] as UserLogin;
            CassandraFeaturesService.GhiNhatKyHoatDong(new NhatKyHoatDongDTO
            {
                Username = sessionUser?.UserName,
                HanhDong = "VIEW_SHOWTIME",
                KetQua = "SUCCESS",
                ChiTiet = "Xem suat chieu phim: " + model.TenPhim + " (PhimID=" + idPhim + ")",
                ControllerName = "DatVe",
                ActionName = "ChonSuat",
                RequestMethod = Request.HttpMethod,
                Browser = Request.Browser.Browser + " " + Request.Browser.Version,
                Device = Request.Browser.IsMobileDevice ? "Mobile" : "Desktop",
                HeDieuHanh = Request.Browser.Platform,
                IpAddress = Request.UserHostAddress
            });

            return View("ChonSuat", model);
        }

        // =====================================================================
        // GET: /DatVe/GetTicketSelection?lichChieuId=
        // =====================================================================
        public ActionResult GetTicketSelection(long lichChieuId)
        {
            var lich = db.Lich_Chieu
                         .Include(l => l.Phim)
                         .Include(l => l.Phong_Chieu.Rap_Chieu)
                         .FirstOrDefault(l => l.LichChieuID == lichChieuId);

            if (lich == null) return HttpNotFound();

            var rap = lich.Phong_Chieu.Rap_Chieu;
            var phim = lich.Phim;

            var ticketTypes = db.TienVes
                                .Select(t => new TicketOptionViewModel
                                {
                                    TenLoaiVe = t.LoaiGhe,
                                    GiaBan = t.GiaTien ?? 0
                                }).ToList();

            var model = new BookingSummaryViewModel
            {
                LichChieuID = lichChieuId,
                TenPhim = phim.TenPhim,
                PhanLoai = phim.PhanLoaiDoTuoi,
                TenRapHienThi = rap.TenRap + " - " + rap.ThanhPho,
                SuatChieu = lich.ThoiGianBatDau?.ToString("HH:mm dd/MM/yyyy"),
                DanhSachLoaiVe = ticketTypes
            };

            return PartialView("_TicketSelection", model);
        }

        // =====================================================================
        // GET: /DatVe/GetSeatMap?lichChieuId=&qtyNorm=&qtyCouple=
        // =====================================================================
        public ActionResult GetSeatMap(long lichChieuId, int qtyNorm, int qtyCouple)
        {
            var lich = db.Lich_Chieu.FirstOrDefault(l => l.LichChieuID == lichChieuId);
            if (lich == null) return HttpNotFound();

            var phongId = lich.PhongID;

            var seats = db.Ghe_Ngoi.Where(g => g.PhongID == phongId).ToList();

            var bookedIds = db.Chi_Tiet_Ve
                              .Where(c => c.LichChieuID == lichChieuId)
                              .Select(c => c.GheID)
                              .ToList();

            // Ghế đang bị khóa tạm thời trên Redis
            var lockedIds = SeatLockService.GetLockedSeatIds(lichChieuId);

            var giaTheoLoai = db.TienVes.ToDictionary(t => t.LoaiGhe, t => t.GiaTien ?? 0);

            var listSeat = seats.Select(g => new SeatViewModel
            {
                GheID = g.GheID,
                MaGhe = g.MaGhe,
                HangGhe = g.HangGhe,
                LoaiGhe = g.LoaiGhe,
                GiaVe = giaTheoLoai.ContainsKey(g.LoaiGhe) ? giaTheoLoai[g.LoaiGhe] : 0,
                TrangThai = bookedIds.Contains(g.GheID) ? 1
                           : (lockedIds.Contains(g.GheID) ? 2 : 0)
            })
            .OrderBy(s => s.HangGhe)
            .ThenBy(s => s.MaGhe)
            .ToList();

            var model = new SeatMapViewModel
            {
                DanhSachGhe = listSeat,
                SoLuongGheDon = qtyNorm,
                SoLuongGheDoi = qtyCouple
            };

            return PartialView("_SeatMap", model);
        }

        // =====================================================================
        // POST: /DatVe/ConfirmSeats
        //
        // Log Cassandra:
        //   - nhat_ky_hoat_dong: LOCK_SEAT SUCCESS
        //   - lich_su_ghe: mỗi ghế 1 dòng trang_thai = "LOCK"
        // =====================================================================
        [HttpPost]
        public ActionResult ConfirmSeats(long lichChieuId, string seatIds)
        {
            if (Session["USER_SESSION"] == null)
                return RedirectToAction("Index_DangNhap", "Login");

            if (string.IsNullOrEmpty(seatIds))
                return RedirectToAction("ChonSuat", new { idPhim = 1 });

            var listSeatIds = seatIds.Split(',').Select(long.Parse).ToList();

            string browser = Request.Browser.Browser + " " + Request.Browser.Version;
            string device = Request.Browser.IsMobileDevice ? "Mobile" : "Desktop";
            string os = Request.Browser.Platform;
            string ip = Request.UserHostAddress;
            string httpMethod = Request.HttpMethod;

            using (var dbCtx = new LTW_DatVeXemPhimEntities())
            {
                // 1. Kiểm tra ghế đã booked trong SQL chưa
                foreach (var id in listSeatIds)
                {
                    bool daDat = dbCtx.Chi_Tiet_Ve.Any(c => c.LichChieuID == lichChieuId && c.GheID == id);
                    if (daDat)
                    {
                        TempData["Error"] = $"Ghế {id} đã được người khác mua thành công. Vui lòng chọn ghế khác.";
                        return Redirect(Request.UrlReferrer?.ToString() ?? "/");
                    }
                }

                var userSession = Session["USER_SESSION"] as UserLogin;
                long khachHangId = 0;
                if (userSession != null)
                {
                    var kh = dbCtx.Khach_Hang.FirstOrDefault(k => k.UserID == userSession.UserID);
                    khachHangId = kh?.KhachHangID ?? userSession.UserID;
                }

                // 2. Khóa ghế nguyên tử trên Redis (SETNX + TTL 60 giây)
                bool lockSuccess = SeatLockService.LockSeats(lichChieuId, listSeatIds, khachHangId, durationSeconds: 60);

                if (!lockSuccess)
                {
                    TempData["Error"] = "Ghế vừa được người khác nhanh tay chọn trước. Vui lòng chọn ghế khác.";
                    return Redirect(Request.UrlReferrer?.ToString() ?? "/");
                }

                // -------------------------------------------------------
                // Cassandra 1: Ghi lich_su_ghe trang_thai = "LOCK" cho mỗi ghế
                // (Bước đầu tiên của Seat Reservation Timeline)
                // -------------------------------------------------------
                foreach (var gheId in listSeatIds)
                {
                    CassandraFeaturesService.GhiLichSuGhe(new LichSuGheDTO
                    {
                        LichChieuId = lichChieuId,
                        GheId = gheId,
                        TrangThai = "LOCK",
                        KhachHangId = khachHangId,
                        DonDatVeId = null,
                        GhiChu = "User chon ghe tren so do",
                        ControllerName = "DatVe",
                        ActionName = "ConfirmSeats",
                        RequestMethod = httpMethod,
                        Browser = browser,
                        Device = device,
                        HeDieuHanh = os,
                        IpAddress = ip,
                        KetQua = "SUCCESS"
                    });
                }

                // Cassandra 2: Ghi nhat_ky_hoat_dong hanh_dong = "LOCK_SEAT"
                CassandraFeaturesService.GhiNhatKyHoatDong(new NhatKyHoatDongDTO
                {
                    Username = userSession?.UserName,
                    HanhDong = "LOCK_SEAT",
                    KetQua = "SUCCESS",
                    ChiTiet = "Khoa ghe: " + seatIds + " (LichChieuID=" + lichChieuId + ")",
                    ControllerName = "DatVe",
                    ActionName = "ConfirmSeats",
                    RequestMethod = httpMethod,
                    Browser = browser,
                    Device = device,
                    HeDieuHanh = os,
                    IpAddress = ip
                });
            }

            return RedirectToAction("Index", "ThanhToan",
                new { lichChieuId = lichChieuId, lockedSeatIds = seatIds });
        }

        // =====================================================================
        // POST: /DatVe/TinhTien
        // =====================================================================
        [HttpPost]
        public JsonResult TinhTien(string seatIds, string discountCode = null)
        {
            if (string.IsNullOrWhiteSpace(seatIds))
                return Json(new { success = false, total = 0 });

            var ids = seatIds.Split(',').Select(long.Parse).ToList();

            var seats = from g in db.Ghe_Ngoi
                        join t in db.TienVes on g.LoaiGhe equals t.LoaiGhe
                        where ids.Contains(g.GheID)
                        select new { g.GheID, g.LoaiGhe, t.GiaTien };

            decimal total = seats.Sum(i => i.GiaTien) ?? 0;
            decimal discount = 0;
            decimal finalAmount = total - discount;

            return Json(new
            {
                success = true,
                total = total,
                discount = discount,
                finalAmount = finalAmount
            });
        }
    }
}
