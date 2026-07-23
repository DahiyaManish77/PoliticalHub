using System.Web.Mvc;

namespace PoliticalLeaderPortal.Controllers
{
    public class SomAppController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Som App";
            return View();
        }
    }
}
