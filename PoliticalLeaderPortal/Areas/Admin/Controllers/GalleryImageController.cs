using PoliticalLeaderPortal.Services;
using PoliticalLeaderPortal.ViewModels;
using System;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    [Authorize]
    public class GalleryImageController : Controller
    {
        private readonly GalleryService _galleryService;

        public GalleryImageController()
        {
            _galleryService = new GalleryService();
        }

        public ActionResult Index()
        {
            var model = _galleryService.GetAllImages();
            return View(model);
        }

        public ActionResult Details(int id)
        {
            var model = _galleryService.GetImageById(id);

            if (model == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        public ActionResult Create()
        {
            var model = new GalleryImageVM
            {
                IsActive = true,
                DisplayOrder = 0,
                Categories = _galleryService.GetCategoryDropdown()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(GalleryImageVM model)
        {
            model.Categories = _galleryService.GetCategoryDropdown();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _galleryService.CreateImages(model);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }

            TempData["SuccessMessage"] =
                "Gallery images uploaded successfully.";

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var model = _galleryService.GetImageById(id);

            if (model == null)
            {
                return HttpNotFound();
            }

            model.Categories = _galleryService.GetCategoryDropdown();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(GalleryImageVM model)
        {
            model.Categories = _galleryService.GetCategoryDropdown();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _galleryService.UpdateImage(model);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }

            TempData["SuccessMessage"] =
                "Gallery image updated successfully.";

            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            var model = _galleryService.GetImageById(id);

            if (model == null)
            {
                return HttpNotFound();
            }

            model.Categories = _galleryService.GetCategoryDropdown();

            return View(model);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            _galleryService.DeleteImage(id);

            TempData["SuccessMessage"] =
                "Gallery image deleted successfully.";

            return RedirectToAction("Index");
        }
    }
}
