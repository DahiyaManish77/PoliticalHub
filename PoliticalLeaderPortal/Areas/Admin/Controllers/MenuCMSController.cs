using PoliticalLeaderPortal.Areas.Admin.Services;
using PoliticalLeaderPortal.Areas.Admin.ViewModels.Menu;
using System.Collections.Generic;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    /// <summary>
    /// ===========================================================
    /// Menu CMS Controller
    /// -----------------------------------------------------------
    /// Purpose:
    /// -----------------------------------------------------------
    /// Handles all Menu Management requests.
    ///
    /// Business Logic:
    /// MenuService
    ///
    /// This controller intentionally remains thin.
    /// ===========================================================
    /// </summary>
    public class MenuCMSController : Controller
    {
        #region PRIVATE MEMBERS

        /// <summary>
        /// Menu Service
        /// </summary>
        private readonly MenuService menuService;

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Initializes Menu Service.
        /// </summary>
        public MenuCMSController()
        {
            menuService = new MenuService();
        }

        #endregion

        #region INDEX

        /// <summary>
        /// ===========================================================
        /// INDEX
        /// ===========================================================
        ///
        /// Displays the Menu Management page.
        ///
        /// The menu grid data is loaded separately using
        /// the MenuList action via AJAX.
        ///
        /// ===========================================================
        /// </summary>
        /// <returns>Menu Management View</returns>
        public ActionResult Index()
        {
            return View();
        }

        #endregion
        #region MENU LIST

        /// <summary>
        /// ===========================================================
        /// MENU LIST
        /// ===========================================================
        ///
        /// Returns the Menu Management Grid.
        ///
        /// Purpose:
        ///
        /// • Loads all menus from MenuService
        /// • Returns Partial View
        /// • Supports future AJAX refresh
        /// • Keeps Index() lightweight
        ///
        /// Used By:
        ///
        /// • Menu CMS Index
        ///
        /// ===========================================================
        /// </summary>
        /// <returns>Menu Grid Partial View</returns>
        [HttpGet]
        public PartialViewResult MenuList()
        {
            // Get menu list from service
            var model = menuService.GetAllMenus();

            // Return menu grid partial view
            return PartialView("_MenuList", model);
        }

        #endregion
        #region CREATE MENU (GET)

        /// <summary>
        /// ===========================================================
        /// CREATE MENU (GET)
        /// ===========================================================
        ///
        /// Displays the Create Menu page.
        ///
        /// Loads Parent Menu dropdown.
        ///
        /// ===========================================================
        /// </summary>
        [HttpGet]
        public ActionResult CreateMenu()
        {
            var model = new MenuEditVM();

            model.ParentMenus = menuService.GetParentMenuDropdown();

            return View(model);
        }

        #endregion
        #region CREATE MENU (POST)

        /// <summary>
        /// ===========================================================
        /// CREATE MENU (POST)
        /// ===========================================================
        ///
        /// Creates a new menu.
        ///
        /// Business Logic:
        /// MenuService
        ///
        /// ===========================================================
        /// </summary>
        [HttpPost]

        [ValidateAntiForgeryToken]
        public ActionResult CreateMenu(MenuEditVM model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var item in ModelState)
                {
                    foreach (var error in item.Value.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            item.Key + " : " + error.ErrorMessage);
                    }
                }

                model.ParentMenus = menuService.GetParentMenuDropdown(model.MenuId);

                return View(model);
            }

            var result = menuService.CreateMenu(model);

            if (result)
            {
                TempData["SuccessMessage"] = "Menu created successfully.";

                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Unable to create menu.");

            model.ParentMenus = menuService.GetParentMenuDropdown(model.MenuId);

            return View(model);
        }

        #endregion
        #region EDIT MENU (GET)

        /// <summary>
        /// ===========================================================
        /// EDIT MENU (GET)
        /// ===========================================================
        ///
        /// Displays the Edit Menu page.
        ///
        /// ===========================================================
        /// </summary>
        [HttpGet]
        public ActionResult EditMenu(int id)
        {
            var model = menuService.GetMenuById(id);

            if (model == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        #endregion
        #region UPDATE MENU (POST)

        /// <summary>
        /// ===========================================================
        /// UPDATE MENU (POST)
        /// ===========================================================
        ///
        /// Updates an existing menu.
        ///
        /// Business Logic:
        /// MenuService
        ///
        /// ===========================================================
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateMenu(MenuEditVM model)
        {
            if (!ModelState.IsValid)
            {
                model.ParentMenus = menuService.GetParentMenuDropdown(model.MenuId);

                return View("EditMenu", model);
            }

            bool result = menuService.UpdateMenu(model);

            if (result)
            {
                TempData["SuccessMessage"] = "Menu updated successfully.";

                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Unable to update menu.");

            model.ParentMenus = menuService.GetParentMenuDropdown(model.MenuId);

            return View("EditMenu", model);
        }

        #endregion
        #region DELETE MENU

        /// <summary>
        /// ===========================================================
        /// DELETE MENU
        /// ===========================================================
        ///
        /// Deletes the specified menu.
        ///
        /// Business Logic:
        /// MenuService
        ///
        /// Returns JSON for AJAX requests.
        ///
        /// ===========================================================
        /// </summary>
        [HttpPost]
        public JsonResult DeleteMenu(int id)
        {
            bool result = menuService.DeleteMenu(id);

            if (result)
            {
                return Json(new
                {
                    success = true,
                    message = "Menu deleted successfully."
                });
            }

            return Json(new
            {
                success = false,
                message = "Unable to delete menu."
            });
        }

        #endregion
        #region TOGGLE ACTIVE

        /// <summary>
        /// ===========================================================
        /// TOGGLE ACTIVE
        /// ===========================================================
        ///
        /// Enables or disables a menu.
        ///
        /// Business Logic:
        /// MenuService
        ///
        /// Returns JSON for AJAX.
        ///
        /// ===========================================================
        /// </summary>
        [HttpPost]
        public JsonResult ToggleActive(int id)
        {
            bool result = menuService.ToggleActive(id);

            if (result)
            {
                return Json(new
                {
                    success = true,
                    message = "Menu status updated successfully."
                });
            }

            return Json(new
            {
                success = false,
                message = "Unable to update menu status."
            });
        }

        #endregion
        #region SAVE MENU ORDER

        /// <summary>
        /// ===========================================================
        /// SAVE MENU ORDER
        /// ===========================================================
        ///
        /// Saves drag-drop menu ordering.
        ///
        /// Business Logic:
        /// MenuService
        ///
        /// Returns JSON.
        ///
        /// ===========================================================
        /// </summary>
        [HttpPost]
        public JsonResult SaveOrder(List<MenuOrderVM> menuOrders)
        {
            if (menuOrders == null)
            {
                return Json(new
                {
                    success = false,
                    message = "No menu data received."
                });
            }

            bool result = menuService.SaveMenuOrder(menuOrders);

            if (result)
            {
                return Json(new
                {
                    success = true,
                    message = "Menu order saved successfully."
                });
            }

            return Json(new
            {
                success = false,
                message = "Unable to save menu order."
            });
        }

        #endregion
        #region ADMIN SIDEBAR

        /// <summary>
        /// ===========================================================
        /// ADMIN SIDEBAR
        /// ===========================================================
        ///
        /// Returns the dynamic Admin Sidebar.
        ///
        /// Used By:
        ///
        /// Areas/Admin/Views/Shared/_AdminSidebar.cshtml
        ///
        /// ===========================================================
        /// </summary>
        [ChildActionOnly]
        public PartialViewResult AdminSidebar()
        {
            string currentPath = string.Empty;

            if (Request != null &&
                Request.Url != null)
            {
                currentPath = Request.Url.AbsolutePath;
            }

            var model = menuService.GetAdminSidebarMenus(currentPath);

            return PartialView("_AdminSidebar", model);
        }
        #endregion
        #region WEBSITE NAVIGATION

        /// <summary>
        /// ===========================================================
        /// WEBSITE NAVIGATION
        /// ===========================================================
        ///
        /// Returns the dynamic Website Navigation.
        ///
        /// Used By:
        ///
        /// Views/Shared/_Navigation.cshtml
        ///
        /// ===========================================================
        /// </summary>
        [ChildActionOnly]
        public PartialViewResult WebsiteNavigation()
        {
            var model = menuService.GetWebsiteMenus();

            return PartialView("~/Views/Shared/_Navigation.cshtml", model);
        }

        #endregion

    }
}
