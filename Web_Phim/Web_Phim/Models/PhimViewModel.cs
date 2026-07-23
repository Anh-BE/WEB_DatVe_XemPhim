using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web_Phim.Models
{
    public class PhimViewModel
    {
        public long PhimID { get; set; } // Hoặc string MaPhim, tùy bạn
        public string TenPhim { get; set; }
        public string Poster { get; set; }
        public int ThoiLuong { get; set; }
        public DateTime? NgayKhoiChieu { get; set; }

        // Đây là thuộc tính chúng ta cần thêm!
        public decimal GiaVeThapNhat { get; set; }
    }
}