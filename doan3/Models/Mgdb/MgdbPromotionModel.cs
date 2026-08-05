using System;
using System.Collections.Generic;

namespace doan3.Models.Mgdb
{
    public class MgdbPromotionModel
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string Category { get; set; } // Vé xem phim, Bắp nước, Ví điện tử, Khuyến mãi Sinh nhật
        public decimal DiscountAmount { get; set; }
        public int Quantity { get; set; }
        public int ClaimedCount { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public List<string> Tags { get; set; }
        public List<string> ClaimedUsers { get; set; } // Danh sách username đã nhận mã này
        public List<string> UsedUsers { get; set; } // Danh sách username đã sử dụng mã này khi mua vé
        public string Status { get; set; } // Active, Expired, Disabled
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public MgdbPromotionModel()
        {
            Tags = new List<string>();
            ClaimedUsers = new List<string>();
            UsedUsers = new List<string>();
            Status = "Active";
            StartDate = DateTime.UtcNow;
            EndDate = DateTime.UtcNow.AddMonths(1);
        }
    }

    public class MgdbPromotionCategoryStats
    {
        public string Category { get; set; }
        public int TotalPromotions { get; set; }
        public int TotalQuantityLeft { get; set; }
        public int TotalClaimed { get; set; }
    }
}
