using AzureMediaPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AzureMediaPortal.Controllers
{
    public class HomeController : Controller
    {
        private AzureMediaPortalContext db = new AzureMediaPortalContext();

        public ActionResult Index() {
            ViewBag.Message = "Welcome to Watch and Learn. View and upload instructional exercise videos to aid in your fitness goals.";

            return View();
        }

        //public ActionResult Index() {
        //    ViewBag.Message = "Login to upload and view your videos";
        //    return View(db.MediaElements.Where(m => m.IsPublic.Equals(true)).ToList());
        //}

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
