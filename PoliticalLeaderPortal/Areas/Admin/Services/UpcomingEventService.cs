using PoliticalLeaderPortal.Models;
using PoliticalLeaderPortal.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace PoliticalLeaderPortal.Services
{
    public class UpcomingEventService
    {
        private readonly PoliticalLeaderPortalDbEntities1 _db;

        public UpcomingEventService()
        {
            _db = new PoliticalLeaderPortalDbEntities1();
        }

        public List<UpcomingEventListVM> GetAll()
        {
            return _db.UpcomingEvents
                .OrderBy(x => x.DisplayOrder)
                .ThenByDescending(x => x.EventDate)
                .Select(x => new UpcomingEventListVM
                {
                    EventId = x.EventId,
                    Title = x.Title,
                    ShortDescription = x.ShortDescription,
                    EventDate = x.EventDate,
                    EventTime = x.EventTime,
                    EventLocation = x.EventLocation,
                    EventImagePath = x.EventImagePath,
                    IsActive = x.IsActive,
                    DisplayOrder = x.DisplayOrder
                })
                .ToList();
        }

        public UpcomingEventVM GetById(int id)
        {
            var entity = _db.UpcomingEvents
                .FirstOrDefault(x => x.EventId == id);

            if (entity == null)
                return null;

            return new UpcomingEventVM
            {
                EventId = entity.EventId,
                Title = entity.Title,
                ShortDescription = entity.ShortDescription,
                FullDescription = entity.FullDescription,
                EventDate = entity.EventDate,
                EventTime = entity.EventTime,
                EventLocation = entity.EventLocation,
                EventImagePath = entity.EventImagePath,
                IsActive = entity.IsActive,
                DisplayOrder = entity.DisplayOrder
            };
        }

        public UpcomingEventDetailsVM GetDetails(int id)
        {
            return _db.UpcomingEvents
                .Where(x => x.EventId == id && x.IsActive)
                .Select(x => new UpcomingEventDetailsVM
                {
                    EventId = x.EventId,
                    Title = x.Title,
                    ShortDescription = x.ShortDescription,
                    FullDescription = x.FullDescription,
                    EventDate = x.EventDate,
                    EventTime = x.EventTime,
                    EventLocation = x.EventLocation,
                    EventImagePath = x.EventImagePath
                })
                .FirstOrDefault();
        }

        public List<UpcomingEventListVM> GetUpcomingEvents(int count)
        {
            DateTime today = DateTime.Today;

            return _db.UpcomingEvents
                .Where(x => x.IsActive && x.EventDate >= today)
                .OrderBy(x => x.EventDate)
                .ThenBy(x => x.DisplayOrder)
                .Take(count)
                .Select(x => new UpcomingEventListVM
                {
                    EventId = x.EventId,
                    Title = x.Title,
                    ShortDescription = x.ShortDescription,
                    EventDate = x.EventDate,
                    EventTime = x.EventTime,
                    EventLocation = x.EventLocation,
                    EventImagePath = x.EventImagePath
                })
                .ToList();
        }

        public void Create(UpcomingEventVM model, string adminName)
        {
            var entity = new UpcomingEvent();

            entity.Title = model.Title;
            entity.ShortDescription = model.ShortDescription;
            entity.FullDescription = model.FullDescription;
            entity.EventDate = model.EventDate;
            entity.EventTime = model.EventTime;
            entity.EventLocation = model.EventLocation;
            entity.IsActive = model.IsActive;
            entity.DisplayOrder = model.DisplayOrder;
            entity.CreatedBy = adminName;
            entity.CreatedDate = DateTime.Now;

            if (model.EventImageFile != null)
            {
                entity.EventImagePath = SaveImage(model.EventImageFile);
            }
            else
            {
                entity.EventImagePath = model.EventImagePath;
            }
            _db.UpcomingEvents.Add(entity);
            _db.SaveChanges();
        }

        public void Update(UpcomingEventVM model, string adminName)
        {
            var entity = _db.UpcomingEvents
                .FirstOrDefault(x => x.EventId == model.EventId);

            if (entity == null)
                return;

            entity.Title = model.Title;
            entity.ShortDescription = model.ShortDescription;
            entity.FullDescription = model.FullDescription;
            entity.EventDate = model.EventDate;
            entity.EventTime = model.EventTime;
            entity.EventLocation = model.EventLocation;
            entity.IsActive = model.IsActive;
            entity.DisplayOrder = model.DisplayOrder;
            entity.UpdatedBy = adminName;
            entity.UpdatedDate = DateTime.Now;

            if (model.EventImageFile != null)
            {
                entity.EventImagePath = SaveImage(model.EventImageFile);
            }

            _db.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _db.UpcomingEvents
                .FirstOrDefault(x => x.EventId == id);

            if (entity == null)
                return;

            _db.UpcomingEvents.Remove(entity);
            _db.SaveChanges();
        }

        private string SaveImage(HttpPostedFileBase file)
        {
            if (file == null)
                return null;

            string fileName =
                Guid.NewGuid().ToString() +
                Path.GetExtension(file.FileName);

            string folder =
                HttpContext.Current.Server.MapPath("~/Uploads/Events/");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string fullPath = Path.Combine(folder, fileName);

            file.SaveAs(fullPath);

            return "/Uploads/Events/" + fileName;
        }
    }
}