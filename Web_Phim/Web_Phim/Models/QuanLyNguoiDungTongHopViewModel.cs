using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web_Phim.Models
{
    public class QuanLyNguoiDungTongHopViewModel
    {
        public List<ThongKeQuyenViewModel> ThongKeTheoQuyen { get; set; }
        public List<NguoiDungChiTietViewModel> DanhSachChiTiet { get; set; }
    }
}