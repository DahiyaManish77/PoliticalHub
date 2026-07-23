using PoliticalLeaderPortal.Areas.Admin.Services;
using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    public class AppDownloadSettingController : Controller
    {
        private readonly AppDownloadSettingService _service;

        public AppDownloadSettingController()
        {
            _service = new AppDownloadSettingService();
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View(_service.GetSetting());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(AppDownloadSettingVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _service.Save(model);
            TempData["Success"] = "App download settings updated successfully.";

            return RedirectToAction("Index");
        }
    }
}
