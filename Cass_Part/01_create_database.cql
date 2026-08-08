-- ============================================================================
-- APACHE CASSANDRA - MODULE LỊCH SỬ / LOG / TIMELINE (ĐỘC LẬP)
-- Website Đặt Vé Xem Phim (MOVANA CINEMA)
-- ============================================================================

CREATE KEYSPACE IF NOT EXISTS cinema_history
WITH replication = {
    'class': 'SimpleStrategy',
    'replication_factor': 1
};

USE cinema_history;

-- ============================================================================
-- BẢNG 1: lich_su_ghe — LỊCH SỬ THAY ĐỔI TRẠNG THÁI GHẾ
-- ============================================================================
CREATE TABLE IF NOT EXISTS lich_su_ghe (
    lich_chieu_id    bigint,
    ghe_id           bigint,
    thoi_gian        timestamp,
    id               uuid,
    trang_thai       text,
    khach_hang_id    bigint,
    don_dat_ve_id    bigint,
    ghi_chu          text,
    controller_name  text,
    action_name      text,
    request_method   text,
    browser          text,
    device           text,
    he_dieu_hanh     text,
    ip_address       text,
    ket_qua          text,
    PRIMARY KEY ((lich_chieu_id, ghe_id), thoi_gian, id)
) WITH CLUSTERING ORDER BY (thoi_gian ASC, id ASC)
  AND comment = 'Seat Reservation Timeline: LOCK -> BOOKED -> CHECK_IN | LOCK -> CANCEL';

-- ============================================================================
-- BẢNG 2: lich_su_dat_ve — LỊCH SỬ ĐẶT VÉ CỦA KHÁCH HÀNG
-- ============================================================================
CREATE TABLE IF NOT EXISTS lich_su_dat_ve (
    khach_hang_id   bigint,
    thoi_gian       timestamp,
    id              uuid,
    don_dat_ve_id   bigint,
    ma_dat_ve       text,
    buoc            text,
    lich_chieu_id   bigint,
    so_ghe          int,
    tong_tien       decimal,
    ghi_chu         text,
    PRIMARY KEY ((khach_hang_id), thoi_gian, id)
) WITH CLUSTERING ORDER BY (thoi_gian DESC, id ASC)
  AND comment = 'Booking lifecycle: Dat ve -> Thanh toan -> Xuat ve -> Huy';

-- ============================================================================
-- BẢNG 3: nhat_ky_hoat_dong — NHẬT KÝ HOẠT ĐỘNG NGƯỜI DÙNG
-- ============================================================================
CREATE TABLE IF NOT EXISTS nhat_ky_hoat_dong (
    username         text,
    ngay             text,
    thoi_gian        timestamp,
    id               uuid,
    hanh_dong        text,
    ket_qua          text,
    chi_tiet         text,
    ip_address       text,
    controller_name  text,
    action_name      text,
    request_method   text,
    browser          text,
    device           text,
    he_dieu_hanh     text,
    PRIMARY KEY ((username, ngay), thoi_gian, id)
) WITH CLUSTERING ORDER BY (thoi_gian DESC, id ASC)
  AND comment = 'User Activity Log: REGISTER|LOGIN|LOGOUT|SEARCH|BOOK|PAYMENT|CHANGE_PASSWORD - SUCCESS/FAILED';

-- ============================================================================
-- DỮ LIỆU MẪU (SEED DATA)
-- ============================================================================

-- ---------- 1) lich_su_ghe ----------
INSERT INTO lich_su_ghe (lich_chieu_id, ghe_id, thoi_gian, id, trang_thai, khach_hang_id, don_dat_ve_id, ghi_chu)
VALUES (1, 1, '2026-08-06T19:50:00.000+07:00', uuid(), 'Giu ghe', 1, null, 'Khach giu ghe A1 tren so do');

INSERT INTO lich_su_ghe (lich_chieu_id, ghe_id, thoi_gian, id, trang_thai, khach_hang_id, don_dat_ve_id, ghi_chu)
VALUES (1, 1, '2026-08-06T19:51:20.000+07:00', uuid(), 'Thanh toan', 1, 1, 'Thanh toan thanh cong don DV00000001');

INSERT INTO lich_su_ghe (lich_chieu_id, ghe_id, thoi_gian, id, trang_thai, khach_hang_id, don_dat_ve_id, ghi_chu)
VALUES (1, 1, '2026-08-06T19:51:21.000+07:00', uuid(), 'Xuat ve', 1, 1, 'Xuat ve thanh cong');


-- ---------- 2) lich_su_dat_ve ----------
INSERT INTO lich_su_dat_ve (khach_hang_id, thoi_gian, id, don_dat_ve_id, ma_dat_ve, buoc, lich_chieu_id, so_ghe, tong_tien, ghi_chu)
VALUES (1, '2026-08-06T19:51:20.000+07:00', uuid(), 1, 'DV00000001', 'Thanh toan', 1, 2, 190000.00, 'Thanh toan 2 ghe thuong');

INSERT INTO lich_su_dat_ve (khach_hang_id, thoi_gian, id, don_dat_ve_id, ma_dat_ve, buoc, lich_chieu_id, so_ghe, tong_tien, ghi_chu)
VALUES (1, '2026-08-06T19:51:21.000+07:00', uuid(), 1, 'DV00000001', 'Xuat ve', 1, 2, 190000.00, 'Ve dien tu da gui qua Gmail');


-- ---------- 3) nhat_ky_hoat_dong ----------
INSERT INTO nhat_ky_hoat_dong (username, ngay, thoi_gian, id, hanh_dong, chi_tiet, ip_address)
VALUES ('kh1_long', '2026-08-06', '2026-08-06T19:45:00.000+07:00', uuid(), 'Dang nhap', 'Dang nhap thanh cong', '127.0.0.1');

INSERT INTO nhat_ky_hoat_dong (username, ngay, thoi_gian, id, hanh_dong, chi_tiet, ip_address)
VALUES ('kh1_long', '2026-08-06', '2026-08-06T19:47:10.000+07:00', uuid(), 'Xem chi tiet phim', 'PhimID=1 - Vung dat chet choc', '127.0.0.1');

INSERT INTO nhat_ky_hoat_dong (username, ngay, thoi_gian, id, hanh_dong, chi_tiet, ip_address)
VALUES ('kh1_long', '2026-08-06', '2026-08-06T19:51:21.000+07:00', uuid(), 'Thanh toan', 'Don DV00000001 - 190000 VND', '127.0.0.1');
