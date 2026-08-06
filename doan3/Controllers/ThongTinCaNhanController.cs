using doan3.Models;
using System.Linq;
using System;
using System.Web.Mvc;
using System.Data.Entity;

namespace doan3.Controllers
{
    public class ThongTinCaNhanController : Controller
    {
        private LTW_DatVeXemPhimEntities db = new LTW_DatVeXemPhimEntities();
        private string username;

        [HttpGet]
        public ActionResult Index()
        {
            var userSession = LayUserSession();
            if (userSession == null) return RedirectToAction("Index_DangNhap", "Login");

            var khachHang = LayThongTinKhachHang(userSession.UserID);

           
            if (khachHang == null) return RedirectToAction("SignOut", "Login");

            try
            {
                doan3.Models.Cass.CassandraFeaturesService.GhiNhatKyHoatDong(
                    new doan3.Models.Cass.DTO.NhatKyHoatDongDTO
                    {
                        Username = username,

                        HanhDong = "Dang nhap",

                        ChiTiet = "Sai tai khoan hoac mat khau",

                        ControllerName = "Login",

                        ActionName = "Login",

                        RequestMethod = Request.HttpMethod,

                        Browser = Request.Browser.Browser + " " + Request.Browser.Version,

                        Device = Request.Browser.IsMobileDevice ? "Mobile" : "Desktop",

                        HeDieuHanh = Request.Browser.Platform,

                        IpAddress = Request.UserHostAddress,

                        KetQua = "Failed"
                    });
            }
            catch
            {
            }

            ViewBag.MatKhauCu = LayMatKhauHienTai(userSession.UserID);

            return View(khachHang);
        }

        [HttpGet]
        public ActionResult DoiMatKhau()
        {
            if (LayUserSession() == null) return RedirectToAction("Index_DangNhap", "Login");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DoiMatKhau(DoiMatKhauViewModel model)
        {
            var userSession = LayUserSession();
            if (userSession == null) return RedirectToAction("Index_DangNhap", "Login");

            if (!ModelState.IsValid) return View(model);

            
            if (!KiemTraMatKhauDung(userSession.UserID, model.MatKhauCu))
            {
                ModelState.AddModelError("MatKhauCu", "Mật khẩu hiện tại không đúng.");
                return View(model);
            }

            try
            {
                doan3.Models.Cass.CassandraFeaturesService.GhiNhatKyHoatDong(
                    new doan3.Models.Cass.DTO.NhatKyHoatDongDTO
                    {
                        Username = userSession.UserName,

                        HanhDong = "Doi mat khau",

                        ChiTiet = "Nhap sai mat khau hien tai",

                        ControllerName = "ThongTinCaNhan",

                        ActionName = "DoiMatKhau",

                        RequestMethod = Request.HttpMethod,

                        Browser = Request.Browser.Browser + " " + Request.Browser.Version,

                        Device = Request.Browser.IsMobileDevice ? "Mobile" : "Desktop",

                        HeDieuHanh = Request.Browser.Platform,

                        IpAddress = Request.UserHostAddress,

                        KetQua = "Failed"
                    });
            }
            catch
            {
            }

            if (model.MatKhauCu == model.MatKhauMoi)
            {
                try
                {
                    doan3.Models.Cass.CassandraFeaturesService.GhiNhatKyHoatDong(
                       new doan3.Models.Cass.DTO.NhatKyHoatDongDTO
                       {
                           Username = username,

                           HanhDong = "Dang nhap",

                           ChiTiet = "Dang nhap thanh cong",

                           ControllerName = "Login",

                           ActionName = "Login",

                           RequestMethod = Request.HttpMethod,

                           Browser = Request.Browser.Browser + " " + Request.Browser.Version,

                           Device = Request.Browser.IsMobileDevice ? "Mobile" : "Desktop",

                           HeDieuHanh = Request.Browser.Platform,

                           IpAddress = Request.UserHostAddress,

                           KetQua = "Success"
                       });
                }
                catch
                {
                }

                ModelState.AddModelError("MatKhauMoi", "Mật khẩu mới phải khác mật khẩu hiện tại.");
                return View(model);
            }

           
            bool ketQua = CapNhatMatKhau(userSession.UserID, model.MatKhauMoi);

            if (ketQua)
            {
                try
                {
                    doan3.Models.Cass.CassandraFeaturesService.GhiNhatKyHoatDong(
                        new doan3.Models.Cass.DTO.NhatKyHoatDongDTO
                        {
                            Username = userSession.UserName,
                            HanhDong = "Doi mat khau",
                            ChiTiet = "Nguoi dung doi mat khau thanh cong",
                            IpAddress = Request.UserHostAddress,

                            ControllerName = "ThongTinCaNhan",
                            ActionName = "DoiMatKhau",
                            RequestMethod = Request.HttpMethod,
                            Browser = Request.Browser.Browser + " " + Request.Browser.Version,
                            Device = Request.Browser.IsMobileDevice ? "Mobile" : "Desktop",
                            HeDieuHanh = Request.Browser.Platform,
                            KetQua = "Success"

                        });
                }
                catch
                {
                }

                TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật dữ liệu.");
                return View(model);
            }
        }

       

        private UserLogin LayUserSession()
        {
            return Session["USER_SESSION"] as UserLogin;
        }

        private Khach_Hang LayThongTinKhachHang(int userId)
        {
            
            var nguoiDung = db.NguoiDungs.FirstOrDefault(u => u.UserID == userId);
            if (nguoiDung == null) return null;

           
            return db.Khach_Hang.FirstOrDefault(k => k.UserID == userId);
        }

        private string LayMatKhauHienTai(int userId)
        {
            return db.NguoiDungs
                .Where(u => u.UserID == userId)
                .Select(u => u.Password)
                .FirstOrDefault();
        }

     
        private bool KiemTraMatKhauDung(int userId, string matKhauNhap)
        {
            var matKhauDb = LayMatKhauHienTai(userId);
         
            return matKhauDb == matKhauNhap;
        }

        private bool CapNhatMatKhau(int userId, string matKhauMoi)
        {
            try
            {
                var nguoiDung = db.NguoiDungs.Find(userId);
                if (nguoiDung == null) return false;

              
                nguoiDung.Password = matKhauMoi;

                db.Entry(nguoiDung).State = EntityState.Modified;
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                
                return false;
            }
        }
    }
}