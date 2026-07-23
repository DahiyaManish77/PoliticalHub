using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using PoliticalLeaderPortal.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace PoliticalLeaderPortal.Areas.Admin.Services
{
    public class DownloadDocumentService
    {
        private readonly PoliticalLeaderPortalDbEntities1 _db;

        public DownloadDocumentService()
        {
            _db =
                new PoliticalLeaderPortalDbEntities1();
        }

        public List<DownloadDocumentVM> GetAll()
        {
            return _db.DownloadDocuments
                .OrderByDescending(x => x.PublishDate)
                .ThenBy(x => x.DisplayOrder)
                .ThenBy(x => x.Title)
                .Select(x => new DownloadDocumentVM
                {
                    DownloadDocumentId = x.DownloadDocumentId,
                    DownloadCategoryId = x.DownloadCategoryId,
                    CategoryName = x.DownloadCategory.CategoryName,
                    DocumentNumber = x.DocumentNumber,
                    Title = x.Title,
                    ShortDescription = x.ShortDescription,
                    FileName = x.FileName,
                    FilePath = x.FilePath,
                    FileExtension = x.FileExtension,
                    FileSizeKB = x.FileSizeKB,
                    VersionNo = x.VersionNo,
                    DownloadsCount = x.DownloadsCount,
                    DisplayOrder = x.DisplayOrder,
                    PublishDate = x.PublishDate,
                    IsFeatured = x.IsFeatured,
                    IsActive = x.IsActive,
                    CreatedDate = x.CreatedDate
                })
                .ToList();
        }

        public DownloadDocumentVM GetById(int id)
        {
            DownloadDocument entity =
                _db.DownloadDocuments
                .FirstOrDefault(x =>
                    x.DownloadDocumentId == id);

            if (entity == null)
            {
                return null;
            }

            return new DownloadDocumentVM
            {
                DownloadDocumentId = entity.DownloadDocumentId,
                DownloadCategoryId = entity.DownloadCategoryId,
                DocumentNumber = entity.DocumentNumber,
                Title = entity.Title,
                ShortDescription = entity.ShortDescription,
                FileName = entity.FileName,
                FilePath = entity.FilePath,
                FileExtension = entity.FileExtension,
                FileSizeKB = entity.FileSizeKB,
                VersionNo = entity.VersionNo,
                DownloadsCount = entity.DownloadsCount,
                DisplayOrder = entity.DisplayOrder,
                PublishDate = entity.PublishDate,
                IsFeatured = entity.IsFeatured,
                IsActive = entity.IsActive,
                CreatedDate = entity.CreatedDate
            };
        }
        public void Create(
    DownloadDocumentVM model,
    HttpServerUtilityBase server)
        {
            if (model.File == null ||
                model.File.ContentLength == 0)
            {
                throw new Exception(
                    "Please select a document.");
            }

            DownloadDocument entity =
                new DownloadDocument();

            MapEntity(
                entity,
                model,
                server);

            entity.CreatedDate =
                DateTime.Now;

            _db.DownloadDocuments.Add(entity);

            _db.SaveChanges();
        }

        public void Update(
            DownloadDocumentVM model,
            HttpServerUtilityBase server)
        {
            DownloadDocument entity =
                _db.DownloadDocuments
                .FirstOrDefault(x =>
                    x.DownloadDocumentId ==
                    model.DownloadDocumentId);

            if (entity == null)
            {
                return;
            }

            MapEntity(
                entity,
                model,
                server);

            _db.SaveChanges();
        }

        public bool Delete(
            int id,
            HttpServerUtilityBase server)
        {
            DownloadDocument entity =
                _db.DownloadDocuments
                .FirstOrDefault(x =>
                    x.DownloadDocumentId == id);

            if (entity == null)
            {
                return false;
            }

            if (!String.IsNullOrWhiteSpace(entity.FilePath))
            {
                string physicalPath =
                    server.MapPath(entity.FilePath);

                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                }
            }

            _db.DownloadDocuments.Remove(entity);

            _db.SaveChanges();

            return true;
        }

        public void IncrementDownload(int id)
        {
            DownloadDocument entity =
                _db.DownloadDocuments
                .FirstOrDefault(x =>
                    x.DownloadDocumentId == id);

            if (entity == null)
            {
                return;
            }

            entity.DownloadsCount =
                entity.DownloadsCount + 1;

            _db.SaveChanges();
        }
        public bool IsDuplicateDocumentNumber(
    string documentNumber,
    int documentId = 0)
        {
            if (String.IsNullOrWhiteSpace(documentNumber))
            {
                return false;
            }

            documentNumber =
                documentNumber.Trim();

            return _db.DownloadDocuments.Any(x =>
                x.DocumentNumber == documentNumber &&
                x.DownloadDocumentId != documentId);
        }

        public List<DownloadDocumentVM> GetActive()
        {
            EnsureOfficialBjpDownloads();

            return _db.DownloadDocuments
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.PublishDate)
                .ThenBy(x => x.DisplayOrder)
                .Select(x => new DownloadDocumentVM
                {
                    DownloadDocumentId = x.DownloadDocumentId,
                    DownloadCategoryId = x.DownloadCategoryId,
                    CategoryName = x.DownloadCategory.CategoryName,
                    DocumentNumber = x.DocumentNumber,
                    Title = x.Title,
                    ShortDescription = x.ShortDescription,
                    FileName = x.FileName,
                    FilePath = x.FilePath,
                    FileExtension = x.FileExtension,
                    FileSizeKB = x.FileSizeKB,
                    VersionNo = x.VersionNo,
                    DownloadsCount = x.DownloadsCount,
                    DisplayOrder = x.DisplayOrder,
                    PublishDate = x.PublishDate,
                    IsFeatured = x.IsFeatured,
                    IsActive = x.IsActive,
                    CreatedDate = x.CreatedDate
                })
                .ToList();
        }

        public List<DownloadDocumentVM> GetFeatured(
            int count)
        {
            return _db.DownloadDocuments
                .Where(x =>
                    x.IsActive &&
                    x.IsFeatured)
                .OrderByDescending(x => x.PublishDate)
                .Take(count)
                .Select(x => new DownloadDocumentVM
                {
                    DownloadDocumentId = x.DownloadDocumentId,
                    DownloadCategoryId = x.DownloadCategoryId,
                    CategoryName = x.DownloadCategory.CategoryName,
                    DocumentNumber = x.DocumentNumber,
                    Title = x.Title,
                    ShortDescription = x.ShortDescription,
                    FileName = x.FileName,
                    FilePath = x.FilePath,
                    FileExtension = x.FileExtension,
                    FileSizeKB = x.FileSizeKB,
                    VersionNo = x.VersionNo,
                    DownloadsCount = x.DownloadsCount,
                    DisplayOrder = x.DisplayOrder,
                    PublishDate = x.PublishDate,
                    IsFeatured = x.IsFeatured,
                    IsActive = x.IsActive,
                    CreatedDate = x.CreatedDate
                })
                .ToList();
        }

        public List<DownloadDocumentVM> GetLatest(
            int count)
        {
            return _db.DownloadDocuments
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.PublishDate)
                .Take(count)
                .Select(x => new DownloadDocumentVM
                {
                    DownloadDocumentId = x.DownloadDocumentId,
                    DownloadCategoryId = x.DownloadCategoryId,
                    CategoryName = x.DownloadCategory.CategoryName,
                    DocumentNumber = x.DocumentNumber,
                    Title = x.Title,
                    ShortDescription = x.ShortDescription,
                    FileName = x.FileName,
                    FilePath = x.FilePath,
                    FileExtension = x.FileExtension,
                    FileSizeKB = x.FileSizeKB,
                    VersionNo = x.VersionNo,
                    DownloadsCount = x.DownloadsCount,
                    DisplayOrder = x.DisplayOrder,
                    PublishDate = x.PublishDate,
                    IsFeatured = x.IsFeatured,
                    IsActive = x.IsActive,
                    CreatedDate = x.CreatedDate
                })
                .ToList();
        }

        public List<DownloadDocumentVM> GetByCategory(
            int categoryId)
        {
            return _db.DownloadDocuments
                .Where(x =>
                    x.DownloadCategoryId == categoryId &&
                    x.IsActive)
                .OrderByDescending(x => x.PublishDate)
                .ThenBy(x => x.DisplayOrder)
                .Select(x => new DownloadDocumentVM
                {
                    DownloadDocumentId = x.DownloadDocumentId,
                    DownloadCategoryId = x.DownloadCategoryId,
                    CategoryName = x.DownloadCategory.CategoryName,
                    DocumentNumber = x.DocumentNumber,
                    Title = x.Title,
                    ShortDescription = x.ShortDescription,
                    FileName = x.FileName,
                    FilePath = x.FilePath,
                    FileExtension = x.FileExtension,
                    FileSizeKB = x.FileSizeKB,
                    VersionNo = x.VersionNo,
                    DownloadsCount = x.DownloadsCount,
                    DisplayOrder = x.DisplayOrder,
                    PublishDate = x.PublishDate,
                    IsFeatured = x.IsFeatured,
                    IsActive = x.IsActive,
                    CreatedDate = x.CreatedDate
                })
                .ToList();
        }

        private void EnsureOfficialBjpDownloads()
        {
            const string categoryName = "BJP Official Documents";
            DownloadCategory category = _db.DownloadCategories.FirstOrDefault(x => x.CategoryName == categoryName);
            if (category == null)
            {
                category = new DownloadCategory
                {
                    CategoryName = categoryName,
                    DisplayOrder = 90,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };
                _db.DownloadCategories.Add(category);
                _db.SaveChanges();
            }

            AddOfficialDownload(category.DownloadCategoryId, "BJP-MANIFESTO-2019-EN", "BJP Election Manifesto 2019 - English", "Official BJP e-Library PDF link.", "BJP-Election-english-2019.pdf", "https://library.bjp.org/jspui/bitstream/123456789/2988/1/BJP-Election-english-2019.pdf", "PDF", 8089, 1);
            AddOfficialDownload(category.DownloadCategoryId, "BJP-MANIFESTO-2019-HI", "BJP Election Manifesto 2019 - Hindi", "Official BJP e-Library PDF link.", "BJP-Election-hindi-2019.pdf", "https://library.bjp.org/jspui/bitstream/123456789/2988/2/BJP-Election-hindi-2019.pdf", "PDF", 4454, 2);
            AddOfficialDownload(category.DownloadCategoryId, "BJP-MANIFESTO-COLLECTION", "BJP Manifesto Collection", "Official BJP e-Library manifesto collection page.", "BJP Manifesto Collection", "https://library.bjp.org/jspui/handle/123456789/260", "LINK", 0, 3);
        }

        private void AddOfficialDownload(int categoryId, string documentNumber, string title, string description, string fileName, string filePath, string extension, decimal fileSizeKb, int displayOrder)
        {
            DownloadDocument doc = _db.DownloadDocuments.FirstOrDefault(x => x.DocumentNumber == documentNumber);
            if (doc == null)
            {
                doc = new DownloadDocument { CreatedDate = DateTime.Now, DownloadsCount = 0 };
                _db.DownloadDocuments.Add(doc);
            }

            doc.DownloadCategoryId = categoryId;
            doc.DocumentNumber = documentNumber;
            doc.Title = title;
            doc.ShortDescription = description;
            doc.FileName = fileName;
            doc.FilePath = filePath;
            doc.FileExtension = extension;
            doc.FileSizeKB = fileSizeKb;
            doc.VersionNo = "Official";
            doc.DisplayOrder = displayOrder;
            doc.PublishDate = new DateTime(2019, 4, 8);
            doc.IsFeatured = true;
            doc.IsActive = true;

            _db.SaveChanges();
        }
        private void MapEntity(
    DownloadDocument entity,
    DownloadDocumentVM model,
    HttpServerUtilityBase server)
        {
            entity.DownloadCategoryId =
                model.DownloadCategoryId;

            entity.DocumentNumber =
                String.IsNullOrWhiteSpace(model.DocumentNumber)
                ? null
                : model.DocumentNumber.Trim();

            entity.Title =
                String.IsNullOrWhiteSpace(model.Title)
                ? null
                : model.Title.Trim();

            entity.ShortDescription =
                String.IsNullOrWhiteSpace(model.ShortDescription)
                ? null
                : model.ShortDescription.Trim();

            entity.VersionNo =
                String.IsNullOrWhiteSpace(model.VersionNo)
                ? null
                : model.VersionNo.Trim();

            entity.DisplayOrder =
                model.DisplayOrder;

            entity.PublishDate =
                model.PublishDate;

            entity.IsFeatured =
                model.IsFeatured;

            entity.IsActive =
                model.IsActive;

            string uploadFolder =
                server.MapPath(
                    "~/Uploads/Documents/");

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(
                    uploadFolder);
            }

            if (model.File == null ||
                model.File.ContentLength == 0)
            {
                return;
            }

            string extension =
                Path.GetExtension(
                    model.File.FileName)
                    .ToLower();

            string[] allowedExtensions =
            {
                ".pdf",
                ".doc",
                ".docx",
                ".xls",
                ".xlsx",
                ".ppt",
                ".pptx",
                ".txt",
                ".rtf",
                ".zip",
                ".rar"
            };

            if (!allowedExtensions.Contains(extension))
            {
                throw new Exception(
                    "Only PDF, Word, Excel, PowerPoint, Text, ZIP and RAR files are allowed.");
            }

            if (!String.IsNullOrWhiteSpace(entity.FilePath))
            {
                string oldFile =
                    server.MapPath(entity.FilePath);

                if (File.Exists(oldFile))
                {
                    File.Delete(oldFile);
                }
            }

            string newFileName =
                DateTime.Now.ToString("yyyyMMddHHmmssfff") +
                extension;

            string physicalPath =
                Path.Combine(
                    uploadFolder,
                    newFileName);

            model.File.SaveAs(
                physicalPath);

            entity.FileName =
                Path.GetFileName(
                    model.File.FileName);

            entity.FilePath =
                "/Uploads/Documents/" +
                newFileName;

            entity.FileExtension =
                extension.Replace(".", "")
                         .ToUpper();

            entity.FileSizeKB =
                Math.Round(
                    (decimal)model.File.ContentLength / 1024,
                    2);
        }
    }
}

