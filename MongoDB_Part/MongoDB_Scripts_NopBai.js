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
// 4. BỘ CÂU LỆNH TRUY VẤN CRUD TRÊN MONGODB
// ===============================================================================

// --- [CRUD 1] TẠO VÀ ĐỌC (CREATE & READ) ---
// Lấy danh sách khuyến mãi đang hoạt động thuộc chuyên mục 'Vé xem phim'
db.cinema_promotions.find({ "category": "Vé xem phim", "status": "Active" });

// Lấy danh sách khiếu nại theo chuyên mục 'Thanh toán'
db.customer_feedbacks.find({ "category": "Thanh toán" });

// --- [CRUD 2] CẬP NHẬT (UPDATE) ---
// Khách hàng bấm lấy mã Voucher -> Giảm số lượng còn lại 1 và tăng số lượng đã lấy 1 (Atomic Update)
db.cinema_promotions.updateOne(
  { "code": "MOVANA50K" },
  { "$inc": { "quantity": -1, "claimedCount": 1 } }
);

// Admin trả lời khiếu nại và chuyển trạng thái thành 'Resolved'
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
// Admin xóa một mã khuyến mãi đã hết hạn khỏi MongoDB
db.cinema_promotions.deleteOne({ "code": "VNPAY20" });

// Người dùng xóa một yêu cầu khiếu nại của mình khỏi MongoDB
db.customer_feedbacks.deleteOne({ "username": "kh1_long", "subject": "tiền chưa về ngân hàng" });

// ===============================================================================
// 5. BỘ CÂU LỆNH AGGREGATION PIPELINE NÂNG CAO (YÊU CẦU ĐỒ ÁN BẮT BUỘC)
// ===============================================================================

// --- [AGGREGATION 1] Thống kê tổng số mã Voucher và lượt cấp theo từng chuyên mục Khuyến mãi ---
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

// --- [AGGREGATION 2] Thống kê số lượng phản hồi/khiếu nại theo từng chuyên mục (category) ---
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
