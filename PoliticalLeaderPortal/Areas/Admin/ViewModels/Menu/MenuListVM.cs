namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.Menu
{
    public class MenuListVM
    {
        public int MenuId { get; set; }

        public string MenuName { get; set; }

        public string ParentMenuName { get; set; }

        public string MenuType { get; set; }

        public string AreaName { get; set; }

        public string ControllerName { get; set; }

        public string ActionName { get; set; }

        public string IconClass { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }

        public bool ShowOnHome { get; set; }

        public bool ShowInAdminSidebar { get; set; }

        public bool ShowInFooter { get; set; }

        public bool ShowInQuickLinks { get; set; }

        public bool IsSystemMenu { get; set; }
    }
}