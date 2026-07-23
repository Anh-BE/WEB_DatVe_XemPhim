using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web_Phim.Models
{
    // Lưu ý: Tên thuộc tính phải khớp với tên cột SELECT trong Stored Procedure
    public class BookingHistoryViewModel
    {
        // Thông tin Đơn đặt vé
        public long DonDatVeID { get; set; }
        public string MaDatVe { get; set; }
        public System.DateTime ThoiGianDat { get; set; }
        public decimal TongTienDonHang { get; set; }
        public string TrangThaiDonHang { get; set; }

        // Thông tin Phim và Rạp
        public string TenPhim { get; set; }
        public string TenRap { get; set; } // Bao gồm cả tên phòng

        // Thông tin chi tiết vé (từ logic SP)
        public string DanhSachMaGhe { get; set; } // Chuỗi ghế (ví dụ: 'A1, A2, B3')
        public int SoLuongVe { get; set; }
    }
}