using PoliticalLeaderPortal.Areas.Admin.Services;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly DashboardService _service;

        public DashboardController()
        {
            _service = new DashboardService();
        }

        public ActionResult Index()
        {
            ViewBag.Title = "Dashboard";
            return View(_service.GetDashboard());
        }
    }
}
