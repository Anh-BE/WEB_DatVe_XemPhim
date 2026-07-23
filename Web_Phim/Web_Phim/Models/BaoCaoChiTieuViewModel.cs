using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Web_Phim.Models
{
    public class BaoCaoChiTieuViewModel
    {
        public long KhachHangID { get; set; }

        [Display(Name = "Họ và Tên")]
        public string TenKhachHang { get; set; }

        [Display(Name = "Số Điện Thoại")]
        public string SoDienThoai { get; set; }

        [Display(Name = "Tổng Chi Tiêu")]
        [DisplayFormat(DataFormatString = "{0:N0} VNĐ")] // Định dạng tiền tệ
        public decimal TongTienDaChi { get; set; }

        [Display(Name = "Hạng Thành Viên")]
        public string HangThanhVien { get; set; }
    }
}