using System;
using System.Net;
using System.Web.Helpers;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Infrastructure.Security
{
    /// <summary>
    /// Enforces anti-forgery validation for every state-changing request in
    /// the Admin area, including legacy AJAX endpoints.
    /// </summary>
    public sealed class AdminAntiForgeryAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            if (!IsAdminRequest(filterContext) ||
                !IsStateChangingMethod(filterContext.HttpContext.Request.HttpMethod))
            {
                return;
            }

            try
            {
                AntiForgery.Validate();
            }
            catch (HttpAntiForgeryException)
            {
                filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;

                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new JsonResult
                    {
                        Data = new
                        {
                            success = false,
                            message = "The security token is missing or expired. Refresh the page and try again."
                        },
                        JsonRequestBehavior = JsonRequestBehavior.DenyGet
                    };
                    return;
                }

                filterContext.Result = new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest,
                    "The security token is missing or expired. Refresh the page and try again.");
            }
        }

        private static bool IsAdminRequest(ActionExecutingContext context)
        {
            string area = Convert.ToString(context.RouteData.DataTokens["area"]);
            return String.Equals(area, "Admin", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsStateChangingMethod(string method)
        {
            return String.Equals(method, "POST", StringComparison.OrdinalIgnoreCase) ||
                   String.Equals(method, "PUT", StringComparison.OrdinalIgnoreCase) ||
                   String.Equals(method, "PATCH", StringComparison.OrdinalIgnoreCase) ||
                   String.Equals(method, "DELETE", StringComparison.OrdinalIgnoreCase);
        }
    }
}
