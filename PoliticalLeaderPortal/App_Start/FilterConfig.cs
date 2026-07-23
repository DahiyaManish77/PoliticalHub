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
        }
    }
}