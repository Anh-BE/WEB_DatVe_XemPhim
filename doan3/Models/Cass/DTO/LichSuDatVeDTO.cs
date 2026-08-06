using System;

namespace doan3.Models.Cass.DTO
{
    /// <summary>
    /// MODULE CASSANDRA (ĐỘC LẬP) - DTO cho bảng lich_su_dat_ve
    /// Dùng để ghi 1 bước trong toàn bộ vòng đời xử lý đơn đặt vé của khách hàng.
    /// Các bước tham khảo: "Dat ve" -> "Thanh toan" -> "Xuat ve" -> "Email" -> "Hoan tien" -> "Huy" -> "Doi suat chieu"
    /// Đây là bảng chỉ INSERT, không UPDATE / DELETE dữ liệu lịch sử cũ.
    /// </summary>
    public class LichSuDatVeDTO
    {
        public long KhachHangId { get; set; }
        public long? DonDatVeId { get; set; }

        public string MaDatVe { get; set; }

        /// <summary>Tên bước xử lý, ví dụ: "Dat ve", "Thanh toan", "Xuat ve", "Email", "Hoan tien", "Huy", "Doi suat chieu"</summary>
        public string Buoc { get; set; }

        public long? LichChieuId { get; set; }
        public long? PhimId { get; set; }

        public long? RapId { get; set; }

        public long? PhongChieuId { get; set; }
        public int? SoGhe { get; set; }
        public decimal? TongTien { get; set; }
        public string VoucherCode { get; set; }

        public decimal? SoTienGiam { get; set; }

        public string PhuongThucThanhToan { get; set; }

        public string TrangThaiThanhToan { get; set; }
        public string GhiChu { get; set; }
        public string TrangThai { get; internal set; }
    }
}
