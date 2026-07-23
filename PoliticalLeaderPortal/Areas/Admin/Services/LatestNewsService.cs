using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using PoliticalLeaderPortal.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace PoliticalLeaderPortal.Areas.Admin.Services
{
    public class LatestNewsService
    {
        private readonly PoliticalLeaderPortalDbEntities1 _db;


    public LatestNewsService()
        {
            _db =
                new PoliticalLeaderPortalDbEntities1();
        }

        public List<LatestNewsVM> GetAll()
        {
            return _db.LatestNews
                .OrderByDescending(x => x.PublishDate)
                .Select(x => new LatestNewsVM
                {
                    NewsId = x.NewsId,
                    Title = x.Title,
                    ShortDescription = x.ShortDescription,
                    FullDescription = x.FullDescription,
                    ImagePath = x.ImagePath,
                    PublishDate = x.PublishDate,
                    DisplayOrder = x.DisplayOrder,
                    IsFeatured = x.IsFeatured,
                    IsActive = x.IsActive,
                    CreatedOn = x.CreatedOn,
                    UpdatedOn = x.UpdatedOn
                })
                .ToList();
        }

        public LatestNewsVM GetById(int id)
        {
            var entity =
                _db.LatestNews
                .FirstOrDefault(x =>
                    x.NewsId == id);

            if (entity == null)
            {
                return null;
            }

            return new LatestNewsVM
            {
                NewsId = entity.NewsId,
                Title = entity.Title,
                ShortDescription = entity.ShortDescription,
                FullDescription = entity.FullDescription,
                ImagePath = entity.ImagePath,
                PublishDate = entity.PublishDate,
                DisplayOrder = entity.DisplayOrder,
                IsFeatured = entity.IsFeatured,
                IsActive = entity.IsActive,
                CreatedOn = entity.CreatedOn,
                UpdatedOn = entity.UpdatedOn
            };
        }

        public void Create(
            LatestNewsVM model,
            HttpServerUtilityBase server)
        {
            LatestNew entity =
                new LatestNew();

            MapEntity(
                entity,
                model,
                server);

            entity.CreatedOn =
                DateTime.Now;

            _db.LatestNews.Add(entity);

            _db.SaveChanges();
        }

        public void Update(
            LatestNewsVM model,
            HttpServerUtilityBase server)
        {
            LatestNew entity =
                _db.LatestNews
                .FirstOrDefault(x =>
                    x.NewsId ==
                    model.NewsId);

            if (entity == null)
            {
                return;
            }

            MapEntity(
                entity,
                model,
                server);

            entity.UpdatedOn =
                DateTime.Now;

            _db.SaveChanges();
        }

        public void Delete(int id)
        {
            LatestNew entity =
                _db.LatestNews
                .FirstOrDefault(x =>
                    x.NewsId == id);

            if (entity == null)
            {
                return;
            }

            _db.LatestNews.Remove(entity);

            _db.SaveChanges();
        }

        private void MapEntity(
            LatestNew entity,
            LatestNewsVM model,
            HttpServerUtilityBase server)
        {
            entity.Title =
                model.Title;

            entity.ShortDescription =
                model.ShortDescription;

            entity.FullDescription =
                model.FullDescription;

            entity.PublishDate =
                model.PublishDate;

            entity.DisplayOrder =
                model.DisplayOrder;

            entity.IsFeatured =
                model.IsFeatured;

            entity.IsActive =
                model.IsActive;

            string uploadFolder =
                server.MapPath(
                    "~/Uploads/LatestNews/");

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(
                    uploadFolder);
            }

            if (model.ImageFile != null &&
                model.ImageFile.ContentLength > 0)
            {
                string fileName =
                    DateTime.Now.Ticks +
                    Path.GetExtension(
                        model.ImageFile.FileName);

                string filePath =
                    Path.Combine(
                        uploadFolder,
                        fileName);

                model.ImageFile.SaveAs(
                    filePath);

                entity.ImagePath =
                    "/Uploads/LatestNews/" +
                    fileName;
            }
        }
    }


}
