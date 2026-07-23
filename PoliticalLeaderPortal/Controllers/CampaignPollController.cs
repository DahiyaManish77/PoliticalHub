using System.Web.Mvc;
using PoliticalLeaderPortal.Areas.Admin.Services;
using PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom;

namespace PoliticalLeaderPortal.Controllers
{
    public class CampaignPollController : Controller
    {
        private readonly ElectionWarRoomService _service;

        public CampaignPollController()
        {
            _service = new ElectionWarRoomService();
        }

        public ActionResult Details(string slug, string source = "web")
        {
            CampaignPollVM model = _service.GetPublicCampaignPoll(slug, Request.Url != null ? Request.Url.GetLeftPart(System.UriPartial.Authority) : string.Empty);
            if (model == null) return HttpNotFound();
            ViewBag.Source = source;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Submit(string slug, string source, CampaignPollResponseVM model)
        {
            if (string.IsNullOrWhiteSpace(source)) source = "web";
            model.Source = source;
            _service.SaveCampaignPollResponse(slug, model, Request.UserHostAddress, Request.UserAgent);
            CampaignPollVM poll = _service.GetPublicCampaignPoll(slug, Request.Url != null ? Request.Url.GetLeftPart(System.UriPartial.Authority) : string.Empty);
            return View("Thanks", poll);
        }
    }
}
