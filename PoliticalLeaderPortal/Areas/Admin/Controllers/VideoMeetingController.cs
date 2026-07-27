using PoliticalLeaderPortal.Areas.Admin.Infrastructure;
using PoliticalLeaderPortal.Areas.Admin.ViewModels.VideoMeeting;
using PoliticalLeaderPortal.Services;
using System;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    [RoleMenuAuthorize]
    public class VideoMeetingController : Controller
    {
        private readonly VideoMeetingService _service = new VideoMeetingService();

        public ActionResult Index()
        {
            var model = _service.GetDashboard();
            model.Capabilities = new[]
            {
                "Secure meeting scheduling and invitation management",
                "Host approval, participant camera and microphone controls",
                "Maximum-participant and meeting-duration controls",
                "Optional recording policy with explicit host configuration"
            };
            return View(model);
        }

        public ActionResult Create()
        {
            return View("Form", new VideoMeetingEditVM
            {
                MeetingType = "Private Leadership Meeting",
                ScheduledStart = DateTime.Now.AddHours(1),
                DurationMinutes = 60,
                MaximumParticipants = 25,
                AllowParticipantCamera = true,
                AllowParticipantMicrophone = true,
                RequireHostApproval = true,
                Status = "Scheduled"
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(VideoMeetingEditVM model)
        {
            if (!ModelState.IsValid) return View("Form", model);
            try
            {
                _service.Save(model, CurrentUserId());
                TempData["SuccessMessage"] = "Video meeting scheduled successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Form", model);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Status(int id, string status)
        {
            try
            {
                _service.SetStatus(id, status, CurrentUserId());
                TempData["SuccessMessage"] = "Meeting status updated.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction("Index");
        }

        private int CurrentUserId()
        {
            int value;
            return Int32.TryParse(Convert.ToString(Session["UserId"]), out value) ? value : 0;
        }
    }
}
