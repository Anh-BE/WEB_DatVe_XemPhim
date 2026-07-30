# HƯỚNG DẪN CẤU HÌNH KẾT NỐI C# VỚI REDIS VÀ TẠO CƠ SỞ DỮ LIỆU REDIS

> **Dự án:** WebDatVeXemPhim (ASP.NET MVC 5 - .NET Framework 4.5)  
> **Thư viện C#:** `StackExchange.Redis` (Version 1.2.6)  
> **Địa chỉ Redis:** `localhost:6379` (Memurai / Redis Windows Service)

---

## PHẦN 1: BẢN CHẤT "CƠ SỞ DỮ LIỆU" TRONG REDIS (DATABASE IN REDIS)

Khác với SQL Server phải chạy lệnh `CREATE DATABASE TenCoSoDuLieu;`, trong **Redis**:

1. **Không có lệnh CREATE DATABASE:**  
   Redis mặc định cung cấp sẵn **16 cơ sở dữ liệu đánh số từ `0` đến `15`**.
2. **Chọn Database trong Code C#:**  
   * Để chọn database số 0 (mặc định): `IDatabase db = RedisService.GetDatabase(0);`
   * Để chọn database số 1: `IDatabase db = RedisService.GetDatabase(1);`
3. **Phân loại dữ liệu theo "Namespace Key" (Cách tổ chức dữ liệu chuẩn):**  
   Dữ liệu được tổ chức thông qua dấu hai chấm `:` trong tên Key để giả lập các bảng.

### 📋 Cấu trúc Key cho 4 Tính năng Đồ án:

| Tính năng | Dạng Key Redis | Kiểu dữ liệu | TTL (Thời gian sống) | Ví dụ Key |
| :--- | :--- | :--- | :--- | :--- |
| **1. Khóa ghế tạm** | `seatlock:{lichChieuId}:{gheId}` | String | 60 giây | `seatlock:102:15` |
| **2. Giỏ hàng thanh toán** | `cart:{username}` | Hash (hoặc String JSON) | 600 giây (10 phút) | `cart:le_ngoc_anh` |
| **3. OTP Thanh toán** | `otp:checkout:{username}` | String | 120 giây (2 phút) | `otp:checkout:le_ngoc_anh` |
| **4. Redis Session** | `session:user:{username}` | Hash / String JSON | 1800 giây (30 phút) | `session:user:le_ngoc_anh` |

---

## PHẦN 2: CẤU HÌNH KẾT NỐI REDIS TRONG C# ASP.NET MVC

### Bước 1: Thêm Connection String vào `Web.config`

Mở tệp `Web.config` trong dự án C# và thêm vào thẻ `<appSettings>`:

```xml
<appSettings>
  <!-- Kết nối Redis localhost cổng 6379 -->
  <add key="RedisConnectionString" value="localhost:6379,abortConnect=false" />
</appSettings>
```

---

### Bước 2: Tạo Lớp Service Kết nối Redis Singleton (`RedisService.cs`)

Tệp `RedisService.cs` đã được tự động tạo tại đường dẫn:  
`C:\Users\Le Ngoc Anh\Desktop\09_WebDatVeXemPhim\09_WebDatVeXemPhim\doan3\doan3\Models\RedisService.cs`

```csharp
using System;
using System.Configuration;
using StackExchange.Redis;

namespace doan3.Models
{
    public class RedisService
    {
        private static readonly Lazy<ConnectionMultiplexer> LazyConnection;

        static RedisService()
        {
            string connectionString = ConfigurationManager.AppSettings["RedisConnectionString"] ?? "localhost:6379,abortConnect=false";
            LazyConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                return ConnectionMultiplexer.Connect(connectionString);
            });
        }

        public static ConnectionMultiplexer Connection => LazyConnection.Value;

        /// <summary>
        /// Lấy đối tượng kết nối tới Database Redis (mặc định dbId = 0)
        /// </summary>
        public static IDatabase GetDatabase(int dbId = 0)
        {
            return Connection.GetDatabase(dbId);
        }
    }
}
```

---

## PHẦN 3: HƯỚNG DẪN THỰC HÀNH CÁC LỆNH TẠO DỮ LIỆU TRÊN REDIS

Bạn có thể chạy thử nghiệm tạo dữ liệu trực tiếp trên phần mềm **Another Redis Desktop Manager** hoặc bằng **Code C#**:

### 1. Thao tác Tính năng 1: Khóa ghế tạm thời
* **Trong C#:**
  ```csharp
  var db = RedisService.GetDatabase();
  string lockKey = "seatlock:102:15"; // Suất chiếu 102, Ghế 15
  
  // SETNX: Chỉ tạo nếu key chưa tồn tại, tự hết hạn sau 60s
  bool isLocked = db.StringSet(lockKey, "LockedBy_UserA", TimeSpan.FromSeconds(60), When.NotExists);
  
  if (isLocked) {
      // Ghế được khóa thành công
  } else {
      // Ghế đã có người khác khóa trước đó!
  }
  ```

### 2. Thao tác Tính năng 2: Giỏ hàng thanh toán
* **Trong C#:**
  ```csharp
  var db = RedisService.GetDatabase();
  string cartKey = "cart:le_ngoc_anh";
  
  db.HashSet(cartKey, new HashEntry[] {
      new HashEntry("lichChieuId", "102"),
      new HashEntry("danhSachGhe", "15,16"),
      new HashEntry("tongTien", "180000")
  });
  db.KeyExpire(cartKey, TimeSpan.FromMinutes(10)); // Giỏ hàng lưu 10 phút
  ```

### 3. Thao tác Tính năng 3: Tạo & Kiểm tra mã OTP
* **Trong C#:**
  ```csharp
  var db = RedisService.GetDatabase();
  string otpKey = "otp:checkout:le_ngoc_anh";
  string otpCode = "889966"; // Mã ngẫu nhiên 6 số
  
  // Lưu OTP với thời gian sống 2 phút (120s)
  db.StringSet(otpKey, otpCode, TimeSpan.FromMinutes(2));
  ```

### 4. Thao tác Tính năng 4: Lưu Redis Session
* **Trong C#:**
  ```csharp
  var db = RedisService.GetDatabase();
  string sessionKey = "session:user:le_ngoc_anh";
  
  db.HashSet(sessionKey, new HashEntry[] {
      new HashEntry("userId", "1"),
      new HashEntry("fullName", "Lê Ngọc Anh"),
      new HashEntry("role", "Admin")
  });
  db.KeyExpire(sessionKey, TimeSpan.FromMinutes(30)); // Session 30 phút
  ```
