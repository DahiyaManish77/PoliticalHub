using System.Web.Mvc;
using System.Web.Routing;

namespace PoliticalLeaderPortal
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "NewsDetails",
                url: "news/{id}",
                defaults: new
                {
                    controller = "News",
                    action = "Details"
                },
                constraints: new
                {
                    id = @"\d+"
                }
            );

            routes.MapRoute(
                name: "NewsIndex",
                url: "news",
                defaults: new
                {
                    controller = "News",
                    action = "Index"
                }
            );

            routes.MapRoute(
                name: "EventDetails",
                url: "events/{id}",
                defaults: new
                {
                    controller = "Event",
                    action = "Details"
                },
                constraints: new
                {
                    id = @"\d+"
                }
            );

            routes.MapRoute(
                name: "Events",
                url: "events",
                defaults: new
                {
                    controller = "Event",
                    action = "Index"
                }
            );

            routes.MapRoute(
                name: "GalleryAlbum",
                url: "gallery/album/{id}",
                defaults: new
                {
                    controller = "Gallery",
                    action = "Album"
                },
                constraints: new
                {
                    id = @"\d+"
                }
            );

            routes.MapRoute(
                name: "Gallery",
                url: "gallery",
                defaults: new
                {
                    controller = "Gallery",
                    action = "Index"
                }
            );

            routes.MapRoute(
                name: "VideoDetails",
                url: "videos/{id}",
                defaults: new
                {
                    controller = "Video",
                    action = "Details"
                },
                constraints: new
                {
                    id = @"\d+"
                }
            );

            routes.MapRoute(
                name: "Videos",
                url: "videos",
                defaults: new
                {
                    controller = "Video",
                    action = "Index"
                }
            );

            routes.MapRoute(
                name: "DownloadFile",
                url: "downloads/file/{id}",
                defaults: new
                {
                    controller = "Downloads",
                    action = "Download"
                },
                constraints: new
                {
                    id = @"\d+"
                }
            );

            routes.MapRoute(
                name: "DownloadsByCategory",
                url: "downloads/category/{id}",
                defaults: new
                {
                    controller = "Downloads",
                    action = "Category"
                },
                constraints: new
                {
                    id = @"\d+"
                }
            );

            routes.MapRoute(
                name: "Downloads",
                url: "downloads",
                defaults: new
                {
                    controller = "Downloads",
                    action = "Index"
                }
            );

            routes.MapRoute(
                name: "MediaCoverageDetails",
                url: "media-coverage/{id}",
                defaults: new
                {
                    controller = "PublicMediaCoverage",
                    action = "Details"
                },
                constraints: new
                {
                    id = @"\d+"
                }
            );

            routes.MapRoute(
                name: "MediaCoverage",
                url: "media-coverage",
                defaults: new
                {
                    controller = "PublicMediaCoverage",
                    action = "Index"
                }
            );

            routes.MapRoute(
                name: "AboutLeaderDefault",
                url: "about-leader",
                defaults: new
                {
                    controller = "AboutLeader",
                    action = "Biography"
                }
            );

            routes.MapRoute(
                name: "AboutLeaderAction",
                url: "about-leader/{action}",
                defaults: new
                {
                    controller = "AboutLeader"
                }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new
                {
                    controller = "Home",
                    action = "Index",
                    id = UrlParameter.Optional
                },
                namespaces: new[] { "PoliticalLeaderPortal.Controllers" }
            );
        }
    }
}
