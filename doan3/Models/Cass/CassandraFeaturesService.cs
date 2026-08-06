using System;
using Cassandra;
using doan3.Models.Cass.DTO;

namespace doan3.Models.Cass
{
    /// <summary>
    /// ============================================================================
    /// MODULE CASSANDRA - HOÀN TOÀN ĐỘC LẬP
    /// ============================================================================
    /// Cung cấp 3 chức năng ghi Log/Timeline/History thời gian thực xuống Cassandra:
    ///   1. GhiLichSuGhe        -> bảng lich_su_ghe
    ///   2. GhiLichSuDatVe      -> bảng lich_su_dat_ve
    ///   3. GhiNhatKyHoatDong   -> bảng nhat_ky_hoat_dong
    ///
    /// NGUYÊN TẮC BẮT BUỘC:
    ///   - Chỉ được gọi các hàm này SAU KHI SQL Server đã Commit/xử lý thành công.
    ///   - Không bao giờ được đặt trong khối using(db.Database.BeginTransaction()) của SQL,
    ///     vì Cassandra không rollback theo giao dịch SQL.
    ///   - Mọi lỗi kết nối/ghi Cassandra đều bị "nuốt" (try/catch) tại đây, TUYỆT ĐỐI
    ///     không throw ngược lên Controller -> không bao giờ làm sập chức năng chính.
    ///   - Bảng chỉ INSERT, không UPDATE/DELETE dữ liệu lịch sử cũ (đúng chuẩn time-series).
    /// ============================================================================
    /// </summary>
    public static class CassandraFeaturesService
    {
        private static readonly TimeSpan VietnamOffset = TimeSpan.FromHours(7);

        /// <summary>Giờ Việt Nam hiện tại (UTC+7), dùng thống nhất cho toàn bộ 3 bảng.</summary>
        private static DateTimeOffset GioVietNamHienTai()
        {
            DateTime utcNow = DateTime.UtcNow;
            return new DateTimeOffset(utcNow, TimeSpan.Zero).ToOffset(VietnamOffset);
        }

        // ==========================================================================
        // CHỨC NĂNG 1: LỊCH SỬ THAY ĐỔI TRẠNG THÁI GHẾ
        // Bảng: lich_su_ghe | Partition Key: (lich_chieu_id, ghe_id) | Clustering: thoi_gian DESC, id
        // ==========================================================================
        public static void GhiLichSuGhe(LichSuGheDTO data)
        {
            if (data == null) return;

            try
            {
                var session = CassandraService.GetSession();

                const string cql = @"
            INSERT INTO lich_su_ghe
            (
                lich_chieu_id,
                ghe_id,
                thoi_gian,
                id,
                trang_thai,
                khach_hang_id,
                don_dat_ve_id,
                ghi_chu,
                controller_name,
                action_name,
                request_method,
                browser,
                device,
                he_dieu_hanh,
                ip_address,
                ket_qua
            )
            VALUES
            (
                ?,?,?,?,?,?,?,?,
                ?,?,?,?,?,?,?,?
            );";

                var stmt = new SimpleStatement(
                    cql,

                    data.LichChieuId,
                    data.GheId,

                    GioVietNamHienTai(),

                    Guid.NewGuid(),

                    data.TrangThai ?? "",

                    data.KhachHangId,

                    data.DonDatVeId,

                    data.GhiChu ?? "",

                    data.ControllerName ?? "",

                    data.ActionName ?? "",

                    data.RequestMethod ?? "",

                    data.Browser ?? "",

                    data.Device ?? "",

                    data.HeDieuHanh ?? "",

                    data.IpAddress ?? "",

                    data.KetQua ?? ""
                );

                session.Execute(stmt);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[Cassandra][lich_su_ghe] Bo qua loi ghi log: {ex.Message}");
            }
        }

        // ==========================================================================
        // CHỨC NĂNG 2: LỊCH SỬ ĐẶT VÉ CỦA KHÁCH HÀNG
        // Bảng: lich_su_dat_ve | Partition Key: khach_hang_id | Clustering: thoi_gian DESC, id
        // ==========================================================================
        public static void GhiLichSuDatVe(LichSuDatVeDTO data)
        {
            if (data == null) return;

            try
            {
                var session = CassandraService.GetSession();

                const string cql = @"
                    INSERT INTO lich_su_dat_ve
                        (khach_hang_id, thoi_gian, id, don_dat_ve_id, ma_dat_ve, buoc, lich_chieu_id, so_ghe, tong_tien, ghi_chu)
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                var stmt = new SimpleStatement(
                    cql,
                    data.KhachHangId,
                    GioVietNamHienTai(),
                    Guid.NewGuid(),
                    data.DonDatVeId,
                    data.MaDatVe ?? "",
                    data.Buoc ?? "",
                    data.LichChieuId,
                    data.SoGhe,
                    data.TongTien,
                    data.GhiChu ?? "");

                session.Execute(stmt);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[Cassandra][lich_su_dat_ve] Bo qua loi ghi log: " + ex.Message);
            }
        }

        // ==========================================================================
        // CHỨC NĂNG 3: NHẬT KÝ HOẠT ĐỘNG NGƯỜI DÙNG
        // Bảng: nhat_ky_hoat_dong | Partition Key: (username, ngay) | Clustering: thoi_gian DESC, id
        // Bucket theo NGÀY để chặn Hot Partition cho tài khoản hoạt động liên tục.
        // ==========================================================================
        public static void GhiNhatKyHoatDong(NhatKyHoatDongDTO data)
        {
            if (data == null) return;

            try
            {
                var session = CassandraService.GetSession();
                var gioVn = GioVietNamHienTai();
                string ngay = gioVn.ToString("yyyy-MM-dd");

                const string cql = @"
                INSERT INTO nhat_ky_hoat_dong
                (
                username,
                ngay,
                thoi_gian,
                id,
                hanh_dong,
                chi_tiet,
                ip_address,
                controller_name,
                action_name,
                request_method,
                browser,
                device,
                he_dieu_hanh,
                ket_qua
                )
                VALUES
                (?,?,?,?,?,?,?,?,?,?,?,?,?,?)";

                var stmt = new SimpleStatement(
                    cql,
                    string.IsNullOrWhiteSpace(data.Username) ? "khach_vang_lai" : data.Username.Trim().ToLower(),
                    ngay,
                    gioVn,
                    Guid.NewGuid(),
                    data.HanhDong ?? "",
                    data.ChiTiet ?? "",
                    data.IpAddress ?? "",
                    data.ControllerName ?? "",
                    data.ActionName ?? "",
                    data.RequestMethod ?? "",
                    data.Browser ?? "",
                    data.Device ?? "",
                    data.HeDieuHanh ?? "",
                    data.KetQua ?? "");

                session.Execute(stmt);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
    }
}
