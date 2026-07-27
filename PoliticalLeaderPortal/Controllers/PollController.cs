using PoliticalLeaderPortal.Areas.Admin.Services.Poll;
using PoliticalLeaderPortal.ViewModels.Poll;
using System;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Controllers
{
    public class PollController : Controller
    {
        private readonly PollService _service;

        public PollController()
        {
            _service = new PollService();
        }

        public ActionResult Details(string slug, string source = "web")
        {
            var model = _service.GetPublicPoll(slug);
            if (model == null)
            {
                return HttpNotFound();
            }

            ViewBag.Source = String.IsNullOrWhiteSpace(source) ? "web" : source;
            ViewBag.IsOpen = _service.IsPollOpen(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Vote(PublicPollVoteVM model)
        {
            if (!ModelState.IsValid)
            {
                var poll = _service.GetPublicPoll(model.PublicSlug);
                ViewBag.Source = model.Source;
                ViewBag.IsOpen = _service.IsPollOpen(poll);
                return View("Details", poll);
            }

            try
            {
                _service.SaveVote(model, Request.UserHostAddress, Request.UserAgent);
                var poll = _service.GetPublicPoll(model.PublicSlug);
                return View("Thanks", poll);
            }
            catch (InvalidOperationException ex)
            {
                var poll = _service.GetPublicPoll(model.PublicSlug);
                ModelState.AddModelError("", ex.Message);
                ViewBag.Source = model.Source;
                ViewBag.IsOpen = _service.IsPollOpen(poll);
                return View("Details", poll);
            }
        }
    }
}
