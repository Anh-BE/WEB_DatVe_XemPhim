// ===============================================================================
// ĐỒ ÁN DỮ LIỆU NOSQL - BÁO CÁO VÀ SCRIPT MONGODB
// Dự án: Web Đặt Vé Xem Phim (Movana Cinema)
// CSDL: MongoDB (Document Store) - Running on Docker (Port 27017)
// Hướng dẫn: Mở MongoDB Compass hoặc mongosh terminal và chạy toàn bộ tệp script này.
// ===============================================================================

// 1. CHUYỂN SANG CSDL 'CinemaNoSQL'
use CinemaNoSQL;

// 2. KHỞI TẠO COLLECTION 1: 'movie_reviews' (Đánh giá & Bình luận Phim)
db.createCollection("movie_reviews");

// Nạp dữ liệu mẫu JSON vào collection 'movie_reviews'
db.movie_reviews.insertMany([
  {
    "movieId": 101,
    "movieTitle": "Lật Mặt 7: Một Điều Ước",
    "userId": 1,
    "username": "nguyenvana",
    "rating": 5,
    "content": "Phim rất cảm động về tình cảm gia đình, dàn diễn viên nhập vai xuất sắc!",
    "tags": ["Gia đình", "Xúc động", "Phim Việt hay"],
    "likesCount": 15,
    "status": "Approved",
    "createdAt": new Date("2026-08-01T10:00:00Z")
  },
  {
    "movieId": 101,
    "movieTitle": "Lật Mặt 7: Một Điều Ước",
    "userId": 2,
    "username": "tranthib",
    "rating": 4,
    "content": "Kịch bản hay nhưng đoạn giữa hơi dài dòng một chút, tổng thể 8.5/10.",
    "tags": ["Kịch tính", "Phim Việt hay"],
    "likesCount": 8,
    "status": "Approved",
    "createdAt": new Date("2026-08-02T14:30:00Z")
  },
  {
    "movieId": 102,
    "movieTitle": "Dune: Hành Tinh Cát 2",
    "userId": 3,
    "username": "lethi_c",
    "rating": 5,
    "content": "Âm thanh sống động, hình ảnh hành tinh Arrakis quá hoành tráng!",
    "tags": ["Bom tấn", "Kỹ xảo đẹp", "Hành động"],
    "likesCount": 24,
    "status": "Approved",
    "createdAt": new Date("2026-08-03T09:15:00Z")
  },
  {
    "movieId": 102,
    "movieTitle": "Dune: Hành Tinh Cát 2",
    "userId": 4,
    "username": "phamvand",
    "rating": 3,
    "content": "Phim dài gần 3 tiếng hơi mệt, fan viễn tưởng chắc sẽ thích hơn.",
    "tags": ["Viễn tưởng", "Thời lượng dài"],
    "likesCount": 2,
    "status": "Pending",
    "createdAt": new Date("2026-08-04T08:00:00Z")
  }
]);

// 3. KHỞI TẠO COLLECTION 2: 'customer_feedbacks' (Phản hồi & Khiếu nại Hỗ trợ)
db.createCollection("customer_feedbacks");

// Nạp dữ liệu mẫu JSON vào collection 'customer_feedbacks'
db.customer_feedbacks.insertMany([
  {
    "userId": 1,
    "username": "nguyenvana",
    "email": "nguyenvana@gmail.com",
    "category": "Thanh toán",
    "subject": "Bị trừ tiền tài khoản Momo nhưng chưa nhận được mã vé QR",
    "content": "Tôi vừa thanh toán 180.000đ lúc 10h15 qua ví Momo, tiền đã trừ nhưng hệ thống chưa gửi vé về email.",
    "imageUrls": [
      "https://example.com/images/proof_momo_123.jpg"
    ],
    "status": "In Progress",
    "conversations": [
      {
        "sender": "Admin",
        "message": "Chào bạn, Ban quản trị đã ghi nhận mã giao dịch Momo và đang đối soát với ngân hàng.",
        "createdAt": new Date("2026-08-01T10:45:00Z")
      }
    ],
    "createdAt": new Date("2026-08-01T10:20:00Z")
  },
  {
    "userId": 2,
    "username": "tranthib",
    "email": "tranthib@gmail.com",
    "category": "Chất lượng rạp",
    "subject": "Góp ý về thái độ phục vụ tại quầy bắp nước Rạp 3",
    "content": "Nhân viên phục vụ chưa được thân thiện lúc 19h tối qua.",
    "imageUrls": [],
    "status": "Resolved",
    "conversations": [
      {
        "sender": "Admin",
        "message": "Cảm ơn bạn đã phản hồi. Quản lý rạp đã nhắc nhở ca trực và xin tặng bạn 1 voucher bắp nước.",
        "createdAt": new Date("2026-08-02T16:00:00Z")
      }
    ],
    "createdAt": new Date("2026-08-02T15:00:00Z")
  },
  {
    "userId": 5,
    "username": "hoangvan_e",
    "email": "hoangvane@gmail.com",
    "category": "Khác",
    "subject": "Hỏi về chương trình khuyến mãi sinh nhật tháng 8",
    "content": "Thành viên có sinh nhật trong tháng 8 được giảm bao nhiêu % khi mua vé tại rạp?",
    "imageUrls": [],
    "status": "New",
    "conversations": [],
    "createdAt": new Date("2026-08-04T11:00:00Z")
  }
]);

// ===============================================================================
// 4. BỘ CÂU LỆNH TRUY VẤN CRUD TRÊN MONGODB
// ===============================================================================

// --- [CRUD 1] TẠO VÀ ĐỌC (CREATE & READ) ---
// Lay tat ca danh sach danh gia cua phim 'Lat Mat 7' (movieId = 101) da duoc duyet
db.movie_reviews.find({ "movieId": 101, "status": "Approved" }).sort({ "likesCount": -1 });

// Lay danh sach khieu nai theo chuyen muc 'Thanh toan'
db.customer_feedbacks.find({ "category": "Thanh toán" });

// --- [CRUD 2] CẬP NHẬT (UPDATE) ---
// Tang 1 luot like cho bài review cua user 'nguyenvana'
db.movie_reviews.updateOne(
  { "movieId": 101, "username": "nguyenvana" },
  { "$inc": { "likesCount": 1 } }
);

// Admin tra loi khieu nai va chuyen trang thai thanh 'Resolved'
db.customer_feedbacks.updateOne(
  { "username": "hoangvan_e", "status": "New" },
  {
    "$set": { "status": "Resolved" },
    "$push": {
      "conversations": {
        "sender": "Admin",
        "message": "Chào bạn, thành viên có sinh nhật tháng 8 được tặng 1 vé miễn phí khi mua từ 2 vé trở lên!",
        "createdAt": new Date()
      }
    }
  }
);

// --- [CRUD 3] XÓA (DELETE) ---
// Xoa mot danh gia bi vi pham quy dinh
db.movie_reviews.deleteOne({ "username": "phamvand", "status": "Pending" });

// ===============================================================================
// 5. BỘ CÂU LỆNH AGGREGATION PIPELINE NÂNG CAO (YÊU CẦU ĐỒ ÁN BẮT BUỘC)
// ===============================================================================

// --- [AGGREGATION 1] Thong ke diem danh gia trung binh ($avg) va tong so review ($sum) theo từng bộ phim ---
db.movie_reviews.aggregate([
  {
    $match: { "status": "Approved" }
  },
  {
    $group: {
      _id: "$movieId",
      movieTitle: { $first: "$movieTitle" },
      avgRating: { $avg: "$rating" },
      totalReviews: { $sum: 1 },
      totalLikes: { $sum: "$likesCount" }
    }
  },
  {
    $project: {
      _id: 0,
      movieId: "$_id",
      movieTitle: 1,
      avgRating: { $round: ["$avgRating", 1] },
      totalReviews: 1,
      totalLikes: 1
    }
  }
]);

// --- [AGGREGATION 2] Thong ke so luong phan hoi/khieu nai theo từng chuyen muc (category) ---
db.customer_feedbacks.aggregate([
  {
    $group: {
      _id: "$category",
      totalTickets: { $sum: 1 },
      resolvedCount: {
        $sum: { $cond: [{ $eq: ["$status", "Resolved"] }, 1, 0] }
      },
      pendingCount: {
        $sum: { $cond: [{ $ne: ["$status", "Resolved"] }, 1, 0] }
      }
    }
  },
  {
    $project: {
      _id: 0,
      category: "$_id",
      totalTickets: 1,
      resolvedCount: 1,
      pendingCount: 1
    }
  }
]);
