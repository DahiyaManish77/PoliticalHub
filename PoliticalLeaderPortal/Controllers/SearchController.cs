using PoliticalLeaderPortal.Models;
using PoliticalLeaderPortal.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Controllers
{
    public class SearchController : Controller
    {
        private readonly PoliticalLeaderPortalDbEntities1 _db;

        public SearchController()
        {
            _db = new PoliticalLeaderPortalDbEntities1();
        }

        public ActionResult Index(string keyword, DateTime? from, DateTime? to)
        {
            var model = new SearchPageVM
            {
                Keyword = keyword,
                FromDate = from,
                ToDate = to,
                Results = BuildResults(keyword, from, to)
            };

            return View(model);
        }

        private List<SearchResultItemVM> BuildResults(string keyword, DateTime? from, DateTime? to)
        {
            var normalizedKeyword = (keyword ?? string.Empty).Trim();
            var hasKeyword = !string.IsNullOrWhiteSpace(normalizedKeyword);
            var results = new List<SearchResultItemVM>();

            results.AddRange(SearchNews(normalizedKeyword, hasKeyword, from, to));
            results.AddRange(SearchEvents(normalizedKeyword, hasKeyword, from, to));
            results.AddRange(SearchMediaCoverage(normalizedKeyword, hasKeyword, from, to));
            results.AddRange(SearchDownloads(normalizedKeyword, hasKeyword, from, to));
            results.AddRange(SearchGallery(normalizedKeyword, hasKeyword, from, to));
            results.AddRange(SearchVideos(normalizedKeyword, hasKeyword, from, to));
            results.AddRange(SearchPages(normalizedKeyword, hasKeyword, from, to));

            return results
                .OrderByDescending(x => x.PublishedDate ?? DateTime.MinValue)
                .Take(100)
                .ToList();
        }

        private IEnumerable<SearchResultItemVM> SearchNews(string keyword, bool hasKeyword, DateTime? from, DateTime? to)
        {
            var query = _db.LatestNews.Where(x => x.IsActive);

            if (hasKeyword)
            {
                query = query.Where(x =>
                    x.Title.Contains(keyword) ||
                    x.ShortDescription.Contains(keyword) ||
                    x.FullDescription.Contains(keyword));
            }

            query = ApplyDateFilter(query, from, to, x => x.PublishDate);

            return query
                .OrderByDescending(x => x.PublishDate)
                .Take(25)
                .ToList()
                .Select(x => new SearchResultItemVM
                {
                    Title = x.Title,
                    Description = x.ShortDescription,
                    Category = "News",
                    Url = Url.Action("Details", "News", new { id = x.NewsId }),
                    ImagePath = x.ImagePath,
                    PublishedDate = x.PublishDate
                });
        }

        private IEnumerable<SearchResultItemVM> SearchEvents(string keyword, bool hasKeyword, DateTime? from, DateTime? to)
        {
            var query = _db.UpcomingEvents.Where(x => x.IsActive);

            if (hasKeyword)
            {
                query = query.Where(x =>
                    x.Title.Contains(keyword) ||
                    x.ShortDescription.Contains(keyword) ||
                    x.FullDescription.Contains(keyword) ||
                    x.EventLocation.Contains(keyword));
            }

            query = ApplyDateFilter(query, from, to, x => x.EventDate);

            return query
                .OrderByDescending(x => x.EventDate)
                .Take(25)
                .ToList()
                .Select(x => new SearchResultItemVM
                {
                    Title = x.Title,
                    Description = x.ShortDescription,
                    Category = "Events",
                    Url = Url.Action("Details", "Event", new { id = x.EventId }),
                    ImagePath = x.EventImagePath,
                    PublishedDate = x.EventDate
                });
        }

        private IEnumerable<SearchResultItemVM> SearchMediaCoverage(string keyword, bool hasKeyword, DateTime? from, DateTime? to)
        {
            var query = _db.MediaCoverages.Where(x => x.IsActive);

            if (hasKeyword)
            {
                query = query.Where(x =>
                    x.Title.Contains(keyword) ||
                    x.ShortDescription.Contains(keyword) ||
                    x.FullDescription.Contains(keyword) ||
                    x.SourceName.Contains(keyword));
            }

            query = ApplyNullableDateFilter(query, from, to, x => x.CoverageDate);

            return query
                .OrderByDescending(x => x.CoverageDate ?? x.CreatedDate)
                .Take(25)
                .ToList()
                .Select(x => new SearchResultItemVM
                {
                    Title = x.Title,
                    Description = x.ShortDescription,
                    Category = "Media Coverage",
                    Url = Url.Action("Details", "PublicMediaCoverage", new { id = x.MediaCoverageId }),
                    ImagePath = x.CoverImagePath,
                    PublishedDate = x.CoverageDate ?? x.CreatedDate
                });
        }

        private IEnumerable<SearchResultItemVM> SearchDownloads(string keyword, bool hasKeyword, DateTime? from, DateTime? to)
        {
            var query = _db.DownloadDocuments.Where(x => x.IsActive);

            if (hasKeyword)
            {
                query = query.Where(x =>
                    x.Title.Contains(keyword) ||
                    x.ShortDescription.Contains(keyword) ||
                    x.DocumentNumber.Contains(keyword) ||
                    x.FileName.Contains(keyword));
            }

            query = ApplyNullableDateFilter(query, from, to, x => x.PublishDate);

            return query
                .OrderByDescending(x => x.PublishDate ?? x.CreatedDate)
                .Take(25)
                .ToList()
                .Select(x => new SearchResultItemVM
                {
                    Title = x.Title,
                    Description = x.ShortDescription,
                    Category = "Downloads",
                    Url = Url.Action("Download", "Downloads", new { id = x.DownloadDocumentId }),
                    ImagePath = null,
                    PublishedDate = x.PublishDate ?? x.CreatedDate
                });
        }

        private IEnumerable<SearchResultItemVM> SearchGallery(string keyword, bool hasKeyword, DateTime? from, DateTime? to)
        {
            var query = _db.GalleryCategories.Where(x => x.IsActive);

            if (hasKeyword)
            {
                query = query.Where(x =>
                    x.CategoryName.Contains(keyword) ||
                    x.CategoryDescription.Contains(keyword));
            }

            query = ApplyDateFilter(query, from, to, x => x.CreatedDate);

            return query
                .OrderByDescending(x => x.CreatedDate)
                .Take(20)
                .ToList()
                .Select(x => new SearchResultItemVM
                {
                    Title = x.CategoryName,
                    Description = x.CategoryDescription,
                    Category = "Gallery",
                    Url = Url.Action("Album", "Gallery", new { id = x.GalleryCategoryId }),
                    ImagePath = x.CoverImagePath,
                    PublishedDate = x.CreatedDate
                });
        }

        private IEnumerable<SearchResultItemVM> SearchVideos(string keyword, bool hasKeyword, DateTime? from, DateTime? to)
        {
            var query = _db.VideoGalleries.Where(x => x.IsActive);

            if (hasKeyword)
            {
                query = query.Where(x =>
                    x.VideoTitle.Contains(keyword) ||
                    x.VideoDescription.Contains(keyword));
            }

            query = ApplyDateFilter(query, from, to, x => x.CreatedDate);

            return query
                .OrderByDescending(x => x.CreatedDate)
                .Take(20)
                .ToList()
                .Select(x => new SearchResultItemVM
                {
                    Title = x.VideoTitle,
                    Description = x.VideoDescription,
                    Category = "Videos",
                    Url = Url.Action("Details", "Video", new { id = x.VideoId }),
                    ImagePath = x.ThumbnailImagePath,
                    PublishedDate = x.CreatedDate
                });
        }

        private IEnumerable<SearchResultItemVM> SearchPages(string keyword, bool hasKeyword, DateTime? from, DateTime? to)
        {
            var query = _db.MenuMasters.Where(x =>
                x.IsActive &&
                x.IsClickable &&
                !x.ShowInAdminSidebar);

            if (hasKeyword)
            {
                query = query.Where(x =>
                    x.MenuName.Contains(keyword) ||
                    x.MenuDescription.Contains(keyword) ||
                    x.PageTitle.Contains(keyword) ||
                    x.MetaDescription.Contains(keyword));
            }

            query = ApplyDateFilter(query, from, to, x => x.CreatedDate);

            return query
                .OrderBy(x => x.DisplayOrder)
                .Take(20)
                .ToList()
                .Select(x => new SearchResultItemVM
                {
                    Title = string.IsNullOrWhiteSpace(x.PageTitle) ? x.MenuName : x.PageTitle,
                    Description = string.IsNullOrWhiteSpace(x.MenuDescription) ? x.MetaDescription : x.MenuDescription,
                    Category = "Pages",
                    Url = ResolveMenuUrl(x),
                    ImagePath = null,
                    PublishedDate = x.ModifiedDate ?? x.CreatedDate
                });
        }

        private IQueryable<T> ApplyDateFilter<T>(
            IQueryable<T> query,
            DateTime? from,
            DateTime? to,
            System.Linq.Expressions.Expression<Func<T, DateTime>> dateSelector)
        {
            if (from.HasValue)
            {
                var fromDate = from.Value.Date;
                query = query.Where(BuildDateComparison(dateSelector, fromDate, true));
            }

            if (to.HasValue)
            {
                var toDate = to.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(BuildDateComparison(dateSelector, toDate, false));
            }

            return query;
        }

        private IQueryable<T> ApplyNullableDateFilter<T>(
            IQueryable<T> query,
            DateTime? from,
            DateTime? to,
            System.Linq.Expressions.Expression<Func<T, DateTime?>> dateSelector)
        {
            if (from.HasValue)
            {
                var fromDate = from.Value.Date;
                query = query.Where(BuildNullableDateComparison(dateSelector, fromDate, true));
            }

            if (to.HasValue)
            {
                var toDate = to.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(BuildNullableDateComparison(dateSelector, toDate, false));
            }

            return query;
        }

        private System.Linq.Expressions.Expression<Func<T, bool>> BuildDateComparison<T>(
            System.Linq.Expressions.Expression<Func<T, DateTime>> selector,
            DateTime value,
            bool isGreaterThan)
        {
            var body = isGreaterThan
                ? System.Linq.Expressions.Expression.GreaterThanOrEqual(selector.Body, System.Linq.Expressions.Expression.Constant(value))
                : System.Linq.Expressions.Expression.LessThanOrEqual(selector.Body, System.Linq.Expressions.Expression.Constant(value));

            return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(body, selector.Parameters);
        }

        private System.Linq.Expressions.Expression<Func<T, bool>> BuildNullableDateComparison<T>(
            System.Linq.Expressions.Expression<Func<T, DateTime?>> selector,
            DateTime value,
            bool isGreaterThan)
        {
            var hasValue = System.Linq.Expressions.Expression.Property(selector.Body, "HasValue");
            var selectedValue = System.Linq.Expressions.Expression.Property(selector.Body, "Value");
            var comparison = isGreaterThan
                ? System.Linq.Expressions.Expression.GreaterThanOrEqual(selectedValue, System.Linq.Expressions.Expression.Constant(value))
                : System.Linq.Expressions.Expression.LessThanOrEqual(selectedValue, System.Linq.Expressions.Expression.Constant(value));
            var body = System.Linq.Expressions.Expression.AndAlso(hasValue, comparison);

            return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(body, selector.Parameters);
        }

        private string ResolveMenuUrl(MenuMaster menu)
        {
            if (!string.IsNullOrWhiteSpace(menu.CustomUrl))
            {
                return menu.CustomUrl;
            }

            if (!string.IsNullOrWhiteSpace(menu.ControllerName) &&
                !string.IsNullOrWhiteSpace(menu.ActionName))
            {
                return Url.Action(
                    menu.ActionName.Trim(),
                    menu.ControllerName.Trim(),
                    new { area = string.IsNullOrWhiteSpace(menu.AreaName) ? "" : menu.AreaName.Trim() });
            }

            return Url.Action("Index", "Home");
        }
    }
}
