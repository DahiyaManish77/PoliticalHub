using PoliticalLeaderPortal.Areas.Admin.Infrastructure;
using PoliticalLeaderPortal.Areas.Admin.Services;
using PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom;
using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    [RoleMenuAuthorize]
    public class VoterController : Controller
    {
        private readonly VoterService voterService;
        private readonly RoleMenuPermissionService permissionService;

        public VoterController()
        {
            voterService = new VoterService();
            permissionService = new RoleMenuPermissionService();
        }

        public ActionResult Index(string keyword = null, string village = null, string assembly = null, string block = null)
        {
            ViewBag.Keyword = keyword;
            ViewBag.Village = village;
            ViewBag.Assembly = assembly;
            ViewBag.Block = block;
            ViewBag.CanCreate = HasPermission("Create");
            ViewBag.CanEdit = HasPermission("Edit");
            ViewBag.CanDelete = HasPermission("Delete");

            return View(voterService.GetVoters(keyword, village, assembly, block));
        }

        public ActionResult Rolls(string keyword = null, string village = null, string partNumber = null)
        {
            ViewBag.Keyword = keyword;
            ViewBag.Village = village;
            ViewBag.PartNumber = partNumber;
            ViewBag.CanCreate = HasPermission("Create");
            ViewBag.CanEdit = HasPermission("Edit");
            ViewBag.CanDelete = HasPermission("Delete");

            return View(voterService.GetVoterRollPdfs(keyword, village, partNumber));
        }

        public ActionResult CreateRoll()
        {
            if (!HasPermission("Create"))
            {
                return AccessDenied();
            }

            return View("RollForm", new VoterRollPdfVM
            {
                State = "Uttar Pradesh",
                District = "Meerut",
                AssemblyConstituency = "44 - Sardhana",
                ParliamentConstituency = "Muzaffarnagar",
                RollYear = DateTime.Now.Year,
                RevisionType = "Final Electoral Roll",
                DownloadDate = DateTime.Now,
                IsActive = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateRoll(VoterRollPdfVM model)
        {
            if (!HasPermission("Create"))
            {
                return AccessDenied();
            }

            try
            {
                SaveRollPdfUpload(model);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            if (!ModelState.IsValid)
            {
                return View("RollForm", model);
            }

            string message;

            if (!voterService.SaveVoterRollPdf(model, CurrentUserId(), out message))
            {
                ModelState.AddModelError("", message);
                return View("RollForm", model);
            }

            TempData["SuccessMessage"] = message;
            return RedirectToAction("Rolls");
        }

        public ActionResult EditRoll(int id)
        {
            if (!HasPermission("Edit"))
            {
                return AccessDenied();
            }

            var model = voterService.GetVoterRollPdfById(id);

            if (model == null)
            {
                return HttpNotFound();
            }

            return View("RollForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditRoll(VoterRollPdfVM model)
        {
            if (!HasPermission("Edit"))
            {
                return AccessDenied();
            }

            try
            {
                SaveRollPdfUpload(model);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            if (!ModelState.IsValid)
            {
                return View("RollForm", model);
            }

            string message;

            if (!voterService.UpdateVoterRollPdf(model, CurrentUserId(), out message))
            {
                ModelState.AddModelError("", message);
                return View("RollForm", model);
            }

            TempData["SuccessMessage"] = message;
            return RedirectToAction("Rolls");
        }

        [HttpPost]
        public JsonResult DeleteRoll(int id)
        {
            if (!HasPermission("Delete"))
            {
                return Json(new { success = false, message = "You are not authorised to delete voter roll PDFs." });
            }

            bool success = voterService.DeleteVoterRollPdf(id, CurrentUserId());

            return Json(new
            {
                success = success,
                message = success ? "Voter roll PDF deleted successfully." : "Voter roll PDF not found."
            });
        }

        public ActionResult DownloadRoll(int id)
        {
            var model = voterService.GetVoterRollPdfById(id);

            if (model == null || String.IsNullOrWhiteSpace(model.PdfFilePath))
            {
                return HttpNotFound();
            }

            string path = Server.MapPath(model.PdfFilePath);

            if (!System.IO.File.Exists(path))
            {
                return HttpNotFound();
            }

            string fileName = "AC44_Sardhana_Part" + model.PartNumber + "_" + model.RollYear + ".pdf";
            return File(path, "application/pdf", fileName);
        }

        public ActionResult Create()
        {
            if (!HasPermission("Create"))
            {
                return AccessDenied();
            }

            return View("Form", new VoterVM
            {
                State = "Uttar Pradesh",
                VoterType = "General",
                PoliticalStatus = "Unknown",
                SupportLevel = "Unknown"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(VoterVM model)
        {
            if (!HasPermission("Create"))
            {
                return AccessDenied();
            }

            if (!ModelState.IsValid)
            {
                return View("Form", model);
            }

            try
            {
                SaveUploads(model);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Form", model);
            }

            string message;

            if (!voterService.Save(model, CurrentUserId(), out message))
            {
                ModelState.AddModelError("", message);
                return View("Form", model);
            }

            TempData["SuccessMessage"] = message;

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            if (!HasPermission("Edit"))
            {
                return AccessDenied();
            }

            var model = voterService.GetById(id);

            if (model == null)
            {
                return HttpNotFound();
            }

            return View("Form", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(VoterVM model)
        {
            if (!HasPermission("Edit"))
            {
                return AccessDenied();
            }

            if (!ModelState.IsValid)
            {
                return View("Form", model);
            }

            try
            {
                SaveUploads(model);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Form", model);
            }

            string message;

            if (!voterService.Update(model, CurrentUserId(), out message))
            {
                ModelState.AddModelError("", message);
                return View("Form", model);
            }

            TempData["SuccessMessage"] = message;

            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            if (!HasPermission("Delete"))
            {
                return Json(new { success = false, message = "You are not authorised to delete voter records." });
            }

            bool success = voterService.Delete(id, CurrentUserId());

            return Json(new
            {
                success = success,
                message = success ? "Voter deleted successfully." : "Voter record not found."
            });
        }

        public JsonResult LocationOptions(string field, string state = null, string district = null, string block = null, string assembly = null, string parliament = null, string village = null)
        {
            var options = voterService.GetLocationOptions(field, state, district, block, assembly, parliament, village);
            return Json(options, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DownloadLatestBackup()
        {
            string path = voterService.GetLatestBackupPath();

            if (String.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
            {
                TempData["ErrorMessage"] = "No voter backup file is available yet.";
                return RedirectToAction("Index");
            }

            return File(path, "text/csv", Path.GetFileName(path));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerateBackupNow()
        {
            voterService.GenerateBackup();
            TempData["SuccessMessage"] = "Voter backup generated successfully.";
            return RedirectToAction("Index");
        }

        private void SaveUploads(VoterVM model)
        {
            if (model.VoterPhotoFile != null && model.VoterPhotoFile.ContentLength > 0)
            {
                model.VoterPhotoPath = SaveImage(model.VoterPhotoFile, "VoterPhotos");
            }

            if (model.AadhaarPhotoFile != null && model.AadhaarPhotoFile.ContentLength > 0)
            {
                model.AadhaarPhotoPath = SaveImage(model.AadhaarPhotoFile, "AadhaarPhotos");
            }
        }

        private void SaveRollPdfUpload(VoterRollPdfVM model)
        {
            if (model.PdfFile == null || model.PdfFile.ContentLength <= 0)
            {
                return;
            }

            string extension = Path.GetExtension(model.PdfFile.FileName);

            if (String.IsNullOrWhiteSpace(extension) ||
                !String.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Only official PDF voter-roll files are allowed.");
            }

            if (model.PdfFile.ContentLength > 25 * 1024 * 1024)
            {
                throw new InvalidOperationException("PDF size must be less than 25 MB.");
            }

            string relativeFolder = "~/Uploads/VoterRolls/Sardhana/";
            string absoluteFolder = Server.MapPath(relativeFolder);

            if (!Directory.Exists(absoluteFolder))
            {
                Directory.CreateDirectory(absoluteFolder);
            }

            string part = String.IsNullOrWhiteSpace(model.PartNumber)
                ? "Part"
                : model.PartNumber.Trim().Replace(" ", "");
            string fileName = "AC44_Sardhana_Part" + part + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf";
            string absolutePath = Path.Combine(absoluteFolder, fileName);

            model.PdfFile.SaveAs(absolutePath);
            model.PdfFilePath = "/Uploads/VoterRolls/Sardhana/" + fileName;
        }

        private string SaveImage(System.Web.HttpPostedFileBase file, string folderName)
        {
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
            string extension = Path.GetExtension(file.FileName);

            if (String.IsNullOrWhiteSpace(extension) ||
                !allowedExtensions.Contains(extension.ToLowerInvariant()))
            {
                throw new InvalidOperationException("Only JPG, PNG and WEBP images are allowed.");
            }

            if (file.ContentLength > 2 * 1024 * 1024)
            {
                throw new InvalidOperationException("Image size must be less than 2 MB.");
            }

            string relativeFolder = "~/Uploads/Voters/" + folderName + "/";
            string absoluteFolder = Server.MapPath(relativeFolder);

            if (!Directory.Exists(absoluteFolder))
            {
                Directory.CreateDirectory(absoluteFolder);
            }

            string fileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + Guid.NewGuid().ToString("N").Substring(0, 8) + extension.ToLowerInvariant();
            string absolutePath = Path.Combine(absoluteFolder, fileName);

            file.SaveAs(absolutePath);

            return "/Uploads/Voters/" + folderName + "/" + fileName;
        }

        private bool HasPermission(string permission)
        {
            return permissionService.HasActionPermission(
                CurrentRoleId(),
                Convert.ToString(Session["RoleName"]),
                "Admin",
                "Voter",
                "Index",
                permission);
        }

        private int? CurrentUserId()
        {
            int userId;

            return Session["UserId"] != null &&
                   Int32.TryParse(Session["UserId"].ToString(), out userId)
                ? (int?)userId
                : null;
        }

        private int? CurrentRoleId()
        {
            int roleId;

            return Session["RoleId"] != null &&
                   Int32.TryParse(Session["RoleId"].ToString(), out roleId)
                ? (int?)roleId
                : null;
        }

        private ActionResult AccessDenied()
        {
            return View("~/Areas/Admin/Views/Shared/AccessDenied.cshtml");
        }
    }
}
