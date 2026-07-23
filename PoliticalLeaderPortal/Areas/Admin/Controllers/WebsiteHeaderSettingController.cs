using PoliticalLeaderPortal.Areas.Admin.Services;
using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    public class WebsiteHeaderSettingController : Controller
    {
        private readonly WebsiteHeaderSettingService _service;

        public WebsiteHeaderSettingController()
        {
            _service = new WebsiteHeaderSettingService();
        }

        [HttpGet]
        public ActionResult Index()
        {
            var model = _service.GetSetting();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(
            WebsiteHeaderSettingVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _service.Save(model, Server);

            TempData["Success"] =
                "Header settings saved successfully.";

            return RedirectToAction("Index");
        }
    }
}