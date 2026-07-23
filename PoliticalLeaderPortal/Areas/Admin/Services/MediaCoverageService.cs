using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using PoliticalLeaderPortal.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace PoliticalLeaderPortal.Areas.Admin.Services
{
    public class MediaCoverageService
    {
        private readonly PoliticalLeaderPortalDbEntities1 _db;

        public MediaCoverageService()
        {
            _db = new PoliticalLeaderPortalDbEntities1();
        }

        public List<MediaCoverageVM> GetAll()
        {
            return _db.MediaCoverages
                .OrderByDescending(x => x.CoverageDate)
                .ThenBy(x => x.DisplayOrder)
                .Select(x => new MediaCoverageVM
                {
                    MediaCoverageId = x.MediaCoverageId,
                    SourceName = x.SourceName,
                    SourceLogoPath = x.SourceLogoPath,
                    Title = x.Title,
                    ShortDescription = x.ShortDescription,
                    CoverImagePath = x.CoverImagePath,
                    CoverageDate = x.CoverageDate,
                    IsFeatured = x.IsFeatured,
                    IsActive = x.IsActive,
                    DisplayOrder = x.DisplayOrder,
                    ViewCount = x.ViewCount
                })
                .ToList();
        }

        public MediaCoverageVM GetById(int id)
        {
            var entity = _db.MediaCoverages
                .FirstOrDefault(x => x.MediaCoverageId == id);

            if (entity == null)
                return null;

            return new MediaCoverageVM
            {
                MediaCoverageId = entity.MediaCoverageId,
                SourceName = entity.SourceName,
                SourceLogoPath = entity.SourceLogoPath,
                Title = entity.Title,
                ShortDescription = entity.ShortDescription,
                FullDescription = entity.FullDescription,
                CoverImagePath = entity.CoverImagePath,
                CoverageDate = entity.CoverageDate,
                ExternalUrl = entity.ExternalUrl,
                IsFeatured = entity.IsFeatured,
                DisplayOrder = entity.DisplayOrder,
                ViewCount = entity.ViewCount,
                IsActive = entity.IsActive
            };
        }

        public void Create(MediaCoverageVM model)
        {
            var entity = new MediaCoverage();

            entity.SourceName = model.SourceName;
            entity.Title = model.Title;
            entity.ShortDescription = model.ShortDescription;
            entity.FullDescription = model.FullDescription;
            entity.CoverageDate = model.CoverageDate;
            entity.ExternalUrl = model.ExternalUrl;
            entity.IsFeatured = model.IsFeatured;
            entity.DisplayOrder = model.DisplayOrder;
            entity.IsActive = model.IsActive;
            entity.ViewCount = 0;
            entity.CreatedDate = DateTime.Now;

            if (model.SourceLogoFile != null)
            {
                entity.SourceLogoPath =
                    SaveSourceLogo(model.SourceLogoFile);
            }

            if (model.CoverImageFile != null)
            {
                entity.CoverImagePath =
                    SaveCoverImage(model.CoverImageFile);
            }

            _db.MediaCoverages.Add(entity);

            _db.SaveChanges();
        }

        public void Update(MediaCoverageVM model)
        {
            var entity = _db.MediaCoverages
                .FirstOrDefault(x => x.MediaCoverageId == model.MediaCoverageId);

            if (entity == null)
                return;

            entity.SourceName = model.SourceName;
            entity.Title = model.Title;
            entity.ShortDescription = model.ShortDescription;
            entity.FullDescription = model.FullDescription;
            entity.CoverageDate = model.CoverageDate;
            entity.ExternalUrl = model.ExternalUrl;
            entity.IsFeatured = model.IsFeatured;
            entity.DisplayOrder = model.DisplayOrder;
            entity.IsActive = model.IsActive;

            if (model.SourceLogoFile != null)
            {
                entity.SourceLogoPath =
                    SaveSourceLogo(model.SourceLogoFile);
            }

            if (model.CoverImageFile != null)
            {
                entity.CoverImagePath =
                    SaveCoverImage(model.CoverImageFile);
            }

            _db.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _db.MediaCoverages
                .FirstOrDefault(x => x.MediaCoverageId == id);

            if (entity == null)
                return;

            _db.MediaCoverages.Remove(entity);

            _db.SaveChanges();
        }


        public List<MediaCoverageVM> GetActiveMediaCoverage()
        {
            return _db.MediaCoverages
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CoverageDate)
                .ThenBy(x => x.DisplayOrder)
                .Select(x => new MediaCoverageVM
                {
                    MediaCoverageId = x.MediaCoverageId,
                    SourceName = x.SourceName,
                    SourceLogoPath = x.SourceLogoPath,
                    Title = x.Title,
                    ShortDescription = x.ShortDescription,
                    CoverImagePath = x.CoverImagePath,
                    CoverageDate = x.CoverageDate,
                    IsFeatured = x.IsFeatured,
                    ViewCount = x.ViewCount
                })
                .ToList();
        }

        public MediaCoverageVM GetPublicDetails(int id)
        {
            return _db.MediaCoverages
                .Where(x =>
                    x.MediaCoverageId == id &&
                    x.IsActive)
                .Select(x => new MediaCoverageVM
                {
                    MediaCoverageId = x.MediaCoverageId,
                    SourceName = x.SourceName,
                    SourceLogoPath = x.SourceLogoPath,
                    Title = x.Title,
                    ShortDescription = x.ShortDescription,
                    FullDescription = x.FullDescription,
                    CoverImagePath = x.CoverImagePath,
                    CoverageDate = x.CoverageDate,
                    ExternalUrl = x.ExternalUrl,
                    IsFeatured = x.IsFeatured,
                    ViewCount = x.ViewCount
                })
                .FirstOrDefault();
        }

        public List<MediaCoverageVM> GetRelatedMediaCoverage(int id)
        {
            return _db.MediaCoverages
                .Where(x =>
                    x.IsActive &&
                    x.MediaCoverageId != id)
                .OrderByDescending(x => x.CoverageDate)
                .Take(4)
                .Select(x => new MediaCoverageVM
                {
                    MediaCoverageId = x.MediaCoverageId,
                    Title = x.Title,
                    CoverImagePath = x.CoverImagePath,
                    CoverageDate = x.CoverageDate,
                    SourceName = x.SourceName
                })
                .ToList();
        }
        // This method retrieves a list of media coverage items to be displayed on the home page. It filters for active items, orders them by featured status and coverage date, and limits the result to 6 items.
        public List<MediaCoverageVM> GetHomeMediaCoverage()
        {
            return _db.MediaCoverages
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.IsFeatured)
                .ThenByDescending(x => x.CoverageDate)
                .Take(6)
                .Select(x => new MediaCoverageVM
                {
                    MediaCoverageId = x.MediaCoverageId,
                    Title = x.Title,
                    SourceName = x.SourceName,
                    CoverImagePath = x.CoverImagePath,
                    CoverageDate = x.CoverageDate,
                    ShortDescription = x.ShortDescription
                })
                .ToList();
        }
        public void IncreaseViewCount(int id)
        {
            var entity =
                _db.MediaCoverages
                .FirstOrDefault(x =>
                    x.MediaCoverageId == id);

            if (entity == null)
                return;

            entity.ViewCount++;

            _db.SaveChanges();
        }


        // this is the end of public methods, now we will implement private methods for saving files
        private string SaveSourceLogo(HttpPostedFileBase file)
        {
            string fileName =
                Guid.NewGuid() +
                Path.GetExtension(file.FileName);

            string folder =
                HttpContext.Current.Server.MapPath(
                    "~/Uploads/MediaCoverage/SourceLogos/");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string path =
                Path.Combine(folder, fileName);

            file.SaveAs(path);

            return "/Uploads/MediaCoverage/SourceLogos/" + fileName;
        }

        private string SaveCoverImage(HttpPostedFileBase file)
        {
            string fileName =
                Guid.NewGuid() +
                Path.GetExtension(file.FileName);

            string folder =
                HttpContext.Current.Server.MapPath(
                    "~/Uploads/MediaCoverage/CoverImages/");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string path =
                Path.Combine(folder, fileName);

            file.SaveAs(path);

            return "/Uploads/MediaCoverage/CoverImages/" + fileName;
        }
    }
}