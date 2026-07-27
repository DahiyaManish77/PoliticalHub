using PoliticalLeaderPortal.Areas.Admin.Infrastructure;
using PoliticalLeaderPortal.Areas.Admin.Services;
using PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionCampaignMaster;
using PoliticalLeaderPortal.Models;
using System;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    [RoleMenuAuthorize]
    public class ElectionCampaignMasterController : Controller
    {
        private readonly ElectionCampaignMasterService _service = new ElectionCampaignMasterService();
        private readonly RoleMenuPermissionService _permissionService = new RoleMenuPermissionService();

        public ActionResult Index(string keyword = null)
        {
            ViewBag.Keyword = keyword;
            ViewBag.CanCreate = HasPermission("CanCreate");
            ViewBag.CanEdit = HasPermission("CanEdit");
            ViewBag.CanDelete = HasPermission("CanDelete");
            return View(_service.GetDashboard(keyword));
        }

        public ActionResult CreateElection()
        {
            if (!HasPermission("CanCreate")) return AccessDenied();
            return View("ElectionForm", new ElectionMasterVM { ElectionYear = DateTime.Today.Year, Status = "Planning", IsActive = true });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CreateElection(ElectionMasterVM model)
        {
            if (!HasPermission("CanCreate")) return AccessDenied();
            ValidateElection(model);
            if (!ModelState.IsValid) return View("ElectionForm", model);
            _service.SaveElection(model, CurrentUserId());
            TempData["Success"] = "Election master created successfully.";
            return RedirectToAction("Index");
        }

        public ActionResult EditElection(int id)
        {
            if (!HasPermission("CanEdit")) return AccessDenied();
            var model = _service.GetElection(id);
            return model == null ? (ActionResult)HttpNotFound() : View("ElectionForm", model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult EditElection(ElectionMasterVM model)
        {
            if (!HasPermission("CanEdit")) return AccessDenied();
            ValidateElection(model);
            if (!ModelState.IsValid) return View("ElectionForm", model);
            _service.UpdateElection(model, CurrentUserId());
            TempData["Success"] = "Election master updated successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult DeleteElection(int id)
        {
            if (!HasPermission("CanDelete")) return AccessDenied();
            _service.DeleteElection(id, CurrentUserId());
            TempData["Success"] = "Election deleted when no active campaign was linked.";
            return RedirectToAction("Index");
        }

        public ActionResult CreateCampaign()
        {
            if (!HasPermission("CanCreate")) return AccessDenied();
            PrepareCampaignForm(null);
            return View("CampaignForm", new CampaignMasterVM { StartDate = DateTime.Today, Status = "Planning", IsActive = true });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CreateCampaign(CampaignMasterVM model)
        {
            if (!HasPermission("CanCreate")) return AccessDenied();
            ValidateCampaign(model);
            if (!ModelState.IsValid) { PrepareCampaignForm(model.ElectionId); return View("CampaignForm", model); }
            _service.SaveCampaign(model, CurrentUserId());
            TempData["Success"] = "Campaign master created successfully.";
            return RedirectToAction("Index");
        }

        public ActionResult EditCampaign(int id)
        {
            if (!HasPermission("CanEdit")) return AccessDenied();
            var model = _service.GetCampaign(id);
            if (model == null) return HttpNotFound();
            PrepareCampaignForm(model.ElectionId);
            return View("CampaignForm", model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult EditCampaign(CampaignMasterVM model)
        {
            if (!HasPermission("CanEdit")) return AccessDenied();
            ValidateCampaign(model);
            if (!ModelState.IsValid) { PrepareCampaignForm(model.ElectionId); return View("CampaignForm", model); }
            _service.UpdateCampaign(model, CurrentUserId());
            TempData["Success"] = "Campaign master updated successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult DeleteCampaign(int id)
        {
            if (!HasPermission("CanDelete")) return AccessDenied();
            _service.DeleteCampaign(id, CurrentUserId());
            TempData["Success"] = "Campaign archived successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ChangeCampaignStatus(int id, string status)
        {
            if (!HasPermission("CanEdit")) return AccessDenied();
            if (status != "Planning" && status != "Active" && status != "On Hold" && status != "Closed" && status != "Cancelled")
                return new HttpStatusCodeResult(400, "Invalid campaign status.");
            _service.SetCampaignStatus(id, status, CurrentUserId());
            TempData["Success"] = "Campaign status changed successfully.";
            return RedirectToAction("Index");
        }

        private void ValidateElection(ElectionMasterVM model)
        {
            if (model.NominationStartDate.HasValue && model.NominationEndDate.HasValue && model.NominationEndDate < model.NominationStartDate)
                ModelState.AddModelError("NominationEndDate", "Nomination end date cannot be before the start date.");
            if (model.PollingDate.HasValue && model.CountingDate.HasValue && model.CountingDate < model.PollingDate)
                ModelState.AddModelError("CountingDate", "Counting date cannot be before polling date.");
            if (!string.IsNullOrWhiteSpace(model.ElectionName) && _service.ElectionNameExists(model.ElectionName.Trim(), model.ElectionId))
                ModelState.AddModelError("ElectionName", "An election with this name already exists.");
        }

        private void ValidateCampaign(CampaignMasterVM model)
        {
            if (model.EndDate.HasValue && model.EndDate < model.StartDate)
                ModelState.AddModelError("EndDate", "Campaign end date cannot be before start date.");
            if (!string.IsNullOrWhiteSpace(model.CampaignCode) && _service.CampaignCodeExists(model.CampaignCode.Trim(), model.CampaignMasterId))
                ModelState.AddModelError("CampaignCode", "Campaign code already exists.");
        }

        private void PrepareCampaignForm(int? selectedElectionId) { ViewBag.Elections = _service.GetElectionOptions(selectedElectionId); }
        private bool HasPermission(string permission) { return _permissionService.HasActionPermission(CurrentRoleId(), Convert.ToString(Session["RoleName"]), "Admin", "ElectionCampaignMaster", "Index", permission); }
        private int CurrentUserId() { int id; return int.TryParse(Convert.ToString(Session["UserId"]), out id) ? id : 0; }
        private int? CurrentRoleId() { int id; return int.TryParse(Convert.ToString(Session["RoleId"]), out id) ? id : (int?)null; }
        private ActionResult AccessDenied() { return View("~/Areas/Admin/Views/Shared/AccessDenied.cshtml"); }
    }
}
