using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using PoliticalLeaderPortal.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    [Authorize]
    public class PublicNewsTickerController : Controller
    {
        public ActionResult Index(string keyword = null, string status = null)
        {
            EnsureTable();
            EnsureAdminSidebarMenu();
            ViewBag.Keyword = keyword;
            ViewBag.Status = status;
            return View(GetAll(keyword, status));
        }

        public ActionResult Create()
        {
            EnsureTable();
            return View("Form", new PublicNewsTickerVM
            {
                Category = "Breaking News",
                DisplayOrder = GetNextDisplayOrder(),
                IsActive = true,
                StartDate = DateTime.Today
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PublicNewsTickerVM model)
        {
            EnsureTable();
            if (!ModelState.IsValid) return View("Form", model);

            using (var db = new PoliticalLeaderPortalDbEntities1())
            {
                db.Database.ExecuteSqlCommand(@"
INSERT INTO dbo.PublicNewsTicker
(TickerText, LinkUrl, Category, DisplayOrder, IsActive, StartDate, EndDate, CreatedDate, UpdatedDate)
VALUES
(@TickerText, @LinkUrl, @Category, @DisplayOrder, @IsActive, @StartDate, @EndDate, GETDATE(), NULL);",
                    Parameter("@TickerText", model.TickerText),
                    Parameter("@LinkUrl", model.LinkUrl),
                    Parameter("@Category", model.Category),
                    new SqlParameter("@DisplayOrder", model.DisplayOrder),
                    new SqlParameter("@IsActive", model.IsActive),
                    Parameter("@StartDate", model.StartDate),
                    Parameter("@EndDate", model.EndDate));
            }

            TempData["SuccessMessage"] = "News ticker item created successfully.";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            EnsureTable();
            var model = GetById(id);
            if (model == null) return HttpNotFound();
            return View("Form", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PublicNewsTickerVM model)
        {
            EnsureTable();
            if (!ModelState.IsValid) return View("Form", model);

            using (var db = new PoliticalLeaderPortalDbEntities1())
            {
                db.Database.ExecuteSqlCommand(@"
UPDATE dbo.PublicNewsTicker
SET TickerText = @TickerText,
    LinkUrl = @LinkUrl,
    Category = @Category,
    DisplayOrder = @DisplayOrder,
    IsActive = @IsActive,
    StartDate = @StartDate,
    EndDate = @EndDate,
    UpdatedDate = GETDATE()
WHERE PublicNewsTickerId = @PublicNewsTickerId;",
                    Parameter("@TickerText", model.TickerText),
                    Parameter("@LinkUrl", model.LinkUrl),
                    Parameter("@Category", model.Category),
                    new SqlParameter("@DisplayOrder", model.DisplayOrder),
                    new SqlParameter("@IsActive", model.IsActive),
                    Parameter("@StartDate", model.StartDate),
                    Parameter("@EndDate", model.EndDate),
                    new SqlParameter("@PublicNewsTickerId", model.PublicNewsTickerId));
            }

            TempData["SuccessMessage"] = "News ticker item updated successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            EnsureTable();
            using (var db = new PoliticalLeaderPortalDbEntities1())
            {
                int affected = db.Database.ExecuteSqlCommand("DELETE FROM dbo.PublicNewsTicker WHERE PublicNewsTickerId = @Id", new SqlParameter("@Id", id));
                return Json(new { success = affected > 0, message = affected > 0 ? "Ticker item deleted successfully." : "Ticker item not found." });
            }
        }

        [HttpPost]
        public JsonResult ToggleActive(int id)
        {
            EnsureTable();
            using (var db = new PoliticalLeaderPortalDbEntities1())
            {
                int affected = db.Database.ExecuteSqlCommand(@"
UPDATE dbo.PublicNewsTicker
SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END,
    UpdatedDate = GETDATE()
WHERE PublicNewsTickerId = @Id", new SqlParameter("@Id", id));

                return Json(new { success = affected > 0, message = affected > 0 ? "Ticker status updated." : "Ticker item not found." });
            }
        }


        private void EnsureAdminSidebarMenu()
        {
            try
            {
                using (var db = new PoliticalLeaderPortalDbEntities1())
                {
                    db.Database.ExecuteSqlCommand(@"
IF OBJECT_ID('dbo.MenuMaster', 'U') IS NOT NULL
AND NOT EXISTS (
    SELECT 1 FROM dbo.MenuMaster
    WHERE AreaName = 'Admin'
      AND ControllerName = 'PublicNewsTicker'
      AND ActionName = 'Index'
)
BEGIN
    INSERT INTO dbo.MenuMaster
    (MenuName, AreaName, ControllerName, ActionName, CustomUrl, IconClass, ParentMenuId, DisplayOrder, IsActive, ShowOnHome, ShowInAdminSidebar, OpenInNewTab, IsClickable, MenuDescription, CssClass, RouteValues)
    VALUES
    ('Public News Ticker', 'Admin', 'PublicNewsTicker', 'Index', NULL, 'bi bi-megaphone', NULL, 72, 1, 0, 1, 0, 1, 'Manage top public breaking news marquee.', NULL, NULL);
END;");
                }
            }
            catch
            {
            }
        }
        private List<PublicNewsTickerVM> GetAll(string keyword, string status)
        {
            using (var db = new PoliticalLeaderPortalDbEntities1())
            {
                var list = db.Database.SqlQuery<PublicNewsTickerVM>(@"
SELECT PublicNewsTickerId, TickerText, LinkUrl, Category, DisplayOrder, IsActive, StartDate, EndDate, CreatedDate, UpdatedDate
FROM dbo.PublicNewsTicker
ORDER BY DisplayOrder, PublicNewsTickerId DESC;").ToList();

                if (!String.IsNullOrWhiteSpace(keyword))
                {
                    string term = keyword.Trim();
                    list = list.Where(x =>
                        (!String.IsNullOrWhiteSpace(x.TickerText) && x.TickerText.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (!String.IsNullOrWhiteSpace(x.Category) && x.Category.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (!String.IsNullOrWhiteSpace(x.LinkUrl) && x.LinkUrl.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)).ToList();
                }

                if (status == "active") list = list.Where(x => x.IsActive).ToList();
                if (status == "inactive") list = list.Where(x => !x.IsActive).ToList();

                return list;
            }
        }

        private PublicNewsTickerVM GetById(int id)
        {
            using (var db = new PoliticalLeaderPortalDbEntities1())
            {
                return db.Database.SqlQuery<PublicNewsTickerVM>(@"
SELECT PublicNewsTickerId, TickerText, LinkUrl, Category, DisplayOrder, IsActive, StartDate, EndDate, CreatedDate, UpdatedDate
FROM dbo.PublicNewsTicker
WHERE PublicNewsTickerId = @Id;", new SqlParameter("@Id", id)).FirstOrDefault();
            }
        }

        private int GetNextDisplayOrder()
        {
            using (var db = new PoliticalLeaderPortalDbEntities1())
            {
                return db.Database.SqlQuery<int>("SELECT ISNULL(MAX(DisplayOrder), 0) + 1 FROM dbo.PublicNewsTicker").FirstOrDefault();
            }
        }

        private void EnsureTable()
        {
            using (var db = new PoliticalLeaderPortalDbEntities1())
            {
                db.Database.ExecuteSqlCommand(@"
IF OBJECT_ID('dbo.PublicNewsTicker', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PublicNewsTicker
    (
        PublicNewsTickerId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TickerText NVARCHAR(300) NOT NULL,
        LinkUrl NVARCHAR(500) NULL,
        Category NVARCHAR(50) NULL,
        DisplayOrder INT NOT NULL DEFAULT(0),
        IsActive BIT NOT NULL DEFAULT(1),
        StartDate DATETIME NULL,
        EndDate DATETIME NULL,
        CreatedDate DATETIME NOT NULL DEFAULT(GETDATE()),
        UpdatedDate DATETIME NULL
    );
END;
IF NOT EXISTS (SELECT 1 FROM dbo.PublicNewsTicker)
BEGIN
    INSERT INTO dbo.PublicNewsTicker (TickerText, LinkUrl, Category, DisplayOrder, IsActive, StartDate, EndDate, CreatedDate)
    VALUES
    (N'नही अजमेरे आते हैं जिन्हें ख्वाजा बुलाते हैं..!', NULL, N'Breaking News', 1, 1, GETDATE(), NULL, GETDATE()),
    (N'जन सेवा, विकास और संवाद से जुड़ें।', NULL, N'Public Update', 2, 1, GETDATE(), NULL, GETDATE()),
    (N'ताज़ा कार्यक्रम, समाचार और जनसम्पर्क अपडेट यहां देखें।', NULL, N'Portal Update', 3, 1, GETDATE(), NULL, GETDATE());
END;");
            }
        }

        private SqlParameter Parameter(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }
    }
}


