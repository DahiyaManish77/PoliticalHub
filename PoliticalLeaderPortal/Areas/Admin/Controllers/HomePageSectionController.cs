using PoliticalLeaderPortal.Areas.Admin.Services;
using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using System;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    [Authorize]
    public class HomePageSectionController : Controller
    {
        private readonly HomePageSectionService _service;

        public HomePageSectionController()
        {
            _service = new HomePageSectionService();
        }

        public ActionResult Index()
        {
            ViewBag.Title = "Home Page Sections";
            return View(_service.GetPage());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(HomePageSectionPageVM model)
        {
            if (model != null && model.Sections != null)
            {
                for (int i = 0; i < model.Sections.Count; i++)
                {
                    if (model.Sections[i].StartDate.HasValue &&
                        model.Sections[i].EndDate.HasValue &&
                        model.Sections[i].EndDate.Value < model.Sections[i].StartDate.Value)
                    {
                        ModelState.AddModelError("Sections[" + i + "].EndDate", "End date must be after start date.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Title = "Home Page Sections";
                return View("Index", model);
            }

            try
            {
                _service.SaveSections(model);
                TempData["SuccessMessage"] = "Home page sections updated successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Unable to update home page sections. " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}
