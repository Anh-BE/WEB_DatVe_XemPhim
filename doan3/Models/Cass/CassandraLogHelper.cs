namespace doan3.Models.Cass
{
    /// <summary>
    /// MODULE CASSANDRA (ĐỘC LẬP) - Helper dùng chung, không phụ thuộc thư viện ngoài.
    /// Phân tích chuỗi User-Agent để lấy Trình duyệt / Thiết bị / Hệ điều hành,
    /// phục vụ ghi đầy đủ cột cho bảng nhat_ky_hoat_dong (tránh để NULL).
    /// Cố ý viết đơn giản (best-effort), không cần chính xác 100% vì chỉ phục vụ mục đích
    /// log/thống kê, không phải logic nghiệp vụ.
    /// </summary>
    public static class CassandraLogHelper
    {
        public static void PhanTichUserAgent(string userAgent, out string trinhDuyet, out string thietBi, out string heDieuHanh)
        {
            trinhDuyet = "Khong xac dinh";
            thietBi = "Desktop";
            heDieuHanh = "Khong xac dinh";

            if (string.IsNullOrWhiteSpace(userAgent))
            {
                return;
            }

            string ua = userAgent.ToLowerInvariant();

            // ----- Trình duyệt -----
            if (ua.Contains("edg/"))
                trinhDuyet = "Edge";
            else if (ua.Contains("opr/") || ua.Contains("opera"))
                trinhDuyet = "Opera";
            else if (ua.Contains("chrome/"))
                trinhDuyet = "Chrome";
            else if (ua.Contains("firefox/"))
                trinhDuyet = "Firefox";
            else if (ua.Contains("safari/"))
                trinhDuyet = "Safari";
            else if (ua.Contains("msie") || ua.Contains("trident/"))
                trinhDuyet = "Internet Explorer";

            // ----- Thiết bị -----
            if (ua.Contains("ipad") || (ua.Contains("tablet") && !ua.Contains("mobile")))
                thietBi = "Tablet";
            else if (ua.Contains("mobile") || ua.Contains("iphone") || ua.Contains("android"))
                thietBi = "Mobile";
            else
                thietBi = "Desktop";

            // ----- Hệ điều hành -----
            if (ua.Contains("windows nt"))
                heDieuHanh = "Windows";
            else if (ua.Contains("mac os x"))
                heDieuHanh = "macOS";
            else if (ua.Contains("android"))
                heDieuHanh = "Android";
            else if (ua.Contains("iphone") || ua.Contains("ipad") || ua.Contains("ios"))
                heDieuHanh = "iOS";
            else if (ua.Contains("linux"))
                heDieuHanh = "Linux";
        }
    }
}
