using PoliticalLeaderPortal.Areas.Admin.Infrastructure;
using PoliticalLeaderPortal.Areas.Admin.ViewModels.VoiceAgent;
using PoliticalLeaderPortal.Services;
using System;
using System.IO;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    [RoleMenuAuthorize]
    public class VoiceAgentController : Controller
    {
        private readonly VoiceAgentService _service = new VoiceAgentService();
        private readonly BulkVoiceCallerService _bulkService = new BulkVoiceCallerService();

        public ActionResult Index()
        {
            string baseUrl = Request.Url == null ? null : Request.Url.GetLeftPart(UriPartial.Authority);
            return View(_service.GetDashboard(baseUrl));
        }

        public ActionResult Settings()
        {
            PopulateWebhookUrls();
            return View(_service.GetSetting());
        }

        public ActionResult BulkCaller()
        {
            string baseUrl = Request.Url == null ? null : Request.Url.GetLeftPart(UriPartial.Authority);
            return View(_bulkService.GetDashboard(baseUrl));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CreateBulkCampaign(BulkVoiceCampaignVM model, int[] selectedPersonIds)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Enter a campaign name and voice message.";
                return RedirectToAction("BulkCaller");
            }
            try
            {
                int id = _bulkService.CreateCampaign(model, selectedPersonIds, CurrentUserId());
                TempData["SuccessMessage"] = "Campaign created and consented-member queue prepared. Review it, then press Start.";
                return RedirectToAction("BulkCaller", new { id = id });
            }
            catch (Exception ex) { TempData["ErrorMessage"] = ex.Message; return RedirectToAction("BulkCaller"); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult BulkCampaignAction(int id, string command)
        {
            try
            {
                if (String.Equals(command, "start", StringComparison.OrdinalIgnoreCase)) _bulkService.Start(id);
                else if (String.Equals(command, "pause", StringComparison.OrdinalIgnoreCase)) _bulkService.Pause(id);
                else if (String.Equals(command, "resume", StringComparison.OrdinalIgnoreCase)) _bulkService.Resume(id);
                else if (String.Equals(command, "stop", StringComparison.OrdinalIgnoreCase)) _bulkService.Stop(id);
                else throw new InvalidOperationException("Unknown campaign command.");
                TempData["SuccessMessage"] = "Campaign command completed.";
            }
            catch (Exception ex) { TempData["ErrorMessage"] = ex.Message; }
            return RedirectToAction("BulkCaller");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Settings(VoiceAgentSettingVM model)
        {
            if (!ModelState.IsValid) { PopulateWebhookUrls(); return View(model); }
            _service.SaveSetting(model, CurrentUserId());
            TempData["SuccessMessage"] = "Voice Agent settings saved. Configure the displayed webhook in your paid voice provider.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult TestCall()
        {
            long id = _service.CreateTestCall();
            TempData["SuccessMessage"] = "A rural-issue test call was created successfully.";
            return RedirectToAction("Details", new { id = id });
        }

        public ActionResult Details(long id)
        {
            var model = _service.GetCall(id);
            return model == null ? (ActionResult)HttpNotFound() : View(model);
        }

        public ActionResult Recording(long id)
        {
            var call = _service.GetCall(id);
            if (call == null || String.IsNullOrWhiteSpace(call.LocalRecordingPath)) return HttpNotFound();
            string path = Server.MapPath("~/" + call.LocalRecordingPath.TrimStart('~', '/'));
            if (!System.IO.File.Exists(path)) return HttpNotFound();
            return File(path, "audio/mpeg", "call-" + id + Path.GetExtension(path));
        }

        private int CurrentUserId()
        {
            int value;
            return Int32.TryParse(Convert.ToString(Session["UserId"]), out value) ? value : 0;
        }

        private void PopulateWebhookUrls()
        {
            string baseUrl = Request.Url == null ? null : Request.Url.GetLeftPart(UriPartial.Authority);
            var dashboard = _service.GetDashboard(baseUrl);
            ViewBag.IncomingWebhookUrl = dashboard.IncomingWebhookUrl;
            ViewBag.StatusWebhookUrl = dashboard.StatusWebhookUrl;
            ViewBag.RecordingWebhookUrl = dashboard.RecordingWebhookUrl;
            ViewBag.HealthCheckUrl = dashboard.HealthCheckUrl;
        }
    }
}
