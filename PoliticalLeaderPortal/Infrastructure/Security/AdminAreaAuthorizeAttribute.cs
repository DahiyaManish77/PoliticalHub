using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace PoliticalLeaderPortal.Infrastructure.Security
{
    /// <summary>
    /// Protects every controller in the Admin area from anonymous access.
    /// Existing action-level role and menu permissions remain unchanged.
    /// </summary>
    public sealed class AdminAreaAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            if (!IsAdminRequest(filterContext.RouteData))
            {
                return;
            }

            base.OnAuthorization(filterContext);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.Result = new JsonResult
                {
                    Data = new
                    {
                        success = false,
                        message = "Your session has expired. Please sign in again.",
                        redirectUrl = BuildLoginUrl(filterContext.HttpContext)
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                filterContext.HttpContext.Response.StatusCode = 401;
                return;
            }

            filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary(new
                {
                    area = "",
                    controller = "Account",
                    action = "Login",
                    returnUrl = filterContext.HttpContext.Request.RawUrl
                }));
        }

        private static bool IsAdminRequest(RouteData routeData)
        {
            string area = Convert.ToString(routeData.DataTokens["area"]);
            return String.Equals(area, "Admin", StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildLoginUrl(HttpContextBase context)
        {
            return new UrlHelper(context.Request.RequestContext)
                .Action("Login", "Account", new
                {
                    area = "",
                    returnUrl = context.Request.RawUrl
                });
        }
    }
}
