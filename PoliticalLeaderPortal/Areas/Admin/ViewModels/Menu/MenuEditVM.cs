using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.Menu
{
    public class MenuEditVM
    {
        public MenuEditVM()
        {
            ParentMenus = new List<SelectListItem>();

            IsActive = true;
            IsClickable = true;

            ShowOnHome = false;
            ShowInAdminSidebar = false;

            OpenInNewTab = false;
            HasMegaMenu = false;

            ShowInFooter = false;
            ShowInQuickLinks = false;

            MenuLevel = 0;
        }

        public int MenuId { get; set; }

        [Display(Name = "Parent Menu")]
        public int? ParentMenuId { get; set; }

        [Required]
        [Display(Name = "Menu Name")]
        public string MenuName { get; set; }

        [Display(Name = "Description")]
        public string MenuDescription { get; set; }

        [Display(Name = "Area")]
        public string AreaName { get; set; }

        [Display(Name = "Controller")]
        public string ControllerName { get; set; }

        [Display(Name = "Action")]
        public string ActionName { get; set; }

        [Display(Name = "Route Values")]
        public string RouteValues { get; set; }

        [Display(Name = "Custom URL")]
        public string CustomUrl { get; set; }

        [Display(Name = "Menu Type")]
        public string MenuType { get; set; }

        [Display(Name = "Icon")]
        public string IconClass { get; set; }

        [Display(Name = "CSS Class")]
        public string CssClass { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }

        public bool ShowOnHome { get; set; }

        public bool ShowInAdminSidebar { get; set; }

        public bool OpenInNewTab { get; set; }

        public bool IsClickable { get; set; }

        public bool HasMegaMenu { get; set; }

        public int MenuLevel { get; set; }

        public bool ShowInFooter { get; set; }

        public bool ShowInQuickLinks { get; set; }

        public bool IsSystemMenu { get; set; }

        public string PageTitle { get; set; }

        public string MetaDescription { get; set; }

        public List<SelectListItem> ParentMenus { get; set; }
    }
}