using doan3.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace doan3.Controllers
{
    public class QuanLyNguoiDungController : Controller
    {
        private LTW_DatVeXemPhimEntities db = new LTW_DatVeXemPhimEntities();

        
        private bool IsAdmin()
        {
            var session = Session["USER_SESSION"] as UserLogin;
        
            return session != null && session.GroupID?.ToString() == "1";
        }

  
        [HttpGet]
        public ActionResult QuanLyNguoiDung(string searchKeyword = "")
        {
            if (!IsAdmin()) return RedirectToAction("Index_DangNhap", "Login");

            var query = db.NguoiDungs.AsQueryable();

            
            if (!string.IsNullOrEmpty(searchKeyword))
            {
                string key = searchKeyword.ToLower();
                query = query.Where(u => u.Name.ToLower().Contains(key) ||
                                         db.Khach_Hang.Any(k => k.UserID == u.UserID && k.SoDienThoai.Contains(key)));
            }

           
            var data = query.OrderBy(u => u.UserID).ToList().Select(u => {
                var kh = db.Khach_Hang.FirstOrDefault(k => k.UserID == u.UserID);
                return new NguoiDungChiTietViewModel
                {
                    UserID = u.UserID,
                    UserName = u.UserName,
                    HoTen = u.Name,
                    TenQuyen = u.NhomNguoiDung.Name,
                    GroupID = u.GroupID ?? 2,
                    Email = kh?.Email,      
                    SoDienThoai = kh?.SoDienThoai,
                    Password = u.Password,
                    DiemThanhVien = kh?.DiemThanhVien ?? 0
                };
            }).ToList();

           
            var stats = db.NguoiDungs.GroupBy(u => u.NhomNguoiDung.Name)
                          .Select(g => new ThongKeQuyenViewModel { TenQuyen = g.Key, SoLuong = g.Count() }).ToList();

            ViewBag.CurrentFilter = searchKeyword;
            ViewBag.IsStaff = false; 

            return View(new QuanLyNguoiDungTongHopViewModel { ThongKeTheoQuyen = stats, DanhSachChiTiet = data });
        }

      
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ThayDoiQuyen(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Index_DangNhap", "Login");

            try
            {
                var user = db.NguoiDungs.Find(id);
                if (user == null) return RedirectToAction("QuanLyNguoiDung");

                if (user.UserName.ToLower() == "admin")
                {
                    TempData["ErrorMessage"] = "Không thể đổi quyền Admin gốc.";
                }
                else
                {
                   
                    user.GroupID = (user.GroupID == 2) ? 3 : 2;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = $"Đã cập nhật quyền cho {user.UserName}.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }
            return RedirectToAction("QuanLyNguoiDung");
        }

     
        public ActionResult XoaUser(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Index_DangNhap", "Login");
            var user = db.NguoiDungs.Find(id);
            return user == null ? (ActionResult)HttpNotFound() : View(user);
        }

        [HttpPost, ActionName("XoaUser")]
        [ValidateAntiForgeryToken]
        public ActionResult XacNhanXoa(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Index_DangNhap", "Login");

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var user = db.NguoiDungs.Find(id);
                    if (user == null || user.UserName.ToLower() == "admin")
                    {
                        TempData["ErrorMessage"] = "Không thể xóa tài khoản này.";
                        return RedirectToAction("QuanLyNguoiDung");
                    }

                  
                    var kh = db.Khach_Hang.FirstOrDefault(k => k.UserID == id);
                    if (kh != null)
                    {
                       
                        var gheDangKhoa = db.Khoa_Ghe_Tam_Thoi.Where(k => k.KhachHangID == kh.KhachHangID).ToList();
                        if (gheDangKhoa.Any())
                        {
                            db.Khoa_Ghe_Tam_Thoi.RemoveRange(gheDangKhoa);
                        }

                        
                        var dsDonHang = db.Don_Dat_Ve.Where(d => d.KhachHangID == kh.KhachHangID).ToList();
                        foreach (var donHang in dsDonHang)
                        {
                            var dsVe = db.Chi_Tiet_Ve.Where(v => v.DonDatVeID == donHang.DonDatVeID).ToList();
                            db.Chi_Tiet_Ve.RemoveRange(dsVe);
                            db.Don_Dat_Ve.Remove(donHang);
                        }

                     
                        db.Khach_Hang.Remove(kh);
                    }

                 
                    db.NguoiDungs.Remove(user);

                    db.SaveChanges();
                    transaction.Commit();

                    TempData["SuccessMessage"] = "Đã xóa tài khoản thành công!";
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                   
                    string innerMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    if (ex.InnerException?.InnerException != null)
                    {
                        innerMsg = ex.InnerException.InnerException.Message;
                    }
                    TempData["ErrorMessage"] = "Lỗi Database: " + innerMsg;
                }
            }
            return RedirectToAction("QuanLyNguoiDung");
        }
    }
}