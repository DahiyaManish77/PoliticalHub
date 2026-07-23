using PoliticalLeaderPortal.Areas.Admin.Services;
using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    [Authorize]
    public class RoleMenuPermissionController : Controller
    {
        private readonly RoleMenuPermissionService permissionService;

        public RoleMenuPermissionController()
        {
            permissionService = new RoleMenuPermissionService();
        }

        public ActionResult Index(int? roleId)
        {
            if (!RoleMenuPermissionService.IsFullAccessRole(Convert.ToString(Session["RoleName"])))
            {
                return View("~/Areas/Admin/Views/Shared/AccessDenied.cshtml");
            }

            return View(permissionService.BuildPage(roleId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(int roleId, List<RoleMenuPermissionItemVM> menus)
        {
            if (!RoleMenuPermissionService.IsFullAccessRole(Convert.ToString(Session["RoleName"])))
            {
                return View("~/Areas/Admin/Views/Shared/AccessDenied.cshtml");
            }

            int userId;
            int? currentUserId = null;

            if (Session["UserId"] != null && Int32.TryParse(Session["UserId"].ToString(), out userId))
            {
                currentUserId = userId;
            }

            permissionService.Save(roleId, menus ?? new List<RoleMenuPermissionItemVM>(), currentUserId);

            TempData["SuccessMessage"] = "Role permissions updated successfully.";

            return RedirectToAction("Index", new { roleId = roleId });
        }
    }
}
