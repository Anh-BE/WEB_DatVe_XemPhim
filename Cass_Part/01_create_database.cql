-- ============================================================================
-- APACHE CASSANDRA - MODULE LỊCH SỬ / LOG / TIMELINE (ĐỘC LẬP)
-- Website Đặt Vé Xem Phim (doan3)
-- SQL Server vẫn là CSDL chính. Cassandra CHỈ lưu Log / Timeline / History.
-- Chạy file này 1 lần duy nhất để khởi tạo Keyspace + 3 bảng.
-- ============================================================================

-- KEYSPACE: cinema_history
-- Lý do đặt tên: mô tả đúng bản chất dữ liệu chứa trong đó (lịch sử/nhật ký),
-- tách biệt hoàn toàn với CSDL nghiệp vụ chính (LTW_DatVeXemPhim trên SQL Server).
-- SimpleStrategy + replication_factor = 1: phù hợp môi trường 1 node (Docker dev/local).
-- Khi triển khai cụm nhiều node/nhiều datacenter thật, đổi sang NetworkTopologyStrategy.
CREATE KEYSPACE IF NOT EXISTS cinema_history
WITH replication = {
    'class': 'SimpleStrategy',
    'replication_factor': 1
};

USE cinema_history;

-- ============================================================================
-- BẢNG 1: lich_su_ghe — LỊCH SỬ THAY ĐỔI TRẠNG THÁI GHẾ
-- ------------------------------------------------------------------------------
-- Vòng đời tham khảo: Giu ghe -> Thanh toan -> Xuat ve -> Check-in -> Hoan thanh
--
-- QUERY CHÍNH:
--   "Xem toàn bộ lịch sử trạng thái của 1 GHẾ trong 1 SUẤT CHIẾU cụ thể, mới nhất trước"
--   SELECT * FROM lich_su_ghe WHERE lich_chieu_id = ? AND ghe_id = ?;
--
-- PARTITION KEY: (lich_chieu_id, ghe_id)
--   -> Mỗi cặp (suất chiếu, ghế) là 1 partition riêng biệt, rất nhỏ (tối đa vài chục
--      dòng vì vòng đời 1 ghế trong 1 suất chỉ có vài trạng thái).
--
-- CLUSTERING KEY: thoi_gian DESC, id ASC
--   -> Trạng thái mới nhất lên đầu; "id" (uuid) chống trùng khi 2 sự kiện xảy ra
--      cùng millisecond (ví dụ thanh toán nhiều ghế cùng lúc).
--
-- HOT PARTITION: KHÔNG có nguy cơ.
--   -> Số dòng/partition bị chặn cứng bởi số trạng thái trong vòng đời vé, hoàn
--      toàn không phụ thuộc lượng truy cập đồng thời (kể cả khi 1 suất chiếu có
--      hàng chục nghìn người chọn ghế thì mỗi ghế vẫn chỉ có 1 partition riêng).
-- ============================================================================
CREATE TABLE IF NOT EXISTS lich_su_ghe (
    lich_chieu_id   bigint,
    ghe_id          bigint,
    thoi_gian       timestamp,
    id              uuid,
    trang_thai      text,
    khach_hang_id   bigint,
    don_dat_ve_id   bigint,
    ghi_chu         text,
    PRIMARY KEY ((lich_chieu_id, ghe_id), thoi_gian, id)
) WITH CLUSTERING ORDER BY (thoi_gian DESC, id ASC)
  AND comment = 'Vong doi trang thai ghe: Giu ghe -> Thanh toan -> Xuat ve -> Check-in -> Hoan thanh';


-- ============================================================================
-- BẢNG 2: lich_su_dat_ve — LỊCH SỬ ĐẶT VÉ CỦA KHÁCH HÀNG
-- ------------------------------------------------------------------------------
-- Các bước tham khảo: Dat ve -> Thanh toan -> Xuat ve -> Email -> Hoan tien -> Huy -> Doi suat chieu
--
-- QUERY CHÍNH:
--   "Xem toàn bộ lịch sử đặt vé của 1 KHÁCH HÀNG, mới nhất trước"
--   SELECT * FROM lich_su_dat_ve WHERE khach_hang_id = ?;
--
-- PARTITION KEY: khach_hang_id
--   -> Khớp trực tiếp với tên chức năng "lịch sử đặt vé CỦA KHÁCH HÀNG".
--
-- CLUSTERING KEY: thoi_gian DESC, id ASC
--   -> Bước mới nhất lên đầu; id chống trùng khi nhiều bước chạy gần như đồng thời
--      (VD: Dat ve + Thanh toan + Xuat ve xảy ra liên tiếp trong 1 giao dịch).
--
-- HOT PARTITION: THẤP trong quy mô đồ án/thực tế cá nhân (1 khách hàng đặt vé
--   tối đa vài trăm lần/năm). Nếu triển khai cho khách hàng doanh nghiệp/đại lý
--   đặt vé số lượng cực lớn nhiều năm liền, có thể mở rộng bucket theo năm:
--   PRIMARY KEY ((khach_hang_id, nam), thoi_gian, id) — không cần thay đổi ngay
--   vì vượt quá quy mô yêu cầu hiện tại, chỉ ghi chú làm hướng mở rộng.
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
  AND comment = 'Cac buoc: Dat ve -> Thanh toan -> Xuat ve -> Email -> Hoan tien -> Huy -> Doi suat chieu';


-- ============================================================================
-- BẢNG 3: nhat_ky_hoat_dong — NHẬT KÝ HOẠT ĐỘNG NGƯỜI DÙNG
-- ------------------------------------------------------------------------------
-- Ví dụ hành động: Dang nhap, Dang xuat, Tim phim, Xem chi tiet phim, Chon ghe,
--                  Huy chon ghe, Thanh toan, Doi mat khau...
--
-- QUERY CHÍNH:
--   "Xem các hoạt động của 1 USER trong 1 NGÀY cụ thể, mới nhất trước"
--   SELECT * FROM nhat_ky_hoat_dong WHERE username = ? AND ngay = ?;
--
-- PARTITION KEY: (username, ngay)  -- ngay dạng 'yyyy-MM-dd' theo giờ Việt Nam
--   -> Đây là bảng CÓ NGUY CƠ HOT PARTITION CAO NHẤT trong 3 bảng vì 1 user có
--      thể đăng nhập/tìm phim/xem chi tiết/chọn ghế rất nhiều lần trong ngày,
--      và các thao tác chưa đăng nhập đều gộp vào 1 "username" dùng chung
--      (khach_vang_lai) nên rất dễ phình to nếu không xử lý.
--
-- XỬ LÝ HOT PARTITION: bucket theo NGÀY.
--   -> Mỗi partition chỉ chứa hoạt động của ĐÚNG 1 ngày => kích thước bị chặn
--      trần tự nhiên theo thời gian, không phình vô hạn dù chạy nhiều năm.
--   -> Muốn xem lịch sử nhiều ngày liên tiếp thì lặp truy vấn theo từng ngày ở
--      tầng ứng dụng (đúng tinh thần "Query First" của Cassandra, không JOIN).
--
-- CLUSTERING KEY: thoi_gian DESC, id ASC
-- ============================================================================
CREATE TABLE IF NOT EXISTS nhat_ky_hoat_dong (
    username        text,
    ngay            text,
    thoi_gian       timestamp,
    id              uuid,
    hanh_dong       text,
    chi_tiet        text,
    ip_address      text,
    PRIMARY KEY ((username, ngay), thoi_gian, id)
) WITH CLUSTERING ORDER BY (thoi_gian DESC, id ASC)
  AND comment = 'Dang nhap, Dang xuat, Tim phim, Xem chi tiet, Chon ghe, Huy chon ghe, Thanh toan, Doi mat khau';
