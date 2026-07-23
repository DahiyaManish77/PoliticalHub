using PoliticalLeaderPortal.Areas.Admin.Infrastructure;
using PoliticalLeaderPortal.Areas.Admin.Services;
using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    [RoleMenuAuthorize]
    public class VoterBackupSettingsController : Controller
    {
        private readonly VoterBackupSettingsService service;
        private readonly VoterService voterService;

        public VoterBackupSettingsController()
        {
            service = new VoterBackupSettingsService();
            voterService = new VoterService();
        }

        public ActionResult Index()
        {
            return View(service.Get());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(VoterBackupSettingsVM model)
        {
            string message;

            if (!service.Save(model, out message))
            {
                ModelState.AddModelError("", message);
                return View(model);
            }

            TempData["SuccessMessage"] = message;
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerateNow()
        {
            voterService.GenerateBackup();
            TempData["SuccessMessage"] = "Voter backup generated successfully.";
            return RedirectToAction("Index");
        }
    }
}
