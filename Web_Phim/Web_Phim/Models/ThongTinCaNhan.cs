using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Web_Phim.Models
{
    public class PersonalInfoViewModel
    {
        public int UserID { get; set; }
        public long KhachHangID { get; set; }
        public string UserName { get; set; }
        public int DiemThanhVien { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        public string TenDayDu { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "SĐT không hợp lệ")]
        public string SoDienThoai { get; set; }

        // --- CÁC TRƯỜNG DÙNG CHO MODAL ĐỔI MẬT KHẨU ---
        // Lưu ý: Không để [Required] ở đây để tránh ảnh hưởng form cập nhật thông tin
        public string MatKhauCu { get; set; }
        public string MatKhauMoi { get; set; }
        public string XacNhanMatKhau { get; set; }
    }
}