using PoliticalLeaderPortal.Areas.Admin.Services;
using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using PoliticalLeaderPortal.Areas.Admin.ViewModels.Menu;
using PoliticalLeaderPortal.Models;
using PoliticalLeaderPortal.Services;
using PoliticalLeaderPortal.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Controllers
{
    public class LayoutController : Controller
    {
        private readonly PoliticalLeaderPortalDbEntities1 _db;
        private readonly GalleryService _galleryService;
        private readonly VideoGalleryService _videoService;
        private readonly MediaCoverageService _mediaCoverageService;
        private readonly HomeMemberService _homeMemberService;
        private readonly AppDownloadSettingService _appDownloadSettingService;
        private readonly MenuService _menuService;
        public LayoutController()
        {
            _db = new PoliticalLeaderPortalDbEntities1();
            _galleryService = new GalleryService();
            _videoService = new VideoGalleryService();
            _mediaCoverageService = new MediaCoverageService();
            _homeMemberService = new HomeMemberService();
            _appDownloadSettingService = new AppDownloadSettingService();
            _menuService = new MenuService();
        }


        [ChildActionOnly]
        public ActionResult PublicNewsTicker()
        {
            var model = new List<PublicNewsTickerVM>();

            try
            {
                EnsurePublicNewsTickerTable();
                model = _db.Database.SqlQuery<PublicNewsTickerVM>(@"
SELECT PublicNewsTickerId, TickerText, LinkUrl, Category, DisplayOrder, IsActive, StartDate, EndDate, CreatedDate, UpdatedDate
FROM dbo.PublicNewsTicker
WHERE IsActive = 1
  AND (StartDate IS NULL OR StartDate <= GETDATE())
  AND (EndDate IS NULL OR EndDate >= CONVERT(date, GETDATE()))
ORDER BY DisplayOrder, PublicNewsTickerId DESC;").ToList();
            }
            catch
            {
                model = new List<PublicNewsTickerVM>();
            }

            if (!model.Any())
            {
                model.Add(new PublicNewsTickerVM { TickerText = "नही अजमेरे आते हैं जिन्हें ख्वाजा बुलाते हैं..!", DisplayOrder = 1, IsActive = true });
                model.Add(new PublicNewsTickerVM { TickerText = "जन सेवा, विकास और संवाद से जुड़ें।", DisplayOrder = 2, IsActive = true });
                model.Add(new PublicNewsTickerVM { TickerText = "ताज़ा कार्यक्रम, समाचार और जनसम्पर्क अपडेट यहां देखें।", DisplayOrder = 3, IsActive = true });
            }

            return PartialView("~/Views/Shared/_PublicNewsTicker.cshtml", model);
        }

        private void EnsurePublicNewsTickerTable()
        {
            _db.Database.ExecuteSqlCommand(@"
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
END;");
        }
        [ChildActionOnly]
        public ActionResult WebsiteNavigation()
        {
            List<MenuVM> model;

            try
            {
                model = _menuService.GetWebsiteMenus();
            }
            catch
            {
                model = new List<MenuVM>();
            }

            return PartialView(
                "~/Views/Shared/_NavigationEnterprise.cshtml",
                model);
        }

        [ChildActionOnly]
        public ActionResult TopBar()
        {
            return PartialView(
                "_TopBar",
                GetHeaderData());
        }

        [ChildActionOnly]
        public ActionResult Header()
        {
            return PartialView(
                "_Header",
                GetHeaderData());
        }

        [ChildActionOnly]
        public ActionResult Footer()
        {
            return PartialView(
                "_Footer",
                GetFooterData());
        }

        [ChildActionOnly]
        public ActionResult HeroSlider()
        {
            try
            {
                var heroRows =
                    _db.HeroSliders
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.DisplayOrder)
                    .ToList();

                var assetVersion =
                    DateTime.UtcNow.Ticks.ToString();

                var model =
                    heroRows
                    .Select(x => new HeroSliderDisplayVM
                    {
                        HeroSliderId = x.HeroSliderId,
                        Title = x.Title,
                        SubTitle = x.SubTitle,
                        Description = x.Description,
                        ImagePath = x.ImagePath,
                        BackgroundImagePath = x.BackgroundImagePath,
                        MobileImagePath = x.MobileImagePath,
                        LeaderImagePath = x.LeaderImagePath,
                        AssetVersion = assetVersion,
                        BackgroundColor = String.IsNullOrWhiteSpace(x.BackgroundColor) ? "#124734" : x.BackgroundColor,
                        HeroHeightCss = "480px",
                        LeaderImagePosition = x.LeaderImagePosition,
                        ShowLeaderImage = x.ShowLeaderImage,
                        ButtonText = x.ButtonText,
                        ButtonUrl = x.ButtonUrl,
                        ButtonText2 = x.ButtonText2,
                        ButtonUrl2 = x.ButtonUrl2,
                        DisplayOrder = x.DisplayOrder,

                        TemplateType = x.TemplateType,
                        OverlayType = x.OverlayType,
                        SliderTransition = x.SliderTransition,
                        TitleAnimation = x.TitleAnimation,
                        SubTitleAnimation = x.SubTitleAnimation,
                        DescriptionAnimation = x.DescriptionAnimation,
                        ButtonAnimation = x.ButtonAnimation,
                        TextAlignment = x.TextAlignment,
                        OverlayOpacity = x.OverlayOpacity,
                        ShowOverlay = true,
                        ShowButtons =
                            (!String.IsNullOrWhiteSpace(x.ButtonText) &&
                             !String.IsNullOrWhiteSpace(x.ButtonUrl)) ||
                            (!String.IsNullOrWhiteSpace(x.ButtonText2) &&
                             !String.IsNullOrWhiteSpace(x.ButtonUrl2))
                    })
                    .ToList();

                ResolveHeroDisplayImages(model);

                AttachHeroVideoFields(model);

                return PartialView(
                    "~/Views/Home/Partials/_HeroSlider.cshtml",
                    model);
            }
            catch
            {
                return PartialView(
                    "~/Views/Home/Partials/_HeroSlider.cshtml",
                    new List<HeroSliderDisplayVM>());
            }
        }

        [ChildActionOnly]
        public ActionResult HomeStatistics()
        {
            try
            {
                var model =
                _db.HomePageStatistics
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new HomeStatisticDisplayVM
                {
                    StatisticId = x.StatisticId,
                    Title = x.Title,
                    StatisticValue = x.StatisticValue,
                    IconClass = x.IconClass,
                    DisplayOrder = x.DisplayOrder
                })
                .ToList();

                return PartialView(
                    "~/Views/Home/Partials/_HomeStatistics.cshtml",
                    model);
            }
            catch
            {
                return PartialView(
                    "~/Views/Home/Partials/_HomeStatistics.cshtml",
                    new List<HomeStatisticDisplayVM>());
            }
        }

        [ChildActionOnly]
        public ActionResult AppDownload()
        {
            try
            {
                var model =
                    _appDownloadSettingService.GetDisplaySetting();

                return PartialView(
                    "~/Views/Home/Partials/_SomAppCallToAction.cshtml",
                    model);
            }
            catch
            {
                return PartialView(
                    "~/Views/Home/Partials/_SomAppCallToAction.cshtml",
                    null);
            }
        }

        [ChildActionOnly]
        public ActionResult HomeMembers()
        {
            try
            {
                var model =
                    _homeMemberService.GetHomeMembers(12);

                return PartialView(
                    "~/Views/Home/Partials/_HomeMembers.cshtml",
                    model);
            }
            catch
            {
                return PartialView(
                    "~/Views/Home/Partials/_HomeMembers.cshtml",
                    new List<HomeMemberDisplayVM>());
            }
        }

        [ChildActionOnly]
        public ActionResult LatestNews()
        {
            try
            {
                var model =
                _db.LatestNews
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.IsFeatured)
                .ThenByDescending(x => x.PublishDate)
                .Select(x => new LatestNewsDisplayVM
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

                return PartialView(
                    "~/Views/Home/Partials/_LatestNews.cshtml",
                    model);
            }
            catch
            {
                return PartialView(
                    "~/Views/Home/Partials/_LatestNews.cshtml",
                    new List<LatestNewsDisplayVM>());
            }
        }

        [ChildActionOnly]
        public ActionResult MediaCoverage()
        {
            try
            {
                var model =
                    _mediaCoverageService
                    .GetHomeMediaCoverage();

                return PartialView("~/Views/Home/Partials/_MediaCoverage.cshtml",
                     model);
            }
            catch
            {
                return PartialView("~/Views/Home/Partials/_MediaCoverage.cshtml",
                    null);
            }
        }

        public PartialViewResult Downloads()
        {
            try
            {
                DownloadDocumentService service =
                    new DownloadDocumentService();

                return PartialView(
                    "_Downloads",
                    service.GetLatest(6));
            }
            catch
            {
                return PartialView("_Downloads", null);
            }
        }

        [ChildActionOnly]
        public ActionResult UpcomingEvents()
        {
            try
            {
                var model = _db.Database.SqlQuery<UpcomingEventListVM>(@"
                    SELECT TOP 4
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
                      AND e.EventDate >= CAST(GETDATE() AS date)
                    ORDER BY e.EventDate, e.StartTime")
                    .ToList();

                return PartialView(
                    "~/Views/Home/Partials/_UpcomingEvents.cshtml",
                    model);
            }
            catch
            {
                return PartialView(
                    "~/Views/Home/Partials/_UpcomingEvents.cshtml",
                    new List<UpcomingEventListVM>());
            }
        }

        [ChildActionOnly]
        public ActionResult PhotoGallery()
        {
            try
            {
                var model =
                    _galleryService.GetHomepageGallery(7);

                return PartialView(
                    "~/Views/Home/Partials/_ImageGallery.cshtml",
                    model);
            }
            catch
            {
                return PartialView(
                    "~/Views/Home/Partials/_ImageGallery.cshtml",
                    null);
            }
        }
        [ChildActionOnly]
        public ActionResult VideoGallery()
        {
            try
            {
                var model =
                    _videoService.GetHomepageVideos(5);

                return PartialView(
                    "~/Views/Home/Partials/_VideoGallery.cshtml",
                    model);
            }
            catch
            {
                return PartialView(
                    "~/Views/Home/Partials/_VideoGallery.cshtml",
                    null);
            }
        }
        [ChildActionOnly]
        public ActionResult PollSurvey()
        {
            try
            {
                var service = new ElectionWarRoomService();
                var model = service.GetCampaignPolls(null)
                    .Where(x =>
                        x.IsActive &&
                        (!x.StartDate.HasValue || x.StartDate.Value.Date <= DateTime.Today) &&
                        (!x.EndDate.HasValue || x.EndDate.Value.Date >= DateTime.Today))
                    .OrderByDescending(x => x.CreatedOn)
                    .FirstOrDefault();

                return PartialView(
                    "~/Views/Home/Partials/_PollSurvey.cshtml",
                    model);
            }
            catch
            {
                return PartialView(
                    "~/Views/Home/Partials/_PollSurvey.cshtml",
                    null);
            }
        }

        private LayoutFooterVM GetFooterData()
        {
            try
            {
                var footer =
                    _db.WebsiteFooterSettings
                    .FirstOrDefault(x =>
                        x.WebsiteFooterSettingId == 1);

                if (footer == null)
                {
                    return GetFallbackFooter();
                }

                return new LayoutFooterVM
                {
                    AboutText = footer.AboutText,
                    Address = footer.Address,
                    PhoneNumber = footer.PhoneNumber,
                    EmailAddress = footer.EmailAddress,
                    CopyrightText = footer.CopyrightText,

                    FacebookUrl = footer.FacebookUrl,
                    InstagramUrl = footer.InstagramUrl,
                    TwitterUrl = footer.TwitterUrl,
                    YoutubeUrl = footer.YoutubeUrl,
                    WhatsappUrl = footer.WhatsappUrl,

                    FooterLogoPath = footer.FooterLogoPath,
                    IsActive = footer.IsActive
                };
            }
            catch
            {
                return GetFallbackFooter();
            }
        }

        private void AttachHeroVideoFields(List<HeroSliderDisplayVM> model)
        {
            if (model == null || !model.Any())
            {
                return;
            }

            try
            {
                var videoRows =
                    _db.Database.SqlQuery<HeroSliderVideoRow>(
                        @"SELECT
                            HeroSliderId,
                            CAST(ISNULL(IsVideoSlide, 0) AS bit) AS IsVideoSlide,
                            ISNULL(VideoSourceType, '') AS VideoSourceType,
                            ISNULL(VideoUrl, '') AS VideoUrl,
                            ISNULL(VideoPath, '') AS VideoPath,
                            CAST(ISNULL(VideoAutoplay, 1) AS bit) AS VideoAutoplay,
                            CAST(ISNULL(VideoMuted, 1) AS bit) AS VideoMuted,
                            CAST(ISNULL(VideoLoop, 1) AS bit) AS VideoLoop,
                            ISNULL(NULLIF(HeroHeightCss, ''), '480px') AS HeroHeightCss
                          FROM dbo.HeroSlider")
                    .ToDictionary(x => x.HeroSliderId);

                foreach (var slide in model)
                {
                    HeroSliderVideoRow video;

                    if (!videoRows.TryGetValue(slide.HeroSliderId, out video))
                    {
                        continue;
                    }

                    slide.IsVideoSlide = video.IsVideoSlide;
                    slide.VideoSourceType = video.VideoSourceType;
                    slide.VideoUrl = video.VideoUrl;
                    slide.VideoPath = video.VideoPath;
                    slide.VideoAutoplay = video.VideoAutoplay;
                    slide.VideoMuted = video.VideoMuted;
                    slide.VideoLoop = video.VideoLoop;
                    slide.HeroHeightCss = video.HeroHeightCss;
                }
            }
            catch
            {
                // Video columns are optional until App_Data/HeroSliderVideoUpgrade.sql is applied.
            }
        }

        private void ResolveHeroDisplayImages(List<HeroSliderDisplayVM> model)
        {
            if (model == null || !model.Any())
            {
                return;
            }

            foreach (var slide in model)
            {
                slide.BackgroundImagePath =
                    ResolveNewestExistingHeroImage(
                        slide.BackgroundImagePath,
                        slide.ImagePath);
            }
        }

        private string ResolveNewestExistingHeroImage(
            string preferredPath,
            string fallbackPath)
        {
            string preferred = NormalizeVirtualPath(preferredPath);
            string fallback = NormalizeVirtualPath(fallbackPath);

            DateTime? preferredModified = GetHeroFileModifiedDate(preferred);
            DateTime? fallbackModified = GetHeroFileModifiedDate(fallback);

            if (preferredModified.HasValue && fallbackModified.HasValue)
            {
                return fallbackModified.Value > preferredModified.Value
                    ? fallback
                    : preferred;
            }

            if (preferredModified.HasValue)
            {
                return preferred;
            }

            if (fallbackModified.HasValue)
            {
                return fallback;
            }

            return !String.IsNullOrWhiteSpace(preferred)
                ? preferred
                : fallback;
        }

        private DateTime? GetHeroFileModifiedDate(string virtualPath)
        {
            if (String.IsNullOrWhiteSpace(virtualPath))
            {
                return null;
            }

            try
            {
                string physicalPath = Server.MapPath(virtualPath);

                if (System.IO.File.Exists(physicalPath))
                {
                    return System.IO.File.GetLastWriteTimeUtc(physicalPath);
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        private string NormalizeVirtualPath(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                return String.Empty;
            }

            string trimmed = path.Trim();

            if (trimmed.StartsWith("~/", StringComparison.Ordinal))
            {
                return trimmed;
            }

            if (trimmed.StartsWith("/", StringComparison.Ordinal))
            {
                return "~" + trimmed;
            }

            return "~/" + trimmed.TrimStart('/');
        }

        private class HeroSliderVideoRow
        {
            public int HeroSliderId { get; set; }
            public bool IsVideoSlide { get; set; }
            public string VideoSourceType { get; set; }
            public string VideoUrl { get; set; }
            public string VideoPath { get; set; }
            public bool VideoAutoplay { get; set; }
            public bool VideoMuted { get; set; }
            public bool VideoLoop { get; set; }
            public string HeroHeightCss { get; set; }
        }

        private LayoutFooterVM GetFallbackFooter()
        {
            return new LayoutFooterVM
            {
                AboutText = "Official public communication portal for constituency updates, development work, news, events and citizen contact.",
                Address = "Sardhana Constituency, Meerut, Uttar Pradesh",
                PhoneNumber = "",
                EmailAddress = "",
                CopyrightText = "Copyright " + System.DateTime.Now.Year + " Political Leader Portal. All Rights Reserved.",
                FooterLogoPath = "~/Content/images/logo.png",
                IsActive = true
            };
        }

        private LayoutHeaderVM GetHeaderData()
        {
            try
            {
                var website =
                    _db.WebsiteSettings
                    .FirstOrDefault(x =>
                        x.WebsiteSettingId == 1);

                var header =
                    _db.WebsiteHeaderSettings
                    .FirstOrDefault(x =>
                        x.WebsiteHeaderSettingId == 1);

                return new LayoutHeaderVM
                {
                    WebsiteName =
                        website != null
                        ? website.WebsiteName
                        : "Political Leader",

                    WebsiteTagline =
                        website != null
                        ? website.WebsiteTagline
                        : "",

                    LogoPath =
                        website != null
                        ? website.WebsiteLogoPath
                        : "",

                    LeaderImagePath =
                        header != null
                        ? header.LeaderImagePath
                        : "",

                    PhoneNumber =
                        header != null
                        ? header.PhoneNumber
                        : "",

                    EmailAddress =
                        header != null
                        ? header.EmailAddress
                        : "",

                    ShowTopBar =
                        header == null
                        ? true
                        : header.ShowTopBar,

                    ShowLanguageSwitcher =
                        header == null
                        ? true
                        : header.ShowLanguageSwitcher,

                    ShowSignIn =
                        header == null
                        ? true
                        : header.ShowSignIn,

                    ShowSignUp =
                        header == null
                        ? true
                        : header.ShowSignUp,

                    HeaderBackgroundColor =
                        header != null
                        ? header.HeaderBackgroundColor
                        : "#ffffff",

                    HeaderTextColor =
                        header != null
                        ? header.HeaderTextColor
                        : "#000000",

                    HeaderFontFamily =
                        header != null
                        ? header.HeaderFontFamily
                        : "Poppins",

                    HeaderFontSize =
                        header != null
                        ? header.HeaderFontSize
                        : "16px"
                };
            }
            catch
            {
                return new LayoutHeaderVM
                {
                    WebsiteName = "Political Leader",
                    WebsiteTagline = "",
                    LogoPath = "~/Content/images/logo.png",
                    LeaderImagePath = "",
                    PhoneNumber = "",
                    EmailAddress = "",
                    ShowTopBar = true,
                    ShowLanguageSwitcher = true,
                    ShowSignIn = true,
                    ShowSignUp = true,
                    HeaderBackgroundColor = "#ffffff",
                    HeaderTextColor = "#000000",
                    HeaderFontFamily = "Poppins",
                    HeaderFontSize = "16px"
                };
            }
        }
    }
}

