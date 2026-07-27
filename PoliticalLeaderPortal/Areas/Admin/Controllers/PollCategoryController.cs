using PoliticalLeaderPortal.Areas.Admin.Services.Poll;
using PoliticalLeaderPortal.Areas.Admin.ViewModels.Poll;
using System;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    [Authorize]
    public class PollCategoryController : Controller
    {
        private readonly PollService _service;

        public PollCategoryController()
        {
            _service = new PollService();
        }

        public ActionResult Index()
        {
            return View(_service.GetCategories());
        }

        public ActionResult Create()
        {
            return View(new PollCategoryVM { IsActive = true, DisplayOrder = 1 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PollCategoryVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (_service.IsDuplicateCategory(model.CategoryName))
            {
                ModelState.AddModelError("CategoryName", "Category already exists.");
                return View(model);
            }

            _service.SaveCategory(model, CurrentUserId());
            TempData["Success"] = "Poll category created successfully.";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var model = _service.GetCategoryById(id);
            return model == null ? (ActionResult)HttpNotFound() : View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PollCategoryVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (_service.IsDuplicateCategory(model.CategoryName, model.PollCategoryId))
            {
                ModelState.AddModelError("CategoryName", "Category already exists.");
                return View(model);
            }

            _service.SaveCategory(model, CurrentUserId());
            TempData["Success"] = "Poll category updated successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            bool result = _service.DeleteCategory(id, CurrentUserId());
            return Json(new
            {
                success = result,
                message = result
                    ? "Poll category deleted successfully."
                    : "Category cannot be deleted because it contains polls."
            });
        }

        private int? CurrentUserId()
        {
            int userId;
            return Session["UserId"] != null && Int32.TryParse(Session["UserId"].ToString(), out userId)
                ? (int?)userId
                : null;
        }
    }
}
