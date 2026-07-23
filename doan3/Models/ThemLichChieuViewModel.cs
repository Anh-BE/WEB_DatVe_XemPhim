using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace doan3.Models
{
    public class ThemLichChieuViewModel
    {
        public long? PhimID { get; set; }
        public long? RapID { get; set; }
        public long? PhongID { get; set; }

        public DateTime? ThoiGianBatDau { get; set; }
        public string DinhDang { get; set; }

        public List<Phim> DanhSachPhim { get; set; }
        public List<Rap_Chieu> DanhSachRap { get; set; }
        public List<Phong_Chieu> DanhSachPhong { get; set; }

        public List<Lich_Chieu> LichChieuHienTai { get; set; }
    }
}