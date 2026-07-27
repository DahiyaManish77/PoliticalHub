using PoliticalLeaderPortal.Areas.Admin.ViewModels.Menu;
using PoliticalLeaderPortal.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace PoliticalLeaderPortal.Areas.Admin.Services
{
    internal sealed class MenuConditionRow
    {
        public int MenuId { get; set; }
        public string MenuLocation { get; set; }
        public bool RequireLogin { get; set; }
        public string RequiredRole { get; set; }
        public string RequiredPermissionKey { get; set; }
        public bool RequireCampaignContext { get; set; }
        public string FeatureKey { get; set; }
        public DateTime? VisibleFrom { get; set; }
        public DateTime? VisibleUntil { get; set; }
        public bool ShowOnDesktop { get; set; }
        public bool ShowOnMobile { get; set; }
        public string LanguageCode { get; set; }
        public string BadgeKey { get; set; }
    }

    public sealed class MenuConditionService
    {
        private readonly PoliticalLeaderPortalDbEntities1 db;

        public MenuConditionService(PoliticalLeaderPortalDbEntities1 context)
        {
            db = context;
        }

        public HashSet<int> GetVisibleMenuIds(IEnumerable<int> menuIds, string location)
        {
            var ids = (menuIds ?? Enumerable.Empty<int>()).Distinct().ToList();
            if (ids.Count == 0 || !TableExists()) return new HashSet<int>(ids);

            var rows = db.Database.SqlQuery<MenuConditionRow>(
                @"SELECT MenuId, MenuLocation, RequireLogin, RequiredRole,
                         RequiredPermissionKey, RequireCampaignContext, FeatureKey,
                         VisibleFrom, VisibleUntil, ShowOnDesktop, ShowOnMobile,
                         LanguageCode, BadgeKey
                  FROM dbo.MenuCondition").ToList();

            var byMenu = rows.ToDictionary(x => x.MenuId);
            return new HashSet<int>(ids.Where(id => !byMenu.ContainsKey(id) || IsVisible(byMenu[id], location)));
        }

        public void Populate(MenuEditVM vm)
        {
            if (vm == null || vm.MenuId <= 0 || !TableExists()) return;
            var row = db.Database.SqlQuery<MenuConditionRow>(
                @"SELECT MenuId, MenuLocation, RequireLogin, RequiredRole,
                         RequiredPermissionKey, RequireCampaignContext, FeatureKey,
                         VisibleFrom, VisibleUntil, ShowOnDesktop, ShowOnMobile,
                         LanguageCode, BadgeKey
                  FROM dbo.MenuCondition WHERE MenuId = @MenuId",
                new SqlParameter("@MenuId", vm.MenuId)).FirstOrDefault();
            if (row == null) return;

            vm.MenuLocation = row.MenuLocation;
            vm.RequireLogin = row.RequireLogin;
            vm.RequiredRole = row.RequiredRole;
            vm.RequiredPermissionKey = row.RequiredPermissionKey;
            vm.RequireCampaignContext = row.RequireCampaignContext;
            vm.FeatureKey = row.FeatureKey;
            vm.VisibleFrom = row.VisibleFrom;
            vm.VisibleUntil = row.VisibleUntil;
            vm.ShowOnDesktop = row.ShowOnDesktop;
            vm.ShowOnMobile = row.ShowOnMobile;
            vm.LanguageCode = row.LanguageCode;
            vm.BadgeKey = row.BadgeKey;
        }

        public void Save(MenuEditVM vm)
        {
            if (vm == null || vm.MenuId <= 0 || !TableExists()) return;
            db.Database.ExecuteSqlCommand(@"
IF EXISTS (SELECT 1 FROM dbo.MenuCondition WHERE MenuId = @MenuId)
BEGIN
 UPDATE dbo.MenuCondition SET MenuLocation=@MenuLocation, RequireLogin=@RequireLogin,
 RequiredRole=@RequiredRole, RequiredPermissionKey=@RequiredPermissionKey,
 RequireCampaignContext=@RequireCampaignContext, FeatureKey=@FeatureKey,
 VisibleFrom=@VisibleFrom, VisibleUntil=@VisibleUntil, ShowOnDesktop=@ShowOnDesktop,
 ShowOnMobile=@ShowOnMobile, LanguageCode=@LanguageCode, BadgeKey=@BadgeKey,
 ModifiedDate=GETDATE() WHERE MenuId=@MenuId;
END
ELSE
BEGIN
 INSERT dbo.MenuCondition(MenuId,MenuLocation,RequireLogin,RequiredRole,RequiredPermissionKey,
 RequireCampaignContext,FeatureKey,VisibleFrom,VisibleUntil,ShowOnDesktop,ShowOnMobile,
 LanguageCode,BadgeKey,CreatedDate)
 VALUES(@MenuId,@MenuLocation,@RequireLogin,@RequiredRole,@RequiredPermissionKey,
 @RequireCampaignContext,@FeatureKey,@VisibleFrom,@VisibleUntil,@ShowOnDesktop,@ShowOnMobile,
 @LanguageCode,@BadgeKey,GETDATE());
END",
                P("@MenuId", vm.MenuId), P("@MenuLocation", vm.MenuLocation), P("@RequireLogin", vm.RequireLogin),
                P("@RequiredRole", vm.RequiredRole), P("@RequiredPermissionKey", vm.RequiredPermissionKey),
                P("@RequireCampaignContext", vm.RequireCampaignContext), P("@FeatureKey", vm.FeatureKey),
                P("@VisibleFrom", vm.VisibleFrom), P("@VisibleUntil", vm.VisibleUntil),
                P("@ShowOnDesktop", vm.ShowOnDesktop), P("@ShowOnMobile", vm.ShowOnMobile),
                P("@LanguageCode", vm.LanguageCode), P("@BadgeKey", vm.BadgeKey));
        }

        private bool IsVisible(MenuConditionRow row, string location)
        {
            var now = DateTime.Now;
            if (!String.IsNullOrWhiteSpace(row.MenuLocation) &&
                !String.Equals(row.MenuLocation, location, StringComparison.OrdinalIgnoreCase)) return false;
            if (row.VisibleFrom.HasValue && row.VisibleFrom.Value > now) return false;
            if (row.VisibleUntil.HasValue && row.VisibleUntil.Value < now) return false;

            var context = HttpContext.Current;
            var isAuthenticated = context != null && context.User != null && context.User.Identity.IsAuthenticated;
            if (row.RequireLogin && !isAuthenticated) return false;

            var roleName = context != null && context.Session != null ? Convert.ToString(context.Session["RoleName"]) : null;
            if (!String.IsNullOrWhiteSpace(row.RequiredRole) &&
                !row.RequiredRole.Split(',').Any(x => String.Equals(x.Trim(), roleName, StringComparison.OrdinalIgnoreCase))) return false;

            if (row.RequireCampaignContext && (context == null || context.Session == null || context.Session["CampaignId"] == null)) return false;

            if (!String.IsNullOrWhiteSpace(row.FeatureKey))
            {
                bool enabled;
                if (!Boolean.TryParse(ConfigurationManager.AppSettings[row.FeatureKey], out enabled) || !enabled) return false;
            }

            var mobile = context != null && context.Request != null && context.Request.Browser != null && context.Request.Browser.IsMobileDevice;
            if (mobile && !row.ShowOnMobile) return false;
            if (!mobile && !row.ShowOnDesktop) return false;

            if (!String.IsNullOrWhiteSpace(row.LanguageCode) && context != null && context.Session != null)
            {
                var language = Convert.ToString(context.Session["LanguageCode"]);
                if (!String.IsNullOrWhiteSpace(language) && !String.Equals(language, row.LanguageCode, StringComparison.OrdinalIgnoreCase)) return false;
            }
            return true;
        }

        private bool TableExists()
        {
            return db.Database.SqlQuery<int>("SELECT CASE WHEN OBJECT_ID('dbo.MenuCondition','U') IS NULL THEN 0 ELSE 1 END").First() == 1;
        }

        private static SqlParameter P(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }
    }
}
