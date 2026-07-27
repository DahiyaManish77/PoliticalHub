using System;
using System.Linq;
using System.Web.Mvc;
using PoliticalLeaderPortal.Areas.Admin.Services;
using PoliticalLeaderPortal.Areas.Admin.Services.Pdf;
using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using PoliticalLeaderPortal.Infrastructure.Pdf;
using QuestPDF.Fluent;
using PoliticalLeaderPortal.Infrastructure.Uploads;
using System.IO;

namespace PoliticalLeaderPortal.Areas.Admin.Controllers
{
    public class VerifiedDocumentController : Controller
    {
        private readonly VerifiedDocumentService _service = new VerifiedDocumentService();

        public ActionResult Index()
        {
            var documents = _service.GetAll();
            var today = DateTime.Today;
            return View(new VerifiedDocumentIndexVM
            {
                Documents = documents,
                ActiveCount = documents.Count(x => x.Status == "Active" && (!x.ExpiryDate.HasValue || x.ExpiryDate >= today)),
                RevokedCount = documents.Count(x => x.Status == "Revoked"),
                ExpiredCount = documents.Count(x => x.Status == "Active" && x.ExpiryDate.HasValue && x.ExpiryDate < today)
            });
        }

        public ActionResult Create()
        {
            return View(Prepare(new VerifiedDocumentVM
            {
                DocumentType = "DigitalCard",
                IssueDate = DateTime.Today,
                ExpiryDate = DateTime.Today.AddYears(1)
            }));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(VerifiedDocumentVM model)
        {
            if (model.DocumentType != "DigitalCard" &&
                (String.IsNullOrWhiteSpace(model.Subject) || String.IsNullOrWhiteSpace(model.BodyText)))
                ModelState.AddModelError("BodyText", "Subject and letter content are required for a letter.");

            if (!ModelState.IsValid)
                return View(Prepare(model));

            if (model.RecipientPhotoFile != null && model.RecipientPhotoFile.ContentLength > 0)
            {
                try
                {
                    string extension = SecureUploadValidator.ValidateImage(
                        model.RecipientPhotoFile, 5 * 1024 * 1024, false);
                    string folder = Server.MapPath("~/Uploads/VerifiedDocuments");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                    string fileName = "recipient-" + Guid.NewGuid().ToString("N") + extension;
                    model.RecipientPhotoFile.SaveAs(Path.Combine(folder, fileName));
                    model.RecipientPhotoPath = "~/Uploads/VerifiedDocuments/" + fileName;
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("RecipientPhotoFile", ex.Message);
                    return View(Prepare(model));
                }
            }

            int id = _service.Create(model, User.Identity.Name);
            TempData["SuccessMessage"] = "Verified document issued successfully.";
            return RedirectToAction("Download", new { id = id });
        }

        public ActionResult Download(int id)
        {
            var model = _service.GetById(id);
            if (model == null) return HttpNotFound();

            string verificationUrl = Url.Action("Index", "VerifyDocument",
                new { area = "", code = model.VerificationCode }, Request.Url.Scheme);
            byte[] pdf;
            if (model.DocumentType == "DigitalCard")
            {
                var card = new DigitalMemberCardVM
                {
                    PartyMemberCode = model.RecipientReference ?? model.DocumentNumber,
                    FullName = model.RecipientName,
                    Designation = model.RecipientRole,
                    WingName = model.CampaignName,
                    PhotoPath = model.RecipientPhotoPath,
                    LeaderPhotoPath = "~/Content/images/leader.png",
                    PartyLogoPath = "~/Content/images/bjp-lotus.png",
                    ValidTill = model.ExpiryDate,
                    VerificationUrl = verificationUrl,
                    QrCodeBase64 = QrCodeHelper.CreateBase64Png(verificationUrl),
                    ApprovedByName = model.IssuedByName,
                    ApprovedByDesignation = model.IssuedByDesignation
                };
                pdf = new MemberCardPdfService().Generate(card);
            }
            else
            {
                pdf = new VerifiedLetterDocument(model, verificationUrl).GeneratePdf();
            }

            return File(pdf, "application/pdf", model.DocumentNumber + ".pdf");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Revoke(int id, string reason)
        {
            if (String.IsNullOrWhiteSpace(reason))
            {
                TempData["ErrorMessage"] = "A revocation reason is required.";
                return RedirectToAction("Index");
            }
            _service.Revoke(id, reason.Trim(), User.Identity.Name);
            TempData["SuccessMessage"] = "Document revoked. Public verification will now reject it.";
            return RedirectToAction("Index");
        }

        private VerifiedDocumentVM Prepare(VerifiedDocumentVM model)
        {
            model.Campaigns = _service.GetCampaigns();
            return model;
        }
    }
}
