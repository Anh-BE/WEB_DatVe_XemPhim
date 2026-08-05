using System.Collections.Generic;
using System.Web.Mvc;
using doan3.Models.Mgdb;

namespace doan3.Controllers
{
    /// <summary>
    /// Controller quản lý Trung tâm Phản hồi & Khiếu nại Hỗ trợ (Sử dụng MongoDB Collection: 'customer_feedbacks')
    /// </summary>
    public class MgdbCustomerFeedbackController : Controller
    {
        // GET: /MgdbCustomerFeedback
        public ActionResult Index()
        {
            var user = Session["User"] as doan3.Models.NguoiDung;
            string username = user != null ? user.UserName : "nguyenvana";

            // Lấy danh sách phản hồi của user từ MongoDB
            List<MgdbCustomerFeedbackModel> feedbacks = MgdbService.GetFeedbacksByUser(username);

            // Lấy thống kê chuyên mục bằng MongoDB Aggregation Pipeline
            List<MgdbFeedbackCategoryStats> categoryStats = MgdbService.GetFeedbackCategoryStats();
            ViewBag.CategoryStats = categoryStats;

            return View(feedbacks);
        }

        // POST: /MgdbCustomerFeedback/CreateTicket
        [HttpPost]
        public ActionResult CreateTicket(string category, string subject, string content, string imageUrl)
        {
            var user = Session["User"] as doan3.Models.NguoiDung;
            string username = user != null ? user.UserName : "nguyenvana";
            int userId = user != null ? user.UserID : 1;
            string email = user != null ? (user.Name + "@email.com") : "nguyenvana@gmail.com";

            var feedback = new MgdbCustomerFeedbackModel
            {
                UserId = userId,
                Username = username,
                Email = email,
                Category = category,
                Subject = subject,
                Content = content,
                ImageUrls = !string.IsNullOrEmpty(imageUrl) ? new List<string> { imageUrl } : new List<string>()
            };

            bool success = MgdbService.AddFeedback(feedback);
            if (success)
            {
                TempData["Message"] = "Gửi yêu cầu hỗ trợ thành công đến MongoDB!";
            }
            else
            {
                TempData["Error"] = "Không thể gửi yêu cầu đến MongoDB!";
            }

            return RedirectToAction("Index");
        }

        // POST: /MgdbCustomerFeedback/ReplyTicket (Dành cho Admin trả lời)
        [HttpPost]
        public ActionResult ReplyTicket(string feedbackId, string replyMessage)
        {
            bool success = MgdbService.ReplyFeedback(feedbackId, replyMessage, "Admin");
            if (success)
            {
                TempData["Message"] = "Admin đã phản hồi khiếu nại trên MongoDB!";
            }
            return RedirectToAction("Index");
        }
    }
}
