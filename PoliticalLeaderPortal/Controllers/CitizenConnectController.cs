using PoliticalLeaderPortal.Services;
using PoliticalLeaderPortal.ViewModels;
using System;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Controllers
{
    public class CitizenConnectController : Controller
    {
        private readonly CitizenConnectService service;

        public CitizenConnectController()
        {
            service = new CitizenConnectService();
        }

        public ActionResult ContactUs()
        {
            return View("Form", BuildModel("Contact", "Contact Office", "Share your issue, request or message with the office team."));
        }

        public ActionResult Volunteer()
        {
            return View("Form", BuildModel("Volunteer", "Become a Volunteer", "Join the field team and support public service activities."));
        }

        public ActionResult Suggestion()
        {
            return View("Form", BuildModel("Suggestion", "Send Suggestion", "Share your development idea, feedback or recommendation."));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Submit(CitizenConnectVM model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.FormTitle = GetTitle(model.RequestType);
                ViewBag.FormDescription = GetDescription(model.RequestType);
                return View("Form", model);
            }

            bool saved = service.Save(model);
            TempData[saved ? "SuccessMessage" : "WarningMessage"] = saved
                ? "Your request has been submitted successfully."
                : "This request already exists. Our team will review the earlier submission.";

            return RedirectToAction(GetAction(model.RequestType));
        }

        private CitizenConnectVM BuildModel(string type, string title, string description)
        {
            ViewBag.FormTitle = title;
            ViewBag.FormDescription = description;
            return new CitizenConnectVM
            {
                RequestType = type,
                Status = "New"
            };
        }

        private static string GetAction(string requestType)
        {
            if (String.Equals(requestType, "Volunteer", StringComparison.OrdinalIgnoreCase)) return "Volunteer";
            if (String.Equals(requestType, "Suggestion", StringComparison.OrdinalIgnoreCase)) return "Suggestion";
            return "ContactUs";
        }

        private static string GetTitle(string requestType)
        {
            if (String.Equals(requestType, "Volunteer", StringComparison.OrdinalIgnoreCase)) return "Become a Volunteer";
            if (String.Equals(requestType, "Suggestion", StringComparison.OrdinalIgnoreCase)) return "Send Suggestion";
            return "Contact Office";
        }

        private static string GetDescription(string requestType)
        {
            if (String.Equals(requestType, "Volunteer", StringComparison.OrdinalIgnoreCase)) return "Join the field team and support public service activities.";
            if (String.Equals(requestType, "Suggestion", StringComparison.OrdinalIgnoreCase)) return "Share your development idea, feedback or recommendation.";
            return "Share your issue, request or message with the office team.";
        }
    }
}
