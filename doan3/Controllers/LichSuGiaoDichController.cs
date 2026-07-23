using doan3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Web.Mvc;

namespace doan3.Controllers
{
    public class LichSuGiaoDichController : Controller
    {
        //
        // GET: /LichSuGiaoDich/
        private LTW_DatVeXemPhimEntities db = new LTW_DatVeXemPhimEntities();

        // GET: User/LichSuDatVe
        public ActionResult LichSuDatVe()
        {
            
            if (Session["USER_SESSION"] == null)
            {
                return RedirectToAction("Index_DangNhap", "Login");
            }

           
            var sessionUser = Session["USER_SESSION"] as UserLogin; 
            var khachHang = db.Khach_Hang.FirstOrDefault(k => k.UserID == sessionUser.UserID);

            if (khachHang == null) return HttpNotFound();

            
            var history = db.Don_Dat_Ve
                            .Where(d => d.KhachHangID == khachHang.KhachHangID)
                            .Include(d => d.Chi_Tiet_Ve.Select(c => c.Lich_Chieu.Phim))
                            .Include(d => d.Chi_Tiet_Ve.Select(c => c.Lich_Chieu.Phong_Chieu.Rap_Chieu))
                            .Include(d => d.Chi_Tiet_Ve.Select(c => c.Ghe_Ngoi))
                            .OrderByDescending(d => d.ThoiGianDat) 
                            .ToList();

            return View(history);
        }
    }
}