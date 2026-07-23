using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using PoliticalLeaderPortal.Models;
using System;
using System.IO;
using System.Linq;
using System.Web;

namespace PoliticalLeaderPortal.Areas.Admin.Services
{
    public class WebsiteFooterSettingService
    {
        private readonly PoliticalLeaderPortalDbEntities1 _db;

        public WebsiteFooterSettingService()
        {
            _db = new PoliticalLeaderPortalDbEntities1();
        }

        public WebsiteFooterSettingVM GetSetting()
        {
            WebsiteFooterSetting entity =
                _db.WebsiteFooterSettings
                .FirstOrDefault(x =>
                    x.WebsiteFooterSettingId == 1);

            if (entity == null)
            {
                return new WebsiteFooterSettingVM
                {
                    WebsiteFooterSettingId = 1,
                    IsActive = true
                };
            }

            return new WebsiteFooterSettingVM
            {
                WebsiteFooterSettingId = entity.WebsiteFooterSettingId,
                AboutText = entity.AboutText,
                Address = entity.Address,
                PhoneNumber = entity.PhoneNumber,
                EmailAddress = entity.EmailAddress,
                CopyrightText = entity.CopyrightText,
                FacebookUrl = entity.FacebookUrl,
                InstagramUrl = entity.InstagramUrl,
                TwitterUrl = entity.TwitterUrl,
                YoutubeUrl = entity.YoutubeUrl,
                WhatsappUrl = entity.WhatsappUrl,
                FooterLogoPath = entity.FooterLogoPath,
                IsActive = entity.IsActive
            };
        }

        public void Save(
            WebsiteFooterSettingVM model,
            HttpServerUtilityBase server)
        {
            WebsiteFooterSetting entity =
                _db.WebsiteFooterSettings
                .FirstOrDefault(x =>
                    x.WebsiteFooterSettingId == 1);

            if (entity == null)
            {
                entity = new WebsiteFooterSetting();

                entity.WebsiteFooterSettingId = 1;
                entity.CreatedDate = DateTime.Now;
                entity.CreatedBy = 1;

                _db.WebsiteFooterSettings.Add(entity);
            }

            entity.AboutText = model.AboutText;
            entity.Address = model.Address;
            entity.PhoneNumber = model.PhoneNumber;
            entity.EmailAddress = model.EmailAddress;

            entity.CopyrightText =
                model.CopyrightText;

            entity.FacebookUrl =
                model.FacebookUrl;

            entity.InstagramUrl =
                model.InstagramUrl;

            entity.TwitterUrl =
                model.TwitterUrl;

            entity.YoutubeUrl =
                model.YoutubeUrl;

            entity.WhatsappUrl =
                model.WhatsappUrl;

            entity.IsActive =
                model.IsActive;

            string uploadFolder =
                server.MapPath(
                    "~/Uploads/Footer/");

            if (!Directory.Exists(
                uploadFolder))
            {
                Directory.CreateDirectory(
                    uploadFolder);
            }

            if (model.FooterLogoFile != null &&
                model.FooterLogoFile.ContentLength > 0)
            {
                string fileName =
                    "footer_" +
                    DateTime.Now.Ticks +
                    Path.GetExtension(
                        model.FooterLogoFile.FileName);

                string filePath =
                    Path.Combine(
                        uploadFolder,
                        fileName);

                model.FooterLogoFile.SaveAs(
                    filePath);

                entity.FooterLogoPath =
                    "/Uploads/Footer/" +
                    fileName;
            }

            entity.ModifiedDate =
                DateTime.Now;

            entity.ModifiedBy = 1;

            _db.SaveChanges();
        }
    }
}