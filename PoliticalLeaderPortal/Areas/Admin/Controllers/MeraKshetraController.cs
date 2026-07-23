using PoliticalLeaderPortal.Services;
using PoliticalLeaderPortal.ViewModels;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    public class MeraKshetraController : Controller
    {
        private readonly MeraKshetraService _service;

        public MeraKshetraController()
        {
            _service = new MeraKshetraService();
        }

        public ActionResult Index()
        {
            return View(_service.GetAll());
        }

        [HttpGet]
        public ActionResult Create()
        {
            LoadDropdowns();

            return View(new MeraKshetraItemVM
            {
                ModuleType = "Nearby Place",
                IconClass = "bi bi-geo-alt",
                DisplayOrder = 1,
                IsActive = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MeraKshetraItemVM model)
        {
            LoadDropdowns();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _service.Create(model, Server);
                TempData["Success"] = "Mera Kshetra record created successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            LoadDropdowns();

            var model = _service.GetById(id);
            if (model == null)
            {
                TempData["Error"] = "Mera Kshetra record not found.";
                return RedirectToAction("Index");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MeraKshetraItemVM model)
        {
            LoadDropdowns();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _service.Update(model, Server);
                TempData["Success"] = "Mera Kshetra record updated successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
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
                success = true,
                message = "Mera Kshetra record deleted successfully."
            });
        }

        private void LoadDropdowns()
        {
            ViewBag.ModuleTypes = new List<SelectListItem>
            {
                new SelectListItem { Text = "Heritage", Value = "Heritage" },
                new SelectListItem { Text = "Nearby Place", Value = "Nearby Place" },
                new SelectListItem { Text = "Development", Value = "Development" },
                new SelectListItem { Text = "Citizen Service", Value = "Citizen Service" },
                new SelectListItem { Text = "Education", Value = "Education" },
                new SelectListItem { Text = "Health", Value = "Health" },
                new SelectListItem { Text = "Transport", Value = "Transport" },
                new SelectListItem { Text = "Tourism", Value = "Tourism" }
            };
        }
    }
}
