using PoliticalLeaderPortal.Infrastructure.Security;
using PoliticalLeaderPortal.Areas.Admin.Infrastructure;
using System.Web.Mvc;

namespace PoliticalLeaderPortal
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(
            GlobalFilterCollection filters)
        {
            filters.Add(
                new HandleErrorAttribute());
            filters.Add(
                new AdminAreaAuthorizeAttribute());
            filters.Add(
                new AdminAntiForgeryAttribute());
            filters.Add(
                new RoleMenuAuthorizeAttribute());
        }
    }
}
