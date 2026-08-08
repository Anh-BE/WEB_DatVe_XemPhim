using doan3.Models;
using doan3.Models.Cass;
using doan3.Models.Cass.DTO;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace doan3.Controllers
{
    public class ThongTinCaNhanController : Controller
    {
        private LTW_DatVeXemPhimEntities db = new LTW_DatVeXemPhimEntities();

        // =====================================================================
        // GET: /ThongTinCaNhan/Index
        // =====================================================================
        [HttpGet]
        public ActionResult Index()
        {
            var userSession = LayUserSession();
            if (userSession == null) return RedirectToAction("Index_DangNhap", "Login");

            var khachHang = LayThongTinKhachHang(userSession.UserID);
            if (khachHang == null) return RedirectToAction("SignOut", "Login");

            ViewBag.MatKhauCu = LayMatKhauHienTai(userSession.UserID);
            return View(khachHang);
        }

        // =====================================================================
        // GET: /ThongTinCaNhan/DoiMatKhau
        // =====================================================================
        [HttpGet]
        public ActionResult DoiMatKhau()
        {
            if (LayUserSession() == null) return RedirectToAction("Index_DangNhap", "Login");
            return View();
        }

        // =====================================================================
        // POST: /ThongTinCaNhan/DoiMatKhau
        //
        // Log Cassandra 3 trường hợp:
        //   CASE 1: Sai mật khẩu cũ          -> CHANGE_PASSWORD FAILED
        //   CASE 2: Mật khẩu mới trùng cũ    -> CHANGE_PASSWORD FAILED
        //   CASE 3: Đổi thành công            -> CHANGE_PASSWORD SUCCESS
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DoiMatKhau(DoiMatKhauViewModel model)
        {
            var userSession = LayUserSession();
            if (userSession == null) return RedirectToAction("Index_DangNhap", "Login");

            string browser = Request.Browser.Browser + " " + Request.Browser.Version;
            string device = Request.Browser.IsMobileDevice ? "Mobile" : "Desktop";
            string os = Request.Browser.Platform;
            string ip = Request.UserHostAddress;
            string httpMethod = Request.HttpMethod;

            // ------------------------------------------------------------------
            // CASE 1: Kiểm tra mật khẩu cũ có đúng không
            // ------------------------------------------------------------------
            if (!KiemTraMatKhauDung(userSession.UserID, model.MatKhauCu))
            {
                ModelState.AddModelError("MatKhauCu", "Mật khẩu hiện tại không đúng.");

                // Cassandra: CHANGE_PASSWORD FAILED — sai mật khẩu cũ
                CassandraFeaturesService.GhiNhatKyHoatDong(new NhatKyHoatDongDTO
                {
                    Username = userSession.UserName,
                    HanhDong = "CHANGE_PASSWORD",
                    KetQua = "FAILED",
                    ChiTiet = "That bai: sai mat khau hien tai",
                    ControllerName = "ThongTinCaNhan",
                    ActionName = "DoiMatKhau",
                    RequestMethod = httpMethod,
                    Browser = browser,
                    Device = device,
                    HeDieuHanh = os,
                    IpAddress = ip
                });

                return View(model);
            }

            // ModelState bao gồm các validation annotation (Required, StringLength, Compare)
            if (!ModelState.IsValid)
                return View(model);

            // ------------------------------------------------------------------
            // CASE 2: Mật khẩu mới không được trùng mật khẩu cũ
            // ------------------------------------------------------------------
            if (model.MatKhauMoi == model.MatKhauCu)
            {
                ModelState.AddModelError("MatKhauMoi", "Mật khẩu mới không được trùng mật khẩu cũ.");

                // Cassandra: CHANGE_PASSWORD FAILED — mật khẩu mới trùng cũ
                CassandraFeaturesService.GhiNhatKyHoatDong(new NhatKyHoatDongDTO
                {
                    Username = userSession.UserName,
                    HanhDong = "CHANGE_PASSWORD",
                    KetQua = "FAILED",
                    ChiTiet = "That bai: mat khau moi khong duoc trung mat khau cu",
                    ControllerName = "ThongTinCaNhan",
                    ActionName = "DoiMatKhau",
                    RequestMethod = httpMethod,
                    Browser = browser,
                    Device = device,
                    HeDieuHanh = os,
                    IpAddress = ip
                });

                return View(model);
            }

            // ------------------------------------------------------------------
            // CASE 3: Cập nhật mật khẩu mới vào SQL Server
            // ------------------------------------------------------------------
            bool ketQua = CapNhatMatKhau(userSession.UserID, model.MatKhauMoi);

            if (ketQua)
            {
                // Cassandra: CHANGE_PASSWORD SUCCESS
                CassandraFeaturesService.GhiNhatKyHoatDong(new NhatKyHoatDongDTO
                {
                    Username = userSession.UserName,
                    HanhDong = "CHANGE_PASSWORD",
                    KetQua = "SUCCESS",
                    ChiTiet = "Doi mat khau thanh cong",
                    ControllerName = "ThongTinCaNhan",
                    ActionName = "DoiMatKhau",
                    RequestMethod = httpMethod,
                    Browser = browser,
                    Device = device,
                    HeDieuHanh = os,
                    IpAddress = ip
                });

                TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                return RedirectToAction("Index");
            }
            else
            {
                // Lỗi kỹ thuật khi SaveChanges (hiếm gặp)
                ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật dữ liệu. Vui lòng thử lại.");
                return View(model);
            }
        }

        // =====================================================================
        // Helpers (private)
        // =====================================================================
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[CapNhatMatKhau] Loi: " + ex.Message);
                return false;
            }
        }
    }
}
