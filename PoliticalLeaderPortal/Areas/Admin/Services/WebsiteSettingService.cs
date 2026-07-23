using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using PoliticalLeaderPortal.Models;
using System;
using System.IO;
using System.Linq;
using System.Web;

namespace PoliticalLeaderPortal.Areas.Admin.Services
{
    public class WebsiteSettingService
    {
        private readonly PoliticalLeaderPortalDbEntities1 _db;

        public WebsiteSettingService()
        {
            _db = new PoliticalLeaderPortalDbEntities1();
        }

        public WebsiteSettingVM GetSetting()
        {
            var entity =
     _db.WebsiteSettings
     .FirstOrDefault(x => x.WebsiteSettingId == 1);

            if (entity == null)
            {
                return new WebsiteSettingVM();
            }

            return new WebsiteSettingVM
            {
                WebsiteSettingId = entity.WebsiteSettingId,
                WebsiteName = entity.WebsiteName,
                WebsiteTagline = entity.WebsiteTagline,
                WebsiteLogoPath = entity.WebsiteLogoPath,
                FaviconPath = entity.FaviconPath,
                DefaultMetaTitle = entity.DefaultMetaTitle,
                DefaultMetaDescription = entity.DefaultMetaDescription,
                DefaultMetaKeywords = entity.DefaultMetaKeywords,
                GoogleAnalyticsCode = entity.GoogleAnalyticsCode,
                GoogleSearchConsoleCode = entity.GoogleSearchConsoleCode,
                DefaultLanguage = entity.DefaultLanguage,
                IsActive = entity.IsActive
            };
        }

        public void Save(
            WebsiteSettingVM model,
            HttpServerUtilityBase server)
        {
            WebsiteSetting entity =
    _db.WebsiteSettings
    .FirstOrDefault(x => x.WebsiteSettingId == 1);

            if (entity == null)
            {
                throw new Exception(
                    "WebsiteSetting master record missing. Expected WebsiteSettingId = 1");
            }

            entity.WebsiteName = model.WebsiteName;
            entity.WebsiteTagline = model.WebsiteTagline;
            entity.DefaultMetaTitle = model.DefaultMetaTitle;
            entity.DefaultMetaDescription = model.DefaultMetaDescription;
            entity.DefaultMetaKeywords = model.DefaultMetaKeywords;
            entity.GoogleAnalyticsCode = model.GoogleAnalyticsCode;
            entity.GoogleSearchConsoleCode = model.GoogleSearchConsoleCode;
            entity.DefaultLanguage = model.DefaultLanguage;
            entity.IsActive = model.IsActive;

            entity.ModifiedDate = DateTime.Now;
            entity.ModifiedBy = 1;

            if (model.LogoFile != null &&
                model.LogoFile.ContentLength > 0)
            {
                string fileName =
                    Guid.NewGuid().ToString()
                    + Path.GetExtension(model.LogoFile.FileName);

                string folder =
                    "~/Uploads/Website/";

                string path =
                    server.MapPath(folder);

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                model.LogoFile.SaveAs(
                    Path.Combine(path, fileName));

                entity.WebsiteLogoPath =
                    "/Uploads/Website/" + fileName;
            }

            if (model.FaviconFile != null &&
                model.FaviconFile.ContentLength > 0)
            {
                string fileName =
                    Guid.NewGuid().ToString()
                    + Path.GetExtension(model.FaviconFile.FileName);

                string folder =
                    "~/Uploads/Website/";

                string path =
                    server.MapPath(folder);

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                model.FaviconFile.SaveAs(
                    Path.Combine(path, fileName));

                entity.FaviconPath =
                    "/Uploads/Website/" + fileName;
            }

            _db.SaveChanges();
        }
    }
}