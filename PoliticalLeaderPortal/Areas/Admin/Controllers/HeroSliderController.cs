using PoliticalLeaderPortal.Areas.Admin.Services;
using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using System.Collections.Generic;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    public class HeroSliderController : Controller
    {
        private readonly HeroSliderService _service;

        public HeroSliderController()
        {
            _service = new HeroSliderService();
        }

        #region Private Methods

        private void LoadDropdowns()
        {
            ViewBag.TemplateTypes = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = "Leader Banner",
                    Value = "Leader Banner"
                },
                new SelectListItem
                {
                    Text = "Campaign Banner",
                    Value = "Campaign Banner"
                },
                new SelectListItem
                {
                    Text = "Announcement Banner",
                    Value = "Announcement Banner"
                },
                new SelectListItem
                {
                    Text = "Full Background Banner",
                    Value = "Full Background Banner"
                },
                new SelectListItem
                {
                    Text = "Video Banner",
                    Value = "Video Banner"
                }
            };

            ViewBag.LeaderPositions = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = "Left",
                    Value = "Left"
                },
                new SelectListItem
                {
                    Text = "Center",
                    Value = "Center"
                },
                new SelectListItem
                {
                    Text = "Right",
                    Value = "Right"
                }
            };
        }

        #endregion

        #region Index

        public ActionResult Index()
        {
            return View(_service.GetAll());
        }

        #endregion

        #region Create

        [HttpGet]
        public ActionResult Create()
        {
            LoadDropdowns();

            HeroSliderVM model = new HeroSliderVM
            {
                IsActive = true,

                DisplayOrder = 1,

                ShowButtons = true,

                ShowOverlay = true,

                ShowLeaderImage = true,

                TemplateType = "Leader Banner",

                LeaderImagePosition = "Right",

                BackgroundColor = "#0B1F3A",

                HeroHeightCss = "440px",

                OverlayOpacity = 0.60M,

                VideoSourceType = "Image",

                VideoAutoplay = true,

                VideoMuted = true,

                VideoLoop = true
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(HeroSliderVM model)
        {
            LoadDropdowns();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _service.Create(model, Server);
            }
            catch (System.InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }

            TempData["Success"] = "Hero Slider created successfully.";

            return RedirectToAction("Index");
        }

        #endregion

        #region Edit

        [HttpGet]
        public ActionResult Edit(int id)
        {
            LoadDropdowns();

            HeroSliderVM model = _service.GetById(id);

            if (model == null)
            {
                TempData["Error"] = "Hero Slider not found.";

                return RedirectToAction("Index");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(HeroSliderVM model)
        {
            LoadDropdowns();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _service.Update(model, Server);
            }
            catch (System.InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }

            TempData["Success"] = "Hero Slider updated successfully.";

            return RedirectToAction("Index");
        }

        #endregion

        #region Delete

        [HttpPost]
        public JsonResult Delete(int id)
        {
            _service.Delete(id, Server);

            return Json(new
            {
                success = true,
                message = "Hero Slider deleted successfully."
            });
        }

        #endregion
    }
}
