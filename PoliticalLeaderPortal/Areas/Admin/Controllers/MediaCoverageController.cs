using PoliticalLeaderPortal.Areas.Admin.Services;
using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using System;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    [Authorize]
    public class MediaCoverageController : Controller
    {
        private readonly MediaCoverageService _service;

        public MediaCoverageController()
        {
            _service = new MediaCoverageService();
        }

        public ActionResult Index()
        {
            return View(_service.GetAll());
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View(
                new MediaCoverageVM
                {
                    IsActive = true,
                    IsFeatured = false,
                    DisplayOrder = 1,
                    CoverageDate = DateTime.Now
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(MediaCoverageVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _service.Create(model);

            TempData["Success"] =
                "Media Coverage created successfully.";

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var model = _service.GetById(id);

            if (model == null)
            {
                return RedirectToAction("Index");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(MediaCoverageVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _service.Update(model);

            TempData["Success"] =
                "Media Coverage updated successfully.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Delete(int id)
        {
            try
            {
                _service.Delete(id);

                return Json(new
                {
                    success = true,
                    message = "Record deleted successfully."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}
