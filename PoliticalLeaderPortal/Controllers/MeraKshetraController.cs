using PoliticalLeaderPortal.Services;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Controllers
{
    public class MeraKshetraController : Controller
    {
        private readonly MeraKshetraService _service;

        public MeraKshetraController()
        {
            _service = new MeraKshetraService();
        }

        public ActionResult Index()
        {
            ViewBag.Title = "Mera Kshetra | Sardhana";
            return View(_service.GetActive());
        }
    }
}
