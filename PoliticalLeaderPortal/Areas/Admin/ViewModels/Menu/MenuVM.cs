using System.Collections.Generic;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.Menu
{
    public class MenuVM
    {
        public MenuVM()
        {
            Children = new List<MenuVM>();
        }

        public int MenuId { get; set; }

        public int? ParentMenuId { get; set; }

        public string MenuName { get; set; }

        public string MenuDescription { get; set; }

        public string AreaName { get; set; }

        public string ControllerName { get; set; }

        public string ActionName { get; set; }

        public string RouteValues { get; set; }

        public string CustomUrl { get; set; }

        public string IconClass { get; set; }

        public string CssClass { get; set; }

        public int DisplayOrder { get; set; }

        public bool OpenInNewTab { get; set; }

        public bool IsClickable { get; set; }

        public bool HasMegaMenu { get; set; }

        public List<MenuVM> Children { get; set; }
        public bool IsActive { get; set; }

        public bool IsExpanded { get; set; }
        public string PageTitle { get; set; }

        public string MetaDescription { get; set; }
    }
}