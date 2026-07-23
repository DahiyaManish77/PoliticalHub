using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using PoliticalLeaderPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Services
{
    public class DownloadCategoryService
    {
        private readonly PoliticalLeaderPortalDbEntities1 _db;

        public DownloadCategoryService()
        {
            _db = new PoliticalLeaderPortalDbEntities1();
        }

        #region Get All

        public List<DownloadCategoryVM> GetAll()
        {
            return _db.DownloadCategories
                      .OrderBy(x => x.DisplayOrder)
                      .ThenBy(x => x.CategoryName)
                      .Select(x => new DownloadCategoryVM
                      {
                          DownloadCategoryId = x.DownloadCategoryId,
                          CategoryName = x.CategoryName,
                          CategoryDescription = x.CategoryDescription,
                          DisplayOrder = x.DisplayOrder,
                          IsActive = x.IsActive
                      }).ToList();
        }

        #endregion

        #region Get By Id

        public DownloadCategoryVM GetById(int id)
        {
            return _db.DownloadCategories
                      .Where(x => x.DownloadCategoryId == id)
                      .Select(x => new DownloadCategoryVM
                      {
                          DownloadCategoryId = x.DownloadCategoryId,
                          CategoryName = x.CategoryName,
                          CategoryDescription = x.CategoryDescription,
                          DisplayOrder = x.DisplayOrder,
                          IsActive = x.IsActive
                      }).FirstOrDefault();
        }

        #endregion

        #region Insert

        public void Insert(DownloadCategoryVM vm)
        {
            DownloadCategory entity = new DownloadCategory();

            entity.CategoryName = vm.CategoryName;
            entity.CategoryDescription = vm.CategoryDescription;
            entity.DisplayOrder = vm.DisplayOrder;
            entity.IsActive = vm.IsActive;
            entity.CreatedDate = DateTime.Now;

            _db.DownloadCategories.Add(entity);
            _db.SaveChanges();
        }

        #endregion

        #region Update

        public void Update(DownloadCategoryVM vm)
        {
            DownloadCategory entity = _db.DownloadCategories
                                         .FirstOrDefault(x => x.DownloadCategoryId == vm.DownloadCategoryId);

            if (entity != null)
            {
                entity.CategoryName = vm.CategoryName;
                entity.CategoryDescription = vm.CategoryDescription;
                entity.DisplayOrder = vm.DisplayOrder;
                entity.IsActive = vm.IsActive;

                _db.SaveChanges();
            }
        }

        #endregion

        #region Delete

        public bool Delete(int id)
        {
            bool hasDocuments =
                _db.DownloadDocuments
                .Any(x => x.DownloadCategoryId == id);

            if (hasDocuments)
            {
                return false;
            }

            DownloadCategory entity =
                _db.DownloadCategories
                .FirstOrDefault(x =>
                    x.DownloadCategoryId == id);

            if (entity == null)
            {
                return false;
            }

            _db.DownloadCategories.Remove(entity);

            _db.SaveChanges();

            return true;
        }

        #endregion

        #region Category Dropdown

        public List<SelectListItem> GetCategoryDropdown()
        {
            return _db.DownloadCategories
                      .Where(x => x.IsActive)
                      .OrderBy(x => x.DisplayOrder)
                      .ThenBy(x => x.CategoryName)
                      .Select(x => new SelectListItem
                      {
                          Value = x.DownloadCategoryId.ToString(),
                          Text = x.CategoryName
                      }).ToList();
        }

        #endregion

        #region Check Duplicate

        public bool IsDuplicate(string categoryName, int id = 0)
        {
            categoryName = categoryName.Trim().ToLower();

            return _db.DownloadCategories.Any(x =>
                x.CategoryName.ToLower() == categoryName &&
                x.DownloadCategoryId != id);
        }

        #endregion
    }
}