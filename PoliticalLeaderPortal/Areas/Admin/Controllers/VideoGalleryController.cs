using PoliticalLeaderPortal.Services;
using PoliticalLeaderPortal.ViewModels;
using System;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    [Authorize]
    public class VideoGalleryController : Controller
    {
        private readonly VideoGalleryService _videoService;

        public VideoGalleryController()
        {
            _videoService = new VideoGalleryService();
        }

        public ActionResult Index()
        {
            var model = _videoService.GetAllVideos();

            return View(model);
        }

        public ActionResult Details(int id)
        {
            var model = _videoService.GetVideoById(id);

            if (model == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        public ActionResult Create()
        {
            var model = new VideoGalleryVM
            {
                IsActive = true,
                IsFeatured = false,
                DisplayOrder = 0,
                Categories = _videoService.GetCategoryDropdown()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create(VideoGalleryVM model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories =
                    _videoService.GetCategoryDropdown();

                return View(model);
            }

            try
            {
                _videoService.CreateVideo(model);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                model.Categories =
                    _videoService.GetCategoryDropdown();

                return View(model);
            }

            TempData["SuccessMessage"] =
                "Video created successfully.";

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var model = _videoService.GetVideoById(id);

            if (model == null)
            {
                return HttpNotFound();
            }

            model.Categories =
                _videoService.GetCategoryDropdown();

            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(VideoGalleryVM model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories =
                    _videoService.GetCategoryDropdown();

                return View(model);
            }

            try
            {
                _videoService.UpdateVideo(model);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                model.Categories =
                    _videoService.GetCategoryDropdown();

                return View(model);
            }

            TempData["SuccessMessage"] =
                "Video updated successfully.";

            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            var model = _videoService.GetVideoById(id);

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
            _videoService.DeleteVideo(id);

            TempData["SuccessMessage"] =
                "Video deleted successfully.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SyncYouTube()
        {
            try
            {
                int imported =
                    _videoService.SyncYouTubeVideos();

                TempData["SuccessMessage"] =
                    imported + " YouTube videos imported successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}
