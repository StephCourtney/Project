using AzureMediaPortal.Models;
using System.Web.Mvc;

namespace AzureMediaPortal.Controllers {
    public class HomeController : Controller {
        private AzureMediaPortalContext db = new AzureMediaPortalContext();

        public ActionResult Index() {
            ViewBag.Message = "Welcome to Watch and Learn. View and upload instructional exercise videos to aid in your fitness goals.";

            return View();
        }

    }
}
