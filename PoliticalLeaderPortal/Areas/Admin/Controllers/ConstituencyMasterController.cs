using PoliticalLeaderPortal.Areas.Admin.Infrastructure;
using PoliticalLeaderPortal.Areas.Admin.Services;
using PoliticalLeaderPortal.Areas.Admin.ViewModels.ConstituencyMaster;
using System;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    [RoleMenuAuthorize]
    public class ConstituencyMasterController : Controller
    {
        private readonly ConstituencyMasterService _service =
            new ConstituencyMasterService();

        private readonly GovernmentGeographyImportService _importService =
            new GovernmentGeographyImportService();

        public ActionResult Index(
            string entityType = "State",
            string keyword = null,
            int page = 1)
        {
            return View(_service.GetDashboard(entityType, keyword, page));
        }

        public ActionResult Create(string entityType = "State")
        {
            var model = new GeographyEditVM
            {
                EntityType = entityType,
                IsActive = true
            };

            LoadOptions(model);
            return View("Form", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(GeographyEditVM model)
        {
            if (_service.Exists(model.EntityType, model.Code, 0))
            {
                ModelState.AddModelError(
                    "Code",
                    "Code already exists.");
            }

            AddHierarchyErrors(model);

            if (!ModelState.IsValid)
            {
                LoadOptions(model);
                return View("Form", model);
            }

            _service.Save(model, CurrentUserId());

            TempData["SuccessMessage"] =
                "Master record created successfully.";

            return RedirectToAction(
                "Index",
                new { entityType = model.EntityType });
        }

        public ActionResult Edit(int id, string entityType)
        {
            var model = _service.Get(id, entityType);

            if (model == null)
                return HttpNotFound();

            LoadOptions(model);
            return View("Form", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(GeographyEditVM model)
        {
            if (_service.Exists(
                model.EntityType,
                model.Code,
                model.Id))
            {
                ModelState.AddModelError(
                    "Code",
                    "Code already exists.");
            }

            AddHierarchyErrors(model);

            if (!ModelState.IsValid)
            {
                LoadOptions(model);
                return View("Form", model);
            }

            _service.Update(model, CurrentUserId());

            TempData["SuccessMessage"] =
                "Master record updated successfully.";

            return RedirectToAction(
                "Index",
                new { entityType = model.EntityType });
        }

        [HttpPost]
        public JsonResult Delete(int id, string entityType)
        {
            return Json(new
            {
                success = _service.Delete(
                    id,
                    entityType,
                    CurrentUserId())
            });
        }

        public ActionResult Import()
        {
            return View(BuildImportModel(null));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Import(GovernmentImportVM model)
        {
            PopulateImportStateOptions(model);

            var file = GetUploadedFile(
                "GovernmentFile",
                model);

            ValidateImportRequest(
                model,
                file,
                "Select the official LGD District XLSX report.",
                "Only the official LGD .xlsx District report is supported.");

            if (!ModelState.IsValid)
                return View("Import", model);

            try
            {
                model.Result = _importService.ImportOfficialDistrictXlsx(
                    file.InputStream,
                    file.FileName,
                    model.StateId.Value,
                    model.UpdateExisting,
                    model.SourceName,
                    CurrentUserId());

                TempData["SuccessMessage"] = String.Format(
                    "LGD District report processed. " +
                    "Inserted: {0}, Updated: {1}, Failed: {2}.",
                    model.Result.Inserted,
                    model.Result.Updated,
                    model.Result.Failed);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            PopulateImportStateOptions(model);
            return View("Import", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ImportSubDistricts(
            GovernmentImportVM model)
        {
            PopulateImportStateOptions(model);

            var file = GetUploadedFile(
                "OfficialFile",
                model);

            ValidateImportRequest(
                model,
                file,
                "Select the official LGD Sub-District XLSX report.",
                "Only the official LGD .xlsx Sub-District report is supported.");

            if (!ModelState.IsValid)
                return View("Import", model);

            try
            {
                var subDistrictImportService =
                    new LgdSubDistrictImportService();

                model.Result =
                    subDistrictImportService.ImportOfficialSubDistrictXlsx(
                        file.InputStream,
                        file.FileName,
                        model.StateId.Value,
                        model.UpdateExisting,
                        model.SourceName,
                        CurrentUserId());

                TempData["SuccessMessage"] = String.Format(
                    "LGD Sub-District report processed. " +
                    "Inserted: {0}, Updated: {1}, Failed: {2}.",
                    model.Result.Inserted,
                    model.Result.Updated,
                    model.Result.Failed);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            PopulateImportStateOptions(model);
            return View("Import", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ImportBlocks(
            GovernmentImportVM model)
        {
            PopulateImportStateOptions(model);

            var file = GetUploadedFile(
                "BlockFile",
                model);

            ValidateImportRequest(
                model,
                file,
                "Select the official LGD Development Block XLSX report.",
                "Only the official LGD .xlsx Development Block report is supported.");

            if (!ModelState.IsValid)
                return View("Import", model);

            try
            {
                var blockImportService =
                    new LgdBlockImportService();

                model.Result =
                    blockImportService.ImportOfficialBlockXlsx(
                        file.InputStream,
                        file.FileName,
                        model.StateId.Value,
                        model.UpdateExisting,
                        model.SourceName,
                        CurrentUserId());

                TempData["SuccessMessage"] = String.Format(
                    "LGD Development Block report processed. " +
                    "Inserted: {0}, Updated: {1}, Failed: {2}.",
                    model.Result.Inserted,
                    model.Result.Updated,
                    model.Result.Failed);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            PopulateImportStateOptions(model);
            return View("Import", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ImportVillages(
            GovernmentImportVM model)
        {
            PopulateImportStateOptions(model);

            var file = GetUploadedFile(
                "VillageFile",
                model);

            ValidateImportRequest(
                model,
                file,
                "Select the official LGD All Villages of a State XLSX report.",
                "Only the official LGD .xlsx Village report is supported.");

            if (!ModelState.IsValid)
                return View("Import", model);

            try
            {
                var villageImportService =
                    new LgdVillageImportService();

                model.Result =
                    villageImportService.ImportOfficialVillageXlsx(
                        file.InputStream,
                        file.FileName,
                        model.StateId.Value,
                        model.UpdateExisting,
                        model.SourceName,
                        CurrentUserId());

                TempData["SuccessMessage"] = String.Format(
                    "LGD Village report processed. " +
                    "Inserted: {0}, Updated: {1}, Failed: {2}.",
                    model.Result.Inserted,
                    model.Result.Updated,
                    model.Result.Failed);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            PopulateImportStateOptions(model);
            return View("Import", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ImportGramPanchayats(GovernmentImportVM model)
        {
            PopulateImportStateOptions(model);
            var file = GetUploadedFile("GramPanchayatFile", model);
            ValidateImportRequest(model, file,
                "Select the official LGD PRI Local Body XLSX report.",
                "Only an official LGD .xlsx Gram Panchayat report is supported.");
            if (!ModelState.IsValid) return View("Import", model);

            try
            {
                model.Result = new LgdGramPanchayatImportService().ImportOfficialGramPanchayatXlsx(
                    file.InputStream, file.FileName, model.StateId.Value,
                    model.UpdateExisting, model.SourceName, CurrentUserId());
                TempData["SuccessMessage"] = String.Format(
                    "LGD Gram Panchayat report processed. Inserted: {0}, Updated: {1}, Failed: {2}.",
                    model.Result.Inserted, model.Result.Updated, model.Result.Failed);
            }
            catch (Exception ex) { ModelState.AddModelError("", ex.Message); }

            PopulateImportStateOptions(model);
            return View("Import", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ImportVillageMappings(GovernmentImportVM model)
        {
            PopulateImportStateOptions(model);
            var file = GetUploadedFile("VillageMappingFile", model);
            ValidateImportRequest(model, file,
                "Select the official LGD Gram Panchayat Mapping to village XLSX report.",
                "Only an official LGD .xlsx village mapping report is supported.");
            if (!ModelState.IsValid) return View("Import", model);

            try
            {
                model.Result = new LgdVillageMappingImportService().ImportOfficialVillageMappingXlsx(
                    file.InputStream, file.FileName, model.StateId.Value, CurrentUserId());
                TempData["SuccessMessage"] = String.Format(
                    "Village mapping report processed. Linked: {0}, Already linked: {1}, Failed: {2}.",
                    model.Result.Updated, model.Result.Skipped, model.Result.Failed);
            }
            catch (Exception ex) { ModelState.AddModelError("", ex.Message); }

            PopulateImportStateOptions(model);
            return View("Import", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ImportParliamentaryConstituencies(GovernmentImportVM model)
        {
            return ImportEciConstituencies(model, false);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ImportAssemblyConstituencies(GovernmentImportVM model)
        {
            return ImportEciConstituencies(model, true);
        }

        private ActionResult ImportEciConstituencies(GovernmentImportVM model, bool assembly)
        {
            PopulateImportStateOptions(model);
            var file = GetUploadedFile(assembly ? "AssemblyFile" : "ParliamentaryFile", model);
            ValidateImportRequest(model, file,
                "Select the official ECI constituency XLSX workbook.",
                "Only an official ECI .xlsx constituency workbook is supported.");
            if (!ModelState.IsValid) return View("Import", model);

            try
            {
                var importer = new EciConstituencyImportService();
                model.Result = assembly
                    ? importer.ImportAssemblyXlsx(file.InputStream, file.FileName, model.StateId.Value, model.UpdateExisting, CurrentUserId())
                    : importer.ImportParliamentaryXlsx(file.InputStream, file.FileName, model.StateId.Value, model.UpdateExisting, CurrentUserId());
                TempData["SuccessMessage"] = String.Format(
                    "ECI {0} report processed. Inserted: {1}, Updated: {2}, Failed: {3}.",
                    assembly ? "Assembly Constituency" : "Parliamentary Constituency",
                    model.Result.Inserted, model.Result.Updated, model.Result.Failed);
            }
            catch (Exception ex) { ModelState.AddModelError("", ex.Message); }

            PopulateImportStateOptions(model);
            return View("Import", model);
        }

        public JsonResult Options(
            string entityType,
            int? parentId = null,
            string parentType = null)
        {
            return Json(
                _service.Options(entityType, parentId, parentType),
                JsonRequestBehavior.AllowGet);
        }

        private GovernmentImportVM BuildImportModel(int? stateId)
        {
            var model = new GovernmentImportVM
            {
                StateId = stateId,
                UpdateExisting = true,
                SourceName = "LGD"
            };

            PopulateImportStateOptions(model);
            return model;
        }

        private void PopulateImportStateOptions(
            GovernmentImportVM model)
        {
            if (model == null)
                return;

            var options =
                _importService.GetStateOptions(model.StateId);

            model.States = options;
            model.StateOptions = options;
        }

        private HttpPostedFileBase GetUploadedFile(
            string preferredRequestKey,
            GovernmentImportVM model)
        {
            var requestFile =
                Request.Files[preferredRequestKey];

            if (requestFile != null &&
                requestFile.ContentLength > 0)
            {
                return requestFile;
            }

            if (model == null)
                return null;

            return model.OfficialFile
                   ?? model.PackageFile
                   ?? model.UploadFile
                   ?? model.ImportFile
                   ?? model.File;
        }

        private void ValidateImportRequest(
            GovernmentImportVM model,
            HttpPostedFileBase file,
            string missingFileMessage,
            string invalidFileMessage)
        {
            if (model == null || !model.StateId.HasValue)
            {
                ModelState.AddModelError(
                    "StateId",
                    "Please select a State / Union Territory.");
            }

            if (file == null || file.ContentLength <= 0)
            {
                ModelState.AddModelError(
                    "",
                    missingFileMessage);

                return;
            }

            if (!String.Equals(
                Path.GetExtension(file.FileName),
                ".xlsx",
                StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(
                    "",
                    invalidFileMessage);
                return;
            }

            const int maximumUploadBytes = 50 * 1024 * 1024;
            if (file.ContentLength > maximumUploadBytes)
            {
                ModelState.AddModelError("", "The XLSX file must not exceed 50 MB.");
                return;
            }

            var stream = file.InputStream;
            var originalPosition = stream.CanSeek ? stream.Position : 0;
            var firstByte = stream.ReadByte();
            var secondByte = stream.ReadByte();
            if (stream.CanSeek)
                stream.Position = originalPosition;

            if (firstByte != 0x50 || secondByte != 0x4B)
            {
                ModelState.AddModelError("", "The uploaded file is not a valid XLSX Open XML package.");
            }
        }

        private void AddHierarchyErrors(GeographyEditVM model)
        {
            if (model == null || String.IsNullOrWhiteSpace(model.EntityType))
                return;

            try
            {
                foreach (var error in _service.ValidateHierarchy(model))
                    ModelState.AddModelError("", error);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("EntityType", ex.Message);
            }
        }

        private void LoadOptions(GeographyEditVM model)
        {
            model.States =
                _service.Options("State");

            model.Districts =
                _service.Options(
                    "District",
                    model.StateId,
                    "State",
                    model.DistrictId);

            model.Tehsils =
                _service.Options(
                    "Tehsil",
                    model.DistrictId,
                    "District",
                    model.TehsilId);

            model.Blocks =
                _service.Options(
                    "Block",
                    model.DistrictId,
                    "District",
                    model.BlockId);

            model.ParliamentaryConstituencies =
                _service.Options(
                    "ParliamentaryConstituency",
                    model.StateId,
                    "State",
                    model.ParliamentaryConstituencyId);

            model.AssemblyConstituencies =
                model.ParliamentaryConstituencyId.HasValue
                    ? _service.Options(
                        "AssemblyConstituency",
                        model.ParliamentaryConstituencyId,
                        "ParliamentaryConstituency",
                        model.AssemblyConstituencyId)
                    : _service.Options(
                        "AssemblyConstituency",
                        model.StateId,
                        "State",
                        model.AssemblyConstituencyId);

            model.GramPanchayats =
                _service.Options(
                    "GramPanchayat",
                    model.BlockId,
                    "Block",
                    model.GramPanchayatId);
        }

        private int CurrentUserId()
        {
            int id;

            return Int32.TryParse(
                Convert.ToString(Session["UserId"]),
                out id)
                ? id
                : 0;
        }
    }
}
