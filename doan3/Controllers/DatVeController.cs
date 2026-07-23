using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using doan3.Models;

namespace doan3.Controllers
{
    public class DatVeController : Controller
    {
        private LTW_DatVeXemPhimEntities db = new LTW_DatVeXemPhimEntities();

        public ActionResult ChonSuat(long idPhim)
        {
            if (Session["USER_SESSION"] == null)
            {
                
                return RedirectToAction("Index_DangNhap", "Login");
            }
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
            {
                return View("EmptySchedule");  
            }

            var model = new MovieBookingViewModel
            {
                PhimID = idPhim,
                TenPhim = rawData.First().TenPhim,
                Poster = rawData.First().Poster,
                CacNgayChieu = rawData
                                    .Where(x => x.ThoiGianBatDau.HasValue)
                                    .Select(x => x.ThoiGianBatDau.Value.Date)
                                    .Distinct()
                                    .OrderBy(x => x)
                                    .ToList(),
                DanhSachThanhPho = rawData.Select(x => x.ThanhPho)
                                          .Distinct()
                                          .ToList(),
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
                        })
                        .OrderBy(s => s.GioChieu)
                        .ToList()
                    }).ToList();

                model.LichChieuTheoNgay.Add(keyNgay, danhSachRap);
            }

            return View("ChonSuat", model);
        }


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
                                })
                                .ToList();

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

        public ActionResult GetSeatMap(long lichChieuId, int qtyNorm, int qtyCouple)
        {
            var lich = db.Lich_Chieu.FirstOrDefault(l => l.LichChieuID == lichChieuId);
            if (lich == null) return HttpNotFound();

            var phongId = lich.PhongID;

            var seats = db.Ghe_Ngoi
                          .Where(g => g.PhongID == phongId)
                          .ToList();

            var now = DateTime.Now;

            var bookedIds = db.Chi_Tiet_Ve
                              .Where(c => c.LichChieuID == lichChieuId)
                              .Select(c => c.GheID)
                              .ToList();

            var lockedIds = db.Khoa_Ghe_Tam_Thoi
                              .Where(k => k.LichChieuID == lichChieuId && k.ThoiGianHetHan > now)
                              .Select(k => k.GheID)
                              .ToList();

            var giaTheoLoai = db.TienVes.ToDictionary(t => t.LoaiGhe, t => t.GiaTien ?? 0);

            var listSeat = seats.Select(g => new SeatViewModel
            {
                GheID = g.GheID,
                MaGhe = g.MaGhe,
                HangGhe = g.HangGhe,
                LoaiGhe = g.LoaiGhe,
                GiaVe = giaTheoLoai.ContainsKey(g.LoaiGhe) ? giaTheoLoai[g.LoaiGhe] : 0,
                TrangThai = bookedIds.Contains(g.GheID)
                                ? 1
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

        [HttpPost]
        public ActionResult ConfirmSeats(long lichChieuId, string seatIds)
        {
         
            if (Session["USER_SESSION"] == null)
            {
                return RedirectToAction("Index_DangNhap", "Login");
            }

            if (string.IsNullOrEmpty(seatIds))
            {
                return RedirectToAction("ChonSuat", new { idPhim = 1 }); 
            }
            using (var db = new LTW_DatVeXemPhimEntities())
            {
                var listSeatIds = seatIds.Split(',').Select(long.Parse).ToList();
                var now = DateTime.Now;

                foreach (var id in listSeatIds)
                {
                    bool daDat = db.Chi_Tiet_Ve.Any(c => c.LichChieuID == lichChieuId && c.GheID == id);

                    bool dangKhoa = db.Khoa_Ghe_Tam_Thoi.Any(k => k.LichChieuID == lichChieuId &&
                                                                  k.GheID == id &&
                                                                  k.ThoiGianHetHan > now);

                    if (daDat || dangKhoa)
                    {
                        TempData["Error"] = $"Ghế {id} vừa được người khác chọn. Vui lòng chọn ghế khác.";
                        return Redirect(Request.UrlReferrer?.ToString() ?? "/");
                    }
                }

                var userSession = Session["USER_SESSION"] as doan3.Models.UserLogin; 
                long? khachHangId = null;
                if (userSession != null)
                {
                    var kh = db.Khach_Hang.FirstOrDefault(k => k.UserID == userSession.UserID);
                    khachHangId = kh?.KhachHangID;
                }

                foreach (var id in listSeatIds)
                {
                    var oldLock = db.Khoa_Ghe_Tam_Thoi.FirstOrDefault(k => k.LichChieuID == lichChieuId && k.GheID == id);
                    if (oldLock != null) db.Khoa_Ghe_Tam_Thoi.Remove(oldLock);

                    var lockSeat = new Khoa_Ghe_Tam_Thoi
                    {
                        LichChieuID = lichChieuId,
                        GheID = id,
                        KhachHangID = khachHangId,
                        ThoiGianKhoa = now,
                        ThoiGianHetHan = now.AddMinutes(1) 
                    };
                    db.Khoa_Ghe_Tam_Thoi.Add(lockSeat);
                }
                db.SaveChanges();
            }
            return RedirectToAction("Index", "ThanhToan", new { lichChieuId = lichChieuId, lockedSeatIds = seatIds });
        }

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
