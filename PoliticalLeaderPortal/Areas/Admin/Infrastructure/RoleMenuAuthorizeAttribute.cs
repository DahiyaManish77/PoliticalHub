using PoliticalLeaderPortal.Areas.Admin.Services;
using System;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Infrastructure
{
    public class RoleMenuAuthorizeAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                base.HandleUnauthorizedRequest(filterContext);
                return;
            }

            filterContext.Result = new ViewResult
            {
                ViewName = "~/Areas/Admin/Views/Shared/AccessDenied.cshtml"
            };
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            string area = Convert.ToString(filterContext.RouteData.DataTokens["area"]);
            if (!String.Equals(area, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (filterContext.IsChildAction)
            {
                return;
            }

            base.OnAuthorization(filterContext);

            if (filterContext.Result != null)
            {
                return;
            }

            var session = filterContext.HttpContext.Session;
            int roleId;
            int? currentRoleId = null;

            if (session["RoleId"] != null && Int32.TryParse(session["RoleId"].ToString(), out roleId))
            {
                currentRoleId = roleId;
            }

            string roleName = Convert.ToString(session["RoleName"]);
            string controller = Convert.ToString(filterContext.RouteData.Values["controller"]);
            string action = Convert.ToString(filterContext.RouteData.Values["action"]);

            var permissionService = new RoleMenuPermissionService();

            if (permissionService.HasActionPermission(
                currentRoleId,
                roleName,
                area,
                controller,
                action,
                InferPermission(action)))
            {
                return;
            }

            HandleUnauthorizedRequest(filterContext);
        }

        private static string InferPermission(string action)
        {
            string normalized = action ?? String.Empty;

            if (normalized.IndexOf("Delete", StringComparison.OrdinalIgnoreCase) >= 0 ||
                normalized.IndexOf("Remove", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "CanDelete";
            }

            if (normalized.StartsWith("Create", StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith("Add", StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith("Save", StringComparison.OrdinalIgnoreCase))
            {
                return "CanCreate";
            }

            if (normalized.StartsWith("Edit", StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith("Update", StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith("Toggle", StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith("Publish", StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith("Close", StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith("Archive", StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith("Import", StringComparison.OrdinalIgnoreCase))
            {
                return "CanEdit";
            }

            return "CanView";
        }
    }
}
