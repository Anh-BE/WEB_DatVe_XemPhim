using System;
using System.Collections.Generic;

namespace doan3.Models.Mgdb
{
    /// <summary>
    /// Model chi tiết từng câu trả lời trong chuỗi phản hồi (Sub-document)
    /// </summary>
    public class MgdbFeedbackConversation
    {
        public string Sender { get; set; } // "User" hoac "Admin"
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }

        public MgdbFeedbackConversation()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Model lưu trữ Phản hồi và Khiếu nại của khách hàng trong MongoDB (Collection: 'customer_feedbacks')
    /// </summary>
    public class MgdbCustomerFeedbackModel
    {
        public string Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Category { get; set; } // "Thanh toán", "Chất lượng rạp", "Khác"
        public string Subject { get; set; }
        public string Content { get; set; }
        public List<string> ImageUrls { get; set; }
        public string Status { get; set; } // "New", "In Progress", "Resolved"
        public List<MgdbFeedbackConversation> Conversations { get; set; }
        public DateTime CreatedAt { get; set; }

        public MgdbCustomerFeedbackModel()
        {
            ImageUrls = new List<string>();
            Conversations = new List<MgdbFeedbackConversation>();
            Status = "New";
            CreatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// DTO chứa kết quả Thống kê Aggregation Pipeline cho Phản hồi hỗ trợ
    /// </summary>
    public class MgdbFeedbackCategoryStats
    {
        public string Category { get; set; }
        public int TotalTickets { get; set; }
        public int ResolvedCount { get; set; }
        public int PendingCount { get; set; }
    }
}
