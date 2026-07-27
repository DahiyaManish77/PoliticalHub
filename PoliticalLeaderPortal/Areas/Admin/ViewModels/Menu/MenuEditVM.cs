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
            ShowOnDesktop = true;
            ShowOnMobile = true;
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

        [Display(Name = "Menu Location")]
        public string MenuLocation { get; set; }

        [Display(Name = "Login Required")]
        public bool RequireLogin { get; set; }

        [Display(Name = "Required Role")]
        public string RequiredRole { get; set; }

        [Display(Name = "Required Permission Key")]
        public string RequiredPermissionKey { get; set; }

        [Display(Name = "Campaign Context Required")]
        public bool RequireCampaignContext { get; set; }

        [Display(Name = "Feature Key")]
        public string FeatureKey { get; set; }

        [Display(Name = "Visible From")]
        public System.DateTime? VisibleFrom { get; set; }

        [Display(Name = "Visible Until")]
        public System.DateTime? VisibleUntil { get; set; }

        [Display(Name = "Show on Desktop")]
        public bool ShowOnDesktop { get; set; }

        [Display(Name = "Show on Mobile")]
        public bool ShowOnMobile { get; set; }

        [Display(Name = "Language Code")]
        public string LanguageCode { get; set; }

        [Display(Name = "Badge Key")]
        public string BadgeKey { get; set; }

        public List<SelectListItem> ParentMenus { get; set; }
    }
}