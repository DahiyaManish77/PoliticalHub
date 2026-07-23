using PoliticalLeaderPortal.Services;
using PoliticalLeaderPortal.ViewModels;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    public class GalleryCategoryController : Controller
    {
        private readonly GalleryService _galleryService;

        public GalleryCategoryController()
        {
            _galleryService = new GalleryService();
        }

        public ActionResult Index()
        {
            var model = _galleryService.GetAllCategories();
            return View(model);
        }

        public ActionResult Details(int id)
        {
            var model = _galleryService.GetCategoryById(id);

            if (model == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        public ActionResult Create()
        {
            var model = new GalleryCategoryVM
            {
                IsActive = true,
                DisplayOrder = 0
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(GalleryCategoryVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _galleryService.CreateCategory(model);

            TempData["SuccessMessage"] = "Gallery category created successfully.";

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var model = _galleryService.GetCategoryById(id);

            if (model == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(GalleryCategoryVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _galleryService.UpdateCategory(model);

            TempData["SuccessMessage"] = "Gallery category updated successfully.";

            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            var model = _galleryService.GetCategoryById(id);

            if (model == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            _galleryService.DeleteCategory(id);

            TempData["SuccessMessage"] = "Gallery category deleted successfully.";

            return RedirectToAction("Index");
        }
    }
}