using PoliticalLeaderPortal.Models;
using PoliticalLeaderPortal.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Controllers
{
    public class EventController : Controller
    {
        private readonly PoliticalLeaderPortalDbEntities1 _db;

        public EventController()
        {
            _db = new PoliticalLeaderPortalDbEntities1();
        }

        public ActionResult Index()
        {
            List<UpcomingEventListVM> model = _db.Database.SqlQuery<UpcomingEventListVM>(@"
                SELECT
                    e.EventId,
                    e.EventTitle AS Title,
                    COALESCE(NULLIF(p.SubTitle, ''), e.Description) AS ShortDescription,
                    e.EventDate,
                    CONVERT(NVARCHAR(20), e.StartTime) AS EventTime,
                    e.Venue AS EventLocation,
                    p.EventImagePath,
                    0 AS DisplayOrder,
                    CAST(1 AS bit) AS IsActive
                FROM dbo.EventMaster e
                INNER JOIN dbo.EventPublicProfile p ON p.EventId = e.EventId
                WHERE e.IsActive = 1
                  AND p.ShowOnHome = 1
                  AND p.IsConfidential = 0
                ORDER BY e.EventDate DESC, e.StartTime")
                .ToList();

            return View(model);
        }

        public ActionResult Details(int id)
        {
            UpcomingEventDetailsVM model = _db.Database.SqlQuery<UpcomingEventDetailsVM>(@"
                SELECT TOP 1
                    e.EventId,
                    e.EventTitle AS Title,
                    COALESCE(NULLIF(p.SubTitle, ''), e.Description) AS ShortDescription,
                    e.Description AS FullDescription,
                    e.EventDate,
                    CONVERT(NVARCHAR(20), e.StartTime) AS EventTime,
                    e.Venue AS EventLocation,
                    p.EventImagePath
                FROM dbo.EventMaster e
                INNER JOIN dbo.EventPublicProfile p ON p.EventId = e.EventId
                WHERE e.EventId = @p0
                  AND e.IsActive = 1
                  AND p.ShowOnHome = 1
                  AND p.IsConfidential = 0",
                id).FirstOrDefault();

            if (model == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }
    }
}
