# TÀI LIỆU THIẾT KẾ PHẦN MỀM TỔNG THỂ HỆ THỐNG
## (SOFTWARE DESIGN SPECIFICATION - SDS)
**Dự án:** Hệ Thống Đặt Vé Xem Phim Trực Tuyến Đa CSDL "MOVANA CINEMA"  
**Công nghệ:** ASP.NET MVC 5 (C#), Entity Framework 6, HTML5/CSS3/Razor, Bootstrap.  
**Hệ CSDL Polyglot Persistence (5 Databases):**
1. **SQL Server 2019+** (RDBMS Core: Phim, Rạp, Suất chiếu, Vé, Hóa đơn)
2. **MongoDB 7.0** (Document Store: Phản hồi/Khiếu nại Khách hàng, Mã Khuyến mãi Voucher)
3. **Redis 7.0** (Key-Value Store: Tạm khóa giữ ghế Realtime, TTL Đếm ngược 5 phút)
4. **Neo4j 5.0** (Graph DB: Mạng đồ thị gợi ý Top phim đặt vé & Yêu thích nhiều nhất)
5. **Apache Cassandra 4.0** (Wide-Column Store: Lưu vết Logs nhật ký hoạt động Big Data)

**Phiên bản tài liệu:** 3.0 (Bản Chi Tiết Hoàn Chỉnh 100% Cho Toàn Bộ Hệ Thống)

---

# MỤC LỤC TÀI LIỆU SDS TỔNG THỂ
- [CHƯƠNG 1: TỔNG QUAN HỆ THỐNG VÀ KIẾN TRÚC TỔNG THỂ (SYSTEM OVERVIEW & ARCHITECTURE)](#chuong-1-tong-quan-he-thong-va-kien-truc-tong-the-system-overview--architecture)
- [CHƯƠNG 2: THIẾT KẾ CSDL QUAN HỆ SQL SERVER (RDBMS CORE DESIGN)](#chuong-2-thiet-ke-csdl-quan-he-sql-server-rdbms-core-design)
- [CHƯƠNG 3: THIẾT KẾ CSDL NOSQL MONGODB (DOCUMENT STORE DESIGN)](#chuong-3-thiet-ke-csdl-nosql-mongodb-document-store-design)
- [CHƯƠNG 4: THIẾT KẾ CSDL NOSQL REDIS (KEY-VALUE STORE DESIGN)](#chuong-4-thiet-ke-csdl-nosql-redis-key-value-store-design)
- [CHƯƠNG 5: THIẾT KẾ CSDL NOSQL NEO4J (GRAPH DATABASE DESIGN)](#chuong-5-thiet-ke-csdl-nosql-neo4j-graph-database-design)
- [CHƯƠNG 6: THIẾT KẾ CSDL NOSQL APACHE CASSANDRA (WIDE-COLUMN STORE DESIGN)](#chuong-6-thiet-ke-csdl-nosql-apache-cassandra-wide-column-store-design)
- [CHƯƠNG 7: THIẾT KẾ MÔ ĐUN XỬ LÝ VÀ SƠ ĐỒ LỚP (CLASS & COMPONENT DESIGN)](#chuong-7-thiet-ke-mo-dun-xu-ly-va-so-do-lop-class--component-design)
- [CHƯƠNG 8: QUY TRÌNH LUỒNG NGHIỆP VỤ HỆ THỐNG (SYSTEM SEQUENCE DIAGRAMS)](#chuong-8-quy-trinh-luong-nghiep-vu-he-thong-system-sequence-diagrams)

---

## CHƯƠNG 1: TỔNG QUAN HỆ THỐNG VÀ KIẾN TRÚC TỔNG THỂ (SYSTEM OVERVIEW & ARCHITECTURE)

### 1.1 Giới thiệu Đề tài và Bối cảnh Dự án
- **Tên dự án:** Hệ Thống Đặt Vé Xem Phim Trực Tuyến Đa CSDL "MOVANA CINEMA".
- **Bối cảnh xây dựng:** Ngành dịch vụ giải trí chiếu phim trực tuyến đòi hỏi hệ thống thông tin phải phục vụ hàng triệu lượt truy cập đồng thời, xử lý giao dịch tài chính chính xác (ACID), phản hồi tìm kiếm siêu tốc, gợi ý nội dung thông minh và lưu trữ nhật ký truy vết quy mô lớn (Big Data).
- **Giải pháp kĩ thuật:** Hệ thống áp dụng mô hình kiến trúc **Polyglot Persistence (Đa cơ sở dữ liệu)** kết hợp giữa CSDL Quan hệ SQL Server 2019 truyền thống và 4 hệ NoSQL hiện đại (MongoDB, Redis, Neo4j, Apache Cassandra) nhằm khai thác tối đa ưu thế vượt trội của từng công nghệ CSDL.

### 1.2 Mục tiêu Hệ thống
1. **Quản lý giao dịch cốt lõi (SQL Server):** Đảm bảo tính toàn vẹn dữ liệu tuyệt đối cho các thực thể Phim, Rạp, Suất chiếu, Vé và Hóa đơn tài chính.
2. **Khóa tạm giữ ghế đếm ngược thời gian thực (Redis):** Giữ tạm thời ghế ngồi trong 5 phút với độ trễ cực thấp (< 1ms), ngăn ngừa xung đột đụng độ 2 người mua cùng 1 ghế.
3. **Mạng đồ thị gợi ý phim thông minh (Neo4j):** Phân tích mối quan hệ đặt vé và yêu thích của người dùng để đề xuất Bảng xếp hạng Top Phim thịnh hành.
4. **Hệ thống Khiếu nại & Khuyến mãi linh hoạt (MongoDB):** Lưu trữ cấu trúc Document linh hoạt dạng mảng lồng `conversations` trao đổi sự cố và quản lý mã Voucher chiết khấu.
5. **Nhật ký truy vết hoạt động Big Data (Apache Cassandra):** Ghi nhận liên tục nhật ký thao tác và lịch sử vé người dùng với tốc độ ghi cao, khả năng mở rộng hàng triệu bản ghi.

### 1.3 Mô hình Kiến trúc Polyglot Persistence (5 Cơ Sở Dữ Liệu)
Sơ đồ tổng quan kết nối giữa tầng ứng dụng ASP.NET MVC Backend và 5 hệ CSDL:

```mermaid
graph TD
    Client["Client Layer: Trình duyệt Web (HTML5, Razor CSHTML, AJAX, JavaScript)"]
    Backend["Application Tier: ASP.NET MVC 5 (C# Web Controller & Business Logic)"]
    
    DB_SQL[("1. SQL Server RDBMS<br/>(Giao dịch Phim, Rạp, Vé, Hóa đơn)")]
    DB_MGDB[("2. MongoDB 7.0 Document Store<br/>(Khiếu nại Khách hàng, Mã giảm giá)")]
    DB_REDIS[("3. Redis 7.0 Key-Value<br/>(Khóa ghế đếm ngược TTL 5 phút)")]
    DB_NEO[("4. Neo4j 5.0 Graph DB<br/>(Gợi ý Top Phim thịnh hành)")]
    DB_CASS[("5. Apache Cassandra 4.0<br/>(Nhật ký Logs Big Data & Lịch sử vé)")]

    Client <-->|HTTP POST/GET Requests| Backend
    Backend <-->|Entity Framework 6| DB_SQL
    Backend <-->|MongoDB C# Driver| DB_MGDB
    Backend <-->|StackExchange.Redis| DB_REDIS
    Backend <-->|REST HTTP API / Cypher| DB_NEO
    Backend <-->|Cassandra C# Driver| DB_CASS
```

---

## CHƯƠNG 2: THIẾT KẾ CSDL QUAN HỆ SQL SERVER (RDBMS CORE DESIGN)

### 2.1 Phạm vi nhiệm vụ
Quản lý các thực thể cố định và đảm bảo tính toàn vẹn dữ liệu giao dịch tài chính (ACID):
- `Phim`, `Rap`, `PhongChieu`, `SuatChieu`, `Ghe`, `NguoiDung`, `HoaDon`, `Ve`.

### 2.2 Bảng Mô tả Chi tiết Thực thể SQL Server:
1. **`NguoiDung`**: `UserID` (PK, int), `TenDangNhap` (nvarchar), `MatKhau` (nvarchar), `HoTen`, `Email`, `SoDienThoai`, `GroupID` (1: Admin, 2: Customer).
2. **`Phim`**: `PhimID` (PK, int), `TenPhim`, `DaoDien`, `DienVien`, `Poster`, `ThoiLuong`, `NgayKhoiChieu`, `MoTa`.
3. **`Rap`**: `RapID` (PK, int), `TenRap`, `DiaChi`, `SoDienThoai`.
4. **`PhongChieu`**: `PhongID` (PK, int), `TenPhong`, `RapID` (FK).
5. **`SuatChieu`**: `SuatChieuID` (PK, int), `PhimID` (FK), `PhongID` (FK), `NgayChieu`, `GioChieu`, `GiaVe`.
6. **`Ghe`**: `GheID` (PK, int), `PhongID` (FK), `TenGhe`, `LoaiGhe` (Thường / VIP).
7. **`HoaDon`**: `HoaDonID` (PK, int), `UserID` (FK), `NgayDat`, `TongTien`, `TrangThai`.
8. **`Ve`**: `VeID` (PK, int), `SuatChieuID` (FK), `GheID` (FK), `HoaDonID` (FK), `GiaVe`.

---

## CHƯƠNG 3: THIẾT KẾ CSDL NOSQL MONGODB (DOCUMENT STORE DESIGN)

### 3.1 Cấu trúc Collection 1: `customer_feedbacks` (Trung tâm Khiếu nại Hỗ trợ)
Lưu trữ ticket sự cố với mảng lồng `conversations` (Embedded Document Array) ghi nhận lịch sử phản hồi giữa Admin và Khách hàng.

```json
{
  "_id": { "$oid": "6a71eaa38676d746beb73484" },
  "userId": 7,
  "username": "huy",
  "email": "huy@gmail.com",
  "category": "Thanh toán",
  "subject": "Bị trừ tiền tài khoản Momo nhưng chưa nhận được mã vé QR",
  "content": "Tôi vừa thanh toán 180.000đ lúc 10h15 qua ví Momo, tiền đã trừ nhưng chưa có vé.",
  "status": "Resolved",
  "conversations": [
    {
      "sender": "Admin",
      "message": "Chào bạn, Ban quản trị đã kiểm tra và hoàn tiền 180.000đ về ví Momo thành công!",
      "createdAt": { "$date": "2026-08-01T10:45:00.000Z" }
    }
  ],
  "createdAt": { "$date": "2026-08-01T10:20:00.000Z" }
}
```

### 3.2 Cấu trúc Collection 2: `cinema_promotions` (Kho Mã Giảm Giá Voucher)
Lưu trữ danh sách mã ưu đãi (`code`, `title`, `discountAmount`, `quantity`, `claimedCount`, `status`, `startDate`, `endDate`).

### 3.3 Thuật toán Aggregation Pipeline (`$group`, `$sum`, `$cond`, `$project`):
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

---

## CHƯƠNG 4: THIẾT KẾ CSDL NOSQL REDIS (KEY-VALUE STORE DESIGN)

### 4.1 Phạm vi nhiệm vụ
Thực hiện khóa tạm thời ghế ngồi (Realtime Seat Locking) khi người dùng đang chọn ghế đặt vé, ngăn chặn đụng độ 2 người mua cùng 1 ghế.

### 4.2 Thiết kế Key Pattern & TTL:
- **Cấu trúc Key:** `seat_lock:{SuatChieuID}:{GheID}` (Ví dụ: `seat_lock:101:A1`)
- **Giá trị (Value):** `UserID:{UserID}` (Ví dụ: `UserID:7`)
- **Thời gian hết hạn (TTL):** `300` giây (5 phút). Sau 5 phút, Key tự động xóa để giải phóng ghế.

### 4.3 Mã lệnh C# Tương tác Redis (StackExchange.Redis):
```csharp
// 1. Đặt khóa giữ ghế trong 5 phút (TTL 300s)
bool isLocked = redisDb.StringSet($"seat_lock:{suatChieuId}:{gheId}", $"UserID:{userId}", TimeSpan.FromMinutes(5), When.NotExists);

// 2. Kiểm tra trạng thái ghế realtime
bool isCurrentlyLocked = redisDb.KeyExists($"seat_lock:{suatChieuId}:{gheId}");

// 3. Giải phóng / Xóa khóa giữ ghế khi thanh toán xong hoặc khách bỏ chọn
redisDb.KeyDelete($"seat_lock:{suatChieuId}:{gheId}");
```

---

## CHƯƠNG 5: THIẾT KẾ CSDL NOSQL NEO4J (GRAPH DATABASE DESIGN)

### 5.1 Phạm vi nhiệm vụ
Lưu trữ cấu trúc đồ thị mối quan hệ giữa Người Dùng, Bộ Phim và Thể Loại Phim để tính toán gợi ý Top Phim Thịnh Hành & Thống kê độ phổ biến.

### 5.2 Sơ đồ Đồ thị (Graph Model):
- **Nodes:**
  - `(:User {userId: 'huy', username: 'huy'})`
  - `(:Movie {movieId: 101, title: 'Lật Mặt 7', poster: '001.png', duration: 120})`
  - `(:Genre {genreId: 1, genreName: 'Hành Động'})`
- **Relationships:**
  - `(:User)-[:BOOKED {bookingId: 'HD001', seatCount: 2, date: '2026-08-01'}]->(:Movie)`
  - `(:User)-[:FAVORITED]->(:Movie)`
  - `(:Movie)-[:BELONGS_TO]->(:Genre)`

### 5.3 Bộ Câu lệnh Cypher Query Đầy Đủ:
```cypher
// 1. Top Phim Được Đặt Vé Nhiều Nhất
MATCH (u:User)-[r:BOOKED]->(m:Movie)
RETURN m.movieId AS MovieId, m.title AS Title, COUNT(r) AS TotalBookings
ORDER BY TotalBookings DESC LIMIT 4;

// 2. Top Phim Được Yêu Thích Nhất
MATCH (u:User)-[r:FAVORITED]->(m:Movie)
RETURN m.movieId AS MovieId, m.title AS Title, COUNT(r) AS TotalFavorites
ORDER BY TotalFavorites DESC LIMIT 4;

// 3. Thả Tim Yêu Thích Phim Realtime (Tạo mới hoặc Xóa mối quan hệ)
MATCH (u:User {userId: 'huy'}), (m:Movie {movieId: 101})
MERGE (u)-[r:FAVORITED]->(m);

// 4. Thống Kê Phân Tích Độ Phổ Biến Theo Thể Loại Phim Đồ Thị
MATCH (m:Movie)-[:BELONGS_TO]->(g:Genre)
OPTIONAL MATCH (u:User)-[b:BOOKED]->(m)
OPTIONAL MATCH (u2:User)-[f:FAVORITED]->(m)
RETURN g.genreId AS GenreId, g.genreName AS GenreName, 
       COUNT(DISTINCT b) AS TotalBookings, COUNT(DISTINCT f) AS TotalFavorites
ORDER BY TotalBookings DESC;
```

---

## CHƯƠNG 6: THIẾT KẾ CSDL NOSQL APACHE CASSANDRA (WIDE-COLUMN STORE DESIGN)

### 6.1 Phạm vi nhiệm vụ
Lưu nhật ký hoạt động người dùng (User Activity Logs) và lịch sử vé theo mô hình Big Data với tốc độ ghi nhanh (High Write Performance).

### 6.2 Cấu trúc Keyspace & Các Bảng CQL:
- **Keyspace:** `cinemadb_analytics` (Replication Strategy: `SimpleStrategy`, `replication_factor: 1`).

#### Bảng 1: `user_activity_logs` (Nhật ký hoạt động)
```sql
CREATE TABLE cinemadb_analytics.user_activity_logs (
    user_id int,
    activity_time timestamp,
    log_id uuid,
    activity_type text,
    description text,
    ip_address text,
    device_info text,
    PRIMARY KEY (user_id, activity_time)
) WITH CLUSTERING ORDER BY (activity_time DESC);
```

#### Bảng 2: `user_ticket_history` (Lịch sử đặt vé)
```sql
CREATE TABLE cinemadb_analytics.user_ticket_history (
    user_id int,
    booking_time timestamp,
    booking_id uuid,
    movie_title text,
    seat_names list<text>,
    total_amount decimal,
    payment_method text,
    status text,
    PRIMARY KEY (user_id, booking_time)
) WITH CLUSTERING ORDER BY (booking_time DESC);
```

#### Bảng 3: `seat_status_history` (Nhật ký biến động trạng thái ghế)
```sql
CREATE TABLE cinemadb_analytics.seat_status_history (
    showtime_id int,
    status_time timestamp,
    seat_id int,
    seat_name text,
    status text,
    user_id int,
    PRIMARY KEY (showtime_id, status_time)
) WITH CLUSTERING ORDER BY (status_time DESC);
```

---

## CHƯƠNG 7: THIẾT KẾ MÔ ĐUN XỬ LÝ VÀ SƠ ĐỒ LỚP (CLASS DIAGRAM)

```mermaid
classDiagram
    class HomeController {
        +PhimDangChieu() ActionResult
        +FavoriteMovie(movieId) ActionResult
    }

    class DatVeController {
        +ChonGhe(suatChieuId) ActionResult
        +LockSeatRedis(suatChieuId, gheId) JsonResult
        +ThanhToan(hoaDonId) ActionResult
    }

    class MgdbCustomerFeedbackController {
        +Index() ActionResult
        +CreateTicket() ActionResult
        +ReplyTicket() ActionResult
    }

    class MgdbPromotionController {
        +Index() ActionResult
        +ApplyVoucher(code) JsonResult
    }

    class CassandraController {
        +UserLogs(userId) ActionResult
        +TicketHistory(userId) ActionResult
    }

    class MgdbService {
        -MongoClient Client
        +GetFeedbacksByUser(userId)
        +AddFeedback(doc)
        +ReplyFeedback(id, replyMsg)
        +GetFeedbackCategoryStats()
        +GetActivePromotions()
        +ClaimPromotion(code)
    }

    class Neo4jService {
        +ExecuteCypher(query)
        +GetTopBookedMovies(limit)
        +GetTopFavoriteMovies(limit)
        +ToggleFavorite(userId, movieId)
        +GetGenreAnalytics()
        +SeedInitialData()
    }

    class CassandraService {
        +GetActivityLogsByUser(userId)
        +LogUserActivity(userId, activityType, desc)
        +GetTicketHistoryByUser(userId)
        +LogTicketHistory(userId, bookingId, amount)
    }

    class RedisManager {
        +LockSeat(suatChieuId, gheId, userId)
        +UnlockSeat(suatChieuId, gheId)
        +IsSeatLocked(suatChieuId, gheId)
    }

    HomeController --> Neo4jService : Gọi gợi ý & thả tim Phim
    DatVeController --> RedisManager : Tạm giữ ghế 5 phút
    DatVeController --> CassandraService : Ghi Log thanh toán vé
    MgdbCustomerFeedbackController --> MgdbService : Quản lý khiếu nại
    MgdbPromotionController --> MgdbService : Tra cứu & Trừ kho Voucher
    CassandraController --> CassandraService : Truy vấn nhật ký Big Data
```

---

## CHƯƠNG 8: QUY TRÌNH LUỒNG NGHIỆP VỤ HỆ THỐNG (SYSTEM SEQUENCE DIAGRAMS)

### 8.1 Phân hệ CSDL Redis (Key-Value Store)

#### 8.1.1 Luồng Khách Hàng Chọn Ghế & Khóa Ghế Đếm Ngược Realtime (TTL 5 Phút)

```mermaid
sequenceDiagram
    autonumber
    actor User as Khách Hàng
    participant Web as ASP.NET MVC Backend
    participant Redis as Redis Key-Value
    participant SQL as SQL Server RDBMS

    User->>Web: Chọn ghế A1 Suất chiếu 101
    Web->>Redis: Lock key 'seat_lock:101:A1' (TTL 300s)
    alt Ghế đã bị người khác chọn
        Redis-->>Web: Trả về Key đã tồn tại
        Web-->>User: Báo lỗi ghế đã bị giữ bởi người khác
    else Ghế còn trống
        Redis-->>Web: Khóa ghế thành công (True)
        Web-->>User: Hiển thị đếm ngược 5 phút thanh toán
    end
```

#### 8.1.2 Luồng Giải Phóng Khóa Giữ Ghế Khi Hủy Chọn Hoặc Thanh Toán Hoàn Tất (Redis Key Delete)

```mermaid
sequenceDiagram
    autonumber
    actor User as Khách Hàng
    participant Web as ASP.NET MVC Backend
    participant Redis as Redis Key-Value
    participant SQL as SQL Server RDBMS

    alt Trường hợp 1: Khách hàng bấm Bỏ Chọn Ghế hoặc Hủy Đơn
        User->>Web: Bấm Bỏ chọn ghế A1 Suất chiếu 101
        Web->>Redis: KeyDelete('seat_lock:101:A1')
        Redis-->>Web: Xóa Key giữ ghế thành công
        Web-->>User: Ghế A1 trở về trạng thái trống cho người khác chọn
    else Trường hợp 2: Khách hàng Hoàn Tất Thanh Toán Hóa Đơn
        User->>Web: Nhấp Thanh Toán Đơn Hàng Thành Công
        Web->>SQL: Lưu Hóa Đơn & Mã Vé vào SQL Server
        Web->>Redis: KeyDelete('seat_lock:101:A1') (Giải phóng khóa tạm)
        Redis-->>Web: Xóa Key thành công
        Web-->>User: Trả về Vé Xem Phim & Mã QR Code thành công
    else Trường hợp 3: Quá 5 Phút Không Thanh Toán (Hết hạn TTL)
        Redis->>Redis: Tự động xóa Key 'seat_lock:101:A1' do TTL = 0
        User->>Web: Bấm Thanh Toán muộn (sau 5 phút)
        Web->>Redis: KeyExists('seat_lock:101:A1')
        Redis-->>Web: Trả về False (Key đã hết hạn)
        Web-->>User: Thông báo 'Đã hết thời gian giữ ghế 5 phút, vui lòng chọn lại!'
    end
```

---

### 8.2 Phân hệ CSDL Neo4j (Graph Database)

#### 8.2.1 Luồng Gợi Ý Top Phim Thịnh Hành Trên Trang Chủ (Neo4j Graph DB + HttpRuntime.Cache)

```mermaid
sequenceDiagram
    autonumber
    actor User as Khách Hàng / Khách vãng cảnh
    participant Home as HomeController (/Home/PhimDangChieu)
    participant Cache as HttpRuntime.Cache (RAM 5 phút)
    participant Neo4j as Neo4j Graph DB Server
    participant SQL as SQL Server DB

    User->>Home: Truy cập Trang Chủ
    Home->>Cache: Kiểm tra Cache['TopBookedNeo4j'] & ['TopFavoritesNeo4j']
    alt Đã có dữ liệu trong RAM (Cache Hit)
        Cache-->>Home: Trả về danh sách Top Phim lập tức (0.001s)
    else Chưa có trong RAM (Cache Miss / Lần đầu)
        Home->>Neo4j: ExecuteCypher("MATCH (u:User)-[r:BOOKED]->(m:Movie) RETURN ...")
        Neo4j-->>Home: Trả về 4 Node Phim có lượt đặt vé & yêu thích cao nhất
        Home->>SQL: Sync tên phim & file ảnh Poster từ bảng Phims
        SQL-->>Home: Trả về dữ liệu đồng bộ
        Home->>Cache: Insert Cache 5 phút (NoSlidingExpiration)
    end
    Home-->>User: Hiển thị Bảng Xếp Hạng Top Phim Thịnh Hành trên Trang Chủ
```

#### 8.2.2 Luồng Khách Hàng Thả Tim Yêu Thích Phim Realtime (Relationship [:FAVORITED])

```mermaid
sequenceDiagram
    autonumber
    actor User as Khách Hàng Đã Đăng Nhập
    participant Web as HomeController / MovieController
    participant Neo4j as Neo4j Graph DB Server

    User->>Web: Nhấp nút Thả Tim / Yêu Thích ❤️ trên Phim (MovieId: 101)
    Web->>Neo4j: ExecuteCypher("MATCH (u:User {userId: 'huy'}), (m:Movie {movieId: 101}) ...")
    alt Đã có mối quan hệ [:FAVORITED]
        Neo4j->>Neo4j: DELETE r (Xóa tim yêu thích)
        Neo4j-->>Web: Trả về trạng thái Unfavorited
        Web-->>User: Cập nhật icon tim rỗng 🤍
    else Chưa có mối quan hệ [:FAVORITED]
        Neo4j->>Neo4j: MERGE (u)-[:FAVORITED]->(m) (Tạo tim mới)
        Neo4j-->>Web: Trả về trạng thái Favorited
        Web-->>User: Cập nhật icon tim đỏ ❤️ & Tăng số lượt yêu thích
    end
```

#### 8.2.3 Luồng Thống Kê Phân Tích Độ Phổ Biến Theo Thể Loại Phim Đồ Thị (GetGenreAnalytics)

```mermaid
sequenceDiagram
    autonumber
    actor Admin as Quản Trị Viên (Admin)
    participant AdminCtrl as AdminController / AnalyticsController
    participant Neo4j as Neo4j Graph DB Server

    Admin->>AdminCtrl: Truy cập Bảng Thống Kê Phân Tích Thể Loại Phim
    AdminCtrl->>Neo4j: ExecuteCypher("MATCH (m:Movie)-[:BELONGS_TO]->(g:Genre) OPTIONAL MATCH ...")
    Neo4j-->>AdminCtrl: Trả về Danh sách Thể loại + Tổng vé đợt đặt + Tổng lượt yêu thích
    AdminCtrl-->>Admin: Hiển thị Biểu đồ / Bảng Thống kê Độ Phổ Biến Thể Loại Phim Neo4j Graph
```

---

### 8.3 Phân hệ CSDL MongoDB (Document Store)

#### 8.3.1 Luồng Tiếp Nhận Khiếu Nại & Trả Lời Của Admin (Collection 'customer_feedbacks')

```mermaid
sequenceDiagram
    autonumber
    actor Customer as Khách Hàng
    actor Admin as Quản Trị Viên (Admin)
    participant Web as MgdbCustomerFeedbackController
    participant Mgdb as MongoDB Database ('customer_feedbacks')

    Customer->>Web: Gửi Ticket khiếu nại (Tiêu đề, Nội dung, Chuyên mục)
    Web->>Mgdb: InsertOne(doc { status: 'New', conversations: [] })
    Mgdb-->>Web: Trả về ObjectId thành công
    Web-->>Customer: Hiển thị Ticket trong Lịch sử Hỗ Trợ

    Admin->>Web: Đăng nhập Admin & Mở trang Quản Lý Phản Hồi
    Web->>Mgdb: Find(filter) & Aggregate($group theo category)
    Mgdb-->>Web: Trả về Danh sách Ticket + Bảng Thống Kê Sự Cố
    Web-->>Admin: Hiển thị Bảng điều khiển Admin

    Admin->>Web: Nhập Lời nhắn trả lời & Bấm Giải quyết
    Web->>Mgdb: UpdateOne(filter, { $push:conversations, $set:status='Resolved' })
    Mgdb-->>Web: Cập nhật Document thành công
    Web-->>Admin: Thông báo đã trả lời Ticket
    Web-->>Customer: Hiển thị câu trả lời của Admin trong lịch sử hội thoại
```

#### 8.3.2 Luồng Áp Dụng Mã Khuyến Mãi & Voucher Giảm Giá (Collection 'cinema_promotions')

```mermaid
sequenceDiagram
    autonumber
    actor Customer as Khách Hàng
    participant Checkout as ThanhToanController / MgdbPromotionController
    participant Mgdb as MongoDB Database ('cinema_promotions')
    participant SQL as SQL Server RDBMS

    Customer->>Checkout: Mở trang Đặt Vé & Nhập Mã Voucher (VD: 'MOVANA50K')
    Checkout->>Mgdb: Find({ code: 'MOVANA50K', status: 'Active', quantity: { $gt: 0 } })
    alt Mã Voucher Không Hợp Lệ hoặc Đã Hết Số Lượng
        Mgdb-->>Checkout: Trả về null / Hết lượt
        Checkout-->>Customer: Thông báo 'Mã giảm giá không tồn tại hoặc đã hết lượt dùng'
    else Mã Voucher Hợp Lệ
        Mgdb-->>Checkout: Trả về BSON Document Voucher (discountAmount: 50.000đ)
        Checkout->>Mgdb: UpdateOne({ code: 'MOVANA50K' }, { $inc: { quantity: -1, claimedCount: 1 } })
        Mgdb-->>Checkout: Trừ số lượng kho Voucher thành công (Atomic Update)
        Checkout->>Checkout: Tính Tổng Tiền = Giá Vé Gốc - 50.000đ
        Checkout->>SQL: Thêm Hóa Đơn đã trừ chiết khấu vào SQL Server
        SQL-->>Checkout: Lưu Hóa Đơn thành công
        Checkout-->>Customer: Hiển thị Đơn hàng đã áp mã giảm 50K thành công!
    end
```

---

### 8.4 Phân hệ CSDL Apache Cassandra (Wide-Column Store)

#### 8.4.1 Luồng Ghi Nhật Ký Hoạt Động Big Data & Lịch Sử Vé (Keyspace 'cinemadb_analytics')

```mermaid
sequenceDiagram
    autonumber
    actor User as Khách Hàng
    participant Web as ASP.NET MVC Backend
    participant Cass as Apache Cassandra Database Server

    User->>Web: Thực hiện thao tác trên Web (Đăng nhập, Xem phim, Đặt vé)
    Web->>Cass: ExecuteAsync("INSERT INTO user_activity_logs (user_id, activity_time, ...)")
    Cass-->>Web: Ghi Log Big Data thành công (Tốc độ cực nhanh < 1ms)

    User->>Web: Xem trang Thông Tin Cá Nhân / Lịch Sử Vé
    Web->>Cass: Execute("SELECT * FROM user_ticket_history WHERE user_id = 7")
    Cass-->>Web: Trả về danh sách Lịch sử đặt vé được sắp xếp theo thời gian mới nhất (ORDER BY booking_time DESC)
    Web-->>User: Hiển thị Lịch sử vé và Nhật ký hoạt động cá nhân
```

---

### 📌 TỔNG KẾT TÀI LIỆU SDS TỔNG THỂ V3.0
Tài liệu SDS v3.0 này cung cấp trọn vẹn 100% thiết kế kỹ thuật của cả **SQL Server và 4 hệ NoSQL (MongoDB, Redis, Neo4j, Cassandra)**, hoàn chỉnh theo chuẩn đồ án cấp trường và tài liệu thiết kế hệ thống lớn.
