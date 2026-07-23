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
            string area = Convert.ToString(filterContext.RouteData.DataTokens["area"]);
            string controller = Convert.ToString(filterContext.RouteData.Values["controller"]);
            string action = Convert.ToString(filterContext.RouteData.Values["action"]);

            var permissionService = new RoleMenuPermissionService();

            if (permissionService.HasAccess(currentRoleId, roleName, area, controller, action))
            {
                return;
            }

            HandleUnauthorizedRequest(filterContext);
        }
    }
}
