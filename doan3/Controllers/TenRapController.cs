using doan3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace doan3.Controllers
{
    public class TenRapController : Controller
    {
        

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult TenRap_PartialView()
        {
            Ten_Rap tr = new Ten_Rap();

            return PartialView(tr.GetTenRap());
        }

    }
}
