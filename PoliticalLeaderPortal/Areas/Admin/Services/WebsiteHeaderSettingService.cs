using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using PoliticalLeaderPortal.Models;
using System;
using System.IO;
using System.Linq;
using System.Web;

namespace PoliticalLeaderPortal.Areas.Admin.Services
{
    public class WebsiteHeaderSettingService
    {
        private readonly PoliticalLeaderPortalDbEntities1 _db;

        public WebsiteHeaderSettingService()
        {
            _db = new PoliticalLeaderPortalDbEntities1();
        }

        public WebsiteHeaderSettingVM GetSetting()
        {
            WebsiteHeaderSetting entity =
                _db.WebsiteHeaderSettings
                .FirstOrDefault(x =>
                    x.WebsiteHeaderSettingId == 1);

            if (entity == null)
            {
                return new WebsiteHeaderSettingVM
                {
                    WebsiteHeaderSettingId = 1,
                    HeaderBackgroundColor = "#ffffff",
                    HeaderTextColor = "#000000",
                    HeaderFontFamily = "Poppins",
                    HeaderFontSize = "16px",
                    ShowTopBar = true,
                    ShowLanguageSwitcher = true,
                    ShowSignIn = true,
                    ShowSignUp = true,
                    IsActive = true
                };
            }

            return new WebsiteHeaderSettingVM
            {
                WebsiteHeaderSettingId = entity.WebsiteHeaderSettingId,
                PhoneNumber = entity.PhoneNumber,
                EmailAddress = entity.EmailAddress,
                Address = entity.Address,
                FacebookUrl = entity.FacebookUrl,
                InstagramUrl = entity.InstagramUrl,
                TwitterUrl = entity.TwitterUrl,
                YoutubeUrl = entity.YoutubeUrl,
                WhatsappUrl = entity.WhatsappUrl,

                ShowTopBar = entity.ShowTopBar,
                ShowLanguageSwitcher = entity.ShowLanguageSwitcher,
                ShowSignIn = entity.ShowSignIn,
                ShowSignUp = entity.ShowSignUp,

                IsActive = entity.IsActive,

                LogoPath = entity.LogoPath,
                LeaderImagePath = entity.LeaderImagePath,

                HeaderBackgroundColor = entity.HeaderBackgroundColor,
                HeaderTextColor = entity.HeaderTextColor,
                HeaderFontFamily = entity.HeaderFontFamily,
                HeaderFontSize = entity.HeaderFontSize,

                LogoAnimationClass = entity.LogoAnimationClass,
                TextAnimationClass = entity.TextAnimationClass
            };
        }

        public void Save(
            WebsiteHeaderSettingVM model,
            HttpServerUtilityBase server)
        {
            WebsiteHeaderSetting entity =
                _db.WebsiteHeaderSettings
                .FirstOrDefault(x =>
                    x.WebsiteHeaderSettingId == 1);

            if (entity == null)
            {
                entity = new WebsiteHeaderSetting();

                entity.WebsiteHeaderSettingId = 1;

                entity.CreatedDate = DateTime.Now;
                entity.CreatedBy = 1;

                _db.WebsiteHeaderSettings.Add(entity);
            }

            entity.PhoneNumber = model.PhoneNumber;
            entity.EmailAddress = model.EmailAddress;
            entity.Address = model.Address;

            entity.FacebookUrl = model.FacebookUrl;
            entity.InstagramUrl = model.InstagramUrl;
            entity.TwitterUrl = model.TwitterUrl;
            entity.YoutubeUrl = model.YoutubeUrl;
            entity.WhatsappUrl = model.WhatsappUrl;

            entity.ShowTopBar = model.ShowTopBar;
            entity.ShowLanguageSwitcher = model.ShowLanguageSwitcher;
            entity.ShowSignIn = model.ShowSignIn;
            entity.ShowSignUp = model.ShowSignUp;

            entity.IsActive = model.IsActive;

            entity.HeaderBackgroundColor =
                model.HeaderBackgroundColor;

            entity.HeaderTextColor =
                model.HeaderTextColor;

            entity.HeaderFontFamily =
                model.HeaderFontFamily;

            entity.HeaderFontSize =
                model.HeaderFontSize;

            entity.LogoAnimationClass =
                model.LogoAnimationClass;

            entity.TextAnimationClass =
                model.TextAnimationClass;

            string uploadFolder =
                server.MapPath("~/Uploads/Header/");

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            if (model.LogoFile != null &&
                model.LogoFile.ContentLength > 0)
            {
                string logoName =
                    "logo_" +
                    DateTime.Now.Ticks +
                    Path.GetExtension(
                        model.LogoFile.FileName);

                string logoPath =
                    Path.Combine(
                        uploadFolder,
                        logoName);

                model.LogoFile.SaveAs(
                    logoPath);

                entity.LogoPath =
                    "/Uploads/Header/" +
                    logoName;
            }

            if (model.LeaderImageFile != null &&
                model.LeaderImageFile.ContentLength > 0)
            {
                string imageName =
                    "leader_" +
                    DateTime.Now.Ticks +
                    Path.GetExtension(
                        model.LeaderImageFile.FileName);

                string imagePath =
                    Path.Combine(
                        uploadFolder,
                        imageName);

                model.LeaderImageFile.SaveAs(
                    imagePath);

                entity.LeaderImagePath =
                    "/Uploads/Header/" +
                    imageName;
            }

            entity.ModifiedDate =
                DateTime.Now;

            entity.ModifiedBy = 1;

            _db.SaveChanges();
        }
    }
}