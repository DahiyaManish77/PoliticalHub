using PoliticalLeaderPortal.Areas.Admin.Infrastructure;
using PoliticalLeaderPortal.Areas.Admin.Services;
using PoliticalLeaderPortal.Areas.Admin.ViewModels.People;
using System;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    [RoleMenuAuthorize]
    public class PeopleController : Controller
    {
        private readonly PeopleService _service = new PeopleService();

        public ActionResult Index(string keyword = null, string status = null, int? assemblyConstituencyId = null, bool volunteersOnly = false)
        {
            ViewBag.ModuleInstalled = _service.IsInstalled();
            return View(_service.GetIndex(keyword, status, assemblyConstituencyId, volunteersOnly));
        }

        public ActionResult Volunteers(string keyword = null, string status = null, int? assemblyConstituencyId = null)
        {
            return RedirectToAction("Index", new { keyword = keyword, status = status, assemblyConstituencyId = assemblyConstituencyId, volunteersOnly = true });
        }

        public ActionResult AddVolunteer()
        {
            return RedirectToAction("Create", new { volunteer = true });
        }

        public ActionResult Create(bool volunteer = false)
        {
            var model = new PersonEditVM { IsVolunteer = volunteer, ConsentSource = "Admin" };
            _service.LoadOptions(model); return View("Form", model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(PersonEditVM model)
        {
            if (!ModelState.IsValid) { _service.LoadOptions(model); return View("Form", model); }
            try { var id = _service.Save(model, CurrentUserId()); TempData["SuccessMessage"] = "Person record created successfully."; return RedirectToAction("Edit", new { id = id }); }
            catch (Exception ex) { ModelState.AddModelError("", ex.Message); _service.LoadOptions(model); return View("Form", model); }
        }

        public ActionResult Edit(int id)
        {
            var model = _service.Get(id); if (model == null) return HttpNotFound(); _service.LoadOptions(model); return View("Form", model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(PersonEditVM model)
        {
            if (!ModelState.IsValid) { _service.LoadOptions(model); return View("Form", model); }
            try { _service.Save(model, CurrentUserId()); TempData["SuccessMessage"] = "Person record updated successfully."; return RedirectToAction("Edit", new { id = model.PersonId }); }
            catch (Exception ex) { ModelState.AddModelError("", ex.Message); _service.LoadOptions(model); return View("Form", model); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult Delete(int id)
        { var ok = _service.Delete(id, CurrentUserId()); return Json(new { success = ok, message = ok ? "Person deactivated successfully." : "Record not found." }); }

        public JsonResult GeographyOptions(string type, int? parentId, string parentType = null)
        { return Json(_service.GetGeography(type, parentId, parentType), JsonRequestBehavior.AllowGet); }

        private int CurrentUserId()
        { int id; return Int32.TryParse(Convert.ToString(Session["UserId"]), out id) ? id : 0; }
    }
}
