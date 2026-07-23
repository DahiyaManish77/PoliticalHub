using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using PoliticalLeaderPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PoliticalLeaderPortal.Areas.Admin.Services
{
    public class StatisticService
    {
        private readonly PoliticalLeaderPortalDbEntities1 _db;


    public StatisticService()
        {
            _db =
                new PoliticalLeaderPortalDbEntities1();
        }

        public List<StatisticVM> GetAll()
        {
            return _db.HomePageStatistics
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new StatisticVM
                {
                    StatisticId = x.StatisticId,
                    Title = x.Title,
                    StatisticValue = x.StatisticValue,
                    IconClass = x.IconClass,
                    DisplayOrder = x.DisplayOrder,
                    IsActive = x.IsActive,
                    CreatedOn = x.CreatedOn,
                    UpdatedOn = x.UpdatedOn
                })
                .ToList();
        }

        public StatisticVM GetById(int id)
        {
            var entity =
                _db.HomePageStatistics
                .FirstOrDefault(x =>
                    x.StatisticId == id);

            if (entity == null)
            {
                return null;
            }

            return new StatisticVM
            {
                StatisticId = entity.StatisticId,
                Title = entity.Title,
                StatisticValue = entity.StatisticValue,
                IconClass = entity.IconClass,
                DisplayOrder = entity.DisplayOrder,
                IsActive = entity.IsActive,
                CreatedOn = entity.CreatedOn,
                UpdatedOn = entity.UpdatedOn
            };
        }

        public void Create(StatisticVM model)
        {
            HomePageStatistic entity =
                new HomePageStatistic();

            entity.Title =
                model.Title;

            entity.StatisticValue =
                model.StatisticValue;

            entity.IconClass =
                model.IconClass;

            entity.DisplayOrder =
                model.DisplayOrder;

            entity.IsActive =
                model.IsActive;

            entity.CreatedOn =
                DateTime.Now;

            _db.HomePageStatistics.Add(entity);

            _db.SaveChanges();
        }

        public void Update(StatisticVM model)
        {
            HomePageStatistic entity =
                _db.HomePageStatistics
                .FirstOrDefault(x =>
                    x.StatisticId ==
                    model.StatisticId);

            if (entity == null)
            {
                return;
            }

            entity.Title =
                model.Title;

            entity.StatisticValue =
                model.StatisticValue;

            entity.IconClass =
                model.IconClass;

            entity.DisplayOrder =
                model.DisplayOrder;

            entity.IsActive =
                model.IsActive;

            entity.UpdatedOn =
                DateTime.Now;

            _db.SaveChanges();
        }

        public void Delete(int id)
        {
            HomePageStatistic entity =
                _db.HomePageStatistics
                .FirstOrDefault(x =>
                    x.StatisticId == id);

            if (entity == null)
            {
                return;
            }

            _db.HomePageStatistics.Remove(entity);

            _db.SaveChanges();
        }
    }


}
