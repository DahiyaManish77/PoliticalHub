using PoliticalLeaderPortal.Services;
using PoliticalLeaderPortal.ViewModels.PeopleSay;
using System;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Controllers
{
    public class PeopleSayController : Controller
    {
        private readonly PeopleSayService _service = new PeopleSayService();

        [ChildActionOnly]
        public ActionResult HomeSection()
        {
            return PartialView("~/Views/Home/Partials/_PeopleSay.cshtml", _service.GetHome());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Submit(PeopleSaySubmissionVM model)
        {
            if (!ModelState.IsValid)
            {
                TempData["PeopleSayError"] = "Please complete all required fields and select a valid video.";
                return Redirect(Url.Action("Index", "Home") + "#people-say");
            }
            try
            {
                _service.Submit(model);
                TempData["PeopleSaySuccess"] = "Your video has been uploaded successfully and sent for administrator review.";
            }
            catch (Exception ex)
            {
                TempData["PeopleSayError"] = ex.Message;
            }
            return Redirect(Url.Action("Index", "Home") + "#people-say");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult Engage(int id, string type)
        {
            try
            {
                return Json(new { success = _service.RecordEngagement(id, type, VisitorKey()) });
            }
            catch
            {
                return Json(new { success = false, message = "Unable to record this action." });
            }
        }

        [HttpGet]
        public JsonResult Comments(int id)
        {
            return Json(_service.GetApprovedComments(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult Comment(PeopleSayCommentVM model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Name and comment are required." });
            try
            {
                bool saved = _service.AddComment(model);
                return Json(new { success = saved, message = saved ? "Comment submitted for moderation." : "Video not found." });
            }
            catch
            {
                return Json(new { success = false, message = "Unable to submit comment." });
            }
        }

        [HttpGet]
        public ActionResult Download(int id)
        {
            var video = _service.GetApprovedVideo(id);
            if (video == null) return HttpNotFound();
            string fullPath = Server.MapPath(video.VideoPath);
            if (!System.IO.File.Exists(fullPath)) return HttpNotFound();
            _service.RecordEngagement(id, "Download", VisitorKey());
            return File(fullPath, MimeMapping.GetMimeMapping(fullPath), Path.GetFileName(fullPath));
        }

        private string VisitorKey()
        {
            const string cookieName = "people_say_visitor";
            HttpCookie cookie = Request.Cookies[cookieName];
            if (cookie != null && !String.IsNullOrWhiteSpace(cookie.Value)) return cookie.Value;
            string value = Guid.NewGuid().ToString("N");
            Response.Cookies.Add(new HttpCookie(cookieName, value)
            {
                HttpOnly = true,
                Secure = Request.IsSecureConnection,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddYears(1)
            });
            return value;
        }
    }
}
