using PoliticalLeaderPortal.Areas.Admin.Services;
using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using System;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    [Authorize]
    public class LatestNewsController : Controller
    {
        private readonly LatestNewsService _service;


    public LatestNewsController()
        {
            _service =
                new LatestNewsService();
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
                new LatestNewsVM
                {
                    PublishDate = DateTime.Now,
                    DisplayOrder = 1,
                    IsActive = true
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            LatestNewsVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _service.Create(
                model,
                Server);

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
            LatestNewsVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _service.Update(
                model,
                Server);

            return RedirectToAction(
                "Index");
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            _service.Delete(id);

            return Json(new
            {
                success = true
            });
        }
    }


}
