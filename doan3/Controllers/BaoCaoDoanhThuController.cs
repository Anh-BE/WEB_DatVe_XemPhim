using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using doan3.Models;             
                                
namespace doan3.Controllers
{
    public class BaoCaoDoanhThuController : Controller
    {
        LTW_DatVeXemPhimEntities db = new LTW_DatVeXemPhimEntities();

      
        public ActionResult ThongKeTong()
        {
            var vm = new BaoCaoDoanhThuViewModel
            {
                TongDoanhThu = TinhTongDoanhThu(),
                TongVeDaBan = TinhTongVeDaBan(),
                SoPhim = DemSoPhim(),
                TopPhim = LayTopPhimDoanhThu(),
                TheoRap = LayDoanhThuTheoRap()
            };

            return View(vm);
        }

     
        private decimal TinhTongDoanhThu()
        {
            return db.Don_Dat_Ve
                .Where(d => d.TrangThaiDonHang == "Đã thanh toán")
                .Sum(d => (decimal?)d.TongTienDonHang) ?? 0;
        }

        private int TinhTongVeDaBan()
        {
            return db.Chi_Tiet_Ve
                .Count(v => v.Don_Dat_Ve.TrangThaiDonHang == "Đã thanh toán");
        }

        private int DemSoRap()
        {
            return db.Rap_Chieu.Count();
        }

        // ==================================================
        // =============== TOP PHIM =========================
        // ==================================================
        private List<TopPhimDoanhThuVM> LayTopPhimDoanhThu()
        {
            var raw = db.Chi_Tiet_Ve
                .Where(v => v.Don_Dat_Ve.TrangThaiDonHang == "Đã thanh toán")
                .GroupBy(v => v.Lich_Chieu.Phim)
                .Select(g => new
                {
                    g.Key.PhimID,
                    g.Key.TenPhim,
                    VeBan = g.Count(),
                    DoanhThu = g.Sum(x => x.TienVe.GiaTien)
                })
                .OrderByDescending(x => x.DoanhThu)
                .Take(5)
                .ToList();

            var tongDoanhThu = raw.Sum(x => x.DoanhThu);

            return raw.Select(x => new TopPhimDoanhThuVM
            {
                PhimID = x.PhimID,
                TenPhim = x.TenPhim,
                VeBan = x.VeBan,
                DoanhThu = x.DoanhThu ?? 0,
                TyLe = tongDoanhThu > 0
                    ? Math.Round((double)(x.DoanhThu / tongDoanhThu * 100), 1)
                    : 0
            }).ToList();
        }

        // ==================================================
        // =============== THEO RẠP =========================
        // ==================================================
        private List<DoanhThuTheoRapVM> LayDoanhThuTheoRap()
        {

            var danhSachRap = db.Rap_Chieu.ToList();

            return danhSachRap
                .Select(rap => new DoanhThuTheoRapVM
                {
                    RapID = rap.RapID,
                    TenRap = rap.TenRap,
                    SuatChieu = DemSuatChieuCoVe(rap),
                    VeBan = DemTongVeBanTheoRap(rap),
                    DoanhThu = TinhDoanhThuTheoRap(rap)
                })
                .OrderByDescending(x => x.DoanhThu)
                .ToList();
        }
        private int DemSuatChieuCoVe(Rap_Chieu rap)
        {
            return rap.Phong_Chieu
                .SelectMany(p => p.Lich_Chieu)
                .Count(lc => lc.Chi_Tiet_Ve
                    .Any(ctv => ctv.Don_Dat_Ve.TrangThaiDonHang == "Đã thanh toán"));
        }
        private int DemTongVeBanTheoRap(Rap_Chieu rap)
        {
            return rap.Phong_Chieu
                .SelectMany(p => p.Lich_Chieu)
                .SelectMany(lc => lc.Chi_Tiet_Ve)
                .Count(ctv => ctv.Don_Dat_Ve.TrangThaiDonHang == "Đã thanh toán");
        }
        private decimal TinhDoanhThuTheoRap(Rap_Chieu rap)
        {
            return rap.Phong_Chieu
                .SelectMany(p => p.Lich_Chieu)
                .SelectMany(lc => lc.Chi_Tiet_Ve)
                .Where(ctv => ctv.Don_Dat_Ve.TrangThaiDonHang == "Đã thanh toán")
                .Sum(ctv => (decimal?)ctv.TienVe.GiaTien) ?? 0;
        }


        // ==================================================
        // ====== CHI TIẾT DOANH THU THEO 1 RẠP ==============
        // ==================================================

        public ActionResult ChiTietTheoRap(
               long id,
               string type = "month",
               DateTime? date = null,
               int? month = null,
               int? year = null
           )
        {
            var rap = LayRapTheoId(id);
            if (rap == null)
                return HttpNotFound();

            DateTime fromDate, toDate;
            XacDinhKhoangThoiGian(type, date, month, year, out fromDate, out toDate);

            var veTheoThoiGian = LayVeTheoThoiGian(id, fromDate, toDate);

            ViewBag.TenRap = rap.TenRap;
            ViewBag.TongDoanhThuRap = TinhTongDoanhThu(veTheoThoiGian);

            ViewBag.TongVeBan = TinhTongVeBan(veTheoThoiGian);

            ViewBag.Type = type;
            ViewBag.Date = date;
            ViewBag.Month = month;
            ViewBag.Year = year;

            var doanhThuTheoPhim = LayDoanhThuTheoPhim(id);

            return View(doanhThuTheoPhim);
        }

        // ==================================================
        // ================== HÀM HỖ TRỢ ====================
        // ==================================================

        private Rap_Chieu LayRapTheoId(long id)
        {
            return db.Rap_Chieu.Find(id);
        }

        // ===== XÁC ĐỊNH THỜI GIAN =====
        private void XacDinhKhoangThoiGian(
            string type,
            DateTime? date,
            int? month,
            int? year,
            out DateTime fromDate,
            out DateTime toDate
        )
        {
            DateTime now = DateTime.Now;

            if (type == "day" && date.HasValue)
            {
                fromDate = date.Value.Date;
                toDate = fromDate.AddDays(1).AddSeconds(-1);
                return;
            }

            if (type == "month" && month.HasValue && year.HasValue)
            {
                fromDate = new DateTime(year.Value, month.Value, 1);
                toDate = fromDate.AddMonths(1).AddSeconds(-1);
                return;
            }

            if (type == "year" && year.HasValue)
            {
                fromDate = new DateTime(year.Value, 1, 1);
                toDate = fromDate.AddYears(1).AddSeconds(-1);
                return;
            }

            // Mặc định: tháng hiện tại
            fromDate = new DateTime(now.Year, now.Month, 1);
            toDate = fromDate.AddMonths(1).AddSeconds(-1);
        }

        // ===== QUERY VÉ THEO THỜI GIAN =====
        private IQueryable<Chi_Tiet_Ve> LayVeTheoThoiGian(
            long rapId,
            DateTime fromDate,
            DateTime toDate
        )
        {
            return db.Chi_Tiet_Ve.Where(v =>
                v.Don_Dat_Ve.TrangThaiDonHang == "Đã thanh toán" &&
                v.Lich_Chieu.Phong_Chieu.RapID == rapId &&
                v.Don_Dat_Ve.ThoiGianDat >= fromDate &&
                v.Don_Dat_Ve.ThoiGianDat <= toDate
            );
        }

        // ===== TỔNG DOANH THU =====
        private decimal TinhTongDoanhThu(IQueryable<Chi_Tiet_Ve> data)
        {
            return data
                        .Sum(x => (decimal?)x.TienVe.GiaTien) ?? 0;
        }

        // ===== TỔNG VÉ =====
        private int TinhTongVeBan(IQueryable<Chi_Tiet_Ve> data)
        {
            return data.Count();
        }
        private int DemSoPhim()
        {
            
            return db.Phims.Count(p => p.TrangThai == "Dang Chieu");
        }
       
        private List<ChiTietDoanhThuRapVM> LayDoanhThuTheoPhim(long rapId)
        {
            return db.Chi_Tiet_Ve
                .Where(v =>
                    v.Don_Dat_Ve.TrangThaiDonHang == "Đã thanh toán" &&
                    v.Lich_Chieu.Phong_Chieu.RapID == rapId
                )
                .GroupBy(v => v.Lich_Chieu.Phim)
                .Select(g => new ChiTietDoanhThuRapVM
                {
                    PhimID = g.Key.PhimID,
                    TenPhim = g.Key.TenPhim,
                    VeBan = g.Count(),
                    DoanhThu = g.Sum(x => (decimal?)x.TienVe.GiaTien) ?? 0
                })
                .OrderByDescending(x => x.DoanhThu)
                .ToList();
        }
    }

}