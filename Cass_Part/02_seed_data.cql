-- ============================================================================
-- DỮ LIỆU MẪU (SEED) - CHỈ ĐỂ KIỂM TRA NHANH TRÊN TABLEPLUS
-- Dữ liệu khớp với dữ liệu mẫu trong DatVeXemPhim.sql (KhachHangID 1-5,
-- username kh1_long..kh5_cuong, LichChieuID 1-8).
-- Sau khi chạy website thật, dữ liệu thật sẽ được ghi thêm vào (INSERT nối tiếp),
-- không ảnh hưởng các dòng seed này.
-- ============================================================================

USE cinema_history;

-- ---------- 1) lich_su_ghe: vòng đời ghế A1 (GheID=1) của LichChieuID=1 ----------
INSERT INTO lich_su_ghe (lich_chieu_id, ghe_id, thoi_gian, id, trang_thai, khach_hang_id, don_dat_ve_id, ghi_chu)
VALUES (1, 1, '2026-08-06T19:50:00.000+07:00', uuid(), 'Giu ghe', 1, null, 'Khach giu ghe A1 tren so do');

INSERT INTO lich_su_ghe (lich_chieu_id, ghe_id, thoi_gian, id, trang_thai, khach_hang_id, don_dat_ve_id, ghi_chu)
VALUES (1, 1, '2026-08-06T19:51:20.000+07:00', uuid(), 'Thanh toan', 1, 1, 'Thanh toan thanh cong don DV00000001');

INSERT INTO lich_su_ghe (lich_chieu_id, ghe_id, thoi_gian, id, trang_thai, khach_hang_id, don_dat_ve_id, ghi_chu)
VALUES (1, 1, '2026-08-06T19:51:21.000+07:00', uuid(), 'Xuat ve', 1, 1, 'Xuat ve thanh cong');

-- ---------- 2) lich_su_dat_ve: khách hàng KhachHangID = 1 (Lê Hoàng Long) ----------
INSERT INTO lich_su_dat_ve (khach_hang_id, thoi_gian, id, don_dat_ve_id, ma_dat_ve, buoc, lich_chieu_id, so_ghe, tong_tien, ghi_chu)
VALUES (1, '2026-08-06T19:51:20.000+07:00', uuid(), 1, 'DV00000001', 'Thanh toan', 1, 2, 190000.00, 'Thanh toan 2 ghe thuong');

INSERT INTO lich_su_dat_ve (khach_hang_id, thoi_gian, id, don_dat_ve_id, ma_dat_ve, buoc, lich_chieu_id, so_ghe, tong_tien, ghi_chu)
VALUES (1, '2026-08-06T19:51:21.000+07:00', uuid(), 1, 'DV00000001', 'Xuat ve', 1, 2, 190000.00, 'Ve dien tu da gui qua Gmail');

-- ---------- 3) nhat_ky_hoat_dong: user kh1_long trong ngày 2026-08-06 ----------
INSERT INTO nhat_ky_hoat_dong (username, ngay, thoi_gian, id, hanh_dong, chi_tiet, ip_address)
VALUES ('kh1_long', '2026-08-06', '2026-08-06T19:45:00.000+07:00', uuid(), 'Dang nhap', 'Dang nhap thanh cong', '127.0.0.1');

INSERT INTO nhat_ky_hoat_dong (username, ngay, thoi_gian, id, hanh_dong, chi_tiet, ip_address)
VALUES ('kh1_long', '2026-08-06', '2026-08-06T19:47:10.000+07:00', uuid(), 'Xem chi tiet phim', 'PhimID=1 - Vung dat chet choc', '127.0.0.1');

INSERT INTO nhat_ky_hoat_dong (username, ngay, thoi_gian, id, hanh_dong, chi_tiet, ip_address)
VALUES ('kh1_long', '2026-08-06', '2026-08-06T19:51:21.000+07:00', uuid(), 'Thanh toan', 'Don DV00000001 - 190000 VND', '127.0.0.1');

-- ---------- Kiểm tra nhanh sau khi seed ----------
-- SELECT * FROM lich_su_ghe WHERE lich_chieu_id = 1 AND ghe_id = 1;
-- SELECT * FROM lich_su_dat_ve WHERE khach_hang_id = 1;
-- SELECT * FROM nhat_ky_hoat_dong WHERE username = 'kh1_long' AND ngay = '2026-08-06';
