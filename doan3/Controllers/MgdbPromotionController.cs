using System;
using System.Collections.Generic;
using System.Web.Mvc;
using doan3.Models.Mgdb;

namespace doan3.Controllers
{
    public class MgdbPromotionController : Controller
    {
        // GET: /MgdbPromotion/Index
        public ActionResult Index(string category = "Tất cả", string search = "")
        {
            var userSession = Session["USER_SESSION"] as doan3.Models.UserLogin;
            var groups = Session["SESSION_GROUP"] as List<string>;
            bool isAdmin = (userSession != null && userSession.GroupID == "1") || (groups != null && groups.Contains("Admin"));

            ViewBag.UserSession = userSession;
            ViewBag.IsAdmin = isAdmin;
            ViewBag.CurrentCategory = category;
            ViewBag.CurrentSearch = search;

            // Lấy danh sách khuyến mãi từ MongoDB (Có tìm kiếm & Lọc)
            List<MgdbPromotionModel> promotions = MgdbService.GetPromotions(category, search);

            // Thống kê chuyên mục Khuyến mãi bằng MongoDB Aggregation Pipeline
            List<MgdbPromotionCategoryStats> categoryStats = MgdbService.GetPromotionCategoryStats();
            ViewBag.CategoryStats = categoryStats;

            return View(promotions);
        }

        // POST: /MgdbPromotion/CreatePromotion (Dành cho Admin đăng khuyến mãi mới)
        [HttpPost]
        public ActionResult CreatePromotion(string title, string code, string category, decimal discountAmount, int quantity, string content, string imageUrl, string tagsStr)
        {
            var userSession = Session["USER_SESSION"] as doan3.Models.UserLogin;
            var groups = Session["SESSION_GROUP"] as List<string>;
            bool isAdmin = (userSession != null && userSession.GroupID == "1") || (groups != null && groups.Contains("Admin"));

            if (!isAdmin)
            {
                TempData["Error"] = "Chỉ có Admin mới có quyền đăng tin khuyến mãi!";
                return RedirectToAction("Index");
            }

            var tags = new List<string>();
            if (!string.IsNullOrEmpty(tagsStr))
            {
                var split = tagsStr.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var t in split)
                {
                    tags.Add(t.Trim());
                }
            }

            var promo = new MgdbPromotionModel
            {
                Code = !string.IsNullOrEmpty(code) ? code.Trim().ToUpper() : "KM" + DateTime.Now.Ticks.ToString().Substring(10),
                Title = title,
                Category = category,
                DiscountAmount = discountAmount,
                Quantity = quantity > 0 ? quantity : 100,
                Content = content,
                ImageUrl = imageUrl,
                Tags = tags,
                Status = "Active"
            };

            bool success = MgdbService.AddPromotion(promo);
            if (success)
            {
                TempData["Message"] = "Đã đăng chương trình Khuyến mãi / Voucher mới lên MongoDB thành công!";
            }
            else
            {
                TempData["Error"] = "Không thể đăng chương trình khuyến mãi!";
            }

            return RedirectToAction("Index");
        }

        // POST: /MgdbPromotion/ClaimVoucher (Dành cho Người dùng lấy mã)
        [HttpPost]
        public ActionResult ClaimVoucher(string promoId, string promoCode)
        {
            var userSession = Session["USER_SESSION"] as doan3.Models.UserLogin;
            if (userSession == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập tài khoản để nhận mã Voucher!";
                return RedirectToAction("Index");
            }

            bool success = MgdbService.ClaimVoucher(promoId, userSession.UserName);
            if (success)
            {
                TempData["Message"] = "🎉 Đã lưu mã Voucher " + promoCode + " vào Ví Voucher của bạn! Khi mua vé thanh toán, bạn có thể chọn tích sử dụng mã này.";
            }
            else
            {
                TempData["Error"] = "Bạn đã nhận mã Voucher này rồi (hoặc mã đã hết lượt phát hành)! Mỗi tài khoản chỉ được nhận 1 lần.";
            }

            return RedirectToAction("Index");
        }

        // POST: /MgdbPromotion/DeletePromotion (Dành cho Admin xóa khuyến mãi hết hạn)
        [HttpPost]
        public ActionResult DeletePromotion(string promoId)
        {
            var userSession = Session["USER_SESSION"] as doan3.Models.UserLogin;
            var groups = Session["SESSION_GROUP"] as List<string>;
            bool isAdmin = (userSession != null && userSession.GroupID == "1") || (groups != null && groups.Contains("Admin"));

            if (!isAdmin)
            {
                TempData["Error"] = "Chỉ Admin mới có quyền xóa chương trình khuyến mãi!";
                return RedirectToAction("Index");
            }

            bool success = MgdbService.DeletePromotion(promoId);
            if (success)
            {
                TempData["Message"] = "Đã xóa bài đăng khuyến mãi khỏi MongoDB!";
            }
            else
            {
                TempData["Error"] = "Không thể xóa khuyến mãi này!";
            }

            return RedirectToAction("Index");
        }
    }
}
