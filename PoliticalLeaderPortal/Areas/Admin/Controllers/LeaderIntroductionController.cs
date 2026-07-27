using PoliticalLeaderPortal.Areas.Admin.Services;
using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using PoliticalLeaderPortal.Infrastructure.Uploads;
using System;
using System.IO;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    [Authorize]
    public class LeaderIntroductionController : Controller
    {
        private readonly LeaderIntroductionService _service;

        public LeaderIntroductionController()
        {
            _service = new LeaderIntroductionService();
        }

        public ActionResult Index()
        {
            ViewBag.Title = "Leader Introduction";
            return View(_service.GetSetting());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(LeaderIntroductionVM model)
        {
            ViewBag.Title = "Leader Introduction";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                if (model.PortraitImageFile != null && model.PortraitImageFile.ContentLength > 0)
                {
                    string extension = SecureUploadValidator.ValidateImage(
                        model.PortraitImageFile,
                        5 * 1024 * 1024,
                        false);
                    string folder = Server.MapPath("~/Uploads/Leader");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    string fileName = "leader-introduction-" + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                    string physicalPath = Path.Combine(folder, fileName);
                    model.PortraitImageFile.SaveAs(physicalPath);
                    model.PortraitImagePath = "~/Uploads/Leader/" + fileName;
                }

                _service.Save(model);
                TempData["SuccessMessage"] = "Leader introduction updated successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Unable to update leader introduction. " + ex.Message;
                return View(model);
            }
        }
    }
}
