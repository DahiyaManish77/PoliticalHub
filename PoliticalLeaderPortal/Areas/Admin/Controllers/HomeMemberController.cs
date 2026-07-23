using PoliticalLeaderPortal.Areas.Admin.Services;
using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using System;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    public class HomeMemberController : Controller
    {
        private readonly HomeMemberService _service;

        public HomeMemberController()
        {
            _service = new HomeMemberService();
        }

        public ActionResult Index()
        {
            return View(_service.GetAll());
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View(new HomeMemberVM
            {
                DisplayOrder = 1,
                IsActive = true,
                ShowOnHome = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(HomeMemberVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _service.Create(model, Server);
                TempData["Success"] = "Home member created successfully.";
                return RedirectToAction("Index");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
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
        public ActionResult Edit(HomeMemberVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _service.Update(model, Server);
                TempData["Success"] = "Home member updated successfully.";
                return RedirectToAction("Index");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
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
