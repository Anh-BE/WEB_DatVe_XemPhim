using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace doan3.Models
{
    public class GheTinhDiem
    {
        public long GheID { get; set; }
        public string MaGhe { get; set; }
        public decimal? GiaTien { get; set; }
        public string LoaiGhe { get; set; } // Thuong, VIP, Doi
    }
}