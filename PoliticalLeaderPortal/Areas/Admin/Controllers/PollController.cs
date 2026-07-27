using PoliticalLeaderPortal.Areas.Admin.Services.Poll;
using PoliticalLeaderPortal.Areas.Admin.ViewModels.Poll;
using PoliticalLeaderPortal.Infrastructure.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    [Authorize]
    public class PollController : Controller
    {
        private readonly PollService _service;

        public PollController()
        {
            _service = new PollService();
        }

        public ActionResult Index()
        {
            return View(_service.GetPolls());
        }

        public ActionResult Create()
        {
            return View("Form", _service.NewPoll());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PollEditVM model, string[] optionTexts)
        {
            return Save(model, optionTexts, "Poll created successfully.");
        }

        public ActionResult Edit(int id)
        {
            var model = _service.GetPollById(id);
            return model == null ? (ActionResult)HttpNotFound() : View("Form", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PollEditVM model, string[] optionTexts)
        {
            return Save(model, optionTexts, "Poll updated successfully.");
        }

        public ActionResult Results(int id)
        {
            string baseUrl = Request.Url != null ? Request.Url.GetLeftPart(UriPartial.Authority) : String.Empty;
            var model = _service.GetResults(id, baseUrl);
            return model == null ? (ActionResult)HttpNotFound() : View(model);
        }

        public ActionResult QrCode(int id)
        {
            string baseUrl = Request.Url != null ? Request.Url.GetLeftPart(UriPartial.Authority) : String.Empty;
            var model = _service.GetResults(id, baseUrl);
            if (model == null) return HttpNotFound();
            return File(QrCodeHelper.CreatePngBytes(model.PublicUrl), "image/png", "poll-" + id + "-qr.png");
        }

        public ActionResult ExportCsv(int id)
        {
            string baseUrl = Request.Url != null ? Request.Url.GetLeftPart(UriPartial.Authority) : String.Empty;
            var poll = _service.GetResults(id, baseUrl);
            if (poll == null) return HttpNotFound();

            var csv = new StringBuilder();
            csv.AppendLine("Vote ID,Submitted Date,Option,Name,Mobile,Area,Source,Consent,Valid");
            foreach (var vote in _service.GetVotesForExport(id))
            {
                csv.AppendLine(String.Join(",", new[]
                {
                    Csv(vote.PollVoteId.ToString()),
                    Csv(vote.SubmittedDate.ToString("yyyy-MM-dd HH:mm:ss")),
                    Csv(vote.OptionText), Csv(vote.RespondentName), Csv(vote.MobileNo),
                    Csv(vote.AreaName), Csv(vote.Source), Csv(vote.ConsentGiven ? "Yes" : "No"),
                    Csv(vote.IsValid ? "Yes" : "No")
                }));
            }
            var bytes = new UTF8Encoding(true).GetBytes(csv.ToString());
            return File(bytes, "text/csv", "poll-" + id + "-responses.csv");
        }

        [HttpPost]
        public JsonResult Publish(int id)
        {
            bool result = _service.PublishPoll(id, CurrentUserId());
            return Json(new
            {
                success = result,
                message = result
                    ? "Poll published successfully."
                    : "Poll needs at least two active options before publishing."
            });
        }

        [HttpPost]
        public JsonResult Close(int id)
        {
            _service.ChangeStatus(id, "Closed", CurrentUserId());
            return Json(new { success = true, message = "Poll closed successfully." });
        }

        [HttpPost]
        public JsonResult Archive(int id)
        {
            _service.ChangeStatus(id, "Archived", CurrentUserId());
            return Json(new { success = true, message = "Poll archived successfully." });
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            bool result = _service.DeletePoll(id, CurrentUserId());
            return Json(new { success = result, message = "Poll removed or archived successfully." });
        }

        private ActionResult Save(PollEditVM model, IEnumerable<string> optionTexts, string message)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = _service.GetCategoryDropdown();
                return View("Form", model);
            }

            try
            {
                _service.SavePoll(model, optionTexts, CurrentUserId());
                TempData["Success"] = message;
                return RedirectToAction("Index");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                model.Categories = _service.GetCategoryDropdown();
                return View("Form", model);
            }
        }

        private int? CurrentUserId()
        {
            int userId;
            return Session["UserId"] != null && Int32.TryParse(Session["UserId"].ToString(), out userId)
                ? (int?)userId
                : null;
        }

        private static string Csv(string value)
        {
            return "\"" + (value ?? String.Empty).Replace("\"", "\"\"") + "\"";
        }
    }
}
