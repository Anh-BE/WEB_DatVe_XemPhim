using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace doan3.Models
{
    public class BaoCaoDoanhThuViewModel
    {
        // Tổng quan hệ thống
        public decimal TongDoanhThu { get; set; }
        public int TongVeDaBan { get; set; }
        public int SoPhim { get; set; }


        // Top phim doanh thu cao nhất
        public List<TopPhimDoanhThuVM> TopPhim { get; set; }


        // Doanh thu theo rạp
        public List<DoanhThuTheoRapVM> TheoRap { get; set; }
    }


    // =============================
    // TOP PHIM DOANH THU
    // =============================
    public class TopPhimDoanhThuVM
    {
        public long PhimID { get; set; }
        public string TenPhim { get; set; }
        public int VeBan { get; set; }
        public decimal DoanhThu { get; set; }
        public double TyLe { get; set; }
    }


    // =============================
    // DOANH THU THEO RẠP
    // =============================
    public class DoanhThuTheoRapVM
    {
        public long RapID { get; set; }
        public string TenRap { get; set; }
        public int SuatChieu { get; set; }
        public int VeBan { get; set; }
        public decimal DoanhThu { get; set; }
    }


    // =============================
    // CHI TIẾT DOANH THU 1 RẠP
    // =============================
    public class ChiTietDoanhThuRapVM
    {
        public long PhimID { get; set; }
        public string TenPhim { get; set; }
        public int VeBan { get; set; }
        public decimal DoanhThu { get; set; }
    }
}