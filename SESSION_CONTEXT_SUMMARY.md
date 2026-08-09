# TÀI LIỆU TỔNG HỢP & KÍCH HOẠT CONTEXT CHO PHIÊN LÀM VIỆC TIẾP THEO
# Dự án: Web Đặt Vé Xem Phim (WebDatVeXemPhim / doan3)
# Công nghệ: ASP.NET MVC 5 (.NET Framework 4.5), C#, MS SQL Server, Redis Cache, Gmail SMTP

---

## I. TỔNG QUAN DỰ ÁN & CÁC THÀNH PHẦN ĐÃ HOÀN THÀNH

### 1. Thông tin cấu hình môi trường:
* **Đường dẫn thư mục dự án:** `C:\Users\Le Ngoc Anh\Desktop\09_WebDatVeXemPhim\09_WebDatVeXemPhim\doan3`
* **Sinh viên thực hiện:** 
  1. Tăng Gia Huy
  2. Lê Ngọc Anh
  3. Đào Trọng Nguyên Vũ
* **GitHub Repository:** `https://github.com/Anh-BE/WEB_DatVe_XemPhim.git` (Nhánh: `main`)
* **CSDL Quan hệ (RDBMS):** MS SQL Server 2022 Instance `LAPTOP-6APG6279\SQLSERVER2022`, Database `LTW_DatVeXemPhim`.
* **CSDL NoSQL 1 (Redis):** Running `localhost:6379`.
* **Tài khoản Gmail SMTP:** `anh874343@gmail.com` (App Password: `lzdt llmo pwex jjlb`).

---

## II. CHI TIẾT 4 TÍNH NĂNG REDIS ĐÃ HOÀN THIỆN (100% VERIFIED)

| # | Tính năng Redis | Tên Key trên Redis | TTL (Thời gian sống) | Mô tả & Cách thức xử lý |
| :---: | :--- | :--- | :---: | :--- |
| 1 | **Khóa ghế tạm thời** | `seatlock:{lichChieuId}:{gheId}` | 90 giây (1.5 phút) | Khóa nguyên tử (Atomic Lock) giữ ghế khi chọn trên sơ đồ, tự giải phóng sau 90s. |
| 2 | **Giỏ hàng thanh toán** | `cart:{username}` *(Hash)* | 600 giây (10 phút) | Lưu tạm danh sách ghế, suất chiếu và tổng tiền chờ thanh toán. |
| 3 | **Xác thực OTP Gmail** | `otp:checkout:{username}` | 120 giây (2 phút) | Sinh mã OTP 6 số, lưu Redis đếm ngược 120s và gửi Mail thật qua Gmail SMTP. Xóa ngay sau khi dùng. |
| 4 | **Redis User Session** | `session:user:{username}` *(Hash)* | 1800 giây (30 phút) | Lưu phiên đăng nhập người dùng trên RAM. Xóa khi người dùng Sign Out. |

---

## III. TỆP TIN VÀ CÁC THAY ĐỔI NỔI BẬT TRONG MÃ NGUỒN

1. **`doan3/Web.config`**:
   - Cấu hình ConnectionString SQL Server: `data source=LAPTOP-6APG6279\SQLSERVER2022`.
   - Cấu hình Gmail SMTP: `SmtpEmail = anh874343@gmail.com`, `SmtpPassword = lzdt llmo pwex jjlb`, `EnableRealGmailOtp = true`.
   - Cấu hình Redis: `RedisConnectionString = localhost:6379,abortConnect=false`.

2. **`doan3/Models/EmailService.cs`**:
   - Dịch vụ gửi Email HTML thương hiệu **Movana Cinema** với giao diện viền vàng mạ, chuẩn Tiếng Việt UTF-8 100%.

3. **`doan3/Models/RedisFeaturesService.cs` & `SeatLockService.cs`**:
   - Thư viện C# giao tiếp với Redis bằng `StackExchange.Redis`.

4. **`doan3/Controllers/LoginController.cs`**:
   - Tích hợp `SaveUserSession` khi đăng nhập thành công và `RemoveUserSession` khi đăng xuất.

5. **`doan3/Controllers/ThanhToanController.cs`**:
   - Tích hợp `SaveCart` khi vào trang thanh toán, `SendOtp` gửi mail thật + lưu Redis, `ClearCart` khi thanh toán xong hoặc hủy.

6. **`doan3/Controllers/RegisterController.cs` & `Views/Register/Index_DangKy.cshtml`**:
   - Kiểm tra định dạng Gmail hợp lệ (6-30 ký tự, đuôi `@gmail.com`).
   - Sửa lỗi escape Razor `@` thành `@@` trong JavaScript.
   - Chuẩn hóa mã hóa tất cả các file View `.cshtml` sang **UTF-8 với BOM**.

7. **`Redis_Scripts_NopBai.redis`**:
   - Tệp chứa toàn bộ các lệnh CLI (`SET`, `GET`, `HSET`, `EXPIRE`, `TTL`, `DEL`) để nộp cho thầy cô.

---

## IV. ĐỀ BÀI ĐỒ ÁN NOSQL & NHIỆM VỤ TIẾP THEO (NEO4J)

* **Tài liệu tham chiếu:** `C:\Users\Le Ngoc Anh\Desktop\NoSQL_DoAn.docx`
* **Hạn nộp đồ án:** Trước ngày **06/08/2026**.
* **Nhiệm vụ tiếp theo (Neo4j Graph Database):**
  * **Tối thiểu 2 loại Node:** `:User` (Khách hàng) và `:Movie` (Bộ phim).
  * **Tối thiểu 3 loại Quan hệ (Relationships):**
    1. `(:User)-[:BOOKED { bookingId, seatCount, date }]->(:Movie)`
    2. `(:User)-[:FAVORITE]->(:Movie)`
    3. `(:User)-[:FRIEND_WITH]->(:User)`
  * **Mục tiêu:** Xây dựng tính năng **Gợi ý phim thông minh (Movie Recommendation Engine)** dựa trên bạn bè hoặc gu xem phim tương tự.

---

## V. HƯỚNG DẪN DÙNG TỆP NÀY ĐỂ KÍCH HOẠT CONTEXT TRONG PHIÊN MỚI

Khi bắt đầu một phiên làm việc mới (New Session), bạn chỉ cần gõ yêu cầu:
> *"Hãy đọc tệp SESSION_CONTEXT_SUMMARY.md trong thư mục dự án doan3 để nắm toàn bộ bối cảnh và tiếp tục làm phần Neo4j."*
