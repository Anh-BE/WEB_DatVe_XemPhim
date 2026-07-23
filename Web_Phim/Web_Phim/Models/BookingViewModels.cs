using System;
using System.Collections.Generic;

namespace Web_Phim.Models
{
    // 1. Class để hứng dữ liệu thô từ Procedure
    public class LichChieuDTO
    {
        public long LichChieuID { get; set; }
        public DateTime ThoiGianBatDau { get; set; }
        public decimal GiaVe { get; set; }
        public string DinhDang { get; set; }
        public string TenPhong { get; set; }
        public long RapID { get; set; }
        public string TenRap { get; set; }
        public string DiaChi { get; set; }
        public string ThanhPho { get; set; }
        public string TenPhim { get; set; }
        public string Poster { get; set; }
    }

    // 2. Class để hiển thị ra View (Đã gom nhóm)
    public class MovieBookingViewModel
    {
        public long PhimID { get; set; }
        public string TenPhim { get; set; }
        public string Poster { get; set; }

        // Danh sách các ngày có suất chiếu (để vẽ các ô vuông màu vàng chọn ngày)
        public List<DateTime> CacNgayChieu { get; set; }
        public List<string> DanhSachThanhPho { get; set; }

        // Dữ liệu đã gom nhóm: Ngày -> Rạp -> Danh sách giờ chiếu
        // Key: Ngày (dd/MM/yyyy), Value: Danh sách Rạp trong ngày đó
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
        public string GioChieu { get; set; } // VD: 19:30
        public string DinhDang { get; set; } // 2D
        public decimal GiaVe { get; set; }
    }
    public class TicketOptionViewModel
    {
        public int LoaiVeID { get; set; }
        public string TenLoaiVe { get; set; }
        public decimal GiaBan { get; set; }
        public int SoLuong { get; set; } = 0; // Mặc định là 0
    }

    public class BookingSummaryViewModel
    {
        public long LichChieuID { get; set; }
        public string TenPhim { get; set; }
        public string TenRapHienThi { get; set; } // Sửa tên property này cho khớp SQL
        public string PhanLoai { get; set; } // T13
        public string SuatChieu { get; set; }
        public List<TicketOptionViewModel> DanhSachLoaiVe { get; set; }
    }
    public class SeatViewModel
    {
        public long GheID { get; set; }
        public string MaGhe { get; set; } // A1, A2
        public string LoaiGhe { get; set; } // Thuong, Vip, Doi
        public decimal GiaVe { get; set; }
        public int TrangThai { get; set; } // 0: Trống, 1: Đã bán, 2: Đang giữ
        public string HangGhe { get; set; } // A, B, C (Để group khi vẽ)
    }
    public class SeatMapViewModel
    {
        public List<SeatViewModel> DanhSachGhe { get; set; }
        public int SoLuongGheDon { get; set; } // Số vé thường + VIP
        public int SoLuongGheDoi { get; set; } // Số vé đôi
    }
}