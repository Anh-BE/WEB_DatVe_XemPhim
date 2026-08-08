-- ============================================================================
-- APACHE CASSANDRA - MODULE LỊCH SỬ / LOG / TIMELINE (ĐỘC LẬP)
-- Website Đặt Vé Xem Phim (doan3)
-- SQL Server vẫn là CSDL chính. Cassandra CHỈ lưu Log / Timeline / History.
-- Chạy file này 1 lần duy nhất để khởi tạo Keyspace + 3 bảng.
-- ============================================================================

-- KEYSPACE: cinema_history
-- SimpleStrategy + replication_factor=1: phù hợp môi trường 1 node (Docker dev/local).
CREATE KEYSPACE IF NOT EXISTS cinema_history
WITH replication = {
    'class': 'SimpleStrategy',
    'replication_factor': 1
};

USE cinema_history;

-- ============================================================================
-- BẢNG 1: lich_su_ghe — LỊCH SỬ THAY ĐỔI TRẠNG THÁI GHẾ (SEAT RESERVATION TIMELINE)
-- ----------------------------------------------------------------------------
-- Vòng đời: LOCK -> BOOKED -> CHECK_IN   hoặc   LOCK -> CANCEL
--
-- QUERY CHÍNH:
--   "Lịch sử 1 GHẾ trong 1 SUẤT CHIẾU theo thứ tự thời gian tăng dần"
--   SELECT * FROM lich_su_ghe WHERE lich_chieu_id = ? AND ghe_id = ?;
--
-- PARTITION KEY: (lich_chieu_id, ghe_id)
--   -> Mỗi cặp (suất chiếu, ghế) = 1 partition riêng, tối đa vài dòng.
--   -> Ghi song song cực nhanh dù có 100.000 người cùng chọn ghế.
--
-- CLUSTERING KEY: thoi_gian ASC, id ASC
--   -> ASC: xem timeline theo thứ tự LOCK -> BOOKED -> CHECK_IN/CANCEL
--   -> id (uuid) chống trùng khi 2 sự kiện cùng millisecond
--
-- HOT PARTITION: KHÔNG có. Số dòng/partition bị giới hạn tự nhiên bởi số
-- trạng thái trong vòng đời (thường 2-4 dòng/ghế/suất).
-- ============================================================================
CREATE TABLE IF NOT EXISTS lich_su_ghe (
    lich_chieu_id    bigint,
    ghe_id           bigint,
    thoi_gian        timestamp,
    id               uuid,
    -- Trạng thái: LOCK | BOOKED | CHECK_IN | CANCEL
    trang_thai       text,
    khach_hang_id    bigint,
    don_dat_ve_id    bigint,
    ghi_chu          text,
    -- Thông tin kỹ thuật (điền đầy đủ để tránh NULL gây lỗi INSERT)
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
-- ----------------------------------------------------------------------------
-- QUERY CHÍNH:
--   "Lịch sử đặt vé của 1 KHÁCH HÀNG, mới nhất trước"
--   SELECT * FROM lich_su_dat_ve WHERE khach_hang_id = ?;
--
-- PARTITION KEY: khach_hang_id
-- CLUSTERING KEY: thoi_gian DESC, id ASC
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
-- BẢNG 3: nhat_ky_hoat_dong — NHẬT KÝ HOẠT ĐỘNG NGƯỜI DÙNG (USER ACTIVITY LOG)
-- ----------------------------------------------------------------------------
-- Hành động: REGISTER|LOGIN|LOGOUT|SEARCH_MOVIE|VIEW_MOVIE_DETAIL|
--            VIEW_MOVIE_BY_GENRE|VIEW_CINEMA|VIEW_SHOWTIME|
--            LOCK_SEAT|BOOK_SEAT|CANCEL_SEAT|
--            PAYMENT_REQUEST|OTP_REQUEST|PAYMENT_SUCCESS|PAYMENT_FAILED|
--            CHANGE_PASSWORD
--
-- Kết quả: SUCCESS | FAILED
--
-- QUERY CHÍNH:
--   "Hoạt động của 1 USER trong 1 NGÀY"
--   SELECT * FROM nhat_ky_hoat_dong WHERE username = ? AND ngay = ?;
--
-- PARTITION KEY: (username, ngay)
--   -> Bucket theo NGÀY (format 'yyyy-MM-dd', giờ VN UTC+7)
--   -> Chặn Hot Partition: kể cả khach_vang_lai cũng chỉ phình theo ngày,
--      không vô hạn.
--
-- CLUSTERING KEY: thoi_gian DESC, id ASC
--   -> Hoạt động mới nhất lên đầu khi query không có ORDER BY
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
-- ALTER TABLE: Thêm cột còn thiếu nếu đã tạo bảng từ phiên bản cũ hơn
-- Lệnh này an toàn: nếu cột đã tồn tại sẽ báo lỗi nhưng không làm hỏng dữ liệu.
-- Bạn có thể chạy lại toàn bộ file này nhiều lần mà không lo.
-- ============================================================================

-- Thêm cột kỹ thuật vào lich_su_ghe (nếu bảng tạo từ schema cũ thiếu cột)
ALTER TABLE lich_su_ghe ADD controller_name text;
ALTER TABLE lich_su_ghe ADD action_name     text;
ALTER TABLE lich_su_ghe ADD request_method  text;
ALTER TABLE lich_su_ghe ADD browser         text;
ALTER TABLE lich_su_ghe ADD device          text;
ALTER TABLE lich_su_ghe ADD he_dieu_hanh    text;
ALTER TABLE lich_su_ghe ADD ip_address      text;
ALTER TABLE lich_su_ghe ADD ket_qua         text;

-- Thêm cột kỹ thuật vào nhat_ky_hoat_dong (nếu bảng tạo từ schema cũ)
ALTER TABLE nhat_ky_hoat_dong ADD ket_qua          text;
ALTER TABLE nhat_ky_hoat_dong ADD controller_name  text;
ALTER TABLE nhat_ky_hoat_dong ADD action_name      text;
ALTER TABLE nhat_ky_hoat_dong ADD request_method   text;
ALTER TABLE nhat_ky_hoat_dong ADD browser          text;
ALTER TABLE nhat_ky_hoat_dong ADD device           text;
ALTER TABLE nhat_ky_hoat_dong ADD he_dieu_hanh     text;
