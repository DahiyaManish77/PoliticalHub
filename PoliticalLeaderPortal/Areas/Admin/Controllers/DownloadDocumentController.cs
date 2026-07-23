using PoliticalLeaderPortal.Areas.Admin.Services;
using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using System;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    [Authorize]
    public class DownloadDocumentController : Controller
    {
        private readonly DownloadDocumentService _service;
        private readonly DownloadCategoryService _categoryService;

        public DownloadDocumentController()
        {
            _service =
                new DownloadDocumentService();

            _categoryService =
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
            DownloadDocumentVM model =
                new DownloadDocumentVM();

            model.PublishDate =
                DateTime.Now;

            model.DisplayOrder =
                1;

            model.IsActive =
                true;

            model.Categories =
                _categoryService.GetCategoryDropdown();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            DownloadDocumentVM model)
        {
            model.Categories =
                _categoryService.GetCategoryDropdown();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (_service.IsDuplicateDocumentNumber(
                model.DocumentNumber))
            {
                ModelState.AddModelError(
                    "DocumentNumber",
                    "Document Number already exists.");

                return View(model);
            }

            try
            {
                _service.Create(
                    model,
                    Server);

                return RedirectToAction(
                    "Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    "",
                    ex.Message);

                return View(model);
            }
        }
        [HttpGet]
        public ActionResult Edit(int id)
        {
            DownloadDocumentVM model =
                _service.GetById(id);

            if (model == null)
            {
                return HttpNotFound();
            }

            model.Categories =
                _categoryService.GetCategoryDropdown();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            DownloadDocumentVM model)
        {
            model.Categories =
                _categoryService.GetCategoryDropdown();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (_service.IsDuplicateDocumentNumber(
                model.DocumentNumber,
                model.DownloadDocumentId))
            {
                ModelState.AddModelError(
                    "DocumentNumber",
                    "Document Number already exists.");

                return View(model);
            }

            try
            {
                _service.Update(
                    model,
                    Server);

                return RedirectToAction(
                    "Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    "",
                    ex.Message);

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Delete(int id)
        {
            try
            {
                bool result =
                    _service.Delete(
                        id,
                        Server);

                if (result)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Document deleted successfully."
                    });
                }

                return Json(new
                {
                    success = false,
                    message = "Document not found."
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
