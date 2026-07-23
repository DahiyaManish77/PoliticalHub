using PoliticalLeaderPortal.Services;
using PoliticalLeaderPortal.ViewModels;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    public class VideoCategoryController : Controller
    {
        private readonly VideoGalleryService _videoService;

        public VideoCategoryController()
        {
            _videoService = new VideoGalleryService();
        }

        public ActionResult Index()
        {
            var model = _videoService.GetAllCategories();

            return View(model);
        }

        public ActionResult Details(int id)
        {
            var model = _videoService.GetCategoryById(id);

            if (model == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        public ActionResult Create()
        {
            var model = new VideoCategoryVM
            {
                IsActive = true,
                DisplayOrder = 0
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(VideoCategoryVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _videoService.CreateCategory(model);

            TempData["SuccessMessage"] =
                "Video category created successfully.";

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var model = _videoService.GetCategoryById(id);

            if (model == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(VideoCategoryVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _videoService.UpdateCategory(model);

            TempData["SuccessMessage"] =
                "Video category updated successfully.";

            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            var model = _videoService.GetCategoryById(id);

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
            _videoService.DeleteCategory(id);

            TempData["SuccessMessage"] =
                "Video category deleted successfully.";

            return RedirectToAction("Index");
        }
    }
}