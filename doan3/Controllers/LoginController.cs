using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using doan3.Models;
namespace doan3.Controllers
{
    public class LoginController : Controller
    {
        //
        // GET: /Login/
        LTW_DatVeXemPhimEntities db = new LTW_DatVeXemPhimEntities();
        public ActionResult Index_DangNhap()
        {
            if(Request.Cookies["Username"] != null && Request.Cookies["Password"] != null) 
            {
                ViewBag.username = Request.Cookies["Username"].Value;
                ViewBag.username = Request.Cookies["Password"].Value;
            }
            return View();
        }
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

        [HttpPost]
        public ActionResult Login(string username, string password, string ghinho)
        {
            if (Request.Cookies["username"] != null && Request.Cookies["password"] != null)
            {
                username = Request.Cookies["username"].Value;
                password = Request.Cookies["password"].Value;
            }



            if (checkpassword(username, password))
            {
                
                var userDb = db.NguoiDungs
                               .SingleOrDefault(x => x.UserName == username && x.Password == password);

                if (userDb != null)
                {
                    var userSession = new UserLogin();
                    userSession.UserName = username;

                    userSession.UserID = userDb.UserID;
                   
                    userSession.GroupID = userDb.GroupID.ToString(); 

                  
                    var khachHang = db.Khach_Hang
                                      .FirstOrDefault(kh => kh.UserID == userDb.UserID);

                    if (khachHang != null)
                    {
                       
                        userSession.FullName = khachHang.TenDayDu;
                    }
                    else
                    {
                       
                        userSession.FullName = userDb.Name;
                    }

                    var listGroups = GetListGroupID(username);
                    Session.Add("SESSION_GROUP", listGroups);
                    Session.Add("USER_SESSION", userSession);

                    

                    if (ghinho == "on")
                        ghinhotaikhoan(username, password);

                    return Redirect("~/Home/PhimDangChieu");
                }               
            }
            ViewBag.Error = "Bạn đã nhập sai tài khoản hay mật khẩu";
            return View("Index_DangNhap");

        }
        public List<string> GetListGroupID(string userName)
        {
            

            var data = (from a in db.NhomNguoiDungs
                        join b in db.NguoiDungs on a.ID equals b.GroupID
                        where b.UserName == userName

                        select new
                        {
                            UserGroupID = b.GroupID,
                            UserGroupName = a.Name
                        });

            return data.Select(x => x.UserGroupName).ToList();

        }
        public bool checkpassword(string username, string password)
        {
            if (db.NguoiDungs.Where(x => x.UserName == username && x.Password == password).Count() > 0)

                return true;
            else
                return false;


        }




        public ActionResult SignOut()
        {
            
            Session["USER_SESSION"] = null;
            Session["SESSION_GROUP"] = null;

            
            Session.Abandon();
            Session.Clear();

           
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


        [ChildActionOnly]
        public ActionResult thongtindangnhap()
        {
            return PartialView("ThongTinDangNhap");

        }

    }
}
