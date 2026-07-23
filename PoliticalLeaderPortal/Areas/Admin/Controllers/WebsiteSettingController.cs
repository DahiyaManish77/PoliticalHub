using PoliticalLeaderPortal.Areas.Admin.Services;
using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    public class WebsiteSettingController : Controller
    {
        private readonly WebsiteSettingService _service;

        public WebsiteSettingController()
        {
            _service = new WebsiteSettingService();
        }

        [HttpGet]
        public ActionResult Index()
        {
            WebsiteSettingVM model =
                _service.GetSetting();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(
            WebsiteSettingVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _service.Save(
                model,
                Server);

            TempData["Success"] =
                "Website settings updated successfully.";

            return RedirectToAction("Index");
        }
    }
}