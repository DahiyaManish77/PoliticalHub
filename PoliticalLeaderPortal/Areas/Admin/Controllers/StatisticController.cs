using PoliticalLeaderPortal.Areas.Admin.Services;
using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    public class StatisticController : Controller
    {
        private readonly StatisticService _service;

    public StatisticController()
        {
            _service =
                new StatisticService();
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
                new StatisticVM
                {
                    DisplayOrder = 1,
                    IsActive = true
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            StatisticVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _service.Create(model);

            TempData["Success"] =
                "Statistic created successfully.";

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            StatisticVM model =
                _service.GetById(id);

            if (model == null)
            {
                return RedirectToAction("Index");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            StatisticVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _service.Update(model);

            TempData["Success"] =
                "Statistic updated successfully.";

            return RedirectToAction("Index");
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
