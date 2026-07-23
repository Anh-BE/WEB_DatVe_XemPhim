using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace doan3.Models
{
    public class ThongKeQuyenViewModel
    {
        public string TenQuyen { get; set; }
        public int SoLuong { get; set; }
    }

    // 2. ViewModel cho từng dòng trong danh sách người dùng
    public class NguoiDungChiTietViewModel
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public string TenQuyen { get; set; }
        public string TrangThai { get; set; }

        public int? DiemThanhVien { get; set; }

        public string Password { get; set; }
        public int GroupID { get; set; }
    }

    // 3. ViewModel tổng hợp (Chứa cả thống kê và danh sách để gửi sang View)
    public class QuanLyNguoiDungTongHopViewModel
    {
        public List<ThongKeQuyenViewModel> ThongKeTheoQuyen { get; set; }
        public List<NguoiDungChiTietViewModel> DanhSachChiTiet { get; set; }
    }

    // 4. ViewModel cho Form thêm nhân viên (Có Validate dữ liệu)
    public class ThemNhanVienViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [Display(Name = "Tên đăng nhập")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; }

        public int GroupID { get; set; } // Để chọn quyền nếu cần
    }
}