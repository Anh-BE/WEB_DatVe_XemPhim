# HƯỚNG DẪN CÀI ĐẶT & TEST MODULE APACHE CASSANDRA (ĐỘC LẬP)

Module này **chỉ bổ sung**, không sửa Redis / MongoDB / Neo4j / SQL Server đang có.
Nếu muốn gỡ bỏ hoàn toàn: xóa thư mục `Models/Cass`, xóa 3 dòng `<add key="Cassandra...">`
trong `Web.config`, xóa 5 dòng `<Compile Include="Models\Cass\...">` trong `doan3.csproj`,
gỡ NuGet package `CassandraCSharpDriver`, rồi bỏ các đoạn code gọi
`doan3.Models.Cass.CassandraFeaturesService...` trong 5 Controller đã chèn — website vẫn
chạy bình thường với SQL Server + Redis + Mongo + Neo4j như cũ.

---

## BƯỚC 1 — Bật container Cassandra (đã có sẵn trong docker-compose.yml, không sửa file)

```bash
docker compose up -d cassandra
```

Kiểm tra container đã sẵn sàng (thường mất 30–60 giây để CQL native port mở):

```bash
docker exec -it docker_cassandra cqlsh -e "describe keyspaces"
```

Khi lệnh trên chạy được (không báo lỗi kết nối) nghĩa là Cassandra đã sẵn sàng nhận CQL.

---

## BƯỚC 2 — Tạo Keyspace + 3 bảng

```bash
docker exec -it docker_cassandra cqlsh -f /dev/stdin < Scripts/Cassandra/01_create_database.cql
```

Nếu lệnh trên báo lỗi vì file không nằm trong container, cách chắc ăn hơn: copy file vào container rồi chạy:

```bash
docker cp Scripts/Cassandra/01_create_database.cql docker_cassandra:/01_create_database.cql
docker exec -it docker_cassandra cqlsh -f /01_create_database.cql
```

Kiểm tra:

```bash
docker exec -it docker_cassandra cqlsh -e "DESCRIBE KEYSPACE cinema_history;"
```

Phải thấy đủ 3 bảng: `lich_su_ghe`, `lich_su_dat_ve`, `nhat_ky_hoat_dong`.

---

## BƯỚC 3 — (Tùy chọn) Nạp dữ liệu mẫu để test nhanh không cần chạy web

```bash
docker cp Scripts/Cassandra/02_seed_data.cql docker_cassandra:/02_seed_data.cql
docker exec -it docker_cassandra cqlsh -f /02_seed_data.cql
```

Kiểm tra ngay:

```bash
docker exec -it docker_cassandra cqlsh -k cinema_history -e "SELECT * FROM lich_su_dat_ve WHERE khach_hang_id = 1;"
```

---

## BƯỚC 4 — Kết nối TablePlus

TablePlus hỗ trợ Cassandra qua kết nối kiểu **Cassandra** (không phải Generic CQL):

* Host: `127.0.0.1`
* Port: `9042`
* Username / Password: để trống (Docker image mặc định `AllowAllAuthenticator`, không yêu cầu đăng nhập)
* Sau khi Connect, mở Keyspace `cinema_history` → sẽ thấy 3 bảng.

---

## BƯỚC 5 — Cài NuGet package cho project (Visual Studio)

Mở Solution trong Visual Studio → **Tools → NuGet Package Manager → Package Manager Console**,
chọn đúng project `doan3` rồi chạy:

```powershell
Install-Package CassandraCSharpDriver -Version 3.19.4 -ProjectName doan3
```

> Vì sao để Visual Studio/NuGet tự cài thay vì tôi tự chèn `<Reference>` thủ công: NuGet sẽ
> tự động (a) tải đúng file `.dll` thật của driver, (b) cập nhật `packages.config`,
> (c) thêm `<Reference>` với `HintPath` chính xác vào `doan3.csproj`, và (d) tự thêm các
> `bindingRedirect` cần thiết vào `<runtime><assemblyBinding>` trong `Web.config` nếu có
> xung đột version. Đây là thao tác **chỉ thêm**, không đụng tới các package
> Redis/Mongo/EF/DotNetOpenAuth đã cài — hoàn toàn an toàn.
>
> Dự án hiện đã có sẵn nhiều package nền tảng dùng chung với driver Cassandra (do MongoDB
> Driver mang theo): `System.Buffers`, `System.Memory`, `Microsoft.Extensions.Logging.Abstractions`,
> `System.Diagnostics.DiagnosticSource`... nên rủi ro xung đột version là thấp.

Sau khi cài xong, build lại Solution (Ctrl+Shift+B). Nếu Visual Studio báo lỗi bindingRedirect,
chọn "Yes" khi được hỏi có muốn tự động thêm binding redirect hay không (đây vẫn là thao tác
tự động chuẩn của Visual Studio, không phải tôi tự ý sửa tay).

Các file `.cs` mới (`CassandraService.cs`, `CassandraFeaturesService.cs`, 3 file DTO) đã được
đăng ký sẵn trong `doan3.csproj` (`<Compile Include>`), Visual Studio sẽ tự nhận diện khi mở lại
Solution — không cần Add Existing Item thủ công.

---

## BƯỚC 6 — Chạy website và test từng chức năng

### Test 1: Nhật ký hoạt động — Đăng nhập / Đăng xuất
1. Vào trang đăng nhập, đăng nhập bằng tài khoản mẫu, ví dụ `kh1_long` / `kh123`.
2. Mở TablePlus, chạy:
   ```sql
   SELECT * FROM nhat_ky_hoat_dong WHERE username = 'kh1_long' AND ngay = '2026-08-06';
   ```
   (thay `2026-08-06` bằng ngày hiện tại của bạn)
3. Phải thấy dòng mới `hanh_dong = 'Dang nhap'` xuất hiện ngay lập tức.
4. Bấm Đăng xuất → refresh TablePlus → thấy thêm dòng `hanh_dong = 'Dang xuat'`.

### Test 2: Nhật ký hoạt động — Tìm phim / Xem chi tiết phim
1. Vào trang Tìm kiếm phim, gõ tên phim bất kỳ, ví dụ "Ma Trận" → bấm Tìm.
2. Bấm vào 1 phim để xem Chi tiết phim.
3. TablePlus:
   ```sql
   SELECT * FROM nhat_ky_hoat_dong WHERE username = 'kh1_long' AND ngay = '2026-08-06';
   ```
   Sẽ thấy thêm các dòng `Tim phim` và `Xem chi tiet phim`.

### Test 3: Lịch sử ghế + Lịch sử đặt vé + Đổi mật khẩu (luồng đầy đủ)
1. Đăng nhập → chọn 1 phim → chọn suất chiếu → chọn 1-2 ghế trên sơ đồ → xác nhận.
   * Ngay tại bước này, TablePlus:
     ```sql
     SELECT * FROM lich_su_ghe WHERE lich_chieu_id = <id_suat_chieu> AND ghe_id = <id_ghe>;
     ```
     Đã thấy dòng `trang_thai = 'Giu ghe'`.
2. Ở trang Thanh toán, bấm "Lấy mã OTP" (OTP sẽ hiện trên UI ở chế độ Demo nếu chưa cấu hình
   Gmail SMTP thật), nhập OTP, bấm Thanh toán.
3. Sau khi thấy trang "Thanh toán thành công":
   * `SELECT * FROM lich_su_ghe WHERE lich_chieu_id = <id> AND ghe_id = <id_ghe>;`
     → có thêm 2 dòng mới: `Thanh toan`, `Xuat ve`.
   * `SELECT * FROM lich_su_dat_ve WHERE khach_hang_id = <id_khach_hang>;`
     → có 2 dòng mới: `Thanh toan`, `Xuat ve`.
   * `SELECT * FROM nhat_ky_hoat_dong WHERE username = 'kh1_long' AND ngay = '...';`
     → có thêm dòng `Thanh toan`.
4. Vào trang "Thông tin cá nhân" → "Đổi mật khẩu" → đổi thành công.
   * `SELECT * FROM nhat_ky_hoat_dong ...` → có thêm dòng `Doi mat khau`.
5. (Tùy chọn) Thử lại từ bước 1 nhưng ở trang Thanh toán bấm "Hủy giao dịch" thay vì thanh toán.
   * `SELECT * FROM nhat_ky_hoat_dong ...` → có thêm dòng `Huy chon ghe`.

### Test tình huống Cassandra OFFLINE (kiểm chứng "module độc lập")
1. `docker compose stop cassandra`
2. Thử đăng nhập / đặt vé / thanh toán như bình thường trên website.
3. Toàn bộ chức năng chính (SQL Server) **vẫn phải chạy đúng, không lỗi 500**, vì mọi lời gọi
   Cassandra đều được bọc try/catch và tự nuốt lỗi trong `CassandraFeaturesService`.
4. `docker compose start cassandra` → làm lại 1 hành động (ví dụ đăng nhập) → dữ liệu ghi tiếp
   bình thường, không cần restart ứng dụng web.

---

## GHI CHÚ VỀ TÌNH HUỐNG DỮ LIỆU BÙNG NỔ (Avengers/Doraemon mở bán, Flash Sale, Tết)

* **`lich_su_ghe`**: dù 100.000 người cùng chọn ghế trong 1 suất chiếu, mỗi ghế vẫn là 1
  partition `(lich_chieu_id, ghe_id)` riêng — ghi song song cực nhanh vì Cassandra phân tán
  các partition này ra nhiều node khác nhau bằng hashing, không có điểm nghẽn chung.
* So với SQL Server: nếu cố ghi hàng trăm nghìn dòng log/giây trực tiếp vào bảng
  `Chi_Tiet_Ve`/`Don_Dat_Ve` sẽ tạo áp lực lock/log file cực lớn lên chính CSDL giao dịch
  (ảnh hưởng luôn tốc độ đặt vé thật). Tách log time-series sang Cassandra giữ cho SQL Server
  chỉ phải xử lý đúng phần nghiệp vụ lõi (transaction đặt vé), còn lịch sử/nhật ký (ghi nhiều,
  gần như không update, không cần JOIN phức tạp) được đẩy sang hệ thống chuyên biệt cho ghi
  (write-optimized, LSM-tree) và mở rộng ngang (scale-out) dễ dàng bằng cách thêm node — điều
  SQL Server (scale-up) khó làm được với chi phí tương đương.
* Đây chính là lý do 500.000–1.000.000 dòng log trong dịp Tết/Flash Sale không làm chậm luồng
  thanh toán chính: ghi Cassandra luôn nằm SAU `giaoDich.Commit()` và được try/catch nuốt lỗi,
  nên kể cả khi Cassandra bị nghẽn tạm thời, người dùng vẫn thanh toán được bình thường, chỉ có
  thể mất/trễ một số dòng log không quan trọng bằng giao dịch tiền thật.
