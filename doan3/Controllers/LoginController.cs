using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using doan3.Models;
using doan3.Models.Cass;
using doan3.Models.Cass.DTO;

namespace doan3.Controllers
{
    public class LoginController : Controller
    {
        LTW_DatVeXemPhimEntities db = new LTW_DatVeXemPhimEntities();

        // =====================================================================
        // GET: /Login/Index_DangNhap
        // =====================================================================
        public ActionResult Index_DangNhap()
        {
            if (Request.Cookies["Username"] != null && Request.Cookies["Password"] != null)
            {
                ViewBag.username = Request.Cookies["Username"].Value;
                ViewBag.password = Request.Cookies["Password"].Value;
            }
            return View();
        }

        // =====================================================================
        // Ghi nhớ tài khoản qua cookie (plaintext — giữ nguyên theo thiết kế gốc)
        // =====================================================================
        public void ghinhotaikhoan(string username, string password)
        {
            HttpCookie us = new HttpCookie("username");
            HttpCookie pas = new HttpCookie("password");

            us.Value = username;
            pas.Value = password;

            us.Expires = DateTime.Now.AddDays(1);
            pas.Expires = DateTime.Now.AddDays(1);

            Response.Cookies.Add(us);
            Response.Cookies.Add(pas);
        }

        // =====================================================================
        // POST: /Login/Login
        // Log Cassandra: LOGIN SUCCESS hoặc LOGIN FAILED
        // =====================================================================
        [HttpPost]
        public ActionResult Login(string username, string password, string ghinho)
        {
            // Đọc từ cookie nếu đã ghi nhớ
            if (Request.Cookies["username"] != null && Request.Cookies["password"] != null)
            {
                username = Request.Cookies["username"].Value;
                password = Request.Cookies["password"].Value;
            }

            string browser = Request.Browser.Browser + " " + Request.Browser.Version;
            string device = Request.Browser.IsMobileDevice ? "Mobile" : "Desktop";
            string os = Request.Browser.Platform;
            string ip = Request.UserHostAddress;
            string httpMethod = Request.HttpMethod;

            if (checkpassword(username, password))
            {
                var userDb = db.NguoiDungs
                               .SingleOrDefault(x => x.UserName == username && x.Password == password);

                if (userDb != null)
                {
                    var userSession = new UserLogin
                    {
                        UserName = username,
                        UserID = userDb.UserID,
                        GroupID = userDb.GroupID.ToString()
                    };

                    var khachHang = db.Khach_Hang.FirstOrDefault(kh => kh.UserID == userDb.UserID);
                    userSession.FullName = khachHang != null ? khachHang.TenDayDu : userDb.Name;

                    var listGroups = GetListGroupID(username);
                    Session.Add("SESSION_GROUP", listGroups);
                    Session.Add("USER_SESSION", userSession);

                    // Redis: lưu session
                    string roleName = listGroups.FirstOrDefault() ?? "Customer";
                    RedisFeaturesService.SaveUserSession(username, userDb.UserID, userSession.FullName, roleName, 1800);

                    // Cassandra: ghi LOGIN SUCCESS
                    CassandraFeaturesService.GhiNhatKyHoatDong(new NhatKyHoatDongDTO
                    {
                        Username = username,
                        HanhDong = "LOGIN",
                        KetQua = "SUCCESS",
                        ChiTiet = "Dang nhap thanh cong",
                        ControllerName = "Login",
                        ActionName = "Login",
                        RequestMethod = httpMethod,
                        Browser = browser,
                        Device = device,
                        HeDieuHanh = os,
                        IpAddress = ip
                    });

                    if (ghinho == "on")
                        ghinhotaikhoan(username, password);

                    return Redirect("~/Home/PhimDangChieu");
                }
            }

            // Cassandra: ghi LOGIN FAILED
            CassandraFeaturesService.GhiNhatKyHoatDong(new NhatKyHoatDongDTO
            {
                Username = username ?? "khong_ro",
                HanhDong = "LOGIN",
                KetQua = "FAILED",
                ChiTiet = "Dang nhap that bai: sai tai khoan hoac mat khau",
                ControllerName = "Login",
                ActionName = "Login",
                RequestMethod = httpMethod,
                Browser = browser,
                Device = device,
                HeDieuHanh = os,
                IpAddress = ip
            });

            ViewBag.Error = "Bạn đã nhập sai tài khoản hay mật khẩu";
            return View("Index_DangNhap");
        }

        // =====================================================================
        // GET: /Login/SignOut
        // Log Cassandra: LOGOUT SUCCESS
        // =====================================================================
        public ActionResult SignOut()
        {
            var sessionUser = Session["USER_SESSION"] as UserLogin;

            if (sessionUser != null)
            {
                CassandraFeaturesService.GhiNhatKyHoatDong(new NhatKyHoatDongDTO
                {
                    Username = sessionUser.UserName,
                    HanhDong = "LOGOUT",
                    KetQua = "SUCCESS",
                    ChiTiet = "Nguoi dung dang xuat thanh cong",
                    ControllerName = "Login",
                    ActionName = "SignOut",
                    RequestMethod = Request.HttpMethod,
                    Browser = Request.Browser.Browser + " " + Request.Browser.Version,
                    Device = Request.Browser.IsMobileDevice ? "Mobile" : "Desktop",
                    HeDieuHanh = Request.Browser.Platform,
                    IpAddress = Request.UserHostAddress
                });
            }

            Session["USER_SESSION"] = null;
            Session["SESSION_GROUP"] = null;
            Session.Abandon();
            Session.Clear();

            // Xóa cookie ghi nhớ tài khoản
            if (Request.Cookies["username"] != null && Request.Cookies["password"] != null)
            {
                HttpCookie us = new HttpCookie("username");
                HttpCookie ps = new HttpCookie("password");
                ps.Expires = DateTime.Now.AddDays(-1);
                us.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(us);
                Response.Cookies.Add(ps);
            }

            return RedirectToAction("PhimDangChieu", "Home");
        }

        // =====================================================================
        // Helpers
        // =====================================================================
        public List<string> GetListGroupID(string userName)
        {
            var data = from a in db.NhomNguoiDungs
                       join b in db.NguoiDungs on a.ID equals b.GroupID
                       where b.UserName == userName
                       select new { UserGroupName = a.Name };

            return data.Select(x => x.UserGroupName).ToList();
        }

        public bool checkpassword(string username, string password)
        {
            return db.NguoiDungs.Any(x => x.UserName == username && x.Password == password);
        }

        [ChildActionOnly]
        public ActionResult thongtindangnhap()
        {
            return PartialView("ThongTinDangNhap");
        }
    }
}
