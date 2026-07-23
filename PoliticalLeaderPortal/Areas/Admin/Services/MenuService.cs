using PoliticalLeaderPortal.Areas.Admin.ViewModels.Menu;
using PoliticalLeaderPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Services
{
    /// <summary>
    /// ============================================================
    /// Menu Service
    /// ------------------------------------------------------------
    /// Purpose
    /// ------------------------------------------------------------
    /// This service acts as the single source of truth for every
    /// menu displayed inside the application.
    ///
    /// It supplies menus for:
    ///
    /// • Website Navigation
    /// • Admin Sidebar
    /// • Menu CMS
    /// • Parent Dropdown
    /// • Future Footer
    /// • Future Quick Links
    /// • Future Role Based Menus
    ///
    /// NOTE
    /// ------------------------------------------------------------
    /// Part 1 contains ONLY Read Operations.
    ///
    /// Create / Update / Delete will be implemented later.
    /// ============================================================
    /// </summary>
    public class MenuService
    {
        #region PRIVATE MEMBERS

        /// <summary>
        /// Entity Framework Database Context
        /// </summary>
        private readonly PoliticalLeaderPortalDbEntities1 db;

        #endregion


        #region CONSTRUCTOR

        /// <summary>
        /// Initialize database context
        /// </summary>
        public MenuService()
        {
            db = new PoliticalLeaderPortalDbEntities1();
        }

        #endregion


        #region WEBSITE NAVIGATION

        #region WEBSITE NAVIGATION

        /// <summary>
        /// =============================================================
        /// WEBSITE NAVIGATION
        /// =============================================================
        ///
        /// Returns only active menus that are allowed to
        /// appear on the website navigation.
        ///
        /// Conditions
        ///
        /// • IsActive = true
        /// • ShowOnHome = true
        ///
        /// Result:
        ///
        /// Hierarchical menu tree.
        ///
        /// Used By:
        ///
        /// Views/Shared/_Navigation.cshtml
        ///
        /// =============================================================
        /// </summary>
        public List<MenuVM> GetWebsiteMenus()
        {
            try
            {
                var menuEntities = db.MenuMasters

                                     .AsNoTracking()

                                     .Where(x =>

                                         x.IsActive &&

                                         x.ShowOnHome &&

                                         !string.IsNullOrEmpty(x.MenuName))

                                     .OrderBy(x => x.DisplayOrder)

                                     .ThenBy(x => x.MenuName)

                                     .ToList();

                var menuViewModels =
                    MapMenuCollection(menuEntities);

                return BuildMenuTree(menuViewModels);
            }
            catch
            {
                // Future:
                // Log Exception

                return new List<MenuVM>();
            }
        }

        #endregion

        #endregion



        #region ADMIN SIDEBAR

        /// <summary>
        /// =============================================================
        /// ADMIN SIDEBAR
        /// =============================================================
        ///
        /// Returns all active menus that should appear in the
        /// Admin Sidebar.
        ///
        /// Conditions:
        /// • IsActive = true
        /// • ShowInAdminSidebar = true
        ///
        /// Used By:
        /// Areas/Admin/Views/Shared/_AdminSidebar.cshtml
        ///
        /// =============================================================
        /// </summary>
        public List<MenuVM> GetAdminSidebarMenus(string currentPath)
        {
            try
            {
                var menuEntities = db.MenuMasters

                                     .AsNoTracking()

                                     .Where(x =>

                                         x.IsActive &&

                                         x.ShowInAdminSidebar &&

                                         x.MenuName != null &&

                                         x.MenuName != "")

                                     .OrderBy(x => x.DisplayOrder)

                                     .ThenBy(x => x.MenuName)

                                     .ToList();

                menuEntities = ApplyRoleMenuFilter(menuEntities);

                var menuViewModels = MapMenuCollection(menuEntities);

                var menuTree = BuildMenuTree(menuViewModels);

                MarkActiveMenus(menuTree, currentPath);

                return menuTree;
            }
            catch
            {
                // Future:
                // Log Exception

                return new List<MenuVM>();
            }
        }
        #endregion

        private List<MenuMaster> ApplyRoleMenuFilter(List<MenuMaster> menuEntities)
        {
            try
            {
                if (HttpContext.Current == null ||
                    HttpContext.Current.Session == null)
                {
                    return menuEntities;
                }

                int roleId;
                int? currentRoleId = null;

                if (HttpContext.Current.Session["RoleId"] != null &&
                    Int32.TryParse(HttpContext.Current.Session["RoleId"].ToString(), out roleId))
                {
                    currentRoleId = roleId;
                }

                string roleName = Convert.ToString(HttpContext.Current.Session["RoleName"]);

                var permissionService = new RoleMenuPermissionService();
                var allowedMenuIds = permissionService.GetAllowedMenuIds(currentRoleId, roleName);

                if (allowedMenuIds == null)
                {
                    return menuEntities;
                }

                var allowedSet = new HashSet<int>(allowedMenuIds);
                var entityLookup = menuEntities.ToDictionary(x => x.MenuId);

                foreach (int allowedId in allowedMenuIds.ToList())
                {
                    MenuMaster current;

                    if (!entityLookup.TryGetValue(allowedId, out current))
                    {
                        continue;
                    }

                    while (current.ParentMenuId.HasValue &&
                           entityLookup.TryGetValue(current.ParentMenuId.Value, out current))
                    {
                        allowedSet.Add(current.MenuId);
                    }
                }

                return menuEntities
                    .Where(x => allowedSet.Contains(x.MenuId))
                    .ToList();
            }
            catch
            {
                return menuEntities;
            }
        }




        #region MENU LIST

        /// <summary>
        /// ===========================================================
        /// GET ALL MENUS
        /// ===========================================================
        ///
        /// Returns all menus for Menu Management.
        /// Includes Active + Inactive menus.
        ///
        /// Used By:
        /// Menu CMS - Index Page
        ///
        /// ===========================================================
        /// </summary>
        public List<MenuListVM> GetAllMenus()
        {
            try
            {
                return db.MenuMasters

                         .AsNoTracking()

                         .OrderBy(x => x.DisplayOrder)

                         .ThenBy(x => x.MenuName)

                         .Select(x => new MenuListVM
                         {
                             MenuId = x.MenuId,

                             ParentMenuName = x.MenuMaster2 != null
                                              ? x.MenuMaster2.MenuName
                                              : "",

                             MenuName = x.MenuName,

                             MenuType = x.MenuType,

                             AreaName = x.AreaName,

                             ControllerName = x.ControllerName,

                             ActionName = x.ActionName,

                             IconClass = x.IconClass,

                             DisplayOrder = x.DisplayOrder,

                             IsActive = x.IsActive,

                             ShowOnHome = x.ShowOnHome,

                             ShowInAdminSidebar = x.ShowInAdminSidebar,

                             ShowInFooter = x.ShowInFooter,

                             ShowInQuickLinks = x.ShowInQuickLinks,

                             IsSystemMenu = x.IsSystemMenu
                         })

                         .ToList();
            }
            catch
            {
                // Future:
                // Log Exception

                return new List<MenuListVM>();
            }
        }

        #endregion


        #region PARENT MENU DROPDOWN

        /// <summary>
        /// ===========================================================
        /// GET PARENT MENU DROPDOWN
        /// ===========================================================
        ///
        /// Returns all active root menus for Parent Menu dropdown.
        ///
        /// Used By:
        /// Create Menu
        /// Edit Menu
        ///
        /// ===========================================================
        /// </summary>
        public List<SelectListItem> GetParentMenuDropdown(int? excludedMenuId = null)
        {
            try
            {
                var list = new List<SelectListItem>();

                list.Add(new SelectListItem
                {
                    Text = "-- Root Menu --",
                    Value = ""
                });

                var menus = db.MenuMasters
                              .AsNoTracking()
                              .Where(x => x.IsActive)
                              .OrderBy(x => x.DisplayOrder)
                              .ThenBy(x => x.MenuName)
                              .ToList();

                var blockedIds = new HashSet<int>();

                if (excludedMenuId.HasValue)
                {
                    blockedIds.Add(excludedMenuId.Value);
                    AddChildMenuIds(menus, excludedMenuId.Value, blockedIds);
                }

                AddParentMenuOptions(
                    list,
                    menus,
                    null,
                    blockedIds,
                    0);

                return list;
            }
            catch
            {
                // Future:
                // Log Exception

                return new List<SelectListItem>
        {
            new SelectListItem
            {
                Text = "-- Root Menu --",
                Value = ""
            }
        };
            }
        }

        #endregion
        #region GET MENU BY ID

        /// <summary>
        /// ===========================================================
        /// GET MENU BY ID
        /// ===========================================================
        ///
        /// Returns a single menu for Edit Screen.
        ///
        /// Used By:
        /// Edit Menu
        ///
        /// ===========================================================
        /// </summary>
        public MenuEditVM GetMenuById(int menuId)
        {
            try
            {
                var entity = db.MenuMasters

                               .AsNoTracking()

                               .FirstOrDefault(x => x.MenuId == menuId);

                if (entity == null)
                    return null;

                return new MenuEditVM
                {
                    MenuId = entity.MenuId,

                    ParentMenuId = entity.ParentMenuId,

                    MenuName = entity.MenuName,

                    MenuDescription = entity.MenuDescription,

                    AreaName = entity.AreaName,

                    ControllerName = entity.ControllerName,

                    ActionName = entity.ActionName,

                    RouteValues = entity.RouteValues,

                    CustomUrl = entity.CustomUrl,

                    MenuType = entity.MenuType,

                    IconClass = entity.IconClass,

                    CssClass = entity.CssClass,

                    DisplayOrder = entity.DisplayOrder,

                    IsActive = entity.IsActive,

                    ShowOnHome = entity.ShowOnHome,

                    ShowInAdminSidebar = entity.ShowInAdminSidebar,

                    OpenInNewTab = entity.OpenInNewTab,

                    IsClickable = entity.IsClickable,

                    HasMegaMenu = entity.HasMegaMenu,

                    MenuLevel = entity.MenuLevel,

                    ShowInFooter = entity.ShowInFooter,

                    ShowInQuickLinks = entity.ShowInQuickLinks,

                    IsSystemMenu = entity.IsSystemMenu,

                    PageTitle = entity.PageTitle,

                    MetaDescription = entity.MetaDescription,

                    ParentMenus = GetParentMenuDropdown(menuId)
                };
            }
            catch
            {
                // Future:
                // Log Exception

                return null;
            }
        }

        #endregion

        #region CREATE MENU

        /// <summary>
        /// ===========================================================
        /// CREATE MENU
        /// ===========================================================
        ///
        /// Creates a new menu with all required validations.
        ///
        /// Used By:
        /// Create Menu
        ///
        /// ===========================================================
        /// </summary>
        public bool CreateMenu(MenuEditVM vm)
        {
            if (vm == null)
                return false;

            // Menu Name Required
            vm.MenuName = (vm.MenuName ?? "").Trim();

            if (string.IsNullOrWhiteSpace(vm.MenuName))
                return false;

            // Duplicate Menu Name
            if (IsDuplicateMenuName(vm.MenuName, 0))
                return false;

            // Duplicate Route
            if (IsDuplicateRoute(
                    vm.AreaName,
                    vm.ControllerName,
                    vm.ActionName,
                    0))
                return false;

            // Self Parent Validation
            if (IsSelfParent(vm))
                return false;

            // Auto Display Order
            if (vm.DisplayOrder <= 0)
            {
                vm.DisplayOrder = db.MenuMasters.Any()
                    ? db.MenuMasters.Max(x => x.DisplayOrder) + 1
                    : 1;
            }

            // Auto Menu Level
            vm.MenuLevel = 0;

            if (vm.ParentMenuId.HasValue)
            {
                var parent = db.MenuMasters
                               .FirstOrDefault(x =>
                                    x.MenuId == vm.ParentMenuId.Value);

                if (parent != null)
                {
                    vm.MenuLevel = parent.MenuLevel + 1;
                }
            }

            var entity = new MenuMaster
            {
                ParentMenuId = vm.ParentMenuId,

                MenuName = vm.MenuName,

                MenuDescription = vm.MenuDescription,

                AreaName = string.IsNullOrWhiteSpace(vm.AreaName)
                            ? null
                            : vm.AreaName.Trim(),

                ControllerName = string.IsNullOrWhiteSpace(vm.ControllerName)
                            ? null
                            : vm.ControllerName.Trim(),

                ActionName = string.IsNullOrWhiteSpace(vm.ActionName)
                            ? null
                            : vm.ActionName.Trim(),

                RouteValues = vm.RouteValues,

                CustomUrl = vm.CustomUrl,

                MenuType = "Navigation",

                IconClass = vm.IconClass,

                CssClass = vm.CssClass,

                DisplayOrder = vm.DisplayOrder,

                IsActive = vm.IsActive,

                ShowOnHome = vm.ShowOnHome,

                ShowInAdminSidebar = vm.ShowInAdminSidebar,

                OpenInNewTab = vm.OpenInNewTab,

                IsClickable = vm.IsClickable,

                HasMegaMenu = vm.HasMegaMenu,

                MenuLevel = vm.MenuLevel,

                ShowInFooter = vm.ShowInFooter,

                ShowInQuickLinks = vm.ShowInQuickLinks,

                IsSystemMenu = vm.IsSystemMenu,

                PageTitle = vm.PageTitle,

                MetaDescription = vm.MetaDescription,

                CreatedBy = 1,

                CreatedDate = DateTime.Now,

                ModifiedBy = null,

                ModifiedDate = null
            };

            db.MenuMasters.Add(entity);

            try
{
    db.SaveChanges();
}
catch (System.Data.Entity.Validation.DbEntityValidationException ex)
{
    foreach (var entityErrors in ex.EntityValidationErrors)
    {
        foreach (var validationError in entityErrors.ValidationErrors)
        {
            System.Diagnostics.Debug.WriteLine(
                validationError.PropertyName + " : " +
                validationError.ErrorMessage);
        }
    }

    throw;
}

            return true;
        }

        #endregion

        #region UPDATE MENU

        /// <summary>
        /// ===========================================================
        /// UPDATE MENU
        /// ===========================================================
        ///
        /// Updates an existing menu.
        ///
        /// Used By:
        /// Edit Menu
        ///
        /// ===========================================================
        /// </summary>
        public bool UpdateMenu(MenuEditVM vm)
        {
            if (vm == null)
                return false;

            // Menu Name Required
            vm.MenuName = (vm.MenuName ?? "").Trim();

            if (string.IsNullOrWhiteSpace(vm.MenuName))
                return false;

            // Duplicate Menu Name
            if (IsDuplicateMenuName(vm.MenuName, vm.MenuId))
                return false;

            // Duplicate Route
            if (IsDuplicateRoute(
                    vm.AreaName,
                    vm.ControllerName,
                    vm.ActionName,
                    vm.MenuId))
                return false;

            // Self Parent Validation
            if (IsSelfParent(vm))
                return false;

            var entity = db.MenuMasters
                           .FirstOrDefault(x => x.MenuId == vm.MenuId);

            if (entity == null)
                return false;

            // Calculate Menu Level
            vm.MenuLevel = 0;

            if (vm.ParentMenuId.HasValue)
            {
                var parent = db.MenuMasters
                               .FirstOrDefault(x =>
                                    x.MenuId == vm.ParentMenuId.Value);

                if (parent != null)
                {
                    vm.MenuLevel = parent.MenuLevel + 1;
                }
            }

            entity.ParentMenuId = vm.ParentMenuId;

            entity.MenuName = vm.MenuName;

            entity.MenuDescription = vm.MenuDescription;

            entity.AreaName = string.IsNullOrWhiteSpace(vm.AreaName)
                            ? null
                            : vm.AreaName.Trim();

            entity.ControllerName = string.IsNullOrWhiteSpace(vm.ControllerName)
                            ? null
                            : vm.ControllerName.Trim();

            entity.ActionName = string.IsNullOrWhiteSpace(vm.ActionName)
                            ? null
                            : vm.ActionName.Trim();

            entity.RouteValues = vm.RouteValues;

            entity.CustomUrl = vm.CustomUrl;

            entity.MenuType = "Navigation";

            entity.IconClass = vm.IconClass;

            entity.CssClass = vm.CssClass;

            entity.DisplayOrder = vm.DisplayOrder;

            entity.IsActive = vm.IsActive;

            entity.ShowOnHome = vm.ShowOnHome;

            entity.ShowInAdminSidebar = vm.ShowInAdminSidebar;

            entity.OpenInNewTab = vm.OpenInNewTab;

            entity.IsClickable = vm.IsClickable;

            entity.HasMegaMenu = vm.HasMegaMenu;

            entity.MenuLevel = vm.MenuLevel;

            entity.ShowInFooter = vm.ShowInFooter;

            entity.ShowInQuickLinks = vm.ShowInQuickLinks;

            entity.IsSystemMenu = vm.IsSystemMenu;

            entity.PageTitle = vm.PageTitle;

            entity.MetaDescription = vm.MetaDescription;

            // Preserve Created Information

            entity.ModifiedBy = 1;

            entity.ModifiedDate = DateTime.Now;

            db.SaveChanges();

            return true;
        }

        #endregion


        #region DELETE MENU

        /// <summary>
        /// ===========================================================
        /// DELETE MENU
        /// ===========================================================
        ///
        /// Deletes a menu after validating:
        ///
        /// • Menu exists
        /// • Menu is not a System Menu
        /// • Menu has no child menus
        ///
        /// Used By:
        /// Menu CMS
        ///
        /// ===========================================================
        /// </summary>
        public bool DeleteMenu(int menuId)
        {
            // Invalid Menu Id
            if (menuId <= 0)
                return false;

            var entity = db.MenuMasters
                           .FirstOrDefault(x => x.MenuId == menuId);

            // Menu Not Found
            if (entity == null)
                return false;

            // System Menu Protection
            if (entity.IsSystemMenu)
                return false;

            // Child Menu Protection
            if (db.MenuMasters.Any(x => x.ParentMenuId == menuId))
                return false;

            db.MenuMasters.Remove(entity);

            db.SaveChanges();

            return true;
        }

        #endregion
        #region TOGGLE ACTIVE STATUS

        /// <summary>
        /// ===========================================================
        /// TOGGLE ACTIVE STATUS
        /// ===========================================================
        ///
        /// Enables or disables a menu.
        ///
        /// Used By:
        ///
        /// • Menu CMS Grid
        /// • AJAX Toggle Switch
        ///
        /// Validation:
        ///
        /// • Menu Id must be valid
        /// • Menu must exist
        ///
        /// Returns:
        ///
        /// True  = Status updated successfully
        /// False = Update failed
        ///
        /// ===========================================================
        /// </summary>
        public bool ToggleActive(int menuId)
        {
            // Invalid Menu Id
            if (menuId <= 0)
                return false;

            // Find Menu
            var entity = db.MenuMasters
                           .FirstOrDefault(x => x.MenuId == menuId);

            // Menu Not Found
            if (entity == null)
                return false;

            // Toggle Status
            entity.IsActive = !entity.IsActive;

            // Audit Information
            entity.ModifiedBy = 1;
            entity.ModifiedDate = DateTime.Now;

            db.SaveChanges();

            return true;
        }

        #endregion

        #region SAVE MENU ORDER

        /// <summary>
        /// ===========================================================
        /// SAVE MENU ORDER
        /// ===========================================================
        ///
        /// Saves the menu hierarchy and display order.
        ///
        /// Used By:
        ///
        /// • Drag & Drop Menu Ordering
        /// • Menu CMS
        ///
        /// Updates:
        ///
        /// • ParentMenuId
        /// • DisplayOrder
        /// • ModifiedBy
        /// • ModifiedDate
        ///
        /// Returns:
        ///
        /// True  = Successfully Saved
        /// False = Validation Failed
        ///
        /// ===========================================================
        /// </summary>
        public bool SaveMenuOrder(List<MenuOrderVM> menuOrders)
        {
            // Validation
            if (menuOrders == null || !menuOrders.Any())
                return false;

            foreach (var item in menuOrders)
            {
                if (item == null)
                    continue;

                var entity = db.MenuMasters
                               .FirstOrDefault(x => x.MenuId == item.MenuId);

                // Skip invalid records
                if (entity == null)
                    continue;

                // Prevent self-parenting
                if (item.ParentMenuId.HasValue &&
                    item.ParentMenuId.Value == item.MenuId)
                {
                    continue;
                }

                // Update Parent
                entity.ParentMenuId = item.ParentMenuId;

                // Update Display Order
                entity.DisplayOrder = item.DisplayOrder;

                // Update Menu Level
                entity.MenuLevel = 0;

                if (item.ParentMenuId.HasValue)
                {
                    var parent = db.MenuMasters
                                   .FirstOrDefault(x => x.MenuId == item.ParentMenuId.Value);

                    if (parent != null)
                    {
                        entity.MenuLevel = parent.MenuLevel + 1;
                    }
                }

                // Audit Information
                entity.ModifiedBy = 1;
                entity.ModifiedDate = DateTime.Now;
            }

            db.SaveChanges();

            return true;
        }

        #endregion

        #region GET FOOTER MENUS

        /// <summary>
        /// ===========================================================
        /// GET FOOTER MENUS
        /// ===========================================================
        ///
        /// Returns all active menus that should appear
        /// in the website footer.
        ///
        /// Conditions:
        ///
        /// • IsActive = true
        /// • ShowInFooter = true
        ///
        /// Returns:
        ///
        /// Hierarchical Footer Menu Tree.
        ///
        /// Used By:
        ///
        /// Views/Shared/_Footer.cshtml
        ///
        /// ===========================================================
        /// </summary>
        public List<MenuVM> GetFooterMenus()
        {
            try
            {
                var menuEntities = db.MenuMasters

                                     .AsNoTracking()

                                     .Where(x =>

                                         x.IsActive &&

                                         x.ShowInFooter &&

                                         x.MenuName != null &&

                                         x.MenuName != "")

                                     .OrderBy(x => x.DisplayOrder)

                                     .ThenBy(x => x.MenuName)

                                     .ToList();

                var menuViewModels = MapMenuCollection(menuEntities);

                return BuildMenuTree(menuViewModels);
            }
            catch
            {
                // Future:
                // Log Exception

                return new List<MenuVM>();
            }
        }
        #endregion

        #region GET QUICK LINK MENUS

        /// <summary>
        /// ===========================================================
        /// GET QUICK LINK MENUS
        /// ===========================================================
        ///
        /// Returns all active menus that should appear
        /// in the website Quick Links section.
        ///
        /// Conditions:
        ///
        /// • IsActive = true
        /// • ShowInQuickLinks = true
        ///
        /// Returns:
        ///
        /// Hierarchical Quick Link Menu Tree.
        ///
        /// Used By:
        ///
        /// Future:
        /// Views/Shared/_QuickLinks.cshtml
        ///
        /// ===========================================================
        /// </summary>
        public List<MenuVM> GetQuickLinkMenus()
        {
            try
            {
                var menuEntities = db.MenuMasters

                                     .AsNoTracking()

                                     .Where(x =>

                                         x.IsActive &&

                                         x.ShowInQuickLinks &&

                                         x.MenuName != null &&

                                         x.MenuName != "")

                                     .OrderBy(x => x.DisplayOrder)

                                     .ThenBy(x => x.MenuName)

                                     .ToList();

                var menuViewModels = MapMenuCollection(menuEntities);

                return BuildMenuTree(menuViewModels);
            }
            catch
            {
                // Future:
                // Log Exception

                return new List<MenuVM>();
            }
        }

        #endregion

        #region MENU EXISTS

        /// <summary>
        /// ===========================================================
        /// MENU EXISTS
        /// ===========================================================
        ///
        /// Checks whether the specified Menu exists.
        ///
        /// Used By:
        ///
        /// • Update Menu
        /// • Delete Menu
        /// • Toggle Active
        /// • Controller Validation
        /// • Future APIs
        ///
        /// Returns:
        ///
        /// True  = Menu Exists
        /// False = Menu Not Found
        ///
        /// ===========================================================
        /// </summary>
        public bool MenuExists(int menuId)
        {
            // Invalid Menu Id
            if (menuId <= 0)
                return false;

            return db.MenuMasters

                     .AsNoTracking()

                     .Any(x => x.MenuId == menuId);
        }

        #endregion

        #region GET NEXT DISPLAY ORDER

        /// <summary>
        /// ===========================================================
        /// GET NEXT DISPLAY ORDER
        /// ===========================================================
        ///
        /// Returns the next available display order for a new menu.
        ///
        /// Rules:
        ///
        /// • If no menu exists, returns 1.
        /// • Otherwise returns (Maximum DisplayOrder + 1).
        ///
        /// Used By:
        ///
        /// • Create Menu
        /// • Future Menu Import
        /// • Future Bulk Menu Creation
        ///
        /// ===========================================================
        /// </summary>
        public int GetNextDisplayOrder()
        {
            try
            {
                if (!db.MenuMasters.Any())
                    return 1;

                return db.MenuMasters
                         .Max(x => x.DisplayOrder) + 1;
            }
            catch
            {
                // Future:
                // Log Exception

                return 1;
            }
        }

        #endregion

        #region GET ROOT MENUS

        /// <summary>
        /// ===========================================================
        /// GET ROOT MENUS
        /// ===========================================================
        ///
        /// Returns all active root menus.
        ///
        /// Root Menu:
        ///
        /// • ParentMenuId == null
        ///
        /// Result:
        ///
        /// Flat list of root menus ordered by DisplayOrder.
        ///
        /// Used By:
        ///
        /// • Parent Menu Dropdown
        /// • Menu Designer
        /// • Future Mega Menu Builder
        /// • Future Sitemap
        ///
        /// ===========================================================
        /// </summary>
        public List<MenuVM> GetRootMenus()
        {
            try
            {
                var menuEntities = db.MenuMasters

                                     .AsNoTracking()

                                     .Where(x =>

                                         x.IsActive &&

                                         x.ParentMenuId == null &&

                                         x.MenuName != null &&

                                         x.MenuName != "")

                                     .OrderBy(x => x.DisplayOrder)

                                     .ThenBy(x => x.MenuName)

                                     .ToList();

                return MapMenuCollection(menuEntities);
            }
            catch
            {
                // Future:
                // Log Exception

                return new List<MenuVM>();
            }
        }

        #endregion

        #region GET CHILD MENUS

        /// <summary>
        /// ===========================================================
        /// GET CHILD MENUS
        /// ===========================================================
        ///
        /// Returns all direct child menus for the specified
        /// parent menu.
        ///
        /// Conditions:
        ///
        /// • ParentMenuId must be valid
        /// • Only Active menus are returned
        ///
        /// Result:
        ///
        /// Flat list of immediate child menus.
        ///
        /// Used By:
        ///
        /// • Menu Designer
        /// • Mega Menu Builder
        /// • AJAX Child Menu Loading
        /// • Future Sitemap
        ///
        /// ===========================================================
        /// </summary>
        public List<MenuVM> GetChildMenus(int parentMenuId)
        {
            try
            {
                // Invalid Parent Menu
                if (parentMenuId <= 0)
                    return new List<MenuVM>();

                var menuEntities = db.MenuMasters

                                     .AsNoTracking()

                                     .Where(x =>

                                         x.IsActive &&

                                         x.ParentMenuId == parentMenuId &&

                                         x.MenuName != null &&

                                         x.MenuName != "")

                                     .OrderBy(x => x.DisplayOrder)

                                     .ThenBy(x => x.MenuName)

                                     .ToList();

                return MapMenuCollection(menuEntities);
            }
            catch
            {
                // Future:
                // Log Exception

                return new List<MenuVM>();
            }
        }

        #endregion

        #region GET MENU DETAILS

        /// <summary>
        /// ===========================================================
        /// GET MENU DETAILS
        /// ===========================================================
        ///
        /// Returns complete read-only details of a menu.
        ///
        /// Used By:
        ///
        /// • Menu Preview
        /// • AJAX Details
        /// • Future APIs
        /// • Dashboard Widgets
        ///
        /// Returns:
        ///
        /// MenuVM if found.
        /// Null if menu does not exist.
        ///
        /// ===========================================================
        /// </summary>
        public MenuVM GetMenuDetails(int menuId)
        {
            try
            {
                // Invalid Menu Id
                if (menuId <= 0)
                    return null;

                var entity = db.MenuMasters

                               .AsNoTracking()

                               .FirstOrDefault(x => x.MenuId == menuId);

                if (entity == null)
                    return null;

                return MapMenu(entity);
            }
            catch
            {
                // Future:
                // Log Exception

                return null;
            }
        }

        #endregion
        #region SEARCH MENUS

        /// <summary>
        /// ===========================================================
        /// SEARCH MENUS
        /// ===========================================================
        ///
        /// Searches menus by Menu Name, Controller,
        /// Action or Area.
        ///
        /// Used By:
        ///
        /// • Menu CMS Search
        /// • Future AJAX Search
        /// • Dashboard Search
        ///
        /// ===========================================================
        /// </summary>
        public List<MenuListVM> SearchMenus(string keyword)
        {
            try
            {
                keyword = (keyword ?? string.Empty).Trim();

                IQueryable<MenuMaster> query =
      db.MenuMasters.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(x =>

                        (x.MenuName ?? "").Contains(keyword) ||

                        (x.ControllerName ?? "").Contains(keyword) ||

                        (x.ActionName ?? "").Contains(keyword) ||

                        (x.AreaName ?? "").Contains(keyword));
                }

                return query

                    .OrderBy(x => x.DisplayOrder)

                    .ThenBy(x => x.MenuName)

                    .Select(x => new MenuListVM
                    {
                        MenuId = x.MenuId,

                        ParentMenuName = x.MenuMaster2 != null
                                            ? x.MenuMaster2.MenuName
                                            : "",

                        MenuName = x.MenuName,

                        MenuType = x.MenuType,

                        AreaName = x.AreaName,

                        ControllerName = x.ControllerName,

                        ActionName = x.ActionName,

                        IconClass = x.IconClass,

                        DisplayOrder = x.DisplayOrder,

                        IsActive = x.IsActive,

                        ShowOnHome = x.ShowOnHome,

                        ShowInAdminSidebar = x.ShowInAdminSidebar,

                        ShowInFooter = x.ShowInFooter,

                        ShowInQuickLinks = x.ShowInQuickLinks,

                        IsSystemMenu = x.IsSystemMenu
                    })

                    .ToList();
            }
            catch
            {
                return new List<MenuListVM>();
            }
        }

        #endregion
        #region PRIVATE MAPPING

        /// <summary>
        /// ==========================================================
        /// ENTITY TO VIEWMODEL MAPPING
        /// ==========================================================
        ///
        /// Converts EF Entity
        ///
        /// MenuMaster
        ///
        /// into
        ///
        /// MenuVM
        ///
        /// ==========================================================
        /// </summary>
        private List<MenuVM> MapMenuCollection(List<MenuMaster> menus)
        {
            return menus.Select(MapMenu).ToList();
        }


        /// <summary>
        /// Maps one MenuMaster into MenuVM
        /// </summary>
        private MenuVM MapMenu(MenuMaster menu)
        {
            var controllerName = menu.ControllerName;
            var actionName = menu.ActionName;
            var customUrl = menu.CustomUrl;

            if (!string.IsNullOrWhiteSpace(menu.MenuName) &&
                menu.MenuName.Trim().Equals("Mera Kshetra", StringComparison.OrdinalIgnoreCase))
            {
                controllerName = "MeraKshetra";
                actionName = "Index";
                customUrl = null;
            }

            return new MenuVM
            {
                MenuId = menu.MenuId,

                ParentMenuId = menu.ParentMenuId,

                MenuName = menu.MenuName,

                MenuDescription = menu.MenuDescription,

                AreaName = menu.AreaName,

                ControllerName = controllerName,

                ActionName = actionName,

                RouteValues = menu.RouteValues,

                CustomUrl = customUrl,

                IconClass = menu.IconClass,

                CssClass = menu.CssClass,

                DisplayOrder = menu.DisplayOrder,

                OpenInNewTab = menu.OpenInNewTab,

                IsClickable = menu.IsClickable,

                HasMegaMenu = menu.HasMegaMenu
            };
        }

        #endregion


        #region BUILD MENU TREE

        /// <summary>
        /// ===========================================================
        /// BUILD MENU TREE
        /// ===========================================================
        ///
        /// Converts a flat menu collection into a hierarchical tree.
        ///
        /// Example:
        ///
        /// Home
        /// About
        /// Biography
        /// Vision
        ///
        /// becomes
        ///
        /// Home
        ///
        /// About
        ///     Biography
        ///     Vision
        ///
        /// Used By:
        ///
        /// • Website Navigation
        /// • Admin Sidebar
        ///
        /// ===========================================================
        /// </summary>
        private List<MenuVM> BuildMenuTree(List<MenuVM> menus)
        {
            if (menus == null || !menus.Any())
                return new List<MenuVM>();

            // Dictionary for fast parent lookup
            var lookup = menus.ToDictionary(x => x.MenuId);

            var rootMenus = new List<MenuVM>();

            foreach (var menu in menus.OrderBy(x => x.DisplayOrder).ThenBy(x => x.MenuName))
            {
                // Always initialize Children
                if (menu.Children == null)
                    menu.Children = new List<MenuVM>();

                // Root Menu
                if (!menu.ParentMenuId.HasValue)
                {
                    rootMenus.Add(menu);
                    continue;
                }

                MenuVM parent;

                if (lookup.TryGetValue(menu.ParentMenuId.Value, out parent))
                {
                    if (parent.Children == null)
                        parent.Children = new List<MenuVM>();

                    parent.Children.Add(menu);
                }
                else
                {
                    // Parent not found
                    // Treat as Root Menu
                    rootMenus.Add(menu);
                }
            }

            SortChildMenus(rootMenus);

            return rootMenus;
        }
        #region SORT CHILD MENUS

        /// <summary>
        /// ===========================================================
        /// SORT CHILD MENUS
        /// ===========================================================
        ///
        /// Recursively sorts every level of the menu tree.
        ///
        /// ===========================================================
        /// </summary>
        private void SortChildMenus(List<MenuVM> menus)
        {
            if (menus == null || !menus.Any())
                return;

            foreach (var menu in menus)
            {
                if (menu.Children != null && menu.Children.Any())
                {
                    menu.Children = menu.Children
                                        .OrderBy(x => x.DisplayOrder)
                                        .ThenBy(x => x.MenuName)
                                        .ToList();

                    SortChildMenus(menu.Children);
                }
            }
        }

        #endregion

        #endregion
        #region MARK ACTIVE MENUS

        /// <summary>
        /// ========================================================================
        /// MARK ACTIVE MENUS
        /// ========================================================================
        ///
        /// Purpose:
        /// Recursively traverses the complete menu hierarchy and marks
        /// the current menu as Active and all its parent menus as Expanded.
        ///
        /// Used By:
        /// GetAdminSidebarMenus()
        ///
        /// Future Modification:
        /// This method can later be extended to support:
        /// • Role Based Menus
        /// • Permission Based Menus
        /// • Dynamic Route Values
        /// • Area Specific Navigation
        ///
        /// ========================================================================
        /// </summary>
        private bool MarkActiveMenus(
            List<MenuVM> menus,
            string currentPath)
        {
            if (menus == null || menus.Count == 0)
            {
                return false;
            }

            currentPath = string.IsNullOrWhiteSpace(currentPath)
                            ? string.Empty
                            : currentPath.Trim().TrimEnd('/').ToLower();

            bool containsActiveMenu = false;

            foreach (var menu in menus)
            {
                menu.IsActive = false;
                menu.IsExpanded = false;

                string menuUrl = string.Empty;

                //--------------------------------------------------------
                // Custom URL
                //--------------------------------------------------------

                if (!string.IsNullOrWhiteSpace(menu.CustomUrl))
                {
                    menuUrl = menu.CustomUrl;
                }

                //--------------------------------------------------------
                // MVC Route
                //--------------------------------------------------------

                else if (!string.IsNullOrWhiteSpace(menu.ControllerName) &&
                         !string.IsNullOrWhiteSpace(menu.ActionName))
                {
                    menuUrl = "/" +
                              (string.IsNullOrWhiteSpace(menu.AreaName)
                                    ? "Admin"
                                    : menu.AreaName.Trim('/')) +
                              "/" +
                              menu.ControllerName.Trim('/') +
                              "/" +
                              menu.ActionName.Trim('/');
                }

                //--------------------------------------------------------
                // Normalize URL
                //--------------------------------------------------------

                menuUrl = string.IsNullOrWhiteSpace(menuUrl)
                            ? string.Empty
                            : menuUrl.Trim().TrimEnd('/').ToLower();

                //--------------------------------------------------------
                // Current Menu Active
                //--------------------------------------------------------

                if (!string.IsNullOrWhiteSpace(menuUrl))
                {
                    menu.IsActive = currentPath.Equals(
                                        menuUrl,
                                        StringComparison.OrdinalIgnoreCase);
                }

                //--------------------------------------------------------
                // Child Menus
                //--------------------------------------------------------

                bool childContainsActiveMenu =
                    MarkActiveMenus(
                        menu.Children,
                        currentPath);

                //--------------------------------------------------------
                // Expand Parent
                //--------------------------------------------------------

                if (childContainsActiveMenu)
                {
                    menu.IsExpanded = true;
                }

                //--------------------------------------------------------
                // Current Branch Status
                //--------------------------------------------------------

                if (menu.IsActive || childContainsActiveMenu)
                {
                    containsActiveMenu = true;
                }
            }

            return containsActiveMenu;
        }

        #endregion
        #region PRIVATE VALIDATION HELPERS
        #region DUPLICATE MENU NAME

        /// <summary>
        /// ===========================================================
        /// CHECK DUPLICATE MENU NAME
        /// ===========================================================
        ///
        /// Checks whether another menu already exists
        /// with the same name.
        ///
        /// Comparison is:
        ///
        /// • Case Insensitive
        /// • Trimmed
        ///
        /// ===========================================================
        /// </summary>
        private bool IsDuplicateMenuName(string menuName, int menuId)
        {
            menuName = (menuName ?? "").Trim().ToLower();

            if (string.IsNullOrEmpty(menuName))
                return false;

            return db.MenuMasters

                     .Where(x =>
                            x.MenuId != menuId &&
                            x.MenuName != null)

                     .AsEnumerable()

                     .Any(x =>
                            x.MenuName.Trim().ToLower() == menuName);
        }

        #endregion

        #region DUPLICATE ROUTE

        /// <summary>
        /// ===========================================================
        /// CHECK DUPLICATE ROUTE
        /// ===========================================================
        ///
        /// Prevents duplicate MVC routes.
        ///
        /// Example:
        ///
        /// Area      Controller      Action
        /// --------------------------------
        /// Admin     HeroSlider      Index
        ///
        /// should exist only once.
        ///
        /// Comparison is:
        ///
        /// • Case Insensitive
        /// • Trimmed
        ///
        /// ===========================================================
        /// </summary>
        private bool IsDuplicateRoute(
     string areaName,
     string controllerName,
     string actionName,
     int menuId)
        {
            areaName = (areaName ?? "").Trim().ToLower();

            controllerName = (controllerName ?? "").Trim().ToLower();

            actionName = (actionName ?? "").Trim().ToLower();

            if (string.IsNullOrEmpty(controllerName) &&
                string.IsNullOrEmpty(actionName))
            {
                return false;
            }

            return db.MenuMasters

                     .Where(x => x.MenuId != menuId)

                     .AsEnumerable()

                     .Any(x =>

                        ((x.AreaName ?? "").Trim().ToLower()) == areaName &&

                        ((x.ControllerName ?? "").Trim().ToLower()) == controllerName &&

                        ((x.ActionName ?? "").Trim().ToLower()) == actionName);
        }

        #endregion

        #region SELF PARENT VALIDATION

        /// <summary>
        /// ===========================================================
        /// CHECK SELF PARENT
        /// ===========================================================
        ///
        /// Prevents a menu from becoming its own parent.
        ///
        /// Example:
        ///
        /// Menu Id      : 5
        /// ParentMenuId : 5
        ///
        /// Result:
        ///
        /// Invalid
        ///
        /// ===========================================================
        /// </summary>
        private bool IsSelfParent(MenuEditVM vm)
        {
            if (vm == null)
                return false;

            if (!vm.ParentMenuId.HasValue)
                return false;

            return vm.MenuId == vm.ParentMenuId.Value;
        }

        #endregion
        #region HAS CHILD MENUS

        /// <summary>
        /// ===========================================================
        /// HAS CHILD MENUS
        /// ===========================================================
        ///
        /// Checks whether the specified menu has one or more
        /// child menus.
        ///
        /// Used Before:
        ///
        /// • Delete Menu
        /// • Future Parent Change Validation
        ///
        /// ===========================================================
        /// </summary>
        private bool HasChildMenus(int menuId)
        {
            if (menuId <= 0)
                return false;

            return db.MenuMasters
                     .AsNoTracking()
                     .Any(x => x.ParentMenuId == menuId);
        }

        #endregion

        #region PARENT DROPDOWN HELPERS

        private void AddParentMenuOptions(
            List<SelectListItem> selectList,
            List<MenuMaster> menus,
            int? parentMenuId,
            HashSet<int> blockedIds,
            int level)
        {
            var children = menus
                .Where(x => x.ParentMenuId == parentMenuId)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.MenuName)
                .ToList();

            foreach (var menu in children)
            {
                if (!blockedIds.Contains(menu.MenuId))
                {
                    selectList.Add(new SelectListItem
                    {
                        Text = new string('-', level * 2) + (level > 0 ? " " : "") + menu.MenuName,
                        Value = menu.MenuId.ToString()
                    });
                }

                AddParentMenuOptions(
                    selectList,
                    menus,
                    menu.MenuId,
                    blockedIds,
                    level + 1);
            }
        }

        private void AddChildMenuIds(
            List<MenuMaster> menus,
            int parentMenuId,
            HashSet<int> blockedIds)
        {
            var children = menus
                .Where(x => x.ParentMenuId == parentMenuId)
                .ToList();

            foreach (var child in children)
            {
                if (blockedIds.Add(child.MenuId))
                {
                    AddChildMenuIds(
                        menus,
                        child.MenuId,
                        blockedIds);
                }
            }
        }

        #endregion

        #region SYSTEM MENU VALIDATION

        /// <summary>
        /// ===========================================================
        /// CHECK SYSTEM MENU
        /// ===========================================================
        ///
        /// Determines whether the specified menu is a
        /// System Menu.
        ///
        /// System Menus are protected and cannot be deleted.
        ///
        /// Used By:
        ///
        /// • Delete Menu
        /// • Future Update Validation
        ///
        /// ===========================================================
        /// </summary>
        private bool IsSystemMenu(int menuId)
        {
            if (menuId <= 0)
                return false;

            return db.MenuMasters

                     .AsNoTracking()

                     .Any(x =>
                            x.MenuId == menuId &&
                            x.IsSystemMenu);
        }

        #endregion
        #endregion
    }
}
