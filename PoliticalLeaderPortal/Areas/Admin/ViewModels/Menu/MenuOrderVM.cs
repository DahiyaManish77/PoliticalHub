namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.Menu
{
    public class MenuOrderVM
    {
        public int MenuId { get; set; }

        public int? ParentMenuId { get; set; }

        public int DisplayOrder { get; set; }
    }
}