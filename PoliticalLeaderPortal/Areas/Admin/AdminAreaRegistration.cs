using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get { return "Admin"; }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            var route = context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{id}",
                new
                {
                    action = "Index",
                    id = UrlParameter.Optional
                },
                namespaces: new[] { "PoliticalLeaderPortal.Areas.Admin.Controllers" }
            );

            route.DataTokens["UseNamespaceFallback"] = false;
        }
    }
}
