using PoliticalLeaderPortal.Models;
using System;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Linq;

namespace PoliticalLeaderPortal.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home";
            ViewBag.RenderHomePagePartials = false;
            ViewBag.HomeHitCount = RecordHomeVisit();

            return View();
        }

        [ChildActionOnly]
        public ActionResult HomeVisitMenu()
        {
            ViewBag.HomeVisitTotal = GetHomeVisitCount();
            return PartialView("~/Views/Shared/_HomeVisitMenu.cshtml");
        }

        private int GetHomeVisitCount()
        {
            try
            {
                using (var db = new PoliticalLeaderPortalDbEntities1())
                {
                    EnsureHomeHitCounter(db);
                    var result = db.Database.SqlQuery<int>("SELECT HitCount FROM dbo.SiteHitCounter WHERE CounterKey = @Key", new SqlParameter("@Key", "Home")).ToList();
                    return result.Count == 0 ? 0 : result[0];
                }
            }
            catch
            {
                return 0;
            }
        }

        private void EnsureHomeHitCounter(PoliticalLeaderPortalDbEntities1 db)
        {
            db.Database.ExecuteSqlCommand(@"
IF OBJECT_ID('dbo.SiteHitCounter', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SiteHitCounter
    (
        CounterKey NVARCHAR(80) NOT NULL PRIMARY KEY,
        HitCount INT NOT NULL DEFAULT(0),
        LastHitOn DATETIME NULL
    );
END;
IF NOT EXISTS (SELECT 1 FROM dbo.SiteHitCounter WHERE CounterKey = 'Home')
BEGIN
    INSERT INTO dbo.SiteHitCounter (CounterKey, HitCount, LastHitOn) VALUES ('Home', 0, GETDATE());
END;");
        }
        private int RecordHomeVisit()
        {
            try
            {
                using (var db = new PoliticalLeaderPortalDbEntities1())
                {
                    EnsureHomeHitCounter(db);


                    db.Database.ExecuteSqlCommand(@"
UPDATE dbo.SiteHitCounter
SET HitCount = HitCount + 1,
    LastHitOn = GETDATE()
WHERE CounterKey = 'Home';");

                    var result = db.Database.SqlQuery<int>("SELECT HitCount FROM dbo.SiteHitCounter WHERE CounterKey = @Key", new SqlParameter("@Key", "Home")).ToList();
                    return result.Count == 0 ? 0 : result[0];
                }
            }
            catch
            {
                return 0;
            }
        }
    }
}



