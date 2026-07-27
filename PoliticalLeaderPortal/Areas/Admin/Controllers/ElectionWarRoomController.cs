using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using PoliticalLeaderPortal.Areas.Admin.Infrastructure;
using PoliticalLeaderPortal.Areas.Admin.Services;
using PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    [RoleMenuAuthorize]
    public class ElectionWarRoomController : Controller
    {
        private readonly ElectionWarRoomService _service;
        private readonly RoleMenuPermissionService _permissionService;

        public ElectionWarRoomController()
        {
            _service = new ElectionWarRoomService();
            _permissionService = new RoleMenuPermissionService();
        }

        public ActionResult Index()
        {
            ViewBag.Title = "Election War Room";
            return View(BuildDashboard());
        }

        public ActionResult CampaignErpBlueprint()
        {
            ViewBag.Title = "Campaign ERP Blueprint";
            return View(_service.GetCampaignErpBlueprint());
        }

        public ActionResult CandidateManagement(string keyword = null)
        {
            return Module("Candidate Management", "Candidate profile, affidavit references, media links, social links and approval workflow.", _service.GetCandidateProfiles(keyword), "CreateCandidate", "EditCandidate", "DeleteCandidate", "CandidateProfileId");
        }

        public ActionResult CreateCandidate()
        {
            if (!HasPermission("CanCreate")) return AccessDenied();
            return View("CandidateForm", new CandidateProfileVM
            {
                ApprovalStatus = "Draft",
                IsActive = true,
                State = "Uttar Pradesh"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCandidate(CandidateProfileVM model)
        {
            if (!HasPermission("CanCreate")) return AccessDenied();
            if (!ModelState.IsValid) return View("CandidateForm", model);

            _service.SaveCandidateProfile(model, CurrentUserId());
            TempData["Success"] = "Candidate profile created successfully.";
            return RedirectToAction("CandidateManagement");
        }

        public ActionResult EditCandidate(int id)
        {
            if (!HasPermission("CanEdit")) return AccessDenied();
            var model = _service.GetCandidateProfileById(id);
            if (model == null) return HttpNotFound();
            return View("CandidateForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCandidate(CandidateProfileVM model)
        {
            if (!HasPermission("CanEdit")) return AccessDenied();
            if (!ModelState.IsValid) return View("CandidateForm", model);

            _service.UpdateCandidateProfile(model, CurrentUserId());
            TempData["Success"] = "Candidate profile updated successfully.";
            return RedirectToAction("CandidateManagement");
        }

        [HttpPost]
        public JsonResult DeleteCandidate(int id)
        {
            if (!HasPermission("CanDelete")) return Json(new { success = false, message = "You do not have delete permission." });
            return Json(new { success = _service.DeleteCandidateProfile(id, CurrentUserId()), message = "Candidate profile deleted successfully." });
        }


        public ActionResult LeaderCampaignKit()
        {
            return View("CampaignErpModule", _service.GetCampaignErpModule("leader-kit"));
        }

        public ActionResult ManifestoTracker()
        {
            return View("CampaignErpModule", _service.GetCampaignErpModule("manifesto"));
        }

        public ActionResult BoothCommittee()
        {
            return View("CampaignErpModule", _service.GetCampaignErpModule("booth-committee"));
        }

        public ActionResult PageSocialCoordination()
        {
            return View("CampaignErpModule", _service.GetCampaignErpModule("page-social"));
        }

        public ActionResult RallyMaterialKit()
        {
            return View("CampaignErpModule", _service.GetCampaignErpModule("rally-material"));
        }

        public ActionResult CampaignTraining()
        {
            return View("CampaignErpModule", _service.GetCampaignErpModule("training"));
        }
        public ActionResult MembershipDrive()
        {
            return RedirectToAction("Index", "VerifiedDocument");
        }

        public ActionResult SocialMediaWarRoom(string keyword = null)
        {
            return Module("Social Media War Room", "Content calendar, approval queue, publishing status and platform performance tracking.", _service.GetSocialMediaPosts(keyword), "CreateSocialPost", "EditSocialPost", "DeleteSocialPost", "SocialMediaPostId");
        }

        public ActionResult CreateSocialPost()
        {
            if (!HasPermission("CanCreate")) return AccessDenied();
            return View("SocialMediaPostForm", new SocialMediaPostVM
            {
                Platform = "Facebook",
                ContentType = "Post",
                ApprovalStatus = "Draft",
                PublishStatus = "Planned",
                IsActive = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateSocialPost(SocialMediaPostVM model)
        {
            if (!HasPermission("CanCreate")) return AccessDenied();
            if (!ModelState.IsValid) return View("SocialMediaPostForm", model);

            _service.SaveSocialMediaPost(model, CurrentUserId());
            TempData["Success"] = "Social media post created successfully.";
            return RedirectToAction("SocialMediaWarRoom");
        }

        public ActionResult EditSocialPost(int id)
        {
            if (!HasPermission("CanEdit")) return AccessDenied();
            var model = _service.GetSocialMediaPostById(id);
            if (model == null) return HttpNotFound();
            return View("SocialMediaPostForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditSocialPost(SocialMediaPostVM model)
        {
            if (!HasPermission("CanEdit")) return AccessDenied();
            if (!ModelState.IsValid) return View("SocialMediaPostForm", model);

            _service.UpdateSocialMediaPost(model, CurrentUserId());
            TempData["Success"] = "Social media post updated successfully.";
            return RedirectToAction("SocialMediaWarRoom");
        }

        [HttpPost]
        public JsonResult DeleteSocialPost(int id)
        {
            if (!HasPermission("CanDelete")) return Json(new { success = false, message = "You do not have delete permission." });
            return Json(new { success = _service.DeleteSocialMediaPost(id, CurrentUserId()), message = "Social media post deleted successfully." });
        }


        public ActionResult CampaignPolls(string keyword = null)
        {
            return Module("Poll & Survey Management", "Create public feedback polls, collect lawful responses and share them on WhatsApp, Facebook or QR/public links.", _service.GetCampaignPolls(keyword), "CreatePoll", "EditPoll", "DeleteCampaignPoll", "CampaignPollId");
        }

        public ActionResult CreatePoll()
        {
            if (!HasPermission("CanCreate")) return AccessDenied();
            return View("CampaignPollForm", new CampaignPollVM
            {
                PollType = "Public Feedback",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(7),
                RequireConsent = true,
                IsActive = true,
                Options = new List<CampaignPollOptionVM>
                {
                    new CampaignPollOptionVM { OptionText = "Development" },
                    new CampaignPollOptionVM { OptionText = "Education" },
                    new CampaignPollOptionVM { OptionText = "Health" },
                    new CampaignPollOptionVM { OptionText = "Roads" }
                }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePoll(CampaignPollVM model, string[] optionTexts)
        {
            if (!HasPermission("CanCreate")) return AccessDenied();
            model.Options = BuildPollOptions(optionTexts);
            if (model.Options.Count < 2) ModelState.AddModelError("Options", "Please enter at least two poll options.");
            if (!ModelState.IsValid) return View("CampaignPollForm", model);

            _service.SaveCampaignPoll(model, CurrentUserId(), Request.Url != null ? Request.Url.GetLeftPart(UriPartial.Authority) : string.Empty);
            TempData["Success"] = "Poll created successfully.";
            return RedirectToAction("CampaignPolls");
        }

        public ActionResult EditPoll(int id)
        {
            if (!HasPermission("CanEdit")) return AccessDenied();
            var model = _service.GetCampaignPollById(id, Request.Url != null ? Request.Url.GetLeftPart(UriPartial.Authority) : string.Empty);
            if (model == null) return HttpNotFound();
            return View("CampaignPollForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPoll(CampaignPollVM model, string[] optionTexts)
        {
            if (!HasPermission("CanEdit")) return AccessDenied();
            model.Options = BuildPollOptions(optionTexts);
            if (model.Options.Count < 2) ModelState.AddModelError("Options", "Please enter at least two poll options.");
            if (!ModelState.IsValid) return View("CampaignPollForm", model);

            _service.UpdateCampaignPoll(model, CurrentUserId(), Request.Url != null ? Request.Url.GetLeftPart(UriPartial.Authority) : string.Empty);
            TempData["Success"] = "Poll updated successfully.";
            return RedirectToAction("CampaignPolls");
        }

        public ActionResult PollResults(int id)
        {
            var model = _service.GetCampaignPollById(id, Request.Url != null ? Request.Url.GetLeftPart(UriPartial.Authority) : string.Empty);
            if (model == null) return HttpNotFound();
            return View("CampaignPollResults", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteCampaignPoll(int id)
        {
            if (!HasPermission("CanDelete")) return Json(new { success = false, message = "You do not have delete permission." });
            return Json(new { success = _service.DeleteCampaignPoll(id, CurrentUserId()), message = "Poll deleted successfully." });
        }

        public ActionResult FinanceAndDonations(string keyword = null)
        {
            return Module("Finance and Donations", "Donation register, campaign spending, payment proof and approval tracking.", _service.GetCampaignFinanceEntries(keyword), "CreateFinanceEntry", "EditFinanceEntry", "DeleteFinanceEntry", "CampaignFinanceEntryId");
        }

        public ActionResult CreateFinanceEntry()
        {
            if (!HasPermission("CanCreate")) return AccessDenied();
            return View("CampaignFinanceEntryForm", new CampaignFinanceEntryVM
            {
                EntryType = "Expense",
                EntryDate = DateTime.Today,
                ApprovalStatus = "Pending",
                IsActive = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateFinanceEntry(CampaignFinanceEntryVM model)
        {
            if (!HasPermission("CanCreate")) return AccessDenied();
            if (!ModelState.IsValid) return View("CampaignFinanceEntryForm", model);

            _service.SaveCampaignFinanceEntry(model, CurrentUserId());
            TempData["Success"] = "Finance entry created successfully.";
            return RedirectToAction("FinanceAndDonations");
        }

        public ActionResult EditFinanceEntry(int id)
        {
            if (!HasPermission("CanEdit")) return AccessDenied();
            var model = _service.GetCampaignFinanceEntryById(id);
            if (model == null) return HttpNotFound();
            return View("CampaignFinanceEntryForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditFinanceEntry(CampaignFinanceEntryVM model)
        {
            if (!HasPermission("CanEdit")) return AccessDenied();
            if (!ModelState.IsValid) return View("CampaignFinanceEntryForm", model);

            _service.UpdateCampaignFinanceEntry(model, CurrentUserId());
            TempData["Success"] = "Finance entry updated successfully.";
            return RedirectToAction("FinanceAndDonations");
        }

        [HttpPost]
        public JsonResult DeleteFinanceEntry(int id)
        {
            if (!HasPermission("CanDelete")) return Json(new { success = false, message = "You do not have delete permission." });
            return Json(new { success = _service.DeleteCampaignFinanceEntry(id, CurrentUserId()), message = "Finance entry deleted successfully." });
        }

        public ActionResult ComplianceCenter(string keyword = null)
        {
            return Module("Compliance Center", "Audit trail for sensitive campaign actions, finance changes, candidate updates and media workflow changes.", _service.GetCampaignAuditLogs(keyword));
        }

        public ActionResult Events(string status = null, string keyword = null)
        {
            int? campaignId = SelectedCampaignId();
            IEnumerable<EventVM> records = !string.IsNullOrWhiteSpace(keyword)
                ? _service.SearchEvents(keyword, campaignId)
                : !string.IsNullOrWhiteSpace(status)
                    ? _service.GetEventsByStatus(status, campaignId)
                    : _service.GetEvents(campaignId);

            return Module("Events", "Campaign event planning, turnout, priority and status tracking.", records, "CreateEvent", "EditEvent", "DeleteEvent", "EventId");
        }

        public ActionResult RallyEvents(string status = null, string keyword = null)
        {
            return Events(status, keyword);
        }

        public ActionResult Vehicles(string keyword = null)
        {
            IEnumerable<EventVehicleVM> records = !string.IsNullOrWhiteSpace(keyword)
                ? _service.SearchVehicles(keyword)
                : _service.GetDashboardVehicles(200);
            records = ApplySelectedCampaign(records);

            return Module("Vehicles", "Vehicle allocation, owner details, village movement, passenger count, fuel cost and verification tracking.", records, "CreateVehicle", "EditVehicle", "DeleteVehicle", "EventVehicleId");
        }

        public ActionResult Attendance(string keyword = null)
        {
            IEnumerable<EventAttendanceVM> records = !string.IsNullOrWhiteSpace(keyword)
                ? _service.SearchAttendances(keyword)
                : _service.GetDashboardAttendances(200);
            records = ApplySelectedCampaign(records);

            return Module("Attendance", "Worker, volunteer and VIP attendance monitoring.", records, "CreateAttendance", "EditAttendance", "DeleteAttendance", "AttendanceId");
        }

        public ActionResult Teams(string keyword = null)
        {
            IEnumerable<EventTeamVM> records = !string.IsNullOrWhiteSpace(keyword)
                ? _service.SearchTeams(keyword)
                : _service.GetDashboardTeams(200);
            records = ApplySelectedCampaign(records);

            return Module("Teams", "Team ownership, assigned areas, members and completion status.", records, "CreateTeam", "EditTeam", "DeleteTeam", "EventTeamId");
        }

        public ActionResult Volunteers(string keyword = null)
        {
            IEnumerable<EventTeamVM> records = !string.IsNullOrWhiteSpace(keyword)
                ? _service.SearchTeams(keyword)
                : _service.GetDashboardTeams(200);
            records = ApplySelectedCampaign(records);

            return Module("Volunteer Management", "Volunteer teams, area assignment, leader contact and active field status.", records, "CreateTeam", "EditTeam", "DeleteTeam", "EventTeamId");
        }

        public ActionResult Guests(string keyword = null)
        {
            IEnumerable<EventGuestVM> records = !string.IsNullOrWhiteSpace(keyword)
                ? _service.SearchGuests(keyword)
                : _service.GetDashboardGuests(200);
            records = ApplySelectedCampaign(records);

            return Module("Guests", "Guest invitations, confirmation and logistics requirements.", records, "CreateGuest", "EditGuest", "DeleteGuest", "EventGuestId");
        }

        public ActionResult Arrangements(string keyword = null)
        {
            IEnumerable<EventArrangementVM> records = !string.IsNullOrWhiteSpace(keyword)
                ? _service.SearchArrangements(keyword)
                : _service.GetDashboardArrangements(200);
            records = ApplySelectedCampaign(records);

            return Module("Arrangements", "Venue, vendor, logistics and verification tracking.", records, "CreateArrangement", "EditArrangement", "DeleteArrangement", "EventArrangementId");
        }

        public ActionResult Expenses(string keyword = null)
        {
            IEnumerable<EventExpenseVM> records = !string.IsNullOrWhiteSpace(keyword)
                ? _service.SearchExpenses(keyword)
                : _service.GetExpenses();
            records = ApplySelectedCampaign(records);

            return Module("Expenses", "Budget, payment and approval monitoring.", records, "CreateExpense", "EditExpense", "DeleteExpense", "EventExpenseId");
        }

        public ActionResult Media(string keyword = null)
        {
            IEnumerable<EventMediaVM> records = !string.IsNullOrWhiteSpace(keyword)
                ? _service.SearchMedia(keyword)
                : _service.GetDashboardMedia(200);
            records = ApplySelectedCampaign(records);

            return Module("Media", "Photos, videos, documents and campaign media approvals.", records, "CreateMedia", "EditMedia", "DeleteMedia", "EventMediaId");
        }

        public ActionResult Tasks(string status = null, string keyword = null)
        {
            IEnumerable<EventTaskVM> records = !string.IsNullOrWhiteSpace(keyword)
                ? _service.SearchTasks(keyword)
                : !string.IsNullOrWhiteSpace(status) ? _service.GetTasksByStatus(status) : _service.GetDashboardTasks(200);
            records = ApplySelectedCampaign(records);

            return Module("Tasks", "Task assignment, progress, priority and completion monitoring.", records, "CreateTask", "EditTask", "DeleteTask", "EventTaskId");
        }

        public ActionResult CampaignTasks(string status = null, string keyword = null)
        {
            return Tasks(status, keyword);
        }

        public ActionResult Polls(string keyword = null)
        {
            IEnumerable<EventPollVM> records = !string.IsNullOrWhiteSpace(keyword)
                ? _service.SearchPolls(keyword)
                : _service.GetDashboardPolls(200);
            records = ApplySelectedCampaign(records);

            return Module("Event Polls", "Event poll publishing, survey responses and question tracking.", records, "CreateEventPoll", "EditEventPoll", "DeleteEventPoll", "EventPollId");
        }

        public ActionResult ElectionBooths(string keyword = null)
        {
            IEnumerable<ElectionBoothVM> records = !string.IsNullOrWhiteSpace(keyword)
                ? _service.SearchBooths(keyword)
                : _service.GetDashboardBooths(200);

            return Module("Election Booths", "Booth coverage, priority, voter strength and follow-up planning.", records, "CreateBooth", "EditBooth", "DeleteBooth", "ElectionBoothId");
        }

        public ActionResult BoothMonitoring(string keyword = null)
        {
            return ElectionBooths(keyword);
        }

        public ActionResult BoothVisits(string keyword = null)
        {
            IEnumerable<ElectionBoothVisitVM> records = !string.IsNullOrWhiteSpace(keyword)
                ? _service.SearchBoothVisits(keyword)
                : _service.GetRecentBoothVisits(200);

            return Module("Booth Visits", "Booth visit activity, house coverage, supporter and complaint records.", records, "CreateBoothVisit", "EditBoothVisit", "DeleteBoothVisit", "ElectionBoothVisitId");
        }

        public ActionResult JanSampark(string keyword = null)
        {
            IEnumerable<JanSamparkVM> records = !string.IsNullOrWhiteSpace(keyword)
                ? _service.SearchJanSampark(keyword)
                : _service.GetRecentJanSampark(200);
            records = ApplySelectedCampaign(records);

            return Module("Jan Sampark", "Public contact, complaint, assignment and resolution tracking.", records, "CreateJanSampark", "EditJanSampark", "DeleteJanSampark", "JanSamparkId");
        }

        public ActionResult PeopleConnect(string keyword = null)
        {
            return JanSampark(keyword);
        }

        public ActionResult ElectionCalendar(string keyword = null)
        {
            IEnumerable<EventVM> records = !string.IsNullOrWhiteSpace(keyword)
                ? _service.SearchEvents(keyword)
                : _service.GetUpcomingEvents();

            return Module("Election Calendar", "Meetings, rallies, campaign programmes and important political dates.", records);
        }

        public ActionResult Campaigns(string status = null, string keyword = null)
        {
            IEnumerable<ElectionCampaignVM> records = !string.IsNullOrWhiteSpace(keyword)
                ? _service.SearchElectionCampaign(keyword)
                : !string.IsNullOrWhiteSpace(status) ? _service.GetElectionCampaignByStatus(status) : _service.GetDashboardElectionCampaigns();

            return Module("Campaigns", "Election campaign schedule, scope, state and status management.", records, "CreateCampaign", "EditCampaign", "DeleteCampaign", "CampaignId");
        }

        public ActionResult CampaignAlerts(string keyword = null)
        {
            int? campaignId = SelectedCampaignId();
            IEnumerable<CampaignAlertVM> records = !string.IsNullOrWhiteSpace(keyword)
                ? _service.SearchCampaignAlerts(keyword, null, null, campaignId)
                : campaignId.HasValue
                    ? _service.GetCampaignAlertsByCampaign(campaignId.Value)
                    : _service.GetDashboardAlerts();

            return Module("Campaign Alerts", "Critical alerts, assigned action, read status and resolution monitoring.", records, "CreateCampaignAlert", "EditCampaignAlert", "DeleteCampaignAlert", "CampaignAlertId");
        }

        private int? SelectedCampaignId()
        {
            int campaignId;
            return Int32.TryParse(Convert.ToString(Session["CampaignId"]), out campaignId)
                ? (int?)campaignId
                : null;
        }

        private IEnumerable<T> ApplySelectedCampaign<T>(IEnumerable<T> records)
        {
            int? campaignId = SelectedCampaignId();
            if (!campaignId.HasValue || records == null)
            {
                return records ?? Enumerable.Empty<T>();
            }

            PropertyInfo eventIdProperty = typeof(T).GetProperty("EventId");
            if (eventIdProperty == null)
            {
                return records;
            }

            HashSet<int> allowedEventIds = new HashSet<int>(
                _service.GetEventIdsByCampaign(campaignId.Value));

            return records.Where(item =>
            {
                object value = item == null ? null : eventIdProperty.GetValue(item, null);
                if (value == null) return false;
                int eventId;
                return Int32.TryParse(Convert.ToString(value), out eventId) &&
                       allowedEventIds.Contains(eventId);
            }).ToList();
        }

        public ActionResult ExpenseTracking(string keyword = null)
        {
            return Expenses(keyword);
        }

        public ActionResult VillageTurnout()
        {
            return TrackingModule("Village Turnout", "Track expected vs actual people from each village, ward and booth.", "VillageTurnout");
        }

        public ActionResult FoodManagement()
        {
            return TrackingModule("Food Management", "Plan menu, plates, quantity, cost and responsible people so food does not fall short.", "Food");
        }

        public ActionResult BorrowedAssets()
        {
            return TrackingModule("Borrowed Assets", "Track LPG cylinders, utensils, tents, chairs, speakers and personal service items for safe return and appreciation.", "BorrowedAsset");
        }

        public ActionResult Appreciation()
        {
            return TrackingModule("Appreciation", "Track volunteers, providers and workers who deserve appreciation after successful events.", "Appreciation");
        }

        public ActionResult CreateEvent()
        {
            if (!HasPermission("CanCreate")) return AccessDenied();
            return View("EventForm", new EventVM
            {
                CampaignId = SelectedCampaignId(),
                EventDate = DateTime.Today,
                EventScope = "ElectionWarRoom",
                ShowInElectionWarRoom = true,
                Priority = "Medium",
                Status = "Planned",
                State = "Uttar Pradesh",
                District = "Meerut"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEvent(EventVM model)
        {
            if (!HasPermission("CanCreate")) return AccessDenied();
            if (!ModelState.IsValid) return View("EventForm", model);

            bool saved = _service.SaveEvent(model, CurrentUserId(), Server);
            if (!saved)
            {
                ModelState.AddModelError("", "This event already exists for the same date and venue.");
                return View("EventForm", model);
            }

            _service.SetEventCampaign(model.EventId, model.CampaignId, CurrentUserId());

            TempData["Success"] = "Event created successfully.";
            return RedirectToAction("Events");
        }

        public ActionResult EditEvent(int id)
        {
            if (!HasPermission("CanEdit")) return AccessDenied();
            var model = _service.GetEventById(id);
            if (model == null) return HttpNotFound();
            return View("EventForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditEvent(EventVM model)
        {
            if (!HasPermission("CanEdit")) return AccessDenied();
            if (!ModelState.IsValid) return View("EventForm", model);

            bool saved = _service.UpdateEvent(model, CurrentUserId(), Server);
            if (!saved)
            {
                ModelState.AddModelError("", "Unable to update this event. Please check duplicate title/date/venue.");
                return View("EventForm", model);
            }

            _service.SetEventCampaign(model.EventId, model.CampaignId, CurrentUserId());

            TempData["Success"] = "Event updated successfully.";
            return RedirectToAction("Events");
        }

        [HttpPost]
        public JsonResult DeleteEvent(int id)
        {
            if (!HasPermission("CanDelete")) return Json(new { success = false, message = "You do not have delete permission." });
            return Json(new { success = _service.DeleteEvent(id, CurrentUserId()), message = "Event deleted successfully." });
        }

        public ActionResult CreateTrackingItem(string category)
        {
            if (!HasPermission("CanCreate")) return AccessDenied();
            PrepareTrackingForm(category);
            return View("TrackingItemForm", new EventTrackingItemVM { Category = category, Status = "Planned", IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateTrackingItem(EventTrackingItemVM model)
        {
            if (!HasPermission("CanCreate")) return AccessDenied();
            PrepareTrackingForm(model.Category);
            if (!ModelState.IsValid) return View("TrackingItemForm", model);

            _service.SaveTrackingItem(model, CurrentUserId());
            TempData["Success"] = "Tracking item saved successfully.";
            return RedirectToTrackingCategory(model.Category);
        }

        public ActionResult EditTrackingItem(int id)
        {
            if (!HasPermission("CanEdit")) return AccessDenied();
            var model = _service.GetTrackingItemById(id);
            if (model == null) return HttpNotFound();
            PrepareTrackingForm(model.Category);
            return View("TrackingItemForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTrackingItem(EventTrackingItemVM model)
        {
            if (!HasPermission("CanEdit")) return AccessDenied();
            PrepareTrackingForm(model.Category);
            if (!ModelState.IsValid) return View("TrackingItemForm", model);

            _service.UpdateTrackingItem(model, CurrentUserId());
            TempData["Success"] = "Tracking item updated successfully.";
            return RedirectToTrackingCategory(model.Category);
        }

        [HttpPost]
        public JsonResult DeleteTrackingItem(int id)
        {
            if (!HasPermission("CanDelete")) return Json(new { success = false, message = "You do not have delete permission." });
            var model = _service.GetTrackingItemById(id);
            _service.DeleteTrackingItem(id, CurrentUserId());
            return Json(new { success = true, message = "Tracking item deleted successfully.", category = model == null ? "" : model.Category });
        }

        public ActionResult CreateVehicle()
        {
            if (!HasPermission("CanCreate")) return AccessDenied();
            PrepareVehicleForm();
            return View("VehicleForm", new EventVehicleVM
            {
                VehicleCount = 1,
                EstimatedPersons = 1,
                IsActive = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateVehicle(EventVehicleVM model)
        {
            if (!HasPermission("CanCreate")) return AccessDenied();
            PrepareVehicleForm();

            if (_service.IsDuplicateVehicleNumber(model.VehicleNumber, model.EventVehicleId))
            {
                ModelState.AddModelError("VehicleNumber", "This vehicle number is already saved.");
            }

            if (!ModelState.IsValid) return View("VehicleForm", model);

            _service.SaveVehicle(model, CurrentUserId());
            TempData["Success"] = "Vehicle record saved successfully.";
            return RedirectToAction("Vehicles");
        }

        public ActionResult EditVehicle(int id)
        {
            if (!HasPermission("CanEdit")) return AccessDenied();
            var model = _service.GetVehicleById(id);
            if (model == null) return HttpNotFound();
            PrepareVehicleForm();
            return View("VehicleForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditVehicle(EventVehicleVM model)
        {
            if (!HasPermission("CanEdit")) return AccessDenied();
            PrepareVehicleForm();

            if (_service.IsDuplicateVehicleNumber(model.VehicleNumber, model.EventVehicleId))
            {
                ModelState.AddModelError("VehicleNumber", "This vehicle number is already saved.");
            }

            if (!ModelState.IsValid) return View("VehicleForm", model);

            _service.UpdateVehicle(model, CurrentUserId());
            TempData["Success"] = "Vehicle record updated successfully.";
            return RedirectToAction("Vehicles");
        }

        [HttpPost]
        public JsonResult DeleteVehicle(int id)
        {
            if (!HasPermission("CanDelete")) return Json(new { success = false, message = "You do not have delete permission." });
            return Json(new { success = _service.DeleteVehicle(id, CurrentUserId()), message = "Vehicle record deleted successfully." });
        }

        public ActionResult CreateBooth()
        {
            if (!HasPermission("CanCreate")) return AccessDenied();
            return View("BoothForm", new ElectionBoothVM { IsActive = true, Priority = "Medium", BoothStrength = "Neutral" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateBooth(ElectionBoothVM model)
        {
            if (!HasPermission("CanCreate")) return AccessDenied();
            if (!ModelState.IsValid) return View("BoothForm", model);

            _service.SaveBooth(model, CurrentUserId());
            TempData["Success"] = "Booth record saved successfully.";
            return RedirectToAction("ElectionBooths");
        }

        public ActionResult EditBooth(int id)
        {
            if (!HasPermission("CanEdit")) return AccessDenied();
            var model = _service.GetBoothById(id);
            if (model == null) return HttpNotFound();
            return View("BoothForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditBooth(ElectionBoothVM model)
        {
            if (!HasPermission("CanEdit")) return AccessDenied();
            if (!ModelState.IsValid) return View("BoothForm", model);

            _service.UpdateBooth(model, CurrentUserId());
            TempData["Success"] = "Booth record updated successfully.";
            return RedirectToAction("ElectionBooths");
        }

        [HttpPost]
        public JsonResult DeleteBooth(int id)
        {
            if (!HasPermission("CanDelete")) return Json(new { success = false, message = "You do not have delete permission." });
            return Json(new { success = _service.DeleteBooth(id, CurrentUserId()), message = "Booth record deleted successfully." });
        }

        public ActionResult CreateTask()
        {
            if (!HasPermission("CanCreate")) return AccessDenied();
            return View("TaskForm", new EventTaskVM { Status = "Pending", Priority = "Medium", AssignedDate = DateTime.Now, StartDate = DateTime.Now, IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateTask(EventTaskVM model)
        {
            if (!HasPermission("CanCreate")) return AccessDenied();
            if (!ModelState.IsValid) return View("TaskForm", model);

            _service.SaveTask(model, CurrentUserId());
            TempData["Success"] = "Task saved successfully.";
            return RedirectToAction("Tasks");
        }

        public ActionResult EditTask(int id)
        {
            if (!HasPermission("CanEdit")) return AccessDenied();
            var model = _service.GetTaskById(id);
            if (model == null) return HttpNotFound();
            return View("TaskForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTask(EventTaskVM model)
        {
            if (!HasPermission("CanEdit")) return AccessDenied();
            if (!ModelState.IsValid) return View("TaskForm", model);

            _service.UpdateTask(model, CurrentUserId());
            TempData["Success"] = "Task updated successfully.";
            return RedirectToAction("Tasks");
        }

        [HttpPost]
        public JsonResult DeleteTask(int id)
        {
            if (!HasPermission("CanDelete")) return Json(new { success = false, message = "You do not have delete permission." });
            return Json(new { success = _service.DeleteTask(id, CurrentUserId()), message = "Task deleted successfully." });
        }

        public ActionResult CreateJanSampark()
        {
            if (!HasPermission("CanCreate")) return AccessDenied();
            return View("JanSamparkForm", new JanSamparkVM { Status = "Open", Priority = "Medium", Category = "General", IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateJanSampark(JanSamparkVM model)
        {
            if (!HasPermission("CanCreate")) return AccessDenied();
            if (_service.IsDuplicateJanSampark(model.CitizenName, model.MobileNumber, model.Subject, model.EventId, model.JanSamparkId))
            {
                ModelState.AddModelError("", "This Jan Sampark record already exists.");
            }
            if (!ModelState.IsValid) return View("JanSamparkForm", model);

            _service.SaveJanSampark(model, CurrentUserId());
            TempData["Success"] = "Jan Sampark record saved successfully.";
            return RedirectToAction("JanSampark");
        }

        public ActionResult EditJanSampark(int id)
        {
            if (!HasPermission("CanEdit")) return AccessDenied();
            var model = _service.GetJanSamparkById(id);
            if (model == null) return HttpNotFound();
            return View("JanSamparkForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditJanSampark(JanSamparkVM model)
        {
            if (!HasPermission("CanEdit")) return AccessDenied();
            if (_service.IsDuplicateJanSampark(model.CitizenName, model.MobileNumber, model.Subject, model.EventId, model.JanSamparkId))
            {
                ModelState.AddModelError("", "This Jan Sampark record already exists.");
            }
            if (!ModelState.IsValid) return View("JanSamparkForm", model);

            _service.UpdateJanSampark(model, CurrentUserId());
            TempData["Success"] = "Jan Sampark record updated successfully.";
            return RedirectToAction("JanSampark");
        }

        [HttpPost]
        public JsonResult DeleteJanSampark(int id)
        {
            if (!HasPermission("CanDelete")) return Json(new { success = false, message = "You do not have delete permission." });
            return Json(new { success = _service.DeleteJanSampark(id, CurrentUserId()), message = "Jan Sampark record deleted successfully." });
        }


        public ActionResult CreateAttendance() { if (!HasPermission("CanCreate")) return AccessDenied(); return GenericForm("Create Attendance", "SaveAttendance", "Attendance", new EventAttendanceVM { EventId = 1, AttendanceType = "Worker", AttendanceMode = "Manual", CheckInTime = DateTime.Now, IsActive = true }); }
        [HttpPost, ValidateAntiForgeryToken] public ActionResult SaveAttendance(EventAttendanceVM model) { if (!HasPermission("CanCreate")) return AccessDenied(); if (!ModelState.IsValid) return GenericForm("Create Attendance", "SaveAttendance", "Attendance", model); _service.SaveAttendance(model, CurrentUserId()); return RedirectToAction("Attendance"); }
        public ActionResult EditAttendance(int id) { if (!HasPermission("CanEdit")) return AccessDenied(); var model = _service.GetAttendanceById(id); return model == null ? HttpNotFound() : GenericForm("Edit Attendance", "UpdateAttendance", "Attendance", model); }
        [HttpPost, ValidateAntiForgeryToken] public ActionResult UpdateAttendance(EventAttendanceVM model) { if (!HasPermission("CanEdit")) return AccessDenied(); if (!ModelState.IsValid) return GenericForm("Edit Attendance", "UpdateAttendance", "Attendance", model); _service.UpdateAttendance(model, CurrentUserId()); return RedirectToAction("Attendance"); }
        [HttpPost] public JsonResult DeleteAttendance(int id) { if (!HasPermission("CanDelete")) return Json(new { success = false, message = "You do not have delete permission." }); return Json(new { success = _service.DeleteAttendance(id, CurrentUserId()), message = "Attendance deleted successfully." }); }

        public ActionResult CreateTeam() { if (!HasPermission("CanCreate")) return AccessDenied(); return GenericForm("Create Team", "SaveTeam", "Teams", new EventTeamVM { EventId = 1, Status = "Pending", Priority = "Medium", IsActive = true }); }
        [HttpPost, ValidateAntiForgeryToken] public ActionResult SaveTeam(EventTeamVM model) { if (!HasPermission("CanCreate")) return AccessDenied(); _service.SaveTeam(model, CurrentUserId()); return RedirectToAction("Teams"); }
        public ActionResult EditTeam(int id) { if (!HasPermission("CanEdit")) return AccessDenied(); var model = _service.GetTeamById(id); return model == null ? HttpNotFound() : GenericForm("Edit Team", "UpdateTeam", "Teams", model); }
        [HttpPost, ValidateAntiForgeryToken] public ActionResult UpdateTeam(EventTeamVM model) { if (!HasPermission("CanEdit")) return AccessDenied(); _service.UpdateTeam(model, CurrentUserId()); return RedirectToAction("Teams"); }
        [HttpPost] public JsonResult DeleteTeam(int id) { if (!HasPermission("CanDelete")) return Json(new { success = false, message = "You do not have delete permission." }); return Json(new { success = _service.DeleteTeam(id, CurrentUserId()), message = "Team deleted successfully." }); }

        public ActionResult CreateGuest() { if (!HasPermission("CanCreate")) return AccessDenied(); return GenericForm("Create Guest", "SaveGuest", "Guests", new EventGuestVM { EventId = 1, InvitationStatus = "Pending", ConfirmationStatus = "Pending", IsActive = true }); }
        [HttpPost, ValidateAntiForgeryToken] public ActionResult SaveGuest(EventGuestVM model) { if (!HasPermission("CanCreate")) return AccessDenied(); _service.SaveGuest(model, CurrentUserId()); return RedirectToAction("Guests"); }
        public ActionResult EditGuest(int id) { if (!HasPermission("CanEdit")) return AccessDenied(); var model = _service.GetGuestById(id); return model == null ? HttpNotFound() : GenericForm("Edit Guest", "UpdateGuest", "Guests", model); }
        [HttpPost, ValidateAntiForgeryToken] public ActionResult UpdateGuest(EventGuestVM model) { if (!HasPermission("CanEdit")) return AccessDenied(); _service.UpdateGuest(model, CurrentUserId()); return RedirectToAction("Guests"); }
        [HttpPost] public JsonResult DeleteGuest(int id) { if (!HasPermission("CanDelete")) return Json(new { success = false, message = "You do not have delete permission." }); return Json(new { success = _service.DeleteGuest(id, CurrentUserId()), message = "Guest deleted successfully." }); }

        public ActionResult CreateArrangement() { if (!HasPermission("CanCreate")) return AccessDenied(); return GenericForm("Create Arrangement", "SaveArrangement", "Arrangements", new EventArrangementVM { EventId = 1, Status = "Pending", Priority = "Medium", IsActive = true }); }
        [HttpPost, ValidateAntiForgeryToken] public ActionResult SaveArrangement(EventArrangementVM model) { if (!HasPermission("CanCreate")) return AccessDenied(); _service.SaveArrangement(model, CurrentUserId()); return RedirectToAction("Arrangements"); }
        public ActionResult EditArrangement(int id) { if (!HasPermission("CanEdit")) return AccessDenied(); var model = _service.GetArrangementById(id); return model == null ? HttpNotFound() : GenericForm("Edit Arrangement", "UpdateArrangement", "Arrangements", model); }
        [HttpPost, ValidateAntiForgeryToken] public ActionResult UpdateArrangement(EventArrangementVM model) { if (!HasPermission("CanEdit")) return AccessDenied(); _service.UpdateArrangement(model, CurrentUserId()); return RedirectToAction("Arrangements"); }
        [HttpPost] public JsonResult DeleteArrangement(int id) { if (!HasPermission("CanDelete")) return Json(new { success = false, message = "You do not have delete permission." }); return Json(new { success = _service.DeleteArrangement(id, CurrentUserId()), message = "Arrangement deleted successfully." }); }

        public ActionResult CreateExpense() { if (!HasPermission("CanCreate")) return AccessDenied(); return GenericForm("Create Expense", "SaveExpense", "Expenses", new EventExpenseVM { EventId = 1, ExpenseDate = DateTime.Today, PaymentStatus = "Pending", ExpenseStatus = "Pending", IsActive = true }); }
        [HttpPost, ValidateAntiForgeryToken] public ActionResult SaveExpense(EventExpenseVM model) { if (!HasPermission("CanCreate")) return AccessDenied(); if (!ModelState.IsValid) return GenericForm("Create Expense", "SaveExpense", "Expenses", model); _service.SaveExpense(model, CurrentUserId()); return RedirectToAction("Expenses"); }
        public ActionResult EditExpense(int id) { if (!HasPermission("CanEdit")) return AccessDenied(); var model = _service.GetExpenseById(id); return model == null ? HttpNotFound() : GenericForm("Edit Expense", "UpdateExpense", "Expenses", model); }
        [HttpPost, ValidateAntiForgeryToken] public ActionResult UpdateExpense(EventExpenseVM model) { if (!HasPermission("CanEdit")) return AccessDenied(); if (!ModelState.IsValid) return GenericForm("Edit Expense", "UpdateExpense", "Expenses", model); _service.UpdateExpense(model, CurrentUserId()); return RedirectToAction("Expenses"); }
        [HttpPost] public JsonResult DeleteExpense(int id) { if (!HasPermission("CanDelete")) return Json(new { success = false, message = "You do not have delete permission." }); return Json(new { success = _service.DeleteExpense(id, CurrentUserId()), message = "Expense deleted successfully." }); }

        public ActionResult CreateMedia() { if (!HasPermission("CanCreate")) return AccessDenied(); return GenericForm("Create Media", "SaveMedia", "Media", new EventMediaVM { EventId = 1, MediaType = "Photo", UploadedDate = DateTime.Now, MediaStatus = "Pending", IsActive = true }); }
        [HttpPost, ValidateAntiForgeryToken] public ActionResult SaveMedia(EventMediaVM model) { if (!HasPermission("CanCreate")) return AccessDenied(); if (!ModelState.IsValid) return GenericForm("Create Media", "SaveMedia", "Media", model); _service.SaveMedia(model, CurrentUserId()); return RedirectToAction("Media"); }
        public ActionResult EditMedia(int id) { if (!HasPermission("CanEdit")) return AccessDenied(); var model = _service.GetMediaById(id); return model == null ? HttpNotFound() : GenericForm("Edit Media", "UpdateMedia", "Media", model); }
        [HttpPost, ValidateAntiForgeryToken] public ActionResult UpdateMedia(EventMediaVM model) { if (!HasPermission("CanEdit")) return AccessDenied(); if (!ModelState.IsValid) return GenericForm("Edit Media", "UpdateMedia", "Media", model); _service.UpdateMedia(model, CurrentUserId()); return RedirectToAction("Media"); }
        [HttpPost] public JsonResult DeleteMedia(int id) { if (!HasPermission("CanDelete")) return Json(new { success = false, message = "You do not have delete permission." }); return Json(new { success = _service.DeleteMedia(id, CurrentUserId()), message = "Media deleted successfully." }); }

        public ActionResult CreateEventPoll() { if (!HasPermission("CanCreate")) return AccessDenied(); return GenericForm("Create Event Poll", "SaveEventPoll", "Polls", new EventPollVM { StartDate = DateTime.Today, Status = "Draft", PollType = "Event Feedback", QuestionType = "Single Choice", IsActive = true }); }
        [HttpPost, ValidateAntiForgeryToken] public ActionResult SaveEventPoll(EventPollVM model) { if (!HasPermission("CanCreate")) return AccessDenied(); if (!ModelState.IsValid) return GenericForm("Create Event Poll", "SaveEventPoll", "Polls", model); _service.SavePoll(model, CurrentUserId()); return RedirectToAction("Polls"); }
        public ActionResult EditEventPoll(int id) { if (!HasPermission("CanEdit")) return AccessDenied(); var model = _service.GetPollById(id); return model == null ? HttpNotFound() : GenericForm("Edit Event Poll", "UpdateEventPoll", "Polls", model); }
        [HttpPost, ValidateAntiForgeryToken] public ActionResult UpdateEventPoll(EventPollVM model) { if (!HasPermission("CanEdit")) return AccessDenied(); if (!ModelState.IsValid) return GenericForm("Edit Event Poll", "UpdateEventPoll", "Polls", model); _service.UpdatePoll(model, CurrentUserId()); return RedirectToAction("Polls"); }
        [HttpPost] public JsonResult DeleteEventPoll(int id) { if (!HasPermission("CanDelete")) return Json(new { success = false, message = "You do not have delete permission." }); return Json(new { success = _service.DeletePoll(id, CurrentUserId()), message = "Event poll deleted successfully." }); }

        public ActionResult CreateBoothVisit() { if (!HasPermission("CanCreate")) return AccessDenied(); return GenericForm("Create Booth Visit", "SaveBoothVisit", "BoothVisits", new ElectionBoothVisitVM { ElectionBoothId = 1, VisitDate = DateTime.Today, VisitStatus = "Scheduled", IsActive = true }); }
        [HttpPost, ValidateAntiForgeryToken] public ActionResult SaveBoothVisit(ElectionBoothVisitVM model) { if (!HasPermission("CanCreate")) return AccessDenied(); _service.SaveBoothVisit(model, CurrentUserId()); return RedirectToAction("BoothVisits"); }
        public ActionResult EditBoothVisit(int id) { if (!HasPermission("CanEdit")) return AccessDenied(); var model = _service.GetBoothVisitById(id); return model == null ? HttpNotFound() : GenericForm("Edit Booth Visit", "UpdateBoothVisit", "BoothVisits", model); }
        [HttpPost, ValidateAntiForgeryToken] public ActionResult UpdateBoothVisit(ElectionBoothVisitVM model) { if (!HasPermission("CanEdit")) return AccessDenied(); _service.UpdateBoothVisit(model, CurrentUserId()); return RedirectToAction("BoothVisits"); }
        [HttpPost] public JsonResult DeleteBoothVisit(int id) { if (!HasPermission("CanDelete")) return Json(new { success = false, message = "You do not have delete permission." }); return Json(new { success = _service.DeleteBoothVisit(id, CurrentUserId()), message = "Booth visit deleted successfully." }); }

        public ActionResult CreateCampaign() { if (!HasPermission("CanCreate")) return AccessDenied(); return GenericForm("Create Campaign", "SaveCampaign", "Campaigns", new ElectionCampaignVM { ElectionType = "Vidhan Sabha", StartDate = DateTime.Today, Status = "Active", IsActive = true }); }
        [HttpPost, ValidateAntiForgeryToken] public ActionResult SaveCampaign(ElectionCampaignVM model) { if (!HasPermission("CanCreate")) return AccessDenied(); if (!ModelState.IsValid) return GenericForm("Create Campaign", "SaveCampaign", "Campaigns", model); _service.SaveElectionCampaign(model, CurrentUserId()); return RedirectToAction("Campaigns"); }
        public ActionResult EditCampaign(int id) { if (!HasPermission("CanEdit")) return AccessDenied(); var model = _service.GetElectionCampaignById(id); return model == null ? HttpNotFound() : GenericForm("Edit Campaign", "UpdateCampaign", "Campaigns", model); }
        [HttpPost, ValidateAntiForgeryToken] public ActionResult UpdateCampaign(ElectionCampaignVM model) { if (!HasPermission("CanEdit")) return AccessDenied(); if (!ModelState.IsValid) return GenericForm("Edit Campaign", "UpdateCampaign", "Campaigns", model); _service.UpdateElectionCampaign(model, CurrentUserId()); return RedirectToAction("Campaigns"); }
        [HttpPost] public JsonResult DeleteCampaign(int id) { if (!HasPermission("CanDelete")) return Json(new { success = false, message = "You do not have delete permission." }); return Json(new { success = _service.DeleteElectionCampaign(id, CurrentUserId()), message = "Campaign deleted successfully." }); }

        public ActionResult CreateCampaignAlert() { if (!HasPermission("CanCreate")) return AccessDenied(); return GenericForm("Create Campaign Alert", "SaveCampaignAlert", "CampaignAlerts", new CampaignAlertVM { AlertType = "Operational", Severity = "Medium", AlertStatus = "Pending", IsDashboard = true, IsActive = true }); }
        [HttpPost, ValidateAntiForgeryToken] public ActionResult SaveCampaignAlert(CampaignAlertVM model) { if (!HasPermission("CanCreate")) return AccessDenied(); _service.SaveCampaignAlert(model, CurrentUserId()); return RedirectToAction("CampaignAlerts"); }
        public ActionResult EditCampaignAlert(int id) { if (!HasPermission("CanEdit")) return AccessDenied(); var model = _service.GetCampaignAlertById(id); return model == null ? HttpNotFound() : GenericForm("Edit Campaign Alert", "UpdateCampaignAlert", "CampaignAlerts", model); }
        [HttpPost, ValidateAntiForgeryToken] public ActionResult UpdateCampaignAlert(CampaignAlertVM model) { if (!HasPermission("CanEdit")) return AccessDenied(); _service.UpdateCampaignAlert(model, CurrentUserId()); return RedirectToAction("CampaignAlerts"); }
        [HttpPost] public JsonResult DeleteCampaignAlert(int id) { if (!HasPermission("CanDelete")) return Json(new { success = false, message = "You do not have delete permission." }); return Json(new { success = _service.DeleteCampaignAlert(id, CurrentUserId()), message = "Campaign alert deleted successfully." }); }

        private ActionResult GenericForm(string title, string postAction, string backAction, object model)
        {
            ViewBag.Title = title;
            ViewBag.PostAction = postAction;
            ViewBag.BackAction = backAction;
            return View("GenericForm", model);
        }

        private ActionResult Module(string title, string description, object records, string createAction = null, string editAction = null, string deleteAction = null, string keyName = null)
        {
            ViewBag.Title = title;
            ViewBag.Description = description;
            ViewBag.CreateAction = createAction;
            ViewBag.EditAction = editAction;
            ViewBag.DeleteAction = deleteAction;
            ViewBag.KeyName = keyName;
            ViewBag.CanCreate = HasPermission("CanCreate");
            ViewBag.CanEdit = HasPermission("CanEdit");
            ViewBag.CanDelete = HasPermission("CanDelete");
            return View("Module", records);
        }

        private ActionResult TrackingModule(string title, string description, string category)
        {
            ViewBag.TrackingCategory = category;
            return Module(title, description, _service.GetTrackingItems(category), "CreateTrackingItem", "EditTrackingItem", "DeleteTrackingItem", "EventTrackingItemId");
        }

        private void PrepareTrackingForm(string category)
        {
            ViewBag.TrackingCategory = category;
            ViewBag.EventList = _service.GetEventDropdown();
        }

        private void PrepareVehicleForm()
        {
            ViewBag.EventList = _service.GetEventDropdown();
        }

        private ActionResult RedirectToTrackingCategory(string category)
        {
            if (String.Equals(category, "VillageTurnout", StringComparison.OrdinalIgnoreCase)) return RedirectToAction("VillageTurnout");
            if (String.Equals(category, "Food", StringComparison.OrdinalIgnoreCase)) return RedirectToAction("FoodManagement");
            if (String.Equals(category, "BorrowedAsset", StringComparison.OrdinalIgnoreCase)) return RedirectToAction("BorrowedAssets");
            if (String.Equals(category, "Appreciation", StringComparison.OrdinalIgnoreCase)) return RedirectToAction("Appreciation");
            return RedirectToAction("Events");
        }


        private static List<CampaignPollOptionVM> BuildPollOptions(IEnumerable<string> optionTexts)
        {
            List<CampaignPollOptionVM> options = new List<CampaignPollOptionVM>();
            if (optionTexts == null) return options;

            int order = 1;
            foreach (string optionText in optionTexts.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                options.Add(new CampaignPollOptionVM { OptionText = optionText.Trim(), DisplayOrder = order++ });
            }

            return options;
        }
        private bool HasPermission(string permission)
        {
            return _permissionService.HasActionPermission(CurrentRoleId(), Convert.ToString(Session["RoleName"]), "Admin", "ElectionWarRoom", "Index", permission);
        }

        private int CurrentUserId()
        {
            int userId;
            return int.TryParse(Convert.ToString(Session["UserId"]), out userId) ? userId : 0;
        }

        private int? CurrentRoleId()
        {
            int roleId;
            return int.TryParse(Convert.ToString(Session["RoleId"]), out roleId) ? roleId : (int?)null;
        }

        private ActionResult AccessDenied()
        {
            return View("~/Areas/Admin/Views/Shared/AccessDenied.cshtml");
        }

        private DashboardVM BuildDashboard()
        {
            var model = new DashboardVM
            {
                TotalEvents = _service.GetEvents().Count,
                TodayEvents = _service.GetTodayEventCount(),
                UpcomingEventsCount = _service.GetUpcomingEventCount(),
                PendingTasks = _service.GetPendingTasks(),
                CompletedTasks = _service.GetCompletedTasks(),
                TotalVehicles = _service.GetTotalVehicleRecords(),
                TotalAttendance = _service.GetTotalAttendanceRecords(),
                TotalGuests = _service.GetTotalGuestRecords(),
                TotalTeams = _service.GetTotalTeamRecords(),
                TotalBooths = _service.GetTotalBooths(),
                VisitedBooths = _service.GetTotalBoothVisits(),
                BoothCoveragePercentage = _service.GetAverageBoothCoverage(),
                TotalPolls = _service.GetTotalPolls(),
                TotalSurveyResponses = _service.GetTotalPollResponses(),
                OpenComplaints = _service.GetPendingJanSamparkCount(),
                ResolvedComplaints = _service.GetResolvedJanSamparkCount(),
                TodayExpense = _service.GetTodayExpense(),
                MonthExpense = _service.GetMonthExpense(),
                TotalAlerts = _service.GetTotalCampaignAlerts(),
                CriticalAlerts = _service.GetCriticalAlertsCount(),
                HighPriorityBooths = _service.GetHighPriorityBooths(),
                TodayBoothVisits = _service.GetTodayBoothVisitCount(),
                TodayJanSampark = _service.GetTodayJanSamparkCount(),
                FollowUpBoothVisits = _service.GetFollowUpBoothCount(),
                OverdueTasks = _service.GetOverdueTaskCount(),
                InProgressTasks = _service.GetInProgressTasks(),
                VerifiedAttendance = _service.GetTotalVerifiedAttendanceRecords(),
                VerifiedVehicles = _service.GetTotalVerifiedVehicleRecords(),
                PendingCampaignAlerts = _service.GetPendingCampaignAlertsCount(),
                UnreadCampaignAlerts = _service.GetUnreadCampaignAlertsCount(),
                NewSupporters = _service.GetTotalNewSupporters(),
                OppositionSupporters = _service.GetTotalOppositionSupporters(),
                NeutralFamilies = _service.GetTotalNeutralFamilies(),
                TotalVisitedHouses = _service.GetTotalVisitedHouses(),
                TotalHouses = _service.GetTotalHouses()
            };

            model.ActiveIssues = model.PendingTasks + model.OpenComplaints + model.CriticalAlerts;
            model.FieldOperations = model.TodayEvents + model.TotalTeams + model.TotalVehicles;
            model.TotalCitizenInteractions = model.TotalAttendance + model.TotalSurveyResponses + model.OpenComplaints + model.ResolvedComplaints;
            model.TaskCompletionPercentage = CalculatePercentage(model.CompletedTasks, model.CompletedTasks + model.PendingTasks);
            model.ComplaintResolutionPercentage = CalculatePercentage(model.ResolvedComplaints, model.ResolvedComplaints + model.OpenComplaints);
            model.VoterContactPercentage = CalculatePercentage(model.TotalVisitedHouses, model.TotalHouses);
            model.SupporterConversionPercentage = CalculatePercentage(model.NewSupporters, model.NewSupporters + model.OppositionSupporters + model.NeutralFamilies);
            model.ResourceVerificationPercentage = CalculatePercentage(model.VerifiedAttendance + model.VerifiedVehicles, model.TotalAttendance + model.TotalVehicles);
            model.OperationalReadinessScore = CalculateReadinessScore(model);
            model.ReadinessStatus = GetReadinessStatus(model.OperationalReadinessScore);
            model.CommandFocus = GetCommandFocus(model);
            model.PollingDayRisk = GetPollingDayRisk(model);
            model.SuggestedNextMove = GetSuggestedNextMove(model);

            model.UpcomingEvents = _service.GetUpcomingWeekEvents()
                .Select(x => new EventSummaryVM
                {
                    EventId = x.EventId,
                    EventCode = x.EventCode,
                    EventTitle = x.EventTitle,
                    EventType = x.EventType,
                    EventDate = x.EventDate,
                    Venue = x.Venue,
                    District = x.District,
                    Status = x.Status
                })
                .ToList();

            model.RecentActivities = _service.GetRecentTaskActivities(8)
                .Select(x => new ActivitySummaryVM
                {
                    ModuleName = "Task",
                    Activity = x.ActivityType,
                    PerformedBy = x.ActivityByName,
                    ActivityDate = x.ActivityDate
                })
                .ToList();

            model.Alerts = _service.GetDashboardAlerts()
                .Take(8)
                .Select(x => new AlertSummaryVM
                {
                    AlertId = x.CampaignAlertId,
                    AlertTitle = x.AlertTitle,
                    Severity = x.Severity,
                    Status = x.AlertStatus,
                    CreatedDate = x.CreatedDate
                })
                .ToList();

            model.TopTeams = _service.GetDashboardTeams(6)
                .Select(x => new TeamSummaryVM
                {
                    TeamName = x.TeamName,
                    Members = x.TotalMembers,
                    PresentMembers = x.ActiveMembers,
                    Status = x.Status
                })
                .ToList();

            model.TopWorkers = _service.GetTopBoothVisitors(6)
                .Select(x => new TopWorkerVM
                {
                    MemberCode = x.VisitorMemberCode,
                    WorkerName = x.VisitorName,
                    Attendance = x.HousesVisited,
                    SurveyCount = x.FamiliesMet,
                    Vehicles = x.TotalTeamMembers,
                    TasksCompleted = x.NewSupporters,
                    PerformanceScore = CalculateWorkerScore(x)
                })
                .ToList();

            model.LowCoverageBooths = _service.GetLowCoverageBooths()
                .Take(6)
                .ToList();

            model.FollowUpVisits = _service.GetPendingFollowUpVisits()
                .Take(6)
                .ToList();

            model.ActionPlan = BuildActionPlan(model);

            return model;
        }

        private static decimal CalculatePercentage(int value, int total)
        {
            if (total <= 0)
                return 0;

            return Math.Round(((decimal)value * 100M) / total, 1);
        }

        private static int CalculateReadinessScore(DashboardVM model)
        {
            decimal boothScore = model.BoothCoveragePercentage;
            decimal taskScore = model.TaskCompletionPercentage;
            decimal complaintScore = model.ComplaintResolutionPercentage;
            decimal alertPenalty = model.CriticalAlerts * 4;
            decimal issuePenalty = model.ActiveIssues > 0 ? Math.Min(20, model.ActiveIssues) : 0;

            decimal score = (boothScore * 0.35M) + (taskScore * 0.30M) + (complaintScore * 0.20M) + 15M;
            score = score - alertPenalty - issuePenalty;

            if (score < 0)
                return 0;

            if (score > 100)
                return 100;

            return (int)Math.Round(score, 0);
        }

        private static int CalculateWorkerScore(ElectionBoothVisitVM visit)
        {
            decimal score = visit.HousesVisited
                + (visit.FamiliesMet * 1.5M)
                + (visit.NewSupporters * 2M)
                + visit.MembershipFormsCollected
                - visit.PublicComplaints;

            if (score < 0)
                return 0;

            if (score > 100)
                return 100;

            return (int)Math.Round(score, 0);
        }

        private static string GetReadinessStatus(int score)
        {
            if (score >= 80)
                return "Election-ready";

            if (score >= 60)
                return "Needs supervision";

            if (score >= 40)
                return "Needs intervention";

            return "High risk";
        }

        private static string GetPollingDayRisk(DashboardVM model)
        {
            if (model.CriticalAlerts > 0 || model.BoothCoveragePercentage < 40 || model.OverdueTasks > 10)
                return "High";

            if (model.BoothCoveragePercentage < 65 || model.OpenComplaints > model.ResolvedComplaints || model.HighPriorityBooths > 0)
                return "Medium";

            return "Controlled";
        }

        private static string GetCommandFocus(DashboardVM model)
        {
            if (model.CriticalAlerts > 0)
                return "Resolve critical campaign alerts first";

            if (model.BoothCoveragePercentage < 60)
                return "Increase booth visits and ground coverage";

            if (model.PendingTasks > model.CompletedTasks)
                return "Clear pending task backlog";

            if (model.OpenComplaints > model.ResolvedComplaints)
                return "Close voter complaints and citizen follow-ups";

            return "Maintain momentum and monitor turnout operations";
        }

        private static string GetSuggestedNextMove(DashboardVM model)
        {
            if (model.CriticalAlerts > 0)
                return "Assign owners to every critical alert and review closure twice today.";

            if (model.BoothCoveragePercentage < 60)
                return "Send booth teams to low coverage booths before expanding new outreach.";

            if (model.OpenComplaints > 0)
                return "Close voter grievances with public follow-up calls and local proof.";

            if (model.PendingTasks > 0)
                return "Clear pending field tasks before adding new campaign activities.";

            return "Increase positive voter contact and protect high-performing booths.";
        }

        private static List<ActionPlanItemVM> BuildActionPlan(DashboardVM model)
        {
            var plan = new List<ActionPlanItemVM>();

            plan.Add(new ActionPlanItemVM
            {
                Title = "Close urgent alerts",
                Detail = "Critical and unread alerts should have one owner and a same-day deadline.",
                ActionName = "CampaignAlerts",
                Tone = model.CriticalAlerts > 0 ? "danger" : "success",
                Count = model.CriticalAlerts + model.UnreadCampaignAlerts
            });

            plan.Add(new ActionPlanItemVM
            {
                Title = "Push booth coverage",
                Detail = "Prioritize high-priority and low-coverage booths for next visits.",
                ActionName = "BoothVisits",
                Tone = model.BoothCoveragePercentage < 65 ? "warning" : "success",
                Count = model.HighPriorityBooths + model.FollowUpBoothVisits
            });

            plan.Add(new ActionPlanItemVM
            {
                Title = "Resolve citizen issues",
                Detail = "Convert open complaints into resolved Jan Sampark follow-ups.",
                ActionName = "JanSampark",
                Tone = model.OpenComplaints > 0 ? "primary" : "success",
                Count = model.OpenComplaints
            });

            plan.Add(new ActionPlanItemVM
            {
                Title = "Clear task backlog",
                Detail = "Move overdue and pending assignments into verified completion.",
                ActionName = "Tasks",
                Tone = model.OverdueTasks > 0 ? "danger" : "warning",
                Count = model.PendingTasks + model.OverdueTasks
            });

            return plan;
        }
    }
}






