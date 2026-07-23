using PoliticalLeaderPortal.Areas.Admin.Services;
using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    public class WebsiteFooterSettingController
        : Controller
    {
        private readonly WebsiteFooterSettingService _service;

        public WebsiteFooterSettingController()
        {
            _service =
                new WebsiteFooterSettingService();
        }

        [HttpGet]
        public ActionResult Index()
        {
            WebsiteFooterSettingVM model =
                _service.GetSetting();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(
            WebsiteFooterSettingVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _service.Save(
                model,
                Server);

            TempData["Success"] =
                "Footer settings saved successfully.";

            return RedirectToAction(
                "Index");
        }
    }
}