using PoliticalLeaderPortal.Areas.Admin.Infrastructure;
using PoliticalLeaderPortal.Areas.Admin.Services;
using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using System;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    [RoleMenuAuthorize]
    public class TodayScheduleController : Controller
    {
        private readonly TodayScheduleService service;
        private readonly RoleMenuPermissionService permissionService;

        public TodayScheduleController()
        {
            service = new TodayScheduleService();
            permissionService = new RoleMenuPermissionService();
        }

        public ActionResult Index(string keyword = null, DateTime? date = null)
        {
            ViewBag.Keyword = keyword;
            ViewBag.Date = (date ?? DateTime.Today).ToString("yyyy-MM-dd");
            ViewBag.CanCreate = HasPermission("Create");
            ViewBag.CanEdit = HasPermission("Edit");
            ViewBag.CanDelete = HasPermission("Delete");

            return View(service.GetAll(keyword, date));
        }

        public ActionResult Create()
        {
            if (!HasPermission("Create")) return AccessDenied();

            return View("Form", new TodayScheduleVM
            {
                ScheduleDate = DateTime.Today,
                ScheduleTime = DateTime.Now.ToString("HH:mm"),
                Category = "Public Program",
                Priority = "Medium",
                Status = "Scheduled",
                IsActive = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TodayScheduleVM model)
        {
            if (!HasPermission("Create")) return AccessDenied();
            if (!ModelState.IsValid) return View("Form", model);

            service.Save(model, CurrentUserId());
            TempData["SuccessMessage"] = "Schedule saved successfully.";
            return RedirectToAction("Index", new { date = model.ScheduleDate.ToString("yyyy-MM-dd") });
        }

        public ActionResult Edit(int id)
        {
            if (!HasPermission("Edit")) return AccessDenied();

            var model = service.GetById(id);
            if (model == null) return HttpNotFound();

            return View("Form", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(TodayScheduleVM model)
        {
            if (!HasPermission("Edit")) return AccessDenied();
            if (!ModelState.IsValid) return View("Form", model);

            service.Update(model, CurrentUserId());
            TempData["SuccessMessage"] = "Schedule updated successfully.";
            return RedirectToAction("Index", new { date = model.ScheduleDate.ToString("yyyy-MM-dd") });
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            if (!HasPermission("Delete"))
            {
                return Json(new { success = false, message = "You are not authorised to delete schedule records." });
            }

            bool success = service.Delete(id, CurrentUserId());

            return Json(new
            {
                success = success,
                message = success ? "Schedule deleted successfully." : "Schedule record not found."
            });
        }

        private bool HasPermission(string permission)
        {
            return permissionService.HasActionPermission(
                CurrentRoleId(),
                Convert.ToString(Session["RoleName"]),
                "Admin",
                "TodaySchedule",
                "Index",
                permission);
        }

        private int? CurrentUserId()
        {
            int userId;
            return Session["UserId"] != null && Int32.TryParse(Session["UserId"].ToString(), out userId) ? (int?)userId : null;
        }

        private int? CurrentRoleId()
        {
            int roleId;
            return Session["RoleId"] != null && Int32.TryParse(Session["RoleId"].ToString(), out roleId) ? (int?)roleId : null;
        }

        private ActionResult AccessDenied()
        {
            return View("~/Areas/Admin/Views/Shared/AccessDenied.cshtml");
        }
    }
}
