using System;
using PoliticalLeaderPortal.Areas.Admin.Services;
using System.Web.Mvc;
using System.Text;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly DashboardService _service;

        public DashboardController()
        {
            _service = new DashboardService();
        }

        public ActionResult Index()
        {
            ViewBag.Title = "Dashboard";
            int? campaignId = CurrentCampaignId();
            return View(_service.GetDashboard(campaignId));
        }

        public ActionResult ExportCsv()
        {
            int? campaignId = CurrentCampaignId();
            var model = _service.GetDashboard(campaignId);
            var csv = new StringBuilder();
            csv.AppendLine("Sangeet Som Campaign Command Report");
            csv.AppendLine("Generated," + Csv(DateTime.Now.ToString("dd MMM yyyy hh:mm tt")));
            csv.AppendLine("Campaign," + Csv(campaignId.HasValue ? _service.GetCampaignName(campaignId.Value) : "All campaigns"));
            csv.AppendLine();
            csv.AppendLine("Metric,Value,Context");
            foreach (var item in model.Metrics)
            {
                csv.AppendLine(Csv(item.Label) + "," + Csv(item.Value) + "," + Csv(item.Hint));
            }
            csv.AppendLine();
            csv.AppendLine("Readiness,Percentage");
            foreach (var item in model.OverviewItems)
            {
                csv.AppendLine(Csv(item.Label) + "," + item.Percentage + "%");
            }
            csv.AppendLine();
            csv.AppendLine("Operational summary," + Csv(model.TodaySummary));

            byte[] content = new UTF8Encoding(true).GetBytes(csv.ToString());
            return File(content, "text/csv", "campaign-command-" + DateTime.Now.ToString("yyyyMMdd-HHmm") + ".csv");
        }

        [ChildActionOnly]
        public ActionResult CampaignSelector()
        {
            int? campaignId = CurrentCampaignId();
            if (!campaignId.HasValue)
            {
                campaignId = _service.GetDefaultCampaignId();
                if (campaignId.HasValue)
                {
                    Session["CampaignId"] = campaignId.Value;
                    Session["CampaignName"] = _service.GetCampaignName(campaignId.Value);
                }
            }

            return PartialView(
                "~/Areas/Admin/Views/Shared/_CampaignSelector.cshtml",
                _service.GetCampaignContext(campaignId, Request.RawUrl));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetCampaign(int? campaignId, string returnUrl)
        {
            if (campaignId.HasValue)
            {
                string campaignName = _service.GetCampaignName(campaignId.Value);
                if (String.IsNullOrWhiteSpace(campaignName))
                {
                    return new HttpStatusCodeResult(400, "The selected campaign is unavailable.");
                }

                Session["CampaignId"] = campaignId.Value;
                Session["CampaignName"] = campaignName;
            }
            else
            {
                Session.Remove("CampaignId");
                Session.Remove("CampaignName");
            }

            if (!String.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index");
        }

        private int? CurrentCampaignId()
        {
            int campaignId;
            return Int32.TryParse(Convert.ToString(Session["CampaignId"]), out campaignId)
                ? (int?)campaignId
                : null;
        }

        private static string Csv(string value)
        {
            string text = value ?? String.Empty;
            return "\"" + text.Replace("\"", "\"\"") + "\"";
        }
    }
}
