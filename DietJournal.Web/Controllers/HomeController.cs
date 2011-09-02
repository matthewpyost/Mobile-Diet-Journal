using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DietJournal.Web.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Journal");
            }
            
            return View();
        }

        public ActionResult Food()
        {
            return View();
        }

        public ActionResult Water()
        {
            return View();
        }

        public ActionResult Supplements()
        {
            return View();
        }

        public ActionResult Exercise()
        {
            return View();
        }

        public ActionResult Donate()
        {
            return View();
        }

        public ActionResult Feedback()
        {
            return View();
        }

        public ActionResult Privacy()
        {
            return View();
        }

        public ActionResult Conditions()
        {
            return View();
        }
    }
}
