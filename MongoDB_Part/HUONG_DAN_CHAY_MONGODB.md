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

### Cách 1: Dùng MongoDB Compass (Giao diện đồ họa GUI)
1. Mở **MongoDB Compass**.
2. Nhập URI kết nối: `mongodb://admin:adminpassword@localhost:27017` -> Chọn **Connect**.
3. Tạo Database tên: `CinemaNoSQL`.
4. Tạo 2 Collection: `movie_reviews` và `customer_feedbacks`.
5. Vào từng collection -> Chọn **Add Data** -> **Import JSON File** -> Chọn file [MongoDB_Data_Seed.json](file:///d:/DuLieuSinhVien/NOSQL/DoAnSQL/WEB_DatVe_XemPhim/MongoDB_Part/MongoDB_Data_Seed.json).

### Cách 2: Chạy Script tự động bằng Command Line (`mongosh`)
Trong PowerShell / Terminal, chạy lệnh:
```bash
docker exec -i docker_mongodb mongosh -u admin -p adminpassword --authenticationDatabase admin < ./MongoDB_Part/MongoDB_Scripts_NopBai.js
```

---

## 🌐 3. Đường dẫn các Trang Web MongoDB (`Mgdb`)

Khi khởi chạy ứng dụng Web C# (`doan3`), bạn có thể truy cập các đường dẫn cô lập sau:

1. **Trang Đánh giá & Bình luận phim:** `http://localhost:XXXX/MgdbMovieReview`
2. **Trang Phản hồi & Khiếu nại khách hàng:** `http://localhost:XXXX/MgdbCustomerFeedback`

---

## 📁 4. Danh sách các File của bạn (Tiền tố `Mgdb`)

- **Document Scripts:** `MongoDB_Part/MongoDB_Scripts_NopBai.js`, `MongoDB_Part/MongoDB_Data_Seed.json`
- **C# Models & Service:** `doan3/Models/Mgdb/MgdbMovieReviewModel.cs`, `doan3/Models/Mgdb/MgdbCustomerFeedbackModel.cs`, `doan3/Models/Mgdb/MgdbService.cs`
- **C# Controllers:** `doan3/Controllers/MgdbMovieReviewController.cs`, `doan3/Controllers/MgdbCustomerFeedbackController.cs`
- **Razor Views:** `doan3/Views/MgdbMovieReview/Index.cshtml`, `doan3/Views/MgdbCustomerFeedback/Index.cshtml`
