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
        public ActionResult Index(string search = "")
        {
            var userSession = Session["USER_SESSION"] as doan3.Models.UserLogin;
            var groups = Session["SESSION_GROUP"] as List<string>;
            bool isAdmin = (userSession != null && userSession.GroupID == "1") || (groups != null && groups.Contains("Admin"));
            string username = userSession != null ? userSession.UserName : "";
            int userId = userSession != null ? userSession.UserID : 0;

            ViewBag.UserSession = userSession;
            ViewBag.IsAdmin = isAdmin;
            ViewBag.CurrentSearch = search ?? "";

            if (userSession == null)
            {
                return View(new List<MgdbCustomerFeedbackModel>());
            }

            // Nếu là Admin: xem & tìm kiếm toàn bộ khiếu nại của khách hàng trong MongoDB; Nếu là Khách hàng: xem khiếu nại của chính mình
            List<MgdbCustomerFeedbackModel> feedbacks;
            if (isAdmin)
            {
                feedbacks = !string.IsNullOrWhiteSpace(search)
                    ? MgdbService.SearchFeedbacksForAdmin(search)
                    : MgdbService.GetAllFeedbacks();
            }
            else
            {
                feedbacks = MgdbService.GetFeedbacksByUser(username, userId);
            }

            // Lấy thống kê chuyên mục bằng MongoDB Aggregation Pipeline
            List<MgdbFeedbackCategoryStats> categoryStats = MgdbService.GetFeedbackCategoryStats();
            ViewBag.CategoryStats = categoryStats;

            return View(feedbacks);
        }

        // POST: /MgdbCustomerFeedback/CreateTicket
        [HttpPost]
        public ActionResult CreateTicket(string category, string subject, string content, string imageUrl)
        {
            var userSession = Session["USER_SESSION"] as doan3.Models.UserLogin;
            string username = userSession != null ? userSession.UserName : "Khách hàng";
            int userId = userSession != null ? userSession.UserID : 1;
            string email = userSession != null ? (userSession.UserName + "@gmail.com") : "nguyenvana@gmail.com";

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

            string errorMsg = MgdbService.AddFeedback(feedback);
            if (errorMsg == null)
            {
                TempData["Message"] = "Gửi yêu cầu hỗ trợ thành công đến MongoDB!";
            }
            else
            {
                TempData["Error"] = "Lỗi MongoDB: " + errorMsg;
            }

            return RedirectToAction("Index");
        }

        // POST: /MgdbCustomerFeedback/DeleteTicket (Dành riêng cho Khách hàng xóa khiếu nại của chính mình)
        [HttpPost]
        public ActionResult DeleteTicket(string feedbackId)
        {
            var userSession = Session["USER_SESSION"] as doan3.Models.UserLogin;
            var groups = Session["SESSION_GROUP"] as List<string>;
            bool isAdmin = (userSession != null && userSession.GroupID == "1") || (groups != null && groups.Contains("Admin"));
            string username = userSession != null ? userSession.UserName : "";

            if (userSession == null || isAdmin)
            {
                return RedirectToAction("Index");
            }

            bool success = MgdbService.DeleteFeedback(feedbackId, username, false);
            if (success)
            {
                TempData["Message"] = "Đã xóa yêu cầu khiếu nại thành công khỏi MongoDB!";
            }
            else
            {
                TempData["Error"] = "Không thể xóa khiếu nại này!";
            }
            return RedirectToAction("Index");
        }
    }
}
