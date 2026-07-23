using System.Collections.Generic;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels
{
    public class RoleMenuPermissionPageVM
    {
        public int SelectedRoleId { get; set; }
        public List<SelectListItem> Roles { get; set; }
        public List<RoleMenuPermissionItemVM> Menus { get; set; }

        public RoleMenuPermissionPageVM()
        {
            Roles = new List<SelectListItem>();
            Menus = new List<RoleMenuPermissionItemVM>();
        }
    }
}
