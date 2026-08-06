using System;

namespace doan3.Models.Cass.DTO
{
    public class NhatKyHoatDongDTO
    {
        public string Username { get; set; }

        public string HanhDong { get; set; }

        public string ChiTiet { get; set; }
        public string IpAddress { get; set; }
        public string ControllerName { get; set; }

        public string ActionName { get; set; }

        public string RequestMethod { get; set; }

        public string Browser { get; set; }

        public string Device { get; set; }

        public string HeDieuHanh { get; set; }
        public string KetQua { get; set; }
    }
}
