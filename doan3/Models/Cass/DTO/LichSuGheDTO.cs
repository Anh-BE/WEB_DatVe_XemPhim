using System;

namespace doan3.Models.Cass.DTO
{
    /// <summary>
    /// MODULE CASSANDRA (ĐỘC LẬP) - DTO cho bảng lich_su_ghe
    /// Dùng để ghi 1 bản ghi lịch sử thay đổi trạng thái của 1 ghế trong 1 suất chiếu.
    /// Vòng đời tham khảo: "Giu ghe" -> "Thanh toan" -> "Xuat ve" -> "Check-in" -> "Hoan thanh"
    /// Đây là bảng chỉ INSERT, không UPDATE / DELETE dữ liệu lịch sử cũ.
    /// </summary>
    public class LichSuGheDTO
    {
        public long LichChieuId { get; set; }
        public long GheId { get; set; }

        /// <summary>Tên trạng thái, ví dụ: "Giu ghe", "Thanh toan", "Xuat ve", "Check-in", "Hoan thanh"</summary>
        public string TrangThai { get; set; }

        public long? KhachHangId { get; set; }
        public long? DonDatVeId { get; set; }

        public long? PhimId { get; set; }

        public long? PhongChieuId { get; set; }

        public string LoaiGhe { get; set; }

        public decimal? GiaVe { get; set; }

        public string ThietBi { get; set; }

        public string IpAddress { get; set; }

        public string GhiChu { get; set; }
        public string ControllerName { get; set; }

        public string ActionName { get; set; }

        public string RequestMethod { get; set; }

        public string Browser { get; set; }

        public string Device { get; set; }

        public string HeDieuHanh { get; set; }

        public string KetQua { get; set; }

        public string MaGhe { get; set; }

        public string MaDatVe { get; set; }

        public string LoaiTacDong { get; set; }
    }
}
