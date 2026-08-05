# HƯỚNG DẪN CHẠY CASSANDRA – HỆ THỐNG ĐẶT VÉ XEM PHIM

## 1\. Thành phần

* `Cassandra\_Script\_NopBai.cql`: tạo keyspace, 12 bảng query-based, index, seed, BATCH, UPDATE, TTL, DELETE và truy vấn kiểm thử.
* `Cassandra\_Data\_Seed.json`: dữ liệu seed tương ứng 100% với các bảng trong script, dùng cho ứng dụng hoặc công cụ import riêng.
* Keyspace: `movie\_booking\_cassandra`.

> Cách nhanh nhất để có đầy đủ dữ liệu là chạy trực tiếp file CQL vì file này đã chứa toàn bộ seed tương thích với JSON.

## 2\. Cài Cassandra bằng Docker

Yêu cầu: Docker Desktop đang chạy.

```powershell
docker pull cassandra:5.0
docker run -d --name movie-cassandra -p 9042:9042 -e CASSANDRA\_CLUSTER\_NAME=MovieCluster -v movie\_cassandra\_data:/var/lib/cassandra cassandra:5.0
docker logs -f movie-cassandra
```

Khi log xuất hiện `Startup complete`, nhấn `Ctrl+C`. Kiểm tra:

```powershell
docker exec -it movie-cassandra nodetool status
docker exec -it movie-cassandra cqlsh
```

## 3\. Cài trực tiếp

1. Cài Java theo phiên bản Cassandra yêu cầu và thiết lập `JAVA\_HOME`.
2. Tải Apache Cassandra binary, giải nén, thêm thư mục `bin` vào `PATH`.
3. Mở terminal tại thư mục Cassandra và chạy:

```powershell
cassandra -f
```

Mở terminal khác:

```powershell
cqlsh 127.0.0.1 9042
nodetool status
```

## 4\. Khởi động và dừng Cassandra Docker

```powershell
docker start movie-cassandra
docker stop movie-cassandra
docker restart movie-cassandra
docker ps -a --filter name=movie-cassandra
```

## 5\. Chạy toàn bộ script

Mở PowerShell tại thư mục `CASSANDRA\_PART`.

### Docker

```powershell
docker cp .\\Cassandra\_Script\_NopBai.cql movie-cassandra:/Cassandra\_Script\_NopBai.cql
docker exec -it movie-cassandra cqlsh -f /Cassandra\_Script\_NopBai.cql
```

### Cài trực tiếp

```powershell
cqlsh 127.0.0.1 9042 -f .\\Cassandra\_Script\_NopBai.cql
```

Script tự thực hiện `CREATE KEYSPACE` và `USE movie\_booking\_cassandra`; không cần tạo thủ công. Muốn tạo riêng:

```sql
CREATE KEYSPACE IF NOT EXISTS movie\_booking\_cassandra
WITH replication = {'class':'SimpleStrategy','replication\_factor':1};
```

## 6\. Import `Cassandra\_Data\_Seed.json`

Cassandra/cqlsh không có lệnh import một file JSON nhiều bảng theo cấu trúc tổng hợp. Có hai cách hợp lệ:

1. **Khuyến nghị:** chạy `Cassandra\_Script\_NopBai.cql`; dữ liệu trong JSON đã được chuyển thành các lệnh `INSERT` tương ứng trong script.
2. Ứng dụng đọc `tables.<table\_name>` trong JSON rồi thực hiện prepared statement vào đúng bảng. Trường `keyspace` cho biết keyspace đích.

Kiểm tra JSON hợp lệ bằng PowerShell:

```powershell
Get-Content .\\Cassandra\_Data\_Seed.json -Raw | ConvertFrom-Json | Select-Object keyspace, format\_version
```

## 7\. Kiểm tra keyspace và bảng

```powershell
docker exec -it movie-cassandra cqlsh
```

```sql
DESCRIBE KEYSPACES;
USE movie\_booking\_cassandra;
DESCRIBE TABLES;
DESCRIBE KEYSPACE movie\_booking\_cassandra;
SELECT table\_name FROM system\_schema.tables WHERE keyspace\_name='movie\_booking\_cassandra';
```

Kết quả phải có 12 bảng:

`user\_activity\_by\_user\_month`, `activity\_by\_day\_type`, `booking\_history\_by\_user`, `booking\_events\_by\_booking`, `seat\_timeline\_by\_showtime`, `payment\_history\_by\_user`, `search\_history\_by\_user`, `analytics\_by\_month\_metric`, `dashboard\_by\_day`, `traffic\_by\_day\_path`, `request\_logs\_by\_day`, `audit\_logs\_by\_month\_action`.

## 8\. Kiểm tra Partition Key và Clustering Key

```sql
SELECT table\_name, column\_name, kind, position
FROM system\_schema.columns
WHERE keyspace\_name='movie\_booking\_cassandra';
```

Ví dụ:

* `user\_activity\_by\_user\_month`: Partition Key `(user\_id, activity\_month)`; Clustering Key `(event\_time, event\_id)`.
* `activity\_by\_day\_type`: Partition Key `(activity\_date, action\_type)`; Clustering Key `(event\_time, event\_id)`.
* `seat\_timeline\_by\_showtime`: Partition Key `showtime\_id`; Clustering Key `(seat\_code, event\_time, event\_id)`.
* `booking\_events\_by\_booking`: Partition Key `booking\_id`; Clustering Key `(event\_time, event\_id)`.
* `audit\_logs\_by\_month\_action`: Partition Key `(audit\_month, action\_type)`; Clustering Key `(action\_time, audit\_id)`.

## 9\. Kiểm tra Timeline và Event Store

```sql
USE movie\_booking\_cassandra;

-- User Timeline
SELECT \* FROM user\_activity\_by\_user\_month
WHERE user\_id=11111111-1111-4111-8111-111111111111
AND activity\_month='2026-06-01';

-- Booking Timeline / Event Store
SELECT \* FROM booking\_events\_by\_booking
WHERE booking\_id=30000000-0000-4000-8000-000000000001;

-- Seat Timeline theo toàn bộ suất chiếu
SELECT \* FROM seat\_timeline\_by\_showtime
WHERE showtime\_id=20000000-0000-4000-8000-000000000001;

-- Seat Timeline của ghế A5
SELECT \* FROM seat\_timeline\_by\_showtime
WHERE showtime\_id=20000000-0000-4000-8000-000000000001
AND seat\_code='A5';
```

## 10\. Kiểm tra lịch sử và truy vấn theo User/ngày/tháng/loại

```sql
-- Booking History theo User + tháng
SELECT \* FROM booking\_history\_by\_user
WHERE user\_id=11111111-1111-4111-8111-111111111111
AND booking\_month='2026-06-01';

-- Payment History
SELECT \* FROM payment\_history\_by\_user
WHERE user\_id=11111111-1111-4111-8111-111111111111
AND payment\_month='2026-06-01';

-- Search History
SELECT \* FROM search\_history\_by\_user
WHERE user\_id=22222222-2222-4222-8222-222222222222
AND search\_month='2026-07-01';

-- Hoạt động theo ngày + loại LOGIN/PAYMENT/BOOKING
SELECT \* FROM activity\_by\_day\_type WHERE activity\_date='2026-06-12' AND action\_type='LOGIN';
SELECT \* FROM activity\_by\_day\_type WHERE activity\_date='2026-06-12' AND action\_type='PAYMENT';
SELECT \* FROM activity\_by\_day\_type WHERE activity\_date='2026-07-20' AND action\_type='BOOKING';
```

## 11\. Kiểm tra Analytics, Dashboard và Traffic

```sql
-- Analytics tháng
SELECT \* FROM analytics\_by\_month\_metric
WHERE metric\_month='2026-06-01' AND metric\_type='REVENUE';

SELECT \* FROM analytics\_by\_month\_metric
WHERE metric\_month='2026-07-01' AND metric\_type='BOOKING';

-- Admin Dashboard
SELECT \* FROM dashboard\_by\_day WHERE dashboard\_date='2026-06-12';
SELECT \* FROM dashboard\_by\_day WHERE dashboard\_date='2026-07-03';

-- Traffic
SELECT \* FROM traffic\_by\_day\_path
WHERE traffic\_date='2026-06-12' AND path='/login';
```

## 12\. Kiểm tra Logs

```sql
-- Request Logs theo ngày
SELECT \* FROM request\_logs\_by\_day WHERE log\_date='2026-07-03';

-- Audit theo tháng + loại hành động
SELECT \* FROM audit\_logs\_by\_month\_action
WHERE audit\_month='2026-06-01' AND action\_type='LOGIN';

SELECT \* FROM audit\_logs\_by\_month\_action
WHERE audit\_month='2026-06-01' AND action\_type='PAYMENT';

SELECT \* FROM audit\_logs\_by\_month\_action
WHERE audit\_month='2026-07-01' AND action\_type='BOOKING';
```

## 13\. Giải thích thiết kế từng bảng

|Bảng|Mục đích|Partition Key|Clustering Key|Truy vấn phục vụ|
|-|-|-|-|-|
|`user\_activity\_by\_user\_month`|User Timeline|`(user\_id, activity\_month)`|`event\_time DESC, event\_id`|Hoạt động một user trong tháng|
|`activity\_by\_day\_type`|Hoạt động theo ngày/loại|`(activity\_date, action\_type)`|`event\_time DESC, event\_id`|LOGIN, PAYMENT, BOOKING trong ngày|
|`booking\_history\_by\_user`|Booking History|`(user\_id, booking\_month)`|`created\_at DESC, booking\_id`|Lịch sử đặt vé user/tháng|
|`booking\_events\_by\_booking`|Event Store|`booking\_id`|`event\_time ASC, event\_id`|Phát lại vòng đời booking|
|`seat\_timeline\_by\_showtime`|Seat Timeline|`showtime\_id`|`seat\_code, event\_time DESC, event\_id`|Ghế theo suất chiếu|
|`payment\_history\_by\_user`|Payment History|`(user\_id, payment\_month)`|`paid\_at DESC, payment\_id`|Thanh toán user/tháng|
|`search\_history\_by\_user`|Search History|`(user\_id, search\_month)`|`searched\_at DESC, search\_id`|Từ khóa user đã tìm|
|`analytics\_by\_month\_metric`|Analytics tháng|`(metric\_month, metric\_type)`|`dimension`|Revenue, booking, login theo tháng|
|`dashboard\_by\_day`|Dashboard giờ/ngày|`dashboard\_date`|`bucket\_hour DESC`|KPI Admin Dashboard|
|`traffic\_by\_day\_path`|Web traffic|`(traffic\_date, path)`|`request\_time DESC, request\_id`|Traffic theo ngày/URL|
|`request\_logs\_by\_day`|Request Log 90 ngày|`log\_date`|`logged\_at DESC, request\_id`|Theo dõi request/lỗi|
|`audit\_logs\_by\_month\_action`|Audit Log|`(audit\_month, action\_type)`|`action\_time DESC, audit\_id`|Audit theo tháng/loại|

Các partition key có thêm ngày/tháng để tránh partition tăng vô hạn. Clustering key đặt thời gian trước để timeline được sắp xếp và truy vấn range hiệu quả. Dữ liệu được chủ động denormalize giữa các bảng vì Cassandra tối ưu theo truy vấn, không dùng JOIN.

## 14\. Checklist bài nộp

* \[x] Keyspace riêng `movie\_booking\_cassandra`.
* \[x] Nhiều bảng Log/History/Event Store.
* \[x] Tất cả bảng có Partition Key.
* \[x] Tất cả bảng có Clustering Key phù hợp truy vấn.
* \[x] Truy vấn theo User, ngày, tháng, loại hành động, phim và suất chiếu.
* \[x] Có `CREATE`, `INSERT`, `UPDATE`, `DELETE`, `SELECT`, `BATCH`, `TTL`, `INDEX`.
* \[x] Dữ liệu test Login, Search, View Movie, Booking, Seat, Payment, Dashboard, Timeline, Analytics, Traffic, Request Log, Audit Log.
* \[x] Script có thể chạy trực tiếp bằng `cqlsh -f`.

