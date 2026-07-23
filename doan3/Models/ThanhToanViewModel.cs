using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace doan3.Models
{
    public class ThanhToanViewModel
    {
        public long LichChieuId { get; set; }
        public string LockedSeatIds { get; set; } // Chuỗi ID ghế: "1,2,3"

        public string TenPhim { get; set; }
        public string TenRap { get; set; }
        public string SuatChieu { get; set; }
        public string PhongChieu { get; set; }

        public string DanhSachGhe { get; set; } // Tên ghế: "A1, A2"
        public decimal TongTien { get; set; }

        public long SoGiayConLai { get; set; }
        public int DiemThanhVien { get; set; }



    }
}