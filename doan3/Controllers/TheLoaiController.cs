using doan3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace doan3.Controllers
{
    public class TheLoaiController : Controller
    {
        //
        // GET: /TheLoai/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult TheLoai_PartialView()
        {
            TheLoaiPhim tl = new TheLoaiPhim();
            
            return PartialView(tl.GetTheLoai());
        }
    }
}
