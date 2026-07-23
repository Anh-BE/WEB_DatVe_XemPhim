using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Web_Phim.Models
{
    public class ThemNhanVienViewModel
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        [StringLength(50)]
        [Display(Name = "Tên Đăng Nhập")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [StringLength(100)]
        [Display(Name = "Mật Khẩu")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống.")]
        [StringLength(50)]
        [Display(Name = "Họ Tên")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Số điện thoại")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; }

        // ĐÃ XÓA GroupID Ở ĐÂY
    }
}