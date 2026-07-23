using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web_Phim.Models
{
    public class NguoiDungChiTietViewModel
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string HoTen { get; set; } // u.Name AS HoTen
        public string TenNhomQuyen { get; set; } // ug.Name AS TenNhomQuyen
                                                 // Bỏ qua Password vì đã loại bỏ khỏi stored procedure
        public string MaKhachHang { get; set; } // Có thể null
        public string Email { get; set; } // Có thể null
        public string SoDienThoai { get; set; } // Có thể null
    }
}