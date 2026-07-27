using PoliticalLeaderPortal.Areas.Admin.Infrastructure;
using PoliticalLeaderPortal.Services;
using System;
using System.Web;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    [RoleMenuAuthorize]
    public class PeopleSayController : Controller
    {
        private readonly PeopleSayService _service = new PeopleSayService();

        public ActionResult Index(string status = "Pending", string keyword = null)
        {
            return View(_service.GetAdmin(status, keyword));
        }

        public ActionResult Analytics()
        {
            return View(_service.GetAnalytics());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Moderate(int id, string status, string reason)
        {
            try
            {
                _service.SetStatus(id, status, reason, CurrentUserId());
                TempData["SuccessMessage"] = "Video status updated successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction("Index", new { status = status });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ApproveDownloadAndDelete(int id)
        {
            try
            {
                var download = _service.ApproveDownloadAndDelete(id, CurrentUserId());
                return File(download.Content, download.ContentType, download.FileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", new { status = "Pending" });
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult LeaderResponse(int id, HttpPostedFileBase responseVideo, string responseMessage)
        {
            try
            {
                _service.AddLeaderResponse(id, responseVideo, responseMessage);
                TempData["SuccessMessage"] = "Leader response video added successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction("Index", new { status = "Approved" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ModerateComment(int id, string status)
        {
            _service.SetCommentStatus(id, status, CurrentUserId());
            TempData["SuccessMessage"] = "Comment moderation updated.";
            return RedirectToAction("Index");
        }

        private int CurrentUserId()
        {
            int value;
            return Int32.TryParse(Convert.ToString(Session["UserId"]), out value) ? value : 0;
        }
    }
}
