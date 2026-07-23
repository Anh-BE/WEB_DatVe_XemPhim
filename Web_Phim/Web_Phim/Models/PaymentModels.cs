using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web_Phim.Models
{
    public class PaymentInfoViewModel
    {
        public long LichChieuID { get; set; }
        public string SeatIds { get; set; } // Chuỗi danh sách ghế (VD: "1,2,3")
        public decimal TongTien { get; set; }
        public string TenPhim { get; set; }
        public string TenRap { get; set; } // Tên rạp + tên phòng
        public string SuatChieu { get; set; }
        public int SoLuongVe { get; set; }
    }

    // Class này dùng để hiển thị hóa đơn sau khi thanh toán thành công
    public class BillViewModel
    {
        public long DonDatVeID { get; set; }
        public decimal TongTien { get; set; }
        public DateTime ThoiGianDat { get; set; }
        public string TenPhim { get; set; }
        public string TenRap { get; set; }
        public string DanhSachGhe { get; set; } // Tên ghế (VD: "A1, A2")
        public string PhuongThuc { get; set; } // MoMo hoặc MBBank
    }
}