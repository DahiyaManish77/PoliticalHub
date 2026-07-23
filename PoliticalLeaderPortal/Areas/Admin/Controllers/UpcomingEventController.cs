using PoliticalLeaderPortal.Services;
using PoliticalLeaderPortal.ViewModels;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    
    public class UpcomingEventController : Controller
    {
        private readonly UpcomingEventService _eventService;

        public UpcomingEventController()
        {
            _eventService = new UpcomingEventService();
        }

        public ActionResult Index()
        {
            var model = _eventService.GetAll();
            return View(model);
        }

        public ActionResult Details(int id)
        {
            var model = _eventService.GetById(id);

            if (model == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        public ActionResult Create()
        {
            var model = new UpcomingEventVM
            {
                IsActive = true,
                DisplayOrder = 0
            };

            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UpcomingEventVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string adminName = User.Identity.Name;

            _eventService.Create(model, adminName);

            TempData["SuccessMessage"] = "Event created successfully.";

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var model = _eventService.GetById(id);

            if (model == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UpcomingEventVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string adminName = User.Identity.Name;

            _eventService.Update(model, adminName);

            TempData["SuccessMessage"] = "Event updated successfully.";

            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            var model = _eventService.GetById(id);

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
            _eventService.Delete(id);

            TempData["SuccessMessage"] = "Event deleted successfully.";

            return RedirectToAction("Index");
        }
    }
}