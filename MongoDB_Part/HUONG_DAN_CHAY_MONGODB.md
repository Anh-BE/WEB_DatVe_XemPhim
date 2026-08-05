# HƯỚNG DẪN CHẠY VÀ KIỂM THỬ PHẦN MONGODB (`Mgdb`)

Tài liệu hướng dẫn riêng cho thành viên phụ trách **MongoDB** trong Đồ án NoSQL.

---

## 🚀 1. Khởi chạy MongoDB qua Docker Desktop

Mở Terminal / PowerShell tại thư mục gốc dự án (`WEB_DatVe_XemPhim`) và chạy duy nhất lệnh:

```bash
docker compose up -d mongodb
```

- **Host/Port:** `localhost:27017`
- **Username:** `admin`
- **Password:** `adminpassword`
- **Database:** `CinemaNoSQL`

---

## 🛠️ 2. Nhập Dữ liệu mẫu & Nộp bài cho Thầy Cô

### Cách 1: Chạy Script tự động bằng Command Line (`mongosh`) ✅ **Khuyến nghị**
Trong PowerShell / Terminal tại thư mục gốc dự án, chạy lệnh sau:
```bash
docker exec -i docker_mongodb mongosh -u admin -p adminpassword --authenticationDatabase admin < ./MongoDB_Part/MongoDB_Scripts_NopBai.js
```
> Script sẽ tự động tạo database `CinemaNoSQL`, tạo đầy đủ 2 collection và nạp toàn bộ dữ liệu mẫu.

### Cách 2: Dùng MongoDB Compass (Giao diện đồ họa GUI)
1. Mở **MongoDB Compass**.
2. Nhập URI kết nối: `mongodb://admin:adminpassword@localhost:27017` → Chọn **Connect**.
3. Tạo Database tên: `CinemaNoSQL`.
4. Tạo 2 Collection: **`cinema_promotions`** và **`customer_feedbacks`**.
5. Vào từng collection → Chọn **Add Data** → **Import JSON File** → Chọn file [MongoDB_Data_Seed.json](./MongoDB_Data_Seed.json).

---

## 📦 3. Các Collection trong Database `CinemaNoSQL`

| Collection | Mô tả | Trang Web |
|---|---|---|
| `cinema_promotions` | Kho Voucher & Mã khuyến mãi rạp phim | `/MgdbPromotion` |
| `customer_feedbacks` | Phản hồi & Khiếu nại của khách hàng | `/MgdbCustomerFeedback` |

---

## 🌐 4. Đường dẫn các Trang Web MongoDB (`Mgdb`)

Khi khởi chạy ứng dụng Web C# (`doan3`), bạn có thể truy cập các đường dẫn sau:

1. **Trang Voucher Khuyến mãi (MongoDB Voucher):** `http://localhost:XXXX/MgdbPromotion`
   - Xem danh sách voucher, lọc theo chuyên mục
   - Đăng nhập để lấy mã voucher về ví cá nhân
   - Admin: Thêm/Xóa voucher, xem thống kê Aggregation Pipeline

2. **Trang Hỗ trợ & Khiếu nại:** `http://localhost:XXXX/MgdbCustomerFeedback`
   - Khách hàng gửi yêu cầu hỗ trợ sự cố
   - Admin: Xem toàn bộ khiếu nại, **tìm kiếm theo Username / Email / SĐT**

---

## 📁 5. Danh sách các File của bạn (Tiền tố `Mgdb`)

- **Document Scripts:** `MongoDB_Part/MongoDB_Scripts_NopBai.js`, `MongoDB_Part/MongoDB_Data_Seed.json`
- **C# Models & Service:** `doan3/Models/Mgdb/MgdbPromotionModel.cs`, `doan3/Models/Mgdb/MgdbCustomerFeedbackModel.cs`, `doan3/Models/Mgdb/MgdbService.cs`
- **C# Controllers:** `doan3/Controllers/MgdbPromotionController.cs`, `doan3/Controllers/MgdbCustomerFeedbackController.cs`
- **Razor Views:** `doan3/Views/MgdbPromotion/Index.cshtml`, `doan3/Views/MgdbCustomerFeedback/Index.cshtml`

---

## ⚠️ 6. Lưu ý khi kéo code từ GitHub về

Sau khi `git clone` hoặc `git pull`, MongoDB **KHÔNG tự có dữ liệu**. Bạn cần:

1. Chạy `docker compose up -d mongodb` để khởi động container MongoDB
2. Chạy lệnh import script ở **Mục 2** để nạp dữ liệu mẫu vào database
3. Khởi động dự án Visual Studio và truy cập các trang `/MgdbPromotion` hoặc `/MgdbCustomerFeedback`
