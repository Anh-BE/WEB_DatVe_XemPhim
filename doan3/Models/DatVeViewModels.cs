using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace doan3.Models
{
    // Dùng để gom dữ liệu từ LINQ (không phải EF entity)
    public class LichChieuDTO
    {
        public long LichChieuID { get; set; }
        public DateTime? ThoiGianBatDau { get; set; }
        public string DinhDang { get; set; }

        public long RapID { get; set; }
        public string TenRap { get; set; }
        public string DiaChi { get; set; }
        public string ThanhPho { get; set; }

        public string TenPhong { get; set; }

        public string TenPhim { get; set; }
        public string Poster { get; set; }
    }

    // Màn hình chọn suất / lịch chiếu
    public class MovieBookingViewModel
    {
        public long PhimID { get; set; }
        public string TenPhim { get; set; }
        public string Poster { get; set; }

        public List<DateTime> CacNgayChieu { get; set; }
        public List<string> DanhSachThanhPho { get; set; }

        // Key: "ddMMyyyy"
        public Dictionary<string, List<RapChieuViewModel>> LichChieuTheoNgay { get; set; }
    }

    public class RapChieuViewModel
    {
        public long RapID { get; set; }
        public string TenRap { get; set; }
        public string DiaChi { get; set; }
        public string ThanhPho { get; set; }

        public List<SuatChieuItem> DanhSachSuatChieu { get; set; }
    }

    public class SuatChieuItem
    {
        public long LichChieuID { get; set; }
        public string GioChieu { get; set; }   // "HH:mm"
        public string DinhDang { get; set; }   // "2D", "3D" ...
    }

    // Loại vé theo loại ghế (Thuong / VIP / Doi) – lấy từ TienVe
    public class TicketOptionViewModel
    {
        public string TenLoaiVe { get; set; }  // Thuong, VIP, Doi
        public decimal GiaBan { get; set; }
        public int SoLuong { get; set; } = 0;
    }

    // View chọn loại vé
    public class BookingSummaryViewModel
    {
        public long LichChieuID { get; set; }
        public string TenPhim { get; set; }
        public string TenRapHienThi { get; set; } // "CGV Aeon Tân Phú - TP.HCM"
        public string PhanLoai { get; set; }      // PhanLoaiDoTuoi
        public string SuatChieu { get; set; }     // "18:30 20/11/2025"

        public List<TicketOptionViewModel> DanhSachLoaiVe { get; set; }
    }

    // View sơ đồ ghế
    public class SeatViewModel
    {
        public long GheID { get; set; }
        public string MaGhe { get; set; }   // A1, B3...
        public string LoaiGhe { get; set; } // Thuong / VIP / Doi
        public string HangGhe { get; set; } // A, B, C...

        public decimal GiaVe { get; set; }
        public int TrangThai { get; set; }  // 0: trống, 1: đã bán, 2: đang khóa tạm
    }

    public class SeatMapViewModel
    {
        public List<SeatViewModel> DanhSachGhe { get; set; }

        // Số vé người dùng đã chọn ở bước trước (đơn + đôi)
        public int SoLuongGheDon { get; set; }
        public int SoLuongGheDoi { get; set; }
    }
}