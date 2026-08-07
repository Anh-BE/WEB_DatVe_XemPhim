// ===============================================================================
// ĐỒ ÁN DỮ LIỆU NOSQL - BÁO CÁO VÀ SCRIPT MONGODB
// Dự án: Web Đặt Vé Xem Phim (Movana Cinema)
// CSDL: MongoDB (Document Store) - Running on Docker (Port 27017)
// Hướng dẫn: Mở MongoDB Compass hoặc mongosh terminal và chạy toàn bộ tệp script này.
// ===============================================================================

// 1. CHUYỂN SANG CSDL 'CinemaNoSQL'
db = db.getSiblingDB("CinemaNoSQL");

// 2. KHỞI TẠO COLLECTION 1: 'cinema_promotions' (Tin tức & Kho Voucher Khuyến mãi)
db.createCollection("cinema_promotions");

// Nạp dữ liệu mẫu JSON vào collection 'cinema_promotions'
db.cinema_promotions.insertMany([
  {
    "code": "MOVANA50K",
    "title": "Ưu đãi Tháng 8 - Giảm 50K khi mua từ 2 vé xem phim",
    "category": "Vé xem phim",
    "discountAmount": 50000,
    "quantity": 100,
    "claimedCount": 18,
    "content": "Áp dụng cho tất cả các suất chiếu từ Thứ 2 đến Thứ 5 tại tất cả các rạp Movana Cinema trên toàn quốc.",
    "imageUrl": "https://images.unsplash.com/photo-1489599849927-2ee91cede3ba?w=600&auto=format&fit=crop",
    "tags": ["Vé xem phim", "Giảm 50K", "Tháng 8"],
    "status": "Active",
    "startDate": new Date("2026-08-01T00:00:00Z"),
    "endDate": new Date("2026-08-31T23:59:59Z")
  },
  {
    "code": "BAPCOMBO0D",
    "title": "Tặng Combo Bắp Nước 0đ khi thanh toán qua ví Momo",
    "category": "Bắp nước",
    "discountAmount": 35000,
    "quantity": 50,
    "claimedCount": 32,
    "content": "Nhận ngay 1 vé bắp ngọt + 1 nước ngọt size L miễn phí khi thanh toán vé xem phim trực tuyến qua ví Momo.",
    "imageUrl": "https://images.unsplash.com/photo-1585647347483-22b66260dfff?w=600&auto=format&fit=crop",
    "tags": ["Bắp nước", "Momo", "Combo 0đ"],
    "status": "Active",
    "startDate": new Date("2026-08-01T00:00:00Z"),
    "endDate": new Date("2026-08-15T23:59:59Z")
  },
  {
    "code": "SINNHATVIP",
    "title": "Quà tặng Sinh Nhật - Tặng 1 vé phim 2D miễn phí",
    "category": "Khuyến mãi Sinh nhật",
    "discountAmount": 110000,
    "quantity": 200,
    "claimedCount": 45,
    "content": "Dành riêng cho thành viên có sinh nhật trong tháng 8. Nhận 1 vé xem phim 2D hoàn toàn miễn phí khi mua cùng 1 vé.",
    "imageUrl": "https://images.unsplash.com/photo-1513151233558-d860c5398176?w=600&auto=format&fit=crop",
    "tags": ["Sinh nhật", "Vé 0đ", "Thành viên VIP"],
    "status": "Active",
    "startDate": new Date("2026-08-01T00:00:00Z"),
    "endDate": new Date("2026-08-31T23:59:59Z")
  },
  {
    "code": "VNPAY20",
    "title": "Giảm 20% tổng hóa đơn khi nhập mã VNPAYMOVANA",
    "category": "Ví điện tử",
    "discountAmount": 30000,
    "quantity": 80,
    "claimedCount": 12,
    "content": "Quét VNPAY-QR tại bước thanh toán để nhận ngay mức giảm 20% tối đa 30.000đ cho mọi đơn hàng.",
    "imageUrl": "https://images.unsplash.com/photo-1556742049-0a674640c66d?w=600&auto=format&fit=crop",
    "tags": ["VNPAY", "Giảm 20%", "Ví điện tử"],
    "status": "Active",
    "startDate": new Date("2026-08-01T00:00:00Z"),
    "endDate": new Date("2026-08-20T23:59:59Z")
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
    "status": "Resolved",
    "conversations": [
      {
        "sender": "Admin",
        "message": "Chào bạn, thành viên có sinh nhật tháng 8 được tặng 1 vé miễn phí khi mua từ 2 vé trở lên!",
        "createdAt": new Date("2026-08-04T13:35:31Z")
      }
    ],
    "createdAt": new Date("2026-08-04T11:00:00Z")
  }
]);

// ===============================================================================
// 4. BỘ CÂU LỆNH TRUY VẤN CRUD 
// ===============================================================================

// -------------------------------------------------------------------------------
// 4.1 COLLECTION 1: 'customer_feedbacks' (Trung Tâm Hỗ Trợ & Khiếu Nại)
// -------------------------------------------------------------------------------

// [CREATE] Khách hàng tạo mới 1 ticket khiếu nại (InsertOne)
db.customer_feedbacks.insertOne({
  "userId": 7,
  "username": "huy",
  "email": "huy@gmail.com",
  "category": "Thanh toán",
  "subject": "Bị trừ tiền Momo nhưng chưa nhận được mã vé QR",
  "content": "Tôi vừa thanh toán 180.000đ lúc 10h15 qua ví Momo, tiền đã trừ nhưng chưa có vé.",
  "imageUrls": [],
  "status": "New",
  "conversations": [],
  "createdAt": new Date()
});

// [READ - Find & Filter & Regex Search & Sort]
// a) Đọc danh sách khiếu nại của chính tài khoản đăng nhập (VD: 'huy') và sắp xếp mới nhất
db.customer_feedbacks.find({ "username": "huy" }).sort({ "createdAt": -1 });

// b) Lọc khiếu nại theo chuyên mục 'Thanh toán'
db.customer_feedbacks.find({ "category": "Thanh toán" });

// c) Admin Tìm kiếm khiếu nại theo từ khóa (Regex Search không phân biệt hoa thường)
db.customer_feedbacks.find({
  $or: [
    { "username": { $regex: "huy", $options: "i" } },
    { "email": { $regex: "huy", $options: "i" } }
  ]
});

// [UPDATE] Admin trả lời ticket và chuyển trạng thái sang 'Resolved' (Đẩy tin nhắn vào Embedded Array 'conversations')
db.customer_feedbacks.updateOne(
  { "username": "huy", "status": "New" },
  {
    "$set": { "status": "Resolved" },
    "$push": {
      "conversations": {
        "sender": "Admin",
        "message": "Chào bạn, Ban quản trị đã kiểm tra và hoàn tiền 180.000đ về ví Momo thành công!",
        "createdAt": new Date()
      }
    }
  }
);

// [DELETE] Người dùng hoặc Admin xóa 1 phiếu khiếu nại khỏi MongoDB
db.customer_feedbacks.deleteOne({ "username": "huy", "subject": "Bị trừ tiền Momo nhưng chưa nhận được mã vé QR" });


// -------------------------------------------------------------------------------
// 4.2 COLLECTION 2: 'cinema_promotions' (Kho Voucher & Mã Khuyến Mãi)
// -------------------------------------------------------------------------------

// [CREATE] Admin phát hành 1 mã khuyến mãi / Voucher giảm giá mới (InsertOne)
db.cinema_promotions.insertOne({
  "code": "KMTHANG8",
  "title": "Siêu Ưu Đãi Tháng 8 - Giảm 30K mọi đơn hàng",
  "category": "Vé xem phim",
  "discountAmount": 30000,
  "quantity": 150,
  "claimedCount": 0,
  "content": "Áp dụng cho mọi khách hàng đặt vé xem phim trong tháng 8.",
  "imageUrl": "https://example.com/promo_thang8.jpg",
  "tags": ["Vé xem phim", "Giảm 30K"],
  "status": "Active",
  "startDate": new Date("2026-08-01T00:00:00Z"),
  "endDate": new Date("2026-08-31T23:59:59Z")
});

// [READ - Find & Filter & Sort]
// a) Lấy danh sách Voucher đang hoạt động (Active) thuộc loại 'Vé xem phim' và sắp xếp theo mức giảm giá
db.cinema_promotions.find({ "category": "Vé xem phim", "status": "Active" }).sort({ "discountAmount": -1 });

// b) Tra cứu thông tin chi tiết của 1 mã Voucher cụ thể
db.cinema_promotions.find({ "code": "MOVANA50K" });

// [UPDATE] Khách hàng bấm lấy mã Voucher -> Giảm số lượng tồn 1 và tăng số lượt đã lấy 1 ($inc Atomic Update)
db.cinema_promotions.updateOne(
  { "code": "MOVANA50K", "quantity": { $gt: 0 } },
  { "$inc": { "quantity": -1, "claimedCount": 1 } }
);

// [DELETE] Admin xóa một mã khuyến mãi hết hạn khỏi MongoDB
db.cinema_promotions.deleteOne({ "code": "VNPAY20" });


// ===============================================================================
// 5. BỘ CÂU LỆNH AGGREGATION PIPELINE NÂNG CAO (YÊU CẦU ĐỒ ÁN BẮT BUỘC)
// ===============================================================================

// --- [AGGREGATION 1 FOR COLLECTION 'customer_feedbacks'] ---
// Thống kê tổng số ticket khiếu nại, số lượng đã giải quyết (Resolved) và đang xử lý (Pending) theo từng chuyên mục
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

// --- [AGGREGATION 2 FOR COLLECTION 'cinema_promotions'] ---
// Thống kê tổng số mã Voucher, tổng số lượng còn lại và tổng lượt đã nhận theo từng loại khuyến mãi
db.cinema_promotions.aggregate([
  {
    $group: {
      _id: "$category",
      totalPromotions: { $sum: 1 },
      totalQuantityLeft: { $sum: "$quantity" },
      totalClaimed: { $sum: "$claimedCount" }
    }
  },
  {
    $project: {
      _id: 0,
      category: "$_id",
      totalPromotions: 1,
      totalQuantityLeft: 1,
      totalClaimed: 1
    }
  }
]);
