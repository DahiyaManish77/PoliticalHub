using PoliticalLeaderPortal.Models;
using PoliticalLeaderPortal.ViewModels;
using System.Linq;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Controllers
{
    public class NewsController : Controller
    {
        private readonly PoliticalLeaderPortalDbEntities1 _db;

        public NewsController()
        {
            _db =
                new PoliticalLeaderPortalDbEntities1();
        }
        public ActionResult Index()
        {
            var model =
                _db.LatestNews
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.PublishDate)
                .Select(x =>
                    new LatestNewsDisplayVM
                    {
                        NewsId = x.NewsId,
                        Title = x.Title,
                        ShortDescription = x.ShortDescription,
                        ImagePath = x.ImagePath,
                        PublishDate = x.PublishDate,
                        IsFeatured = x.IsFeatured,
                        DisplayOrder = x.DisplayOrder
                    })
                .ToList();

            return View(model);
        }
        public ActionResult Details(int id)
        {
            var news =
                _db.LatestNews
                .FirstOrDefault(x =>
                    x.NewsId == id &&
                    x.IsActive);

            if (news == null)
            {
                return HttpNotFound();
            }

            NewsDetailsVM model =
                new NewsDetailsVM();

            model.NewsId =
                news.NewsId;

            model.Title =
                news.Title;

            model.ShortDescription =
                news.ShortDescription;

            model.FullDescription =
                news.FullDescription;

            model.ImagePath =
                news.ImagePath;

            model.PublishDate =
                news.PublishDate;

            model.RelatedNews =
                _db.LatestNews
                .Where(x =>
                    x.NewsId != id &&
                    x.IsActive)
                .OrderByDescending(x =>
                    x.PublishDate)
                .Take(4)
                .Select(x =>
                    new LatestNewsDisplayVM
                    {
                        NewsId = x.NewsId,
                        Title = x.Title,
                        ShortDescription =
                            x.ShortDescription,
                        ImagePath =
                            x.ImagePath,
                        PublishDate =
                            x.PublishDate
                    })
                .ToList();

            return View(model);
        }
    }
}