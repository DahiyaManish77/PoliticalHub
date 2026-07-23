namespace PoliticalLeaderPortal.Areas.Admin.ViewModels
{
    public class RoleMenuPermissionItemVM
    {
        public int MenuId { get; set; }
        public int? ParentMenuId { get; set; }
        public string MenuName { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string IconClass { get; set; }
        public int MenuLevel { get; set; }
        public bool IsAllowed { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}
