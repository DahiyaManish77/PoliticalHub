using PoliticalLeaderPortal.Services;
using PoliticalLeaderPortal.ViewModels;
using System;
using System.IO;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Controllers
{
    public class CitizenConnectController : Controller
    {
        private readonly CitizenConnectService service = new CitizenConnectService();

        public ActionResult ContactUs()
        {
            return Form("Contact", "Contact Office", "Share your issue, request or message with the office team.");
        }

        public ActionResult Suggestion()
        {
            return Form("Suggestion", "Send Suggestion", "Share your development idea, feedback or recommendation.");
        }

        public ActionResult Volunteer()
        {
            return Form("Volunteer", "Become a Volunteer", "Join the field team and support public service activities.");
        }

        public ActionResult Issue()
        {
            return Form("Issue", "Raise an Issue", "Report a local concern for review and follow-up by the office team.");
        }

        public ActionResult Appointment()
        {
            return Form("Appointment", "Request an Appointment", "Request a meeting with the public office and share the purpose of your visit.");
        }

        public ActionResult Invitation()
        {
            return Form("Invitation", "Send an Invitation", "Invite the leader or office team and attach your invitation card if available.");
        }

        [ChildActionOnly]
        public ActionResult HomeSection()
        {
            var model = new HomeCitizenConnectVM();
            service.LoadGeography(model.Contact);
            service.LoadGeography(model.Suggestion);
            service.LoadGeography(model.Volunteer);
            return PartialView("~/Views/CitizenConnect/Partials/_CitizenConnect.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Submit(CitizenConnectVM model)
        {
            model.RequestType = NormalizeRequestType(model.RequestType);
            bool isVolunteer = String.Equals(model.RequestType, "Volunteer", StringComparison.OrdinalIgnoreCase);

            if (!isVolunteer)
            {
                ModelState.Remove("PrivacyConsent");
                ModelState.Remove("PreferredRole");
                model.PrivacyConsent = false;
            }
            else
            {
                model.Subject = "Volunteer Application";
                if (String.IsNullOrWhiteSpace(model.PreferredRole))
                    ModelState.AddModelError("PreferredRole", "Please select a preferred volunteer role.");
            }

            if (!ModelState.IsValid)
                return InvalidForm(model);

            try
            {
                AttachUploadedFile(model);
                bool saved = service.Save(model);
                if (saved)
                    TempData["SuccessMessage"] = isVolunteer
                        ? "Your volunteer application has been submitted successfully."
                        : "Your request has been submitted successfully.";
                else
                    TempData["WarningMessage"] = "A similar request was submitted from this mobile number during the last 24 hours.";

                return RedirectToAction(GetAction(model.RequestType));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return InvalidForm(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitQuick(CitizenConnectVM model)
        {
            model.RequestType = NormalizeRequestType(model.RequestType);
            bool isVolunteer = String.Equals(model.RequestType, "Volunteer", StringComparison.OrdinalIgnoreCase);

            if (isVolunteer)
            {
                model.Subject = "Volunteer Application";
                model.PreferredRole = String.IsNullOrWhiteSpace(model.PreferredRole) ? "Public Outreach" : model.PreferredRole;
                model.Message = String.IsNullOrWhiteSpace(model.Message) ? "Volunteer application submitted from the home page." : model.Message;
                model.PrivacyConsent = true;
            }
            else
            {
                ModelState.Remove("PrivacyConsent");
                ModelState.Remove("PreferredRole");
            }

            if (!String.IsNullOrWhiteSpace(model.VillageName))
            {
                model.Message = (model.Message ?? "") + Environment.NewLine + "Village: " + model.VillageName.Trim();
            }

            if (!ModelState.IsValid)
            {
                TempData["WarningMessage"] = "Please complete all required fields with valid information.";
                return Redirect(Url.Action("Index", "Home") + "#get-involved");
            }

            try
            {
                AttachUploadedFile(model);
                bool saved = service.Save(model);
                TempData[saved ? "SuccessMessage" : "WarningMessage"] = saved
                    ? "Your " + model.RequestType.ToLowerInvariant() + " request has been submitted successfully."
                    : "A similar request was submitted from this mobile number during the last 24 hours.";
            }
            catch (Exception ex)
            {
                TempData["WarningMessage"] = ex.Message;
            }

            return Redirect(Url.Action("Index", "Home") + "#get-involved");
        }

        [HttpGet]
        public JsonResult GeographyOptions(string type, int? parentId, string parentType = null)
        {
            return Json(service.GetGeography(type, parentId, parentType), JsonRequestBehavior.AllowGet);
        }

        private ActionResult InvalidForm(CitizenConnectVM model)
        {
            SetHeading(model.RequestType);
            service.LoadGeography(model);
            return View("Form", model);
        }

        private ActionResult Form(string type, string title, string description)
        {
            ViewBag.FormTitle = title;
            ViewBag.FormDescription = description;
            var model = new CitizenConnectVM
            {
                RequestType = type,
                Status = "New",
                Subject = type == "Volunteer" ? "Volunteer Application" : null
            };
            service.LoadGeography(model);
            return View("Form", model);
        }

        private void SetHeading(string type)
        {
            ViewBag.FormTitle = GetTitle(type);
            ViewBag.FormDescription = GetDescription(type);
        }

        private static string NormalizeRequestType(string type)
        {
            if (String.Equals(type, "Volunteer", StringComparison.OrdinalIgnoreCase)) return "Volunteer";
            if (String.Equals(type, "Suggestion", StringComparison.OrdinalIgnoreCase)) return "Suggestion";
            if (String.Equals(type, "Issue", StringComparison.OrdinalIgnoreCase)) return "Issue";
            if (String.Equals(type, "Appointment", StringComparison.OrdinalIgnoreCase)) return "Appointment";
            if (String.Equals(type, "Invitation", StringComparison.OrdinalIgnoreCase)) return "Invitation";
            return "Contact";
        }

        private static string GetAction(string type)
        {
            if (String.Equals(type, "Volunteer", StringComparison.OrdinalIgnoreCase)) return "Volunteer";
            if (String.Equals(type, "Suggestion", StringComparison.OrdinalIgnoreCase)) return "Suggestion";
            if (String.Equals(type, "Issue", StringComparison.OrdinalIgnoreCase)) return "Issue";
            if (String.Equals(type, "Appointment", StringComparison.OrdinalIgnoreCase)) return "Appointment";
            if (String.Equals(type, "Invitation", StringComparison.OrdinalIgnoreCase)) return "Invitation";
            return "ContactUs";
        }

        private static string GetTitle(string type)
        {
            if (String.Equals(type, "Volunteer", StringComparison.OrdinalIgnoreCase)) return "Become a Volunteer";
            if (String.Equals(type, "Suggestion", StringComparison.OrdinalIgnoreCase)) return "Send Suggestion";
            if (String.Equals(type, "Issue", StringComparison.OrdinalIgnoreCase)) return "Raise an Issue";
            if (String.Equals(type, "Appointment", StringComparison.OrdinalIgnoreCase)) return "Request an Appointment";
            if (String.Equals(type, "Invitation", StringComparison.OrdinalIgnoreCase)) return "Send an Invitation";
            return "Contact Office";
        }

        private static string GetDescription(string type)
        {
            if (String.Equals(type, "Volunteer", StringComparison.OrdinalIgnoreCase)) return "Join the field team and support public service activities.";
            if (String.Equals(type, "Suggestion", StringComparison.OrdinalIgnoreCase)) return "Share your development idea, feedback or recommendation.";
            if (String.Equals(type, "Issue", StringComparison.OrdinalIgnoreCase)) return "Report a local concern for review and follow-up.";
            if (String.Equals(type, "Appointment", StringComparison.OrdinalIgnoreCase)) return "Request a meeting and share the purpose of your visit.";
            if (String.Equals(type, "Invitation", StringComparison.OrdinalIgnoreCase)) return "Invite the leader or office team and attach your invitation card.";
            return "Share your issue, request or message with the office team.";
        }

        private void AttachUploadedFile(CitizenConnectVM model)
        {
            var file = model == null ? null : model.AttachmentFile;
            if (file == null || file.ContentLength <= 0) return;

            if (file.ContentLength > 10 * 1024 * 1024)
                throw new InvalidOperationException("The attachment must be 10 MB or smaller.");

            string extension = Path.GetExtension(file.FileName ?? "").ToLowerInvariant();
            string[] allowed = { ".pdf", ".jpg", ".jpeg", ".png", ".webp", ".gif", ".txt" };
            if (Array.IndexOf(allowed, extension) < 0)
                throw new InvalidOperationException("Only PDF, image, or TXT files are allowed.");

            string folder = Server.MapPath("~/Uploads/CitizenConnect/");
            Directory.CreateDirectory(folder);
            string fileName = Guid.NewGuid().ToString("N") + extension;
            file.SaveAs(Path.Combine(folder, fileName));
            string publicPath = "/Uploads/CitizenConnect/" + fileName;
            model.Message = (model.Message ?? "") + Environment.NewLine + "Attachment: " + publicPath;
        }
    }
}
