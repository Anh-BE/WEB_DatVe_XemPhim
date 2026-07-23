using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web_Phim.Models
{
    public class ThongKeQuyenViewModel
    {
        // Tên cột phải trùng với tên trả về trong SQL (ug.Name AS TenNhomQuyen)
        public string TenNhomQuyen { get; set; }

        // COUNT(u.UserID) AS SoLuongTaiKhoan
        public int SoLuongTaiKhoan { get; set; }
    }
}