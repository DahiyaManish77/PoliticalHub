using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using PoliticalLeaderPortal.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;

namespace PoliticalLeaderPortal.Areas.Admin.Services
{
    public class HeroSliderService
    {
        private readonly PoliticalLeaderPortalDbEntities1 _db;

        public HeroSliderService()
        {
            _db = new PoliticalLeaderPortalDbEntities1();
        }

        #region Get All

        public List<HeroSliderVM> GetAll()
        {
            EnsureVideoColumns();

            return _db.HeroSliders
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new HeroSliderVM
                {
                    HeroSliderId = x.HeroSliderId,

                    /*--------------------------------------------------
                     CONTENT
                    ---------------------------------------------------*/

                    Title = x.Title,

                    SubTitle = x.SubTitle,

                    Description = x.Description,

                    /*--------------------------------------------------
                     OLD IMAGES
                    ---------------------------------------------------*/

                    ImagePath = x.ImagePath,

                    MobileImagePath = x.MobileImagePath,

                    /*--------------------------------------------------
                     NEW IMAGES
                    ---------------------------------------------------*/

                    BackgroundImagePath = x.BackgroundImagePath,

                    LeaderImagePath = x.LeaderImagePath,

                    /*--------------------------------------------------
                     BUTTONS
                    ---------------------------------------------------*/

                    ButtonText = x.ButtonText,

                    ButtonUrl = x.ButtonUrl,

                    ButtonText2 = x.ButtonText2,

                    ButtonUrl2 = x.ButtonUrl2,

                    ShowButtons = x.ShowButtons,

                    /*--------------------------------------------------
                     DISPLAY
                    ---------------------------------------------------*/

                    DisplayOrder = x.DisplayOrder,

                    IsActive = x.IsActive,

                    TemplateType = x.TemplateType,

                    BackgroundColor = x.BackgroundColor,

                    HeroHeightCss = "440px",

                    LeaderImagePosition = x.LeaderImagePosition,

                    ShowLeaderImage = x.ShowLeaderImage,

                    ShowOverlay = x.ShowOverlay,

                    /*--------------------------------------------------
                     ADVANCED
                    ---------------------------------------------------*/

                    OverlayType = x.OverlayType,

                    SliderTransition = x.SliderTransition,

                    TitleAnimation = x.TitleAnimation,

                    SubTitleAnimation = x.SubTitleAnimation,

                    DescriptionAnimation = x.DescriptionAnimation,

                    ButtonAnimation = x.ButtonAnimation,

                    TextAlignment = x.TextAlignment,

                    OverlayOpacity = x.OverlayOpacity
                })
                .ToList()
                .Select(AttachVideoFields)
                .ToList();
        }

        #endregion
        #region Get By Id

        public HeroSliderVM GetById(int id)
        {
            EnsureVideoColumns();

            HeroSlider entity = _db.HeroSliders
                                   .FirstOrDefault(x => x.HeroSliderId == id);

            if (entity == null)
            {
                return null;
            }

            return AttachVideoFields(new HeroSliderVM
            {
                HeroSliderId = entity.HeroSliderId,

                /*--------------------------------------------------
                 CONTENT
                ---------------------------------------------------*/

                Title = entity.Title,

                SubTitle = entity.SubTitle,

                Description = entity.Description,

                /*--------------------------------------------------
                 OLD IMAGES
                ---------------------------------------------------*/

                ImagePath = entity.ImagePath,

                MobileImagePath = entity.MobileImagePath,

                /*--------------------------------------------------
                 NEW IMAGES
                ---------------------------------------------------*/

                BackgroundImagePath = entity.BackgroundImagePath,

                LeaderImagePath = entity.LeaderImagePath,

                /*--------------------------------------------------
                 BUTTONS
                ---------------------------------------------------*/

                ButtonText = entity.ButtonText,

                ButtonUrl = entity.ButtonUrl,

                ButtonText2 = entity.ButtonText2,

                ButtonUrl2 = entity.ButtonUrl2,

                ShowButtons = entity.ShowButtons,

                /*--------------------------------------------------
                 DISPLAY
                ---------------------------------------------------*/

                DisplayOrder = entity.DisplayOrder,

                IsActive = entity.IsActive,

                TemplateType = entity.TemplateType,

                BackgroundColor = entity.BackgroundColor,

                HeroHeightCss = "440px",

                LeaderImagePosition = entity.LeaderImagePosition,

                ShowLeaderImage = entity.ShowLeaderImage,

                ShowOverlay = entity.ShowOverlay,

                /*--------------------------------------------------
                 ADVANCED
                ---------------------------------------------------*/

                OverlayType = entity.OverlayType,

                SliderTransition = entity.SliderTransition,

                TitleAnimation = entity.TitleAnimation,

                SubTitleAnimation = entity.SubTitleAnimation,

                DescriptionAnimation = entity.DescriptionAnimation,

                ButtonAnimation = entity.ButtonAnimation,

                TextAlignment = entity.TextAlignment,

                OverlayOpacity = entity.OverlayOpacity
            });
        }

        #endregion
        #region Create

        public void Create(
            HeroSliderVM model,
            HttpServerUtilityBase server)
        {
            EnsureVideoColumns();

            HeroSlider entity =
                new HeroSlider();

            MapEntity(
                entity,
                model,
                server);

            entity.CreatedDate =
                DateTime.Now;

            entity.CreatedBy = 1;

            _db.HeroSliders.Add(entity);

            try
            {
                _db.SaveChanges();

                model.HeroSliderId = entity.HeroSliderId;

                SaveVideoFields(
                    model,
                    server);
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var eve in ex.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        throw new Exception(
                            ve.PropertyName +
                            " : " +
                            ve.ErrorMessage);
                    }
                }

                throw;
            }
        }

        #endregion

        #region Update

        public void Update(
            HeroSliderVM model,
            HttpServerUtilityBase server)
        {
            EnsureVideoColumns();

            HeroSlider entity =
                _db.HeroSliders
                .FirstOrDefault(x =>
                    x.HeroSliderId ==
                    model.HeroSliderId);

            if (entity == null)
            {
                return;
            }

            MapEntity(
                entity,
                model,
                server);

            entity.ModifiedDate =
                DateTime.Now;

            entity.ModifiedBy = 1;

            _db.SaveChanges();

            SaveVideoFields(
                model,
                server);
        }

        #endregion

        #region Delete

        public void Delete(
    int id,
    HttpServerUtilityBase server)
        {
            EnsureVideoColumns();

            HeroSlider entity =
                _db.HeroSliders
                .FirstOrDefault(x =>
                    x.HeroSliderId == id);

            if (entity == null)
            {
                return;
            }

            DeletePhysicalFile(server,
     entity.ImagePath);

            DeletePhysicalFile(
    server,
    entity.MobileImagePath);

            DeletePhysicalFile(
       server,
       entity.BackgroundImagePath);

            DeletePhysicalFile(server,
                entity.LeaderImagePath);

            DeletePhysicalFile(server,
                GetVideoPath(id));

            _db.HeroSliders.Remove(entity);

            _db.SaveChanges();
        }

        #endregion        #region Map Entity

        private void MapEntity(
            HeroSlider entity,
            HeroSliderVM model,
            HttpServerUtilityBase server)
        {
            /*==========================================================
                CONTENT
            ==========================================================*/

            entity.Title = model.Title;

            entity.SubTitle = model.SubTitle;

            entity.Description = model.Description;

            /*==========================================================
                BUTTONS
            ==========================================================*/

            entity.ButtonText = model.ButtonText;

            entity.ButtonUrl = model.ButtonUrl;

            entity.ButtonText2 = model.ButtonText2;

            entity.ButtonUrl2 = model.ButtonUrl2;

            entity.ShowButtons = model.ShowButtons;

            /*==========================================================
                DISPLAY
            ==========================================================*/

            entity.DisplayOrder = model.DisplayOrder;

            entity.IsActive = model.IsActive;

            entity.TemplateType = model.TemplateType;

            entity.BackgroundColor = model.BackgroundColor;

            entity.LeaderImagePosition = model.LeaderImagePosition;

            entity.ShowLeaderImage = model.ShowLeaderImage;

            entity.ShowOverlay = model.ShowOverlay;

            /*==========================================================
                ADVANCED
            ==========================================================*/

            entity.OverlayType = model.OverlayType;

            entity.SliderTransition = model.SliderTransition;

            entity.TitleAnimation = model.TitleAnimation;

            entity.SubTitleAnimation = model.SubTitleAnimation;

            entity.DescriptionAnimation = model.DescriptionAnimation;

            entity.ButtonAnimation = model.ButtonAnimation;

            entity.TextAlignment = model.TextAlignment;

            entity.OverlayOpacity = model.OverlayOpacity;

            /*==========================================================
                IMAGE UPLOAD FOLDER
            ==========================================================*/

            string uploadFolder =
                server.MapPath("~/Uploads/HeroSlider/");

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            /*==========================================================
                HERO IMAGE
            ==========================================================*/

            if (model.ImageFile != null &&
                model.ImageFile.ContentLength > 0)
            {
                DeletePhysicalFile(
                    server,
                    entity.ImagePath);

                if (!String.Equals(
                    entity.ImagePath,
                    entity.BackgroundImagePath,
                    StringComparison.OrdinalIgnoreCase))
                {
                    DeletePhysicalFile(
                        server,
                        entity.BackgroundImagePath);
                }

                string heroImagePath =
                    UploadImage(
                        model.ImageFile,
                        uploadFolder,
                        "hero");

                entity.ImagePath = heroImagePath;
                entity.BackgroundImagePath = heroImagePath;
            }

            /*==========================================================
                MOBILE IMAGE
            ==========================================================*/

            if (model.MobileImageFile != null &&
                model.MobileImageFile.ContentLength > 0)
            {
                DeletePhysicalFile(
                    server,
                    entity.MobileImagePath);

                entity.MobileImagePath =
                    UploadImage(
                        model.MobileImageFile,
                        uploadFolder,
                        "mobile");
            }

            /*==========================================================
      BACKGROUND IMAGE
  ==========================================================*/

            if (model.BackgroundImageFile != null &&
                model.BackgroundImageFile.ContentLength > 0)
            {
                DeletePhysicalFile(
                    server,
                    entity.BackgroundImagePath);

                DeletePhysicalFile(
                    server,
                    entity.ImagePath);

                string backgroundImagePath =
                    UploadImage(
                        model.BackgroundImageFile,
                        uploadFolder,
                        "background");

                entity.BackgroundImagePath = backgroundImagePath;

                // Keep old ImagePath for backward compatibility
                entity.ImagePath = backgroundImagePath;
            }

            /*==========================================================
                LEADER IMAGE
            ==========================================================*/

            if (model.LeaderImageFile != null &&
                model.LeaderImageFile.ContentLength > 0)
            {
                DeletePhysicalFile(
                    server,
                    entity.LeaderImagePath);

                entity.LeaderImagePath =
                    UploadImage(
                        model.LeaderImageFile,
                        uploadFolder,
                        "leader");
            }
            /*==========================================================
    BACKWARD COMPATIBILITY
==========================================================*/

            if (String.IsNullOrWhiteSpace(entity.ImagePath))
            {
                entity.ImagePath = entity.BackgroundImagePath;
            }

            if (String.IsNullOrWhiteSpace(entity.BackgroundImagePath))
            {
                entity.BackgroundImagePath = entity.ImagePath;
            }
        }

        #region Video Fields

        private void EnsureVideoColumns()
        {
            _db.Database.ExecuteSqlCommand(@"
IF COL_LENGTH('dbo.HeroSlider', 'IsVideoSlide') IS NULL
    ALTER TABLE dbo.HeroSlider ADD IsVideoSlide BIT NOT NULL CONSTRAINT DF_HeroSlider_IsVideoSlide DEFAULT(0);

IF COL_LENGTH('dbo.HeroSlider', 'VideoSourceType') IS NULL
    ALTER TABLE dbo.HeroSlider ADD VideoSourceType NVARCHAR(30) NULL;

IF COL_LENGTH('dbo.HeroSlider', 'VideoUrl') IS NULL
    ALTER TABLE dbo.HeroSlider ADD VideoUrl NVARCHAR(500) NULL;

IF COL_LENGTH('dbo.HeroSlider', 'VideoPath') IS NULL
    ALTER TABLE dbo.HeroSlider ADD VideoPath NVARCHAR(500) NULL;

IF COL_LENGTH('dbo.HeroSlider', 'VideoAutoplay') IS NULL
    ALTER TABLE dbo.HeroSlider ADD VideoAutoplay BIT NOT NULL CONSTRAINT DF_HeroSlider_VideoAutoplay DEFAULT(1);

IF COL_LENGTH('dbo.HeroSlider', 'VideoMuted') IS NULL
    ALTER TABLE dbo.HeroSlider ADD VideoMuted BIT NOT NULL CONSTRAINT DF_HeroSlider_VideoMuted DEFAULT(1);

IF COL_LENGTH('dbo.HeroSlider', 'VideoLoop') IS NULL
    ALTER TABLE dbo.HeroSlider ADD VideoLoop BIT NOT NULL CONSTRAINT DF_HeroSlider_VideoLoop DEFAULT(0);

IF COL_LENGTH('dbo.HeroSlider', 'HeroHeightCss') IS NULL
    ALTER TABLE dbo.HeroSlider ADD HeroHeightCss NVARCHAR(50) NULL;");
        }

        private HeroSliderVM AttachVideoFields(HeroSliderVM model)
        {
            if (model == null || model.HeroSliderId <= 0)
            {
                return model;
            }

            try
            {
                HeroSliderVideoFields video =
                    _db.Database.SqlQuery<HeroSliderVideoFields>(
                        @"SELECT
                            CAST(ISNULL(IsVideoSlide, 0) AS bit) AS IsVideoSlide,
                            ISNULL(VideoSourceType, '') AS VideoSourceType,
                            ISNULL(VideoUrl, '') AS VideoUrl,
                            ISNULL(VideoPath, '') AS VideoPath,
                            CAST(ISNULL(VideoAutoplay, 1) AS bit) AS VideoAutoplay,
                            CAST(ISNULL(VideoMuted, 1) AS bit) AS VideoMuted,
                            CAST(ISNULL(VideoLoop, 1) AS bit) AS VideoLoop,
                            ISNULL(NULLIF(HeroHeightCss, ''), '440px') AS HeroHeightCss
                          FROM dbo.HeroSlider
                          WHERE HeroSliderId = @HeroSliderId",
                        new SqlParameter("@HeroSliderId", model.HeroSliderId))
                    .FirstOrDefault();

                if (video == null)
                {
                    return model;
                }

                model.IsVideoSlide = video.IsVideoSlide;
                model.VideoSourceType = video.VideoSourceType;
                model.VideoUrl = video.VideoUrl;
                model.VideoPath = video.VideoPath;
                model.VideoAutoplay = video.VideoAutoplay;
                model.VideoMuted = video.VideoMuted;
                model.VideoLoop = video.VideoLoop;
                model.HeroHeightCss = video.HeroHeightCss;
            }
            catch
            {
                model.VideoSourceType = "Image";
                model.VideoAutoplay = true;
                model.VideoMuted = true;
                model.VideoLoop = true;
                model.HeroHeightCss = "440px";
            }

            return model;
        }

        private void SaveVideoFields(
            HeroSliderVM model,
            HttpServerUtilityBase server)
        {
            if (model == null || model.HeroSliderId <= 0)
            {
                return;
            }

            string uploadFolder =
                server.MapPath("~/Uploads/HeroSlider/");

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            string videoPath = model.VideoPath;

            if (model.VideoFile != null &&
                model.VideoFile.ContentLength > 0)
            {
                DeletePhysicalFile(
                    server,
                    videoPath);

                videoPath =
                    UploadMedia(
                        model.VideoFile,
                        uploadFolder,
                        "video",
                        new[] { ".mp4", ".webm", ".mov" });
            }

            try
            {
                _db.Database.ExecuteSqlCommand(
                    @"UPDATE dbo.HeroSlider
                      SET IsVideoSlide = @IsVideoSlide,
                          VideoSourceType = @VideoSourceType,
                          VideoUrl = @VideoUrl,
                          VideoPath = @VideoPath,
                          VideoAutoplay = @VideoAutoplay,
                          VideoMuted = @VideoMuted,
                          VideoLoop = @VideoLoop,
                          HeroHeightCss = @HeroHeightCss
                      WHERE HeroSliderId = @HeroSliderId",
                    new SqlParameter("@IsVideoSlide", model.IsVideoSlide),
                    new SqlParameter("@VideoSourceType", (object)(model.VideoSourceType ?? "Image")),
                    new SqlParameter("@VideoUrl", (object)(model.VideoUrl ?? String.Empty)),
                    new SqlParameter("@VideoPath", (object)(videoPath ?? String.Empty)),
                    new SqlParameter("@VideoAutoplay", model.VideoAutoplay),
                    new SqlParameter("@VideoMuted", model.VideoMuted),
                    new SqlParameter("@VideoLoop", model.VideoLoop),
                    new SqlParameter("@HeroHeightCss", (object)(model.HeroHeightCss ?? "440px")),
                    new SqlParameter("@HeroSliderId", model.HeroSliderId));
            }
            catch
            {
                // Video columns are added by App_Data/HeroSliderVideoUpgrade.sql.
                // Keep image-only slider management working until the script is applied.
            }
        }

        private string GetVideoPath(int heroSliderId)
        {
            try
            {
                return _db.Database.SqlQuery<string>(
                    "SELECT ISNULL(VideoPath, '') FROM dbo.HeroSlider WHERE HeroSliderId = @HeroSliderId",
                    new SqlParameter("@HeroSliderId", heroSliderId))
                    .FirstOrDefault();
            }
            catch
            {
                return String.Empty;
            }
        }

        private class HeroSliderVideoFields
        {
            public bool IsVideoSlide { get; set; }
            public string VideoSourceType { get; set; }
            public string VideoUrl { get; set; }
            public string VideoPath { get; set; }
            public bool VideoAutoplay { get; set; }
            public bool VideoMuted { get; set; }
            public bool VideoLoop { get; set; }
            public string HeroHeightCss { get; set; }
        }

        #endregion


        #region Upload Image

        private string UploadImage(
            HttpPostedFileBase file,
            string uploadFolder,
            string prefix)
        {
            if (file == null ||
                file.ContentLength <= 0)
            {
                return null;
            }

            string extension =
                Path.GetExtension(
                    file.FileName);

            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

            if (String.IsNullOrWhiteSpace(extension) ||
                !allowedExtensions.Contains(extension.ToLowerInvariant()))
            {
                throw new InvalidOperationException("Only JPG, JPEG, PNG and WEBP images are allowed.");
            }

            if (file.ContentLength > 5 * 1024 * 1024)
            {
                throw new InvalidOperationException("Image size must be less than 5 MB.");
            }

            string fileName =
                prefix + "_" +
                DateTime.Now.ToString("yyyyMMddHHmmssfff") +
                extension;

            string physicalPath =
                Path.Combine(
                    uploadFolder,
                    fileName);

            file.SaveAs(
                physicalPath);

            return "/Uploads/HeroSlider/" +
                   fileName;
        }

        private string UploadMedia(
            HttpPostedFileBase file,
            string uploadFolder,
            string prefix,
            string[] allowedExtensions)
        {
            if (file == null ||
                file.ContentLength <= 0)
            {
                return null;
            }

            string extension =
                Path.GetExtension(file.FileName);

            if (String.IsNullOrWhiteSpace(extension) ||
                !allowedExtensions.Contains(extension.ToLower()))
            {
                throw new InvalidOperationException("Unsupported video format. Use MP4, WEBM or MOV.");
            }

            if (file.ContentLength > 50 * 1024 * 1024)
            {
                throw new InvalidOperationException("Video size must be less than 50 MB.");
            }

            string fileName =
                prefix + "_" +
                DateTime.Now.ToString("yyyyMMddHHmmssfff") +
                extension;

            string physicalPath =
                Path.Combine(
                    uploadFolder,
                    fileName);

            file.SaveAs(
                physicalPath);

            return "/Uploads/HeroSlider/" +
                   fileName;
        }

        #endregion

        #region Delete Physical File

        private void DeletePhysicalFile(
            HttpServerUtilityBase server,
            string filePath)
        {
            if (String.IsNullOrWhiteSpace(filePath))
            {
                return;
            }

            try
            {
                string physicalPath =
                    server.MapPath(filePath);

                if (File.Exists(
                    physicalPath))
                {
                    File.Delete(
                        physicalPath);
                }
            }
            catch
            {
                // Ignore delete errors
            }
        }

        #endregion

    }
}
