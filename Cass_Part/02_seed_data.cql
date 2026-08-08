-- ============================================================================
-- DỮ LIỆU MẪU (SEED) - CHỈ ĐỂ KIỂM TRA NHANH TRÊN TABLEPLUS
-- Chạy SAU khi đã chạy 01_create_database.cql thành công.
-- Dữ liệu khớp với DatVeXemPhim.sql: KhachHangID 1-5, LichChieuID 1-8.
-- Sau khi chạy website thật, dữ liệu thật sẽ INSERT tiếp vào (không xung đột).
-- ============================================================================

USE cinema_history;

-- ============================================================================
-- PHẦN 1: lich_su_ghe — Seat Reservation Timeline
-- Demo 2 vòng đời:
--   Ghế A1 (GheID=1) của LichChieuID=1: LOCK -> BOOKED -> CHECK_IN (hoàn thành)
--   Ghế A2 (GheID=2) của LichChieuID=1: LOCK -> CANCEL (hủy)
-- ============================================================================

-- Ghế A1: Vòng đời hoàn chỉnh LOCK -> BOOKED -> CHECK_IN
INSERT INTO lich_su_ghe
    (lich_chieu_id, ghe_id, thoi_gian, id,
     trang_thai, khach_hang_id, don_dat_ve_id, ghi_chu,
     controller_name, action_name, request_method,
     browser, device, he_dieu_hanh, ip_address, ket_qua)
VALUES (1, 1, '2026-08-06T19:50:00.000+07:00', uuid(),
        'LOCK', 1, null, 'User chon ghe A1 tren so do',
        'DatVe', 'ConfirmSeats', 'POST',
        'Chrome', 'Desktop', 'Windows', '127.0.0.1', 'SUCCESS');

INSERT INTO lich_su_ghe
    (lich_chieu_id, ghe_id, thoi_gian, id,
     trang_thai, khach_hang_id, don_dat_ve_id, ghi_chu,
     controller_name, action_name, request_method,
     browser, device, he_dieu_hanh, ip_address, ket_qua)
VALUES (1, 1, '2026-08-06T19:51:20.000+07:00', uuid(),
        'BOOKED', 1, 1, 'Thanh toan OTP thanh cong - DonDatVeID=1',
        'ThanhToan', 'ProcessPayment', 'POST',
        'Chrome', 'Desktop', 'Windows', '127.0.0.1', 'SUCCESS');

INSERT INTO lich_su_ghe
    (lich_chieu_id, ghe_id, thoi_gian, id,
     trang_thai, khach_hang_id, don_dat_ve_id, ghi_chu,
     controller_name, action_name, request_method,
     browser, device, he_dieu_hanh, ip_address, ket_qua)
VALUES (1, 1, '2026-08-06T20:15:00.000+07:00', uuid(),
        'CHECK_IN', 1, 1, 'Quet ve vao rap - check in thanh cong',
        'Admin', 'CheckIn', 'POST',
        'Chrome', 'Desktop', 'Windows', '127.0.0.1', 'SUCCESS');

-- Ghế A2: Vòng đời LOCK -> CANCEL
INSERT INTO lich_su_ghe
    (lich_chieu_id, ghe_id, thoi_gian, id,
     trang_thai, khach_hang_id, don_dat_ve_id, ghi_chu,
     controller_name, action_name, request_method,
     browser, device, he_dieu_hanh, ip_address, ket_qua)
VALUES (1, 2, '2026-08-06T19:52:00.000+07:00', uuid(),
        'LOCK', 2, null, 'User chon ghe A2 tren so do',
        'DatVe', 'ConfirmSeats', 'POST',
        'Firefox', 'Mobile', 'Android', '192.168.1.10', 'SUCCESS');

INSERT INTO lich_su_ghe
    (lich_chieu_id, ghe_id, thoi_gian, id,
     trang_thai, khach_hang_id, don_dat_ve_id, ghi_chu,
     controller_name, action_name, request_method,
     browser, device, he_dieu_hanh, ip_address, ket_qua)
VALUES (1, 2, '2026-08-06T19:53:30.000+07:00', uuid(),
        'CANCEL', 2, null, 'User huy giao dich truoc khi thanh toan',
        'ThanhToan', 'CancelTransaction', 'POST',
        'Firefox', 'Mobile', 'Android', '192.168.1.10', 'SUCCESS');


-- ============================================================================
-- PHẦN 2: lich_su_dat_ve — Booking Lifecycle
-- KhachHangID=1 (Lê Hoàng Long): đặt vé DonDatVeID=1 thành công
-- ============================================================================
INSERT INTO lich_su_dat_ve
    (khach_hang_id, thoi_gian, id, don_dat_ve_id,
     ma_dat_ve, buoc, lich_chieu_id, so_ghe, tong_tien, ghi_chu)
VALUES (1, '2026-08-06T19:50:00.000+07:00', uuid(), null,
        '', 'LOCK_SEAT', 1, 2, 0.00, 'Khoa 2 ghe: A1, A3');

INSERT INTO lich_su_dat_ve
    (khach_hang_id, thoi_gian, id, don_dat_ve_id,
     ma_dat_ve, buoc, lich_chieu_id, so_ghe, tong_tien, ghi_chu)
VALUES (1, '2026-08-06T19:51:20.000+07:00', uuid(), 1,
        'DV00000001', 'PAYMENT_SUCCESS', 1, 2, 190000.00, 'Thanh toan 2 ghe - OTP hop le');

INSERT INTO lich_su_dat_ve
    (khach_hang_id, thoi_gian, id, don_dat_ve_id,
     ma_dat_ve, buoc, lich_chieu_id, so_ghe, tong_tien, ghi_chu)
VALUES (1, '2026-08-06T19:51:22.000+07:00', uuid(), 1,
        'DV00000001', 'XUAT_VE', 1, 2, 190000.00, 'Ve dien tu da gui qua Gmail');


-- ============================================================================
-- PHẦN 3: nhat_ky_hoat_dong — User Activity Log
-- user: kh1_long, ngày: 2026-08-06
-- Bao phủ đủ flow: REGISTER -> LOGIN -> SEARCH -> VIEW -> BOOK -> PAYMENT -> CHANGE_PASSWORD -> LOGOUT
-- ============================================================================

-- REGISTER (user mới - không phải kh1_long, để demo FAILED)
INSERT INTO nhat_ky_hoat_dong
    (username, ngay, thoi_gian, id,
     hanh_dong, ket_qua, chi_tiet,
     ip_address, controller_name, action_name, request_method,
     browser, device, he_dieu_hanh)
VALUES ('test_e2e_01', '2026-08-06', '2026-08-06T19:44:00.000+07:00', uuid(),
        'REGISTER', 'SUCCESS', 'Dang ky tai khoan thanh cong',
        '127.0.0.1', 'Register', 'DangKy', 'POST',
        'Chrome', 'Desktop', 'Windows');

INSERT INTO nhat_ky_hoat_dong
    (username, ngay, thoi_gian, id,
     hanh_dong, ket_qua, chi_tiet,
     ip_address, controller_name, action_name, request_method,
     browser, device, he_dieu_hanh)
VALUES ('test_e2e_01', '2026-08-06', '2026-08-06T19:44:30.000+07:00', uuid(),
        'REGISTER', 'FAILED', 'That bai: Ten dang nhap nay da duoc su dung.',
        '127.0.0.1', 'Register', 'DangKy', 'POST',
        'Chrome', 'Desktop', 'Windows');

-- LOGIN
INSERT INTO nhat_ky_hoat_dong
    (username, ngay, thoi_gian, id,
     hanh_dong, ket_qua, chi_tiet,
     ip_address, controller_name, action_name, request_method,
     browser, device, he_dieu_hanh)
VALUES ('kh1_long', '2026-08-06', '2026-08-06T19:45:00.000+07:00', uuid(),
        'LOGIN', 'SUCCESS', 'Dang nhap thanh cong',
        '127.0.0.1', 'Login', 'Login', 'POST',
        'Chrome', 'Desktop', 'Windows');

-- SEARCH_MOVIE SUCCESS
INSERT INTO nhat_ky_hoat_dong
    (username, ngay, thoi_gian, id,
     hanh_dong, ket_qua, chi_tiet,
     ip_address, controller_name, action_name, request_method,
     browser, device, he_dieu_hanh)
VALUES ('kh1_long', '2026-08-06', '2026-08-06T19:46:00.000+07:00', uuid(),
        'SEARCH_MOVIE', 'SUCCESS', 'Tim kiem: "Vung dat" - 3 ket qua',
        '127.0.0.1', 'Home', 'TimKiemPhim', 'POST',
        'Chrome', 'Desktop', 'Windows');

-- SEARCH_MOVIE FAILED
INSERT INTO nhat_ky_hoat_dong
    (username, ngay, thoi_gian, id,
     hanh_dong, ket_qua, chi_tiet,
     ip_address, controller_name, action_name, request_method,
     browser, device, he_dieu_hanh)
VALUES ('kh1_long', '2026-08-06', '2026-08-06T19:46:30.000+07:00', uuid(),
        'SEARCH_MOVIE', 'FAILED', 'Tim kiem: "zzzxyz123" - 0 ket qua',
        '127.0.0.1', 'Home', 'TimKiemPhim', 'POST',
        'Chrome', 'Desktop', 'Windows');

-- VIEW_MOVIE_DETAIL
INSERT INTO nhat_ky_hoat_dong
    (username, ngay, thoi_gian, id,
     hanh_dong, ket_qua, chi_tiet,
     ip_address, controller_name, action_name, request_method,
     browser, device, he_dieu_hanh)
VALUES ('kh1_long', '2026-08-06', '2026-08-06T19:47:00.000+07:00', uuid(),
        'VIEW_MOVIE_DETAIL', 'SUCCESS', 'Xem chi tiet phim: Vung dat chet choc (PhimID=1)',
        '127.0.0.1', 'Home', 'ChiTietPhim', 'GET',
        'Chrome', 'Desktop', 'Windows');

-- VIEW_SHOWTIME
INSERT INTO nhat_ky_hoat_dong
    (username, ngay, thoi_gian, id,
     hanh_dong, ket_qua, chi_tiet,
     ip_address, controller_name, action_name, request_method,
     browser, device, he_dieu_hanh)
VALUES ('kh1_long', '2026-08-06', '2026-08-06T19:49:00.000+07:00', uuid(),
        'VIEW_SHOWTIME', 'SUCCESS', 'Xem suat chieu phim: Vung dat chet choc (PhimID=1)',
        '127.0.0.1', 'DatVe', 'ChonSuat', 'GET',
        'Chrome', 'Desktop', 'Windows');

-- LOCK_SEAT
INSERT INTO nhat_ky_hoat_dong
    (username, ngay, thoi_gian, id,
     hanh_dong, ket_qua, chi_tiet,
     ip_address, controller_name, action_name, request_method,
     browser, device, he_dieu_hanh)
VALUES ('kh1_long', '2026-08-06', '2026-08-06T19:50:00.000+07:00', uuid(),
        'LOCK_SEAT', 'SUCCESS', 'Khoa ghe: A1,A3 (LichChieuID=1)',
        '127.0.0.1', 'DatVe', 'ConfirmSeats', 'POST',
        'Chrome', 'Desktop', 'Windows');

-- OTP_REQUEST
INSERT INTO nhat_ky_hoat_dong
    (username, ngay, thoi_gian, id,
     hanh_dong, ket_qua, chi_tiet,
     ip_address, controller_name, action_name, request_method,
     browser, device, he_dieu_hanh)
VALUES ('kh1_long', '2026-08-06', '2026-08-06T19:51:00.000+07:00', uuid(),
        'OTP_REQUEST', 'SUCCESS', 'Gui OTP thanh cong den Gmail kh1_long@gmail.com',
        '127.0.0.1', 'ThanhToan', 'SendOtp', 'POST',
        'Chrome', 'Desktop', 'Windows');

-- PAYMENT_SUCCESS
INSERT INTO nhat_ky_hoat_dong
    (username, ngay, thoi_gian, id,
     hanh_dong, ket_qua, chi_tiet,
     ip_address, controller_name, action_name, request_method,
     browser, device, he_dieu_hanh)
VALUES ('kh1_long', '2026-08-06', '2026-08-06T19:51:20.000+07:00', uuid(),
        'PAYMENT_SUCCESS', 'SUCCESS', 'Thanh toan thanh cong - DonDatVeID=1 - 190000 VND',
        '127.0.0.1', 'ThanhToan', 'ProcessPayment', 'POST',
        'Chrome', 'Desktop', 'Windows');

-- CHANGE_PASSWORD FAILED (sai mat khau cu)
INSERT INTO nhat_ky_hoat_dong
    (username, ngay, thoi_gian, id,
     hanh_dong, ket_qua, chi_tiet,
     ip_address, controller_name, action_name, request_method,
     browser, device, he_dieu_hanh)
VALUES ('kh1_long', '2026-08-06', '2026-08-06T19:55:00.000+07:00', uuid(),
        'CHANGE_PASSWORD', 'FAILED', 'That bai: sai mat khau hien tai',
        '127.0.0.1', 'ThongTinCaNhan', 'DoiMatKhau', 'POST',
        'Chrome', 'Desktop', 'Windows');

-- CHANGE_PASSWORD FAILED (mat khau moi trung mat khau cu)
INSERT INTO nhat_ky_hoat_dong
    (username, ngay, thoi_gian, id,
     hanh_dong, ket_qua, chi_tiet,
     ip_address, controller_name, action_name, request_method,
     browser, device, he_dieu_hanh)
VALUES ('kh1_long', '2026-08-06', '2026-08-06T19:55:30.000+07:00', uuid(),
        'CHANGE_PASSWORD', 'FAILED', 'That bai: mat khau moi khong duoc trung mat khau cu',
        '127.0.0.1', 'ThongTinCaNhan', 'DoiMatKhau', 'POST',
        'Chrome', 'Desktop', 'Windows');

-- CHANGE_PASSWORD SUCCESS
INSERT INTO nhat_ky_hoat_dong
    (username, ngay, thoi_gian, id,
     hanh_dong, ket_qua, chi_tiet,
     ip_address, controller_name, action_name, request_method,
     browser, device, he_dieu_hanh)
VALUES ('kh1_long', '2026-08-06', '2026-08-06T19:56:00.000+07:00', uuid(),
        'CHANGE_PASSWORD', 'SUCCESS', 'Doi mat khau thanh cong',
        '127.0.0.1', 'ThongTinCaNhan', 'DoiMatKhau', 'POST',
        'Chrome', 'Desktop', 'Windows');

-- LOGOUT
INSERT INTO nhat_ky_hoat_dong
    (username, ngay, thoi_gian, id,
     hanh_dong, ket_qua, chi_tiet,
     ip_address, controller_name, action_name, request_method,
     browser, device, he_dieu_hanh)
VALUES ('kh1_long', '2026-08-06', '2026-08-06T20:00:00.000+07:00', uuid(),
        'LOGOUT', 'SUCCESS', 'Nguoi dung dang xuat thanh cong',
        '127.0.0.1', 'Login', 'SignOut', 'GET',
        'Chrome', 'Desktop', 'Windows');


-- ============================================================================
-- QUERY KIỂM TRA SAU KHI SEED (chạy trong cqlsh hoặc TablePlus)
-- ============================================================================
-- 1. Seat timeline ghế A1 (LichChieuID=1, GheID=1) - phải thấy LOCK -> BOOKED -> CHECK_IN
-- SELECT trang_thai, khach_hang_id, ghi_chu, thoi_gian
--   FROM lich_su_ghe WHERE lich_chieu_id = 1 AND ghe_id = 1;

-- 2. Seat timeline ghế A2 bị hủy - phải thấy LOCK -> CANCEL
-- SELECT trang_thai, khach_hang_id, ghi_chu, thoi_gian
--   FROM lich_su_ghe WHERE lich_chieu_id = 1 AND ghe_id = 2;

-- 3. Lịch sử đặt vé khách hàng ID=1
-- SELECT buoc, tong_tien, ghi_chu, thoi_gian
--   FROM lich_su_dat_ve WHERE khach_hang_id = 1;

-- 4. Toàn bộ activity log của kh1_long ngày 2026-08-06
-- SELECT hanh_dong, ket_qua, chi_tiet, thoi_gian
--   FROM nhat_ky_hoat_dong WHERE username = 'kh1_long' AND ngay = '2026-08-06';

-- 5. Activity log của test_e2e_01 (REGISTER SUCCESS + FAILED)
-- SELECT hanh_dong, ket_qua, chi_tiet, thoi_gian
--   FROM nhat_ky_hoat_dong WHERE username = 'test_e2e_01' AND ngay = '2026-08-06';
