using PoliticalLeaderPortal.Areas.Admin.Services;
using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    public class DownloadCategoryController : Controller
    {
        private readonly DownloadCategoryService _service;

        public DownloadCategoryController()
        {
            _service =
                new DownloadCategoryService();
        }

        public ActionResult Index()
        {
            return View(
                _service.GetAll());
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View(
                new DownloadCategoryVM
                {
                    DisplayOrder = 1,
                    IsActive = true
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            DownloadCategoryVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (_service.IsDuplicate(model.CategoryName))
            {
                ModelState.AddModelError(
                    "CategoryName",
                    "Category already exists.");

                return View(model);
            }

            _service.Insert(model);

            return RedirectToAction(
                "Index");
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            return View(
                _service.GetById(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            DownloadCategoryVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (_service.IsDuplicate(
                model.CategoryName,
                model.DownloadCategoryId))
            {
                ModelState.AddModelError(
                    "CategoryName",
                    "Category already exists.");

                return View(model);
            }

            _service.Update(model);

            return RedirectToAction(
                "Index");
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            bool result =
                _service.Delete(id);

            return Json(new
            {
                success = result,
                message = result
                    ? "Category deleted successfully."
                    : "Category cannot be deleted because it contains documents."
            });
        }
    }
}