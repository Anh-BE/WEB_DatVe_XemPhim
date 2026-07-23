using doan3.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace doan3.Controllers
{
    public class HoTroKhachHangController : Controller
    {
        private LTW_DatVeXemPhimEntities db = new LTW_DatVeXemPhimEntities();

       
        [HttpGet]
        public ActionResult Index(string searchKeyword = "", long? pendingOrderId = null)
        {
            
            var session = Session["USER_SESSION"] as UserLogin;
            if (session == null || session.GroupID?.ToString() != "3") return RedirectToAction("Index_DangNhap", "Login");

            
            var query = from u in db.NguoiDungs
                        join k in db.Khach_Hang on u.UserID equals k.UserID
                        where u.GroupID == 2
                        select new { User = u, Info = k };

            if (!string.IsNullOrEmpty(searchKeyword))
            {
                string key = searchKeyword.ToLower();
                query = query.Where(x => x.User.Name.ToLower().Contains(key) || x.Info.SoDienThoai.Contains(key));
            }

            var model = query.OrderBy(x => x.User.UserID).ToList()
                             .Select(x => new NguoiDungChiTietViewModel
                             {
                                 UserID = x.User.UserID,
                                 UserName = x.User.UserName,
                                 Password = x.User.Password,
                                 HoTen = x.User.Name,
                                 Email = x.Info.Email,
                                 SoDienThoai = x.Info.SoDienThoai,
                                 DiemThanhVien = x.Info.DiemThanhVien
                             }).ToList();

            ViewBag.CurrentFilter = searchKeyword;
            ViewBag.PendingOrderId = pendingOrderId; 

            return View(model);
        }

        
        [HttpPost]
        public ActionResult XacNhanTichDiem(long orderId, int targetUserId)
        {
         
            var session = Session["USER_SESSION"] as UserLogin;
            if (session == null || session.GroupID?.ToString() != "3")
                return RedirectToAction("Index_DangNhap", "Login");

            try
            {
                
                var donHang = db.Don_Dat_Ve.Find(orderId);
                var khach = db.Khach_Hang.FirstOrDefault(k => k.UserID == targetUserId); 

                if (donHang != null && khach != null)
                {
                   
                    int diemCong = 0;
                    var listVe = db.Chi_Tiet_Ve.Where(c => c.DonDatVeID == orderId).ToList();

                    foreach (var ve in listVe)
                    {
                        string loai = (ve.LoaiGhe ?? "").ToLower();
                        if (loai.Contains("vip")) diemCong += 2;
                        else if (loai.Contains("doi")) diemCong += 3;
                        else diemCong += 1;
                    }

                    
                    donHang.KhachHangID = khach.KhachHangID;
                    khach.DiemThanhVien = (khach.DiemThanhVien ?? 0) + diemCong; 

                    db.SaveChanges();

                   
                    TempData["SuccessMessage"] = $"Đã tích thành công {diemCong} điểm cho khách: {khach.TenDayDu}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
            }

            
            return RedirectToAction("Index");
        }
    }
}