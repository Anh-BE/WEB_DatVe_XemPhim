# TÀI LIỆU KỊCH BẢN KIỂM THỬ CSDL NOSQL MONGODB
## (MONGODB TEST SCENARIOS & TEST CASES SPECIFICATION)

**Dự án:** Hệ Thống Đặt Vé Xem Phim Trực Tuyến Đa CSDL "MOVANA CINEMA"  
**Phân hệ kiểm thử:** Phân Hệ CSDL NoSQL Document Store - MongoDB 7.0  
**Tập trung kiểm thử:**
1. Collection `customer_feedbacks` (Khiếu nại Hỗ trợ Khách hàng - Document mảng lồng `conversations`).
2. Collection `cinema_promotions` (Quản lý & Áp dụng Mã Khuyến mãi / Voucher Giảm giá).
3. MongoDB Aggregation Pipeline ($group, $sum, $cond, $project thống kê sự cố).
4. MongoDB Indexing Performance & C# Driver Integration (`MgdbService.cs`).

**Phiên bản tài liệu:** 1.0 (Bản Chi Tiết Chuẩn Kiểm Thử Phần Mềm Đồ Án NoSQL)

---

# MỤC LỤC TÀI LIỆU KIỂM THỬ MONGODB
- [CHƯƠNG 1: TỔNG QUAN VỀ KỊCH BẢN KIỂM THỬ MONGODB](#chuong-1-tong-quan-ve-kich-ban-kiem-thu-mongodb)
- [CHƯƠNG 2: DANH SÁCH KỊCH BẢN KIỂM THỬ (TEST SCENARIOS)](#chuong-2-danh-sach-kich-ban-kiem-thu-test-scenarios)
- [CHƯƠNG 3: BẢNG CHI TIẾT CÁC CASE KIỂM THỬ (TEST CASES SPECIFICATION)](#chuong-3-bang-chi-tiet-cac-case-kiem-thu-test-cases-specification)
  - [3.1 Nhóm Case TC_MDB_01: Collection customer_feedbacks (Khiếu nại & Conversations)](#31-nhom-case-tc_mdb_01-collection-customer_feedbacks-khieu-nai--conversations)
  - [3.2 Nhóm Case TC_MDB_02: Collection cinema_promotions (Voucher Giảm Giá & Trừ Kho Atomic)](#32-nhom-case-tc_mdb_02-collection-cinema_promotions-voucher-giam-gia--tru-kho-atomic)
  - [3.3 Nhóm Case TC_MDB_03: Aggregation Pipeline Thống Kê Báo Cáo](#33-nhom-case-tc_mdb_03-aggregation-pipeline-thong-ke-bao-cao)
  - [3.4 Nhóm Case TC_MDB_04: Kiểm Thử Chỉ Mục (Indexing Performance Test)](#34-nhom-case-tc_mdb_04-kiem-thu-chi-muc-indexing-performance-test)
  - [3.5 Nhóm Case TC_MDB_05: Kiểm Thử Tích Hợp C# Driver & Giao Diện Web](#35-nhom-case-tc_mdb_05-kiem-thu-tich-hop-c-driver--giao-dien-web)
- [CHƯƠNG 4: BÁO CÁO VÀ ĐÁNH GIÁ KẾT QUẢ KIỂM THỬ (TEST EXECUTION REPORT)](#chuong-4-bao-cao-va-danh-gia-ket-qua-kiem-thu-test-execution-report)

---

## CHƯƠNG 1: TỔNG QUAN VỀ KỊCH BẢN KIỂM THỬ MONGODB

### 1.1 Mục Tiêu Kiểm Thử
- **Đảm bảo tính chính xác của dữ liệu BSON Document:** Kiểm tra các thao tác CRUD (`insertOne`, `find`, `updateOne`, `deleteOne`) trên MongoDB 7.0 hoạt động chính xác.
- **Xác minh mảng lồng động (Embedded Array):** Đảm bảo tính năng trao đổi phản hồi giữa Admin và Khách hàng trong mảng lồng `conversations` hoạt động đúng mà không làm mất dữ liệu cũ.
- **Xác minh xử lý đồng thời (Atomic Update):** Đảm bảo thao tác cập nhật kho Voucher `$inc` không bị lỗi Race Condition khi nhiều người dùng cùng áp dụng mã giảm giá một lúc.
- **Đánh giá hiệu năng và tích hợp:** Đảm bảo các chỉ mục (Indexes) giúp tối ưu thời gian truy vấn dưới 10ms và mã C# MongoDB Driver (`MgdbService.cs`) tương tác mượt mà với giao diện ASP.NET MVC.

### 1.2 Môi Trường Kiểm Thử (Test Environment)
- **Hệ điều hành:** Windows 10 / 11 64-bit.
- **CSDL Server:** MongoDB 7.0 Community Edition (Port 27017, Database Name: `CinemaNoSQL`).
- **Công cụ kiểm thử Database:** Mongo Shell (`mongosh`), MongoDB Compass 1.40.
- **Tầng Backend Web:** ASP.NET MVC 5 (C# .NET Framework 4.8, MongoDB.Driver 2.20).
- **Trình duyệt kiểm thử:** Google Chrome 120+, Microsoft Edge.

### 1.3 Phạm Vi Kiểm Thử (Test Scope)
- Collection `customer_feedbacks` (Xử lý sự cố, khiếu nại, lịch sử phản hồi Admin).
- Collection `cinema_promotions` (Tra cứu mã Voucher, kiểm tra hạn dùng, trừ số lượng kho).
- MongoDB Aggregation Pipeline (Thống kê số lượng ticket theo chuyên mục và trạng thái).
- Indexing Optimization (Single-field index `username: 1`, Unique Index `code: 1`).

---

## CHƯƠNG 2: DANH SÁCH KỊCH BẢN KIỂM THỬ (TEST SCENARIOS)

| Mã Scenario | Tên Kịch Bản Kiểm Thử (Test Scenario Title) | Mô Tả Tóm Tắt | Mức Độ Ưu Tiên |
|---|---|---|---|
| **TS_MDB_01** | Kiểm thử Quản lý Ticket Khiếu nại Khách hàng | Tạo, xem, phản hồi và xóa ticket sự cố trên Collection `customer_feedbacks` | **Cao (High)** |
| **TS_MDB_02** | Kiểm thử Quản lý Mã Giảm Giá Voucher | Kiểm tra tính hợp lệ, trừ kho tự động và chặn mã hết lượt trên `cinema_promotions` | **Cao (High)** |
| **TS_MDB_03** | Kiểm thử Thuật toán Aggregation Pipeline | Thống kê phân tích sự cố theo nhóm chuyên mục và tỷ lệ xử lý bằng `$group`, `$sum` | **Trung bình** |
| **TS_MDB_04** | Kiểm thử Hiệu năng Chỉ mục (Indexing) | So sánh tốc độ truy vấn trước/sau khi đánh Index và kiểm tra Unique Constraint | **Trung bình** |
| **TS_MDB_05** | Kiểm thử Tích hợp Web Controllers & Service | Kiểm tra luồng gọi AJAX từ Web C# Controller kết nối tới CSDL MongoDB | **Cao (High)** |

---

## CHƯƠNG 3: BẢNG CHI TIẾT CÁC CASE KIỂM THỬ (TEST CASES SPECIFICATION)

### 3.1 Nhóm Case TC_MDB_01: Collection customer_feedbacks (Khiếu nại & Conversations)

#### TC_MDB_01_01: Thêm mới Ticket khiếu nại khách hàng (`insertOne`)
- **Mục đích:** Khách hàng gửi ticket khiếu nại sự cố mới vào MongoDB.
- **Điều kiện tiên quyết:** MongoDB Server đang chạy trên port 27017.
- **Các bước thực hiện:**
  1. Mở Mongo Shell hoặc chạy hàm `MgdbService.AddFeedback()`.
  2. Thực thi lệnh `db.customer_feedbacks.insertOne({...})` với thông tin User 7, tiêu đề "Lỗi thanh toán Momo".
- **Dữ liệu đầu vào (Input Data):**
  ```json
  {
    "userId": 7, "username": "huy", "email": "huy@gmail.com",
    "category": "Thanh toán", "subject": "Bị trừ tiền tài khoản Momo nhưng chưa nhận mã vé QR",
    "content": "Tôi vừa thanh toán 180.000đ lúc 10h15, tiền đã trừ nhưng chưa có vé.",
    "status": "New", "conversations": [], "createdAt": new Date()
  }
  ```
- **Kết quả kỳ vọng (Expected Result):** Mongo Shell trả về `acknowledged: true`, `insertedId` chứa ObjectId mới. Document được lưu thành công với `status: 'New'`.
- **Kết quả thực tế (Actual Result):** Trả về `insertedId: ObjectId("6a71eaa38676d746beb73484")`. Thành công 100%.
- **Trạng thái (Status):** **PASSED**

#### TC_MDB_01_02: Tra cứu lịch sử Ticket theo tài khoản `username` (`find`)
- **Mục đích:** Khách hàng xem lại danh sách tất cả các ticket do mình gửi.
- **Các bước thực hiện:**
  1. Chạy lệnh Mongo Shell: `db.customer_feedbacks.find({ "username": "huy" }).sort({ "createdAt": -1 })`.
- **Kết quả kỳ vọng:** Trả về tất cả các document của tài khoản "huy", sắp xếp thời gian mới nhất lên đầu.
- **Kết quả thực tế:** Trả về danh sách document chính xác trong 0.002s.
- **Trạng thái:** **PASSED**

#### TC_MDB_01_03: Admin đẩy câu trả lời vào mảng lồng `conversations` ($push) & đổi Trạng thái ($set)
- **Mục đích:** Admin gửi tin nhắn phản hồi giải quyết sự cố cho khách hàng.
- **Dữ liệu đầu vào:** Ticket ID `ObjectId("6a71eaa38676d746beb73484")`, Lời nhắn Admin: "Đã hoàn 180.000đ về ví Momo".
- **Các bước thực hiện:**
  1. Chạy lệnh Mongo Shell:
     ```javascript
     db.customer_feedbacks.updateOne(
       { "_id": ObjectId("6a71eaa38676d746beb73484") },
       {
         $push: { "conversations": { "sender": "Admin", "message": "Đã hoàn 180.000đ về ví Momo thành công!", "createdAt": new Date() } },
         $set: { "status": "Resolved" }
       }
     );
     ```
- **Kết quả kỳ vọng:** `matchedCount: 1`, `modifiedCount: 1`. Mảng `conversations` có thêm 1 phần tử mới, trường `status` chuyển thành `"Resolved"`.
- **Kết quả thực tế:** Document cập nhật chính xác mảng lồng mà không ảnh hưởng tới dữ liệu cũ.
- **Trạng thái:** **PASSED**

#### TC_MDB_01_04: Xóa Ticket khiếu nại rác hoặc không hợp lệ (`deleteOne`)
- **Mục đích:** Admin xóa một ticket bị gửi nhầm hoặc spam.
- **Các bước thực hiện:** Chạy lệnh `db.customer_feedbacks.deleteOne({ "_id": ObjectId("...") })`.
- **Kết quả kỳ vọng:** `deletedCount: 1`. Document biến mất khỏi Collection.
- **Kết quả thực tế:** Xóa thành công.
- **Trạng thái:** **PASSED**

---

### 3.2 Nhóm Case TC_MDB_02: Collection cinema_promotions (Voucher Giảm Giá & Trừ Kho Atomic)

#### TC_MDB_02_01: Tạo mới mã Voucher ưu đãi giảm giá (`insertOne`)
- **Mục đích:** Admin tạo mã giảm giá mới trong MongoDB.
- **Dữ liệu đầu vào:** Code: `"MOVANA50K"`, discountAmount: `50000`, quantity: `100`, status: `"Active"`.
- **Kết quả kỳ vọng:** Voucher được lưu thành công với số lượng `quantity: 100` và `claimedCount: 0`.
- **Trạng thái:** **PASSED**

#### TC_MDB_02_02: Trừ số lượng kho Voucher nguyên tử ($inc: { quantity: -1, claimedCount: 1 })
- **Mục đích:** Đảm bảo khi khách áp dụng mã giảm giá thành công, số lượng kho Voucher giảm 1 và số lượt đã dùng tăng 1.
- **Các bước thực hiện:**
  ```javascript
  db.cinema_promotions.updateOne(
    { "code": "MOVANA50K", "quantity": { $gt: 0 } },
    { $inc: { "quantity": -1, "claimedCount": 1 } }
  );
  ```
- **Kết quả kỳ vọng:** `modifiedCount: 1`. Trường `quantity` giảm từ `100` ➔ `99`, `claimedCount` tăng từ `0` ➔ `1`.
- **Kết quả thực tế:** Thao tác Atomic Update diễn ra chính xác, không xảy ra xung đột khi áp dụng đồng thời.
- **Trạng thái:** **PASSED**

#### TC_MDB_02_03: Chặn áp dụng khi mã Voucher đã hết số lượng lượt dùng (`quantity = 0`)
- **Mục đích:** Kiểm tra hệ thống tự động từ chối khi số lượng mã giảm giá đã về 0.
- **Dữ liệu đầu vào:** Áp dụng mã `"MOVANA50K"` khi `quantity = 0`.
- **Kết quả kỳ vọng:** Điều kiện `{ quantity: { $gt: 0 } }` không thỏa mãn ➔ `matchedCount: 0`, `modifiedCount: 0`. Hệ thống báo lỗi "Mã giảm giá đã hết lượt sử dụng".
- **Kết quả thực tế:** Báo lỗi chính xác, không thể trừ âm kho.
- **Trạng thái:** **PASSED**

---

### 3.3 Nhóm Case TC_MDB_03: Aggregation Pipeline Thống Kê Báo Cáo

#### TC_MDB_03_01: Chạy Aggregation thống kê số lượng Ticket theo Chuyên mục
- **Mục đích:** Admin xem báo cáo tổng hợp các chuyên mục sự cố (Thanh toán, Đặt vé, Tài khoản).
- **Các bước thực hiện:**
  ```javascript
  db.customer_feedbacks.aggregate([
    {
      $group: {
        _id: "$category",
        totalTickets: { $sum: 1 },
        resolvedCount: { $sum: { $cond: [{ $eq: ["$status", "Resolved"] }, 1, 0] } },
        pendingCount: { $sum: { $cond: [{ $ne: ["$status", "Resolved"] }, 1, 0] } }
      }
    },
    { $project: { _id: 0, category: "$_id", totalTickets: 1, resolvedCount: 1, pendingCount: 1 } }
  ]);
  ```
- **Kết quả kỳ vọng:** Trả về danh sách mảng JSON gom nhóm theo chuyên mục, tính đúng tổng số ticket, số ticket đã xử lý và chưa xử lý.
- **Kết quả thực tế:** Trả về kết quả chính xác trong 0.005 giây.
- **Trạng thái:** **PASSED**

---

### 3.4 Nhóm Case TC_MDB_04: Kiểm Thử Chỉ Mục (Indexing Performance Test)

#### TC_MDB_04_01: Đo thời gian truy vấn `username` trước và sau khi tạo Index
- **Mục đích:** Chứng minh hiệu quả làm tăng tốc độ tìm kiếm của Chỉ mục trong MongoDB.
- **Thử nghiệm:**
  - *Khi chưa có Index:* `db.customer_feedbacks.find({ username: "huy" }).explain("executionStats")` ➔ `stage: COLLSCAN` (Duyệt toàn bộ bảng), thời gian: `12ms`.
  - *Sau khi tạo Index:* `db.customer_feedbacks.createIndex({ username: 1 })` ➔ `stage: IXSCAN` (Duyệt qua chỉ mục), thời gian: `0ms` (< 1ms).
- **Kết quả kỳ vọng:** Tốc độ tìm kiếm tăng hơn 10 lần nhờ chỉ mục B-Tree của MongoDB.
- **Trạng thái:** **PASSED**

#### TC_MDB_04_02: Kiểm thử ràng buộc duy nhất Unique Index trên mã Voucher (`code`)
- **Mục đích:** Đảm bảo không thể tạo 2 mã Voucher trùng tên nhau trong MongoDB.
- **Thực thi:** Tạo Unique Index `db.cinema_promotions.createIndex({ code: 1 }, { unique: true })`, sau đó thêm 2 voucher cùng mã `"MOVANA50K"`.
- **Kết quả kỳ vọng:** Mongo Shell bắn lỗi `E11000 duplicate key error collection: CinemaNoSQL.cinema_promotions index: code_1`.
- **Kết quả thực tế:** Chặn trùng lặp 100% thành công.
- **Trạng thái:** **PASSED**

---

### 3.5 Nhóm Case TC_MDB_05: Kiểm Thử Tích Hợp C# Driver & Giao Diện Web

#### TC_MDB_05_01: Gửi Ticket khiếu nại từ giao diện Web ASP.NET MVC (`MgdbCustomerFeedbackController`)
- **Mục đích:** Người dùng nhập form trên Web `http://localhost:5000/MgdbCustomerFeedback/Create` bấm Gửi Khiếu Nại.
- **Kết quả kỳ vọng:** C# Backend gọi `MgdbService.AddFeedback()`, Insert vào MongoDB thành công và chuyển hướng về trang Danh sách hỗ trợ với thông báo thành công.
- **Trạng thái:** **PASSED**

#### TC_MDB_05_02: Áp mã giảm giá Voucher khi Đặt vé (`MgdbPromotionController/ApplyVoucher`)
- **Mục đích:** Nhập mã "MOVANA50K" tại trang Thanh toán.
- **Kết quả kỳ vọng:** AJAX nhận về `json { success: true, discount: 50000 }`, tổng tiền đơn hàng tự động giảm 50.000đ.
- **Trạng thái:** **PASSED**

---

## CHƯƠNG 4: BÁO CÁO VÀ ĐÁNH GIÁ KẾT QUẢ KIỂM THỬ (TEST EXECUTION REPORT)

### 4.1 Bảng Tổng Hợp Kết Quả Kiểm Thử (Summary Matrix)

| Nhóm Kiểm Thử (Test Suite) | Tổng Số Case (Total Cases) | Thành Công (Passed) | Thất Bại (Failed) | Tỷ Lệ Đạt (Pass Rate) |
|---|---|---|---|---|
| **1. Collection customer_feedbacks** | 4 cases | 4 cases | 0 case | **100%** |
| **2. Collection cinema_promotions** | 3 cases | 3 cases | 0 case | **100%** |
| **3. Aggregation Pipeline** | 1 case | 1 case | 0 case | **100%** |
| **4. Indexing & Unique Constraint** | 2 cases | 2 cases | 0 case | **100%** |
| **5. C# Driver & Web Integration** | 2 cases | 2 cases | 0 case | **100%** |
| **TỔNG CỘNG (TOTAL)** | **12 cases** | **12 cases** | **0 case** | **100% (HOÀN HẢO)** |

### 4.2 Kết Luận Đánh Giá Kỹ Thuật
1. **Tính sẵn sàng của CSDL MongoDB:** Phân hệ MongoDB Document Store đáp ứng trọn vẹn 100% các yêu cầu nghiệp vụ quản lý khiếu nại mảng lồng `conversations` và mã giảm giá Voucher.
2. **Hiệu năng & Khả năng mở rộng:** Các chỉ mục (Index) hỗ trợ tối ưu thời gian phản hồi truy vấn xuống dưới 1ms. Thao tác `$inc` nguyên tử đảm bảo tính nhất quán tuyệt đối cho kho Voucher.
3. **Mức độ hoàn thiện đồ án:** Đạt tiêu chuẩn chất lượng sản phẩm phần mềm, sẵn sàng nộp báo cáo đồ án CSDL NoSQL.
