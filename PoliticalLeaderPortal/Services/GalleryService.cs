using PoliticalLeaderPortal.Models;
using PoliticalLeaderPortal.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace PoliticalLeaderPortal.Services
{
    public class GalleryService
    {
        private const int MaxGalleryImageBytes = 10 * 1024 * 1024;

        private readonly PoliticalLeaderPortalDbEntities1 _db;

        public GalleryService()
        {
            _db = new PoliticalLeaderPortalDbEntities1();
        }

        #region Gallery Category

        public List<GalleryCategoryListVM> GetAllCategories()
        {
            return _db.GalleryCategories
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new GalleryCategoryListVM
                {
                    GalleryCategoryId = x.GalleryCategoryId,
                    CategoryName = x.CategoryName,
                    CategoryDescription = x.CategoryDescription,
                    CoverImagePath = x.CoverImagePath,
                    DisplayOrder = x.DisplayOrder,
                    IsActive = x.IsActive,
                    TotalImages = x.GalleryImages.Count()
                })
                .ToList();
        }

        public GalleryCategoryVM GetCategoryById(int id)
        {
            var entity = _db.GalleryCategories
                .FirstOrDefault(x => x.GalleryCategoryId == id);

            if (entity == null)
                return null;

            return new GalleryCategoryVM
            {
                GalleryCategoryId = entity.GalleryCategoryId,
                CategoryName = entity.CategoryName,
                CategoryDescription = entity.CategoryDescription,
                CoverImagePath = entity.CoverImagePath,
                DisplayOrder = entity.DisplayOrder,
                IsActive = entity.IsActive,
                CreatedDate = entity.CreatedDate,
                UpdatedDate = entity.UpdatedDate
            };
        }

        public void CreateCategory(GalleryCategoryVM model)
        {
            var entity = new GalleryCategory();

            entity.CategoryName = model.CategoryName;
            entity.CategoryDescription = model.CategoryDescription;
            entity.DisplayOrder = model.DisplayOrder;
            entity.IsActive = model.IsActive;
            entity.CreatedDate = DateTime.Now;

            if (model.CoverImageFile != null)
            {
                entity.CoverImagePath = SaveCategoryImage(model.CoverImageFile);
            }

            _db.GalleryCategories.Add(entity);
            _db.SaveChanges();
        }

        public void UpdateCategory(GalleryCategoryVM model)
        {
            var entity = _db.GalleryCategories
                .FirstOrDefault(x => x.GalleryCategoryId == model.GalleryCategoryId);

            if (entity == null)
                return;

            entity.CategoryName = model.CategoryName;
            entity.CategoryDescription = model.CategoryDescription;
            entity.DisplayOrder = model.DisplayOrder;
            entity.IsActive = model.IsActive;
            entity.UpdatedDate = DateTime.Now;

            if (model.CoverImageFile != null)
            {
                entity.CoverImagePath = SaveCategoryImage(model.CoverImageFile);
            }

            _db.SaveChanges();
        }

        public void DeleteCategory(int id)
        {
            var entity = _db.GalleryCategories
                .FirstOrDefault(x => x.GalleryCategoryId == id);

            if (entity == null)
                return;

            _db.GalleryCategories.Remove(entity);
            _db.SaveChanges();
        }

        #endregion

        #region Gallery Images

        public List<GalleryImageListVM> GetAllImages()
        {
            return _db.GalleryImages
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new GalleryImageListVM
                {
                    GalleryImageId = x.GalleryImageId,
                    GalleryCategoryId = x.GalleryCategoryId,
                    CategoryName = x.GalleryCategory.CategoryName,
                    ImageTitle = x.ImageTitle,
                    ImageCaption = x.ImageCaption,
                    ImagePath = x.ImagePath,
                    DisplayOrder = x.DisplayOrder,
                    IsActive = x.IsActive
                })
                .ToList();
        }

        public GalleryImageVM GetImageById(int id)
        {
            var entity = _db.GalleryImages
                .FirstOrDefault(x => x.GalleryImageId == id);

            if (entity == null)
                return null;

            return new GalleryImageVM
            {
                GalleryImageId = entity.GalleryImageId,
                GalleryCategoryId = entity.GalleryCategoryId,
                ImageTitle = entity.ImageTitle,
                ImageCaption = entity.ImageCaption,
                ImagePath = entity.ImagePath,
                DisplayOrder = entity.DisplayOrder,
                IsActive = entity.IsActive,
                CreatedDate = entity.CreatedDate,
                UpdatedDate = entity.UpdatedDate,

                Categories = GetCategoryDropdown()
            };
        }

        public void CreateImages(GalleryImageVM model)
        {
            if (model.ImageFiles == null || !model.ImageFiles.Any())
                return;

            foreach (var file in model.ImageFiles)
            {
                if (file == null)
                    continue;

                var entity = new GalleryImage();

                entity.GalleryCategoryId = model.GalleryCategoryId;
                entity.ImageTitle = model.ImageTitle;
                entity.ImageCaption = model.ImageCaption;
                entity.DisplayOrder = model.DisplayOrder;
                entity.IsActive = model.IsActive;
                entity.CreatedDate = DateTime.Now;
                entity.ImagePath = SaveGalleryImage(file);

                _db.GalleryImages.Add(entity);
            }

            _db.SaveChanges();
        }

        public void UpdateImage(GalleryImageVM model)
        {
            var entity = _db.GalleryImages
                .FirstOrDefault(x => x.GalleryImageId == model.GalleryImageId);

            if (entity == null)
                return;

            entity.GalleryCategoryId = model.GalleryCategoryId;
            entity.ImageTitle = model.ImageTitle;
            entity.ImageCaption = model.ImageCaption;
            entity.DisplayOrder = model.DisplayOrder;
            entity.IsActive = model.IsActive;
            entity.UpdatedDate = DateTime.Now;

            if (model.ImageFile != null)
            {
                entity.ImagePath = SaveGalleryImage(model.ImageFile);
            }

            _db.SaveChanges();
        }

        public void DeleteImage(int id)
        {
            var entity = _db.GalleryImages
                .FirstOrDefault(x => x.GalleryImageId == id);

            if (entity == null)
                return;

            _db.GalleryImages.Remove(entity);
            _db.SaveChanges();
        }

        #endregion

        #region Public Gallery

        public List<GalleryCategoryListVM> GetActiveCategories()
        {
            return _db.GalleryCategories
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new GalleryCategoryListVM
                {
                    GalleryCategoryId = x.GalleryCategoryId,
                    CategoryName = x.CategoryName,
                    CategoryDescription = x.CategoryDescription,
                    CoverImagePath = x.CoverImagePath,
                    TotalImages = x.GalleryImages.Count(y => y.IsActive)
                })
                .ToList();
        }

        public List<GalleryImageListVM> GetImagesByCategory(int categoryId)
        {
            return _db.GalleryImages
                .Where(x => x.GalleryCategoryId == categoryId && x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new GalleryImageListVM
                {
                    GalleryImageId = x.GalleryImageId,
                    GalleryCategoryId = x.GalleryCategoryId,
                    CategoryName = x.GalleryCategory.CategoryName,
                    ImageTitle = x.ImageTitle,
                    ImageCaption = x.ImageCaption,
                    ImagePath = x.ImagePath
                })
                .ToList();
        }

        public List<GalleryImageListVM> GetLatestImages(int count)
        {
            return _db.GalleryImages
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CreatedDate)
                .Take(count)
                .Select(x => new GalleryImageListVM
                {
                    GalleryImageId = x.GalleryImageId,
                    ImageTitle = x.ImageTitle,
                    ImageCaption = x.ImageCaption,
                    ImagePath = x.ImagePath,
                    CategoryName = x.GalleryCategory.CategoryName
                })
                .ToList();
        }

        #endregion

        #region Private Methods

        private string SaveCategoryImage(HttpPostedFileBase file)
        {
            if (file == null)
                return null;

            string fileName = Guid.NewGuid() +
                              Path.GetExtension(file.FileName);

            string folder = HttpContext.Current.Server.MapPath(
                "~/Uploads/Gallery/Categories/");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string fullPath = Path.Combine(folder, fileName);

            file.SaveAs(fullPath);

            return "/Uploads/Gallery/Categories/" + fileName;
        }

        private string SaveGalleryImage(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength <= 0)
                return null;

            if (file.ContentLength > MaxGalleryImageBytes)
                throw new InvalidOperationException("Each gallery image must be 10 MB or smaller.");

            if (file.ContentType == null || !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Only image files can be uploaded in the gallery.");

            string fileName = Guid.NewGuid() +
                              Path.GetExtension(file.FileName);

            string folder = HttpContext.Current.Server.MapPath(
                "~/Uploads/Gallery/Images/");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string fullPath = Path.Combine(folder, fileName);

            file.SaveAs(fullPath);

            return "/Uploads/Gallery/Images/" + fileName;
        }

        public List<System.Web.Mvc.SelectListItem> GetCategoryDropdown()
        {
            return _db.GalleryCategories
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new System.Web.Mvc.SelectListItem
                {
                    Value = x.GalleryCategoryId.ToString(),
                    Text = x.CategoryName
                })
                .ToList();
        }
        public List<PublicGalleryImageVM> GetHomepageGallery(int count)
        {
            return _db.GalleryImages
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CreatedDate)
                .Take(count)
                .Select(x => new PublicGalleryImageVM
                {
                    GalleryImageId = x.GalleryImageId,
                    ImageTitle = x.ImageTitle,
                    ImageCaption = x.ImageCaption,
                    ImagePath = x.ImagePath
                })
                .ToList();
        }
        public List<PublicGalleryImageVM> GetPublicGalleryImages()
        {
            return _db.GalleryImages
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CreatedDate)
                .ThenBy(x => x.DisplayOrder)
                .Select(x => new PublicGalleryImageVM
                {
                    GalleryImageId = x.GalleryImageId,
                    ImageTitle = x.ImageTitle,
                    ImageCaption = x.ImageCaption,
                    ImagePath = x.ImagePath
                })
                .ToList();
        }
        public PublicGalleryAlbumVM GetPublicGalleryAlbum(int categoryId)
        {
            var category = _db.GalleryCategories
                .FirstOrDefault(x =>
                    x.GalleryCategoryId == categoryId &&
                    x.IsActive);

            if (category == null)
                return null;

            var model = new PublicGalleryAlbumVM();

            model.GalleryCategoryId = category.GalleryCategoryId;
            model.CategoryName = category.CategoryName;
            model.CategoryDescription = category.CategoryDescription;
            model.CoverImagePath = category.CoverImagePath;

            model.Images = _db.GalleryImages
                .Where(x =>
                    x.GalleryCategoryId == categoryId &&
                    x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new PublicGalleryImageVM
                {
                    GalleryImageId = x.GalleryImageId,
                    ImageTitle = x.ImageTitle,
                    ImageCaption = x.ImageCaption,
                    ImagePath = x.ImagePath
                })
                .ToList();

            return model;
        }
        public List<PublicGalleryCategoryVM> GetPublicGalleryCategories()
        {
            return _db.GalleryCategories
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new PublicGalleryCategoryVM
                {
                    GalleryCategoryId = x.GalleryCategoryId,
                    CategoryName = x.CategoryName,
                    CategoryDescription = x.CategoryDescription,
                    CoverImagePath = x.CoverImagePath,
                    TotalImages = x.GalleryImages.Count(y => y.IsActive)
                })
                .ToList();
        }

        #endregion
    }
}
